/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Warewolf.Resource.Errors;

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

            if (mapper == null)
            {
                throw new Exception(string.Format(ErrorResource.CouldntCreateMapper, data));
            }

            return mapper.Map(data);
        }

        public object SelectScalar(IPath path, object data)
        {
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
            INavigator navigator = interrogator.CreateNavigator(data, path.GetType());

            if (navigator == null)
            {
                throw new Exception(string.Format(ErrorResource.CouldntCreateNavigator, path));
            }

            object value = navigator.SelectScalar(path);

            navigator.Dispose();

            return value;
        }

        public IEnumerable<object> SelectEnumerable(IPath path, object data)
        {
            IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
            INavigator navigator = interrogator.CreateNavigator(data, path.GetType());

            if (navigator == null)
            {
                throw new Exception(string.Format(ErrorResource.CouldntCreateNavigator, path));
            }

            IEnumerable<object> values = navigator.SelectEnumerable(path);

            navigator.Dispose();

            return values;
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths, object data)
        {
            Dictionary<IPath, IList<object>> values;

            if (paths.Count > 0)
            {
                IInterrogator interrogator = InterrogatorFactory.CreateInteregator(data.GetType());
                INavigator navigator = interrogator.CreateNavigator(data, paths[0].GetType());

                if (navigator == null)
                {
                    throw new Exception(string.Format(ErrorResource.CouldntCreateNavigator, paths[0]));
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