using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class DbServicesTests
    {
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

            var services = new DbServicesMock();
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

            var services = new DbServicesMock();
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

            var services = new DbServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            Assert.AreEqual(result.Name, service.Recordset.Name);
        }

        [TestMethod]
        public void DbTestWithValidArgsAndRecordsetFieldsExpectedDoesNotAddRecordsetFields()
        {
            var svc = CreateCountriesDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new DbServicesMock();
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

            var services = new DbServicesMock();
            services.DbTest(args, workspaceID, Guid.Empty);
            Assert.IsTrue(services.FetchRecordsetAddFields);
        }

        [TestMethod]
        public void DbTestWithValidArgsExpectedFetchesRecordset()
        {
            var svc = CreateCountriesDbService();
            var args = svc.ToString();
            var workspaceID = Guid.NewGuid();

            var services = new DbServicesMock();
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

            var services = new DbServicesMock();
            var result = services.DbTest(args, workspaceID, Guid.Empty);

            Assert.AreEqual(1, services.FetchRecordsetHitCount);
            Assert.AreEqual(0, result.Records.Count);
        }
        #endregion

        #region CreateCountriesDbService

        public static DbService CreateCountriesDbService()
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
    }
}
