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
    using System.Timers;

    /// <summary>
    /// Parser for EOS Bus data.
    /// </summary>
    public class EosParser
    {
        private const byte START_BYTE = 0xbb;
        private const byte LEADIN_BYTE = 0x88;

        private enum BUSRECIEVESTATE
        {
            PACKET_S_START,
            PACKET_S_LEADIN,
            PACKET_S_ADDRESS,
            PACKET_S_SRC,
            PACKET_S_COMMAND,
            PACKET_S_DATALEN,
            PACKET_S_DATA,
            PACKET_S_CHKSUM
        }

        private BUSRECIEVESTATE _state;
        private EosPacket _currentPacket;
        private byte _dataRemainig;
        private Timer _parseTimer;

        /// <summary>
        /// Fires when a new packet is received on the EOSBus.
        /// </summary>
        public event EosPacketEventHandler PacketReady;

        /// <summary>
        /// Fires when a packet parsing error has occured.
        /// </summary>
        public event EosParserErrorEventHandler ParseError;

        /// <summary>
        /// Creates a new Eos packet parser.
        /// </summary>
        /// <param name="packetTimeout">Time in milliseconds to wait for a full packet before reseting. 0 = no timeout</param>
        public EosParser(int packetTimeout)
        {
            if (packetTimeout > 0)
            {
                _parseTimer = new Timer(packetTimeout);
                _parseTimer.AutoReset = false;
                _parseTimer.Elapsed += ParseTimer_Elapsed;
            }
        }

        private void ParseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Reset();
            //Console.WriteLine("Parse Timeout");
            OnParesError(EosParserError.PacketTimeout);
        }

        /// <summary>
        /// Processes a byte received on the bus.
        /// </summary>
        /// <param name="data">Byte recevied on the bus</param>
        public bool ProcessData(byte data)
        {
            //Console.Write("{0,2:x} ", data);
            bool packetReady = false;
            switch (_state)
            {
                case BUSRECIEVESTATE.PACKET_S_START:
                    if (data == START_BYTE)
                    {
                        //Console.WriteLine("Start");
                        _state = BUSRECIEVESTATE.PACKET_S_LEADIN;
                    }
                    else
                    {
                        //Console.WriteLine("Skip");
                    }
                    break;
                case BUSRECIEVESTATE.PACKET_S_LEADIN:
                    if (data == LEADIN_BYTE)
                    {
                        //Console.WriteLine("Leadin");
                        _state = BUSRECIEVESTATE.PACKET_S_ADDRESS;
                    }
                    else
                    {
                        //Console.WriteLine("Reset");
                        _state = BUSRECIEVESTATE.PACKET_S_START;
                    }
                    break;
                case BUSRECIEVESTATE.PACKET_S_ADDRESS:
                    //Console.WriteLine("Destination");
                    _parseTimer.Stop();
                    _parseTimer.Start();
                    _currentPacket = new EosPacket();
                    _currentPacket.Destination = data;
                    _state = BUSRECIEVESTATE.PACKET_S_SRC;
                    break;
                case BUSRECIEVESTATE.PACKET_S_SRC:
                    //Console.WriteLine("Source");
                    _parseTimer.Stop();
                    _parseTimer.Start();
                    _currentPacket.Source = data;
                    _state = BUSRECIEVESTATE.PACKET_S_COMMAND;
                    break;
                case BUSRECIEVESTATE.PACKET_S_COMMAND:
                    //Console.WriteLine("Command");
                    _parseTimer.Stop();
                    _parseTimer.Start();
                    _currentPacket.Command = data;
                    _state = BUSRECIEVESTATE.PACKET_S_DATALEN;
                    break;
                case BUSRECIEVESTATE.PACKET_S_DATALEN:
                    //Console.WriteLine("Data Length");
                    _parseTimer.Stop();
                    _parseTimer.Start();
                    _dataRemainig = data;
                    if (_dataRemainig > 0)
                    {
                        _state = BUSRECIEVESTATE.PACKET_S_DATA;
                    }
                    else
                    {
                        _state = BUSRECIEVESTATE.PACKET_S_CHKSUM;
                    }
                    break;
                case BUSRECIEVESTATE.PACKET_S_DATA:
                    //Console.WriteLine("Data");
                    _parseTimer.Stop();
                    _parseTimer.Start();
                    _currentPacket.Add(data);
                    if (--_dataRemainig == 0)
                    {
                        _state = BUSRECIEVESTATE.PACKET_S_CHKSUM;
                    }
                    break;
                case BUSRECIEVESTATE.PACKET_S_CHKSUM:
                    _parseTimer.Stop();
                    if (_parseTimer != null)
                    {
                        _parseTimer.Stop();
                    }
                    if (data == _currentPacket.Checksum)
                    {
                        //Console.WriteLine("Good Packet");
                        OnPacketReceived(_currentPacket);
                    }
                    else
                    {
                        //Console.WriteLine("Bad Packet");
                        OnParesError(EosParserError.CorruptPacket);
                    }
                    Reset();
                    packetReady = true;
                    break;
            }

            return packetReady;
        }

        /// <summary>
        /// Processes data received on the bus.
        /// </summary>
        /// <param name="buffer">Byte array buffer which contains the data</param>
        /// <param name="offset">Offset in the buffer to start prcoessing data</param>
        /// <param name="length">Amount of bytes to process in the buffer</param>
        public void ProcessData(byte[] buffer, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                ProcessData(buffer[offset + i]);
            }
        }

        /// <summary>
        /// Resets the parser to start looking for the packet start.
        /// </summary>
        public void Reset()
        {
            _parseTimer.Stop();
            _state = BUSRECIEVESTATE.PACKET_S_START;
            _currentPacket = null;
        }

        protected void OnParesError(EosParserError error)
        {
            EosParserErrorEventArgs e = new EosParserErrorEventArgs(error);
            EosParserErrorEventHandler handler = ParseError;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnPacketReceived(EosPacket packet)
        {
            EosPacketEventArgs e = new EosPacketEventArgs(packet);
            EosPacketEventHandler handler = PacketReady;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

}
