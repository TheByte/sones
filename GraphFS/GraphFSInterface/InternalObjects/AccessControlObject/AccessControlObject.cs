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

/* <id Name=�GraphFS � AccessControlObject� />
 * <copyright file=�AccessControlObject.cs�
 *            company=�sones GmbH�>
 * Copyright (c) sones GmbH. All rights reserved.
 * </copyright>
 * <developer>Henning Rauch</developer>
 * <developer>Achim Friedland</developer>
 * <summary>This implements the data structure for handling (access-)
 * rights on file system objects.<summary>
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
using sones.Lib.NewFastSerializer;
using sones.Lib.DataStructures;
using sones.GraphFS.Objects;
using sones.GraphFS.DataStructures;
using sones.GraphFS.Exceptions;

#endregion

namespace sones.GraphFS.InternalObjects
{

    /// <summary>
    /// This implements the data structure for handling (access-)
    /// rights on file system objects.
    /// </summary>
    public class AccessControlObject : AFSObject
    {


        #region Properties

        #region Estimated Size

        private UInt64 _estimatedSize = 0;

        #endregion

        #region DefaultRule

        private DefaultRuleTypes _DefaultRule = DefaultRuleTypes.ALLOW_OVER_DENY;

        /// <summary>
        /// This property defines the priority of allowing and denying.
        /// </summary>
        public DefaultRuleTypes DefaultRule
        {
            
            get
            {
                return _DefaultRule;
            }
            
            set
            {
                _DefaultRule = value;
                isDirty = true;
            }
        
        }

        #endregion

        #region AllowACL

        private Dictionary<RightUUID, HashSet<EntityUUID>> _AllowACL = new Dictionary<RightUUID,HashSet<EntityUUID>>();

        /// <summary>
        /// the dictionary of rights with corresponding ACLs that allow the 
        /// access to an object in the GraphFS
        /// </summary>
        public Dictionary<RightUUID, HashSet<EntityUUID>> AllowACL
        {

            get
            {
                return _AllowACL;
            }
        
        }

        #endregion

        #region DenyACL

        private Dictionary<RightUUID, HashSet<EntityUUID>> _DenyACL = new Dictionary<RightUUID,HashSet<EntityUUID>>();

        /// <summary>
        /// the dictionary of rights with corresponding ACLs that denies the 
        /// access to an object in the GraphFS
        /// </summary>
        public Dictionary<RightUUID, HashSet<EntityUUID>> DenyACL
        {

            get
            {
                return _DenyACL;
            }
        
        }

        #endregion

        #region NotificationHandling

        private Object _NotificationHandlingLock = new Object();

        private NHAccessControlObject _NotificationHandling = 0;

        /// <summary>
        /// Returns the NotificationHandling bitfield that indicates which
        /// notifications should be triggered.
        /// </summary>
        public NHAccessControlObject NotificationHandling
        {

            get
            {
                return _NotificationHandling;
            }

        }

        #endregion

        #endregion


        #region Constructor

        #region AccessControlObject()

        /// <summary>
        /// This will create an empty AccessControlObject
        /// </summary>
        public AccessControlObject()
        {

            // Members of AGraphStructure
            _StructureVersion   = 1;

            // Members of AGraphObject
            _ObjectStream = FSConstants.ACCESSCONTROLSTREAM;

            // Object specific data...

        }

        #endregion

        #region AccessControlObject(myObjectLocation, myAllowACL, myDenyACL, myDefaultRule, NotificationHandlingList)

        /// <summary>
        /// The constructor for creating a new AccessControlObject with certain rights.
        /// </summary>
        /// <param name="myObjectLocation">the location of the AccessControlObject (constisting of the ObjectPath and ObjectName) within the file system</param>
        /// <param name="myDefaultRule">This property defines the priority of allowing and denying.</param>
        /// <param name="myAllowACL">The dictionary of rights with corresponding ACLs that allow the access to an object in the GraphFS.</param>
        /// <param name="myDenyACL">The dictionary of rights with corresponding ACLs that denies the access to an object in the GraphFS.</param>
        /// <param name="myNotificationHandling">the NotificationHandling bitfield</param>
        public AccessControlObject(ObjectLocation myObjectLocation, DefaultRuleTypes myDefaultRule, Dictionary<RightUUID, HashSet<EntityUUID>> myAllowACL, Dictionary<RightUUID, HashSet<EntityUUID>> myDenyACL, NHAccessControlObject myNotificationHandling)
        {

            if (myAllowACL == null)
                throw new ArgumentNullException("Invalid AllowACL!");

            if (myDenyACL == null)
                throw new ArgumentNullException("Invalid DenyACL!");

            _AllowACL               = myAllowACL;
            _DenyACL                = myDenyACL;
            _DefaultRule            = myDefaultRule;
            _NotificationHandling   = myNotificationHandling;

            #region calc estimated size

            #region AllowACL

            _estimatedSize += CalcACLSize(myAllowACL);

            #endregion

            #region DenyACL

            _estimatedSize += CalcACLSize(myDenyACL);

            #endregion

            _estimatedSize += EstimatedSizeConstants.EnumByte + EstimatedSizeConstants.EnumUInt64 + GetClassDefaultSize();

            #endregion
        }

        #endregion

        #region AccessControlObject(myObjectLocation, mySerializedData, myIntegrityCheckAlgorithm, myEncryptionAlgorithm)

        /// <summary>
        /// A constructor used for fast deserializing
        /// </summary>
        /// <param name="myObjectLocation">the location of the AccessControlObject (constisting of the ObjectPath and ObjectName) within the file system</param>
        /// <param name="mySerializedData">An array of bytes[] containing the serialized AccessControlObject</param>
        public AccessControlObject(ObjectLocation myObjectLocation, Byte[] mySerializedData, IIntegrityCheck myIntegrityCheckAlgorithm, ISymmetricEncryption myEncryptionAlgorithm)
        {

            if (mySerializedData == null || mySerializedData.Length == 0)
                throw new ArgumentNullException("mySerializedData must not be null or its length be zero!");

            Deserialize(mySerializedData, myIntegrityCheckAlgorithm, myEncryptionAlgorithm, false);
            _isNew = false;

        }

        #endregion

        #endregion


        #region Members of AGraphObject

        #region Clone()

        public override AFSObject Clone()
        {

            var newT = new AccessControlObject();
            newT.Deserialize(Serialize(null, null, false), null, null, this);

            return newT;

        }

        #endregion

        #endregion


        #region Object-specific methods

        #region AddToAllowACL(myEntityUUID, myRightUUID)

        /// <summary>
        /// Add an entity to the AllowACL
        /// </summary>
        /// <param name="myEntityUUID">The UUID which references the entity.</param>
        /// <param name="myRightUUID">The UUID which references the right to be granted.</param>
        public void AddToAllowACL(EntityUUID myEntityUUID, RightUUID myRightUUID)
        {

            if (myEntityUUID == null)
                throw new ArgumentNullException("The EntityUUID must not be null!");

            if (myRightUUID == null)
                throw new ArgumentNullException("The RightUUID must not be null!");


            lock (_AllowACL)
            {

                if (_AllowACL.ContainsKey(myRightUUID))
                {
                    _AllowACL[myRightUUID].Add(myEntityUUID);
                }
                else
                {
                    // Create new RightKey and empty HashSet within the AllowACL
                    HashSet<EntityUUID> _HashSet = new HashSet<EntityUUID>();
                    _HashSet.Add(myEntityUUID);
                    _AllowACL.Add(myRightUUID, _HashSet);

                    _estimatedSize += EstimatedSizeConstants.HashSet;
                    _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(myRightUUID);
                }

                _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(myEntityUUID);

            }

            isDirty = true;

        }

        #endregion

        #region AllowACL_RemoveEntity(myEntityUUID, myRightUUID)

        /// <summary>
        /// Removes an entity from the AllowACL
        /// </summary>
        /// <param name="myEntityUUID">The UUID which references the entity.</param>
        /// <param name="myRightUUID">The UUID which references the right to be denied.</param>
        public void RemoveFromAllowACL(EntityUUID myEntityUUID, RightUUID myRightUUID)
        {

            if (myEntityUUID == null)
                throw new ArgumentNullException("The EntityUUID must not be null!");

            if (myRightUUID == null)
                throw new ArgumentNullException("The RightUUID must not be null!");


            lock (_AllowACL)
            {

                if (_AllowACL.ContainsKey(myRightUUID))
                {
                    if (_AllowACL[myRightUUID].Remove(myEntityUUID))
                    {
                        _estimatedSize -= EstimatedSizeConstants.CalcUUIDSize(myEntityUUID);
                    }
                }

            }

            isDirty = true;

        }
        
        #endregion


        #region AddToDenyACL(myEntityUUID, myRightUUID)

        /// <summary>
        /// Add an entity to the DenyACL
        /// </summary>
        /// <param name="myEntityUUID">The UUID which references the entity.</param>
        /// <param name="myRightUUID">The UUID which references the right to be granted.</param>
        public void AddToDenyACL(EntityUUID myEntityUUID, RightUUID myRightUUID)
        {

            if (myEntityUUID == null)
                throw new ArgumentNullException("The EntityUUID must not be null!");

            if (myRightUUID == null)
                throw new ArgumentNullException("The RightUUID must not be null!");


            lock (_DenyACL)
            {

                if (_DenyACL.ContainsKey(myRightUUID))
                    _DenyACL[myRightUUID].Add(myEntityUUID);

                else
                {
                    // Create new RightKey and empty HashSet within the AllowACL
                    HashSet<EntityUUID> _HashSet = new HashSet<EntityUUID>();
                    _HashSet.Add(myEntityUUID);
                    _DenyACL.Add(myRightUUID, _HashSet);

                    _estimatedSize += EstimatedSizeConstants.HashSet;
                    _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(myRightUUID);
                }

                _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(myEntityUUID);
            }

            isDirty = true;
        }

        #endregion

        #region RemoveFromDenyACL(myEntityUUID, myRightUUID)

        /// <summary>
        /// Removes an entity from the AllowACL
        /// </summary>
        /// <param name="myEntityUUID">The UUID which references the entity.</param>
        /// <param name="myRightUUID">The UUID which references the right to be denied.</param>
        public void RemoveFromDenyACL(EntityUUID myEntityUUID, RightUUID myRightUUID)
        {

            if (myEntityUUID == null)
                throw new ArgumentNullException("The EntityUUID must not be null!");

            if (myRightUUID == null)
                throw new ArgumentNullException("The RightUUID must not be null!");


            lock (_DenyACL)
            {

                if (_DenyACL.ContainsKey(myRightUUID))
                {
                    if (_DenyACL[myRightUUID].Remove(myEntityUUID))
                    {
                        _estimatedSize -= EstimatedSizeConstants.CalcUUIDSize(myEntityUUID);
                    }
                }

            }

            isDirty = true;

        }
        
        #endregion


        #region SubscribeNotifications(myNotificationHandling)

        /// <summary>
        /// This method adds the given NotificationHandling flags.
        /// </summary>
        /// <param name="myNotificationHandling">The NotificationHandlings to be added.</param>
        public void SubscribeNotifications(NHAccessControlObject myNotificationHandling)
        {

            lock (_NotificationHandlingLock)
            {
                _NotificationHandling |= myNotificationHandling;
            }

        }

        #endregion

        #region UnsubscribeNotifications(myNotificationHandling)

        /// <summary>
        /// This method removes the given NotificationHandling flags.
        /// </summary>
        /// <param name="myNotificationHandling">The NotificationHandlings to be removed.</param>
        public void UnsubscribeNotifications(NHAccessControlObject myNotificationHandling)
        {

            lock (_NotificationHandlingLock)
            {
                _NotificationHandling &= ~myNotificationHandling;
            }

        }

        #endregion

        #endregion

        #region IFastSerialize

        public override void Serialize(ref SerializationWriter mySerializationWriter)
        {

            try
            {

                #region NotificationHandling

                mySerializationWriter.WriteByte((Byte)_NotificationHandling);

                #endregion

                #region DefaultRule

                mySerializationWriter.WriteByte((Byte)_DefaultRule);

                #endregion

                #region AllowACL

                // Write number of AllowACL items
                mySerializationWriter.WriteUInt64((UInt64)_AllowACL.Count);

                foreach (KeyValuePair<RightUUID, HashSet<EntityUUID>> _KeyValuePair in _AllowACL)
                {

                    // KEY: RightUUID
                    mySerializationWriter.Write(_KeyValuePair.Key.GetByteArray());

                    // VALUE:
                    // Write number of HashSet elements
                    mySerializationWriter.WriteUInt64((UInt64)_KeyValuePair.Value.Count);

                    // Write the elements itself
                    foreach (EntityUUID _HashItem in _KeyValuePair.Value)
                        mySerializationWriter.Write(_HashItem.GetByteArray());

                }

                #endregion

                #region DenyACL

                // Write number of DenyACL items
                mySerializationWriter.WriteUInt64((UInt64)_DenyACL.Count);

                foreach (KeyValuePair<RightUUID, HashSet<EntityUUID>> _KeyValuePair in _DenyACL)
                {

                    // KEY:
                    mySerializationWriter.Write(_KeyValuePair.Key.GetByteArray());

                    // VALUE:
                    // Write number of HashSet elements
                    mySerializationWriter.WriteUInt64((UInt64)_KeyValuePair.Value.Count);

                    // Write the elements itself
                    foreach (EntityUUID _HashItem in _KeyValuePair.Value)
                        mySerializationWriter.Write(_HashItem.GetByteArray());

                }

                #endregion

            }

            catch (SerializationException e)
            {
                throw new SerializationException(e.Message);
            }

        }

        public override void Deserialize(ref SerializationReader mySerializationReader)
        {

            #region Data

            UInt64               _NumberOfACLs;
            RightUUID            _RightUUID;
            EntityUUID           _EntityUUID;
            UInt64               _NumberOfEntityUUIDs;
            HashSet<EntityUUID>  _EntityUUIDHashSet;

            #endregion

            _estimatedSize = 0;

            try
            {

                #region NotificationHandling

                _NotificationHandling = (NHAccessControlObject)mySerializationReader.ReadOptimizedByte();

                #region Estimated size

                _estimatedSize += EstimatedSizeConstants.EnumUInt64;

                #endregion

                #endregion

                #region DefaultRule

                _DefaultRule = (DefaultRuleTypes)mySerializationReader.ReadOptimizedByte();

                #region Estimated size

                _estimatedSize += EstimatedSizeConstants.EnumByte;

                #endregion

                #endregion

                #region AllowACL

                #region Estimated size

                _estimatedSize += EstimatedSizeConstants.Dictionary;

                #endregion

                _AllowACL = new Dictionary<RightUUID, HashSet<EntityUUID>>();

                _NumberOfACLs = mySerializationReader.ReadUInt64();

                for (UInt64 i=0; i < _NumberOfACLs; i++)
                {

                    #region KEY

                    _RightUUID = new RightUUID(mySerializationReader.ReadByteArray());

                    #region Estimated size

                    _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(_RightUUID);

                    #endregion

                    #endregion

                    #region VALUE

                    #region Estimated size

                    _estimatedSize += EstimatedSizeConstants.HashSet;

                    #endregion

                    _EntityUUIDHashSet = new HashSet<EntityUUID>();

                    _NumberOfEntityUUIDs = mySerializationReader.ReadUInt64();

                    for (UInt64 j=0; j < _NumberOfEntityUUIDs; j++)
                    {

                        _EntityUUID = new EntityUUID(mySerializationReader.ReadByteArray());
                        _EntityUUIDHashSet.Add(_EntityUUID);

                        #region Estimated size

                        _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(_EntityUUID);

                        #endregion
                    }

                    // Finally... add it to the AllowACL!
                    _AllowACL.Add(_RightUUID, _EntityUUIDHashSet);

                    #endregion

                }

                #endregion

                #region DenyACL

                #region Estimated size

                _estimatedSize += EstimatedSizeConstants.Dictionary;

                #endregion

                _DenyACL = new Dictionary<RightUUID, HashSet<EntityUUID>>();

                _NumberOfACLs = mySerializationReader.ReadUInt64();

                for (UInt64 i=0; i < _NumberOfACLs; i++)
                {

                    #region KEY

                    _RightUUID = new RightUUID(mySerializationReader.ReadByteArray());

                    #region Estimated size

                    _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(_RightUUID);

                    #endregion

                    #endregion

                    #region VALUE

                    #region Estimated size

                    _estimatedSize += EstimatedSizeConstants.HashSet;

                    #endregion

                    _EntityUUIDHashSet = new HashSet<EntityUUID>();

                    _NumberOfEntityUUIDs = mySerializationReader.ReadUInt64();

                    for (UInt64 j=0; j < _NumberOfEntityUUIDs; j++)
                    {
                        _EntityUUID = new EntityUUID(mySerializationReader.ReadByteArray());
                        _EntityUUIDHashSet.Add(_EntityUUID);

                        #region Estimated size

                        _estimatedSize += EstimatedSizeConstants.CalcUUIDSize(_EntityUUID);

                        #endregion
                    }

                    // Finally... add it to the DenyACL!
                    _DenyACL.Add(_RightUUID, _EntityUUIDHashSet);

                    #endregion

                }

                #endregion
            }

            catch (Exception e)
            {
                throw new GraphFSException_AccessControlObjectCouldNotBeDeserialized("AccessControlObject could not be deserialized!\n\n" + e);
            }

            #region remaining estimated size impacts
            _estimatedSize += GetClassDefaultSize();

            #endregion

        }

        #endregion

        #region IEstimable Members

        public override ulong GetEstimatedSize()
        {
            return _estimatedSize;
        }

        private ulong GetClassDefaultSize()
        {
            //ClassDeffaultSize, EstimatedSize, SizeOfADBBAseObject
            return EstimatedSizeConstants.GetStandardSizes() + GetAFSObjectOntologyEstimatedSize();
        }

        private UInt64 CalcACLSize(Dictionary<RightUUID, HashSet<EntityUUID>> myAllowACL)
        {
            UInt64 estimatedSize = EstimatedSizeConstants.Dictionary;

            foreach (var aAllowElement in myAllowACL)
            {
                //key
                estimatedSize += EstimatedSizeConstants.CalcUUIDSize(aAllowElement.Key);

                //value
                estimatedSize += EstimatedSizeConstants.HashSet;

                foreach (var aHashSetElemt in aAllowElement.Value)
                {
                    estimatedSize += EstimatedSizeConstants.CalcUUIDSize(aHashSetElemt);
                }
            }

            return estimatedSize;
        }

        #endregion

    }

}
