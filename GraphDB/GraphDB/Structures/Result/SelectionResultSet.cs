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

/* <id name="sones GraphDB – Selection list element result" />
 * <copyright file="SelectionListElementResult.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH 2007-2010
 * </copyright>
 * <developer>Henning Rauch</developer>
 */

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using sones.GraphDB.TypeManagement;
using sones.Lib;

#endregion

namespace sones.GraphDB.Structures.Result
{

    /// <summary>
    /// The instances of this class represent a single selection in the selectionList.
    /// </summary>
    public class SelectionResultSet : IEnumerable<DBObjectReadout>
    {

        #region Data

        public UInt64 NumberOfAffectedObjects
        {
            get
            {
                return _Objects.ULongCount();
            }
        }

        #region Type

        private GraphDBType _Type = null;

        public GraphDBType Type
        {
            get
            {
                return _Type;
            }
        }

        #endregion

        #region Objects

        private IEnumerable<DBObjectReadout> _Objects = null;

        public IEnumerable<DBObjectReadout> Objects
        {
            get
            {
                return _Objects;
            }
        }

        #endregion

        #region SelectedAttributes

        private Dictionary<int, Dictionary<String, String>> _SelectedAttributes = null;

        /// <summary>
        /// Dictionary&lt;Level, List&lt;KeyValuePair&lt;AttributeName, Alias&gt;&gt;&gt;
        /// </summary>
        public Dictionary<int, Dictionary<String, String>> SelectedAttributes
        {
            get { return _SelectedAttributes; }
        }

        #endregion

        #region this[Int32]

        public DBObjectReadout this[Int32 elementAt]
        {
            get
            {
                return _Objects.ElementAt(elementAt);
            }
        }

        #endregion

        #endregion

        #region Constructor

        public SelectionResultSet(GraphDBType myGraphType, UInt64 numberOfAffectedObjects)
        {
            _Type = myGraphType;
        }

        public SelectionResultSet(IEnumerable<DBObjectReadout> myListOfDBObjectReadout)
            : this(new GraphDBType(), myListOfDBObjectReadout)
        {}

        public SelectionResultSet(DBObjectReadout myListOfDBObjectReadout)
            : this(new GraphDBType(), new List<DBObjectReadout>(){ myListOfDBObjectReadout })
        { }

        public SelectionResultSet(GraphDBType myGraphType, IEnumerable<DBObjectReadout> myListOfDBObjectReadout)
        {

            _Type               = myGraphType;
            _Objects            = myListOfDBObjectReadout ?? new List<DBObjectReadout>();
            _SelectedAttributes = new Dictionary<int, Dictionary<String, String>>();
            int Count = 0;

            foreach (var Item in myListOfDBObjectReadout)
            {                
                _SelectedAttributes.Add(Count, new Dictionary<String, String>());

                foreach (var Attr in Item.Attributes)
                {
                    if (Attr.Value != null)
                    {
                        _SelectedAttributes[Count].Add(Attr.Key, Attr.Value.ToString());
                    }
                    else
                    {
                        _SelectedAttributes[Count].Add(Attr.Key, String.Empty);
                    }
                }

                Count++;
            }

        }


        public SelectionResultSet(GraphDBType myGraphType, IEnumerable<DBObjectReadout> myListOfDBObjectReadout, Dictionary<String, String> mySelectedAttributes)
        {
            _Type                   = myGraphType;
            _Objects                = myListOfDBObjectReadout ?? new List<DBObjectReadout>();

            _SelectedAttributes = new Dictionary<int, Dictionary<String, String>>();
            _SelectedAttributes.Add(0, mySelectedAttributes);
        }



        #endregion


        #region IEnumerable<DBObjectReadout> Members

        public IEnumerator<DBObjectReadout> GetEnumerator()
        {
            return _Objects.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }

}
