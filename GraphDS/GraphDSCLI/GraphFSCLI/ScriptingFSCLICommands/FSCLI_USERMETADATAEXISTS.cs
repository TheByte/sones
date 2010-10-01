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

///* GraphFS CLI - USERMETADATAEXISTS
// * (c) Henning Rauch, 2009
// * 
// * Checks if a user metadata object exists
// * 
// * Lead programmer:
// *      Henning Rauch
// *      Achim Friedland
// * 
// */

//#region Usings

//using System;
//using System.Linq;
//using System.Text;
//using System.Collections.Generic;
//using System.IO;

//using sones.Lib.Frameworks.CLIrony.Compiler;
//using sones.Graph.Storage.GraphFS;
//using sones.Graph.Storage.GraphFS.Objects;
//using sones.Lib;
//using sones.Lib.CLI
//using sones.Graph.Storage;
//using sones.Graph.Storage.GraphFS.Session;

//#endregion

//namespace sones.Graph.Applications.GraphFSCLI
//{

//    /// <summary>
//    /// Checks if a user metadata object exists
//    /// </summary>

//    public class FSCLI_USERMETADATAEXISTS : AScriptingFSCLICommands
//    {


//        #region Properties

//        #region General Command Infos

//        public override String ShortCommand { get { return "USERMETADATAEXISTS"; } }
//        public override String Command      { get { return "USERMETADATAEXISTS"; } }
//        public override String ShortInfo    { get { return "Checks if a certain user metadata object exists."; } }
//        public override String Information  { get { return "Checks if a certain user metadata object exists."; } }

//        public override CLICommandCategory aCategory
//        {
//            get { return CLICommandCategory.ScriptingFSCLICommand; }
//        }

//        #endregion

//        #region Command Grammar

//        public override Grammar CommandGrammar
//        {

//            get
//            {

//                _CommandGrammar.Terminals.AddRange(_CommandTerminals);


//                foreach (NonTerminal aNonTerminal in _CommandNonTerminals)
//                {
//                    if (!aNonTerminal.GraphOptions.Contains(GraphOption.IsOption))
//                    {
//                        aNonTerminal.GraphOptions.Add(GraphOption.IsStructuralObject);
//                    }
//                }

//                _CommandGrammar.NonTerminals.AddRange(_CommandNonTerminals);

//                foreach (SymbolTerminal _aCommandSymbolTerminal in _CommandSymbolTerminal)
//                {
//                    _CommandGrammar.Terminals.Add(_aCommandSymbolTerminal);
//                }

//                _CommandGrammar.Root = USERMETADATAEXISTS;

//                return _CommandGrammar;

//            }

//        }


//        #endregion

//        #endregion

//        #region Constructor

//        public FSCLI_USERMETADATAEXISTS()
//        {

//            #region BNF rules

//            USERMETADATAEXISTS.Rule = USERMETADATAEXISTS_CommandString + stringLiteralPVFS;
            
//            #endregion

//            #region Todo: via reflection

//            #region Non-terminal integration

//            _CommandNonTerminals.Add(USERMETADATAEXISTS);

//            #endregion

//            #region Symbol integration

//            _CommandSymbolTerminal.Add(USERMETADATAEXISTS_CommandString);
            
//            #endregion

//            #endregion

//        }

//        #endregion


//        #region Symbol declaration

//        SymbolTerminal USERMETADATAEXISTS_CommandString = Symbol("USERMETADATAEXISTS");

//        #endregion

//        #region Non-terminal declaration

//        NonTerminal USERMETADATAEXISTS = new NonTerminal("USERMETADATAEXISTS");

//        #endregion

//        #region Terminal declaration



//        #endregion

//        #region Execute Command

//        public override void Execute(ref object myPFSObject, ref object myPDBObject, ref String myCurrentPath, Dictionary<string, List<AbstractCLIOption>> myOptions, string myInputString)
//        {
//            WriteLine(((IGraphFSSession)myPFSObject).ObjectStreamExists2(FSConstants.USERMETADATASTREAM, GetObjectLocation(myCurrentPath, myOptions.ElementAt(1).Value[0].Option)));
//        }

//        #endregion

//    }

//}
