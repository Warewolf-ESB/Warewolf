using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemNodeViewModel : ExplorerItemViewModel, IExplorerItemNodeViewModel {
        Visibility _textVisibility;
        Visibility _isMainNode;
        // ReSharper disable TooManyDependencies
        public ExplorerItemNodeViewModel(IShellViewModel shellViewModel, IServer server, IExplorerHelpDescriptorBuilder builder, IExplorerItemViewModel parent)
            // ReSharper restore TooManyDependencies
            : base(shellViewModel, server, builder, parent)
        {
            Self = this;
            Weight = 1;
            IsMainNode = Visibility.Collapsed;
            IsNotMainNode = Visibility.Visible;
        }


        #region Implementation of IExplorerItemNodeViewModel

        public IExplorerItemNodeViewModel Self { get; set; }
        public int Weight { get; set; }
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        public ICollection<ExplorerItemNodeViewModel> NodeChildren
            // ReSharper restore ReturnTypeCanBeEnumerable.Global
        {
            get
            {
                return new ObservableCollection<ExplorerItemNodeViewModel>(Children.Select((a=> a as ExplorerItemNodeViewModel)));
            }
        }

        public ICollection<IExplorerItemNodeViewModel> AsList()
        {
            return null;
        }

        public Visibility IsMainNode
        {
            get
            {
                return _isMainNode;
            }
            set
            {
                
                _isMainNode = value;
                if(value == Visibility.Visible)
                {
                    IsNotMainNode = Visibility.Collapsed;
                }
            }
        }
        public Visibility IsNotMainNode { get; set; }

        #endregion

        public Visibility TextVisibility
        {
            private get
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