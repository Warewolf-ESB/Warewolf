using System;
using System.IO;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI: 801
    // BUG: 8477

    [TestClass]
    public class ServicesTests
    {
        const string SourceName = "BobsDB";
        const string ServiceName = "BobsCustomers";
        const string ServiceResourceType = "SqlDatabase";
        const string JsonSource = "{{\"ResourceID\":\"{0}\",\"ResourceType\":\"SqlDatabase\",\"ResourceName\":\"" + SourceName + "\",\"ResourcePath\":\"Bob\"}}";
        const string JsonService = "{{\"resourceID\":\"{0}\",\"resourceType\":\"" + ServiceResourceType + "\",\"resourceName\":\"" + ServiceName + "\",\"resourcePath\":\"Bob\",\"source\":{1}}}";

        [TestMethod]
        public void TestMockOutput()
        {
            //var services = new Dev2.Runtime.ServiceModel.Services();
            //string s  = services.ExtractCodedEntities("server=RSAKLFSVRGENDEV;database=Dev2TestingDB;integrated security=false;User Id=testuser;Password=test123");
            //Assert.AreEqual(s,s);

            //string expected = "[{\"Name\":\"Action1\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]},{\"Name\":\"Action2\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]},{\"Name\":\"Action3\",\"ActionType\":\"Unknown\",\"SourceName\":null,\"SourceMethod\":null,\"OutputDescription\":null,\"ServiceActionInputs\":[],\"ServiceActionOutputs\":[]}]";
            //string actual = services.Actions("", Guid.Empty, Guid.Empty); ;

            //Assert.AreEqual(expected, actual);
        }

        #region Save

        [TestMethod]
        public void SaveWithNullArgsExpectedReturnsErrorValidationResult()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Save(null, Guid.Empty, Guid.Empty);
            var validationResult = JsonConvert.DeserializeObject<ValidationResult>(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void SaveWithInvalidArgsExpectedReturnsErrorValidationResult()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Save("xxxxx", Guid.Empty, Guid.Empty);
            var validationResult = JsonConvert.DeserializeObject<ValidationResult>(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void SaveWithValidArgsAndEmptyResourceIDExpectedAssignsNewResourceID()
        {
            var jsonSource = string.Format(JsonSource, Guid.NewGuid());
            var jsonService = string.Format(JsonService, Guid.Empty, jsonSource);

            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                var result = services.Save(jsonService, workspaceID, Guid.Empty);
                var service = JsonConvert.DeserializeObject<Service>(result);
                Assert.AreNotEqual(Guid.Empty, service.ResourceID);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        [TestMethod]
        public void SaveWithValidArgsAndResourceIDExpectedDoesNotAssignNewResourceID()
        {
            var serviceID = Guid.NewGuid();
            var jsonSource = string.Format(JsonSource, Guid.NewGuid());
            var jsonService = string.Format(JsonService, serviceID, jsonSource);

            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                var result = services.Save(jsonService, workspaceID, Guid.Empty);
                var service = JsonConvert.DeserializeObject<Service>(result);
                Assert.AreEqual(serviceID, service.ResourceID);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        [TestMethod]
        public void SaveWithValidArgsExpectedSavesXmlToDisk()
        {
            var serviceID = Guid.NewGuid();
            var jsonSource = string.Format(JsonSource, Guid.NewGuid());
            var jsonService = string.Format(JsonService, serviceID, jsonSource);

            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, Dev2.Runtime.ServiceModel.Resources.RootFolders[enSourceType.SqlDatabase]);
            var fileName = String.Format("{0}\\{1}.xml", path, ServiceName);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                services.Save(jsonService, workspaceID, Guid.Empty);
                var exists = File.Exists(fileName);

                Assert.IsTrue(exists);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
        {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        #endregion

        #region Get

        [TestMethod]
        public void GetWithNullArgsExpectedReturnsNewService()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Get(null, Guid.Empty, Guid.Empty);

            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void GetWithInvalidArgsExpectedReturnsNewService()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Get("xxxxx", Guid.Empty, Guid.Empty);

            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void GetWithValidArgsExpectedReturnsService()
        {
            var serviceID = Guid.NewGuid();
            var jsonSource = string.Format(JsonSource, Guid.NewGuid());
            var jsonService = string.Format(JsonService, serviceID, jsonSource);
            var args = string.Format("{{\"resourceID\":\"{0}\",\"resourceType\":\"{1}\"}}", serviceID, ServiceResourceType);

            var workspaceID = Guid.NewGuid();
            var workspacePath = GlobalConstants.GetWorkspacePath(workspaceID);
            try
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
                var saveResult = services.Save(jsonService, workspaceID, Guid.Empty);
                var saveService = JsonConvert.DeserializeObject<Service>(saveResult);
                var getResult = services.Get(args, workspaceID, Guid.Empty);
                Assert.AreEqual(saveService.ResourceID, getResult.ResourceID);
                Assert.AreEqual(saveService.ResourceName, getResult.ResourceName);
                Assert.AreEqual(saveService.ResourcePath, getResult.ResourcePath);
                Assert.AreEqual(saveService.ResourceType, getResult.ResourceType);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    Directory.Delete(workspacePath, true);
                }
            }
        }

        #endregion
    }
}
