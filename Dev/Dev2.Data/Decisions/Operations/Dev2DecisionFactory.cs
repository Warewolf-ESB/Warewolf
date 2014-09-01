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
