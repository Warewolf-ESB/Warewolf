using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2.TO
{
    public class SharepointReadListTo : ValidatedObject, ISharepointReadListTo, IEquatable<SharepointReadListTo>
    {
        int _indexNumber;

        public SharepointReadListTo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SharepointReadListTo(string variableName, string fieldName, string internalName, string type)
        {
            FieldName = fieldName;
            VariableName = variableName;
            InternalName = internalName;
            Type = type;
        }

        public string InternalName { get; set; }

        public string FieldName { get; set; }
        [FindMissing]
        public string VariableName { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public bool CanRemove()
        {
            return false;
        }

        public bool CanAdd()
        {
            return false;
        }

        public void ClearRow()
        {
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return FieldName;
        }

        public bool Inserted { get; set; }

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            return new RuleSet();
        }

        public bool Equals(SharepointReadListTo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IndexNumber == other.IndexNumber
                && string.Equals(InternalName, other.InternalName)
                && string.Equals(FieldName, other.FieldName) 
                && string.Equals(VariableName, other.VariableName) 
                && string.Equals(Type, other.Type) 
                && IsRequired == other.IsRequired
                && Inserted == other.Inserted;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SharepointReadListTo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _indexNumber;
                hashCode = (hashCode * 397) ^ (InternalName != null ? InternalName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FieldName != null ? FieldName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VariableName != null ? VariableName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsRequired.GetHashCode();
                hashCode = (hashCode * 397) ^ Inserted.GetHashCode();
                return hashCode;
            }
        }
    }
}