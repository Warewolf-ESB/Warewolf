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
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Providers.Events;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Studio.Interfaces.Enums;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ResourceModelTest
    {
        void Setup()
        {
            var environmentModel = CreateMockEnvironment(new Mock<IEventPublisher>().Object);


            new ResourceModel(environmentModel.Object)
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_Update_WhenWorkflowXamlChanged_ExpectUpdatedResourceModelWithNewXaml()
        {
            //------------Setup for test--------------------------
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_DataListPropertyWhereChangedToSameString_NotifyPropertyChangedNotFiredTwice()
        {
            //------------Setup for test--------------------------
            var testEnvironmentModel = CreateMockEnvironment();
            var resourceModel = new ResourceModel(testEnvironmentModel.Object);
            var timesFired = 0;
            var dataListFired = 0;
            resourceModel.PropertyChanged += (sender, args) =>
            {
                timesFired++;
            };
            resourceModel.OnDataListChanged += () =>
                {
                    dataListFired++;
                };
            //------------Execute Test---------------------------
            resourceModel.DataList = "TestDataList";
            resourceModel.DataList = "TestDataList";
            //------------Assert Results-------------------------
            Assert.AreEqual(1, timesFired);
            Assert.AreEqual(1, dataListFired);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_OnWorkflowSaved_IsWorkflowchangedWherePropertyUpdated_FireOnWorkflowSaved()
        {
            //------------Setup for test--------------------------
            Setup();
            var testEnvironmentModel = CreateMockEnvironment(EventPublishers.Studio);
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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

            var environmentModel = CreateMockEnvironment(new Mock<IEventPublisher>().Object);

            var resourceModel = new ResourceModel(environmentModel.Object)
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

            var eventWasFired = false;
            resourceModel.OnDataListChanged += () =>
                {
                    eventWasFired = true;
                };
            resourceModel.DataList = newDataList;

            var result = resourceModel.DataList;

            var xe = resourceModel.WorkflowXaml.ToXElement();
            var dlElms = xe.Elements("DataList");

            var firstOrDefault = dlElms.FirstOrDefault();
            if (firstOrDefault != null)
            {
                var wfResult = firstOrDefault.ToString(SaveOptions.None);
                FixBreaks(ref result, ref wfResult);
                Assert.AreEqual(result, wfResult);
                Assert.IsTrue(eventWasFired);
            }
            else
            {
                Assert.Fail();
            }
        }

        void FixBreaks(ref string expected, ref string actual)
        {
            expected = new StringBuilder(expected).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
            actual = new StringBuilder(actual).Replace(Environment.NewLine, "\n").Replace("\r", "").ToString();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        [Description("Design validation memo errors must be added to the errors list.")]
        public void ResourceModel_DesignValidationServicePublishingMemo_UpdatesErrors()
        {
            var instanceID = Guid.NewGuid();
            var pubMemo = new DesignValidationMemo { InstanceID = instanceID };
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = "Critical error.", InstanceID = instanceID });
            pubMemo.Errors.Add(new ErrorInfo { ErrorType = ErrorType.Warning, Message = "Warning error.", InstanceID = instanceID });

            var eventPublisher = new EventPublisher();
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IServer>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };

            model.OnDesignValidationReceived += (sender, memo) =>
            {
                Assert.AreEqual(memo.Errors.Count, model.Errors.Count, "OnDesignValidationReceived did not update the number of errors correctly.");

                foreach (var error in memo.Errors)
                {
                    var error1 = error;
                    var modelError = model.Errors.FirstOrDefault(me => me.ErrorType == error1.ErrorType && me.Message == error1.Message);
                    Assert.AreSame(error, modelError, "OnDesignValidationReceived did not set the error.");
                }
            };

            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        [Description("Design validation memo errors must be added to the errors list.")]
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

            var environmentModel = new Mock<IServer>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);

            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };

            model.OnDesignValidationReceived += (sender, memo) => Assert.AreEqual(0, model.Errors.Count, "OnDesignValidationReceived did not update the number of errors correctly.");

            eventPublisher.Publish(pubMemo);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        [Description("Resource model rollback must restore fixed errors.")]
        public void ResourceModel_Rollback_FixedErrorsRestored()
        {
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
                switch (hitCount++)
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
                    default:
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        [Description("Resource model commit must not restore fixed errors.")]
        public void ResourceModel_Commit_FixedErrorsNotRestored()
        {
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
                switch (hitCount++)
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
                    default:
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_ToServiceDefinition_WorkflowXaml_IsNull()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
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
            Assert.IsNull(serviceDefinition);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(MakeMessage("test")).Verifiable();

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
            repo.Verify(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_ToServiceDefinition_GivenXamlPresent_ExpectExistingXamlUsed()
        {
            const string TestCategory = "Test2";
            const string TestXaml = "current xaml";

            Verify_ToServiceDefinition_GivenXamlPresent(ResourceType.WorkflowService, TestCategory, TestXaml, true, serviceElement =>

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
        }

        void Verify_ToServiceDefinition_GivenXamlPresent(ResourceType resourceType, string category, string workflowXaml, bool hasWorkflowXaml, Action<XElement> verify)
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(MakeMessage(workflowXaml));

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

            verify?.Invoke(serviceElement);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_ToServiceDefinition_GivenHasMoreThanOneError_ThenThereShouldBeTwoErrorElements()
        {
            //------------Setup for test--------------------------
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
            var xElements = errorMessagesElement.Elements().ToList();
            Assert.AreEqual("Critical error.", xElements[0].Attribute("Message").Value);
            Assert.AreEqual("Warning error.", xElements[1].Attribute("Message").Value);
        }

        static ResourceModel CreateResourceModel(string resourceDefintion = "resource xaml")
        {
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(r => r.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(MakeMessage(resourceDefintion));

            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var model = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID
            };
            return model;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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

        public static Mock<IServer> CreateMockEnvironment()
        {
            return CreateMockEnvironment(new EventPublisher());
        }

        public static Mock<IServer> CreateMockEnvironment(IEventPublisher eventPublisher)
        {
            var connection = new Mock<IEnvironmentConnection>();
            connection.Setup(model => model.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>())).Returns(new StringBuilder());
            connection.Setup(e => e.ServerEvents).Returns(eventPublisher);

            var environmentModel = new Mock<IServer>();
            environmentModel.Setup(e => e.Connection).Returns(connection.Object);
            return environmentModel;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_DisplayName_IsNullOrEmptyAndResourceTypeIsWorkflowService_Workflow()
        {
            //------------Setup for test--------------------------
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object)
            {
                ResourceType = ResourceType.WorkflowService
            };

            //------------Execute Test---------------------------
            var displayName = model.DisplayName;

            //------------Assert Results-------------------------
            Assert.AreEqual("Workflow", displayName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_DisplayName_IsNullOrEmptyAndResourceTypeIsNotWorkflowService_ResourceTypeToString()
        {
            //------------Setup for test--------------------------
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object)
            {
                ResourceType = ResourceType.Service
            };

            //------------Execute Test---------------------------
            var displayName = model.DisplayName;

            //------------Assert Results-------------------------
            Assert.AreEqual("Service", displayName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_GetErrors_ErrorsForInstance()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1.Object);
            model.AddError(err2.Object);

            //------------Execute Test---------------------------
            var errors = model.GetErrors(instanceID);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(instanceID, errors[0].InstanceID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_ClearErrors_ClearsErrors()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1.Object);
            model.AddError(err2.Object);

            var errorInfos = model.Errors;

            //------------Assert Preconditions-------------------------
            Assert.AreEqual(2, errorInfos.Count);
            //------------Execute Test---------------------------
            model.ClearErrors();
            //-------------Assert Results------------------------
            errorInfos = model.Errors;
            Assert.AreEqual(0, errorInfos.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_IDataErrorInfo_ThisAccessor_ResourceName()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1.Object);
            model.AddError(err2.Object);
            //------------Execute Test---------------------------
            var errMessage = model["ResourceName"];
            //-------------Assert Results------------------------
            Assert.IsNotNull(errMessage);
            Assert.AreEqual("Please enter a name for this resource", errMessage);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_IDataErrorInfo_ThisAccessor_HelpLink()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object) { HelpLink = "somePath" };
            //------------Execute Test---------------------------
            var errMsg = model["HelpLink"];
            //-------------Assert Results------------------------
            Assert.IsNotNull(errMsg);
            Assert.AreEqual("The help link is not in a valid format", errMsg);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_ClearErrors_FiresPropertyChangeForErrors()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1.Object);
            model.AddError(err2.Object);
            var _propertyChangedFired = false;
            model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Errors")
                {
                    _propertyChangedFired = true;
                }
            };
            //------------Execute Test---------------------------
            model.ClearErrors();
            //-------------Assert Results------------------------
            Assert.IsTrue(_propertyChangedFired);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_ClearErrors_FiresPropertyChangeFoIsValid()
        {
            //------------Setup for test--------------------------
            var instanceID = Guid.NewGuid();

            var err1 = new Mock<IErrorInfo>();
            err1.Setup(e => e.InstanceID).Returns(instanceID);
            var err2 = new Mock<IErrorInfo>();
            err2.Setup(e => e.InstanceID).Returns(Guid.NewGuid());
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
            model.AddError(err1.Object);
            model.AddError(err2.Object);
            var _propertyChangedFired = false;
            model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsValid")
                {
                    _propertyChangedFired = true;
                }
            };
            //------------Execute Test---------------------------
            model.ClearErrors();
            //-------------Assert Results------------------------
            Assert.IsTrue(_propertyChangedFired);
        }

        public static ExecuteMessage MakeMessage(string msg)
        {
            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg);

            return result;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
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
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_Environment_DesignValidationService_WiredUp()
        {
            var eventPublisher = new EventPublisher();

            var environmentID = Guid.NewGuid();
            var environment = new Mock<IServer>();
            environment.Setup(e => e.EnvironmentID).Returns(environmentID);
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
                Errors = new List<IErrorInfo>
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_Authorization_IsAuthorized_IsCorrectForAllPermissions()
        {
            Verify_Authorization_IsAuthorized(AuthorizationContext.Execute);
            Verify_Authorization_IsAuthorized(AuthorizationContext.View);
        }

        static void Verify_Authorization_IsAuthorized(AuthorizationContext authorizationContext)
        {
            //------------Setup for test--------------------------
            var requiredPermissions = authorizationContext.ToPermissions();
            var model = new ResourceModel(new Mock<IServer>().Object, new Mock<IEventAggregator>().Object);

            foreach (Permissions permission in Enum.GetValues(typeof(Permissions)))
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_Default_Properties()
        {
            var resourceModel = new ResourceModel();

            Assert.IsFalse(resourceModel.HasErrors);

            Assert.IsFalse(resourceModel.IsDatabaseService);
            Assert.IsFalse(resourceModel.IsPluginService);
            Assert.IsFalse(resourceModel.IsResourceService);
            Assert.IsFalse(resourceModel.IsVersionResource);
            Assert.IsFalse(resourceModel.IsNewWorkflow);
            Assert.IsFalse(resourceModel.IsNotWarewolfPath);
            Assert.IsFalse(resourceModel.IsOpeningFromOtherDir);
            Assert.IsNull(resourceModel.UnitTestTargetWorkflowService);

            resourceModel.IsDatabaseService = true;
            resourceModel.IsPluginService = true;
            resourceModel.IsResourceService = true;
            resourceModel.IsVersionResource = true;
            resourceModel.IsNewWorkflow = true;
            resourceModel.IsNotWarewolfPath = true;
            resourceModel.IsOpeningFromOtherDir = true;
            resourceModel.UnitTestTargetWorkflowService = "Target Workflow Service";

            Assert.IsTrue(resourceModel.IsDatabaseService);
            Assert.IsTrue(resourceModel.IsPluginService);
            Assert.IsTrue(resourceModel.IsResourceService);
            Assert.IsTrue(resourceModel.IsVersionResource);
            Assert.IsTrue(resourceModel.IsNewWorkflow);
            Assert.IsTrue(resourceModel.IsNotWarewolfPath);
            Assert.IsTrue(resourceModel.IsOpeningFromOtherDir);
            Assert.AreEqual("Target Workflow Service", resourceModel.UnitTestTargetWorkflowService);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        [ExpectedException(typeof(Exception))]
        public void ResourceModel_ToServiceDefinition_InvalidResourceType_ThrowsException()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ExecuteMessage());
            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var resourceModel = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = null,
                ResourceType = ResourceType.Unknown
            };

            //------------Execute Test---------------------------
            var serviceDefinition = resourceModel.ToServiceDefinition();

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_GetSavePath()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var instanceID = Guid.NewGuid();
            var resourceModel = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = null,
                ResourceType = ResourceType.WorkflowService
            };

            var savePath = resourceModel.GetSavePath();
            Assert.AreEqual("", savePath);

            resourceModel.Category = "Item\\Resource";
            resourceModel.ResourceName = "Resource";

            savePath = resourceModel.GetSavePath();
            Assert.AreEqual("Item\\", savePath);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_GetWorkflowXaml_NotNull()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var instanceID = Guid.NewGuid();
            var resourceModel = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = null,
                ResourceType = ResourceType.WorkflowService
            };

            var xaml = new StringBuilder("new xaml");

            resourceModel.WorkflowXaml = xaml;

            var workflowXaml = resourceModel.GetWorkflowXaml();
            Assert.AreEqual(xaml, workflowXaml);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResourceModel))]
        public void ResourceModel_GetWorkflowXaml_IsNull()
        {
            //------------Setup for test--------------------------
            Setup();
            var eventPublisher = new EventPublisher();
            var environmentModel = CreateMockEnvironment(eventPublisher);

            var msg = MakeMessage("test");

            var repo = new Mock<IResourceRepository>();
            repo.Setup(repository => repository.FetchResourceDefinition(It.IsAny<IServer>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>())).Returns(msg);
            environmentModel.Setup(e => e.ResourceRepository).Returns(repo.Object);

            var instanceID = Guid.NewGuid();
            var resourceModel = new ResourceModel(environmentModel.Object)
            {
                ID = instanceID,
                WorkflowXaml = null,
                ResourceType = ResourceType.WorkflowService
            };

            var workflowXaml = resourceModel.GetWorkflowXaml();
            Assert.AreEqual(msg.Message, workflowXaml);
        }
    }
}
