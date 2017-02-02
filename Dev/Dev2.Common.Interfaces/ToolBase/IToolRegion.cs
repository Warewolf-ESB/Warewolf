using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dev2.Common.Interfaces.ToolBase
{
    public interface IToolRegion:INotifyPropertyChanged
    {
        string ToolRegionName { get; set; }
        bool IsEnabled { get; set; }
        IList<IToolRegion> Dependants { get; set; }
        IList<string> Errors { get; }

        IToolRegion CloneRegion();
        void RestoreRegion(IToolRegion toRestore);

        EventHandler<List<string>> ErrorsHandler { get; set; } 
    }

    public delegate void SomethingChanged(object sender, IToolRegion args);
}