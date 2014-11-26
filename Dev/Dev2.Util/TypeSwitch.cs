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
using System.Linq;

namespace Dev2.Studio.Core.Helpers
{
    /// <summary>
    ///     NO TEST COVERAGE!!!!
    /// </summary>
    public static class TypeSwitch
    {
        public static void Do(object source, params CaseInfo[] cases)
        {
            if (cases == null)
            {
                throw new ArgumentNullException("cases");
            }

            if (source == null)
            {
                if (!cases.ToList().Any(c => c.IsDefault))
                    throw new Exception("Can not do switch on null type argument if no default implementation provided");

                foreach (CaseInfo entry in cases.Where(entry => entry.IsDefault))
                {
                    entry.Action(null);
                    break;
                }
            }
            else
            {
                Type type = source.GetType();
                foreach (CaseInfo entry in cases.Where(entry => entry.IsDefault || entry.Target.IsAssignableFrom(type)))
                {
                    entry.Action(source);
                    break;
                }
            }
        }

        public static CaseInfo Case<T>(Action action)
        {
            return new CaseInfo
            {
                Action = x => action(),
                Target = typeof (T)
            };
        }

        public static CaseInfo Case<T>(Action<T> action)
        {
            return new CaseInfo
            {
                Action = x => action((T) x),
                Target = typeof (T)
            };
        }

        public static CaseInfo Default(Action action)
        {
            return new CaseInfo
            {
                Action = x => action(),
                IsDefault = true
            };
        }

        public class CaseInfo
        {
            public bool IsDefault { get; set; }
            public Type Target { get; set; }
            public Action<object> Action { get; set; }
        }
    }
}