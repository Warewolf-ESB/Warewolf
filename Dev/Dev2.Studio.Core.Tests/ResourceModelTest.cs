using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ResourceModelTest
    {
        private IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            //Setup();
        }

        void Setup()
        {
            //   ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            var environmentModel = CreateMockEnvironment(new Mock<IEventPublisher>().Object);


            _resourceModel = new ResourceModel(environmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                WorkflowXaml = new StringBuilder(@"
<Service Name=""abc"">
    <Inputs/>
    <Outputs/>
    <DataList>
        <Country/>
        <State />
        <City>
            <Name/>
            <GeoLocation />
        </City>
    </DataList>
</Service>
")
            };
        }

        #endregion Test Initialization

        #region Update Tests

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceModel_Update")]
        public void ResourceModel_Update_WhenWorkflowXamlChanged_ExpectUpdatedResourceModelWithNewXaml()
        {
            //------------Setup for test--------------------------
            // Setup();
            var environmentModel = CreateMockEnvironment(new EventPublisher());
            var resourceModel = new ResourceModel(environmentModel.Object);
            const string category = "TestCat";
            const string comment = "TestComment";
            const string displayName = "DisplayName";
            const string resourceName = "TestResourceName";
            var id = Guid.NewGuid();
            const string tags = "TestTags";
            resourceModel.Category = category;
            resourceModel.Comment = comment;
            resourceModel.DisplayName = displayName;
            resourceModel.ID = id;
            resourceModel.ResourceName = resourceName;
            resourceModel.Tags = tags;
            resourceModel.WorkflowXaml = new StringBuilder("new xaml");
            //------------Execute Test---------------------------
            var updateResourceModel = new ResourceModel(environmentModel.Object) { WorkflowXaml = new StringBuilder("old xaml") };
            updateResourceModel.Update(resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual("new xaml", updateResourceModel.WorkflowXaml.ToString());
        }

        [TestMethod]
        public void ResourceModel_UpdateResourceModelExpectPropertiesUpdated()
        {
            //------------Setup for test--------------------------
            Setup();
            var environmentModel = CreateMockEnvironment(new EventPublisher());
            var resourceModel = new ResourceModel(environmentModel.Object);
            const Permissions UserPermissions = Permissions.Contribute;
            const string category = "TestCat";
            const string comment = "TestComment";
            const string displayName = "DisplayName";
            const string resourceName = "TestResourceName";
            const string inputs = "this is the inputs";
            const string outputs = "this is the outputs";
            var id = Guid.NewGuid();
            const string tags = "TestTags";
            resourceModel.Category = category;
            resourceModel.Comment = comment;
            resourceModel.DisplayName = displayName;
            resourceModel.ID = id;
            resourceModel.ResourceName = resourceName;
            resourceModel.Tags = tags;
            resourceModel.UserPermissions = UserPermissions;
            resourceModel.Inputs = inputs;
            resourceModel.Outputs = outputs;
            //------------Execute Test---------------------------
            var updateResourceModel = new ResourceModel(environmentModel.Object);
            updateResourceModel.Update(resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(category, updateResourceModel.Category);
            Assert.AreEqual(comment, updateResourceModel.Comment);
            Assert.AreEqual(displayName, updateResourceModel.DisplayName);
            Assert.AreEqual(id, updateResourceModel.ID);
            Assert.AreEqual(tags, updateResourceModel.Tags);
            Assert.AreEqual(UserPermissions, updateResourceModel.UserPermissions);
            Assert.AreEqual(inputs, updateResourceModel.Inputs);
            Assert.AreEqual(outputs, updateResourceModel.Outputs);
        }
        #endregion Update Tests

        [TestMethod]
        public void ResourceModel_DataListPropertyWhereChangedToSameString_NotifyPropertyChangedNotFiredTwice()
        {
            //------------Setup for test--------------------------
            // Setup();
            Mock<IEnvironmentModel> testEnvironmentModel = CreateMockEnvironment();
            var resourceModel = new ResourceModel(testEnvironmentModel.Object);
            var timesFired = 0;
            resourceModel.PropertyChanged += (sender, args) =>
            {
                timesFired++;
            };
            //------------Execute Test---------------------------
            resourceModel.DataList = "TestDataList";
            resourceModel.DataList = "TestDataList";
            //------------Assert Results-------------------------
            Assert.AreEqual(1, timesFired);
        }

        [TestMethod]
        public void ResourceModel_OnWorkflowSaved_IsWorkflowchangedWherePropertyUpdated_FireOnWorkflowSaved()
        {
            //------------Setup for test--------------------------
            Setup();
            Mock<IEnvironmentModel> testEnvironmentModel = CreateMockEnvironment(EventPublishers.Studio);
            var resourceModel = new ResourceModel(testEnvironmentModel.Object);
            var eventFired = false;
            IContextualResourceModel eventResourceModel = null;
            resourceModel.OnResourceSaved += model =>
            {
                eventResourceModel = model;
                eventFired = true;
            };
            //------------Execute Test---------------------------
            resourceModel.IsWorkflowSaved = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(eventResourceModel);
            Assert.AreSame(resourceModel, eventResourceModel);
        }

        #region ToServiceDefinition Tests


        #endregion ToServiceDefinition Tests

        #region DataList Tests

        [TestMethod]
        public void ResourceModel_DataList_Setter_UpdatedDataListSectionInServiceDefinition()
        {
            Setup();
            const string newDataList = @"<DataList>
  <Country />
  <State />
  <City>
    <Name />
    <GeoLocation />
  </City>
</DataList>";
            _resourceModel.DataList = newDataList;

            string result = _resourceModel.DataList;

            var xe = _resourceModel.WorkflowXaml.ToXElement();
            var dlElms = xe.Elements("DataList");

            // new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(_resourceModel.WorkflowXaml.ToString()).GetValue("DataList")
            var firstOrDefault = dlElms.FirstOrDefault();
            if(firstOrDefault != null)
            {
                var wfResult = firstOrDefault.ToString(SaveOptions.None);
                StringAssert.Contains(result, wfResult);
            }
            else
            {
                Assert.Fail();
            }

        }

        [TestMethod]
        public void ResourceModel_Constructor_IsWorkflowSaved()
        {
            //------------Setup for test--------------------------
            Setup();
            var environmentModel = CreateMockEnvironment(new Mock<IEventPublisher>().Object);

            //------------Execute Test---------------------------
            var resourceModel = new ResourceModel(environmentModel.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(resourceModel.IsWorkflowSaved);
        }
        #endregion DataList Tests


        [TestMethod]
        [TestCategory("ResourceModel_DesignValidationService")]
        [Description("Design validation memo errors must be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_DesignValidationServicePublishingMemo_UpdatesErrors()
        {
            //Setup();
            var instanceID = Guid.NewGuid();
            var pubMemo = new DesignValidationMemo { InstanceID = instanceID };
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = instanceID });
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = instanceID });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };

            model.OnDesignValidationReceived += (sender, memo) =>
            {
                Assert.AreEqual(memo.Errors.Count, model.Errors.Count, "OnDesignValidationReceived did not update the number of errors correctly.");

                foreach(var error in memo.Errors)
                {
                    var modelError = model.Errors.FirstOrDefault(me => me.ErrorType == error.ErrorType && me.Message == error.Message);
                    Assert.AreSame(error, modelError, "OnDesignValidationReceived did not set the error.");
                }
            };

            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [TestCategory("ResourceModel_DesignValidationService")]
        [Description("Design validation memo errors must be added to the errors list.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_DesignValidationServicePublishingMemo_NoInstanceID_DoesNotUpdatesErrors()
        {
            //Setup();
            var instanceID = Guid.NewGuid();
            var pubMemo = new DesignValidationMemo { InstanceID = instanceID };
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error." });
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error." });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };

            model.OnDesignValidationReceived += (sender, memo) => Assert.AreEqual(0, model.Errors.Count, "OnDesignValidationReceived did not update the number of errors correctly.");

            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [TestCategory("ResourceModel_Rollback")]
        [Description("Resource model rollback must restore fixed errors.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_Rollback_FixedErrorsRestored()
        {
            //Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };

            var hitCount = 0;
            model.OnDesignValidationReceived += (sender, memo) =>
            {
                switch(hitCount++)
                {
                    case 0:
                        Assert.AreEqual(2, model.Errors.Count);
                        Assert.AreEqual(0, model.FixedErrors.Count);
                        break;

                    case 1:
                        Assert.AreEqual(1, model.Errors.Count);
                        Assert.AreEqual(1, model.FixedErrors.Count);

                        model.Rollback();

                        Assert.AreEqual(2, model.Errors.Count);
                        Assert.AreEqual(0, model.FixedErrors.Count);
                        break;
                }
            };

            // Publish 2 errors
            var pubMemo = new DesignValidationMemo { InstanceID = instanceID };
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = instanceID });
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = instanceID });
            eventPublisher.Publish(pubMemo);

            // Fix 1 error and publish
            pubMemo.Errors.RemoveAt(1);
            eventPublisher.Publish(pubMemo);
        }


        [TestMethod]
        [TestCategory("ResourceModel_Rollback")]
        [Description("Resource model commit must not restore fixed errors.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_Commit_FixedErrorsNotRestored()
        {
            //Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };

            var hitCount = 0;
            model.OnDesignValidationReceived += (sender, memo) =>
            {
                switch(hitCount++)
                {
                    case 0:
                        Assert.AreEqual(2, model.Errors.Count);
                        Assert.AreEqual(0, model.FixedErrors.Count);
                        break;

                    case 1:
                        Assert.AreEqual(1, model.Errors.Count);
                        Assert.AreEqual(1, model.FixedErrors.Count);

                        model.Commit();

                        Assert.AreEqual(1, model.Errors.Count);
                        Assert.AreEqual(0, model.FixedErrors.Count);
                        break;
                }
            };

            // Publish 2 errors
            var pubMemo = new DesignValidationMemo { InstanceID = instanceID };
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = instanceID });
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = instanceID });
            eventPublisher.Publish(pubMemo);

            // Fix 1 error and publish
            pubMemo.Errors.RemoveAt(1);
            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        [ExpectedException(typeof(Exception))]
        public void ResourceModel_ToServiceDefinition_InvalidResourceType_ThrowsException()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = null,
                ResourceType = ResourceType.Server
            };

            //------------Execute Test---------------------------
            var serviceDefinition = model.ToServiceDefinition();

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, serviceDefinition);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_GivenXamlNull_ExpectFetchOfXaml()
        {
            Verify_ToServiceDefinition_GivenXamlNull(ResourceType.WorkflowService);
            Verify_ToServiceDefinition_GivenXamlNull(ResourceType.Source);
        }

        void Verify_ToServiceDefinition_GivenXamlNull(ResourceType resourceType)
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(MakeMessage("test")).Verifiable();

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = null,
                ResourceType = resourceType
            };

            //------------Execute Test---------------------------
            model.ToServiceDefinition();

            //------------Assert Results-------------------------
            repo.Verify(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_GivenXamlPresent_ExpectExistingXamlUsed()
        {
            const string TestCategory = "Test2";
            const string TestXaml = "current xaml";
            // ReSharper disable ImplicitlyCapturedClosure
            Verify_ToServiceDefinition_GivenXamlPresent(ResourceType.WorkflowService, TestCategory, TestXaml, true, serviceElement =>
            // ReSharper restore ImplicitlyCapturedClosure
            {
                var actionElement = serviceElement.Element("Action");
                Assert.IsNotNull(actionElement, "actionElement = null");
                var xamlDefinition = actionElement.Element("XamlDefinition");
                Assert.IsNotNull(xamlDefinition, "xamlDefinition == null");
                Assert.AreEqual(TestXaml, xamlDefinition.Value);

            });
            Verify_ToServiceDefinition_GivenXamlPresent(ResourceType.Source, TestCategory, "<Root><Category>Test</Category><Source>" + TestXaml + "</Source></Root>", true, serviceElement =>
            {
                var category = serviceElement.ElementSafe("Category");
                var source = serviceElement.ElementSafe("Source");
                Assert.AreEqual(TestCategory, category);
                Assert.AreEqual(TestXaml, source);
            });
            // ReSharper disable ImplicitlyCapturedClosure
            Verify_ToServiceDefinition_GivenXamlPresent(ResourceType.Service, TestCategory, "<Root><Category>Test</Category><Source>" + TestXaml + "</Source></Root>", true, serviceElement =>
            // ReSharper restore ImplicitlyCapturedClosure
            {
                var category = serviceElement.ElementSafe("Category");
                Assert.AreEqual(TestCategory, category);
            });
        }

        void Verify_ToServiceDefinition_GivenXamlPresent(ResourceType resourceType, string category, string workflowXaml, bool hasWorkflowXaml, Action<XElement> verify)
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(MakeMessage(workflowXaml));

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = hasWorkflowXaml ? new StringBuilder(workflowXaml) : null,
                ResourceType = resourceType,
                Category = category
            };

            //------------Execute Test---------------------------
            var serviceDefinition = model.ToServiceDefinition();

            //------------Assert Results-------------------------
            var serviceElement = XElement.Parse(serviceDefinition.ToString());
            Assert.IsNotNull(serviceElement);

            verify(serviceElement);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_GivenHasMoreThanOneError_ThenThereShouldBeTwoErrorElements()
        {
            //------------Setup for test--------------------------
            //Setup();
            var model = CreateResourceModel();
            model.AddError(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = Guid.NewGuid(), FixData = "Some fix data" });
            model.AddError(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = Guid.NewGuid(), FixData = "Some fix data" });
            //------------Execute Test---------------------------
            var serviceDefinition = model.ToServiceDefinition();
            //------------Assert Results-------------------------
            var serviceElement = XElement.Parse(serviceDefinition.ToString());
            Assert.IsNotNull(serviceElement);
            var errorMessagesElement = serviceElement.Element("ErrorMessages");
            Assert.IsNotNull(errorMessagesElement);
            Assert.AreEqual(2, errorMessagesElement.Elements().Count());
            List<XElement> xElements = errorMessagesElement.Elements().ToList();
            Assert.AreEqual("Critical error.", xElements[0].Attribute("Message").Value);
            Assert.AreEqual("Warning error.", xElements[1].Attribute("Message").Value);
        }

        static ResourceModel CreateResourceModel(string resourceDefintion = "resource xaml")
        {
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(MakeMessage(resourceDefintion));

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };
            return model;
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_WhenServiceType_ShouldHaveFullServiceDefinition()
        {
            //------------Setup for test--------------------------
            XElement element = new XElement("Action");
            var resourceModel = CreateResourceModel(element.ToStringBuilder().ToString());
            resourceModel.ResourceType = ResourceType.Service;
            resourceModel.ServerResourceType = "WebService";
            resourceModel.WorkflowXaml = null;

            //------------Execute Test---------------------------
            var serviceDefinition = resourceModel.ToServiceDefinition().ToString();
            //------------Assert Results-------------------------
            StringAssert.Contains(serviceDefinition, "ResourceType=\"WebService\"");
            StringAssert.Contains(serviceDefinition, "Service ID=");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_WhenSourceType_ShouldHaveFullServiceDefinition()
        {
            //------------Setup for test--------------------------
            var resourceModel = CreateResourceModel("from resource definition");
            resourceModel.ResourceType = ResourceType.Source;
            resourceModel.ServerResourceType = "WebSource";
            resourceModel.WorkflowXaml = null;

            //------------Execute Test---------------------------
            var serviceDefinition = resourceModel.ToServiceDefinition().ToString();
            //------------Assert Results-------------------------
            StringAssert.Contains(serviceDefinition, "from resource definition");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_Workflow_WhenHasAnAmpersand_ShouldHaveServiceDefinition()
        {
            //------------Setup for test--------------------------
            var resourceModel = CreateResourceModel();
            resourceModel.ResourceType = ResourceType.WorkflowService;
            resourceModel.ServerResourceType = "Workflow";
            resourceModel.WorkflowXaml = new StringBuilder("this has a &");
            //------------Execute Test---------------------------
            var serviceDefinition = resourceModel.ToServiceDefinition().ToString();
            //------------Assert Results-------------------------
            StringAssert.Contains(serviceDefinition, "this has a &amp;");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_Service_WhenHasAnAmpersand_ShouldHaveServiceDefinition()
        {
            //------------Setup for test--------------------------
            var resourceModel = CreateResourceModel();
            resourceModel.ResourceType = ResourceType.Service;
            resourceModel.ServerResourceType = "WebService";
            resourceModel.WorkflowXaml = new StringBuilder("this has a &");
            //------------Execute Test---------------------------
            var serviceDefinition = resourceModel.ToServiceDefinition().ToString();
            //------------Assert Results-------------------------
            StringAssert.Contains(serviceDefinition, "this has a &amp;");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_Source_WhenHasAnAmpersand_ShouldHaveServiceDefinition()
        {
            //------------Setup for test--------------------------
            var resourceModel = CreateResourceModel();
            resourceModel.ResourceType = ResourceType.Source;
            resourceModel.ServerResourceType = "DbSource";
            resourceModel.WorkflowXaml = new StringBuilder("this has a &");
            //------------Execute Test---------------------------
            var serviceDefinition = resourceModel.ToServiceDefinition().ToString();
            //------------Assert Results-------------------------
            StringAssert.Contains(serviceDefinition, "this has a &");
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment()
        {
            return CreateMockEnvironment(new EventPublisher());
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(IEventPublisher eventPublisher)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(model => model.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new StringBuilder());
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);
            return environmentModel;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_DisplayName")]
        public void ResourceModel_DisplayName_IsNullOrEmptyAndResourceTypeIsWorkflowService_Workflow()
        {
            //------------Setup for test--------------------------
            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object)
            {
                ResourceType = ResourceType.WorkflowService
            };

            //------------Execute Test---------------------------
            var displayName = model.DisplayName;

            //------------Assert Results-------------------------
            Assert.AreEqual("Workflow", displayName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_DisplayName")]
        public void ResourceModel_DisplayName_IsNullOrEmptyAndResourceTypeIsNotWorkflowService_ResourceTypeToString()
        {
            //------------Setup for test--------------------------
            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object)
            {
                ResourceType = ResourceType.Service
            };

            //------------Execute Test---------------------------
            var displayName = model.DisplayName;

            //------------Assert Results-------------------------
            Assert.AreEqual("Service", displayName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_GetErrors")]
        public void ResourceModel_GetErrors_ErrorsForInstance()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());

            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1.Object);
            model.AddError(err2.Object);

            //------------Execute Test---------------------------
            var errors = model.GetErrors(instanceID);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(instanceID, errors[0].InstanceID);
        }

        public static ExecuteMessage MakeMessage(string msg)
        {
            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg);

            return result;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_RemoveError")]
        public void ResourceModel_RemoveError_ErrorIsFound_ErrorRemoved()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            IErrorInfo err1 = new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.ReloadMapping
            };

            IErrorInfo err2 = new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Warning,
                FixType = FixType.ReloadMapping
            };

            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1);
            model.AddError(err2);

            //------------Execute Test---------------------------
            model.RemoveError(err1);

            //------------Assert Results-------------------------
            var errors = model.GetErrors(instanceID);

            Assert.AreEqual(1, errors.Count);
            Assert.AreSame(err2, errors[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_RemoveError")]
        public void ResourceModel_RemoveError_ErrorIsNotFound_DoesNothing()
        {
            var instanceID = Guid.NewGuid();

            IErrorInfo err1 = new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.ReloadMapping
            };

            IErrorInfo err2 = new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Warning,
                FixType = FixType.ReloadMapping
            };

            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1);
            model.AddError(err2);

            //------------Execute Test---------------------------
            model.RemoveError(new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.None,
                FixType = FixType.Delete
            });

            //------------Assert Results-------------------------
            var errors = model.GetErrors(instanceID);

            Assert.AreEqual(2, errors.Count);
            Assert.AreSame(err1, errors[0]);
            Assert.AreSame(err2, errors[1]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_RemoveError")]
        public void ResourceModel_RemoveError_ErrorIsNotFoundButErrorExistsWithSameErrorAndFixType_MatchingErrorRemoved()
        {
            var instanceID = Guid.NewGuid();

            IErrorInfo err1 = new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.ReloadMapping
            };

            IErrorInfo err2 = new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Warning,
                FixType = FixType.ReloadMapping
            };

            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1);
            model.AddError(err2);

            //------------Execute Test---------------------------
            model.RemoveError(new ErrorInfo
            {
                InstanceID = instanceID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.ReloadMapping
            });

            //------------Assert Results-------------------------
            var errors = model.GetErrors(instanceID);

            Assert.AreEqual(1, errors.Count);
            Assert.AreSame(err2, errors[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_Environment")]
        public void ResourceModel_Environment_DesignValidationService_WiredUp()
        {
            var eventPublisher = new EventPublisher();

            var environmentID = Guid.NewGuid();
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(e => e.ID).Returns(environmentID);
            environment.Setup(e => e.Connection.ServerEvents).Returns(eventPublisher);

            var instanceID = Guid.NewGuid();

            var model = new ResourceModel(environment.Object, new Mock<IEventAggregator>().Object);

            var errors = model.GetErrors(instanceID);
            Assert.AreEqual(0, errors.Count);

            var err = new ErrorInfo
            {
                InstanceID = instanceID,
            };

            var memo = new DesignValidationMemo
            {
                InstanceID = environmentID,
                Errors = new List<ErrorInfo>
                {
                    err
                }
            };

            //------------Execute Test---------------------------
            eventPublisher.Publish(memo);


            //------------Assert Results-------------------------
            errors = model.GetErrors(instanceID);
            Assert.AreEqual(1, errors.Count);
            Assert.AreSame(err, errors[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ResourceModel_Authorization")]
        public void ResourceModel_Authorization_IsAuthorized_IsCorrectForAllPermissions()
        {
            Verify_Authorization_IsAuthorized(AuthorizationContext.Execute);
            Verify_Authorization_IsAuthorized(AuthorizationContext.View);
        }

        static void Verify_Authorization_IsAuthorized(AuthorizationContext authorizationContext)
        {
            //------------Setup for test--------------------------
            var requiredPermissions = authorizationContext.ToPermissions();
            var model = new ResourceModel(new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object);

            foreach(Permissions permission in Enum.GetValues(typeof(Permissions)))
            {
                model.UserPermissions = permission;
                var expected = (permission & requiredPermissions) != 0;

                //------------Execute Test---------------------------
                var authorized = model.IsAuthorized(authorizationContext);

                //------------Assert Results-------------------------
                Assert.AreEqual(expected, authorized);
            }
        }

        [TestMethod]
        [TestCategory("ResourceModel_PermissionsModifiedService")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_PermissionsModifiedService_ModifiedPermissionsDoesMatchResourceIDAndMemoHasNonEmptyInstanceID_DoesNotUpdateUserPermissions()
        {
            Verify_PermissionsModifiedService_ModifiedPermissionsDoesMatchResourceID(Guid.NewGuid());
        }

        [TestMethod]
        [TestCategory("ResourceModel_PermissionsModifiedService")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_PermissionsModifiedService_ModifiedPermissionsDoesNotMatchResourceID_DoesNotUpdateUserPermissions()
        {
            //------------Setup for test--------------------------
            const Permissions ExpectedPermissions = Permissions.DeployFrom;

            var resourceID = Guid.NewGuid();

            var pubMemo = new PermissionsModifiedMemo();
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = Guid.NewGuid(), Permissions = Permissions.Execute });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = Guid.NewGuid(), Permissions = Permissions.DeployTo });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = resourceID,
                UserPermissions = ExpectedPermissions
            };
            Assert.AreEqual(ExpectedPermissions, model.UserPermissions);

            model.OnPermissionsModifiedReceived += (sender, memo) => Assert.AreEqual(ExpectedPermissions, model.UserPermissions);

            //------------Execute Test---------------------------
            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [TestCategory("ResourceModel_PermissionsModifiedService")]
        [Owner("Tshepo Ntlhokoa")]
        public void ResourceModel_PermissionsModifiedService_MemoIDEqualsEnvironmentServerId_UserPermissionChanges()
        {
            //------------Setup for test--------------------------
            const Permissions ExpectedPermissions = Permissions.Contribute;

            var resourceID = Guid.NewGuid();
            //var connectionServerId = Guid.NewGuid();
            var memoServerID = Guid.NewGuid();

            var pubMemo = new PermissionsModifiedMemo();

            pubMemo.ServerID = memoServerID;
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.Execute });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.DeployTo });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);
            connection.SetupGet(c => c.ServerID).Returns(memoServerID);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(m => m.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Contribute);
            environmentModel.Setup(e => e.AuthorizationService).Returns(authorizationService.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = resourceID,
                UserPermissions = ExpectedPermissions
            };
            Assert.AreEqual(ExpectedPermissions, model.UserPermissions);

            model.OnPermissionsModifiedReceived += (sender, memo) => Assert.AreEqual(ExpectedPermissions, model.UserPermissions);

            //------------Execute Test---------------------------
            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [TestCategory("ResourceModel_PermissionsModifiedService")]
        [Owner("Tshepo Ntlhokoa")]
        public void ResourceModel_PermissionsModifiedService_MemoIDNotEqualsEnvironmentServerId_UserPermissionDoesNotChange()
        {
            //------------Setup for test--------------------------
            const Permissions ExpectedPermissions = Permissions.DeployFrom;

            var resourceID = Guid.NewGuid();
            //var connectionServerId = Guid.NewGuid();
            var memoServerID = Guid.NewGuid();

            var pubMemo = new PermissionsModifiedMemo();

            pubMemo.ServerID = memoServerID;
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.Execute });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.DeployTo });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);
            connection.SetupGet(c => c.ServerID).Returns(Guid.NewGuid());

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(m => m.GetResourcePermissions(It.IsAny<Guid>())).Returns(Permissions.Contribute);
            environmentModel.Setup(e => e.AuthorizationService).Returns(authorizationService.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = resourceID,
                UserPermissions = ExpectedPermissions
            };
            Assert.AreEqual(ExpectedPermissions, model.UserPermissions);

            model.OnPermissionsModifiedReceived += (sender, memo) => Assert.AreEqual(ExpectedPermissions, model.UserPermissions);

            //------------Execute Test---------------------------
            eventPublisher.Publish(pubMemo);
        }

        void Verify_PermissionsModifiedService_ModifiedPermissionsDoesMatchResourceID(Guid instanceID)
        {
            //------------Setup for test--------------------------
            const Permissions ResourcePermissions = Permissions.DeployFrom;
            const Permissions ExpectedPermissions = Permissions.Execute | Permissions.View;

            var resourceID = Guid.NewGuid();
            var pubMemo = new PermissionsModifiedMemo { InstanceID = instanceID };
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.Execute });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = resourceID, Permissions = Permissions.View });
            pubMemo.ModifiedPermissions.Add(new WindowsGroupPermission { ResourceID = Guid.NewGuid(), Permissions = Permissions.DeployTo });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = resourceID,
                UserPermissions = ResourcePermissions
            };
            Assert.AreEqual(ResourcePermissions, model.UserPermissions);

            model.OnPermissionsModifiedReceived += (sender, memo) => Assert.AreEqual(instanceID == Guid.Empty ? ExpectedPermissions : ResourcePermissions, model.UserPermissions);

            //------------Execute Test---------------------------
            eventPublisher.Publish(pubMemo);
        }

    }
}
