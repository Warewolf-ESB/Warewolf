using System.Collections.Generic;

namespace Dev2.Data.TO
{
    /// <summary>
    /// Used to work around aliases and foreach execution ;)
    /// </summary>
    public class XMLPopulateIndexTO
    {

        public HashSet<int> Gaps { get; private set; }

        public int Min { get; private set; }

        public int Max { get; private set; }


        public XMLPopulateIndexTO(HashSet<int> gaps, int min, int max)
        {
            Gaps = gaps;
            Min = min;
            Max = max;
        }
    }
}
