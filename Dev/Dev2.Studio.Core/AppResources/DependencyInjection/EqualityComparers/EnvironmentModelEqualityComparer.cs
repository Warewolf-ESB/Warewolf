#region

using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

#endregion

namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class EnvironmentModelEqualityComparer : IEqualityComparer<IEnvironmentModel>
    {
        #region Class Members

        private static readonly Lazy<EnvironmentModelEqualityComparer> _current
            = new Lazy<EnvironmentModelEqualityComparer>(() => new EnvironmentModelEqualityComparer());

        private EnvironmentModelEqualityComparer()
        {
        }

        #endregion Class Members

        #region Methods

        public bool Equals(IEnvironmentModel x, IEnvironmentModel y)
        {
            return x.DsfAddress.AbsoluteUri == y.DsfAddress.AbsoluteUri;
        }

        public int GetHashCode(IEnvironmentModel obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(IEnvironmentModel x, object y)
        {
            var environment = y as IEnvironmentModel;
            if (environment == null) return false;
            if (x == null) return false;
            if (environment.DsfAddress == null || x.DsfAddress == null) return false;

            return x.DsfAddress.AbsoluteUri == environment.DsfAddress.AbsoluteUri;
        }

        #endregion Methods

        #region Properties

        public static EnvironmentModelEqualityComparer Current
        {
            get { return _current.Value; }
        }

        #endregion Properties
    }
}