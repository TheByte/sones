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

#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.Lib.ErrorHandling;
using sones.GraphDB.ObjectManagement;
using sones.GraphDB.TypeManagement.BasicTypes;
using sones.GraphFS.Session;

using sones.GraphDB.TypeManagement;
using sones.GraphDB.Errors;
using System.Diagnostics;

#endregion

namespace sones.GraphDB.TypeManagement.SpecialTypeAttributes
{
    public class SpecialTypeAttribute_LASTMODIFICATIONTIME : ASpecialTypeAttribute
    {

        #region AttributeUUID

        public static AttributeUUID AttributeUUID = new AttributeUUID(7);

        #endregion

        #region Name

        public static String AttributeName = "LASTMODIFICATIONTIME";

        #endregion


        #region constructors

        public SpecialTypeAttribute_LASTMODIFICATIONTIME()
        {
            Name = AttributeName;
            UUID = AttributeUUID;
        }

        #endregion


        public override string ShowSettingName
        {
            get { return "LASTMODIFICATIONTIME"; }
        }

        public override Exceptional<Object> ApplyTo(DBObjectStream myNewDBObject, object myValue, params object[] myOptionalParameters)
        {
            return new Exceptional<Object>(new Error_NotImplemented(new StackTrace(true)));
        }

        public override Exceptional<IObject> ExtractValue(DBObjectStream dbObjectStream, GraphDBType graphDBType, DBContext dbContext)
        {
            return new Exceptional<IObject>(new DBUInt64(dbObjectStream.INodeReference.LastModificationTime));
        }

    }
}
