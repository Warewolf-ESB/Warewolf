
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common;

namespace Dev2.Runtime.Compiler.CompileRules
{
    /// <summary>
    /// Repo to load compile rules ;)
    /// </summary>
    public class CompileRulesRepo : SpookyAction<IServiceCompileRule, ServerCompileMessageType>
    {

        /// <summary>
        /// Fetches the rules for a type of service
        /// </summary>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        public IEnumerable<IServiceCompileRule> FetchRulesFor(ServerCompileMessageType typeOf)
        {
            IList<IServiceCompileRule> rules = FindAll();

            if(rules != null)
            {
                return (rules.Where(c => c.HandlesType() == typeOf));
            }

            return null;
        }

    }
}
