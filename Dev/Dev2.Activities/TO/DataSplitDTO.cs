using Dev2.DataList.Contract;
using Dev2.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Util;
using Dev2.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DataSplitDTO : INotifyPropertyChanged, IDev2TOFn, IOutputTOConvert
    {
        private string _outputVariable;
        private string _splitType;
        private string _at;
        private int _indexNum;
        private bool _enableAt;
        private bool _include;
        private List<string> _outList;

        public DataSplitDTO()
        {

        }

        public DataSplitDTO(string outputVariable, string splitType, string at, int indexNum, bool include = false)
        {
            OutputVariable = outputVariable;
            SplitType = splitType;
            At = at;
            IndexNumber = indexNum;
            Include = include;
            _enableAt = true;
            _outList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        public bool EnableAt
        {
            get
            {
                return _enableAt;
            }
            set
            {
                _enableAt = value;
                OnPropertyChanged("EnableAt");
            }
        }

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged("IndexNum");
            }
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

        public bool Include
        {
            get
            {
                return _include;
            }
            set
            {
                _include = value;
                OnPropertyChanged("Include");
            }
        }

        [FindMissing]
        public string OutputVariable
        {
            get
            {
                return _outputVariable;
            }
            set
            {
                _outputVariable = value;
                OnPropertyChanged("OutputVariable");
            }
        }

        public string SplitType
        {
            get
            {
                return _splitType;
            }
            set
            {
                _splitType = value;
                OnPropertyChanged("SplitType");
            }
        }

        [FindMissing]
        public string At
        {
            get
            {
                return _at;
            }
            set
            {
                _at = value;
                OnPropertyChanged("At");
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
            if (SplitType == "Index" || SplitType == "Chars")
            {
                if (string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CanAdd()
        {
            bool result = true;
            if (SplitType == "Index" || SplitType == "Chars")
            {
                if (string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    result = false;
                }
            }
            return result;
        }

        public void ClearRow()
        {
            OutputVariable = string.Empty;
            SplitType = "Char";
            At = string.Empty;
            Include = false;
        }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(OutputVariable, OutList);
        }
    }
}
