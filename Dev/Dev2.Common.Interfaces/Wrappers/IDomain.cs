using System.DirectoryServices.ActiveDirectory;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDomain
    {
        Domain GetComputerDomain();
    }
}