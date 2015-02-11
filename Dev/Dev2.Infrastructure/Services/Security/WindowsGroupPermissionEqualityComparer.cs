
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Services.Security
{
    public class WindowsGroupPermissionEqualityComparer : IEqualityComparer<IWindowsGroupPermission>
    {
        #region Implementation of IEqualityComparer<in WindowsGroupPermission>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
        public bool Equals(IWindowsGroupPermission x, IWindowsGroupPermission y)
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
        public int GetHashCode(IWindowsGroupPermission obj)
        {
            var hashCode = 0;
            hashCode += obj.Permissions.GetHashCode() + obj.ResourceID.GetHashCode();
            if(!string.IsNullOrEmpty(obj.ResourceName))
            {
                hashCode += obj.ResourceName.GetHashCode();
            }
            if(!string.IsNullOrEmpty(obj.WindowsGroup))
            {
                hashCode += obj.WindowsGroup.GetHashCode();
            }
            return hashCode;
        }

        #endregion
    }
}
