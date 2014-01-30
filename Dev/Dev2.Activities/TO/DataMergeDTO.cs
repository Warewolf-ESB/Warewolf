using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DataMergeDTO : IDev2TOFn, IPerformsValidation
    {
        #region Fields

        string _inputVariable;
        string _mergeType;
        string _at;
        int _indexNum;
        bool _enableAt;
        Dictionary<string, List<IActionableErrorInfo>> _errors;

        #endregion

        #region Ctor
        public DataMergeDTO(string inputVariable, string mergeType, string at, int indexNum, string padding, string alignment, bool inserted = false)
        {
            Inserted = inserted;
            InputVariable = inputVariable;
            MergeType = string.IsNullOrEmpty(mergeType) ? "Index" : mergeType;
            At = string.IsNullOrEmpty(at) ? string.Empty : at;
            IndexNumber = indexNum;
            _enableAt = true;
            Padding = string.IsNullOrEmpty(padding) ? string.Empty : padding;
            Alignment = string.IsNullOrEmpty(alignment) ? "Left" : alignment;
        }

        public DataMergeDTO()
        {
            Errors = new Dictionary<string, List<IActionableErrorInfo>>();
        }

        #endregion

        #region Properties

        public bool Inserted { get; set; }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        [FindMissing]
        public string Padding { get; set; }

        public string Alignment { get; set; }

        public bool EnableAt
        {
            get { return _enableAt; }
            set
            {
                _enableAt = value;
                OnPropertyChanged("EnableAt");
            }
        }

        public int IndexNumber
        {
            get { return _indexNum; }
            set
            {
                _indexNum = value;
                OnPropertyChanged("IndexNum");
            }
        }

        [FindMissing]
        public string InputVariable
        {
            get { return _inputVariable; }
            set
            {
                _inputVariable = value;
                OnPropertyChanged("InputVariable");
                RaiseCanAddRemoveChanged();
            }
        }

        public string MergeType
        {
            get { return _mergeType; }
            set
            {
                if(value != null)
                {
                    _mergeType = value;
                    OnPropertyChanged("MergeType");
                    RaiseCanAddRemoveChanged();
                }
            }
        }

        [FindMissing]
        public string At
        {
            get { return _at; }
            set
            {
                _at = value;
                OnPropertyChanged("At");
                RaiseCanAddRemoveChanged();
            }
        }

        #endregion

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region CanAdd, CanRemove and ClearRow

        public bool CanRemove()
        {
            if(MergeType == "Index" || MergeType == "Chars")
            {
                if(string.IsNullOrEmpty(InputVariable) && string.IsNullOrEmpty(At))
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
            if(MergeType == "Index" || MergeType == "Chars")
            {
                if(string.IsNullOrEmpty(InputVariable) && string.IsNullOrEmpty(At))
                {
                    result = false;
                }
            }
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
            if(InputVariable == string.Empty && MergeType == "Index" && string.IsNullOrEmpty(At) || InputVariable == string.Empty && MergeType == "Chars" && string.IsNullOrEmpty(At) || InputVariable == string.Empty && MergeType == "None" && string.IsNullOrEmpty(At))
            {
                return true;
            }
            else
            {
                return false;
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

        public void Validate()
        {
        }

        RuleSet GetFieldNameRuleSet()
        {
            var ruleSet = new RuleSet();
            return ruleSet;
        }
        #endregion
    }
}
