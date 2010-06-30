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

/* <id name="sones GraphDB – ASettingTypeNode" />
 * <copyright file="ASettingTypeNode.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Dirk Bludau</developer>
 * <summary></summary>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.QueryLanguage.NonTerminalClasses.Structure;
using sones.GraphDB.TypeManagement;
using sones.Lib.Frameworks.Irony.Parsing;

namespace sones.GraphDB.QueryLanguage.NonTerminalCLasses.Structure
{
    public class SettingTypeNode : AStructureNode
    {
        #region Data
        List<GraphDBType> _TypeList;
        #endregion

        #region Constructor
        public SettingTypeNode()
        {
            _TypeList = new List<GraphDBType>();
        }
        #endregion

        public void GetContent(CompilerContext context, ParseTreeNode parseNode)
        {
            if (parseNode == null)
                return;

            if (parseNode.HasChildNodes() && parseNode.ChildNodes[1].HasChildNodes())
            {
                foreach (var Node in parseNode.ChildNodes[1].ChildNodes)
                {
                    GraphDBType typeOfNode = (Node.AstNode as ATypeNode).DBTypeStream as GraphDBType;
                    if (typeOfNode != null)
                    {
                        if (!_TypeList.Contains(typeOfNode))
                            _TypeList.Add(typeOfNode);
                    }
                }
            }
        }

        #region Accessor

        public List<GraphDBType> Types
        { get { return _TypeList; } }

        #endregion
    }
}
