/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class DataColumnMappingTests
    {
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_GivenObject_Equal_False()
        {
            var dataColumn = new DataColumnMapping { };
            var other = new object();
            var dataColumnEqual = dataColumn.Equals(other);
            Assert.IsFalse(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_GivenObject_Equal_Null()
        {
            object dataColumn = new DataColumnMapping { };
            object other = null;
            var dataColumnEqual = dataColumn.Equals(other);
            Assert.IsFalse(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_GivenObject_Equals_ShouldReturnTrue()
        {
            object dataColumn = new DataColumnMapping { };
            object other = new DataColumnMapping { };
            var dataColumnEqual = dataColumn.Equals(other);
            Assert.IsTrue(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_GivenObjectDataColumnMapping_Equals_ShouldReturnTrue()
        {
            object dataColumn = new DataColumnMapping { };
            object other = dataColumn;
            var dataColumnEqual = dataColumn.Equals(other);
            Assert.IsTrue(dataColumnEqual);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_GetHashCode()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                IndexNumber = 1,
                InputColumn = "a",
                OutputColumn = new DbColumn()
                {
                    ColumnName = "a",
                    DataType = typeof(string),
                    MaxLength = 1,
                    IsNullable = false,
                    IsAutoIncrement = false
                }
            };

            var actual = dataColumnMapping.GetHashCode();
            Assert.AreNotEqual(0, actual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Inserted()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                IndexNumber = 1,
                InputColumn = "a",
                Inserted = true
            };
            Assert.AreEqual(true, dataColumnMapping.Inserted);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Validate_NoRulesSetup()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                InputColumn = "a"
            };
            Assert.AreEqual(0, dataColumnMapping.GetRuleSet("InputColumn", "").Rules.Count);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_ClearRow()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                InputColumn = "a"
            };
            Assert.IsFalse(string.IsNullOrEmpty(dataColumnMapping.InputColumn));
            dataColumnMapping.ClearRow();
            Assert.IsFalse(string.IsNullOrEmpty(dataColumnMapping.InputColumn));
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_CanAdd_CanRemove()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                InputColumn = "a"
            };
            var canAdd = dataColumnMapping.CanAdd();
            var canRemove = dataColumnMapping.CanRemove();
            Assert.IsFalse(canAdd);
            Assert.IsFalse(canRemove);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Equal_True()
        {
            var dataColumn = new DataColumnMapping { };
            var other = dataColumn;
            var dataColumnEqual = dataColumn.Equals(other);
            Assert.IsTrue(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Equal_Null()
        {
            var dataColumn = new DataColumnMapping { };
            var dataColumnEqual = dataColumn.Equals(null);
            Assert.IsFalse(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Equal_OutputColumn_Null()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                OutputColumn = null
            };
            var other = new DataColumnMapping()
            {
                OutputColumn = null
            };
            var dataColumnEqual = dataColumnMapping.Equals(other);
            Assert.IsTrue(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Equal_OutputColumn_notNull()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                IndexNumber = 1,
                InputColumn = "a",
                Inserted = true,
                OutputColumn = new DbColumn()
                {
                    ColumnName = "a",
                    DataType = typeof(string),
                    MaxLength = 1,
                    IsNullable = false,
                    IsAutoIncrement = false
                }
            };
            var other = new DataColumnMapping()
            {
                IndexNumber = 1,
                InputColumn = "a",
                Inserted = true,
                OutputColumn = new DbColumn()
                {
                    ColumnName = "a",
                    DataType = typeof(string),
                    MaxLength = 1,
                    IsNullable = false,
                    IsAutoIncrement = false
                }
            };
            var dataColumnEqual = dataColumnMapping.Equals(other);
            Assert.IsTrue(dataColumnEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_GetHashCode_Return0()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                IndexNumber = 0,
                InputColumn = null,
                OutputColumn = null
            };

            var actual = dataColumnMapping.GetHashCode();
            Assert.AreEqual(0, actual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("DataColumnMapping")]
        public void DataColumnMapping_Equal_ColumnsDifferent_ReturnFalse()
        {
            var dataColumnMapping = new DataColumnMapping()
            {
                IndexNumber = 1,
                InputColumn = "a",
                Inserted = true,
                OutputColumn = new DbColumn()
                {
                    ColumnName = "a",
                    DataType = typeof(string),
                    MaxLength = 1,
                    IsNullable = false,
                    IsAutoIncrement = false
                }
            };
            var other = new DataColumnMapping()
            {
                IndexNumber = 2,
                InputColumn = "b",
                Inserted = false,
                OutputColumn = new DbColumn()
                {
                    ColumnName = "b",
                    DataType = typeof(string),
                    MaxLength = 1,
                    IsNullable = false,
                    IsAutoIncrement = false
                }
            };
            var dataColumnEqual = dataColumnMapping.Equals(other);
            Assert.IsFalse(dataColumnEqual);
        }
    }
}
