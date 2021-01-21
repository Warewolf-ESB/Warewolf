/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Warewolf.Options;

namespace Warewolf.Data.Decisions.Operations
{
    public class Dev2WebPostFormDataDecisionFactory : SpookyAction<IDecisionOperation, Enum>
    {
        static Dev2WebPostFormDataDecisionFactory _inst;

        public static Dev2WebPostFormDataDecisionFactory Instance()
        {
            if (_inst == null)
            {
                _inst = new Dev2WebPostFormDataDecisionFactory();
            }

            return _inst;
        }

        public IDecisionOperation FetchDecisionFunction(enFormDataTableType typeOf) => FindMatch(typeOf);

    }
}
