#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{    
    public class DataMergeDTO : ValidatedObject, IDev2TOFn    
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

        public DataMergeDTO(string inputVariable, string mergeType, string at, int indexNum, string padding, string alignment)
            : this(inputVariable, mergeType, at, indexNum, padding, alignment, false)
        {
        }

        public DataMergeDTO(string inputVariable, string mergeType, string at, int indexNum, string padding, string alignment, bool inserted)
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

        public bool IsPaddingFocused { get => _isPaddingFocused; set => OnPropertyChanged(ref _isPaddingFocused, value); }

        public bool IsAtFocused { get => _isAtFocused; set => OnPropertyChanged(ref _isAtFocused, value); }

        public bool IsFieldNameFocused { get => _isFieldNameFocused; set => OnPropertyChanged(ref _isFieldNameFocused, value); }

        public bool Inserted { get; set; }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            
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

        public bool EnableAt { get => _enableAt; set => OnPropertyChanged(ref _enableAt, value); }

        public bool EnablePadding { get => _enablePadding; set => OnPropertyChanged(ref _enablePadding, value); }

        public int IndexNumber { get => _indexNum; set => OnPropertyChanged(ref _indexNum, value); }

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

        public bool CanAdd() => !((MergeType == MergeTypeIndex || MergeType == MergeTypeChars) && string.IsNullOrEmpty(InputVariable) && string.IsNullOrEmpty(At));

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

        public bool IsEmpty() => string.IsNullOrEmpty(InputVariable) && MergeType == MergeTypeIndex && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(InputVariable) && MergeType == MergeTypeChars && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(InputVariable) && MergeType == MergeTypeNone && string.IsNullOrEmpty(At);

        #endregion

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();
            if (IsEmpty())
            {
                return ruleSet;
            }
            switch (propertyName)
            {
                case "Input":
                    if (!string.IsNullOrEmpty(InputVariable))
                    {
                        var inputExprRule = new IsValidExpressionRule(() => InputVariable, datalist, "0", new VariableUtils());
                        ruleSet.Add(inputExprRule);
                    }
                    else
                    {
                        ruleSet.Add(new IsStringEmptyRule(() => InputVariable));
                    }

                    break;
                case "At":
                    if (MergeType == MergeTypeIndex)
                    {
                        var atExprRule = new IsValidExpressionRule(() => At, datalist, "1", new VariableUtils());
                        ruleSet.Add(atExprRule);

                        ruleSet.Add(new IsStringEmptyRule(() => atExprRule.ExpressionValue));
                        ruleSet.Add(new IsPositiveNumberRule(() => atExprRule.ExpressionValue));
                    }
                    break;
                case "Padding":
                    if (!string.IsNullOrEmpty(Padding))
                    {
                        var paddingExprRule = new IsValidExpressionRule(() => Padding, datalist, "0", new VariableUtils());
                        ruleSet.Add(paddingExprRule);

                        ruleSet.Add(new IsSingleCharRule(() => paddingExprRule.ExpressionValue));
                    }
                    break;
                default:
                    return ruleSet;
            }
            return ruleSet;
        }
    }
}
