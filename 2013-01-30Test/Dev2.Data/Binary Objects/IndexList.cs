using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{

        [Serializable]
        public class IndexList
        {
            public int MaxValue { get; private set; }
            public int MinValue { get; private set; }
            private readonly HashSet<int> _gaps;

            public IndexList()
            {
                _gaps = new HashSet<int>();
                MinValue = 1;
                MaxValue = 1;
            }


            public void SetMaxValue(int idx)
            {
                if (idx < MaxValue)
                {
                    // remove gaps
                    if (_gaps.Contains(idx))
                    {
                        _gaps.Remove(idx);
                    }
                }
                else
                {
                    // set new max
                    int dif = (idx - MaxValue);
                    if (dif > 1)
                    {
                        // find the gaps ;)
                        for (int i = 1; i < dif; i++)
                        {
                            _gaps.Add((MaxValue + i));
                        }
                    }
                    MaxValue = idx;
                }
            }

            public void AddGap(int idx)
            {
                _gaps.Add(idx);
            }

            public bool Contains(int idx)
            {
                bool result = (idx <= MaxValue && idx >= 0 && !_gaps.Contains(idx));

                return result;
            }

            public int Count()
            {
                return MaxValue - _gaps.Count;
            }

            public IIndexIterator FetchIterator()
            {
                return new IndexIterator(_gaps, MaxValue);
            }

        }
    }

