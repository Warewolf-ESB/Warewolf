
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common;

namespace Dev2.Data.Decisions.Operations
{
    public class Dev2DecisionFactory : SpookyAction<IDecisionOperation, Enum>
    {
        private static Dev2DecisionFactory _inst;

        public static Dev2DecisionFactory Instance()
        {
            if (_inst == null)
            {
                _inst = new Dev2DecisionFactory(); 
            }

            return _inst;
        }

        public IDecisionOperation FetchDecisionFunction(enDecisionType typeOf)
        {
            return FindMatch(typeOf);
        }

    }
}
