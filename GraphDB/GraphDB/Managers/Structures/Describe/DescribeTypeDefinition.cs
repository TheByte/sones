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
 * DescribeTypeDefinition
 * (c) Stefan Licht, 2010
 */

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using sones.GraphDB.Structures.EdgeTypes;
using sones.Lib.ErrorHandling;
using sones.GraphDB.Errors;
using sones.GraphDB.TypeManagement;
using sones.GraphDBInterface.Result;

#endregion

namespace sones.GraphDB.Managers.Structures.Describe
{
    public class DescribeTypeDefinition : ADescribeDefinition
    {
        
        #region Data

        private String _TypeName;
        
        #endregion

        #region Ctor

        public DescribeTypeDefinition(string myTypeName = null)
        {
            _TypeName = myTypeName;
        }

        #endregion

        #region ADescribeDefinition

        public override Exceptional<SelectionResultSet> GetResult(DBContext myDBContext)
        {

            if (!String.IsNullOrEmpty(_TypeName))
            {

                #region Specific type

                var type = myDBContext.DBTypeManager.GetTypeByName(_TypeName);
                if (type != null)
                {
                    return new Exceptional<SelectionResultSet>(new SelectionResultSet(GenerateOutput(myDBContext, type)));
                }
                else
                {
                    return new Exceptional<SelectionResultSet>(new Error_TypeDoesNotExist(_TypeName));
                }

                #endregion

            }
            else
            {

                #region All types

                List<DBObjectReadout> resultingReadouts = new List<DBObjectReadout>();

                foreach (var type in myDBContext.DBTypeManager.GetAllTypes())
                {
                    resultingReadouts.Add(GenerateOutput(myDBContext, type));
                }

                return new Exceptional<SelectionResultSet>(new SelectionResultSet(resultingReadouts));

                #endregion
            }
            
        }
        
        #endregion

        #region GenerateOutput(myDBContext, myGraphDBType)

        /// <summary>
        /// Generate an output for an type with the attributes of the types and all parent types
        /// </summary>
        private DBObjectReadout GenerateOutput(DBContext myDBContext, GraphDBType myGraphDBType)
        {

            GraphDBType _ParentType = null;

            if (myGraphDBType.ParentTypeUUID != null)
                _ParentType = myDBContext.DBTypeManager.GetTypeByUUID(myGraphDBType.ParentTypeUUID);

            var _CurrentType = new Dictionary<String, Object>();

            _CurrentType.Add("Name", myGraphDBType.Name);
            _CurrentType.Add("UUID", myGraphDBType.UUID);
            _CurrentType.Add("Comment", myGraphDBType.Comment);
            _CurrentType.Add("Attributes", GenerateAttributeOutput(myGraphDBType, myDBContext));

            if (_ParentType != null)
                _CurrentType.Add("ParentType", GenerateOutput(myDBContext, _ParentType));

            return new DBObjectReadout(_CurrentType);

        }

        /// <summary>
        /// output for the type attributes
        /// </summary>
        /// <param name="myGraphDBType">the type</param>
        /// <param name="myDBContext">typemanager</param>
        /// <returns>a list of readouts, contains the attributes</returns>
        private IEnumerable<DBObjectReadout> GenerateAttributeOutput(GraphDBType myGraphDBType, DBContext myDBContext)
        {

            var _AttributeReadout = new List<DBObjectReadout>();

            foreach (var _TypeAttribute in myGraphDBType.Attributes)
            {
                var Attributes = new Dictionary<String, Object>();
                Attributes.Add("Name", _TypeAttribute.Value.Name);
                Attributes.Add("Type", _TypeAttribute.Value.GetDBType(myDBContext.DBTypeManager));
                Attributes.Add("UUID", _TypeAttribute.Value.UUID);

                _AttributeReadout.Add(new DBObjectReadout(Attributes));
            }

            if (_AttributeReadout.Count > 0)
                return _AttributeReadout;
            else
                return null;
        }

        #endregion

    }
}
