/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2
{
    public class CaseConvertTO : ICaseConvertTO, IPerformsValidation,IEquatable<CaseConvertTO>
    {
        #region Fields

        private string _convertType;
        private Dictionary<string, List<IActionableErrorInfo>> _errors;
        private string _result;
        private string _stringToConvert;

        #endregion Fields

        #region Ctor

        public CaseConvertTO()
        {
            Errors = new Dictionary<string, List<IActionableErrorInfo>>();
        }

        public CaseConvertTO(string stringToConvert, string convertType, string result, int indexNumber,
            bool inserted = false)
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
            get { return _stringToConvert; }
            set
            {
                _stringToConvert = value;
                _result = value;
                    // This is set as the result for now as it is the same value till we do the advanced view.
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

        private void RaiseCanAddRemoveChanged()
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of IDataErrorInfo

        /// <summary>
        ///     Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        ///     The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName] => null;

        /// <summary>
        ///     Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        ///     An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        
        
        public string Error { get; private set; }

        

        #endregion

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors
        {
            get { return _errors; }
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
                List<IActionableErrorInfo> errorsTos = ruleSet.ValidateRules();
                List<IActionableErrorInfo> actionableErrorInfos =
                    errorsTos.ConvertAll<IActionableErrorInfo>(input => new ActionableErrorInfo(input, () =>
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

        private RuleSet GetFieldNameRuleSet()
        {
            var ruleSet = new RuleSet();
            return ruleSet;
        }

        #endregion

        public bool Equals(CaseConvertTO other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_convertType, other._convertType) && Equals(_errors, other._errors) && string.Equals(_result, other._result) && string.Equals(_stringToConvert, other._stringToConvert) && Inserted == other.Inserted && Equals(Expressions, other.Expressions) && IndexNumber == other.IndexNumber && string.Equals(ExpressionToConvert, other.ExpressionToConvert) && string.Equals(WatermarkTextVariable, other.WatermarkTextVariable) && string.Equals(Error, other.Error);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CaseConvertTO) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_convertType != null ? _convertType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_errors != null ? _errors.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_result != null ? _result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_stringToConvert != null ? _stringToConvert.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Inserted.GetHashCode();
                hashCode = (hashCode * 397) ^ (Expressions != null ? Expressions.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IndexNumber;
                hashCode = (hashCode * 397) ^ (ExpressionToConvert != null ? ExpressionToConvert.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WatermarkTextVariable != null ? WatermarkTextVariable.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Error != null ? Error.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}