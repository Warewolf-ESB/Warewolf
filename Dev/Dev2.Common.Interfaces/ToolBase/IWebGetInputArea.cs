using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IWebGetInputArea : IToolRegion
    {
        string QueryString { get; set; }
        string RequestUrl { get; set; }

        ObservableCollection<INameValue> Headers { get; set; }
    }
}