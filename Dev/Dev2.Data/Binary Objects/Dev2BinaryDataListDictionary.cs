using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class Dev2BinaryDataListDictionary
    {
        private IDictionary<int, IDictionary<int, IList<IBinaryDataListItem>>> _masterData;
        //private BinaryDataListDictionary<BinaryDataListDictionary<IList<IBinaryDataListItem>>> _masterData; 
        //private IList<int> _populatedKeys;
        private IndexList _populatedKeys;

        public Dev2BinaryDataListDictionary()
        {
            _masterData = new Dictionary<int, IDictionary<int, IList<IBinaryDataListItem>>>(GlobalConstants.DefaultDataListCacheSizeLvl1);
            //_masterData = new BinaryDataListDictionary<BinaryDataListDictionary<IList<IBinaryDataListItem>>>();
            _populatedKeys = new IndexList();
        }

        public IList<IBinaryDataListItem> this[int key]
        {
            get
            {
                IList<IBinaryDataListItem> v;
                TryGetValue(key, out v);
                return v;
            }
            set
            {

                int mIdx = FetchMasterIndexKey(key);
                int sIdx = FetchSlaveIndexKey(key);

                //if (_masterData[mIdx] != null)
                if (_masterData.ContainsKey(mIdx))
                {

                    _masterData[mIdx][sIdx] = value;

                    if (value != null)
                    {
                        _populatedKeys.SetMaxValue(key);
                    }
                    else
                    {
                        _populatedKeys.AddGap(key);
                    }
                }
                else
                {
                    _masterData[mIdx] = new Dictionary<int, IList<IBinaryDataListItem>>(GlobalConstants.DefaultDataListCacheSizeLvl1);
                    //_masterData[mIdx] = new BinaryDataListDictionary<IList<IBinaryDataListItem>>();
                    _masterData[mIdx][sIdx] = new List<IBinaryDataListItem>(GlobalConstants.DefaultColumnSizeLvl1);
                    _masterData[mIdx][sIdx] = value;

                    _populatedKeys.SetMaxValue(key);
                }

            }
        }

        public bool TryGetValue(int key, out IList<IBinaryDataListItem> value)
        {
            int mIdx = FetchMasterIndexKey(key);
            int sIdx = FetchSlaveIndexKey(key);
            bool r = false;

            //if(_masterData[mIdx] == null){
            if (!_masterData.ContainsKey(mIdx))
            {
                _masterData[mIdx] = new Dictionary<int, IList<IBinaryDataListItem>>(GlobalConstants.DefaultDataListCacheSizeLvl1);
                //_masterData[mIdx] = new BinaryDataListDictionary<IList<IBinaryDataListItem>>();
                _masterData[mIdx][sIdx] = new List<IBinaryDataListItem>(GlobalConstants.DefaultColumnSizeLvl1);
                value = _masterData[mIdx][sIdx];
            }
            else
            {
                r = true;

                // 1, 2, 4, insert at 3
                if (_populatedKeys.Contains(key))
                {
                    try
                    {
                        value = _masterData[mIdx][sIdx];
                    }
                    catch (Exception)
                    {
                        value = null;
                    }

                    if (value == null)
                    {
                        r = false;
                    }
                }
                else
                {
                    r = false;
                    value = null;
                }

            }

            return r;
        }


        public void Add(int key, IList<IBinaryDataListItem> value)
        {
            this[key] = value;
        }

        public bool ContainsKey(int key)
        {
            return (_populatedKeys.Contains(key));
        }

        // TODO: Replace References ;)
        public IIndexIterator Keys
        {
            get { return _populatedKeys.FetchIterator(); }
        }

        public bool Remove(int key)
        {
            this[key] = null;

            return true;
        }

        public ICollection<IList<IBinaryDataListItem>> Values
        {
            get { throw new NotImplementedException(); }
        }

        public void Add(KeyValuePair<int, IList<IBinaryDataListItem>> item)
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            _populatedKeys = new IndexList();
            _masterData = new Dictionary<int, IDictionary<int, IList<IBinaryDataListItem>>>(GlobalConstants.DefaultColumnSizeLvl1);
            //_masterData = new BinaryDataListDictionary<BinaryDataListDictionary<IList<IBinaryDataListItem>>>();
            //_masterData = new List<IList<IList<IBinaryDataListItem>>>(_slabSize);
        }

        public bool Contains(KeyValuePair<int, IList<IBinaryDataListItem>> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return _populatedKeys.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<int, IList<IBinaryDataListItem>> item)
        {
            this[item.Key] = null;

            return true;
        }

        #region Private Methods
        private int FetchMasterIndexKey(int idx)
        {
            int lookIdx = (idx / GlobalConstants.DefaultCachePageSizeLvl1);

            return lookIdx;
        }

        private int FetchSlaveIndexKey(int idx)
        {
            int lookIdx = FetchMasterIndexKey(idx);
            int result = 1;

            if (lookIdx > 0)
            {
                result -= (lookIdx * GlobalConstants.DefaultCachePageSizeLvl1);
            }
            else
            {
                result = idx;
            }

            return result;
        }

        #endregion



    }

}
