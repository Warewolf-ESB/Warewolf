using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Moq;
using Warewolf.Core;
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
            : base(new ToolboxModel(new Mock<IServer>().Object, new Mock<IServer>().Object, new Mock<IPluginProxy>().Object), new ToolboxModel(new Mock<IServer>().Object, new Mock<IServer>().Object, new Mock<IPluginProxy>().Object))
        {
            SetupTools();
        }

        void SetupTools()
        {
            Tools = new ObservableCollection<IToolDescriptorViewModel>();
            ResourceDictionary rd = new ResourceDictionary { Source = new Uri("/Warewolf.Studio.Themes.Luna;component/Theme.xaml", UriKind.RelativeOrAbsolute) };
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Copy Records", (DrawingImage)rd["RecordSet-CopyRecord"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Count Records", (DrawingImage)rd["RecordSet-CountRecords"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Delete", (DrawingImage)rd["RecordSet-Delete"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Find Records", (DrawingImage)rd["RecordSet-FindRecords"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Length", (DrawingImage)rd["RecordSet-Length"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Sort Records", (DrawingImage)rd["RecordSet-SortRecords"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "SQL Bulk Insert", (DrawingImage)rd["RecordSet-SQLBulkInsert"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
            //Tools.Add(new ToolDescriptorViewModel(new ToolDescriptor(Guid.NewGuid(), typeof(string), typeof(string), typeof(Vector), "Unique Records", (DrawingImage)rd["RecordSet-UniqueRecords"], new Version(1, 0, 1), new Mock<IHelpDescriptor>().Object, true, "Recordset", ToolType.Native), true));
             }

        #endregion
    }
}
