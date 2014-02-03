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
        string _alignment;

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

        public bool IsPaddingFocused { get { return _isPaddingFocused; } set { OnPropertyChanged(ref _isPaddingFocused, value); } }

        public bool IsAtFocused { get { return _isAtFocused; } set { OnPropertyChanged(ref _isAtFocused, value); } }

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

        public string Alignment
        {
            get { return _alignment; }
            set
            {
                if(value != null)
                {
                    OnPropertyChanged(ref _alignment, value);
                }
            }
        }

        public bool EnableAt { get { return _enableAt; } set { OnPropertyChanged(ref _enableAt, value); } }

        public int IndexNumber { get { return _indexNum; } set { OnPropertyChanged(ref _indexNum, value); } }

        [FindMissing]
        public string InputVariable
        {
            get { return _inputVariable; }
            set
            {
                OnPropertyChanged(ref _inputVariable, value);
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
                    OnPropertyChanged(ref _mergeType, value);
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
                OnPropertyChanged(ref _at, value);
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

        public override IRuleSet GetRuleSet(string propertyName)
        {
            RuleSet ruleSet = new RuleSet();
            if(IsEmpty())
            {
                return ruleSet;
            }
            switch(propertyName)
            {
                case "At":
                    if(MergeType == "Index")
                    {
                        var atExprRule = new IsValidExpressionRule(() => At, "1");
                        ruleSet.Add(atExprRule);

                        ruleSet.Add(new IsStringNullOrEmptyRule(() => atExprRule.ExpressionValue));
                        ruleSet.Add(new IsNumericRule(() => atExprRule.ExpressionValue));
                        ruleSet.Add(new IsPositiveNumberRule(() => atExprRule.ExpressionValue));
                    }
                    break;
                case "Padding":
                    if(!string.IsNullOrEmpty(Padding))
                    {
                        var paddingExprRule = new IsValidExpressionRule(() => Padding, "1");
                        ruleSet.Add(paddingExprRule);

                        ruleSet.Add(new IsNumericRule(() => paddingExprRule.ExpressionValue));
                        ruleSet.Add(new IsPositiveNumberRule(() => paddingExprRule.ExpressionValue));
                    }
                    break;
            }
            return ruleSet;
        }
    }
}
