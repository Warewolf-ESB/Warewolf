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
using System.Linq;
using Warewolf.Resource.Errors;

namespace Dev2.Studio.Core.Helpers
{
    public static class TypeSwitch
    {
        public static void Do(object source, params CaseInfo[] cases)
        {
            if (cases == null)
            {
                throw new ArgumentNullException(nameof(cases));
            }

            if (source == null)
            {
                if (!cases.ToList().Any(c => c.IsDefault))
                {
                    throw new Exception(ErrorResource.CannotDoSwitchOnNullType);
                }

                foreach (var entry in cases.Where(entry => entry.IsDefault))
                {
                    entry.Action(null);
                    break;
                }
            }
            else
            {
                var type = source.GetType();
                foreach (var entry in cases.Where(entry => entry.IsDefault || entry.Target.IsAssignableFrom(type)))
                {
                    entry.Action(source);
                    break;
                }
            }
        }

        public static CaseInfo Case<T>(Action action) => new CaseInfo
        {
            Action = x => action?.Invoke(),
            Target = typeof(T)
        };

        public static CaseInfo Case<T>(Action<T> action) => new CaseInfo
        {
            Action = x => action?.Invoke((T)x),
            Target = typeof(T)
        };

        public static CaseInfo Default(Action action) => new CaseInfo
        {
            Action = x => action?.Invoke(),
            IsDefault = true
        };

        public class CaseInfo
        {
            public bool IsDefault { get; set; }
            public Type Target { get; set; }
            public Action<object> Action { get; set; }
        }
    }
}
