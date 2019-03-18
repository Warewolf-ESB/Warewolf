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
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Util;
using Dev2.Providers.Validation.Rules;
using Dev2.Validation;

namespace Dev2
{
    public class BaseConvertTO : ValidatableObject,IDev2TOFn,IEquatable<BaseConvertTO>
    {
        string _fromExpression;
        string _fromType;
        string _toExpression;
        string _toType;
        bool _isFromExpressionFocused;


        public BaseConvertTO()
        {
        }

        public BaseConvertTO(string fromExpression, string fromType, string toType, string toExpression, int indexNumber)
            : this(fromExpression, fromType, toType, toExpression, indexNumber, false)
        {
        }

        public BaseConvertTO(string fromExpression, string fromType, string toType, string toExpression, int indexNumber,
            bool inserted)
        {
            Inserted = inserted;
            ToType = string.IsNullOrEmpty(toType) ? "Base 64" : toType;
            FromType = string.IsNullOrEmpty(fromType) ? "Text" : fromType;
            ToExpression = string.IsNullOrEmpty(toExpression) ? string.Empty : toExpression;
            FromExpression = fromExpression;
            IndexNumber = indexNumber;
        }

        /// <summary>
        ///     Current base type
        /// </summary>
        public string FromType
        {
            get => _fromType;
            set
            {
                if (value != null)
                {
                    _fromType = value;
                    OnPropertyChanged("FromType");
                }
            }
        }

        /// <summary>
        ///     Target base conversion type
        /// </summary>
        public string ToType
        {
            get => _toType;
            set
            {
                if (value != null)
                {
                    _toType = value;
                    OnPropertyChanged("ToType");
                }
            }
        }

        /// <summary>
        ///     The Input to use for the from
        /// </summary>
        [FindMissing]
        public string FromExpression
        {
            get => _fromExpression;
            set
            {
                _fromExpression = value;
                OnPropertyChanged("FromExpression");
                RaiseCanAddRemoveChanged();
            }
        }

        /// <summary>
        ///     Where to place the result, will be the same as From until wizards are created
        /// </summary>
        [FindMissing]
        public string ToExpression
        {
            get => _toExpression;
            set
            {
                _toExpression = value;
                OnPropertyChanged("ToExpression");
            }
        }

        public IList<string> Expressions { get; set; }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkText { get; set; }
        public bool Inserted { get; set; }

        public int IndexNumber { get; set; }

        public bool CanRemove() => string.IsNullOrWhiteSpace(FromExpression);

        public bool CanAdd() => !string.IsNullOrWhiteSpace(FromExpression);

        public void ClearRow()
        {
            FromType = "";
            ToType = "";
            FromExpression = string.Empty;
            ToExpression = string.Empty;
        }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }
        public bool IsFromExpressionFocused { get => _isFromExpressionFocused; set => OnPropertyChanged(ref _isFromExpressionFocused, value); }
        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();

            if (propertyName == "FromExpression" && !string.IsNullOrEmpty(FromExpression))
            {
                var outputExprRule = new IsValidExpressionRule(() => FromExpression, datalist, "0", new VariableUtils());
                ruleSet.Add(outputExprRule);
                ruleSet.Add(new IsValidExpressionRule(() => outputExprRule.ExpressionValue, datalist, new VariableUtils()));
            }
            return ruleSet;
        }

        public bool Equals(BaseConvertTO other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var collectionEquals = CommonEqualityOps.CollectionEquals(Expressions, other.Expressions, StringComparer.Ordinal);
            return string.Equals(FromExpression, other.FromExpression)
                   && string.Equals(FromType, other.FromType)
                   && string.Equals(ToExpression, other.ToExpression)
                   && string.Equals(ToType, other.ToType)
                   && collectionEquals
                   && Inserted == other.Inserted
                   && IndexNumber == other.IndexNumber
                   && string.Equals(Error, other.Error);
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

            return Equals((BaseConvertTO) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FromExpression != null ? FromExpression.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FromType != null ? FromType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToExpression != null ? ToExpression.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ToType != null ? ToType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Expressions != null ? Expressions.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WatermarkTextVariable != null ? WatermarkTextVariable.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (WatermarkText != null ? WatermarkText.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Inserted.GetHashCode();
                hashCode = (hashCode * 397) ^ IndexNumber;
                hashCode = (hashCode * 397) ^ (Error != null ? Error.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Errors != null ? Errors.GetHashCode() : 0);
                return hashCode;
            }
        }

       
    }
}