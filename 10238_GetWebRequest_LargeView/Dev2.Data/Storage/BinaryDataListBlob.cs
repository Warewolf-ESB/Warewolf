using System;
using Dev2.Data.Storage;
using Dev2.Data.SystemTemplates;

namespace Dev2.Data.Storge
{
    /// <summary>
    /// Used to store binary data ;)
    /// </summary>
    [Serializable]
    public class BinaryDataListBlob : BinaryDataListStorageRoot<byte, byte[]>
    {

        public BinaryDataListBlob(int colCnt) : base(colCnt) { }

        public override byte[] FetchValue(int idx)
        {

            byte[] result = new byte[0];

            if (idx < _startIdx.Length)
            {
                int start = _startIdx[idx];
                if (start > DataListConstants.EmptyRowStartIdx)
                {
                    // we have data cool beans ;)   
                    int len = _columnLen[idx];
                    if (len > 0)
                    {
                        
                        int end = (start + len);
                        int pos = 0;

                        for (int i = start; i < end; i++)
                        {
                            result[pos] = _rowData[i];
                            pos++;
                        }

                        return result;

                    }
                }
            }

            return result;
        }

        public override void UpdateValue(byte[] val, int idx)
        {
            if (val != null && val.Length > 0)
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

                        // do we have space
                        int requiredSize = (_usedStorage + candiateLen);
                        if (requiredSize >= _storageCapacity)
                        {
                            // Nope, grow array
                            GrowRow(requiredSize);
                        }

                        // else yes we have the space, but in either case we need to append to end ;)
                        start = FetchStorageStartIdx();
                        int iterationLen = (start + candiateLen);

                        Array.ConstrainedCopy(val, 0, _rowData, start, iterationLen);

                        // finally update the storage location data
                        _startIdx[idx] = start;
                        _columnLen[idx] = candiateLen;
                        _usedStorage += candiateLen;
                    }
                    else
                    {
                        // same size or small we can fit it into the same location

                        // first update data
                        int overWriteLen = (start + candiateLen);

                        Array.ConstrainedCopy(val, 0, _rowData, start, overWriteLen);

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
                    int iterateLen = (start + canidateLen);

                    Array.ConstrainedCopy(val, 0, _rowData, start, iterateLen);

                    // finally, update the storage data
                    _startIdx[idx] = start;
                    _columnLen[idx] = canidateLen;
                    _usedStorage += canidateLen;

                }
            }
            else
            {
                // clear the existing storage requirements out ;)
                _startIdx[idx] = DataListConstants.EmptyRowStartIdx;
                _columnLen[idx] = DataListConstants.EmptyRowStartIdx;
                // TODO : Blank the data too ;)
            }
        }
    }
}
