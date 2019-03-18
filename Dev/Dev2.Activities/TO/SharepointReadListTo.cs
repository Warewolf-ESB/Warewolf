#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
        public int IndexNumber { get => _indexNumber; set => OnPropertyChanged(ref _indexNumber, value); }

        public bool CanRemove() => false;

        public bool CanAdd() => false;

        public void ClearRow()
        {
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() => FieldName;

        public bool Inserted { get; set; }

        public override IRuleSet GetRuleSet(string propertyName, string datalist) => new RuleSet();

        public bool Equals(SharepointReadListTo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

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