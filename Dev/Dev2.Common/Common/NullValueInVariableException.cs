using System;

namespace Dev2.Common.Common
{
    public class NullValueInVariableException : Exception
    {
        public NullValueInVariableException(string message, string variableName)
            : base(message)
        {
            VariableName = variableName;
        }

        public string VariableName
        {
            get;
            set;
        }
    }
}