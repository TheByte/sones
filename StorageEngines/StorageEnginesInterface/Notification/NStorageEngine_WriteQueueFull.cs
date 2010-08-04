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


/* PandoraLib - NStorageEngine_WriteQueueFull
 * (c) Stefan Licht, 2009
 * 
 * Lead programmer:
 *      Stefan Licht
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.Notifications;
using sones.Notifications.NotificationTypes;
using sones.Lib.Serializer;
using sones.Lib.NewFastSerializer;

namespace sones.StorageEngines.Notification
{

    /// <summary>
    /// Notifies about an WriteQueue which is full.
    /// </summary>

    public class NStorageEngine_WriteQueueFull : NStorageEngine
    {

        public new class Arguments : INotificationArguments
        {

            public UInt32 NumberOfQueueEntries;

            #region INotificationArguments Members

            public byte[] Serialize()
            {
                var _SerializationWriter = new SerializationWriter();
                _SerializationWriter.WriteUInt32(NumberOfQueueEntries);

                return _SerializationWriter.ToArray();
            }

            public void Deserialize(byte[] mySerializedBytes)
            {
                var _SerializationReader = new SerializationReader(mySerializedBytes);
                NumberOfQueueEntries = _SerializationReader.ReadUInt32();
            }

            #endregion

        }


        #region INotificationType Members

        public override string Description
        {
            get { return "Notifies about an WriteQueue which is full"; }
        }

        public override INotificationArguments GetEmptyArgumentInstance()
        {
            return new Arguments();
        }

        #endregion
    
    }

}
