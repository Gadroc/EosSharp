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
    /// EOS Controller for communicating with a EOS Bus Shield.
    /// </summary>
    public class EosBusInterfaceSerial : EosBusSerialBase
    {
        private enum ParseStates
        {
            COMMAND,
            MODE,
            NAME,
            FIRMWARE,
            ENUMERATIONCOUNT,
            ENUMERATION,
            PACKET,
            STATS
        }

        private ParseStates _parseState = ParseStates.COMMAND;
        private byte[] _parseBuffer = new byte[64];
        private byte _parseBufferCount = 0;

        private string _name;

        private string _firmware;

        private EosBusStatistics _stats;

        // List of all devices found on this pus.
        private List<EosDevice> _devices;

        // Read only collection to expose devices to consumer applications
        private EosDeviceCollection _externalDevices;

        public EosBusInterfaceSerial(string port, int baudRate)
            : base(port, baudRate, 64)
        {
            _stats = new EosBusStatistics();
            _devices = new List<EosDevice>();
            _externalDevices = new EosDeviceCollection(_devices);
        }

        #region Properties

        public override EosDeviceCollection Devices
        {
            get { return _externalDevices; }
        }

        public override EosBusStatistics Stats
        {
            get { return _stats; }
        }

        /// <summary>
        /// Returns the bus interface device name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Returns the firmware id string for the bus device
        /// </summary>
        public string Firmware { get { return _firmware; } }

        #endregion

        public override void Connect()
        {
            base.Connect();
            IsConnected = true;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            IsConnected = false;
        }

        public override void StartPolling()
        {
            SendData((byte)112);
        }

        public override void StopPolling()
        {
            SendData((byte)113);
        }

        public override void Reset()
        {
            SendData((byte)116);
        }

        public override void Rescan()
        {
            SendData((byte)114);
        }

        public override void SendPacket(EosPacket packet)
        {
            byte[] buffer = new byte[64];
            buffer[0] = 115;
            buffer[1] = packet.Destination;
            buffer[2] = packet.Command;
            buffer[3] = (byte)(packet.Data.Count);
            packet.Data.CopyTo(buffer, 4);

            SendData(buffer, 0, packet.Data.Count + 4);

            if (packet.Command > 127)
            {
                State = EosBusState.WAITING_RESPONSE;
            }
        }

        public override void ProcessData(byte[] buffer, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                byte data = buffer[offset + i];
                switch (_parseState)
                {
                    case ParseStates.COMMAND:
                        ProcessCommand(data);
                        break;

                    case ParseStates.MODE:
                        ProcessMode(data);
                        break;

                    case ParseStates.PACKET:
                        _parseBuffer[_parseBufferCount++] = data;

                        if ((_parseBufferCount == 4 && _parseBuffer[3] == 0) ||
                            (_parseBufferCount > 4 && _parseBufferCount == (4 + _parseBuffer[3])))
                        {
                            EosPacket packet = new EosPacket();
                            packet.Destination = _parseBuffer[0];
                            packet.Source = _parseBuffer[1];
                            packet.Command = _parseBuffer[2];
                            for (int j = 0; j < _parseBuffer[3]; j++)
                            {
                                packet.Add(_parseBuffer[4 + j]);
                            }
                            OnPacketReceived(packet);

                            if (State == EosBusState.WAITING_RESPONSE)
                            {
                                OnResponseReceived(packet);
                            }

                            if (packet.Command == 127 && packet.Data.Count > 0)
                            {
                                EosDevice device = Devices.GetByAddress(packet.Source);
                                device.UpdateState(packet);
                                OnDeviceUpdated(device);
                            }

                            _parseState = ParseStates.COMMAND;
                            //Console.WriteLine();
                        }
                        break;

                    case ParseStates.ENUMERATIONCOUNT:
                        if (data > 0)
                        {
                            _parseBuffer[0] = data;
                            _parseState = ParseStates.ENUMERATION;
                        }
                        else
                        {
                            _parseState = ParseStates.COMMAND;
                        }
                        break;

                    case ParseStates.ENUMERATION:
                        _parseBuffer[1 + _parseBufferCount++] = data;
                        if (_parseBufferCount == 21)
                        {
                            List<byte> deviceData = new List<byte>(20);
                            for (int j = 0; j < 20; j++) { deviceData.Add(_parseBuffer[2 + j]); }
                            _devices.Add(new EosDevice(this, _parseBuffer[1], deviceData));
                            _parseBuffer[0]--;
                            _parseBufferCount = 0;
                            if (_parseBuffer[0] == 0)
                            {
                                _parseState = ParseStates.COMMAND;
                                OnBusReset();
                                //Console.WriteLine();
                            }
                        }
                        break;

                    default:
                        if (data == 58)
                        {
                            ProcessCommandData();
                            //Console.WriteLine();
                        }
                        else
                        {
                            _parseBuffer[_parseBufferCount++] = data;
                        }
                        break;
                }
            }
        }

        private void ProcessCommandData()
        {
            switch (_parseState)
            {
                case ParseStates.NAME:
                    _name = System.Text.ASCIIEncoding.ASCII.GetString(_parseBuffer, 0, _parseBufferCount);
                    break;

                case ParseStates.FIRMWARE:
                    _firmware = System.Text.ASCIIEncoding.ASCII.GetString(_parseBuffer, 0, _parseBufferCount);
                    break;

                case ParseStates.STATS:
                    // TODO: Parse stats
                    break;
            }
            _parseBufferCount = 0;
            _parseState = ParseStates.COMMAND;
        }

        private void ProcessMode(byte mode)
        {
            switch (mode)
            {
                case 112:
                    State = EosBusState.POLLING;
                    break;

                case 115:
                    State = EosBusState.SCANNING;
                    break;

                case 119:
                    State = EosBusState.WAITING_RESPONSE;
                    break;

                case 105:
                    State = EosBusState.IDLE;
                    break;
            }
            _parseState = ParseStates.COMMAND;
        }

        private void ProcessCommand(byte command)
        {
            switch (command)
            {
                case 101:
                    _devices.Clear();
                    _parseState = ParseStates.ENUMERATIONCOUNT;
                    break;

                case 102:
                    _parseState = ParseStates.FIRMWARE;
                    break;

                case 105:
                    _parseState = ParseStates.STATS;
                    break;

                case 109:
                    _parseState = ParseStates.MODE;
                    break;

                case 110:
                    _parseState = ParseStates.NAME;
                    break;

                case 112:
                    _parseState = ParseStates.PACKET;
                    break;

                case 113: // Timeout
                case 114: // Bus Error
                    if (State == EosBusState.WAITING_RESPONSE)
                    {
                        OnResponseReceived((EosPacket)null);
                        State = EosBusState.POLLING;
                    }
                    break;
            }
            _parseBufferCount = 0;

        }
    }

}
