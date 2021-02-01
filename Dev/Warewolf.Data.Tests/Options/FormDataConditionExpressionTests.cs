/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.ExtMethods;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.Storage;

namespace Warewolf.Data.Tests.Options
{
    [TestClass]
    [TestCategory(nameof(FormDataConditionExpression))]
    public class FormDataConditionExpressionTests
    {
        private IDSFDataObject _dataObject;

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionBetween_Key_ValueIsNullOrWhiteSpace_ExpectIsInstanceOfType()
        {
            var sut = new FormDataConditionExpression 
            {
                Key = string.Empty,
                Cond = new FormDataConditionBetween 
                {
                    File = "file content",
                    FileName = "testFileName",
                    MatchType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            Assert.IsInstanceOfType(result, typeof(List<FormDataParameters>));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionBetween_File_With_InvalidBase64String_ExpectFormatException()
        {
            var environment = new ExecutionEnvironment();

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = "testKey",
                Cond = new FormDataConditionBetween
                {
                    File = "bad file content",
                    FileName = "testFileName",
                    MatchType = enFormDataTableType.File
                }
            };

            Assert.ThrowsException<FormatException>(()=> sut.Eval(getTestArgumentsFunc, true), "Invalid length for a Base-64 char array or string.");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionBetween_File_With_ValidBase64String_ExpectFormatException()
        {
            var testKey = "testKey";
            var testFileName = "testFileName";
            var testFileContentName = "[[storedFileContent]]";
            
            var base64FileContent = Convert.ToBase64String("good file content".ToBytesArray());
            var bytesFileContent = Convert.FromBase64String(base64FileContent);
            var environment = new ExecutionEnvironment();
            environment.Assign(testFileContentName, base64FileContent, 0);

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = testKey,
                Cond = new FormDataConditionBetween
                {
                    File = testFileContentName,
                    FileName = testFileName,
                    MatchType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as FileParameter;

            Assert.IsNotNull(firstResult);
            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.FileName, testFileName);
            Assert.AreEqual(firstResult.File.ToString(), bytesFileContent.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionBetween_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
        {
            var testKey = "testKey";
            var testFileName = "testFileName";
            var testFileContent = "good file message";

            var testKeyName = "[[storedKeyName]]";
            var testFileName_Name = "[[storedFileName]]";
            var testFileContentName = "[[storedFileContentName]]";

            var base64FileContent = Convert.ToBase64String(testFileContent.ToBytesArray());
            var bytesFileContent = Convert.FromBase64String(base64FileContent);
            var environment = new ExecutionEnvironment();
            environment.Assign(testKeyName, testKey, 0);
            environment.Assign(testFileName_Name, testFileName, 0);
            environment.Assign(testFileContentName, base64FileContent, 0);

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = testKeyName,
                Cond = new FormDataConditionBetween
                {
                    File = testFileContentName,
                    FileName = testFileName_Name,
                    MatchType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as FileParameter;

            Assert.IsNotNull(firstResult);
            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.FileName, testFileName);
            Assert.AreEqual(firstResult.File.ToString(), bytesFileContent.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionMatch_Key_ValueIsNullOrWhiteSpace_ExpectIsInstanceOfType()
        {
            var sut = new FormDataConditionExpression
            {
                Key = string.Empty,
                Cond = new FormDataConditionMatch
                {
                    Value = "test txt message",
                    MatchType = enFormDataTableType.Text
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            Assert.IsInstanceOfType(result, typeof(List<FormDataParameters>));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionMatch_Value_With_EmptyString_ExpectSuccess()
        {
            var testKey = "testKey";
            var testValue = string.Empty;

            var environment = new ExecutionEnvironment();

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = testKey,
                Cond = new FormDataConditionMatch
                {
                    Value = testValue,
                    MatchType = enFormDataTableType.Text
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as TextParameter;

            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.Value, testValue);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionMatch_Value_With_StoredEnvironmentVarible_ExpectSuccess()
        {
            var testKey = "testKey";
            var testValue = "good text message";
            var testFileContentName = "[[storedFileContent]]";

            var environment = new ExecutionEnvironment();
            environment.Assign(testFileContentName, testValue, 0);

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = testKey,
                Cond = new FormDataConditionMatch
                {
                    Value = testFileContentName,
                    MatchType = enFormDataTableType.Text
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as TextParameter;

            Assert.IsNotNull(firstResult);
            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.Value, testValue);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionMatch_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
        {
            var testKey = "testKey";
            var testValue = "good text message";

            var testKeyName = "[[storedKeyName]]";
            var testValueName = "[[storedValueName]]";

            var environment = new ExecutionEnvironment();
            environment.Assign(testValueName, testValue, 0);
            environment.Assign(testKeyName, testKey, 0);

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = testKeyName,
                Cond = new FormDataConditionMatch
                {
                    Value = testValueName,
                    MatchType = enFormDataTableType.Text
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as TextParameter;

            Assert.IsNotNull(firstResult);
            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.Value, testValue);
        }


        private IEnumerable<string[]> getTestArgumentsFunc(string col1s, string col2s, string col3s)
        {
            var col1 = _dataObject.Environment.EvalAsList(col1s, 0, false);
            var col2 = _dataObject.Environment.EvalAsList(col2s ?? "", 0, false);
            var col3 = _dataObject.Environment.EvalAsList(col3s ?? "", 0, false);

            var iter = new WarewolfListIterator();
            var c1 = new WarewolfAtomIterator(col1);
            var c2 = new WarewolfAtomIterator(col2);
            var c3 = new WarewolfAtomIterator(col3);
            iter.AddVariableToIterateOn(c1);
            iter.AddVariableToIterateOn(c2);
            iter.AddVariableToIterateOn(c3);

            while (iter.HasMoreData())
            {
                var item = new string[] { iter.FetchNextValue(c1), iter.FetchNextValue(c2), iter.FetchNextValue(c3) };
                yield return item;
            }
            yield break;
        }
    }
}
