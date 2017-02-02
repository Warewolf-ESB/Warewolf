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
        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(NameValue other)
        {
            if (ReferenceEquals(null, other))
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
            return Equals((NameValue)obj);
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
                // ReSharper disable NonReadonlyFieldInGetHashCode
                return ((_name?.GetHashCode() ?? 0) * 397) ^ (_value?.GetHashCode() ?? 0);
                // ReSharper restore NonReadonlyFieldInGetHashCode
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

        #endregion

        protected string _name;
        protected string _value;

        #region Implementation of INameValue

        public NameValue()
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Name = "";

            Value = "";
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        public NameValue(string name, string value)
        {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Name = name;

            Value = value;
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
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

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ObservableAwareNameValue : NameValue
    {
        readonly ObservableCollection<INameValue> _sourceCollection;
        readonly Action<string> _update;

        public ObservableAwareNameValue(ObservableCollection<INameValue> sourceCollection, Action<string> update)
        {
            _sourceCollection = sourceCollection;
            _update = update;
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Name = "";

            Value = "";
            AddRowCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(AddRow);
            RemoveRowCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(RemoveRow);
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        void RemoveRow()
        {
            if (!ReferenceEquals(_sourceCollection.Last(), this))
            {
                _sourceCollection.Remove(this);
            }
        }

        void AddRow()
        {
            _sourceCollection.Insert(_sourceCollection.IndexOf(this), new ObservableAwareNameValue(_sourceCollection, _update));
        }

        #region Overrides of NameValue

        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(_value) && String.IsNullOrEmpty(_name) && ReferenceEquals(_sourceCollection.Last(), this))
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection, _update));
                }
                _name = value;
                _update?.Invoke(_name);
            }
        }

        #region Overrides of NameValue

        public override string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(_value) && String.IsNullOrEmpty(_name) && ReferenceEquals(_sourceCollection.Last(), this))
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection, _update));
                }
                _value = value;
                _update?.Invoke(_value);
            }
        }
        public ICommand RemoveRowCommand { get; set; }
        public ICommand AddRowCommand { get; set; }

        #endregion

        #endregion
    }
}
