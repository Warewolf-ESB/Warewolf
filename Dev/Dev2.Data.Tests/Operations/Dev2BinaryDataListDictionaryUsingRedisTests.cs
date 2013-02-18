using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Redis;
using ServiceStack.Common.Extensions;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    [Ignore]
    public class Dev2BinaryDataListDictionaryUsingRedisTests
    {
        //Hagashen Naidu
        [TestMethod]
        public void Construct_Where_NoHostName_Expected_RedisClient()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var redisClient = new RedisClient();
            //------------Assert Results-------------------------
            Assert.IsNotNull(redisClient);
        }    
        
        //Hagashen Naidu
        [TestMethod]
        public void RedisClient_Where_SingleIBinaryListItem_Expected_ItemInStore()
        {
            //------------Setup for test--------------------------
            var redisClient = new RedisClient();
            var dataListEntry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.EvalautionScalar, string.Empty);
            var dataListItem = dataListEntry.FetchScalar();
            dataListItem.UpdateValue("myValue");
            redisClient.As<IBinaryDataListItem>();
            //------------Execute Test---------------------------
            redisClient.Set("myKey",dataListItem);
            //------------Assert Results-------------------------
            var retrievedItem = redisClient.Get<IBinaryDataListItem>("myKey");
            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(dataListItem.TheValue, retrievedItem.TheValue);
        }

        [TestMethod]
        public void RedisClient_Where_SingleIBinaryDataList_Expected_ItemInStore()
        {
            //------------Setup for test--------------------------
            var redisClient = new RedisClient();
            var binaryDataList = BuildData();
            redisClient.As<IBinaryDataList>();
            var key = binaryDataList.UID.ToString();
            //------------Execute Test---------------------------
            redisClient.Set(key, binaryDataList);
            //------------Assert Results-------------------------
            var retrievedItem = redisClient.Get<IBinaryDataList>(key);
            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(binaryDataList.UID, retrievedItem.UID);
        }


        [TestMethod]
        public void RedisClient_Where_Multiple100kIBinaryDataList_Expected_ItemInStore()
        {
            //------------Setup for test--------------------------
            var redisClient = new RedisClient();
            redisClient.FlushAll();
            redisClient.As<IBinaryDataListEntry>();
            //------------Execute Test---------------------------
            List<string> keyList = new List<string>();
            var stopwatch = Stopwatch.StartNew();
            for(int i = 0; i < 20000; i++)
            {
                var binaryDataList = BuildData();
                binaryDataList.FetchAllEntries().ForEach(entry =>
                {
                    var stopwatch1 = Stopwatch.StartNew();
                    var key = Guid.NewGuid().ToString();
                    redisClient.Set(key, entry);
                    keyList.Add(key);
                    stopwatch1.Stop();
                    Console.WriteLine(stopwatch1.Elapsed + " Item: " + entry.Namespace);
                });
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
           //------------Assert Results-------------------------
            var retrievedItems = redisClient.GetAll<IBinaryDataListEntry>(keyList);
            Assert.AreEqual(keyList.Count,retrievedItems.Count);
            CollectionAssert.AllItemsAreUnique(retrievedItems.Values.ToList());
            redisClient.FlushAll();
        }


        [TestMethod]
        public void RedisClient_Where_Multiple1MilIBinaryDataList_Expected_ItemInStore()
        {
            //------------Setup for test--------------------------
            var redisClient = new RedisClient();
            redisClient.FlushAll();
            redisClient.As<IBinaryDataList>();
            //------------Execute Test---------------------------
            List<string> keyList = new List<string>();
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000000; i++)
            {
                var binaryDataList = BuildData();
                var key = binaryDataList.UID.ToString();
                redisClient.Set(key, binaryDataList);
                keyList.Add(key);
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            //------------Assert Results-------------------------
            stopwatch.Restart();
            var retrievedItems = redisClient.GetAll<IBinaryDataList>(keyList);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Assert.AreEqual(keyList.Count, retrievedItems.Count);
            CollectionAssert.AllItemsAreUnique(retrievedItems.Values.ToList());

        }



        [TestMethod]
        public void RedisClient_Where_BatchBinaryDataList_Expected_ItemInStore()
        {
            //------------Setup for test--------------------------
            var redisClient = new RedisClient();
            redisClient.FlushAll();
            redisClient.As<IBinaryDataList>();
            //------------Execute Test---------------------------
            List<string> keyList = new List<string>();
            Dictionary<string,IBinaryDataList> data = new Dictionary<string, IBinaryDataList>();
            var stopwatch = Stopwatch.StartNew();
            for(var j = 0; j < 10000; j++)
            {
                for(var i = 0; i < 100; i++)
                {
                    var binaryDataList = BuildData();
                    var key = binaryDataList.UID.ToString();
                    data.Add(key,binaryDataList);
                    keyList.Add(key);
                }
                redisClient.SetAll<IBinaryDataList>(data);
                data.Clear();
            }
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            //------------Assert Results-------------------------
            stopwatch.Restart();
            var retrievedItems = redisClient.GetAll<IBinaryDataList>(keyList);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Assert.AreEqual(keyList.Count, retrievedItems.Count);
            CollectionAssert.AllItemsAreUnique(retrievedItems.Values.ToList());
            redisClient.FlushAll();
        }
        

        private IBinaryDataList BuildData()
        {
            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));
            string error;
            var dl2 = Dev2BinaryDataListFactory.CreateDataList();
            
            dl2.TryCreateScalarTemplate(string.Empty, "idx", "A scalar", true, out error);
            dl2.TryCreateScalarTemplate(string.Empty, "idx2", "A another scalar", true, out error);
            dl2.TryCreateScalarTemplate(string.Empty, "idx3", "A another scalar 1", true, out error);
            dl2.TryCreateScalarTemplate(string.Empty, "idx4", "A another scalar 2", true, out error);
            dl2.TryCreateScalarValue("1", "idx", out error);
            dl2.TryCreateScalarValue("2", "idx2", out error);
            dl2.TryCreateScalarValue("3", "idx3", out error);
            dl2.TryCreateScalarValue("4", "idx4", out error);

            dl2.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            dl2.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl2.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl2.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            dl2.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl2.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            dl2.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);
            return dl2;
        }
    }
}
