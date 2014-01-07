using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.Compiler.CompileRules;

namespace Dev2.Runtime.Compiler
{
    /// <summary>
    /// Compile a service model ;)
    /// </summary>
    public class ServiceModelCompiler
    {
        private readonly CompileRulesRepo _ruleRepo = new CompileRulesRepo();

        /// <summary>
        /// Compiles the specified service
        /// </summary>
        /// <param name="serviceId">The service id.</param>
        /// <param name="typeOf">The type of.</param>
        /// <param name="beforeAction">The before action.</param>
        /// <param name="afterAction">The after action.</param>
        /// <returns></returns>
        public IList<CompileMessageTO> Compile(Guid serviceId, ServerCompileMessageType typeOf, StringBuilder beforeAction, StringBuilder afterAction)
        {
            IList<CompileMessageTO> result = new List<CompileMessageTO>();

            // fetch rules for this service type ;)
            var ruleSet = _ruleRepo.FetchRulesFor(typeOf);

            if (ruleSet != null)
            {
                foreach (var rule in ruleSet)
                {
                    var msg = rule.ApplyRule(serviceId, beforeAction, afterAction);

                    if (msg != null)
                    {
                        result.Add(msg);
                    }
                }
            }

            return result;
        }



    }
}
