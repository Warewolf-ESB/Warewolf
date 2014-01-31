using System.Collections.Generic;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    // ReSharper disable InconsistentNaming
    public class DataMergeDTO : ValidatedObject, IDev2TOFn
    // ReSharper restore InconsistentNaming
    {
        #region Fields

        string _inputVariable;
        string _mergeType;
        string _at;
        int _indexNum;
        bool _enableAt;
        bool _isAtFocused;
        bool _isPaddingFocused;

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

        public bool IsPaddingFocused
        {
            get
            {
                return _isPaddingFocused;
            }
            set
            {
                OnPropertyChanged(ref _isPaddingFocused, value);
            }
        }

        public bool IsAtFocused
        {
            get
            {
                return _isAtFocused;
            }
            set
            {
                OnPropertyChanged(ref _isAtFocused, value);
            }
        }

        public bool Inserted { get; set; }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
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
            }
        }

        public int IndexNumber
        {
            get { return _indexNum; }
            set
            {
                _indexNum = value;
            }
        }

        [FindMissing]
        public string InputVariable
        {
            get { return _inputVariable; }
            set
            {
                _inputVariable = value;
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
                RaiseCanAddRemoveChanged();
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
            return false;
        }

        #endregion


        #region Implementation of IPerformsValidation

        protected override RuleSet GetRuleSet(string propertyName)
        {
            RuleSet ruleSet = new RuleSet();
            switch(propertyName)
            {
                case "At":
                    if(MergeType == "Index" && !IsEmpty())
                    {
                        ruleSet.Add(new StringCannotBeEmptyOrNullRule(At, () => IsAtFocused = true));
                        if(At.StartsWith("[[") && At.EndsWith("]]"))
                        {
                            ruleSet.Add(new StringCannotBeInvalidExpressionRule(At, () => IsAtFocused = true));
                        }
                        else
                        {
                            ruleSet.Add(new IsNumericRule(At, () => IsAtFocused = true));
                            ruleSet.Add(new IsPositiveNumberRule(At, () => IsAtFocused = true));
                        }
                    }
                    break;
                case "Padding":
                    if(!string.IsNullOrEmpty(Padding))
                    {
                        if(Padding.StartsWith("[[") && Padding.EndsWith("]]"))
                        {
                            ruleSet.Add(new StringCannotBeInvalidExpressionRule(Padding, () => IsPaddingFocused = true));
                        }
                        else
                        {
                            ruleSet.Add(new IsNumericRule(Padding, () => IsPaddingFocused = true));
                            ruleSet.Add(new IsPositiveNumberRule(Padding, () => IsPaddingFocused = true));
                        }
                    }
                    break;
            }
            return ruleSet;
        }

        public override void Validate()
        {
            Validate("At");
            Validate("Padding");
        }

        #endregion
    }
}
