using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Wizards;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Studio.Core.Wizards.Interfaces;
using System.Reflection;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Factories;
using Moq;
using Dev2.Studio.Core;
using System.Xml;
using System.Xml.Linq;
using System.Activities.Presentation.Model;
using Dev2.Core.Tests.Utils;
using Dev2.DataList.Contract;
using System.Diagnostics;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class WizardEngineTests
    {
        private Mock<IEnvironmentModel> environment;
        private IContextualResourceModel resource;
        private WizardEngine wizEng;
        private Mock<IContextualResourceModel> mockResource1;
        private Mock<IContextualResourceModel> mockResource2;
        private Mock<IContextualResourceModel> mockResource3;

        #region Initialize

        [TestInitialize()]
        public void Initialize()
        {
            mockResource1 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestWorkflow1");
            mockResource2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestWorkflow2");
            mockResource3 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "TestWorkflow1.wiz");
            environment = Dev2MockFactory.SetupEnvironmentModel();
            resource = ResourceModelFactory.CreateResourceModel(environment.Object);
            resource.ResourceName = "TestName";
            resource.DisplayName = "TestName";

            ImportService.CurrentContext = CompositionInitializer.PopUpProviderForTestsWithMockMainViewModel();
            wizEng = new WizardEngine();

            ImportService.SatisfyImports(wizEng);
        }

        #endregion

        #region CallbackHandlers

        //TODO - 5780, Uncomment this test when there are implementations of the IActivityWizardCallbackHandler<> interface for all activities
        //[TestMethod]
        //public void Test_ThatThereAreCallbackHandlersForAllActivitiesWhichInheritFromDsfActivityAbstract()
        //{
        //    ImportService.CurrentContext = _importServiceContext;

        //    Type baseType = typeof(DsfActivityAbstract<>);
        //    IList<Type> activityTypes = typeof(DsfActivityAbstract<>).Assembly.GetTypes().Where(t => t != baseType && t.InheritsOrImplements(baseType)).ToList();

        //    foreach (Type type in activityTypes)
        //    {
        //        Type contract = typeof(IActivitySpecificSettingsWizardCallbackHandler<>).MakeGenericType(new Type[] { type });
        //        MethodInfo mi = typeof(ImportService).GetMethods().Where(m => m.IsGenericMethod && m.Name == "GetExportValue" && m.GetParameters().Length == 0).First().MakeGenericMethod(contract);

        //        try
        //        {
        //            mi.Invoke(null, null);
        //        }
        //        catch
        //        {
        //            throw new Exception("No exported implementation of the IActivityWizardCallbackHandler<> interface could be found for the '" + type.Name + "' activity.");
        //        }
        //    }
        //}

        #endregion CallbackHandlers

        #region CreateResourceWizard Tests

        [TestMethod]
        public void CreateResourceWizard_ForWorkflow_Positive_Expected_New_Workflow()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource1, new List<IResourceModel>());
            mockResource2.Setup(moq => moq.ResourceType).Returns(ResourceType.WorkflowService); 
            mockResource2.Setup(moq => moq.DataList).Returns(@"<ADL>
	<Host IsEditable=""true"" Description=""""/>
	<Port IsEditable=""true"" Description=""""/>
	<From IsEditable=""true"" Description=""""/>
	<To IsEditable=""true"" Description=""""/>
	<Subject IsEditable=""true"" Description=""""/>
	<BodyType IsEditable=""true"" Description=""""/>
	<Body IsEditable=""true"" Description=""""/>
	<Attachment IsEditable=""true"" Description=""""/>
	<FailureMessage IsEditable=""true"" Description=""""/>
	<Message IsEditable=""true"" Description=""""/>
</ADL>");
            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            IContextualResourceModel wizResource = null;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => wizResource = o as IContextualResourceModel);

            wizEng.CreateResourceWizard(mockResource2.Object);

            string expectedDataList = @"<DataList><Host IsEditable=""False""></Host><Port IsEditable=""False""></Port><From IsEditable=""False""></From><To IsEditable=""False""></To><Subject IsEditable=""False""></Subject><BodyType IsEditable=""False""></BodyType><Body IsEditable=""False""></Body><Attachment IsEditable=""False""></Attachment><FailureMessage IsEditable=""False""></FailureMessage><Message IsEditable=""False""></Message></DataList>";

            Assert.IsTrue(wizResource.ResourceName == "TestWorkflow2.wiz" && (wizResource.DataList == expectedDataList));
        }

        [TestMethod]
        public void CreateResourceWizard_ForService_Positive_Expected_New_Workflow()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource1, new List<IResourceModel>());
            mockResource2.Setup(moq => moq.ResourceType).Returns(ResourceType.Service);  
            mockResource2.Setup(moq => moq.ServiceDefinition).Returns(@"<Service Name=""Email Service"">
  <Actions>
    <Action Name=""EmailService"" Type=""Plugin"" SourceName=""Email Plugin"" SourceMethod=""Send"">
      <Inputs>
        <Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Port"" Source=""Port"" DefaultValue=""25"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""To"" Source=""To"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Subject"" Source=""Subject"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Body"" Source=""Body"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE"">
          <Validator Type=""Required"" />
        </Input>
      </Inputs>
      <Outputs>
        <Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" />
        <Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" />
      </Outputs>
    </Action>
  </Actions>
  <AuthorRoles>Schema Admins,Enterprise Admins,Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Fax Administrators,Windows SBS Virtual Private Network Users,All Users,Windows SBS Administrators,Windows SBS SharePoint_OwnersGroup,Windows SBS Link Users,Windows SBS Admin Tools Group,Company Users,Business Design Studio Developers,</AuthorRoles>
  <Comment>Service originates Emails</Comment>
  <Category>Communication</Category>
  <Tags>communication,email</Tags>
  <HelpLink>http://d</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <BizRule />
  <WorkflowActivityDef />
  <Source />
  <XamlDefinition />
  <DisplayName>Service</DisplayName>
  <DataList />
</Service>");
            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            IContextualResourceModel wizResource = null;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => wizResource = o as IContextualResourceModel);

            wizEng.CreateResourceWizard(mockResource2.Object);

            string expectedDataList = @"<DataList><Host Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><Port Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><From Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><To Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><Subject Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><BodyType Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><Body Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><Attachment Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><FailureMessage Description="""" ColumnIODirection=""None"" IsEditable=""False"" /><Message Description="""" ColumnIODirection=""None"" IsEditable=""False"" /></DataList>";
            XElement.DeepEquals(XElement.Parse(expectedDataList), XElement.Parse(wizResource.DataList));
            Assert.IsTrue(wizResource.ResourceName == "TestWorkflow2.wiz");
            Assert.IsTrue(XElement.DeepEquals(XElement.Parse(expectedDataList), XElement.Parse(wizResource.DataList)));
        }

        [TestMethod]
        public void CreateResourceWizard_For_Workflow_With_Blank_DataList_Positive_Expected_New_Workflow()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource1, new List<IResourceModel>());
            mockResource2.Setup(moq => moq.ResourceType).Returns(ResourceType.WorkflowService);
            mockResource2.Setup(moq => moq.DataList).Returns("<DataList></DataList>");        
            mockResource2.Setup(moq => moq.ServiceDefinition).Returns(@"<Service Name=""Email Service"">
  <Actions>
    <Action Name=""EmailService"" Type=""Plugin"" SourceName=""Email Plugin"" SourceMethod=""Send"">
      <Inputs>
        <Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Port"" Source=""Port"" DefaultValue=""25"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""To"" Source=""To"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Subject"" Source=""Subject"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Body"" Source=""Body"">
          <Validator Type=""Required"" />
        </Input>
        <Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE"">
          <Validator Type=""Required"" />
        </Input>
      </Inputs>
      <Outputs>
        <Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" />
        <Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" />
      </Outputs>
    </Action>
  </Actions>
  <AuthorRoles>Schema Admins,Enterprise Admins,Domain Admins,Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Fax Administrators,Windows SBS Virtual Private Network Users,All Users,Windows SBS Administrators,Windows SBS SharePoint_OwnersGroup,Windows SBS Link Users,Windows SBS Admin Tools Group,Company Users,Business Design Studio Developers,</AuthorRoles>
  <Comment>Service originates Emails</Comment>
  <Category>Communication</Category>
  <Tags>communication,email</Tags>
  <HelpLink>http://d</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <BizRule />
  <WorkflowActivityDef />
  <Source />
  <XamlDefinition />
  <DisplayName>Service</DisplayName>
  <DataList />
</Service>");
            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            IContextualResourceModel wizResource = null;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => wizResource = o as IContextualResourceModel);

            wizEng.CreateResourceWizard(mockResource2.Object);

            string expectedDataList = @"<DataList></DataList>";

            Assert.IsTrue(wizResource.ResourceName == "TestWorkflow2.wiz" && (wizResource.DataList == expectedDataList));
        }

        #endregion CreateResourceWizard Tests

        #region GetWizardInvocationTO Tests

        [TestMethod]
        public void GetActivityWizardInvocationTO_DsfActivity_Postive_Expected_correct_TO_Returned()
        {
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource");
            Mock<IContextualResourceModel> testRes2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource2");
            Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel(testRes, new List<IResourceModel>());
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            testRes2.Setup(moq => moq.Environment).Returns(envModel.Object);
            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "testResource";
            Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
            Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);
            WizardInvocationTO transObj = wizEng.GetActivityWizardInvocationTO(testItem, testRes2.Object, testCompiler);

            Assert.IsTrue(transObj.TransferDatalistID != Guid.Empty && transObj.Endpoint.ToString().Contains("testResource.wiz?"));
        }

        [TestMethod]
        public void GetActivityWizardInvocationTO_ToolingActivity_Postive_Expected_correct_TO_Returned()
        {
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource");
            Mock<IContextualResourceModel> testRes2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource2");
            Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel(testRes, new List<IResourceModel>());
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
            testRes2.Setup(moq => moq.Environment).Returns(envModel.Object);
            DsfCalculateActivity Act = new DsfCalculateActivity();
            Act.Expression = "testExpression";
            Act.Result = "[[testRes]]";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);
            WizardInvocationTO transObj = wizEng.GetActivityWizardInvocationTO(testItem, testRes2.Object, testCompiler);

            Assert.IsTrue(transObj.TransferDatalistID != Guid.Empty && transObj.Endpoint.ToString().Contains("Dev2DsfCalculateActivityWizard?"));
        }

        //Comment back in when the settings tab is working
        //[TestMethod]
        //public void GetActivitySettingsWizardInvocationTO_Postive_Expected_correct_TO_Returned()
        //{
        //    Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(enResourceType.WorkflowService, "testResource");
        //    Mock<IContextualResourceModel> testRes2 = Dev2MockFactory.SetupResourceModelMock(enResourceType.WorkflowService, "testResource2");
        //    Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel(testRes, new List<IResourceModel>());
        //    IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();
        //    testRes2.Setup(moq => moq.Environment).Returns(envModel.Object);
        //    DsfActivity Act = new DsfActivity();
        //    Act.ServiceName = "testResource";
        //    Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
        //    Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
        //    ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);
        //    WizardInvocationTO transObj = wizEng.GetActivitySettingsWizardInvocationTO(testItem, testRes2.Object, testCompiler);

        //    Assert.IsTrue(transObj.TransferDatalistID != (new Guid()) && transObj.Endpoint.ToString().Contains("testResource.wiz?"));
        //}

        #endregion GetWizardInvocationTO Tests

        #region EditWizard Tests

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void EditWizard_Where_ResourceIsntWizard_Expected_Exception()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource1, new List<IResourceModel>());

            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            IContextualResourceModel wizResource = null;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => wizResource = o as IContextualResourceModel);

            wizEng.EditWizard(mockResource1.Object, mockResource1.Object);
        }

        [TestMethod]
        public void EditWizard_Where_ResourceIsWizard_Expected_Wizard_Workflow()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource1, new List<IResourceModel>());
            mockResource3.Setup(moq => moq.ResourceType).Returns(ResourceType.WorkflowService);
            mockResource3.Setup(moq => moq.DataList).Returns(@"<ADL>
	<Host IsEditable=""false"" Description=""""/>
	<Port IsEditable=""false"" Description=""""/>
	<From IsEditable=""false"" Description=""""/>
	<To IsEditable=""false"" Description=""""/>
	<Subject IsEditable=""false"" Description=""""/>
	<BodyType IsEditable=""false"" Description=""""/>
	<Body IsEditable=""false"" Description=""""/>
	<Attachment IsEditable=""false"" Description=""""/>
	<FailureMessage IsEditable=""false"" Description=""""/>
	<Message IsEditable=""false"" Description=""""/>
</ADL>");
            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            IContextualResourceModel wizResource = null;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => wizResource = o as IContextualResourceModel);

            wizEng.EditWizard(mockResource3.Object, mockResource1.Object);

            string popupMessage = @"The following items have changed on the Data List of TestWorkflow1: 
 
 Added: NUD2347, number, vehicleColor, Fines_Speed, Fines_Date, Fines_Location, Registrations_Speed, Registrations_Date, Registrations_Location
-------------------------------------------------";

            Assert.IsTrue(wizResource.ResourceName == "TestWorkflow1.wiz" && (wizEng.Popup.Description == popupMessage));
        }

        [TestMethod]
        public void EditWizard_Where_ResourceIsParentOfWizard_Expected_Wizard_Workflow()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource3, new List<IResourceModel>());
            mockResource1.Setup(moq => moq.Environment).Returns(environment.Object);
            mockResource3.Setup(moq => moq.ResourceType).Returns(ResourceType.WorkflowService);    
            mockResource3.Setup(moq => moq.DataList).Returns(@"<ADL>
	<Host IsEditable=""False"" Description=""""/>
	<Port IsEditable=""False"" Description=""""/>
	<From IsEditable=""False"" Description=""""/>
	<To IsEditable=""False"" Description=""""/>
	<Subject IsEditable=""False"" Description=""""/>
	<BodyType IsEditable=""False"" Description=""""/>
	<Body IsEditable=""False"" Description=""""/>
	<Attachment IsEditable=""False"" Description=""""/>
	<FailureMessage IsEditable=""False"" Description=""""/>
    <RemoveVar IsEditable=""False"" Description=""""/>
	<Message IsEditable=""False"" Description=""""/>
</ADL>");
            MediatorMessageTrapper.DeregUserInterfaceLayoutProvider();
            IContextualResourceModel wizResource = null;
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, o => wizResource = o as IContextualResourceModel);

            wizEng.EditResourceWizard(mockResource1.Object);

            string popupMessage = @"The following items have changed on the Data List of TestWorkflow1: 
 
 Added: NUD2347, number, vehicleColor, Fines_Speed, Fines_Date, Fines_Location, Registrations_Speed, Registrations_Date, Registrations_Location
------------------------------------------------- 
 Removed: Host, Port, From, To, Subject, BodyType, Body, Attachment, FailureMessage, RemoveVar, Message
-------------------------------------------------";

            Assert.IsTrue(wizResource.ResourceName == "TestWorkflow1.wiz" && (wizEng.Popup.Description == popupMessage));
        }        

        #endregion EditWizard Tests        

        #region Is Wizard Tests

        [TestMethod]
        public void IsResourceWizard_Expected_True()
        {
            Assert.IsTrue(wizEng.IsResourceWizard(mockResource3.Object));
        }

        [TestMethod]
        public void IsResourceWizard_Expected_False()
        {
            Assert.IsFalse(wizEng.IsResourceWizard(mockResource1.Object));
        }

        [TestMethod]
        public void IsResourceWizard_Where_ResourceIsNull_Expected_False()
        {
            Assert.IsFalse(wizEng.IsResourceWizard(null));
        }

        [TestMethod]
        public void IsSystemWizard_Expected_True()
        {
            mockResource1.Setup(moq => moq.ResourceName).Returns("Dev2TestWorkflowWizard");
            Assert.IsTrue(wizEng.IsSystemWizard(mockResource1.Object));
        }

        [TestMethod]
        public void IsSystemWizard_Expected_False()
        {
            Assert.IsFalse(wizEng.IsSystemWizard(mockResource1.Object));
        }

        [TestMethod]
        public void IsSystemWizard_Where_ResourceIsNull_Expected_False()
        {
            Assert.IsFalse(wizEng.IsSystemWizard(null));
        }

        [TestMethod]
        public void IsWizard_ResourceWizard_Expected_True()
        {            
            Assert.IsTrue(wizEng.IsWizard(mockResource3.Object));
        }

        [TestMethod]
        public void IsWizard_SystemWizard_Expected_True()
        {
            mockResource1.Setup(moq => moq.ResourceName).Returns("Dev2TestWorkflowWizard");
            Assert.IsTrue(wizEng.IsWizard(mockResource1.Object));
        }

        [TestMethod]
        public void IsWizard_Expected_False()
        {
            Assert.IsFalse(wizEng.IsWizard(mockResource1.Object));
        }

        [TestMethod]
        public void IsWizard_Where_ResourceIsNull_Expected_False()
        {
            Assert.IsFalse(wizEng.IsWizard(null));
        }

        #endregion Is Wizard Tests

        #region Get Parent Tests

        [TestMethod]
        public void GetParent_Positive_Expected_Parent_Returned()
        {
            environment = Dev2MockFactory.SetupEnvironmentModel(mockResource1, new List<IResourceModel>());
            mockResource3.Setup(moq => moq.Environment).Returns(environment.Object);
            IContextualResourceModel parent = wizEng.GetParent(mockResource3.Object);
            Assert.IsTrue(parent.ResourceName == "TestWorkflow1");
        }

        [TestMethod]
        public void GetParent_NULL_Expected_Null_Returned()
        {
            IContextualResourceModel parent = wizEng.GetParent(null);
            Assert.IsNull(parent);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Can't get a parent for a resource that is not a wizard. The attempt was made on 'TestWorkflow1'.")]
        public void GetParent_NotResourceWizard_Expected_Exception()
        {
            IContextualResourceModel parent = wizEng.GetParent(mockResource1.Object);         
        }

        #endregion Get Parent Tests

        #region HasWizard Tests

        [TestMethod]
        public void HasWizard_Where_ActivityIsDsfActivityAndResourceHasWizard_Expected_True()
        {
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource");
            Mock<IContextualResourceModel> testRes2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource2");
            Mock<IEnvironmentModel> envModel = Dev2MockFactory.SetupEnvironmentModel(testRes, new List<IResourceModel>());
            testRes2.Setup(moq => moq.Environment).Returns(envModel.Object);
            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "testResource";
            Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
            Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            bool result = wizEng.HasWizard(testItem, testRes.Object);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasWizard_Where_ActivityIsDsfActivityAndDoesntHaveWizard_Expected_False()
        {
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource", false);
            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "testResource";
            Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
            Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            bool result = wizEng.HasWizard(testItem, testRes.Object);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasWizard_Where_ActivityIsCodedActivityAndResourceHasWizard_Expected_True()
        {
            Mock<IContextualResourceModel> testRes2 = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "Dev2DsfMultiAssignActivityWizard");
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource", testRes2);

            DsfMultiAssignActivity Act = new DsfMultiAssignActivity();
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            bool result = wizEng.HasWizard(testItem, testRes.Object);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasWizard_Where_ActivityIsCodedActivityAndResourceDoesntHaveWizard_Expected_False()
        {
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource", false);

            DsfMultiAssignActivity Act = new DsfMultiAssignActivity();
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            bool result = wizEng.HasWizard(testItem, testRes.Object);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HasWizard_Where_ActivityIsNull_Expected_Exception()
        {
            Mock<IContextualResourceModel> testRes = Dev2MockFactory.SetupResourceModelMock(ResourceType.WorkflowService, "testResource", false);

            bool result = wizEng.HasWizard(null, testRes.Object);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HasWizard_Where_HostResourceIsNull_Expected_Exception()
        {
            DsfMultiAssignActivity Act = new DsfMultiAssignActivity();
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            bool result = wizEng.HasWizard(testItem, null);

            Assert.IsFalse(result);
        }

        #endregion HasWizard Tests
    }
}
