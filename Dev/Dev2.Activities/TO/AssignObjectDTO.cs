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
        string _fieldName;
        string _fieldValue;
        int _indexNumber;
        bool _isFieldNameFocused;
        bool _isFieldValueFocused;

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

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            IRuleSet ruleSet = new RuleSet();
            return ruleSet;
        }

        void RaiseCanAddRemoveChanged()
        {

            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");

        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(AssignObjectDTO other)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

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