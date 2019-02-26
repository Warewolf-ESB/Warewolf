using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public class NameValue : INameValue, IEquatable<NameValue>
    {
        public bool Equals(NameValue other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_name, other._name) && string.Equals(_value, other._value);
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
            if (obj is NameValue nameValue)
            {
                return Equals(nameValue);
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {                
                return ((_name?.GetHashCode() ?? 0) * 397) ^ (_value?.GetHashCode() ?? 0);                
            }
        }

        public static bool operator ==(NameValue left, NameValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NameValue left, NameValue right)
        {
            return !Equals(left, right);
        }

        protected string _name;
        protected string _value;

        public NameValue()
        {
            
            Name = "";

            Value = "";
            
        }

        public NameValue(string name, string value)
        {
            
            Name = name;

            Value = value;
            
        }


        public virtual string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
   
        public virtual string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
