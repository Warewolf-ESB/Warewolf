#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Storage.Interfaces;
namespace Warewolf.Storage.Tests
{
    [TestClass]
    public class WarewolfListIteratorTests
    {
        IWarewolfIterator _expr1;
        IWarewolfIterator _expr2;
        IWarewolfIterator _expr3;
        IExecutionEnvironment _environment;
        IWarewolfListIterator _warewolfListIterator;

        const string Result = "[[Result]]";

        [TestInitialize]
        public void TestInitialize()
        {
            _environment = new ExecutionEnvironment();

            _environment.Assign("[[rec().a]]", "1", 0);
            _environment.Assign("[[rec().a]]", "2", 0);
            _environment.Assign("[[rec().a]]", "3", 0);
            _environment.Assign("[[rec().a]]", "4", 0);
            _environment.Assign(Result, "Success", 0);

            _environment.CommitAssign();
            _warewolfListIterator = new WarewolfListIterator();


            _expr1 = new WarewolfIterator(_environment.Eval(Result, 0));
            _expr2 = new WarewolfIterator(_environment.Eval("[[rec().a]]", 0));

            _warewolfListIterator.AddVariableToIterateOn(_expr1);
            _warewolfListIterator.AddVariableToIterateOn(_expr2);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Instance_ShouldHaveConstructor()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn");
            var count = privateObj.GetField("_count");
            Assert.IsNotNull(variablesToIterateOn);
            Assert.AreEqual(-1, count);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void GivenResultExpression_WarewolfListIterator_FetchNextValue_Should()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);

            var result = variablesToIterateOn.FirstOrDefault();
            var fetchNextValue = _warewolfListIterator.FetchNextValue(result);
            Assert.AreEqual("Success", fetchNextValue);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void GivenPosition_WarewolfListIterator_FetchNextValue_Should()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            var fetchNextValue = listIterator.FetchNextValue(0);
            Assert.AreEqual("Success", fetchNextValue);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_AddVariableToIterateOn_Should()
        {
            _expr3 = new WarewolfIterator(_environment.Eval("[[RecSet()]]", 0));
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            Assert.AreEqual(2, variablesToIterateOn.Count);
            _warewolfListIterator.AddVariableToIterateOn(_expr3);
            Assert.AreEqual(3, variablesToIterateOn.Count);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetMax_ShouldReturn1()
        {
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "1" ,0);
            env.Assign("[[rec().a]]", "2", 0);
            env.Assign("[[rec().a]]", "3", 0);
            env.Assign("[[rec().a]]", "4", 0);
            env.CommitAssign();

            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);

                var max = warewolfListIterator.GetMax();
                Assert.AreEqual(1, max);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Read_ShouldReturnTrue()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            Assert.AreEqual(2, listIterator.FieldCount);
            var read = listIterator.Read();
            Assert.IsTrue(read);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_NextResult_ShouldReturnFalse()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            Assert.IsFalse(listIterator.IsClosed);
            var nextResult = listIterator.NextResult();
            Assert.IsFalse(nextResult);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_HasMoreData_ShouldReturnTrue()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var hasMoreData = _warewolfListIterator.HasMoreData();
            Assert.IsTrue(hasMoreData);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetName_ShouldReturnString()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            var count = privateObj.GetField("_count");
            Assert.AreEqual(count, listIterator.Depth);
            var dataTypeName = listIterator.GetName(0);
            Assert.AreEqual("", dataTypeName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetDataTypeName_ShouldReturnTrue()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            var dataTypeName = listIterator.GetDataTypeName(0);
            Assert.AreEqual("String", dataTypeName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetFieldType_ShouldReturnTrue()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            var fieldType = listIterator.GetFieldType(0);
            Assert.AreEqual(typeof(string), fieldType);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetOrdinal_ShouldReturn0()
        {
            var names = new List<string> { Result };
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            listIterator.Names = names;
            listIterator.Types = variablesToIterateOn.Select(iterator => iterator.GetType()).ToList();
            var ordinal = listIterator.GetOrdinal(Result);
            Assert.AreEqual(0, ordinal);
            Assert.IsNotNull(listIterator.Types);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void GivenStringTrueIsAssigned_WarewolfListIterator_GetBoolean_ShouldReturnTrue()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "True", 0);
            if (AssignExpression(out WarewolfListIterator listIterator))
            {
                return;
            }

            var boolean = listIterator.GetBoolean(2);
            Assert.IsTrue(boolean);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetChars_ShouldReturn0()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "SomeValue", 0);
            if (AssignExpression(out WarewolfListIterator listIterator))
            {
                return;
            }

            var chars = listIterator.GetChars(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<char[]>(), It.IsAny<int>(), It.IsAny<int>());
            Assert.AreEqual(0, chars);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetStrings_ShouldReturnStringValue()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "SomeValue", 0);
            if (AssignExpression(out WarewolfListIterator listIterator))
            {
                return;
            }

            var strig = listIterator.GetString(2);
            Assert.AreEqual("SomeValue", strig);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetDateTime_ShouldReturnDateFormat()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "01/01/2000", 0);
            if (AssignExpression(out WarewolfListIterator listIterator))
            {
                return;
            }

            var toDate = Convert.ToDateTime("01/01/2000");
            var dateTime = listIterator.GetDateTime(2);
            Assert.AreEqual(toDate, dateTime);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetDataAndGetSchemaTable_ShouldReturnNull()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "", 0);
            if (AssignExpression(out WarewolfListIterator listIterator))
            {
                return;
            }

            var dataReader = listIterator.GetData(2);
            Assert.IsNull(dataReader);
            var schemaTable = listIterator.GetSchemaTable();
            Assert.IsNull(schemaTable);
            var isDbNull = listIterator.IsDBNull(2);
            Assert.IsFalse(isDbNull);
        }

        bool AssignExpression(out WarewolfListIterator listIterator)
        {
            _expr3 = new WarewolfIterator(_environment.Eval("[[RecSet().a]]", 0));
            _warewolfListIterator.AddVariableToIterateOn(_expr3);
            listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return true;
            }

            Assert.AreEqual(0, listIterator.RecordsAffected);
            return false;
        }

        void ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn)
        {
            Assert.IsNotNull(_warewolfListIterator);
            privateObj = new PrivateObject(_warewolfListIterator);
            variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_GetGuid_ShouldReturn0()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "00d1c07e-7fa7-4127-a85f-3ae9aaa7c6de", 0);
            _expr3 = new WarewolfIterator(_environment.Eval("[[RecSet().a]]", 0));
            _warewolfListIterator.AddVariableToIterateOn(_expr3);
            var listIterator = _warewolfListIterator as WarewolfListIterator;
            if (listIterator == null)
            {
                return;
            }

            var guid = listIterator.GetGuid(2);
            Assert.IsNotNull(guid);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_IDataRecordFunctions_Should()
        {
            ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "1", 0);
            if (AssignExpression(out WarewolfListIterator listIterator))
            {
                return;
            }

            var bytes = listIterator.GetBytes(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<byte[]>(), It.IsAny<int>(),
                It.IsAny<int>());
            Assert.AreEqual(0, bytes);
            var aToChar = Convert.ToChar("1");
            var cha = listIterator.GetChar(2);
            Assert.AreEqual(aToChar, cha);
            var aToByte = Convert.ToByte("1");
            var bite = listIterator.GetByte(2);
            Assert.AreEqual(aToByte, bite);
            var aToDouble = Convert.ToDouble("1");
            var duble = listIterator.GetDouble(2);
            Assert.AreEqual(aToDouble, duble);
            var aToDecimal = Convert.ToDecimal("1");
            var desimal = listIterator.GetDecimal(2);
            Assert.AreEqual(aToDecimal, desimal);
            var aToFloat = Convert.ToSingle("1");
            var flot = listIterator.GetFloat(2);
            Assert.AreEqual(aToFloat, flot);
            var aToInt64 = Convert.ToInt64("1");
            var log = listIterator.GetInt64(2);
            Assert.AreEqual(aToInt64, log);
            var aToInt32 = Convert.ToInt32("1");
            var innt = listIterator.GetInt32(2);
            Assert.AreEqual(aToInt32, innt);
            var aToInt16 = Convert.ToInt16("1");
            var shot = listIterator.GetInt16(2);
            Assert.AreEqual(aToInt16, shot);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WarewolfListIterator_FetchNextValue_NoValuesToIterateOn_ReturnsException()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
                //------------Execute Test---------------------------
                var value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.IsNull(value);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_FetchNextValue_HasValues_ShouldReturnValue()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
                //------------Execute Test---------------------------
                var value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.AreEqual("Test5", value);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_FetchNextValue_WithStar_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec(*).a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
                //------------Execute Test---------------------------
                var value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.AreEqual("Test", value);
                //------------Execute Test---------------------------
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.AreEqual("Test2", value);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_FetchNextValue_WithIndex_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            //------------Execute Test---------------------------
            string value;
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec(3).a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.AreEqual("Test4", value);
                //------------Execute Test---------------------------
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
            }
            //------------Assert Results-------------------------
            Assert.AreEqual("Test4", value);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_FetchNextValue_PassLastValue_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.CommitAssign();
            //------------Execute Test---------------------------
            string value;
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec(*).a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.AreEqual("Test", value);
                //------------Execute Test---------------------------
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
            }
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_FetchNextValue_WithScalar_HasValues_ShouldReturnValues()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[a]]", "Test", 0);
            env.CommitAssign();
            //------------Execute Test---------------------------
            string value;
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
                //------------Assert Results-------------------------
                Assert.AreEqual("Test", value);
                //------------Execute Test---------------------------
                value = warewolfListIterator.FetchNextValue(warewolfIterator);
            }
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", value);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_FetchNextValue_WithIndex_HasMoreData_ShouldReturnTrue_WhenCounterSmallerThanLargestIndex()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            env.Assign("[[rec().a]]", "Test", 0);
            env.Assign("[[rec().a]]", "Test2", 0);
            env.Assign("[[rec().a]]", "Test4", 0);
            env.Assign("[[rec().a]]", "Test5", 0);
            env.CommitAssign();
            //------------Execute Test---------------------------
            bool hasMoreData;
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                var warewolfIterator = new WarewolfIterator(env.Eval("[[rec().a]]", 0));
                warewolfListIterator.AddVariableToIterateOn(warewolfIterator);
                hasMoreData = warewolfListIterator.HasMoreData();
            }
            //------------Assert Results-------------------------
            Assert.IsTrue(hasMoreData);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_String()
        {
            //------------Setup for test--------------------------
            _expr3 = new WarewolfIterator(_environment.Eval("[[RecSet()]]", 0));
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            _warewolfListIterator.AddVariableToIterateOn(_expr3);

            var listIterator = _warewolfListIterator as WarewolfListIterator;
            listIterator.Types = variablesToIterateOn.Select(iterator => iterator.GetType()).ToList();
            //------------Execute Test---------------------------
            var val = listIterator.GetValue(0);
            listIterator.Close();
            //------------Assert Results-------------------------
            Assert.AreEqual("Success", val);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Guid_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = Guid.NewGuid();
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            WarewolfListIterator listIterator;
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);
                listIterator = warewolfListIterator;
            }

            listIterator.Types = new List<Type> { typeof(Guid) };
            //------------Execute Test---------------------------
            var val = listIterator.GetValue(0);
            listIterator.Close();
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, val);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Decimal_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            decimal expected = 32M;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);
                var listIterator = warewolfListIterator;
            

                listIterator.Types = new List<Type> { typeof(decimal) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_String_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = "some string";
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(string) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Double_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = (double)3.4;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(double) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Float_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = (float)32.1;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(float) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Int64_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = (Int64)54.32;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(Int64) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Int16_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = (Int16)12;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(Int16) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Byte_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = (byte)0x34;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(byte) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_Char_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = 'c';
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(char) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected, val);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValue_DateTime_Success()
        {
            //------------Setup for test--------------------------
            var env = new ExecutionEnvironment();
            var expected = DateTime.Now;
            env.Assign("[[a]]", expected.ToString(), 0);

            var v = env.Eval("[[a]]", 0);

            var iter = new WarewolfIterator(v);
            using (var warewolfListIterator = new WarewolfListIterator())
            {
                warewolfListIterator.AddVariableToIterateOn(iter);

                var listIterator = warewolfListIterator;
                listIterator.Types = new List<Type> { typeof(DateTime) };
                //------------Execute Test---------------------------
                var val = listIterator.GetValue(0);
                listIterator.Close();
                //------------Assert Results-------------------------
                Assert.AreEqual(expected.ToString(), val.ToString());
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfListIterator))]
        public void WarewolfListIterator_Object_GetValues()
        {
            //------------Setup for test--------------------------
            _expr3 = new WarewolfIterator(_environment.Eval("[[RecSet()]]", 0));
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            _warewolfListIterator.AddVariableToIterateOn(_expr3);

            var listIterator = _warewolfListIterator as WarewolfListIterator;
            listIterator.Types = variablesToIterateOn.Select(iterator => iterator.GetType()).ToList();

            int integerPrimitive = 2;
            float floatPrimitive = 1.23f;
            Object[] objects = new Object[] {
                integerPrimitive,
                floatPrimitive };

            //------------Execute Test---------------------------
            var val = listIterator.GetValues(objects);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, val);
        }
    }
}