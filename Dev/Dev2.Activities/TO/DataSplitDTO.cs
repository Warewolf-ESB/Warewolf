using System.Collections.Generic;
using System.ComponentModel;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DataSplitDTO : IDev2TOFn, IOutputTOConvert, IPerformsValidation
    {
        private string _outputVariable;
        private string _splitType;
        private string _at;
        private int _indexNum;
        private bool _enableAt;
        private bool _include;
        private List<string> _outList;
        Dictionary<string, List<IActionableErrorInfo>> _errors;
        string _escapeChar;

        public DataSplitDTO()
        {
            Errors = new Dictionary<string, List<IActionableErrorInfo>>();
        }

        public DataSplitDTO(string outputVariable, string splitType, string at, int indexNum, bool include = false, bool inserted = false)
        {
            Inserted = inserted;
            OutputVariable = outputVariable;
            SplitType = string.IsNullOrEmpty(splitType) ? "Index" : splitType;
            At = string.IsNullOrEmpty(at) ? string.Empty : at;
            IndexNumber = indexNum;
            Include = include;
            _enableAt = true;
            _outList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

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
        public string EscapeChar
        {
            get { return _escapeChar; }
            set
            {
                _escapeChar = value;
                OnPropertyChanged("EscapeChar");
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
                RaiseCanAddRemoveChanged();
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
                if(value != null)
                {
                    _splitType = value;
                    OnPropertyChanged("SplitType");
                    RaiseCanAddRemoveChanged();
                }
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
                RaiseCanAddRemoveChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool CanRemove()
        {
            if(SplitType == "Index" || SplitType == "Chars")
            {
                if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        public bool CanAdd()
        {
            bool result = true;
            if(SplitType == "Index" || SplitType == "Chars")
            {
                if(string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
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
            EscapeChar = string.Empty;
        }

        public bool Inserted { get; set; }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(OutputVariable, OutList);
        }

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName] { get { return null; } }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; private set; }

        #endregion

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            if(ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }
            else
            {
                var errorsTos = ruleSet.ValidateRules();
                var actionableErrorInfos = errorsTos.ConvertAll<IActionableErrorInfo>(input => new ActionableErrorInfo(input, () =>
                {
                    //
                }));
                Errors[propertyName] = actionableErrorInfos;
            }
            OnPropertyChanged("Errors");
            List<IActionableErrorInfo> errorList;
            if(Errors.TryGetValue(propertyName, out errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public bool Validate(string propertyName)
        {
            RuleSet ruleSet = null;
            switch(propertyName)
            {
                case "FieldName":
                    ruleSet = GetFieldNameRuleSet();
                    break;
                case "FieldValue":
                    break;
            }
            return Validate(propertyName, ruleSet);
        }

        RuleSet GetFieldNameRuleSet()
        {
            var ruleSet = new RuleSet();
            return ruleSet;
        }
        #endregion
    }
}
