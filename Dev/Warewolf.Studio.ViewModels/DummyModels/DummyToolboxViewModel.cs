using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    class DummyToolboxViewModel:IToolboxViewModel
    {
        #region Implementation of IToolboxViewModel

        public DummyToolboxViewModel()
        {
            Tools = new ObservableCollection<IToolDescriptorViewModel>();
            
            IsEnabled = true;
        }
        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolDescriptorViewModel> Tools { get; private set; }
        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// filters the list of tools available to the user.
        /// </summary>
        /// <param name="searchString"></param>
        public void Filter(string searchString)
        {
        }

        /// <summary>
        /// Clear Filters all tools visible
        /// </summary>
        public void ClearFilter()
        {
        }

        /// <summary>
        /// Is the designer focused. This is used externally to disable the toolbox.
        /// </summary>
        public bool IsDesignerFocused { get; set; }

        #endregion
    }
}
