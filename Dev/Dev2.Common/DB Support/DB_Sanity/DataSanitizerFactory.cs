
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
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.DB
{
    /*
     * Provides new data sanitizing functionality for DB sources
     */
    public class DataSanitizerFactory
    {
        private static readonly Dictionary<enSupportedDBTypes, Type> _options 
            = new Dictionary<enSupportedDBTypes, Type>();


        static DataSanitizerFactory(){
            var type = typeof(IDataProviderSanitizer);

            List<Type> types = typeof(IDataProviderSanitizer).Assembly.GetTypes()
                   .Where(t => (type.IsAssignableFrom(t))).ToList();

            foreach (Type t in types)
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    IDataProviderSanitizer item = Activator.CreateInstance(t, true) as IDataProviderSanitizer;
                    if (item != null)
                    {
                        _options.Add(item.HandlesType(), t);
                    }
                }
            }
        }
        
        /// <summary>
        /// Fetch the type to be handled, then generate a concerte instance
        /// </summary>
        /// <param name="typeOf"></param>
        /// <returns></returns>
        public static IDataProviderSanitizer GenerateNewSanitizer(enSupportedDBTypes typeOf)
        {
            IDataProviderSanitizer result;
            Type outType;

            if (!_options.TryGetValue(typeOf, out outType))
            {
                result = null;
            }
            else
            {
                result = Activator.CreateInstance(outType, true) as IDataProviderSanitizer;
            }

            return result;
        }
    }
}
