using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolboxViewModel
    {
        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        ICollection<IToolDescriptorViewModel> Tools { get; }

        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        bool IsEnabled { get;  }
        bool IsVisible { get; set; }

        /// <summary>
        /// Is the designer focused. This is used externally to disable the toolbox.
        /// </summary>
        bool IsDesignerFocused { get; set; }

        IToolDescriptorViewModel SelectedTool { get; set; }
        ICommand ClearFilterCommand { get; set; }
        string SearchTerm { get; set; }
        ObservableCollection<IToolDescriptorViewModel> BackedUpTools { get; set; }

        void BuildToolsList();
    }
}