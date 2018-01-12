using Dev2.Common.Interfaces;
using System;

namespace WarewolfParserInterop
{
    public class AssignValue : IAssignValue, IEquatable<AssignValue>
    {
        public AssignValue(string name, string value)
        {
            Value = value;
            Name = name;
        }

        #region Implementation of IAssignValue

        public string Name { get; private set; }
        public string Value { get; private set; }

        #endregion

        #region Equality members

        public bool Equals(AssignValue other) => string.Equals(Name, other.Name) && string.Equals(Value, other.Value);

        public override bool Equals(object obj) => Equals((AssignValue)obj);

        public override int GetHashCode() => throw new NotImplementedException();

        #endregion
    }
}
