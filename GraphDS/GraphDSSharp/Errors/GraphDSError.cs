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

/*
 * GraphDSError
 * (c) Achim Friedland, 2010
 */

#region Usings

using System;
using System.Diagnostics;
using sones.Lib.ErrorHandling;

#endregion

namespace sones.GraphFS.Errors
{

    /// <summary>
    /// The generic class for all errors within the GraphDS
    /// </summary>
    public class GraphDSError : GeneralError
    {

        #region Constructor(s)

        #region GraphDSError()

        public GraphDSError()
        {
            Message     = default(String);
            StackTrace  = null;
        }

        #endregion

        #region GraphDSError(myMessage)

        public GraphDSError(String myMessage)
        {
            Message     = myMessage;
            StackTrace  = null;
        }

        #endregion

        #region GraphDSError(myMessage, myStackTrace)

        public GraphDSError(String myMessage, StackTrace myStackTrace)
        {
            Message     = myMessage;
            StackTrace  = myStackTrace;
        }

        #endregion

        #endregion

    }

}
