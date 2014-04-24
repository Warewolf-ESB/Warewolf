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
