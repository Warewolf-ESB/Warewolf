using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Tests.Runtime.Poco;
using HgCo.WindowsLive.SkyDrive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Tests.Runtime.ESB.Brokers
{
    // BUG 9500 - 2013.05.31 - TWR : refactored to this class
    [TestClass]
    public class PluginBrokerTests
    {
        #region ApplyMappings


        [TestMethod]
        public void PluginBrokerApplyMappingsWithNullResultExpectedNoException()
        {
            //Initialization
            var myBroker = new PluginBroker();
            object pluginResult = null;

            //Execute
            var dataSourceShape = myBroker.TestPluginResult(pluginResult);

            //Assert
            Assert.AreEqual(0, dataSourceShape.DataSourceShapes[0].Paths.Count);
        }

        #endregion

        #region TestPluginResult

        // BUG 9626 - 2013.06.11 - TWR: added
        [TestMethod]
        public void PluginServicesTestPluginResultWithValidPocoExpectedAddsRecordsetFields()
        {
            var broker = new PluginBroker();
            var poco = PocoTestFactory.CreateCompany();

            #region poco should yield these paths

            /*             
                Paths[0] {Name}
                Paths[1] {Departments.Capacity}
                Paths[2] {Departments.Count}
                Paths[3] {Departments().Name}
                Paths[4] {Departments().Employees.Capacity}
                Paths[5] {Departments().Employees.Count}
                Paths[6] {Departments().Employees().Name}
            */

            #endregion

            var result = broker.TestPluginResult(poco);
            var recordsets = result.ToRecordsetList();

            Assert.AreEqual(3, recordsets.Count);
            foreach(var rs in recordsets)
            {
                switch(rs.Name)
                {
                    case "":
                        Assert.AreEqual(3, rs.Fields.Count);
                        VerifyField(rs.Fields, "Name", "Name", "Name");
                        VerifyField(rs.Fields, "DepartmentsCapacity", "DepartmentsCapacity", "Departments.Capacity");
                        VerifyField(rs.Fields, "DepartmentsCount", "DepartmentsCount", "Departments.Count");
                        break;
                    case "Departments":
                        Assert.AreEqual(3, rs.Fields.Count);
                        VerifyField(rs.Fields, "Name", "Name", "Departments().Name");
                        VerifyField(rs.Fields, "EmployeesCapacity", "EmployeesCapacity", "Departments().Employees.Capacity");
                        VerifyField(rs.Fields, "EmployeesCount", "EmployeesCount", "Departments().Employees.Count");
                        break;
                    case "Departments_Employees":
                        Assert.AreEqual(1, rs.Fields.Count);
                        VerifyField(rs.Fields, "Name", "Name", "Departments().Employees().Name");
                        break;
                    default:
                        Assert.Fail("Extra Recordset: " + rs.Name);
                        break;
                }
            }
        }

        #endregion

        static void VerifyField(IEnumerable<RecordsetField> fields, string name, string alias, string path)
        {
            var field = fields.FirstOrDefault(f => f.Name == name);
            if(field == null)
            {
                Assert.Fail("Field not found: " + name);
            }
            Assert.AreEqual(name, field.Name);
            Assert.AreEqual(alias, field.Alias);
            Assert.AreEqual(path, field.Path.ActualPath);

        }

    }
}
