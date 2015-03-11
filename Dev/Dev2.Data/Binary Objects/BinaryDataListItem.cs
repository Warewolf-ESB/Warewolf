
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Binary_Objects.Structs;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class BinaryDataListItem : IBinaryDataListItem
    {
        #region Internal Struct

        private SBinaryDataListItem _internalObj;

        #endregion

        #region Properties
        
        public string TheValue
        {
            get
            {
                var theValue = _internalObj.TheValue;
                if(theValue == null)
                {
                    var displayValue = String.IsNullOrEmpty(DisplayValue) ? FieldName : DisplayValue;
                    var variableName = DataListUtil.AddBracketsToValueIfNotExist(displayValue);
                    throw new NullValueInVariableException(string.Format("No Value assigned for: {0}", variableName), variableName);
                }
                return theValue;
            }
            set
            {
                _internalObj.TheValue = value;
            }
        }

        public int ItemCollectionIndex { get { return _internalObj.ItemCollectionIndex; } private set { _internalObj.ItemCollectionIndex = value; } }

        public string Namespace { get { return _internalObj.Namespace; } private set { _internalObj.Namespace = value; } }

        public string FieldName { get { return _internalObj.FieldName; } private set { _internalObj.FieldName = value; } }

        public string DisplayValue
        {
            get
            {
                if(_internalObj.ItemCollectionIndex >= 0)
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

        public bool IsDeferredRead { get; set; }

        #endregion Properties

        #region Internal Methods

        internal BinaryDataListItem(string val, string ns, string field, string idx)
        {
            _internalObj.TheValue = val;
            _internalObj.Namespace = String.IsNullOrEmpty(ns) ? GlobalConstants.NullEntryNamespace : ns;
            _internalObj.FieldName = field;
            int tmp;
            _internalObj.ItemCollectionIndex = Int32.TryParse(idx, out tmp) ? tmp : Int32.MinValue;
        }

        internal BinaryDataListItem(string val, string ns, string field, int idx)
        {
            _internalObj.TheValue = val;
            _internalObj.Namespace = String.IsNullOrEmpty(ns) ? GlobalConstants.NullEntryNamespace : ns;
            _internalObj.FieldName = field;
            _internalObj.ItemCollectionIndex = idx;
        }

        internal BinaryDataListItem(string val, string fieldName)
        {
            _internalObj.TheValue = val;
            _internalObj.Namespace = GlobalConstants.NullEntryNamespace;
            _internalObj.FieldName = fieldName;
            _internalObj.ItemCollectionIndex = -1;
            _internalObj.DisplayValue = fieldName;
        }


        public void Clear()
        {
            _internalObj.TheValue = string.Empty;
            _internalObj.Namespace = string.Empty;
            _internalObj.FieldName = string.Empty;
            _internalObj.ItemCollectionIndex = -1;
            _internalObj.DisplayValue = string.Empty;
        }

        public void ToClear()
        {
            _internalObj.TheValue = string.Empty;
            _internalObj.ItemCollectionIndex = -1;
        }

        public IBinaryDataListItem Clone()
        {
            IBinaryDataListItem result;

            try
            {
                if(string.IsNullOrEmpty(_internalObj.TheValue) && string.IsNullOrEmpty(_internalObj.Namespace) && string.IsNullOrEmpty(_internalObj.FieldName))
                {
                    return this;
                }

                result = _internalObj.ItemCollectionIndex > 0 ? new BinaryDataListItem(_internalObj.TheValue, _internalObj.Namespace, _internalObj.FieldName, _internalObj.ItemCollectionIndex) : new BinaryDataListItem(_internalObj.TheValue, _internalObj.FieldName);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
                return this;
            }

            return result;
        }

        public void HtmlEncodeRegionBrackets()
        {
            _internalObj.TheValue = DataListUtil.HtmlEncodeRegionBrackets(_internalObj.TheValue);
        }

        public void UpdateValue(string val)
        {
            IsDeferredRead = false; // If the value is ever updated then turn off deffered read
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
            // BUG 9144
            _internalObj.ItemCollectionIndex = idx;
        }

        public string FetchDeferredLocation()
        {
            throw new NotImplementedException("Standard BinaryDataList Item is does not support this feature");
        }

        #endregion Internal Methods
    }
}
