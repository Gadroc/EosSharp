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

    public delegate void EosDeviceEventHandler(object sender, EosDeviceEventArgs e);

    public class EosDeviceEventArgs : EventArgs
    {
        private EosDevice _device;

        public EosDeviceEventArgs(EosDevice device)
        {
            _device = device;
        }

        public EosDevice Device { get { return _device; } }
    }
}
