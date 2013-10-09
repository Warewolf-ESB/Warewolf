using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.SqlBulkInsert
{
    [TestClass]
    public class SqlBulkInsertDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SqlBulkInsertDesignerViewModel_Constructor_EnvironmentModelIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new SqlBulkInsertDesignerViewModel(CreateModelItem(), null);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_Constructor")]
        public void SqlBulkInsertDesignerViewModel_Constructor_InitializesProperties()
        {
            //------------Setup for test--------------------------
            const int DatabaseCount = 2;
            var databases = CreateDatabaseList(DatabaseCount);

            //------------Execute Test---------------------------
            var viewModel = CreateViewModel(databases);


            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.ModelItemCollection);
            Assert.IsNotNull(viewModel.EditDatabaseCommand);
            Assert.IsNotNull(viewModel.RefreshTablesCommand);
            Assert.IsNotNull(viewModel.Databases);
            Assert.IsNotNull(viewModel.Tables);
            Assert.IsNull(viewModel.Database);
            Assert.IsFalse(viewModel.CanEditDatabase);

            Assert.AreEqual("InputMappings", viewModel.CollectionName);
            Assert.AreEqual(DatabaseCount + 1, viewModel.Databases.Count); 
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CanEditDatabase")]
        public void SqlBulkInsertDesignerViewModel_CanEditDatabase_DatabaseIsNull_False()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases);

            viewModel.Database = databases[0];

            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.CanEditDatabase);

            //------------Execute Test---------------------------
            viewModel.Database = null;

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Database);
            Assert.IsFalse(viewModel.CanEditDatabase);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_CanEditDatabase")]
        public void SqlBulkInsertDesignerViewModel_CanEditDatabase_DatabaseIsNotNull_True()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases);

            //------------Execute Test---------------------------
            viewModel.Database = databases[0];

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.CanEditDatabase);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged")]
        public void SqlBulkInsertDesignerViewModel_OnModelItemPropertyChanged_PropertyNameIsDatabase_LoadsDatabaseTables()
        {
            //------------Setup for test--------------------------
            var databases = CreateDatabaseList(2);
            var viewModel = CreateViewModel(databases);

            var tableNames = new List<string>
            {
                "table1",
                "table2",
                "table3"
            };


            //------------Execute Test---------------------------
            viewModel.Database = databases[0];

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Database);
            Assert.IsTrue(viewModel.CanEditDatabase);
        }

        static TestSqlBulkInsertDesignerViewModel CreateViewModel(IEnumerable<DbSource> sources = null)
        {
            var sourceDefs = sources == null ? null : sources.Select(s => s.ToXml().ToString());
            var envModel = new Mock<IEnvironmentModel>();
            envModel.Setup(e => e.Connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(string.Format("<XmlData>{0}</XmlData>", sourceDefs == null ? "" : string.Join("\n", sourceDefs)));

            var modelItem = CreateModelItem();
            return new TestSqlBulkInsertDesignerViewModel(modelItem, envModel.Object);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSqlBulkInsertActivity());
        }

        static List<DbSource> CreateDatabaseList(int count)
        {
            var result = new List<DbSource>();
            for(var i = 0; i < count; i++)
            {
                result.Add(new DbSource { ResourceID = Guid.NewGuid(), ResourceName = "Db1" });
            }
            return result;
        }

    }
}
