using System;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Composition;
using Dev2.Core.Tests;
using Dev2.Core.Tests.Environments;
using Dev2.Core.Tests.XML;
using Dev2.DynamicServices;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework;

namespace BusinessDesignStudio.Unit.Tests
{
    /// <summary>
    /// Summary description for ResourceRepositoryTest
    /// </summary>
    [TestClass]
    public class ResourceRepositoryTest
    {

        #region Variables

        // Global variables
        readonly Mock<IEnvironmentConnection> _environmentConnection = CreateEnvironmentConnection();
        readonly Mock<IEnvironmentModel> _environmentModel = ResourceModelTest.CreateMockEnvironment();
        readonly Mock<IStudioClientContext> _dataChannel = new Mock<IStudioClientContext>();
        readonly Mock<IResourceModel> _resourceModel = new Mock<IResourceModel>();
        ResourceRepository _repo;
        readonly Mock<IFrameworkSecurityContext> _securityContext = new Mock<IFrameworkSecurityContext>();
        private Guid _resourceGuid = Guid.NewGuid();
        private Guid _serverID = Guid.NewGuid();
        private Guid _workspaceID = Guid.NewGuid();

        #endregion Variables

        #region Additional result attributes
        //Use TestInitialize to run code before running each result 
        [TestInitialize()]
        public void MyTestInitialize()
        {
           // Setup();
        }

        void Setup()
        {
            //ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            _resourceModel.Setup(res => res.ResourceName).Returns("Resource");
            _resourceModel.Setup(res => res.DisplayName).Returns("My New Resource");
            _resourceModel.Setup(res => res.ServiceDefinition).Returns("My new Resource service definition");
            _resourceModel.Setup(res => res.ID).Returns(_resourceGuid);
            _resourceModel.Setup(res => res.WorkflowXaml).Returns("OriginalXaml");

            _dataChannel.Setup(channel => channel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<x><text>Im Happy</text></x>").Verifiable();
            _dataChannel.Setup(channel => channel.ServerID).Returns(_serverID);
            _dataChannel.Setup(channel => channel.WorkspaceID).Returns(_workspaceID);

            _securityContext.Setup(s => s.Roles).Returns(new string[2]);

            _environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            _environmentConnection.Setup(prop => prop.DataChannel).Returns(_dataChannel.Object);
            _environmentConnection.Setup(prop => prop.IsConnected).Returns(true);
            _environmentConnection.Setup(c => c.SecurityContext).Returns(_securityContext.Object);
            var mock = new Mock<IStudioNetworkMessageAggregator>();
            _environmentConnection.Setup(connection => connection.MessageAggregator).Returns(mock.Object);

            _environmentModel.Setup(m => m.LoadResources()).Verifiable();
            _environmentModel.Setup(m => m.DsfChannel).Returns(_dataChannel.Object);
            _environmentModel.Setup(e => e.Connection).Returns(_environmentConnection.Object);
            //_environmentModel.Setup(e => e.IsConnected).Returns(true);
            //_environmentModel.Setup(e => e.ID).Returns(Guid.NewGuid());

            _repo = new ResourceRepository(_environmentModel.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object) { IsLoaded = true }; // Prevent clearing of internal list and call to connection!
        }

        // Use TestCleanup to run code after each result has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Hydrate Resource Model

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_WhenDataListContainsIDTag_ValidResourceModel()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockWizEngine = new Mock<IWizardEngine>();
            var mockSecurityCtx = new Mock<IFrameworkSecurityContext>();

            var resourceRepository = new ResourceRepository(mockEnvironmentModel.Object, mockWizEngine.Object, mockSecurityCtx.Object);
            
            // create the required dynamic ;)
            var dataObject = GetTestData();
            dataObject = dataObject.Replace("@@@@", "");
            var uo = new UnlimitedObject(dataObject);

            //------------Execute Test---------------------------

            var model = resourceRepository.HydrateResourceModel(ResourceType.WorkflowService, uo, false);

            //------------Assert Results-------------------------

            Assert.IsNotNull(model);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsDbService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "");
            data = data.Replace("WorkflowService", "DbService");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/DatabaseService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsDbSource_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "");
            data = data.Replace("WorkflowService", "DbSource");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/DatabaseService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsEmailSource_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "");
            data = data.Replace("WorkflowService", "EmailSource");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/ToolSendEmail-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsPluginSource_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "");
            data = data.Replace("WorkflowService", "PluginSource");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/PluginService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWebService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "");
            data = data.Replace("WorkflowService", "WebService");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/WebService-32.png");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWebSource_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "asdfsd");
            data = data.Replace("WorkflowService", "WebSource");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/WebService-32.png");
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsPluginService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "sffds");
            data = data.Replace("WorkflowService", "PluginService");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.Service, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/PluginService-32.png");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_ResourceTypeIsWorkflowService_IconPathIsValid()
        {
            //------------Setup for test--------------------------
            var resourceRepository = GetResourceRepository();
            // create the required dynamic ;)
            var data = GetTestData();
            data = data.Replace("@@@@", "");
            var uo = new UnlimitedObject(data);
            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.WorkflowService, uo, false);
            //------------Assert Results-------------------------
            Assert.IsNotNull(model);
            Assert.AreEqual(model.IconPath, "pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ResourceRepository_HydrateResourceModel")]
        public void ResourceRepository_HydrateResourceModel_WhenDataIsNewWorkflow_NewWorkFlowNamesUpdated()
        {
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockWizEngine = new Mock<IWizardEngine>();
            var mockSecurityCtx = new Mock<IFrameworkSecurityContext>();

            var resourceRepository = new ResourceRepository(mockEnvironmentModel.Object, mockWizEngine.Object, mockSecurityCtx.Object);

            #region Test Data

            var data = @"<Service ID=""b5bbbd9c-fe5f-47da-874a-285cdc9b03db"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""Unsaved 1"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>Unsaved 1</DisplayName>
  <Category>TU_BENCHMARK_DATA_RULES</Category>
  <IsNewWorkflow>true</IsNewWorkflow>
  <AuthorRoles></AuthorRoles>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath></IconPath>
  <HelpLink>http://warewolf.io/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <TotalRecords Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <idx Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <RSAID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <FirstNames Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <ImportData Description="""" IsEditable=""True"" ColumnIODirection=""Input"">
      <ID Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <FirstName1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <FirstName2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <FirstName3 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    </ImportData>
    <ValidationResult Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <Msg Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
      <ID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
      <RSAID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </ValidationResult>
    <Validation Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <Msg Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
      <RSAID Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </Validation>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""Unsaved 1"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=System"" xmlns:sco1=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;1226,802&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco1:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco1:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""First Name Validation"" sap:VirtualizedContainerService.HintSize=""1186,762""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;x:Double x:Key=""Width""&gt;1172.5&lt;/x:Double&gt;&lt;x:Double x:Key=""Height""&gt;725.5&lt;/x:Double&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,107.5 590.5,107.5 590.5,196&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID7&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID7""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;490.5,196&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;200,90&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;690.5,241 808.5,241&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfCountRecordsetActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CountNumber=""[[TotalRecords]]"" DatabindRecursive=""False"" DisplayName=""Count Records"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,90"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" RecordsetName=""[[ImportData()]]"" SimulationMode=""OnDemand"" UniqueID=""03152e28-0ce1-44db-bcfb-8fc7fee6466b""&gt;&lt;uaba:DsfCountRecordsetActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfCountRecordsetActivity.AmbientDataList&gt;&lt;uaba:DsfCountRecordsetActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfCountRecordsetActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfCountRecordsetActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID2""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;808.5,196.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;990.5,285.5 990.5,316.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a6160c77-3c0e-4601-9226-fcc1dec75eb8"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[idx]]"" FieldValue=""2"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;808.5,316.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;990.5,405.5 990.5,459&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""d9bc5ae0-9973-4697-8013-11f9ddda05aa"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[FirstNames]]"" FieldValue=""[[ImportData([[idx]]).FirstName1]] [[ImportData([[idx]]).FirstName2]] [[ImportData([[idx]]).FirstName3]]"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName=""[[RSAID]]"" FieldValue=""[[ImportData([[idx]]).ID]]"" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""3"" Inserted=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID3""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;865.5,459&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;250,84&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;990.5,543 990.5,637.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=""{x:Null}"" ActivityStateData=""{x:Null}"" AuthorRoles=""{x:Null}"" Category=""{x:Null}"" Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" DataTags=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ResultValidationExpression=""{x:Null}"" ResultValidationRequiredTags=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceUri=""{x:Null}"" SimulationOutput=""{x:Null}"" Tags=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DeferExecution=""False"" DisplayName=""Name Validation"" EnvironmentID=""00000000-0000-0000-0000-000000000000"" FriendlySourceName=""localhost"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,84"" IconPath=""[Nothing]"" InputMapping=""&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Name&amp;quot; Source=&amp;quot;[[FirstNames]]&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;TypeCheck&amp;quot; Source=&amp;quot;First Names&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;RSAID&amp;quot; Source=&amp;quot;[[RSAID]]&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""True"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputMapping=""&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;Msg&amp;quot; MapsTo=&amp;quot;Msg&amp;quot; Value=&amp;quot;[[ValidationResult(*).Msg]]&amp;quot; Recordset=&amp;quot;Validation&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;ID&amp;quot; MapsTo=&amp;quot;ID&amp;quot; Value=&amp;quot;[[ValidationResult(*).ID]]&amp;quot; Recordset=&amp;quot;Validation&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;"" RemoveInputFromOutput=""False"" ResourceID=""ff6d0864-e8ac-4b67-bb82-aba65037b3ba"" ServiceName=""Name Validation"" ServiceServer=""00000000-0000-0000-0000-000000000000"" SimulationMode=""OnDemand"" ToolboxFriendlyName=""Name Validation"" Type=""Workflow"" UniqueID=""fb86530e-cbdf-496c-bd8c-8801310536f5""&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=""x:String""&gt;&lt;Literal x:TypeArguments=""x:String"" Value="""" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowDecision x:Name=""__ReferenceID1"" DisplayName=""If [[idx]] Is Less Than or Equal [[TotalRecords]]"" sap:VirtualizedContainerService.HintSize=""160,87""&gt;&lt;FlowDecision.Condition&gt;&lt;uaba:DsfFlowDecisionActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Decision"" ExpressionText=""Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(&amp;quot;{!TheStack!:[{!Col1!:![[idx]]!,!Col2!:![[TotalRecords]]!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsLessThanOrEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If [[idx]] Is Less Than or Equal [[TotalRecords]]!}&amp;quot;,AmbientDataList)"" HasError=""[HasError]"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""6439f233-76fc-4ac4-88c9-1ed8990d4eac""&gt;&lt;uaba:DsfFlowDecisionActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfFlowDecisionActivity.AmbientDataList&gt;&lt;uaba:DsfFlowDecisionActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfFlowDecisionActivity.ParentInstanceID&gt;&lt;/uaba:DsfFlowDecisionActivity&gt;&lt;/FlowDecision.Condition&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;910.5,637.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;160,87&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""TrueConnector""&gt;910.5,681 842.5,681&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;FlowDecision.True&gt;&lt;FlowStep x:Name=""__ReferenceID5""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;478.5,636.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;478.5,681 448.5,681 448.5,611 400.5,611&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""9dcc476e-737a-4a2a-b5c2-1c3cd4ec101b"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[Validation().Msg]]"" FieldValue=""[[ValidationResult(*).Msg]]"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName=""[[Validation().RSAID]]"" FieldValue=""[[ValidationResult(*).RSAID]]"" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""3"" Inserted=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID6""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;200.5,566&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;200,90&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300.5,566 300.5,525.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDeleteRecordActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Delete Record"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,90"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" RecordsetName=""[[Validation(*)]]"" Result="""" SimulationMode=""OnDemand"" UniqueID=""d08ee008-1b26-4202-8dae-ec1e67c952f1""&gt;&lt;uaba:DsfDeleteRecordActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDeleteRecordActivity.AmbientDataList&gt;&lt;uaba:DsfDeleteRecordActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDeleteRecordActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDeleteRecordActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID4""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;118.5,436.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;482.5,481 512.5,481 512.5,361 808.5,361&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""fab6831a-0067-44e8-adb4-aed544be9d69"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[idx]]"" FieldValue=""!~calculation~![[idx]]+1!~~calculation~!"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowDecision.True&gt;&lt;/FlowDecision&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID2&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID3&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID4&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID5&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID6&lt;/x:Reference&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <BizRule />
  <WorkflowActivityDef />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>rfR9Ry6mHVMie+vSkOi0pJlq7jM=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>gTC12dhYhIgcIdYVoUgXvii8S/s46txocwldoFqoLPNab0ryiU1xGOgwWbB3jG1BUpSGg1ETh50GGKvsfA9a6nB7NeKB6Oh9uLXYyBkf9t7AC8IiCeJuGjmh8I/twEflvsJ7OMDmyE+T7TWxiEelCrRiN/iwRz1W7QBtOLNBG+Y=</SignatureValue>
  </Signature>
</Service>";

            #endregion

            UnlimitedObject uo = new UnlimitedObject(data);

            //------------Execute Test---------------------------
            var model = resourceRepository.HydrateResourceModel(ResourceType.WorkflowService, uo);

            // Assert NewWorkFlowNames instance updated
            Assert.IsNotNull(model);
            Assert.IsTrue(NewWorkflowNames.Instance.Contains("Unsaved 1"));
        }
        
        #endregion

        #region Load Tests

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void Load_CreateAndLoadResource_SingleResource_Expected_ResourceReturned()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);


            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.Load();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(1));


        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadSuccessfullLoadExpectIsLoadedTrue()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload></Payload>"));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            Assert.IsTrue(_repo.IsLoaded);
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadWithExceptionOnLoadExpectsIsLoadedFalse()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns("<Payload><Service Name=\"TestWorkflowService1\", <= Bad payload</Payload>");

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            //Assert
            Assert.IsFalse(_repo.IsLoaded);

        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadWith2WorkflowsExpectResourcesLoaded()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            var guid1 = Guid.NewGuid().ToString();
            var guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", guid1, guid2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();
            int resources = _repo.All().Count;
            //Assert
            Assert.IsTrue(resources.Equals(2));
            var resource = _repo.All().First();

            Assert.IsTrue(resource.ResourceType == ResourceType.WorkflowService);
            Assert.IsTrue(resource.ResourceName == "TestWorkflowService1");
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void LoadWorkflowExpectsFromCache()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            var guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", _resourceGuid, guid2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.ForceLoad();

            int resources = _repo.All().Count;

            guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" ResourceType=\"WorkflowService\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));

            _repo.ForceLoad();

            //Assert
            Assert.IsTrue(resources.Equals(2));
            var resource = _repo.All().First();
            var resource2 = _repo.All().Last();

            Assert.IsTrue(resource.WorkflowXaml == "ChangedDefinition");
            Assert.IsTrue(resource2.WorkflowXaml == "ChangedDefinition");
        }

        private Mock<IEnvironmentConnection> SetupConnection()
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri)
                .Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            conn.Setup(c => c.SecurityContext).Returns(securityContext.Object);
            return conn;
        }

        /// <summary>
        /// Test case for creating a resource and saving the resource model in resource factory
        /// </summary>
        [TestMethod]
        public void ForceLoadWith2ReservedResourcesExpectsServicesAdded()
        {
            //Arrange
            Setup();
            var conn = SetupConnection();

            const string Reserved1 = "TestName1";
            const string Reserved2 = "TestName2";
            const string ServiceFormat = "<Service Name=\"{0}\" ResourceType=\"{1}\" />";

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload>{0}{1}</Payload>",
                    string.Format(ServiceFormat, Reserved1, Dev2.Data.ServiceModel.ResourceType.ReservedService),
                    string.Format(ServiceFormat, Reserved2, Dev2.Data.ServiceModel.ResourceType.ReservedService)
                    ));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            Assert.IsTrue(_repo.IsReservedService(Reserved1));
            Assert.IsTrue(_repo.IsReservedService(Reserved2));
        }

        /// <summary>
        /// Create resource with source type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_SourceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.Source);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        /// <summary>
        /// Create resource with human Interface service type
        /// </summary>
        [TestMethod]
        public void UpdateResourcesExpectsWorkspacesLoadedBypassCache()
        {
            //Arrange
            Setup();
            new Mock<IResourceModel>().Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

            var conn = SetupConnection();


            var guid2 = Guid.NewGuid().ToString();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedXaml\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", _resourceGuid, guid2));

            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Load();
            //Assert
            var resource = _repo.All().First();
            Assert.IsTrue(resource.WorkflowXaml.Equals("OriginalXaml"));

            var workspaceItemMock = new Mock<IWorkspaceItem>();
            workspaceItemMock.Setup(s => s.ServerID).Returns(_serverID);
            workspaceItemMock.Setup(s => s.WorkspaceID).Returns(_workspaceID);

            _repo.UpdateWorkspace(new List<IWorkspaceItem> { workspaceItemMock.Object });
            resource = _repo.All().First();
            Assert.IsTrue(resource.WorkflowXaml.Equals("ChangedXaml"));
        }

        /// <summary>
        /// Create resource with human Interface service type
        /// </summary>
        [TestMethod]
        public void LoadMultipleResourceLoad_HumanInterfaceServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        /// <summary>
        /// Create resource with workflow service type
        /// </summary>
        [TestMethod]
        public void Load_MultipleResourceLoad_WorkflowServiceType_Expected_AllResourcesReturned()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceType).Returns(ResourceType.WorkflowService);

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //Act
            _repo.Save(_resourceModel.Object);
            _repo.Save(model.Object);
            _repo.Load();
            //Assert
            Assert.IsTrue(_repo.All().Count.Equals(2));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Load_CreateResourceNullEnvironmentConnection_Expected_InvalidOperationException()
        {
            Setup();
            _environmentConnection.Setup(prop => prop.IsConnected).Returns(false);
            _repo.Save(_resourceModel.Object);
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Load")]
        [Description("ResourceRepository Load must only do one server call to retrieve all resources")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceRepository_UnitTest_Load_InvokesAddResourcesOnce()
        {
            var wizardEngine = new Mock<IWizardEngine>();

            var envConnection = new Mock<IEnvironmentConnection>();
            envConnection.Setup(e => e.WorkspaceID).Returns(Guid.NewGuid());
            envConnection.Setup(e => e.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            envConnection.Setup(e => e.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Empty);

            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection).Returns(envConnection.Object);

            var resourceRepo = new TestResourceRepository(envModel.Object, wizardEngine.Object);
            resourceRepo.Load();

            Assert.AreEqual(1, resourceRepo.LoadResourcesHitCount, "ResourceRepository Load did more than one server call.");
        }

        #endregion Load Tests

        #region Save Tests

        //Updating the resources if there were ready exist in resource repository

        [TestMethod]
        public void UpdateResource()
        {
            //Arrange
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceName).Returns("TestName");

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(model.Object);
            _repo.Load();
            model.Setup(c => c.ResourceName).Returns("NewName");
            _repo.Save(model.Object);
            //Assert
            ICollection<IResourceModel> set = _repo.All();
            int cnt = set.Count;

            IResourceModel[] setArray = set.ToArray();
            Assert.IsTrue(cnt == 1 && setArray[0].ResourceName == "NewName");
        }

        //Create a resource with the same resource name
        [TestMethod]
        public void SameResourceName()
        {
            Setup();
            Mock<IResourceModel> model2 = new Mock<IResourceModel>();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.DisplayName).Returns("result");
            model.Setup(c => c.ResourceName).Returns("result");
            model2.Setup(c => c.DisplayName).Returns("result");
            model2.Setup(c => c.ResourceName).Returns("result");

            var conn = SetupConnection();

            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);


            //Act
            _repo.Save(model.Object);
            _repo.Save(model2.Object);
            _repo.Load();

            Assert.IsTrue(_repo.All().Count.Equals(1));
        }

        #endregion Save Tests

        #region RemoveResource Tests

        [TestMethod]
        public void WorkFlowService_OnDelete_Expected_NotInRepository()
        {
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.Connection.SecurityContext).Returns(_securityContext.Object);
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(x => x.Connection.ServerEvents).Returns(new EventPublisher());

            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.AppServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel).Returns(Dev2MockFactory.SetupIFrameworkDataChannel_EmptyReturn().Object);

            mockEnvironmentModel.Setup(model1 => model1.Connection.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var myItem = new ResourceModel(mockEnvironmentModel.Object);
            myItem.ResourceName = "TestResource";
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            int exp = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            mockEnvironmentModel.Object.ResourceRepository.Remove(myItem);
            Assert.AreEqual(exp - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            Assert.AreEqual(1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
        }

        [TestMethod]
        public void NonExistantWorkFlowService_OnDelete_Expected_Failure()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var myRepo = new ResourceRepository(env.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);
            var myItem = new ResourceModel(env.Object);
            var actual = myRepo.DeleteResource(myItem);
            Assert.AreEqual("Failure", actual.InnerXmlString, "Non existant resource deleted successfully");
        }

        [TestMethod]
        [TestCategory("ResourceRepository_Delete")]
        [Description("Unassigned resources can be deleted")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_UnitTest_DeleteUnassignedResource_ResourceDeletedFromRepository()
        // ReSharper restore InconsistentNaming
        {
            //Isolate delete unassigned resource as a functional unit
            Mock<IEnvironmentModel> mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.SetupGet(x => x.Connection.AppServerUri).Returns(new Uri("http://127.0.0.1/"));
            mockEnvironmentModel.SetupGet(x => x.Connection.SecurityContext).Returns(_securityContext.Object);
            mockEnvironmentModel.SetupGet(x => x.IsConnected).Returns(true);
            mockEnvironmentModel.SetupGet(x => x.Connection.ServerEvents).Returns(new EventPublisher());

            mockEnvironmentModel.Setup(environmentModel => environmentModel.Connection.AppServerUri).Returns(new Uri(StringResources.Uri_WebServer));
            mockEnvironmentModel.Setup(environmentModel => environmentModel.DsfChannel).Returns(Dev2MockFactory.SetupIFrameworkDataChannel_EmptyReturn().Object);

            mockEnvironmentModel.Setup(model1 => model1.Connection.ExecuteCommand(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            var ResourceRepository = new ResourceRepository(mockEnvironmentModel.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);

            mockEnvironmentModel.SetupGet(x => x.ResourceRepository).Returns(ResourceRepository);
            mockEnvironmentModel.Setup(x => x.LoadResources());

            var myItem = new ResourceModel(mockEnvironmentModel.Object);
            myItem.ResourceName = "TestResource";
            myItem.Category = string.Empty;
            mockEnvironmentModel.Object.ResourceRepository.Add(myItem);
            int expectedCount = mockEnvironmentModel.Object.ResourceRepository.All().Count;

            //Do delete
            mockEnvironmentModel.Object.ResourceRepository.DeleteResource(myItem);

            //Assert resource deleted from repository
            Assert.AreEqual(expectedCount - 1, mockEnvironmentModel.Object.ResourceRepository.All().Count);
        }

        #endregion RemoveResource Tests

        #region Missing Environment Information Tests

        //Create resource repository without connected to any environment
        [TestMethod]
        public void CreateResourceEnvironmentConnectionNotConnected()
        {
            //Arrange
            Setup();
            _environmentConnection.Setup(envConn => envConn.IsConnected).Returns(false);
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(false);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_resourceModel.Object);
            try
            {
                _repo.Load();
            }
            //Assert
            catch(InvalidOperationException iex)
            {
                Assert.AreEqual("No connected environment found to perform operation on.", iex.Message);
            }
        }
        //Create resource with no address to connet to any environment
        [TestMethod]
        public void CreateResourceNoAddressEnvironmentConnection()
        {
            //Arrange
            Setup();
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.DataChannel).Returns(_dataChannel.Object);
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }
        //Create resource with no data channel connected to
        [TestMethod]
        public void CreateResourceNoDataChannelEnvironmentConnection()
        {
            //Arrange
            Setup();
            Mock<IEnvironmentConnection> environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(prop => prop.AppServerUri).Returns(new Uri("http://localhost:77/dsf"));
            environmentConnection.Setup(prop => prop.IsConnected).Returns(true);

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            _repo.Save(_resourceModel.Object);
            _repo.Load();
            int resources = _repo.All().Count(res => res.ResourceName == "Resource");
            //Assert
            Assert.IsTrue(resources == 1);

        }

        #endregion Missing Environment Information Tests

        #region ReloadResource Tests

        ///// <summary>
        ///// Create resource with source type
        ///// </summary>
        [TestMethod]
        public void ReloadResourcesWhereNothingLoadedExpectNonEmptyList()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest());
            //------------Assert Results-------------------------
            Assert.AreEqual(2, reloadedResources.Count);
        }


        [TestMethod]
        public void ResourceRepositoryReloadResourcesWithValidArgsExpectedSetsProperties()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var serverID = Guid.NewGuid();
            var version = new Version(3, 1, 0, 0);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ServerID=\"{1}\" IsValid=\"true\" Version=\"{2}\"><ErrorMessages>{3}</ErrorMessages></Service></Payload>", _resourceGuid, serverID, version,
                "<ErrorMessage Message=\"MappingChange\" ErrorType=\"Critical\" FixType=\"None\" StackTrace=\"SomethingWentWrong\" />"));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            var reloadedResources = _repo.ReloadResource(_resourceGuid, ResourceType.WorkflowService, new ResourceModelEqualityComparerForTest());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, reloadedResources.Count);
            var resources = _repo.All().ToList();
            var actual = (IContextualResourceModel)resources[0];
            Assert.AreEqual(_resourceGuid, actual.ID);
            Assert.AreEqual(serverID, actual.ServerID);
            Assert.AreEqual(version, actual.Version);
            Assert.AreEqual(true, actual.IsValid);

            Assert.IsNotNull(actual.Errors);
            Assert.AreEqual(1, actual.Errors.Count);
            var error = actual.Errors[0];
            Assert.AreEqual(ErrorType.Critical, error.ErrorType);
            Assert.AreEqual(FixType.None, error.FixType);
            Assert.AreEqual("MappingChange", error.Message);
            Assert.AreEqual("SomethingWentWrong", error.StackTrace);
        }

        #endregion ReloadResource Tests

        #region FindResourcesByID

        [TestMethod]
        public void FindResourcesByID_With_NullParameters_Expected_ReturnsEmptyList()
        {
            var result = ResourceRepository.FindResourcesByID(null, null, ResourceType.Source);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindResourcesByID_With_NonNullParameters_Expected_ReturnsNonEmptyList()
        {
            var servers = new List<string> { EnviromentRepositoryTest.Server1ID };
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var result = ResourceRepository.FindResourcesByID(env.Object, servers, ResourceType.Source);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region FindSourcesByType

        [TestMethod]
        public void FindSourcesByType_With_NullParameters_Expected_ReturnsEmptyList()
        {
            var result = ResourceRepository.FindSourcesByType(null, enSourceType.Dev2Server);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindSourcesByType_With_NonNullParameters_Expected_ReturnsNonEmptyList()
        {
            var env = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var result = ResourceRepository.FindSourcesByType(env.Object, enSourceType.Dev2Server);

            Assert.AreNotEqual(0, result.Count);
        }

        #endregion

        #region Find

        [TestMethod]
        public void FindWithValidFunctionExpectResourceReturned()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var resourceModels = _repo.Find(model => model.ID == newGuid);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, resourceModels.Count);
            Assert.AreEqual(newGuid, resourceModels.ToList()[0].ID);
        }

        [TestMethod]
        public void FindWithNullFunctionExpectNullReturned()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var resourceModels = _repo.Find(null);
            //------------Assert Results-------------------------
            Assert.IsNull(resourceModels);
        }
        #endregion

        #region IsWorkflow
        [TestMethod]
        public void IsWorkflowValidWorkflowExpectTrue()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var isWorkFlow = _repo.IsWorkflow("TestWorkflowService1");
            //------------Assert Results-------------------------
            Assert.IsTrue(isWorkFlow);
        }

        [TestMethod]
        public void IsWorkflowNotValidWorkflowExpectFalse()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.ForceLoad();
            //------------Execute Test---------------------------
            var isWorkFlow = _repo.IsWorkflow("TestWorkflowService");
            //------------Assert Results-------------------------
            Assert.IsFalse(isWorkFlow);
        }
        #endregion

        #region AddEnvironment

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddEnvironment_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            ResourceRepository.AddEnvironment(null, null);
        }

        [TestMethod]
        public void AddEnvironment_With_NonNullParameters_Expected_InvokesExecuteCommandOnTargetEnvironment()
        {
            var testEnv = EnviromentRepositoryTest.CreateMockEnvironment();

            var targetEnv = new Mock<IEnvironmentModel>();
            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);
            ResourceRepository.AddEnvironment(targetEnv.Object, testEnv.Object);

            connection.Verify(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        void DoesNothing()
        {
        }

        #endregion

        #region RemoveEnvironment

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveEnvironment_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            ResourceRepository.RemoveEnvironment(null, null);
        }

        [TestMethod]
        public void RemoveEnvironment_With_NonNullParameters_Expected_InvokesExecuteCommandOnTargetEnvironment()
        {
            var testEnv = EnviromentRepositoryTest.CreateMockEnvironment();

            var targetEnv = new Mock<IEnvironmentModel>();
            var rand = new Random();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            connection.Setup(c => c.IsConnected).Returns(true);
            connection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            targetEnv.Setup(e => e.Connection).Returns(connection.Object);


            ResourceRepository.RemoveEnvironment(targetEnv.Object, testEnv.Object);

            connection.Verify(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        #endregion

        #region BuildUnlimitedPackage

        [TestMethod]
        public void BuildUnlimitedPackageWhereResourceExpectResourceDefinitionInPackage()
        {
            //------------Setup for test--------------------------
            Setup();
            var model = new Mock<IResourceModel>();
            model.Setup(c => c.ResourceName).Returns("TestName");
            const string expectedValueForResourceDefinition = "This is the resource definition";
            model.Setup(c => c.ToServiceDefinition()).Returns(expectedValueForResourceDefinition);
            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            _repo.Save(model.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(model.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition, expectedValueForResourceDefinition);
        }

        [TestMethod]
        public void BuildUnlimitedPackageWhereResourceIsSourceExpectResourceDefinitionInPackage()
        {
            //------------Setup for test--------------------------
            Setup();
            const string ExpectedValueForResourceDefinition = "This is the resource definition";
            var resource = new ResourceModel(_environmentModel.Object);
            resource.ResourceType = ResourceType.Source;
            resource.ServiceDefinition = ExpectedValueForResourceDefinition;
            resource.ResourceName = "TestName";

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(resource);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition, ExpectedValueForResourceDefinition);
        }

        [TestMethod]
        public void BuildUnlimitedPackageWhereResourceIsServiceExpectResourceDefinitionInPackage()
        {
            //------------Setup for test--------------------------
            Setup();
            const string ExpectedValueForResourceDefinition = "This is the resource definition";
            var resource = new ResourceModel(_environmentModel.Object);
            resource.ResourceType = ResourceType.Service;
            resource.ServiceDefinition = ExpectedValueForResourceDefinition;
            resource.ResourceName = "TestName";

            var securityContext = new Mock<IFrameworkSecurityContext>();
            securityContext.Setup(s => s.Roles).Returns(new string[0]);
            _environmentConnection.Setup(connection => connection.SecurityContext).Returns(securityContext.Object);
            var rand = new Random();
            var conn = new Mock<IEnvironmentConnection>();
            conn.Setup(c => c.AppServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}/dsf", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.WebServerUri).Returns(new Uri(string.Format("http://127.0.0.{0}:{1}", rand.Next(1, 100), rand.Next(1, 100))));
            conn.Setup(c => c.IsConnected).Returns(true);
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            //------------Execute Test---------------------------
            var buildUnlimitedPackage = _repo.BuildUnlimitedPackage(resource);
            //------------Assert Results-------------------------
            Assert.IsNotNull(buildUnlimitedPackage.ResourceDefinition);
            StringAssert.Contains(buildUnlimitedPackage.ResourceDefinition, ExpectedValueForResourceDefinition);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildUnlimitedPackageWhereNullResourceExpectException()
        {
            Setup();
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            _repo.BuildUnlimitedPackage(null);
            //------------Assert Results-------------------------
        }

        #endregion

        #region IsLoaded


        #endregion IsLoaded

        #region Constructor
        #endregion

        #region HydrateResourceTest

        [TestMethod]
        public void HydrateResourceHydratesConnectionString()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format(TestResourceStringsTest.ResourcesToHydrate, _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, _repo.All().Count(r => r.ConnectionString == TestResourceStringsTest.ResourceToHydrateConnectionString1));
            Assert.AreEqual(1, _repo.All().Count(r => r.ConnectionString == TestResourceStringsTest.ResourceToHydrateConnectionString2));
        }

        [TestMethod]
        public void HydrateResourceHydratesResourceType()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var newGuid = Guid.NewGuid();
            var guid2 = newGuid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format(TestResourceStringsTest.ResourcesToHydrate, _resourceGuid, guid2));
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();
            var resources = _repo.All().Cast<IContextualResourceModel>();
            var servers = resources.Where(r => r.ServerResourceType == "Server");

            //------------Assert Results-------------------------
            Assert.AreEqual(2, servers.Count());

        }


        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("HydrateResourceModel must hydrate the resource's errors.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceRepositoryHydrateResourceModel_ResourceRepositoryUnitTest_ResourceErrors_Hydrated()
        {
            //------------Setup for test--------------------------
            Setup();
            var conn = SetupConnection();
            var resourceXml = XmlResource.Fetch("ResourceWithErrors").ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(resourceXml);
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);

            //------------Execute Test---------------------------
            _repo.Save(new Mock<IResourceModel>().Object);
            _repo.ForceLoad();
            var resources = _repo.All();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, resources.Count, "HydrateResourceModel failed to load the resource.");

            var resource = resources.First();
            Assert.IsFalse(resource.IsValid, "HydrateResourceModel failed to hydrate IsValid.");
            Assert.AreEqual(2, resource.Errors.Count);

            var err = resource.Errors.FirstOrDefault(e => e.InstanceID == Guid.Parse("edadb62e-83f4-44bf-a260-7639d6b43169"));
            Assert.IsNotNull(err, "Error not hydrated.");
            Assert.AreEqual(ErrorType.Critical, err.ErrorType, "HydrateResourceModel failed to hydrate the ErrorType.");
            Assert.AreEqual(FixType.ReloadMapping, err.FixType, "HydrateResourceModel failed to hydrate the FixType.");
            Assert.AreEqual("Mapping out of date", err.Message, "HydrateResourceModel failed to hydrate the Message.");
            Assert.AreEqual("", err.StackTrace, "HydrateResourceModel failed to hydrate the StackTrace.");
            Assert.AreEqual("<Args><Input>[{\"Name\":\"n1\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false},{\"Name\":\"n2\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false}]</Input><Output>[{\"Name\":\"result\",\"MapsTo\":\"\",\"Value\":\"\",\"IsRecordSet\":false,\"RecordSetName\":\"\",\"IsEvaluated\":false,\"DefaultValue\":\"\",\"IsRequired\":false,\"RawValue\":\"\",\"EmptyToNull\":false}]</Output></Args>", err.FixData, "HydrateResourceModel failed to hydrate the FixData.");
        }

        #endregion

        #region IsInCache

        [TestMethod]
        public void IsInCacheExpectsWhenResourceInCacheReturnsTrue()
        {
            //--------------------------Setup-------------------------------------------
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" ResourceType=\"WorkflowService\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Execute--------------------------------------------------------------
            var isInCache = _repo.IsInCache(guid);
            //--------------------------------------------Assert Results----------------------------------------------------
            Assert.IsTrue(isInCache);
        }

        [TestMethod]
        public void IsInCacheExpectsWhenResourceNotInCacheReturnsFalse()
        {
            //--------------------------Setup-------------------------------------------
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Execute--------------------------------------------------------------
            var isInCache = _repo.IsInCache(Guid.NewGuid());
            //--------------------------------------------Assert Results----------------------------------------------------
            Assert.IsFalse(isInCache);
        }
        #endregion

        #region RemoveFromCache

        [TestMethod]
        public void RemoveFromCacheExpectsWhenResourceInCacheRemovesFromCache()
        {
            //--------------------------Setup-------------------------------------------
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\" ResourceType=\"WorkflowService\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\" ResourceType=\"WorkflowService\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" ResourceType=\"WorkflowService\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Assert Precondtion----------------------------------------------
            var isInCache = _repo.IsInCache(guid);
            Assert.IsTrue(isInCache);
            //--------------------------------------------Execute--------------------------------------------------------------
            _repo.RemoveFromCache(guid);
            //--------------------------------------------Assert Results----------------------------------------------------
            isInCache = _repo.IsInCache(guid);
            Assert.IsFalse(isInCache);
        }

        [TestMethod]
        public void RemoveFromCacheExpectsWhenResourceNotInCacheDoesNothing()
        {
            //--------------------------Setup-------------------------------------------
            Setup();
            var conn = SetupConnection();
            var guid2 = Guid.NewGuid().ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"OriginalDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" XamlDefinition=\"OriginalDefinition\" ID=\"{1}\"></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            _environmentModel.Setup(e => e.Connection).Returns(conn.Object);
            int resources = _repo.All().Count;
            var guid = Guid.NewGuid();
            guid2 = guid.ToString();
            conn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
             .Returns(string.Format("<Payload><Service Name=\"TestWorkflowService1\" XamlDefinition=\"ChangedDefinition\" ID=\"{0}\"></Service><Service Name=\"TestWorkflowService2\" ID=\"{1}\" XamlDefinition=\"ChangedDefinition\" ></Service></Payload>", _resourceGuid, guid2));
            _repo.ForceLoad();
            //--------------------------------------------Assert Precondition-------------------------------------------
            var newGuid = Guid.NewGuid();
            var isInCache = _repo.IsInCache(newGuid);
            Assert.IsFalse(isInCache);
            //--------------------------------------------Execute--------------------------------------------------------------
            _repo.RemoveFromCache(newGuid);
            //--------------------------------------------Assert Results----------------------------------------------------
            isInCache = _repo.IsInCache(newGuid);
            Assert.IsFalse(isInCache);
        }

        #endregion

        #region DeployResource

        // BUG 9703 - 2013.06.21 - TWR - added

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ResourceRepositoryDeployResourceWithNullExpectedThrowsArgumentNullException()
        {
            var repoConn = new Mock<IEnvironmentConnection>();
            repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);

            var repoEnv = new Mock<IEnvironmentModel>();
            repoEnv.Setup(e => e.Connection).Returns(repoConn.Object);

            var repo = new ResourceRepository(repoEnv.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);

            repo.DeployResource(null);
        }

        // BUG 9703 - 2013.06.21 - TWR - added
        [TestMethod]
        public void ResourceRepositoryDeployResourceWithNewResourceExpectedCreatesAndAddsNewResourceWithRepositoryEnvironment()
        {
            var repoConn = new Mock<IEnvironmentConnection>();
            repoConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            repoConn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<XmlData></XmlData>");

            // DO NOT USE Mock EnvironmentModel's - otherwise EnvironmentModel.IEquatable will fail!
            //new Mock<IWizardEngine>().Object
            var repoEnv = new EnvironmentModel(Guid.NewGuid(), repoConn.Object, false);

            var resourceConn = new Mock<IEnvironmentConnection>();
            resourceConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            resourceConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            //, new Mock<IWizardEngine>().Object
            var resourceEnv = new EnvironmentModel(Guid.NewGuid(), resourceConn.Object, false);

            var newResource = new ResourceModel(resourceEnv)
            {
                ID = Guid.NewGuid(),
                Category = "Test",
                ResourceName = "TestResource"
            };

            repoEnv.ResourceRepository.DeployResource(newResource);

            var actual = repoEnv.ResourceRepository.FindSingle(r => r.ID == newResource.ID) as IContextualResourceModel;

            Assert.IsNotNull(actual);
            Assert.AreNotSame(newResource, actual);
            Assert.AreSame(repoEnv, actual.Environment);
        }

        // BUG 9703 - 2013.06.21 - TWR - added
        [TestMethod]
        public void ResourceRepositoryDeployResourceWithExistingResourceExpectedCreatesAndAddsNewResourceWithRepositoryEnvironment()
        {
            var repoConn = new Mock<IEnvironmentConnection>();
            repoConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            repoConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            repoConn.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("<XmlData></XmlData>");

            // DO NOT USE Mock EnvironmentModel's - otherwise EnvironmentModel.IEquatable will fail!
            //, new Mock<IWizardEngine>().Object
            var repoEnv = new EnvironmentModel(Guid.NewGuid(), repoConn.Object, false);

            var resourceConn = new Mock<IEnvironmentConnection>();
            resourceConn.Setup(c => c.ServerEvents).Returns(new EventPublisher());
            resourceConn.Setup(c => c.SecurityContext).Returns(new Mock<IFrameworkSecurityContext>().Object);
            //, new Mock<IWizardEngine>().Object
            var resourceEnv = new EnvironmentModel(Guid.NewGuid(), resourceConn.Object, false);

            var oldResource = new ResourceModel(repoEnv)
            {
                ID = Guid.NewGuid(),
                Category = "Test",
                ResourceName = "TestResource"
            };
            repoEnv.ResourceRepository.Add(oldResource);

            var newResource = new ResourceModel(resourceEnv)
            {
                ID = oldResource.ID,
                Category = "Test",
                ResourceName = oldResource.ResourceName
            };

            repoEnv.ResourceRepository.DeployResource(newResource);

            var actual = repoEnv.ResourceRepository.FindSingle(r => r.ID == newResource.ID) as IContextualResourceModel;

            Assert.IsNotNull(actual);
            Assert.AreNotSame(newResource, actual);
            Assert.AreSame(repoEnv, actual.Environment);
        }

        #endregion

        #region Rename

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename function: Rename is called and connection is expected to be open with correct package to the server")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void
            ResourceRepository_ResourceRepositoryUnitTest_RenameResource_ExecuteCommandExecutesTheRightXmlPayload()
            // ReSharper restore InconsistentNaming
        {
            //init
            var resID = Guid.NewGuid();
            var newResName = "New Test Name";
            var mockEnvironment = new Mock<IEnvironmentModel>();
            mockEnvironment.Setup(c => c.Connection.SecurityContext);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            var expected = @"<XmlData>
  <Service>RenameResourceService</Service>
  <NewName>" + newResName + @"</NewName>
  <ResourceID>" + resID + @"</ResourceID>
</XmlData>";
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(expected, It.IsAny<Guid>(), It.IsAny<Guid>()))
                                     .Returns(string.Format("<XmlData>Renamed Resource</XmlData>")).Verifiable();
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            var vm = new ResourceRepository(mockEnvironment.Object, new Mock<IWizardEngine>().Object,
                                            new Mock<IFrameworkSecurityContext>().Object);
            var resourceModel = new Mock<IResourceModel>();
            resourceModel.Setup(res => res.ID).Returns(resID);
            string actualRenamedValue = null;
            resourceModel.SetupSet(res => res.ResourceName).Callback(value =>
                {
                    actualRenamedValue = value;
                });
            vm.Add(resourceModel.Object);

            //exe
            vm.Rename(resID.ToString(), newResName);

            //assert
            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(expected, It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            Assert.IsNotNull(actualRenamedValue, "Resource not renamed locally");
            Assert.AreEqual(newResName, actualRenamedValue);
        }

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename category function")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_ResourceRepositoryUnitTest_RenameCategory_ExecuteCommandIsCalledOnce()
        // ReSharper restore InconsistentNaming
        {
            //MEF!!!
            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            mockEnvironment.Setup(c => c.Connection.SecurityContext);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>{0}</XmlData>", string.Join("\n", new { })));
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            var vm = new ResourceRepository(mockEnvironment.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);
            vm.RenameCategory("Test Category", "New Test Category", ResourceType.WorkflowService);

            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("ResourceRepositoryUnitTest")]
        [Description("Test for ResourceRepository's rename function: Rename is called and connection is expected to be executed with correct package")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ResourceRepository_RenameResource_DashesInTheName_ExecuteCommandExecutesTheRightXmlPayload()
        // ReSharper restore InconsistentNaming
        {
            var resourceID = Guid.NewGuid().ToString();
            var expected = @"<XmlData>
  <Service>RenameResourceService</Service>
  <NewName>New-Test-Name</NewName>
  <ResourceID>"+resourceID+@"</ResourceID>
</XmlData>";
            //init conn
            var mockEnvironment = new Mock<IEnvironmentModel>();
            mockEnvironment.Setup(c => c.Connection.SecurityContext);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(c => c.ExecuteCommand(expected, It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(string.Format("<XmlData>\n</XmlData>"));
            mockEnvironment.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);

            //init repo
            var repo = new ResourceRepository(mockEnvironment.Object, new Mock<IWizardEngine>().Object, new Mock<IFrameworkSecurityContext>().Object);

            //exe rename
            repo.Rename(resourceID, "New-Test-Name");

            //assert correct command sent to server
            mockEnvironmentConnection.Verify(connection => connection.ExecuteCommand(expected, It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        #endregion

        #region Helper Method

        private ResourceRepository GetResourceRepository()
        {
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockWizEngine = new Mock<IWizardEngine>();
            var mockSecurityCtx = new Mock<IFrameworkSecurityContext>();
            return new ResourceRepository(mockEnvironmentModel.Object, mockWizEngine.Object, mockSecurityCtx.Object);
        }

        string GetTestData()
        {
            return @"<Service ID=""b5bbbd9c-fe5f-47da-874a-285cdc9b03db"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""First Name Validation"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>First Name Validation</DisplayName>
  <Category>TU_BENCHMARK_DATA_RULES</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles></AuthorRoles>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath>@@@@</IconPath>
  <HelpLink>http://warewolf.io/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <TotalRecords Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <idx Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <RSAID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <FirstNames Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    <ImportData Description="""" IsEditable=""True"" ColumnIODirection=""Input"">
      <ID Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <FirstName1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <FirstName2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <FirstName3 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    </ImportData>
    <ValidationResult Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <Msg Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
      <ID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
      <RSAID Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </ValidationResult>
    <Validation Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <Msg Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
      <RSAID Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </Validation>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""First Name Validation"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=System"" xmlns:sco1=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;1226,802&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco1:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco1:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""First Name Validation"" sap:VirtualizedContainerService.HintSize=""1186,762""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;x:Double x:Key=""Width""&gt;1172.5&lt;/x:Double&gt;&lt;x:Double x:Key=""Height""&gt;725.5&lt;/x:Double&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,107.5 590.5,107.5 590.5,196&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID7&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID7""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;490.5,196&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;200,90&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;690.5,241 808.5,241&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfCountRecordsetActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CountNumber=""[[TotalRecords]]"" DatabindRecursive=""False"" DisplayName=""Count Records"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,90"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" RecordsetName=""[[ImportData()]]"" SimulationMode=""OnDemand"" UniqueID=""03152e28-0ce1-44db-bcfb-8fc7fee6466b""&gt;&lt;uaba:DsfCountRecordsetActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfCountRecordsetActivity.AmbientDataList&gt;&lt;uaba:DsfCountRecordsetActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfCountRecordsetActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfCountRecordsetActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID2""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;808.5,196.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;990.5,285.5 990.5,316.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a6160c77-3c0e-4601-9226-fcc1dec75eb8"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[idx]]"" FieldValue=""2"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;808.5,316.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;990.5,405.5 990.5,459&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""d9bc5ae0-9973-4697-8013-11f9ddda05aa"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[FirstNames]]"" FieldValue=""[[ImportData([[idx]]).FirstName1]] [[ImportData([[idx]]).FirstName2]] [[ImportData([[idx]]).FirstName3]]"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName=""[[RSAID]]"" FieldValue=""[[ImportData([[idx]]).ID]]"" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""3"" Inserted=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID3""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;865.5,459&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;250,84&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;990.5,543 990.5,637.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfActivity ActionName=""{x:Null}"" ActivityStateData=""{x:Null}"" AuthorRoles=""{x:Null}"" Category=""{x:Null}"" Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" DataTags=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ResultValidationExpression=""{x:Null}"" ResultValidationRequiredTags=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceUri=""{x:Null}"" SimulationOutput=""{x:Null}"" Tags=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DeferExecution=""False"" DisplayName=""Name Validation"" EnvironmentID=""00000000-0000-0000-0000-000000000000"" FriendlySourceName=""localhost"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""250,84"" IconPath=""[Nothing]"" InputMapping=""&amp;lt;Inputs&amp;gt;&amp;lt;Input Name=&amp;quot;Name&amp;quot; Source=&amp;quot;[[FirstNames]]&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;TypeCheck&amp;quot; Source=&amp;quot;First Names&amp;quot; /&amp;gt;&amp;lt;Input Name=&amp;quot;RSAID&amp;quot; Source=&amp;quot;[[RSAID]]&amp;quot; /&amp;gt;&amp;lt;/Inputs&amp;gt;"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""True"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputMapping=""&amp;lt;Outputs&amp;gt;&amp;lt;Output Name=&amp;quot;Msg&amp;quot; MapsTo=&amp;quot;Msg&amp;quot; Value=&amp;quot;[[ValidationResult(*).Msg]]&amp;quot; Recordset=&amp;quot;Validation&amp;quot; /&amp;gt;&amp;lt;Output Name=&amp;quot;ID&amp;quot; MapsTo=&amp;quot;ID&amp;quot; Value=&amp;quot;[[ValidationResult(*).ID]]&amp;quot; Recordset=&amp;quot;Validation&amp;quot; /&amp;gt;&amp;lt;/Outputs&amp;gt;"" RemoveInputFromOutput=""False"" ResourceID=""ff6d0864-e8ac-4b67-bb82-aba65037b3ba"" ServiceName=""Name Validation"" ServiceServer=""00000000-0000-0000-0000-000000000000"" SimulationMode=""OnDemand"" ToolboxFriendlyName=""Name Validation"" Type=""Workflow"" UniqueID=""fb86530e-cbdf-496c-bd8c-8801310536f5""&gt;&lt;uaba:DsfActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfActivity.AmbientDataList&gt;&lt;uaba:DsfActivity.HelpLink&gt;&lt;InArgument x:TypeArguments=""x:String""&gt;&lt;Literal x:TypeArguments=""x:String"" Value="""" /&gt;&lt;/InArgument&gt;&lt;/uaba:DsfActivity.HelpLink&gt;&lt;uaba:DsfActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowDecision x:Name=""__ReferenceID1"" DisplayName=""If [[idx]] Is Less Than or Equal [[TotalRecords]]"" sap:VirtualizedContainerService.HintSize=""160,87""&gt;&lt;FlowDecision.Condition&gt;&lt;uaba:DsfFlowDecisionActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Decision"" ExpressionText=""Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(&amp;quot;{!TheStack!:[{!Col1!:![[idx]]!,!Col2!:![[TotalRecords]]!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsLessThanOrEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!,!DisplayText!:!If [[idx]] Is Less Than or Equal [[TotalRecords]]!}&amp;quot;,AmbientDataList)"" HasError=""[HasError]"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""6439f233-76fc-4ac4-88c9-1ed8990d4eac""&gt;&lt;uaba:DsfFlowDecisionActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfFlowDecisionActivity.AmbientDataList&gt;&lt;uaba:DsfFlowDecisionActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfFlowDecisionActivity.ParentInstanceID&gt;&lt;/uaba:DsfFlowDecisionActivity&gt;&lt;/FlowDecision.Condition&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;910.5,637.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;160,87&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""TrueConnector""&gt;910.5,681 842.5,681&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;FlowDecision.True&gt;&lt;FlowStep x:Name=""__ReferenceID5""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;478.5,636.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;478.5,681 448.5,681 448.5,611 400.5,611&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""9dcc476e-737a-4a2a-b5c2-1c3cd4ec101b"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[Validation().Msg]]"" FieldValue=""[[ValidationResult(*).Msg]]"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName=""[[Validation().RSAID]]"" FieldValue=""[[ValidationResult(*).RSAID]]"" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""3"" Inserted=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID6""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;200.5,566&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;200,90&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300.5,566 300.5,525.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDeleteRecordActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Delete Record"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,90"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" RecordsetName=""[[Validation(*)]]"" Result="""" SimulationMode=""OnDemand"" UniqueID=""d08ee008-1b26-4202-8dae-ec1e67c952f1""&gt;&lt;uaba:DsfDeleteRecordActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDeleteRecordActivity.AmbientDataList&gt;&lt;uaba:DsfDeleteRecordActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDeleteRecordActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDeleteRecordActivity&gt;&lt;FlowStep.Next&gt;&lt;FlowStep x:Name=""__ReferenceID4""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;118.5,436.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;364,89&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;482.5,481 512.5,481 512.5,361 808.5,361&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""364,89"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""fab6831a-0067-44e8-adb4-aed544be9d69"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;sco:ObservableCollection x:TypeArguments=""uaba:ActivityDTO""&gt;&lt;uaba:ActivityDTO FieldName=""[[idx]]"" FieldValue=""!~calculation~![[idx]]+1!~~calculation~!"" IndexNumber=""1"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/sco:ObservableCollection&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowDecision.True&gt;&lt;/FlowDecision&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID2&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID3&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID4&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID5&lt;/x:Reference&gt;&lt;x:Reference&gt;__ReferenceID6&lt;/x:Reference&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <BizRule />
  <WorkflowActivityDef />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>rfR9Ry6mHVMie+vSkOi0pJlq7jM=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>gTC12dhYhIgcIdYVoUgXvii8S/s46txocwldoFqoLPNab0ryiU1xGOgwWbB3jG1BUpSGg1ETh50GGKvsfA9a6nB7NeKB6Oh9uLXYyBkf9t7AC8IiCeJuGjmh8I/twEflvsJ7OMDmyE+T7TWxiEelCrRiN/iwRz1W7QBtOLNBG+Y=</SignatureValue>
  </Signature>
</Service>";
        }

        #endregion

        static Mock<IEnvironmentConnection> CreateEnvironmentConnection()
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(new EventPublisher());
            return connection;
        }
    }

    public class ResourceModelEqualityComparerForTest : IEqualityComparer<IResourceModel>
    {

        public bool Equals(IResourceModel x, IResourceModel y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(IResourceModel obj)
        {
            return obj.GetHashCode();
        }
    }
}
