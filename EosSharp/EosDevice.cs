//  Copyright 2014 Craig Courtney
//    
//  EosSharp is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  EosSharp is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Eos
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class representing a device on the EosBus.
    /// </summary>
    public class EosDevice
    {
        private EosBus _bus;
        private byte _address;
        private byte _groupAddress;
        private String _name;
        private String _firmware;
        private byte _pollingErrors;
        private byte _digialInputs;
        private byte _analogInputs;
        private byte _rotaryEncoders;
        private byte _ledOutputs;
        private byte _steppers;
        private byte _servos;
        private byte _alphaNumericDisplays;

        private EosPacket _powerPacket;
        private EosPacket _levelPacket;

        private List<byte> _state;

        /// <summary>
        /// Creates an EOS Bus Device for an address and the supplied info packet from the device.
        /// </summary>
        /// <param name="bus">Bus that this device is attached to.</param>
        /// <param name="address">Address which this device is assigned.</param>
        /// <param name="data">Data from this devices INFO_RESPONSE packet.</param>
        internal EosDevice(EosBus bus, byte address, List<byte> data)
        {
            _bus = bus;
            _address = address;
            _pollingErrors = 0;

            byte[] stringBuffer = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            int length = 8;
            for (int i = 0; i < 8; i++)
            {
                if (data[i] == 0)
                {
                    length = i;
                    break;
                }
                stringBuffer[i] = data[i];
            }
            _name = System.Text.ASCIIEncoding.ASCII.GetString(stringBuffer, 0, length);

            length = 4;
            for (int i = 0; i < 4; i++)
            {
                if (data[i] == 0)
                {
                    length = i;
                    break;
                }
                stringBuffer[i] = data[i + 8];
            }
            stringBuffer[4] = 0;
            _firmware = System.Text.ASCIIEncoding.ASCII.GetString(stringBuffer, 0, 4);

            _digialInputs = data[12];
            _analogInputs = data[13];
            _rotaryEncoders = data[14];
            _ledOutputs = data[15];
            _steppers = data[16];
            _servos = data[17];
            _alphaNumericDisplays = data[18];
            _groupAddress = data[19];

            _powerPacket = new EosPacket(Address, EosBusCommands.BACKLIGHT_POWER);
            _powerPacket.Add((byte)0);
            _powerPacket.IsSent = true;

            _levelPacket = new EosPacket(Address, EosBusCommands.BACKLIGHT_LEVEL);
            _levelPacket.Add((byte)0);
            _levelPacket.IsSent = true;
        }

        #region Properties

        /// <summary>
        /// Gets the bus this device resides on.
        /// </summary>
        public EosBus Bus
        {
            get { return _bus; }
        }

        /// <summary>
        /// Gets the address of this device on the bus.
        /// </summary>
        public byte Address
        {
            get { return _address; }
        }

        /// <summary>
        /// Gets the group that this device response on.
        /// </summary>
        public byte Group
        {
            get { return _groupAddress; }
        }

        /// <summary>
        /// Gets the name of this devcie.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the firmware version string for this device.
        /// </summary>
        public string Firmware
        {
            get { return _firmware; }
        }

        /// <summary>
        /// Gets the number of digital inputs on this device.
        /// </summary>
        public byte DigitalInputs
        {
            get { return _digialInputs; }
        }

        /// <summary>
        /// Gets the number of analog inputs on this device.
        /// </summary>
        public byte AnalogInputs
        {
            get { return _analogInputs; }
        }

        /// <summary>
        /// Gets the number of rotary encoders on this device.
        /// </summary>
        public byte RotaryEncoders
        {
            get { return _rotaryEncoders; }
        }

        /// <summary>
        /// Gets the number of LED outputs on this device.
        /// </summary>
        public byte LedOutputs
        {
            get { return _ledOutputs; }
        }

        /// <summary>
        /// Gets the number of stepper motors on this device.
        /// </summary>
        public byte StepperMotors
        {
            get { return _steppers; }
        }

        /// <summary>
        /// Gets the number of servo motors on this device.
        /// </summary>
        public byte ServoMotors
        {
            get { return _servos; }
        }

        /// <summary>
        /// Gets the number of alpha/numeric displays on this device.
        /// </summary>
        public byte AlpahNumbericDisplays
        {
            get { return _alphaNumericDisplays; }
        }

        /// <summary>
        /// Number of errors encountered when polling this device. May not be present
        /// on all bus implementations.
        /// </summary>
        public byte PollingErrors
        {
            get { return _pollingErrors; }
            set
            {
                _pollingErrors = value;
            }
        }

        #endregion

        #region Devcie Commands

        /// <summary>
        /// Sets the backlight brightness level for this device.
        /// </summary>
        /// <param name="level">Brightness level for the backlight.</param>
        public void SetBacklightLevel(byte level)
        {
            // If the previous level packet has not been sent yet just modify it's data,
            // other wise put it back on the bus.
            lock (_levelPacket)
            {
                _levelPacket.Data[0] = level;
                if (_levelPacket.IsSent)
                {
                    _levelPacket.IsSent = false;
                    Bus.SendPacket(_levelPacket);
                }
            }
        }

        /// <summary>
        /// Sets the backlight power state for this device.
        /// </summary>
        /// <param name="on">True will turn on the backlight, false will turn it off.</param>
        public void SetBacklightPower(bool on)
        {
            // If the previous power packet has not been sent yet just modify it's data,
            // other wise put it back on the bus.
            lock (_powerPacket)
            {
                _powerPacket.Data[0] = on ? (byte)1 : (byte)0;
                if (_powerPacket.IsSent)
                {
                    _powerPacket.IsSent = false;
                    Bus.SendPacket(_powerPacket);
                }
            }
        }

        /// <summary>
        /// Sets brightness level for an LED output on this device.  Note: Not all LED outputs
        /// support brightness.  If not supported this command will be ignored.
        /// </summary>
        /// <param name="led">ID for the led to set the brightness for.</param>
        /// <param name="level">Brightness level for the LED.</param>
        public void SetLedLevel(byte led, byte level)
        {
            LedPacket(EosBusCommands.LED_LEVEL, led, level);
        }

        /// <summary>
        /// Sets the power state for an LED output on this device.
        /// </summary>
        /// <param name="led">ID for the led to set the brightness for.</param>
        /// <param name="on">True will turn on the LED, false will turn it off.</param>
        public void SetLedPower(byte led, bool on)
        {
            LedPacket(EosBusCommands.LED_POWER, led, on ? (byte)1 : (byte)0);
        }

        // Helper function to construct LED packets
        private void LedPacket(EosBusCommands command, byte data1, byte? data2 = null)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)command;
            packet.Add(data1);
            if (data2 != null)
            {
                packet.Add((byte)data2);
            }
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Sets the target position for a stepper on this device.
        /// </summary>
        /// <param name="stepperId">ID of the stepper to set the position for.</param>
        /// <param name="position">Position to move the stepper to.</param>
        public void SetStepperTargetPosition(byte stepperId, long position)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.STEPPER_TARGET;
            packet.Add(stepperId);
            packet.Add(position);
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Sets the current position of the stepper to zero.
        /// </summary>
        /// <param name="stepperId">ID of the stepper to zero.</param>
        public void ZeroStepperPosition(byte stepperId)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.ZERO_STEPPER;
            packet.Add(stepperId);
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Sets a new address for a node on this device.
        /// </summary>
        /// <param name="newAddress">New address that this device should respond to.</param>
        public void SetNodeAddress(byte newAddress)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SET_ADDRESS;
            packet.Add(newAddress);
            _bus.SendPacket(packet);

            _address = newAddress;
        }

        /// <summary>
        /// Sets a new group address for this device.
        /// </summary>
        /// <param name="newGroup">New address taht this device should listen on.</param>
        public void SetGroup(byte newGroup)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SET_GROUP;
            packet.Add(newGroup);
            _bus.SendPacket(packet);

            _groupAddress = newGroup;
        }

        /// <summary>
        /// Sets the name that this device reports on the bus.
        /// </summary>
        /// <param name="name">New name for this device.</param>
        public void SetName(string name)
        {
            byte[] nameBuffer = new byte[9];
            System.Text.Encoding.ASCII.GetBytes(name, 0, Math.Min(8, name.Length), nameBuffer, 0);
            nameBuffer[8] = 0;
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SET_NAME;
            for (int i = 0; i < name.Length && i < 8; i++)
            {
                packet.Add(nameBuffer[i]);
            }
            _bus.SendPacket(packet);

            _name = name;
        }

        /// <summary>
        /// Sets the text output for an alphanumeric display on this device.
        /// </summary>
        /// <param name="display">ID of the display to set the text for.</param>
        /// <param name="text">Text to display on the device.</param>
        public void SetDisplayText(byte display, string text)
        {
            byte[] bytes = new byte[248];
            byte length = (byte)Math.Min(248, text.Length);
            System.Text.Encoding.ASCII.GetBytes(text, 0, length, bytes, 0);
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SET_TEXT;
            packet.Add(display);
            packet.Add(length);
            for (int i = 0; i < length; i++)
            {
                packet.Add(bytes[i]);
            }
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Sets the signal output value for the servo.
        /// </summary>
        /// <param name="servo">ID of the servo to set the output signal for.</param>
        /// <param name="value">Signal value in microseconds.</param>
        public void SetServoValue(byte servo, int value)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SERVO_VALUE;
            packet.Add(servo);
            packet.Add(value);
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Requests the config items for a servo.
        /// </summary>
        /// <param name="servo">ID of the servo to get the config for.</param>
        public void GetServoConfig(byte servo)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SERVO_GET_CONFIG;
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Sets the config items for a servo.
        /// </summary>
        /// <param name="servo">ID of the servo to set the config for.</param>
        /// <param name="minValue">Minimum singal value in microseconds.</param>
        /// <param name="maxValue">Maximum signal value in microseconds.</param>
        /// <param name="defaultValue">Default signal value in microseconds.</param>
        public void SetServoConfig(byte servo, int minValue, int maxValue, int defaultValue)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.SERVO_SET_CONFIG;
            packet.Add(servo);
            packet.Add(minValue);
            packet.Add(maxValue);
            packet.Add(defaultValue);
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Requests the config items for a stepper.
        /// </summary>
        /// <param name="servo">ID of the stepper to get the config for.</param>
        public void GetStepperConfig(byte stepper)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.STEPPER_GET_CONFIG;
            _bus.SendPacket(packet);
        }

        /// <summary>
        /// Sets the config items for a stepper.
        /// </summary>
        /// <param name="stepper">ID of the setpper to set the config for.</param>
        /// <param name="maxSpeed">Minimum singal value in microseconds.</param>
        /// <param name="maxValue">Maximum signal value in microseconds.</param>
        /// <param name="defaultValue">Default signal value in microseconds.</param>
        public void SetStepperConfig(byte stepper, uint maxSpeed, ulong idleSleep)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = _address;
            packet.Command = (byte)EosBusCommands.STEPPER_SET_CONFIG;
            packet.Add(stepper);
            packet.Add((int)maxSpeed);
            packet.Add((long)idleSleep);
            _bus.SendPacket(packet);
        }

        #endregion

        #region Device State

        public void UpdateState(EosPacket packet)
        {
            _state = packet.Data;
        }

        public bool DigitalState(byte input)
        {
            if (_state != null && input < _digialInputs)
            {
                int address = input / 8;
                byte bitmask = (byte)(1 << (input % 8));
                if (_state.Count > address)
                {
                    return (_state[address] & bitmask) > 0;
                }
            }
            return false;
        }

        public int AnanlogState(byte input)
        {
            if (_state != null && input < _analogInputs)
            {
                int address = ((_digialInputs - 1) / 8 + 1) + (input * 2);
                if (_state.Count > address)
                {
                    return (_state[address] << 8) + _state[address + 1];
                }
            }
            return 0;
        }

        #endregion

    }

}
