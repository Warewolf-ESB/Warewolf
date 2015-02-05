using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemNodeViewModel : ExplorerItemViewModel, IExplorerItemNodeViewModel {
        ICollection<ExplorerItemNodeViewModel> _nodeChildren;
        Visibility _textVisibility;
        // ReSharper disable TooManyDependencies
        public ExplorerItemNodeViewModel(IShellViewModel shellViewModel, IServer server, IExplorerHelpDescriptorBuilder builder, IExplorerItemViewModel parent)
            // ReSharper restore TooManyDependencies
            : base(shellViewModel, server, builder, parent)
        {
            Self = this;
            Weight = 1;
            IsMainNode = Visibility.Collapsed;

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

        public Visibility IsMainNode { get; set; }

        #endregion

        public void SetTextVisibility(bool? isChecked)
        {
            if(isChecked!= null)
            {
                if(isChecked.Value)
                {
                    TextVisibility = Visibility.Visible;
                }
                else
                {
                    TextVisibility = Visibility.Collapsed;
                }
                foreach(var explorerItemNodeViewModel in NodeChildren)
                {
                    SetTextVisibility(isChecked);
                }
            }
        }

        public Visibility TextVisibility
        {
            get
            {
                return _textVisibility;
            }
            set
            {
                _textVisibility = value;
                OnPropertyChanged(()=>TextVisibility);
            }
        }
    }
}