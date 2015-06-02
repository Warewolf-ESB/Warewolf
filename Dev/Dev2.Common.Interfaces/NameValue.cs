using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;

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
                return ((_name != null ? _name.GetHashCode() : 0) * 397) ^ (_value != null ? _value.GetHashCode() : 0);
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

        #endregion
    }
    public class ObservableAwareNameValue:NameValue
    {
        readonly ObservableCollection<NameValue> _sourceCollection;
        readonly Action<string> _update;

        ICommand _removeRowCommand;
        ICommand _addRowCommand;
        public ObservableAwareNameValue(ObservableCollection<NameValue> sourceCollection,Action<string> update )
        {
            _sourceCollection = sourceCollection;
            _update = update;
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Name = "";

            Value = "";
            _addRowCommand = new DelegateCommand(AddRow);
            _removeRowCommand = new DelegateCommand(RemoveRow);
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        void RemoveRow()
        {
            if(!ReferenceEquals(_sourceCollection.Last(),this))
            {
                _sourceCollection.Remove(this);
            }
        }

        void AddRow()
        {
            _sourceCollection.Insert(_sourceCollection.IndexOf(this),new ObservableAwareNameValue(_sourceCollection,_update));
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
                if (!String.IsNullOrEmpty(value) && String.IsNullOrEmpty(_value) && String.IsNullOrEmpty(_name) && _sourceCollection.Last()==this)
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection,_update));
                }
                _name = value;
                if(_update != null)
                {
                    _update(_name);
                }
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
                if(!String.IsNullOrEmpty(value)  && String.IsNullOrEmpty(_value) && String.IsNullOrEmpty(_name) && _sourceCollection.Last()==this)
                {
                    _sourceCollection.Add(new ObservableAwareNameValue(_sourceCollection,_update));
                }
                _value = value;
                if(_update != null)
                {
                    _update(_value);
                }
            }
        }
        public ICommand RemoveRowCommand
        {
            get
            {
                return _removeRowCommand;
            }
            set
            {
                _removeRowCommand = value;
            }
        }
        public ICommand AddRowCommand
        {
            get
            {
                return _addRowCommand;
            }
            set
            {
                _addRowCommand = value;
            }
        }

        #endregion

        #endregion
    }
}