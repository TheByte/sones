﻿/* GraphFS - ObjectFEC
 * (c) Achim Friedland, 2008 - 2009
 * 
 * Lead programmer:
 *      Achim Friedland
 * 
 * */

#region Usings

using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using sones.Lib;
using sones.Lib.Serializer;

using sones.Lib.DataStructures;
using sones.StorageEngines;
using sones.GraphFS.DataStructures;
using sones.GraphFS.Objects;
using sones.Lib.DataStructures.UUID;
using sones.GraphFS.InternalObjects;

#endregion

namespace sones.GraphFS.DataStructures
{

    /// <summary>
    ///  The ObjectFEC is a virtual directory to handle the
    ///  getting and setting of the forward-error-correction type
    ///  of an object.
    /// </summary>

    

    public class ObjectFEC : IDirectoryListing
    {

        #region Data

        public  ForwardErrorCorrectionTypes Algorithm;
        private String SymlinkName = "UsedAlgorithm";

        #endregion


        #region IDirectoryObject Members

        #region Members of AGraphStructure

        #region isNew

        [NonSerialized]
        private Boolean _isNew;

        [NotIFastSerialized]
        public Boolean isNew
        {

            get
            {
                return _isNew;
            }

            set
            {
                _isNew = value;
            }

        }

        #endregion

        #region INodeReference

        [NonSerialized]
        private INode _INodeReference;

        [NotIFastSerialized]
        public INode INodeReference
        {

            get
            {
                return _INodeReference;
            }

            set
            {
                _INodeReference = value;
            }

        }

        #endregion

        #region ObjectLocatorReference

        [NonSerialized]
        private ObjectLocator _ObjectLocatorReference;

        [NotIFastSerialized]
        public ObjectLocator ObjectLocatorReference
        {

            get
            {
                return _ObjectLocatorReference;
            }

            set
            {
                _ObjectLocatorReference = value;
            }

        }

        #endregion

        #region ObjectUUID

        [NonSerialized]
        private ObjectUUID _ObjectUUID;

        [NotIFastSerialized]
        public ObjectUUID ObjectUUID
        {

            get
            {
                return _ObjectUUID;
            }

            set
            {
                _ObjectUUID = value;
            }

        }

        #endregion

        #endregion

        #region Members of AGraphObject

        #region ObjectPath

        [NonSerialized]
        private ObjectLocation _ObjectPath;

        [NotIFastSerialized]
        public ObjectLocation ObjectPath
        {

            get
            {
                return _ObjectPath;
            }

            set
            {
                _ObjectPath      = value;
                _ObjectLocation = new ObjectLocation(_ObjectPath, _ObjectName);
            }

        }

        #endregion

        #region ObjectName

        [NonSerialized]
        private String _ObjectName;

        [NotIFastSerialized]
        public String ObjectName
        {

            get
            {
                return _ObjectName;
            }

            set
            {
                _ObjectName      = value;
                _ObjectLocation = new ObjectLocation(_ObjectPath, _ObjectName);
            }

        }

        #endregion

        #region ObjectLocation

        [NonSerialized]
        private ObjectLocation _ObjectLocation;

        [NotIFastSerialized]
        public ObjectLocation ObjectLocation
        {

            get
            {
                return _ObjectLocation;
            }

            set
            {
                _ObjectLocation  = value;
                _ObjectPath      = _ObjectLocation.Path;
                _ObjectName      = _ObjectLocation.Name;

            }

        }

        #endregion

        #endregion

        #region Members of IFastSerialize

        #region isDirty

        [NonSerialized]
        private Boolean _isDirty;

        [NotIFastSerialized]
        public Boolean isDirty
        {
            
            get
            {
                return _isDirty;
            }
            
            set
            {
                _isDirty = value;
            }

        }

        #endregion

        #endregion


        #region IGraphFSReference

        private IGraphFS _IGraphFSReference;

        public IGraphFS IGraphFSReference
        {

            get
            {
                return _IGraphFSReference;
            }

            set
            {
                _IGraphFSReference = value;
            }

        }

        #endregion

        public Trinary ObjectExists(String myObjectName)
        {

            if (myObjectName.Equals(FSConstants.DotLink))
                return Trinary.TRUE;

            if (myObjectName.Equals(FSConstants.DotDotLink))
                return Trinary.TRUE;

            if (myObjectName.Equals(SymlinkName))
                return Trinary.TRUE;

            foreach (String item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
                if (myObjectName.Equals(item))
                    return Trinary.TRUE;

            return Trinary.FALSE;

        }

        public Trinary ObjectStreamExists(String myObjectName, String myObjectStream)
        {

            if (myObjectName.Equals(FSConstants.DotLink) && myObjectStream == FSConstants.VIRTUALDIRECTORY)
                return Trinary.TRUE;

            if (myObjectName.Equals(FSConstants.DotDotLink) && myObjectStream == FSConstants.VIRTUALDIRECTORY)
                return Trinary.TRUE;

            if (myObjectName.Equals(SymlinkName) && myObjectStream == FSConstants.SYMLINK)
                return Trinary.TRUE;

            foreach (String item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
                if (myObjectName.Equals(item) && myObjectStream == FSConstants.INLINEDATA)
                    return Trinary.TRUE;

            return Trinary.FALSE;

        }

        public IEnumerable<String> GetObjectStreamsList(String myObjectName)
        {

            if (myObjectName.Equals(FSConstants.DotLink))
                return new List<String> { FSConstants.VIRTUALDIRECTORY };

            if (myObjectName.Equals(FSConstants.DotDotLink))
                return new List<String> { FSConstants.VIRTUALDIRECTORY };

            if (myObjectName.Equals(SymlinkName))
                return new List<String> { FSConstants.VIRTUALDIRECTORY };

            foreach (String item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
                if (myObjectName.Equals(item))
                    return new List<String> { FSConstants.INLINEDATA };

            return new List<String>();

        }

        public void RemoveObjectStream(String myObjectName, String myObjectStream)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExtendedPosition> GetObjectINodePositions(String myObjectName)
        {
            return new List<ExtendedPosition>();
        }

        public void StoreInlineData(String myObjectName, Byte[] myInlineData, Boolean myAllowOverwritting)
        {
            throw new NotImplementedException();
        }

        public Byte[] GetInlineData(String myObjectName)
        {

            foreach (String item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
                if (myObjectName.Equals(item))
                    return Encoding.UTF8.GetBytes(item.ToString());

            return new Byte[0];

        }

        public Trinary hasInlineData(String myObjectName)
        {
            return ObjectStreamExists(myObjectName, FSConstants.INLINEDATA);
        }

        public void DeleteInlineData(String myObjectName)
        {
            throw new NotImplementedException();
        }

        public void AddSymlink(String myObjectName, String myTargetObject)
        {

            if (!myObjectName.Equals(SymlinkName))
                throw new NotImplementedException();

            Boolean found = false;

            foreach (String item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
                if (myTargetObject.Equals(item))
                    found = true;

            if (!found)
                throw new NotImplementedException();

        }

        public ObjectLocation GetSymlink(String myObjectName)
        {

            if (myObjectName.Equals(SymlinkName))
                return new ObjectLocation(".", Algorithm.ToString());

            return null;

        }

        public Trinary isSymlink(String myObjectName)
        {

            if (myObjectName.Equals(SymlinkName))
                return Trinary.TRUE;

            return Trinary.FALSE;

        }

        public void RemoveSymlink(String myObjectName)
        {
            throw new NotImplementedException();
        }

        #region GetDirectoryListing()

        public IEnumerable<String> GetDirectoryListing()
        {

            var _DirectoryListing = new List<String>();

            _DirectoryListing.Add(".");
            _DirectoryListing.Add("..");

            foreach (var item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
                _DirectoryListing.Add(item);

            _DirectoryListing.Add(SymlinkName);

            return _DirectoryListing;

        }

        #endregion

        #region GetDirectoryListing(myFunc)

        public IEnumerable<String> GetDirectoryListing(Func<KeyValuePair<String, DirectoryEntry>, Boolean> myFunc)
        {
            throw new NotImplementedException();
        }

        #endregion

        public IEnumerable<String> GetDirectoryListing(String[] myName, String[] myIgnoreName, String[] myRegExpr, List<String> myObjectStreams, List<String> myIgnoreObjectStreamTypes)
        {
            return GetDirectoryListing();
        }

        #region GetExtendedDirectoryListing()

        public IEnumerable<DirectoryEntryInformation> GetExtendedDirectoryListing()
        {

            var _ExtendedDirectoryListing = new List<DirectoryEntryInformation>();
            var _OutputParameter = new DirectoryEntryInformation();

            _OutputParameter.Name = ".";
            _OutputParameter.Streams = new HashSet<String> { FSConstants.VIRTUALDIRECTORY };
            _ExtendedDirectoryListing.Add(_OutputParameter);

            _OutputParameter.Name = "..";
            _OutputParameter.Streams = new HashSet<String> { FSConstants.VIRTUALDIRECTORY };
            _ExtendedDirectoryListing.Add(_OutputParameter);

            foreach (var item in Enum.GetNames(typeof(ForwardErrorCorrectionTypes)))
            {
                _OutputParameter.Name = item;
                _OutputParameter.Streams = new HashSet<String> { FSConstants.INLINEDATA };
                _ExtendedDirectoryListing.Add(_OutputParameter);
            }

            _OutputParameter.Name = SymlinkName;
            _OutputParameter.Streams = new HashSet<String> { FSConstants.SYMLINK };
            _ExtendedDirectoryListing.Add(_OutputParameter);

            return _ExtendedDirectoryListing;

        }

        #endregion

        #region GetExtendedDirectoryListing(myFunc)

        public IEnumerable<DirectoryEntryInformation> GetExtendedDirectoryListing(Func<KeyValuePair<String, DirectoryEntry>, Boolean> myFunc)
        {
            throw new NotImplementedException();
        }

        #endregion

        public IEnumerable<DirectoryEntryInformation> GetExtendedDirectoryListing(string[] myName, string[] myIgnoreName, string[] myRegExpr, List<String> myObjectStream, List<String> myIgnoreObjectStreamTypes)
        {
            throw new NotImplementedException();
        }


        #region DirCount()

        public UInt64 DirCount
        {
            get
            {
                return (UInt64)GetDirectoryListing().LongCount();
            }
        }

        #endregion


        public NHIDirectoryObject NotificationHandling
        {
            get { throw new NotImplementedException(); }
        }

        public void SubscribeNotification(NHIDirectoryObject myNotificationHandling)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeNotification(NHIDirectoryObject myNotificationHandling)
        {
            throw new NotImplementedException();
        }


        public DirectoryEntry GetDirectoryEntry(String myObjectName)
        {
            throw new NotImplementedException();
        }

        public AFSObject Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ToString()

        public override String ToString()
        {
            return Algorithm.ToString();
        }

        #endregion


    }

}