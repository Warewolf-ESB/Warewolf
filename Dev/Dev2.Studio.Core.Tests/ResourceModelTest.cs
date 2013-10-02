using System;
using System.Linq;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Framework;

namespace Dev2.Core.Tests
{
    [TestClass]
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
            environmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");

            _resourceModel = new ResourceModel(environmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                ServiceDefinition = @"
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
"
            };
        }

        #endregion Test Initialization

        #region Update Tests

        [TestMethod]
        public void UpdateResourceModelExpectPropertiesUpdated()
        {
            //------------Setup for test--------------------------
            Setup();
            var environmentModel = CreateMockEnvironment(new EventPublisher());
            environmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
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
        public void ResourceModel_UnitTesty_DataListPropertyWhereChangedToSameString_NotifyPropertyChangedNotFiredTwice()
        {
            //------------Setup for test--------------------------
            Setup();
            Mock<IEnvironmentModel> _testEnvironmentModel = new Mock<IEnvironmentModel>();
            var resourceModel = new ResourceModel(_testEnvironmentModel.Object);
            var timesFired = 0;
            resourceModel.PropertyChanged += (sender, args) => 
            {
                timesFired++;
            };
            //------------Execute Test---------------------------
            resourceModel.DataList = "TestDataList";
            resourceModel.DataList = "TestDataList";
            //------------Assert Results-------------------------
            Assert.AreEqual(1,timesFired);
        }

        [TestMethod]
        public void OnWorkflowSaved_UnitTest_IsWorkflowchangedWherePropertyUpdated_FireOnWorkflowSaved()
        {
            //------------Setup for test--------------------------
            Setup();
            Mock<IEnvironmentModel> _testEnvironmentModel = new Mock<IEnvironmentModel>();
            var resourceModel = new ResourceModel(_testEnvironmentModel.Object);
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
            Assert.AreSame(resourceModel,eventResourceModel);
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
            Assert.AreEqual(new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(_resourceModel.ServiceDefinition).GetValue("DataList"), result);

        }


        [TestMethod]
        public void ConstructResourceModelExpectIsWorkflowSaved()
        {
            //------------Setup for test--------------------------
            Setup();
            var environmentModel = CreateMockEnvironment(new Mock<IEventPublisher>().Object);
            environmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
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
            Setup();
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
            Setup();
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

            model.OnDesignValidationReceived += (sender, memo) =>
            {
                Assert.AreEqual(memo.Errors.Count, model.Errors.Count, "OnDesignValidationReceived did not update the number of errors correctly.");
                Assert.AreEqual(2, model.Errors.Count, "OnDesignValidationReceived did not update the number of errors correctly.");                
            };

            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [TestCategory("ResourceModel_Rollback")]
        [Description("Resource model rollback must restore fixed errors.")]
        [Owner("Trevor Williams-Ros")]
        public void ResourceModel_UnitTest_Rollback_FixedErrorsRestored()
        {
            Setup();
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
            Setup();
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
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.",InstanceID = instanceID});
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = instanceID });
            eventPublisher.Publish(pubMemo);

            // Fix 1 error and publish
            pubMemo.Errors.RemoveAt(1);
            eventPublisher.Publish(pubMemo);
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment()
        {
            return CreateMockEnvironment(new EventPublisher());
        }

        public static Mock<IEnvironmentModel> CreateMockEnvironment(IEventPublisher eventPublisher)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IEnvironmentModel>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);
            return environmentModel;
        }

        
    }
}
