using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Warewolf.Storage.Interfaces;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class WarewolfListIteratorTests
    {
        IWarewolfIterator _expr1;
        IWarewolfIterator _expr2;
        IWarewolfIterator _expr3;
        private IExecutionEnvironment _environment;
        private IWarewolfListIterator _warewolfListIterator;

        const string Result = "[[Result]]";

        [TestInitialize]
        public void TestInitialize()
        {
            _environment = new ExecutionEnvironment();
            _warewolfListIterator = new WarewolfListIterator();

            _environment.Assign(Result, "Success", 0);
            _expr1 = new WarewolfIterator(_environment.Eval(Result, 0));
            _expr2 = new WarewolfIterator(_environment.Eval("[[@Parent()]]", 0));

            _warewolfListIterator.AddVariableToIterateOn(_expr1);
            _warewolfListIterator.AddVariableToIterateOn(_expr2);            
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIteratorInstance_ShouldHaveConstructor()
        {            
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn");
            var count = privateObj.GetField("_count");
            Assert.IsNotNull(variablesToIterateOn);
            Assert.AreEqual(-1, count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetMax_ShouldReturn1()
        {
            Assert.IsNotNull(_warewolfListIterator);
            var privateObj = new PrivateObject(_warewolfListIterator);
            var variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
            var max = _warewolfListIterator.GetMax();
            Assert.AreEqual(1, max);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
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
        [Owner("Sanele Mthembu")]
        public void GivenStringTrueIsAssigned_WarewolfListIterator_GetBoolean_ShouldReturnTrue()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "True", 0);
            WarewolfListIterator listIterator;
            if (AssignExpression(out listIterator))
            {
                return;
            }

            var boolean = listIterator.GetBoolean(2);
            Assert.IsTrue(boolean);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetChars_ShouldReturn0()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "SomeValue", 0);
            WarewolfListIterator listIterator;
            if (AssignExpression(out listIterator))
            {
                return;
            }

            var chars = listIterator.GetChars(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<char[]>(), It.IsAny<int>(), It.IsAny<int>());
            Assert.AreEqual(0, chars);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetStrings_ShouldReturnStringValue()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "SomeValue", 0);
            WarewolfListIterator listIterator;
            if (AssignExpression(out listIterator))
            {
                return;
            }

            var strig = listIterator.GetString(2);
            Assert.AreEqual("SomeValue", strig);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetDateTime_ShouldReturnDateFormat()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "01/01/2000", 0);
            WarewolfListIterator listIterator;
            if (AssignExpression(out listIterator))
            {
                return;
            }

            var toDate = Convert.ToDateTime("01/01/2000");
            var dateTime = listIterator.GetDateTime(2);
            Assert.AreEqual(toDate, dateTime);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetDataAndGetSchemaTable_ShouldReturnNull()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "", 0);
            WarewolfListIterator listIterator;
            if (AssignExpression(out listIterator))
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

        private bool AssignExpression(out WarewolfListIterator listIterator)
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

        private void ValidateInstance(out PrivateObject privateObj, out List<IWarewolfIterator> variablesToIterateOn)
        {
            Assert.IsNotNull(_warewolfListIterator);
            privateObj = new PrivateObject(_warewolfListIterator);
            variablesToIterateOn = privateObj.GetField("_variablesToIterateOn") as List<IWarewolfIterator>;
            Assert.IsNotNull(variablesToIterateOn);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_GetGuid_ShouldReturn0()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
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
        [Owner("Sanele Mthembu")]
        public void WarewolfListIterator_IDataRecordFunctions_Should()
        {
            PrivateObject privateObj;
            List<IWarewolfIterator> variablesToIterateOn;
            ValidateInstance(out privateObj, out variablesToIterateOn);
            _environment.Assign("[[RecSet().a]]", "1", 0);
            WarewolfListIterator listIterator;
            if (AssignExpression(out listIterator))
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
    }
}