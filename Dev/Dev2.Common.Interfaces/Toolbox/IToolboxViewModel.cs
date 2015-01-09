using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Toolbox
{
    public interface IToolboxViewModel
    {
        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        ICollection<IToolDescriptorViewModel> Tools { get; }

        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        ICollection<IToolboxCatergoryViewModel> CategorisedTools { get; }

        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        bool IsEnabled { get;  }

        /// <summary>
        /// filters the list of tools available to the user.
        /// </summary>
        /// <param name="searchString"></param>
        void Filter(string searchString);

        /// <summary>
        /// Clear Filters all tools visible
        /// </summary>
        void ClearFilter();
        /// <summary>
        /// Is the designer focused. This is used externally to disable the toolbox.
        /// </summary>
        bool IsDesignerFocused { get; set; }
    }
}