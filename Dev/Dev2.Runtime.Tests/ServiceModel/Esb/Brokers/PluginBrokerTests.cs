
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using Dev2.Runtime.ServiceModel.Data;
//using Dev2.Runtime.ServiceModel.Esb.Brokers;
//using Dev2.Tests.Runtime.Poco;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Dev2.Tests.Runtime.ESB.Brokers
//{
//    // BUG 9500 - 2013.05.31 - TWR : refactored to this class
//    [TestClass]
//    [ExcludeFromCodeCoverage]
//    public class PluginBrokerTests
//    {
//        #region ApplyMappings


//        [TestMethod]
//        public void PluginBrokerApplyMappingsWithNullResultExpectedNoException()
//        {
//            //Initialization
//            var myBroker = new PluginBroker();
//            object pluginResult = null;

//            //Execute
//            var dataSourceShape = myBroker.TestPlugin(pluginResult);

//            //Assert
//            Assert.AreEqual(0, dataSourceShape.DataSourceShapes[0].Paths.Count);
//        }

//        #endregion

//        #region TestPluginResult

//        // BUG 9626 - 2013.06.11 - TWR: added
//        [TestMethod]
//        public void PluginServicesTestPluginResultWithValidPocoExpectedAddsRecordsetFields()
//        {
//            var broker = new PluginBroker();
//            var poco = PocoTestFactory.CreateCompany();

//            #region poco should yield these paths

//            /*             
//                Paths[0] {Name}
//                Paths[1] {Departments.Capacity}
//                Paths[2] {Departments.Count}
//                Paths[3] {Departments().Name}
//                Paths[4] {Departments().Employees.Capacity}
//                Paths[5] {Departments().Employees.Count}
//                Paths[6] {Departments().Employees().Name}
//            */

//            #endregion

//            var result = broker.TestPlugin(poco);

//            // It appears as though the result.ToRecordsetList() is causing the sway ;)

//            var recordsets = result.ToRecordsetList();

//            Assert.AreEqual(3, recordsets.Count);
//            foreach(var rs in recordsets)
//            {
//                switch(rs.Name)
//                {
//                    case "":
//                        Assert.AreEqual(3, rs.Fields.Count);
//                        VerifyField(rs.Fields, "Name", "Name", "Name");
//                        VerifyField(rs.Fields, "DepartmentsCapacity", "DepartmentsCapacity", "Departments.Capacity");
//                        VerifyField(rs.Fields, "DepartmentsCount", "DepartmentsCount", "Departments.Count");
//                        break;
//                    case "Departments":
//                        Assert.AreEqual(3, rs.Fields.Count);
//                        VerifyField(rs.Fields, "Name", "Name", "Departments().Name");
//                        VerifyField(rs.Fields, "EmployeesCapacity", "EmployeesCapacity", "Departments().Employees.Capacity");
//                        VerifyField(rs.Fields, "EmployeesCount", "EmployeesCount", "Departments().Employees.Count");
//                        break;
//                    case "Departments_Employees":
//                        Assert.AreEqual(1, rs.Fields.Count);
//                        VerifyField(rs.Fields, "Name", "Name", "Departments().Employees().Name");
//                        break;
//                    default:
//                        Assert.Fail("Extra Recordset: " + rs.Name);
//                        break;
//                }
//            }
//        }


//        [TestMethod]
//        [Owner("Travis Frisinger")]
//        [TestCategory("PluginBroker_TestPlugin")]
//        public void PluginBroker_TestPlugin_WhenPocoInput_ValidUserPaths()
//        {
//            //------------Setup for test--------------------------

//            var broker = new PluginBroker();
//            var poco = PocoTestFactory.CreateCompany();

//            //------------Execute Test---------------------------

//            var result = broker.TestPlugin(poco);

//            //------------Assert Results-------------------------
//            var shapeList = result.DataSourceShapes[0];

//            // --- Assert Display Paths
//            Assert.AreEqual("Name", shapeList.Paths[0].DisplayPath);
//            Assert.AreEqual("Departments.Capacity", shapeList.Paths[1].DisplayPath);
//            Assert.AreEqual("Departments.Count", shapeList.Paths[2].DisplayPath);
//            Assert.AreEqual("Departments().Name", shapeList.Paths[3].DisplayPath);
//            Assert.AreEqual("Departments().Employees.Capacity", shapeList.Paths[4].DisplayPath);
//            Assert.AreEqual("Departments().Employees.Count", shapeList.Paths[5].DisplayPath);
//            Assert.AreEqual("Departments.Employees().Name", shapeList.Paths[6].DisplayPath);

//            // --- Assert Actual Paths
//            Assert.AreEqual("Name", shapeList.Paths[0].ActualPath);
//            Assert.AreEqual("Departments.Capacity", shapeList.Paths[1].ActualPath);
//            Assert.AreEqual("Departments.Count", shapeList.Paths[2].ActualPath);
//            Assert.AreEqual("Departments().Name", shapeList.Paths[3].ActualPath);
//            Assert.AreEqual("Departments().Employees.Capacity", shapeList.Paths[4].ActualPath);
//            Assert.AreEqual("Departments().Employees.Count", shapeList.Paths[5].ActualPath);
//            Assert.AreEqual("Departments().Employees().Name", shapeList.Paths[6].ActualPath);

//        }

//        [TestMethod]
//        [Owner("Travis Frisinger")]
//        [TestCategory("PluginBroker_TestPlugin")]
//        public void PluginBroker_TestPlugin_WhenXmlInput_ValidXmlPaths()
//        {
//            //------------Setup for test--------------------------
//            var pluginBroker = new PluginBroker();

//            #region XML Fragment
//            const string xmlFragment = @"<Company Name='Dev2'>
//    <Motto>Eat lots of cake</Motto>
//    <PreviousMotto/>
// <Departments TestAttrib='testing'>
//  <Department Name='Dev'>
//   <Employees>
//    <Person Name='Brendon' Surename='Page' />
//    <Person Name='Jayd' Surename='Page' />
//   </Employees>
//  </Department>
//  <Department Name='Accounts'>
//   <Employees>
//    <Person Name='Bob' Surename='Soap' />
//    <Person Name='Joe' Surename='Pants' />
//   </Employees>
//  </Department>
// </Departments>
//    <InlineRecordSet>
//        RandomData
//    </InlineRecordSet>
//    <InlineRecordSet>
//        RandomData1
//    </InlineRecordSet>
//    <OuterNestedRecordSet>
//        <InnerNestedRecordSet ItemValue='val1' />
//        <InnerNestedRecordSet ItemValue='val2' />
//    </OuterNestedRecordSet>
//    <OuterNestedRecordSet>
//        <InnerNestedRecordSet ItemValue='val3' />
//        <InnerNestedRecordSet ItemValue='val4' />
//    </OuterNestedRecordSet>
//</Company>";
//            #endregion

//            List<string> expectedActualPaths = new List<string>
//                                                                {
//                                                                    "Company:Name",
//                                                                    "Company.Motto",
//                                                                    "Company.PreviousMotto",
//                                                                    "Company.Departments:TestAttrib",
//                                                                    "Company.Departments().Department:Name",
//                                                                    "Company.Departments().Department.Employees().Person:Name",
//                                                                    "Company.Departments().Department.Employees().Person:Surename",
//                                                                    "Company().InlineRecordSet",
//                                                                    "Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue"
//                                                                 };

//            List<string> expectedUserVisiblePaths = new List<string>
//                                                                {
//                                                                    "Company:Name",
//                                                                    "Company.Motto",
//                                                                    "Company.PreviousMotto",
//                                                                    "Company.Departments:TestAttrib",
//                                                                    "Company.Departments().Department:Name",
//                                                                    "Company.Departments.Department.Employees().Person:Name",
//                                                                    "Company.Departments.Department.Employees().Person:Surename",
//                                                                    "Company().InlineRecordSet",
//                                                                    "Company.OuterNestedRecordSet().InnerNestedRecordSet:ItemValue"
//                                                                 };

//            //------------Execute Test---------------------------

//            var result = pluginBroker.TestPlugin(xmlFragment);

//            //------------Assert Results-------------------------

//            var xmlPaths = result.DataSourceShapes[0].Paths;

//            List<string> resultActualPaths = xmlPaths.Select(value => value.ActualPath).ToList();

//            List<string> resultUserVisiblePaths = xmlPaths.Select(value => value.DisplayPath).ToList();

//            CollectionAssert.AreEqual(expectedActualPaths, resultActualPaths);

//            CollectionAssert.AreEqual(expectedUserVisiblePaths, resultUserVisiblePaths);
//        }

//        [TestMethod]
//        [Owner("Travis Frisinger")]
//        [TestCategory("PluginBroker_TestPlugin")]
//        public void PluginBroker_TestPlugin_WhenJsonInput_ValidJsonPaths()
//        {
//            //------------Setup for test--------------------------
//            var pluginBroker = new PluginBroker();

//            #region JSON Fragment
//            const string jsonFragment = @"{
//    ""Name"": ""Dev2"",
//    ""Motto"": ""Eat lots of cake"",
//    ""Departments"": [      
//        {
//          ""Name"": ""Dev"",
//          ""Employees"": [
//              {
//                ""Name"": ""Brendon"",
//                ""Surename"": ""Page""
//              },
//              {
//                ""Name"": ""Jayd"",
//                ""Surename"": ""Page""
//              }
//            ]
//        },
//        {
//          ""Name"": ""Accounts"",
//          ""Employees"": [
//              {
//                ""Name"": ""Bob"",
//                ""Surename"": ""Soap""
//              },
//              {
//                ""Name"": ""Joe"",
//                ""Surename"": ""Pants""
//              }
//            ]
//        }
//      ],
//    ""Contractors"": [      
//        {
//          ""Name"": ""Roofs Inc."",
//          ""PhoneNumber"": ""123"",
//        },
//        {
//          ""Name"": ""Glass Inc."",
//          ""PhoneNumber"": ""1234"",
//        },
//        {
//          ""Name"": ""Doors Inc."",
//          ""PhoneNumber"": ""1235"",
//        },
//        {
//          ""Name"": ""Cakes Inc."",
//          ""PhoneNumber"": ""1236"",
//        }
//      ],
//    ""PrimitiveRecordset"": [
//      ""
//        RandomData
//    "",
//      ""
//        RandomData1
//    ""
//    ],
//  }";
//            #endregion

//            List<string> expectedExpressions = new List<string>
//                                                                {
//                                                                    "Name",
//                                                                    "Motto",
//                                                                    "PrimitiveRecordset()",
//                                                                    "Departments().Name",
//                                                                    "Departments().Employees().Name",
//                                                                    "Departments().Employees().Surename",
//                                                                    "Contractors().Name",
//                                                                    "Contractors().PhoneNumber"
//                                                                 };

//            //------------Execute Test---------------------------

//            var result = pluginBroker.TestPlugin(jsonFragment);

//            //------------Assert Results-------------------------

//            var jsonPaths = result.DataSourceShapes[0].Paths;

//            List<string> resultExpressions = jsonPaths.Select(value => value.ActualPath).ToList();


//            CollectionAssert.AreEqual(expectedExpressions, resultExpressions);
//        }

//        #endregion


//        static void VerifyField(IEnumerable<RecordsetField> fields, string name, string alias, string path)
//        {
//            var field = fields.FirstOrDefault(f => f.Name == name);
//            if(field == null)
//            {
//                Assert.Fail("Field not found: " + name);
//            }
//            Assert.AreEqual(name, field.Name);
//            Assert.AreEqual(alias, field.Alias);
//            Assert.AreEqual(path, field.Path.ActualPath);

//        }

//    }
//}
