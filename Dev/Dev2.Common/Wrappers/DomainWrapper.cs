using System.DirectoryServices.ActiveDirectory;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    public class DomainWrapper : IDomain
    {
        #region Implementation of IDomain

        public Domain GetComputerDomain()
        {
            return Domain.GetComputerDomain();
        }

        #endregion
    }
}