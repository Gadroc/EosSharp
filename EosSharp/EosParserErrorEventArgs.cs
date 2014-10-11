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
    /// Event handler for EOS bus parsing errors.
    /// </summary>
    /// <param name="sender">EOS Parser which encountered the error.</param>
    /// <param name="e">Error arguments.</param>
    public delegate void EosParserErrorEventHandler(object sender, EosParserErrorEventArgs e);

    /// <summary>
    /// Arguments for a EosParserError event. 
    /// </summary>
    public class EosParserErrorEventArgs : EventArgs
    {
        private EosParserError _error;

        /// <summary>
        /// Creates an arguments object for an Eos Parser error event.
        /// </summary>
        /// <param name="error">Error code associated with this event.</param>
        public EosParserErrorEventArgs(EosParserError error)
        {
            _error = error;
        }

        /// <summary>
        /// Returns the error encountered which triggered this event.
        /// </summary>
        public EosParserError Error { get { return _error; } }
    }
}
