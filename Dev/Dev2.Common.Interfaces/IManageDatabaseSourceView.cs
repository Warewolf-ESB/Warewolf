#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ViewEngines;
#else
using Microsoft.Practices.Prism.Mvvm;
#endif

namespace Dev2.Common.Interfaces
{
    public interface IManageDatabaseSourceView : IView
    {
    }
}
