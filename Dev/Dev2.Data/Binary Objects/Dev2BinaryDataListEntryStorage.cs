using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Binary_Objects
{
    public class Dev2BinaryDataListEntryStorage
    {

        private static readonly string _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string _savePath = @"Dev2\DataListServer\_indexData\";
        private static readonly string _dataListPersistPath = Path.Combine(_rootPath, _savePath); //_debugLoc + "\\persistSettings.dat";

        public IList<IBinaryDataListItem> this[int key]
        {
            get { return null; }
        }

        public IIndexIterator Keys
        {
            get { return null; }
        }

        public ICollection<IList<IBinaryDataListItem>> Values
        {
            get { throw new NotImplementedException(); }
        }


        public bool TryGetValue(int key, out IList<IBinaryDataListItem> value)
        {
            value = null;
            return false;
        }

        public void Add(int key, IList<IBinaryDataListItem> value)
        {
            //this[key] = value;
        }

        public bool ContainsKey(int key)
        {
            //return (_populatedKeys.Contains(key));
            return false;
        }

        public bool Remove(int key)
        {
            //this[key] = null;

            return true;
        }

        public void Add(KeyValuePair<int, IList<IBinaryDataListItem>> item)
        {
            //this[item.Key] = item.Value;
        }

        public void Clear()
        {
            //_populatedKeys = new IndexList();
            //_masterData = new BinaryDataListDictionary<BinaryDataListDictionary<IList<IBinaryDataListItem>>>();
            //_masterData = new List<IList<IList<IBinaryDataListItem>>>(_slabSize);
        }

        public bool Contains(KeyValuePair<int, IList<IBinaryDataListItem>> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return 1; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<int, IList<IBinaryDataListItem>> item)
        {
            //this[item.Key] = null;

            return true;
        }
        
    }
}
