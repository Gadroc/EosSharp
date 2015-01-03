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
    using System.ComponentModel;

    /// <summary>
    /// Abstract class for EOSBus implementations.
    /// </summary>
    public abstract class EosBus : INotifyPropertyChanged
    {
        public enum EosBusState
        {
            POLLING,
            SCANNING,
            TRANSMITTING,
            WAITING_RESPONSE,
            IDLE
        }

        private EosBusState _state;
        private bool _connected;

        public event PropertyChangedEventHandler PropertyChanged;

        protected EosBus()
        {
            _state = EosBusState.POLLING;
            _connected = false;
        }

        /// <summary>
        /// Fires when a new packet is received on the EOSBus.  Used for monitoring purposes 
        /// or where you want to process raw packets.
        /// </summary>
        public event EosPacketEventHandler PacketReceived;

        /// <summary>
        /// Fires when a response is received to a request sent through this bus.  In the event
        /// of a response time out event will be fired with a null response packet.
        /// </summary>
        public event EosPacketEventHandler ResponseReceived;

        /// <summary>
        /// Fires when the bus has been reset and devices are reloaded.  Client must
        /// release device handles it's holding onto and get new ones.
        /// </summary>
        public event EventHandler BusReset;

        /// <summary>
        /// Fires when a device has updated state information.
        /// </summary>
        public event EosDeviceEventHandler DeviceUpdate;

        #region Properties

        /// <summary>
        /// Returns the current state of the bus
        /// </summary>
        public EosBusState State
        {
            get
            {
                return _state;
            }
            protected set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        /// <summary>
        /// Returns true if this bus is currently connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _connected;
            }
            protected set
            {
                if (_connected != value)
                {
                    _connected = value;
                    OnPropertyChanged("IsConnected");
                }
            }
        }

        /// <summary>
        /// Enumerates device IDs on the bus.
        /// </summary>
        public abstract EosDeviceCollection Devices { get; }

        /// <summary>
        /// Returns statistics for the current bus.
        /// </summary>
        public abstract EosBusStatistics Stats { get; }

        #endregion

        /// <summary>
        /// Connects this bus and starts reading events.
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Disconnects this bus and stops reading events.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Starts the system polling EOS devices.
        /// </summary>
        public abstract void StartPolling();

        /// <summary>
        /// Stops the system from polling EOS devices.
        /// </summary>
        public abstract void StopPolling();

        /// <summary>
        /// Reset statistics and forces poll response from all devices.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Rescans the bus for devices.
        /// </summary>
        public abstract void Rescan();

        /// <summary>
        /// Sends a packet to the bus.
        /// </summary>
        /// <param name="packet">Packet which to send.</param>
        public abstract void SendPacket(EosPacket packet);

        /// <summary>
        /// Sends a command to the bus.
        /// </summary>
        /// <param name="address">Address of the device this command is for.</param>
        /// <param name="command">Command of this address</param>
        /// <param name="data">Data to include in this command.</param>
        public void SendCommand(byte address, EosBusCommands command, byte[] data = null)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = address;
            packet.Command = (byte)command;
            packet.Add(data);
        }

        /// <summary>
        /// Sends a response to the given packet.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="data"></param>
        public void SendResponse(EosPacket request, byte[] data = null)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = request.Source;
            packet.Command = request.Command;
            packet.IsResponse = true;
            packet.Add(data);
            SendPacket(packet);
        }

        /// <summary>
        /// Sends a GetInfo packet to the given device.
        /// </summary>
        /// <param name="address">Address to send the GetInfo packet to.</param>
        public void GetInfo(byte address)
        {
            EosPacket packet = new EosPacket();
            packet.Destination = address;
            packet.Command = 130;
            SendPacket(packet);
        }

        #region Event Trigger Methods

        protected void OnDeviceUpdated(EosDeviceEventArgs e)
        {
            EosDeviceEventHandler handler = DeviceUpdate;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }

        protected void OnDeviceUpdated(EosDevice device)
        {
            OnDeviceUpdated(new EosDeviceEventArgs(device));
        }

        protected void OnPacketReceived(EosPacketEventArgs e)
        {
            EosPacketEventHandler handler = PacketReceived;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }

        protected void OnPacketReceived(EosPacket packet)
        {
            OnPacketReceived(new EosPacketEventArgs(packet));
        }

        protected void OnResponseReceived(EosPacketEventArgs e)
        {
            EosPacketEventHandler handler = ResponseReceived;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }

        protected void OnResponseReceived(EosPacket packet)
        {
            OnResponseReceived(new EosPacketEventArgs(packet));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, e);
            }
        }

        protected void OnBusReset()
        {
            EventHandler handler = BusReset;
            if (handler != null)
            {
                handler.Invoke(this, new EventArgs());
            }
        }

        #endregion
    }
}
