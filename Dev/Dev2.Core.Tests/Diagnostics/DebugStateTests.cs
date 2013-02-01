using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    public class DebugStateTests
    {
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

        //[TestMethod]
        //// ReSharper disable InconsistentNaming - Unit Test
        //public void TryCache_With_ItemsCountGreaterThanMaxDispatchCount_Expected_DoesTruncateItems()
        //// ReSharper restore InconsistentNaming
        //{
        //    TestTryCache(DebugItem.MaxItemDispatchCount * 2, (int)(DebugItem.MaxItemDispatchCount * 1.5), 2);
        //}

        //[TestMethod]
        //// ReSharper disable InconsistentNaming - Unit Test
        //public void TryCache_With_ItemsCountLessThanMaxDispatchCount_Expected_DoesNotTruncateItems()
        //// ReSharper restore InconsistentNaming
        //{
        //    TestTryCache(DebugItem.MaxItemDispatchCount - 2, DebugItem.MaxItemDispatchCount - 4, 0);
        //}


        #region TestTryCache

        static void TestTryCache(int test1Count, int test2Count, int expectedSaveGroupHitCount)
        {
            const int NumItems = 50;
            var items = new List<IDebugItem>();
            for(var i = 0; i < NumItems; i++)
            {
                var idxStr = i.ToString(CultureInfo.InvariantCulture);
                var item = new DebugItem(idxStr, string.Format("[[VAR{0}]]", idxStr), string.Format("VAL{0}", idxStr))
                {
                    Group = i >= 5 && i < (5 + test1Count) ? "TEST1" : (i >= 30 && i < (30 + test2Count) ? "TEST2" : null)
                };

                items.Add(item);
            }
            var debugState = new DebugStateMock();
            debugState.TryCache(items);

            var actualTest1Count = items.Count(i => i.Group == "TEST1");
            var actualTest2Count = items.Count(i => i.Group == "TEST2");
            var actualEmptyCount = items.Count(i => string.IsNullOrEmpty(i.Group));

            Assert.AreEqual(expectedSaveGroupHitCount, debugState.SaveGroupHitCount);
            if(expectedSaveGroupHitCount > 0)
            {
                Assert.AreEqual(test1Count, debugState.SaveGroupItems["TEST1"].Count);
                Assert.AreEqual(test2Count, debugState.SaveGroupItems["TEST2"].Count);
                Assert.AreEqual(DebugItem.MaxItemDispatchCount, actualTest1Count);
                Assert.AreEqual(DebugItem.MaxItemDispatchCount, actualTest2Count);
            }
            else
            {
                Assert.AreEqual(0, debugState.SaveGroupItems.Count);
                Assert.AreEqual(test1Count, actualTest1Count);
                Assert.AreEqual(test2Count, actualTest2Count);
            }

            Assert.AreEqual(NumItems - test1Count - test2Count, actualEmptyCount);
        }

        #endregion

        #endregion

        #region SaveGroup

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        //// ReSharper disable InconsistentNaming - Unit Test
        //public void SaveGroup_With_NullParameters_Expected_ThrowsArgumentNullException()
        //// ReSharper restore InconsistentNaming
        //{
        //    var debugState = new DebugState();
        //    debugState.SaveGroup(null, null);
        //}

        //[TestMethod]
        //// ReSharper disable InconsistentNaming - Unit Test
        //public void SaveGroup_With_Items_Expected_SavesFileToDisk()
        //// ReSharper restore InconsistentNaming
        //{
        //    var items = new List<IDebugItem>();
        //    for(var i = 0; i < 50; i++)
        //    {
        //        var item = new DebugItem();
        //        if(i >= 5 && i < 25)
        //        {
        //            item.Add(CreateDebugItemResult(i, "TEST1"));
        //        }
        //        else if(i >= 30 && i < 45)
        //        {
        //            item.Add(CreateDebugItemResult(i, "TEST2"));
        //        }
        //        else
        //        {
        //            item.Add(CreateDebugItemResult(i, null));
        //        }

        //        items.Add(item);
        //    }
        //    var debugState = new DebugState() { Name = "TestActivity(2)" };
        //    var path = debugState.SaveGroup(items, "Test1");
        //    var exists = File.Exists(path);

        //    Assert.IsTrue(exists);
        //}

        static DebugItemResult CreateDebugItemResult(int index, string groupName)
        {
            var rem = index % 3;
            var isGroup = !string.IsNullOrEmpty(groupName);

            var result = new DebugItemResult
            {
                GroupIndex = isGroup ? index - rem : 0,
                GroupName = isGroup ? groupName : null,
                Type = (DebugItemResultType)rem,
            };

            var idxStr = index.ToString(CultureInfo.InvariantCulture);
            result.Value = string.Format("[[{0}{1}]]", result.Type, idxStr);

            return result;
        }

        #endregion

    }
}
