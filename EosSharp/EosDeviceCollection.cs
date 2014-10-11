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
    /// Readonly collection of EOS Bus Devices.
    /// </summary>
    public class EosDeviceCollection : ICollection<EosDevice>
    {
        private ICollection<EosDevice> _devices;

        public EosDeviceCollection(ICollection<EosDevice> devices)
        {
            if (devices == null)
            {
                throw new NullReferenceException("Can not create EosDeviceCollection from null collection");
            }

            _devices = devices;
        }

        #region ICollection Interface

        public void Add(EosDevice item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(EosDevice item)
        {
            return _devices.Contains(item);
        }

        public void CopyTo(EosDevice[] array, int arrayIndex)
        {
            _devices.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _devices.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(EosDevice item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<EosDevice> GetEnumerator()
        {
            return _devices.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _devices.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Returns device by it's assigned address.
        /// </summary>
        /// <param name="address">Address of the device to look up.</param>
        /// <returns>Device assigned to given address or null if not device was found with that address.</returns>
        public EosDevice GetByAddress(byte address)
        {
            foreach (EosDevice device in _devices)
            {
                if (device.Address == address)
                {
                    return device;
                }
            }
            return null;
        }
    }
}
