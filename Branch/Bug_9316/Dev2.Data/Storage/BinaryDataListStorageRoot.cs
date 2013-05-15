using System;
using Dev2.Data.Storge;
using Dev2.Data.SystemTemplates;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// Root storage object for re-use ;)
    /// T is the storage type
    /// RT is the return type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="RT">The type of the T.</typeparam>
    [Serializable]
    public abstract class BinaryDataListStorageRoot<T, RT> : IBinaryDataListStorage<RT>
    {
        internal T[] _rowData;
        internal int[] _startIdx;
        internal int[] _columnLen;

        internal int _colCnt;
        internal int _storageCapacity;
        internal int _usedStorage;

        internal BinaryDataListStorageRoot(int columnCnt)
        {
            
            _startIdx = new int[columnCnt];
            _columnLen = new int[columnCnt];

            _colCnt = columnCnt;

            if(columnCnt < DataListConstants.MinRowSize)
            {
                _storageCapacity = DataListConstants.MinRowSize;
            }
            else
            {
                _storageCapacity = columnCnt;
            }

            _rowData = new T[_storageCapacity]; // est a single char per column
            _usedStorage = 0;

            Init();
        }

        public bool IsEmpty
        {
            get
            {
                return (_usedStorage == 0);
            }
        }

        #region Abstract Methods

        public abstract RT FetchValue(int idx);

        public abstract void UpdateValue(RT val, int idx);

        #endregion

        #region Private Methods

        private void Init()
        {
            // init start indexes and length
            for(int i = 0; i < _colCnt; i++)
            {
                _startIdx[i] = DataListConstants.EmptyRowStartIdx;
                _columnLen[i] = DataListConstants.EmptyRowStartIdx;
            }
        }

        /// <summary>
        /// Grows the row data storage capacity
        /// </summary>
        /// <param name="targetSize">Size of the target.</param>
        internal void GrowRow(int targetSize)
        {

            // TODO : Get fancy and reclaim storage ;)

            // grow it by a factor of 1.5 of the target size to avoid growing too often ;)
            int growthSize = (int)(targetSize * DataListConstants.RowGrowthFactor);

            T[] tmp = new T[growthSize];

            Array.Copy(_rowData,tmp, _usedStorage);

            _rowData = tmp;
            _storageCapacity = growthSize;
        }

        internal int FetchStorageStartIdx()
        {
            if(_usedStorage == 0)
            {
                return 0;
            }

            return (_usedStorage);
        }

        #endregion

    }
}
