/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

        public bool CanRemove()
        {
            return string.IsNullOrWhiteSpace(FromExpression);
        }

        public bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(FromExpression);
        }

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
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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