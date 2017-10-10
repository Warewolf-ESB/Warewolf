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
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Interfaces;

namespace Dev2.TO
{

    public class AssignObjectDTO : ValidatedObject, IDev2TOFn, IEquatable<AssignObjectDTO>
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

        public AssignObjectDTO(string fieldName, string fieldValue, int indexNumber)
            : this(fieldName, fieldValue, indexNumber, false)
        {
        }

        public AssignObjectDTO(string fieldName, string fieldValue, int indexNumber, bool inserted)
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

            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");

        }

        public bool Equals(AssignObjectDTO other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FieldName, other.FieldName)
                && string.Equals(FieldValue, other.FieldValue)
                && IndexNumber == other.IndexNumber
                && IsFieldNameFocused == other.IsFieldNameFocused
                && IsFieldValueFocused == other.IsFieldValueFocused
                && string.Equals(WatermarkTextVariable, other.WatermarkTextVariable)
                && string.Equals(WatermarkTextValue, other.WatermarkTextValue)
                && Inserted == other.Inserted && OutList.SequenceEqual(other.OutList, StringComparer.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssignObjectDTO)obj);
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
                return hashCode;
            }
        }
    }
}