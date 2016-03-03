using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebServicePutViewModel : IWebServiceBaseViewModel
    {
        IWebPutInputArea InputArea { get; set; }
    }

    public interface IWebPutInputArea : IToolRegion
    {
        string QueryString { get; set; }
        string RequestUrl { get; set; }

        ObservableCollection<INameValue> Headers { get; set; }

        double HeadersHeight { get; set; }
        double MaxHeadersHeight { get; set; }
    }
}