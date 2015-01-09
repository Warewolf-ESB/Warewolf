using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;
using Moq;
using Warewolf.Studio.Models.Toolbox;
using Warewolf.Studio.ViewModels.ToolBox;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyToolboxViewModel:BindableBase, IToolboxViewModel
    {
        bool _isDesignerFocused;
        bool _isEnabled;

        #region Implementation of IToolboxViewModel

        public DummyToolboxViewModel()
        {
            SetupTools();
            IsEnabled = true;
        }

        void SetupTools()
        {
            Tools = new ObservableCollection<IToolDescriptorViewModel>();
            ResourceDictionary rd = new ResourceDictionary { Source = new Uri("/Warewolf.Studio.ViewModel;component/DummyModels/Icons.xaml", UriKind.RelativeOrAbsolute) };
            Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "name", (DrawingImage)rd["TaskScheduler-32"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "tools", ToolType.Native), true));
            Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "name", (DrawingImage)rd["HelpCommunity-32"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "tools", ToolType.Native), true));
            Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "name", (DrawingImage)rd["ClearFilter-32"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "tools", ToolType.Native), true));
        }

        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolDescriptorViewModel> Tools { get; private set; }
        /// <summary>
        /// points to the active servers tools. unlike explorer, this only ever needs to look at one set of tools at a time
        /// </summary>
        public ICollection<IToolboxCatergoryViewModel> CategorisedTools { get; private set; }
        /// <summary>
        /// the toolbox is only enabled when the active server is connected and the designer is in focus
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return IsDesignerFocused && _isEnabled;
            }
            private set
            {
                OnPropertyChanged("IsEnabled");
                _isEnabled = value;
            }
        }

        public void SetEnabled(bool val)
        {
            _isEnabled = val;
        }
        /// <summary>
        /// filters the list of tools available to the user.
        /// </summary>
        /// <param name="searchString"></param>
        public void Filter(string searchString)
        {
            SetupTools();
            // ReSharper disable ReturnValueOfPureMethodIsNotUsed
            Tools.Where(a => a.Tool.Name.Contains(searchString));
            // ReSharper restore ReturnValueOfPureMethodIsNotUsed
        }

        /// <summary>
        /// Clear Filters all tools visible
        /// </summary>
        public void ClearFilter()
        {
            SetupTools();
        }

        /// <summary>
        /// Is the designer focused. This is used externally to disable the toolbox.
        /// </summary>
        public bool IsDesignerFocused
        {
            get
            {
                return _isDesignerFocused;
            }
            set
            {
                OnPropertyChanged("IsEnabled");
                _isDesignerFocused = value;
            }
        }

        #endregion
    }
}
