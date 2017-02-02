using System;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Util;

namespace Warewolf.Core
{
    [Serializable]
    public class ServiceInput : ObservableObject, IServiceInput, IEquatable<ServiceInput>
    {
        private string _value;
        private bool _requiredField;
        private bool _emptyIsNull;
        private string _name;

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ServiceInput other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name) && RequiredField.Equals(other.RequiredField) && EmptyIsNull.Equals(other.EmptyIsNull);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ServiceInput)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ RequiredField.GetHashCode();
                hashCode = (hashCode * 397) ^ EmptyIsNull.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ServiceInput left, ServiceInput right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServiceInput left, ServiceInput right)
        {
            return !Equals(left, right);
        }

        #endregion

        public ServiceInput(string name, string value)
        {
            Name = name.Replace("`", "");
            Value = value;
            RequiredField = true;
            EmptyIsNull = true;
        }

        public ServiceInput()
        {
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        [FindMissing]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        public bool RequiredField
        {
            get
            {
                return _requiredField;
            }
            set
            {
                _requiredField = value;
                OnPropertyChanged();
            }
        }
        public bool EmptyIsNull
        {
            get
            {
                return _emptyIsNull;
            }
            set
            {
                _emptyIsNull = value;
                OnPropertyChanged();
            }
        }
        public string TypeName { get; set; }

        public enIntellisensePartType IntellisenseFilter { get; set; }
        public bool IsObject { get; set; }
        public string Dev2ReturnType { get; set; }
        public string ShortTypeName { get; set; }
        public string FullName
        {
            get
            {
                var type = ShortTypeName == null ? "" : "(" + ShortTypeName + ")";
                if (string.IsNullOrEmpty(Name)) return "";
                var fullName = Name + type;
                return fullName;
            }
        }
    }
}
