using Dev2.Common.Interfaces.DB;

namespace Warewolf.Studio.ViewModels
{
    public class DbInput:IDbInput
    {
        public DbInput(string name, string value)
        {
            Name = name;
            Value = value;
            DefaultValue = "";
            RequiredField = true;
            EmptyIsNull = true;
        }

        #region Implementation of IDbInput

        public string Name { get; set; }

        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public bool RequiredField { get; set; }
        public bool EmptyIsNull { get; set; }

        #endregion
    }
}
