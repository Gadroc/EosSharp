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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
namespace GadrocsWorkshop.Eos
{
    /// <summary>
    /// EOS Bus implementation where the computer is the bus master.  Useful for single device directly connected via RS-232 or an entire bus via an RS-232 to RS-485 adapter.
    /// Caution should be used for larger busses, this implementation has a higher latency and slower bus polling than a hardware interface.
    /// </summary>
    public class EosBusMaster : EosBusSerialBase
    {
        private enum BusMasterState
        {
            IDLE,            // Bus is waiting for something to do
            TRANSMITTING,    // Bus is transmitting data
            WAITINGRESPONSE, // Bus is waiting on a repsonse
            CLEARINGSCAN,    // Waiting for current action to complete to start rescan
            CLEARINGRESET    // Waiting for current action to complete to rest bus
        }

        private const int TRANSMIT_CHECK_INTERVAL = 5;   // Time in miliseconds which the transmit queue is checked
        private const int RESPONSE_TIMEOUT = 150;        // Amount of time in milliseconds waited before timingout a resposne
        private const int POLL_INTERVAL = 100;           // Amount of time in milliseconds to wait between sending polling packets

        //  State of the EOS Bus Master
        private BusMasterState _masterState;

        // Address which we are waiting on a response from.
        private byte _resposneAddress;

        // Flag indicating that this bus has been scaned already.  This is used
        // to automatically scan a bus the first time it is connected.
        private bool _scanned;

        // Packet parser for raw packets
        private EosParser _parser;

        // Timer used to manage timeouts and poll intervals.
        private Timer _busTimer;

        // Holds the statistics for this bus.
        private EosBusStatistics _stats;

        // Flag indicating wheter bus should be poling or not.
        private bool _polling = false;

        // During scanning holds the address we are currently scanning, but 
        // during polling it is the index into the _devices list of the current
        // device being polled.
        private byte _pollId = 0;

        // Queue of Packets we need to send over the bus
        private ConcurrentQueue<EosPacket> _sendPackets;

        // List of all devices found on this pus.
        private List<EosDevice> _devices;

        // Read only collection to expose devices to consumer applications
        private EosDeviceCollection _externalDevices;

        public EosBusMaster(string port, int baudRate)
            : base(port, baudRate, 64)
        {
            _stats = new EosBusStatistics();
            _masterState = BusMasterState.IDLE;
            State = EosBusState.IDLE;

            _parser = new EosParser(150);
            _parser.PacketReady += new EosPacketEventHandler(Parser_PacketReady);
            _parser.ParseError += new EosParserErrorEventHandler(Parser_ParseError);

            _busTimer = new Timer(100);
            _busTimer.AutoReset = false;
            _busTimer.Elapsed += TimerEvent;

            _scanned = false;

            _devices = new List<EosDevice>();
            _externalDevices = new EosDeviceCollection(_devices);
            _sendPackets = new ConcurrentQueue<EosPacket>();
        }

        ~EosBusMaster()
        {
            _busTimer.Stop();
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

        #endregion

        #region Bus Processing

        public override void ProcessData(byte[] buffer, int offset, int length)
        {
            _parser.ProcessData(buffer, offset, length);
        }

        /// <summary>
        /// Transmits the current packet to the bus
        /// </summary>
        private void TransmitPacket(EosPacket sendPacket)
        {
            byte[] buffer = new byte[64];
            lock (sendPacket)
            {
                sendPacket.Source = 0;
                buffer[0] = 0xbb;
                buffer[1] = 0x88;
                buffer[2] = sendPacket.Destination;
                buffer[3] = sendPacket.Source;
                buffer[4] = sendPacket.Command;
                buffer[5] = (byte)(sendPacket.Data.Count);
                sendPacket.Data.CopyTo(buffer, 6);
                buffer[6 + sendPacket.Data.Count] = sendPacket.Checksum;

                _masterState = BusMasterState.TRANSMITTING;
                SendData(buffer, 0, sendPacket.Data.Count + 7);
                if (sendPacket.Command > 127)
                {
                    _resposneAddress = sendPacket.Destination;
                }
                else
                {
                    _resposneAddress = 0xff;
                }
                _busTimer.Interval = TRANSMIT_CHECK_INTERVAL;
                _busTimer.Start();
                sendPacket.IsSent = true;
            }
        }

        /// <summary>
        /// Timer event which is fired when a timeout occurs or a polling
        /// wait interval is over.
        /// </summary>
        private void TimerEvent(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                switch (_masterState)
                {
                    case BusMasterState.CLEARINGSCAN:
                        _masterState = BusMasterState.IDLE;
                        Rescan();
                        break;

                    case BusMasterState.CLEARINGRESET:
                        _masterState = BusMasterState.IDLE;
                        Reset();
                        break;

                    case BusMasterState.IDLE:
                        DoIdle();
                        break;

                    case BusMasterState.TRANSMITTING:
                        if (!IsWriting)
                        {
                            if (_resposneAddress != 0xff)
                            {
                                switch (State)
                                {
                                    case EosBusState.TRANSMITTING:
                                        State = EosBusState.WAITING_RESPONSE;
                                        break;

                                    case EosBusState.POLLING:
                                    case EosBusState.IDLE:
                                        State = _polling ? EosBusState.POLLING : EosBusState.IDLE;
                                        break;
                                }
                                _masterState = BusMasterState.WAITINGRESPONSE;
                                _busTimer.Interval = RESPONSE_TIMEOUT;
                                _busTimer.Start();
                            }
                            else
                            {
                                switch (State)
                                {
                                    case EosBusState.TRANSMITTING:
                                    case EosBusState.POLLING:
                                    case EosBusState.IDLE:
                                        State = _polling ? EosBusState.POLLING : EosBusState.IDLE;
                                        break;
                                }
                                _masterState = BusMasterState.IDLE;
                                DoIdle();
                            }
                        }
                        break;

                    case BusMasterState.WAITINGRESPONSE:
                        ProcessResponseTimeout();
                        if (State == EosBusState.WAITING_RESPONSE)
                        {
                            State = _polling ? EosBusState.POLLING : EosBusState.IDLE;
                        }
                        _masterState = BusMasterState.IDLE;
                        DoIdle();
                        break;
                }
            }
        }

        /// <summary>
        /// Executes the appropraite action when the bus is idle
        /// </summary>
        private void DoIdle()
        {
            EosPacket sendPacket = null;
            if (_sendPackets.TryDequeue(out sendPacket))
            {
                //Console.WriteLine("Transmitting Packet Begin");
                TransmitPacket(sendPacket);
                //Console.WriteLine("Transmitting Packet End");
            }
            else
            {
                switch (State)
                {
                    case EosBusState.POLLING:
                        if (_polling)
                        {
                            NextPoll();
                        }
                        else
                        {
                            State = EosBusState.IDLE;
                        }
                        break;

                    case EosBusState.SCANNING:
                        NextScan();
                        break;

                    case EosBusState.IDLE:
                        if (_polling)
                        {
                            State = EosBusState.POLLING;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Sends out the next scan packet.
        /// </summary>
        private void NextScan()
        {
            _pollId++;
            if (_pollId > 31)
            {
                _scanned = true;
                _pollId = 1;
                OnBusReset();
                State = _polling ? EosBusState.POLLING : EosBusState.IDLE;
                DoIdle();
                return;
            }
            else
            {
                State = EosBusState.SCANNING;
            }
            //Console.WriteLine("Scanning {0}", _pollId);
            TransmitPacket(new EosPacket(_pollId, EosBusCommands.INFO));
        }

        /// <summary>
        /// Sends out the next poll request packet.
        /// </summary>
        private void NextPoll()
        {
            if (_devices.Count > 0)
            {
                _pollId++;
                if (_pollId >= _devices.Count)
                {
                    _pollId = 0;
                }

                // Skip over any devices which are getting to many errors.
                byte loopId = _pollId;
                while (_devices[_pollId].PollingErrors == 255)
                {
                    _pollId++;
                    if (_pollId == _devices.Count)
                    {
                        _pollId = 0;
                    }
                    if (_pollId == loopId)
                    {
                        // Hmm well no more boards left
                        return;
                    }
                }

                //Console.WriteLine("Polling {0}", _pollId);
                TransmitPacket(new EosPacket(_devices[_pollId].Address, EosBusCommands.POLL));
            }
        }

        /// <summary>
        /// Process a response timeout
        /// </summary>
        private void ProcessResponseTimeout()
        {
            switch (State)
            {
                case EosBusState.POLLING:
                    //Console.WriteLine("Polling timeout");
                    _devices[_pollId].PollingErrors++;
                    break;

                case EosBusState.WAITING_RESPONSE:
                    OnResponseReceived((EosPacket)null);
                    _masterState = BusMasterState.IDLE;
                    break;
            }
        }

        private void Parser_ParseError(object sender, EosParserErrorEventArgs e)
        {
            lock (this)
            {
                if (_masterState == BusMasterState.CLEARINGSCAN)
                {
                    _masterState = BusMasterState.IDLE;
                    Rescan();
                }
                else if (_masterState == BusMasterState.CLEARINGRESET)
                {
                    _masterState = BusMasterState.IDLE;
                    Reset();
                }
                else if (_masterState == BusMasterState.WAITINGRESPONSE)
                {
                    _masterState = BusMasterState.IDLE;
                    _busTimer.Stop();
                    switch (State)
                    {
                        case EosBusState.SCANNING:
                            NextScan();
                            break;

                        case EosBusState.POLLING:
                            //Console.WriteLine("Polling Parse Error");
                            _devices[_pollId].PollingErrors++;
                            break;

                        case EosBusState.WAITING_RESPONSE:
                            OnResponseReceived((EosPacket)null);
                            break;
                    }
                }
                else
                {
                    // Received errored packet when we are not waiting on one.
                    _stats.Collisions++;
                }
            }
        }

        private void Parser_PacketReady(object sender, EosPacketEventArgs e)
        {
            lock (this)
            {
                //Console.WriteLine("Packet Received (masterState {0}, responseAddress {1})", _masterState, _resposneAddress);
                OnPacketReceived(e.Packet);

                // Check to see if we got the response before we checked for done transmitting
                if (_masterState == BusMasterState.TRANSMITTING && _resposneAddress != 0xff && !IsWriting)
                {
                    _masterState = BusMasterState.WAITINGRESPONSE;
                    switch (State)
                    {
                        case EosBusState.TRANSMITTING:
                            State = EosBusState.WAITING_RESPONSE;
                            break;

                        case EosBusState.POLLING:
                        case EosBusState.IDLE:
                            State = _polling ? EosBusState.POLLING : EosBusState.IDLE;
                            break;
                    }
                }

                if (_masterState == BusMasterState.WAITINGRESPONSE && e.Packet.Source == _resposneAddress)
                {
                    _busTimer.Stop();
                    _masterState = BusMasterState.IDLE;
                    switch (State)
                    {
                        case EosBusState.POLLING:
                            //Console.WriteLine("Polling Received");

                            if (e.Packet.Data.Count > 0)
                            {
                                EosDevice device = Devices.GetByAddress(e.Packet.Source);
                                device.UpdateState(e.Packet);
                                OnDeviceUpdated(device);
                            }

                            _busTimer.Interval = POLL_INTERVAL;
                            _busTimer.Start();
                            break;

                        case EosBusState.SCANNING:
                            // Scan found a valid response
                            _devices.Add(new EosDevice(this, e.Packet.Source, e.Packet.Data));
                            NextScan();
                            break;

                        case EosBusState.WAITING_RESPONSE:
                            //Console.WriteLine("Response sent");
                            OnResponseReceived(e.Packet);
                            if (_polling)
                            {
                                State = EosBusState.POLLING;
                                _busTimer.Interval = POLL_INTERVAL;
                                _busTimer.Start();
                            }
                            else
                            {
                                State = EosBusState.IDLE;
                            }
                            break;
                    }
                }
                else if (_masterState == BusMasterState.CLEARINGSCAN)
                {
                    _masterState = BusMasterState.IDLE;
                    Rescan();
                }
                else if (_masterState == BusMasterState.CLEARINGRESET)
                {
                    _masterState = BusMasterState.IDLE;
                    Reset();
                }
                else
                {
                    // Received errored packet when we are not waiting on one.
                    _stats.Collisions++;
                }

            }
        }

        #endregion

        #region EosBus Interface

        public override void Connect()
        {
            base.Connect();
            if (!_scanned)
            {
                Reset();
            }
            IsConnected = true;
        }

        public override void SendPacket(EosPacket packet)
        {
            _sendPackets.Enqueue(packet);
            if (State == EosBusState.IDLE)
            {
                _busTimer.Interval = TRANSMIT_CHECK_INTERVAL;
                _busTimer.Start();
            }
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _busTimer.Stop();
            _parser.Reset();
            IsConnected = false;
        }

        public override void StartPolling()
        {
            _polling = true;
            if (State == EosBusState.IDLE)
            {
                State = EosBusState.POLLING;
                NextPoll();
            }
        }

        public override void StopPolling()
        {
            _polling = false;
        }

        public override void Reset()
        {
            if (_masterState != BusMasterState.IDLE)
            {
                _masterState = BusMasterState.CLEARINGRESET;
                return;
            };

            SendPacket(new EosPacket(0xff, EosBusCommands.RESET));

            // Reset our error statistics
            _stats.Timeouts = 0;
            _stats.Collisions = 0;
            _stats.Overruns = 0;
            _stats.PacketErrors = 0;
        }

        public override void Rescan()
        {
            if (_masterState != BusMasterState.IDLE)
            {
                _masterState = BusMasterState.CLEARINGSCAN;
                return;
            };

            // Stop current activity
            _busTimer.Stop();
            _parser.Reset();

            // Setup for a bus scan
            _pollId = 1;
            State = EosBusState.SCANNING;
            _devices.Clear();

            // Start the scan.
            NextScan();
        }

        #endregion
    }
}
