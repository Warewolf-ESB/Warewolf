using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Converters;
using Dev2.Enums;
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
