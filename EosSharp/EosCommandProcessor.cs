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
    /// Command parser for text commands to an EOS bus.
    /// 
    /// TODO: Document text command format.
    /// 
    /// </summary>
    public class EosCommandProcessor
    {
        private EosBus _bus;

        public EosCommandProcessor(EosBus bus)
        {
            _bus = bus;
        }

        public void ProcessCommand(String command)
        {
            this.ProcessCommand(command.Split(' '));
        }

        public void ProcessCommand(IList<String> command)
        {
            Queue<String> queue = new Queue<string>(command);
            ProcessCommand(queue);
        }

        public void ProcessCommand(Queue<String> arguments)
        {
            string command = arguments.Dequeue().ToLower();
            switch (command)
            {
                case "reset":
                    _bus.Reset();
                    break;

                case "rescan":
                    _bus.Rescan();
                    break;

                case "start":
                    _bus.StartPolling();
                    break;

                case "stop":
                    _bus.StopPolling();
                    break;

                case "device":
                    byte address = byte.Parse(arguments.Dequeue());
                    EosDevice device = _bus.Devices.GetByAddress(address);
                    if (device != null)
                    {
                        ProcessDeviceCommand(device, arguments);
                    }
                    break;
            }
        }

        public void ProcessDeviceCommand(EosDevice device, Queue<String> arguments)
        {
            string deviceCommand = arguments.Dequeue().ToLower();
            switch (deviceCommand)
            {
                case "set":
                    ProcessSetCommand(device, arguments);
                    break;

                case "stepper":
                    ProcessStepperCommand(device, arguments);
                    break;

                case "servo":
                    ProcessServoCommand(device, arguments);
                    break;

                case "backlight":
                    ProcessLedCommand(device, arguments, null);
                    break;

                case "led":
                    byte led = byte.Parse(arguments.Dequeue());
                    ProcessLedCommand(device, arguments, led);
                    break;

                case "text":
                    ProcessTextCommand(device, arguments);
                    break;
            }
        }

        public void ProcessSetCommand(EosDevice device, Queue<string> arguments)
        {
            string setCommand = arguments.Dequeue().ToLower();
            switch (setCommand)
            {
                case "address":
                    byte newAddress = byte.Parse(arguments.Dequeue());
                    device.SetNodeAddress(newAddress);
                    break;

                case "group":
                    byte newGroup = byte.Parse(arguments.Dequeue());
                    device.SetGroup(newGroup);
                    break;

                case "name":
                    device.SetName(arguments.Dequeue());
                    break;
            }
        }

        public void ProcessTextCommand(EosDevice device, Queue<String> arguments)
        {
            byte display = byte.Parse(arguments.Dequeue());
            device.SetDisplayText(display, arguments.Dequeue());
        }

        public void ProcessLedCommand(EosDevice device, Queue<String> arguments, byte? ledId)
        {
            string ledCommand = arguments.Dequeue().ToLower();
            switch (ledCommand)
            {
                case "level":
                    byte level = byte.Parse(arguments.Dequeue());
                    if (ledId == null)
                    {
                        device.SetBacklightLevel(level);
                    }
                    else
                    {
                        device.SetLedLevel((byte)ledId, level);
                    }
                    break;

                case "on":
                    if (ledId == null)
                    {
                        device.SetBacklightPower(true);
                    }
                    else
                    {
                        device.SetLedPower((byte)ledId, true);
                    }
                    break;

                case "off":
                    if (ledId == null)
                    {
                        device.SetBacklightPower(false);
                    }
                    else
                    {
                        device.SetLedPower((byte)ledId, false);
                    }
                    break;
            }
        }

        public void ProcessServoCommand(EosDevice device, Queue<String> arguments)
        {
            byte servo = byte.Parse(arguments.Dequeue());
            string servoCommand = arguments.Dequeue().ToLower();
            switch (servoCommand)
            {
                case "value":
                    int target = int.Parse(arguments.Dequeue());
                    device.SetServoValue(servo, target);
                    break;
                case "set":
                    int minValue = int.Parse(arguments.Dequeue());
                    int maxValue = int.Parse(arguments.Dequeue());
                    int defaultValue = int.Parse(arguments.Dequeue());
                    device.SetServoConfig(servo, minValue, maxValue, defaultValue);
                    break;
                case "get":
                    device.GetServoConfig(servo);
                    break;
            }
        }

        public void ProcessStepperCommand(EosDevice device, Queue<String> arguments)
        {
            byte stepper = byte.Parse(arguments.Dequeue());
            string stepperCommand = arguments.Dequeue().ToLower();
            switch (stepperCommand)
            {
                case "target":
                    long target = long.Parse(arguments.Dequeue());
                    device.SetStepperTargetPosition(stepper, target);
                    break;
                case "zero":
                    device.ZeroStepperPosition(stepper);
                    break;
            }
        }
    }
}
