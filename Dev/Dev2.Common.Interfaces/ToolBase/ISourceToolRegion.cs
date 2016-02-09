using System.Collections.Generic;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface ISourceToolRegion<T>:IToolRegion
    {
        T SelectedSource { get; set; }
        ICollection<T> Sources { get; set; }

        ICommand EditSourceCommand { get; }
        ICommand NewSourceCommand { get; }
        event SomethingChanged SomethingChanged; 
    }
}