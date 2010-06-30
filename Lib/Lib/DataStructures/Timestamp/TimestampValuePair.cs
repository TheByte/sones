﻿/*
* sones GraphDB - OpenSource Graph Database - http://www.sones.com
* Copyright (C) 2007-2010 sones GmbH
*
* This file is part of sones GraphDB OpenSource Edition.
*
* sones GraphDB OSE is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
*
* sones GraphDB OSE is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB OSE. If not, see <http://www.gnu.org/licenses/>.
*/


/* 
 * TimestampValuePair
 * Achim Friedland, 2009
 */

#region Usings

using System;

#endregion

namespace sones.Lib.DataStructures.Timestamp
{

    /// <summary>
    /// This datastructure implements a timestamped value
    /// to be used within single-value datastructures
    /// </summary>
    /// <typeparam name="TValue">The type of the stored value</typeparam>

    public class TimestampValuePair<TValue>
    {

        #region Properties

        #region Timestamp

        private UInt64 _Timestamp;

        public UInt64 Timestamp
        {
            get
            {
                return _Timestamp;
            }
        }

        #endregion

        #region Value

        private TValue _Value;

        public TValue Value
        {
            get
            {
                return _Value;
            }
        }

        #endregion

        #endregion


        #region Constructors

        #region TimestampValuePair()

        /// <summary>
        /// Creates a new TimestampValuePair using default values
        /// </summary>
        public TimestampValuePair()
        {
            _Timestamp          = sones.Lib.DataStructures.Timestamp.TimestampNonce.Ticks;
            _Value              = default(TValue);
        }

        #endregion

        #region TimestampValuePair(myValue)

        /// <summary>
        /// Creates a new TimestampValuePair, setting the internal value to the content
        /// of myValue and using the actual time as internal timestamp
        /// </summary>
        /// <param name="myValue">The actual value</param>
        public TimestampValuePair(TValue myValue)
        {
            _Timestamp          = sones.Lib.DataStructures.Timestamp.TimestampNonce.Ticks;
            _Value              = myValue;
        }

        #endregion

        #region TimestampValuePair(myTimestamp, myValue)

        /// <summary>
        /// Creates a new TimestampValuePair, setting the internal value to the content of myValue
        /// and setting the interal timestamp to the content of myTimestamp
        /// </summary>
        /// <param name="myTimestamp">The timestamp of the value</param>
        /// <param name="myValue">The actual value</param>
        public TimestampValuePair(UInt64 myTimestamp, TValue myValue)
        {
            _Timestamp          = myTimestamp;
            _Value              = myValue;
        }

        #endregion

        #endregion


        #region ToString

        public override String ToString()
        {

            if (_Value != null)
                return _Timestamp.ToString() + " : " + _Value.ToString() + " (" + typeof(TValue).Name + ")";

            return _Timestamp.ToString() + " : <null>" + " (" + typeof(TValue).Name + ")";

        }

        #endregion

        #region GetHashCode()

        public override Int32 GetHashCode()
        {
            return Timestamp.GetHashCode() ^ Value.GetHashCode();
        }

        #endregion


    }

}
