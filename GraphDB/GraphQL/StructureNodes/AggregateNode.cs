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

/* <id name="GraphDB � aggregate node" />
 * <copyright file="AggregateNode.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH. All rights reserved.
 * </copyright>
 * <developer>Henning Rauch</developer>
 * <summary>This node is requested in case of an aggregate statement.</summary>
 */

#region Usings

using sones.GraphDB.Managers.Structures;
using sones.GraphDB.GraphQL.StructureNodes;
using sones.Lib.Frameworks.Irony.Parsing;

#endregion

namespace sones.GraphDB.GraphQL.StructureNodes
{

    /// <summary>
    /// This node is requested in case of an aggregate statement.
    /// </summary>
    public class AggregateNode : FuncCallNode
    {

        public AggregateDefinition AggregateDefinition { get; private set; }

        #region constructor

        public AggregateNode()
        {
            
        }

        #endregion

        public new void GetContent(CompilerContext context, ParseTreeNode parseNode)
        {
            
            base.GetContent(context, parseNode);

            AggregateDefinition = new AggregateDefinition(new ChainPartAggregateDefinition(base.FuncDefinition));

        }

    }

}
