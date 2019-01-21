using System;

namespace Dev2.Common.Interfaces
{

    public interface INameValue
    {
        string Name { get; set; }
        string Value { get; set; }
    }

    public class NameValue : INameValue, IEquatable<NameValue>
    {
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

#pragma warning disable S2292
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
#pragma warning restore S2292

        public override string ToString() => Name;


        public static bool operator ==(NameValue left, NameValue right) => Equals(left, right);
        public static bool operator !=(NameValue left, NameValue right) => !Equals(left, right);


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

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name?.GetHashCode() ?? 0) * 397) ^ (_value?.GetHashCode() ?? 0);
            }
        }
    }
}
