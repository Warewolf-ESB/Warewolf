/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

// ReSharper disable once CheckNamespace
namespace Dev2.Diagnostics
{
    public class DebugItemResultEqualityComparer : IEqualityComparer<IDebugItemResult>
    {
        public bool Equals(IDebugItemResult x, IDebugItemResult y)
        {
            if(x != null && y != null)
            {
                return x.Type == y.Type
                       && x.Label == y.Label
                       && x.Variable == y.Variable
                       && x.Value == y.Value
                       && x.GroupName == y.GroupName
                       && x.GroupIndex == y.GroupIndex
                       && x.MoreLink == y.MoreLink;
            }
            return false;
        }

        public int GetHashCode(IDebugItemResult obj)
        {
            unchecked
            {
                var hashCode = typeof(IDebugItemResult).GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.Label != null ? obj.Label.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Variable != null ? obj.Variable.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.Operator != null ? obj.Operator.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.GroupName != null ? obj.GroupName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.GroupIndex;
                hashCode = (hashCode * 397) ^ (obj.Value != null ? obj.Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.MoreLink != null ? obj.MoreLink.GetHashCode() : 0);
                return hashCode;
            }


        }


    }
}
