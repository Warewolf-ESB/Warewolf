using Dev2.Common.Interfaces.DB;

namespace Warewolf.Studio.ViewModels
{
    public class DbInput:IDbInput
    {
        public DbInput(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #region Implementation of IDbInput

        public string Name { get; set; }

        public string Value { get; set; }

        #endregion
    }
}
