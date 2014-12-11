using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerViewModel:BindableBase,IExplorerViewModel
    {
        ICollection<IExplorerItemViewModel> _explorerItems;
        public ICollection<IExplorerItemViewModel> ExplorerItems
        {
            get
            {
                return _explorerItems;
            }
            set
            {
                _explorerItems = value;
                OnPropertyChanged(() => ExplorerItems);
            }
        }
    }

    public class ExplorerItemViewModel : BindableBase,IExplorerItemViewModel
    {
        string _resourceName;

        #region Implementation of IExplorerItemViewModel

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                OnPropertyChanged(() => ResourceName);
            }
        }
        public string IconPath
        {
            get;
            set;
        }

        #endregion
    }
}
