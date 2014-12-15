
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Interfaces;
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
        public const string MergeTypeIndex = "Index";
        public const string MergeTypeChars = "Chars";
        public const string MergeTypeNone = "None";

        public const string AlignmentLeft = "Left";

        #region Fields

        string _inputVariable;
        string _mergeType;
        string _at;
        int _indexNum;
        bool _enableAt;
        bool _isAtFocused;
        bool _isFieldNameFocused;
        bool _enablePadding;
        bool _isPaddingFocused;
        string _alignment;

        #endregion

        #region Ctor

        public DataMergeDTO(string inputVariable, string mergeType, string at, int indexNum, string padding, string alignment, bool inserted = false)
        {
            Inserted = inserted;

            InputVariable = string.IsNullOrEmpty(inputVariable) ? string.Empty : inputVariable;
            MergeType = string.IsNullOrEmpty(mergeType) ? MergeTypeIndex : mergeType;
            At = string.IsNullOrEmpty(at) ? string.Empty : at;
            IndexNumber = indexNum;
            _enableAt = true;
            Padding = string.IsNullOrEmpty(padding) ? string.Empty : padding;
            Alignment = string.IsNullOrEmpty(alignment) ? AlignmentLeft : alignment;

        }

        public DataMergeDTO()
        {
            Errors = new Dictionary<string, List<IActionableErrorInfo>>();
            MergeType = MergeTypeIndex;
            _enableAt = true;
        }

        #endregion

        #region Properties

        public bool IsPaddingFocused { get { return _isPaddingFocused; } set { OnPropertyChanged(ref _isPaddingFocused, value); } }

        public bool IsAtFocused { get { return _isAtFocused; } set { OnPropertyChanged(ref _isAtFocused, value); } }

        public bool IsFieldNameFocused { get { return _isFieldNameFocused; } set { OnPropertyChanged(ref _isFieldNameFocused, value); } }

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
                if (value != null)
                {
                    OnPropertyChanged(ref _alignment, value);
                }
            }
        }

        public bool EnableAt { get { return _enableAt; } set { OnPropertyChanged(ref _enableAt, value); } }

        public bool EnablePadding { get { return _enablePadding; } set { OnPropertyChanged(ref _enablePadding, value); } }

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
                if (value != null)
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
            if (MergeType == MergeTypeIndex || MergeType == MergeTypeChars)
            {
                if (string.IsNullOrEmpty(InputVariable) && string.IsNullOrEmpty(At))
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
            if (MergeType == MergeTypeIndex || MergeType == MergeTypeChars)
            {
                if (string.IsNullOrEmpty(InputVariable) && string.IsNullOrEmpty(At))
                {
                    result = false;
                }
            }
            return result;
        }

        public void ClearRow()
        {
            Padding = string.Empty;
            Alignment = AlignmentLeft;
            InputVariable = string.Empty;
            MergeType = MergeTypeChars;
            At = string.Empty;
        }

        #endregion

        #region IsEmpty

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(InputVariable) && MergeType == MergeTypeIndex && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(InputVariable) && MergeType == MergeTypeChars && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(InputVariable) && MergeType == MergeTypeNone && string.IsNullOrEmpty(At);
        }

        #endregion

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            RuleSet ruleSet = new RuleSet();
            if (IsEmpty())
            {
                return ruleSet;
            }
            switch (propertyName)
            {
                case "Input":
                    if (!string.IsNullOrEmpty(InputVariable))
                    {
                        var inputExprRule = new IsValidExpressionRule(() => InputVariable, datalist, "0");
                        ruleSet.Add(inputExprRule);
                    }
                    else
                        ruleSet.Add(new IsStringEmptyRule(() => InputVariable));
                    break;
                case "At":
                    if (MergeType == MergeTypeIndex)
                    {
                        var atExprRule = new IsValidExpressionRule(() => At, datalist, "1");
                        ruleSet.Add(atExprRule);

                        ruleSet.Add(new IsStringEmptyRule(() => atExprRule.ExpressionValue));
                        ruleSet.Add(new IsPositiveNumberRule(() => atExprRule.ExpressionValue));
                    }
                    break;
                case "Padding":
                    if (!string.IsNullOrEmpty(Padding))
                    {
                        var paddingExprRule = new IsValidExpressionRule(() => Padding, datalist, "0");
                        ruleSet.Add(paddingExprRule);

                        ruleSet.Add(new IsSingleCharRule(() => paddingExprRule.ExpressionValue));
                    }
                    break;
            }
            return ruleSet;
        }
    }
}
