/*
* sones GraphDB - Open Source Edition - http://www.sones.com
* Copyright (C) 2007-2010 sones GmbH
*
* This file is part of sones GraphDB Open Source Edition (OSE).
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
* 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sones.Lib.NewFastSerializer
{

    /// <summary>
    /// Interface to allow helper classes to be used to serialize objects
    /// that are not directly supported by SerializationWriter/SerializationReader
    /// </summary>
    public interface IFastSerializationTypeSurrogate
    {
        /// <summary>
        /// Allows a surrogate to be queried as to whether a particular type is supported
        /// </summary>
        /// <param name="type">The type being queried</param>
        /// <returns>true if the type is supported; otherwise false</returns>
        bool SupportsType(Type type);
        /// <summary>
        /// FastSerializes the object into the SerializationWriter.
        /// </summary>
        /// <param name="writer">The SerializationWriter into which the object is to be serialized.</param>
        /// <param name="value">The object to serialize.</param>
        void Serialize(SerializationWriter writer, object value);
        /// <summary>
        /// Deserializes an object of the supplied type from the SerializationReader.
        /// </summary>
        /// <param name="reader">The SerializationReader containing the serialized object.</param>
        /// <param name="type">The type of object required to be deserialized.</param>
        /// <returns></returns>
        object Deserialize(SerializationReader reader, Type type);

        UInt32 TypeCode { get; }

    }

}
