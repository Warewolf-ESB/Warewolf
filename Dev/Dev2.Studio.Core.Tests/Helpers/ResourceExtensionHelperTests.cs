using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;

namespace Dev2.Studio
{
    [TestClass]

    public class ResourceExtensionHelperTests
    {
        [TestMethod]
        public void HandleResourceNotInResourceFolderAsync_Given_MoveCancel_On_Popup_MessageBox_Has_Null_ViewModel()
        {
            var popupController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popupController.Setup(p => p.ShowResourcesNotInCorrectPath()).Returns(System.Windows.MessageBoxResult.Cancel);
            var shellViewModel = new Mock<IShellViewModel>().Object;
            var file = new Mock<IFile>();
            var path = new Mock<IFilePath>();
            path.Setup(p => p.GetFileName(It.IsAny<string>())).Returns(It.IsAny<string>());
            var serverRepository = new Mock<IServerRepository>();
            var results = ResourceExtensionHelper.HandleResourceNotInResourceFolderAsync(It.IsAny<string>(), It.IsAny<string>(), popupController.Object, shellViewModel, file.Object, path.Object, serverRepository.Object);
            Assert.IsNull(results.Result);
        }

        [TestMethod]
        public void HandleResourceNotInResourceFolderAsync_Given_MoveOK_On_Popup_MessageBox_Has_Null_ViewModel()
        {
            var file = new Mock<IFile>();
            var path = new Mock<IFilePath>();
            path.Setup(p => p.GetFileName(It.IsAny<string>())).Returns(It.IsAny<string>());
            var content = new MemoryStream(Encoding.ASCII.GetBytes(WorkflowContent));
            var serverRepository = new Mock<IServerRepository>();
            var resource = new Mock<IResource>();
            resource.Setup(p => p.ResourceID).Returns(Guid.Parse("e7ea5196-33f7-4e0e-9d66-44bd67528a96"));
            resource.Setup(p => p.ResourceName).Returns("AssignOutput");
            resource.Setup(p => p.ResourceType).Returns("Workflow");
            var popupController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            popupController.Setup(p => p.ShowResourcesNotInCorrectPath()).Returns(System.Windows.MessageBoxResult.OK);
            var shellViewModel = new Mock<IShellViewModel>();
            shellViewModel.Setup(p => p.CreateResourceFromStreamContent(It.IsAny<string>())).Returns(resource.Object);
            file.Setup(p => p.OpenRead(It.IsAny<string>())).Returns(content);
            serverRepository.Setup(p => p.ActiveServer).Returns(new Mock<IServer>().Object);
            var results = ResourceExtensionHelper.HandleResourceNotInResourceFolderAsync(It.IsAny<string>(), It.IsAny<string>(), popupController.Object, shellViewModel.Object, file.Object, path.Object, serverRepository.Object);
            Assert.IsNotNull(results);
            Assert.AreEqual("AssignOutput", results.Result.ResourceName);
        }

        [TestMethod]
        public void HandleResourceNotInResourceFolderAsync_Given_Source_And_MoveOK_On_Popup_MessageBox_Has_Null_ViewModel()
        {
            var file = new Mock<IFile>();
            var path = new Mock<IFilePath>();
            var content = new MemoryStream(Encoding.ASCII.GetBytes(SourceContent));
            var activeServer = new Mock<IServer>();
            var resourceRepo = new Mock<IResourceRepository>();
            var shellViewModel = new Mock<IShellViewModel>();
            var serverRepository = new Mock<IServerRepository>();
            var explorerViewModel = new Mock<IExplorerViewModel>();
            var popupController = new Mock<Common.Interfaces.Studio.Controller.IPopupController>();
            var contextualResourceModel = new Mock<IContextualResourceModel>();
            contextualResourceModel.Setup(p => p.ResourceName).Returns("WebPutServiceSource");
            var resource = new Mock<IResource>();
            resource.Setup(p => p.ResourceID).Returns(Guid.Parse("e7ea5196-33f7-4e0e-9d66-44bd67528a96"));
            resource.Setup(p => p.ResourceName).Returns("AssignOutput");
            resource.Setup(p => p.ResourceType).Returns("Source");
            shellViewModel.Setup(p => p.ExplorerViewModel).Returns(explorerViewModel.Object);
            shellViewModel.Setup(p => p.CreateResourceFromStreamContent(It.IsAny<string>())).Returns(resource.Object);
            popupController.Setup(p => p.ShowResourcesNotInCorrectPath()).Returns(System.Windows.MessageBoxResult.OK);
            popupController.Setup(p => p.ShowCanNotMoveResource()).Returns(System.Windows.MessageBoxResult.OK);
            file.Setup(p => p.OpenRead(It.IsAny<string>())).Returns(content);
            file.Setup(p => p.Move(It.IsAny<string>(), It.IsAny<string>()));
            path.Setup(p => p.GetFileName(It.IsAny<string>())).Returns(It.IsAny<string>());
            resourceRepo.Setup(p => p.LoadContextualResourceModel(It.IsAny<Guid>())).Returns(contextualResourceModel.Object);
            activeServer.Setup(p => p.ResourceRepository).Returns(resourceRepo.Object);
            serverRepository.Setup(p => p.ActiveServer).Returns(activeServer.Object);
            var results = ResourceExtensionHelper.HandleResourceNotInResourceFolderAsync(It.IsAny<string>(), It.IsAny<string>(), popupController.Object, shellViewModel.Object, file.Object, path.Object, serverRepository.Object);
            Assert.IsNotNull(results);
            Assert.AreEqual("WebPutServiceSource", results.Result.ResourceName);
        }
        public string WorkflowContent
        {
            get
            {
                return @"<Service ID=""e7ea5196 - 33f7 - 4e0e - 9d66 - 44bd67528a96"" Version=""1.0"" ServerID=""51a58300 - 7e9d - 4927 - a57b - e5d700b11b55"" Name=""AssignOutput"" ResourceType=""WorkflowService"" IsValid=""false"" ServerVersion=""0.0.6530.13965"">  <DisplayName>AssignOutput</DisplayName>  <Category>  </Category>  <IsNewWorkflow>false</IsNewWorkflow>  <AuthorRoles>  </AuthorRoles>  <Comment>  </Comment>  <Tags>  </Tags>  <HelpLink>  </HelpLink>  <UnitTestTargetWorkflowService>  </UnitTestTargetWorkflowService>  <DataList>    <a Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />  </DataList>  <Action Name=""InvokeWorkflow"" Type=""Workflow"">    <XamlDefinition>&lt;Activity x:Class=""AssignOutput"" sap:VirtualizedContainerService.HintSize=""654,679"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dciipe=""clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""6""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""AssignOutput"" sap:VirtualizedContainerService.HintSize=""614,639""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,127.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;185,127.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,88&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity CurrentResult=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,88"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsService=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""19e486a9-28e3-4a92-99da-579779e16bd7"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" WatermarkTextVariable=""{x:Null}"" FieldName=""[[a]]"" FieldValue=""a"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""Value""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" Path=""{x:Null}"" WatermarkTextVariable=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" IsFieldValueFocused=""False"" WatermarkTextValue=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dciipe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>  </Action>  <ErrorMessages />  <VersionInfo DateTimeStamp=""2017-05-24T14:09:17.5096789+02:00"" Reason=""Save"" User=""Unknown"" VersionNumber=""1"" ResourceId=""e7ea5196-33f7-4e0e-9d66-44bd67528a96"" VersionId=""a7600426-c0b8-42a5-8200-8c2669152190"" /></Service>";
            }
        }
        public string SourceContent
        {
            get
            {
                return @"<Source ID=""0fb49fec - e454 - 4357 - a06f - 08f329558b18"" Name=""WebPutServiceSource"" ResourceType=""WebSource"" IsValid=""false"" ConnectionString=""AQAAANCMnd8BFdERjHoAwE / Cl + sBAAAA / 1ic3IlWqU2izrIx9s6raAQAAAACAAAAAAADZgAAwAAAABAAAAC4Ok4wmWIHXpfvh + nOGdVxAAAAAASAAACgAAAAEAAAAI3YkxGwme0czR6uMkmpA + vAAAAABmZEtF5W / zZkoviQHdIDe6NVHG66gBarJyaIp7QWn4vlz0DqkH78M6XELFvQCskVU6XyanerGXTd / vf1tcGg8r + 1KssjUL59ow / 12DurAUz9EXUiD71wbxJZsG2TIXEKbp7AtMIwHiNN / A2o7Flj105Vbjo3vp8muB19zK24qsxa8rlhYgQKZUhQt09Wb5IUquRzfcC4d5liv6uU + Pbs27pY + In6XdwwVV4kbZQ2EBgG0oLcyA51Sag7bmknG4wbFAAAADyMjQbD / PJbzjNOUX1PqNhdY5zv"" Type=""WebSource"" ServerVersion=""0.0.6530.13965"" ServerID=""51a58300 - 7e9d - 4927 - a57b - e5d700b11b55"">  <DisplayName>WebPutServiceSource</DisplayName>  <AuthorRoles>  </AuthorRoles>  <ErrorMessages />  <TypeOf>WebSource</TypeOf>  <VersionInfo DateTimeStamp=""2016 - 11 - 21T17: 23:26.9875401 + 02:00"" Reason="""" User=""DEV2\nkosinathi.sangweni"" VersionNumber=""3"" ResourceId=""0fb49fec - e454 - 4357 - a06f - 08f329558b18"" VersionId=""065ae8f1 - 19ca - 4a34 - ab6f - f828a00935c1"" /></Source>";
            }
        }
    }
}