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


/* <id name="sones GraphDB – SelectResultManager" />
 * <copyright file="SelectResultManager.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Stefan Licht</developer>
 * <summary>This will create the result of any kind of select - working on an IExpressionGraph.</summary>
 */

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using sones.GraphDB.Errors;
using sones.GraphDB.Exceptions;
using sones.GraphDB.Managers.Structures;
using sones.GraphDB.ObjectManagement;
using sones.GraphDB.Structures.Enums;
using sones.GraphDB.Structures.ExpressionGraph;
using sones.GraphDB.Structures.Result;
using sones.GraphDB.Settings;
using sones.GraphDB.Structures.EdgeTypes;
using sones.GraphDB.TypeManagement;
using sones.GraphDB.TypeManagement.BasicTypes;
using sones.GraphDB.TypeManagement.SpecialTypeAttributes;
using sones.GraphFS.DataStructures;
using sones.Lib;
using sones.Lib.ErrorHandling;
using sones.Lib.Session;
using sones.Lib.DataStructures.UUID;

#endregion

namespace sones.GraphDB.Managers.Select
{

    #region GroupingKey and GroupingStructure

    public struct GroupingValuesKey
    {
        public AttributeUUID AttributeUUID;
        public String AttributeAlias;
        public String AttributeName;

        public GroupingValuesKey(TypeManagement.AttributeUUID myAttributeUUID, String myAttributeAlias)
        {
            AttributeUUID = myAttributeUUID;
            AttributeAlias = myAttributeAlias;
            AttributeName = null;
        }

        public GroupingValuesKey(String myAttributeName, String myAttributeAlias)
        {
            AttributeName = myAttributeName;
            AttributeAlias = myAttributeAlias;
            AttributeUUID = null;
        }

        public override string ToString()
        {
            return AttributeAlias;
        }

        public override int GetHashCode()
        {
            if (AttributeUUID == null)
            {
                return AttributeAlias.GetHashCode();
            }
            else
            {
                if (AttributeName != null)
                {
                    return AttributeName.GetHashCode();
                }
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GroupingValuesKey))
            {
                return false;
            }

            if (((GroupingValuesKey)obj).AttributeUUID != null)
            {
                return ((GroupingValuesKey)obj).AttributeUUID.Equals(AttributeUUID);
            }
            else
            {
                return ((GroupingValuesKey)obj).AttributeName.Equals(AttributeName);
            }
        }

    }

    public class GroupingKey : IComparable<Dictionary<GroupingValuesKey, ADBBaseObject>>
    {

        Dictionary<GroupingValuesKey, ADBBaseObject> _Values;
        Dictionary<GroupingValuesKey, IDChainDefinition> _GroupingAttr;

        public Dictionary<GroupingValuesKey, ADBBaseObject> Values
        {
            get { return _Values; }
            set { _Values = value; }
        }

        public GroupingKey(Dictionary<GroupingValuesKey, ADBBaseObject> myValues, Dictionary<GroupingValuesKey, IDChainDefinition> myGroupingAttr)
        {
            _Values = myValues;
            _GroupingAttr = myGroupingAttr;
        }

        #region IComparable<ADBBaseObject> Members

        public int CompareTo(Dictionary<GroupingValuesKey, ADBBaseObject> other)
        {

            // it could happen, that not all grouped aatributes are existing in all DBOs, so use the group by from the select to check
            foreach (var attr in _GroupingAttr.Keys)
            {
                if (_Values.ContainsKey(attr) && other.ContainsKey(attr))
                    if (_Values[attr].CompareTo(other[attr]) != 0)
                        return -1;
            }

            return 0;
        }

        #endregion

        public override int GetHashCode()
        {
            Int32 retVal = -1;
            foreach (KeyValuePair<GroupingValuesKey, ADBBaseObject> keyValPair in _Values)
            {
                if (retVal == -1)
                    retVal = keyValPair.Value.Value.GetHashCode();
                else
                    retVal = retVal ^ keyValPair.Value.Value.GetHashCode();
            }

            return retVal;
        }

        public override bool Equals(object obj)
        {
            return CompareTo(((GroupingKey)obj).Values) == 0;
        }
    }

    public class GroupingStructure
    {

        private Dictionary<GroupingValuesKey, IDChainDefinition> _GroupingAttr;

        // The attribute value of each Group BY, Set of DBOs which have exactly these values
        private Dictionary<GroupingKey, HashSet<DBObjectReadout>> _GroupsAndAggregates;
        public Dictionary<GroupingKey, HashSet<DBObjectReadout>> GroupsAndAggregates
        {
            get { return _GroupsAndAggregates; }
            set { _GroupsAndAggregates = value; }
        }

        public GraphDBType DBTypeStream
        {
            get;
            set;
        }

        public GroupingStructure(Dictionary<GroupingValuesKey, IDChainDefinition> myGroupingAttr)
        {
            _GroupsAndAggregates = new Dictionary<GroupingKey, HashSet<DBObjectReadout>>();
            _GroupingAttr = myGroupingAttr;
        }

        public void Add(DBObjectReadout myDBObjectReadout, Dictionary<GroupingValuesKey, ADBBaseObject> myValues)
        {
            GroupingKey curKey = new GroupingKey(myValues, _GroupingAttr);
            if (!_GroupsAndAggregates.ContainsKey(curKey))
            {
                _GroupsAndAggregates.Add(curKey, new HashSet<DBObjectReadout>());
            }
            _GroupsAndAggregates[curKey].Add(myDBObjectReadout);
        }
    }

    #endregion

    public class SelectResultManager
    {

        #region Data

        #region DBObjectCache

        DBObjectCache _DBObjectCache;
        public DBObjectCache DBObjectCache
        {
            set
            {
                _DBObjectCache = value;
            }
        }

        #endregion

        #region SessionToken

        SessionSettings _SessionToken;

        public SessionSettings SessionToken
        {
            set
            {
                _SessionToken = value;
            }
        }
        
        #endregion

        #region ExpressionGraph

        IExpressionGraph _ExpressionGraph;
        public IExpressionGraph ExpressionGraph
        {
            set
            {
                _ExpressionGraph = value;
            }
        } 

        #endregion

        #region Some private fields

        DBContext _DBContext;
        SelectSettingCache _SettingCache;
        IEnumerable<DBObjectReadout> _DBOsEnumerable;

        /// <summary>
        /// Dictionary of the base type and the selections of this type: e.g. U.Name and U.Cars.Color are both of basetype User.
        /// reference [LevelKey, [selection]]
        /// With that, we need to 'work' on each attribute only one time!
        /// </summary>
        /// The Level is the Level of LevelKey: e.g. U.Name == 0 and U.Cars.Color == 1
        /// If the User selected U.Name, U.Age than we have two SelectionElements in level 0 of type User
        /// In case of: "FROM User SELECT Friends.TOP(2).TOP(1).Friends.TOP(2).Name, Friends.TOP(2).TOP(1).Name, Name WHERE Name = 'Lila'"
        /// we have for reference 'User' the edgeList for Name, Friends (with Name and Friends) and Friends.Friends (with Name)
        Dictionary<String, Dictionary<EdgeList, List<SelectionElement>>> _Selections;


        List<SelectionElement> _SelectionElementsTypeIndependend;

        /// <summary>
        /// the selection element of _Selections , the aggregate selection element
        /// </summary>
        List<SelectionElementAggregate> _Aggregates;
        List<SelectionElement> _Groupings;

        Dictionary<String, GroupingStructure> _GroupingStructure;
        BinaryExpressionDefinition _HavingExpression;

        Dictionary<GraphDBType, List<TypeAttribute>> _BackwardEdgeAttributesByType = new Dictionary<GraphDBType, List<TypeAttribute>>();
        Dictionary<GraphDBType, List<TypeAttribute>> _SpecialAttributesByType      = new Dictionary<GraphDBType, List<TypeAttribute>>();

        #endregion

        #endregion

        #region Ctor

        public SelectResultManager(DBContext dbContext)
        {

            _DBContext = dbContext;
            _SettingCache = new SelectSettingCache();

            _Selections = new Dictionary<string, Dictionary<EdgeList, List<SelectionElement>>>();
            _SelectionElementsTypeIndependend = new List<SelectionElement>();
            _GroupingStructure = new Dictionary<string,GroupingStructure>();

            _Aggregates = new List<SelectionElementAggregate>();
            _Groupings = new List<SelectionElement>();

        }

        #endregion

        #region Adding elements to selection

        /// <summary>
        /// Adds the typeNode as an asterisk *, rhomb # or minus - or ad
        /// </summary>
        /// <param name="typeNode"></param>
        public Exceptional AddSelectionType(String myReference, GraphDBType myType, TypesOfSelect mySelType, TypeUUID myTypeID = null)
        {
            var selElem = new SelectionElement(mySelType, myTypeID);

            if (!_Selections.ContainsKey(myReference))
                _Selections.Add(myReference, new Dictionary<EdgeList, List<SelectionElement>>());

            var level = new EdgeList(new EdgeKey(myType.UUID, null));

            if (!_Selections[myReference].ContainsKey(level))
                _Selections[myReference].Add(level, new List<SelectionElement>());

            if (!_Selections[myReference][level].Exists(item => item.Selection == mySelType))
            {
                _Selections[myReference][level].Add(selElem);
            }

            return Exceptional.OK;

        }
        
        /// <summary>
        /// Single IDNode selection attribute
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="alias"></param>
        /// <param name="myAStructureNode"></param>
        /// <param name="myGraphType"></param>
        public Exceptional AddElementToSelection(string alias, String reference, IDChainDefinition myIDChainDefinition, Boolean isGroupedOrAggregated)
        {
            SelectionElement lastElem = null;

            var curLevel = new EdgeList();
            EdgeList preLevel = null;

            if (reference != null && _Selections.ContainsKey(reference) && _Selections[reference].Any(kv => kv.Value.Any(se => se.RelatedIDChainDefinition == myIDChainDefinition && se.Alias == alias)))
            {
                return new Exceptional(new Error_DuplicateAttributeSelection(alias));
            }

            foreach (var nodeEdgeKey in myIDChainDefinition)
            {

                if (nodeEdgeKey is ChainPartTypeOrAttributeDefinition)
                {

                    #region Usual attribute 

                    preLevel = null;

                    var selElem = new SelectionElement(alias, curLevel, isGroupedOrAggregated, myIDChainDefinition);

                    var typeOrAttr = (nodeEdgeKey as ChainPartTypeOrAttributeDefinition);

                    if (typeOrAttr.DBType != null && typeOrAttr.TypeAttribute != null)
                    {
                        #region defined

                        var edgeKey = typeOrAttr.EdgeKey;
                        selElem.Element = _DBContext.DBTypeManager.GetTypeAttributeByEdge(edgeKey);

                        if (String.IsNullOrEmpty(selElem.Alias) || (nodeEdgeKey.Next != null && !(nodeEdgeKey.Next is ChainPartFuncDefinition)))
                        {
                            selElem.Alias = _DBContext.DBTypeManager.GetTypeAttributeByEdge(edgeKey).Name;
                        }

                        curLevel += edgeKey;
                        preLevel = curLevel.GetPredecessorLevel(); 

                        #endregion
                    }
                    else
                    {
                        #region undefined attribute
                        
                        if (myIDChainDefinition.Level == 0)
                        {
                            preLevel = new EdgeList(myIDChainDefinition.LastType.UUID);
                        }
                        else
                        {
                            var element = _Selections[reference].Last();
                            
                            preLevel = curLevel;
                        }
                        selElem.Alias = typeOrAttr.TypeOrAttributeName;

                        #endregion
                    }

                    if (!_Selections.ContainsKey(reference))
                    {
                        _Selections.Add(reference, new Dictionary<EdgeList, List<SelectionElement>>());
                    }

                    if (!_Selections[reference].ContainsKey(preLevel))
                    {
                        _Selections[reference].Add(preLevel, new List<SelectionElement>());
                    }

                    ///
                    /// Duplicate AttributeSelection is: "U.Name, U.Name" or "U.Name.TOUPPER(), U.Name" but not "U.Friends.TOP(1).Name, U.Friends.TOP(1).Age"
                    if ((nodeEdgeKey.Next == null || (nodeEdgeKey.Next is ChainPartFuncDefinition && nodeEdgeKey.Next.Next == null))
                        //                                                        U.Name, U.Name                  U.Name.TOUPPER, U.Name
                        && _Selections[reference][preLevel].Exists(item => item.Alias == selElem.Alias && selElem.EdgeList.Level == item.EdgeList.Level && item.RelatedIDChainDefinition.Depth == selElem.RelatedIDChainDefinition.Depth && item.Element != null) && !isGroupedOrAggregated)
                    {
                        return new Exceptional(new Error_DuplicateAttributeSelection(selElem.Alias));
                    }

                    if (nodeEdgeKey.Next != null && _Selections[reference][preLevel].Exists(item => item.Alias == selElem.Alias && item.RelatedIDChainDefinition.Depth == selElem.RelatedIDChainDefinition.Depth))
                    {
                        // do not add this element again!
                    }
                    else
                    {                        
                        _Selections[reference][preLevel].Add(selElem);
                    }

                    lastElem = selElem;

                    #endregion

                }
                else if (nodeEdgeKey is ChainPartFuncDefinition)
                {

                    var chainPartFuncDefinition = (nodeEdgeKey as ChainPartFuncDefinition);

                    #region Function

                    if (reference == null)
                    {

                        #region Type independent functions

                        var selElem = new SelectionElement(alias, myIDChainDefinition);
                        if (String.IsNullOrEmpty(selElem.Alias))
                        {
                            selElem.Alias = chainPartFuncDefinition.SourceParsedString;
                        }
                        var funcElem = new SelectionElementFunction(selElem, chainPartFuncDefinition, chainPartFuncDefinition.Parameters);

                        if (lastElem is SelectionElementFunction)
                        {
                            (lastElem as SelectionElementFunction).AddFollowingFunction(funcElem);
                            lastElem = funcElem;
                        }
                        else
                        {
                            if (_SelectionElementsTypeIndependend.Any(se => se.Alias == funcElem.Alias))
                            {
                                return new Exceptional(new Error_DuplicateAttributeSelection(funcElem.Alias));
                            }

                            _SelectionElementsTypeIndependend.Add(funcElem);
                            lastElem = funcElem;
                        }

                        #endregion

                    }
                    else
                    {

                        var funcElem = new SelectionElementFunction(lastElem, (nodeEdgeKey as ChainPartFuncDefinition), (nodeEdgeKey as ChainPartFuncDefinition).Parameters);
                        funcElem.RelatedIDChainDefinition = myIDChainDefinition;

                        if (!String.IsNullOrEmpty(alias) && nodeEdgeKey.Next == null)
                        {
                            funcElem.Alias = alias;
                        }

                        if (lastElem is SelectionElementFunction)
                        {
                            (lastElem as SelectionElementFunction).AddFollowingFunction(funcElem);
                            lastElem = funcElem;
                        }
                        else if (_Selections[reference][preLevel].Contains(lastElem))
                        {

                            #region Add function to the last selection element (replace it)

                            _Selections[reference][preLevel].Remove(lastElem);

                            //lastElem = new SelectionElementFunction(lastElem, (nodeEdgeKey as ChainPartFuncDefinition), (nodeEdgeKey as ChainPartFuncDefinition).Parameters);
                            //lastElem.RelatedIDChainDefinition = myIDChainDefinition;

                            //if (!String.IsNullOrEmpty(alias) && nodeEdgeKey.Next == null)
                            //{
                            //    lastElem.Alias = alias;
                            //}
                            lastElem = funcElem;

                            if (!_Selections[reference][preLevel].Contains(lastElem)) // In case this Element with func is already in the selection list do nothing.
                            {

                                _Selections[reference][preLevel].Add(lastElem);

                            }

                            #endregion

                        }
                        else if (!_Selections[reference][preLevel].Contains(funcElem))
                        {

                            #region In this case we have a similar function but NOT THE SAME. Since we don't know what to do, return error.

                            return new Exceptional(new Error_InvalidAttributeSelection(myIDChainDefinition.ContentString));
                            
                            #endregion

                        }

                    }
                    
                    #endregion

                }
            }

            return Exceptional.OK;

        }

        /// <summary>
        /// Multi IDNode selection attribute
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="alias"></param>
        /// <param name="myAStructureNode"></param>
        /// <param name="myGraphType"></param>
        public Exceptional AddAggregateElementToSelection(string alias, string reference, SelectionElementAggregate mySelectionPartAggregate)
        {

            #region Check for index aggregate

            foreach (var edge in mySelectionPartAggregate.EdgeList.Edges)
            {

                #region COUNT(*)

                if (edge.AttrUUID == null)
                {
                    mySelectionPartAggregate.IndexAggregate = _DBContext.DBTypeManager.GetTypeByUUID(edge.TypeUUID).GetUUIDIndex(_DBContext.DBTypeManager);
                    mySelectionPartAggregate.Element = _DBContext.DBTypeManager.GetUUIDTypeAttribute();
                }

                #endregion

                else if (_DBContext.DBTypeManager.GetTypeAttributeByEdge(edge).EdgeType is ASetOfReferencesEdgeType)
                {
                    return new Exceptional(new Error_AggregateIsNotValidOnThisAttribute(_DBContext.DBTypeManager.GetTypeAttributeByEdge(edge).Name));
                }

                else
                {
                    // if the GetAttributeIndex did not return null we will pass this as the aggregate operation value
                    mySelectionPartAggregate.IndexAggregate = _DBContext.DBTypeManager.GetTypeByUUID(edge.TypeUUID).GetAttributeIndex(edge.AttrUUID, null).Value;
                    mySelectionPartAggregate.Element = _DBContext.DBTypeManager.GetTypeAttributeByEdge(edge);
                }
            }

            #endregion

            _Aggregates.Add(mySelectionPartAggregate);

            if (mySelectionPartAggregate.IndexAggregate == null) // If this is not an index aggregate, we need to get all values prior - so add to usual select
            {
                var addResult = AddElementToSelection(null, reference, mySelectionPartAggregate.Parameter, true);
                if (addResult.Failed())
                {
                    return new Exceptional(addResult);
                }
            }

            return Exceptional.OK;

        }

        /// <summary>
        /// Adds a group element to the selection and validat it
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="myIDChainDefinition"></param>
        public void AddGroupElementToSelection(string reference, IDChainDefinition myIDChainDefinition)
        {
            // if the grouped attr is not selected
            if ((!_Selections.ContainsKey(reference) || !_Selections[reference].Any(l => l.Value.Any(se => se.Element != null && se.Element == myIDChainDefinition.LastAttribute))) && !myIDChainDefinition.IsUndefinedAttribute)
                throw new GraphDBException(new Error_GroupedAttributeIsNotSelected(myIDChainDefinition.LastAttribute));
            else
                _Groupings.Add(new SelectionElement(reference, new EdgeList(myIDChainDefinition.Edges), false, myIDChainDefinition, myIDChainDefinition.LastAttribute));
                //{ Alias = reference, Element = myIDChainDefinition.LastAttribute, RelatedIDChainDefinition = myIDChainDefinition, EdgeList = new EdgeList(myIDChainDefinition.Edges) });
        }

        /// <summary>
        /// Adds a having to the selection
        /// </summary>
        /// <param name="myHavingExpression"></param>
        public void AddHavingToSelection(BinaryExpressionDefinition myHavingExpression)
        {
            _HavingExpression = myHavingExpression;
        }

        #endregion

        #region Examine

        /// <summary>
        /// Examine a TypeNode to a specific <paramref name="myResolutionDepth"/> by using the underlying graph or the type guid index
        /// </summary>
        /// <param name="myResolutionDepth">The depth to which the reference attributes should be resolved</param>
        /// <param name="myTypeNode">The type node which should be examined on selections</param>
        /// <param name="myUsingGraph">True if there is a valid where expression of this typeNode</param>
        /// <returns>True if succeeded, false if there was nothing to select for this type</returns>
        public Exceptional<Boolean> Examine(Int64 myResolutionDepth, String myReference, GraphDBType myReferencedDBType, Boolean myUsingGraph, DBObjectCache myDBObjectCache, SessionSettings mySessionToken, ref IEnumerable<DBObjectReadout> dbosEnumerable)
        {
            #region Return false if no selections were found for this typenode

            if (!_Selections.ContainsKey(myReference) || !_Selections[myReference].ContainsKey(new EdgeList(myReferencedDBType.UUID)))
            {
                
                #region Aggregates on index wont be in the _Selections

                if (_Aggregates.IsNullOrEmpty())
                {
                    return new Exceptional<bool>(false);
                }
                else
                {
                    if (myUsingGraph) // Due to we have a where expression we can't use the index - so add all AggrergateIndices to the selection
                    {
                        foreach(var aggr in _Aggregates.Where(a => a.IndexAggregate != null))
                        {
                            AddElementToSelection(null, myReference, aggr.Parameter, true);
                        }
                    }
                    else
                    {
                        return new Exceptional<bool>(true);
                    }
                } 
                
                #endregion

            }

            #endregion

            var levelKey = new EdgeList(myReferencedDBType.UUID);
            //_DBOsEnumerable = dbosEnumerable;

            #region For groupings we cant create a enumerable

            if (!_Groupings.IsNullOrEmpty())
            {
                CreateGroupings(myResolutionDepth, myReference, myReferencedDBType, levelKey, myUsingGraph, myDBObjectCache, mySessionToken);
            }
            else
            {
                _DBOsEnumerable = dbosEnumerable;
                dbosEnumerable  = ExamineDBO(myResolutionDepth, myReference, myReferencedDBType, levelKey, myUsingGraph, myDBObjectCache, mySessionToken);
            }

            #endregion

            return new Exceptional<Boolean>(true);

        }

        /// <summary>
        /// This is the main function. It will check all selections on this type and will create the readouts
        /// </summary>
        /// <param name="dbos"></param>
        /// <param name="myResolutionDepth"></param>
        /// <param name="myTypeNode"></param>
        /// <param name="levelKey"></param>
        /// <param name="myUsingGraph">True if for all selects the graph will be asked for DBOs</param>
        /// <param name="myDBObjectCache"></param>
        /// <param name="mySessionToken"></param>
        /// <returns></returns>
        private IEnumerable<DBObjectReadout> ExamineDBO(long myResolutionDepth, String myReference, GraphDBType myReferencedDBType, EdgeList levelKey, bool myUsingGraph, DBObjectCache myDBObjectCache, SessionSettings mySessionToken)
        {

            #region Get all selection for this reference, type and level
            
            var selections = getAttributeSelections(myReference, myReferencedDBType, levelKey);

            #endregion

            if (!selections.IsNullOrEmpty() && selections.All(s => s.IsReferenceToSkip(levelKey) ))
            {

                #region If there are only references in this level, we will skip this level (and add the attribute as placeholder) and step to the next one

                var Attributes = new Dictionary<String, Object>();

                foreach (var sel in selections)
                {
                    var edgeKey = new EdgeKey(sel.Element.RelatedGraphDBTypeUUID, sel.Element.UUID);                    
                    Attributes.Add(sel.Alias, new Edge(ExamineDBO(myResolutionDepth, myReference, myReferencedDBType, levelKey + edgeKey, myUsingGraph, myDBObjectCache, mySessionToken), sel.Element.GetDBType(_DBContext.DBTypeManager).Name));
                }
                
                yield return new DBObjectReadout(Attributes);

                #endregion

            }
            else
            {

                #region Otherwise load all dbos until this level and return them

                #region Get dbos enumerable of the first level - this need to be changed if the graph is used but the first (0) level is not selected

                IEnumerable<Exceptional<DBObjectStream>> dbos;
                if (myUsingGraph)
                {
                    dbos = _ExpressionGraph.Select(new LevelKey(levelKey.Edges, _DBContext.DBTypeManager), null, true);
                }
                else // using GUID index
                {
                    dbos = _DBObjectCache.SelectDBObjectsForLevelKey(new LevelKey(levelKey.Edges, _DBContext.DBTypeManager), _DBContext);
                }

                #endregion

                foreach (var aDBObject in dbos)
                {
                    if (aDBObject.Failed())
                    {
                        // since we are in an yield we can do nothing else than throw a exception
                        throw new GraphDBException(aDBObject.Errors);
                    }

                    #region Create a readoutObject for this DBO and yield it: on an failure throw a exception

                    var examineResult = CreateReadOutObject(aDBObject.Value, myResolutionDepth, myReference, myReferencedDBType, levelKey, myUsingGraph, myDBObjectCache, mySessionToken);

                    if (examineResult.Failed())
                    {
                        // since we are in an yield we can do nothing else than throw a exception
                        throw new GraphDBException(examineResult.Errors);
                    }

                    if (examineResult.Value != null) // could happens, if the DBO has no selected attributes
                        yield return examineResult.Value;

                    #endregion

                }

                #endregion

            }

        }

        /// <summary>
        /// Create readouts of all attributes of a specific type <paramref name="myTypeOfDBObject"/>
        /// </summary>
        /// <param name="myDBObject">The DBObject which is examined</param>
        /// <param name="myDepth">The Depth</param>
        /// <param name="myTypeOfDBObject">The Type</param>
        /// <param name="myLevelKey">The levelKey</param>
        /// <returns>Successful or Failed for any error</returns>
        private Exceptional<DBObjectReadout> CreateReadOutObject(DBObjectStream myDBObject, Int64 myDepth, String myReference, GraphDBType myReferencedDBType, EdgeList myLevelKey, Boolean myUsingGraph, DBObjectCache myObjectCache, SessionSettings mySessionToken)
        {

            var Attributes = GetAllSelectedAttributesFromDBO(myDBObject, myReferencedDBType, myDepth, myLevelKey, myReference, myUsingGraph);

            if (!Attributes.IsNullOrEmpty())
            {
                return new Exceptional<DBObjectReadout>(new DBObjectReadout(Attributes));
            }
            else
            {
                // we found no attributes, so we return null because currently we do not want to add empty attribute readouts
                return new Exceptional<DBObjectReadout>((DBObjectReadout)null);
            }

        }

        private Exceptional<FuncParameter> ExecuteFunction(SelectionElementFunction mySelectionElementFunction, DBObjectStream myDBObject, IObject myCallingObject, Int64 myDepth, String myReference, GraphDBType myReferencedDBType, EdgeList myLevelKey, Boolean myUsingGraph)
        {

            #region Function

            if (myCallingObject == null) // DBObject does not have the attribute
            {
                return null;
            }

            #region Get the FunctionNode and validate the Element

            var func = mySelectionElementFunction.Function;
            func.Function.CallingAttribute = mySelectionElementFunction.Element;

            if (mySelectionElementFunction.Element == null)
            {
                return null;
            }

            #endregion

            func.Function.CallingObject = myCallingObject;

            #region CallingDBObjectStream

            func.Function.CallingDBObjectStream = myDBObject;

            #endregion

            #region Execute the function

            var res = func.Execute(myReferencedDBType, myDBObject, myReference, _DBContext, _DBObjectCache);
            if (res.Failed())
            {
                return new Exceptional<FuncParameter>(res);
            }
            else
            {
                if (res.Value.Value == null)
                {
                    return null; // no result for this object because of not set attribute value
                }

                if (mySelectionElementFunction.FollowingFunction != null)
                {
                    return ExecuteFunction(mySelectionElementFunction.FollowingFunction, myDBObject, res.Value.Value, myDepth, myReference, myReferencedDBType, myLevelKey, myUsingGraph);
                }
                else
                {
                    return res;
                }

            }


            #endregion

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myDepth">The target depth (defined by the select)</param>
        /// <param name="minDepth">The minimum depth. If the depth of the setting or mydepth is lower then minDepth will be returned</param>
        /// <param name="typeUUID"></param>
        /// <param name="attributeUUID">May be NULL, than only the depth settings of the type will be checked</param>
        /// <param name="sessionToken"></param>
        /// <returns></returns>
        private long GetDepth(long myDepth, long minDepth, GraphDBType type, TypeAttribute attribute = null)
        {
            Int64 Depth = 0;

            #region Get the depth for this type or from select

            /// This results of all selected attributes and their LevelKey. If U.Friends.Friends.Name is selected, than the MinDepth is 3

            if (attribute != null)
            {

                if (myDepth > -1)
                    Depth = Math.Max(minDepth, myDepth);
                else
                    Depth = Math.Max(minDepth, (Int64)_SettingCache.GetValue(type, attribute, SettingDepth.UUID, _DBContext).Value);

            }
            else
            {
            
                if (myDepth > -1)
                    Depth = Math.Max(minDepth, myDepth);
                else
                    Depth = Math.Max(minDepth, (Int64)(_DBContext.DBSettingsManager.GetSetting(SettingDepth.UUID, _DBContext, TypesSettingScope.TYPE, type).Value.Value).Value);

            }

            #endregion

            return Depth;
        }

        /// <summary>
        /// Get all selections on this <paramref name="reference"/>, <paramref name="type"/> and <paramref name="myLevelKey"/>
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="type"></param>
        /// <param name="myLevelKey"></param>
        /// <returns></returns>
        private List<SelectionElement> getAttributeSelections(String reference, GraphDBType type, EdgeList myLevelKey)
        {
            if (_Selections.ContainsKey(reference) && (_Selections[reference].ContainsKey(myLevelKey)))
            {
                return _Selections[reference][myLevelKey];
            }
            else
            {
                return null;
            }

        }

        #endregion

        #region Grouping and Aggregates

        /// <summary>
        /// Initialize the grouping and aggregate and validate them
        /// </summary>
        /// <param name="typeNode"></param>
        /// <returns></returns>
        public Exceptional<bool> InitGroupingOrAggregate(String myReference, GraphDBType myDBType)
        {
            #region if we have grouping or aggregate the must be NO attribute in the select list which is not covert by an aggregate or grouping

            // all selections which are NOT from an aggregate
            var selectionsWithoutGroupOrAggregate = ((_Selections.ContainsKey(myReference)) ? _Selections[myReference].Aggregate(0, (result, el) => result + el.Value.Count) : 0) - _Aggregates.Count(e => e.IndexAggregate == null) - _Groupings.Count;


            #region If we have defined grouping, all selects must be groups or aggregates

            if (_Groupings.Count > 0 || _Aggregates.Count > 0)
            {

                if (selectionsWithoutGroupOrAggregate > 0)
                    return new Exceptional<bool>(new Error_NoGroupingArgument());

            }

            #endregion

            #region Store the List of aggregated uuids

            if (!_Aggregates.IsNullOrEmpty())
            {
                foreach (var aggrNode in _Aggregates)
                {

                    // We checked prio in GetContent of SelectNode that we have exactly one expression
                    var aAggrIDNode = aggrNode.Parameter;

                    if (aAggrIDNode == null)
                    {
                        return new Exceptional<bool>(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
                    }
                    else if (aAggrIDNode.Level > 1)
                    {
                        return new Exceptional<bool>(new Error_AggregateIsNotValidOnThisAttribute(aAggrIDNode.ToString()));
                    }
                }
            }

            #endregion

            if (!_Groupings.IsNullOrEmpty())
            {

                var groupingAttr = new Dictionary<GroupingValuesKey, IDChainDefinition>();

                foreach (var intAttr in _Groupings)
                {
                    if (intAttr.Element == null)
                    {
                        groupingAttr.Add(new GroupingValuesKey(intAttr.RelatedIDChainDefinition.UndefinedAttribute, intAttr.Alias), intAttr.RelatedIDChainDefinition);
                    }
                    else
                    {
                        groupingAttr.Add(new GroupingValuesKey((intAttr.Element as TypeAttribute).UUID, intAttr.Alias), intAttr.RelatedIDChainDefinition);
                    }
                    break;
                }
                _GroupingStructure.Add(myReference, new GroupingStructure(groupingAttr) { DBTypeStream = myDBType });
  
            }
            #endregion

            return new Exceptional<bool>();
        }

        private void CreateGroupings(long myResolutionDepth, String myReference, GraphDBType myReferencedDBType, EdgeList levelKey, bool myUsingGraph, DBObjectCache myDBObjectCache, SessionSettings mySessionToken)
        {
            IEnumerable<Exceptional<DBObjectStream>> dbos;

            #region Get dbos enumerable of the first level - this need to be changed if the graph is used but the first (0) level is not selected

            if (myUsingGraph)
            {
                dbos = _ExpressionGraph.Select(new LevelKey( levelKey.Edges, _DBContext.DBTypeManager), null, true);
            }
            else // using GUID index
            {
                dbos = _DBObjectCache.SelectDBObjectsForLevelKey(new LevelKey(myReferencedDBType, _DBContext.DBTypeManager), _DBContext);
            }

            #endregion


            var interestingAttributes = GetInterestingAttributes(myReference, myReferencedDBType);

            foreach (var dbo in dbos)
            {

                var myDBObject = dbo.Value;
                var groupingVals = new Dictionary<GroupingValuesKey, ADBBaseObject>();

                foreach (var _InterestingAttribute in _Groupings)
                {

                    var typeAttr = _InterestingAttribute.Element as TypeAttribute;
                    // some grouping

                    if (typeAttr == null)
                    {
                        #region undefined attributes
                        
                        if (myDBObject.ContainsUndefinedAttribute(_InterestingAttribute.RelatedIDChainDefinition.UndefinedAttribute, _DBContext.DBObjectManager))
                        {
                            var attrValue = myDBObject.GetUndefinedAttributeValue(_InterestingAttribute.RelatedIDChainDefinition.UndefinedAttribute, _DBContext.DBObjectManager);

                            if (!attrValue.Success())
                            {
                                throw new GraphDBException(attrValue.Errors);
                            }

                            groupingVals.Add(new GroupingValuesKey(_InterestingAttribute.RelatedIDChainDefinition.UndefinedAttribute, _InterestingAttribute.Alias), GraphDBTypeMapper.GetBaseObjectFromCSharpType(attrValue.Value.GetReadoutValue()));
                        }

                        #endregion
                    }
                    else
                    {
                        #region defined attributes                      

                        if (myDBObject.HasAttribute(typeAttr.UUID, myReferencedDBType))
                        {
                            groupingVals.Add(new GroupingValuesKey(typeAttr.UUID, _InterestingAttribute.Alias),
                                GraphDBTypeMapper.GetGraphObjectFromTypeName(
                                typeAttr.GetDBType(_DBContext.DBTypeManager).Name, ResolveAttributeValue(_InterestingAttribute.Element as TypeAttribute, myDBObject.GetAttribute(typeAttr.UUID, myReferencedDBType, _DBContext),
                                myResolutionDepth, levelKey, myDBObject, myReference, myUsingGraph)
                                ));
                        }

                        #endregion
                    }
                }
                
                //_GroupingStructure[myTypeNode.Reference].Add(new DBObjectReadout(myDBObject.GetAttributes(), myTypeNode.DBTypeStream), groupingVals);
                _GroupingStructure[myReference].Add(new DBObjectReadout(GetInterestingAttributeValues(myDBObject, interestingAttributes, myReferencedDBType, mySessionToken)), groupingVals);
            }

        }

        private Dictionary<string, Object> GetInterestingAttributeValues(DBObjectStream myDBObject, HashSet<Object> interestingAttributes, GraphDBType graphDBType, SessionSettings mySessionToken)
        {
            Dictionary<string, Object> result = new Dictionary<string, Object>();

            foreach (var aTypeAttribute in interestingAttributes)
            {
                if (aTypeAttribute is TypeAttribute)
                {
                    #region defined attributes
                    
                    var tAttribute = aTypeAttribute as TypeAttribute;

                    if (!result.ContainsKey(tAttribute.Name))
                    {
                        result.Add(tAttribute.Name, ((IObject)myDBObject.GetAttribute(tAttribute, graphDBType, _DBContext)).GetReadoutValue());
                    }

                    #endregion
                }
                else if (aTypeAttribute is IDChainDefinition)
                {
                    var tAttribute = aTypeAttribute as IDChainDefinition;

                    if (myDBObject.ContainsUndefinedAttribute(tAttribute.UndefinedAttribute, _DBContext.DBObjectManager))
                    {
                        var resultExcept = myDBObject.GetUndefinedAttributeValue(tAttribute.UndefinedAttribute, _DBContext.DBObjectManager);

                        if (!resultExcept.Success())
                        {
                            throw new GraphDBException(resultExcept.Errors);
                        }

                        if (!result.ContainsKey(tAttribute.UndefinedAttribute))
                        {
                            result.Add(tAttribute.UndefinedAttribute, resultExcept.Value.GetReadoutValue());
                        }
                    }
                }
                else
                {
                    throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
                }
            }

            return result;
        }

        private HashSet<Object> GetInterestingAttributes(String myReference, GraphDBType graphDBType)
        {
            HashSet<Object> result = new HashSet<Object>();

            foreach (var aAggregate in _Aggregates)
            {
                result.Add(aAggregate.Element);
            }

            foreach (var aGrouping  in _Groupings.Where(item => item.Alias == myReference))
            {
                if (aGrouping.Element == null)
                {
                    result.Add(aGrouping.RelatedIDChainDefinition);
                }
                else
                {
                    result.Add(aGrouping.Element);
                }
            }

            foreach (var aSelection in _Selections.Where(item => item.Key == myReference).Select(aMatchingElement => aMatchingElement.Value))
            {
                foreach (var aLevelKey in aSelection.Select(item => item.Value))
                {
                    foreach (var aInnerSelection in aLevelKey)
                    {
                        if (aInnerSelection.RelatedIDChainDefinition.IsUndefinedAttribute)
                        {   
                            result.Add(aInnerSelection.RelatedIDChainDefinition);
                        }
                        else
                        {
                            result.Add(aInnerSelection.Element);
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region GetReadouts - from a SetReferenceEdgeType

        /// <summary>
        /// Resolve a AListReferenceEdgeType to a DBObjectReadouts. This will resolve each edge target using the 'GetAllAttributesFromDBO' method
        /// </summary>
        /// <param name="typeOfAttribute"></param>
        /// <param name="dbos"></param>
        /// <param name="myDepth"></param>
        /// <param name="currentLvl"></param>
        /// <returns></returns>
        private IEnumerable<DBObjectReadout> GetReadouts(GraphDBType typeOfAttribute, ASetOfReferencesEdgeType dbos, IEnumerable<Exceptional<DBObjectStream>> myObjectUUIDs, Int64 myDepth, EdgeList myLevelKey, String reference, Boolean myUsingGraph)
        {
            foreach (var aReadOut in dbos.GetReadouts(a =>
            {
                var dbStream = _DBObjectCache.LoadDBObjectStream(typeOfAttribute, a);
                if (dbStream.Failed())
                {
                    throw new GraphDBException(dbStream.Errors);
                }

                return new DBObjectReadout(GetAllSelectedAttributesFromDBO(dbStream.Value, typeOfAttribute, myDepth, myLevelKey, reference, myUsingGraph));
            }, myObjectUUIDs))
            {
                yield return aReadOut;
            }

            yield break;
        }

        /// <summary>
        /// Resolve a AListReferenceEdgeType to a List of DBObjectReadout. This will resolve each edge target using the 'GetAllAttributesFromDBO' method
        /// </summary>
        /// <param name="typeOfAttribute"></param>
        /// <param name="dbos"></param>
        /// <param name="myDepth"></param>
        /// <param name="currentLvl"></param>
        /// <returns></returns>
        private IEnumerable<DBObjectReadout> GetReadouts(GraphDBType typeOfAttribute, ASetOfReferencesEdgeType dbos, Int64 myDepth, EdgeList myLevelKey, String reference, Boolean myUsingGraph)
        {
            SettingInvalidReferenceHandling invalidReferenceSetting = null;
            GraphDBType currentType = null;

            foreach (var aReadOut in dbos.GetReadouts(a =>
            {
                var dbStream = _DBObjectCache.LoadDBObjectStream(typeOfAttribute, a);
                if (!dbStream.Success())
                {
                    #region error

                    #region get setting

                    if (invalidReferenceSetting == null)
                    {
                        currentType = _DBContext.DBTypeManager.GetTypeByUUID(myLevelKey.LastEdge.TypeUUID);

                        if (myLevelKey.LastEdge.AttrUUID != null)
                        {
                            invalidReferenceSetting = (SettingInvalidReferenceHandling)_DBContext.DBSettingsManager.GetSetting(SettingInvalidReferenceHandling.UUID, _DBContext, TypesSettingScope.ATTRIBUTE, currentType, currentType.GetTypeAttributeByUUID(myLevelKey.LastEdge.AttrUUID)).Value;
                        }
                        else
                        {
                            invalidReferenceSetting = (SettingInvalidReferenceHandling)_DBContext.DBSettingsManager.GetSetting(SettingInvalidReferenceHandling.UUID, _DBContext, TypesSettingScope.TYPE, currentType).Value;
                        }
                    }

                    #endregion

                    switch (invalidReferenceSetting.Behaviour)
                    {
                        case BehaviourOnInvalidReference.ignore:
                            #region ignore

                            return GenerateUndefindedDBReadout(a, currentType);

                            #endregion
                        case BehaviourOnInvalidReference.log:
                        
                        default:

                            throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace()));
                    }

                    #endregion
                }

                return new DBObjectReadout(GetAllSelectedAttributesFromDBO(dbStream.Value, typeOfAttribute, myDepth, myLevelKey, reference, myUsingGraph));               
            }))
            {
                if (aReadOut != null)
                {
                    yield return aReadOut;
                }
            }

            yield break;
        }

        #endregion

        #region AddAttribute and get the values

        /// <summary>
        /// Instead of using the Type with the attributes it will show ALL attributes of the <paramref name="myDBObject"/>
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="type"></param>
        /// <param name="myDBObject"></param>
        /// <param name="depth"></param>
        /// <param name="myEdgeList"></param>
        /// <param name="reference"></param>
        /// <param name="myUsingGraph"></param>
        private void AddAttributesByDBO(ref Dictionary<string, object> attributes, GraphDBType type, DBObjectStream myDBObject, long depth, EdgeList myEdgeList, string reference, bool myUsingGraph, TypesOfSelect mySelType, TypeUUID myTypeID = null)
        {

            #region Get all attributes which are stored at the DBO

            foreach (var attr in myDBObject.GetAttributes())
            {

                #region Check whether the attribute is still exist in the type - if not, continue

                var typeAttr = type.GetTypeAttributeByUUID(attr.Key);

                if (typeAttr == null)
                    continue;

                #endregion

                if (mySelType == TypesOfSelect.Ad)
                {                    
                    if (myTypeID != typeAttr.GetDBType(_DBContext.DBTypeManager).UUID)
                    {
                        continue;
                    }
                }
                
                if (attr.Value is ADBBaseObject)
                {
                    if (mySelType != TypesOfSelect.Minus && mySelType != TypesOfSelect.Gt && mySelType != TypesOfSelect.Lt)
                    {
                        attributes.Add(typeAttr.Name, (attr.Value as ADBBaseObject).GetReadoutValue());
                    }
                }
                else if (attr.Value is IBaseEdge)
                {
                    if (mySelType != TypesOfSelect.Minus && mySelType != TypesOfSelect.Gt && mySelType != TypesOfSelect.Lt)
                    {
                        attributes.Add(typeAttr.Name, (attr.Value as IBaseEdge).GetReadoutValues());
                    }
                }
                else if (attr.Value is IReferenceEdge)
                {
                    if (mySelType == TypesOfSelect.Minus || mySelType == TypesOfSelect.Asterisk || mySelType == TypesOfSelect.Ad || mySelType == TypesOfSelect.Gt)
                    {
                        // Since we can define special depth (via setting) for attributes we need to check them now
                        depth = GetDepth(-1, depth, type, typeAttr);
                        if (depth > 0)
                        {
                            attributes.Add(typeAttr.Name, ResolveAttributeValue(typeAttr, attr.Value, depth - 1, myEdgeList, myDBObject, reference, myUsingGraph));
                        }
                        else
                        {
                            attributes.Add(typeAttr.Name, GetNotResolvedReferenceAttributeValue(myDBObject, typeAttr, type, myEdgeList, myUsingGraph, _DBContext));
                        }
                    }
                }
                else
                    throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));

            }

            #endregion

            #region Get all backwardEdge attributes

            if (mySelType == TypesOfSelect.Minus || mySelType == TypesOfSelect.Asterisk || mySelType == TypesOfSelect.Ad || mySelType == TypesOfSelect.Lt)
            {
                foreach (var beAttr in GetBackwardEdgeAttributes(type))
                {
                    if (depth > 0)
                    {
                        if (mySelType == TypesOfSelect.Ad)
                        {
                            if (beAttr.BackwardEdgeDefinition.TypeUUID != myTypeID)
                            {
                                continue;
                            }
                        }
                        
                        var bes = myDBObject.GetBackwardEdges(beAttr.BackwardEdgeDefinition, _DBContext, _DBObjectCache, beAttr.GetDBType(_DBContext.DBTypeManager));
                        
                        if (bes.Failed())
                            throw new GraphDBException(bes.Errors);

                        if (bes.Value != null) // otherwise the DBO does not have any
                            attributes.Add(beAttr.Name, ResolveAttributeValue(beAttr, bes.Value, depth - 1, myEdgeList, myDBObject, reference, myUsingGraph));
                    }
                    else
                    {
                        if (mySelType == TypesOfSelect.Ad)
                        {
                            if (beAttr.BackwardEdgeDefinition.TypeUUID != myTypeID)
                            {
                                continue;
                            }
                        }
                        
                        var notResolvedBEs = GetNotResolvedBackwardEdgeReferenceAttributeValue(myDBObject, beAttr, beAttr.BackwardEdgeDefinition, myEdgeList, myUsingGraph, _DBContext);
                        if (notResolvedBEs != null)
                        {
                            attributes.Add(beAttr.Name, notResolvedBEs);
                        }
                    }
                }
            }
            #endregion

            #region Get all untyped attributes from DBO - to be done

            if (mySelType == TypesOfSelect.Asterisk || mySelType == TypesOfSelect.Rhomb)
            {
                var undefAttrException = myDBObject.GetUndefinedAttributes(_DBContext.DBObjectManager);

                if (undefAttrException.Failed())
                    throw new GraphDBException(undefAttrException.Errors);

                foreach (var undefAttr in undefAttrException.Value)
                    attributes.Add(undefAttr.Key, undefAttr.Value.GetReadoutValue());
            }

            #endregion

            #region Add special attributes

            if (mySelType == TypesOfSelect.Asterisk)
            {
                foreach (var specialAttr in GetSpecialAttributes(type))
                {
                    if (!attributes.ContainsKey(specialAttr.Name))
                    {
                        var result = (specialAttr as ASpecialTypeAttribute).ExtractValue(myDBObject, type, _DBContext);
                        if (result.Failed())
                        {
                            throw new GraphDBException(result.Errors);
                        }

                        attributes.Add(specialAttr.Name, result.Value.GetReadoutValue());
                    }
                }
            }

            #endregion
        }

        private IEnumerable<TypeAttribute> GetBackwardEdgeAttributes(GraphDBType type)
        {
            if (!_BackwardEdgeAttributesByType.ContainsKey(type))
            {
                _BackwardEdgeAttributesByType.Add(type, type.GetAllAttributes(t => t.IsBackwardEdge, _DBContext).ToList());
            }
            return _BackwardEdgeAttributesByType[type];
        }
        
        private IEnumerable<TypeAttribute> GetSpecialAttributes(GraphDBType type)
        {
            if (!_SpecialAttributesByType.ContainsKey(type))
            {
                _SpecialAttributesByType.Add(type, type.GetAllAttributes(t => t is ASpecialTypeAttribute, _DBContext).ToList());
            }
            return _SpecialAttributesByType[type];
        }

        /// <summary>   Gets an attribute value. </summary>
        ///
        /// <remarks>   Stefan, 16.04.2010. </remarks>
        ///
        /// <param name="myType">           Type. </param>
        /// <param name="myTypeAttribute">  my type attribute. </param>
        /// <param name="myDBObject">       my database object. </param>
        /// <param name="myDepth">          Depth of my. </param>
        /// <param name="myLevelKey">       my level key. </param>
        /// <param name="reference">        The reference. </param>
        /// <param name="myUsingGraph">     true to my using graph. </param>
        /// <param name="attributeValue">   [out] The attribute value. </param>
        ///
        /// <returns>   true if it succeeds, false if the DBO does not have the attribute. </returns>
        private Boolean GetAttributeValueAndResolve(GraphDBType myType, TypeAttribute myTypeAttribute, DBObjectStream myDBObject, Int64 myDepth, EdgeList myLevelKey, String reference, Boolean myUsingGraph, out Object attributeValue, String myUndefAttrName = null)
        {
                        
            if (myTypeAttribute == null)
            {
                #region undefined attributes

                attributeValue = null;

                if (myDBObject.ContainsUndefinedAttribute(myUndefAttrName, _DBContext.DBObjectManager))
                {
                    var retExcept = myDBObject.GetUndefinedAttributeValue(myUndefAttrName, _DBContext.DBObjectManager);

                    if (!retExcept.Success())
                        throw new GraphDBException(retExcept.Errors);

                    if (retExcept.Value is ADBBaseObject)
                        attributeValue = (retExcept.Value as ADBBaseObject).GetReadoutValue();
                    else if (retExcept.Value is IBaseEdge)
                        attributeValue = (retExcept.Value as IBaseEdge).GetReadoutValues();
                    else
                        attributeValue = retExcept.Value;

                    return true;
                }
                else
                {
                    return false;
                }
                
                #endregion
            }
            else if (myTypeAttribute.TypeCharacteristics.IsBackwardEdge)
            {
                #region IsBackwardEdge

                EdgeKey edgeKey = myTypeAttribute.BackwardEdgeDefinition;
                var contBackwardExcept = myDBObject.ContainsBackwardEdge(edgeKey, _DBContext, _DBObjectCache, myType);

                if (contBackwardExcept.Failed())
                    throw new GraphDBException(contBackwardExcept.Errors);

                if (contBackwardExcept.Value)
                {
                    if (myDepth > 0)
                    {
                        var dbos = myDBObject.GetBackwardEdges(edgeKey, _DBContext, _DBObjectCache, myTypeAttribute.GetDBType(_DBContext.DBTypeManager));

                        if (dbos.Failed())
                            throw new GraphDBException(dbos.Errors);

                        if (dbos.Value != null)
                        {
                            attributeValue = ResolveAttributeValue(myTypeAttribute, dbos.Value, myDepth - 1, myLevelKey, myDBObject, reference, myUsingGraph);
                            return true;
                        }
                    }
                    else
                    {
                        attributeValue = GetNotResolvedBackwardEdgeReferenceAttributeValue(myDBObject, myTypeAttribute, edgeKey, myLevelKey, myUsingGraph, _DBContext);
                        return true;

                    }
                }

                #endregion

            }
            else if (myDBObject.HasAttribute(myTypeAttribute.UUID, myType))
            {

                #region ELSE (!IsBackwardEdge)

                #region not a reference attribute value

                if (!myTypeAttribute.GetDBType(_DBContext.DBTypeManager).IsUserDefined)
                {
                    var attrVal = myDBObject.GetAttribute(myTypeAttribute.UUID, myType, _DBContext);
                    // currently, we do not want to return a ADBBaseObject but the real value
                    if (attrVal is ADBBaseObject)
                        attributeValue = (attrVal as ADBBaseObject).GetReadoutValue();
                    else if (attrVal is IBaseEdge)
                        attributeValue = (attrVal as IBaseEdge).GetReadoutValues();
                    else
                        attributeValue = attrVal;

                    return true;
                }

                #endregion

                #region ELSE Reference attribute value

                else
                {
                    if (myDepth > 0)
                    {
                        var attrValue = myDBObject.GetAttribute(myTypeAttribute.UUID, myType, _DBContext);
                        attributeValue = ResolveAttributeValue(myTypeAttribute, attrValue, myDepth - 1, myLevelKey, myDBObject, reference, myUsingGraph);
                        return true;
                    }
                    else
                    {
                        attributeValue = GetNotResolvedReferenceAttributeValue(myDBObject, myTypeAttribute, myType, myLevelKey, myUsingGraph, _DBContext);
                        return true;
                    }
                }

                #endregion

                #endregion

            }

            attributeValue = null;
            return false;
        }

        #region GetNotResolved edges

        private Edge GetNotResolvedReferenceAttributeValue(IReferenceEdge referenceEdge, TypeAttribute typeAttribute, GraphDBType graphDBType, DBContext _DBContext)
        {

            if (referenceEdge is ASetOfReferencesEdgeType)
            {
                return new Edge((referenceEdge as ASetOfReferencesEdgeType).GetReadouts((uuid) => GenerateUndefindedDBReadout(uuid, graphDBType)), graphDBType.Name);
            }
            else if (referenceEdge is ASingleReferenceEdgeType)
            {
                return new Edge((referenceEdge as ASingleReferenceEdgeType).GetReadout((uuid) => GenerateUndefindedDBReadout(uuid, graphDBType)), graphDBType.Name);
            }
            else
            {
                throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
            }
        }

        private Edge GetNotResolvedReferenceAttributeValue(DBObjectStream myDBObject, TypeAttribute myTypeAttribute, GraphDBType myType, EdgeList currentEdgeList, Boolean myUsingGraph, DBContext _DBContext)
        {
            IObject attrValue = null;

            if (myUsingGraph)
            {
                var interestingLevelKey = new LevelKey((currentEdgeList + new EdgeKey(myType.UUID, myTypeAttribute.UUID)).Edges, _DBContext.DBTypeManager);

                var interestingUUIDs = _ExpressionGraph.SelectUUIDs(interestingLevelKey, myDBObject);

                attrValue = ((IReferenceEdge)myDBObject.GetAttribute(myTypeAttribute.UUID, myType, _DBContext)).GetNewInstance(interestingUUIDs, myType.UUID);
            }
            else
            {
                attrValue = myDBObject.GetAttribute(myTypeAttribute.UUID, myType, _DBContext);
            }

            if (attrValue == null)
            {
                return null;
            }
            else if (!(attrValue is IReferenceEdge))
            {
                throw new GraphDBException(new Error_InvalidEdgeType(attrValue.GetType(), typeof(IReferenceEdge)));
            }

            List<DBObjectReadout> readouts = new List<DBObjectReadout>();
            var typeName = myTypeAttribute.GetDBType(_DBContext.DBTypeManager).Name;

            if (attrValue is ASetOfReferencesEdgeType)
            {
                return new Edge((attrValue as ASetOfReferencesEdgeType).GetReadouts((uuid) => GenerateUndefindedDBReadout(uuid, myTypeAttribute.GetDBType(_DBContext.DBTypeManager))), typeName);
            }
            else if (attrValue is ASingleReferenceEdgeType)
            {
                return new Edge((attrValue as ASingleReferenceEdgeType).GetReadout((uuid) => GenerateUndefindedDBReadout(uuid, myTypeAttribute.GetDBType(_DBContext.DBTypeManager))), typeName);
            }
            else
            {
                throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
            }
        }

        private DBObjectReadout GenerateUndefindedDBReadout(ObjectUUID reference, GraphDBType myType)
        {
            var specialAttributes = new Dictionary<string, object>();
            specialAttributes.Add(SpecialTypeAttribute_UUID.AttributeName, reference);
            specialAttributes.Add(SpecialTypeAttribute_TYPE.AttributeName, myType);

            return new DBObjectReadout(specialAttributes);
        }

        private Edge GetNotResolvedBackwardEdgeReferenceAttributeValue(DBObjectStream myDBObject, TypeAttribute myTypeAttribute, EdgeKey edgeKey, EdgeList currentEdgeList, Boolean myUsingGraph, DBContext _DBContext)
        {
            IObject attrValue = null;

            if (myUsingGraph)
            {
                var interestingLevelKey = new LevelKey((currentEdgeList + new EdgeKey(myTypeAttribute.RelatedGraphDBTypeUUID, myTypeAttribute.UUID)).Edges, _DBContext.DBTypeManager);

                attrValue = new EdgeTypeSetOfReferences(_ExpressionGraph.SelectUUIDs(interestingLevelKey, myDBObject), myTypeAttribute.DBTypeUUID);
            }
            else
            {
                var attrValueException = myDBObject.GetBackwardEdges(edgeKey, _DBContext, _DBObjectCache, myTypeAttribute.GetDBType(_DBContext.DBTypeManager));
                if (attrValueException.Failed())
                {
                    throw new GraphDBException(attrValueException.Errors);
                }

                attrValue = attrValueException.Value;
            }

            if (attrValue == null)
            {
                return null;
            }
            else if (!(attrValue is IReferenceEdge))
            {
                throw new GraphDBException(new Error_InvalidEdgeType(attrValue.GetType(), typeof(IReferenceEdge)));
            }

            List<DBObjectReadout> readouts = new List<DBObjectReadout>();
            var typeName = _DBContext.DBTypeManager.GetTypeByUUID(edgeKey.TypeUUID).Name;
            foreach (var reference in (attrValue as IReferenceEdge).GetAllReferenceIDs())
            {
                var specialAttributes = new Dictionary<string, object>();
                specialAttributes.Add(SpecialTypeAttribute_UUID.AttributeName, reference);
                specialAttributes.Add(SpecialTypeAttribute_TYPE.AttributeName, typeName);

                readouts.Add(new DBObjectReadout(specialAttributes));
            }


            return new Edge(readouts, _DBContext.DBTypeManager.GetTypeAttributeByEdge(edgeKey).GetDBType(_DBContext.DBTypeManager).Name);
        }
        
        #endregion

        /// <summary>
        /// Extracts the attribute from <paramref name="myDBObject"/>.
        /// </summary>
        /// <param name="myType"></param>
        /// <param name="myTypeAttribute"></param>
        /// <param name="myDBObject"></param>
        /// <param name="myLevelKey"></param>
        /// <returns></returns>
        private Object GetAttributeValue(GraphDBType myType, TypeAttribute myTypeAttribute, DBObjectStream myDBObject, EdgeList myLevelKey)
        {
            if (myTypeAttribute.TypeCharacteristics.IsBackwardEdge)
            {

                #region IsBackwardEdge

                EdgeKey edgeKey = myTypeAttribute.BackwardEdgeDefinition;
                var contBackwardExcept = myDBObject.ContainsBackwardEdge(edgeKey, _DBContext, _DBObjectCache, myType);

                if (contBackwardExcept.Failed())
                    throw new GraphDBException(contBackwardExcept.Errors);

                if (contBackwardExcept.Value)
                {
                    var getBackwardExcept = myDBObject.GetBackwardEdges(edgeKey, _DBContext, _DBObjectCache, myTypeAttribute.GetDBType(_DBContext.DBTypeManager));

                    if (getBackwardExcept.Failed())
                        throw new GraphDBException(getBackwardExcept.Errors);

                    return getBackwardExcept.Value;
                }

                #endregion

            }
            else if (myDBObject.HasAttribute(myTypeAttribute.UUID, myType))
            {

                #region ELSE (!IsBackwardEdge)

                return myDBObject.GetAttribute(myTypeAttribute.UUID, myType, _DBContext);

                #endregion

            }

            return null;
        }

        /// <summary>
        /// Resolves an attribute 
        /// </summary>
        /// <param name="attrDefinition">The TypeAttribute</param>
        /// <param name="attributeValue">The attribute value, either a AListReferenceEdgeType or ASingleEdgeType or a basic object (Int, String, ...)</param>
        /// <param name="myDepth">The current depth defined by a setting or in the select</param>
        /// <param name="currentLvl">The current level (for recursive resolve)</param>
        /// <returns>List&lt;DBObjectReadout&gt; for user defined list attributes; DBObjectReadout for reference attributes, Object for basic type values </returns>
        private object ResolveAttributeValue(TypeAttribute attrDefinition, object attributeValue, Int64 myDepth, EdgeList myEdgeList, DBObjectStream mySourceDBObject, String reference, Boolean myUsingGraph)
        {

            #region attributeValue is not a reference type just return the value

            if (!((attributeValue is ASetOfReferencesEdgeType) || (attributeValue is ASingleReferenceEdgeType)))
            {
                return attributeValue;
            }

            #endregion

            #region get typeOfAttribute

            var typeOfAttribute = attrDefinition.GetDBType(_DBContext.DBTypeManager);

            // For backwardEdges, the baseType is the type of the DBObjects!
            if (attrDefinition.TypeCharacteristics.IsBackwardEdge)
                typeOfAttribute = _DBContext.DBTypeManager.GetTypeAttributeByEdge(attrDefinition.BackwardEdgeDefinition).GetRelatedType(_DBContext.DBTypeManager);

            #endregion

            #region Get levelKey and UsingGraph

            if (!(attrDefinition is ASpecialTypeAttribute))
            {
                if (myEdgeList.Level == 0)
                {
                    myEdgeList = new EdgeList(new EdgeKey(attrDefinition.RelatedGraphDBTypeUUID, attrDefinition.UUID));
                }
                else
                {
                    myEdgeList += new EdgeKey(attrDefinition.RelatedGraphDBTypeUUID, attrDefinition.UUID);
                }
            }

            // at some deeper level we could get into graph independend results. From this time, we can use the GUID index rather than asking the graph all the time
            if (myUsingGraph)
            {
                myUsingGraph = _ExpressionGraph.IsGraphRelevant(new LevelKey(myEdgeList.Edges, _DBContext.DBTypeManager), mySourceDBObject);
            }

            #endregion

            if (attributeValue is ASetOfReferencesEdgeType)
            {

                #region SetReference attribute

                IEnumerable<DBObjectReadout> resultList = null;

                var edge = ((ASetOfReferencesEdgeType)attributeValue);

                if (myUsingGraph)
                {
                    var dbos = _ExpressionGraph.Select(new LevelKey(myEdgeList.Edges, _DBContext.DBTypeManager), mySourceDBObject, true);

                    resultList = GetReadouts(typeOfAttribute, edge, dbos, myDepth, myEdgeList, reference, myUsingGraph);
                }
                else
                {
                    resultList = GetReadouts(typeOfAttribute, edge, myDepth, myEdgeList, reference, myUsingGraph);
                }

                return new Edge(resultList, typeOfAttribute.Name);

                #endregion

            }
            else if (attributeValue is ASingleReferenceEdgeType)
            {

                #region Single reference

                attributeValue = new Edge((attributeValue as ASingleReferenceEdgeType).GetReadout(a =>
                {
                    var dbStream = _DBObjectCache.LoadDBObjectStream(typeOfAttribute, a);
                    if (dbStream.Failed())
                    {
                        throw new GraphDBException(dbStream.Errors);
                    }

                    return new DBObjectReadout(GetAllSelectedAttributesFromDBO(dbStream.Value, typeOfAttribute, myDepth, myEdgeList, reference, myUsingGraph));
                }), typeOfAttribute.Name);

                return attributeValue;

                #endregion

            }
            else
            {
                throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
            }
        }

        /// <summary>
        /// Gets all selected attributes of an <paramref name="aDBObject"/> or on asterisk all attributes
        /// </summary>
        /// <param name="aDBObject"></param>
        /// <param name="typeOfAttribute"></param>
        /// <param name="myDepth"></param>
        /// <param name="myLevelKey"></param>
        /// <param name="reference"></param>
        /// <param name="myUsingGraph"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetAllSelectedAttributesFromDBO(DBObjectStream myDBObject, GraphDBType myDBType, Int64 myDepth, EdgeList myLevelKey, String myReference, Boolean myUsingGraph)
        {
            Dictionary<string, object> Attributes = new Dictionary<string, object>();
            Int64 Depth;

            var minDepth = 0;

            var attributeSelections = getAttributeSelections(myReference, myDBType, myLevelKey);

            if (attributeSelections.IsNullOrEmpty())// && myLevelKey.Level > 0)
            {

                #region Get all attributes from the DBO if nothing special was selected

                AddAttributesByDBO(ref Attributes, myDBType, myDBObject, myDepth, myLevelKey, myReference, myUsingGraph, TypesOfSelect.Asterisk);

                #endregion

            }
            else
            {

                foreach (var attrSel in attributeSelections)
                {

                    #region extract the selected infos

                    //myDepth = attrSel.EdgeList.Level;

                    if (attrSel.Selection != TypesOfSelect.None)
                    {

                        #region Asterisk (*), Rhomb (#), Minus (-), Ad (@) selection

                        Depth = GetDepth(myDepth, 0, myDBType);

                        AddAttributesByDBO(ref Attributes, myDBType, myDBObject, Depth, myLevelKey, myReference, myUsingGraph, attrSel.Selection, attrSel.TypeID);

                        #endregion

                        continue;

                    }

                    String alias = String.Empty;

                    if (attrSel.Element == null)
                    {
                        alias = (attrSel.Alias == null) ? attrSel.RelatedIDChainDefinition.UndefinedAttribute : attrSel.Alias;
                    }
                    else
                    {
                        alias = (attrSel.Alias == null) ? (attrSel.Element as TypeAttribute).Name : attrSel.Alias;
                    }

                    if (Attributes.ContainsKey(alias))
                    {
                        // This is a bug in the attributeSelections add method. No attribute should be in the selected list twice. 
                        // If one attribute was selected more than one, these information will be stored in the next level.
                        continue;
                    }

                    if (attrSel is SelectionElementFunction)
                    {

                        #region Select a function

                        var selectionElementFunction = (attrSel as SelectionElementFunction);

                        #region Get the CallingObject

                        IObject callingObject = null;
                        var typeOfDBObjects = selectionElementFunction.Element.GetDBType(_DBContext.DBTypeManager);

                        if (myUsingGraph)
                        {
                            myUsingGraph = _ExpressionGraph.IsGraphRelevant(new LevelKey((myLevelKey + new EdgeKey(selectionElementFunction.Element)).Edges, _DBContext.DBTypeManager), myDBObject);
                        }

                        if (myUsingGraph && (typeOfDBObjects.IsUserDefined || typeOfDBObjects.IsBackwardEdge))
                        {
                            var edge = GetAttributeValue(myDBType, selectionElementFunction.Element, myDBObject, myLevelKey);
                            if (edge is IReferenceEdge)
                                callingObject = (edge as IReferenceEdge).GetNewInstance(_ExpressionGraph.SelectUUIDs(new LevelKey((myLevelKey + new EdgeKey(selectionElementFunction.Element)).Edges, _DBContext.DBTypeManager), myDBObject, true), typeOfDBObjects.UUID);
                            else if (edge == null)
                                callingObject = null;
                            else
                                throw new GraphDBException(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
                        }
                        else
                        {
                            callingObject = GetAttributeValue(myDBType, selectionElementFunction.Element, myDBObject, myLevelKey) as IObject;
                        }

                        #endregion

                        #region Execute the function

                        var res = ExecuteFunction(selectionElementFunction, myDBObject, callingObject, myDepth, myReference, myDBType, myLevelKey, myUsingGraph);

                        if (res == null)
                        {
                            continue;
                        }
                        if (res.Failed())
                        {
                            throw new GraphDBException(res.Errors);
                        }

                        #endregion

                        if (res.Value.Value is IReferenceEdge)
                        {
                            //minDepth = attrSel.EdgeList.Level + 1; // depth should be at least the depth of the selected element
                            minDepth = (attrSel.RelatedIDChainDefinition != null) ? attrSel.RelatedIDChainDefinition.Edges.Count - 1 : 0;
                            myUsingGraph = false;

                            Depth = GetDepth(myDepth, minDepth, myDBType);

                            if (Depth > myLevelKey.Level || getAttributeSelections(myReference, myDBType, myLevelKey + new EdgeKey((attrSel as SelectionElementFunction).Element)).IsNotNullOrEmpty())
                            {

                                myUsingGraph = false;

                                #region Resolve DBReferences

                                Attributes.Add(alias,
                                    ResolveAttributeValue(((FuncParameter)res.Value).TypeAttribute, ((FuncParameter)res.Value).Value, Depth - 1, myLevelKey, myDBObject, myReference, myUsingGraph));

                                #endregion

                            }
                            else
                            {

                                Attributes.Add(alias, GetNotResolvedReferenceAttributeValue(res.Value.Value as IReferenceEdge, res.Value.TypeAttribute, res.Value.TypeAttribute.GetDBType(_DBContext.DBTypeManager), _DBContext));

                            }

                        }
                        else
                        {
                            Attributes.Add(alias, ((FuncParameter)res.Value).Value.GetReadoutValue());
                        }

                        #endregion

                    }
                    else if (attrSel.Element == null)
                    {
                        #region undefined attribute selection

                        var undef_alias = attrSel.Alias;

                        if (!Attributes.ContainsKey(undef_alias))
                        {
                            Object attrValue = null;                            

                            if (GetAttributeValueAndResolve(myDBType, null, myDBObject, 0, myLevelKey, myReference, myUsingGraph, out attrValue, undef_alias))
                            {
                                Attributes.Add(undef_alias, attrValue);
                            }                            
                        }                        

                        #endregion
                    }
                    else
                    {

                        #region Attribute selection

                        minDepth = (attrSel.RelatedIDChainDefinition != null) ? attrSel.RelatedIDChainDefinition.Edges.Count - 1 : 0;

                        //var alias = (attrSel.Alias == null) ? (attrSel.Element as TypeAttribute).Name : attrSel.Alias;

                        if (myLevelKey.Level > 0 && !(attrSel.Element is ASpecialTypeAttribute)) // use the related type instead
                            Depth = GetDepth(myDepth, minDepth, (attrSel.Element as TypeAttribute).GetRelatedType(_DBContext.DBTypeManager), (attrSel.Element as TypeAttribute));
                        else
                            Depth = GetDepth(myDepth, minDepth, myDBType, (attrSel.Element as TypeAttribute));

                        Object attrValue = null;

                        if (!Attributes.ContainsKey(alias))
                        {
                            if (GetAttributeValueAndResolve(myDBType, attrSel.Element as TypeAttribute, myDBObject, Depth, myLevelKey, myReference, myUsingGraph, out attrValue))
                            {
                                Attributes.Add(alias, attrValue);
                            }
                        }

                        #endregion

                    }

                    #endregion

                }

            }

            return Attributes;
        }

        #endregion

        #region GetResult

        /// <summary>
        /// This will return the result of all examined DBOs, including calculated aggregates and groupings.
        /// The Value of the returned GraphResult is of type IEnumerable&lt;DBObjectReadout&gt;
        /// </summary>
        /// <param name="typeNode">The current typeNode</param>
        /// <param name="dbObjectReadouts">The precalculated return objects</param>
        /// <returns>A GraphResult including the ResultType with the occured errors.</returns>
        public IEnumerable<DBObjectReadout> GetResult(String myReference, GraphDBType myReferencedDBType, IEnumerable<DBObjectReadout> dbObjectReadouts, Boolean isWhereExpressionDependent = false)
        {

            if (!_Aggregates.IsNullOrEmpty() && _Groupings.IsNullOrEmpty())
            //if (_AggregateNodes != null && _GroupingAttr == null)

            #region We have only aggregates!

            {

                var Attributes = new Dictionary<String, Object>();

                foreach (var aggr in _Aggregates)
                {

                    if (aggr.IndexAggregate != null && !isWhereExpressionDependent)
                    {
                        #region Aggregate on index and no where expression

                        var res = aggr.Aggregate.Aggregate(aggr.IndexAggregate, aggr.Parameter.Reference.Item2, _DBContext, _DBObjectCache, _SessionToken);
                        if (res.Failed())
                        {
                            throw new GraphDBException(res.Errors);
                        }
                        Attributes.Add(aggr.Alias, res.Value);

                        #endregion
                    }
                    else
                    {
                        #region Aggregate on the dbObjectReadouts created by the select run

                        var res = aggr.Aggregate.Aggregate(dbObjectReadouts, aggr.Parameter.LastAttribute, _DBContext, _DBObjectCache, _SessionToken);
                        if (res.Failed())
                        {
                            throw new GraphDBException(res.Errors);
                        }

                        Attributes.Add(aggr.Alias, res.Value);

                        #endregion
                    }

                }

                //_DBOs = new IEnumerable<DBObjectReadout>();
                //_DBOs.Add(new DBObjectReadoutGroup(Attributes, new DBObjectSystemData()));

                //yield return new DBObjectReadoutGroup(Attributes);
                yield return new DBObjectReadout(Attributes);

            }

            #endregion

            else //if (_AggregateNodes != null || _GroupingAttr != null)
                if (!_Aggregates.IsNullOrEmpty() || !_Groupings.IsNullOrEmpty())
                #region We have aggregates and/or groupings
                {

                    foreach (var keyValPair in _GroupingStructure[myReference].GroupsAndAggregates)
                    {

                        var Attributes = new Dictionary<string, object>();

                        foreach (var groupKeyValPair in keyValPair.Key.Values)
                        {

                            TypeAttribute theAttribute = null;

                            //if we have an undefined attribute
                            if (groupKeyValPair.Key.AttributeUUID != null)
                            {
                                theAttribute = _GroupingStructure[myReference].DBTypeStream.GetTypeAttributeByUUID(groupKeyValPair.Key.AttributeUUID);
                            }
                            
                            if (theAttribute != null)
                            {
                                Attributes.Add(theAttribute.Name, groupKeyValPair.Value.GetReadoutValue());
                            }
                            else
                            {   
                                var Setting = _DBContext.DBSettingsManager.GetSetting(new UUID(groupKeyValPair.Key.ToString()), _DBContext, TypesSettingScope.TYPE, _GroupingStructure[myReference].DBTypeStream).Value;

                                if (Setting != null)
                                {
                                    Attributes.Add(Setting.Name, groupKeyValPair.Value.GetReadoutValue());
                                }
                                else
                                {
                                    #region undefined attributes
                                    
                                    Attributes.Add(groupKeyValPair.Key.AttributeName, groupKeyValPair.Value.GetReadoutValue());

                                    #endregion
                                }
                            }
                        }

                        #region Calculate aggregate

                        if (!_Aggregates.IsNullOrEmpty())
                        {
                            foreach (var aggr in _Aggregates)
                            {
                                var res = aggr.Aggregate.Aggregate(keyValPair.Value, aggr.Parameter.LastAttribute, _DBContext, _DBObjectCache, _SessionToken);
                                
                                if (res.Failed())
                                {
                                    //return new Exceptional<IEnumerable<DBObjectReadout>>(res.Errors);
                                }
                                Attributes.Add(aggr.Alias, res.Value);
                            }
                        }

                        #endregion

                        var _DBObjectReadoutGroup = new DBObjectReadoutGroup(Attributes, keyValPair.Value);
                        //_DBObjectReadoutGroup.GrouppedVertices = keyValPair.Value;

                        #region Check for having expressions and evaluate them

                        if (_HavingExpression == null)
                        {
                            //_DBOs.Add(readoutGroup);
                            yield return _DBObjectReadoutGroup;
                        }
                        else
                        {

                            var res = _HavingExpression.IsSatisfyHaving(_DBObjectReadoutGroup, _DBContext);
                            if (res.Failed())
                                throw new GraphDBException(res.Errors);
                            else if (res.Value)
                                yield return _DBObjectReadoutGroup;
                        }

                        #endregion

                    }

                }

                #endregion

                //return _DBOs;
                else
                {
                    foreach (var dbo in dbObjectReadouts)
                        yield return dbo;
                }

        }

        public IEnumerable<DBObjectReadout> GetTypeIndependendResult()
        {

            //_DBOs = new IEnumerable<DBObjectReadout>();
            var Attributes = new Dictionary<string, object>();

            #region Go through all _SelectionElementsTypeIndependend

            foreach (var selection in _SelectionElementsTypeIndependend)
            {
                if (selection is SelectionElementFunction)
                {
                    var func = ((SelectionElementFunction)selection);
                    Exceptional<FuncParameter> funcResult = null;
                    var alias = func.Alias;

                    while (func != null)
                    {
                        funcResult = func.Function.Execute(null, null, null, _DBContext, _DBObjectCache);
                        if (funcResult.Success())
                        {

                            if (funcResult.Value.Value == null)
                            {
                                break; // no result for this object because of not set attribute value
                            }
                            else
                            {
                                func = func.FollowingFunction;
                                if (func != null)
                                {
                                    func.Function.Function.CallingObject = funcResult.Value.Value;
                                }
                            }
                            
                        }
                        else
                        {
                            //return new Exceptional<IEnumerable<DBObjectReadout>>(res.Errors);
                            throw new GraphDBException(funcResult.Errors);
                        }
                    }

                    if (funcResult.Value.Value == null)
                    {
                        continue; // no result for this object because of not set attribute value
                    }

                    Attributes.Add(alias, ((FuncParameter)funcResult.Value).Value.GetReadoutValue());
                }
            }

            #endregion

            if (!Attributes.IsNullOrEmpty())
            {
                DBObjectReadout tempROObject = new DBObjectReadout(Attributes);
                //_DBOs.Add(tempROObject);
                yield return tempROObject;
            }

        }

        #endregion

        #region GetSelectedAttributesList

        public Dictionary<String, String> GetSelectedAttributesList()
        {

            var retVal = new Dictionary<String, String>();

            foreach (var selType in _Selections)
            {
                foreach (var sel in selType.Value)
                {
                    foreach (var selElem in sel.Value)
                    {
                        if (selElem.Element != null && !selElem.IsGroupedOrAggregated)
                        {
                            if (!retVal.ContainsKey(selElem.Element.Name))
                                retVal.Add(selElem.Element.Name, selElem.Alias);
                        }
                        else if (selElem.Selection != TypesOfSelect.None)
                        {                            
                            var attributes = _DBContext.DBTypeManager.GetTypeByUUID(sel.Key.Edges[0].TypeUUID).GetAllAttributes(_DBContext);

                            switch(selElem.Selection)
                            {   
                                case TypesOfSelect.Minus:
                                    foreach (var attr in attributes)
                                    {
                                        if (!retVal.ContainsKey(attr.Name) && (attr.IsBackwardEdge || attr.GetDBType(_DBContext.DBTypeManager).IsUserDefined))
                                        {
                                            retVal.Add(attr.Name, attr.Name);
                                        }
                                    }

                                    break;

                                case TypesOfSelect.Rhomb:
                                    foreach (var attr in attributes)
                                    {
                                        if (!retVal.ContainsKey(attr.Name) && (!attr.GetDBType(_DBContext.DBTypeManager).IsUserDefined) && (attr.KindOfType != KindsOfType.SpecialAttribute))
                                        {
                                            retVal.Add(attr.Name, attr.Name);
                                        }
                                    }
                                    break;

                                case TypesOfSelect.Ad:
                                    foreach (var attr in attributes)
                                    {
                                        if (!retVal.ContainsKey(attr.Name))
                                        {
                                            if (attr.GetDBType(_DBContext.DBTypeManager).UUID == selElem.TypeID)
                                            {
                                                retVal.Add(attr.Name, attr.Name);
                                            }
                                        }
                                    }
                                    break;

                                case TypesOfSelect.Gt:
                                    foreach (var attr in attributes)
                                    {
                                        if (!retVal.ContainsKey(attr.Name) && (attr.GetDBType(_DBContext.DBTypeManager).IsUserDefined) && (!attr.IsBackwardEdge) && (attr.KindOfType != KindsOfType.SpecialAttribute))
                                        {
                                            retVal.Add(attr.Name, attr.Name);
                                        }
                                    }
                                    break;

                                case TypesOfSelect.Lt:
                                    foreach (var attr in attributes)
                                    {
                                        if (!retVal.ContainsKey(attr.Name) && (attr.GetDBType(_DBContext.DBTypeManager).IsUserDefined) && (attr.IsBackwardEdge) && (attr.KindOfType != KindsOfType.SpecialAttribute))
                                        {
                                            retVal.Add(attr.Name, attr.Name);
                                        }
                                    }
                                    break;
                                
                                case TypesOfSelect.Asterisk:
                                    foreach (var attr in attributes)
                                    {
                                        if (!retVal.ContainsKey(attr.Name))
                                            retVal.Add(attr.Name, attr.Name);
                                    }
                                    break;                               
                            }
                        }

                    }
                }
            }

            #region Aggregates

            if (!_Aggregates.IsNullOrEmpty())
            {
                foreach (var selElem in _Aggregates)
                {
                    retVal.Add(selElem.AggregateDefinition.ChainPartAggregateDefinition.SourceParsedString, selElem.Alias);
                }
            }


            #endregion

            return retVal;
        }

        #endregion

    }
}
