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


/* <id name="sones GraphDB – DefaultValueDefNode" />
 * <copyright file="DefaultValueDefNode.cs
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Stefan Licht</developer>
 * <summary>This node just hold the value of a DEFAULT parameter of a list definition.</summary>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.Lib.Frameworks.Irony.Parsing;
using sones.GraphDB.QueryLanguage.NonTerminalClasses.Structure;

namespace sones.GraphDB.QueryLanguage.NonTerminalCLasses.Structure
{
    /// <summary>
    /// This node just hold the value of a DEFAULT parameter of a list definition.
    /// </summary>
    public class DefaultValueDefNode : AStructureNode, IAstNodeInit
    {
        /// <summary>
        /// The default value.
        /// </summary>
        public Object Value { get; private set; }

        private void GetContent(CompilerContext context, ParseTreeNode parseNode)
        {
            Value = parseNode.ChildNodes[2].Token.Value;
        }

        #region IAstNodeInit Members

        public void Init(CompilerContext context, ParseTreeNode parseNode)
        {
            GetContent(context, parseNode);
        }

        #endregion
    }
}
