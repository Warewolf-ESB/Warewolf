using System;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

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
            return ((_name != null ? _name.GetHashCode() : 0) * 397) ^ (_value != null ? _value.GetHashCode() : 0);
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

    string _name;
    string _value;

    #region Implementation of INameValue

    public NameValue()
    {
        Name = "";
        Value = "";
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
        }
    }
    public string Value
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