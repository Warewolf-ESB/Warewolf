#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;


namespace Dev2.DataList
{
    /// <summary>
    /// Static class for returning the search option that was selected by the user
    /// </summary>
    public static class FindRecsetOptions
    {

        static Dictionary<string, IFindRecsetOptions> _options = new Dictionary<string, IFindRecsetOptions>();

        /// <summary>
        /// A static constructor is used to initialize any static data,
        ///  or to perform a particular action that needs to be performed once only.
        ///  It is called automatically before the first instance is created or any static members are referenced.
        /// </summary>
        static FindRecsetOptions()
        {
            var type = typeof(IFindRecsetOptions);

            var types = typeof(IFindRecsetOptions).Assembly.GetTypes()
                   .Where(t => type.IsAssignableFrom(t)).ToList();

            foreach (Type t in types)
            {
                if (!t.IsAbstract && !t.IsInterface && Activator.CreateInstance(t, true) is IFindRecsetOptions item)
                {
                    _options.Add(item.HandlesType(), item);
                }

            }
            SortRecordsetOptions();
        }

        static void SortRecordsetOptions()
        {
            var tmpDictionary = new Dictionary<string, IFindRecsetOptions>();

            foreach (string findRecordsOperation in GlobalConstants.FindRecordsOperations)

            {
                var firstOrDefault = _options.FirstOrDefault(c => c.Value.HandlesType().Equals(findRecordsOperation, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrEmpty(firstOrDefault.Key))
                {
                    tmpDictionary.Add(firstOrDefault.Key, firstOrDefault.Value);
                }
            }

            _options = tmpDictionary;
        }
        /// <summary>
        /// Find the matching search object
        /// </summary>
        /// <param name="expressionType"></param>
        /// <returns></returns>
        public static IFindRecsetOptions FindMatch(string expressionType)
        {
            if (!_options.TryGetValue(expressionType, out IFindRecsetOptions result))
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
            var findRecsetOptionses = _options.Values.Where(a=>a.ArgumentCount>0).ToList();
            return findRecsetOptionses;
        }

        /// <summary>
        /// Find all AbstractRecsetSearchValidation objects
        /// </summary>
        /// <returns></returns>
        public static IList<IFindRecsetOptions> FindAllDecision() => _options.Values.ToList();
    }
}
