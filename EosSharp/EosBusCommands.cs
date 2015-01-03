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
    /// Enumeration of command values available on the EOS Bus.
    /// </summary>
    public enum EosBusCommands : byte
    {
        RESET = 1,
        INFO = 130,
        INFO_RESPONSE = 66,
        SET_ADDRESS = 3,
        SET_NAME = 4,
        SET_GROUP = 5,
        GET_CONFIG = 134,
        GET_CONFIG_RESPONSE = 70,
        SET_CONFIG = 7,
        BACKLIGHT_LEVEL = 8,
        BACKLIGHT_POWER = 9,
        LED_LEVEL = 10,
        LED_POWER = 11,
        SET_TEXT = 12,
        ZERO_STEPPER = 13,
        STEPPER_TARGET = 14,
        SERVO_VALUE = 15,
        SERVO_SET_CONFIG = 16,
        SERVO_GET_CONFIG = 145,
        SERVO_GET_CONFIG_RESPONSE = 81,
        STEPPER_SET_CONFIG = 18,
        STEPPER_GET_CONFIG = 147,
        STEPPER_GET_CONFIG_RESPONSE = 83,
        POLL = 191,
        POLL_RESPONSE = 127
    }
}
