using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class ListOfIndex
    {
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public List<int> Indexes { get; private set; }

        private ListOfIndex()
        {

        }

        public ListOfIndex(List<int> indexes)
        {
            Indexes = indexes;
        }

        public int GetMaxIndex()
        {
            int result = -1;

            if(Indexes != null)
            {
                result = Indexes.Max();
            }

            return result;
        }

        public int GetMinIndex()
        {
            int result = -1;

            if(Indexes != null)
            {
                result = Indexes.Min();
            }

            return result;
        }

        public bool Contains(int idx)
        {
            bool result = false;

            if(Indexes != null)
            {
                result = Indexes.Contains(idx);
            }

            return result;
        }

        public int Count()
        {
            int result = -1;
            if(Indexes != null)
            {
                result = Indexes.Count;
            }
            return result;
        }

        public IIndexIterator FetchIterator()
        {
            return new ListIndexIterator(Indexes);
        }
    }
}