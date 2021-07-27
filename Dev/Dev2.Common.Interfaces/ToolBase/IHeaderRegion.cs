using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IHeaderRegion
    {
        ObservableCollection<INameValue> Headers { get; set; }
        ObservableCollection<INameValue> Settings { get; set; }
    }
}