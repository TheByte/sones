/* GraphFS BSTreeElement
 * (c) Daniel Kirstenpfad, 2007 - 2008
 * (c) Achim Friedland, 2008 - 2009
 * 
 * This is the smallest unit of the BSTree - a BSTreeElement
 * 
 * Lead programmer:
 *      Daniel Kirstenpfad
 *      Achim Friedland
 * 
 * */

#region Usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace sones.Lib.BTree
{

    /// <summary>
    /// This is the smallest unit of the BSTree - a BSTreeElement
    /// </summary>
    /// <typeparam name="T">the type of the object that is stored in here (e.g. string ...)</typeparam>

    

    public class BSTreeElement<T> : IComparable<T>, IComparable
    {


        #region Data

        public T  ID;

        #endregion


        #region Properties

        #region INodePositions
        
        private List<UInt64> _INodePositions;

        public List<UInt64> INodePositions
        {

            get
            {
                return _INodePositions;
            }

            set
            {
                _INodePositions  = value;
                _InlineData      = null;
            }

        }

        #endregion

        //#region ObjectStreamsBitfield

        //private ObjectStreamTypes _ObjectStreamsBitfield;

        //public ObjectStreamTypes ObjectStreamsBitfield
        //{

        //    get
        //    {
        //        return _ObjectStreamsBitfield;
        //    }

        //    set
        //    {
        //        _ObjectStreamsBitfield  = value;
        //    }

        //}

        //#endregion

        #region InlineData

        private Byte[] _InlineData;

        public Byte[] InlineData
        {

            get
            {
                return _InlineData;
            }

            set
            {
                _InlineData      = value;
                _INodePositions  = null;
            }

        }

        #endregion

        #endregion



        #region Constructor

        #region BSTreeElement()

        /// <summary>
        /// Plain constructor
        /// </summary>
        public BSTreeElement()
        {
            ID                      = default(T);
            _INodePositions         = null;
            _InlineData             = null;
//            _ObjectStreamsBitfield  = 0;
        }

        #endregion

        #region BSTreeElement(myElementId)

        /// <summary>
        /// The constructor of one BSTreeElement - the smallest unit of a BSTree
        /// </summary>
        /// <param name="myIdentification">this sets the ID</param>
        public BSTreeElement(T myElementId)
        {
            ID                      = myElementId;
            _INodePositions         = null;
            _InlineData             = null;
//            _ObjectStreamsBitfield  = 0;
        }

        #endregion

        #endregion



        #region ToString

        /// <summary>
        /// A ToString() Override
        /// </summary>
        /// <returns>the ID of this BSTreeElement</returns>
        public override string ToString()
        {
            return ID.ToString();
        }

        #endregion

        #region IComparable<T> Member

        /// <summary>
        /// generic IComparable Interface Implementation
        /// </summary>
        /// <param name="other">the BSTreeElement that needs to be compared to this one</param>
        /// <returns></returns>
        int IComparable<T>.CompareTo(T other)
        {

            return ID.ToString().CompareTo(other.ToString());

        }

        #endregion

        #region IComparable Member

        /// <summary>
        /// IComparable Interface Implementation
        /// </summary>
        /// <param name="obj">the BSTreeElement that needs to be compared to this one</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {

            return ID.ToString().CompareTo(obj.ToString());

        }

        #endregion

    
    }

}
