using Dev2.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Providers.Validation;

namespace Dev2
{
    public class CaseConvertTO : ICaseConvertTO, IPerformsValidation
    {
        #region Fields

        private string _result;
        private string _stringToConvert;
        private string _convertType;
        private IList<string> _expressions;
        Dictionary<string, List<IActionableErrorInfo>> _errors;

        #endregion Fields

        #region Ctor

        public CaseConvertTO()
        {
            Errors = new Dictionary<string, List<IActionableErrorInfo>>();
        }

        public CaseConvertTO(string stringToConvert, string convertType, string result, int indexNumber,bool inserted = false)
        {
            Inserted = inserted;
            StringToConvert = stringToConvert;
            ConvertType = string.IsNullOrEmpty(convertType) ? "UPPER" : convertType;
            Result = string.IsNullOrEmpty(result) ? string.Empty : result;
            IndexNumber = indexNumber;
        }

        #endregion Ctor

        #region Properties

        public bool Inserted { get; set; }

        [FindMissing]
        public string StringToConvert
        {
            get
            {
                return _stringToConvert;
            }
            set
            {
                _stringToConvert = value;
                _result = value; // This is set as the result for now as it is the same value till we do the advanced view.
                OnPropertyChanged("StringToConvert");
                OnPropertyChanged("Result");
                RaiseCanAddRemoveChanged();
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
                if (value != null)
                {
                    _convertType = value;
                    OnPropertyChanged("ConvertType");
                }
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

        [FindMissing]
        public string Result
        {
            get
            {
                //Add the below code when the wizard comes in
                if (string.IsNullOrWhiteSpace(_result))
                {
                    _result = StringToConvert;
                }
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public int IndexNumber { get; set; }

        [FindMissing]
        public string ExpressionToConvert { get; set; }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        #endregion Properties

        public bool CanRemove()
        {
            return string.IsNullOrWhiteSpace(StringToConvert);
        }

        public bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(StringToConvert);
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

        public bool Validate(string propertyName, IRuleSet ruleSet)
        {
            if (ruleSet == null)
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
            if (Errors.TryGetValue(propertyName, out errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public bool Validate(string propertyName, string datalist)
        {
            RuleSet ruleSet = null;
            switch (propertyName)
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
