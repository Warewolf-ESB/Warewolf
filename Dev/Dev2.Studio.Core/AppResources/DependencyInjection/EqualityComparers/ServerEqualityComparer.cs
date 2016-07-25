/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.AppResources.DependencyInjection.EqualityComparers
{
    public class ServerEqualityComparer : IEqualityComparer<IEnvironmentModel>
    {
        #region Class Members

        // ReSharper disable InconsistentNaming
        private static readonly ServerEqualityComparer _current = new ServerEqualityComparer();

        #endregion Class Members

        #region Methods

        public bool Equals(IEnvironmentModel x, IEnvironmentModel y)
        {
            if(x == null || y == null) return false;
            return (x.Connection.AppServerUri == y.Connection.AppServerUri) && (x.DisplayName == y.DisplayName);
        }

        public int GetHashCode(IEnvironmentModel obj)
        {
            return obj.GetHashCode();
        }

        #endregion Methods

        #region Properties

        public static ServerEqualityComparer Current => _current;

        #endregion Properties
    }
}
