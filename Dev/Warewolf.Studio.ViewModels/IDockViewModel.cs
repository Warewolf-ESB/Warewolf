using Dev2.Common.Interfaces;
#if NETFRAMEWORK
using Microsoft.Practices.Prism;
#endif
using Prism;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;

namespace Warewolf.Studio.ViewModels
{
    public interface IDockViewModel : IActiveAware, IDockAware, IUpdatesHelp
    {
    }
}
