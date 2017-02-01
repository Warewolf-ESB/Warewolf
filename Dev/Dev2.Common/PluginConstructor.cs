using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    [Serializable]
    public class PluginConstructor : IPluginConstructor, IEquatable<PluginConstructor>
    {
        public PluginConstructor()
        {
            Inputs = new List<IConstructorParameter>();
        }
        #region Implementation of IConstructor

        public IList<IConstructorParameter> Inputs { get; set; }
        public string ReturnObject { get; set; }
        public string ConstructorName { get; set; }

        public string GetIdentifier()
        {
            return ConstructorName;
        }

        #endregion

        #region Implementation of IPluginConstructor

        public bool IsExistingObject { get; set; }

        #endregion

        public bool Equals(PluginConstructor other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (GetHashCode() == other.GetHashCode())
                return true;

            return string.Equals(ConstructorName, other.ConstructorName);
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
            return Equals((PluginConstructor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Inputs?.GetHashCode() ?? 0) * 397) ^ (ConstructorName?.GetHashCode() ?? 0);
            }
        }
        public static bool operator ==(PluginConstructor left, PluginConstructor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PluginConstructor left, PluginConstructor right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return ConstructorName;
        }

        #region Implementation of IPluginConstructor

        public Guid ID { get; set; }

        #endregion
    }
}