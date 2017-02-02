using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IConstructorRegion<T> : IToolRegion
    {
        T SelectedConstructor { get; set; }
        ICollection<T> Constructors { get; set; }
        ICommand RefreshConstructorsCommand { get; }
        bool IsConstructorEnabled { get; }
        bool IsConstructorExpanded { get; set; }
        bool IsRefreshing { get; set; }
        event SomethingChanged SomethingChanged;
        double LabelWidth { get; set; }
    }
}