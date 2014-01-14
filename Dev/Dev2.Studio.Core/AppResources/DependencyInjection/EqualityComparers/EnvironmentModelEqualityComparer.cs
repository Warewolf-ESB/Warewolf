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
            return x.Connection.AppServerUri.AbsoluteUri == y.Connection.AppServerUri.AbsoluteUri;
        }

        public int GetHashCode(IEnvironmentModel obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(IEnvironmentModel x, object y)
        {
            var environment = y as IEnvironmentModel;
            if(environment == null)
            {
                return false;
            }
            if(x == null)
            {
                return false;
            }
            if(environment.ID == x.ID)
            {
                return true;
            }
            if(environment.Connection.AppServerUri == null || x.Connection.AppServerUri == null)
            {
                return false;
            }

            return x.Connection.AppServerUri.AbsoluteUri == environment.Connection.AppServerUri.AbsoluteUri;
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