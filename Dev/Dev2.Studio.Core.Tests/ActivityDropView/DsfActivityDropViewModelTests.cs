
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq.Expressions;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Models;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Core.Tests.ActivityDropView
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfActivityDropViewModelTests
    {

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsWorkflowServiceAndFilterIsWorkflow_True()
        {
            RunCanOkTestCases(ResourceType.WorkflowService, enDsfActivityType.Workflow, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsWorkflowServiceAndFilterIsService_False()
        {
            RunCanOkTestCases(ResourceType.WorkflowService, enDsfActivityType.Service, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsDbServiceAndFilterIsService_True()
        {
            RunCanOkTestCases(ResourceType.DbService, enDsfActivityType.Service, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsPluginServiceAndFilterIsService_True()
        {
            RunCanOkTestCases(ResourceType.PluginService, enDsfActivityType.Service, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsWorkflowServiceAndFilterIsService_True()
        {
            RunCanOkTestCases(ResourceType.WorkflowService, enDsfActivityType.Service, false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsAnyAndFilterIsAll_True()
        {
            RunCanOkTestCases(ResourceType.Folder, enDsfActivityType.All, false);
            RunCanOkTestCases(ResourceType.DbService, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.DbSource, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.EmailSource, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.PluginService, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.PluginSource, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.ReservedService, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.Server, enDsfActivityType.All, false);
            RunCanOkTestCases(ResourceType.ServerSource, enDsfActivityType.All, false);
            RunCanOkTestCases(ResourceType.Unknown, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.WebService, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.WebSource, enDsfActivityType.All, true);
            RunCanOkTestCases(ResourceType.WorkflowService, enDsfActivityType.All, true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOkay_SelectedItemResourceTypeIsAnyAndFilterIsSource_CanOkSetSetAppropriately()
        {
            RunCanOkTestCases(ResourceType.Folder, enDsfActivityType.Source, false);
            RunCanOkTestCases(ResourceType.DbService, enDsfActivityType.Source, false);
            RunCanOkTestCases(ResourceType.DbSource, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.EmailSource, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.PluginService, enDsfActivityType.Source, false);
            RunCanOkTestCases(ResourceType.PluginSource, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.ReservedService, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.Server, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.ServerSource, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.Unknown, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.WebService, enDsfActivityType.Source, false);
            RunCanOkTestCases(ResourceType.WebSource, enDsfActivityType.Source, true);
            RunCanOkTestCases(ResourceType.WorkflowService, enDsfActivityType.Source, false);
        }

        static void RunCanOkTestCases(ResourceType resourceType, enDsfActivityType ty, bool expected)
        {
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns(new ExplorerItemModel
                {
                    ResourceType = resourceType
                });
            var vm = new DsfActivityDropViewModel(navigationVM.Object, ty);
            Assert.AreEqual(expected, vm.CanOkay);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_CanOk")]
        public void DsfActivityDropViewModel_CanOk_SelectedItemIsNull_False()
        {
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns((ExplorerItemModel)null);
            var vm = new DsfActivityDropViewModel(navigationVM.Object, enDsfActivityType.All);
            Assert.AreEqual(false, vm.CanOkay);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_Okay")]
        public void DsfActivityDropViewModel_Okay_NavigationViewModelSelectedItemIsNull_RequestClosedIsNotCalled()
        {
            //------------Setup for test--------------------------
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns((ExplorerItemModel)null);
            var vm = new DsfActivityDropViewModel(navigationVM.Object, enDsfActivityType.All);
            //------------Execute Test---------------------------
            vm.Okay();
            //------------Assert Results-------------------------
            Assert.IsFalse(vm.CloseRequested);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_Okay")]
        public void DsfActivityDropViewModel_Okay_NavigationViewModelSelectedItemIsNull_SelectedResourceModelIsNotSet()
        {
            //------------Setup for test--------------------------
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns((ExplorerItemModel)null);
            var vm = new DsfActivityDropViewModel(navigationVM.Object, enDsfActivityType.All);
            //------------Execute Test---------------------------
            vm.Okay();
            //------------Assert Results-------------------------
            Assert.IsNull(vm.SelectedResourceModel);
            Assert.IsFalse(vm.CloseRequested);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_Okay")]
        public void DsfActivityDropViewModel_Okay_EnvironmentNotFoundOnTheEnvironmentRepository_SelectedResourceModelIsSet()
        {
            //------------Setup for test--------------------------
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns(new ExplorerItemModel
            {
                ResourceType = ResourceType.WorkflowService,
                ResourceId = Guid.NewGuid(),
                EnvironmentId = Guid.NewGuid()
            });

            var vm = new DsfActivityDropViewModel(navigationVM.Object, enDsfActivityType.All)
                {
                    GetEnvironmentRepository = () =>
                        {
                            var env = new Mock<IEnvironmentRepository>();
                            env.Setup(m => m.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()))
                               .Returns((IEnvironmentModel)null);
                            return env.Object;
                        }
                };
            //------------Execute Test---------------------------
            vm.Okay();
            //------------Assert Results-------------------------
            Assert.IsNull(vm.SelectedResourceModel);
            Assert.IsFalse(vm.CloseRequested);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_Okay")]
        public void DsfActivityDropViewModel_Okay_ResourceIsNotFoundOnTheResourceRepository_SelectedResourceModelIsSet()
        {
            //------------Setup for test--------------------------
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns(new ExplorerItemModel
            {
                ResourceType = ResourceType.WorkflowService,
                ResourceId = Guid.NewGuid(),
                EnvironmentId = Guid.NewGuid()
            });

            var vm = new DsfActivityDropViewModel(navigationVM.Object, enDsfActivityType.All);
            var environmentModel = new Mock<IEnvironmentModel>();

            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(m => m.FindSingleWithPayLoad(It.IsAny<Expression<Func<IResourceModel, bool>>>()))
                        .Returns((IResourceModel)null);
            environmentModel.Setup(m => m.ResourceRepository).Returns(resourceRepo.Object);

            vm.GetEnvironmentRepository = () =>
            {
                var env = new Mock<IEnvironmentRepository>();
                env.Setup(m => m.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()))
                                .Returns(environmentModel.Object);
                return env.Object;
            };
            //------------Execute Test---------------------------
            vm.Okay();
            //------------Assert Results-------------------------
            Assert.IsNull(vm.SelectedResourceModel);
            Assert.IsFalse(vm.CloseRequested);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfActivityDropViewModel_Okay")]
        public void DsfActivityDropViewModel_Okay_NavigationViewModelSelectedItemIsValid_SelectedResourceModelIsSet()
        {
            //------------Setup for test--------------------------
            var navigationVM = new Mock<INavigationViewModel>();
            navigationVM.Setup(m => m.SelectedItem).Returns(new ExplorerItemModel
            {
                ResourceType = ResourceType.WorkflowService,
                ResourceId = Guid.NewGuid(),
                EnvironmentId = Guid.NewGuid()
            });
            var vm = new DsfActivityDropViewModel(navigationVM.Object, enDsfActivityType.All);
            var environmentModel = new Mock<IEnvironmentModel>();
            var resourceModel = new Mock<IContextualResourceModel>();
            var resourceRepo = new Mock<IResourceRepository>();
            resourceRepo.Setup(m => m.FindSingleWithPayLoad(It.IsAny<Expression<Func<IResourceModel, bool>>>()))
                        .Returns(resourceModel.Object);
            environmentModel.Setup(m => m.ResourceRepository).Returns(resourceRepo.Object);
            vm.GetEnvironmentRepository = () =>
            {
                var env = new Mock<IEnvironmentRepository>();
                env.Setup(m => m.FindSingle(It.IsAny<Expression<Func<IEnvironmentModel, bool>>>()))
                                .Returns(environmentModel.Object);
                return env.Object;
            };
            //------------Execute Test---------------------------
            vm.Okay();
            //------------Assert Results-------------------------
            Assert.IsNotNull(vm.SelectedResourceModel);
            Assert.IsTrue(vm.CloseRequested);
        }
    }
}
