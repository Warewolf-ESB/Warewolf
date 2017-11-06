/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System;

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
            get { return _outputVariable; }
            set
            {
                OnPropertyChanged(ref _outputVariable, value);
                RaiseCanAddRemoveChanged();
            }
        }

        public bool IsOutputVariableFocused { get => _isOutputVariableFocused; set => OnPropertyChanged(ref _isOutputVariableFocused, value); }

        public string SplitType
        {
            get { return _splitType; }
            set
            {
                if(value != null)
                {
                    OnPropertyChanged(ref _splitType, value);
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

        public bool IsAtFocused { get => _isAtFocused; set => OnPropertyChanged(ref _isAtFocused, value); }

        public bool CanRemove()
        {
            if(SplitType == SplitTypeIndex || SplitType == SplitTypeChars)
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
            if(SplitType == SplitTypeIndex || SplitType == SplitTypeChars)
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
            SplitType = SplitTypeChars;
            At = string.Empty;
            Include = false;
            EscapeChar = string.Empty;
        }

        public bool Inserted { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeIndex && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeChars && string.IsNullOrEmpty(At)
                   || string.IsNullOrEmpty(OutputVariable) && SplitType == SplitTypeNone && string.IsNullOrEmpty(At);
        }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();
            if(IsEmpty())
            {
                return ruleSet;
            }

            switch(propertyName)
            {
                case "OutputVariable":
                    if (!string.IsNullOrEmpty(OutputVariable))
                    {
                        var outputExprRule = new IsValidExpressionRule(() => OutputVariable, datalist, "0");
                        ruleSet.Add(outputExprRule);
                        ruleSet.Add(new IsValidExpressionRule(() => outputExprRule.ExpressionValue, datalist));
                    }
                    break;
                case "At":
                    switch(SplitType)
                    {
                        case SplitTypeIndex:
                            var atIndexExprRule = new IsValidExpressionRule(() => At, datalist, "1");
                            ruleSet.Add(atIndexExprRule);
                            ruleSet.Add(new IsPositiveNumberRule(() => atIndexExprRule.ExpressionValue));
                            break;
                        case SplitTypeChars:
                            var atCharsExprRule = new IsValidExpressionRule(() => At, datalist, ",");
                            ruleSet.Add(atCharsExprRule);
                            ruleSet.Add(new IsStringEmptyRule(() => atCharsExprRule.ExpressionValue));
                            break;
                        default:
                            throw new ArgumentException("Unrecognized split type: " + SplitType);
                    }
                    break;
                default:
                    return ruleSet;
            }
            return ruleSet;
        }
    }
}
