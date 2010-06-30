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


/* <id name="sones GraphDB – DBInt64" />
 * <copyright file="DBInt64.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Stefan Licht</developer>
 * <summary>The Int64.</summary>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.QueryLanguage.Enums;

using sones.Lib.NewFastSerializer;
using sones.GraphFS.DataStructures;
using sones.Lib;

namespace sones.GraphDB.TypeManagement.PandoraTypes
{
    
    public class DBInt64 : DBNumber
    {
        public new static readonly TypeUUID UUID = new TypeUUID(1030);
        public new const string Name = DBConstants.DBInteger;

        #region TypeCode 
        public override UInt32 TypeCode { get { return 406; } }
        #endregion

        #region Data

        private Int64 _Value;

        #endregion

        #region Constructors

        public DBInt64()
        {
            _Value = 0;
        }
        
        public DBInt64(DBObjectInitializeType myDBObjectInitializeType)
        {
            SetValue(myDBObjectInitializeType);
        }

        public DBInt64(Object myValue)
        {
            Value = myValue;
        }

        public DBInt64(Int64 myValue)
        {
            _Value = myValue;
        }

        #endregion

        #region Overrides

        public override int CompareTo(ADBBaseObject obj)
        {
            return CompareTo(obj.Value);
        }

        public override int CompareTo(object obj)
        {
            Int64 val;
            if (obj is DBInt64)
                val = (Int64)((DBInt64)obj).Value;
            else
                val = Convert.ToInt64(obj);
            return _Value.CompareTo(val);
        }

        public override object Value
        {
            get { return _Value; }
            set
            {
                if (value is DBInt64)
                {
                    _Value = ((DBInt64)value)._Value;
                }
                else
                {
                    if (value is ADBBaseObject)
                    {
                        _Value = Convert.ToInt64(((ADBBaseObject)value).Value);
                    }
                    else
                    {
                        if (value is ObjectUUID)
                        {
                            _Value = Convert.ToInt64(((ObjectUUID)value).ToString());
                        }
                        else
                        {
                            _Value = Convert.ToInt64(value);
                        }
                    }
                }
            }
        }

        #endregion

        #region Operations

        public static DBInt64 operator +(DBInt64 myPandoraObjectA, Int64 myValue)
        {
            myPandoraObjectA.Value = (Int64)myPandoraObjectA.Value + myValue;
            return myPandoraObjectA;
        }

        public static DBInt64 operator -(DBInt64 myPandoraObjectA, Int64 myValue)
        {
            myPandoraObjectA.Value = (Int64)myPandoraObjectA.Value - myValue;
            return myPandoraObjectA;
        }

        public static DBInt64 operator *(DBInt64 myPandoraObjectA, Int64 myValue)
        {
            myPandoraObjectA.Value = (Int64)myPandoraObjectA.Value * myValue;
            return myPandoraObjectA;
        }

        public static DBInt64 operator /(DBInt64 myPandoraObjectA, Int64 myValue)
        {
            myPandoraObjectA.Value = (Int64)myPandoraObjectA.Value / myValue;
            return myPandoraObjectA;
        }

        public override ADBBaseObject Add(ADBBaseObject myPandoraObjectA, ADBBaseObject myPandoraObjectB)
        {

            Int64 valA = Convert.ToInt64(myPandoraObjectA.Value);
            Int64 valB = Convert.ToInt64(myPandoraObjectB.Value);
            return new DBInt64(valA + valB);
        }

        public override ADBBaseObject Sub(ADBBaseObject myPandoraObjectA, ADBBaseObject myPandoraObjectB)
        {
            Int64 valA = Convert.ToInt64(myPandoraObjectA.Value);
            Int64 valB = Convert.ToInt64(myPandoraObjectB.Value);
            return new DBInt64(valA - valB);
        }

        public override ADBBaseObject Mul(ADBBaseObject myPandoraObjectA, ADBBaseObject myPandoraObjectB)
        {
            Int64 valA = Convert.ToInt64(myPandoraObjectA.Value);
            Int64 valB = Convert.ToInt64(myPandoraObjectB.Value);
            return new DBInt64(valA * valB);
        }

        public override ADBBaseObject Div(ADBBaseObject myPandoraObjectA, ADBBaseObject myPandoraObjectB)
        {
            Int64 valA = Convert.ToInt64(myPandoraObjectA.Value);
            Int64 valB = Convert.ToInt64(myPandoraObjectB.Value);
            return new DBInt64(valA / valB);
        }

        public override void Add(ADBBaseObject myPandoraObject)
        {
            _Value += Convert.ToInt64(myPandoraObject.Value);
        }

        public override void Sub(ADBBaseObject myPandoraObject)
        {
            _Value -= Convert.ToInt64(myPandoraObject.Value);
        }

        public override void Mul(ADBBaseObject myPandoraObject)
        {
            _Value *= Convert.ToInt64(myPandoraObject.Value);
        }

        public override void Div(ADBBaseObject myPandoraObject)
        {
            _Value /= Convert.ToInt64(myPandoraObject.Value);
        }

        #endregion

        #region IsValid

        public new static Boolean IsValid(Object myObject)
        {
            if (myObject == null) return false;

            Int64 newValue;
            Int64.TryParse(myObject.ToString(), out newValue);

            return myObject.ToString() == newValue.ToString();
        }

        public override bool IsValidValue(Object myValue)
        {
            return DBInt64.IsValid(myValue);
        }

        #endregion

        #region Clone

        public override ADBBaseObject Clone()
        {
            return new DBInt64(_Value);
        }

        public override ADBBaseObject Clone(Object myValue)
        {
            return new DBInt64(myValue);
        }

        #endregion

        public override void SetValue(DBObjectInitializeType myDBObjectInitializeType)
        {
            switch (myDBObjectInitializeType)
            {
                case DBObjectInitializeType.Default:
                    _Value = 0;
                    break;
                case DBObjectInitializeType.MinValue:
                    _Value = Int64.MinValue;
                    break;
                case DBObjectInitializeType.MaxValue:
                    _Value = Int64.MaxValue;
                    break;
                default:
                    _Value = 0;
                    break;
            }
        }

        public override void SetValue(object myValue)
        {
            Value = myValue;
        }

        public override TypesOfOperatorResult Type
        {
            get { return TypesOfOperatorResult.Int64; }
        }


        public override TypeUUID ID
        {
            get { return UUID; }
        }

        public override string ObjectName
        {
            get { return Name; }
        }
    
        #region IFastSerialize Members

        public override void Serialize(ref SerializationWriter mySerializationWriter)
        {
            Serialize(ref mySerializationWriter, this);
        }

        public override void Deserialize(ref SerializationReader mySerializationReader)
        {
            Deserialize(ref mySerializationReader, this);
        }

        #endregion

        private void Serialize(ref SerializationWriter mySerializationWriter, DBInt64 myValue)
        {
            mySerializationWriter.WriteObject(myValue._Value);
        }

        private object Deserialize(ref SerializationReader mySerializationReader, DBInt64 myValue)
        {
            myValue._Value = (Int64)mySerializationReader.ReadObject();
            return myValue;
        }

        #region IFastSerializationTypeSurrogate
        public override bool SupportsType(Type type)
        {
            return this.GetType() == type;
        }

        public override void Serialize(SerializationWriter writer, object value)
        {
            Serialize(ref writer, (DBInt64)value);
        }

        public override object Deserialize(SerializationReader reader, Type type)
        {
            DBInt64 thisObject = (DBInt64)Activator.CreateInstance(type);
            return Deserialize(ref reader, thisObject);
        }

        #endregion

        public override string ToString(IFormatProvider provider)
        {
            return _Value.ToString(provider);
        }

    }
}
