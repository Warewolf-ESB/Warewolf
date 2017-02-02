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
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Services.Security
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class WindowsGroupPermissionEqualityComparer : IEqualityComparer<WindowsGroupPermission>
    {
        #region Implementation of IEqualityComparer<in WindowsGroupPermission>

        // ReSharper disable CSharpWarnings::CS1584
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        // ReSharper restore CSharpWarnings::CS1584
        public bool Equals(WindowsGroupPermission x, WindowsGroupPermission y)
        {
            var isEqual = x.Permissions.Equals(y.Permissions) && x.ResourceID.Equals(y.ResourceID);
            if(!string.IsNullOrEmpty(x.ResourceName) && !string.IsNullOrEmpty(y.ResourceName))
            {
                isEqual = isEqual && x.ResourceName.Equals(y.ResourceName);
            }
            if(!string.IsNullOrEmpty(x.WindowsGroup) && !string.IsNullOrEmpty(y.WindowsGroup))
            {
                isEqual = isEqual && x.WindowsGroup.Equals(y.WindowsGroup);
            }
            return isEqual;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(WindowsGroupPermission obj)
        {
            var hashCode = 397;
            hashCode += obj.Permissions.GetHashCode() ^ obj.ResourceID.GetHashCode();
            if(!string.IsNullOrEmpty(obj.ResourceName))
            {
                hashCode += hashCode^ obj.ResourceName.GetHashCode();
            }
            if(!string.IsNullOrEmpty(obj.WindowsGroup))
            {
                hashCode +=  hashCode^ obj.WindowsGroup.GetHashCode();
            }
            return hashCode;
        }

        #endregion
    }
}
