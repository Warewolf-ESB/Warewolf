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
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    [Serializable]
    class PocoPathSegment : IPathSegment
    {
        internal PocoPathSegment()
        {
        }

        internal PocoPathSegment(string name, bool isEnumarable)
        {
            ActualSegment = name;
            IsEnumarable = isEnumarable;
        }

        public string ActualSegment { get; set; }
        public string DisplaySegment { get; set; }
        public bool IsEnumarable { get; set; }

        public string ToString(bool considerEnumerable)
        {
            if (considerEnumerable && IsEnumarable)
            {
                return ActualSegment + PocoPath.EnumerableSymbol;
            }

            return ActualSegment;
        }

        public override string ToString()
        {
            if (IsEnumarable)
            {
                return ActualSegment + PocoPath.EnumerableSymbol;
            }

            return ActualSegment;
        }

        public T As<T>() where T : class, IPathSegment
        {
            var ret = this as T;
            if (ret != null)
            {
                return ret;
            }
            throw new NotImplementedException();
        }
    }
}