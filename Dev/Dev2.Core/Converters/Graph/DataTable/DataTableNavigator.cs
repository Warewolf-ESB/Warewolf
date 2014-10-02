
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
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTableNavigator : INavigator
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public object Data { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        public object SelectScalar(IPath path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> SelectEnumerable(IPath path)
        {
            throw new NotImplementedException();
        }

        public Dictionary<IPath, IList<object>> SelectEnumerablesAsRelated(IList<IPath> paths)
        {
            throw new NotImplementedException();
        }
    }
}
