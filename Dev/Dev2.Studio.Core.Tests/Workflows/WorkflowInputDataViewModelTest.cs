/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Services.Security;

namespace Dev2.Core.Tests.Workflows
{
    [TestClass]
    public class WorkflowInputDataViewModelTests
    {
        readonly Guid _resourceID = Guid.Parse("2b975c6d-670e-49bb-ac4d-fb1ce578f66a");
        readonly Guid _serverID = Guid.Parse("51a58300-7e9d-4927-a57b-e5d700b11b55");
        const string ResourceName = "TestWorkflow";

        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_LoadInputs_Expected_Inputs_Loaded()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                IList<IDataListItem> testDataListItems = GetInputTestDataDataNames();
                for (int i = 1; i < workflowInputDataviewModel.WorkflowInputs.Count; i++)
                {
                    Assert.AreEqual(testDataListItems[i].DisplayValue, workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                    Assert.AreEqual(testDataListItems[i].Value, workflowInputDataviewModel.WorkflowInputs[i].Value);
                }
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_LoadInputsExpectedOnlyInputsLoaded()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                IList<IDataListItem> testDataListItems = GetInputTestDataDataNames();
                for (int i = 1; i < workflowInputDataviewModel.WorkflowInputs.Count; i++)
                {
                    Assert.AreEqual(testDataListItems[i].DisplayValue, workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                    Assert.AreEqual(testDataListItems[i].Value, workflowInputDataviewModel.WorkflowInputs[i].Value);
                }
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_LoadInputs_BlankXMLData_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns("<DataList></DataList>");
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                foreach (var input in workflowInputDataviewModel.WorkflowInputs)
                {
                    Assert.AreEqual(string.Empty, input.Value);
                }
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_LoadInputs_BlankDataList_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns("<DataList></DataList>");
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                Assert.IsTrue(workflowInputDataviewModel.WorkflowInputs.Count == 0);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_Save_EmptyDataList_Expected_NoErrors()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);

            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                workflowInputDataviewModel.OkCommand.Execute(null);
                Assert.AreEqual("", workflowInputDataviewModel.DebugTo.Error);
                popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Never);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_OkCommand_IsInError_Expected_ErrorShown()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);

            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.IsInError = true;
                workflowInputDataviewModel.OkCommand.Execute(null);
                popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_ViewInBrowserCommand_IsInError_Expected_ErrorNotShown()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);

            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.IsInError = true;
                workflowInputDataviewModel.ViewInBrowserCommand.Execute(null);
                popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
            }
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_WithoutActionTrackingViewInBrowser_Expected_ErrorNotShown()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);

            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                workflowInputDataviewModel.WithoutActionTrackingViewInBrowser();
                Assert.AreEqual("", workflowInputDataviewModel.DebugTo.Error);
                popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Never);
            }
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_WithoutActionTrackingViewInBrowser_Expected_ErrorShown()
        {
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false));
            CustomContainer.Register(popupController.Object);

            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.IsInError = true;
                workflowInputDataviewModel.WithoutActionTrackingViewInBrowser();
                popupController.Verify(controller => controller.Show(StringResources.DataInput_Error, StringResources.DataInput_Error_Title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, string.Empty, false, true, false, false, false, false), Times.Once);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_Close_Updates_WorkflowLink()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                var workSurfaceContextViewModelMock = new Mock<IWorkSurfaceContextViewModel>();
                var wdMock = new Mock<IWorkflowDesignerViewModel>();
                var wdMock_asWorkSurfaceViewModelMock = wdMock.As<IWorkSurfaceViewModel>();
                workSurfaceContextViewModelMock.Setup(o => o.WorkSurfaceViewModel).Returns(wdMock_asWorkSurfaceViewModelMock.Object);
                wdMock.Setup(o => o.GetAndUpdateWorkflowLinkWithWorkspaceID()).Returns("").Verifiable();
                workSurfaceContextViewModelMock.Setup(o => o.Parent).Returns(wdMock.Object).Verifiable();
                workflowInputDataViewModel.Parent = workSurfaceContextViewModelMock.Object;

                workflowInputDataViewModel.ViewClosed();

                wdMock.Verify();
                wdMock_asWorkSurfaceViewModelMock.Verify();
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_Cancel_NullDataList_Expected_NoErrors()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                workflowInputDataviewModel.CancelCommand.Execute(null);
                Assert.AreEqual("", workflowInputDataviewModel.DebugTo.Error);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_LoadInputs_NullDataList_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                Assert.IsTrue(workflowInputDataviewModel.WorkflowInputs.Count == 0);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkflowInputDataViewModel_Create_NullResourceModel_ThrowException()
        {
            using (WorkflowInputDataViewModel.Create(null))
            {
                //Expected Exception
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_Create_WithResourceModel_IsValid()
        {
            //------------Setup for test--------------------------
            var mockResouce = GetMockResource();
            //------------Execute Test---------------------------
            using (var viewModel = WorkflowInputDataViewModel.Create(mockResouce.Object))
            {
                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel);
                Assert.IsNotNull(viewModel.DebugTo);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_Create_WithResourceModel_AuthorizationService_NotNull()
        {
            //------------Setup for test--------------------------
            var mockAuth = new Mock<IAuthorizationService>();

            var mockServer = new Mock<IServer>();
            mockServer.Setup(server => server.AuthorizationService).Returns(mockAuth.Object);

            var mockResouce = GetMockResource();
            mockResouce.Setup(resource => resource.Environment).Returns(mockServer.Object);
            //------------Execute Test---------------------------
            using (var viewModel = WorkflowInputDataViewModel.Create(mockResouce.Object))
            {
                //------------Assert Results-------------------------
                Assert.IsNotNull(viewModel);
                Assert.IsNotNull(viewModel.DebugTo);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_ExtraRows_Expected_Row_Available()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel().SessionID))
            {
                workflowInputDataviewModel.LoadWorkflowInputs();
                var inputValues = GetInputTestDataDataNames();

                // Cannot perform Collection Assert due to use of mocks for datalist items to remove dependancies during test
                for (int i = 0; i < workflowInputDataviewModel.WorkflowInputs.Count; i++)
                {
                    Assert.AreEqual(inputValues[i].DisplayValue, workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                    Assert.AreEqual(inputValues[i].Value, workflowInputDataviewModel.WorkflowInputs[i].Value);
                    Assert.AreEqual(inputValues[i].CanHaveMutipleRows, workflowInputDataviewModel.WorkflowInputs[i].CanHaveMutipleRows);
                    Assert.AreEqual(inputValues[i].Index, workflowInputDataviewModel.WorkflowInputs[i].Index);
                    Assert.AreEqual(inputValues[i].Field, workflowInputDataviewModel.WorkflowInputs[i].Field);
                }
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_AddRow_WhenNotAllColumnsInput_ExpectNewRowWithOnlyInputColumns()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            var itemToAdd = new DataListItem { DisplayValue = "rec(1).a", Field = "a", Recordset = "rec", CanHaveMutipleRows = true, Index = "1", RecordsetIndexType = enRecordsetIndexType.Numeric, Value = "1" };

            //------------Execute Test---------------------------
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)
                workflowInputDataViewModel.AddRow(itemToAdd);


                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(2, inputs.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_AddBlankRow_WhenNotAllColumnsInput_ExpectNewRowWithOnlyInputColumns()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            //------------Execute Test---------------------------
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);


                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(3, inputs.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_RemoveRow_WhenNotAllColumnsInput_ExpectRowRemoved()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);

                //------------Execute Test---------------------------
                workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect);


                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(3, inputs.Count);
                var count = workflowInputDataViewModel.WorkflowInputCount;
                Assert.AreEqual(3, count);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_RemoveRow_WhenNotAllColumnsInput_ExpectRowRemoved2()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList>
<rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" >
<a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
<a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
<b Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
</rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(2, inputs.Count);
                inputs[0].Value = "";
                inputs[0].CanHaveMutipleRows = false;
                inputs[0].Index = "2";
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);

                //------------Execute Test---------------------------
                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(null, out indexToSelect));

                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect));

                inputs[0].CanHaveMutipleRows = true;
                Assert.IsTrue(workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect));



                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                var count = workflowInputDataViewModel.WorkflowInputCount;
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_RemoveRow_WhenNotAllColumnsInput_ExpectRowRemoved4()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList>
<rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" >
<a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
<b Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
</rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "";
                inputs[0].CanHaveMutipleRows = false;
                inputs[0].Index = "2";
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);

                //------------Execute Test---------------------------
                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(null, out indexToSelect));

                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect));

                inputs[0].CanHaveMutipleRows = true;
                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect));



                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                var count = workflowInputDataViewModel.WorkflowInputCount;
                Assert.AreEqual(1, count);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_RemoveRow_WhenNotAllColumnsInput_ExpectRowRemoved3()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList>
<rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" >
<a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
<a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
<a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
<b Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
</rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(3, inputs.Count);
                inputs[0].Value = "";
                inputs[0].CanHaveMutipleRows = false;
                inputs[0].Index = "3";
                inputs[1].Index = "2";
                inputs[1].Value = "not empty";
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);


                //------------Execute Test---------------------------
                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(null, out indexToSelect));

                Assert.IsFalse(workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect));

                inputs[0].CanHaveMutipleRows = true;
                Assert.IsTrue(workflowInputDataViewModel.RemoveRow(inputs[0], out indexToSelect));

                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(2, inputs.Count);
                var count = workflowInputDataViewModel.WorkflowInputCount;
                Assert.AreEqual(2, count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_GetNexRow_WhenNotAllColumnsInput_ExpectRowRemoved()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);

                //------------Execute Test---------------------------
                var dataListItem = workflowInputDataViewModel.GetNextRow(inputs[0]);

                //------------Assert Results-------------------------
                Assert.IsNotNull(dataListItem);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_GetPreviousRow_WhenNotAllColumnsInput_ExpectRowRemoved()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)
                workflowInputDataViewModel.AddBlankRow(inputs[0], out int indexToSelect);

                //------------Execute Test---------------------------
                var dataListItem = workflowInputDataViewModel.GetPreviousRow(inputs[1]);

                //------------Assert Results-------------------------
                Assert.IsNotNull(dataListItem);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_GetPreviousRow_NotFound_ExpectItem()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)

                //------------Execute Test---------------------------
                var dataListItem = workflowInputDataViewModel.GetPreviousRow(inputs[0]);

                //------------Assert Results-------------------------
                Assert.IsNotNull(dataListItem);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_GetPreviousRow_Null_ExpectNull()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(1, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)

                //------------Execute Test---------------------------
                var dataListItem = workflowInputDataViewModel.GetPreviousRow(null);

                //------------Assert Results-------------------------
                Assert.IsNull(dataListItem);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetXmlData_WithObjectAndRecordSet()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec><Person Description="""" IsEditable=""True"" ColumnIODirection=""Input"" IsJson=""True""><Name></Name><Age></Age></Person></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);
            var mockDataListViewModel = new Mock<IDataListViewModel>();
            var personObject = new ComplexObjectItemModel("Person", null, enDev2ColumnArgumentDirection.Input);
            personObject.Children.Add(new ComplexObjectItemModel("Age", personObject, enDev2ColumnArgumentDirection.Input));
            personObject.Children.Add(new ComplexObjectItemModel("Name", personObject, enDev2ColumnArgumentDirection.Input));
            var complexObjectItemModels = new ObservableCollection<IComplexObjectItemModel> { personObject };
            mockDataListViewModel.Setup(model => model.ComplexObjectCollection).Returns(complexObjectItemModels);
            DataListSingleton.SetDataList(mockDataListViewModel.Object);
            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(2, inputs.Count);
                inputs[0].Value = "bob";
                //------------Execute Test---------------------------
                workflowInputDataViewModel.SetXmlData();
                //------------Assert Results-------------------------
                Assert.AreEqual("<DataList><rec><a>bob</a></rec><Person><Age></Age><Name></Name></Person></DataList>", workflowInputDataViewModel.XmlData.Replace(Environment.NewLine, "").Replace(" ", ""));
                Assert.AreEqual("{\r\n  \"rec\": [\r\n    {\r\n      \"a\": \"bob\"\r\n    }\r\n  ],\r\n  \"Person\": {\r\n    \"Age\": \"\",\r\n    \"Name\": \"\"\r\n  }\r\n}", workflowInputDataViewModel.JsonData);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_SetWorkflowInputData_AddRow_WhenAddingScalarAndNotAllColumnsHaveInput_ExpectNoNewInputs()
        {
            //------------Setup for test--------------------------
            const string Shape = @"<DataList><scalar Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(Shape);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();

            var itemToAdd = new DataListItem { DisplayValue = "scalar", Field = "scalar", CanHaveMutipleRows = false, Value = "1" };

            //------------Execute Test---------------------------
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                var inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(2, inputs.Count);
                inputs[0].Value = "1"; // trick it into thinking this happened from the UI ;)
                workflowInputDataViewModel.AddRow(itemToAdd);

                //------------Assert Results-------------------------
                inputs = workflowInputDataViewModel.WorkflowInputs;
                Assert.AreEqual(2, inputs.Count);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_CloseInputExpectFinishMessage()
        {
            var debugVM = CreateDebugOutputViewModel();
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            using (var workflowInputDataviewModel = new WorkflowInputDataViewModelMock(serviceDebugInfo.Object, debugVM))
            {
                workflowInputDataviewModel.DebugExecutionFinished += () => debugVM.DebugStatus = DebugStatus.Finished;
                var workSurfaceContextViewModelMock = new Mock<IWorkSurfaceContextViewModel>();
                var wdMock = new Mock<IWorkflowDesignerViewModel>();
                var wdMock_asWorkSurfaceViewModelMock = wdMock.As<IWorkSurfaceViewModel>();
                workSurfaceContextViewModelMock.Setup(o => o.WorkSurfaceViewModel).Returns(wdMock_asWorkSurfaceViewModelMock.Object);
                workSurfaceContextViewModelMock.Setup(o => o.Parent).Returns(wdMock.Object).Verifiable();
                workflowInputDataviewModel.Parent = workSurfaceContextViewModelMock.Object;
                workflowInputDataviewModel.ViewClosed();
                Assert.AreEqual(DebugStatus.Finished, debugVM.DebugStatus);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_Constructor_DebugTO_Initialized()
        {
            //------------Setup for test--------------------------
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();
            //------------Execute Test---------------------------
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM.SessionID))
            {

                //------------Assert Results-------------------------
                Assert.AreEqual(rm.Object.DataList, workflowInputDataViewModel.DebugTo.DataList);
                Assert.AreEqual(rm.Object.ResourceName, workflowInputDataViewModel.DebugTo.ServiceName);
                Assert.AreEqual(rm.Object.ResourceName, workflowInputDataViewModel.DebugTo.WorkflowID);
                // Travis 05.12 - Was rm.Object.WorkflowXaml.ToString(), since we no longer carry strings this was silly ;)
                Assert.AreEqual(string.Empty, workflowInputDataViewModel.DebugTo.WorkflowXaml);
                Assert.AreEqual(serviceDebugInfoModel.ServiceInputData, workflowInputDataViewModel.DebugTo.XmlData);
                Assert.AreEqual(rm.Object.ID, workflowInputDataViewModel.DebugTo.ResourceID);
                Assert.AreEqual(rm.Object.ServerID, workflowInputDataViewModel.DebugTo.ServerID);
                Assert.AreEqual(serviceDebugInfoModel.RememberInputs, workflowInputDataViewModel.DebugTo.RememberInputs);
                Assert.AreEqual(debugVM.SessionID, workflowInputDataViewModel.DebugTo.SessionID);
                Assert.IsTrue(workflowInputDataViewModel.DebugTo.IsDebugMode);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_ExecuteWorkflow_InvokesSendExecuteRequest()
        {
            //------------Setup for test--------------------------
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            rm.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "<DataList></DataList>"
            };

            var debugOutputViewModel = CreateDebugOutputViewModel();
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModelMock(serviceDebugInfoModel, debugOutputViewModel))
            {
                workflowInputDataViewModel.DebugExecutionStart += () => debugOutputViewModel.DebugStatus = DebugStatus.Executing;
                //------------Execute Test---------------------------
                workflowInputDataViewModel.ExecuteWorkflow();

                //------------Assert Results-------------------------
                Assert.AreEqual(DebugStatus.Executing, debugOutputViewModel.DebugStatus);

                Assert.AreEqual(1, workflowInputDataViewModel.SendExecuteRequestHitCount);
                Assert.IsNotNull(workflowInputDataViewModel.SendExecuteRequestPayload);

                var payload = XElement.Parse(workflowInputDataViewModel.DebugTo.XmlData);
                payload.Add(new XElement("BDSDebugMode", workflowInputDataViewModel.DebugTo.IsDebugMode));
                payload.Add(new XElement("DebugSessionID", workflowInputDataViewModel.DebugTo.SessionID));
                payload.Add(new XElement("EnvironmentID", Guid.Empty));

                var expectedPayload = payload.ToString(SaveOptions.None);
                var actualPayload = workflowInputDataViewModel.SendExecuteRequestPayload.ToString(SaveOptions.None);
                Assert.AreEqual(expectedPayload, actualPayload);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_ExecuteWorkflowViewInBrowser_InvokesSendViewInBrowserRequest_RecSet()
        {
            //------------Setup for test--------------------------
            const string datalist = @"<DataList><notInput /><rs ColumnIODirection=""Input""><val ColumnIODirection=""Input""/></rs></DataList>";
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns("SomeOtherWorkflow");
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(datalist);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            rm.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = datalist
            };

            var debugOutputViewModel = CreateDebugOutputViewModel();
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModelMock(serviceDebugInfoModel, debugOutputViewModel) { DebugTo = { DataList = datalist } })
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                workflowInputDataViewModel.XmlData = @"<DataList><rs><val>1</val></rs><rs><val>2</val></rs></DataList>";
                workflowInputDataViewModel.SetWorkflowInputData();
                //------------Execute Test---------------------------
                workflowInputDataViewModel.ViewInBrowser();
                //------------Assert Results-------------------------
                Assert.AreEqual(1, workflowInputDataViewModel.SendViewInBrowserRequestHitCount);
                Assert.IsNotNull(workflowInputDataViewModel.SendViewInBrowserRequestPayload);
                const string expectedPayload = @"<DataList><rs><val>1</val></rs><rs><val>2</val></rs></DataList>";
                var actualPayload = workflowInputDataViewModel.SendViewInBrowserRequestPayload;
                Assert.AreEqual(expectedPayload, actualPayload);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(WorkflowInputDataViewModel))]
        public void WorkflowInputDataViewModel_ExecuteWorkflowViewInBrowser_InvokesSendViewInBrowserRequest_ScalarsOnly()
        {
            //------------Setup for test--------------------------
            const string datalist = @"<DataList><val IsEditable=""True"" ColumnIODirection=""Input""/><res IsEditable=""True"" ColumnIODirection=""Input""/></DataList>";
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns("AnotherWorkflow");
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(datalist);
            var mockEnvironmentModel = new Mock<IServer>();
            mockEnvironmentModel.Setup(model => model.EnvironmentID).Returns(Guid.Empty);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            rm.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);


            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = datalist
            };

            var debugOutputViewModel = CreateDebugOutputViewModel();
            using (var workflowInputDataViewModel = new WorkflowInputDataViewModelMock(serviceDebugInfoModel, debugOutputViewModel) { DebugTo = { DataList = datalist } })
            {
                workflowInputDataViewModel.LoadWorkflowInputs();
                workflowInputDataViewModel.XmlData = @"<DataList><val>1</val><res>2</res></DataList>";
                workflowInputDataViewModel.SetWorkflowInputData();
                //------------Execute Test---------------------------
                workflowInputDataViewModel.ViewInBrowser();
                //------------Assert Results-------------------------
                Assert.AreEqual(1, workflowInputDataViewModel.SendViewInBrowserRequestHitCount);
                Assert.IsNotNull(workflowInputDataViewModel.SendViewInBrowserRequestPayload);
                const string expectedPayload = @"val=1&res=2";
                var actualPayload = workflowInputDataViewModel.SendViewInBrowserRequestPayload;
                Assert.AreEqual(expectedPayload, actualPayload);
            }
        }

        static OptomizedObservableCollection<IDataListItem> GetInputTestDataDataNames()
        {
            const int numberOfRecords = 6;
            const int numberOfRecordFields = 2;
            var items = new OptomizedObservableCollection<IDataListItem>();
            items.AddRange(GetDataListItemScalar());
            items.AddRange(CreateTestDataListItemRecords(numberOfRecords, numberOfRecordFields));

            return items;


        }

        static IList<IDataListItem> GetDataListItemScalar()
        {
            IList<IDataListItem> scalars = new OptomizedObservableCollection<IDataListItem>
                                                                            {  CreateScalar("scalar1", "ScalarData1")
                                                                             , CreateScalar("scalar2", "ScalarData2")
                                                                            };
            return scalars;

        }

        static IList<IDataListItem> CreateTestDataListItemRecords(int numberOfRecords, int recordFieldCount)
        {
            IList<IDataListItem> recordSets = new List<IDataListItem>();
            for (int i = 1; i <= numberOfRecords; i++)
            {
                for (int j = 1; j <= recordFieldCount; j++)
                {
                    recordSets.Add(CreateRecord("Recset", "Field" + j, "Field" + j + "Data" + i, i));
                }
            }

            return recordSets;
        }

        static IDataListItem CreateScalar(string scalarName, string scalarValue)
        {
            var item = new Mock<IDataListItem>();
            item.Setup(itemName => itemName.DisplayValue).Returns(scalarName);
            item.Setup(itemName => itemName.Field).Returns(scalarName);
            item.Setup(itemName => itemName.RecordsetIndexType).Returns(enRecordsetIndexType.Numeric);
            item.Setup(itemName => itemName.Value).Returns(scalarValue);

            return item.Object;
        }

        static IDataListItem CreateRecord(string recordSetName, string recordSetField, string recordSetValue, int recordSetIndex)
        {
            var records = new Mock<IDataListItem>();
            records.Setup(rec => rec.DisplayValue).Returns(recordSetName + "(" + recordSetIndex + ")." + recordSetField);
            records.Setup(rec => rec.Field).Returns(recordSetField);
            records.Setup(rec => rec.Index).Returns(Convert.ToString(recordSetIndex));
            records.Setup(rec => rec.RecordsetIndexType).Returns(enRecordsetIndexType.Numeric);
            records.Setup(rec => rec.Recordset).Returns(recordSetName);
            records.Setup(rec => rec.Value).Returns(recordSetValue);
            records.Setup(rec => rec.CanHaveMutipleRows).Returns(true);

            return records.Object;
        }

        Mock<IContextualResourceModel> GetMockResource()
        {
            var mockResource = new Mock<IContextualResourceModel>();
            mockResource.SetupGet(r => r.ServerID).Returns(_serverID);
            mockResource.SetupGet(r => r.ResourceName).Returns(ResourceName);
            mockResource.SetupGet(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            mockResource.SetupGet(r => r.ID).Returns(_resourceID);
            mockResource.SetupGet(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_NoInputs_XMLData);
            return mockResource;
        }

        static Mock<IServiceDebugInfoModel> GetMockServiceDebugInfo(Mock<IContextualResourceModel> mockResouce)
        {
            var serviceDebugInfo = new Mock<IServiceDebugInfoModel>();
            serviceDebugInfo.SetupGet(sd => sd.DebugModeSetting).Returns(DebugMode.DebugInteractive);
            serviceDebugInfo.SetupGet(sd => sd.ResourceModel).Returns(mockResouce.Object);
            serviceDebugInfo.SetupGet(sd => sd.RememberInputs).Returns(false);
            serviceDebugInfo.SetupGet(sd => sd.ServiceInputData).Returns(mockResouce.Object.DataList);
            return serviceDebugInfo;
        }

        static DebugOutputViewModel CreateDebugOutputViewModel()
        {
            var models = new List<IServer>();
            var envRepo = new Mock<IServerRepository>();
            envRepo.Setup(s => s.All()).Returns(models);
            envRepo.Setup(s => s.IsLoaded).Returns(true);
            envRepo.Setup(repository => repository.Source).Returns(new Mock<IServer>().Object);

            return new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo.Object, new Mock<IDebugOutputFilterStrategy>().Object);
        }
    }

    public class WorkflowInputDataViewModelMock : WorkflowInputDataViewModel
    {
        public WorkflowInputDataViewModelMock(IServiceDebugInfoModel serviceDebugInfoModel, DebugOutputViewModel debugOutputViewModel)
            : base(serviceDebugInfoModel, debugOutputViewModel.SessionID)
        {
        }

        public int SendExecuteRequestHitCount { get; private set; }
        public int SendViewInBrowserRequestHitCount { get; private set; }
        public XElement SendExecuteRequestPayload { get; private set; }
        public string SendViewInBrowserRequestPayload { get; set; }

        protected override void SendExecuteRequest(XElement payload)
        {
            SendExecuteRequestHitCount++;
            SendExecuteRequestPayload = payload;
        }

        protected override void OpenUriInBrowser(string payload)
        {
            SendViewInBrowserRequestHitCount++;
            SendViewInBrowserRequestPayload = payload;
        }
    }
}
