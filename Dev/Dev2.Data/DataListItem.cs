/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract.Binary_Objects
{

    // Not sure this belongs here, should be in the studio?
    public class DataListItem : IDataListItem, INotifyPropertyChanged
    {
        #region Fields

        private string _field;
        private string _recordset;
        private string _displayValue;
        private bool _canHaveMutipleRows;
        private string _index;
        private string _value;
        private enRecordsetIndexType _recordsetIndexType;
        private bool _isObject;

        #endregion Fields

        #region Properties

        public string Description { get; set; }

        public string Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
                OnNotifyPropertyChange("Field");
            }
        }

        public string Recordset
        {
            get
            {
                return _recordset;
            }
            set
            {
                _recordset = value;
                OnNotifyPropertyChange("Recordset");
            }
        }

        public string Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                OnNotifyPropertyChange("Index");
            }
        }

        public enRecordsetIndexType RecordsetIndexType
        {
            get
            {
                return _recordsetIndexType;
            }
            set
            {
                _recordsetIndexType = value;
                OnNotifyPropertyChange("RecordsetIndexType");
            }
        }

        public bool CanHaveMutipleRows
        {
            get
            {
                return _canHaveMutipleRows;
            }
            set
            {
                _canHaveMutipleRows = value;
                OnNotifyPropertyChange("CanHaveMutipleRows");
            }
        }
        public string ParentName { get; set; }
        public bool IsObject
        {
            get
            {
                return _isObject;
            }
            set
            {
                _isObject = value;
                OnNotifyPropertyChange("IsObject");
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnNotifyPropertyChange("Value");
            }
        }

        public string DisplayValue
        {
            get
            {
                return _displayValue;
            }
            set
            {
                _displayValue = value;
                OnNotifyPropertyChange("DisplayValue");
            }
        }

        #endregion Properties

        #region Methods



        #endregion Methods

        #region Private Methods



        #endregion Private Methods

        #region INotifyPropertyChanged Implementaion


        protected void OnNotifyPropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Implementaion
    }
}
