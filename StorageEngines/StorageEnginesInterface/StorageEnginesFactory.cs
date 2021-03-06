﻿/*
 * StorageEngineFactory
 * (c) Achim Friedland, 2008 - 2010
 */

#region Usings

using System;
using System.Linq;

using sones.Lib;
using sones.Lib.Singleton;
using sones.Lib.Reflection;
using sones.Lib.ErrorHandling;

#endregion

namespace sones.StorageEngines
{

    /// <summary>
    /// A factory which uses reflection to generate a apropriate IStorageEngine for you.
    /// As this implements the singleton pattern, use StorageEngineFactory.Instance.ActivateIStorageEngine(...)
    /// </summary>
    public class StorageEngineFactory : Singleton<StorageEngineFactory.StorageEngineFactory_internal>
    {

        /// <summary>
        /// An internal helper class for the GraphFSFactory class
        /// </summary>
        public class StorageEngineFactory_internal : AutoDiscovery<IStorageEngine>
        {

            #region Constructor

            #region StorageEngineFactory_internal()

            /// <summary>
            /// This constructor will autodiscover all implementations of IStorageEngine
            /// </summary>
            public StorageEngineFactory_internal()
            {

                FindAndRegisterImplementations(false, new String[] { "." }, t =>
                {
                    try
                    {
                        var _IStorageEngine = (IStorageEngine) Activator.CreateInstance(t);
                        return _IStorageEngine.URIPrefix;
                    }
                    catch
                    {
                        return null;
                    }
                });

            }

            #endregion

            #region StorageEngineFactory_internal(myStrings)

            /// <summary>
            /// This constructor will autodiscover all implementations of IStorageEngine
            /// </summary>
            public StorageEngineFactory_internal(String[] myStrings)
            {

                FindAndRegisterImplementations(false, myStrings, t =>
                {
                    try
                    {
                        var _IStorageEngine = (IStorageEngine) Activator.CreateInstance(t);
                        return _IStorageEngine.URIPrefix;
                    }
                    catch
                    {
                        return null;
                    }
                });

            }

            #endregion

            #endregion


            #region ActivateIStorageEngine(myImplementation)

            public Exceptional<IStorageEngine> ActivateIStorageEngine(String myStorageLocation)
            {

                if (myStorageLocation.IsNullOrEmpty())
                    throw new ArgumentNullException("myStorageLocation must not be null or empty!");

                return ActivateT_protected(myStorageLocation.Substring(0, myStorageLocation.IndexOf(':'))).
                    WhenSucceded<IStorageEngine>(v =>
                    {
                        v.Value.AttachStorage(myStorageLocation);
                        return v;
                    }).
                    FailedAction<IStorageEngine>(v =>
                        v.PushIError(new StorageEngineError("Could not find StorageEngine for '" + myStorageLocation + "'")));

            }

            #endregion

            #region CreateIStorageEngine(myStorageLocation, myNumberOfBytes, myBufferSize, myOverwriteExistingFilesystem, myAction)

            /// <summary>
            /// Activates a new instance of a IStorageEngine based on the given storage
            /// location and tries to create and fortmat a new storage engine there. The
            /// StorageEngine will not be opend for reading nor writting! Use
            /// OpenIStorageEngine(myStorageLocation) to open the StorageEngine after the
            /// creation was successful!
            /// </summary>
            /// <param name="myStorageLocation">The location of the IStorageEngine, e.g. file://myFileStorage.fs</param>
            /// <param name="myNumberOfBytes">The initial size of the image file in byte.</param>
            /// <param name="myBufferSize">The size of the internal buffer during formating the image file.</param>
            /// <param name="myOverwriteExistingFilesystem">Delete an existing image file?</param>
            /// <param name="myAction">An action called to indicate to progress on formating the image file.</param>
            /// <returns>An activated instance of IStorageEngine</returns>
            public IStorageEngine CreateIStorageEngine(String myStorageLocation, UInt64 myNumberOfBytes, UInt32 myBufferSize, Boolean myOverwriteExistingFilesystem, Action<Double> myAction)
            {

                lock (this)
                {

                    try
                    {

                        foreach (var _IStorageEngine in _DictionaryT)
                            if (myStorageLocation.StartsWith(_IStorageEngine.Key + "://"))
                                return (IStorageEngine) Activator.CreateInstance(_IStorageEngine.Value, myStorageLocation, myNumberOfBytes, myBufferSize, myOverwriteExistingFilesystem, myAction);

                    }
                    catch (Exception e)
                    {
                        throw new StorageEngineException("Could not create StorageEngine '" + myStorageLocation + "'!" + e);
                    }

                }

                throw new StorageEngineException("Could not create StorageEngine '" + myStorageLocation + "'!");

            }

            #endregion

        }

    }

}
