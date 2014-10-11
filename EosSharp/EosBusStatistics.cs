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

    /// <summary>
    /// Object containing statistics gathered for an Eos Bus.
    /// </summary>
    public class EosBusStatistics
    {
        /// <summary>
        /// Number of times this bus has received processed a packet with a bad checksum.
        /// </summary>
        public ulong PacketErrors { get; set; }

        /// <summary>
        /// Number of times this bus has received more data than it's buffer can handle.
        /// </summary>
        public ulong Overruns { get; set; }

        /// <summary>
        /// Number of times this bus has received data while contructing a response.
        /// </summary>
        public ulong Collisions { get; set; }

        /// <summary>
        /// Number of times this bus has had a time out while waiting for a response.
        /// </summary>
        public ulong Timeouts { get; set; }

    }
}
