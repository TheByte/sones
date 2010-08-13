﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sones.GraphDB.Managers.Structures
{
    public class SelectDefinition : AExpressionDefinition
    {

        #region Properties

        /// <summary>
        /// List of selected types
        /// </summary>
        public List<TypeReferenceDefinition> TypeList { get; private set; }

        /// <summary>
        /// AExpressionDefinition, Alias
        /// </summary>
        public Dictionary<AExpressionDefinition, String> SelectedElements { get; private set; }

        /// <summary>
        /// Group by definitions
        /// </summary>
        public List<IDChainDefinition> GroupByIDs { get; private set; }

        /// <summary>
        /// Having definition
        /// </summary>
        public BinaryExpressionDefinition Having { get; private set; }

        /// <summary>
        /// OrderBy section
        /// </summary>
        public OrderByDefinition OrderByDefinition { get; private set; }

        /// <summary>
        /// Limit section
        /// </summary>
        public UInt64? Limit { get; private set; }

        /// <summary>
        /// Offset section
        /// </summary>
        public UInt64? Offset { get; private set; }

        /// <summary>
        /// Resolution depth
        /// </summary>
        public Int64 ResolutionDepth { get; private set; }

        public BinaryExpressionDefinition WhereExpressionDefinition { get; private set; }

        #endregion

        #region Ctor

        public SelectDefinition(List<TypeReferenceDefinition> myTypeList, Dictionary<AExpressionDefinition, string> mySelectedElements, BinaryExpressionDefinition myWhereExpressionDefinition,
            List<IDChainDefinition> myGroupByIDs, BinaryExpressionDefinition myHaving, ulong? myLimit, ulong? myOffset, Structures.OrderByDefinition myOrderByDefinition, long myResolutionDepth)
        {
            TypeList = myTypeList;
            SelectedElements = mySelectedElements;
            WhereExpressionDefinition = myWhereExpressionDefinition;
            GroupByIDs = myGroupByIDs;
            Having = myHaving;
            Limit = myLimit;
            Offset = myOffset;
            OrderByDefinition = myOrderByDefinition;
            ResolutionDepth = myResolutionDepth;
        }

        #endregion
    
    }
}