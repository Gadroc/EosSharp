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
    /// Base class for all outputs supported by EOS devices.
    /// </summary>
    public class EosOutput
    {
        /// <summary>
        /// ID of this particular output (1 for LED #1 on the device).  Must be unique by output type.
        /// </summary>
        private byte _id = 0;

        /// <summary>
        /// Device that this output is a part of.
        /// </summary>
        private EosDevice _device = null;

        /// <summary>
        /// Creates an output for a device and it's unique id.
        /// </summary>
        /// <param name="device">Device this output belongs to.</param>
        /// <param name="id">Unique id by type for this output.</param>
        internal EosOutput(EosDevice device, byte id)
        {
            _id = id;
            _device = device;
        }

        /// <summary>
        /// Returns the unique id by type for this device.
        /// </summary>
        public byte Id
        {
            get { return _id; }
        }

        /// <summary>
        ///  Returns the device that this output belongs to.
        /// </summary>
        public EosDevice Device
        {
            get { return _device; }
        }
    }
}
