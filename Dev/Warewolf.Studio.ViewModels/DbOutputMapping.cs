using Dev2.Common.Interfaces.DB;

namespace Warewolf.Studio.ViewModels
{
    public class DbOutputMapping : IDbOutputMapping
    {
        public DbOutputMapping(string name, string mapping)
        {
            Name = name;
            OutputName = mapping;
        }

        #region Implementation of IDbOutputMapping

        public string Name { get; set; }
        public string OutputName { get; set; }

        #endregion
    }
}