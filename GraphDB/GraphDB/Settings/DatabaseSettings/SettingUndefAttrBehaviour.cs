﻿/* <id name="GraphDB – undefined attribute behave" />
 * <copyright file="SettingUndefAttrBehaviour.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH. All rights reserved.
 * </copyright>
 * <developer>Dirk Bludau</developer>
 * <summary>set the behave of undefined attributes</summary>
 */


#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.TypeManagement.BasicTypes;
using sones.GraphFS.Session;
using sones.GraphDB.Errors;
using sones.GraphDB.Exceptions;
using sones.Lib.NewFastSerializer;

#endregion

namespace sones.GraphDB.Settings.DatabaseSettings
{
    public class SettingUndefAttrBehaviour : APersistentSetting, IDBDatabaseOnlySetting
    {

        public static readonly SettingUUID UUID = new SettingUUID(121);

        #region TypeCode
        
        public override UInt32 TypeCode { get { return 537; } }
        
        #endregion

        #region properties

        public UndefAttributeBehaviour Behaviour { get; private set; }

        #endregion

        #region constructors

        public SettingUndefAttrBehaviour()
        {
            Name            = "SETUNDEFBEHAVE";
            Description     = "Set the behave for undefined attributes.";
            Type            = DBString.UUID;
            Default         = new DBString(UndefAttributeBehaviour.allow.ToString());
            this._Value     = Default.Clone();
            Behaviour       = UndefAttributeBehaviour.allow;
        }        

        public SettingUndefAttrBehaviour(Byte[] mySerializedData)
            : base(mySerializedData)
        { }

        public SettingUndefAttrBehaviour(APersistentSetting myCopy)
            : base(myCopy)
        {
            Behaviour = ((SettingUndefAttrBehaviour)myCopy).Behaviour;
        }

        #endregion

        #region overrides        

        public override ADBBaseObject Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                if(value is DBString)
                {
                    UndefAttributeBehaviour result = UndefAttributeBehaviour.allow;
                    
                    if (!Enum.TryParse<UndefAttributeBehaviour>((String)value.Value, true, out result))
                    {
                        throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true), value.GetType().Name));
                    }

                    Behaviour = result;
                }
                else
                {
                    throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true), value.GetType().Name));
                }
                
                this._Value = new DBString();                
            }
        }

        public override SettingUUID ID
        {
            get { return UUID; }
        }

        public override ISettings Clone()
        {
            return new SettingUndefAttrBehaviour(this);
        }

        #endregion

        #region IFastSerializationTypeSurrogate Members

        public new bool SupportsType(Type type)
        {
            return GetType() == type;
        }

        public new void Serialize(SerializationWriter writer, object value)
        {
            base.Serialize(writer, this);
            writer.WriteByte((Byte)Behaviour);
        }

        public new object Deserialize(SerializationReader reader, Type type)
        {
            return MyDeserialize(reader, type);
        }

        private object MyDeserialize(SerializationReader reader, System.Type type)
        {
            var result = (SettingUndefAttrBehaviour)base.Deserialize(reader, type);
            result.Behaviour = (UndefAttributeBehaviour)reader.ReadOptimizedByte();

            return result;
        }

        #endregion
        
    }

    public enum UndefAttributeBehaviour
    { 
        allow,
        warn,
        disallow,
    }
}
