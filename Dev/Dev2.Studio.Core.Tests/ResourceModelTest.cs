using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core.Models;
using Unlimited.Framework;
using Dev2.Studio.Core.Interfaces;
using Moq;

namespace Dev2.Core.Tests {
    [TestClass]
    public class ResourceModelTest {
        private IResourceModel _resourceModel;

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            Mock<IEnvironmentModel> _testEnvironmentModel = new Mock<IEnvironmentModel>();
            _testEnvironmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");

            _resourceModel = new ResourceModel(_testEnvironmentModel.Object); 
            _resourceModel.ResourceName = "test";
            _resourceModel.ResourceType = ResourceType.Service;
            _resourceModel.ServiceDefinition = @"
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
";            
        }

        #endregion Test Initialization

        #region Update Tests

        [TestMethod]
        public void UpdateResourceModelExpectPropertiesUpdated()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> _testEnvironmentModel = new Mock<IEnvironmentModel>();
            _testEnvironmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            var resourceModel = new ResourceModel(_testEnvironmentModel.Object);
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
            var updateResourceModel = new ResourceModel(_testEnvironmentModel.Object);
            updateResourceModel.Update(resourceModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(authorRoles,updateResourceModel.AuthorRoles);
            Assert.AreEqual(category,updateResourceModel.Category);
            Assert.AreEqual(comment,updateResourceModel.Comment);
            Assert.AreEqual(displayName,updateResourceModel.DisplayName);
            Assert.AreEqual(id,updateResourceModel.ID);
            Assert.AreEqual(tags,updateResourceModel.Tags);
        }
        #endregion Update Tests

        #region ToServiceDefinition Tests


        #endregion ToServiceDefinition Tests

        #region DataList Tests

        [TestMethod]
        public void DataList_Setter_ExpectUpdatedDataListSectionInServiceDefinition() {
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
            Assert.AreEqual(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(_resourceModel.ServiceDefinition).GetValue("DataList"), result);

        }


        [TestMethod]
        public void ConstructResourceModelExpectIsWorkflowSaved()
        {
            //------------Setup for test--------------------------
            Mock<IEnvironmentModel> _testEnvironmentModel = new Mock<IEnvironmentModel>();
            _testEnvironmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            //------------Execute Test---------------------------
            var resourceModel = new ResourceModel(_testEnvironmentModel.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(resourceModel.IsWorkflowSaved);
        }
        #endregion DataList Tests
    }
}
