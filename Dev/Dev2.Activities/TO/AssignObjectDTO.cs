/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using System.Collections.Generic;

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
            _fieldName = fieldName;
            _fieldValue = fieldValue;
            _indexNumber = indexNumber;
            Inserted = inserted;
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
    }
}