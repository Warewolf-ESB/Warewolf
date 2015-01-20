using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Infragistics.Windows.DockManager;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;
using Moq;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Infragistics_Prism_Region_Adapter;
using Warewolf.Studio.ViewModels.DummyModels;
using Warewolf.Studio.ViewModels.VariableList;
using Warewolf.Studio.Views;
using Warewolf.Studio.Views.Variable_List;

namespace Warewolf.Studio
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            var window = (Window)Shell;

            var regionManager = Container.Resolve<IRegionManager>();
            var explorerRegion = regionManager.Regions[RegionNames.Explorer];
            var explorerView = new ExplorerView { DataContext = new DummyExplorerViewModel() };
            explorerRegion.Add(explorerView, RegionNames.Explorer);
            explorerRegion.Activate(explorerView);

            var toolboxRegion = regionManager.Regions[RegionNames.Toolbox];
            var toolBoxView = new ToolboxView { DataContext = new DummyToolboxViewModel() };
            toolboxRegion.Add(toolBoxView, RegionNames.Toolbox);
            toolboxRegion.Activate(toolBoxView);
            VariableListViewModel vm ;
            ICollection<IVariableListViewColumnViewModel> colls = new ObservableCollection<IVariableListViewColumnViewModel>();
            colls.Add( new VariableListColumnViewModel("col", "bob", new Mock<IVariableListViewModel>().Object, colls){Input = true}) ;
            var variableListRegion = regionManager.Regions[RegionNames.VariableList];
            var convertedRecset = new VariableListViewRecordSetViewModel("bob", colls, new Mock<IVariableListViewModel>().Object, new List<IVariablelistViewRecordSetViewModel>());
            var convertedRecset2 = new VariableListViewRecordSetViewModel("dave", new VariableListColumnViewModel[0], new Mock<IVariableListViewModel>().Object, new List<IVariablelistViewRecordSetViewModel>());
            var convertedScalar = new VariableListItemViewScalarViewModel("martha", new Mock<IVariableListViewModel>().Object, new List<IVariableListViewScalarViewModel>());
            var convertedScalar2 = new VariableListItemViewScalarViewModel("stewart", new Mock<IVariableListViewModel>().Object, new List<IVariableListViewScalarViewModel>()){Used = false};
            var expressions = new List<IDataExpression>{new Mock<IDataExpression>().Object,new Mock<IDataExpression>().Object,new Mock<IDataExpression>().Object,new Mock<IDataExpression>().Object};
            var convertor = new Mock<IDatalistViewExpressionConvertor>();
            convertor.Setup(a => a.Create(expressions[0])).Returns(convertedRecset);
            convertor.Setup(a => a.Create(expressions[1])).Returns(convertedRecset2);
            convertor.Setup(a => a.Create(expressions[2])).Returns(convertedScalar);
            convertor.Setup(a => a.Create(expressions[3])).Returns(convertedScalar2);
            var variableList = new VariableListView() { DataContext = new VariableListViewModel(expressions,convertor.Object) };
            variableListRegion.Add(variableList, RegionNames.VariableList);
            var menuRegion = regionManager.Regions[RegionNames.Menu];
            var menuView = new MenuView();
            menuRegion.Add(menuView, RegionNames.Menu);
            menuRegion.Activate(menuView);

            window.Show();

        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping(typeof(TabGroupPane), Container.Resolve<TabGroupPaneRegionAdapter>());
            return mappings;
        }

    }
}
