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

/* <id name="sones GraphDB – ID node" />
 * <copyright file="IDNode.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Henning Rauch</developer>
 * <summary>This node is requested in case of an ID statement. This might be something like U.Name or Name or U.$GUID or U.Friends.Name. It is necessary to execute an AType node (or TypeWrapper) in previous.</summary>
 */

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using sones.GraphDB.Errors;
using sones.GraphDB.Exceptions;
using sones.GraphDB.ObjectManagement;
using sones.GraphDB.QueryLanguage.NonTerminalCLasses.Structure;
using sones.GraphDB.TypeManagement;
using sones.Lib.ErrorHandling;
using sones.Lib.Session;
using sones.Lib;
using sones.Lib.Frameworks.Irony.Parsing;
using sones.GraphDB.QueryLanguage.Enums;
using sones.GraphDB.Managers.Structures;

#endregion

namespace sones.GraphDB.QueryLanguage.NonTerminalClasses.Structure
{

    /// <summary>
    /// This node is requested in case of an ID statement. This might be something like U.Name or Name or U.$GUID or U.Friends.Name.
    /// It is necessary to execute an AType node (or TypeWrapper) in previous.
    /// </summary>
    public class IDNode : AStructureNode
    {

        public IDChainDefinition IDChainDefinition { get; private set; }
        private String _IDNodeString;

        #region constructor

        public IDNode()
        {
        
        }

        public IDNode(GraphDBType myType, String myReference)
        {

            IDChainDefinition = new Managers.Structures.IDChainDefinition();
            IDChainDefinition.AddPart(new ChainPartTypeOrAttributeDefinition(myType.Name));
            var listOfRefs = new Dictionary<String, GraphDBType>();
            listOfRefs.Add(myReference, myType);

        }

        #endregion
        
        /// <summary>
        /// This method extracts information of irony child nodes.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="parseNode"></param>
        /// <param name="myTypeManager"></param>
        public void GetContent(CompilerContext context, ParseTreeNode parseNode)
        {
            var dbContext = context.IContext as DBContext;
            var typeManager = dbContext.DBTypeManager;

            ExtractIDNodeString(parseNode);

            IDChainDefinition = new IDChainDefinition(_IDNodeString, GetTypeReferenceDefinitions(context));

            foreach (var child in parseNode.ChildNodes)
            {
                if (child.AstNode is EdgeTraversalNode)
                {

                    #region EdgeTraversalNode

                    var edgeTraversal = (child.AstNode as EdgeTraversalNode);
                    if (edgeTraversal.FuncCall != null)
                    {
                        IDChainDefinition.AddPart(edgeTraversal.FuncCall.FuncDefinition, new IDChainDelemiter(edgeTraversal.Delimiter.GetKindOfDelimiter()));
                    }
                    else
                    {
                        IDChainDefinition.AddPart(new ChainPartTypeOrAttributeDefinition(edgeTraversal.AttributeName), new IDChainDelemiter(edgeTraversal.Delimiter.GetKindOfDelimiter()));
                    }
                    
                    #endregion

                }
                else if (child.AstNode is EdgeInformationNode)
                {

                    #region EdgeInformation

                    var aEdgeInformation = (EdgeInformationNode)child.AstNode;

                    throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true), "Start here integrating edgeinfos"));

                    #endregion

                }
                else if (child.AstNode is FuncCallNode)
                {
                    IDChainDefinition.AddPart((child.AstNode as FuncCallNode).FuncDefinition);
                }
                else
                {
                    IDChainDefinition.AddPart(new ChainPartTypeOrAttributeDefinition(child.Token.ValueString));
                }
            }

        }


        /// <summary>
        /// This method extracts the IDNodeString from the irony parse tree
        /// </summary>
        /// <param name="parseNode">A ParseTree node.</param>
        /// <returns>The IDNode String.</returns>
        private void ExtractIDNodeString(ParseTreeNode parseNode)
        {
            foreach (var aChildNode in parseNode.ChildNodes)
            {
                if (aChildNode.AstNode == null)
                {
                    _IDNodeString += aChildNode.Token.ValueString;
                }
                else
                {
                    if (aChildNode.AstNode is EdgeTraversalNode)
                    {
                        var aEdgeTraversalNode = (EdgeTraversalNode)aChildNode.AstNode;

                        _IDNodeString += aEdgeTraversalNode.Delimiter.GetDelimiterString();

                        if (aEdgeTraversalNode.FuncCall != null)
                        {
                            _IDNodeString += aEdgeTraversalNode.FuncCall.FuncDefinition.SourceParsedString;
                        }
                        else
                        {
                            _IDNodeString += aEdgeTraversalNode.AttributeName;
                        }
                    }
                    else
                    {
                        if (aChildNode.AstNode is FuncCallNode)
                        {
                            _IDNodeString += ((FuncCallNode)aChildNode.AstNode).FuncDefinition.SourceParsedString;
                        }
                        else
                        {
                            if (aChildNode.AstNode is EdgeInformationNode)
                            {
                                var aEdgeInformationNode = (EdgeInformationNode)aChildNode.AstNode;

                                _IDNodeString += aEdgeInformationNode.Delimiter.GetDelimiterString();

                                _IDNodeString += aEdgeInformationNode.EdgeInformationName;
                            }
                            else
                            {
                                throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
                            }
                        }
                    }
                }
            }
        }
    }
}
