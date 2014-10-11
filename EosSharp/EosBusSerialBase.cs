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
    using System.IO.Ports;

    /// <summary>
    /// Base class for all serial based bus implemenations.
    /// </summary>
    public abstract class EosBusSerialBase : EosBus
    {
        // Communications Port we will be using to emulate a EOS Bus Master
        private SerialPort _com;
        private string _port;
        private int _baud;

        // Byte buffer used to construct data to send to the com port
        private byte[] _inputBuffer;

        public EosBusSerialBase(string port, int baudRate, int inputBufferSize)
        {
            _port = port;
            _baud = baudRate;
            _inputBuffer = new byte[inputBufferSize];
        }

        ~EosBusSerialBase()
        {
            if (_com != null && _com.IsOpen)
            {
                Disconnect();
            }
        }

        #region Properties

        /// <summary>
        /// Returns true if there is still data to write in the output buffer.
        /// </summary>
        public bool IsWriting
        {
            get
            {
                return _com != null && _com.IsOpen && _com.BytesToWrite > 0;
            }
        }

        #endregion

        public override void Connect()
        {
            if (_com == null)
            {
                _com = new SerialPort(_port, _baud, Parity.None, 8, StopBits.One);
                _com.DataReceived += DataReceived;
            }
            if (!_com.IsOpen)
            {
                _com.Open();
                _com.RtsEnable = true;
                _com.DtrEnable = true;
            }
        }

        public override void Disconnect()
        {
            if (_com != null)
            {
                _com.RtsEnable = false;
                _com.DtrEnable = false;
                _com.Close();
                _com.DataReceived -= DataReceived;
                _com = null;
            }
        }

        public void SendData(byte[] buffer, int offset, int length)
        {
            if (_com != null && _com.IsOpen)
            {
                _com.Write(buffer, offset, length);
            }
        }

        public void SendData(byte data)
        {
            byte[] buffer = new byte[1];
            buffer[0] = data;
            if (_com != null && _com.IsOpen)
            {
                _com.Write(buffer, 0, 1);
            }
        }

        public void SendData(String data)
        {
            if (_com != null && _com.IsOpen)
            {
                _com.Write(data);
            }
        }

        /// <summary>
        /// Relay data from serial port into EOS Parser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_com.BytesToRead > 0)
            {
                int toRead = Math.Min(_com.BytesToRead, _inputBuffer.Length);
                _com.Read(_inputBuffer, 0, toRead);
                ProcessData(_inputBuffer, 0, toRead);
            }
        }

        public abstract void ProcessData(byte[] buffer, int offset, int length);
    }
}
