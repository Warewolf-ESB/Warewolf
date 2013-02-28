using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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

            IBinaryDataListRow row1 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0);
            row1.UpdateValue("col2", 1);
            row1.UpdateValue("col3", 2);
            row1.UpdateValue("col4", 3);
            row1.UpdateValue("col5", 4);

            //IBinaryDataListRow row2 = new BinaryDataListRow(5);
            //row2.UpdateValue("col1", 0);
            //row2.UpdateValue("col2", 1);

            IBinaryDataListRow row3 = new BinaryDataListRow(5);
            row3.UpdateValue("col3", 2);
            row3.UpdateValue("col4", 3);
            row3.UpdateValue("col5", 4);


            string key1 = Guid.NewGuid().ToString();
            string key2 = Guid.NewGuid().ToString();
            string key3 = Guid.NewGuid().ToString();

            dic.Remove(key2);

            //dic[key2] = row2;
            dic[key3] = row3;
            dic[key1] = row1;

            dic.Remove(key2);
            dic.Compact();

            IBinaryDataListRow fetchedRow1 = dic[key1];
            //IBinaryDataListRow fetchedRow2 = dic[key2];
            IBinaryDataListRow fetchedRow3 = dic[key3];

            Assert.AreEqual("col1", fetchedRow1.FetchValue(0));
            Assert.AreEqual("col2", fetchedRow1.FetchValue(1));
            Assert.AreEqual("col3", fetchedRow1.FetchValue(2));
            Assert.AreEqual("col4", fetchedRow1.FetchValue(3));
            Assert.AreEqual("col5", fetchedRow1.FetchValue(4));

            //Assert.AreEqual("col1", fetchedRow2.FetchValue(0));
            //Assert.AreEqual("col2", fetchedRow2.FetchValue(1));

            Assert.AreEqual("col3", fetchedRow3.FetchValue(2));
            Assert.AreEqual("col4", fetchedRow3.FetchValue(3));
            Assert.AreEqual("col5", fetchedRow3.FetchValue(4));
        }
    }
}
