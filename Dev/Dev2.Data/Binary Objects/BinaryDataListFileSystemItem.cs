using System;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects.Structs;
using Dev2.PathOperations;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class BinaryDataListFileSystemItem : IBinaryDataListItem
    {
        #region Internal Struct

        private SBinaryDataListItem _internalObj;

        private string _fileData;
        private readonly string _filePath;

        #endregion

        #region Properties

        // Properly return the data from the file system on demand
        public string TheValue
        {

            get
            {
                if(string.IsNullOrEmpty(_fileData))
                {
                    // Use File Broker ;)
                    BinaryDataListUtil bdlUtil = new BinaryDataListUtil();
                    IActivityIOOperationsEndPoint endPoint = bdlUtil.DeserializeDeferredItem<IActivityIOOperationsEndPoint>(_internalObj.TheValue);

                    IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                    _fileData = broker.Get(endPoint, false);
                }

                return _fileData;
            }

            set
            {
                _fileData = value;
            }
        }

        public int ItemCollectionIndex { get { return _internalObj.ItemCollectionIndex; } private set { _internalObj.ItemCollectionIndex = value; } }

        public string Namespace { get { return _internalObj.Namespace; } private set { _internalObj.Namespace = value; } }

        public string FieldName { get { return _internalObj.FieldName; } private set { _internalObj.FieldName = value; } }


        public string DisplayValue
        {
            get
            {
                if(_internalObj.ItemCollectionIndex >= 0 && _internalObj.DisplayValue.Length < 1)
                {
                    _internalObj.DisplayValue = DataListUtil.ComposeIntoUserVisibleRecordset(_internalObj.Namespace, _internalObj.ItemCollectionIndex, _internalObj.FieldName);
                }

                if(_internalObj.DisplayValue == null)
                {
                    return "null";
                }

                return _internalObj.DisplayValue;
            }
        }

        public bool IsDeferredRead { get { return true; } set { throw new NotImplementedException(); } }

        #endregion Properties

        #region Internal Methods

        internal BinaryDataListFileSystemItem(string base64Obj, string filePath, string ns, string field, string idx)
        {
            _internalObj.TheValue = base64Obj;
            _internalObj.Namespace = String.IsNullOrEmpty(ns) ? GlobalConstants.NullEntryNamespace : ns;
            _internalObj.FieldName = field;
            _filePath = filePath;
            int tmp;
            if(Int32.TryParse(idx, out tmp))
            {
                _internalObj.ItemCollectionIndex = tmp;
            }
            else
            {
                _internalObj.ItemCollectionIndex = Int32.MinValue;
            }
        }

        internal BinaryDataListFileSystemItem(string base64Obj, string filePath, string ns, string field, int idx)
        {
            _internalObj.TheValue = base64Obj;
            _filePath = filePath;
            _internalObj.Namespace = String.IsNullOrEmpty(ns) ? GlobalConstants.NullEntryNamespace : ns;
            _internalObj.FieldName = field;
            _internalObj.ItemCollectionIndex = idx;
        }

        internal BinaryDataListFileSystemItem(string base64Obj, string filePath, string fieldName)
        {
            _internalObj.TheValue = base64Obj;
            _filePath = filePath;
            _internalObj.Namespace = GlobalConstants.NullEntryNamespace;
            _internalObj.FieldName = fieldName;
            _internalObj.ItemCollectionIndex = -1;
            _internalObj.DisplayValue = fieldName;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public IBinaryDataListItem Clone()
        {
            IBinaryDataListItem result;

            if(_internalObj.ItemCollectionIndex > 0)
            {
                result = new BinaryDataListFileSystemItem(_internalObj.TheValue, _filePath, _internalObj.Namespace, _internalObj.FieldName, _internalObj.ItemCollectionIndex);
            }
            else
            {
                result = new BinaryDataListFileSystemItem(_internalObj.TheValue, _filePath, _internalObj.FieldName);
            }

            return result;
        }

        public void HtmlEncodeRegionBrackets()
        {
            throw new NotImplementedException("FileSystem Item Cannot Add Brackets!");
        }

        public void UpdateValue(string val)
        {
            TheValue = val;
        }

        public void UpdateField(string val)
        {
            FieldName = val;
        }

        public void UpdateRecordset(string val)
        {
            Namespace = val;
        }

        public void UpdateIndex(int idx)
        {
            ItemCollectionIndex = idx;
        }

        public string FetchDeferredLocation()
        {
            StringBuilder result = new StringBuilder("Contents not visible because this value is a deferred read of  '");

            result.Append(_filePath);
            result.Append("'");
            return result.ToString();
        }

        #endregion Internal Methods


        public void ToClear()
        {
            // Do nothing...
        }
    }
}
