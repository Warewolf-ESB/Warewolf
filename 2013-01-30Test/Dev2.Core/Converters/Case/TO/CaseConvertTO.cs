using Dev2.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dev2
{
    public class CaseConvertTO : ICaseConvertTO
    {
        #region Fields

        private string _result;
        private string _stringToConvert;
        private string _convertType;
        private IList<string> _expressions;

        #endregion Fields

        #region Ctor

        public CaseConvertTO()
        {
        }

        public CaseConvertTO(string stringToConvert, string convertType, string result, int indexNumber)
        {
            StringToConvert = stringToConvert;
            ConvertType = convertType;
            Result = result;
            IndexNumber = indexNumber;
        }

        #endregion Ctor

        #region Properties

        public string StringToConvert
        {
            get
            {
                return _stringToConvert;
            }
            set
            {
                _stringToConvert = value;
                OnPropertyChanged("StringToConvert");
            }
        }

        public string ConvertType
        {
            get
            {
                return _convertType;
            }
            set
            {
                _convertType = value;
                OnPropertyChanged("ConvertType");
            }
        }

        public IList<string> Expressions
        {
            get
            {
                return _expressions;
            }
            set
            {
                _expressions = value;
            }
        }

        public string Result
        {
            get
            {
                //Add the below code when the wizard comes in
                //if (string.IsNullOrWhiteSpace(_result))
                //{
                _result = StringToConvert;
                //}
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public int IndexNumber { get; set; }

        public string ExpressionToConvert { get; set; }

        public string WatermarkTextVariable { get; set; }

        #endregion Properties

        public bool CanRemove()
        {
            bool result = false;
            if (string.IsNullOrEmpty(StringToConvert))
            {
                result = true;
            }
            return result;
        }

        public bool CanAdd()
        {
            bool result = true;
            if (string.IsNullOrEmpty(StringToConvert))
            {
                result = false;
            }
            return result;
        }

        public void ClearRow()
        {
            StringToConvert = string.Empty;
            ConvertType = "UPPER";
            Result = string.Empty;
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
