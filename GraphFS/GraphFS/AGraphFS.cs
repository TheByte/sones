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
 * AGraphFS
 * Achim Friedland, 2010
 */

#region Usings

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using sones.Lib;

using sones.Notifications;
using sones.Lib.DataStructures;

using sones.GraphFS.Caches;
using sones.GraphFS.Notification;

using sones.StorageEngines;
using sones.GraphFS.DataStructures;
using sones.GraphFS.Objects;
using sones.GraphFS.Exceptions;
using sones.Lib.ErrorHandling;
using sones.GraphFS.Errors;
using sones.GraphFS.InternalObjects;
using sones.GraphFS;
using sones.GraphFS.Session;
using sones.Lib.Session;
using sones.GraphFS.Transactions;
using sones.Lib.DataStructures.Indices;

#endregion

namespace sones
{

    /// <summary>
    /// AGraphFS - Version 1.0
    /// AGraphFS is an abstract implementation of the IGraphFS interface
    /// </summary>

    public abstract class AGraphFS : IGraphFS
    {


        #region Data

        protected ForestUUID              _ForestUUID;

        protected String                  _FileSystemDescription;
        protected AccessModeTypes         _AccessMode;

        //protected NotificationDispatcher  _NotificationDispatcher;

        protected String                  _ChangedRootDirectoryPrefix;

        protected Regex                   _MoreThanOnePathSeperatorRegExpr = new Regex("\\" + FSPathConstants.PathDelimiter + "\\" + FSPathConstants.PathDelimiter);

        protected GraphFSLookuptable      _GraphFSLookuptable;

        protected const UInt64            NUMBER_OF_DEFAULT_DIRECTORYENTRIES = 6;

        #endregion

        #region Properties

        #region FileSystemUUID

        protected FileSystemUUID _FileSystemUUID;

        public FileSystemUUID FileSystemUUID
        {

            get
            {
                return _FileSystemUUID;
            }

            set
            {

                _FileSystemUUID = value;

                //if (_NotificationDispatcher != null)
                //    _NotificationDispatcher.SenderID = _FileSystemUUID;

            }

        }

        #endregion

        #region IsPersistent

        public abstract Boolean IsPersistent { get; }

        #endregion

        #region NotificationSettings

        protected NotificationSettings _NotificationSettings;

        public NotificationSettings NotificationSettings
        {

            get
            {
                return _NotificationSettings;
            }

            set
            {
                _NotificationSettings = value;
            }

        }

        #endregion

        #region NotificationDispatcher

        protected NotificationDispatcher _NotificationDispatcher;

        public NotificationDispatcher NotificationDispatcher
        {

            get
            {
                return _NotificationDispatcher;
            }

            set
            {
                _NotificationDispatcher = value;
            }

        }

        #endregion

        #region ObjectCacheSettings

        protected ObjectCacheSettings _ObjectCacheSettings;

        public ObjectCacheSettings ObjectCacheSettings
        {

            get
            {
                return _ObjectCacheSettings;
            }

            set
            {
                _ObjectCacheSettings = value;
            }

        }

        #endregion

        #endregion

        #region Constructor

        #region AGraphFS()

        public AGraphFS()
        {
            _ForestUUID             = ForestUUID.NewUUID;
            _FileSystemUUID         = FileSystemUUID.NewUUID;
            _AccessMode             = AccessModeTypes.rw;
            _GraphFSLookuptable            = new GraphFSLookuptable();
            _NotificationSettings   = new NotificationSettings();
        }

        #endregion

        #endregion

        #region Information Methods

        #region isMounted

        /// <summary>
        /// Returns true if the file system was mounted correctly
        /// </summary>
        /// <returns>true if the file system was mounted correctly</returns>
        public abstract Boolean isMounted { get; }

        #endregion


        #region GetFileSystemUUID()

        /// <summary>
        /// Returns the UUID of this file system
        /// </summary>
        /// <returns>The UUID of this file system</returns>
        public FileSystemUUID GetFileSystemUUID(SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return _FileSystemUUID;

        }

        #endregion

        #region GetFileSystemUUID(myObjectLocation)

        /// <summary>
        /// Returns the UUID of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <returns>The UUID of the file system at the given ObjectLocation</returns>
        public FileSystemUUID GetFileSystemUUID(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return GetChildFileSystem(myObjectLocation, true, mySessionToken).GetFileSystemUUID(mySessionToken);

        }

        #endregion

        #region GetFileSystemUUIDs(myRecursiveOperation)

        /// <summary>
        /// Returns a (recursive) list of FileSystemUUIDs of all mounted file systems
        /// </summary>
        /// <param name="myRecursiveOperation">Recursive operation?</param>
        /// <returns>A (recursive) list of FileSystemUUIDs of all mounted file systems</returns>
        public IEnumerable<FileSystemUUID> GetFileSystemUUIDs(Boolean myRecursiveOperation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var _ListOfFileSystemUUIDs = new List<FileSystemUUID>();

            if (myRecursiveOperation)
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                {

                    if (_IPandoraFS == this)
                        _ListOfFileSystemUUIDs.Add(GetFileSystemUUID(mySessionToken));

                    else
                        foreach (var lala in _IPandoraFS.GetFileSystemUUIDs(myRecursiveOperation, mySessionToken))
                            _ListOfFileSystemUUIDs.Add(lala);

                }

            else
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                    _ListOfFileSystemUUIDs.Add(_IPandoraFS.GetFileSystemUUID(mySessionToken));

            return _ListOfFileSystemUUIDs;

        }

        #endregion


        #region GetFileSystemDescription

        /// <summary>
        /// Returns the Name or a description of this file system.
        /// </summary>
        /// <returns>The Name or a description of this file system</returns>
        public String GetFileSystemDescription(SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return _FileSystemDescription;

        }

        #endregion

        #region GetFileSystemDescription(myObjectLocation, SessionToken mySessionToken)

        /// <summary>
        /// Returns the Name or a description of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <returns>The Name or a description of the file system at the given ObjectLocation</returns>
        public String GetFileSystemDescription(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return GetChildFileSystem(myObjectLocation, true, mySessionToken).GetFileSystemDescription(mySessionToken);

        }

        #endregion

        #region GetFileSystemDescriptions(myRecursiveOperation, SessionToken mySessionToken)

        /// <summary>
        /// Returns a (recursive) list of FileSystemDescriptions of all mounted file systems
        /// </summary>
        /// <param name="myRecursiveOperation">Recursive operation?</param>
        /// <returns>A (recursive) list of FileSystemDescriptions of all mounted file systems</returns>
        public IEnumerable<String> GetFileSystemDescriptions(Boolean myRecursiveOperation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var _ListOfFileSystemDescriptions = new List<String>();

            if (myRecursiveOperation)
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                {

                    if (_IPandoraFS == this)
                        _ListOfFileSystemDescriptions.Add(GetFileSystemDescription(mySessionToken));

                    else
                        foreach (var _ListOfRecursiveFileSystemDescriptions in _IPandoraFS.GetFileSystemDescriptions(myRecursiveOperation, mySessionToken))
                            _ListOfFileSystemDescriptions.Add(_ListOfRecursiveFileSystemDescriptions);

                }

            else
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                    _ListOfFileSystemDescriptions.Add(_IPandoraFS.GetFileSystemDescription(mySessionToken));

            return _ListOfFileSystemDescriptions;

        }

        #endregion


        #region SetFileSystemDescription(myFileSystemDescription)

        /// <summary>
        /// Sets the Name or a description of this file system.
        /// </summary>
        /// <param name="myFileSystemDescription">the Name or a description of this file system</param>
        public void SetFileSystemDescription(String myFileSystemDescription, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            _FileSystemDescription = myFileSystemDescription;

        }

        #endregion

        #region SetFileSystemDescription(myObjectLocation, myFileSystemDescription, SessionToken mySessionToken)

        /// <summary>
        /// Sets the Name or a description of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <param name="myFileSystemDescription">the Name or a description of the file system at the given ObjectLocation</param>
        public void SetFileSystemDescription(ObjectLocation myObjectLocation, String myFileSystemDescription, SessionToken mySessionToken)
        {
            GetChildFileSystem(myObjectLocation, true, mySessionToken).SetFileSystemDescription(myFileSystemDescription, mySessionToken);
        }

        #endregion


        #region GetNumberOfBytes(SessionToken mySessionToken)

        public abstract UInt64 GetNumberOfBytes(SessionToken mySessionToken);

        #endregion

        #region GetNumberOfBytes(myObjectLocation, SessionToken mySessionToken)

        public UInt64 GetNumberOfBytes(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return GetChildFileSystem(myObjectLocation, true, mySessionToken).GetNumberOfBytes(mySessionToken);

        }

        #endregion

        #region GetNumberOfBytes(myRecursiveOperation, SessionToken mySessionToken)

        public IEnumerable<UInt64> GetNumberOfBytes(Boolean myRecursiveOperation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var _ListOfNumberOfBytes = new List<UInt64>();

            if (myRecursiveOperation)
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                {

                    if (_IPandoraFS == this)
                        _ListOfNumberOfBytes.Add(GetNumberOfBytes(mySessionToken));

                    else
                        foreach (var _ListOfRecursiveNumberOfBytes in _IPandoraFS.GetNumberOfBytes(myRecursiveOperation, mySessionToken))
                            _ListOfNumberOfBytes.Add(_ListOfRecursiveNumberOfBytes);

                }

            else
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                    _ListOfNumberOfBytes.Add(_IPandoraFS.GetNumberOfBytes(mySessionToken));

            return _ListOfNumberOfBytes;

        }

        #endregion


        #region GetNumberOfFreeBytes(SessionToken mySessionToken)

        public abstract UInt64 GetNumberOfFreeBytes(SessionToken mySessionToken);

        #endregion

        #region GetNumberOfFreeBytes(myObjectLocation, SessionToken mySessionToken)

        public UInt64 GetNumberOfFreeBytes(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return GetChildFileSystem(myObjectLocation, true, mySessionToken).GetNumberOfFreeBytes(mySessionToken);

        }

        #endregion

        #region GetNumberOfFreeBytes(myRecursiveOperation, SessionToken mySessionToken)

        public IEnumerable<UInt64> GetNumberOfFreeBytes(Boolean myRecursiveOperation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var _ListOfNumberOfFreeBytes = new List<UInt64>();

            if (myRecursiveOperation)
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                {

                    if (_IPandoraFS == this)
                        _ListOfNumberOfFreeBytes.Add(GetNumberOfFreeBytes(mySessionToken));

                    else
                        foreach (var _ListOfRecursiveNumberOfFreeBytes in _IPandoraFS.GetNumberOfFreeBytes(myRecursiveOperation, mySessionToken))
                            _ListOfNumberOfFreeBytes.Add(_ListOfRecursiveNumberOfFreeBytes);

                }

            else
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                    _ListOfNumberOfFreeBytes.Add(_IPandoraFS.GetNumberOfFreeBytes(mySessionToken));

            return _ListOfNumberOfFreeBytes;

        }

        #endregion


        #region GetAccessMode(SessionToken mySessionToken)

        public AccessModeTypes GetAccessMode(SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return _AccessMode;

        }

        #endregion

        #region GetAccessMode(myObjectLocation)

        public AccessModeTypes GetAccessMode(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            return GetChildFileSystem(myObjectLocation, true, mySessionToken).GetAccessMode(mySessionToken);

        }

        #endregion

        #region GetAccessModes(myRecursiveOperation, SessionToken mySessionToken)

        public IEnumerable<AccessModeTypes> GetAccessModes(Boolean myRecursiveOperation, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var _ListOfAccessModes = new List<AccessModeTypes>();

            if (myRecursiveOperation)
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                {

                    if (_IPandoraFS == this)
                        _ListOfAccessModes.Add(GetAccessMode(mySessionToken));

                    else
                        foreach (var _ListOfRecursiveAccessModes in _IPandoraFS.GetAccessModes(myRecursiveOperation, mySessionToken))
                            _ListOfAccessModes.Add(_ListOfRecursiveAccessModes);

                }

            else
                foreach (var _IPandoraFS in _GraphFSLookuptable.ChildFSs)
                    _ListOfAccessModes.Add(_IPandoraFS.GetAccessMode(mySessionToken));


            return _ListOfAccessModes;

        }

        #endregion


        #region ParentFileSystem

        protected IGraphFS _ParentFileSystem;

        public IGraphFS ParentFileSystem
        {

            get
            {
                return _ParentFileSystem;
            }

            set
            {
                _ParentFileSystem = value;
            }

        }

        #endregion

        #region GetChildFileSystem(myObjectLocation, myRecursive, mySessionToken)

        public IGraphFS GetChildFileSystem(ObjectLocation myObjectLocation, Boolean myRecursive, SessionToken mySessionToken)
        {

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var _PathLength = Int32.MinValue;
            IGraphFS _ChildIPandoraFS = this;

            foreach (var __Mountpoint_IPandoraFS in _GraphFSLookuptable.MountedFSs)
            {

                if (myObjectLocation.StartsWith(__Mountpoint_IPandoraFS.Key) &&
                    (_PathLength < __Mountpoint_IPandoraFS.Key.Length))
                {
                    _PathLength = __Mountpoint_IPandoraFS.Key.Length;
                    _ChildIPandoraFS = __Mountpoint_IPandoraFS.Value;
                }

            }

            if (myRecursive && _ChildIPandoraFS != this)
                _ChildIPandoraFS = _ChildIPandoraFS.GetChildFileSystem(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), true, mySessionToken);

            return _ChildIPandoraFS;

        }

        #endregion

        #region GetChildFileSystemMountpoints(myRecursiveOperation, SessionToken mySessionToken)

        public IEnumerable<ObjectLocation> GetChildFileSystemMountpoints(Boolean myRecursiveOperation, SessionToken mySessionToken)
        {

            return new List<ObjectLocation>();

            //if (!isMounted)
            //    throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            //var _ListOfChildFileSystemMountpoints = new List<ObjectLocation>();

            //if (myRecursiveOperation)
            //    foreach (var __MountPoint_IPandoraFS in _Lookuptable.MountedFSs)
            //    {

            //        if (__MountPoint_IPandoraFS.Value == this)
            //            foreach (var _Mountpoint in _Lookuptable.Mountpoints)
            //            {
            //                if (!_Mountpoint.Equals(FSPathConstants.PathDelimiter))
            //                    _ListOfChildFileSystemMountpoints.Add(_Mountpoint);
            //            }

            //        else
            //            foreach (var _ChildMountpoints in __MountPoint_IPandoraFS.Value.GetChildFileSystemMountpoints(myRecursiveOperation, mySessionToken))
            //                _ListOfChildFileSystemMountpoints.Add(new ObjectLocation(__MountPoint_IPandoraFS.Key + _ChildMountpoints));

            //    }

            //else
            //    foreach (var _Mountpoint in _Lookuptable.Mountpoints)
            //    {
            //        if (!_Mountpoint.Equals(FSPathConstants.PathDelimiter))
            //            _ListOfChildFileSystemMountpoints.Add(_Mountpoint);
            //    }

            //return _ListOfChildFileSystemMountpoints;

        }

        #endregion

        #region (protected) GetChildFileSystemMountpoint(myObjectLocation, SessionToken mySessionToken)

        protected ObjectLocation GetChildFileSystemMountpoint(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {


            Int32  _PathLength       = Int32.MinValue;
            String _ChildMountpoint  = FSPathConstants.PathDelimiter;

            foreach (var __Mountpoint_IPandoraFS in _GraphFSLookuptable.MountedFSs)
            {

                if (myObjectLocation.StartsWith(__Mountpoint_IPandoraFS.Key) &&
                    (_PathLength < __Mountpoint_IPandoraFS.Key.Length))
                {
                    _PathLength       = __Mountpoint_IPandoraFS.Key.Length;
                    _ChildMountpoint  = __Mountpoint_IPandoraFS.Key;
                }

            }

            return new ObjectLocation(_ChildMountpoint);

        }

        #endregion

        #region (protected) GetObjectLocationOnChildFileSystem(myObjectLocation, SessionToken mySessionToken)

        protected ObjectLocation GetObjectLocationOnChildFileSystem(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            var newObjectLocation = myObjectLocation.Substring(GetChildFileSystemMountpoint(myObjectLocation, mySessionToken).Length);

            if (!newObjectLocation.StartsWith(FSPathConstants.PathDelimiter))
                newObjectLocation = String.Concat(FSPathConstants.PathDelimiter, newObjectLocation);

            return new ObjectLocation(newObjectLocation);

        }

        #endregion

        #endregion


        #region Make-/Grow-/ShrinkFileSystem

        public abstract Exceptional<FileSystemUUID> MakeFileSystem(IEnumerable<IStorageEngine> myIStorageEngines, String myDescription, Boolean myOverwriteExistingFileSystem, Action<Double> myAction, SessionToken mySessionToken);
        public abstract Exceptional<UInt64> GrowFileSystem(UInt64 myNumberOfBytesToAdd, SessionToken mySessionToken);
        public abstract Exceptional<UInt64> ShrinkFileSystem(UInt64 myNumberOfBytesToRemove, SessionToken mySessionToken);

        #endregion

        #region MountFileSystem

        public abstract Exceptional MountFileSystem(String myStorageLocation, AccessModeTypes myFSAccessMode, SessionToken mySessionToken);

        #region MountFileSystem(myStorageLocation, myMountPoint, myFSAccessMode, SessionToken mySessionToken)

        /// <summary>
        /// This method will mount the file system from a StorageLocation serving
        /// the file system into the given ObjectLocation using the given file system
        /// access mode. If the mountpoint is located within another file system this
        /// file system will be called to process this request in a recursive way.
        /// </summary>
        /// <param name="myStorageLocation">A StorageLocation (device or filename) the file system can be read from</param>
        /// <param name="myMountPoint">The location the file system should be mounted at</param>
        /// <param name="myFSAccessMode">The access mode of the file system to mount</param>
        public Exceptional MountFileSystem(String myStorageLocation, ObjectLocation myMountPoint, AccessModeTypes myFSAccessMode, SessionToken mySessionToken)
        {

            throw new NotImplementedException("Please do not use this method at the moment!");

            #region Pre-Mounting checks

            // Check if the root filesystem is mounted
            if (!isMounted)
            {
                if (myMountPoint.Equals(FSPathConstants.PathDelimiter))
                {
                    return MountFileSystem(myStorageLocation, myFSAccessMode, mySessionToken);
                }
                else
                    throw new PandoraFSException_MountFileSystemFailed("Please mount a (root) file system first!");
            }

            // Remove an ending FSPathConstants.PathDelimiter: "/Volumes/test/" -> "/Volumes/test"
            if ((myMountPoint.Length > FSPathConstants.PathDelimiter.Length) && (myMountPoint.EndsWith(FSPathConstants.PathDelimiter)))
                myMountPoint = new ObjectLocation(myMountPoint.Substring(0, myMountPoint.Length - FSPathConstants.PathDelimiter.Length));

            #endregion

            var _ChildIPandoraFS = GetChildFileSystem(myMountPoint, false, mySessionToken);

            if (_ChildIPandoraFS == this)
            {

                if (myMountPoint.Equals(FSPathConstants.PathDelimiter))
                    MountFileSystem(myStorageLocation, myFSAccessMode, mySessionToken);

                else
                {

                    #region Checks against other mounted filesystems

                    // Check if the _exact_ _MountPoint is already used by another filesystem
                    foreach (var _Mountpoint in _GraphFSLookuptable.Mountpoints)
                    {

                        if (myMountPoint.Equals(_Mountpoint))
                            throw new PandoraFSException_MountFileSystemFailed("This mountpoint is already in use!");

                        //// Check if the filesystem is already mounted != readonly
                        //if (MountedFileSystems[_Filesystem].StorageIDs.Equals(myStorageLocation))
                        //{
                        //    switch (MountedFileSystems[_Filesystem].AccessMode)
                        //    {
                        //        case AccessModeTypes.rw: throw new PandoraFSException_MountFileSystemFailed("File system is already mounted read/write");
                        //        case AccessModeTypes.ap: throw new PandoraFSException_MountFileSystemFailed("File system is already mounted appendable");
                        //        case AccessModeTypes.metarw: throw new PandoraFSException_MountFileSystemFailed("File system is already mounted meta-read/write");
                        //        case AccessModeTypes.metaap: throw new PandoraFSException_MountFileSystemFailed("File system is already mounted meta-appendable");
                        //    }
                        //}

                    }

                    // Check if the directory mentioned in the _MountPoint is existend

                    #endregion

                    #region Mount and register the new file system

                    //IGraphFS newFSObject = new TmpFS1();
                    ////newFSObject.SetNotificationDispatcher(_NotificationDispatcher, mySessionToken);
                    //newFSObject.MountFileSystem(myStorageLocation, myFSAccessMode, mySessionToken);

                    ////ParentMountedFS = FindMountedFileSystemByPath(myMountPoint);

                    //// Register thsi file system as parent file system
                    //newFSObject.ParentFileSystem = this;

                    //// Register the mountedFS object in the list of ChildFileSystems
                    //_GraphFSLookuptable.Set(myMountPoint, newFSObject);

                    #endregion

                }

            }

            else
                _ChildIPandoraFS.MountFileSystem(myStorageLocation, GetObjectLocationOnChildFileSystem(myMountPoint, mySessionToken), myFSAccessMode, mySessionToken);

            return new Exceptional();

        }

        #endregion

        public abstract Exceptional RemountFileSystem(AccessModeTypes myFSAccessMode, SessionToken mySessionToken);

        #region RemountFileSystem(myMountPoint, myFSAccessMode, mySessionToken)

        public Exceptional RemountFileSystem(ObjectLocation myMountPoint, AccessModeTypes myFSAccessMode, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        public abstract Exceptional UnmountFileSystem(SessionToken mySessionToken);

        #region UnmountFileSystem(myMountPoint, mySessionToken)

        public Exceptional UnmountFileSystem(ObjectLocation myMountPoint, SessionToken mySessionToken)
        {
            var _ChildIPandoraFS = GetChildFileSystem(myMountPoint, false, mySessionToken);
            return _ChildIPandoraFS.UnmountFileSystem(mySessionToken);
        }

        #endregion

        #region UnmountAllFileSystems(mySessionToken)

        public Exceptional UnmountAllFileSystems(SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional();

            // Loop till there is only one (this file system itself) left!
            while (_GraphFSLookuptable.ChildFSs.LongCount() > 1)
            {

                var _ListOfChildIPandoraFSs = new List<KeyValuePair<ObjectLocation, IGraphFS>>();

                foreach (var __ChildMountpoint_IPandoraFS in _GraphFSLookuptable.MountedFSs)
                    if (__ChildMountpoint_IPandoraFS.Value != this)
                        _ListOfChildIPandoraFSs.Add(__ChildMountpoint_IPandoraFS);

                if (_ListOfChildIPandoraFSs.Count > 0)
                {
                    _ListOfChildIPandoraFSs[0].Value.UnmountAllFileSystems(mySessionToken);
                    _GraphFSLookuptable.Remove(_ListOfChildIPandoraFSs[0].Key);
                }

            }

            return UnmountFileSystem(mySessionToken);

        }

        #endregion

        public abstract Exceptional ChangeRootDirectory(String myChangeRootPrefix, SessionToken mySessionToken);

        #endregion

        
        #region INode and ObjectLocator methods

        #region (protected) GetINode_protected(myObjectLocation, mySessionToken)

        protected Exceptional<INode> GetINode_protected(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional<INode>();
            var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null && _ObjectLocatorExceptional.Value.INodeReference != null)
                _Exceptional.Value = _ObjectLocatorExceptional.Value.INodeReference;

            else
                return _ObjectLocatorExceptional.Convert<INode>().PushT(new GraphFSError_INodeCouldNotBeLoaded(myObjectLocation));

            return _Exceptional;

        }

        #endregion

        #region GetINode(myObjectLocation, mySessionToken)

        public Exceptional<INode> GetINode(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            return GetINode_protected(myObjectLocation, mySessionToken);
        }

        #endregion

        #region (private) GetObjectLocator_private(myObjectLocation, mySessionToken)

        protected abstract Exceptional<ObjectLocator> GetObjectLocator_protected(ObjectLocation myObjectLocation, SessionToken mySessionToken);

        #endregion

        #region GetObjectLocator(myObjectLocation, mySessionToken)

        public Exceptional<ObjectLocator> GetObjectLocator(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            return GetObjectLocator_protected(myObjectLocation, mySessionToken);

            //var _Exceptional = new Exceptional<ObjectLocator>();

            //_Exceptional.Value = _Lookuptable.GetObjectLocator(myObjectLocation);

            //if (_Exceptional.Value == null)
            //    _Exceptional.Add(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));

            //return _Exceptional;
            
        }

        #endregion

        #endregion

        #region Cache handling

        protected abstract void CacheAdd(ObjectLocation myObjectLocation, ObjectLocator myObjectLocator, Boolean myIsPinned);
        protected abstract void CacheAdd(CacheUUID myCacheUUID, AFSObject myAPandoraObject, Boolean myIsPinned);
        protected abstract Exceptional<PT> CacheGet<PT>(CacheUUID myCacheUUID) where PT : AFSObject;
        protected abstract void CacheMove(ObjectLocation myOldObjectLocation, ObjectLocation myNewObjectLocation, Boolean myRecursive);
        protected abstract void CacheRemove(ObjectLocation myObjectLocation, Boolean myRecursive);
        protected abstract void CacheRemove(CacheUUID myCacheUUID);

        #endregion

        #region FSObject specific methods

        #region LockObject(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, myObjectLock, myObjectLockType, myLockingTime, mySessionToken)

        public Boolean LockObject(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, ObjectLocks myObjectLock, ObjectLockTypes myObjectLockType, UInt64 myLockingTime, SessionToken mySessionToken)
        {
            return false;
        }

        #endregion


        public Exceptional<PT> GetOrCreateObject<PT>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, UInt64 myObjectCopy, Boolean myIgnoreIntegrityCheckFailures, SessionToken mySessionToken) where PT : AFSObject, new()
        {
            return GetOrCreateObject<PT>(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, myObjectCopy, myIgnoreIntegrityCheckFailures, new Func<PT>(delegate { return new PT(); }), mySessionToken);
        }

        #region GetOrCreateObject<PT>(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, myIgnoreIntegrityCheckFailures, mySessionToken)

        public Exceptional<PT> GetOrCreateObject<PT>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, UInt64 myObjectCopy, Boolean myIgnoreIntegrityCheckFailures, Func<PT> myFunc, SessionToken mySessionToken) where PT : AFSObject
        {

            lock (this)
            {

                #region Get an existing Object...

                var _Exceptional = GetObject<PT>(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, myObjectCopy, myIgnoreIntegrityCheckFailures, myFunc, mySessionToken);

                #endregion

                #region ...or create a new one!

                try
                {

                    if (_Exceptional.Value == null)
                    {

                        _Exceptional       = new Exceptional<PT>();
                        //_Exceptional.Value = new PT() { ObjectLocation = myObjectLocation };
                        _Exceptional.Value = myFunc();
                        _Exceptional.Value.ObjectLocation = myObjectLocation;

                        if (myObjectStream != null && myObjectStream.Length > 0)
                            _Exceptional.Value.ObjectStream = myObjectStream;

                        if (myObjectEdition != null && myObjectEdition.Length > 0)
                            _Exceptional.Value.ObjectEdition = myObjectEdition;

                        if (myObjectRevisionID != null && myObjectRevisionID.Timestamp != null && myObjectRevisionID.UUID != null)
                            _Exceptional.Value.ObjectRevision = myObjectRevisionID;

                    }

                }

                catch (Exception e)
                {
                    _Exceptional.PushT(new GraphFSError(e.Message));
                }

                #endregion

                return _Exceptional;
            
            }

        }

        #endregion


        //protected abstract Exceptional<PT> LoadObject_protected<PT>(ObjectLocator myObjectLocator, ObjectRevision myObjectRevision, Boolean myIgnoreIntegrityCheckFailures) where PT : AFSObject, new();
        protected abstract Exceptional<AFSObject> LoadAFSObject_protected(ObjectLocator myObjectLocator, ObjectRevision myObjectRevision, Boolean myIgnoreIntegrityCheckFailures, AFSObject myAFSObject);


        public Exceptional<PT> GetObject<PT>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, UInt64 myObjectCopy, Boolean myIgnoreIntegrityCheckFailures, SessionToken mySessionToken) where PT : AFSObject, new()
        {
            return GetObject<PT>(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, myObjectCopy, myIgnoreIntegrityCheckFailures, new Func<PT>(delegate { return new PT(); }), mySessionToken);
        }


        #region GetObject<PT>(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, myObjectCopy, myIgnoreIntegrityCheckFailures, mySessionToken)

        public Exceptional<PT> GetObject<PT>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, UInt64 myObjectCopy, Boolean myIgnoreIntegrityCheckFailures, Func<PT> myFunc, SessionToken mySessionToken) where PT : AFSObject
        {

            lock (this)
            {


                PT newT = myFunc();

                #region Input validation

                using (var _Exceptional2 = new Exceptional<PT>().NotNullOrEmptyMsg<PT>("The myObjectLocation must not be null or its length zero!", myObjectLocation))
                {
                    if (_Exceptional2.Failed)
                        return _Exceptional2;
                }

                if (myObjectStream == null)
                    myObjectStream = newT.ObjectStream;

                if (myObjectEdition == null)
                    myObjectEdition = FSConstants.DefaultEdition;

                ObjectStream   _ObjectStream   = null;
                ObjectEdition  _ObjectEdition  = null;
                ObjectRevision _ObjectRevision = null;

                #endregion

                var _Exceptional = new Exceptional<PT>();

                try
                {

                    var _ObjectLocatorExceptional = GetObjectLocator_protected(myObjectLocation, mySessionToken).
                        WhenFailed<ObjectLocator>(e => e.PushT(new GraphFSError_ObjectLocatorNotFound(myObjectLocation)));

                    if (_ObjectLocatorExceptional.Failed)
                        return _ObjectLocatorExceptional.Convert<PT>();

                    if (_ObjectLocatorExceptional != null && _ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null)
                    {

                        #region Resolve ObjectStream, -Edition and -RevisionID

                        if (_ObjectLocatorExceptional.Value.ContainsKey(myObjectStream))
                        {

                            _ObjectStream = _ObjectLocatorExceptional.Value[myObjectStream];

                            if (_ObjectStream != null)
                            {

                                if (_ObjectStream.ContainsKey(myObjectEdition))
                                {

                                    _ObjectEdition = _ObjectStream[myObjectEdition];

                                    if (_ObjectEdition != null)
                                    {

                                        if (_ObjectEdition.IsDeleted)
                                            return new Exceptional<PT>(new GraphFSError_ObjectNotFound(myObjectLocation));

                                        // If nothing specified => Return the LatestRevision
                                        if (myObjectRevisionID == null || myObjectRevisionID.UUID == null)
                                        {
                                            _ObjectRevision    = _ObjectEdition.LatestRevision;
                                            myObjectRevisionID = _ObjectEdition.LatestRevisionID;
                                        }

                                        else
                                        {
                                            _ObjectRevision = _ObjectEdition[myObjectRevisionID];
                                        }

                                    }
                                    else
                                        return new Exceptional<PT>(new GraphFSError_NoObjectRevisionsFound(myObjectLocation, myObjectStream, myObjectEdition));

                                }
                                else
                                    return new Exceptional<PT>(new GraphFSError_ObjectEditionNotFound(myObjectLocation, myObjectEdition, myObjectStream));

                            }
                            else
                                return new Exceptional<PT>(new GraphFSError_NoObjectEditionsFound(myObjectLocation, myObjectStream));

                        }
                        else
                            return new Exceptional<PT>(new GraphFSError_ObjectStreamNotFound(myObjectLocation, myObjectStream));

                        #endregion

                        if (_ObjectRevision != null && _ObjectRevision.CacheUUID != null)
                        {

                            #region Try to get the object from the internal cache...

                            var _GetObjectFromCacheExceptional = CacheGet<PT>(_ObjectRevision.CacheUUID);

                            if (_GetObjectFromCacheExceptional != null && _GetObjectFromCacheExceptional.Success && _GetObjectFromCacheExceptional.Value != null)
                            {
                                
                                _Exceptional.Value       = _GetObjectFromCacheExceptional.Value;

                                if (_Exceptional.Value.ObjectLocatorReference == null)
                                    _Exceptional.Value.ObjectLocatorReference = _ObjectLocatorExceptional.Value;

                                if (_Exceptional.Value.INodeReference == null)
                                    _Exceptional.Value.INodeReference = _ObjectLocatorExceptional.Value.INodeReference;

                                _Exceptional.Value.isNew = false;

                                return _Exceptional;

                            }

                            #endregion

                            #region ...or try to load the object via the IGraphFS internal loading-mechanism!

                            else
                            {

                                if (_ObjectRevision.Count > 2)
                                {
                                    Debug.WriteLine("_ObjectRevision.Count > 2");
                                }

                                //var _LoadObjectExceptional = LoadObject_protected<PT>(_ObjectLocatorExceptional.Value, _ObjectRevision, myIgnoreIntegrityCheckFailures);
                                var _LoadObjectExceptional = LoadAFSObject_protected(_ObjectLocatorExceptional.Value, _ObjectRevision, myIgnoreIntegrityCheckFailures, newT);

                                if (_LoadObjectExceptional != null && _LoadObjectExceptional.Success && _LoadObjectExceptional.Value != null)
                                {

                                    _LoadObjectExceptional.Value.ObjectStream      = myObjectStream;
                                    _LoadObjectExceptional.Value.ObjectStream      = myObjectStream;
                                    _LoadObjectExceptional.Value.ObjectEdition     = myObjectEdition;
                                    _LoadObjectExceptional.Value.ObjectRevision    = myObjectRevisionID;

                                    // Cache the loaded object
                                    CacheAdd(_ObjectRevision.CacheUUID, _LoadObjectExceptional.Value, false);

                                    //_Exceptional.Value = _LoadObjectExceptional.Value;
                                    _Exceptional.Value = _LoadObjectExceptional.Value as PT;

                                }

                                else
                                {
                                    // ErrorHandling!
                                    return _Exceptional.PushT(new GraphFSError_AllObjectCopiesFailed(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID));
                                }

                            }

                            #endregion

                        }

                        else
                            _Exceptional.PushT(new GraphFSError("Could not find a valid CacheUUID for this object!"));

                    }

                }

                catch (Exception e)
                {
                    return new Exceptional<PT>(new GraphFSError(e.Message));
                }

                return _Exceptional;

            }

        }

        #endregion


        protected abstract Exceptional StoreAFSObject_protected(ObjectLocation myObjectLocation, AFSObject myAPandoraObject, Boolean myAllowOverwritting, SessionToken mySessionToken);

        #region StoreFSObject(myObjectLocation, myAPandoraObject, myAllowOverwritting, SessionToken mySessionToken)

        public Exceptional StoreFSObject(ObjectLocation myObjectLocation, AFSObject myAFSObject, Boolean myAllowOverwritting, SessionToken mySessionToken)
        {

            lock (this)
            {

                //Debug.WriteLine(String.Format("StoreObject {0}:{1}", myObjectLocation, myAFSObject.ObjectStream));

                var _Exceptional = new Exceptional();

                #region Sanity Checks and OnSaveEvent

                _Exceptional.NotNullMsg("APandoraObject must not be null!", myAFSObject);
                _Exceptional.NotNullOrEmptyMsg("The ObjectStream must not be null or its length zero!", myAFSObject.ObjectStream);

                if (_Exceptional.Failed)
                    return _Exceptional;

                myAFSObject.OnSaveEvent(EventArgs.Empty);


                #endregion

                #region Check/Set myAPandoraObject.ObjectStream/-Edition/-RevisionID

                if (myAFSObject.ObjectEdition == null)
                    myAFSObject.ObjectEdition = FSConstants.DefaultEdition;

                // Use the _ForestUUID to distinguish ObjectRevisions in a distributed environment!
                if (myAFSObject.ObjectRevision == null)
                    myAFSObject.ObjectRevision = new RevisionID(_ForestUUID);

                #endregion

                #region Check/Set the ObjectLocator and the INode

                if (myAFSObject.ObjectLocatorReference == null)
                {

                    // Will load the ParentIDirectoryObject
                    var _APandoraObjectLocator = GetObjectLocator(myObjectLocation, mySessionToken);

                    if (_APandoraObjectLocator.Failed)
                    {

                        var _ParentIDirectoryObjectLocator = GetObjectLocator(new ObjectLocation(myObjectLocation.Path), mySessionToken);

                        if (_ParentIDirectoryObjectLocator.Failed)
                        {
                            //return _ParentIDirectoryObjectLocator.Push(new GraphFSError_ObjectLocatorNotFound(new ObjectLocation(myObjectLocation.Path)));
                            throw new GraphFSException("ObjectLocator for parent ObjectLocation '" + myObjectLocation.Path + "' was not found!");
                        }

                    }

                    myAFSObject.ObjectLocatorReference = _APandoraObjectLocator.Value;

                    if (myAFSObject.ObjectLocatorReference == null)
                    {

                        myAFSObject.ObjectLocatorReference = new ObjectLocator(myAFSObject.ObjectLocation, myAFSObject.ObjectUUID);
                        myAFSObject.ObjectLocatorReference.INodeReference = new INode(myAFSObject.ObjectUUID);
                        myAFSObject.ObjectLocatorReference.INodeReference.LastModificationTime = myAFSObject.ObjectRevision.Timestamp;

                        myAFSObject.ObjectLocatorReference.INodeReference.ObjectLocatorReference = myAFSObject.ObjectLocatorReference;

                    }

                    myAFSObject.INodeReference = myAFSObject.ObjectLocatorReference.INodeReference;

                }

                if (myAFSObject.INodeReference.ObjectLocatorReference == null)
                    myAFSObject.INodeReference.ObjectLocatorReference = myAFSObject.ObjectLocatorReference;

                if (myAFSObject.ObjectLocatorReference.INodeReference == null)
                    myAFSObject.ObjectLocatorReference.INodeReference = myAFSObject.INodeReference;

                #endregion

                #region Check/Set the ObjectStream and ObjectEdition

                // Add ObjectStreamName
                if (myAFSObject.ObjectLocatorReference == null ||
                    myAFSObject.ObjectLocatorReference.ContainsKey(myAFSObject.ObjectStream) == false)
                    myAFSObject.ObjectLocatorReference.Add(myAFSObject.ObjectStream, null);


                // Add ObjectStream
                if (myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream] == null)
                    myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream] = new ObjectStream(myAFSObject.ObjectStream, myAFSObject.ObjectEdition, null);

                // Add ObjectEditionName
                else if (!myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream].ContainsKey(myAFSObject.ObjectEdition))
                    myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream].Add(myAFSObject.ObjectEdition, null);


                // Add ObjectEdition
                if (myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition] == null)
                    myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition] = new ObjectEdition(myAFSObject.ObjectEdition, myAFSObject.ObjectRevision, null)
                    {
                        IsDeleted = false,
                        MinNumberOfRevisions = FSConstants.MIN_NUMBER_OF_REVISIONS,
                        MaxNumberOfRevisions = FSConstants.MAX_NUMBER_OF_REVISIONS,
                        MinRevisionDelta = FSConstants.MIN_REVISION_DELTA,
                        MaxRevisionAge = FSConstants.MAX_REVISION_AGE
                    };

                else

                    // The ObjectEdition might be marked as deleted before => remove this mark and store another ObjectRevision
                    if (myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].IsDeleted)
                        myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].IsDeleted = false;

                #endregion

                #region ObjectRevision

                #region Create the first ObjectRevision

                if (myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].Count() == 0)
                {
                    myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].
                        Add(myAFSObject.ObjectRevision, new ObjectRevision(myAFSObject.ObjectStream)
                    {
                        MinNumberOfCopies = FSConstants.MIN_NUMBER_OF_COPIES,
                        MaxNumberOfCopies = FSConstants.MAX_NUMBER_OF_COPIES
                    });
                }

                #endregion


                // Unknown := newer or older but not first ObjectRevision
                else
                {

                    #region Try to overwrite the latest ObjectRevision

                    if (myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].LatestRevisionID == myAFSObject.ObjectRevision)
                    {

                        if (myAllowOverwritting)
                        {

                            myAFSObject.ObjectRevision = new RevisionID(_ForestUUID);

                            myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].
                                Add(myAFSObject.ObjectRevision, new ObjectRevision(myAFSObject.ObjectStream)
                            {
                                MinNumberOfCopies = FSConstants.MIN_NUMBER_OF_COPIES,
                                MaxNumberOfCopies = FSConstants.MAX_NUMBER_OF_COPIES
                            });

                        }

                        else
                        {
                            return _Exceptional.Push(new GraphFSError_CouldNotOverwriteRevision(myAFSObject.ObjectLocation, myAFSObject.ObjectStream, myAFSObject.ObjectEdition, myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].LatestRevisionID));
                        }

                    }

                    #endregion

                    #region Try to add a very old ObjectRevision

                    else if (myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].LatestRevisionID > myAFSObject.ObjectRevision)
                    {
                        if (myAllowOverwritting)
                        {

                            myAFSObject.ObjectRevision = new RevisionID(_ForestUUID);

                            myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].
                                Add(myAFSObject.ObjectRevision, new ObjectRevision(myAFSObject.ObjectStream)
                            {
                                MinNumberOfCopies = FSConstants.MIN_NUMBER_OF_COPIES,
                                MaxNumberOfCopies = FSConstants.MAX_NUMBER_OF_COPIES
                            });

                        }

                        else
                        {
                            return _Exceptional.Push(new GraphFSError_CouldNotOverwriteRevision(myAFSObject.ObjectLocation, myAFSObject.ObjectStream, myAFSObject.ObjectEdition, myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition].LatestRevisionID));
                        }
                    }

                    #endregion

                    #region Add a very new ObjectRevision

                    else// if (myAPandoraObject.ObjectLocatorReference[myAPandoraObject.ObjectStream][myAPandoraObject.ObjectEdition][myAPandoraObject.ObjectRevision] == null)
                    {
                        myAFSObject.ObjectLocatorReference[myAFSObject.ObjectStream][myAFSObject.ObjectEdition][myAFSObject.ObjectRevision] =
                            new ObjectRevision(myAFSObject.ObjectStream)
                        {
                            MinNumberOfCopies = FSConstants.MIN_NUMBER_OF_COPIES,
                            MaxNumberOfCopies = FSConstants.MAX_NUMBER_OF_COPIES
                        };
                    }

                    #endregion

                }


                //// Check if there is already an existing revision
                //if (myAPandoraObject.ObjectLocatorReference[myAPandoraObject.ObjectStream][myAPandoraObject.ObjectEdition].ULongCount() > 0 && !myAllowOverwritting)
                //{
                //    _Exceptional.Add(new GraphFSError_CouldNotOverwriteRevision(myAPandoraObject.ObjectLocation, myAPandoraObject.ObjectStream, myAPandoraObject.ObjectEdition, myAPandoraObject.ObjectLocatorReference[myAPandoraObject.ObjectStream][myAPandoraObject.ObjectEdition].LatestRevisionID));
                //    //return _Exceptional;
                //    throw new PandoraFSException_ObjectStreamAlreadyExists(myAPandoraObject.ObjectStream + " '" + myObjectLocation + "' already exists!");
                //}

                //else if (!myAPandoraObject.ObjectLocatorReference[myAPandoraObject.ObjectStream][myAPandoraObject.ObjectEdition].ContainsKey(myAPandoraObject.ObjectRevisionID))
                //    myAPandoraObject.ObjectLocatorReference[myAPandoraObject.ObjectStream][myAPandoraObject.ObjectEdition].Add(myAPandoraObject.ObjectRevisionID, null);

                #endregion

                StoreAFSObject_protected(myObjectLocation, myAFSObject, myAllowOverwritting, mySessionToken);

                myAFSObject.OnSavedEvent(EventArgs.Empty);

                if (_NotificationDispatcher != null)
                {

                    var args = new NObjectStored.Arguments(myObjectLocation, myAFSObject.ObjectStream, myAFSObject.ObjectEdition, myAFSObject.ObjectRevision);

                    //Console.WriteLine("Notify {0} {1} {2} {3}!", myObjectLocation, myAPandoraObject.ObjectStream, myAPandoraObject.ObjectEdition, myAPandoraObject.ObjectRevisionID);
                    _NotificationDispatcher.SendNotification(typeof(NObjectStored), args);

                }

                return new Exceptional();

            }

        }

        #endregion


        #region ObjectExists(myObjectLocation)

        public Exceptional<Trinary> ObjectExists(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            //myObjectLocation = ResolveObjectLocation(myObjectLocation, false, mySessionToken);

            if (myObjectLocation == null)
                return new Exceptional<Trinary>(Trinary.FALSE);

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.ObjectExists(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

            #endregion

            var _Exceptional = new Exceptional<Trinary>();
            var _ParentDirectoryObjectExceptional = GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), FSConstants.DIRECTORYSTREAM, null, null, 0, false, mySessionToken);

            if (_ParentDirectoryObjectExceptional != null && _ParentDirectoryObjectExceptional.Success && _ParentDirectoryObjectExceptional.Value != null)
                _Exceptional.Value = _ParentDirectoryObjectExceptional.Value.ObjectExists(myObjectLocation.Name);

            else
            {
                _Exceptional.PushT(new GraphFSError_DirectoryObjectNotFound(myObjectLocation.Path));
//                _Exceptional.Add(_ParentDirectoryObjectExceptional.Errors);
                _Exceptional.Value = Trinary.FALSE;
            }

            return _Exceptional;

        }

        #endregion

        #region ObjectStreamExists(myObjectLocation, myObjectStream, mySessionToken)

        public Exceptional<Trinary> ObjectStreamExists(ObjectLocation myObjectLocation, String myObjectStream, SessionToken mySessionToken)
        {

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            //myObjectLocation = ResolveObjectLocation(myObjectLocation, false, mySessionToken);

            //if (myObjectLocation == null)
            //    return Trinary.FALSE;

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.ObjectStreamExists(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myObjectStream, mySessionToken);

            #endregion

            var _Exceptional = new Exceptional<Trinary>();
            var _ParentDirectoryObjectExceptional = GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), FSConstants.DIRECTORYSTREAM, null, null, 0, false, mySessionToken);

            if (_ParentDirectoryObjectExceptional != null && _ParentDirectoryObjectExceptional.Success && _ParentDirectoryObjectExceptional.Value != null)
            {

                // Get DirectoryEntry
                var _DirectoryEntry = _ParentDirectoryObjectExceptional.Value.GetDirectoryEntry(myObjectLocation.Name);

                if (_DirectoryEntry != null && _DirectoryEntry.ObjectStreamsList != null)
                    _Exceptional.Value = _DirectoryEntry.ObjectStreamsList.Contains(myObjectStream);

                else _Exceptional.Value = Trinary.FALSE;

            }

            else
            {
                _Exceptional = new Exceptional<Trinary>(_ParentDirectoryObjectExceptional);
                _Exceptional.Push(new GraphFSError_DirectoryObjectNotFound(myObjectLocation));
                _Exceptional.Value = Trinary.FALSE;
            }

            return _Exceptional;

        }

        #endregion

        #region ObjectEditionExists(myObjectLocation, myObjectStream, myObjectEdition, mySessionToken)

        public Exceptional<Trinary> ObjectEditionExists(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, SessionToken mySessionToken)
        {

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            myObjectLocation = ResolveObjectLocation(myObjectLocation, false, mySessionToken);

            if (myObjectLocation == null)
                return new Exceptional<Trinary>() { Value = Trinary.FALSE };

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.ObjectEditionExists(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myObjectStream, myObjectEdition, mySessionToken);

            #endregion

            var _Exceptional = new Exceptional<Trinary>();
            var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional != null && _ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null)
            {

                // Get ObjectStream
                var _ObjectStream = _ObjectLocatorExceptional.Value[myObjectStream];

                if (_ObjectStream != null)
                    _Exceptional.Value = _ObjectStream.ContainsKey(myObjectEdition);

                else
                {
                    _Exceptional.PushT(new GraphFSError_ObjectStreamNotFound(myObjectLocation, myObjectStream));
                    _Exceptional.Value = Trinary.FALSE;
                }

            }

            else
            {
                _Exceptional = new Exceptional<Trinary>(_ObjectLocatorExceptional);
                _Exceptional.PushT(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));
                _Exceptional.Value = Trinary.FALSE;
            }

            return _Exceptional;

        }

        #endregion

        #region ObjectRevisionExists(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, mySessionToken)

        public Exceptional<Trinary> ObjectRevisionExists(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, SessionToken mySessionToken)
        {

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            myObjectLocation = ResolveObjectLocation(myObjectLocation, false, mySessionToken);

            if (myObjectLocation == null)
                return new Exceptional<Trinary>() { Value = Trinary.FALSE };

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.ObjectRevisionExists(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myObjectStream, myObjectEdition, myObjectRevisionID, mySessionToken);

            #endregion

            var _Exceptional = new Exceptional<Trinary>();
            var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional != null && _ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null)
            {

                // Get ObjectStream
                var _ObjectStream = _ObjectLocatorExceptional.Value[myObjectStream];

                if (_ObjectStream != null && _ObjectStream.ContainsKey(myObjectEdition))
                {

                    // Get ObjectEdition
                    var _ObjectEdition = _ObjectStream[myObjectEdition];

                    if (_ObjectEdition != null)
                        _Exceptional.Value = _ObjectEdition.ContainsKey(myObjectRevisionID);
                    
                    else
                    {
                        _Exceptional.PushT(new GraphFSError_ObjectEditionNotFound(myObjectLocation, myObjectStream, myObjectEdition));
                        _Exceptional.Value = Trinary.FALSE;
                    }

                }

                else
                {
                    _Exceptional.PushT(new GraphFSError_ObjectStreamNotFound(myObjectLocation, myObjectStream));
                    _Exceptional.Value = Trinary.FALSE;
                }

            }

            else
            {
                _Exceptional = new Exceptional<Trinary>(_ObjectLocatorExceptional);
                _Exceptional.PushT(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));
                _Exceptional.Value = Trinary.FALSE;
            }

            return _Exceptional;

        }

        #endregion


        #region GetObjectStreams(myObjectLocation, mySessionToken)

        public Exceptional<IEnumerable<String>> GetObjectStreams(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            lock (this)
            {

                var _Exceptional = new Exceptional<IEnumerable<String>>();

                #region Special handling of the root directory as we can not ask its parent directory!

                if (myObjectLocation.Equals(FSPathConstants.PathDelimiter))
                {

                    var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

                    if (_ObjectLocatorExceptional != null && _ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null)
                        _Exceptional.Value = _ObjectLocatorExceptional.Value.Keys;

                    else
                    {
                        _Exceptional = _ObjectLocatorExceptional.Convert<IEnumerable<String>>().
                            PushT(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));
                    }

                }

                #endregion

                #region Just ask the parent directory, as this avoids unnecessary loading of the INode and ObjectLocator!

                else
                {

                    var _ParentDirectoryObjectExceptional = GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), FSConstants.DIRECTORYSTREAM, null, null, 0, false, mySessionToken);

                    if (_ParentDirectoryObjectExceptional != null && _ParentDirectoryObjectExceptional.Success && _ParentDirectoryObjectExceptional.Value != null)
                    {

                        // Get DirectoryEntry
                        var _DirectoryEntry = _ParentDirectoryObjectExceptional.Value.GetDirectoryEntry(myObjectLocation.Name);

                        if (_DirectoryEntry != null && _DirectoryEntry.ObjectStreamsList != null)
                            _Exceptional.Value = _DirectoryEntry.ObjectStreamsList;

                        else
                            _Exceptional.PushT(new GraphFSError_CouldNotGetObjectStreams(myObjectLocation));

                    }

                    else
                    {
                        _Exceptional = _ParentDirectoryObjectExceptional.Convert<IEnumerable<String>>().
                            PushT(new GraphFSError_DirectoryObjectNotFound(myObjectLocation));
                    }

                }

                #endregion

                return _Exceptional;

            }

        }

        #endregion

        #region GetObjectEditions(myObjectLocation, myObjectStream, mySessionToken)

        public Exceptional<IEnumerable<String>> GetObjectEditions(ObjectLocation myObjectLocation, String myObjectStream, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional<IEnumerable<String>>();
            var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional != null && _ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null)
            {

                // Get ObjectStream
                var _ObjectStream = _ObjectLocatorExceptional.Value[myObjectStream];

                if (_ObjectStream != null)
                    _Exceptional.Value = _ObjectStream.Keys;

                else
                    _Exceptional.PushT(new GraphFSError_ObjectStreamNotFound(myObjectLocation, myObjectStream));

            }

            else
            {
                _Exceptional = _ObjectLocatorExceptional.Convert<IEnumerable<String>>().
                    PushT(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));
            }

            return _Exceptional;

        }

        #endregion

        #region GetObjectRevisionIDs(myObjectLocation, myObjectStream, myObjectEdition, mySessionToken)

        public Exceptional<IEnumerable<RevisionID>> GetObjectRevisionIDs(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional<IEnumerable<RevisionID>>();
            var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional != null && _ObjectLocatorExceptional.Success && _ObjectLocatorExceptional.Value != null)
            {

                // Get ObjectStream
                var _ObjectStream = _ObjectLocatorExceptional.Value[myObjectStream];

                if (_ObjectStream != null && _ObjectStream.ContainsKey(myObjectEdition))
                {

                    // Get ObjectEdition
                    var _ObjectEdition = _ObjectStream[myObjectEdition];

                    if (_ObjectEdition != null)
                        _Exceptional.Value = _ObjectEdition.Keys;

                    else
                        _Exceptional.PushT(new GraphFSError_ObjectEditionNotFound(myObjectLocation, myObjectStream, myObjectEdition));

                }

                else
                    _Exceptional.PushT(new GraphFSError_ObjectStreamNotFound(myObjectLocation, myObjectStream));

            }

            else
            {
                _Exceptional = _ObjectLocatorExceptional.Convert<IEnumerable<RevisionID>>().
                    PushT(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));
            }

            return _Exceptional;

        }

        #endregion


        #region RenameObject(myObjectLocation, myNewObjectName, mySessionToken)

        public Exceptional RenameObject(ObjectLocation myObjectLocation, String myNewObjectName, SessionToken mySessionToken)
        {

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            myObjectLocation = ResolveObjectLocation(myObjectLocation, false, mySessionToken);

            if (myObjectLocation == null)
                throw new ArgumentNullException("myObjectLocation must not be null!");

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.RenameObject(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myNewObjectName, mySessionToken);

            #endregion

            var _Exceptional = new Exceptional();

            var _ParentDirectoryObjectExceptional = GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), FSConstants.DIRECTORYSTREAM, null, null, 0, false, mySessionToken);

            if (_ParentDirectoryObjectExceptional != null && _ParentDirectoryObjectExceptional.Success && _ParentDirectoryObjectExceptional.Value != null)
            {

                if (!_ParentDirectoryObjectExceptional.Value.RenameDirectoryEntry(myObjectLocation.Name, myNewObjectName))
                    _Exceptional.Push(new GraphFSError_ObjectNotFound(myObjectLocation));

                var _GetObjectLocatorExceptional = GetObjectLocator_protected(myObjectLocation, mySessionToken);
                if (_GetObjectLocatorExceptional == null || _GetObjectLocatorExceptional.Failed || _GetObjectLocatorExceptional.Value == null)
                    _Exceptional.Push(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));

                else
                {
                    CacheMove(myObjectLocation, new ObjectLocation(myObjectLocation.Path, myNewObjectName), true);
                }

                return _Exceptional;

            }

            else
            {
                _Exceptional = _ParentDirectoryObjectExceptional.Convert<IEnumerable<RevisionID>>().
                    PushT(new GraphFSError_DirectoryObjectNotFound(myObjectLocation.Path));
            }

            return _Exceptional;

        }

        #endregion

        #region RemoveObject(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, mySessionToken)

        protected abstract Exceptional RemoveObject_protected(ObjectLocator myObjectLocator, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, SessionToken mySessionToken);

        public Exceptional RemoveObject(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, SessionToken mySessionToken)
        {

            #region GetObjectLocator

            var _Exceptional = new Exceptional<IEnumerable<String>>();
            var _ObjectLocatorExceptional = GetObjectLocator(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional == null || _ObjectLocatorExceptional.Failed || _ObjectLocatorExceptional.Value == null)
                return _Exceptional.Push(new GraphFSError_ObjectLocatorNotFound(myObjectLocation));

            #endregion

            #region Check the ObjectOntology

            var _ObjectLocator = _ObjectLocatorExceptional.Value;

            // ObjectStream
            if (myObjectStream == null || _ObjectLocator.ContainsKey(myObjectStream) == false)
                return new Exceptional().Push(new GraphFSError("Invalid ObjectStream '" + myObjectStream + "'!"));

            var _ObjectStream = _ObjectLocator[myObjectStream];

            // ObjectEdition
            if (myObjectEdition == null || _ObjectStream.ContainsKey(myObjectEdition) == false)
                return new Exceptional().Push(new GraphFSError("Invalid ObjectEdition '" + myObjectEdition + "'!"));

            var _ObjectEdition = _ObjectStream[myObjectEdition];

            if (_ObjectEdition.IsDeleted)
                return Exceptional.OK;

            // ObjectRevision
            if (myObjectRevisionID == null || _ObjectEdition.ContainsKey(myObjectRevisionID) == false)
                return new Exceptional().Push(new GraphFSError("Invalid ObjectRevisionID '" + myObjectRevisionID + "'!"));

            var _ObjectRevision = _ObjectEdition[myObjectRevisionID];

            #endregion

            #region Remove FSObject from ObjectCache

            try
            {
                // ErrorHandling?!
                CacheRemove(_ObjectRevision.CacheUUID);
            }
            catch (Exception)
            { }

            #endregion

            #region Mark ObjectEdition as deleted

            _ObjectEdition.IsDeleted = true;
            RemoveObject_protected(_ObjectLocator, myObjectStream, myObjectEdition, myObjectRevisionID, mySessionToken);

            //var _success = _ObjectEdition.Remove(myObjectRevisionID);

            //// If the last ObjectRevision was removed => Remove the ObjectEdition
            //if (_ObjectEdition.Count == 0)
            //{

            //    _ObjectStream.Remove(myObjectEdition);

            //    // If the last ObjectEdition was removed => Remove the ObjectEdition
            //    if (_ObjectStream.Count == 0)
            //    {

            //        _ObjectLocator.Remove(myObjectStream);

            //        if (_ObjectLocator.Count == 0)
            //            CacheRemove(myObjectLocation, false);

            //    }

            //}

            #endregion

            #region Remove ObjectStream from ParentIDirectoryObject

            return GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), null, null, null, 0, false, mySessionToken).
                WhenFailed<DirectoryObject>(e => e.PushT(new GraphFSError_DirectoryObjectNotFound(myObjectLocation.Path))).
                WhenSucceded<DirectoryObject>(e =>
                {

                    e.Value.IGraphFSReference = this;
                    e.Value.RemoveObjectStream(myObjectLocation.Name, myObjectStream);

                    var _ObjectStreamsList = e.Value.GetObjectStreamsList(myObjectLocation.Name);

                    if (_ObjectStreamsList.Contains(FSConstants.ACCESSCONTROLSTREAM) &&
                        _ObjectStreamsList.CountIs(2))
                        e.Value.RemoveObjectStream(myObjectLocation.Name, FSConstants.ACCESSCONTROLSTREAM);

                    return e;

                }
                );

            #endregion

        }

        #endregion

        #region EraseObject(myObjectLocation, myObjectStream, myObjectEdition, myObjectRevisionID, mySessionToken)

        public Exceptional EraseObject(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevisionID, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Symlink methods

        protected abstract Exceptional AddSymlink_protected(ObjectLocation myObjectLocation, ObjectLocation myTargetLocation, SessionToken mySessionToken);

        #region AddSymlink(myObjectLocation, myTargetLocation, mySessionToken)

        public Exceptional AddSymlink(ObjectLocation myObjectLocation, ObjectLocation myTargetLocation, SessionToken mySessionToken)
        {

            #region Resolve all symlinks...

            myObjectLocation = new ObjectLocation(DirectoryHelper.Combine(ResolveObjectLocation(new ObjectLocation(myObjectLocation.Path), true, mySessionToken),
                                                  DirectoryHelper.GetObjectName(myObjectLocation)));

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
            {
                _ChildIPandoraFS.AddSymlink(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myTargetLocation, mySessionToken);
                return new Exceptional();
            }

            #endregion

            return AddSymlink_protected(myObjectLocation, myTargetLocation, mySessionToken);

        }

        #endregion

        #region isSymlink(myObjectLocation, SessionToken mySessionToken)

        public Exceptional<Trinary> isSymlink(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            var myIParentDirectoryLocation = ResolveObjectLocation(new ObjectLocation(myObjectLocation.Path), true, mySessionToken);
            if (myIParentDirectoryLocation.Length == 0)
                return new Exceptional<Trinary>(Trinary.FALSE);

            myObjectLocation = new ObjectLocation(DirectoryHelper.Combine(myIParentDirectoryLocation, myObjectLocation.Name));

            IGraphFS _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.isSymlink(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

            #endregion

            lock (this)
            {

                var _Exceptional = new Exceptional<Trinary>();

                // Add symlink to ParentIDirectoryObject
                var _ParentIDirectoryObject = GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), null, null, null, 0, false, mySessionToken);

                if (_ParentIDirectoryObject.Failed || _ParentIDirectoryObject.Value == null)
                {
                    return _ParentIDirectoryObject.Convert<Trinary>().PushT(new GraphFSError_DirectoryObjectNotFound(myObjectLocation.Path));
                }

                _Exceptional.Value = _ParentIDirectoryObject.Value.isSymlink(myObjectLocation.Name);

                return _Exceptional;

            }

        }

        #endregion

        #region GetSymlink(myObjectLocation, SessionToken mySessionToken)

        public Exceptional<ObjectLocation> GetSymlink(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            #region Resolve all symlinks...

            myObjectLocation = new ObjectLocation(DirectoryHelper.Combine(ResolveObjectLocation(new ObjectLocation(myObjectLocation.Path), true, mySessionToken),
                                                       DirectoryHelper.GetObjectName(myObjectLocation)));

            IGraphFS _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.GetSymlink(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

            #endregion

            lock (this)
            {

                var _Exceptional = new Exceptional<ObjectLocation>();

                // Add symlink to ParentIDirectoryObject
                var _ParentIDirectoryObject = GetObject<DirectoryObject>(new ObjectLocation(myObjectLocation.Path), null, null, null, 0, false, mySessionToken);

                if (_ParentIDirectoryObject.Failed || _ParentIDirectoryObject.Value == null)
                {
                    return _ParentIDirectoryObject.Convert<ObjectLocation>().PushT(new GraphFSError_DirectoryObjectNotFound(myObjectLocation.Path));
                }

                _Exceptional.Value = _ParentIDirectoryObject.Value.GetSymlink(myObjectLocation.Name);

                return _Exceptional;

            }

        }

        #endregion

        #region RemoveSymlink(myObjectLocation, SessionToken mySessionToken)

        /// <summary>
        /// This method removes a symlink from the parent directory.
        /// </summary>
        /// <param name="myObjectLocation">the location of this object (ObjectPath and ObjectName) of the requested file within the file system</param>
        public Exceptional RemoveSymlink(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            #region Check if the ObjectLocation is a symlink

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            if (isSymlink(myObjectLocation, mySessionToken).Value != Trinary.TRUE)
                throw new PandoraFSException_SymlinkCouldNotBeRemoved("Only symlinks can be removed by this method!");

            #endregion

            #region Resolve all symlinks and call myself on a possible ChildFileSystem...

            if (!isMounted)
                throw new PandoraFSException_FileSystemNotMounted("Please mount a file system first!");

            // Will throw an exception if the ObjectLocation could not be resolved
            myObjectLocation = new ObjectLocation(DirectoryHelper.Combine(ResolveObjectLocation(new ObjectLocation(myObjectLocation), true, mySessionToken),
                                                       DirectoryHelper.GetObjectName(myObjectLocation)));

            IGraphFS _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.RemoveSymlink(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

            #endregion


            //if (mySessionToken.Transaction == null)
            //    using (FSTransaction transaction = beginTransaction(mySessionToken, true))
            //    {
            //        RemoveAPandoraObject_private(myObjectLocation, FSConstants.SYMLINK, null, null, mySessionToken);
            //        CommitTransaction(mySessionToken);
            //    }
            //else
            return RemoveObject(myObjectLocation, FSConstants.SYMLINK, null, null, mySessionToken);

        }

        #endregion

        #endregion

        #region DirectoryObject Methods

        protected abstract IDirectoryObject CreateDirectoryObject_protected(ObjectLocation myObjectLocation, UInt64 myBlocksize);

        #region CreateDirectoryObject(myObjectLocation, myBlocksize, mySessionToken)

        public Exceptional<IDirectoryObject> CreateDirectoryObject(ObjectLocation myObjectLocation, UInt64 myBlocksize, SessionToken mySessionToken)
        {

            //if (mySessionToken.Transaction == null)
            //    BeginTransaction(mySessionToken, true);

            var _Exceptional = new Exceptional<IDirectoryObject>();

            try
            {                

                #region Initialize a new DirectoryObject

                // Just create a DirectoryObject within the IGraphFS implementation
                var _IDirectoryObject = CreateDirectoryObject_protected(myObjectLocation, myBlocksize);

                // Add standard subdirectories to the new DirectoryObject
                _IDirectoryObject.AddObjectStream(FSConstants.DotLink,     FSConstants.DIRECTORYSTREAM, new List<ExtendedPosition> { new ExtendedPosition(0, 0) });
                _IDirectoryObject.AddObjectStream(FSConstants.DotDotLink,  FSConstants.DIRECTORYSTREAM, new List<ExtendedPosition> { new ExtendedPosition(0, 0) });
                _IDirectoryObject.AddObjectStream(FSConstants.DotMetadata, FSConstants.VIRTUALDIRECTORY);
                _IDirectoryObject.AddObjectStream(FSConstants.DotSystem,   FSConstants.VIRTUALDIRECTORY);
                _IDirectoryObject.AddObjectStream(FSConstants.DotStreams,  FSConstants.VIRTUALDIRECTORY);
                _IDirectoryObject.StoreInlineData(FSConstants.DotUUID,     _IDirectoryObject.ObjectUUID.GetByteArray(), true);

                #endregion

                #region Store the new DirectoryObject

                var _AFSObject = _IDirectoryObject as AFSObject;

                if (_AFSObject == null)
                    throw new GraphFSException("'_AFSObject = _IDirectoryObject as AFSObject' failed!");

                using (var _StoreDirectoryObjectExceptional = StoreFSObject(myObjectLocation, _AFSObject, true, mySessionToken))
                {
                    if (_StoreDirectoryObjectExceptional.Failed)
                        return _StoreDirectoryObjectExceptional.Convert<IDirectoryObject>().PushT(new GraphFSError_CreateDirectoryFailed(myObjectLocation));
                }

                //_DirectoryObject.IPandoraFSReference = this;

                #endregion

                _Exceptional.Value = _IDirectoryObject;

            }

            catch (Exception e)
            {
                _Exceptional.PushT(new GraphFSError_CreateDirectoryFailed(myObjectLocation));
            }

            return _Exceptional;            

        }

        #endregion

        #region CreateDirectoryObject(myObjectLocation, myBlocksize, myRecursive, mySessionToken)

        public Exceptional<IDirectoryObject> CreateDirectoryObject(ObjectLocation myObjectLocation, UInt64 myBlocksize, Boolean myRecursive, SessionToken mySessionToken)
        {

            var _ObjectPath = new ObjectLocation(myObjectLocation.Path);
            var _ObjectName = myObjectLocation.Name;

            IEnumerable<String> _ObjectStreams  = new List<String>();
            IDirectoryObject    _ParentDir      = null;
            IGraphFS            _PandoraFS      = this;

            while (!ResolveObjectLocation(ref _ObjectPath, out _ObjectStreams, out _ObjectPath, out _ObjectName, out _ParentDir, out _PandoraFS, mySessionToken))
            {
                _PandoraFS.CreateDirectoryObject(new ObjectLocation(DirectoryHelper.Combine(_ParentDir.ObjectLocation, _ObjectName)), 0, mySessionToken);
                _ObjectPath = new ObjectLocation(myObjectLocation.Path);
                _ObjectName = myObjectLocation.Name;
            }

            myObjectLocation = new ObjectLocation(DirectoryHelper.Combine(ResolveObjectLocation(new ObjectLocation(myObjectLocation.Path), true, mySessionToken),
                                                  DirectoryHelper.GetObjectName(myObjectLocation)));

            return CreateDirectoryObject(myObjectLocation, myBlocksize, mySessionToken);

        }

        #endregion


        #region isIDirectoryObject(myObjectLocation, mySessionToken)

        public Exceptional<Trinary> isIDirectoryObject(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            lock (this)
            {

                return GetObjectStreams(myObjectLocation, mySessionToken).
                           ConvertWithFunc<IEnumerable<String>, Trinary>(v => v.Contains(FSConstants.DIRECTORYSTREAM)).
                           WhenFailed<Trinary>(e => new Exceptional<Trinary>(Trinary.FALSE));

                ////ToDo: Resolve SymLinks!

                //var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

                //if (_ChildIPandoraFS != this)
                //    return _ChildIPandoraFS.isIDirectoryObject(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

                //// Get ParentDirectoryObject, check ObjectStream and return Trinary.FALSE on any error!
                //return GetObject<DirectoryObject>(myObjectLocation.Path, FSConstants.DIRECTORYSTREAM, FSConstants.DefaultEdition, null, 0, false, mySessionToken).
                //    Convert<DirectoryObject, Trinary>(v => v.ObjectStreamExists(myObjectLocation.Name, FSConstants.DIRECTORYSTREAM)).
                //    WhenFailed<Trinary>(e => new Exceptional<Trinary>(Trinary.FALSE));

            }

        }

        #endregion


        #region GetDirectoryListing(myObjectLocation, mySessionToken)

        public Exceptional<IEnumerable<String>> GetDirectoryListing(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            lock (this)
            {

                //ToDo: Resolve SymLinks!

                var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

                if (_ChildIPandoraFS != this)
                    return _ChildIPandoraFS.GetDirectoryListing(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

                // Get DirectoryObject and return the directory listing
                return GetObject<DirectoryObject>(myObjectLocation, FSConstants.DIRECTORYSTREAM, FSConstants.DefaultEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc<DirectoryObject, IEnumerable<String>>(v => v.GetDirectoryListing());

            }

        }

        #endregion

        #region GetDirectoryListing(myObjectLocation, myFunc, mySessionToken)

        public Exceptional<IEnumerable<String>> GetDirectoryListing(ObjectLocation myObjectLocation, Func<KeyValuePair<String, DirectoryEntry>, Boolean> myFunc, SessionToken mySessionToken)
        {

            lock (this)
            {

                //ToDo: Resolve SymLinks!

                var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

                if (_ChildIPandoraFS != this)
                    return _ChildIPandoraFS.GetDirectoryListing(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myFunc, mySessionToken);

                // Get DirectoryObject and return the directory listing
                return GetObject<DirectoryObject>(myObjectLocation, FSConstants.DIRECTORYSTREAM, FSConstants.DefaultEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc<DirectoryObject, IEnumerable<String>>(v => v.GetDirectoryListing(myFunc));

            }

        }

        #endregion

        #region GetFilteredDirectoryListing(myObjectLocation, myLogin, myIgnoreName, myRegExpr, myObjectStream, myIgnoreObjectStreamType, mySize, myCreationTime, myModificationTime, myLastAccessTime, myDeletionTime, mySessionToken)

        public Exceptional<IEnumerable<String>> GetFilteredDirectoryListing(ObjectLocation myObjectLocation, String[] myName, String[] myIgnoreName, String[] myRegExpr, List<String> myObjectStreams, List<String> myIgnoreObjectStreams, String[] mySize, String[] myCreationTime, String[] myLastModificationTime, String[] myLastAccessTime, String[] myDeletionTime, SessionToken mySessionToken)
        {

            //ToDo: Resolve SymLinks!

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.GetFilteredDirectoryListing(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myName, myIgnoreName, myRegExpr, myObjectStreams, myIgnoreObjectStreams, mySize, myCreationTime, myLastModificationTime, myLastAccessTime, myDeletionTime, mySessionToken);

            var     _Output = new List<String>();
            INode   _INode  = null;
            Boolean _AddEntry;

            // Get DirectoryObject and return the filtered directory listing
            return GetObject<DirectoryObject>(myObjectLocation, FSConstants.DIRECTORYSTREAM, FSConstants.DefaultEdition, null, 0, false, mySessionToken).
                   ConvertWithFunc<DirectoryObject, IEnumerable<String>>(v => v.GetDirectoryListing(myName, myIgnoreName, myRegExpr, myObjectStreams, myIgnoreObjectStreams)).
                   WhenSucceded<IEnumerable<String>>(_Exceptional =>
                   {

                       #region Apply additional filters

                       if (mySize                   != null ||
                           myCreationTime           != null ||
                           myLastModificationTime   != null ||
                           myLastAccessTime         != null ||
                           myDeletionTime           != null)
                       {

                           foreach (var _ObjectName in _Exceptional.Value)
                           {

                               _AddEntry = false;

                               #region Load the INode via this methods in order to make use of the object cache

                               if (myObjectLocation.Equals(FSPathConstants.PathDelimiter))
                                   _INode = GetINode(new ObjectLocation(_ObjectName), mySessionToken).Value;

                               else
                                   _INode = GetINode(new ObjectLocation(myObjectLocation, _ObjectName), mySessionToken).Value;

                               #endregion

                               #region Parameter --size=[<|=|>]NumberOfBytes

                               if (mySize != null)
                                   try
                                   {
                                       foreach (var _Size in mySize)
                                           switch (_Size[0])
                                           {
                                               case '<': if (_INode.ObjectSize < UInt64.Parse(_Size.Substring(1))) _AddEntry = true; break;
                                               case '=': if (_INode.ObjectSize == UInt64.Parse(_Size.Substring(1))) _AddEntry = true; break;
                                               case '>': if (_INode.ObjectSize > UInt64.Parse(_Size.Substring(1))) _AddEntry = true; break;
                                               default: throw new PandoraFSException_ParsedSizeIsInvalid("The parameter 'size' could not be parsed!");
                                           }

                                   }
                                   catch (Exception)
                                   {
                                       throw new PandoraFSException_ParsedSizeIsInvalid("The parameter 'size' could not be parsed!");
                                   }

                               #endregion

                               #region Parameter --CreationTime=[<|=|>]Timestamp

                               if (myCreationTime != null)
                                   try
                                   {
                                       foreach (var _CreationTime in myCreationTime)
                                           switch (_CreationTime[0])
                                           {
                                               case '<': if (_INode.CreationTime < UInt64.Parse(_CreationTime.Substring(1))) _AddEntry = true; break;
                                               case '=': if (_INode.CreationTime == UInt64.Parse(_CreationTime.Substring(1))) _AddEntry = true; break;
                                               case '>': if (_INode.CreationTime > UInt64.Parse(_CreationTime.Substring(1))) _AddEntry = true; break;
                                               default: throw new PandoraFSException_ParsedCreationTimeIsInvalid("The parameter 'CreationTime' could not be parsed!");
                                           }

                                   }
                                   catch (Exception)
                                   {
                                       throw new PandoraFSException_ParsedCreationTimeIsInvalid("The parameter 'CreationTime' could not be parsed!");
                                   }

                               #endregion

                               #region Parameter --LastAccessTime=[<|=|>]Timestamp

                               if (myLastAccessTime != null)
                                   try
                                   {
                                       foreach (var _LastAccessTime in myLastAccessTime)
                                           switch (_LastAccessTime[0])
                                           {
                                               case '<': if (_INode.LastAccessTime < UInt64.Parse(_LastAccessTime.Substring(1))) _AddEntry = true; break;
                                               case '=': if (_INode.LastAccessTime == UInt64.Parse(_LastAccessTime.Substring(1))) _AddEntry = true; break;
                                               case '>': if (_INode.LastAccessTime > UInt64.Parse(_LastAccessTime.Substring(1))) _AddEntry = true; break;
                                               default: throw new PandoraFSException_ParsedLastAccessTimeIsInvalid("The parameter 'LastAccessTime' could not be parsed!");
                                           }

                                   }
                                   catch (Exception)
                                   {
                                       throw new PandoraFSException_ParsedLastAccessTimeIsInvalid("The parameter 'LastAccessTime' could not be parsed!");
                                   }

                               #endregion

                               #region Parameter --LastModificationTime=[<|=|>]Timestamp

                               if (myLastModificationTime != null)
                                   try
                                   {
                                       foreach (var _LastModificationTime in myLastModificationTime)
                                           switch (_LastModificationTime[0])
                                           {
                                               case '<': if (_INode.CreationTime < UInt64.Parse(_LastModificationTime.Substring(1))) _AddEntry = true; break;
                                               case '=': if (_INode.CreationTime == UInt64.Parse(_LastModificationTime.Substring(1))) _AddEntry = true; break;
                                               case '>': if (_INode.CreationTime > UInt64.Parse(_LastModificationTime.Substring(1))) _AddEntry = true; break;
                                               default: throw new PandoraFSException_ParsedLastModificationTimeIsInvalid("The parameter 'LastModificationTime' could not be parsed!");
                                           }

                                   }
                                   catch (Exception)
                                   {
                                       throw new PandoraFSException_ParsedLastModificationTimeIsInvalid("The parameter 'LastModificationTime' could not be parsed!");
                                   }

                               #endregion

                               #region Parameter --DeletionTime=[<|=|>]Timestamp

                               if (myDeletionTime != null)
                                   try
                                   {
                                       foreach (var _DeletionTime in myDeletionTime)
                                           switch (_DeletionTime[0])
                                           {
                                               case '<': if (_INode.DeletionTime < UInt64.Parse(_DeletionTime.Substring(1))) _AddEntry = true; break;
                                               case '=': if (_INode.DeletionTime == UInt64.Parse(_DeletionTime.Substring(1))) _AddEntry = true; break;
                                               case '>': if (_INode.DeletionTime > UInt64.Parse(_DeletionTime.Substring(1))) _AddEntry = true; break;
                                               default: throw new PandoraFSException_ParsedDeletionTimeIsInvalid("The parameter 'DeletionTime' could not be parsed!");
                                           }

                                   }
                                   catch (Exception)
                                   {
                                       throw new PandoraFSException_ParsedDeletionTimeIsInvalid("The parameter 'DeletionTime' could not be parsed!");
                                   }

                               #endregion


                               if (_AddEntry) _Output.Add(_ObjectName);

                           }

                       }

                       else
                           _Output.AddRange(_Exceptional.Value);

                       #endregion

                       return new Exceptional<IEnumerable<string>>(_Output);

                    });

        }

        #endregion


        #region GetExtendedDirectoryListing(myObjectLocation, mySessionToken)

        public Exceptional<IEnumerable<DirectoryEntryInformation>> GetExtendedDirectoryListing(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            lock (this)
            {

                // Resolve SymLinks!

                var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

                if (_ChildIPandoraFS != this)
                    return _ChildIPandoraFS.GetExtendedDirectoryListing(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), mySessionToken);

                // Get DirectoryObject and return the extended directory listing
                return GetObject<DirectoryObject>(myObjectLocation, FSConstants.DIRECTORYSTREAM, FSConstants.DefaultEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc<DirectoryObject, IEnumerable<DirectoryEntryInformation>>(v => v.GetExtendedDirectoryListing());

            }

        }

        #endregion

        #region GetExtendedDirectoryListing(myObjectLocation, myFunc, mySessionToken)

        public Exceptional<IEnumerable<DirectoryEntryInformation>> GetExtendedDirectoryListing(ObjectLocation myObjectLocation, Func<KeyValuePair<String, DirectoryEntry>, Boolean> myFunc, SessionToken mySessionToken)
        {

            lock (this)
            {

                // Resolve SymLinks!

                var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

                if (_ChildIPandoraFS != this)
                    return _ChildIPandoraFS.GetExtendedDirectoryListing(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myFunc, mySessionToken);

                // Get DirectoryObject and return the extended directory listing
                return GetObject<DirectoryObject>(myObjectLocation, FSConstants.DIRECTORYSTREAM, FSConstants.DefaultEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc<DirectoryObject, IEnumerable<DirectoryEntryInformation>>(v => v.GetExtendedDirectoryListing(myFunc));

            }

        }

        #endregion

        #region GetFilteredExtendedDirectoryListing(myObjectLocation, myLogin, myIgnoreName, myRegExpr, myObjectStream, myIgnoreObjectStreamType, mySize, myCreationTime, myModificationTime, myLastAccessTime, myDeletionTime, mySessionToken)

        public Exceptional<IEnumerable<DirectoryEntryInformation>> GetFilteredExtendedDirectoryListing(ObjectLocation myObjectLocation, String[] myName, String[] myIgnoreName, String[] myRegExpr, List<String> myObjectStreams, List<String> myIgnoreObjectStreamTypes, String[] mySize, String[] myCreationTime, String[] myLastModificationTime, String[] myLastAccessTime, String[] myDeletionTime, SessionToken mySessionToken)
        {

            #region Resolve all symlinks...

            //myObjectLocation = new ObjectLocation(ResolveObjectLocation(myObjectLocation, true, mySessionToken));

            #endregion

            var _ChildIPandoraFS = GetChildFileSystem(myObjectLocation, false, mySessionToken);

            if (_ChildIPandoraFS != this)
                return _ChildIPandoraFS.GetFilteredExtendedDirectoryListing(GetObjectLocationOnChildFileSystem(myObjectLocation, mySessionToken), myName, myIgnoreName, myRegExpr, myObjectStreams, myIgnoreObjectStreamTypes, mySize, myCreationTime, myLastModificationTime, myLastAccessTime, myDeletionTime, mySessionToken);


            #region _IDirectoryObject.GetExtendedDirectoryListing(...)

            var _IDirectoryObject = (IDirectoryObject) GetObject<DirectoryObject>(myObjectLocation, FSConstants.DIRECTORYSTREAM, null, null, 0, false, mySessionToken).Value;
            var _DirectoryListing = _IDirectoryObject.GetExtendedDirectoryListing(myName, myIgnoreName, myRegExpr, myObjectStreams, myIgnoreObjectStreamTypes);
            var _Output = new List<DirectoryEntryInformation>(); ;

            INode _INode = null;
            Boolean _AddEntry;
            HashSet<String> _ObjectStreamTypes;
            DirectoryEntryInformation __ExtendedDirectoryListing;

            #endregion

            #region Additional Filters

            foreach (var _ExtendedDirectoryListing in _DirectoryListing)
            {

                _AddEntry = false;

                _ObjectStreamTypes = _ExtendedDirectoryListing.Streams;

                if (
                    (_ObjectStreamTypes.Contains(FSConstants.INLINEDATA)) ||
                    (_ObjectStreamTypes.Contains(FSConstants.SYMLINK)) ||
                    (_ObjectStreamTypes.Contains(FSConstants.VIRTUALDIRECTORY)) ||
                    (_ExtendedDirectoryListing.Name.Equals(FSConstants.DotLink)) ||
                    (_ExtendedDirectoryListing.Name.Equals(FSConstants.DotDotLink))
                   )
                {
                    __ExtendedDirectoryListing = _ExtendedDirectoryListing;
                    __ExtendedDirectoryListing.Size = 0;
                    __ExtendedDirectoryListing.Timestamp = 0;
                    _Output.Add(__ExtendedDirectoryListing);
                }

                else
                {

                    #region Load the INode via this methods in order to make use of the object cache

                    if (myObjectLocation.Equals(FSPathConstants.PathDelimiter))
                        _INode = GetINode(new ObjectLocation(_ExtendedDirectoryListing.Name), mySessionToken).Value;

                    else
                        _INode = GetINode(new ObjectLocation(myObjectLocation, _ExtendedDirectoryListing.Name), mySessionToken).Value;

                    #endregion


                    if (mySize != null ||
                        myCreationTime != null ||
                        myLastModificationTime != null ||
                        myLastAccessTime != null ||
                        myDeletionTime != null)
                    {

                        #region Parameter --size=[<|=|>]NumberOfBytes

                        if (mySize != null)
                            try
                            {
                                foreach (String _Size in mySize)
                                    switch (_Size[0])
                                    {
                                        case '<': if (_INode.ObjectSize < UInt64.Parse(_Size.Substring(1))) _AddEntry = true; break;
                                        case '=': if (_INode.ObjectSize == UInt64.Parse(_Size.Substring(1))) _AddEntry = true; break;
                                        case '>': if (_INode.ObjectSize > UInt64.Parse(_Size.Substring(1))) _AddEntry = true; break;
                                        default: throw new PandoraFSException_ParsedSizeIsInvalid("The parameter 'size' could not be parsed!");
                                    }

                            }
                            catch (Exception)
                            {
                                throw new PandoraFSException_ParsedSizeIsInvalid("The parameter 'size' could not be parsed!");
                            }

                        #endregion

                        #region Parameter --CreationTime=[<|=|>]Timestamp

                        if (myCreationTime != null)
                            try
                            {
                                foreach (var _CreationTime in myCreationTime)
                                    switch (_CreationTime[0])
                                    {
                                        case '<': if (_INode.CreationTime < UInt64.Parse(_CreationTime.Substring(1))) _AddEntry = true; break;
                                        case '=': if (_INode.CreationTime == UInt64.Parse(_CreationTime.Substring(1))) _AddEntry = true; break;
                                        case '>': if (_INode.CreationTime > UInt64.Parse(_CreationTime.Substring(1))) _AddEntry = true; break;
                                        default: throw new PandoraFSException_ParsedCreationTimeIsInvalid("The parameter 'CreationTime' could not be parsed!");
                                    }

                            }
                            catch (Exception)
                            {
                                throw new PandoraFSException_ParsedCreationTimeIsInvalid("The parameter 'CreationTime' could not be parsed!");
                            }

                        #endregion

                        #region Parameter --LastAccessTime=[<|=|>]Timestamp

                        if (myLastAccessTime != null)
                            try
                            {
                                foreach (var _LastAccessTime in myLastAccessTime)
                                    switch (_LastAccessTime[0])
                                    {
                                        case '<': if (_INode.LastAccessTime < UInt64.Parse(_LastAccessTime.Substring(1))) _AddEntry = true; break;
                                        case '=': if (_INode.LastAccessTime == UInt64.Parse(_LastAccessTime.Substring(1))) _AddEntry = true; break;
                                        case '>': if (_INode.LastAccessTime > UInt64.Parse(_LastAccessTime.Substring(1))) _AddEntry = true; break;
                                        default: throw new PandoraFSException_ParsedLastAccessTimeIsInvalid("The parameter 'LastAccessTime' could not be parsed!");
                                    }

                            }
                            catch (Exception)
                            {
                                throw new PandoraFSException_ParsedLastAccessTimeIsInvalid("The parameter 'LastAccessTime' could not be parsed!");
                            }

                        #endregion

                        #region Parameter --LastModificationTime=[<|=|>]Timestamp

                        if (myLastModificationTime != null)
                            try
                            {
                                foreach (var _LastModificationTime in myLastModificationTime)
                                    switch (_LastModificationTime[0])
                                    {
                                        case '<': if (_INode.CreationTime < UInt64.Parse(_LastModificationTime.Substring(1))) _AddEntry = true; break;
                                        case '=': if (_INode.CreationTime == UInt64.Parse(_LastModificationTime.Substring(1))) _AddEntry = true; break;
                                        case '>': if (_INode.CreationTime > UInt64.Parse(_LastModificationTime.Substring(1))) _AddEntry = true; break;
                                        default: throw new PandoraFSException_ParsedLastModificationTimeIsInvalid("The parameter 'LastModificationTime' could not be parsed!");
                                    }

                            }
                            catch (Exception)
                            {
                                throw new PandoraFSException_ParsedLastModificationTimeIsInvalid("The parameter 'LastModificationTime' could not be parsed!");
                            }

                        #endregion

                        #region Parameter --DeletionTime=[<|=|>]Timestamp

                        if (myDeletionTime != null)
                            try
                            {
                                foreach (var _DeletionTime in myDeletionTime)
                                    switch (_DeletionTime[0])
                                    {
                                        case '<': if (_INode.DeletionTime < UInt64.Parse(_DeletionTime.Substring(1))) _AddEntry = true; break;
                                        case '=': if (_INode.DeletionTime == UInt64.Parse(_DeletionTime.Substring(1))) _AddEntry = true; break;
                                        case '>': if (_INode.DeletionTime > UInt64.Parse(_DeletionTime.Substring(1))) _AddEntry = true; break;
                                        default: throw new PandoraFSException_ParsedDeletionTimeIsInvalid("The parameter 'DeletionTime' could not be parsed!");
                                    }

                            }
                            catch (Exception)
                            {
                                throw new PandoraFSException_ParsedDeletionTimeIsInvalid("The parameter 'DeletionTime' could not be parsed!");
                            }

                        #endregion


                        if (_AddEntry)
                        {
                            __ExtendedDirectoryListing = _ExtendedDirectoryListing;
                            __ExtendedDirectoryListing.Size = _INode.ObjectSize;
                            __ExtendedDirectoryListing.Timestamp = _INode.LastModificationTime;
                            _Output.Add(__ExtendedDirectoryListing);
                        }

                    }

                    else
                    {
                        __ExtendedDirectoryListing = _ExtendedDirectoryListing;
                        __ExtendedDirectoryListing.Size = _INode.ObjectSize;
                        __ExtendedDirectoryListing.Timestamp = _INode.LastModificationTime;
                        _Output.Add(__ExtendedDirectoryListing);
                    }

                }

            }

            #endregion

            var _Exceptional = new Exceptional<IEnumerable<DirectoryEntryInformation>>();
            _Exceptional.Value = _Output;
            return _Exceptional;

        }

        #endregion


        #region RemoveDirectoryObject(myObjectLocation, myRemoveRecursive, mySessionToken)

        public Exceptional RemoveDirectoryObject(ObjectLocation myObjectLocation, Boolean myRemoveRecursive, SessionToken mySessionToken)
        {

            #region Initial Checks

            if (myObjectLocation == null)
                throw new ArgumentNullException("Parameter myObjectLocation must not be null!");
            
            if (mySessionToken == null)
                throw new ArgumentNullException("Parameter mySessionToken must not be null!");

            var _Exceptional = new Exceptional();

            #endregion

            #region Get a listing of the DirectoryObject

            var _DirectoryListingExceptional = GetExtendedDirectoryListing(myObjectLocation, mySessionToken);

            if (_DirectoryListingExceptional == null || _DirectoryListingExceptional.Failed || _DirectoryListingExceptional.Value == null)
            {
                return _DirectoryListingExceptional.Push(new GraphFSError_CouldNotGetDirectoryListing(myObjectLocation));
            }

            #endregion

            #region If the directory is not empty...

            if (_DirectoryListingExceptional.Value.ULongCount() > NUMBER_OF_DEFAULT_DIRECTORYENTRIES)
            {

                #region Fail if myRemoveRecursive == false

                if (!myRemoveRecursive)
                {
                    return _Exceptional.Push(new GraphFSError_DirectoryIsNotEmpty(myObjectLocation));
                }

                #endregion

                #region Remove all sub elements

                foreach (var _DirectoryEntryInformation in _DirectoryListingExceptional.Value)
                {

                    #region check if standard dir

                    // Hack: This should be done dynamically. 
                    if (!((_DirectoryEntryInformation.Name == FSConstants.DotLink)     ||
                          (_DirectoryEntryInformation.Name == FSConstants.DotDotLink)  ||
                          (_DirectoryEntryInformation.Name == FSConstants.DotMetadata) ||
                          (_DirectoryEntryInformation.Name == FSConstants.DotSystem)   ||
                          (_DirectoryEntryInformation.Name == FSConstants.DotStreams)))
                    {

                        #region Get a copy of all ObjectStreams
                        
                        var _ListOfStreams = new List<String>();

                        foreach (var _Stream in _DirectoryEntryInformation.Streams)
                            _ListOfStreams.Add(_Stream);

                        #endregion

                        foreach (var _StreamType in _ListOfStreams)
                        {

                            if (_StreamType == FSConstants.DIRECTORYSTREAM)
                            {
                                var _RemoveSubobjectExceptional = RemoveDirectoryObject(new ObjectLocation(myObjectLocation, _DirectoryEntryInformation.Name), true, mySessionToken);

                                // if (

                            }

                            else if (_StreamType == FSConstants.INLINEDATA)
                            {
                                // do nothing!
                            }

                            else
                            {

                                #region Get list of ObjectEditions and ObjectRevisionID to remove

                                var _ListOfObjectEditionsAndRevisionIDs = new List<KeyValuePair<String, RevisionID>>();

                                var _SubobjectLocatorExceptional = GetObjectLocator_protected(new ObjectLocation(myObjectLocation, _DirectoryEntryInformation.Name), mySessionToken);

                                if (_SubobjectLocatorExceptional == null || _SubobjectLocatorExceptional.Failed || _SubobjectLocatorExceptional.Value == null)
                                {
                                    return _SubobjectLocatorExceptional.Push(new GraphFSError_CouldNotGetObjectLocator(new ObjectLocation(myObjectLocation, _DirectoryEntryInformation.Name)));
                                }

                                foreach (var _ObjectEditionKeyValuePair in _SubobjectLocatorExceptional.Value[_StreamType])
                                {
                                    foreach (var _ObjectRevisionKeyValuePair in _ObjectEditionKeyValuePair.Value)
                                    {
                                        _ListOfObjectEditionsAndRevisionIDs.Add(new KeyValuePair<String, RevisionID>(_ObjectEditionKeyValuePair.Key, _ObjectRevisionKeyValuePair.Key));
                                    }
                                }

                                #endregion

                                #region Finally remove them all...

                                foreach (var _ObjectEditionAndRevisionID in _ListOfObjectEditionsAndRevisionIDs)
                                {

                                    var _RemoveSubobjectExceptional = RemoveObject(new ObjectLocation(myObjectLocation, _DirectoryEntryInformation.Name), _StreamType, _ObjectEditionAndRevisionID.Key, _ObjectEditionAndRevisionID.Value, mySessionToken);

                                    if (_RemoveSubobjectExceptional == null || _RemoveSubobjectExceptional.Failed)
                                    {
                                        return _RemoveSubobjectExceptional;
                                    }

                                }

                                #endregion

                            }                            

                        }

                    }

                    #endregion

                }

                #endregion
            
            }

            #endregion

            #region Remove the DirectoryObject itself!

            // To ensure optimistic concurrency: First get the latest RevisionID
            var _ObjectLocatorExceptional = GetObjectLocator_protected(myObjectLocation, mySessionToken);

            if (_ObjectLocatorExceptional == null || _ObjectLocatorExceptional.Failed || _ObjectLocatorExceptional.Value == null)
            {
                return _ObjectLocatorExceptional.Push(new GraphFSError_CouldNotGetObjectLocator(myObjectLocation));
            }

            // Remove the $DefaultEditon and $LatestRevision
            var _RemoveObjectExceptional = RemoveObject(myObjectLocation, FSConstants.DIRECTORYSTREAM, _ObjectLocatorExceptional.Value[FSConstants.DIRECTORYSTREAM].DefaultEditionName, _ObjectLocatorExceptional.Value[FSConstants.DIRECTORYSTREAM].DefaultEdition.LatestRevisionID, mySessionToken);

            if (_RemoveObjectExceptional == null || _RemoveObjectExceptional.Failed)
            {
                return _RemoveObjectExceptional.Push(new GraphFSError_CouldNotRemoveDirectoryObject(myObjectLocation));
            }

            return _Exceptional;

            #endregion

        }

        #endregion

        #region EraseDirectoryObject(myObjectLocation, myEraseRecursive, mySessionToken)

        public Exceptional EraseDirectoryObject(ObjectLocation myObjectLocation, Boolean myEraseRecursive, SessionToken mySessionToken)
        {

            #region Erase recursive

            if (myEraseRecursive)
            {

                #region really erase recursive?

                if (GetDirectoryListing(myObjectLocation, mySessionToken).Value.LongCount().Equals((Int64)NUMBER_OF_DEFAULT_DIRECTORYENTRIES))
                {
                    return EraseObject(myObjectLocation, FSConstants.DIRECTORYSTREAM, null, null, mySessionToken);
                }

                #endregion

                else//yes, erase recursive!
                {

                    #region get directory information

                    var dirEntries = GetExtendedDirectoryListing(myObjectLocation, mySessionToken);

                    if (dirEntries == null)
                        return new Exceptional<Boolean>(new GraphFSError_DirectoryListingFailed(myObjectLocation));

                    #endregion

                    #region erase all sub elements

                    foreach (var aDirInformation in dirEntries.Value)
                    {

                        #region check if standard dir

                        //Hack: This should be done dynamically. 
                        if (!(aDirInformation.Name.Equals(FSConstants.DotLink) ||
                            aDirInformation.Name.Equals(FSConstants.DotDotLink) ||
                            aDirInformation.Name.Equals(FSConstants.DotMetadata) ||
                            aDirInformation.Name.Equals(FSConstants.DotUUID) ||
                            aDirInformation.Name.Equals(FSConstants.DotSystem) ||
                            aDirInformation.Name.Equals(FSConstants.DotStreams)))
                        {

                        #endregion

                            #region get deeper

                            if (aDirInformation.Streams.Contains(FSConstants.DIRECTORYSTREAM))
                            {

                                var _ChildIPandoraFS = GetChildFileSystem(new ObjectLocation(myObjectLocation, aDirInformation.Name), false, mySessionToken);

                                if (_ChildIPandoraFS != this)
                                {
                                    throw new PandoraFSException_DeviceBusy("Device at mointpoint \"" + myObjectLocation + FSPathConstants.PathDelimiter + aDirInformation.Name + "\" busy!");
                                }

                                //if (!EraseDirectoryObject(new ObjectLocation(myObjectLocation, aDirInformation.Name), true, mySessionToken))
                                //{
                                //    throw new PandoraFSException_DirectoryObjectIsNotEmpty("The directory \"" + myObjectLocation + FSPathConstants.PathDelimiter + aDirInformation.Name + "\" could not be erased.");
                                //}

                            }

                            #endregion

                            else
                            {
                                #region erase

                                foreach (var aStreamType in aDirInformation.Streams)
                                {
                                    if (!aStreamType.Equals(FSConstants.INLINEDATA))
                                    {
                                        EraseObject(new ObjectLocation(myObjectLocation, aDirInformation.Name), aStreamType, null, null, mySessionToken);
                                    }
                                }

                                #endregion
                            }
                        }
                    }

                    #endregion

                    #region erase the directory itself

                    List<String> streamTypes = new List<String>();

                    streamTypes.AddRange(GetObjectStreams(myObjectLocation, mySessionToken).Value);

                    foreach (String aStreamType in streamTypes)
                    {
                        EraseObject(myObjectLocation, aStreamType, null, null, mySessionToken);
                    }

                    #endregion
                }
            }
            #endregion

            // non-recursive
            else
            {
                if (GetDirectoryListing(myObjectLocation, mySessionToken).Value.LongCount() > (Int64)NUMBER_OF_DEFAULT_DIRECTORYENTRIES)
                    throw new PandoraFSException_DirectoryObjectIsNotEmpty("This DirectoryObject is not empty!");

                return EraseObject(myObjectLocation, FSConstants.DIRECTORYSTREAM, null, null, mySessionToken);

            }

            return new Exceptional<Boolean>();


        }

        #endregion

        #endregion

        #region MetadataObject Methods

        #region SetMetadatum<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myKey, myValue, myIndexSetStrategy, mySessionToken)

        public Exceptional SetMetadatum<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myKey, TValue myValue, IndexSetStrategy myIndexSetStrategy, SessionToken mySessionToken)
        {

            #region Get or create MetadataObject

            var _MetadataObjectExceptional = GetOrCreateObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            {
                return _MetadataObjectExceptional.Push(new GraphFSError_CouldNotSetMetadata(myObjectLocation, myObjectStream, myObjectEdition));
            }

            #endregion

            #region If new, store the MetadataObject explicitly

            if (_MetadataObjectExceptional.Value.isNew)
            {

                _MetadataObjectExceptional.Value.Set(myKey, myValue, myIndexSetStrategy);
//                _MetadataObjectExceptional.Value.Save();

                using (var _StoreObjectExceptional = StoreFSObject(myObjectLocation, _MetadataObjectExceptional.Value, true, mySessionToken))
                {
                    if (_StoreObjectExceptional == null || _StoreObjectExceptional.Failed)
                    {
                        return _MetadataObjectExceptional.Push(new GraphFSError_CouldNotStoreObject(myObjectLocation, myObjectStream, myObjectEdition));
                    }
                }

            }

            #endregion

            _MetadataObjectExceptional.Value.Set(myKey, myValue, myIndexSetStrategy);
            //_MetadataObjectExceptional.Value.Save();

            return StoreFSObject(myObjectLocation, _MetadataObjectExceptional.Value, true, mySessionToken).
                       WhenFailed(e => e.Push(new GraphFSError_CouldNotStoreObject(myObjectLocation, myObjectStream, myObjectEdition)));

            //using (var _StoreObjectExceptional = StoreFSObject(myObjectLocation, _MetadataObjectExceptional.Value, true, mySessionToken))
            //{
            //    if (_StoreObjectExceptional == null || _StoreObjectExceptional.Failed)
            //    {
            //        var _Exceptional = new Exceptional();
            //        _Exceptional.Add(new GraphFSError_CouldNotStoreObject(myObjectLocation, myObjectStream, myObjectEdition));
            //        _Exceptional.Add(_MetadataObjectExceptional.Errors);
            //        return _Exceptional;
            //    }
            //}

            //return new Exceptional();

        }

        #endregion

        #region SetMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myMetadata, myIndexSetStrategy, mySessionToken)

        public Exceptional SetMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, IEnumerable<KeyValuePair<String, TValue>> myMetadata, IndexSetStrategy myIndexSetStrategy, SessionToken mySessionToken)
        {


            var _MetadataObjectExceptional = GetOrCreateObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken).
                                                 WhenFailed(e => e.PushT(new GraphFSError_CouldNotSetMetadata(myObjectLocation, myObjectStream, myObjectEdition)));

            if (_MetadataObjectExceptional.Failed)
                return _MetadataObjectExceptional;



            //#region Get or create MetadataObject

            //var _MetadataObjectExceptional = GetOrCreateObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            //if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            //{
            //    var _Exceptional = new Exceptional();
            //    _Exceptional.Add(new GraphFSError_CouldNotSetMetadata(myObjectLocation, myObjectStream, myObjectEdition));
            //    _Exceptional.Add(_MetadataObjectExceptional.Errors);
            //    return _Exceptional;
            //}

            //#endregion

            #region If new, store the MetadataObject within the cache

            // FIXME!
            _MetadataObjectExceptional.Value.Set(myMetadata, myIndexSetStrategy);

            if (_MetadataObjectExceptional.Value.isNew)
            {

                return StoreFSObject(myObjectLocation, _MetadataObjectExceptional.Value, true, mySessionToken).
                           WhenFailed(e => e.Push(new GraphFSError_CouldNotStoreObject(myObjectLocation, myObjectStream, myObjectEdition)));


                //var _StoreObjectExceptional = StoreFSObject(myObjectLocation, _MetadataObjectExceptional.Value, true, mySessionToken);

                //if (_StoreObjectExceptional == null || _StoreObjectExceptional.Failed)
                //{
                //    var _Exceptional = new Exceptional();
                //    _Exceptional.Add(new GraphFSError_CouldNotStoreObject(myObjectLocation, myObjectStream, myObjectEdition));
                //    _Exceptional.Add(_MetadataObjectExceptional.Errors);
                //    return _Exceptional;
                //}

            }

            #endregion

            return new Exceptional();

        }

        #endregion


        #region MetadatumExists<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myKey, myValue, mySessionToken)

        public Exceptional<Trinary> MetadatumExists<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myKey, TValue myValue, SessionToken mySessionToken)
        {

            return GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc(v => v.Contains(myKey, myValue)).
                       WhenFailed(e => e = new Exceptional<Trinary>(Trinary.FALSE));


            //var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            //if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            //{
            //    _Exceptional.Value = Trinary.FALSE;
            //    _Exceptional.Add(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            //    _Exceptional.Add(_MetadataObjectExceptional.Errors);
            //}

            //else
            //    _Exceptional.Value = _MetadataObjectExceptional.Value.Contains(myKey, myValue);

            //return _Exceptional;

        }

        #endregion

        #region MetadataExists<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myKey, mySessionToken)

        public Exceptional<Trinary> MetadataExists<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myKey, SessionToken mySessionToken)
        {

            return GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc(v => v.ContainsKey(myKey)).
                       WhenFailed(e => e = new Exceptional<Trinary>(Trinary.FALSE));


            //var _Exceptional = new Exceptional<Trinary>();
            //var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            //if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            //{
            //    _Exceptional.Value = Trinary.FALSE;
            //    _Exceptional.Add(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            //    _Exceptional.Add(_MetadataObjectExceptional.Errors);
            //}

            //else
            //    _Exceptional.Value = _MetadataObjectExceptional.Value.ContainsKey(myKey);

            //return _Exceptional;

        }

        #endregion


        #region GetMetadatum<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myKey, mySessionToken)

        public Exceptional<IEnumerable<TValue>> GetMetadatum<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myKey, SessionToken mySessionToken)
        {

            return GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken).
                       ConvertWithFunc(v =>
                       {
                           var _ListOfValues = new List<TValue>();

                           foreach (var _item in v[myKey])
                           {
                               try
                               {
                                   _ListOfValues.Add((TValue)_item);
                               }
                               catch (Exception)
                               {
                               }
                           }

                           return (IEnumerable<TValue>) _ListOfValues;
                       }).
                       WhenFailed(e => e.PushT(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition)));



            //var _Exceptional = new Exceptional<IEnumerable<TValue>>();
            //var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            //if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            //{
            //    _Exceptional.Push(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            //}

            //else
            //{

            //    var _ListOfValues = new List<TValue>();

            //    foreach (var _item in _MetadataObjectExceptional.Value[myKey])
            //    {
            //        try
            //        {
            //            _ListOfValues.Add( (TValue) _item);
            //        }
            //        catch (Exception e)
            //        {
            //        }
            //    }

            //    _Exceptional.Value = _ListOfValues;

            //}

            //return _Exceptional;

        }

        #endregion

        #region GetMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, mySessionToken)

        public Exceptional<IEnumerable<KeyValuePair<String, TValue>>> GetMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, SessionToken mySessionToken)
        {
            
            var _Exceptional = new Exceptional<IEnumerable<KeyValuePair<String, TValue>>>();
            var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            {
                _Exceptional = _MetadataObjectExceptional.Convert<IEnumerable<KeyValuePair<String,TValue>>>().
                                    PushT(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            }

            else
            {

                var _ListOfKeyValuePairs = new List<KeyValuePair<String, TValue>>();

                foreach (var _KeyValuesPair in _MetadataObjectExceptional.Value)
                {
                    foreach (var _KeyValuePair in _KeyValuesPair.Value)
                    {
                        try
                        {
                            _ListOfKeyValuePairs.Add(new KeyValuePair<String, TValue>(_KeyValuesPair.Key, (TValue)_KeyValuePair));
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }

                _Exceptional.Value = _ListOfKeyValuePairs;

            }

            return _Exceptional;

        }

        #endregion

        #region GetMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myMinKey, myMaxKey, mySessionToken)

        public Exceptional<IEnumerable<KeyValuePair<String, TValue>>> GetMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myMinKey, String myMaxKey, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional<IEnumerable<KeyValuePair<String, TValue>>>();
            var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            {
                _Exceptional = _MetadataObjectExceptional.Convert<IEnumerable<KeyValuePair<String, TValue>>>().
                    PushT(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            }

            else
            {

                var _ListOfKeyValuePairs = new List<KeyValuePair<String, TValue>>();

                foreach (var _KeyValuesPair in _MetadataObjectExceptional.Value)
                {
                    foreach (var _KeyValuePair in _KeyValuesPair.Value)
                    {
                        try
                        {
                            if (_KeyValuesPair.Key.CompareTo(myMinKey) >= 0 && _KeyValuesPair.Key.CompareTo(myMaxKey) <= 0)
                                _ListOfKeyValuePairs.Add(new KeyValuePair<String, TValue>(_KeyValuesPair.Key, (TValue) _KeyValuePair));
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }

                _Exceptional.Value = _ListOfKeyValuePairs;

            }

            return _Exceptional;

        }

        #endregion

        #region GetMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myFunc, mySessionToken)

        public Exceptional<IEnumerable<KeyValuePair<String, TValue>>> GetMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, Func<KeyValuePair<String, TValue>, Boolean> myFunc, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region RemoveMetadatum<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myKey, myValue, mySessionToken)

        public Exceptional RemoveMetadatum<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myKey, TValue myValue, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional();
            var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            {
                _Exceptional = _MetadataObjectExceptional.Convert<IEnumerable<KeyValuePair<String, TValue>>>().
                    PushT(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            }

            else
            {

                try
                {
                    _MetadataObjectExceptional.Value.Remove(myKey, myValue);
                }
                catch (Exception e)
                {
                }

            }

            return _Exceptional;

        }

        #endregion

        #region RemoveMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myKey, mySessionToken)

        public Exceptional RemoveMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, String myKey, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional();
            var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            {
                _Exceptional = _MetadataObjectExceptional.Convert<IEnumerable<KeyValuePair<String, TValue>>>().
                    PushT(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            }

            else
            {

                try
                {
                    _MetadataObjectExceptional.Value.Remove(myKey);
                }
                catch (Exception e)
                {
                }

                var _StoreObjectExceptional = StoreFSObject(myObjectLocation, _MetadataObjectExceptional.Value, true, mySessionToken);

                if (_StoreObjectExceptional == null || _StoreObjectExceptional.Failed)
                {
                    return _MetadataObjectExceptional.Convert<IEnumerable<KeyValuePair<String, TValue>>>().
                        PushT(new GraphFSError_CouldNotStoreObject(myObjectLocation, myObjectStream, myObjectEdition));
                }

            }

            return _Exceptional;

        }

        #endregion

        #region RemoveMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myMetadata, mySessionToken)

        public Exceptional RemoveMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, IEnumerable<KeyValuePair<String, TValue>> myMetadata, SessionToken mySessionToken)
        {

            var _Exceptional = new Exceptional<IEnumerable<KeyValuePair<String, TValue>>>();
            var _MetadataObjectExceptional = GetObject<MetadataObject<TValue>>(myObjectLocation, myObjectStream, myObjectEdition, null, 0, false, mySessionToken);

            if (_MetadataObjectExceptional == null || _MetadataObjectExceptional.Failed || _MetadataObjectExceptional.Value == null)
            {
                _Exceptional = _MetadataObjectExceptional.Convert<IEnumerable<KeyValuePair<String, TValue>>>().
                    PushT(new GraphFSError_MetadataObjectNotFound(myObjectLocation, myObjectStream, myObjectEdition));
            }

            else
            {

                foreach (var _KeyValuesPair in myMetadata)
                {   
                 
                    try
                    {
                        _MetadataObjectExceptional.Value.Remove(_KeyValuesPair.Key, _KeyValuesPair.Value);
                    }
                    catch (Exception e)
                    {
                    }
                    
                }

            }

            return _Exceptional;

        }

        #endregion

        #region RemoveMetadata<TValue>(myObjectLocation, myObjectStream, myObjectEdition, myFunc, mySessionToken)

        public Exceptional RemoveMetadata<TValue>(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, Func<KeyValuePair<String, TValue>, Boolean> myFunc, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion




        #region Stream methods

        #region OpenStream(mySessionToken, myObjectLocation, myObjectStream, myObjectEdition, myObjectRevision, myObjectCopy)

        public virtual Exceptional<IGraphFSStream> OpenStream(SessionToken mySessionToken, ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevision, UInt64 myObjectCopy)
        {
            throw new NotImplementedException("This method can not be used on this GraphFS implementation!");
        }

        #endregion

        #region OpenStream(mySessionToken, myObjectLocation, myObjectStream, myObjectEdition, myObjectRevision, myObjectCopy, myFileMode, myFileAccess, myFileShare, myFileOptions, myBufferSize)

        public virtual Exceptional<IGraphFSStream> OpenStream(SessionToken mySessionToken, ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, RevisionID myObjectRevision, UInt64 myObjectCopy,
                                           FileMode myFileMode, FileAccess myFileAccess, FileShare myFileShare, FileOptions myFileOptions, UInt64 myBufferSize)
        {
            throw new NotImplementedException("This method can not be used on this GraphFS implementation!");
        }

        #endregion

        #endregion


        #region StorageEngine Maintenance

        #region StorageLocations(mySessionToken)

        public IEnumerable<String> StorageLocations(SessionToken mySessionToken)
        {
            return new List<String>();
        }

        #endregion

        #region StorageLocations(myObjectLocation, mySessionToken)

        public IEnumerable<String> StorageLocations(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            return new List<String>();
        }

        #endregion


        #region StorageUUIDs(mySessionToken)

        public IEnumerable<StorageUUID> StorageUUIDs(SessionToken mySessionToken)
        {
            return new List<StorageUUID>();
        }

        #endregion

        #region StorageUUIDs(myObjectLocation, mySessionToken)

        public IEnumerable<StorageUUID> StorageUUIDs(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            return new List<StorageUUID>();
        }

        #endregion


        #region StorageDescriptions(mySessionToken)

        public IEnumerable<String> StorageDescriptions(SessionToken mySessionToken)
        {
            return new List<String>();
        }

        #endregion

        #region StorageDescriptions(myObjectLocation, mySessionToken)

        public IEnumerable<String> StorageDescriptions(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            return new List<String>();
        }

        #endregion

        #endregion


        #region Transactions

        public FSTransaction BeginTransaction(SessionToken mySessionToken, Boolean myDistributed = false, Boolean myLongRunning = false, IsolationLevel myIsolationLevel = IsolationLevel.Serializable, String myName = "", DateTime? timestamp = null)
        {
            var _FSTransaction = new FSTransaction(myDistributed, myLongRunning, myIsolationLevel, myName);
            _FSTransaction.OnDispose += new TransactionDisposedHandler(TransactionOnDisposeHandler);
            return _FSTransaction;
        }

        // --------------------

        private void TransactionOnDisposeHandler(object sender, TransactionDisposedEventArgs args)
        {

            var _FSTransaction = args.Transaction;
            _FSTransaction.Rollback();//args.SessionToken);

        }

        //private void rollbackTransaction(FSTransaction myTransaction, SessionToken mySessionToken)
        //{

        //    //foreach (var keyValPair in myTransaction.TransactionDetails)
        //    //{

        //    //    var currentLocator = _ObjectCache.GetCachedObjectLocator(keyValPair.Key);

        //    //    #region If the ObjectLocator and TransactionUUID is not null, deallocate the ObjectLocator

        //    //    if (keyValPair.Value.ObjectLocator != null)
        //    //    {
        //    //        keyValPair.Value.ObjectLocator.TransactionUUID = null;
        //    //        _AllocationMap.Deallocate(ref keyValPair.Value.ObjectLocator.PreallocationTickets);

        //    //        if (ObjectExists(new ObjectLocation(keyValPair.Key), mySessionToken) == Trinary.TRUE)
        //    //        {
        //    //            if (currentLocator != null)
        //    //            {
        //    //                keyValPair.Value.ObjectLocator.CopyTo(currentLocator);
        //    //                keyValPair.Value.INode.CopyTo(currentLocator.INodeReference);
        //    //            }
        //    //            else
        //    //            {
        //    //                currentLocator = keyValPair.Value.ObjectLocator;
        //    //                _ObjectCache.StoreObjectLocator(keyValPair.Key, currentLocator);
        //    //            }
        //    //        }

        //    //    }
        //    //    if (keyValPair.Value.INode != null)
        //    //    {
        //    //        keyValPair.Value.INode.TransactionUUID = null;
        //    //        _AllocationMap.Deallocate(ref keyValPair.Value.INode.PreallocationTickets);
        //    //    }

        //    //    #endregion

        //    //    if (currentLocator == null) continue;

        //    //    #region Go through all streams and editions to deallocate the TransactionRevision

        //    //    foreach (KeyValuePair<String, ObjectStream> editionKeyValPair in currentLocator)
        //    //    {
        //    //        ObjectStream edition = editionKeyValPair.Value;
        //    //        foreach (ObjectEdition revision in edition.Values)
        //    //        {
        //    //            if (revision.HoldTransaction)
        //    //            {

        //    //                #region Deallocate the TransactionRevision

        //    //                APandoraObject pandoraObject = ((StreamCacheEntry)_ObjectCache.RemoveTempEntry(revision.TransactionRevision.CacheUUID)).CachedAPandoraObject;
        //    //                _AllocationMap.Deallocate(ref pandoraObject.PreallocationTickets);

        //    //                #endregion

        //    //                pandoraObject.TransactionUUID = null;

        //    //                #region Rollback the TransactionRevision

        //    //                revision.RollbackTransaction();

        //    //                #endregion

        //    //            }
        //    //        }

        //    //    }

        //    //    #endregion

        //    //    keyValPair.Value.ObjectLocator = null;
        //    //    keyValPair.Value.INode = null;

        //    //}

        //}

        ///// <summary>
        ///// Rolls back the current transaction
        ///// </summary>
        ///// <param name="mySessionToken"></param>
        //public void RollbackTransaction(SessionToken mySessionToken)
        //{

        //    //#region Get the current Transaction

        //    //FSTransaction lastFSTransaction;
        //    //lock (mySessionToken)
        //    //{
        //    //    lastFSTransaction = (FSTransaction)mySessionToken.RemoveTransaction();
        //    //}

        //    //#endregion

        //    //rollbackTransaction(lastFSTransaction, mySessionToken);

        //    //lastFSTransaction.Rollback();

        //}

        // /// <summary>
        ///// Commit the current transaction
        ///// </summary>
        ///// <param name="mySessionToken"></param>
        //public void CommitTransaction(SessionToken mySessionToken)
        //{

        //    //#region Get the current Transaction

        //    //FSTransaction lastFSTransaction;

        //    //lock (mySessionToken)
        //    //{
        //    //    lastFSTransaction = (FSTransaction) mySessionToken.RemoveTransaction();
        //    //}

        //    //if (lastFSTransaction == null)
        //    //    return;

        //    //#endregion

        //    //#region Go through all transactionDetails (locations)

        //    //foreach (var _TransactionDetail in lastFSTransaction.TransactionDetails)
        //    //{

        //    //    // put the ObjectLocator stored in Transaction into the cache
        //    //    var _CurrentObjectLocator = _ObjectCache.GetCachedObjectLocator(_TransactionDetail.Key);

        //    //    #region If the ObjectLocator and TransactionUUID is not null, the ObjectLocator changed in this transaction

        //    //    if (_TransactionDetail.Value.ObjectLocator != null && _TransactionDetail.Value.ObjectLocator.TransactionUUID != null)
        //    //    {

        //    //        _TransactionDetail.Value.INode.TransactionUUID = null;
        //    //        _TransactionDetail.Value.ObjectLocator.TransactionUUID = null;

        //    //        if (_CurrentObjectLocator == null)
        //    //        {
                        
        //    //            #region The ObjectLocator is new
                        
        //    //            _ObjectCache.StoreObjectLocator(_TransactionDetail.Key, _TransactionDetail.Value.ObjectLocator, DirectoryHelper.GetObjectPath(_TransactionDetail.Value.ObjectLocator.ObjectLocation));
        //    //            _CurrentObjectLocator = _TransactionDetail.Value.ObjectLocator;

        //    //            #endregion

        //    //        }
        //    //        else
        //    //        {

        //    //            #region The ObjectLocator already exist and need to be updated

        //    //            #region Deallocate currentLocator before overwrite it from Transactional locator

        //    //            _AllocationMap.Deallocate(ref _CurrentObjectLocator.PreallocationTickets);
        //    //            _AllocationMap.Deallocate(ref _CurrentObjectLocator.INodeReference.PreallocationTickets);

        //    //            #endregion

        //    //            _TransactionDetail.Value.ObjectLocator.CopyTo(_CurrentObjectLocator);
        //    //            _TransactionDetail.Value.INode.CopyTo(_CurrentObjectLocator.INodeReference);

        //    //            if (_TransactionDetail.Value.WasRemoved)
        //    //            {
        //    //                _ObjectCache.RemoveObjectLocator(_CurrentObjectLocator.ObjectLocation, true);
        //    //            }

        //    //            #endregion

        //    //        }
        //    //        //Debug.WriteLine("\t\t StoreObjectLocator[" + keyValPair.Key + "] " + currentLocator.ToString());

        //    //    }

        //    //    #endregion

        //    //    if (_CurrentObjectLocator.INodeReference.ObjectLocatorStates == ObjectLocatorStates.Erased)
        //    //    {

        //    //        foreach (var _ObjectEditions in _CurrentObjectLocator.Values)
        //    //        {
        //    //            foreach (var _ObjectRevision in _ObjectEditions.Values)
        //    //            {
        //    //                if (_ObjectRevision.HoldTransaction)
        //    //                {

        //    //                    APandoraObject _APandoraObject = _ObjectCache.GetAPandoraObject(_ObjectRevision.TransactionRevision.CacheUUID);
        //    //                    _AllocationMap.Deallocate(ref _APandoraObject.PreallocationTickets);
        //    //                    _ObjectCache.RemoveTempEntry(_ObjectRevision.TransactionRevision.CacheUUID);
        //    //                    _APandoraObject = null;

        //    //                }
        //    //            }
        //    //        }

        //    //        _AllocationMap.Deallocate(ref _CurrentObjectLocator.PreallocationTickets);
        //    //        _CurrentObjectLocator = null;
        //    //        lastFSTransaction.Commit();
                 
        //    //        continue;

        //    //    }

        //    //    #region Go through all streams and editions to move the transactional ObjectStreams (PandoraObjects) from temp to the cache

        //    //    foreach (KeyValuePair<String, ObjectStream> editionKeyValPair in _CurrentObjectLocator)
        //    //    {
        //    //        ObjectStream edition = editionKeyValPair.Value;
        //    //        foreach (ObjectEdition revision in edition.Values)
        //    //        {
        //    //            if (revision.HoldTransaction)
        //    //            {

        //    //                #region Commit the TransactionRevision and move the Object from Cache.temp to cache

        //    //                //Debug.WriteLine("\t\t commit cacheuuid " + revision.TransactionRevision.CacheUUID);
        //    //                revision.CommitTransaction();
        //    //                //Debug.WriteLine("\t\t copy to cache " + revision.LatestRevision.CacheUUID);
        //    //                APandoraObject pandoraObject = ((StreamCacheEntry)_ObjectCache.MoveTempEntryToCache(revision.LatestRevision.CacheUUID)).CachedAPandoraObject;
        //    //                pandoraObject.TransactionUUID = null;
                            
        //    //                #endregion

        //    //            }
        //    //        }

        //    //        #region Delete old revision greater than MaxNumberOfRevisions

        //    //        while (edition.DefaultEdition.MaxNumberOfRevisions < edition.DefaultEdition.GetMaxPathLength(edition.DefaultEdition.LatestRevisionID))
        //    //        {
        //    //            DeleteOldestObjectRevision(_CurrentObjectLocator, editionKeyValPair.Key, edition.DefaultEditionName, mySessionToken);
        //    //        }

        //    //        #endregion
        //    //    }

        //    //    #endregion

        //    //    #region Update the global _AllocationMap (Locator, INode) if the Objectlocator contains the AllocationMap

        //    //    //TODO: change this if there are more than one AllocationMap

        //    //    if (_CurrentObjectLocator.ContainsKey(FSConstants.ALLOCATIONMAPSTREAM)
        //    //        && _TransactionDetail.Value.ObjectLocator != null && _TransactionDetail.Value.INode != null)
        //    //    {
        //    //        //if (_AllocationMapLocator != null)
        //    //        //{
        //    //        //    currentLocator.CopyTo(_AllocationMapLocator);
        //    //        //}
        //    //        //else
        //    //        //{
        //    //        _AllocationMapLocator = _CurrentObjectLocator;
        //    //            _AllocationMap.ObjectLocatorReference = _AllocationMapLocator;
        //    //        //}

        //    //        //if (_AllocationMapINode != null)
        //    //        //{
        //    //        //    keyValPair.Value.INode.CopyTo(_AllocationMapINode);
        //    //        //}
        //    //        //else
        //    //        //{
        //    //        _AllocationMapINode = _TransactionDetail.Value.INode;
        //    //            _AllocationMap.INodeReference = _AllocationMapINode;
        //    //        //}
        //    //    }

        //    //    #endregion

        //    //}

        //    //#endregion

        //    //lastFSTransaction.Commit();

        //}

        #endregion


        public void FlushObjectLocationNew(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }


        


        #region ResolveObjectLocation(myObjectLocation, ..., SessionToken mySessionToken)

        public abstract ResolveTypes ResolveObjectLocation_Internal(ref ObjectLocation myObjectLocation, out IEnumerable<String> myObjectStreams, out ObjectLocation myObjectPath, out String myObjectName, out IDirectoryObject myIDirectoryObject, ref List<String> mySymlinkTargets, SessionToken mySessionToken);
        public abstract ResolveTypes ResolveObjectLocationRecursive_Internal(ref ObjectLocation myObjectLocation, out IEnumerable<String> myObjectStreams, out ObjectLocation myObjectPath, out String myObjectName, out IDirectoryObject myIDirectoryObject, out IGraphFS myIPandoraFS, ref List<String> mySymlinkTargets, SessionToken mySessionToken);
        public abstract Trinary ResolveObjectLocation(ref ObjectLocation myObjectLocation, out IEnumerable<String> myObjectStreams, out ObjectLocation myObjectPath, out String myObjectName, out IDirectoryObject myIDirectoryObject, out IGraphFS myIPandoraFS, SessionToken mySessionToken);
        public abstract ObjectLocation ResolveObjectLocation(ObjectLocation myObjectLocation, Boolean myThrowObjectNotFoundException, SessionToken mySessionToken);

        #endregion


        #region NotificationDispatcher

        // The NotificationDispatcher handles all kind of notification between system parts or other dispatchers.
        // Use register to get notified as recipient.
        // Use SendNotification to send a notification to all subscribed recipients.

        #region GetNotificationDispatcher(SessionToken mySessionToken)

        /// <summary>
        /// Returns the NotificationDispatcher of this file system.
        /// </summary>
        /// <returns>The NotificationDispatcher of this file system</returns>
        public NotificationDispatcher GetNotificationDispatcher(SessionToken mySessionToken)
        {
            return _NotificationDispatcher;
        }

        #endregion

        #region GetNotificationDispatcher(myObjectLocation, mySessionToken)

        /// <summary>
        /// Returns the NotificationDispatcher of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <returns>The NotificationDispatcher of the file system at the given ObjectLocation</returns>
        public NotificationDispatcher GetNotificationDispatcher(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {

            var _PathLength = Int32.MinValue;
            var _NotificationDispatcher = GetNotificationDispatcher(mySessionToken);

            foreach (var __Mountpoint_IPandoraFS in _GraphFSLookuptable.MountedFSs)
            {

                if (myObjectLocation.StartsWith(__Mountpoint_IPandoraFS.Key) &&
                    (_PathLength < __Mountpoint_IPandoraFS.Key.Length))
                {
                    _PathLength = __Mountpoint_IPandoraFS.Key.Length;
                    _NotificationDispatcher = __Mountpoint_IPandoraFS.Value.GetNotificationDispatcher(mySessionToken);
                }

            }

            return _NotificationDispatcher;
        }

        #endregion


        #region SetNotificationDispatcher(myNotificationDispatcher, mySessionToken)

        /// <summary>
        /// Sets the NotificationDispatcher of this file system.
        /// </summary>
        /// <param name="myNotificationDispatcher">A NotificationDispatcher object</param>
        public void SetNotificationDispatcher(NotificationDispatcher myNotificationDispatcher, SessionToken mySessionToken)
        {
            _NotificationDispatcher = myNotificationDispatcher;
        }

        #endregion

        #region SetNotificationDispatcher(myObjectLocation, myNotificationDispatcher, SessionToken mySessionToken)

        /// <summary>
        /// Sets the NotificationDispatcher of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <param name="myNotificationDispatcher">A NotificationDispatcher object</param>
        public void SetNotificationDispatcher(ObjectLocation myObjectLocation, NotificationDispatcher myNotificationDispatcher, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region GetNotificationSettings(SessionToken mySessionToken)

        /// <summary>
        /// Returns the NotificationDispatcher settings of this file system
        /// </summary>
        /// <returns>The NotificationDispatcher settings of this file system</returns>
        public NotificationSettings GetNotificationSettings(SessionToken mySessionToken)
        {
            return _NotificationSettings;
        }

        #endregion

        #region GetNotificationSettings(myObjectLocation, SessionToken mySessionToken)

        /// <summary>
        /// Returns the NotificationDispatcher settings of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <returns>The NotificationDispatcher settings of the file system at the given ObjectLocation</returns>
        public NotificationSettings GetNotificationSettings(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region SetNotificationSettings(myNotificationSettings, SessionToken mySessionToken)

        /// <summary>
        /// Sets the NotificationDispatcher settings of this file system
        /// </summary>
        /// <param name="myNotificationSettings">A NotificationSettings object</param>
        public void SetNotificationSettings(NotificationSettings myNotificationSettings, SessionToken mySessionToken)
        {
            _NotificationSettings = myNotificationSettings;
        }

        #endregion

        #region SetNotificationSettings(myObjectLocation, myNotificationSettings, SessionToken mySessionToken)

        /// <summary>
        /// Sets the NotificationDispatcher settings of the file system at the given ObjectLocation
        /// </summary>
        /// <param name="myObjectLocation">the ObjectLocation or path of interest</param>
        /// <param name="myNotificationSettings">A NotificationSettings object</param>
        public void SetNotificationSettings(ObjectLocation myObjectLocation, NotificationSettings myNotificationSettings, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion



        #region IGraphFS Members


        bool IGraphFS.SetFileSystemDescription(string myFileSystemDescription, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        bool IGraphFS.SetFileSystemDescription(ObjectLocation myObjectLocation, string myFileSystemDescription, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        Exceptional IGraphFS.LockObject(ObjectLocation myObjectLocation, string myObjectStream, string myObjectEdition, RevisionID myObjectRevisionID, ObjectLocks myObjectLock, ObjectLockTypes myObjectLockType, ulong myLockingTime, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }












        public ObjectCacheSettings GetObjectCacheSettings(SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        public ObjectCacheSettings GetObjectCacheSettings(ObjectLocation myObjectLocation, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        public void SetObjectCacheSettings(ObjectCacheSettings myObjectCacheSettings, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        public void SetObjectCacheSettings(ObjectLocation myObjectLocation, ObjectCacheSettings myObjectCacheSettings, SessionToken mySessionToken)
        {
            throw new NotImplementedException();
        }

        #endregion

    
    }

}
