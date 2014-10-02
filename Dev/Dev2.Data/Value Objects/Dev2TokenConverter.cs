
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Concurrent;
using System.Collections.Generic;
using Dev2.Data.Enums;

namespace Dev2.DataList.Contract.Value_Objects
{
    public class Dev2TokenConverter
    {
        // yes only single instance per application, a token is a token after all
        private readonly static IDictionary<string, IIntellisenseResult> _partsCache = new ConcurrentDictionary<string, IIntellisenseResult>();
        // Language Parser
        private readonly IDev2DataLanguageParser _parser = DataListFactory.CreateLanguageParser();

        /// <summary>
        /// Parses the token for match.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="canidateParts">The canidate parts.</param>
        /// <returns></returns>
        public IIntellisenseResult ParseTokenForMatch(string exp, IList<IDev2DataLanguageIntellisensePart> canidateParts)
        {
            IIntellisenseResult part;

            // first check part cache ;)
            if (!_partsCache.TryGetValue(exp, out part))
            {
                // if cache miss, find the part and cache ;)
                IList<IIntellisenseResult> parts = _parser.ParseExpressionIntoParts(exp, canidateParts);
                int pos = 0;
                while (pos < parts.Count && part == null)
                {
                    if (parts[pos].Option.DisplayValue.Equals(exp) && parts[pos].Type == enIntellisenseResultType.Selectable)
                    {
                        part = parts[pos];
                        _partsCache[exp] = part; //stach in cache ;)
                    }
                    
                    pos++;
                }
            }

            return part;
        }
    }
}
