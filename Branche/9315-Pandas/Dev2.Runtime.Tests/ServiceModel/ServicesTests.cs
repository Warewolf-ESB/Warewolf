using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using DummyNamespaceForTest;
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
        static object _testGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);
        }
        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
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
            var svc = CreateCountriesDbService();
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
            var svc = CreateCountriesDbService();
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
            var svc = CreateCountriesDbService();
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
            var svc = CreateCountriesDbService();
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
        #region PluginMethods

        [TestMethod]
        public void PluginMethodsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.PluginMethods(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginMethodsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.PluginMethods("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginMethodsWithValidArgsExpectedReturnsList()
        {
            var service = GetSource();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new ServicesMock();
            var result = services.PluginMethods(args, workspaceID, Guid.Empty);

            Assert.AreEqual(7, result.Count);
        }

        #endregion
        #region DbMethods

        [TestMethod]
        public void DbMethodsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.DbMethods(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DbMethodsWithInvalidArgsExpectedReturnsErrorList()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.DbMethods("xxxx", Guid.Empty, Guid.Empty);
            Assert.AreEqual(1, result.Count, "Got " + result);
        }

        [TestMethod]
        public void DbMethodsWithValidArgsExpectedReturnsList()
        {
            var service = CreateCountriesDbService();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new ServicesMock();
            var result = services.DbMethods(args, workspaceID, Guid.Empty);

            // TODO: Fix
            Assert.AreEqual(50, result.Count);
        }

        #endregion

        #region DbTest

        [TestMethod]
        public void DbTestWithNullArgsExpectedReturnsRecordsetWithError()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.DbTest(null, Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void DbTestWithInvalidArgsExpectedReturnsRecordsetWithError()
        {
            var services = new Dev2.Runtime.ServiceModel.Services();
            var result = services.DbTest("xxx", Guid.Empty, Guid.Empty);
            Assert.IsTrue(result.HasErrors);
        }

        [TestMethod]
        public void DbTestWithValidArgsAndNoRecordsetNameExpectedUpdatesRecordsetNameToServiceMethodName()
        {
            var service = CreateCountriesDbService();
            service.Recordset.Name = null;
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            Assert.AreEqual(result.Name, service.Method.Name);
        }

        [TestMethod]
        public void DbTestWithValidArgsAndRecordsetNameExpectedDoesNotUpdateRecordsetNameToServiceMethodName()
        {
            var service = CreateCountriesDbService();
            service.Recordset.Name = "MyCities";
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            Assert.AreEqual(result.Name, service.Recordset.Name);
        }

        [TestMethod]
        public void DbTestWithValidArgsAndRecordsetFieldsExpectedDoesNotAddRecordsetFields()
        {
            var svc = CreateCountriesDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);
            Assert.IsFalse(services.FetchRecordsetAddFields);
            Assert.AreEqual(svc.Recordset.Fields.Count, result.Fields.Count);
        }

        [TestMethod]
        public void DbTestWithValidArgsAndNoRecordsetFieldsExpectedAddsRecordsetFields()
        {
            var svc = CreateCountriesDbService();
            svc.Recordset.Fields.Clear();

            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            services.DbTest(args, workspaceID, Guid.Empty);
            Assert.IsTrue(services.FetchRecordsetAddFields);
        }

        [TestMethod]
        public void DbTestWithValidArgsExpectedFetchesRecordset()
        {
            var svc = CreateCountriesDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            Assert.AreEqual(1, services.FetchRecordsetHitCount);
            Assert.AreEqual(result.Name, svc.Recordset.Name);
            Assert.AreEqual(result.Fields.Count, svc.Recordset.Fields.Count);
        }

        [TestMethod]
        public void DbTestWithValidArgsExpectedClearsRecordsFirst()
        {
            var service = CreateCountriesDbService();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new ServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            Assert.AreEqual(1, services.FetchRecordsetHitCount);
            Assert.AreEqual(0, result.Records.Count);
        }
        #endregion
        #region PluginTest

        [TestMethod]
        public void PluginTestWithNullArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new Dev2.Runtime.ServiceModel.Services();
            //------------Execute Test---------------------------
            var result = services.PluginTest(null, Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsTrue(result[0].HasErrors);
        }

        [TestMethod]
        public void PluginTestWithInvalidArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new Dev2.Runtime.ServiceModel.Services();
            //------------Execute Test---------------------------
            var result = services.PluginTest("xxx", Guid.Empty, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsTrue(result[0].HasErrors);
        }

        [TestMethod]
        public void PluginTestWithValidArgsAndNoRecordsetNameExpectedUpdatesRecordsetNameToServiceMethodName()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyPluginService();
            service.Recordset.Name = null;
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            var services = new ServicesMock();
            var result = services.PluginTest(args, workspaceID, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual(result[0].Name, service.Method.Name);
        }

        [TestMethod]
        public void PluginTestWithValidArgsAndRecordsetNameExpectedDoesNotUpdateRecordsetNameToServiceMethodName()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyPluginService();
            service.Recordset.Name = "Test";
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            var services = new ServicesMock();
            var result = services.PluginTest(args, workspaceID, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual(result[0].Name, service.Recordset.Name);
        }

        [TestMethod]
        public void PluginTestWithValidArgsAndRecordsetFieldsExpectedDoesNotAddRecordsetFields()
        {
            //------------Setup for test--------------------------
            var svc = CreateDummyPluginService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            var services = new ServicesMock();
            var result = services.PluginTest(args, workspaceID, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.IsFalse(services.FetchRecordsetAddFields);
            Assert.AreEqual(svc.Recordset.Fields.Count, result[0].Fields.Count);
        }

        [TestMethod]
        public void PluginTestWithValidArgsAndNoRecordsetFieldsExpectedAddsRecordsetFields()
        {
            //------------Setup for test--------------------------
            var svc = CreateDummyPluginService();
            svc.Recordset.Fields.Clear();
            //------------Execute Test---------------------------
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            //------------Assert Results-------------------------
            var services = new ServicesMock();
            services.PluginTest(args, workspaceID, Guid.Empty);
            Assert.IsTrue(services.FetchRecordsetAddFields);
        }

        [TestMethod]
        public void PluginTestWithValidArgsExpectedFetchesRecordset()
        {
            //------------Setup for test--------------------------
            var svc = CreateDummyPluginService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            var services = new ServicesMock();
            var result = services.PluginTest(args, workspaceID, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, services.FetchRecordsetHitCount);
            Assert.AreEqual(result[0].Name, svc.Recordset.Name);
            Assert.AreEqual(result[0].Fields.Count, svc.Recordset.Fields.Count);
        }

        [TestMethod]
        public void PluginTestWithValidArgsExpectedClearsRecordsFirst()
        {
            //------------Setup for test--------------------------
            var service = CreateDummyPluginService();
            var args = service.ToString();
            var workspaceID = Guid.NewGuid();
            //------------Execute Test---------------------------
            var services = new ServicesMock();
            var result = services.PluginTest(args, workspaceID, Guid.Empty);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, services.FetchRecordsetHitCount);
            Assert.AreEqual(0, result[0].Records.Count);
        }
        #endregion
        #region CreateCountriesDbService

        static Service CreateCountriesDbService()
        {
            var service = new DbService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "CountriesService",
                ResourceType = ResourceType.DbService,
                ResourcePath = "Test",
                Method = new ServiceMethod
                {
                    Name = "dbo.spGetCountries",
                    Parameters = new List<MethodParameter>(new[]
                    {
                        new MethodParameter { Name = "@Prefix", EmptyToNull = false, IsRequired = true, Value = null, DefaultValue = "b" }
                    })
                },
                Recordset = new Recordset
                {
                    Name = "Countries",
                },
                Source = new DbSource
                {

                    ResourceID = Guid.NewGuid(),
                    ResourceName = "CitiesDB",
                    ResourceType = ResourceType.DbSource,
                    ResourcePath = "Test",
                    Server = "RSAKLFSVRGENDEV",
                    DatabaseName = "Cities",
                    AuthenticationType = AuthenticationType.Windows,
                }
            };
            service.Recordset.Fields.AddRange(new[]
            {
                new RecordsetField { Name = "CountryID", Alias = "CountryID" },
                new RecordsetField { Name = "Description", Alias = "Name" }
            });
            return service;
        }

        #endregion
        #region CreateDummyService

        PluginService CreateDummyPluginService()
        {

            var source = GetSource();
            var service = new PluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = ResourceType.PluginService,
                ResourcePath = "Tests",
                Method = new ServiceMethod
                {
                    Name = "DummyMethod"

                },
                Recordset = new Recordset
                {
                    Name = "Test",
                },
                Source = source
            };
            service.Recordset.Fields.AddRange(new[]
            {
                new RecordsetField { Name = "Description", Alias = "Name" }
            });
            return service;
        }

        PluginSource GetSource()
        {
            var type = typeof(DummyClassForPluginTest);
            var assembly = type.Assembly;
            var fullName = assembly.GetName().Name;
            return new PluginSource
            {
                AssemblyLocation = assembly.Location,
                FullName = type.FullName,
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dummy",
                ResourceType = ResourceType.PluginSource,
                ResourcePath = "Test",
            };
        }


        #endregion
    }
}
namespace DummyNamespaceForTest
{

    public class DummyClassForPluginTest
    {
        public string Name { get; set; }
        public DummyClassForPluginTest DummyMethod()
        {
            return new DummyClassForPluginTest
            {
                Name = "test data"
            };
        }
    }
}