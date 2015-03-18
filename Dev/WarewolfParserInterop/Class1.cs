

namespace WarewolfParserInterop
{

    public interface IAssignValue
    {
        string Name { get; }
        string Value { get;  }
    }
    public class AssignValue : IAssignValue
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
    }
}
