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
 * IVersionedDictionaryInterface
 * Achim Friedland, 2009 - 2010
 */

#region Usings

using System;
using System.Text;
using sones.Lib.DataStructures;
using System.Collections.Generic;
using sones.Lib.DataStructures.Timestamp;

#endregion

namespace sones.Lib.DataStructures.Dictionaries
{

    /// <summary>
    /// The interface of a VersionedDictionaryObject to store a mapping TKey => DictionaryValueHistory&lt;TValue&gt;.
    /// </summary>
    /// <typeparam name="TKey">Must implement IComparable</typeparam>

    public interface IVersionedDictionaryInterface<TKey, TValue> : IDictionaryInterface<TKey, TValue>
        where TKey : IComparable
    {

        #region Replace - will fail if the key does not exist or the given timestamp != actual timestamp

        /// <summary>
        /// Replaces the value indexed by myKey with myNewValue as long as the given timestamp matches the actual timestamp.
        /// Will fail if the key is not existent or the actual timestamp is not equals myTimestamp due to concurrency conflicts.
        /// </summary>
        /// <param name="myKey">the key to access the value</param>
        /// <param name="myValue">the value storing some information</param>
        /// <returns>0 if it failed or the timestamp of the replace operation</returns>
        UInt64 ReplaceByTimestamp(TKey myKey, UInt64 myTimestamp, TValue myNewValue);

        #endregion

        #region Contains

        Trinary ContainsKey(TKey myKey, Int64 myVersion);

        Trinary ContainsValue(TValue myValue, Int64 myVersion);

        Trinary Contains(TKey myKey, TValue myValue, Int64 myVersion);

        Trinary Contains(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);

        #endregion

        #region Get/Keys/Values/Enumerator

        TValue                      this[TKey myKey, Int64 myVersion]  { get; }
        TValue                      this[TKey myKey, UInt64 myVersion] { get; }
        Boolean                     TryGetValue(TKey key, out TValue value, Int64 myVersion);

        IEnumerable<TKey>           Keys(Int64 myVersion);
        IEnumerable<TKey>           Keys(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);
        UInt64                      KeyCount(Int64 myVersion);
        UInt64                      KeyCount(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);

        IEnumerable<TValue>         Values(Int64 myVersion);
        IEnumerable<TValue>         Values(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);
        UInt64                      ValueCount(Int64 myVersion);
        UInt64                      ValueCount(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);

        IDictionary<TKey, TValue>   GetIDictionary(Int64 myVersion);
        IDictionary<TKey, TValue>   GetIDictionary(Int64 myVersion, params TKey[] myKeys);
        IDictionary<TKey, TValue>   GetIDictionary(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);

        IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator(Int64 myVersion);
        IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator(Func<KeyValuePair<TKey, TValue>, Boolean> myFunc, Int64 myVersion);

        #endregion

        #region Additional methods

        TimestampValuePair<TValue> GetTimestampValuePair(TKey myKey);

        UInt64 VersionCount(TKey myKey);

        void ClearHistory(TKey myKey);

        #endregion

    }

}