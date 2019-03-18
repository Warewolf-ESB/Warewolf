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
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;
using Dev2.Common;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DataSplitDTO : ValidatedObject, IDev2TOFn, IOutputTOConvert
    {
        public const string SplitTypeIndex = "Index";
        public const string SplitTypeChars = "Chars";
        public const string SplitTypeNewLine = "New Line";
        public const string SplitTypeSpace = "Space";
        public const string SplitTypeTab = "Tab";
        public const string SplitTypeEnd = "End";
        public const string SplitTypeNone = "None";

        string _outputVariable;
        string _splitType;
        string _at;
        int _indexNum;
        bool _enableAt;
        bool _include;
        string _escapeChar;
        bool _isEscapeCharFocused;
        bool _isOutputVariableFocused;
        bool _isAtFocused;
        bool _isEscapeCharEnabled;

        public DataSplitDTO()
        {
            SplitType = SplitTypeIndex;
            _enableAt = true;
            _isEscapeCharEnabled = true;
        }

        public DataSplitDTO(string outputVariable, string splitType, string at, int indexNum)
            : this(outputVariable, splitType, at, indexNum, false, false)
        {
        }

        public DataSplitDTO(string outputVariable, string splitType, string at, int indexNum, bool include, bool inserted)
        {
            Inserted = inserted;
            OutputVariable = outputVariable;
            SplitType = string.IsNullOrEmpty(splitType) ? SplitTypeIndex : splitType;
            At = string.IsNullOrEmpty(at) ? string.Empty : at;
            IndexNumber = indexNum;
            Include = include;
            _enableAt = splitType == "Index" || splitType == "Chars";
            _isEscapeCharEnabled = true;
            OutList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        public bool EnableAt { get => _enableAt; set => OnPropertyChanged(ref _enableAt, value); }

        public int IndexNumber { get => _indexNum; set => OnPropertyChanged(ref _indexNum, value); }

        public List<string> OutList { get; set; }

        public bool Include { get => _include; set => OnPropertyChanged(ref _include, value); }

        [FindMissing]
        public string EscapeChar { get => _escapeChar; set => OnPropertyChanged(ref _escapeChar, value); }

        public bool IsEscapeCharFocused { get => _isEscapeCharFocused; set => OnPropertyChanged(ref _isEscapeCharFocused, value); }

        public bool IsEscapeCharEnabled { get => _isEscapeCharEnabled; set => OnPropertyChanged(ref _isEscapeCharEnabled, value); }

        [FindMissing]
        public string OutputVariable
        {
            get => _outputVariable;
            set
            {
                OnPropertyChanged(ref _outputVariable, value);
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsOutputVariableFocused { get => _isOutputVariableFocused; set => OnPropertyChanged(ref _isOutputVariableFocused, value); }

        public string SplitType
        {
            get => _splitType;
            set
            {
                if (value != null)
                {
                    OnPropertyChanged(ref _splitType, value);
                    RaiseCanAddRemoveChanged();
                }
            }
        }

        [FindMissing]
        public string At
        {
            get => _at;
            set
            {
                OnPropertyChanged(ref _at, value);
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsAtFocused { get => _isAtFocused; set => OnPropertyChanged(ref _isAtFocused, value); }

        public bool CanRemove()
        {
            if (SplitType == SplitTypeIndex || SplitType == SplitTypeChars)
            {
                if (string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool CanAdd()
        {
            if (SplitType == SplitTypeIndex || SplitType == SplitTypeChars)
            {
                if (string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(At))
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        public void ClearRow()
        {
            OutputVariable = string.Empty;
            SplitType = SplitTypeChars;
            At = string.Empty;
            Include = false;
            EscapeChar = string.Empty;
        }

        public bool Inserted { get; set; }

        public bool IsEmpty() => string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeIndex && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeChars && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeNone && string.IsNullOrEmpty(At);

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();
            if (IsEmpty())
            {
                return ruleSet;
            }

            switch (propertyName)
            {
                case "OutputVariable":
                    if (!string.IsNullOrEmpty(OutputVariable))
                    {
                        var outputExprRule = new IsValidExpressionRule(() => OutputVariable, datalist, "0", new VariableUtils());
                        ruleSet.Add(outputExprRule);
                        ruleSet.Add(new IsValidExpressionRule(() => outputExprRule.ExpressionValue, datalist, new VariableUtils()));
                    }
                    break;
                case "At":
                    switch (SplitType)
                    {
                        case SplitTypeIndex:
                            var atIndexExprRule = new IsValidExpressionRule(() => At, datalist, "1", new VariableUtils());
                            ruleSet.Add(atIndexExprRule);
                            ruleSet.Add(new IsPositiveNumberRule(() => atIndexExprRule.ExpressionValue));
                            break;
                        case SplitTypeChars:
                            var atCharsExprRule = new IsValidExpressionRule(() => At, datalist, ",", new VariableUtils());
                            ruleSet.Add(atCharsExprRule);
                            ruleSet.Add(new IsStringEmptyRule(() => atCharsExprRule.ExpressionValue));
                            break;
                        default:
                            Dev2Logger.Info("No Rule Set for the Data Split DTO Property Name: " + propertyName, GlobalConstants.WarewolfInfo);
                            break;
                    }
                    break;
                default:
                    return ruleSet;
            }
            return ruleSet;
        }
    }
}
