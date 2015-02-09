
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
using Dev2.Common.Interfaces;
using Dev2.Interfaces;

namespace Dev2.Factories
{
    public class Dev2FindMissingStrategyFactory : SpookyAction<IFindMissingStrategy, Enum>
    {        
        /// <summary>
        /// Create a find missing strategy
        /// </summary>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        public IFindMissingStrategy CreateFindMissingStrategy(enFindMissingType typeOf)
        {
            return FindMatch(typeOf);
        }
    }    
}
