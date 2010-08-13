﻿///* GraphFS CLI - FILEEXISTS
// * (c) Henning Rauch, 2009
// * 
// * Checks if a file object exists
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
//    /// Checks if a file object exists
//    /// </summary>

//    public class FSCLI_FILEEXISTS : AScriptingFSCLICommands
//    {


//        #region Properties

//        #region General Command Infos

//        public override String ShortCommand { get { return "FILEEXISTS"; } }
//        public override String Command      { get { return "FILEEXISTS"; } }
//        public override String ShortInfo    { get { return "Checks if a certain file exists."; } }
//        public override String Information  { get { return "Checks if a certain file exists."; } }

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

//                _CommandGrammar.Root = FILEEXISTS;

//                return _CommandGrammar;

//            }

//        }


//        #endregion

//        #endregion

//        #region Constructor

//        public FSCLI_FILEEXISTS()
//        {

//            #region BNF rules

//            FILEEXISTS.Rule = FILEEXISTS_CommandString + stringLiteralPVFS;
            
//            #endregion

//            #region Todo: via reflection

//            #region Non-terminal integration

//            _CommandNonTerminals.Add(FILEEXISTS);

//            #endregion

//            #region Symbol integration

//            _CommandSymbolTerminal.Add(FILEEXISTS_CommandString);
            
//            #endregion

//            #endregion

//        }

//        #endregion


//        #region Symbol declaration

//        SymbolTerminal FILEEXISTS_CommandString = Symbol("FILEEXISTS");

//        #endregion

//        #region Non-terminal declaration

//        NonTerminal FILEEXISTS = new NonTerminal("FILEEXISTS");

//        #endregion

//        #region Terminal declaration



//        #endregion

//        #region Execute Command

//        public override void Execute(ref object myPFSObject, ref object myPDBObject, ref String myCurrentPath, Dictionary<string, List<AbstractCLIOption>> myOptions, string myInputString)
//        {
//            WriteLine(((IGraphFSSession)myPFSObject).ObjectExists(GetObjectLocation(myCurrentPath, myOptions.ElementAt(1).Value[0].Option)));
//        }

//        #endregion

//    }

//}
