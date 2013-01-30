using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers
{
    public class ServerEqualityComparer : IEqualityComparer<IServer>
    {
        #region Class Members

        private static readonly ServerEqualityComparer _current = new ServerEqualityComparer();

        #endregion Class Members

        #region Methods

        public bool Equals(IServer x, IServer y)
        {
            if (x == null || y == null) return false;
            return x.AppAddress == y.AppAddress;
        }

        public bool Equals(IServer x, object y)
        {
            IServer server = y as IServer;
            return server != null && x.AppAddress == server.AppAddress;
        }

        public bool Equals(IServer x, IEnvironmentModel y)
        {
            return x.AppAddress == y.DsfAddress.AbsoluteUri;
        }

        public int GetHashCode(IServer obj)
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
