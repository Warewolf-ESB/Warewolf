using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.ToolBase.DotNet
{
    public interface INamespaceToolRegion<T> : IToolRegion
    {
        T SelectedNamespace { get; set; }
        ICollection<T> Namespaces { get; set; }
        ICommand RefreshNamespaceCommand { get; }
        bool IsNamespaceEnabled { get; set; }
        bool IsRefreshing { get; set; }
        event SomethingChanged SomethingChanged;
        double LabelWidth { get; set; }
    }
}
