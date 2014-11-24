
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
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class EnvironmentModelEqualityComparer : IEqualityComparer<IEnvironmentModel>
    {
        #region Class Members

        private static readonly Lazy<EnvironmentModelEqualityComparer> Instance
            = new Lazy<EnvironmentModelEqualityComparer>(() => new EnvironmentModelEqualityComparer());

        private EnvironmentModelEqualityComparer()
        {
        }

        #endregion Class Members

        #region Methods

        public bool Equals(IEnvironmentModel x, IEnvironmentModel y)
        {
            if(x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }

        public int GetHashCode(IEnvironmentModel obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(IEnvironmentModel x, object y)
        {
            return Equals(x, y as IEnvironmentModel);
        }

        #endregion Methods

        #region Properties

        public static EnvironmentModelEqualityComparer Current
        {
            get { return Instance.Value; }
        }

        #endregion Properties
    }
}
