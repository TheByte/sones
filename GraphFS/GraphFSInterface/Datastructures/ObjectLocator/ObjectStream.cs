﻿/* 
 * GraphFSInterface - ObjectStream
 * (c) Achim Friedland, 2008 - 2010
 */

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
    /// An object edition is part of the object locator describing
    /// different editions of an object stream.
    /// </summary>

    public class ObjectStream : IGraphFSDictionary<String, ObjectEdition>, IDirectoryListing
    {


        #region Data

        private Dictionary<String, ObjectEdition> _ObjectEditions;

        #endregion

        #region Properties

        #region Name

        private String _ObjectStreamName;

        public String Name
        {
            get
            {
                return _ObjectStreamName;
            }
        }

        #endregion

        #region DefaultEdition

        /// <summary>
        /// Returns the object revision object of the default edition
        /// </summary>
        public ObjectEdition DefaultEdition
        {

            get
            {

                ObjectEdition _ObjectRevisions;

                if (_ObjectEditions.TryGetValue(_DefaultEditionName, out _ObjectRevisions))
                    return _ObjectRevisions;

                return null;

            }

            set
            {

                if (_ObjectEditions.ContainsKey(_DefaultEditionName))
                    _ObjectEditions[_DefaultEditionName] = value;

                else
                    throw new ArgumentException("The default edition was not defined!");

            }

        }

        #endregion

        #region DefaultEditionName

        private String _DefaultEditionName;

        public String DefaultEditionName
        {
            get
            {
                return _DefaultEditionName;
            }
        }

        #endregion

        #region DefaultEditionExists

        /// <summary>
        /// Checks if there is a default ObjectEdition
        /// </summary>
        public Boolean DefaultEditionExists
        {
            get
            {

                if (_DefaultEditionName.Length > 0)
                    return _ObjectEditions.ContainsKey(_DefaultEditionName);

                return false;

            }
        }

        #endregion

        #endregion

        #region Constructor

        #region ObjectEditions()

        /// <summary>
        /// Main constructor
        /// </summary>
        public ObjectStream()
        {

            _ObjectPath                 = null;
            _ObjectName                 = "";
            _ObjectLocation             = null;

            _ObjectStreamName           = "";
            _ObjectEditions             = new Dictionary<String, ObjectEdition>();
            _DefaultEditionName         = FSConstants.DefaultEdition;

        }

        #endregion

        #region ObjectEditions(myObjectStreamName)

        /// <summary>
        /// Sets the ObjectStreamName
        /// </summary>
        /// <param name="myObjectStreamName"></param>
        public ObjectStream(String myObjectStreamName)
            : this()
        {
            _ObjectStreamName = myObjectStreamName;
        }

        #endregion

        #region ObjectEditions(myObjectRevision)

        /// <summary>
        /// This will create a new ObjectEditions object and add the given
        /// ObjectRevisions object using FSConstants.DefaultEdition as edition name.
        /// </summary>
        /// <param name="myObjectRevision">The ObjectRevisions object to be added</param>
        public ObjectStream(ObjectEdition myObjectRevision)
            : this()
        {
            _ObjectEditions.Add(FSConstants.DefaultEdition, myObjectRevision);
        }

        #endregion

        #region ObjectEditions(myObjectStreamName, myEditionName, myObjectRevision)

        /// <summary>
        /// This will create a new ObjectEditions object and add the given
        /// ObjectRevisions object using FSConstants.DefaultEdition as edition name.
        /// </summary>
        /// <param name="myObjectStreamName"></param>
        /// <param name="myEditionName">The name of the edition to be added</param>
        /// <param name="myObjectRevision">The ObjectRevisions object to be added</param>
        public ObjectStream(String myObjectStreamName, String myEditionName, ObjectEdition myObjectRevision)
            : this(myObjectStreamName)
        {
            _ObjectEditions.Add(myEditionName, myObjectRevision);
        }

        #endregion

        #endregion


        #region ObjectStream-specific methods

        #region SetAsDefaultEdition(myEditionName)

        /// <summary>
        /// Sets the "default edition" to the Name of a already existing edition.
        /// </summary>
        /// <param name="myEditionName"></param>
        public Boolean SetAsDefaultEdition(String myEditionName)
        {

            if (_ObjectEditions.ContainsKey(myEditionName))
            {
                _DefaultEditionName = myEditionName;
                return true;
            }

            return false;

        }

        #endregion

        #endregion


        #region IGraphFSDictionary<String, ObjectRevisions> Members

        #region Add(myEditionName, myObjectRevisions)

        /// <summary>
        /// Adds the given object edition to the list of editions.
        /// </summary>
        /// <param name="myEditionName">the name of the edition</param>
        /// <param name="myObjectRevisions">the ObjectRevisions object</param>
        public Boolean Add(String myEditionName, ObjectEdition myObjectRevisions)
        {

            try
            {

                _ObjectEditions.Add(myEditionName, myObjectRevisions);

                if (_ObjectEditions.Count == 1)
                    _DefaultEditionName = myEditionName;

                isDirty = true;

                return true;

            }

            catch (Exception)
            {
                return false;
            }

        }

        #endregion

        #region Add(myKeyValuePair)

        public Boolean Add(KeyValuePair<String, ObjectEdition> myKeyValuePair)
        {
            return Add(myKeyValuePair.Key, myKeyValuePair.Value);
        }

        #endregion


        #region this[myEditionName]

        public ObjectEdition this[String myEditionName]
        {

            get
            {

                ObjectEdition _ObjectRevisions;

                if (_ObjectEditions.TryGetValue(myEditionName, out _ObjectRevisions))
                    return _ObjectRevisions;

                return null;

            }

            set
            {

                if (_ObjectEditions.ContainsKey(myEditionName))
                    _ObjectEditions[myEditionName] = value;

                else Add(myEditionName, value);

                isDirty = true;

            }

        }

        #endregion


        #region ContainsKey(myEditionName)

        public Boolean ContainsKey(String myEditionName)
        {

            ObjectEdition _ObjectRevisions;

            if (_ObjectEditions.TryGetValue(myEditionName, out _ObjectRevisions))
                return true;

            return false;

        }

        #endregion

        #region Contains(myKeyValuePair)

        public Boolean Contains(KeyValuePair<String, ObjectEdition> myKeyValuePair)
        {

            ObjectEdition _ObjectRevisions;

            if (_ObjectEditions.TryGetValue(myKeyValuePair.Key, out _ObjectRevisions))
                if (myKeyValuePair.Value.Equals(_ObjectRevisions))
                    return true;

            return false;

        }

        #endregion

        #region Keys

        public IEnumerable<String> Keys
        {
            get
            {
                return from _Items in _ObjectEditions select _Items.Key;
            }
        }

        #endregion

        #region Values

        public IEnumerable<ObjectEdition> Values
        {
            get
            {
                return from _Items in _ObjectEditions select _Items.Value;
            }
        }

        #endregion

        #region Count

        public UInt64 Count
        {
            get
            {
                return _ObjectEditions.ULongCount();
            }
        }

        #endregion


        #region Remove(myEditionName)

        public Boolean Remove(String myEditionName)
        {

            try
            {

                if (_ObjectEditions.Remove(myEditionName))
                {

                    if (_DefaultEditionName == myEditionName)
                        _DefaultEditionName = "";

                    isDirty = true;

                    return true;

                }

                return false;

            }

            catch (Exception)
            {
                return false;
            }

        }

        #endregion

        #region Remove(myKeyValuePair)

        public Boolean Remove(KeyValuePair<String, ObjectEdition> myKeyValuePair)
        {

            ObjectEdition _ObjectRevisions;

            if (_ObjectEditions.TryGetValue(myKeyValuePair.Key, out _ObjectRevisions))
                if (myKeyValuePair.Value.Equals(_ObjectRevisions))
                    return _ObjectEditions.Remove(myKeyValuePair.Key);

            return false;

        }

        #endregion

        #region Clear()

        public void Clear()
        {
            _ObjectEditions.Clear();
        }

        #endregion

        #endregion

        #region IEnumerable<KeyValuePair<String, ObjectRevisions>> Members

        public IEnumerator<KeyValuePair<String, ObjectEdition>> GetEnumerator()
        {
            return _ObjectEditions.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _ObjectEditions.GetEnumerator();
        }

        #endregion

        #region IObjectLocation Members

        #region ObjectPath

        [NonSerialized]
        private ObjectLocation _ObjectPath;

        [NotIFastSerialized]
        ObjectLocation IObjectLocation.ObjectPath
        {

            get
            {
                return _ObjectPath;
            }

            //set
            //{
            //    _ObjectPath     = value;
            //    _ObjectLocation = new ObjectLocation(_ObjectPath, _ObjectName);
            //}

        }

        #endregion

        #region ObjectName

        [NonSerialized]
        private String _ObjectName;

        [NotIFastSerialized]
        String IObjectLocation.ObjectName
        {

            get
            {
                return _ObjectName;
            }

            //set
            //{
            //    _ObjectName = value;
            //    _ObjectLocation = new ObjectLocation(_ObjectPath, _ObjectName);
            //}

        }

        #endregion

        #region ObjectLocation

        [NonSerialized]
        private ObjectLocation _ObjectLocation;

        [NotIFastSerialized]
        ObjectLocation IObjectLocation.ObjectLocation
        {

            get
            {
                return _ObjectLocation;
            }

            //set
            //{
            //    _ObjectLocation = value;
            //    _ObjectPath = _ObjectLocation.Path;
            //    _ObjectName = _ObjectLocation.Name;
            //}

        }

        #endregion

        #endregion

        #region IDirectoryListing Members

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

        #region ObjectExists(myObjectName)

        Trinary IDirectoryListing.ObjectExists(String myObjectName)
        {

            if (myObjectName.Equals(FSConstants.DotLink))
                return Trinary.TRUE;

            if (myObjectName.Equals(FSConstants.DotDotLink))
                return Trinary.TRUE;

            if (myObjectName.Equals(FSConstants.DotDefaultEditionSymlink))
                return Trinary.TRUE;

            foreach (String _ObjectName in _ObjectEditions.Keys)
                if (_ObjectName.Equals(myObjectName))
                    return Trinary.TRUE;

            return Trinary.FALSE;

        }

        #endregion

        #region ObjectStreamExists(myObjectName, myObjectStream)

        Trinary IDirectoryListing.ObjectStreamExists(String myObjectName, String myObjectStream)
        {

            if (myObjectName.Equals(FSConstants.DotLink) && myObjectStream == FSConstants.VIRTUALDIRECTORY)
                return Trinary.TRUE;

            if (myObjectName.Equals(FSConstants.DotDotLink) && myObjectStream == FSConstants.VIRTUALDIRECTORY)
                return Trinary.TRUE;

            if (myObjectName.Equals(FSConstants.DotDefaultEditionSymlink) && myObjectStream == FSConstants.SYMLINK)
                return Trinary.TRUE;

            foreach (String _DirectoryEntry in _ObjectEditions.Keys)
                if (_DirectoryEntry.Equals(myObjectName) && myObjectStream == FSConstants.VIRTUALDIRECTORY)
                    return Trinary.TRUE;

            return Trinary.FALSE;

        }

        #endregion

        #region GetObjectStreamsList(myObjectName)

        IEnumerable<String> IDirectoryListing.GetObjectStreamsList(String myObjectName)
        {

            if (myObjectName.Equals(FSConstants.DotLink))
                return new List<String> { FSConstants.VIRTUALDIRECTORY };

            if (myObjectName.Equals(FSConstants.DotDotLink))
                return new List<String> { FSConstants.VIRTUALDIRECTORY };

            if (myObjectName.Equals(FSConstants.DotDefaultEditionSymlink))
                return new List<String> { FSConstants.SYMLINK };

            foreach (String _DirectoryEntry in _ObjectEditions.Keys)
                if (_DirectoryEntry.Equals(myObjectName))
                    return new List<String> { FSConstants.VIRTUALDIRECTORY };

            return new List<String>();

        }

        #endregion

        #region GetObjectINodePositions(String myObjectName)

        IEnumerable<ExtendedPosition> IDirectoryListing.GetObjectINodePositions(String myObjectName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetInlineData(myObjectName)

        Byte[] IDirectoryListing.GetInlineData(String myObjectName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region hasInlineData(myObjectName)

        Trinary IDirectoryListing.hasInlineData(String myObjectName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetSymlink(myObjectName)

        ObjectLocation IDirectoryListing.GetSymlink(String myObjectName)
        {

            if (myObjectName.Equals(FSConstants.DotDefaultEditionSymlink))
                return new ObjectLocation(".", DefaultEditionName);

            return null;

        }

        #endregion

        #region isSymlink(myObjectName)

        Trinary IDirectoryListing.isSymlink(String myObjectName)
        {

            if (myObjectName.Equals(FSConstants.DotDefaultEditionSymlink))
                return Trinary.TRUE;

            return Trinary.FALSE;

        }

        #endregion

        #region GetDirectoryListing()

        IEnumerable<String> IDirectoryListing.GetDirectoryListing()
        {

            var _DirectoryListing = new List<String>();

            _DirectoryListing.Add(".");
            _DirectoryListing.Add("..");

            foreach (var _DirectoryEntry in _ObjectEditions.Keys)
                _DirectoryListing.Add(_DirectoryEntry);

            _DirectoryListing.Add(FSConstants.DotDefaultEditionSymlink);

            return _DirectoryListing;

        }

        #endregion

        #region GetDirectoryListing(myFunc)

        IEnumerable<String> IDirectoryListing.GetDirectoryListing(Func<KeyValuePair<String, DirectoryEntry>, Boolean> myFunc)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetDirectoryListing(myName, myIgnoreName, myRegExpr, myObjectStreams, myIgnoreObjectStreams)

        IEnumerable<String> IDirectoryListing.GetDirectoryListing(String[] myName, String[] myIgnoreName, String[] myRegExpr, List<String> myObjectStream, List<String> myIgnoreObjectStreamTypes)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetExtendedDirectoryListing()

        IEnumerable<DirectoryEntryInformation> IDirectoryListing.GetExtendedDirectoryListing()
        {

            var _Output           = new List<DirectoryEntryInformation>();
            var _OutputParameter  = new DirectoryEntryInformation();

            _OutputParameter.Name         = ".";
            _OutputParameter.Streams = new HashSet<String> { FSConstants.VIRTUALDIRECTORY };
            _Output.Add(_OutputParameter);

            _OutputParameter.Name         = "..";
            _OutputParameter.Streams = new HashSet<String> { FSConstants.VIRTUALDIRECTORY };
            _Output.Add(_OutputParameter);

            foreach (var _DirectoryEntry in _ObjectEditions.Keys)
            {
                _OutputParameter.Name         = _DirectoryEntry;
                _OutputParameter.Streams = new HashSet<String> { FSConstants.VIRTUALDIRECTORY };
                _Output.Add(_OutputParameter);
            }

            _OutputParameter.Name         = FSConstants.DotDefaultEditionSymlink;
            _OutputParameter.Streams = new HashSet<String> { FSConstants.SYMLINK };
            _Output.Add(_OutputParameter);

            return _Output;

        }

        #endregion

        #region GetExtendedDirectoryListing(myFunc)

        IEnumerable<DirectoryEntryInformation> IDirectoryListing.GetExtendedDirectoryListing(Func<KeyValuePair<String, DirectoryEntry>, Boolean> myFunc)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GetExtendedDirectoryListing(myName, myIgnoreName, myRegExpr, myObjectStreams, myIgnoreObjectStreams)

        IEnumerable<DirectoryEntryInformation> IDirectoryListing.GetExtendedDirectoryListing(String[] myName, String[] myIgnoreName, String[] myRegExpr, List<String> myObjectStreams, List<String> myIgnoreObjectStreamTypes)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region DirCount()

        UInt64 IDirectoryListing.DirCount
        {
            get
            {
                return (UInt64)_ObjectEditions.Keys.LongCount() + 3;
            }
        }

        #endregion


        NHIDirectoryObject IDirectoryListing.NotificationHandling
        {
            get { throw new NotImplementedException(); }
        }

        void IDirectoryListing.SubscribeNotification(NHIDirectoryObject myNotificationHandling)
        {
            throw new NotImplementedException();
        }

        void IDirectoryListing.UnsubscribeNotification(NHIDirectoryObject myNotificationHandling)
        {
            throw new NotImplementedException();
        }


        #region GetDirectoryEntry(myObjectName)

        DirectoryEntry IDirectoryListing.GetDirectoryEntry(String myObjectName)
        {
            
            foreach (var _DirectoryEntry in _ObjectEditions.Keys)
                if (_DirectoryEntry.Equals(myObjectName))
                    return new DirectoryEntry { Virtual = new HashSet<String>{ FSConstants.VIRTUALDIRECTORY }};

            if (myObjectName.Equals(FSConstants.DotDefaultEditionSymlink))
                return new DirectoryEntry { Virtual = new HashSet<String> { FSConstants.SYMLINK } };

            return null;

        }

        #endregion

        #endregion


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

        #region ToString()

        public override String ToString()
        {

            var _ReturnValue = "[";
            var _ObjectEditionList = new List<String>(_ObjectEditions.Keys);

            for (int i = 0; i < _ObjectEditionList.Count - 1; i++)
            {

                _ReturnValue += _ObjectEditionList[i];

                if (_ObjectEditionList[i].Equals(_DefaultEditionName))
                    _ReturnValue += " (default)";

                _ReturnValue += ", ";

            }

            _ReturnValue += _ObjectEditionList[_ObjectEditions.Keys.Count - 1] + "]";

            return _ReturnValue;

        }

        #endregion


    }

}
