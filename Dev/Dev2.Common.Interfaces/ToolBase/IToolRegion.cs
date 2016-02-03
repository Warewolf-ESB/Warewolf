using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IToolRegion:INotifyPropertyChanged
    {
        double MinHeight{get;set;}
        double CurrentHeight{get;set;}
        bool IsVisible { get; set; }
        double MaxHeight { get; set; }
        event HeightChanged  HeightChanged; 
    }

    public interface IWebGetInputArea : IToolRegion
    {
        string QueryString { get; set; }
        string RequestUrl { get; set; }

        ICollection<NameValue> Headers { get; set; }
        ICommand AddRowCommand { get; }
        ICommand RemoveRowCommand { get; }
    }

    public delegate void HeightChanged(object sender, IToolRegion args);
    public delegate void SomethingChanged(object sender, IToolRegion args);
}