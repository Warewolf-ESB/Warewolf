using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Tests.Runtime.XML;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DynamicServicesInvokerTest
    {
        static readonly Guid TestWorkspaceID = new Guid("B1890C86-95D8-4612-A7C3-953250ED237A");

        static readonly XElement TestWorkspaceItemXml = XmlResource.Fetch("WorkspaceItem");

        const int VersionNo = 9999;

        const string ServiceName = "Calculate_RecordSet_Subtract";

        const string ServiceNameUnsigned = "TestDecisionUnsigned";

        const string SourceName = "CitiesDatabase";

        readonly Guid SourceID = Guid.NewGuid();

        readonly Guid ServiceID = Guid.NewGuid();

        readonly Guid UnsignedServiceID = Guid.NewGuid();

        public const string ServerConnection1Name = "ServerConnection1";

        public const string ServerConnection1ResourceName = "MyDevServer";

        public const string ServerConnection1ID = "68F5B4FE-4573-442A-BA0C-5303F828344F";

        public const string ServerConnection2Name = "ServerConnection2";

        public const string ServerConnection2ResourceName = "MySecondDevServer";

        public const string ServerConnection2ID = "70238921-FDC7-4F7A-9651-3104EEDA1211";

        Guid _workspaceID;

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            _workspaceID = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(CultureInfo.InvariantCulture), true, false,
                new[] { SourceName, ServerConnection1Name, ServerConnection2Name },
                new[] { ServiceName, ServiceNameUnsigned },
                out resources,
                new[] { SourceID, Guid.Parse(ServerConnection1ID), Guid.Parse(ServerConnection2ID) },
                new[] { ServiceID, UnsignedServiceID });

            ResourceCatalog.Instance.LoadWorkspace(_workspaceID);
        }

        #endregion

        #region UpdateWorkspaceItem

        [TestMethod]
        public void UpdateWorkspaceItemWithNull()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder();
            data["Roles"] = new StringBuilder();

            var result = endpoint.Execute(data, workspace.Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.IsTrue(obj.Message.Contains("Invalid workspace item definition"));
            Assert.IsTrue(obj.HasError);
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithInvalidItemXml()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder("<xxxx/>");
            data["Roles"] = null;

            var result = endpoint.Execute(data, workspace.Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.IsTrue(obj.Message.Contains("Error updating workspace item"));
            Assert.IsTrue(obj.HasError);
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithItemXmlFromAnotherWorkspace()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            var workspaceItem = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.Empty);
            var itemXml = workspaceItem.ToXml().ToString();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder(itemXml);
            data["Roles"] = new StringBuilder();

            var result = endpoint.Execute(data, workspace.Object);

            var obj = ConvertToMsg(result.ToString());

            Assert.IsTrue(obj.Message.Contains("Cannot update a workspace item from another workspace"));
            Assert.IsTrue(obj.HasError);

        }

        [TestMethod]
        public void UpdateWorkspaceItemWithValidItemXml()
        {
            var workspaceItem = new WorkspaceItem(TestWorkspaceItemXml);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>())).Verifiable();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ItemXml"] = new StringBuilder(TestWorkspaceItemXml.ToString());

            endpoint.Execute(data, workspace.Object);

            workspace.Verify(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>()), Times.Exactly(1));

        }

        #endregion UpdateWorkspaceItem

        #region FindResourcesByID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindResourcesByID_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindResourcesByID();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["GuidCsv"] = null;
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }

        [TestMethod]
        public void FindResourcesByID_With_EmptyServerGuid_Expected_FindsZeroServers()
        {
            FindResourcesByID(0);
        }

        [TestMethod]
        public void FindResourcesByID_With_InvalidServerGuids_Expected_FindsZeroServers()
        {
            FindResourcesByID(0, Guid.NewGuid().ToString(), "xxx");
        }

        [TestMethod]
        public void FindResourcesByID_With_OneValidServerGuidAndOneInvalidServerGuiD_Expected_FindsOneServer()
        {
            FindResourcesByID(1, ServerConnection1ID, Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void FindResourcesByID_With_TwoValidServerGuidAndOneInvalidServerGuiD_Expected_FindsTwoServers()
        {
            FindResourcesByID(2, ServerConnection1ID, ServerConnection2ID, Guid.NewGuid().ToString());
        }

        void FindResourcesByID(int expectedCount, params string[] guids)
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint findResourcesEndPoint = new FindResourcesByID();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["GuidCsv"] = new StringBuilder(string.Join(",", guids));
            data["ResourceType"] = new StringBuilder("Source");

            var resources = findResourcesEndPoint.Execute(data, workspace.Object);

            var resourcesObj = JsonConvert.DeserializeObject<List<Resource>>(resources.ToString());

            var actualCount = 0;
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(var res in resourcesObj)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                if(res.ResourceType == ResourceType.DbSource || res.ResourceType == ResourceType.PluginSource ||
                    res.ResourceType == ResourceType.WebSource || res.ResourceType == ResourceType.EmailSource || res.ResourceType == ResourceType.Server)
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

        #region FetchResourceDefinition

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenDefintionExist_ResourceDefinition()
        {

            //------------Setup for test--------------------------

            #region Expected
            const string expected = @"<Activity mc:Ignorable=""sap sads"" x:Class=""Calculate_RecordSet_Subtract"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">  <x:Members>    <x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" />    <x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" />    <x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" />  </x:Members>  <sap:VirtualizedContainerService.HintSize>870,839</sap:VirtualizedContainerService.HintSize>  <mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings>  <Flowchart DisplayName=""Calculate_RecordSet_Subtract"" sap:VirtualizedContainerService.HintSize=""830,799"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces"">    <Flowchart.Variables>      <Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" />      <Variable x:TypeArguments=""x:String"" Name=""LastResult"" />      <Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" />      <Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" />      <Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" />      <Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />      <Variable x:TypeArguments=""uaba:Util"" Name=""t"" />      <Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" />    </Flowchart.Variables>    <sap:WorkflowViewStateService.ViewState>      <scg:Dictionary x:TypeArguments=""x:String, x:Object"">        <x:Boolean x:Key=""IsExpanded"">False</x:Boolean>        <av:Point x:Key=""ShapeLocation"">270,2.5</av:Point>        <av:Size x:Key=""ShapeSize"">60,75</av:Size>        <av:PointCollection x:Key=""ConnectorLocation"">270,40 162.5,40 162.5,152</av:PointCollection>        <x:Double x:Key=""Width"">816</x:Double>        <x:Double x:Key=""Height"">763</x:Double>      </scg:Dictionary>    </sap:WorkflowViewStateService.ViewState>    <Flowchart.StartNode>      <FlowStep x:Name=""__ReferenceID0"">        <sap:WorkflowViewStateService.ViewState>          <scg:Dictionary x:TypeArguments=""x:String, x:Object"">            <av:Point x:Key=""ShapeLocation"">23.5,152</av:Point>            <av:Size x:Key=""ShapeSize"">278,88</av:Size>            <av:PointCollection x:Key=""ConnectorLocation"">301.5,196 342,196 342,407</av:PointCollection>          </scg:Dictionary>        </sap:WorkflowViewStateService.ViewState>        <uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (4)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""278,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a6079387-2e23-4ad5-b00d-559ef4b81f68"" UpdateAllOccurrences=""False"">          <uaba:DsfMultiAssignActivity.FieldsCollection>            <scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8"">              <uaba:ActivityDTO FieldName=""[[Employees(1).Name]]"" FieldValue=""Sashen"" IndexNumber=""1"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]"">                <uaba:ActivityDTO.OutList>                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />                </uaba:ActivityDTO.OutList>              </uaba:ActivityDTO>              <uaba:ActivityDTO FieldName=""[[Employees(1).Funds]]"" FieldValue=""1234"" IndexNumber=""2"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]"">                <uaba:ActivityDTO.OutList>                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />                </uaba:ActivityDTO.OutList>              </uaba:ActivityDTO>              <uaba:ActivityDTO FieldName=""[[Employees(2).Name]]"" FieldValue=""Ninja"" IndexNumber=""3"" WatermarkTextValue="""" WatermarkTextVariable="""">                <uaba:ActivityDTO.OutList>                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />                </uaba:ActivityDTO.OutList>              </uaba:ActivityDTO>              <uaba:ActivityDTO FieldName=""[[Employees(2).Funds]]"" FieldValue=""2000000"" IndexNumber=""4"" WatermarkTextValue="""" WatermarkTextVariable="""">                <uaba:ActivityDTO.OutList>                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />                </uaba:ActivityDTO.OutList>              </uaba:ActivityDTO>              <uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""5"" WatermarkTextValue="""" WatermarkTextVariable="""">                <uaba:ActivityDTO.OutList>                  <scg:List x:TypeArguments=""x:String"" Capacity=""0"" />                </uaba:ActivityDTO.OutList>              </uaba:ActivityDTO>            </scg:List>          </uaba:DsfMultiAssignActivity.FieldsCollection>          <uaba:DsfMultiAssignActivity.ParentInstanceID>            <InOutArgument x:TypeArguments=""x:String"" />          </uaba:DsfMultiAssignActivity.ParentInstanceID>        </uaba:DsfMultiAssignActivity>        <FlowStep.Next>          <FlowStep x:Name=""__ReferenceID1"">            <sap:WorkflowViewStateService.ViewState>              <scg:Dictionary x:TypeArguments=""x:String, x:Object"">                <av:Point x:Key=""ShapeLocation"">227,407</av:Point>                <av:Size x:Key=""ShapeSize"">230,106</av:Size>              </scg:Dictionary>            </sap:WorkflowViewStateService.ViewState>            <uaba:DsfCalculateActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""DsfCalculateActivity"" Expression=""mod([[Employees(2).Funds]],[[Employees(1).Funds]])"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,106"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[result]]"" SimulationMode=""OnDemand"" UniqueID=""4b5ab147-ceaf-4686-b353-6e82e6c0a651"">              <uaba:DsfCalculateActivity.ParentInstanceID>                <InOutArgument x:TypeArguments=""x:String"" />              </uaba:DsfCalculateActivity.ParentInstanceID>            </uaba:DsfCalculateActivity>          </FlowStep>        </FlowStep.Next>      </FlowStep>    </Flowchart.StartNode>    <x:Reference>__ReferenceID0</x:Reference>    <x:Reference>__ReferenceID1</x:Reference>    <FlowStep>      <sap:WorkflowViewStateService.ViewState>        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">          <av:Point x:Key=""ShapeLocation"">519,243</av:Point>          <av:Size x:Key=""ShapeSize"">256,80</av:Size>        </scg:Dictionary>      </sap:WorkflowViewStateService.ViewState>      <uaba:DsfCommentActivity DisplayName=""Input"" sap:VirtualizedContainerService.HintSize=""256,80"" Text=""Employees(2).Funds finding the remainder&#xA;when divided by Employees(1).Funds"" />    </FlowStep>    <FlowStep>      <sap:WorkflowViewStateService.ViewState>        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">          <av:Point x:Key=""ShapeLocation"">500,3</av:Point>          <av:Size x:Key=""ShapeSize"">278,80</av:Size>        </scg:Dictionary>      </sap:WorkflowViewStateService.ViewState>      <uaba:DsfCommentActivity DisplayName=""Description"" sap:VirtualizedContainerService.HintSize=""278,80"" Text=""This will find the modulus of a record set value&#xA;when divided by another recordset value"" />    </FlowStep>    <FlowStep>      <sap:WorkflowViewStateService.ViewState>        <scg:Dictionary x:TypeArguments=""x:String, x:Object"">          <av:Point x:Key=""ShapeLocation"">580,503</av:Point>          <av:Size x:Key=""ShapeSize"">202,260</av:Size>        </scg:Dictionary>      </sap:WorkflowViewStateService.ViewState>      <uaba:DsfCommentActivity DisplayName=""Expected"" sap:VirtualizedContainerService.HintSize=""202,260"" Text=""&lt;Dev2XMLResult&gt;&#xA;&lt;ADL&gt;&#xA;&lt;Employees&gt;&#xA;&lt;Funds&gt;1234&lt;/Funds&gt;&#xA;&lt;Name&gt;Sashen&lt;/Name&gt;&#xA;&lt;/Employees&gt;&#xA;&lt;Employees&gt;&#xA;&lt;Funds&gt;2000000&lt;/Funds&gt;&#xA;&lt;Name&gt;Ninja&lt;/Name&gt;&#xA;&lt;/Employees&gt;&#xA;&lt;result&gt;920&lt;/result&gt;&#xA;&lt;/ADL&gt;&#xA;&lt;JSON/&gt;&#xA;&lt;/Dev2XMLResult&gt;"" />    </FlowStep>  </Flowchart></Activity>";
            #endregion

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();

            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder("b2b0cc87-32ba-4504-8046-79edfb18d5fd");
            //data["ResourceType"] = new StringBuilder();

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------

            var obj = ConvertToMsg(xaml.ToString());

            Assert.AreEqual(expected, obj.Message.ToString());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenSourceDefintionExist_ResourceDefinition()
        {

            //------------Setup for test--------------------------

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            var sourceXML = XmlResource.Fetch(SourceName);
            var nameAttribute = sourceXML.Attribute("Name");
            var serverIDAttribute = sourceXML.Attribute("ServerID");
            ResourceCatalog.Instance.SaveResource(_workspaceID, sourceXML.ToStringBuilder());
            var resource = ResourceCatalog.Instance.GetResource(_workspaceID, SourceName);
            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();

            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder(resource.ResourceID.ToString());

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            var obj = ConvertToMsg(xaml.ToString());
            var actual = obj.Message.ToString();
            Assert.IsFalse(String.IsNullOrWhiteSpace(actual));
            Assert.IsNotNull(nameAttribute);
            StringAssert.Contains(actual, nameAttribute.ToString());
            StringAssert.Contains(actual, serverIDAttribute.ToString());
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenDefintionDoesNotExist_ExpectNothing()
        {
            //------------Setup for test--------------------------
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["ResourceID"] = new StringBuilder(Guid.NewGuid().ToString());

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            var obj = ConvertToMsg(xaml.ToString());
            Assert.AreEqual(string.Empty, obj.Message.ToString());
        }

        #endregion

        #region FindSourcesByType

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindSourcesByType_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            Dictionary<string, StringBuilder> data = new Dictionary<string, StringBuilder>();
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }


        #endregion

        #region Invoke

        // BUG 9706 - 2013.06.22 - TWR : added
        [TestMethod]
        public void DynamicServicesInvokerInvokeWithErrorsExpectedInvokesDebugDispatcherBeforeAndAfterExecution()
        {
            const string PreErrorMessage = "There was an pre error.";

            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                "<DataList><Prefix>an</Prefix><Dev2System.Dev2Error>" + PreErrorMessage + "</Dev2System.Dev2Error></DataList>",
                "<ADL><Prefix></Prefix><Countries><CountryID></CountryID><CountryName></CountryName></Countries></ADL>", out errors);

            var workspaceID = Guid.NewGuid();
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(w => w.ID).Returns(workspaceID);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.WorkspaceID).Returns(workspaceID);
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.IsDebug).Returns(true);

            var actualStates = new List<IDebugState>();

            var debugWriter = new Mock<IDebugWriter>();
            debugWriter.Setup(w => w.Write(It.IsAny<IDebugState>())).Callback<IDebugState>(actualStates.Add).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, debugWriter.Object);

            var dsi = new DynamicServicesInvoker(new Mock<IEsbChannel>().Object, null, workspace.Object);
            dsi.Invoke(dataObj.Object, out errors);

            Thread.Sleep(3000);  // wait for DebugDispatcher Write Queue 

            // Clean up
            DebugDispatcher.Instance.Remove(workspaceID);

            // Will get called twice once for pre and once for post
            debugWriter.Verify(w => w.Write(It.IsAny<IDebugState>()), Times.Exactly(2));

            for(var i = 0; i < actualStates.Count; i++)
            {
                Assert.IsNotNull(actualStates[i]);
                Assert.IsTrue(actualStates[i].HasError);
                Assert.AreEqual(ActivityType.Workflow, actualStates[i].ActivityType);
                switch(i)
                {
                    case 0:
                        Assert.AreEqual(PreErrorMessage, actualStates[i].ErrorMessage);
                        Assert.AreEqual(StateType.Before, actualStates[i].StateType);
                        break;
                    case 1:
                        Assert.AreEqual("Error: Service was not specified", actualStates[i].ErrorMessage);
                        Assert.AreEqual(StateType.End, actualStates[i].StateType);
                        break;
                    default:
                        Assert.Fail("Too many DebugDispatcher.Write invocations");
                        break;
                }
            }
        }

        #endregion

        private ExecuteMessage ConvertToMsg(string payload)
        {
            return JsonConvert.DeserializeObject<ExecuteMessage>(payload);
        }

    }
}
