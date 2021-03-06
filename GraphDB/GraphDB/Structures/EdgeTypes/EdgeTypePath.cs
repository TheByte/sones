﻿/* <id name="EdgeTypePath" />
 * <copyright file="AListReferenceEdgeType.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH. All rights reserved.
 * </copyright>
 * <developer>Stefan Licht</developer>
 * <summary>Special edge to store paths and create the readout of them.</summary>
 */

#region Usings

using System;
using System.Linq;
using System.Collections.Generic;

using sones.GraphFS.DataStructures;

using sones.GraphDB.NewAPI;
using sones.GraphDB.Result;
using sones.GraphDB.TypeManagement;
using sones.GraphDB.ObjectManagement;
using sones.GraphDB.Managers.Structures;
using sones.GraphDB.TypeManagement.BasicTypes;

using sones.Lib;
using sones.Lib.ErrorHandling;
using sones.Lib.NewFastSerializer;

#endregion

namespace sones.GraphDB.Structures.EdgeTypes
{

    /// <summary>
    /// Sepecial edge to store paths and create the readout of them
    /// </summary>
    public class EdgeTypePath : ASetOfReferencesEdgeType
    {

        #region TypeCode
        public override UInt32 TypeCode { get { return 454; } }
        #endregion

        #region Data and Ctors

        private IEnumerable<List<ObjectUUID>> _Paths;
        private TypeAttribute _pathAttribute;
        private GraphDBType _typeOfObjects;

        public EdgeTypePath() { }

        public EdgeTypePath(IEnumerable<List<ObjectUUID>> myPaths, TypeAttribute myTypeAttribute, GraphDBType myTypeOfObjects)
        {
            _Paths = myPaths;
            _pathAttribute = myTypeAttribute;
            _typeOfObjects = myTypeOfObjects;
        }
        
        #endregion

        #region AEdgeType Members

        public override String EdgeTypeName { get { return "PATH"; } }

        public override EdgeTypeUUID EdgeTypeUUID { get { return new EdgeTypeUUID("1002"); } }

        public override void ApplyParams(params EdgeTypeParamDefinition[] myParams)
        {
            throw new NotImplementedException();
        }

        public override IEdgeType GetNewInstance()
        {
            throw new NotImplementedException();
        }

        public override IReferenceEdge GetNewInstance(IEnumerable<Exceptional<DBObjectStream>> iEnumerable)
        {
            throw new NotImplementedException();
        }

        public override IReferenceEdge GetNewInstance(IEnumerable<ObjectUUID> iEnumerable, TypeUUID typeUUID)
        {
            throw new NotImplementedException();
        }

        public override String GetGDDL(GraphDBType myGraphDBType)
        {
            return String.Concat(EdgeTypeName.ToUpper(), "<", myGraphDBType.Name, ">");
        }

        public override String GetDescribeOutput(GraphDBType myGraphDBType)
        {
            return GetGDDL(myGraphDBType);
        }

        #endregion

        public override IEnumerable<ObjectUUID> GetAllReferenceIDs()
        {
            throw new NotImplementedException();
        }


        public override IEnumerable<Vertex> GetVertices(Func<ObjectUUID, Vertex> GetAllAttributesFromDBO)
        {

            var result = new List<Vertex>();

            foreach (var path in _Paths)
            {
                var keyVal = new Dictionary<String, Object>();

                result.Add(resolvePath(null, path, GetAllAttributesFromDBO));
            }

            return result;

        }

        private Vertex resolvePath(Vertex myVertexPath, IEnumerable<ObjectUUID> myPathEntries, Func<ObjectUUID, Vertex> myGetAllAttributesFromVertex)
        {
            
            var _Vertex = myGetAllAttributesFromVertex(myPathEntries.First());

            if (myPathEntries.Count() > 1)
            {

                var res = resolvePath(myVertexPath, myPathEntries.Skip(1), myGetAllAttributesFromVertex);

                if (_Vertex.HasAttribute(_pathAttribute.Name))
                {

                    var listOfVertices = _Vertex.ObsoleteAttributes[_pathAttribute.Name] as List<IVertex>;
                    
                    if (listOfVertices == null)
                    {
                        _Vertex.ObsoleteAttributes[_pathAttribute.Name] = new Edge(null, new List<IVertex>() { res }) { EdgeTypeName = _typeOfObjects.Name };
                    }
                    
                    else
                    {
                        var newContent = new List<IVertex>(_Vertex.GetNeighbors(_pathAttribute.Name));
                        newContent.Add(res);

                        _Vertex.ObsoleteAttributes[_pathAttribute.Name] = new Edge(null, new List<IVertex>(newContent)) { EdgeTypeName = _typeOfObjects.Name };
                    }

                }

                else
                {

                    _Vertex.ObsoleteAttributes.Add(_pathAttribute.Name, new Edge(null, new List<IVertex>() { res }) { EdgeTypeName = _typeOfObjects.Name});
                }

            }
            else
            {
                _Vertex.ObsoleteAttributes.Remove(_pathAttribute.Name);
            }

            return _Vertex;

        }

        public override IEnumerable<Vertex> GetReadouts(Func<ObjectUUID, Vertex> GetAllAttributesFromDBO, IEnumerable<Exceptional<DBObjectStream>> myDBObjectStreams)
        {

            var result = new List<Vertex>();

            foreach (var path in _Paths)
            {
                var keyVal = new Dictionary<String, Object>();

                result.Add(resolvePath(null, path, GetAllAttributesFromDBO));
            }

            return result;

        }

        public override ObjectUUID FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<ObjectUUID> hashSet, TypeUUID typeOfDBObjects, params ADBBaseObject[] myParameters)
        {
            throw new NotImplementedException();
        }

        public override void Add(ObjectUUID myValue, TypeUUID typeOfDBObjects, params ADBBaseObject[] myParameters)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveUUID(ObjectUUID myValue)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveUUID(IEnumerable<ObjectUUID> myObjectUUIDs)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(ObjectUUID myValue)
        {
            throw new NotImplementedException();
        }

        public override ulong Count()
        {
            throw new NotImplementedException();
        }

        public override IListOrSetEdgeType GetTopAsEdge(ulong myNumOfEntries)
        {

            if (!_Paths.CountIsGreaterOrEquals((int)myNumOfEntries))
            {
                myNumOfEntries = _Paths.ULongCount();
            }

            return new EdgeTypePath(_Paths.Take((int)myNumOfEntries), _pathAttribute, _typeOfObjects);

        }

        public override void UnionWith(IListOrSetEdgeType myAListEdgeType)
        {
            throw new NotImplementedException();
        }

        public override void Distinction()
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return EdgeTypeUUID.ToString() + "," + EdgeTypeName;
        }

        public override void Serialize(ref SerializationWriter mySerializationWriter)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(ref SerializationReader mySerializationReader)
        {
            throw new NotImplementedException();
        }

        public override bool SupportsType(Type type)
        {
            return this.GetType() == type;
            throw new NotImplementedException();
        }

        public override void Serialize(SerializationWriter writer, object value)
        {
            throw new NotImplementedException();
        }

        public override object Deserialize(SerializationReader reader, Type type)
        {
            throw new NotImplementedException();
        }

        #region Equals

        public override bool Equals(object obj)
        {
            var otherEdge = obj as EdgeTypePath;
            if (otherEdge == null)
            {
                return false;
            }

            if (EdgeTypeName != otherEdge.EdgeTypeName)
            {
                return false;
            }
            if (EdgeTypeUUID != otherEdge.EdgeTypeUUID)
            {
                return false;
            }

            return true;
        }

        #endregion

        public override IEnumerable<Exceptional<DBObjectStream>> GetAllEdgeDestinations(DBObjectCache dbObjectCache)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Reference> GetAllReferences()
        {
            throw new NotImplementedException();
        }

        #region IComparable Members

        public override int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IObject

        public override ulong GetEstimatedSize()
        {
            return 1;
        }

        #endregion

    }
}
