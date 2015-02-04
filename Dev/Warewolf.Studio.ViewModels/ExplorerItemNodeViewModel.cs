using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemNodeViewModel : ExplorerItemViewModel, IExplorerItemNodeViewModel {
        ICollection<ExplorerItemNodeViewModel> _nodeChildren;
        // ReSharper disable TooManyDependencies
        public ExplorerItemNodeViewModel(IShellViewModel shellViewModel, IServer server, IExplorerHelpDescriptorBuilder builder, IExplorerItemViewModel parent)
            // ReSharper restore TooManyDependencies
            : base(shellViewModel, server, builder, parent)
        {
            Self = this;
            Weight = 1;
        }


        #region Implementation of IExplorerItemNodeViewModel

        public IExplorerItemNodeViewModel Self { get; set; }
        public int Weight { get; set; }
        public ICollection<ExplorerItemNodeViewModel> NodeChildren
        {
            get
            {
                return new ObservableCollection<ExplorerItemNodeViewModel>(Children.Select((a=> a as ExplorerItemNodeViewModel)));
            }
            set
            {
                _nodeChildren = value;
            }
        }

        public ICollection<IExplorerItemNodeViewModel> AsList()
        {
            return null;
        }

        #endregion
    }
}