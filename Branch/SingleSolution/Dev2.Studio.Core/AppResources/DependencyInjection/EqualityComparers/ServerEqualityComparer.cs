using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.AppResources.DependencyInjection.EqualityComparers
{
    public class ServerEqualityComparer : IEqualityComparer<IEnvironmentModel>
    {
        #region Class Members

        private static readonly ServerEqualityComparer _current = new ServerEqualityComparer();

        #endregion Class Members

        #region Methods

        public bool Equals(IEnvironmentModel x, IEnvironmentModel y)
        {
            if (x == null || y == null) return false;
            return x.Connection.AppServerUri == y.Connection.AppServerUri;
        }

        public bool Equals(IEnvironmentModel x, object y)
        {
            IEnvironmentModel server = y as IEnvironmentModel;
            return server != null && x.Connection.AppServerUri == server.Connection.AppServerUri;
        }

        public int GetHashCode(IEnvironmentModel obj)
        {
            return obj.GetHashCode();
        }

        #endregion Methods

        #region Properties

        public static ServerEqualityComparer Current
        {
            get
            {
                return _current;
            }
        }

        #endregion Properties
    }
}
