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



#region Usings

using System;

using sones.GraphDB.Exceptions;
using sones.GraphDB.QueryLanguage.NonTerminalClasses.Structure;

using sones.Lib.Frameworks.Irony.Parsing;

#endregion

namespace sones.GraphDB.QueryLanguage.NonTerminalCLasses.Statements.Dump
{

    [Flags]
    public enum DumpFormats
    {
        GQL = 1,
        CSV = 2
    }

    public class DumpFormatNode : AStructureNode
    {

        public DumpFormats DumpFormat { get; set; }

        public void GetContent(CompilerContext context, ParseTreeNode parseNode)
        {
            
            var _GraphQL = GetGraphQLGrammar(context);

            if (parseNode.HasChildNodes())
            {

                var _Terminal = parseNode.ChildNodes[1].Token.Terminal;

                if (_Terminal == _GraphQL.S_DUMP_FORMAT_GQL)
                {
                    DumpFormat = DumpFormats.GQL;
                }
                else if (_Terminal == _GraphQL.S_DUMP_FORMAT_CSV)
                {
                    DumpFormat = DumpFormats.CSV;
                }
                else
                {
                    throw new GraphDBException(new Errors.Error_InvalidDumpFormat(_Terminal.DisplayName));
                }

            }
            else
            {
                DumpFormat = DumpFormats.GQL;
            }

        }

    }

}
