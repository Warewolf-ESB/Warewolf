/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    public class DebugItemTests
    {
        const string LongText = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            + "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. "
            + "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. "
            + "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";


        string _oldWebServerUri;
        [TestInitialize]
        public void TestInit()
        {
            _oldWebServerUri = EnvironmentVariables.WebServerUri;
            EnvironmentVariables.WebServerUri = "http://localhost:3142";
        }

        [TestCleanup]
        public void Cleanup()
        {
            EnvironmentVariables.WebServerUri = _oldWebServerUri;
        }

        [TestMethod]
        public void DebugItem_Flush()
        {
            var item = new DebugItem();
            item.FlushStringBuilder();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_Constructor_With_Array_Expected_InitializesWithArray()
        {
            var result = new DebugItemResult { GroupName = "Hello", Value = "world" };
            var item = new DebugItem();
            item.Add(result);
            Assert.AreEqual(1, item.FetchResultsList().Count);
            Assert.AreSame(result, item.FetchResultsList()[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_Contains_With_NullFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupName = "Hello", Value = "world" });
            var result = item.Contains(null);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_Contains_With_EmptyFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupName = "Hello", Value = "world" });
            var result = item.Contains(string.Empty);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_Contains_With_ValidFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupName = "Hello", Value = "world" });
            var result = item.Contains("world");
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_Contains_With_InvalidFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupName = "Hello", Value = "world" });
            var result = item.Contains("the");
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_Contains_With_Filter_Expected_IsCaseInsensitive()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupName = "Hello", Value = "world" });
            var result = item.Contains("hel");
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_TruncateItemResultIfNeeded_With_ValueGreaterThanMaxCharDispatchCount_Expected_TruncatesValueToActCharDispatchCount()
        {
            var item = CreateDebugItemWithLongValue();

            var debugState = new DebugItem();
            debugState.TruncateItemResultIfNeeded(item);

            Assert.AreEqual(DebugItem.ActCharDispatchCount, item.Value.Length);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_TruncateItemResultIfNeeded_With_ValueGreaterThanMaxCharDispatchCount_Expected_InvokesSaveFileWithFullContent()
        {
            var item = CreateDebugItemWithLongValue();

            var expectedContents = item.Value;

            var debugItem = new DebugItemMock();
            debugItem.TruncateItemResultIfNeeded(item);

            Assert.AreEqual(1, debugItem.SaveFileHitCount);
            Assert.AreEqual(expectedContents, debugItem.SaveFileContents);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_TruncateItemResultIfNeeded_With_ValueEqualToMaxCharDispatchCount_Expected_DoesNotTruncateValueToActCharDispatchCount()
        {
            var item = CreateDebugItemWithLongValue();
            item.Value = item.Value.Substring(0, DebugItem.MaxCharDispatchCount);

            var debugItem = new DebugItemMock();
            debugItem.TruncateItemResultIfNeeded(item);

            Assert.AreEqual(DebugItem.MaxCharDispatchCount, item.Value.Length);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_TruncateItemResultIfNeeded_With_ValueEqualToMaxCharDispatchCount_Expected_DoesNotInvokeSaveFile()
        {
            var item = CreateDebugItemWithLongValue();
            item.Value = item.Value.Substring(0, DebugItem.MaxCharDispatchCount);

            var debugState = new DebugItemMock();
            debugState.TruncateItemResultIfNeeded(item);

            Assert.AreEqual(0, debugState.SaveFileHitCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_TruncateItemResultIfNeeded_With_ValueLessThanMaxCharDispatchCount_Expected_DoesNotTruncateValueToActCharDispatchCount()
        {
            const int ExpectedLength = 100;
            var item = CreateDebugItemWithLongValue();
            item.Value = item.Value.Substring(0, ExpectedLength);

            var debugState = new DebugItemMock();
            debugState.TruncateItemResultIfNeeded(item);

            Assert.AreEqual(ExpectedLength, item.Value.Length);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_TruncateItemResultIfNeeded_With_ValueLessThanMaxCharDispatchCount_Expected_DoesNotInvokeSaveFile()
        {
            const int ExpectedLength = 100;
            var item = CreateDebugItemWithLongValue();
            item.Value = item.Value.Substring(0, ExpectedLength);

            var debugState = new DebugItemMock();
            debugState.TruncateItemResultIfNeeded(item);

            Assert.AreEqual(0, debugState.SaveFileHitCount);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DebugItem_SaveFile_With_NullParameters_Expected_ThrowsArgumentNullException()
        {
            var debugState = new DebugItem();

            debugState.SaveFile(null, null);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_SaveFile_With_Contents_Expected_SavesFileToDisk()
        {
            var debugState = new DebugItem();

            debugState.ClearFile("TestFile.txt");
            EnvironmentVariables.WebServerUri = "http://localhost:3142";
            var uri = debugState.SaveFile(LongText, "TestFile.txt");
            var path = new Uri(uri).OriginalString.Replace("?DebugItemFilePath=", "").Replace(EnvironmentVariables.WebServerUri + "/Services/FetchDebugItemFileService", "");
            var exists = File.Exists(path);
            Assert.IsTrue(exists);

            var contents = File.ReadAllText(path);
            Assert.AreEqual(LongText, contents);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugIem_SaveFile_WithContentsNewLineChars_ExpectedSavesFileToDiskWithCorrectChars()
        {
            var debugState = new DebugItem();

            debugState.ClearFile("TestFile.txt");
            EnvironmentVariables.WebServerUri = "http://localhost:3142";
            const string expeced = "\r\nThis is\r\n the text\\n that we are writing";
            const string textToWrite = "\nThis is\r\n the text\\n that we are writing";

            var uri = debugState.SaveFile(textToWrite, "TestFile.txt");
            var path = new Uri(uri).OriginalString.Replace("?DebugItemFilePath=", "").Replace(EnvironmentVariables.WebServerUri + "/Services/FetchDebugItemFileService", "");
            var exists = File.Exists(path);
            Assert.IsTrue(exists);

            var contents = File.ReadAllText(path);
            Assert.AreEqual(expeced, contents);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_GroupIndex_Greater_And_Equal_To_MaxItemDispatchCount()
        {
            var oldWebServerUri = EnvironmentVariables.WebServerUri;
            try
            {
                EnvironmentVariables.WebServerUri = "http://localhost:3142";
                var item = new DebugItem();
                item.Add(new DebugItemResult { GroupIndex = 11, GroupName = "Hello", Value = "world" });
                Assert.AreEqual(1, item.ResultsList.Count);
                Assert.AreEqual(11, item.ResultsList[0].GroupIndex);
                Assert.AreEqual("Hello", item.ResultsList[0].GroupName);
                Assert.IsFalse(item.ResultsList[0].HasError);
                Assert.AreEqual("world", item.ResultsList[0].Value);
                Assert.IsTrue(item.ResultsList[0].MoreLink.StartsWith("http://localhost:3142/Services/FetchDebugItemFileService?DebugItemFilePath=C:\\ProgramData\\Warewolf\\Temp\\Warewolf\\Debug\\", StringComparison.Ordinal), "Expected " + item.ResultsList[0].MoreLink);
            } finally
            {
                EnvironmentVariables.WebServerUri = oldWebServerUri;
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_GroupIndex_Greater_Than_MaxItemDispatchCount()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupIndex = 15, GroupName = "Hello", Value = "world", Type = DebugItemResultType.Value });
            Assert.AreEqual(0, item.ResultsList.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_GroupIndex_Greater_Than_MaxItemDispatchCount_Type_Label()
        {
            var item = new DebugItem();
            const string val = LongText + LongText + LongText + LongText + LongText + LongText + LongText + LongText +
                               LongText + LongText + LongText + LongText + LongText + LongText + LongText + LongText +
                               LongText + LongText + LongText + LongText + LongText + LongText + LongText + LongText;

            item.Add(new DebugItemResult { GroupIndex = 15, GroupName = "Hello", Value = val, Type = DebugItemResultType.Label });
            Assert.AreEqual(0, item.ResultsList.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_GroupIndex_Less_Than_MaxItemDispatchCount()
        {
            var item = new DebugItem();
            item.Add(new DebugItemResult { GroupIndex = 5, GroupName = "Hello", Value = LongText, Type = DebugItemResultType.Value });
            Assert.AreEqual(1, item.ResultsList.Count);
            Assert.AreEqual(5, item.ResultsList[0].GroupIndex);
            Assert.AreEqual("Hello", item.ResultsList[0].GroupName);
            Assert.IsFalse(item.ResultsList[0].HasError);
            Assert.AreEqual("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore", item.ResultsList[0].Value);
            Assert.IsTrue(item.ResultsList[0].MoreLink.StartsWith("http://localhost:3142/Services/FetchDebugItemFileService?DebugItemFilePath=C:\\ProgramData\\Warewolf\\Temp\\Warewolf\\Debug\\", StringComparison.Ordinal), "Expected " + item.ResultsList[0].MoreLink);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_With_Results_AddRange()
        {
            var results = new[] { new DebugItemResult(), new DebugItemResult { GroupName = "group1", GroupIndex = 1 } };
            var item = new DebugItem(results);
            Assert.AreEqual(2, item.ResultsList.Count);

            Assert.AreEqual(0, item.ResultsList[0].GroupIndex);
            Assert.IsNull(item.ResultsList[0].GroupName);
            Assert.IsFalse(item.ResultsList[0].HasError);
            Assert.IsNull(item.ResultsList[0].Value);
            Assert.IsNull(item.ResultsList[0].MoreLink);

            Assert.AreEqual(1, item.ResultsList[1].GroupIndex);
            Assert.AreEqual("group1", item.ResultsList[1].GroupName);
            Assert.IsFalse(item.ResultsList[1].HasError);
            Assert.IsNull(item.ResultsList[1].Value);
            Assert.IsNull(item.ResultsList[1].MoreLink);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_With_Results_AddRange_Matching_Label()
        {
            var results = new[]
            {
                new DebugItemResult(),
                new DebugItemResult
                {
                    GroupName = "group1",
                    GroupIndex = 1,
                    Type = DebugItemResultType.Variable
                }
            };
            var item = new DebugItem(results);

            Assert.AreEqual(1, item.ResultsList.Count);

            Assert.AreEqual(0, item.ResultsList[0].GroupIndex);
            Assert.IsNull(item.ResultsList[0].GroupName);
            Assert.IsFalse(item.ResultsList[0].HasError);
            Assert.IsNull(item.ResultsList[0].Value);
            Assert.IsNull(item.ResultsList[0].MoreLink);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DebugItem))]
        public void DebugItem_ResultList_AddMoreThanTenItems_ShouldKeepValues()
        {
            var item = new DebugItem();

            const int max = 12;
            for (int i = 1; i <= max; i++)
            {
                var item1 = new DebugItemResult
                {
                    GroupIndex = i,
                    GroupName = "[[CoinMarketCap(*)]]",
                    HasError = false,
                    Label = "",
                    MockSelected = false,
                    MoreLink = null,
                    Operator = "=",
                    TestStepHasError = false,
                    Type = DebugItemResultType.Variable,
                    Value = "2019/04/11 06:00:03 AM",
                    Variable = "[[CoinMarketCap(" + i + ").date_updated]]"
                };

                item.Add(item1);
            }

            Assert.AreEqual(11, item.ResultsList.Count);

            foreach (var res in item.ResultsList)
            {
                Assert.IsNotNull(res.Variable, "GroupIndex " + res.GroupIndex.ToString() + " failed.");
                Assert.IsNotNull(res.Value, "GroupIndex " + res.GroupIndex.ToString() + " failed.");
            }
        }

        static DebugItemResult CreateDebugItemWithLongValue()
        {
            return new DebugItemResult { Type = DebugItemResultType.Value, Value = LongText };
        }
    }
}
