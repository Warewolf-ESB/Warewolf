using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core.Web.Base
{
    public interface IRegionClone : IToolRegion
    {
        ObservableCollection<INameValue> Headers { get; set; }
        string QueryString { get; set; }
        string RequestUrl { get; set; }

        void RestoreRegion(IRegionClone toRestore);
    }

    public interface IWebInputAreaClone : IRegionClone
    {
        double HeadersHeight { get; set; }
        double MaxHeadersHeight { get; set; }
    }

    public interface IWebPutInputAreaClone : IWebInputAreaClone
    {
    }
}