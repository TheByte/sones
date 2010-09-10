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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.Managers.Select;

namespace sones.GraphDB.Managers.Structures
{
    public class SelectDefinition : AExpressionDefinition
    {

        #region Properties

        /// <summary>
        /// List of selected types
        /// </summary>
        public List<TypeReferenceDefinition> TypeList { get; private set; }

        /// <summary>
        /// AExpressionDefinition, Alias
        /// </summary>
        public List<Tuple<AExpressionDefinition, string, SelectValueAssignment>> SelectedElements { get; private set; }

        /// <summary>
        /// Group by definitions
        /// </summary>
        public List<IDChainDefinition> GroupByIDs { get; private set; }

        /// <summary>
        /// Having definition
        /// </summary>
        public BinaryExpressionDefinition Having { get; private set; }

        /// <summary>
        /// OrderBy section
        /// </summary>
        public OrderByDefinition OrderByDefinition { get; private set; }

        /// <summary>
        /// Limit section
        /// </summary>
        public UInt64? Limit { get; private set; }

        /// <summary>
        /// Offset section
        /// </summary>
        public UInt64? Offset { get; private set; }

        /// <summary>
        /// Resolution depth
        /// </summary>
        public Int64 ResolutionDepth { get; private set; }

        public BinaryExpressionDefinition WhereExpressionDefinition { get; private set; }

        #endregion

        #region Ctor

        public SelectDefinition(List<TypeReferenceDefinition> myTypeList, List<Tuple<AExpressionDefinition, string, SelectValueAssignment>> mySelectedElements, BinaryExpressionDefinition myWhereExpressionDefinition,
            List<IDChainDefinition> myGroupByIDs, BinaryExpressionDefinition myHaving, ulong? myLimit, ulong? myOffset, Structures.OrderByDefinition myOrderByDefinition, long myResolutionDepth)
        {
            TypeList = myTypeList;
            SelectedElements = mySelectedElements;
            WhereExpressionDefinition = myWhereExpressionDefinition;
            GroupByIDs = myGroupByIDs;
            Having = myHaving;
            Limit = myLimit;
            Offset = myOffset;
            OrderByDefinition = myOrderByDefinition;
            ResolutionDepth = myResolutionDepth;
        }

        #endregion
    
    }
}
