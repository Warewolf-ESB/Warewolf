using System;
using System.IO;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI: 801
    // BUG: 8477

    /// <author>trevor.williams-ros</author>
    /// <date>2013/02/13</date>
    [TestClass]
    public class ServicesTests
    {
        static readonly object TestGuard = new object();

        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(TestGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(TestGuard);
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
            var svc = DbServicesTests.CreateCountriesDbService();
            svc.ResourceID = Guid.Empty;
            var args = svc.ToString();

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                var result = services.Save(args, workspaceID, Guid.Empty);
                var service = JsonConvert.DeserializeObject<Service>(result);
                Assert.AreNotEqual(Guid.Empty, service.ResourceID);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        [TestMethod]
        public void SaveWithValidArgsAndResourceIDExpectedDoesNotAssignNewResourceID()
        {
            var svc = DbServicesTests.CreateCountriesDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                var result = services.Save(args, workspaceID, Guid.Empty);
                var service = JsonConvert.DeserializeObject<Service>(result);
                Assert.AreEqual(svc.ResourceID, service.ResourceID);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        [TestMethod]
        public void SaveWithValidArgsExpectedSavesXmlToDisk()
        {
            var svc = DbServicesTests.CreateCountriesDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            var path = Path.Combine(workspacePath, Dev2.Runtime.ServiceModel.Resources.RootFolders[ResourceType.DbService]);
            var fileName = String.Format("{0}\\{1}.xml", path, svc.ResourceName);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                services.Save(args, workspaceID, Guid.Empty);
                var exists = File.Exists(fileName);

                Assert.IsTrue(exists);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
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
            var svc = DbServicesTests.CreateCountriesDbService();
            var saveArgs = svc.ToString();
            var getArgs = string.Format("{{\"resourceID\":\"{0}\",\"resourceType\":\"{1}\"}}", svc.ResourceID, ResourceType.DbService);

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var services = new Dev2.Runtime.ServiceModel.Services();
                services.Save(saveArgs, workspaceID, Guid.Empty);
                var getResult = services.Get(getArgs, workspaceID, Guid.Empty);
                Assert.AreEqual(svc.ResourceID, getResult.ResourceID);
                Assert.AreEqual(svc.ResourceName, getResult.ResourceName);
                Assert.AreEqual(svc.ResourcePath, getResult.ResourcePath);
                Assert.AreEqual(svc.ResourceType, getResult.ResourceType);
            }
            finally
            {
                if(Directory.Exists(workspacePath))
                {
                    DirectoryHelper.CleanUp(workspacePath);
                }
            }
        }

        #endregion


    }
}