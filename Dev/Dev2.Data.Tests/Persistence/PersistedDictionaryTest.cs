using System;
using System.IO;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    [TestClass]
    public class PersistedDictionaryTest
    {
        [TestMethod]
        public void CanAddToFileExpectAddedEntry()
        {
            Dev2PersistantDictionary<IBinaryDataListRow> dic = new Dev2PersistantDictionary<IBinaryDataListRow>(Path.GetTempFileName());

            IBinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0);
            row.UpdateValue("col2", 1);
            row.UpdateValue("col3", 2);
            row.UpdateValue("col4", 3);
            row.UpdateValue("col5", 4);

            string key = Guid.NewGuid().ToString();

            dic.Add(key, row);

            var keys = dic.Keys;
            var keysItr = keys.GetEnumerator();

            Assert.AreEqual(1, keys.Count);
            keysItr.MoveNext();
            Assert.AreEqual(key, keysItr.Current);

        }

        [TestMethod]
        public void CanRemoveFromFileExpectNoEntries()
        {
            Dev2PersistantDictionary<IBinaryDataListRow> dic = new Dev2PersistantDictionary<IBinaryDataListRow>(Path.GetTempFileName());

            IBinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0);
            row.UpdateValue("col2", 1);
            row.UpdateValue("col3", 2);
            row.UpdateValue("col4", 3);
            row.UpdateValue("col5", 4);

            string key = Guid.NewGuid().ToString();

            dic.Add(key, row);

            dic.Remove(key);

            Assert.AreEqual(0,dic.Count);

        }

        [TestMethod]
        public void CanUseIndexerToAddExpectAddedEntry()
        {
            Dev2PersistantDictionary<IBinaryDataListRow> dic = new Dev2PersistantDictionary<IBinaryDataListRow>(Path.GetTempFileName());

            IBinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0);
            row.UpdateValue("col2", 1);
            row.UpdateValue("col3", 2);
            row.UpdateValue("col4", 3);
            row.UpdateValue("col5", 4);

            string key = Guid.NewGuid().ToString();

            dic[key] = row;

            Assert.AreEqual(1, dic.Keys.Count);
            var keyItr = dic.Keys.GetEnumerator();
            keyItr.MoveNext();
            Assert.AreEqual(key, keyItr.Current);
        }

        [TestMethod]
        public void CanFetchValueExpectValidRow()
        {
            Dev2PersistantDictionary<IBinaryDataListRow> dic = new Dev2PersistantDictionary<IBinaryDataListRow>(Path.GetTempFileName());

            IBinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0);
            row.UpdateValue("col2", 1);
            row.UpdateValue("col3", 2);
            row.UpdateValue("col4", 3);
            row.UpdateValue("col5", 4);

            string key = Guid.NewGuid().ToString();

            dic[key] = row;

            IBinaryDataListRow fetchedRow = dic[key];

            Assert.AreEqual("col1",fetchedRow.FetchValue(0));
            Assert.AreEqual("col2", fetchedRow.FetchValue(1));
            Assert.AreEqual("col3", fetchedRow.FetchValue(2));
            Assert.AreEqual("col4", fetchedRow.FetchValue(3));
            Assert.AreEqual("col5", fetchedRow.FetchValue(4));
        }
    }
}
