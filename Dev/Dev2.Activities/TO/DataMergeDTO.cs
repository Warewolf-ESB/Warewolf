using Dev2.Interfaces;
using System.ComponentModel;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DataMergeDTO : INotifyPropertyChanged, IDev2TOFn
    {
        #region Fields

        private string _inputVariable;
        private string _mergeType;
        private string _at;
        private int _indexNum;
        private bool _enableAt;

        #endregion

        #region Ctor

        public DataMergeDTO()
        {

        }

        public DataMergeDTO(string inputVariable, string mergeType, string at, int indexNum, string padding, string alignment)
        {
            InputVariable = inputVariable;
            MergeType = mergeType;
            At = at;
            IndexNumber = indexNum;
            _enableAt = true;
            Padding = padding;
            Alignment = alignment;
        }

        #endregion

        #region Properties

        public string WatermarkTextVariable { get; set; }

        public string Padding { get; set; }

        public string Alignment { get; set; }

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

        public string InputVariable
        {
            get
            {
                return _inputVariable;
            }
            set
            {
                _inputVariable = value;
                OnPropertyChanged("InputVariable");
            }
        }

        public string MergeType
        {
            get
            {
                return _mergeType;
            }
            set
            {
                _mergeType = value;
                OnPropertyChanged("MergeType");
            }
        }

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

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region CanAdd, CanRemove and ClearRow

        public bool CanRemove()
        {
            if (MergeType == "Index" || MergeType == "Chars")
            {
                if (string.IsNullOrEmpty(InputVariable) && string.IsNullOrEmpty(At))
                {
                    return true;
                }
            }
            else if (MergeType == "None")
            {
                return true;
            }
            return false;
        }

        public bool CanAdd()
        {
            bool result = !string.IsNullOrEmpty(InputVariable);
            return result;
        }

        public void ClearRow()
        {
            Padding = " ";
            Alignment = "Left";
            InputVariable = string.Empty;
            MergeType = "Char";
            At = string.Empty;
        }

        #endregion

        #region IsEmpty

        public bool IsEmpty()
        {
            if (InputVariable == string.Empty && MergeType == "Index" && string.IsNullOrEmpty(At) || InputVariable == string.Empty && MergeType == "Chars" && string.IsNullOrEmpty(At) || InputVariable == string.Empty && MergeType == "None" && string.IsNullOrEmpty(At))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
