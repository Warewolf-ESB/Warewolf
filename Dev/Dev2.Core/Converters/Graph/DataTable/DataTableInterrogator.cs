using System;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTableInterrogator : IInterrogator
    {
        public IMapper CreateMapper(object data)
        {
            return new DataTableMapper();
        }

        public INavigator CreateNavigator(object data, Type pathType)
        {
            return new DataTableNavigator();
        }
    }
}
