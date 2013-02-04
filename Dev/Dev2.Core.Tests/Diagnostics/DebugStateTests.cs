using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    public class DebugStateTests
    {
        const string LongText = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            + "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. "
            + "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. "
            + "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        #region TryCache

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_NullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugStateMock();
            debugState.TryCache(null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueGreaterThanMaxCharDispatchCount_Expected_TruncatesValueToActCharDispatchCount()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(DebugItem.ActCharDispatchCount, item[0].Value.Length);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueGreaterThanMaxCharDispatchCount_Expected_InvokesSaveFileWithFullContent()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();

            var expectedContents = item[0].Value;

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(1, debugState.SaveFileHitCount);
            Assert.AreEqual(expectedContents, debugState.SaveFileContents);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueEqualToMaxCharDispatchCount_Expected_DoesNotTruncateValueToActCharDispatchCount()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, DebugItem.MaxCharDispatchCount);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(DebugItem.MaxCharDispatchCount, item[0].Value.Length);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueEqualToMaxCharDispatchCount_Expected_DoesNotInvokeSaveFile()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, DebugItem.MaxCharDispatchCount);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(0, debugState.SaveFileHitCount);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueLessThanMaxCharDispatchCount_Expected_DoesNotTruncateValueToActCharDispatchCount()
        // ReSharper restore InconsistentNaming
        {
            const int ExpectedLength = 100;
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, ExpectedLength);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(ExpectedLength, item[0].Value.Length);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueLessThanMaxCharDispatchCount_Expected_DoesNotInvokeSaveFile()
        // ReSharper restore InconsistentNaming
        {
            const int ExpectedLength = 100;
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, ExpectedLength);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(0, debugState.SaveFileHitCount);
        }
        #endregion

        #region SaveFile

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Test
        public void SaveFile_With_NullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugState();
            debugState.SaveFile(null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void SaveFile_With_Contents_Expected_SavesFileToDisk()
        // ReSharper restore InconsistentNaming
        {

            var debugState = new DebugState();
            var uri = debugState.SaveFile(LongText);

            var path = new Uri(uri).LocalPath;
            var exists = File.Exists(path);
            Assert.IsTrue(exists);

            var contents = File.ReadAllText(path);
            Assert.AreEqual(LongText, contents);
        }

        #endregion

        #region CreateDebugItemWithLongValue

        static DebugItem CreateDebugItemWithLongValue()
        {
            return new DebugItem
            {
                new DebugItemResult
                {
                    Type = DebugItemResultType.Value,
                    Value = LongText
                }
            };

        }

        #endregion

    }
}
