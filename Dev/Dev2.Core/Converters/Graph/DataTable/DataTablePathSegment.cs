using System;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTablePathSegment : IPathSegment
    {
        public string ActualSegment { get; set; }
        
        public string DisplaySegment { get; set; }
        
        public bool IsEnumarable { get; set; }

        public string ToString(bool considerEnumerable)
        {
            throw new NotImplementedException();
        }
    }
}
