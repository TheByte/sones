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


/* StudiVZ Database Demo - DBCLI_DBQUERY
 * Achim Friedland, 2009
 * 
 * StudiVZ Database Demo
 * 
 * Lead programmer:
 *      Achim Friedland
 * 
 */

#region Usings

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using sones.Lib.ErrorHandling;
using sones.Lib.CLI;
using sones.Lib.Frameworks.CLIrony.Compiler;

using sones.GraphDB;
using sones.GraphDB.QueryLanguage.Result;

#endregion

namespace sones.GraphDB.Connectors.GraphDBCLI
{

    /// <summary>
    /// This command enables the user to get informations concerning 
    /// the myAttributes of a given type.
    /// </summary>

    public class DBCLI_DBQUERY : AllBasicDBCLICommands
    {

        #region Constructor

        public DBCLI_DBQUERY()
        {

            // Command name and description
            InitCommand("DBQUERY",
                        "Executes a query",
                        "Executes a query");

            // BNF rule
            CreateBNFRule(CLICommandSymbolTerminal + stringLiteral);

        }

        #endregion

        #region Execute Command

        public override void Execute(ref object myIGraphFS2Session, ref object myIPandoraDBSession, ref String myCurrentPath, Dictionary<String, List<AbstractCLIOption>> myOptions, String myInputString)
        {

            _CancelCommand = false;
            //IGraphFSSession _IGraphFS2Session = myIGraphFS2Session as IGraphFSSession;
            var _IPandoraDBSession = myIPandoraDBSession as IGraphDBSession;
            QueryResult _QueryResult;

            if (_IPandoraDBSession == null)
            {
                WriteLine("No database instance started...");
                return;
            }

            if (CLI_Output == CLI_Output.Standard)
                _QueryResult = QueryDB(myOptions.ElementAt(1).Value[0].Option, _IPandoraDBSession);
            else
                _QueryResult = QueryDB(myOptions.ElementAt(1).Value[0].Option, _IPandoraDBSession, false);

            Write(_QueryResult.toTEXT(CLI_Output));

            if (CLI_Output == CLI_Output.Standard)
                WriteLine();

        }

        #endregion

    }

}

