using System;
using System.Collections.Generic;
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
        const string MethodName = "dbo.Pr_GetCake_05";

        const string JsonMethod = "\"methodName\":\"" + MethodName + "\",\"methodParameters\":[{{\"Name\":\"@Param1\",\"EmptyToNull\":false,\"IsRequired\":false,\"Value\":\"\",\"DefaultValue\":\"\"}}],\"methodRecordset\":{{\"Name\":\"\",\"Fields\":[],\"Records\":[]}}";
        const string JsonSource = "{{\"resourceID\":\"{0}\",\"resourceType\":\"SqlDatabase\",\"resourceName\":\"" + SourceName + "\",\"resourcePath\":\"Bob\"}}";
        const string JsonService = "{{\"resourceID\":\"{0}\",\"resourceType\":\"" + ServiceResourceType + "\",\"resourceName\":\"" + ServiceName + "\",\"resourcePath\":\"Bob\",\"source\":{1}," + JsonMethod + "}}";

        #region Methods

        [TestMethod]
        public void MethodsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Methods(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void MethodsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Methods("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void MethodsWithValidArgsExpectedReturnsList()
        {
            var jsonSource = string.Format(JsonSource, Guid.NewGuid());
            var jsonService = string.Format(JsonService, Guid.Empty, jsonSource);

            var workspaceID = Guid.NewGuid();
            GlobalConstants.GetWorkspacePath(workspaceID);

            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Methods(jsonService, workspaceID, Guid.Empty);

            Assert.AreEqual(50, result.Count);
        }
        #endregion

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

        #region Test

        [TestMethod]
        public void TestWithNullArgsExpectedReturnsRecordsetWithError()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Test(null, Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void TestWithInvalidArgsExpectedReturnsRecordsetWithError()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.Test("xxx", Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void TestWithValidArgsAndNoRecordsetNameExpectedUpdatesRecordsetNameToServiceMethodName()
        {
            var service = CreateCountriesDbService();
            service.MethodRecordset.Name = null;
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.Test(args, workspaceID, Guid.Empty);

            Assert.AreEqual(result.Name, service.MethodName);
        }

        [TestMethod]
        public void TestWithValidArgsAndRecordsetNameExpectedDoesNotUpdateRecordsetNameToServiceMethodName()
        {
            var service = CreateCountriesDbService();
            service.MethodRecordset.Name = "MyCities";
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.Test(args, workspaceID, Guid.Empty);

            Assert.AreEqual(result.Name, service.MethodRecordset.Name);
        }

        [TestMethod]
        public void TestWithValidArgsAndRecordsetFieldsExpectedDoesNotAddRecordsetFields()
        {
            var service = CreateCountriesDbService();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.Test(args, workspaceID, Guid.Empty);
            Assert.IsFalse(services.FetchRecordsetUpdateFields);
            Assert.AreEqual(service.MethodRecordset.Fields.Count, result.Fields.Count);
        }

        [TestMethod]
        public void TestWithValidArgsAndNoRecordsetFieldsExpectedAddsRecordsetFields()
        {
            var service = CreateCountriesDbService();
            service.MethodRecordset.Fields.Clear();

            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            services.Test(args, workspaceID, Guid.Empty);
            Assert.IsTrue(services.FetchRecordsetUpdateFields);
        }

        [TestMethod]
        public void TestWithValidArgsExpectedFetchesRecordset()
        {
            var service = CreateCountriesDbService();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.Test(args, workspaceID, Guid.Empty);

            Assert.AreEqual(1, services.FetchRecordsetHitCount);
            Assert.AreEqual(result.Name, service.MethodRecordset.Name);
            Assert.AreEqual(result.Fields.Count, service.MethodRecordset.Fields.Count);
        }

        #endregion

        #region CreateDbService

        static Service CreateCountriesDbService()
        {
            var service = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CountriesService",
                ResourceType = enSourceType.SqlDatabase,
                ResourcePath = "Test",
                MethodName = "dbo.spGetCountries",
                MethodParameters = new List<MethodParameter>(new[]
                {
                    new MethodParameter("@Prefix", false, true, null, "b")
                }),
                MethodRecordset = new Recordset
                {
                    Name = "Countries",
                },
                Source = new DbSource
                {

                    ResourceID = Guid.NewGuid(),
                    ResourceName = "CitiesDB",
                    ResourceType = enSourceType.SqlDatabase,
                    ResourcePath = "Test",
                    Server = "RSAKLFSVRGENDEV",
                    Database = "Cities",
                    AuthenticationType = AuthenticationType.Windows,
                }
            };
            service.MethodRecordset.Fields.AddRange(new[]
            {
                new RecordsetField{Name = "CountryID", Alias = "CountryID"}, 
                new RecordsetField{Name = "Description", Alias = "Name"}
            });
            return service;
        }

        #endregion

    }
}
