using System.Collections.Generic;
using System.ComponentModel;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IToolRegion:INotifyPropertyChanged
    {
        string ToolRegionName { get; set; }
        double MinHeight{get;set;}
        double CurrentHeight{get;set;}
        bool IsVisible { get; set; }
        double MaxHeight { get; set; }
        event HeightChanged  HeightChanged;
        IList<IToolRegion> Dependants { get; set; }
        IList<string> Errors { get; }

        IToolRegion CloneRegion();
        void RestoreRegion(IToolRegion toRestore);
    }

    public delegate void HeightChanged(object sender, IToolRegion args);
    public delegate void SomethingChanged(object sender, IToolRegion args);
}