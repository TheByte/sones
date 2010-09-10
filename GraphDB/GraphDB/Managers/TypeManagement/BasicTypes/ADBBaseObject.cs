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

/* <id name="GraphDB � ADBBaseObject" />
 * <copyright file="ADBBaseObject.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH. All rights reserved.
 * </copyright>
 * <developer>Stefan Licht</developer>
 * <summary>This is the base object. Each DataObject like Int, String, Double must derive from this class.</summary>
 */

using System;
using sones.Lib.NewFastSerializer;
using sones.Lib.Serializer;
using sones.GraphDBInterface.TypeManagement;

namespace sones.GraphDB.TypeManagement.BasicTypes
{

    public enum DBObjectInitializeType : byte
    {
        Default,
        MinValue,
        MaxValue
    }

//    [Serializable]
    public abstract class ADBBaseObject : IObject, IFastSerialize, IFastSerializationTypeSurrogate
    {

        public abstract ADBBaseObject Add(ADBBaseObject myGraphObjectA, ADBBaseObject myGraphObjectB);
        public abstract ADBBaseObject Sub(ADBBaseObject myGraphObjectA, ADBBaseObject myGraphObjectB);
        public abstract ADBBaseObject Mul(ADBBaseObject myGraphObjectA, ADBBaseObject myGraphObjectB);
        public abstract ADBBaseObject Div(ADBBaseObject myGraphObjectA, ADBBaseObject myGraphObjectB);

        public abstract void Add(ADBBaseObject myGraphObject);
        public abstract void Sub(ADBBaseObject myGraphObject);
        public abstract void Mul(ADBBaseObject myGraphObject);
        public abstract void Div(ADBBaseObject myGraphObject);

        #region IComparable Members

        public abstract int CompareTo(ADBBaseObject obj);
        public abstract int CompareTo(object obj);

        #endregion

        public abstract BasicType Type { get; } 

        public abstract void SetValue(DBObjectInitializeType myDBObjectInitializeType);
        public abstract void SetValue(Object myValue);

        //public abstract TypeUUID ID { get; }
        public abstract string ObjectName { get; }

        /// <summary>
        /// Create an exact clone
        /// </summary>
        /// <returns></returns>
        public abstract ADBBaseObject Clone();

        /// <summary>
        /// Create a clone and set the value
        /// </summary>
        /// <param name="myValue">The new value for this cloned type</param>
        /// <returns></returns>
        public abstract ADBBaseObject Clone(Object myValue);

        public abstract Object Value
        {
            get;
            set;
        }

        public abstract Boolean IsValidValue(Object myValue);

        public override string ToString()
        {
            return Value.ToString();// +" {" + Value.GetType().Name + "}";
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public abstract String ToString(IFormatProvider provider);

        public override bool Equals(object obj)
        {
            if (obj is ADBBaseObject)
                return Value.Equals(((ADBBaseObject)obj).Value);
            else
                return Value.Equals(obj);

        }
        #region IFastSerialize Members

        public bool isDirty
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public DateTime ModificationTime
        {
            get { throw new NotImplementedException(); }
        }

        public abstract void Serialize(ref SerializationWriter mySerializationWriter);

        public abstract void Deserialize(ref SerializationReader mySerializationReader);

        #endregion

        #region IFastSerializationTypeSurrogate Members

        public abstract bool SupportsType(Type type);

        public abstract void Serialize(SerializationWriter writer, object value);

        public abstract object Deserialize(SerializationReader reader, Type type);

        public abstract UInt32 TypeCode { get; }

        #endregion

    }
}
