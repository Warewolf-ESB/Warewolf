using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Moq;
using Warewolf.Studio.Models.Toolbox;
using Warewolf.Studio.ViewModels.ToolBox;

namespace Warewolf.Studio.ViewModels.DummyModels
{
    public class DummyToolboxViewModel:ToolboxViewModel
    {
        bool _isDesignerFocused;
        bool _isEnabled;
        IToolDescriptorViewModel _selectedTool;

        #region Implementation of IToolboxViewModel

        public DummyToolboxViewModel()
            : base(new ToolboxModel(new DummyServer(), new DummyServer(), new Mock<IPluginProxy>().Object), new ToolboxModel(new DummyServer(), new DummyServer(), new Mock<IPluginProxy>().Object))
        {
            SetupTools();
        }

        void SetupTools()
        {
            Tools = new ObservableCollection<IToolDescriptorViewModel>();
            ResourceDictionary rd = new ResourceDictionary { Source = new Uri("/Warewolf.Studio.ViewModels;component/DummyModels/Icons.xaml", UriKind.RelativeOrAbsolute) };
            Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "name", (DrawingImage)rd["TaskScheduler-32"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "tools", ToolType.Native), true));
            Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "name", (DrawingImage)rd["ToolDataSplit-32"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "tools", ToolType.Native), true));
            Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "name", (DrawingImage)rd["ToolSystemInformation-32"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "tools", ToolType.Native), true));
        }

        #endregion
    }
}
