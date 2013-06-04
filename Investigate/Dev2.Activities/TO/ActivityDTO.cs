using Dev2.DataList.Contract;
using Dev2.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Util;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class ActivityDTO : INotifyPropertyChanged, IDev2TOFn
    {
        private string _fieldName;
        private string _fieldValue;
        private int _indexNumber;
        private List<string> _outList;


        public ActivityDTO()
            : this("[[Variable]]", "Expression", 0)
        {

        }

        public ActivityDTO(string fieldName, string fieldValue, int indexNumber)
        {
            FieldName = fieldName;
            FieldValue = fieldValue;
            IndexNumber = indexNumber;
            _outList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkTextValue { get; set; }

        [FindMissing]
        public string FieldName
        {
            get
            {
                return _fieldName;

            }
            set
            {
                _fieldName = value;
                OnPropertyChanged("FieldName");
            }
        }

        [FindMissing]
        public string FieldValue
        {
            get
            {
                return _fieldValue;
            }
            set
            {
                _fieldValue = value;
                OnPropertyChanged("FieldValue");
            }
        }

        public int IndexNumber
        {
            get
            {
                return _indexNumber;
            }
            set
            {
                _indexNumber = value;
                OnPropertyChanged("IndexNumber");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public bool CanRemove()
        {
            bool result = false;
            if (string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue))
            {
                result = true;
            }
            return result;
        }


        public bool CanAdd()
        {
            bool result = true;
            if (string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue))
            {
                result = false;
            }
            return result;
        }

        public void ClearRow()
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
        }

        public List<string> OutList
        {
            get
            {
                return _outList;
            }
            set
            {
                _outList = value;
            }
        }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(FieldName, OutList);
        }
    }
}
