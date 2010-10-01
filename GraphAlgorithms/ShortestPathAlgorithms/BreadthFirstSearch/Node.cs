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

/* <id name="GraphDB � BreadthFirstSearch" />
 * <copyright file="BreadthFirstSearch.cs"
 *            company="sones GmbH">
 * Copyright (c) sones GmbH. All rights reserved.
 * </copyright>
 * <developer>Martin Junghanns</developer>
 * <developer>Michael Woidak</developer>
 * <summary>
 * A Node represents a node in a graph. The Key is unique (ObjectUUID) and every
 * node know about its parents and childrens.
 * 
 * The Node class is used by BFS search and evaluation.
 * </summary>
 */

using System.Collections.Generic;
using sones.Lib.DataStructures.UUID;
using sones.GraphFS.DataStructures;

namespace sones.GraphAlgorithms.PathAlgorithm.BFSTreeStructure
{

    public class Node
    {

        #region private members

        //unique identifier
        private ObjectUUID _Key;

        //are used to create the paths recursive
        private HashSet<Node> _Parents;

        //are used to create the paths recursive
        private HashSet<Node> _Children;

        //is used to check if the node is already in path
        private bool _AlreadyInPath;

        #endregion

        #region constructors

        public Node()
        {
            _Key = null;

            _Parents = new HashSet<Node>();

            _Children = new HashSet<Node>();

            _AlreadyInPath = false;
        }

        public Node(ObjectUUID myObjectUUID) : this()
        {
            _Key = myObjectUUID;
        }

        public Node(ObjectUUID myObjectUUID, bool AlreadyInPath)
            : this(myObjectUUID)
        {
            _AlreadyInPath = AlreadyInPath;
        }

        public Node(ObjectUUID myObjectUUID, Node myParent) 
            : this(myObjectUUID)
        {
            _Parents.Add(myParent);
        }

        #endregion

        #region getter/setter

        public ObjectUUID Key
        {
            get { return this._Key; }
            set { this._Key = value; }
        }

        public HashSet<Node> Parents
        {
            get { return this._Parents; }
            set { this._Parents = value; }
        }

        public HashSet<Node> Children
        {
            get { return this._Children; }
            set { this._Children = value; }
        }

        public bool AlreadyInPath
        {
            get { return this._AlreadyInPath; }
            set { this._AlreadyInPath = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// F�gt dem Knoten ein Child hinzu, existiert dieser schon, werden die Parents und Children des existierenden aktualisiert.
        /// </summary>
        /// <param name="myChild">Child welches hinzugef�gt werden soll.</param>
        /// <returns></returns>
        public bool addChild(Node myChild)
        {
            bool equal = false;

            foreach (var thisChild in _Children)
            {
                //check if the node wich should be added IS already existing
                if (thisChild._Key.Equals(myChild.Key))
                {
                    equal = true;

                    break;
                }
            }

            if (!equal)
            {
                return _Children.Add(myChild);
            }

            return false;
        }

        /// <summary>
        /// F�gt eine Liste von Children hinzu.
        /// </summary>
        /// <param name="myChildren">Liste von Children.</param>
        public void addChildren(HashSet<Node> myChildren)
        {
            foreach (var myChild in myChildren)
            {
                addChild(myChild);
            }
        }

        /// <summary>
        /// F�gt dem Knoten ein Parent hinzu, existiert dieser schon, werden die Parents und Children des existierenden aktualisiert.
        /// </summary>
        /// <param name="myParent">Parent welcher hinzugef�gt werden soll.</param>
        /// <returns></returns>
        public bool addParent(Node myParent)
        {            
            bool equal = false;

            foreach (var thisParent in _Parents)
            {
                //check if the node wich should be added IS already existing
                if (thisParent._Key.Equals(myParent.Key))
                {
                    //exists
                    equal = true;

                    break;
                }
            }

            //node is NOT already existing, add
            if (!equal)
            {
                return _Parents.Add(myParent);
            }
            
            return false;
        }

        /// <summary>
        /// F�gt eine Liste von Parents hinzu.
        /// </summary>
        /// <param name="myParents">Liste von Parents.</param>
        public void addParents(HashSet<Node> myParents)
        {
            foreach (var myParent in myParents)
            {
                addParent(myParent);
            }
        }

        /// <summary>
        /// �berpr�ft ob der angegebene Key bereits vorhanden ist.
        /// </summary>
        /// <param name="key">Key nach dem gesucht werden soll.</param>
        /// <returns></returns>
        public bool ChildrenContainsKey(ObjectUUID key)
        {
            foreach (var node in _Children)
            {
                if (node.Key.Equals(key))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// �berpr�ft ob der angegebene Key bereits vorhanden ist.
        /// </summary>
        /// <param name="key">Key nach dem gesucht werden soll.</param>
        /// <returns></returns>
        public bool ParentsContainsKey(ObjectUUID key)
        {
            foreach (var node in _Parents)
            {
                if (node.Key.Equals(key))
                    return true;
            }

            return false;
        }

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            return _Key.GetHashCode();
        }

        /// <summary>
        /// �berpr�ft ob Nodes identisch sind.
        /// </summary>
        /// <param name="obj">Objekt der �berpr�ft werden soll. Muss vom Typ "Node" sein.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Node)
            {
                return _Key.Equals((obj as Node)._Key);
            }
            else
            {
                return false;
            }
        }

        #endregion

    }

}
