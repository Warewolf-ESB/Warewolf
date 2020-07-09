/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Security;
using Warewolf.Services;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class TokenRequestHandlerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TokenRequestHandler))]
        [ExpectedException(typeof(NullReferenceException))]
        public void TokenRequestHandler_ProcessRequest_GiveNullCommunicationContext_ThrowsException()
        {
            //------------Setup for test-------------------------
            var handler = new TokenRequestHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TokenRequestHandler))]
        public void TokenRequestHandler_Return_EncryptedUserGroups_Token()
        {
            Dev2.Common.Utilities.ServerUser = new Mock<IPrincipal>().Object;

            var mockWarewolfPerformanceCounterLocater = new Mock<IWarewolfPerformanceCounterLocater>();
            var mockCounter = new Mock<IPerformanceCounter>();
            mockWarewolfPerformanceCounterLocater
                .Setup(o => o.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>()))
                .Returns(mockCounter.Object);
            mockWarewolfPerformanceCounterLocater.Setup(o => o.GetCounter(It.IsAny<string>()))
                .Returns(mockCounter.Object);
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(mockWarewolfPerformanceCounterLocater.Object);
            var resourceId = Guid.NewGuid();
            var resourceName = "loginAuth";
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            principal.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);
            var outerEnv = new ExecutionEnvironment();
            outerEnv.Assign("[[UserGroups().Name]]", "public", 0);
            outerEnv.Assign("[[UserGroups().Name]]", "whatever", 0);
            var payload =
                "<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>";
            var dataObject = new DsfDataObject(payload, Guid.Empty);
            dataObject.Environment = outerEnv;
            dataObject.ServiceName = resourceName;
            dataObject.ExecutingUser = principal.Object;

            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service =>
                    service.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<IResource>()))
                .Returns(true);

            var r = new Workflow(XElement.Parse("<Service ID=\"" + resourceId +
                                                "\" Version=\"1.0\" ServerID=\"18ca645a-b2c2-4d1d-907c-f42cff71a462\" Name=\"" +
                                                resourceName +
                                                "\" ResourceType=\"WorkflowService\" IsValid=\"false\" ServerVersion=\"0.0.0.0\"><DisplayName>LoginOverride1</DisplayName><Category></Category><IsNewWorkflow>false</IsNewWorkflow><AuthorRoles></AuthorRoles><Comment></Comment><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService><DataList><Groups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" /></Groups><UserGroups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></UserGroups></DataList><Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition>&lt;Activity x:Class=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"654,676\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dciipe=\"clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,74.6666666666667&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.1666666666667 300,127.166666666667&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,127.166666666667&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;230,88&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDotNetMultiAssignActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (1)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"230,88\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"ca79a220-acf9-4ebf-b1ca-f76b5a4b30d5\" UpdateAllOccurrences=\"False\"&gt;&lt;uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\"&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"[[UserGroups().Name]]\" FieldValue=\"Public\" IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDotNetMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition></Action><ErrorMessages /><VersionInfo DateTimeStamp=\"2020-05-15T20:22:02.3670157+02:00\" Reason=\"Save\" User=\"T004178\\Rory McGuire\" VersionNumber=\"2\" ResourceId=\"" +
                                                resourceId +
                                                "\" VersionId=\"df405b6c-3b59-4023-acaa-7b16a8183f26\" /></Service>"));

            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(r);
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), resourceName)).Returns(r);
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockCatalog.Setup(o => o.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(mockDev2Activity.Object);
            mockCatalog.Setup(a => a.GetResourcePath(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("bob\\dave");
            mockCatalog.Setup(o => o.GetDynamicObjects<DynamicService>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new[]
                {
                    new DynamicService
                    {
                        Actions = new List<ServiceAction>
                        {
                            new ServiceAction
                            {
                                ActionType = enActionType.Workflow,
                            }
                        }
                    }
                }.ToList());
            CustomContainer.Register(mockCatalog.Object);
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var mockResourceNameProvider = new Mock<IResourceNameProvider>();
            mockResourceNameProvider.Setup(a => a.GetResourceNameById(It.IsAny<Guid>())).Returns(resourceName);
            CustomContainer.Register(mockResourceNameProvider.Object);

            var doFactory = new TestTokenRequestDataObjectFactory(dataObject);
            var mockEsbChannelFactory = new Mock<IEsbChannelFactory>();
            var esbChannel = new Mock<IEsbChannel>();
            esbChannel.Setup(o =>
                    o.ExecuteRequest(dataObject, It.IsAny<EsbExecuteRequest>(), It.IsAny<Guid>(),
                        out It.Ref<ErrorResultTO>.IsAny))
                .Callback(new ExecuteRequestCallback((IDSFDataObject dobject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errorResultTO) =>
                {
                    // assign fake result as though a workflow has been run
                    errorResultTO = new ErrorResultTO();
                    request.ExecuteResult = new StringBuilder(payload);
                }));

            mockEsbChannelFactory.Setup(o => o.New()).Returns(esbChannel.Object);
            //---------------Assert Precondition----------------
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);

            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO
            {
                AuthenticationOverrideWorkflow = new NamedGuid
                {
                    Name = resourceName,
                    Value = resourceId
                },
                SecretKey = secretKey,
            });
            var securitySettings = mockSecuritySettings.Object;
            var handlerMock = new TokenRequestHandlerMock(mockCatalog.Object, wRepo.Object, authorizationService.Object, doFactory, mockEsbChannelFactory.Object, securitySettings);
            var headers = new Mock<NameValueCollection>();
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = resourceName,
                WebServerUrl = "http://localhost:3142/public/loginAuth"
            };
            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, resourceName, Guid.Empty.ToString(), headers.Object, principal.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(responseWriter);
            var mockMessageContext = new Mock<IResponseMessageContext>();
            var response = new HttpResponseMessage();
            mockMessageContext.Setup(o => o.ResponseMessage).Returns(response);
            responseWriter.Write(mockMessageContext.Object);
            mockMessageContext.Verify(o => o.ResponseMessage, Times.AtLeast(1));
            var responseText = response.Content.ReadAsStringAsync().Result;
            Assert.IsTrue(!string.IsNullOrWhiteSpace(responseText), "expected non empty token");
            var bodyJson = JsonConvert.DeserializeObject<JObject>(responseText);
            Assert.IsNotNull(bodyJson, "expected a json payload");
            var text = new JwtManager(securitySettings).ValidateToken(bodyJson["token"].ToString());

            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "expected valid token that can be decrypted, has the encryption key changed?");
            var json = JsonConvert.DeserializeObject<JObject>(text);
            Assert.IsNotNull(json, "unable to parse response token into JSON");
            var group1 = json["UserGroups"][0]["Name"].ToString();
            var group2 = json["UserGroups"][1]["Name"].ToString();
            var hasBothGroups =
                (group1 == "public" && group2 == "whatever") || (group2 == "public" && group1 == "whatever");
            Assert.IsTrue(hasBothGroups, "groups not found in response token");
            Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType, "application/json media type expected");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [ExpectedException(typeof(HttpResponseException))]
        [TestCategory(nameof(TokenRequestHandler))]
        public void TokenRequestHandler_UserGroup_IsNullOrWhiteSpace_InternalServerError()
        {
            Dev2.Common.Utilities.ServerUser = new Mock<IPrincipal>().Object;

            var mockWarewolfPerformanceCounterLocater = new Mock<IWarewolfPerformanceCounterLocater>();
            var mockCounter = new Mock<IPerformanceCounter>();
            mockWarewolfPerformanceCounterLocater
                .Setup(o => o.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>()))
                .Returns(mockCounter.Object);
            mockWarewolfPerformanceCounterLocater.Setup(o => o.GetCounter(It.IsAny<string>()))
                .Returns(mockCounter.Object);
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(mockWarewolfPerformanceCounterLocater.Object);
            var resourceId = Guid.NewGuid();
            var resourceName = "loginAuth";
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            principal.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);
            var outerEnv = new ExecutionEnvironment();
            outerEnv.Assign("[[UserGroups().Name]]", "", 0);
            var payload =
                "<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>";
            var dataObject = new DsfDataObject(payload, Guid.Empty);
            dataObject.Environment = outerEnv;
            dataObject.ServiceName = resourceName;
            dataObject.ExecutingUser = principal.Object;

            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service =>
                    service.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<IResource>()))
                .Returns(true);
            // var mockResource = new Mock<IResource>();
            // mockResource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            // mockResource.Setup(o => o.ResourceName).Returns(resourceName);
            // mockResource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>"));

            var r = new Workflow(XElement.Parse("<Service ID=\"" + resourceId +
                                                "\" Version=\"1.0\" ServerID=\"18ca645a-b2c2-4d1d-907c-f42cff71a462\" Name=\"" +
                                                resourceName +
                                                "\" ResourceType=\"WorkflowService\" IsValid=\"false\" ServerVersion=\"0.0.0.0\"><DisplayName>LoginOverride1</DisplayName><Category></Category><IsNewWorkflow>false</IsNewWorkflow><AuthorRoles></AuthorRoles><Comment></Comment><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService><DataList><Groups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" /></Groups><UserGroups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></UserGroups></DataList><Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition>&lt;Activity x:Class=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"654,676\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dciipe=\"clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,74.6666666666667&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.1666666666667 300,127.166666666667&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,127.166666666667&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;230,88&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDotNetMultiAssignActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (1)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"230,88\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"ca79a220-acf9-4ebf-b1ca-f76b5a4b30d5\" UpdateAllOccurrences=\"False\"&gt;&lt;uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\"&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"[[UserGroups().Name]]\" FieldValue=\"Public\" IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDotNetMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition></Action><ErrorMessages /><VersionInfo DateTimeStamp=\"2020-05-15T20:22:02.3670157+02:00\" Reason=\"Save\" User=\"T004178\\Rory McGuire\" VersionNumber=\"2\" ResourceId=\"" +
                                                resourceId +
                                                "\" VersionId=\"df405b6c-3b59-4023-acaa-7b16a8183f26\" /></Service>"));

            // var mockWarewolfResource = mockResource.As<IWarewolfResource>();
            // mockWarewolfResource.Setup(o => o.ResourceID).Returns(resourceId);
            // mockWarewolfResource.Setup(o => o.ResourceName).Returns(resourceName);
            // mockWarewolfResource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>"));
            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(r);
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), resourceName)).Returns(r);
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockCatalog.Setup(o => o.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(mockDev2Activity.Object);
            mockCatalog.Setup(a => a.GetResourcePath(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("bob\\dave");
            mockCatalog.Setup(o => o.GetDynamicObjects<DynamicService>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new[]
                {
                    new DynamicService
                    {
                        Actions = new List<ServiceAction>
                        {
                            new ServiceAction
                            {
                                ActionType = enActionType.Workflow,
                            }
                        }
                    }
                }.ToList());
            CustomContainer.Register(mockCatalog.Object);
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var mockResourceNameProvider = new Mock<IResourceNameProvider>();
            mockResourceNameProvider.Setup(a => a.GetResourceNameById(It.IsAny<Guid>())).Returns(resourceName);
            CustomContainer.Register(mockResourceNameProvider.Object);

            var doFactory = new TestTokenRequestDataObjectFactory(dataObject);
            var mockEsbChannelFactory = new Mock<IEsbChannelFactory>();
            var esbChannel = new Mock<IEsbChannel>();
            esbChannel.Setup(o =>
                    o.ExecuteRequest(dataObject, It.IsAny<EsbExecuteRequest>(), It.IsAny<Guid>(),
                        out It.Ref<ErrorResultTO>.IsAny))
                .Callback(new ExecuteRequestCallback((IDSFDataObject dobject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errorResultTO) =>
                {
                    // assign fake result as though a workflow has been run
                    errorResultTO = new ErrorResultTO();
                    request.ExecuteResult = new StringBuilder(payload);
                }));

            mockEsbChannelFactory.Setup(o => o.New()).Returns(esbChannel.Object);
            //---------------Assert Precondition----------------
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);

            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO
            {
                AuthenticationOverrideWorkflow = new NamedGuid
                {
                    Name = resourceName,
                    Value = resourceId
                },
                SecretKey = secretKey,
            });
            var securitySettings = mockSecuritySettings.Object;
            var handlerMock = new TokenRequestHandlerMock(mockCatalog.Object, wRepo.Object, authorizationService.Object, doFactory, mockEsbChannelFactory.Object, securitySettings);
            var headers = new Mock<NameValueCollection>();
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = resourceName,
                WebServerUrl = "http://localhost:3142/public/loginAuth"
            };
            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, resourceName, Guid.Empty.ToString(), headers.Object, principal.Object);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [ExpectedException(typeof(HttpResponseException))]
        [TestCategory(nameof(TokenRequestHandler))]
        public void TokenRequestHandler_UserGroup_IsNullOrEmpty_InternalServerError()
        {
            Dev2.Common.Utilities.ServerUser = new Mock<IPrincipal>().Object;

            var mockWarewolfPerformanceCounterLocater = new Mock<IWarewolfPerformanceCounterLocater>();
            var mockCounter = new Mock<IPerformanceCounter>();
            mockWarewolfPerformanceCounterLocater
                .Setup(o => o.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>()))
                .Returns(mockCounter.Object);
            mockWarewolfPerformanceCounterLocater.Setup(o => o.GetCounter(It.IsAny<string>()))
                .Returns(mockCounter.Object);
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(mockWarewolfPerformanceCounterLocater.Object);
            var resourceId = Guid.NewGuid();
            var resourceName = "loginAuth";
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            principal.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);
            var outerEnv = new ExecutionEnvironment();

            var payload =
                "<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>";
            var dataObject = new DsfDataObject(payload, Guid.Empty);
            dataObject.Environment = outerEnv;
            dataObject.ServiceName = resourceName;
            dataObject.ExecutingUser = principal.Object;

            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service =>
                    service.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<IResource>()))
                .Returns(true);
            var r = new Workflow(XElement.Parse("<Service ID=\"" + resourceId +
                                                "\" Version=\"1.0\" ServerID=\"18ca645a-b2c2-4d1d-907c-f42cff71a462\" Name=\"" +
                                                resourceName +
                                                "\" ResourceType=\"WorkflowService\" IsValid=\"false\" ServerVersion=\"0.0.0.0\"><DisplayName>LoginOverride1</DisplayName><Category></Category><IsNewWorkflow>false</IsNewWorkflow><AuthorRoles></AuthorRoles><Comment></Comment><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService><DataList><Groups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" /></Groups><UserGroups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></UserGroups></DataList><Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition>&lt;Activity x:Class=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"654,676\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dciipe=\"clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,74.6666666666667&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.1666666666667 300,127.166666666667&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,127.166666666667&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;230,88&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDotNetMultiAssignActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (1)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"230,88\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"ca79a220-acf9-4ebf-b1ca-f76b5a4b30d5\" UpdateAllOccurrences=\"False\"&gt;&lt;uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\"&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"[[UserGroups().Name]]\" FieldValue=\"Public\" IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDotNetMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition></Action><ErrorMessages /><VersionInfo DateTimeStamp=\"2020-05-15T20:22:02.3670157+02:00\" Reason=\"Save\" User=\"T004178\\Rory McGuire\" VersionNumber=\"2\" ResourceId=\"" +
                                                resourceId +
                                                "\" VersionId=\"df405b6c-3b59-4023-acaa-7b16a8183f26\" /></Service>"));

            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(r);
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), resourceName)).Returns(r);
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockCatalog.Setup(o => o.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(mockDev2Activity.Object);
            mockCatalog.Setup(a => a.GetResourcePath(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("bob\\dave");
            mockCatalog.Setup(o => o.GetDynamicObjects<DynamicService>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new[]
                {
                    new DynamicService
                    {
                        Actions = new List<ServiceAction>
                        {
                            new ServiceAction
                            {
                                ActionType = enActionType.Workflow,
                            }
                        }
                    }
                }.ToList());
            CustomContainer.Register(mockCatalog.Object);
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var mockResourceNameProvider = new Mock<IResourceNameProvider>();
            mockResourceNameProvider.Setup(a => a.GetResourceNameById(It.IsAny<Guid>())).Returns(resourceName);
            CustomContainer.Register(mockResourceNameProvider.Object);

            var doFactory = new TestTokenRequestDataObjectFactory(dataObject);
            var mockEsbChannelFactory = new Mock<IEsbChannelFactory>();
            var esbChannel = new Mock<IEsbChannel>();
            esbChannel.Setup(o =>
                    o.ExecuteRequest(dataObject, It.IsAny<EsbExecuteRequest>(), It.IsAny<Guid>(),
                        out It.Ref<ErrorResultTO>.IsAny))
                .Callback(new ExecuteRequestCallback((IDSFDataObject dobject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errorResultTO) =>
                {
                    // assign fake result as though a workflow has been run
                    errorResultTO = new ErrorResultTO();
                    request.ExecuteResult = new StringBuilder(payload);
                }));

            mockEsbChannelFactory.Setup(o => o.New()).Returns(esbChannel.Object);
            //---------------Assert Precondition----------------
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);

            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO
            {
                AuthenticationOverrideWorkflow = new NamedGuid
                {
                    Name = resourceName,
                    Value = resourceId
                },
                SecretKey = secretKey,
            });
            var securitySettings = mockSecuritySettings.Object;
            var handlerMock = new TokenRequestHandlerMock(mockCatalog.Object, wRepo.Object, authorizationService.Object, doFactory, mockEsbChannelFactory.Object, securitySettings);
            var headers = new Mock<NameValueCollection>();
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = resourceName,
                WebServerUrl = "http://localhost:3142/public/loginAuth"
            };
            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, resourceName, Guid.Empty.ToString(), headers.Object, principal.Object);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TokenRequestHandler))]
        [ExpectedException(typeof(HttpResponseException))]
        public void TokenRequestHandler_EmptyUserGroups_InternalServerError()
        {
            Dev2.Common.Utilities.ServerUser = new Mock<IPrincipal>().Object;

            var mockWarewolfPerformanceCounterLocater = new Mock<IWarewolfPerformanceCounterLocater>();
            var mockCounter = new Mock<IPerformanceCounter>();
            mockWarewolfPerformanceCounterLocater
                .Setup(o => o.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>()))
                .Returns(mockCounter.Object);
            mockWarewolfPerformanceCounterLocater.Setup(o => o.GetCounter(It.IsAny<string>()))
                .Returns(mockCounter.Object);
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(mockWarewolfPerformanceCounterLocater.Object);
            var resourceId = Guid.NewGuid();
            var resourceName = "loginAuth";
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            principal.Setup(o => o.IsInRole(It.IsAny<string>())).Returns(true);
            var outerEnv = new ExecutionEnvironment();
            outerEnv.Assign("[[Testing().Name]]", "public", 0);
            outerEnv.Assign("[[Testing().Name]]", "whatever", 0);
            var payload =
                "<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>";
            var dataObject = new DsfDataObject(payload, Guid.Empty);
            dataObject.Environment = outerEnv;
            dataObject.ServiceName = resourceName;
            dataObject.ExecutingUser = principal.Object;

            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service =>
                    service.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<IResource>()))
                .Returns(true);

            var r = new Workflow(XElement.Parse("<Service ID=\"" + resourceId +
                                                "\" Version=\"1.0\" ServerID=\"18ca645a-b2c2-4d1d-907c-f42cff71a462\" Name=\"" +
                                                resourceName +
                                                "\" ResourceType=\"WorkflowService\" IsValid=\"false\" ServerVersion=\"0.0.0.0\"><DisplayName>LoginOverride1</DisplayName><Category></Category><IsNewWorkflow>false</IsNewWorkflow><AuthorRoles></AuthorRoles><Comment></Comment><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService><DataList><Groups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" /></Groups><UserGroups Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\"><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></UserGroups></DataList><Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition>&lt;Activity x:Class=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"654,676\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:av=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:dciipe=\"clr-namespace:Dev2.Common.Interfaces.Infrastructure.Providers.Errors;assembly=Dev2.Common.Interfaces\" xmlns:ddd=\"clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data\" xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:sco=\"clr-namespace:System.Collections.ObjectModel;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"6\"&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=\"AssemblyReference\"&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=\"LoginOverride1\" sap:VirtualizedContainerService.HintSize=\"614,636\"&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=\"scg:List(x:String)\" Name=\"InstructionList\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"LastResult\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /&gt;&lt;Variable x:TypeArguments=\"x:String\" Name=\"ExplicitDataList\" /&gt;&lt;Variable x:TypeArguments=\"x:Boolean\" Name=\"IsValid\" /&gt;&lt;Variable x:TypeArguments=\"uaba:Util\" Name=\"t\" /&gt;&lt;Variable x:TypeArguments=\"ddd:Dev2DataListDecisionHandler\" Name=\"Dev2DecisionHandler\" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;60,74.6666666666667&lt;/av:Size&gt;&lt;av:PointCollection x:Key=\"ConnectorLocation\"&gt;300,77.1666666666667 300,127.166666666667&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=\"__ReferenceID0\"&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;av:Point x:Key=\"ShapeLocation\"&gt;185,127.166666666667&lt;/av:Point&gt;&lt;av:Size x:Key=\"ShapeSize\"&gt;230,88&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDotNetMultiAssignActivity CurrentResult=\"{x:Null}\" ExplicitDataList=\"{x:Null}\" InputMapping=\"{x:Null}\" InputTransformation=\"{x:Null}\" OnErrorVariable=\"{x:Null}\" OnErrorWorkflow=\"{x:Null}\" OnResumeKeepList=\"{x:Null}\" OutputMapping=\"{x:Null}\" ParentServiceID=\"{x:Null}\" ParentServiceName=\"{x:Null}\" ParentWorkflowInstanceId=\"{x:Null}\" ResultTransformation=\"{x:Null}\" ScenarioID=\"{x:Null}\" ScopingObject=\"{x:Null}\" ServiceHost=\"{x:Null}\" SimulationOutput=\"{x:Null}\" Add=\"False\" CreateBookmark=\"False\" DatabindRecursive=\"False\" DisplayName=\"Assign (1)\" HasError=\"[HasError]\" sap:VirtualizedContainerService.HintSize=\"230,88\" InstructionList=\"[InstructionList]\" IsEndedOnError=\"False\" IsService=\"False\" IsSimulationEnabled=\"False\" IsUIStep=\"False\" IsValid=\"[IsValid]\" IsWorkflow=\"False\" OnResumeClearAmbientDataList=\"False\" OnResumeClearTags=\"FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage\" SimulationMode=\"OnDemand\" UniqueID=\"ca79a220-acf9-4ebf-b1ca-f76b5a4b30d5\" UpdateAllOccurrences=\"False\"&gt;&lt;uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=\"scg:List(x:String)\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\"&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"[[UserGroups().Name]]\" FieldValue=\"Public\" IndexNumber=\"1\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=\"{x:Null}\" Path=\"{x:Null}\" WatermarkTextValue=\"{x:Null}\" WatermarkTextVariable=\"{x:Null}\" FieldName=\"\" FieldValue=\"\" IndexNumber=\"2\" Inserted=\"False\" IsFieldNameFocused=\"False\" IsFieldValueFocused=\"False\"&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, scg:List(dciipe:IActionableErrorInfo)\" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=\"x:String\" Capacity=\"0\" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=\"x:String\" /&gt;&lt;/uaba:DsfDotNetMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=\"x:String, x:Object\"&gt;&lt;x:Boolean x:Key=\"IsExpanded\"&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDotNetMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition></Action><ErrorMessages /><VersionInfo DateTimeStamp=\"2020-05-15T20:22:02.3670157+02:00\" Reason=\"Save\" User=\"T004178\\Rory McGuire\" VersionNumber=\"2\" ResourceId=\"" +
                                                resourceId +
                                                "\" VersionId=\"df405b6c-3b59-4023-acaa-7b16a8183f26\" /></Service>"));

            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(r);
            mockCatalog.Setup(a => a.GetResource(It.IsAny<Guid>(), resourceName)).Returns(r);
            var mockDev2Activity = new Mock<IDev2Activity>();
            mockCatalog.Setup(o => o.Parse(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(mockDev2Activity.Object);
            mockCatalog.Setup(a => a.GetResourcePath(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("bob\\dave");
            mockCatalog.Setup(o => o.GetDynamicObjects<DynamicService>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new[]
                {
                    new DynamicService
                    {
                        Actions = new List<ServiceAction>
                        {
                            new ServiceAction
                            {
                                ActionType = enActionType.Workflow,
                            }
                        }
                    }
                }.ToList());
            CustomContainer.Register(mockCatalog.Object);
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var mockResourceNameProvider = new Mock<IResourceNameProvider>();
            mockResourceNameProvider.Setup(a => a.GetResourceNameById(It.IsAny<Guid>())).Returns(resourceName);
            CustomContainer.Register(mockResourceNameProvider.Object);

            var doFactory = new TestTokenRequestDataObjectFactory(dataObject);
            var mockEsbChannelFactory = new Mock<IEsbChannelFactory>();
            var esbChannel = new Mock<IEsbChannel>();
            esbChannel.Setup(o =>
                    o.ExecuteRequest(dataObject, It.IsAny<EsbExecuteRequest>(), It.IsAny<Guid>(),
                        out It.Ref<ErrorResultTO>.IsAny))
                .Callback(new ExecuteRequestCallback((IDSFDataObject dobject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errorResultTO) =>
                {
                    // assign fake result as though a workflow has been run
                    errorResultTO = new ErrorResultTO();
                    request.ExecuteResult = new StringBuilder(payload);
                }));

            mockEsbChannelFactory.Setup(o => o.New()).Returns(esbChannel.Object);
            //---------------Assert Precondition----------------
            var mockSecuritySettings = new Mock<ISecuritySettings>();
            var hmac = new HMACSHA256();
            var secretKey = Convert.ToBase64String(hmac.Key);

            mockSecuritySettings.Setup(o => o.ReadSettingsFile(It.IsAny<IResourceNameProvider>())).Returns(new SecuritySettingsTO
            {
                AuthenticationOverrideWorkflow = new NamedGuid
                {
                    Name = resourceName,
                    Value = resourceId
                },
                SecretKey = secretKey,
            });
            var securitySettings = mockSecuritySettings.Object;
            var handlerMock = new TokenRequestHandlerMock(mockCatalog.Object, wRepo.Object, authorizationService.Object, doFactory, mockEsbChannelFactory.Object, securitySettings);
            var headers = new Mock<NameValueCollection>();
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = resourceName,
                WebServerUrl = "http://localhost:3142/public/loginAuth"
            };
            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, resourceName, Guid.Empty.ToString(), headers.Object, principal.Object);
            //------------Assert Results-------------------------
        }

        delegate void ExecuteRequestCallback(IDSFDataObject dobject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errorResultTO);

        static void GetExecutingUser(Mock<IPrincipal> principal)
        {
            var identity = new Mock<IIdentity>();
            identity.Setup(p => p.Name).Returns("User1");
            principal.Setup(p => p.Identity).Returns(identity.Object);
        }
    }

    class TestTokenRequestDataObjectFactory : TokenRequestHandler.IDataObjectFactory
    {
        readonly IDSFDataObject _dataObject;

        public TestTokenRequestDataObjectFactory(IDSFDataObject dataObject)
        {
            _dataObject = dataObject;
        }

        public IDSFDataObject New(Guid workspaceGuid, IPrincipal user, string serviceName, WebRequestTO webRequest) => _dataObject;
    }

    class TokenRequestHandlerMock : TokenRequestHandler
    {
        public TokenRequestHandlerMock(IResourceCatalog resourceCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, ISecuritySettings securitySettings)
            : base(resourceCatalog, workspaceRepository, authorizationService, dataObjectFactory, esbChannelFactory, securitySettings)
        {
        }

        public IResponseWriter CreateFromMock(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            return ExecuteWorkflow(webRequest, serviceName, workspaceId, headers, user);
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}