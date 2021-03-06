﻿//  Copyright 2014 Craig Courtney
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

    public delegate void EosPacketEventHandler(object sender, EosPacketEventArgs e);

    public class EosPacketEventArgs : EventArgs
    {
        private EosPacket _packet;

        public EosPacketEventArgs(EosPacket packet)
        {
            _packet = packet;
        }

        /// <summary>
        /// Contains the packet that was received.
        /// </summary>
        public EosPacket Packet { get { return _packet; } }
    }
}
