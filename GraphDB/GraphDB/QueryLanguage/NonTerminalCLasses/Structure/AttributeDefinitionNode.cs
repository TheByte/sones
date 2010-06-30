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


/* <id name="sones GraphDB – Attribute Definition astnode" />
 * <copyright file="AttributeDefinitionNode.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Henning Rauch</developer>
 * <summary>This node is requested in case of attribute definition statement.</summary>
 */

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.Lib.Frameworks.Irony.Scripting.Ast;
using sones.Lib.Frameworks.Irony.Parsing;
using sones.GraphDB.QueryLanguage.Enums;
using sones.GraphDB.TypeManagement;
using sones.GraphDB.Structures.EdgeTypes;
using sones.GraphDB.Exceptions;
using sones.GraphDB.Errors;
using sones.GraphDB.QueryLanguage.NonTerminalCLasses.Structure;

#endregion

namespace sones.GraphDB.QueryLanguage.NonTerminalClasses.Structure
{
    /// <summary>
    /// This node is requested in case of an Create Type statement.
    /// </summary>
    public class AttributeDefinitionNode : AStructureNode
    {
        #region Data

        private String _Type = null;
        private TypeAttribute _TypeAttribute = null;

        #endregion

        #region constructor

        public AttributeDefinitionNode()
        {
            
        }

        #endregion

        public void GetContent(CompilerContext myCompilerContext, ParseTreeNode myParseTreeNode)
        {
            var dbContext = myCompilerContext.IContext as DBContext;
            var typeManager = dbContext.DBTypeManager;
            
            if (dbContext.DBSettingsManager.HasSetting(myParseTreeNode.ChildNodes[1].Token.ValueString))
                throw new GraphDBException(new Error_AttributeAlreadyExists(myParseTreeNode.ChildNodes[1].Token.ValueString));
            
            _TypeAttribute = new TypeAttribute();
            
            #region get Attribute Name

            _TypeAttribute.Name = myParseTreeNode.ChildNodes[1].Token.ValueString;
            
            #endregion

            #region get Attribute type

            GraphDBTypeNode aTypeNode = (GraphDBTypeNode)myParseTreeNode.ChildNodes[0].AstNode;
            
            //we can not validate the type at this point, because in the bulk type creation we does not have the type
            _Type = aTypeNode.Name;

            //if we have an default value for this attribute, then check for correct value and attribute type
            if (myParseTreeNode.ChildNodes[2].AstNode != null)
            {
                AttrDefaultValueNode defaultValueNode = (AttrDefaultValueNode)myParseTreeNode.ChildNodes[2].AstNode;
                
                if (defaultValueNode.Value != null)
                {
                    if (_Type != defaultValueNode.BaseTypeName)
                    {
                        throw new GraphDBException(new Error_InvalidAttrDefaultValueAssignment(_TypeAttribute.Name, _Type, defaultValueNode.BaseTypeName));
                    }

                    if (defaultValueNode.Value is AListBaseEdgeType)
                    {
                        if (myParseTreeNode.ChildNodes[0].FirstChild.Token.Text.ToUpper() == DBConstants.SET && defaultValueNode.TypeOfList == TypesOfPandoraType.ListOfNoneReferences)
                        {
                            throw new GraphDBException(new Error_InvalidAttrDefaultValueAssignment(_TypeAttribute.Name, TypesOfPandoraType.ListOfNoneReferences.ToString(), TypesOfPandoraType.SetOfReferences.ToString()));
                        }

                        if (defaultValueNode.TypeOfList == TypesOfPandoraType.SetOfReferences)
                        {
                            ((AListBaseEdgeType)defaultValueNode.Value).UnionWith((AListBaseEdgeType)defaultValueNode.Value);
                        }
                    }
                }

                _TypeAttribute.DefaultValue = defaultValueNode.Value;
            }

            switch (aTypeNode.Type)
            {
                case TypesOfPandoraType.ListOfNoneReferences:
                    _TypeAttribute.KindOfType = KindsOfType.ListOfNoneReferences;
                    break;

                case TypesOfPandoraType.SetOfNoneReferences:
                    _TypeAttribute.KindOfType = KindsOfType.SetOfNoneReferences;
                    break;

                case TypesOfPandoraType.SetOfReferences:
                    _TypeAttribute.KindOfType = KindsOfType.SetOfReferences;
                    break;
                
                default:
                    _TypeAttribute.KindOfType = KindsOfType.SingleReference;
                    break;
            }            

            if (aTypeNode.TypeCharacteristics != null)
                _TypeAttribute.TypeCharacteristics = aTypeNode.TypeCharacteristics;

            _TypeAttribute.EdgeType = aTypeNode.EdgeType;
            
            #endregion
        }

        public String Name { get { return _TypeAttribute.Name; } }
        public String Type { get { return _Type; } }
        public TypeAttribute TypeAttribute { get { return _TypeAttribute; } }
        
    }
}
