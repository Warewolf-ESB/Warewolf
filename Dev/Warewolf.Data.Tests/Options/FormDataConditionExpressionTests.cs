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
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                Cond = new FormDataConditionFile 
                {
                    FileBase64 = "file content",
                    FileName = "testFileName",
                    TableType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            Assert.IsInstanceOfType(result, typeof(List<IFormDataParameters>));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionBetween_File_With_InvalidBase64String_ExpectNotFormatException()
        {
            var environment = new ExecutionEnvironment();

            var mockDSFDataObject = new Mock<IDSFDataObject>();
            mockDSFDataObject.Setup(o => o.Environment).Returns(environment);

            _dataObject = mockDSFDataObject.Object;

            var sut = new FormDataConditionExpression
            {
                Key = "testKey",
                Cond = new FormDataConditionFile
                {
                    FileBase64 = "bad file content",
                    FileName = "testFileName",
                    TableType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            Assert.IsNotNull(result);
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
                Cond = new FormDataConditionFile
                {
                    FileBase64 = testFileContentName,
                    FileName = testFileName,
                    TableType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as FileParameter;

            Assert.IsNotNull(firstResult);
            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.FileName, testFileName);
            Assert.AreEqual(firstResult.FileBytes.ToString(), bytesFileContent.ToString());
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
                Cond = new FormDataConditionFile
                {
                    FileBase64 = testFileContentName,
                    FileName = testFileName_Name,
                    TableType = enFormDataTableType.File
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            var firstResult = result.First() as FileParameter;

            Assert.IsNotNull(firstResult);
            Assert.AreEqual(firstResult.Key, testKey);
            Assert.AreEqual(firstResult.FileName, testFileName);
            Assert.AreEqual(firstResult.FileBytes.ToString(), bytesFileContent.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FormDataConditionMatch_Key_ValueIsNullOrWhiteSpace_ExpectIsInstanceOfType()
        {
            var sut = new FormDataConditionExpression
            {
                Key = string.Empty,
                Cond = new FormDataConditionText
                {
                    Value = "test txt message",
                    TableType = enFormDataTableType.Text
                }
            };

            var result = sut.Eval(getTestArgumentsFunc, true);

            Assert.IsInstanceOfType(result, typeof(List<IFormDataParameters>));
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
                Cond = new FormDataConditionText
                {
                    Value = testValue,
                    TableType = enFormDataTableType.Text
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
                Cond = new FormDataConditionText
                {
                    Value = testFileContentName,
                    TableType = enFormDataTableType.Text
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
                Cond = new FormDataConditionText
                {
                    Value = testValueName,
                    TableType = enFormDataTableType.Text
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
        public void FormDataConditionExpression_ToFormDataParameter_FormDataConditionMatch_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
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
                Cond = new FormDataConditionText
                {
                    Value = testValueName,
                    TableType = enFormDataTableType.Text
                }
            }.ToFormDataParameter();

            var result = sut as TextParameter;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Key, testKeyName);
            Assert.AreEqual(result.Value, testValueName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_ToFormDataParameter_FormDataConditionBetween_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
        {
            var testKey = "testKey";
            var testFileName = "testFileName";
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
                Cond = new FormDataConditionFile
                {
                    FileBase64 = testValueName,
                    FileName = testFileName,
                    TableType = enFormDataTableType.Text
                }
            }.ToFormDataParameter();

            var result = sut as FileParameter;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Key, testKeyName);
            Assert.AreEqual(result.FileName, testFileName);
            Assert.AreEqual(result.FileBase64, testValueName);
            Assert.ThrowsException<FormatException>(() => result.FileBytes, "this is an invalid format string, expecting base64 string");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_ToString_FormDataConditionBetween_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
        {
            var testKey = "testKey";
            var testFileName = "testFileName";
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
                Cond = new FormDataConditionFile
                {
                    FileBase64 = testValueName,
                    FileName = testFileName,
                    TableType = enFormDataTableType.Text
                }
            }.ToString();

            var result = sut;

            Assert.IsNotNull(result);
            Assert.AreEqual("Key: [[storedKeyName]] File Content: [[storedValueName]] File Name: testFileName", result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_RenderDescription_FormDataConditionMatch_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
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

            var sb = new StringBuilder();
            new FormDataConditionExpression
            {
                Key = testKeyName,
                Cond = new FormDataConditionText
                {
                    Value = testValueName,
                    TableType = enFormDataTableType.Text
                }
            }.RenderDescription(sb);

            var result = sb;

            var testSb = new StringBuilder();
            testSb.Append("Key: " + testKeyName);
            testSb.Append(" ");
            enFormDataTableType.Text.RenderDescription(testSb);
            testSb.Append(": ");
            testSb.Append(testValueName);

            Assert.IsNotNull(result);
            Assert.AreEqual(testSb.ToString(), result.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_ToOptions_FormDataConditionMatch_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
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
                Cond = new FormDataConditionText
                {
                    Value = testValueName,
                    TableType = enFormDataTableType.Text
                }
            }.ToOptions();

            var result = sut.First() as FormDataOptionConditionExpression;

            Assert.AreEqual(testKeyName, result.Key);
            Assert.AreEqual(testValueName, result.Value);
            Assert.AreEqual(enFormDataTableType.Text.ToString(), result.SelectedTableType.Name);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void FormDataConditionExpression_FromOption_FormDataConditionMatch_With_AllValues_StoredEnvironmentVarible_ExpectSuccess()
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
                Key = testKeyName
            };

            sut.FromOption(new FormDataOptionConditionExpression
            {
                Key = testKeyName,
                Value = testValueName
            });

            var result = sut.Cond as FormDataConditionText;

            Assert.AreEqual(testKeyName, sut.Key);
            Assert.AreEqual(testValueName, result.Value);
            Assert.AreEqual(enFormDataTableType.Text, result.TableType);
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
                var item = new[] { iter.FetchNextValue(c1), iter.FetchNextValue(c2), iter.FetchNextValue(c3) };
                yield return item;
            }
            yield break;
        }
    }
}
