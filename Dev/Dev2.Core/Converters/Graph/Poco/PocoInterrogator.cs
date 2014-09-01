using System;
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    public class PocoInterrogator : IInterrogator
    {
        #region Methods

        public IMapper CreateMapper(object data)
        {
            return new PocoMapper();
        }

        public INavigator CreateNavigator(object data, Type pathType)
        {
            if (!pathType.GetInterfaces().Contains(typeof(IPath)))
            {
                throw new Exception("'" + pathType + "' doesn't implement '" + typeof(IPath) + "'");
            }

            return new PocoNavigator(data);
        }

        #endregion Methods
    }
}
