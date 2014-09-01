using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph
{
    [Serializable]
    public class DataBrowser : IDataBrowser
    {
        #region Methods

        public IEnumerable<IPath> Map(object data)
        {
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
            IMapper mapper = interrogator.CreateMapper(data);

            if(mapper == null)
            {
                throw new Exception(string.Concat("Couldn't create a mapper for '", data.ToString(), "'."));
            }

            return mapper.Map(data);
        }

        public object SelectScalar(IPath path, object data)
        {
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
            INavigator navigator = interrogator.CreateNavigator(data, path.GetType());

            if(navigator == null)
            {
                throw new Exception(string.Concat("Couldn't create a navigator for the path '", path.ToString(), "'."));
            }

            object value = navigator.SelectScalar(path);

            navigator.Dispose();

            return value;
        }

        public IEnumerable<object> SelectEnumerable(IPath path, object data)
        {
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
            INavigator navigator = interrogator.CreateNavigator(data, path.GetType());

            if(navigator == null)
            {
                throw new Exception(string.Concat("Couldn't create a navigator for the path '", path.ToString(), "'."));
            }

            IEnumerable<object> values = navigator.SelectEnumerable(path);

            navigator.Dispose();

            return values;
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths, object data)
        {
            Dictionary<IPath, IList<object>> values;

            if(paths.Count > 0)
            {
                IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
                INavigator navigator = interrogator.CreateNavigator(data, paths[0].GetType());

                if(navigator == null)
                {
                    throw new Exception(string.Concat("Couldn't create a navigator for the path '", paths[0].ToString(), "'."));
                }

                values = navigator.SelectEnumerablesAsRelated(paths);

                navigator.Dispose();
            }
            else
            {
                values = new Dictionary<IPath, IList<object>>();
            }

            return values;
        }

        #endregion Methods
    }
}
