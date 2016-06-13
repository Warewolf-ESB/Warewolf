/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.DB;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace Dev2.Common.DB
{
    /// <summary>
    ///     Used to create helper generation instances
    /// </summary>
    public class DBHelperFactory
    {
        private static readonly Dictionary<enSupportedDBTypes, Type> _options
            = new Dictionary<enSupportedDBTypes, Type>();

        static DBHelperFactory()
        {
            Type type = typeof(IDBHelper);

            List<Type> types = typeof(IDBHelper).Assembly.GetTypes()
                .Where(t => type.IsAssignableFrom(t)).ToList();

            foreach (Type t in types)
            {
                if (!t.IsAbstract && !t.IsInterface)
                {
                    var item = Activator.CreateInstance(t, true) as IDBHelper;
                    if (item != null)
                    {
                        _options.Add(item.HandlesType(), t);
                    }
                }
            }
        }
    }
}