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
    using System.Text;

    public class EosPacket
    {
        private byte _destination;
        private byte _source;
        private byte _command;
        private byte _checksum;
        private bool _sent;

        private List<byte> _data;

        /// <summary>
        /// Creates a blank packet
        /// </summary>
        public EosPacket()
        {
            _sent = false;
            _data = new List<byte>();
            _checksum = 0;
        }

        /// <summary>
        /// Creates a command packet
        /// </summary>
        /// <param name="address">Address of the device to send this command to.</param>
        /// <param name="command">Command to send the device.</param>
        public EosPacket(byte address, EosBusCommands command)
            : this()
        {
            Destination = address;
            Command = (byte)command;
        }

        /// <summary>
        /// Creates a response packet for a given request packet.
        /// </summary>
        /// <param name="request">Packet containing the request for data.</param>
        public EosPacket(EosPacket request)
            : this()
        {
            Destination = request.Source;
            Command = request.Command;
            IsResponse = true;
        }

        #region Properties

        /// <summary>
        /// Gets/Sets the destination address for this packet.
        /// </summary>
        public byte Destination
        {
            get { return _destination; }
            set { _destination = value; }
        }

        /// <summary>
        /// Gets/Sets the source address for this packet.
        /// </summary>
        public byte Source
        {
            get { return _source; }
            set { _source = value; }
        }

        /// <summary>
        /// Gets/Sets the command for this packet.
        /// </summary>
        public byte Command
        {
            get { return _command; }
            set { _command = value; }
        }

        /// <summary>
        /// Gets/Sets the flag indicating this packet has already been sent.
        /// </summary>
        public bool IsSent
        {
            get { return _sent; }
            set { _sent = value; }
        }

        /// <summary>
        /// Gets/Sets the flag indicating if this packet is a response.
        /// </summary>
        public bool IsResponse
        {
            get { return (_command & (byte)64) == (byte)64; }
            set
            {
                if (value)
                {
                    _command |= (byte)64;
                }
                else
                {
                    _command &= (byte)191;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the flag indicating if this packet is a requires a response.
        /// </summary>
        public bool IsResponseRequired
        {
            get { return (_command & (byte)128) == (byte)128; }
        }

        /// <summary>
        /// Returns the calculated checksum for this packet.
        /// </summary>
        public byte Checksum
        {
            get
            {
                _checksum = (byte)(Destination + Source + Command + Data.Count);
                foreach (byte data in _data)
                {
                    _checksum += data;
                }
                return _checksum;
            }
        }

        /// <summary>
        /// Returns the data payload contained in this packet.
        /// </summary>
        public List<byte> Data
        {
            get { return _data; }
        }

        #endregion

        /// <summary>
        /// Adds a byte to the payload of this packet.
        /// </summary>
        /// <param name="data">Data to add to payload.</param>
        public void Add(byte data)
        {
            _data.Add(data);
        }

        /// <summary>
        /// Adds a byte array to the payload of this packet.
        /// </summary>
        /// <param name="data">Data to add to the payload</param>
        public void Add(byte[] data)
        {
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    Add(data[i]);
                }
            }
        }

        /// <summary>
        /// Adds an integer to the payload of this packet.
        /// </summary>
        /// <param name="data">Data to add to payload.</param>
        public void Add(int data)
        {
            Add((byte)((data >> 8) & 0xff));
            Add((byte)(data & 0xff));
        }

        /// <summary>
        /// Adds a long integer to the payload of this packet.
        /// </summary>
        /// <param name="data">Data to add to payload.</param>
        public void Add(long data)
        {
            Add((byte)((data >> 24) & 0xff));
            Add((byte)((data >> 16) & 0xff));
            Add((byte)((data >> 8) & 0xff));
            Add((byte)(data & 0xff));
        }

        /// <summary>
        /// Gets a string containing hex strings for each byte of the packet payload separated by spaces
        /// </summary>
        public string DataString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte item in _data)
                {
                    sb.AppendFormat("{0,2:X} ", item);
                }
                return sb.ToString();
            }
        }
    }

}
