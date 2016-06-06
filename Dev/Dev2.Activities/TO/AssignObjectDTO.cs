/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Warewolf.Resource.Errors;

namespace Dev2.TO
{
    // ReSharper disable once InconsistentNaming
    public class AssignObjectDTO : ValidatedObject, IDev2TOFn
    {
        private string _fieldName;
        private string _fieldValue;
        private int _indexNumber;
        private bool _isFieldNameFocused;
        private bool _isFieldValueFocused;

        public AssignObjectDTO()
            : this("[[Variable]]", "Expression", 0)
        {
        }

        public AssignObjectDTO(string fieldName, string fieldValue, int indexNumber, bool inserted = false)
        {
            this._fieldName = fieldName;
            this._fieldValue = fieldValue;
            this._indexNumber = indexNumber;
            this.Inserted = inserted;
            OutList = new List<string>();
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
            get
            {
                return _fieldValue;
            }
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

        public string WatermarkTextVariable { get; set; }

        public string WatermarkTextValue { get; set; }

        public bool Inserted { get; set; }

        public List<string> OutList { get; set; }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(FieldName, OutList);
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

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            IRuleSet ruleSet = new RuleSet();
            return ruleSet;
        }

        private void RaiseCanAddRemoveChanged()
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
            // ReSharper restore ExplicitCallerInfoArgument
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

        private static string GetPropertyName<TU>(Expression<Func<TU>> propertyName)
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
    }
}