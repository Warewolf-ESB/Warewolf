using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels;

namespace Warewolf.UIBindingTests.Deploy
{
    class DeploySourceExplorerViewModelForTesting : DeploySourceExplorerViewModel
    {
        public IList<IExplorerItemViewModel> Children { get; set; }
        public void SetSelecetdItems(IEnumerable<IExplorerTreeItem> items)
        {
            foreach (var explorerTreeItem in items)
                SelectedItems.Add(explorerTreeItem);
        }

        public void SetSelecetdItemsForConflicts(IEnumerable<IExplorerTreeItem> items)
        {
            foreach (var explorerTreeItem in items)
            {
                var explorerItem = new Mock<IExplorerTreeItem>();
                explorerItem.SetupGet(item => item.ResourceId).Returns(Guid.NewGuid);
                explorerItem.SetupGet(item => item.ResourceName).Returns(explorerTreeItem.ResourceName);
                explorerItem.SetupGet(item => item.ResourcePath).Returns(explorerTreeItem.ResourcePath);
                SelectedItems.Add(explorerItem.Object);
            }
        }

        #region Overrides of DeploySourceExplorerViewModel

        /// <summary>
        /// root and all children of selected items
        /// </summary>
        public override ICollection<IExplorerTreeItem> SelectedItems { get; set; }

        #endregion

        public DeploySourceExplorerViewModelForTesting(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IDeployStatsViewerViewModel statsArea)
            : base(shellViewModel, aggregator, statsArea)
        {
            // ReSharper disable once VirtualMemberCallInContructor
            SelectedItems = new List<IExplorerTreeItem>();
        }

        public override Version ServerVersion => new Version(SelectedServer.GetServerVersion());

        #region Overrides of DeploySourceExplorerViewModel

        protected override void LoadEnvironment(IEnvironmentViewModel localhostEnvironment)
        {
            localhostEnvironment.Children = new ObservableCollection<IExplorerItemViewModel>(Children ?? new List<IExplorerItemViewModel> { CreateExplorerVMS() });
            PrivateObject p = new PrivateObject(localhostEnvironment);
            p.SetField("_isConnected", true);
            localhostEnvironment.ResourceId = Guid.Empty;
            AfterLoad(localhostEnvironment.Server.EnvironmentID);
        }

        IExplorerItemViewModel CreateExplorerVMS()
        {
            ExplorerItemViewModel ax = null;
            var perm = new Mock<IServer>();
            List<IWindowsGroupPermission> perms = new List<IWindowsGroupPermission>();
            perms.Add(new WindowsGroupPermission
            {
                Permissions = Permissions.View
            });
            perm.Setup(server => server.Permissions)
                .Returns(perms);
            ax = new ExplorerItemViewModel(new Mock<IServer>().Object, null, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object)
            {
                ResourceName = "Examples",
                ResourcePath = "Utility - Date and Time",
                ResourceId = Guid.NewGuid(),
                Children = new ObservableCollection<IExplorerItemViewModel>
                {
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Utility - Date and Time", ResourcePath = "Examples\\Utility - Date and Time",ResourceType = "WorkflowService" },
                    new ExplorerItemViewModel(perm.Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("9CC8CA4E-8261-433F-8EF1-612DE003907C"),ResourceName = "bob", ResourcePath = "Examples\\bob"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("5C8B5660-CE6E-4D22-84D8-5B77DC749F70"),ResourceName = "DemoDB", ResourcePath = "sqlServers\\DemoDB",ResourceType = "DbSource" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Data - Data - Data Split", ResourcePath = "Examples\\Data - Data - Data Split" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Control Flow - Switch", ResourcePath = "Examples\\Control Flow - Switch" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Control Flow - Sequence", ResourcePath = "Examples\\Control Flow - Sequence" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "File and Folder - Copy", ResourcePath = "Examples\\File and Folder - Copy" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "File and Folder - Create", ResourcePath = "Examples\\File and Folder - Create" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "FetchPlayers", ResourcePath = "DB Service\\FetchPlayers",ResourceType = "DbService"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Source", ResourcePath = "Remote\\Source",ResourceType = "DbSource"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "NameIdConflict", ResourcePath = "Examples\bob",ResourceType = "DbSource"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("7CC8CA4E-8261-433F-8EF1-612DE003907C"),ResourceName = "DifferentNameSameID", ResourcePath = "Examples\\DifferentNameSameID",ResourceType = "DbSource"}
                },
                UnfilteredChildren = new ObservableCollection<IExplorerItemViewModel>
                {
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Utility - Date and Time", ResourcePath = "Examples\\Utility - Date and Time",ResourceType = "WorkflowService" },
                    new ExplorerItemViewModel(perm.Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("9CC8CA4E-8261-433F-8EF1-612DE003907C"),ResourceName = "bob", ResourcePath = "Examples\\bob"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("5C8B5660-CE6E-4D22-84D8-5B77DC749F70"),ResourceName = "DemoDB", ResourcePath = "sqlServers\\DemoDB",ResourceType = "DbSource" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Data - Data - Data Split", ResourcePath = "Examples\\Data - Data - Data Split" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Control Flow - Switch", ResourcePath = "Examples\\Control Flow - Switch" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Control Flow - Sequence", ResourcePath = "Examples\\Control Flow - Sequence" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "File and Folder - Copy", ResourcePath = "Examples\\File and Folder - Copy" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "File and Folder - Create", ResourcePath = "Examples\\File and Folder - Create" },
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "FetchPlayers", ResourcePath = "DB Service\\FetchPlayers",ResourceType = "DbService"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "Source", ResourcePath = "Remote\\Source",ResourceType = "DbSource"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.NewGuid(),ResourceName = "NameIdConflict", ResourcePath = "Examples\bob",ResourceType = "DbSource"},
                    new ExplorerItemViewModel(new Mock<IServer>().Object, ax, a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object) {ResourceId = Guid.Parse("7CC8CA4E-8261-433F-8EF1-612DE003907C"),ResourceName = "DifferentNameSameID", ResourcePath = "Examples\\DifferentNameSameID",ResourceType = "DbSource"}
                }
            };
            ax.Children.Apply(a => a.Parent = ax);
            return ax;
        }

        #endregion
    }
}