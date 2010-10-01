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

/*
 * NObjectStored
 * (c) Achim Friedland, 20
 * 
 */

#region Usings

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using sones.Notifications;
using sones.Lib.NewFastSerializer;
using sones.Lib.DataStructures;
using sones.GraphFS.DataStructures;

#endregion

namespace sones.GraphFS.Notification
{

    /// <summary>
    /// Notifies about a stored object
    /// </summary>

    public class NObjectStored : NFileSystem
    {

        #region Nested class Arguments

        public new class Arguments : INotificationArguments
        {

            public ObjectLocation   ObjectLocation      { get; private set; }
            public String           ObjectStream        { get; private set; }
            public String           ObjectEdition       { get; private set; }
            public ObjectRevisionID       ObjectRevisionID    { get; private set; }

            public Arguments()
            {
                ObjectLocation = new ObjectLocation(FSPathConstants.PathDelimiter);
            }

            public Arguments(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, ObjectRevisionID myObjectRevisionID)
            {
                ObjectLocation      = myObjectLocation;
                ObjectStream        = myObjectStream;
                ObjectEdition       = myObjectEdition;
                ObjectRevisionID    = myObjectRevisionID;
            }

            #region INotificationArguments Members

            public Byte[] Serialize()
            {

                var _SerializationWriter = new SerializationWriter();

                _SerializationWriter.WriteString(ObjectLocation.ToString());
                _SerializationWriter.WriteString(ObjectStream);
                _SerializationWriter.WriteString(ObjectEdition);
                _SerializationWriter.WriteString(ObjectRevisionID.ToString());

                return _SerializationWriter.ToArray();

            }

            public void Deserialize(Byte[] mySerializedBytes)
            {
                
                var _SerializationReader = new SerializationReader(mySerializedBytes);

                ObjectLocation      = new ObjectLocation(_SerializationReader.ReadString());
                ObjectStream        = _SerializationReader.ReadString();
                ObjectEdition       = _SerializationReader.ReadString();
                ObjectRevisionID    = new ObjectRevisionID(_SerializationReader.ReadString());

            }

            #endregion

        }

        #endregion


        #region Clientside properties for validate()

        public ObjectLocation   ObjectLocation      { get; private set; }
        public String           ObjectStream        { get; private set; }
        public String           ObjectEdition       { get; private set; }
        public ObjectRevisionID       ObjectRevisionID    { get; private set; }

        public NObjectStored()
        { }

        public NObjectStored(ObjectLocation myObjectLocation, String myObjectStream, String myObjectEdition, ObjectRevisionID myObjectRevisionID)
        {
            ObjectLocation      = myObjectLocation;
            ObjectStream        = myObjectStream;
            ObjectEdition       = myObjectEdition;
            ObjectRevisionID    = myObjectRevisionID;
        }

        #endregion



        #region ANotificationType

        public override Boolean Validate(INotificationArguments myNotificationArguments)
        {
            return ((NObjectStored.Arguments) myNotificationArguments).ObjectLocation == ObjectLocation;
        }

        public override String Description
        {
            get { return "Notifies about any changes for a particular ObjectLocator"; }
        }

        public override INotificationArguments GetEmptyArgumentInstance()
        {
            return new Arguments();
        }

        #endregion

    }
}
