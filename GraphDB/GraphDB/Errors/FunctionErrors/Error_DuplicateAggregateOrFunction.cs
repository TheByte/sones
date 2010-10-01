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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sones.GraphDB.Errors
{
    public class Error_DuplicateAggregateOrFunction : GraphDBFunctionError
    {
        public String FunctionName { get; private set; }
        public Boolean IsFunction { get; private set; }

        public Error_DuplicateAggregateOrFunction(String myFunctionName, Boolean myIsFunction = true)
        {
            FunctionName = myFunctionName;
            IsFunction   = myIsFunction;
        }

        public override string ToString()
        {
            if (IsFunction)
            {
                return String.Format("The function name \"{0}\" is duplicate! The name has to be unique!", FunctionName);
            }
            else
            {
                return String.Format("The aggregate name \"{0}\" is duplicate! The name has to be unique!", FunctionName);
            }
        }
    }
}
