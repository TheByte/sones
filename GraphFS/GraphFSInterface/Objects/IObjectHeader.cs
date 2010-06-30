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
 * IObjectHeader
 * Achim Friedland, 2008 - 2010
 */

#region Usings

using System;

using sones;
using sones.GraphFS.Session;
using sones.GraphFS.DataStructures;
using sones.Lib.DataStructures.UUID;
using sones.Lib.Session;
using sones.Lib.DataStructures.WeakReference;

#endregion

namespace sones.GraphFS.Objects
{

    public interface IObjectHeader
    {

        Boolean         isNew                       { get; set; }
        ObjectUUID      ObjectUUID                  { get; }
        UInt16          StructureVersion            { get; }

        WeakReference<IGraphFSSession> IGraphFSSessionReference { get; set; }
        WeakReference<IGraphFS>        IGraphFSReference           { get; set; }
        INode           INodeReference              { get; set; }
        ObjectLocator   ObjectLocatorReference      { get; set; }

        Byte[]          EncryptionParameters        { get; set; }
        Byte[]          IntegrityCheckValue         { get; }

        DateTime        ModificationTime            { get; }

        UInt64          EstimatedSize               { get; set; }
        UInt64          ReservedSize                { get; set; }
        Byte[]          SerializedAPandoraStructure { get; set; }

        SessionToken    SessionToken                { get; set; }

        Boolean         isDirty                     { get; set; }

    }

}
