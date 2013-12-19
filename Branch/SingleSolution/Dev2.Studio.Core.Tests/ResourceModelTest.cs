using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework;

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
            var authorRoles = "TestAuthorRoles";
            var category = "TestCat";
            var comment = "TestComment";
            var displayName = "DisplayName";
            var resourceName = "TestResourceName";
            var id = Guid.NewGuid();
            var tags = "TestTags";
            resourceModel.AuthorRoles = authorRoles;
            resourceModel.Category = category;
            resourceModel.Comment = comment;
            resourceModel.DisplayName = displayName;
            resourceModel.ID = id;
            resourceModel.ResourceName = resourceName;
            resourceModel.Tags = tags;
            resourceModel.WorkflowXaml = new StringBuilder("new xaml");
            //------------Execute Test---------------------------
            var updateResourceModel = new ResourceModel(environmentModel.Object);
            updateResourceModel.WorkflowXaml = new StringBuilder("old xaml");
            updateResourceModel.Update(resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual("new xaml", updateResourceModel.WorkflowXaml.ToString());
        }

        [TestMethod]
        public void UpdateResourceModelExpectPropertiesUpdated()
        {
            //------------Setup for test--------------------------
            Setup();
            var environmentModel = CreateMockEnvironment(new EventPublisher());
            var resourceModel = new ResourceModel(environmentModel.Object);
            var authorRoles = "TestAuthorRoles";
            var category = "TestCat";
            var comment = "TestComment";
            var displayName = "DisplayName";
            var resourceName = "TestResourceName";
            var id = Guid.NewGuid();
            var tags = "TestTags";
            resourceModel.AuthorRoles = authorRoles;
            resourceModel.Category = category;
            resourceModel.Comment = comment;
            resourceModel.DisplayName = displayName;
            resourceModel.ID = id;
            resourceModel.ResourceName = resourceName;
            resourceModel.Tags = tags;
            //------------Execute Test---------------------------
            var updateResourceModel = new ResourceModel(environmentModel.Object);
            updateResourceModel.Update(resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(authorRoles, updateResourceModel.AuthorRoles);
            Assert.AreEqual(category, updateResourceModel.Category);
            Assert.AreEqual(comment, updateResourceModel.Comment);
            Assert.AreEqual(displayName, updateResourceModel.DisplayName);
            Assert.AreEqual(id, updateResourceModel.ID);
            Assert.AreEqual(tags, updateResourceModel.Tags);
        }
        #endregion Update Tests

        [TestMethod]
        public void ResourceModel_UnitTest_DataListPropertyWhereChangedToSameString_NotifyPropertyChangedNotFiredTwice()
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
        public void OnWorkflowSaved_UnitTest_IsWorkflowchangedWherePropertyUpdated_FireOnWorkflowSaved()
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
        public void DataList_Setter_ExpectUpdatedDataListSectionInServiceDefinition()
        {
            Setup();
            string newDataList = @"<DataList>
  <Country />
  <State />
  <City>
    <Name />
    <GeoLocation />
  </City>
</DataList>";
            _resourceModel.DataList = newDataList;

            string result = _resourceModel.DataList;
            Assert.AreEqual(new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(_resourceModel.WorkflowXaml.ToString()).GetValue("DataList"), result);

        }


        [TestMethod]
        public void ConstructResourceModelExpectIsWorkflowSaved()
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
        public void ResourceModel_UnitTest_DesignValidationServicePublishingMemo_UpdatesErrors()
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
        public void ResourceModel_UnitTest_DesignValidationServicePublishingMemo_NoInstanceID_DoesNotUpdatesErrors()
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
        public void ResourceModel_UnitTest_Rollback_FixedErrorsRestored()
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
        public void ResourceModel_UnitTest_Commit_FixedErrorsNotRestored()
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
        public void ResourceModel_ToServiceDefinition_GivenXamlNull_ExpectFetchOfXaml()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(MakeMessage("resource xaml"));

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };
            
            //------------Execute Test---------------------------
            var serviceDefinition = model.ToServiceDefinition().ToString();
            //------------Assert Results-------------------------
            var serviceElement = XElement.Parse(serviceDefinition);
            Assert.IsNotNull(serviceElement);

            var actionElement = serviceElement.Element("Action");
            var xmalElement = actionElement.Element("XamlDefinition");

            Assert.AreEqual("resource xaml", xmalElement.Value);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_GivenXamlPresent_ExpectExistingXamlUsed()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(MakeMessage("resource xaml"));

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = new StringBuilder("current xaml")
            };

            //------------Execute Test---------------------------
            var serviceDefinition = model.ToServiceDefinition();
            //------------Assert Results-------------------------
            var serviceElement = XElement.Parse(serviceDefinition.ToString());
            Assert.IsNotNull(serviceElement);

            var actionElement = serviceElement.Element("Action");
            var xmalElement = actionElement.Element("XamlDefinition");

            Assert.AreEqual("current xaml", xmalElement.Value);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ResourceModel_ToServiceDefinition")]
        public void ResourceModel_ToServiceDefinition_GivenHasMoreThanOneError_ThenThereShouldBeTwoErrorElements()
        {
            //------------Setup for test--------------------------
            //Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IEnvironmentModel>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(MakeMessage("resource xaml"));

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };
            model.AddError(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = Guid.NewGuid(), FixData = "Some fix data" });
            model.AddError(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = Guid.NewGuid(), FixData = "Some fix data" });
            //------------Execute Test---------------------------
            var serviceDefinition = model.ToServiceDefinition();
            //------------Assert Results-------------------------
            var serviceElement = XElement.Parse(serviceDefinition.ToString());
            Assert.IsNotNull(serviceElement);
            var errorMessagesElement = serviceElement.Element("ErrorMessages");
            Assert.IsNotNull(errorMessagesElement);
            Assert.AreEqual(2,errorMessagesElement.Elements().Count());
            List<XElement> xElements = errorMessagesElement.Elements().ToList();
            Assert.AreEqual("Critical error.", xElements[0].Attribute("Message").Value);
            Assert.AreEqual("Warning error.", xElements[1].Attribute("Message").Value);
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

        public static ExecuteMessage MakeMessage(string msg)
        {
            var result = new ExecuteMessage() {HasError = false};
            result.SetMessage(msg);

            return result;
        }


    }
}
