using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Used for activties
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class ActivityDTO : ValidatedObject, IDev2TOFn
    // ReSharper restore InconsistentNaming
    {
        string _fieldName;
        string _fieldValue;
        int _indexNumber;
        List<string> _outList;

        bool _isFieldNameFocused;
        private bool _isFieldValueFocused;
        private string _errorMessage;

        public ActivityDTO()
            : this("[[Variable]]", "Expression", 0)
        {
        }

        public ActivityDTO(string fieldName, string fieldValue, int indexNumber, bool inserted = false)
        {
            Inserted = inserted;
            FieldName = fieldName;
            FieldValue = fieldValue;
            IndexNumber = indexNumber;
            _outList = new List<string>();
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
            get
            {
                return _fieldName;
            }
            set
            {
                if(_fieldName != value)
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
            get
            {
                return _fieldValue;
            }
            set
            {
                if(_fieldValue != value)
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
            get
            {
                return _indexNumber;
            }
            set
            {
                _indexNumber = value;
                OnPropertyChanged();
            }
        }

        public bool CanRemove()
        {
            bool result = string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue);
            return result;
        }


        public bool CanAdd()
        {
            bool result = !(string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue));
            return result;
        }

        public void ClearRow()
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
        }

        public bool Inserted { get; set; }

        public List<string> OutList
        {
            get
            {
                return _outList;
            }
            set
            {
                _outList = value;
            }
        }


        public OutputTO ConvertToOutputTO()
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

        /// <summary>
        /// Validates the property name with the default rule set in <value>ActivityDTO</value>
        /// </summary>
        /// <param name="property">Property to validate</param>
        /// <param name="ruleSet">Ruleset to use during validation</param>
        /// <returns></returns>
        public bool Validate(Expression<Func<string>> property, RuleSet ruleSet)
        {
            var propertyName = GetPropertyName(property);
            return Validate(propertyName, ruleSet);
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
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
            get
            {
                return _isFieldNameFocused;
            }
            set
            {
                OnPropertyChanged(ref _isFieldNameFocused, value);
            }
        }

        public bool IsFieldValueFocused
        {
            get
            {
                return _isFieldValueFocused;
            }
            set
            {
                OnPropertyChanged(ref _isFieldValueFocused, value);
            }
        }

        static string GetPropertyName<TU>(Expression<Func<TU>> propertyName)
        {
            if(propertyName.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException(@"Value must be a lamda expression", "propertyName");
            }

            var body = propertyName.Body as MemberExpression;

            if(body == null)
            {
                throw new ArgumentException("Must have body");
            }
            if(body.Member == null)
            {
                throw new ArgumentException("Body must have Member");
            }
            return body.Member.Name;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(FieldName)
                   && string.IsNullOrEmpty(FieldValue);
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
                case "FieldName":
                    ruleSet.Add(new IsStringEmptyRule(() => FieldName));
                    ruleSet.Add(new IsValidExpressionRule(() => FieldName, datalist, "1"));
                    break;
                case "FieldValue":                    
                    ruleSet.Add(new IsValidExpressionRule(() => FieldValue, datalist, "1"));
                    break;

                //case "EscapeChar":
                //    break;
            }
            return ruleSet;
        }
    }
}
