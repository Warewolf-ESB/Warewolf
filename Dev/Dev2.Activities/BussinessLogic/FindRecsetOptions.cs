
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.DataList.Contract;
// ReSharper disable CheckNamespace

namespace Dev2.DataList
{
    /// <summary>
    /// Static class for returning the search option that was selected by the user
    /// </summary>
    public static class FindRecsetOptions
    {

        private static Dictionary<string, IFindRecsetOptions> _options = new Dictionary<string, IFindRecsetOptions>();


        /// <summary>
        /// Find the matching search object
        /// </summary>
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public static IFindRecsetOptions FindMatch(string expressionType)
        {
            IFindRecsetOptions result;
            if(!_options.TryGetValue(expressionType, out result))
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Find all AbstractRecsetSearchValidation objects
        /// </summary>
        /// <returns></returns>
        public static IList<IFindRecsetOptions> FindAll()
        {
            return _options.Values.Where(a=>a.ArgumentCount>0).ToList();
        }
        /// <summary>
        /// Find all AbstractRecsetSearchValidation objects
        /// </summary>
        /// <returns></returns>
        public static IList<IFindRecsetOptions> FindAllDecision()
        {
            return _options.Values.ToList();
        }
    }
}
