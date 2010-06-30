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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.QueryLanguage.NonTerminalClasses.Structure;
using sones.Lib.Frameworks.Irony.Parsing;
using sones.GraphDB.QueryLanguage.Enums;

namespace sones.GraphDB.QueryLanguage.NonTerminalCLasses.Structure
{

    public class SettingOperationNode : AStructureNode, IAstNodeInit
    {

        TypesOfSettingOperation _OperationType;
        public TypesOfSettingOperation OperationType
        {
            get { return _OperationType; }
        }
        Dictionary<string, string> _Settings = null;
        public Dictionary<string, string> Settings
        {
            get { return _Settings; }
        }

        public void GetContent(CompilerContext context, ParseTreeNode parseNode)
        {
            _Settings = new Dictionary<string, string>();

            foreach (ParseTreeNode pChildNode in parseNode.ChildNodes)
            {
                if (pChildNode.HasChildNodes())
                {
                    switch (pChildNode.ChildNodes[0].Token.Text.ToUpper())
                    {
                        case "GET":
                            _OperationType = TypesOfSettingOperation.GET;
                            break;
                        case "SET":
                            _OperationType = TypesOfSettingOperation.SET;
                            break;
                        case "REMOVE":
                            _OperationType = TypesOfSettingOperation.REMOVE;
                            break;
                    }
                    foreach (ParseTreeNode pValChild in pChildNode.ChildNodes[1].ChildNodes)
                    {
                        if (pValChild != null)
                        {
                            if (pValChild.HasChildNodes())
                            {
                                if (pValChild.ChildNodes[0] != null)
                                {
                                    if (pValChild.ChildNodes[2] != null)
                                    {
                                        if (pValChild.ChildNodes[0].Token != null && pValChild.ChildNodes[2].Token != null)
                                        {
                                            var Temp = pValChild.ChildNodes[2].Token.Text.ToUpper();

                                            if (Temp.Contains("DEFAULT"))
                                                _Settings.Add(pValChild.ChildNodes[0].Token.ValueString.ToUpper(), Temp);
                                            else
                                                _Settings.Add(pValChild.ChildNodes[0].Token.ValueString.ToUpper(), pValChild.ChildNodes[2].Token.ValueString.ToUpper());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (pValChild.Token != null)
                                    _Settings.Add(pValChild.Token.ValueString, "");
                            }
                        }
                    }
                }
            }
        }

        #region IAstNodeInit Members

        public void Init(CompilerContext context, ParseTreeNode parseNode)
        {
            GetContent(context, parseNode);
        }

        #endregion
    }
}
