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

    public class EosLed : EosOutput
    {
        private EosPacket _powerPacket;
        private EosPacket _levelPacket;

        internal EosLed(EosDevice device, byte id)
            : base(device, id)
        {
            _powerPacket = new EosPacket(device.Address, EosBusCommands.LED_POWER);
            _powerPacket.Add((byte)id);
            _powerPacket.Add((byte)0);
            _powerPacket.IsSent = true;

            _levelPacket = new EosPacket(device.Address, EosBusCommands.LED_LEVEL);
            _levelPacket.Add((byte)id);
            _levelPacket.Add((byte)0);
            _levelPacket.IsSent = true;
        }

        /// <summary>
        /// Sets the power state for this LED.
        /// </summary>
        /// <param name="on">True will turn on the LED, false will turn it off.</param>
        public void SetPower(bool on)
        {
            // If the previous power packet has not been sent yet just modify it's data,
            // other wise put it back on the bus.
            lock (_powerPacket)
            {
                _powerPacket.Data[1] = on ? (byte)1 : (byte)0;
                if (_powerPacket.IsSent)
                {
                    _powerPacket.IsSent = false;
                    Device.Bus.SendPacket(_powerPacket);
                }
            }
        }

        /// <summary>
        /// Sets brightness level for this LED.  Note: Not all LED outputs
        /// support brightness.  If not supported this command will be ignored.
        /// </summary>
        /// <param name="level">Brightness level for the LED.</param>
        public void SetLevel(byte level)
        {
            // If the previous level packet has not been sent yet just modify it's data,
            // other wise put it back on the bus.
            lock (_levelPacket)
            {
                _levelPacket.Data[1] = level;
                if (_levelPacket.IsSent)
                {
                    _levelPacket.IsSent = false;
                    Device.Bus.SendPacket(_levelPacket);
                }
            }
        }
    }
}
