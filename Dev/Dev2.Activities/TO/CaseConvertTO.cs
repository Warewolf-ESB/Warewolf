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
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Validation;

namespace Dev2.TO
{
    public class CaseConvertTO : ValidatedObject, ICaseConvertTO
    {
        private string _convertType;
        private string _result;
        private string _stringToConvert;
        private bool _isStringToConvertFocused;

        public CaseConvertTO()
        {
            Errors = new Dictionary<string, List<IActionableErrorInfo>>();
        }

        public CaseConvertTO(string stringToConvert, string convertType, string result, int indexNumber)
            : this(stringToConvert, convertType, result, indexNumber, false)
        {
        }

        public CaseConvertTO(string stringToConvert, string convertType, string result, int indexNumber,
            bool inserted)
        {
            Inserted = inserted;
            StringToConvert = stringToConvert;
            ConvertType = string.IsNullOrEmpty(convertType) ? "UPPER" : convertType;
            Result = string.IsNullOrEmpty(result) ? string.Empty : result;
            IndexNumber = indexNumber;
        }

        public bool Inserted { get; set; }

        [FindMissing]
        public string StringToConvert
        {
            get { return _stringToConvert; }
            set
            {
                _stringToConvert = value;
                _result = value;
                OnPropertyChanged("StringToConvert");
                OnPropertyChanged("Result");
                RaiseCanAddRemoveChanged();
            }
        }

        public string ConvertType
        {
            get { return _convertType; }
            set
            {
                if (value != null)
                {
                    _convertType = value;
                    OnPropertyChanged("ConvertType");
                }
            }
        }

        public IList<string> Expressions { get; set; }

        [FindMissing]
        public string Result
        {
            get
            {
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

        private void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

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

        public bool IsStringToConvertFocused { get => _isStringToConvertFocused; set => OnPropertyChanged(ref _isStringToConvertFocused, value); }         

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            RuleSet ruleSet = new RuleSet();
            if (propertyName == "StringToConvert")
            {

                if (!string.IsNullOrEmpty(StringToConvert))
                {
                    var inputExprRule = new IsValidExpressionRule(() => StringToConvert, datalist, "0");
                    ruleSet.Add(inputExprRule);
                }
                else
                {
                    ruleSet.Add(new IsStringEmptyRule(() => StringToConvert));
                }

            }
            return ruleSet;
        }          
    }
}