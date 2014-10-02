
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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetVersionTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetVersions_HandlesType")]
// ReSharper disable InconsistentNaming
        public void GetVersions_HandlesType_ExpectName()

        {
            //------------Setup for test--------------------------
            var getVersions = new GetVersion();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("GetVersion", getVersions.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetVersions_Execute_InvalidDictionary")]
        public void GetVersions_Execute_ExpectException()
        {
            //------------Setup for test--------------------------
            var getVersions = new GetVersion();
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var ax = getVersions.Execute(null, new Workspace(Guid.NewGuid()));
            Assert.AreEqual(ExecStatus.Fail, serialiser.Deserialize<ExplorerRepositoryResult>(ax).Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetVersions_Execute_InvalidDictionary")]
        public void GetVersions_Execute_ExpectException_NoArgs()
        {
            //------------Setup for test--------------------------
            var getVersions = new GetVersion();
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var ax = getVersions.Execute(new Dictionary<string, StringBuilder>(), new Workspace(Guid.NewGuid()));
            Assert.AreEqual(ExecStatus.Fail, serialiser.Deserialize<ExplorerRepositoryResult>(ax).Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetVersions_Execute_InvalidDictionary")]
        public void GetVersions_Execute_ExpectSuccess_DbType()
        {
            //------------Setup for test--------------------------
            var getVersions = new GetVersion();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            //var wsId = Guid.NewGuid();
            var ws = new Mock<IWorkspace>();
            VersionInfo version = new VersionInfo(DateTime.Now,"bob","dave","2",resourceId,versionId);
            var mockRes = new Mock<IResource>();
            var servVer = new Mock<IServerVersionRepository>();
            servVer.Setup(a => a.GetVersion(It.IsAny<VersionInfo>())).Returns(new StringBuilder(resourceOne));
            mockRes.Setup(a => a.ResourceType).Returns(ResourceType.DbSource);

            var cat = new Mock<IResourceCatalog>();
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockRes.Object);
            var serializer = new Dev2JsonSerializer();
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            getVersions.ServerVersionRepo = servVer.Object;
            getVersions.Resources = cat.Object;
            Dev2JsonSerializer serialisr = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var ax = getVersions.Execute(new Dictionary<string, StringBuilder> { { "versionInfo", serialisr.SerializeToBuilder(version) } }, ws.Object);

            //------------Assert Results-------------------------
            var ver = serializer.Deserialize<ExecuteMessage>(ax.ToString());
            Assert.AreEqual(ver.Message.ToString(),resourceOne);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetVersions_Execute_InvalidDictionary")]
        public void GetVersions_Execute_ExpectSuccess_NonDbType()
        {
            //------------Setup for test--------------------------
            var getVersions = new GetVersion();
            var resourceId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            //var wsId = Guid.NewGuid();
            var ws = new Mock<IWorkspace>();
            VersionInfo version = new VersionInfo(DateTime.Now, "bob", "dave", "2", resourceId, versionId);
            var mockRes = new Mock<IResource>();
            var servVer = new Mock<IServerVersionRepository>();
            servVer.Setup(a => a.GetVersion(It.IsAny<VersionInfo>())).Returns(new StringBuilder(resourceOne));
            mockRes.Setup(a => a.ResourceType).Returns(ResourceType.WorkflowService);

            var cat = new Mock<IResourceCatalog>();
            cat.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockRes.Object);
            var serializer = new Dev2JsonSerializer();
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            getVersions.ServerVersionRepo = servVer.Object;
            getVersions.Resources = cat.Object;
            Dev2JsonSerializer serialisr = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var ax = getVersions.Execute(new Dictionary<string, StringBuilder> { { "versionInfo", serialisr.SerializeToBuilder(version) } }, ws.Object);

            //------------Assert Results-------------------------
            var ver = serializer.Deserialize<ExecuteMessage>(ax.ToString());
            Assert.AreEqual(ver.Message.ToString(), ExpectedModifiedVersion);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetVersions_HandlesType")]
        public void GetVersions_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var getVersion = new GetVersion();


            //------------Execute Test---------------------------
            var a = getVersion.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification;
            Assert.AreEqual(@"<DataList><ResourceID ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>", b);
        }

        const string resourceOne = @"<Service ID=""fef087f1-18ba-406d-a9da-44b6aa2dd1bf"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""UnitTestResource"" ResourceType=""WorkflowService"" IsValid=""false"">
  <DisplayName>UnitTestResource</DisplayName>
  <Category>UnitTestResource</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>
  </AuthorRoles>
  <Comment>
  </Comment>
  <Tags>
  </Tags>
  <IconPath>
  </IconPath>
  <HelpLink>
  </HelpLink>
  <UnitTestTargetWorkflowService>
  </UnitTestTargetWorkflowService>
  <DataList>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""UnitTestResource"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""6""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""UnitTestResource"" sap:VirtualizedContainerService.HintSize=""614,636""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Null /&gt;&lt;/Flowchart.StartNode&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <VersionInfo DateTimeStamp=""2014-08-19T11:18:11.5677239+02:00"" Reason=""Save"" User=""Unknown"" VersionNumber=""1"" ResourceId=""fef087f1-18ba-406d-a9da-44b6aa2dd1bf"" VersionId=""ee694a65-37ca-4e0c-9741-a6d39dd5c12a"" />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>AIWhvyy9UufXE9STU6Q0MIV4T+0=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>Sxuo1EtJA0TzLFcLOYK4VC9j3I+/FOMLbal3PtTukuUcwOPp/bP3PSrthsSclpPD3+nWyw+yIDReKTXxiqn67k0CEq4wtETI/YGJlcRiDAenkSvEv51YfAsABwWG9baJw42FEJdOf3oAQnRh1pPYX897+Yr2a1D2L32JwT8gxQ8=</SignatureValue>
  </Signature>
</Service>";

        const string ExpectedModifiedVersion = "<Activity x:Class=\"UnitTestResource\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dc=\"clr-namespace:Dev2.Common;assembly=Dev2.Common\" xmlns:ddc=\"clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data\" xmlns:ddcb=\"clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:dddo=\"clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data\" xmlns:ddsm=\"clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"><x:Members><x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /><x:Property Name=\"ParentWorkflowInstanceId\" Type=\"InOutArgument(s:Guid)\" /><x:Property Name=\"ParentServiceName\" Type=\"InOutArgument(x:String)\" /></x:Members><sap:VirtualizedContainerService.HintSize>654,676</sap:VirtualizedContainerService.HintSize><mva:VisualBasic.Settings>Assembly references and imported namespaces serialized as XML namespaces</mva:VisualBasic.Settings><TextExpression.NamespacesForImplementation><scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"><x:String>Dev2.Common</x:String><x:String>Dev2.Data.Decisions.Operations</x:String><x:String>Dev2.Data.SystemTemplates.Models</x:String><x:String>Dev2.DataList.Contract</x:String><x:String>Dev2.DataList.Contract.Binary_Objects</x:String><x:String>Unlimited.Applications.BusinessDesignStudio.Activities</x:String></scg:List></TextExpression.NamespacesForImplementation><TextExpression.ReferencesForImplementation><sco:Collection x:TypeArguments=\"AssemblyReference\"><AssemblyReference>Dev2.Common</AssemblyReference><AssemblyReference>Dev2.Data</AssemblyReference><AssemblyReference>Dev2.Activities</AssemblyReference></sco:Collection></TextExpression.ReferencesForImplementation><Flowchart DisplayName=\"UnitTestResource\" sap:VirtualizedContainerService.HintSize=\"614,636\"><Flowchart.Variables><Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /><Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /><Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /><Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /><Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /><Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /></Flowchart.Variables><sap:WorkflowViewStateService.ViewState><scg:Dictionary x:TypeArguments=\"x:String, x:Object\"><x:Boolean x:Key=\"IsExpanded\">False</x:Boolean><av:Point x:Key=\"ShapeLocation\">270,2.5</av:Point><av:Size x:Key=\"ShapeSize\">60,75</av:Size></scg:Dictionary></sap:WorkflowViewStateService.ViewState><Flowchart.StartNode><x:Null /></Flowchart.StartNode></Flowchart></Activity>";
    }
}
// ReSharper restore InconsistentNaming
