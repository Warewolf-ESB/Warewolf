using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Core
{
    public class WarewolfType : IWarewolfType, IEquatable<WarewolfType>
    {
        public WarewolfType(string fullyQualifiedName, Version version, string containingAssemblyPath)
        {
            ContainingAssemblyPath = containingAssemblyPath;
            Version = version;
            FullyQualifiedName = fullyQualifiedName;           
        }

        #region Implementation of IWarewolfType

        public string FullyQualifiedName { get; private set; }
        public Version Version { get; private set; }
        public string ContainingAssemblyPath { get; private set; }
        public Type ActualType { get; set; }

        #endregion
        
        #region Equality members
        
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((WarewolfType)obj);
        }

        public bool Equals(WarewolfType other) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

        public static bool operator ==(WarewolfType left, WarewolfType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WarewolfType left, WarewolfType right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
