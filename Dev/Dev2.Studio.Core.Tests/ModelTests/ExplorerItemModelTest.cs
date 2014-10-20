
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Statements;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.ConnectionHelpers;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.Utils;
using Dev2.Interfaces;
using Dev2.Messages;
using Dev2.Models;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.ModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExplorerItemModelTest
    {
        [TestInitialize]
        public void Init()
        {
            CustomContainer.Clear();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_Constructor")]
        public void ExplorerItemModel_Constructor_HasChildrenCollectionInstantiated()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModel);
            Assert.IsNotNull(explorerItemModel.Children);
            Assert.AreEqual(0, explorerItemModel.ChildrenCount);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Constructor")]
        public void ExplorerItemModel_Constructor_Has4ChildrenCount()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>().Object;
            var explorerItemModel = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object);
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModel);
            Assert.IsNotNull(explorerItemModel.Children);
            Assert.AreEqual(4, explorerItemModel.ChildrenCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_Constructor")]
        public void ExplorerItemModel_Constructor_With4WorkFlowServicesAndTwoVersionServices_DeployChildrenIsFour()
        {
            //------------Execute Test---------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>().Object;
            var explorerItemModel = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object);
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Version });
            explorerItemModel.Children.Add(new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Version });
            //------------Assert Results-------------------------
            Assert.IsNotNull(explorerItemModel);
            Assert.IsNotNull(explorerItemModel.Children);
            Assert.AreEqual(4, explorerItemModel.ChildrenCount);
            Assert.AreEqual(4, explorerItemModel.DeployChildren.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_Clone")]
        public void ExplorerItemModel_Clone_ReturnsNewObjectWithAllPropertiesSet()
        {
            //------------Setup for test--------------------------
            var connectControlSingleton = new Mock<IConnectControlSingleton>().Object;
            var explorerItemModel = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object);
            var parentModel = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object);
            explorerItemModel.DisplayName = "Test1";
            var childItemModel = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object);
            explorerItemModel.Children.Add(childItemModel);
            explorerItemModel.EnvironmentId = Guid.NewGuid();
            explorerItemModel.Parent = parentModel;
            explorerItemModel.Permissions = Permissions.Contribute;
            explorerItemModel.ResourceId = Guid.NewGuid();
            explorerItemModel.ResourcePath = "TestResourcePath";
            explorerItemModel.ResourceType = ResourceType.PluginService;
            explorerItemModel.VersionInfo = new VersionInfo { VersionNumber = "2" , Reason = "Save", ResourceId =  Guid.NewGuid()};

            var properties = typeof(ExplorerItemModel).GetProperties();

            //------------Execute Test---------------------------
            var clonedItemModel = explorerItemModel.Clone(connectControlSingleton, new Mock<IStudioResourceRepository>().Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(64, properties.Length);
            Assert.AreEqual(explorerItemModel.DisplayName, clonedItemModel.DisplayName);
            Assert.AreEqual(explorerItemModel.Children.Count, clonedItemModel.Children.Count);
            Assert.AreEqual(explorerItemModel.CanConnect, clonedItemModel.CanConnect);
            Assert.AreEqual(explorerItemModel.CanDelete, clonedItemModel.CanDelete);
            Assert.AreEqual(explorerItemModel.CanDebug, clonedItemModel.CanDebug);
            Assert.AreEqual(explorerItemModel.CanDeploy, clonedItemModel.CanDeploy);
            Assert.AreEqual(explorerItemModel.CanDisconnect, clonedItemModel.CanDisconnect);
            Assert.AreEqual(explorerItemModel.CanEdit, clonedItemModel.CanEdit);
            Assert.AreEqual(explorerItemModel.CanExecute, clonedItemModel.CanExecute);
            Assert.AreEqual(explorerItemModel.CanRemove, clonedItemModel.CanRemove);
            Assert.AreEqual(explorerItemModel.CanRename, clonedItemModel.CanRename);
            Assert.AreEqual(explorerItemModel.CanShowDependencies, clonedItemModel.CanShowDependencies);
            Assert.AreEqual(explorerItemModel.CanSelectDependencies, clonedItemModel.CanSelectDependencies);
            Assert.AreEqual(explorerItemModel.EnvironmentId, clonedItemModel.EnvironmentId);
            Assert.AreEqual(explorerItemModel.Parent, clonedItemModel.Parent);
            Assert.AreEqual(explorerItemModel.Permissions, clonedItemModel.Permissions);
            Assert.AreEqual(explorerItemModel.ResourceId, clonedItemModel.ResourceId);
            Assert.AreEqual(explorerItemModel.ResourcePath, clonedItemModel.ResourcePath);
            Assert.AreEqual(explorerItemModel.ResourceType, clonedItemModel.ResourceType);
            Assert.AreEqual(explorerItemModel.VersionInfo, clonedItemModel.VersionInfo);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_WorkflowService_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestWorkflow1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.WorkflowService, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(true, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }



        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_DbService_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestDbService1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbService, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.DbService, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(true, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_PluginService_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestPluginService1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginService, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.PluginService, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(true, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_WebService_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestWebService1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebService, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.WebService, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(true, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_DbSource_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestDbSource1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbSource, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.DbSource, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_PluginSource_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestPluginSource1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginSource, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.PluginSource, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_WebSource_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestWebSource1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebSource, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.WebSource, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_EmailSource_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestEmailSource1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.EmailSource, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.EmailSource, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(true, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(true, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(true, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_ServerSource_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestServerSource1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.ServerSource, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.ServerSource, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(true, explorerItemModel.CanEdit);
            Assert.AreEqual(false, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(false, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(false, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_Folder_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestFolder1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.Folder, explorerItemModel.ResourceType);
            Assert.AreEqual(true, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(true, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(true, explorerItemModel.CanDelete);
            Assert.AreEqual(false, explorerItemModel.CanEdit);
            Assert.AreEqual(false, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(true, explorerItemModel.CanDeploy);
            Assert.AreEqual(false, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(false, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_Server_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestServer1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.Server, explorerItemModel.ResourceType);
            Assert.AreEqual(true, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(false, explorerItemModel.CanRename);
            Assert.AreEqual(true, explorerItemModel.CanRemove);
            Assert.AreEqual(false, explorerItemModel.CanDelete);
            Assert.AreEqual(false, explorerItemModel.CanEdit);
            Assert.AreEqual(false, explorerItemModel.CanExecute);
            Assert.AreEqual(true, explorerItemModel.CanConnect);
            Assert.AreEqual(false, explorerItemModel.CanDeploy);
            Assert.AreEqual(false, explorerItemModel.CanShowDependencies);
            Assert.AreEqual(false, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(true, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ExplorerItemModel_Convert")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_ContructorTest_ReservedService_PropertiesSetCorrectly()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            const string name = "TestReservedService1";
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.ReservedService, Permissions = Permissions.Administrator, DisplayName = name };
            //------------Assert Results-------------------------
            Assert.AreEqual(name, explorerItemModel.DisplayName);
            Assert.AreEqual(Permissions.Administrator, explorerItemModel.Permissions);
            Assert.AreEqual(ResourceType.ReservedService, explorerItemModel.ResourceType);
            Assert.AreEqual(false, explorerItemModel.CanAddResoure);
            Assert.AreEqual(false, explorerItemModel.CanDebug);
            Assert.AreEqual(false, explorerItemModel.CanRename);
            Assert.AreEqual(false, explorerItemModel.CanRemove);
            Assert.AreEqual(false, explorerItemModel.CanDelete);
            Assert.AreEqual(false, explorerItemModel.CanEdit);
            Assert.AreEqual(false, explorerItemModel.CanExecute);
            Assert.AreEqual(false, explorerItemModel.CanConnect);
            Assert.AreEqual(false, explorerItemModel.CanDeploy);
            Assert.AreEqual(false, explorerItemModel.CanSelectDependencies);
            Assert.AreEqual(false, explorerItemModel.CanDisconnect);
            Assert.AreEqual(false, explorerItemModel.IsRenaming);
            Assert.AreEqual(true, explorerItemModel.IsConnected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerSelected);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceSelected);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetSelected);
            Assert.AreEqual(false, explorerItemModel.IsExplorerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsResourcePickerExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeploySourceExpanded);
            Assert.AreEqual(false, explorerItemModel.IsDeployTargetExpanded);
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_EditCommand")]
        public void ExplorerItemModel_EditCommand_HasSelectedItem_CallEditForSelectedResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                actualResourceInvoked = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.EditCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(actualResourceInvoked);
            Assert.AreEqual(resourceId, actualResourceInvoked.ID);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_EditCommand")]
        public void ExplorerItemModel_EditCommand_HasSelectedItem_EnvironmentNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                actualResourceInvoked = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, Guid.NewGuid(), resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.EditCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNull(actualResourceInvoked);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_EditCommand")]
        public void ExplorerItemModel_EditCommand_HasSelectedItem_ResourceNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                actualResourceInvoked = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var setupExplorerItemModelWithFolderAndOneChild = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.EditCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(setupExplorerItemModelWithFolderAndOneChild);
            Assert.IsNull(actualResourceInvoked);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_EditCommand")]
        public void ExplorerItemModel_EditCommand_ResourceTypeIsVersionAndGetVersionReturnsNull_AddWorkSurfaceMessageIsNotPublished()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                actualResourceInvoked = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            IExplorerItemModel outvalue;
            var studioResourceRepo = new Mock<IStudioResourceRepository>();
            studioResourceRepo.Setup(s => s.GetVersion(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()))
                .Returns((StringBuilder)null)
                .Verifiable();
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.Empty, Guid.NewGuid(), out outvalue, studioResourceRepo.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            explorerItemModel.ResourceType = ResourceType.Version;
            //------------Execute Test---------------------------
            explorerItemModel.EditCommand.Execute(null);
            //------------Assert Result--------------------------
            Assert.IsNull(actualResourceInvoked);
            studioResourceRepo.Verify(s => s.GetVersion(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_EditCommand")]
        public void ExplorerItemModel_EditCommand_ResourceTypeIsVersionAndGetVersionReturnsAStringBuilder_AddWorkSurfaceMessageIsPublished()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                actualResourceInvoked = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            IExplorerItemModel outvalue;
            var studioResourceRepo = new Mock<IStudioResourceRepository>();
            studioResourceRepo.Setup(s => s.GetVersion(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()))
                .Returns(new StringBuilder("Hail HTML5"))
                .Verifiable();
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.Empty, Guid.NewGuid(), out outvalue, studioResourceRepo.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            explorerItemModel.ResourceType = ResourceType.Version;
            var parent = new Mock<IExplorerItemModel>();
            parent.Setup(p => p.DisplayName).Returns("Javascript the enabler");
            explorerItemModel.Parent = parent.Object;
            var versionMock = new Mock<IVersionInfo>();
            versionMock.Setup(v => v.VersionNumber).Returns("3");
            explorerItemModel.VersionInfo = versionMock.Object;
            //------------Execute Test---------------------------
            explorerItemModel.EditCommand.Execute(null);
            //------------Assert Result--------------------------
            Assert.IsNotNull(actualResourceInvoked);
            studioResourceRepo.Verify(s => s.GetVersion(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DeleteCommand")]
        public void ExplorerItemModel_DeleteCommand_HasSelectedItem_CallDeleteForSelectedResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            Collection<IContextualResourceModel> actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DeleteResourcesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DeleteResourcesMessage) ? (msg as DeleteResourcesMessage).ResourceModels : null;
                actualResourceInvoked = (workSurfaceObject is Collection<IContextualResourceModel>) ? (workSurfaceObject as Collection<IContextualResourceModel>) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(actualResourceInvoked);
            Assert.AreEqual(1, actualResourceInvoked.Count);
            Assert.AreEqual(resourceId, actualResourceInvoked[0].ID);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DeleteCommand")]
        public void ExplorerItemModel_DeleteCommand_FolderWithNoItems_CallDeleteFolder()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualFolderNameInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DeleteFolderMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DeleteFolderMessage) ? (msg as DeleteFolderMessage).FolderName : null;
                actualFolderNameInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderNoChild(displayName, envID, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(actualFolderNameInvoked);
            Assert.AreEqual(resourceItem.DisplayName, actualFolderNameInvoked);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DeleteCommand")]
        public void ExplorerItemModel_DeleteCommand_HasSelectedItemHasChildren_CallDeleteForAllChildren()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            Collection<IContextualResourceModel> actualResourceInvoked = null;
            string folderName = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DeleteResourcesMessage>())).Callback<object>(msg =>
            {
                DeleteResourcesMessage deleteResourcesMessage = (msg as DeleteResourcesMessage);
                var workSurfaceObject = (msg is DeleteResourcesMessage) ? deleteResourcesMessage.ResourceModels : null;
                actualResourceInvoked = (workSurfaceObject is Collection<IContextualResourceModel>) ? (workSurfaceObject as Collection<IContextualResourceModel>) : null;
                if(deleteResourcesMessage != null)
                {
                    folderName = deleteResourcesMessage.FolderName;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndFolderChildAndResources(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            resourceItem.Parent.Children.Add(resourceItem);
            resourceItem.Parent.Children.Add(resourceItem);
            //------------Execute Test---------------------------
            resourceItem.Parent.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------


            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(actualResourceInvoked);
            Assert.AreEqual(3, actualResourceInvoked.Count);
            Assert.AreEqual(resourceId, actualResourceInvoked[0].ID);
            Assert.AreEqual(resourceId, actualResourceInvoked[1].ID);
            Assert.AreEqual(resourceId, actualResourceInvoked[2].ID);
            Assert.IsNotNull(folderName);
            Assert.AreEqual(resourceItem.Parent.DisplayName, folderName);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_DeleteCommand")]
        public void ExplorerItemModel_DeleteCommand_HasSelectedItemHasChildren_CallDelete_RelaodForWebSource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            aggregator.Setup(a => a.Publish(It.IsAny<DeleteResourcesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DeleteResourcesMessage) ? (msg as DeleteResourcesMessage).ResourceModels : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);

            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IContextualResourceModel)null);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndFolderChildAndResources(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            serverItem.Children.First().Children.First().ResourceType = ResourceType.WebSource;
            var item = serverItem.Children.First().Children.First();
            resourceItem.Parent.Children.Add(resourceItem);
            resourceItem.Parent.Children.Add(resourceItem);
            //------------Execute Test---------------------------
            resourceItem.Parent.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            mockResourceRepository.Verify(a => a.ReloadResource(item.ResourceId, Studio.Core.AppResources.Enums.ResourceType.Source, It.IsAny<ResourceModelEqualityComparer>(), true), Times.Once());

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DeleteCommand")]
        public void ExplorerItemModel_DeleteCommand_HasSelectedItem_EnvironmentNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            Collection<IContextualResourceModel> actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DeleteResourcesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DeleteResourcesMessage) ? (msg as DeleteResourcesMessage).ResourceModels : null;
                actualResourceInvoked = (workSurfaceObject is Collection<IContextualResourceModel>) ? (workSurfaceObject as Collection<IContextualResourceModel>) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, Guid.NewGuid(), resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNull(actualResourceInvoked);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DeleteCommand")]
        public void ExplorerItemModel_DeleteCommand_HasSelectedItem_ResourceNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            Collection<IContextualResourceModel> actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DeleteResourcesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DeleteResourcesMessage) ? (msg as DeleteResourcesMessage).ResourceModels : null;
                actualResourceInvoked = (workSurfaceObject is Collection<IContextualResourceModel>) ? (workSurfaceObject as Collection<IContextualResourceModel>) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var setupExplorerItemModelWithFolderAndOneChild = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DeleteCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(setupExplorerItemModelWithFolderAndOneChild);
            Assert.IsNull(actualResourceInvoked);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DeployCommand")]
        public void ExplorerItemModel_DeployCommand_HasSelectedItem_CallDeployForSelectedResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IExplorerItemModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DeployResourcesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DeployResourcesMessage) ? (msg as DeployResourcesMessage).ViewModel : null;
                actualResourceInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DeployCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(actualResourceInvoked);
            Assert.AreEqual(resourceId, actualResourceInvoked.ResourceId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_ShowDependenciesCommand")]
        public void ExplorerItemModel_ShowDependenciesCommand_HasSelectedItem_CallShowDependenciesForSelectedResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            var dependsOnMe = false;
            aggregator.Setup(a => a.Publish(It.IsAny<ShowDependenciesMessage>())).Callback<object>(msg =>
            {
                if(msg is ShowDependenciesMessage)
                {
                    var showDependenciesMessage = msg as ShowDependenciesMessage;
                    var workSurfaceObject = showDependenciesMessage.ResourceModel;
                    dependsOnMe = showDependenciesMessage.ShowDependentOnMe;
                    actualResourceInvoked = workSurfaceObject;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.ShowDependenciesCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(actualResourceInvoked);
            Assert.AreEqual(resourceId, actualResourceInvoked.ID);
            Assert.IsTrue(dependsOnMe);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_ShowDependenciesCommand")]
        public void ExplorerItemModel_ShowDependenciesCommand_HasSelectedItem_EnvironmentNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<ShowDependenciesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowDependenciesMessage) ? (msg as ShowDependenciesMessage).ResourceModel : null;
                actualResourceInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, Guid.NewGuid(), resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.ShowDependenciesCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNull(actualResourceInvoked);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_ShowDependenciesCommand")]
        public void ExplorerItemModel_ShowDependenciesCommand_HasSelectedItem_ResourceNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<ShowDependenciesMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowDependenciesMessage) ? (msg as ShowDependenciesMessage).ResourceModel : null;
                actualResourceInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var setupExplorerItemModelWithFolderAndOneChild = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.ShowDependenciesCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(setupExplorerItemModelWithFolderAndOneChild);
            Assert.IsNull(actualResourceInvoked);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DebugCommand")]
        public void ExplorerItemModel_DebugCommand_HasSelectedItem_CallDebugForSelectedResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DebugResourceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DebugResourceMessage) ? (msg as DebugResourceMessage).Resource : null;
                actualResourceInvoked = workSurfaceObject;
            });

            IResourceModel editResource = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                editResource = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DebugCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNotNull(editResource);
            Assert.AreEqual(resourceId, editResource.ID);
            Assert.IsNotNull(actualResourceInvoked);
            Assert.AreEqual(resourceId, actualResourceInvoked.ID);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DebugCommand")]
        public void ExplorerItemModel_DebugCommand_HasSelectedItem_EnvironmentNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DebugResourceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DebugResourceMessage) ? (msg as DebugResourceMessage).Resource : null;
                actualResourceInvoked = workSurfaceObject;
            });

            IResourceModel editResource = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                editResource = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, Guid.NewGuid(), resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DebugCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.IsNull(editResource);
            Assert.IsNull(actualResourceInvoked);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_DebugCommand")]
        public void ExplorerItemModel_DebugCommand_HasSelectedItem_ResourceNotFound_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<DebugResourceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is DebugResourceMessage) ? (msg as DebugResourceMessage).Resource : null;
                actualResourceInvoked = workSurfaceObject;
            });

            IResourceModel editResource = null;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is AddWorkSurfaceMessage) ? (msg as AddWorkSurfaceMessage).WorkSurfaceObject : null;
                editResource = (workSurfaceObject is IResourceModel) ? (workSurfaceObject as IResourceModel) : null;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns((IResourceModel)null);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var setupExplorerItemModelWithFolderAndOneChild = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.DebugCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(setupExplorerItemModelWithFolderAndOneChild);
            Assert.IsNull(editResource);
            Assert.IsNull(actualResourceInvoked);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_Null_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("", actualResourceType);
            Assert.AreEqual("", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_EmptyString_DoesNothing()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("", actualResourceType);
            Assert.AreEqual("", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_WorkflowService_CallsNewResourceWithWorkflowResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("WorkflowService");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("WorkflowService", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_DatabaseService_CallsNewResourceWithDatabaseServiceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("DatabaseService");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("DatabaseService", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_ResourceService_CallsNewResourceWithResourceServiceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("ResourceService");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("ResourceService", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_WebService_CallsNewResourceWithWebServiceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("WebService");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("WebService", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_Server_CallsNewResourceWithServerResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("Server");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("Server", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_DbSource_CallsNewResourceWithDbSourceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("DbSource");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("DbSource", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_ResourceSource_CallsNewResourceWithResourceSourceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("ResourceSource");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("ResourceSource", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_WebSource_CallsNewResourceWithResourceSourceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("WebSource");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("WebSource", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_RenameCommand_HasSelectedItem_SetsIsRenamingToTrue()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
#pragma warning disable 219
            Guid resourceGuid;

            string oldName;
            string newName;
#pragma warning restore 219
            aggregator.Setup(a => a.Publish(It.IsAny<UpdateWorksurfaceFlowNodeDisplayName>())).Callback<object>(msg =>
                {
                    var workSurfaceObject = (msg as UpdateWorksurfaceFlowNodeDisplayName);
                    if(workSurfaceObject != null)
                    {
                        resourceGuid = workSurfaceObject.WorkflowDesignerResourceID;
                        oldName = workSurfaceObject.OldName;
                        newName = workSurfaceObject.NewName;
                    }
                });

            EventPublishers.Aggregator = aggregator.Object;
            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);
            var mockStudioRepository = new Mock<IStudioResourceRepository>();

            mockStudioRepository.Setup(a => a.RenameItem(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));
            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(resourceItem.IsRenaming);
            resourceItem.RenameCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(resourceItem.IsRenaming);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_NewResourceCommand")]
        public void ExplorerItemModel_NewResourceCommand_HasSelectedItem_EmailSource_CallsNewResourceWithResourceSourceResource()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            string actualResourceType = "";
            string actualResourceCategory = "";
            aggregator.Setup(a => a.Publish(It.IsAny<ShowNewResourceWizard>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is ShowNewResourceWizard) ? (msg as ShowNewResourceWizard) : null;
                if(workSurfaceObject != null)
                {
                    actualResourceCategory = workSurfaceObject.ResourcePath;
                    actualResourceType = workSurfaceObject.ResourceType;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChild(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem);
            //------------Execute Test---------------------------
            resourceItem.NewResourceCommand.Execute("EmailSource");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverItem);
            Assert.AreEqual("EmailSource", actualResourceType);
            Assert.AreEqual("Path", actualResourceCategory);
        }
        //,,,,,,,
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayName_PublishesFlowNodeMessagesForItem_ExpectMessagesWithCorrectValues()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();

            Guid resourceGuid = Guid.Empty;

            bool wasResourceRemoved = false;
            aggregator.Setup(a => a.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg as RemoveResourceAndCloseTabMessage);
                if(workSurfaceObject != null)
                {
                    resourceGuid = workSurfaceObject.ResourceToRemove.ID;
                    wasResourceRemoved = true;
                }
            });

            bool wasResourceReAdded = false;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg as AddWorkSurfaceMessage);
                if(workSurfaceObject != null)
                {
                    wasResourceReAdded = true;
                }
            });
            
            EventPublishers.Aggregator = aggregator.Object;

            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(resourceItem.IsRenaming);
            var previousName = resourceItem.DisplayName;
            resourceItem.RenameCommand.Execute(null);

            resourceItem.DisplayName = "bob";
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.RenameItem(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));
            Assert.AreEqual(resourceGuid, resourceId);
            Assert.IsTrue(wasResourceReAdded);
            Assert.IsTrue(wasResourceRemoved);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayNameNonEmptyFolder_ChecksIfRenameWasSuccessful()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            EventPublishers.Aggregator = aggregator.Object;
            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            var folderItem = serverItem.Children[0];
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(folderItem.IsRenaming);
            var previousName = folderItem.DisplayName;

            folderItem.RenameCommand.Execute(null);

            folderItem.DisplayName = "bob";
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.RenameFolder(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayNameEmptyFolder_ChecksIfRenameWasSuccessful()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            EventPublishers.Aggregator = aggregator.Object;
            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            var folderItem = serverItem.Children[0];
            folderItem.Children.Clear();
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(folderItem.IsRenaming);
            var previousName = folderItem.DisplayName;

            folderItem.RenameCommand.Execute(null);

            folderItem.DisplayName = "bob";
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.RenameFolder(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDuplicateDisplayNameEmptyFolder_FailsToRename()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            EventPublishers.Aggregator = aggregator.Object;
            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            var folderItem = serverItem.Children[0];
            folderItem.Children.Clear();
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            // setup that the item already exists
            mockStudioRepository.Setup(a => a.FindItem(It.IsAny<Func<IExplorerItemModel, bool>>())).Returns(serverItem);
            //------------Execute Test---------------------------
            Assert.IsFalse(folderItem.IsRenaming);
            var previousName = folderItem.DisplayName;

            folderItem.RenameCommand.Execute(null);

            folderItem.DisplayName = "bob";
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.RenameFolder(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()),Times.Never());
            Assert.AreEqual(previousName,folderItem.DisplayName);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_UpdateCategoryIfOpened_ExpectCategory()
        {
            //------------Setup for test--------------------------
            CustomContainer.Register(new Mock<IMainViewModel>().Object);
            var aggregator = new Mock<EventAggregator>();
            EventPublishers.Aggregator = aggregator.Object;
            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            mockResourceModel.Setup(a => a.WorkflowXaml).Returns(new StringBuilder("Category=dave\\bob"));

            resourceItem.UpdateCategoryIfOpened("bobthebuilder");


            //------------Assert Results-------------------------
            mockResourceModel.VerifySet(a=>a.Category = "bobthebuilder");
            mockResourceModel.VerifySet(a => a.WorkflowXaml = It.IsAny<StringBuilder>());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_UpdateCategoryIfOpened_ExpectCategoryUpdate_NoWFXamlSet()
        {
            //------------Setup for test--------------------------
            CustomContainer.Register(new Mock<IMainViewModel>().Object);
            var aggregator = new Mock<EventAggregator>();
            EventPublishers.Aggregator = aggregator.Object;
            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
          

            resourceItem.UpdateCategoryIfOpened("bobthebuilder");


            //------------Assert Results-------------------------
            mockResourceModel.VerifySet(a => a.Category = "bobthebuilder");
            mockResourceModel.VerifySet(a => a.WorkflowXaml = It.IsAny<StringBuilder>(),Times.Never());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayName_PublishesDisplayNameMessagesForItem_ExpectMessagesWithCorrectValues()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();

            Guid resourceGuid = Guid.Empty;
            
            bool wasResourceRemoved = false;
            aggregator.Setup(a => a.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg as RemoveResourceAndCloseTabMessage);
                if(workSurfaceObject != null)
                {
                    resourceGuid = workSurfaceObject.ResourceToRemove.ID;
                    wasResourceRemoved = true;
                }
            });

            bool wasResourceReAdded = false;
            aggregator.Setup(a => a.Publish(It.IsAny<AddWorkSurfaceMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg as AddWorkSurfaceMessage);
                if(workSurfaceObject != null)
                {
                    wasResourceReAdded = true;
                }
            });


            EventPublishers.Aggregator = aggregator.Object;

            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
#pragma warning restore 168
            //------------Execute Test---------------------------
            Assert.IsFalse(resourceItem.IsRenaming);
            var previousName = resourceItem.DisplayName;
            resourceItem.RenameCommand.Execute(null);

            resourceItem.DisplayName = "bob";
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.RenameItem(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));
            Assert.AreEqual(resourceGuid, resourceId);
           Assert.IsTrue(wasResourceReAdded);
           Assert.IsTrue(wasResourceRemoved);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayName_DoesNotPublishDisplayNameMessagesWhenChildrenContainsSameName_ExpectMessagesWithCorrectValues()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();

            Guid resourceGuid = Guid.Empty;

            aggregator.Setup(a => a.Publish(It.IsAny<UpdateWorksurfaceDisplayName>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg as UpdateWorksurfaceDisplayName);
                if(workSurfaceObject != null)
                {
                    resourceGuid = workSurfaceObject.WorksurfaceResourceID;
                }
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
            ExplorerItemModel childItem = new ExplorerItemModel(mockStudioRepository.Object, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, new Mock<IConnectControlSingleton>().Object) { DisplayName = "bob" };
            resourceItem.Children.Add(childItem);
#pragma warning restore 168
            //------------Execute Test---------------------------
            Assert.IsFalse(resourceItem.IsRenaming);
            var previousName = resourceItem.DisplayName;
            resourceItem.RenameCommand.Execute(null);

            resourceItem.DisplayName = "bob";
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.RenameItem(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()), Times.Never());
            Assert.AreNotEqual(resourceGuid, resourceId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayName_UpdatesResourceXaml_ExpectXAMLSet()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();

            EventPublishers.Aggregator = aggregator.Object;

            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            mockResourceModel.Setup(a => a.WorkflowXaml).Returns(new StringBuilder(string.Format("Name=\"{0}\" x:Class=\"{1}\"ToolboxFriendlyName=\"{2}\"DisplayName=\"{3}\"", resourceItem.DisplayName, resourceItem.DisplayName, resourceItem.DisplayName, resourceItem.DisplayName)));
            mockResourceModel.Setup(a => a.ResourceName).Returns(resourceItem.DisplayName);
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(resourceItem.IsRenaming);

            resourceItem.RenameCommand.Execute(null);

            resourceItem.DisplayName = "bob";
            //------------Assert Results-------------------------

            mockStudioRepository.Verify(a => a.RenameItem(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));

            Assert.AreEqual(mockResourceModel.Object.WorkflowXaml.ToString(), @"Name=""bob"" x:Class=""bob""ToolboxFriendlyName=""bob""DisplayName=""bob""");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RenameCommand")]
        public void ExplorerItemModel_SettingDisplayName_RenameFolder_ExpectNothingSetExceptDisplayName()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();

            Guid resourceGuid = Guid.Empty;

            string oldName = "";
            string newName = "";

            aggregator.Setup(a => a.Publish(It.IsAny<UpdateWorksurfaceDisplayName>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg as UpdateWorksurfaceDisplayName);
                if(workSurfaceObject != null)
                {
                    resourceGuid = workSurfaceObject.WorksurfaceResourceID;
                    oldName = workSurfaceObject.OldName;
                    newName = workSurfaceObject.NewName;
                }
            });

            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceModel.Setup(a => a.DisplayName).Returns("bob");
            mockResourceModel.Setup(a => a.Category).Returns("dave\\bob");
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
            resourceItem.ResourceType = ResourceType.Folder;
            mockResourceModel.Setup(a => a.WorkflowXaml).Returns(new StringBuilder(string.Format("Name=\"{0}\" x:Class=\"{1}\"ToolboxFriendlyName=\"{2}\"DisplayName=\"{3}\"", resourceItem.DisplayName, resourceItem.DisplayName, resourceItem.DisplayName, resourceItem.DisplayName)));
            mockResourceModel.Setup(a => a.ResourceName).Returns(resourceItem.DisplayName);
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            //------------Execute Test---------------------------
            Assert.IsFalse(resourceItem.IsRenaming);

            resourceItem.RenameCommand.Execute(null);

            resourceItem.DisplayName = "bob";
            //------------Assert Results-------------------------

            mockStudioRepository.Verify(a => a.RenameFolder(It.IsAny<ExplorerItemModel>(), It.IsAny<string>()));
            mockResourceModel.Verify(a => a.WorkflowXaml, Times.Never());
            Assert.AreEqual("", oldName);
            Assert.AreEqual("", newName);
            Assert.AreEqual(Guid.Empty, resourceGuid);
            mockResourceRepository.Verify(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false), Times.Never());
        }

        [TestMethod]
        [Owner("Hagshen Naidu")]
        [TestCategory("ExplorerItemModel_RefreshCommand")]
        public void ExplorerItemModel_RefreshCommandExecute_NotServer_ExpectNoCallRepo()
        {
            //------------Setup for test--------------------------
            Guid resourceGuid = Guid.Empty;

            var mockStudioRepository = new Mock<IStudioResourceRepository>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            mockStudioRepository.Setup(repository => repository.Load(envID, It.IsAny<IAsyncWorker>())).Verifiable();

            const string displayName = "localhost";
            ExplorerItemModel resourceItem;
#pragma warning disable 168
            var serverItem = SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(displayName, envID, resourceId, new Mock<IConnectControlSingleton>().Object, out resourceItem, mockStudioRepository.Object);
#pragma warning restore 168
            resourceItem.ResourceType = ResourceType.Folder;
            mockResourceModel.Setup(a => a.WorkflowXaml).Returns(new StringBuilder(string.Format("Name=\"{0}\" x:Class=\"{1}\"ToolboxFriendlyName=\"{2}\"DisplayName=\"{3}\"", resourceItem.DisplayName, resourceItem.DisplayName, resourceItem.DisplayName, resourceItem.DisplayName)));
            mockResourceModel.Setup(a => a.ResourceName).Returns(resourceItem.DisplayName);
            //------------Execute Test---------------------------
            resourceItem.RefreshCommand.Execute(null);
            //------------Assert Results-------------------------
            mockStudioRepository.Verify(a => a.Load(envID, It.IsAny<IAsyncWorker>()), Times.Never());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_Permissions")]
        // ReSharper disable InconsistentNaming
        public void ExplorerItemModel_Permissions_Change_UpdateDeployAndExecute()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------

            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebService, Permissions = Permissions.Administrator };

            //------------Assert Results-------------------------
            Assert.IsTrue(explorerItemModel.CanExecute);
            Assert.IsTrue(explorerItemModel.CanDeploy);
            explorerItemModel.Permissions = Permissions.View;
            Assert.IsFalse(explorerItemModel.CanExecute);
            Assert.IsFalse(explorerItemModel.CanDeploy);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_ViewRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.View };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_NoneRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.None };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_ExecuteRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.Execute };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_DeployFromRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.DeployFrom };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_DeployToRights_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.DeployTo };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_AdministratorRights_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.Administrator };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_ContributeRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.Contribute };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployTo = explorerItemModel.IsAuthorizedDeployTo;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_ViewRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.View };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_NoneRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.None };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_ExecuteRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.Execute };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_DeployToRights_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.DeployTo };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsFalse(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_DeployFromRights_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.DeployFrom };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_AdministratorRights_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.Administrator };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_ContributeRights_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.DeployFrom };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_NotServer_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Server, Permissions = Permissions.DeployFrom };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_NotServer_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginSource, Permissions = Permissions.Contribute };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_DbService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_DbService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_WorkflowService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_WorkflowService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WorkflowService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_WebSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_WebSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_WebService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_WebService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.WebService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_ReservedService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.ReservedService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_ReservedService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.ReservedService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_DbSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_DbSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_EmailSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.EmailSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_EmailSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.EmailSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_Folder_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_Folder_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_PluginService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_PluginService_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginService };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployFrom")]
        public void ExplorerItemModel_IsAutorizedDeployFrom_PluginSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerItemModel_IsAutorizedDeployTo")]
        public void ExplorerItemModel_IsAutorizedDeployTo_PluginSource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.PluginSource };
            //------------Execute Test---------------------------
            bool isAuthorizedDeployFrom = explorerItemModel.IsAuthorizedDeployFrom;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorizedDeployFrom);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_AddNewFolder")]
        public void ExplorerItemModel_AddNewFolder_AddsUniqueName_ServerMessageReceived()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel outvalue;
            var rep = new Mock<IStudioResourceRepository>();
            bool called = false;
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), out outvalue, rep.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            rep.Setup(a => a.AddItem(It.IsAny<IExplorerItemModel>())).Callback((IExplorerItemModel a) =>
                {
                    Assert.AreEqual(a.DisplayName, "New Folder");
                    Assert.AreEqual(a.Parent, explorerItemModel);
                    Assert.AreEqual(a.Children.Count, 0);
                    Assert.AreEqual(a.ResourceType, ResourceType.Folder);
                    Assert.AreEqual(a.Permissions, explorerItemModel.Permissions);
                    explorerItemModel.Children.Add(a);
                    called = true;
                }
                );

            Assert.AreEqual(explorerItemModel.Children.Count, 1);
            explorerItemModel.AddNewFolder();
            Assert.AreEqual(explorerItemModel.Children.Count, 2);
            Assert.IsTrue(explorerItemModel.Children.Count(a => a.DisplayName == "New Folder") > 0);
            Assert.IsTrue(explorerItemModel.Children.First(a => a.DisplayName == "New Folder").IsRenaming = true);
            Assert.IsTrue(called);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_AddNewFolder")]
        public void ExplorerItemModel_AddNewFolder_AddsUniqueNameIfExists_ExpectChildren()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel outvalue;
            var rep = new Mock<IStudioResourceRepository>();
            bool called = false;
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), out outvalue, rep.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            rep.Setup(a => a.AddItem(It.IsAny<IExplorerItemModel>())).Callback((IExplorerItemModel a) =>
            {
                Assert.AreEqual(a.DisplayName, "New Folder");
                Assert.AreEqual(a.Parent, explorerItemModel);
                Assert.AreEqual(a.Children.Count, 0);
                Assert.AreEqual(a.ResourceType, ResourceType.Folder);
                Assert.AreEqual(a.Permissions, explorerItemModel.Permissions);
                explorerItemModel.Children.Add(a);
                called = true;
            }
                );

            Assert.AreEqual(explorerItemModel.Children.Count, 1);
            explorerItemModel.AddNewFolder();
            rep.Setup(a => a.AddItem(It.IsAny<ExplorerItemModel>())).Callback((IExplorerItemModel a) =>
            {
                Assert.AreEqual(a.DisplayName, "New Folder1");
                Assert.AreEqual(a.Parent, explorerItemModel);
                Assert.AreEqual(a.Children.Count, 0);
                Assert.AreEqual(a.ResourceType, ResourceType.Folder);
                Assert.AreEqual(a.Permissions, explorerItemModel.Permissions);
                explorerItemModel.Children.Add(a);

            }
      );
            explorerItemModel.AddNewFolder();
            Assert.IsTrue(called);
            Assert.AreEqual(explorerItemModel.Children.Count, 3);
            Assert.IsTrue(explorerItemModel.Children.Count(a => a.DisplayName == "New Folder") > 0);
            Assert.IsTrue(explorerItemModel.Children.First(a => a.DisplayName == "New Folder").IsRenaming = true);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_AddNewFolder")]
        public void ExplorerItemModel_AddNewFolder_AddsUniqueName_ServerMessageNotReceived()
        {
            //------------Setup for test--------------------------
            IExplorerItemModel outvalue;
            var rep = new Mock<IStudioResourceRepository>();
            bool called = false;
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), out outvalue, rep.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            rep.Setup(a => a.AddItem(It.IsAny<IExplorerItemModel>())).Callback((IExplorerItemModel a) =>
            {
                Assert.AreEqual(a.DisplayName, "New Folder");
                Assert.AreEqual(a.Parent, explorerItemModel);
                Assert.AreEqual(a.Children.Count, 0);
                Assert.AreEqual(a.ResourceType, ResourceType.Folder);
                Assert.AreEqual(a.Permissions, explorerItemModel.Permissions);

                called = true;
            }
                );

            Assert.AreEqual(explorerItemModel.Children.Count, 1);
            explorerItemModel.AddNewFolder();
            Assert.IsTrue(called);
            Assert.AreEqual(explorerItemModel.Children.Count, 1);
            Assert.IsFalse(explorerItemModel.Children.Count(a => a.DisplayName == "New Folder") > 0);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanAddNewFolder")]
        public void ExplorerItemModel_CanAddNewFolder_Folder_Expect_Success()
        {
            const ResourceType type = ResourceType.Folder;
            const bool result = true;
            AssertCanAdd(type, result);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanAddNewFolder")]
        public void ExplorerItemModel_CanAddNewFolder_NonFolder_Expect_False()
        {

            AssertCanAdd(ResourceType.DbService, false);
            AssertCanAdd(ResourceType.DbSource, false);
            AssertCanAdd(ResourceType.EmailSource, false);
            AssertCanAdd(ResourceType.PluginService, false);
            AssertCanAdd(ResourceType.PluginSource, false);
            AssertCanAdd(ResourceType.ReservedService, false);
            AssertCanAdd(ResourceType.Server, true);
            AssertCanAdd(ResourceType.ServerSource, false);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_ActivityName")]
        public void ExplorerItemModel_ActivityName_Expect_DSFActivity()
        {

            Assert.AreEqual(Item(ResourceType.Server).ActivityName, typeof(DsfActivity).AssemblyQualifiedName);
            Assert.AreEqual(Item(ResourceType.Folder).ActivityName, typeof(DsfActivity).AssemblyQualifiedName);
            Assert.AreEqual(Item(ResourceType.WorkflowService).ActivityName, typeof(DsfActivity).AssemblyQualifiedName);
            Assert.AreEqual(Item(ResourceType.ReservedService).ActivityName, typeof(DsfActivity).AssemblyQualifiedName);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_ActivityName")]
        public void ExplorerItemModel_ActivityName_Expect_NonDSFActivity()
        {

            Assert.AreEqual(Item(ResourceType.PluginService).ActivityName, typeof(DsfPluginActivity).AssemblyQualifiedName);
            Assert.AreEqual(Item(ResourceType.WebService).ActivityName, typeof(DsfWebserviceActivity).AssemblyQualifiedName);
            Assert.AreEqual(Item(ResourceType.DbService).ActivityName, typeof(DsfDatabaseActivity).AssemblyQualifiedName);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsLocalHost")]
        public void ExplorerItemModel_IsLocalHostr_ServerWithGuid_Expect_Success()
        {

            IsServer(ResourceType.Server, false, Guid.NewGuid());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanEdit")]
        public void ExplorerItemModel_CanEdit_ServerWithGuid_Expect_Success()
        {

            PermissionsTest(a => a.CanEdit, Permissions.Administrator, true);
            PermissionsTest(a => a.CanEdit, Permissions.View, true);
            PermissionsTest(a => a.CanEdit, Permissions.Contribute, true);
            PermissionsTest(a => a.CanEdit, Permissions.Execute, false);
            PermissionsTest(a => a.CanEdit, Permissions.DeployFrom, false);
            PermissionsTest(a => a.CanEdit, Permissions.DeployTo, false);
            PermissionsTest(a => a.CanConnect, Permissions.None, false);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanExecute")]
        public void ExplorerItemModel_CanExecute_ServerWithGuid_Expect_Success()
        {
            PermissionsTest(a => a.CanExecute, Permissions.Administrator, true);
            PermissionsTest(a => a.CanExecute, Permissions.View, false);
            PermissionsTest(a => a.CanExecute, Permissions.Contribute, true);
            PermissionsTest(a => a.CanExecute, Permissions.Execute, true);
            PermissionsTest(a => a.CanExecute, Permissions.DeployFrom, false);
            PermissionsTest(a => a.CanExecute, Permissions.DeployTo, false);
            PermissionsTest(a => a.CanConnect, Permissions.None, false);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanConnect")]
        public void ExplorerItemModel_CanConnect_Expect_Success()
        {

            PermissionsTest(a => a.CanConnect, Permissions.Administrator, true, ResourceType.Server);
            PermissionsTest(a => a.CanConnect, Permissions.View, true, ResourceType.Server);
            PermissionsTest(a => a.CanConnect, Permissions.Contribute, true, ResourceType.Server);
            PermissionsTest(a => a.CanConnect, Permissions.Execute, true, ResourceType.Server);
            PermissionsTest(a => a.CanConnect, Permissions.DeployFrom, true, ResourceType.Server);
            PermissionsTest(a => a.CanConnect, Permissions.DeployTo, true, ResourceType.Server);
            PermissionsTest(a => a.CanConnect, Permissions.None, false, ResourceType.Server);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanDisConnect")]
        public void ExplorerItemModel_CanDisConnect_ServerWithGuid_Expect_Success()
        {

            PermissionsTest(a => a.CanDisconnect, Permissions.Administrator, true, ResourceType.Server);
            PermissionsTest(a => a.CanDisconnect, Permissions.View, true, ResourceType.Server);
            PermissionsTest(a => a.CanDisconnect, Permissions.Contribute, true, ResourceType.Server);
            PermissionsTest(a => a.CanDisconnect, Permissions.Execute, true, ResourceType.Server);
            PermissionsTest(a => a.CanDisconnect, Permissions.DeployFrom, true, ResourceType.Server);
            PermissionsTest(a => a.CanDisconnect, Permissions.DeployTo, true, ResourceType.Server);
            PermissionsTest(a => a.CanDisconnect, Permissions.None, false, ResourceType.Server);
        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_CanViewDependencies")]
        public void ExplorerItemModel_CanViewDependencies_ServerWithGuid_Expect_Success()
        {

            PermissionsTest(a => a.CanShowDependencies, Permissions.Administrator, true);
            PermissionsTest(a => a.CanShowDependencies, Permissions.View, true);
            PermissionsTest(a => a.CanShowDependencies, Permissions.Contribute, true);
            PermissionsTest(a => a.CanShowDependencies, Permissions.Execute, true);
            PermissionsTest(a => a.CanShowDependencies, Permissions.DeployFrom, true);
            PermissionsTest(a => a.CanShowDependencies, Permissions.DeployTo, true);
            PermissionsTest(a => a.CanShowDependencies, Permissions.None, false);
        }


        public void PermissionsTest(Func<IExplorerItemModel, bool> property, Permissions permissions, bool expected, ResourceType resourceType = ResourceType.WorkflowService)
        {

            var serverItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = resourceType,
                DisplayName = "bob",
                ResourceId = Guid.Empty,
                Permissions = permissions,
                EnvironmentId = Guid.NewGuid()
            };
            Assert.AreEqual(property(serverItem), expected);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsLocalHost")]
        public void ExplorerItemModel_DeployTitle_ServerWithGuid_Expect_All()
        {

            var serverItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "bob",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };
            Assert.AreEqual("Deploy All bob", serverItem.DeployTitle);

            serverItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
           {
               ResourceType = ResourceType.Server,
               DisplayName = "bob",
               ResourceId = Guid.Empty,
               Permissions = Permissions.Administrator,
               EnvironmentId = Guid.NewGuid()
           };
            Assert.AreEqual("Deploy All bob", serverItem.DeployTitle);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsLocalHost")]
        public void ExplorerItemModel_DeployTitle_ServerWithGuid_Expect_NotAll()
        {

            var it = Item(ResourceType.DbService);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.DbSource);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.EmailSource);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.PluginService);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.PluginSource);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.ReservedService);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.Unknown);
            Assert.AreEqual("Deploy bob", it.DeployTitle);
            it = Item(ResourceType.WebService);
            Assert.AreEqual("Deploy bob", it.DeployTitle);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsAuthorised")]
        public void ExplorerItemModel_IsAuthorised_ServerNotAuthorised()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            item.ResourceType = ResourceType.Server;
            item.IsAuthorized = false;
            Assert.IsFalse(item.Children[0].IsAuthorized);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsAuthorised")]
        public void ExplorerItemModel_IsAuthorised_ServerAuthorised()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            item.ResourceType = ResourceType.Server;
            item.IsAuthorized = true;
            Assert.IsTrue(item.Children[0].IsAuthorized);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsLocalHost")]
        public void ExplorerItemModel_ActivityNames_ServerWithGuid_ExpectCorrectTypes()
        {

            IsServer(ResourceType.Server, false, Guid.NewGuid());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_IsLocalHost")]
        public void ExplorerItemModel_IsLocalHostr_NonServerResourceTypes_ExpectNotLocalHost()
        {

            IsServer(ResourceType.DbService, false, Guid.Empty);
            IsServer(ResourceType.DbSource, false, Guid.Empty);
            IsServer(ResourceType.EmailSource, false, Guid.Empty);
            IsServer(ResourceType.PluginService, false, Guid.Empty);
            IsServer(ResourceType.PluginSource, false, Guid.Empty);
            IsServer(ResourceType.ReservedService, false, Guid.Empty);
            IsServer(ResourceType.Server, true, Guid.Empty);
            IsServer(ResourceType.ServerSource, false, Guid.Empty);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_ExpectNULLSet()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            item.SetIsChecked(null, true, false);
            Assert.IsNull(item.IsChecked);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_ResourcePathFolder()
        {
            ExplorerItemModel exp;
            bool updateStats = false;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            ExplorerItemModel.OnCheckedStateChangedAction = a => { updateStats = a.UpdateStats; };
            item.SetIsChecked(null, true, false);
            Assert.IsTrue(updateStats);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_ResourcePathIsInFolder()
        {
            ExplorerItemModel exp;
            bool updateStats = false;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob\\dave", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            ExplorerItemModel.OnCheckedStateChangedAction = a =>
            {
                updateStats = a.UpdateStats;
            };
            item.Children[0].ResourceType = ResourceType.WebService;
            item.Children[0].ResourcePath = "bob\\dave\\item";
            item.Children[0].SetIsChecked(null, true, true);
            Assert.IsTrue(updateStats);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_ResourcePathInRoot()
        {
            ExplorerItemModel exp;
            bool updateStats = false;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob\\dave", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            ExplorerItemModel.OnCheckedStateChangedAction = a =>
            {
                updateStats =  a.UpdateStats;
            };
            item.Children[0].ResourceType = ResourceType.WebService;
            item.Children[0].ResourcePath = "bob";
            item.Children[0].SetIsChecked(null, true, true);
            Assert.IsTrue(updateStats);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_RemoveCommand")]
        public void ExplorerItemModel_RemoveCommand_ExpectRemoveCalled()
        {
            var rep = new Mock<IStudioResourceRepository>();
            var worker = new Mock<IAsyncWorker>();
            IExplorerItemModel outvalue;
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            connectControlSingleton.Setup(c => c.Remove(It.IsAny<Guid>())).Verifiable();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), out outvalue, rep.Object, worker.Object, connectControlSingleton.Object);

            explorerItemModel.RemoveCommand.Execute(null);
            connectControlSingleton.Verify(c => c.Remove(It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_ExpectChildrenUpdated()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            item.SetIsChecked(true, true, false);

            Assert.IsTrue(item.IsChecked != null && item.IsChecked.Value);
            var isChecked = item.Children[0].IsChecked;
            Assert.IsTrue(isChecked != null && isChecked.Value);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_ExpectParentOverwriteChanged()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            Assert.IsFalse(item.IsChecked != null && item.IsChecked.Value);
            item.IsOverwrite = true;
            item.Children[0].SetIsChecked(true, false, false);
            item.Children[0].SetIsChecked(false, false, true);
            Assert.IsFalse(item.IsChecked != null && item.IsChecked.Value);
            var isChecked = item.Children[0].IsChecked;
            Assert.IsFalse(isChecked != null && isChecked.Value);
            Assert.IsFalse(item.IsOverwrite);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_UpdateFolderPermissions_ExpectParentOverwriteChanged()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);

            Assert.IsFalse(item.IsChecked != null && item.IsChecked.Value);
            item.IsOverwrite = true;
            item.Children[0].SetIsChecked(true, false, false);
            item.Children[0].SetIsChecked(false, false, true);
            Assert.IsFalse(item.IsChecked != null && item.IsChecked.Value);
            var isChecked = item.Children[0].IsChecked;
            Assert.IsFalse(isChecked != null && isChecked.Value);
            Assert.IsFalse(item.IsOverwrite);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerItemModel_SetIsChecked")]
        public void ExplorerItemModel_SetIsChecked_CancelRename()
        {
            ExplorerItemModel exp;
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), new Mock<IConnectControlSingleton>().Object, out exp);
            item.IsRenaming = true;
            item.CancelRename();
            Assert.IsFalse(item.IsRenaming);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsRefreshing")]
        public void ExplorerItemModel_IsRefreshing_ConnectedStatusIsBusy_True()
        {
            ExplorerItemModel exp;
            var enviId = Guid.NewGuid();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", enviId, Guid.NewGuid(), connectControlSingleton.Object, out exp);
            var IsRefreshingBefore = item.IsRefreshing;
            var connectionStatusChangedEventArg = new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, enviId, true);
            connectControlSingleton.Raise(m => m.ConnectedStatusChanged += null, connectionStatusChangedEventArg);
            var IsRefreshingAfter = item.IsRefreshing;
            Assert.IsFalse(IsRefreshingBefore);
            Assert.IsTrue(IsRefreshingAfter);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsRefreshing")]
        public void ExplorerItemModel_IsRefreshing_ConnectedStatusIsConnected_False()
        {
            ExplorerItemModel exp;
            var enviId = Guid.NewGuid();
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var item = SetupExplorerItemModelWithFolderAndOneChild("bob", enviId, Guid.NewGuid(), connectControlSingleton.Object, out exp);
            var IsRefreshingBefore = item.IsRefreshing;
            var connectionStatusChangedEventArg = new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Connected, enviId, true);
            connectControlSingleton.Raise(m => m.ConnectedStatusChanged += null, connectionStatusChangedEventArg);
            var IsRefreshingAfter = item.IsRefreshing;
            Assert.IsFalse(IsRefreshingBefore);
            Assert.IsFalse(IsRefreshingAfter);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsUnknownAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.Unknown, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsWorkflowServiceAndPermissionIsView_True()
        {
            CanShowVersionHistoryTestExecution(ResourceType.WorkflowService, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsDbServiceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.DbService, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsVersionAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.Version, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsPluginServiceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.PluginService, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsWebServiceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.WebService, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsDbSourceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.DbSource, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsWebSourceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.WebSource, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsEmailSourceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.EmailSource, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsServerSourceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.ServerSource, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsFolderAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.Folder, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsServerAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.Server, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsReservedServiceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.ReservedService, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsMessageAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.Message, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowHistory")]
        public void ExplorerItemModel_CanShowHistory_ResourceTypeIsPluginSourceAndPermissionIsView_False()
        {
            CanShowVersionHistoryTestExecution(ResourceType.PluginSource, Permissions.View, false);
        }

        static void CanShowVersionHistoryTestExecution(ResourceType resourceType, Permissions permission, bool expectedResult)
        {
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
                {
                    ResourceType = resourceType,
                    Permissions = permission,
                    DisplayName = "My Resource"
                };
            Assert.AreEqual(expectedResult, explorerItemModel.CanShowHistory);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsUnknownAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.Unknown, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsWorkflowServiceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.WorkflowService, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsDbServiceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.DbService, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsVersionAndPermissionIsView_False()
        {
            CanShowDependenciesTestExecution(ResourceType.Version, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsPluginServiceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.PluginService, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsWebServiceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.WebService, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsDbSourceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.DbSource, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsWebSourceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.WebSource, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsEmailSourceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.EmailSource, Permissions.View, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsServerSourceAndPermissionIsView_False()
        {
            CanShowDependenciesTestExecution(ResourceType.ServerSource, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsFolderAndPermissionIsView_False()
        {
            CanShowDependenciesTestExecution(ResourceType.Folder, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsServerAndPermissionIsView_False()
        {
            CanShowDependenciesTestExecution(ResourceType.Server, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsReservedServiceAndPermissionIsView_False()
        {
            CanShowDependenciesTestExecution(ResourceType.ReservedService, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsMessageAndPermissionIsView_False()
        {
            CanShowDependenciesTestExecution(ResourceType.Message, Permissions.View, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_CanShowDependencies")]
        public void ExplorerItemModel_CanShowDependencies_ResourceTypeIsPluginSourceAndPermissionIsView_True()
        {
            CanShowDependenciesTestExecution(ResourceType.PluginSource, Permissions.View, true);
        }

        static void CanShowDependenciesTestExecution(ResourceType resourceType, Permissions permission, bool expectedResult)
        {
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = resourceType,
                Permissions = permission,
                DisplayName = "My Resource"
            };
            Assert.AreEqual(expectedResult, explorerItemModel.CanShowDependencies);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsAuthorized")]
        public void ExplorerItemModel_IsAuthorized_ResourceTypeIsServerPermissionIsView_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
                {
                    ResourceType = ResourceType.Server,
                    Permissions = Permissions.View
                };
            //------------Execute Test---------------------------
            bool isAuthorized = explorerItemModel.IsAuthorized;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsAuthorized")]
        public void ExplorerItemModel_IsAuthorized_ResourceTypeIsVersionPermissionIsView_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Version,
                Permissions = Permissions.View
            };
            //------------Execute Test---------------------------
            bool isAuthorized = explorerItemModel.IsAuthorized;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsAuthorized")]
        public void ExplorerItemModel_IsAuthorized_ResourceTypeIsMessagePermissionIsView_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Message,
                Permissions = Permissions.View
            };
            //------------Execute Test---------------------------
            bool isAuthorized = explorerItemModel.IsAuthorized;
            //------------Assert Results-------------------------
            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_ToggleVersionHistoryCommand")]
        public void ExplorerItemModel_ToggleVersionHistoryCommand_ExecuteExplorerItemHasNoChildren_ShowVersionHistoryIsCalled()
        {
            //------------Setup for test--------------------------
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.ShowVersionHistory(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Verifiable();
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, studioResourceRepository.Object)
            {
                ResourceType = ResourceType.Message,
                Permissions = Permissions.View
            };
            //------------Execute Test---------------------------
            explorerItemModel.ToggleVersionHistoryCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Hide Version History", explorerItemModel.ToggleVersionHistoryHeader);
            studioResourceRepository.Verify(s => s.ShowVersionHistory(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_ToggleVersionHistoryCommand")]
        public void ExplorerItemModel_ToggleVersionHistoryCommand_ExecuteExplorerItemHasChildren_HideVersionHistoryIsCalled()
        {
            //------------Setup for test--------------------------
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.HideVersionHistory(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Verifiable();
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, studioResourceRepository.Object)
            {
                ResourceType = ResourceType.Message,
                Permissions = Permissions.View
            };
            explorerItemModel.Children.Add(new Mock<IExplorerItemModel>().Object);
            explorerItemModel.Children.Add(new Mock<IExplorerItemModel>().Object);
            //------------Execute Test---------------------------
            explorerItemModel.ToggleVersionHistoryCommand.Execute(null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Show Version History", explorerItemModel.ToggleVersionHistoryHeader);
            studioResourceRepository.Verify(s => s.HideVersionHistory(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_RollbackCommand")]
        public void ExplorerItemModel_RollbackCommand_ResourceFoundOnTheRepository_CallsRollbackOnStudioRepository()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is RemoveResourceAndCloseTabMessage) ? (msg as RemoveResourceAndCloseTabMessage).ResourceToRemove : null;
                actualResourceInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();
            var envID = Guid.Empty;

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(envID);
            GetEnvironmentRepository(mockEnvironment);

            IExplorerItemModel outvalue;
            var studioResourceRepo = new Mock<IStudioResourceRepository>();
            studioResourceRepo.Setup(s => s.RollbackTo(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()))
                .Verifiable();
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.Empty, Guid.NewGuid(), out outvalue, studioResourceRepo.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            var parent = new Mock<IExplorerItemModel>();
            parent.Setup(m => m.DisplayName).Returns("Some name");
            explorerItemModel.VersionInfo = new VersionInfo { VersionNumber = "1" };
            explorerItemModel.Parent = parent.Object;
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.ShowRollbackVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            var mainViewModel = new Mock<IMainViewModel>();
            mainViewModel.Setup(p => p.IsWorkFlowOpened(It.IsAny<IContextualResourceModel>())).Returns(true);
            CustomContainer.Register(mainViewModel.Object);
            //------------Execute Test---------------------------
            explorerItemModel.RollbackCommand.Execute(null);
            //------------Assert Result--------------------------
            Assert.IsNotNull(actualResourceInvoked);
            studioResourceRepo.Verify(s => s.RollbackTo(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_RollbackCommand")]
        public void ExplorerItemModel_RollbackCommand_ResourceNotFoundOnTheRepository_DoesNotCallRollbackOnStudioRepository()
        {
            //------------Setup for test--------------------------
            var aggregator = new Mock<EventAggregator>();
            IContextualResourceModel actualResourceInvoked = null;
            aggregator.Setup(a => a.Publish(It.IsAny<RemoveResourceAndCloseTabMessage>())).Callback<object>(msg =>
            {
                var workSurfaceObject = (msg is RemoveResourceAndCloseTabMessage) ? (msg as RemoveResourceAndCloseTabMessage).ResourceToRemove : null;
                actualResourceInvoked = workSurfaceObject;
            });

            EventPublishers.Aggregator = aggregator.Object;

            var mockResourceRepository = new Mock<IResourceRepository>();
            var resourceId = Guid.NewGuid();

            var mockResourceModel = new Mock<IContextualResourceModel>();
            mockResourceModel.Setup(model => model.ID).Returns(resourceId);
            mockResourceRepository.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>(), false)).Returns(mockResourceModel.Object);
            Mock<IEnvironmentModel> mockEnvironment = EnviromentRepositoryTest.CreateMockEnvironment(mockResourceRepository.Object, "localhost");
            mockEnvironment.Setup(model => model.ID).Returns(Guid.NewGuid);
            GetEnvironmentRepository(mockEnvironment);

            IExplorerItemModel outvalue;
            var studioResourceRepo = new Mock<IStudioResourceRepository>();
            studioResourceRepo.Setup(s => s.RollbackTo(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()))
                .Verifiable();
            var worker = new Mock<IAsyncWorker>();
            var explorerItemModel = SetupExplorerItemModelWithFolderAndOneChild("bob", Guid.NewGuid(), Guid.NewGuid(), out outvalue, studioResourceRepo.Object, worker.Object, new Mock<IConnectControlSingleton>().Object);
            var parent = new Mock<IExplorerItemModel>();
            parent.Setup(m => m.DisplayName).Returns("Some name");
            explorerItemModel.VersionInfo = new VersionInfo { VersionNumber = "1" };
            explorerItemModel.Parent = parent.Object;
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.ShowDeleteVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            //------------Execute Test---------------------------
            explorerItemModel.RollbackCommand.Execute(null);
            //------------Assert Result--------------------------
            Assert.IsNull(actualResourceInvoked);
            studioResourceRepo.Verify(s => s.RollbackTo(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_DeleteVersionCommand")]
        public void ExplorerItemModel_DeleteVersionCommand_CallsDeleteVersionOnStudioRepository()
        {
            //------------Setup for test--------------------------
            var studioResourceRepository = new Mock<IStudioResourceRepository>();
            studioResourceRepository.Setup(s => s.DeleteVersion(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()))
                .Verifiable();
            var explorerItemModel = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, studioResourceRepository.Object)
            {
                ResourceType = ResourceType.Message,
                Permissions = Permissions.View
            };
            explorerItemModel.Children.Add(new Mock<IExplorerItemModel>().Object);
            explorerItemModel.Children.Add(new Mock<IExplorerItemModel>().Object);
            var parent = new Mock<IExplorerItemModel>();
            parent.Setup(m => m.DisplayName).Returns("Some name");
            explorerItemModel.VersionInfo = new VersionInfo { VersionNumber = "1" };
            explorerItemModel.Parent = parent.Object;
            var popupController = new Mock<IPopupController>();
            popupController.Setup(p => p.ShowDeleteVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);
            CustomContainer.Register(popupController.Object);
            //------------Execute Test---------------------------
            explorerItemModel.DeleteVersionCommand.Execute(null);
            //------------Assert Results-------------------------
            studioResourceRepository.Verify(s => s.DeleteVersion(It.IsAny<IVersionInfo>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_SetDisplay")]
        public void ExplorerItemModel_SetDisplay_ChangesDisplayName()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };
            //------------Execute Test---------------------------
            explorerItem.SetDisplay("S222");
            //------------Assert Results-------------------------
            Assert.AreEqual("S222", explorerItem.DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsResourcePickerExpanded")]
        public void ExplorerItemModel_IsResourcePickerExpanded_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
                {
                    actualName = e.PropertyName;
                };

            //------------Execute Test---------------------------
            explorerItem.IsResourcePickerExpanded = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsResourcePickerExpanded", actualName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsExplorerExpanded")]
        public void ExplorerItemModel_IsExplorerExpanded_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsExplorerExpanded = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsExplorerExpanded", actualName);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsDeploySourceExpanded")]
        public void ExplorerItemModel_IsDeploySourceExpanded_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsDeploySourceExpanded = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsDeploySourceExpanded", actualName);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsDeployTargetExpanded")]
        public void ExplorerItemModel_IsDeployTargetExpanded_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsDeployTargetExpanded = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsDeployTargetExpanded", actualName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsExplorerSelected")]
        public void ExplorerItemModel_IsExplorerSelected_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsExplorerSelected = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsExplorerSelected", actualName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsResourcePickerSelected")]
        public void ExplorerItemModel_IsResourcePickerSelected_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsResourcePickerSelected = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsResourcePickerSelected", actualName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsDeploySourceSelected")]
        public void ExplorerItemModel_IsDeploySourceSelected_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsDeploySourceSelected = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsDeploySourceSelected", actualName);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsDeployTargetSelected")]
        public void ExplorerItemModel_IsDeployTargetSelected_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsDeployTargetSelected = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsDeployTargetSelected", actualName);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsRenaming")]
        public void ExplorerItemModel_IsRenaming_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsRenaming = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsRenaming", actualName);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsRefreshing")]
        public void ExplorerItemModel_IsRefreshing_Set_RaisesPropertyChanged()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = Guid.NewGuid()
            };

            var actualName = "";

            explorerItem.PropertyChanged += (s, e) =>
            {
                actualName = e.PropertyName;
            };

            //------------Execute Test---------------------------
            explorerItem.IsRefreshing = false;
            //------------Assert Results-------------------------
            Assert.AreEqual("IsRefreshing", actualName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsVersion")]
        public void ExplorerItemModel_IsVersion_GetResourceTypeIsVersion_True()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Version,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.View,
                EnvironmentId = Guid.NewGuid()
            };
            //------------Execute Test---------------------------
            var isVersion = explorerItem.IsVersion;
            //------------Assert Results-------------------------
            Assert.IsTrue(isVersion);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ExplorerItemModel_IsVersion")]
        public void ExplorerItemModel_IsVersion_GetResourceTypeIsNotVersion_False()
        {
            //------------Setup for test--------------------------
            var explorerItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.WorkflowService,
                DisplayName = "S111",
                ResourceId = Guid.Empty,
                Permissions = Permissions.View,
                EnvironmentId = Guid.NewGuid()
            };
            //------------Execute Test---------------------------
            var isVersion = explorerItem.IsVersion;
            //------------Assert Results-------------------------
            Assert.IsFalse(isVersion);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CheckStateChangedArgs_Ctor")]
        public void CheckStateChangedArgs_Ctor_CheckAllFieldsAreSet_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var guid = Guid.NewGuid();
            var checkStateChangedArgs = new CheckStateChangedArgs(true,false,guid,ResourceType.Message,false);
            Assert.IsTrue(checkStateChangedArgs.PreviousState);
            Assert.IsFalse(checkStateChangedArgs.NewState);
            Assert.AreEqual(checkStateChangedArgs.ResourceId,guid);
            Assert.AreEqual(ResourceType.Message,checkStateChangedArgs.ResourceType);
            Assert.IsFalse(checkStateChangedArgs.UpdateStats);

        }

        static void IsServer(ResourceType type, bool result, Guid id)
        {
            var serverItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = type,
                DisplayName = "bob",
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = id
            };
            Assert.AreEqual(result, serverItem.IsLocalHost);
        }

        static ExplorerItemModel Item(ResourceType type)
        {
            return new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
             {
                 ResourceType = type,
                 DisplayName = "bob",
                 ResourceId = Guid.Empty,
                 Permissions = Permissions.Administrator,
                 EnvironmentId = Guid.NewGuid()
             };

        }

        static void AssertCanAdd(ResourceType type, bool result)
        {
            var serverItem = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object)
                {
                    ResourceType = type,
                    DisplayName = "bob",
                    ResourceId = Guid.Empty,
                    Permissions = Permissions.Administrator,
                    EnvironmentId = Guid.NewGuid()
                };
            Assert.AreEqual(result, serverItem.CanCreateNewFolder);
        }

        static ExplorerItemModel SetupExplorerItemModelWithFolderAndOneChild(string displayName, Guid envID, Guid resourceId, IConnectControlSingleton connectControlSingleton, out ExplorerItemModel resourceItem)
        {
            var serverItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Server,
                DisplayName = displayName,
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };
            var folderItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID,
                Parent = serverItem
            };

            resourceItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.WorkflowService,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = resourceId,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID,
                ResourcePath = "Path",
                IsExplorerSelected = true,
                Parent = folderItem
            };
            folderItem.Children.Add(resourceItem);
            serverItem.Children.Add(folderItem);
            return serverItem;
        }

        static ExplorerItemModel SetupExplorerItemModelWithFolderNoChild(string displayName, Guid envID, IConnectControlSingleton connectControlSingleton, out ExplorerItemModel resourceItem)
        {
            var serverItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Server,
                DisplayName = displayName,
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };
            var folderItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID,
                Parent = serverItem
            };

            resourceItem = folderItem;
            serverItem.Children.Add(folderItem);
            return serverItem;
        }

        static IExplorerItemModel SetupExplorerItemModelWithFolderAndOneChild(string displayName, Guid envID, Guid resourceId, out IExplorerItemModel resourceItem, IStudioResourceRepository rep, IAsyncWorker worker, IConnectControlSingleton connectControlSingleton)
        {
            var serverItem = new ExplorerItemModel(rep, worker, connectControlSingleton)
            {
                ResourceType = ResourceType.Server,
                DisplayName = displayName,
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };
            var folderItem = new ExplorerItemModel(rep, worker, connectControlSingleton)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };

            resourceItem = new ExplorerItemModel(rep, worker, connectControlSingleton)
            {
                ResourceType = ResourceType.WorkflowService,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = resourceId,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID,
                ResourcePath = "Path",
                IsExplorerSelected = true,
                Parent = folderItem
            };
            folderItem.Children.Add(resourceItem);
            serverItem.Children.Add(folderItem);
            return serverItem;
        }

        static ExplorerItemModel SetupExplorerItemModelWithFolderAndFolderChildAndResources(string displayName, Guid envID, Guid resourceId, IConnectControlSingleton connectControlSingleton, out ExplorerItemModel resourceItem)
        {
            var serverItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Server,
                DisplayName = displayName,
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };
            var folderItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };

            var folderItem2 = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };

            resourceItem = new ExplorerItemModel(connectControlSingleton, new Mock<IStudioResourceRepository>().Object)
            {
                ResourceType = ResourceType.WorkflowService,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = resourceId,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID,
                ResourcePath = "Path",
                IsExplorerSelected = true,
                Parent = folderItem
            };
            folderItem.Children.Add(resourceItem);
            folderItem2.Children.Add(folderItem);
            serverItem.Children.Add(folderItem2);
            return serverItem;
        }

        private static void GetEnvironmentRepository(Mock<IEnvironmentModel> mockEnvironment)
        {
            var repo = new TestLoadEnvironmentRespository(mockEnvironment.Object) { IsLoaded = true };
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(repo);
            // ReSharper restore ObjectCreationAsStatement
        }

        static ExplorerItemModel SetupExplorerItemModelWithFolderAndOneChildMockedStudioRepository(string displayName, Guid envID, Guid resourceId, IConnectControlSingleton connectControlSingleton, out ExplorerItemModel resourceItem, IStudioResourceRepository repo)
        {
            var serverItem = new ExplorerItemModel(repo, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, connectControlSingleton)
            {
                ResourceType = ResourceType.Server,
                DisplayName = displayName,
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };
            var folderItem = new ExplorerItemModel(repo, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, connectControlSingleton)
            {
                ResourceType = ResourceType.Folder,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = Guid.Empty,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID
            };

            resourceItem = new ExplorerItemModel(repo, AsyncWorkerTests.CreateSynchronousAsyncWorker().Object, connectControlSingleton)
            {
                ResourceType = ResourceType.WorkflowService,
                DisplayName = Guid.NewGuid().ToString(),
                ResourceId = resourceId,
                Permissions = Permissions.Administrator,
                EnvironmentId = envID,
                IsExplorerSelected = true,
                Parent = folderItem,
                ResourcePath = folderItem.DisplayName
            };
            folderItem.Children.Add(resourceItem);
            serverItem.Children.Add(folderItem);
            return serverItem;
        }

    }
}