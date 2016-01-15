using System.Collections.Generic;
using Dev2.Common.Interfaces.Toolbox;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolBoxCategoryViewModel:IToolboxCatergoryViewModel
    {
        public ToolBoxCategoryViewModel(string name, ICollection<IToolDescriptorViewModel> toolDescriptorViewModels)
        {
            Name = name;
            Tools = toolDescriptorViewModels;
        }

        #region Implementation of IToolboxCatergoryViewModel

        public string Name { get; private set; }
        public ICollection<IToolDescriptorViewModel> Tools { get; private set; }

        #endregion
    }
}