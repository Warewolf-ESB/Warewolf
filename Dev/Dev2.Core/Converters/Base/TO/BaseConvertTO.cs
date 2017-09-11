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
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Util;

namespace Dev2
{
    public class BaseConvertTO : IDev2TOFn, IPerformsValidation,IEquatable<BaseConvertTO>
    {
        #region Fields

        private string _fromExpression;
        private string _fromType;
        private string _toExpression;
        private string _toType;

        #endregion

        #region Ctor

        public BaseConvertTO()
        {
        }

        public BaseConvertTO(string fromExpression, string fromType, string toType, string toExpression, int indexNumber,
            bool inserted = false)
        {
            Inserted = inserted;
            ToType = string.IsNullOrEmpty(toType) ? "Base 64" : toType;
            FromType = string.IsNullOrEmpty(fromType) ? "Text" : fromType;
            ToExpression = string.IsNullOrEmpty(toExpression) ? string.Empty : toExpression;
            FromExpression = fromExpression;
            IndexNumber = indexNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Current base type
        /// </summary>
        public string FromType
        {
            get { return _fromType; }
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
            get { return _toType; }
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
            get { return _fromExpression; }
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
            get { return _toExpression; }
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

        #endregion

        #region Public Methods

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

        #endregion

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
        public string this[string columnName] => "";

        /// <summary>
        ///     Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        ///     An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        
        
        public string Error { get; private set; }

        

        #endregion

        private void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get; set; }

        public bool Validate(string propertyName, IRuleSet ruleSet)
        {
            return false;
        }

        public bool Validate(string propertyName, string datalist)
        {
            return false;
        }

        #endregion

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