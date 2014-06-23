using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Dev2.Data.Storage;
using Dev2.Data.Storage.ProtocolBuffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for Dev2BinaryStorageTest
    /// </summary>
    [TestClass]    
    public class Dev2BinaryStorageTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region BinaryDataListRow

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("binaryDataListRow_ProtoBuff")]
        public void BinaryDataListRow_ProtoBuffSerialize_NormalSerialization_AllDataConverted()
        {

            //------------Setup for test--------------------------

            BinaryDataListRow row1 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0, 5);
            row1.UpdateValue("col2", 1, 5);
            row1.UpdateValue("col3", 2, 5);
            row1.UpdateValue("col4", 3, 5);
            row1.UpdateValue("col5", 4, 5);

            //------------Execute Test---------------------------
            var bytes = row1.ToByteArray();

            //------------Assert Results-------------------------
            Assert.AreEqual(92, bytes.Length);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("binaryDataListRow_ProtoBuff")]
        public void BinaryDataListRow_ProtoBuffDeserialize_NormalDeserialization_AllDataHydrated()
        {

            //------------Setup for test--------------------------

            BinaryDataListRow row1 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0, 5);
            row1.UpdateValue("col2", 1, 5);
            row1.UpdateValue("col3", 2, 5);
            row1.UpdateValue("col4", 3, 5);
            row1.UpdateValue("col5", 4, 5);

            //------------Execute Test---------------------------
            var bytes = row1.ToByteArray();

            BinaryDataListRow row2 = new BinaryDataListRow(5);
            row2.ToObject(bytes);

            //------------Assert Results-------------------------
            Assert.AreEqual("col1",row2.FetchValue(0, 5));
        }

        #endregion

        #region PersistedDictionary Ported Test

        [TestMethod]
        public void CanAddToFileExpectAddedEntry()
        {
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(),1024);

            BinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0, 5);
            row.UpdateValue("col2", 1, 5);
            row.UpdateValue("col3", 2, 5);
            row.UpdateValue("col4", 3, 5);
            row.UpdateValue("col5", 4, 5);

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
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 1024);

            BinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0, 5);
            row.UpdateValue("col2", 1, 5);
            row.UpdateValue("col3", 2, 5);
            row.UpdateValue("col4", 3, 5);
            row.UpdateValue("col5", 4, 5);

            string key = Guid.NewGuid().ToString();

            dic.Add(key, row);

            dic.Remove(key);

            Assert.AreEqual(0, dic.Count);

        }

        [TestMethod]
        public void CanUseIndexerToAddExpectAddedEntry()
        {
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 1024);

            BinaryDataListRow row = new BinaryDataListRow(5);
            row.UpdateValue("col1", 0, 5);
            row.UpdateValue("col2", 1, 5);
            row.UpdateValue("col3", 2, 5);
            row.UpdateValue("col4", 3, 5);
            row.UpdateValue("col5", 4, 5);

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
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 1024);

            BinaryDataListRow row1 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0, 5);
            row1.UpdateValue("col2", 1, 5);
            row1.UpdateValue("col3", 2, 5);
            row1.UpdateValue("col4", 3, 5);
            row1.UpdateValue("col5", 4, 5);

            BinaryDataListRow row3 = new BinaryDataListRow(5);
            row3.UpdateValue("col3", 2, 5);
            row3.UpdateValue("col4", 3, 5);
            row3.UpdateValue("col5", 4, 5);


            string key1 = Guid.NewGuid().ToString();
            string key2 = Guid.NewGuid().ToString();
            string key3 = Guid.NewGuid().ToString();

            dic.Remove(key2);
            dic[key3] = row3;
            dic[key1] = row1;

            dic.Remove(key2);
            //dic.Compact();

            BinaryDataListRow fetchedRow1 = dic[key1];
            BinaryDataListRow fetchedRow3 = dic[key3];

            Assert.AreEqual("col1", fetchedRow1.FetchValue(0, 5));
            Assert.AreEqual("col2", fetchedRow1.FetchValue(1, 5));
            Assert.AreEqual("col3", fetchedRow1.FetchValue(2, 5));
            Assert.AreEqual("col4", fetchedRow1.FetchValue(3, 5));
            Assert.AreEqual("col5", fetchedRow1.FetchValue(4, 5));


            Assert.AreEqual("col3", fetchedRow3.FetchValue(2, 5));
            Assert.AreEqual("col4", fetchedRow3.FetchValue(3, 5));
            Assert.AreEqual("col5", fetchedRow3.FetchValue(4, 5));
        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2BinaryStorage_BasicUsage")]
        public void Dev2BinaryStorage_BasicUsage_WhenAddingRemovingAndCompacting_AllDataFetched()
        {

            //------------Setup for test--------------------------
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 756);

            BinaryDataListRow row1 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0, 5);
            row1.UpdateValue("col2", 1, 5);
            row1.UpdateValue("col3", 2, 5);
            row1.UpdateValue("col4", 3, 5);
            row1.UpdateValue("col5", 4, 5);

            BinaryDataListRow row3 = new BinaryDataListRow(5);
            row3.UpdateValue("col3", 2, 5);
            row3.UpdateValue("col4", 3, 5);
            row3.UpdateValue("col5", 4, 5);


            BinaryDataListRow row2 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0, 5);
            row1.UpdateValue("col2", 1, 5);
            row1.UpdateValue("col3", 2, 5);
            row1.UpdateValue("col4", 3, 5);
            row1.UpdateValue("col5", 4, 5);

            string key1 = Guid.NewGuid().ToString();
            string key2 = Guid.NewGuid().ToString();
            string key3 = Guid.NewGuid().ToString();

            //------------Execute Test---------------------------
            dic[key3] = row3;
            dic[key2] = row2;
            dic[key1] = row1;

            dic.Remove(key2);
            dic.Compact();

            BinaryDataListRow fetchedRow1 = dic[key1];
            BinaryDataListRow fetchedRow3 = dic[key3];

            //------------Assert Results-------------------------
            Assert.AreEqual("col1", fetchedRow1.FetchValue(0, 5));
            Assert.AreEqual("col2", fetchedRow1.FetchValue(1, 5));
            Assert.AreEqual("col3", fetchedRow1.FetchValue(2, 5));
            Assert.AreEqual("col4", fetchedRow1.FetchValue(3, 5));
            Assert.AreEqual("col5", fetchedRow1.FetchValue(4, 5));


            Assert.AreEqual("col3", fetchedRow3.FetchValue(2, 5));
            Assert.AreEqual("col4", fetchedRow3.FetchValue(3, 5));
            Assert.AreEqual("col5", fetchedRow3.FetchValue(4, 5));

   
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2BinaryStorage_BasicUsage")]
        public void Dev2BinaryStorage_WhenBaseSmallerThanView_AllDataFetched()
        {

            // ------------Setup for test--------------------------
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 756);

            const int baseRowCount = 5;
            const int viewRowCount = 6;

            BinaryDataListRow row1 = new BinaryDataListRow(5);
            row1.UpdateValue("col1", 0, baseRowCount);
            row1.UpdateValue("col2", 1, baseRowCount);
            row1.UpdateValue("col3", 2, baseRowCount);
            row1.UpdateValue("col4", 3, baseRowCount);
            row1.UpdateValue("col5", 4, baseRowCount);

            string key1 = Guid.NewGuid().ToString();

            // ------------Execute Test---------------------------

            dic[key1] = row1;
            row1.UpdateValue("col6", 5, viewRowCount);
            dic[key1] = row1;

            BinaryDataListRow fetchedRow1 = dic[key1];

            // ------------Assert Results-------------------------
            Assert.AreEqual("col1", fetchedRow1.FetchValue(0, viewRowCount));
            Assert.AreEqual("col2", fetchedRow1.FetchValue(1, viewRowCount));
            Assert.AreEqual("col3", fetchedRow1.FetchValue(2, viewRowCount));
            Assert.AreEqual("col4", fetchedRow1.FetchValue(3, viewRowCount));
            Assert.AreEqual("col5", fetchedRow1.FetchValue(4, viewRowCount));
            Assert.AreEqual("col6", fetchedRow1.FetchValue(5, viewRowCount));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2BinaryStorage_BasicUsage")]
        public void Dev2BinaryStorage_BasicUsage_WhenAddingRemovingAtVolumn_AllDataFetched()
        {

            //------------Setup for test--------------------------
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 4194304); // 4MB buffer ;)

            int cnt = 50000;

            IList<BinaryDataListRow> rows = new List<BinaryDataListRow>(cnt);
            IList<string> keys = new List<string>();


            for (int i = 0; i < cnt; i++)
            {
                BinaryDataListRow row = new BinaryDataListRow(5);
                row.UpdateValue("col1" + Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid(), 0, 5);
                row.UpdateValue("col2", 1, 5);
                row.UpdateValue("col3" + Guid.NewGuid(), 2, 5);
                row.UpdateValue("col4", 3, 5);
                row.UpdateValue("col5" + Guid.NewGuid(), 4, 5);

                rows.Add(row);
                keys.Add(Guid.NewGuid().ToString());
            }

            //------------Execute Test---------------------------

            // add rows
            for (int i = 0; i < cnt; i++)
            {
                dic[keys[i]] = rows[i];

                // fake removals ;)
                if ((i + 1)%100 == 0)
                {
                    dic.Remove(keys[i]);
                }

                // fake compact
                //if((i + 1) % 200 == 0)
                //{
                //    dic.Compact();
                //}
            }

            
            // check first and 2nd to last row, since last was removed ;)
            BinaryDataListRow fetchedRow1 = dic[keys[0]];
            BinaryDataListRow fetchedRow2 = dic[keys[cnt-2]];

            //------------Assert Results-------------------------
            Assert.AreEqual("col2", fetchedRow1.FetchValue(1, 5));
            Assert.AreEqual("col4", fetchedRow1.FetchValue(3, 5));

            Assert.AreEqual("col2", fetchedRow2.FetchValue(1, 5));
            Assert.AreEqual("col4", fetchedRow2.FetchValue(3, 5));

        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("Dev2BinaryStorage_BasicUsage")]
        public void Dev2BinaryStorage_BasicUsage_WhenAddingAndRemovingAll_AllDataFetched()
        {

            //------------Setup for test--------------------------
            Dev2BinaryStorage<BinaryDataListRow> dic = new Dev2BinaryStorage<BinaryDataListRow>(Guid.NewGuid().ToString(), 4194304); // 4MB buffer ;)

            int cnt = 10000;

            IList<BinaryDataListRow> rows = new List<BinaryDataListRow>(cnt);
            IList<string> keys = new List<string>();

            List<Guid> theList = new List<Guid>();
            List<int> itemCount = new List<int>();

            for(int i = 0; i < cnt; i++)
            {
                BinaryDataListRow row = new BinaryDataListRow(5);
                row.UpdateValue("col1" + Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid(), 0, 5);
                row.UpdateValue("col2", 1, 5);
                row.UpdateValue("col3" + Guid.NewGuid(), 2, 5);
                row.UpdateValue("col4", 3, 5);
                row.UpdateValue("col5" + Guid.NewGuid(), 4, 5);

                rows.Add(row);
                Guid key = Guid.NewGuid();
                keys.Add(key.ToString());                
                theList.Add(key);
            }

            //------------Execute Test---------------------------

            // add rows
            //for(int i = 0; i < cnt; i++)
            //{
            //    dic[keys[i]] = rows[i];

            //    int itemsToRemove = dic.ItemCount;
            //    // fake removals ;)
            //    if((i + 1) % 100 == 0)
            //    {
            //        if(i == cnt-1)
            //        {
            //            Assert.AreEqual(38, dic.RemoveAll(theList));                      
            //        }
            //        else
            //        {
            //            Assert.AreEqual(itemsToRemove, dic.RemoveAll(theList));                          
            //        }                    
            //    }                
            //}
        }
    }
}
