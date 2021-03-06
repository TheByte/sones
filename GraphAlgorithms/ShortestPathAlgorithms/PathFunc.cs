#region usings

using System;
using System.Collections.Generic;
using System.Linq;
using sones.GraphAlgorithms.PathAlgorithm.BreadthFirstSearch;

using sones.GraphDB.Errors;
using sones.GraphDB.Exceptions;
using sones.GraphDB.Managers.Structures;
using sones.GraphDB.ObjectManagement;
using sones.GraphDB.Functions;
using sones.GraphDB.Structures.EdgeTypes;
using sones.GraphDB.TypeManagement;
using sones.GraphDB.TypeManagement.BasicTypes;
using sones.GraphFS.DataStructures;
using sones.Lib.ErrorHandling;

using sones.GraphDB.TypeManagement;
using System.Diagnostics;

#endregion

namespace sones.GraphDB
{
    public class PathFunc : ABaseFunction
    {
        public override string FunctionName
        {
            get { return "PATH"; }
        }

        #region GetDescribeOutput()

        public override String GetDescribeOutput()
        {
            return "A path algorithm.";
        }

        #endregion

        public PathFunc()
        {
            /// these are the starting edges and TypeAttribute.
            /// This is not the starting DBObject but just the content of the attribute defined by TypeAttribute!!!
            Parameters.Add(new ParameterValue("TargetDBO", new DBEdge()));
            Parameters.Add(new ParameterValue("MaxDepth", new DBInt64()));
            Parameters.Add(new ParameterValue("MaxPathLength", new DBInt64()));
            Parameters.Add(new ParameterValue("OnlyShortestPath", new DBBoolean()));
            Parameters.Add(new ParameterValue("AllPaths", new DBBoolean()));
        }

        public override bool ValidateWorkingBase(IObject workingBase, DBTypeManager typeManager)
        {
            if (workingBase is DBTypeAttribute)
            {

                var workingTypeAttribute = (workingBase as DBTypeAttribute).GetValue();
                if (workingTypeAttribute.TypeCharacteristics.IsBackwardEdge)
                {
                    return true;
                }
                else if (workingTypeAttribute.GetDBType(typeManager).IsUserDefined)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public override Exceptional<FuncParameter> ExecFunc(DBContext dbContext, params FuncParameter[] myParams)
        {
            // The edge we starting of (e.g. Friends)
            var typeAttribute = CallingAttribute;

            // The destination DBObjects, they are of the type "typeAttribute.RelatedDBType"
            var destDBOs = (myParams[0].Value as DBEdge).GetDBObjects();

            byte maxDepth = Convert.ToByte((myParams[1].Value as DBInt64).GetValue());

            byte maxPathLength = Convert.ToByte((myParams[2].Value as DBInt64).GetValue());

            //check if values incorrect
            if (maxDepth < 1 && maxPathLength < 2)
            {
                Exceptional<FuncParameter> errorResult = new Exceptional<FuncParameter>();
                IError error = new Error_InvalidFunctionParameter("maxDepth", ">= 1", maxDepth);
                errorResult.PushIError(error);
                error = new Error_InvalidFunctionParameter("maxPathLength", ">= 2", maxPathLength);
                errorResult.PushIError(error);

                return errorResult;
            }
            else if (maxDepth < 1)
            {
                return new Exceptional<FuncParameter>(new Error_InvalidFunctionParameter("maxDepth", ">= 1", maxDepth));
            }
            else if (maxPathLength < 2)
            {
                return new Exceptional<FuncParameter>(new Error_InvalidFunctionParameter("maxPathLength", ">= 2", maxPathLength));
            }

            bool onlyShortestPath = (myParams[3].Value as DBBoolean).GetValue();

            bool allPaths = (myParams[4].Value as DBBoolean).GetValue();

            if (!onlyShortestPath && !allPaths)
            {
                allPaths = true;
            }

            #region Call graph function

            if (destDBOs.Count() != 1)
                throw new GraphDBException(new Error_NotImplemented(new StackTrace(true)));

            var dbObject = destDBOs.First();
            if (dbObject.Failed())
            {
                throw new GraphDBException(dbObject.IErrors);
            }

            HashSet<List<ObjectUUID>> paths;

            if (onlyShortestPath && allPaths) //use bi-directional search for "all shortest paths"
            {
                //normal BFS
                //paths = new BreadthFirstSearch().Find(typeAttribute, typeManager, cache, mySourceDBObject, dbObject, onlyShortestPath, allPaths, maxDepth, maxPathLength);

                //bidirectional BFS
                paths = new BidirectionalBFS().Find(typeAttribute, dbContext, CallingDBObjectStream as DBObjectStream, CallingObject as IReferenceEdge, dbObject.Value, onlyShortestPath, allPaths, maxDepth, maxPathLength);
            }
            else //use uni-directional search for "shortest path and all paths up to given depth"
            {
                //normal BFS
                //paths = new BreadthFirstSearch().Find(typeAttribute, dbContext.DBTypeManager, dbContext.DBObjectCache, CallingDBObjectStream as DBObjectStream, dbObject.Value, onlyShortestPath, allPaths, maxDepth, maxPathLength);

                //bidirectional BFS
                paths = new BidirectionalBFS().Find(typeAttribute, dbContext, CallingDBObjectStream as DBObjectStream, CallingObject as IReferenceEdge, dbObject.Value, onlyShortestPath, allPaths, maxDepth, maxPathLength);
            }

            //This variable will be returned
            Exceptional<FuncParameter> pResult = new Exceptional<FuncParameter>();
            
            if (paths != null)
            {
                pResult.Value = new FuncParameter(new EdgeTypePath(paths, typeAttribute, typeAttribute.GetDBType(dbContext.DBTypeManager)), typeAttribute);
            }
            else
            {
                return new Exceptional<FuncParameter>(new FuncParameter(new EdgeTypePath(new HashSet<List<ObjectUUID>>(), typeAttribute, typeAttribute.GetDBType(dbContext.DBTypeManager)), typeAttribute));
            }

            #endregion

            return pResult;

        }
    }
}
