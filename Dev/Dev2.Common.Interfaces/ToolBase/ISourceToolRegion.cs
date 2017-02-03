using System;
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
        Action SourceChangedAction { get; set; }
        event SomethingChanged SomethingChanged;
        double LabelWidth { get; set; }
        string NewSourceHelpText { get; set; }
        string EditSourceHelpText { get; set; }
        string SourcesHelpText { get; set; }
        string NewSourceTooltip { get; set; }
        string EditSourceTooltip { get; set; }
        string SourcesTooltip { get; set; }
    }
}