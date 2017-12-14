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
using System.Linq;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;
using Warewolf.Resource.Errors;
using Dev2.Common;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <summary>
    /// Used for activties
    /// </summary>

    public class ActivityDTO : ValidatedObject, IDev2TOFn,  IEquatable<ActivityDTO>

    {
        string _fieldName;
        string _fieldValue;
        int _indexNumber;

        bool _isFieldNameFocused;
        bool _isFieldValueFocused;
        string _errorMessage;

        public ActivityDTO()
            : this("[[Variable]]", "Expression", 0)
        {
        }

        public ActivityDTO(string fieldName, string fieldValue, int indexNumber)
            : this(fieldName, fieldValue, indexNumber, false)
        {
        }

        public ActivityDTO(string fieldName, string fieldValue, int indexNumber, bool inserted)
        {
            Inserted = inserted;
            FieldName = fieldName;
            FieldValue = fieldValue;
            IndexNumber = indexNumber;
            OutList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkTextValue { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        [FindMissing]
        public string FieldName
        {
            get => _fieldName;
            set
            {
                if (_fieldName != value)
                {
                    _fieldName = value;
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                }
                //DO NOT call validation here as it will cause issues during serialization
            }
        }

        [FindMissing]
        public string FieldValue
        {
            get => _fieldValue;
            set
            {
                if (_fieldValue != value)
                {
                    _fieldValue = value;
                    OnPropertyChanged();
                    RaiseCanAddRemoveChanged();
                }
                //DO NOT call validation here as it will cause issues during serialization
            }
        }

        public int IndexNumber
        {
            get => _indexNumber;
            set
            {
                _indexNumber = value;
                OnPropertyChanged();
            }
        }

        public bool CanRemove()
        {
            var result = string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue);
            return result;
        }

        public bool CanAdd()
        {
            var result = !(string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue));
            return result;
        }

        public void ClearRow()
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
        }

        public bool Inserted { get; set; }

        public List<string> OutList { get; set; }

        public OutputTO ConvertToOutputTo()
        {
            return DataListFactory.CreateOutputTO(FieldName, OutList);
        }

        /// <summary>
        /// Validates the property name with the default rule set in <value>ActivityDTO</value>
        /// </summary>
        /// <param name="property"></param>
        /// <param name="datalist"></param>
        /// <returns></returns>
        public bool Validate(Expression<Func<string>> property, string datalist)
        {
            var propertyName = GetPropertyName(property);
            return Validate(propertyName, datalist);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasError
        {
            get
            {
                var errorCount = Errors.SelectMany(pair => pair.Value).Count();
                return errorCount != 0;
            }
        }
        public bool IsFieldNameFocused
        {
            get => _isFieldNameFocused;
            set
            {
                OnPropertyChanged(ref _isFieldNameFocused, value);
            }
        }

        public bool IsFieldValueFocused
        {
            get => _isFieldValueFocused;
            set
            {
                OnPropertyChanged(ref _isFieldValueFocused, value);
            }
        }

        static string GetPropertyName<TU>(Expression<Func<TU>> propertyName)
        {
            if (propertyName.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException(ErrorResource.ExpectedLambdaExpresion, "propertyName");
            }

            var body = propertyName.Body as MemberExpression;

            if (body == null)
            {
                throw new ArgumentException(ErrorResource.MustHaveBody);
            }
            if (body.Member == null)
            {
                throw new ArgumentException(ErrorResource.BodyMustHaveMember);
            }
            return body.Member.Name;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue);
        }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            if (IsEmpty())
            {
                return ruleSet;
            }

            switch (propertyName)
            {
                case "FieldName":
                    ruleSet.Add(new IsStringEmptyRule(() => FieldName));
                    ruleSet.Add(new IsValidExpressionRule(() => FieldName, datalist, "1", new VariableUtils()));
                    break;
                case "FieldValue":
                    ruleSet.Add(new IsValidExpressionRule(() => FieldValue, datalist, "1", new VariableUtils()));
                    break;
                case "FieldValueAndCalculate":
                    ruleSet.Add(new ComposableRule<string>(new IsValidExpressionRule(() => FieldValue, datalist, "1", new VariableUtils())).Or(new IsValidCalculateRule(() => FieldValue)));
                    break;
                default:
                    Dev2Logger.Info("No Rule Set for the Activity DTO Property Name: " + propertyName, GlobalConstants.WarewolfInfo);
                    break;
            }
            return ruleSet;
        }
        
        public bool Equals(ActivityDTO other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FieldName, other.FieldName) 
                && string.Equals(FieldValue, other.FieldValue) 
                && IndexNumber == other.IndexNumber
                && IsFieldNameFocused == other.IsFieldNameFocused
                && IsFieldValueFocused == other.IsFieldValueFocused 
                && string.Equals(ErrorMessage, other.ErrorMessage) 
                && string.Equals(WatermarkTextVariable, other.WatermarkTextVariable)
                && string.Equals(WatermarkTextValue, other.WatermarkTextValue) 
                && Inserted == other.Inserted 
                && OutList.SequenceEqual(other.OutList, StringComparer.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActivityDTO) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FieldName != null ? FieldName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FieldValue != null ? FieldValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IndexNumber;
                hashCode = (hashCode * 397) ^ IsFieldNameFocused.GetHashCode();
                hashCode = (hashCode * 397) ^ IsFieldValueFocused.GetHashCode();
                hashCode = (hashCode * 397) ^ (ErrorMessage != null ? ErrorMessage.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
