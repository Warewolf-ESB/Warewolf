/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Warewolf.Data
{
     public class NamedGuid : INamedGuid, IEquatable<NamedGuid>, INotifyPropertyChanged
    {
        protected string _name;
        protected Guid _value;

        public NamedGuid()
        {
            Name = "";
            Value = Guid.Empty;
        }

        public NamedGuid(string name, Guid value)
        {
            Name = name;
            Value = value;
        }

        public virtual string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public virtual Guid Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public override string ToString() => Name;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool operator ==(NamedGuid left, NamedGuid right) => Equals(left, right);
        public static bool operator !=(NamedGuid left, NamedGuid right) => !Equals(left, right);

        public bool Equals(NamedGuid other)
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

        public override bool Equals(object obj)
        {
            if (obj is null)
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
            return Equals((NamedGuid)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name?.GetHashCode() ?? 0) * 397) ^ (_value.GetHashCode());
            }
        }

        public NamedGuid Clone()
        {
            return new NamedGuid
            {
                _name = _name,
                _value = _value,
            };
        }
    }
}
