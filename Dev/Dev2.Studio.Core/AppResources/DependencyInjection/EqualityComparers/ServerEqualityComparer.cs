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

        public bool Equals(IEnvironmentModel x, object y)
        {
            if(x == null || y == null) return false;
            IEnvironmentModel server = y as IEnvironmentModel;
            if(server == null) return false;
            return (x.Connection.AppServerUri == server.Connection.AppServerUri) && (x.DisplayName == server.DisplayName);
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
