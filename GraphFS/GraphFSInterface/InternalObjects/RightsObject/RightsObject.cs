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


/* <id Name=”PandoraFS – RightsObject” />
 * <copyright file=”RightsObject.cs”
 *            company=”sones GmbH”>
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Henning Rauch</developer>
 * <summary>The RightsObject carries information about a bunch of Rights<summary>
 */

#region Usings

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using sones.Lib;
using sones.Lib.BTree;
using sones.Lib.Serializer;

using sones.Lib.Cryptography.IntegrityCheck;
using sones.Lib.Cryptography.SymmetricEncryption;
using sones.Lib.DataStructures;
using sones.GraphFS.Objects;
using sones.Lib.DataStructures.UUID;
using sones.GraphFS.DataStructures;

#endregion

namespace sones.GraphFS.InternalObjects
{

    /// <summary>
    /// The RightsObject carries information about a bunch of Rights.
    /// </summary>

    
    public class RightsObject : ADictionaryObject<RightUUID, Right>
    {


        #region Constructor

        #region RightsObject()

        /// <summary>
        /// This will create an empty RightsObject
        /// </summary>
        public RightsObject()
        {

            // Members of APandoraStructure
            _StructureVersion   = 1;

            // Members of APandoraObject
            _ObjectStream   = FSConstants.RIGHTSSTREAM;

            // Object specific data...

        }

        #endregion

        #region RightsObject(myObjectLocation, mySerializedData, myIntegrityCheckAlgorithm, myEncryptionAlgorithm)

        /// <summary>
        /// A constructor used for fast deserializing
        /// </summary>
        /// <param name="myObjectLocation">the location of the RightsObject (constisting of the ObjectPath and ObjectName) within the file system</param>
        /// <param name="mySerializedData">An array of bytes[] containing the serialized RightsObject</param>
        public RightsObject(ObjectLocation myObjectLocation, Byte[] mySerializedData, IIntegrityCheck myIntegrityCheckAlgorithm, ISymmetricEncryption myEncryptionAlgorithm)
        {

            if (mySerializedData == null || mySerializedData.Length == 0)
                throw new ArgumentNullException("mySerializedData must not be null or its length be zero!");

            Deserialize(mySerializedData, myIntegrityCheckAlgorithm, myEncryptionAlgorithm, false);
            _isNew = false;

        }

        #endregion

        #endregion


        #region Members of APandoraObject

        #region Clone()

        public override AFSObject Clone()
        {

            var newT = new RightsObject();
            newT.Deserialize(Serialize(null, null, false), null, null, this);

            return newT;

        }

        #endregion

        #endregion


        #region Object-specific methods

        #region AddRight(myRight)

        public void AddRight(Right myRight)
        {
            Add(myRight.UUID, myRight);
        }

        #endregion

        #region ContainsRight(myRightUUID)

        public Trinary ContainsRight(RightUUID myRightUUID)
        {
            return ContainsKey(myRightUUID);
        }

        #endregion

        #region ContainsName(myLogin)

        public Boolean ContainsName(String myName)
        {
            return ListOfNames.Contains(myName);
        }

        #endregion


        //ToDo: Remove this!
        #region this[String myRightUUID]

        public new Right this[RightUUID myRightUUID]
        {

            get 
            {
                return base[myRightUUID];
            }

            set 
            {
                base[myRightUUID] = value;
                isDirty                = true;
            }

        }

        #endregion

        #region GetRightByName(myRightName)

        public Right GetRightByName(String myRightName)
        {

            foreach (Right _Right in this.Values())
                if (_Right.RightsName.Equals(myRightName))
                    return _Right;

            return null;

        }

        #endregion

        #region ListOfNames

        public List<String> ListOfNames
        {

            get
            {

                var _ListOfNames = new List<String>();

                foreach (var _Right in this.Values())
                    _ListOfNames.Add(_Right.RightsName);

                return _ListOfNames;

            }

        }

        #endregion

        #region ListOfUUIDs

        public List<UUID> ListOfUUIDs
        {

            get
            {

                var ListOfUUIDs = new List<UUID>();

                foreach (var _Right in this.Values())
                    ListOfUUIDs.Add(_Right.UUID);

                return ListOfUUIDs;

            }

        }

        #endregion

        #region ListOfRights

        public List<Right> ListOfRights
        {

            get
            {
                var _ListOfRights = new List<Right>();

                foreach (var _Right in this.Values())
                    _ListOfRights.Add(_Right);

                return _ListOfRights;

            }

        }

        #endregion


        #region GetEnumerator()

        public IEnumerator<KeyValuePair<RightUUID, Right>> GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IsUserDefined(myLogin)

        public Boolean IsUserDefined(RightUUID myRightUUID)
        {
            return this[myRightUUID].IsUserDefined;
        }

        #endregion


        #region RemoveRight(myRight)

        public Boolean RemoveRight(RightUUID myRightUUID)
        {
            return Remove(myRightUUID);
        }

        #endregion

        #endregion
     

    }

}