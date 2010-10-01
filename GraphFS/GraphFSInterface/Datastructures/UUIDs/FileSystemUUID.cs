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
 * FileSystemUUID
 * (c) Achim Friedland, 2008 - 2010
 */

#region Usings

using System;
using sones.Lib.DataStructures.UUID;

#endregion

namespace sones.GraphFS.DataStructures
{

    public sealed class FileSystemUUID : UUID
    {

        #region TypeCode

        public override UInt32 TypeCode { get { return 233; } }

        #endregion

        #region Constructors

        #region FileSystemUUID()

        public FileSystemUUID()
            : base()
        {
        }

        #endregion

        #region FileSystemUUID(myUInt64)

        public FileSystemUUID(UInt64 myUInt64)
            : base(myUInt64)
        {
        }

        #endregion

        #region FileSystemUUID(myString)

        public FileSystemUUID(String myString)
            : base(myString)
        {
        }

        #endregion

        #region FileSystemUUID(mySerializedData)

        public FileSystemUUID(Byte[] mySerializedData)
            : base(mySerializedData)
        {
        }

        #endregion

        #region FileSystemUUID(myUUID)

        /// <summary>
        /// Generates a UUID based on the content of myUUID
        /// </summary>
        /// <param name="myUUID">A UUID</param>
        public FileSystemUUID(UUID myUUID)
        {
            var _ByteArray = myUUID.GetByteArray();
            _UUID = new Byte[_ByteArray.LongLength];
            Array.Copy(_ByteArray, 0, _UUID, 0, _ByteArray.LongLength);
        }

        #endregion

        #endregion

        #region NewUUID

        public new static FileSystemUUID NewUUID
        {
            get
            {
                return new FileSystemUUID(Guid.NewGuid().ToByteArray());
            }
        }

        #endregion

    }

}
