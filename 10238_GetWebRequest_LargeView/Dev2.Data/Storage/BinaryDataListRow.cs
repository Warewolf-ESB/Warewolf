using System;
using Dev2.Data.SystemTemplates;
using System.Text;
using System.Linq;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class BinaryDataListRow : IBinaryDataListRow
    {
        private char[] _rowData;
        private int[] _startIdx;
        private int[] _columnLen;
        private bool[] _deferedColumns;

        private int _colCnt;
        private int _storageCapacity;
        private int _usedStorage;

        public BinaryDataListRow(int columnCnt)
        {

            _startIdx = new int[columnCnt];
            _columnLen = new int[columnCnt];
            _deferedColumns = new bool[columnCnt];

            _colCnt = columnCnt;

            if (columnCnt < DataListConstants.MinRowSize)
            {
                _storageCapacity = DataListConstants.MinRowSize;
            }
            else
            {
                _storageCapacity = columnCnt;
            }

            _rowData = new char[_storageCapacity]; // est a single char per column

            _usedStorage = 0;

            Init();
        }

        public bool IsEmpty
        {
            get
            {
                return (_startIdx.ToList().All(idx => idx == DataListConstants.EmptyRowStartIdx));
            }
        }

        public bool IsDeferredRead(int idx)
        {
            if (idx < _startIdx.Length && idx >= 0)
            {
                return _deferedColumns[idx];
            }

            return false;
        }

        public string FetchValue(int idx)
        {
            if (idx < _startIdx.Length)
            {
                int start = _startIdx[idx];
                if (start > DataListConstants.EmptyRowStartIdx)
                {
                    // we have data cool beans ;)   
                    int len = _columnLen[idx];
                    if (len > 0)
                    {
                        StringBuilder result = new StringBuilder();
                        int end = (start + len);

                        for (int i = start; i < end; i++)
                        {
                            result.Append(_rowData[i]);
                        }

                        return result.ToString();

                    }
                }
            }

            return string.Empty;
        }

        public void UpdateValue(string val, int idx)
        {
            if (val != string.Empty)
            {
                int start = _startIdx[idx];
                if (start > DataListConstants.EmptyRowStartIdx)
                {
                    // we have data, cool beans ;)   
                    int candiateLen = val.Length;
                    int len = _columnLen[idx];

                    // if candiate is larger then prev value, we need to append to end of storage ;)   
                    if (candiateLen > len)
                    {

                        // first clear the old storage location ;)
                        for (int i = start; i < len; i++)
                        {
                            _rowData[i] = '\0';
                        }

                        // do we have space
                        int requiredSize = (_usedStorage + candiateLen);
                        if (requiredSize >= _storageCapacity)
                        {
                            // Nope, grow array
                            GrowRow(requiredSize);
                        }

                        // else yes we have the space, but in either case we need to append to end ;)
                        start = FetchStorageStartIdx();
                        int pos = 0;
                        int iterationLen = (start + candiateLen);
                        for (int i = start; i < iterationLen; i++)
                        {
                            _rowData[i] = val[pos];
                            pos++;
                        }

                        // finally update the storage location data
                        _startIdx[idx] = start;
                        _columnLen[idx] = candiateLen;
                        _usedStorage += candiateLen;
                    }
                    else
                    {
                        // same size or small we can fit it into the same location

                        // first update data
                        int pos = 0;
                        int overWriteLen = (start + candiateLen);
                        for (int i = start; i < overWriteLen; i++)
                        {
                            _rowData[i] = val[pos];
                            pos++;
                        }

                        // finally update length array ;)
                        _columnLen[idx] = candiateLen;
                    }
                }
                else
                {
                    // It is a new value that needs to be inserted ;)

                    int canidateLen = val.Length;
                    int storageReq = _usedStorage + canidateLen;

                    if (storageReq >= _storageCapacity)
                    {
                        // sad panda, we need to grow the storage
                        GrowRow(storageReq);
                    }

                    // now we have space, it is time to save the value ;)
                    start = FetchStorageStartIdx();
                    CharEnumerator itr = val.GetEnumerator();
                    int iterateLen = (start + canidateLen);
                    for (int i = start; i < iterateLen; i++)
                    {
                        itr.MoveNext();
                        _rowData[i] = itr.Current;
                    }


                    itr.Dispose();

                    // finally, update the storage data
                    _startIdx[idx] = start;
                    _columnLen[idx] = canidateLen;
                    _usedStorage += canidateLen;

                }
            }
            else
            {
                // clear the existing storage requirements out ;)
                _startIdx[idx] = 0;
                _columnLen[idx] = 0;
                // TODO : Blank the data too ;)
            }
        }

        public string FetchDeferredLocation(int idx)
        {
            int start = _startIdx[idx];
            if (start > DataListConstants.EmptyRowStartIdx)
            {
                // we have data cool beans ;)   
                if (_deferedColumns[idx])
                {
                    // we have a defered location ;)
                    return FetchValue(idx);
                }
            }

            return string.Empty;
        }

        #region Private Methods

        private void Init()
        {
            // init start indexes and length
            for (int i = 0; i < _colCnt; i++)
            {
                _startIdx[i] = DataListConstants.EmptyRowStartIdx;
                _columnLen[i] = DataListConstants.EmptyRowStartIdx;
            }
        }

        /// <summary>
        /// Grows the row data storage capacity
        /// </summary>
        /// <param name="targetSize">Size of the target.</param>
        private void GrowRow(int targetSize)
        {

            // TODO : Get fancy and reclaim storage ;)

            // grow it by a factor of 1.5 of the target size to avoid growing too often ;)
            int growthSize = (int)(targetSize * DataListConstants.RowGrowthFactor);

            char[] tmp = new char[growthSize];


            Array.Copy(_rowData, tmp, _usedStorage);

            _rowData = tmp;
        }

        private int FetchStorageStartIdx()
        {
            if (_usedStorage == 0)
            {
                return 0;
            }

            return (_usedStorage);

        }

        #endregion
    }
}
