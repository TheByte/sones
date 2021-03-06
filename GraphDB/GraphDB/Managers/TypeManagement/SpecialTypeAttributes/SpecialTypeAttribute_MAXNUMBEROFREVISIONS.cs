﻿#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.ObjectManagement;
using sones.Lib.ErrorHandling;

using sones.GraphDB.Errors;
using sones.GraphDB.TypeManagement.BasicTypes;
using sones.GraphDB.TypeManagement;
using System.Diagnostics;

#endregion

namespace sones.GraphDB.TypeManagement.SpecialTypeAttributes
{
    public class SpecialTypeAttribute_MAXNUMBEROFREVISIONS : ASpecialTypeAttribute
    {

        #region AttributeUUID

        public static AttributeUUID AttributeUUID = new AttributeUUID(17);

        #endregion

        #region Name

        public static String AttributeName = "MAX_NUMBER_OF_REVISIONS";

        #endregion        
        
        #region constructors

        public SpecialTypeAttribute_MAXNUMBEROFREVISIONS()
        {
            Name = AttributeName;
            UUID = AttributeUUID;
        }

        #endregion

        public override string ShowSettingName
        {
            get
            {
                return "MAX_NUMBER_OF_REVISIONS";
            }
        }

        public override Exceptional<Object> ApplyTo(DBObjectStream myNewDBObject, object myValue, params object[] myOptionalParameters)
        {
            return new Exceptional<Object>(new Error_NotImplemented(new StackTrace(true)));
        }

        public override Exceptional<IObject> ExtractValue(DBObjectStream dbObjectStream, GraphDBType graphDBType, DBContext dbContext)
        {
            return new Exceptional<IObject>(new DBUInt64(dbObjectStream.MaxNumberOfRevisions));
        }

    }
}
