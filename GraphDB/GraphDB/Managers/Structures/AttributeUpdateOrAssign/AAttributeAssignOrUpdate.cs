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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sones.GraphDB.Structures.Enums;

using sones.Lib.ErrorHandling;

using sones.GraphDB.ObjectManagement;
using sones.GraphDB.TypeManagement;
using sones.GraphDB.Structures.EdgeTypes;
using sones.GraphDB.Exceptions;
using sones.GraphDB.Errors;
using sones.GraphDB.TypeManagement.BasicTypes;
using sones.GraphFS.DataStructures;
using sones.GraphDB.TypeManagement;

#endregion

namespace sones.GraphDB.Managers.Structures
{

    public abstract class AAttributeAssignOrUpdate : AAttributeAssignOrUpdateOrRemove
    {

        #region Ctors

        public AAttributeAssignOrUpdate() { }

        public AAttributeAssignOrUpdate(IDChainDefinition myIDChainDefinition)
        {
            AttributeIDChain = myIDChainDefinition;
        }

        #endregion

        #region abstract GetValueForAttribute

        public abstract Exceptional<IObject> GetValueForAttribute(DBObjectStream aDBObject, DBContext dbContext, GraphDBType myGraphDBType);

        #endregion

        #region Update

        public override Exceptional<Dictionary<string, Tuple<TypeAttribute, IObject>>> Update(DBContext myDBContext, DBObjectStream myDBObjectStream, GraphDBType myGraphDBType)
        {

            Dictionary<String, Tuple<TypeAttribute, IObject>> attrsForResult = new Dictionary<String, Tuple<TypeAttribute, IObject>>();

            if (base.AttributeIDChain.IsUndefinedAttribute)
            {

                #region Undefined attribute

                var applyResult = ApplyAssignUndefinedAttribute(myDBContext, myDBObjectStream, myGraphDBType);

                if (applyResult.Failed())
                {
                    return new Exceptional<Dictionary<string, Tuple<TypeAttribute, IObject>>>(applyResult);
                }

                if (applyResult.Value != null)
                {
                    //sthChanged = true;

                    #region Add to queryResult

                    attrsForResult.Add(applyResult.Value.Item1, new Tuple<TypeAttribute, IObject>(applyResult.Value.Item2, applyResult.Value.Item3));

                    #endregion
                
                }

                #endregion

            }

            else
            {

                #region Usual attribute

                var applyResult = ApplyAssignAttribute(this, myDBContext, myDBObjectStream, myGraphDBType);

                if (applyResult.Failed())
                {
                    return new Exceptional<Dictionary<string, Tuple<TypeAttribute, IObject>>>(applyResult);
                }

                if (applyResult.Value != null)
                {
                    //sthChanged = true;

                    #region Add to queryResult

                    attrsForResult.Add(applyResult.Value.Item1, new Tuple<TypeAttribute, IObject>(applyResult.Value.Item2, applyResult.Value.Item3));

                    #endregion
                
                }

                #endregion
            
            }

            return new Exceptional<Dictionary<string, Tuple<TypeAttribute, IObject>>>(attrsForResult);

        }


        #region override AAttributeAssignOrUpdateOrRemove.Update

        private Exceptional<Tuple<String, TypeAttribute, IObject>> ApplyAssignUndefinedAttribute(DBContext myDBContext, DBObjectStream myDBObjectStream, GraphDBType myGraphDBType)
        {
            Dictionary<String, Tuple<TypeAttribute, IObject>> attrsForResult = new Dictionary<String, Tuple<TypeAttribute, IObject>>();

            #region undefined attributes

            var newValue = GetValueForAttribute(myDBObjectStream, myDBContext, myGraphDBType);
            if (newValue.Failed())
            {
                return new Exceptional<Tuple<string, TypeAttribute, IObject>>(newValue);
            }

            if (myDBObjectStream.ContainsUndefinedAttribute(AttributeIDChain.UndefinedAttribute, myDBContext.DBObjectManager))
            {
                var removeResult =myDBObjectStream.RemoveUndefinedAttribute(AttributeIDChain.UndefinedAttribute, myDBContext.DBObjectManager);
                if (removeResult.Failed())
                {
                    return new Exceptional<Tuple<string, TypeAttribute, IObject>>(removeResult);
                }
            }

            //TODO: change this to a more handling thing than KeyValuePair
            var addExcept = myDBContext.DBObjectManager.AddUndefinedAttribute(AttributeIDChain.UndefinedAttribute, newValue.Value, myDBObjectStream);

            if (addExcept.Failed())
            {
                return new Exceptional<Tuple<String, TypeAttribute, IObject>>(addExcept);
            }

            //sthChanged = true;

            attrsForResult.Add(AttributeIDChain.UndefinedAttribute, new Tuple<TypeAttribute, IObject>(null, newValue.Value));

            #endregion

            return new Exceptional<Tuple<String, TypeAttribute, IObject>>(new Tuple<String, TypeAttribute, IObject>(AttributeIDChain.UndefinedAttribute, AttributeIDChain.LastAttribute, newValue.Value));

        }

        #endregion

        internal Exceptional<Tuple<String, TypeAttribute, IObject>> ApplyAssignAttribute(AAttributeAssignOrUpdate myAAttributeAssign, DBContext myDBContext, DBObjectStream myDBObject, GraphDBType myGraphDBType)
        {

            System.Diagnostics.Debug.Assert(myAAttributeAssign != null);

            //get value for assignement
            var aValue = myAAttributeAssign.GetValueForAttribute(myDBObject, myDBContext, myGraphDBType);
            if (aValue.Failed())
            {
                return new Exceptional<Tuple<String, TypeAttribute, IObject>>(aValue);
            }

            object oldValue = null;
            IObject newValue = aValue.Value;

            if (myDBObject.HasAttribute(myAAttributeAssign.AttributeIDChain.LastAttribute.UUID, myGraphDBType))
            {

                #region Update the value because it already exists

                oldValue = myDBObject.GetAttribute(myAAttributeAssign.AttributeIDChain.LastAttribute.UUID);

                switch (myAAttributeAssign.AttributeIDChain.LastAttribute.KindOfType)
                {
                    case KindsOfType.SetOfReferences:
                        var typeOfCollection = ((AttributeAssignOrUpdateList)myAAttributeAssign).CollectionDefinition.CollectionType;

                        if (typeOfCollection == CollectionType.List)
                            return new Exceptional<Tuple<String, TypeAttribute, IObject>>(new Error_InvalidAssignOfSet(myAAttributeAssign.AttributeIDChain.LastAttribute.Name));

                        var removeRefExcept = RemoveBackwardEdgesOnReferences(myAAttributeAssign, (IReferenceEdge)oldValue, myDBObject, myDBContext);

                        if (!removeRefExcept.Success())
                            return new Exceptional<Tuple<String, TypeAttribute, IObject>>(removeRefExcept.IErrors.First());

                        newValue = (ASetOfReferencesEdgeType)newValue;
                        break;

                    case KindsOfType.SetOfNoneReferences:
                    case KindsOfType.ListOfNoneReferences:
                        newValue = (IBaseEdge)newValue;
                        break;

                    case KindsOfType.SingleNoneReference:
                        if (!(oldValue as ADBBaseObject).IsValidValue((newValue as ADBBaseObject).Value))
                        {
                            return new Exceptional<Tuple<string, TypeAttribute, IObject>>(new Error_DataTypeDoesNotMatch((oldValue as ADBBaseObject).ObjectName, (newValue as ADBBaseObject).ObjectName));
                        }
                        newValue = (oldValue as ADBBaseObject).Clone((newValue as ADBBaseObject).Value);
                        break;

                    case KindsOfType.SingleReference:
                        if (newValue is ASingleReferenceEdgeType)
                        {
                            removeRefExcept = RemoveBackwardEdgesOnReferences(myAAttributeAssign, (IReferenceEdge)oldValue, myDBObject, myDBContext);

                            if (!removeRefExcept.Success())
                                return new Exceptional<Tuple<String, TypeAttribute, IObject>>(removeRefExcept.IErrors.First());

                            ((ASingleReferenceEdgeType)oldValue).Merge((ASingleReferenceEdgeType)newValue);
                            newValue = (ASingleReferenceEdgeType)oldValue;
                        }
                        break;

                    default:
                        return new Exceptional<Tuple<String, TypeAttribute, IObject>>(new Error_NotImplemented(new System.Diagnostics.StackTrace(true)));
                }

                #endregion

            }

            var alterExcept = myDBObject.AlterAttribute(myAAttributeAssign.AttributeIDChain.LastAttribute.UUID, newValue);

            if (alterExcept.Failed())
                return new Exceptional<Tuple<string, TypeAttribute, IObject>>(alterExcept);

            if (!alterExcept.Value)
            {
                myDBObject.AddAttribute(myAAttributeAssign.AttributeIDChain.LastAttribute.UUID, newValue);
            }

            #region add backward edges

            if (myAAttributeAssign.AttributeIDChain.LastAttribute.GetDBType(myDBContext.DBTypeManager).IsUserDefined)
            {
                Dictionary<AttributeUUID, IObject> userdefinedAttributes = new Dictionary<AttributeUUID, IObject>();
                userdefinedAttributes.Add(myAAttributeAssign.AttributeIDChain.LastAttribute.UUID, newValue);

                var omm = new ObjectManipulationManager();
                var setBackEdges = omm.SetBackwardEdges(myGraphDBType, userdefinedAttributes, myDBObject.ObjectUUID, myDBContext);

                if (setBackEdges.Failed())
                    return new Exceptional<Tuple<string, TypeAttribute, IObject>>(setBackEdges);
            }

            #endregion

            return new Exceptional<Tuple<String, TypeAttribute, IObject>>(new Tuple<String, TypeAttribute, IObject>(myAAttributeAssign.AttributeIDChain.LastAttribute.Name, myAAttributeAssign.AttributeIDChain.LastAttribute, newValue));
        }

        protected Exceptional<Boolean> RemoveBackwardEdgesOnReferences(AAttributeAssignOrUpdate myAAttributeAssign, IReferenceEdge myReference, DBObjectStream myDBObject, DBContext myDBContext)
        {
            foreach (var item in myReference.GetAllReferenceIDs())
            {
                var streamExcept = myDBContext.DBObjectCache.LoadDBObjectStream(myAAttributeAssign.AttributeIDChain.LastAttribute.GetDBType(myDBContext.DBTypeManager), (ObjectUUID)item);

                if (!streamExcept.Success())
                    return new Exceptional<Boolean>(streamExcept.IErrors.First());

                var removeExcept = myDBContext.DBObjectManager.RemoveBackwardEdge(streamExcept.Value, myAAttributeAssign.AttributeIDChain.LastAttribute.RelatedGraphDBTypeUUID, myAAttributeAssign.AttributeIDChain.LastAttribute.UUID, myDBObject.ObjectUUID);

                if (!removeExcept.Success())
                    return new Exceptional<Boolean>(removeExcept.IErrors.First());
            }

            return new Exceptional<Boolean>(true);
        }

        #endregion

    }

}
