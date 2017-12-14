using Dev2.TO;
using System.Collections.Generic;

namespace Dev2.Comparer
{
    internal class JsonMappingToComparer : IEqualityComparer<JsonMappingTo>
    {
        public bool Equals(JsonMappingTo x, JsonMappingTo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return string.Equals(x.DestinationName, y.DestinationName)
                && x.IndexNumber.Equals(y.IndexNumber)
                && x.Inserted.Equals(y.Inserted)
                && x.IsSourceNameFocused.Equals(y.IsSourceNameFocused)
                && x.IsDestinationNameFocused.Equals(y.IsDestinationNameFocused)
                && string.Equals(x.SourceName, y.SourceName)
            ;
        }

        public int GetHashCode(JsonMappingTo obj)
        {
            return 1;
        }
    }
}
