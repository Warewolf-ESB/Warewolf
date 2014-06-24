using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Simulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Repository
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SimulationRespositoryTest
    {
        static SimulationRepository _testInstance;
        static ISimulationResult _testResult;
        static object l = new object();

        #region Class Initialize/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            Directory.SetCurrentDirectory(testContext.TestDir);
            _testInstance = SimulationRepository.Instance;
            _testResult = new SimulationResult
            {
                Key = CreateKey(),
                Value = Dev2BinaryDataListFactory.CreateDataList()
            };
            _testInstance.Save(_testResult);
        }

        [ClassCleanup]
        public static void MyClassCleanup()
        {
            _testInstance.Delete(_testResult);
        }

        #endregion

        #region Get Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetWithNull_Expected_ThrowsNullArgumentException()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                thisInstance.Get(null, true);
            }
        }

        [TestMethod]
        public void GetWithValidKey_Expected_ReturnsItem()
        {
            lock(l)
            {
                var result = _testInstance.Get(_testResult.Key);
                Assert.AreSame(result, _testResult);
            }
        }

        [TestMethod]
        public void GetWithInvalidKey_Expected_ReturnsNull()
        {
            lock(l)
            {
                var key = CreateKey();
                var item = _testInstance.Get(key);
                Assert.IsNull(item);
            }
        }

        [TestMethod]
        public void GetWithForce_Expected_ReloadsItem()
        {
            lock(l)
            {
                var item = CreateResult(CreateKey());
                _testInstance.Save(item);

                var result = _testInstance.Get(item.Key, true);
                Assert.AreNotSame(item, result);
            }
        }

        //BUILD
        //[TestMethod]
        //public void GetWithForceKeyNotExists_Expected_ReturnsNull() {
        //    var item = CreateResult(CreateKey());


        //    var result = _testInstance.Get(item.Key, true);
        //    Assert.IsNull(item);
        //}

        #endregion Get Tests

        #region Delete

        [TestMethod]
        public void DeleteWithValidItem_Expected_ItemCountDecreasesByOne()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                var item = CreateResult(CreateKey());
                thisInstance.Save(item);
                var expected = thisInstance.Count - 1;

                thisInstance.Delete(item);
                Assert.AreEqual(expected, thisInstance.Count);
            }
        }

        [TestMethod]
        public void DeleteWithInvalidItem_Expected_ItemCountIsSame()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                int expected = thisInstance.Count;
                ISimulationKey theKey = CreateKey();
                ISimulationResult item = CreateResult(theKey);
                string id1 = theKey.ActivityID;
                string id2 = theKey.ScenarioID;
                string id3 = theKey.WorkflowID;
                Assert.AreEqual(expected, thisInstance.Count, "Fail 1");
                thisInstance.Delete(item); // Problem over here!
                Assert.AreEqual(expected, thisInstance.Count, "Fail 2");
            }
        }

        [TestMethod]
        public void DeleteWithNullItem_Expected_NoOperationPerformed()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                var expected = thisInstance.Count;
                thisInstance.Delete(null);
                Assert.AreEqual(expected, thisInstance.Count);
            }
        }

        #endregion Delete

        #region Save

        [TestMethod]
        public void SaveWithNewItem_Expected_ItemCountIncreasesByOne()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                var expected = thisInstance.Count + 1;
                var item = CreateResult(CreateKey());
                thisInstance.Save(item);
                Assert.AreEqual(expected, thisInstance.Count);
            }
        }

        [TestMethod]
        public void SaveWithExistingItem_Expected_ItemCountIsSame()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                var expected = thisInstance.Count;
                thisInstance.Save(_testResult);
                Assert.AreEqual(expected, thisInstance.Count);
            }
        }

        [TestMethod]
        public void SaveWithNullItem_Expected_NoOperationPerformed()
        {
            lock(l)
            {
                SimulationRepository thisInstance = _testInstance;
                var expected = thisInstance.Count;
                thisInstance.Save(null);
                Assert.AreEqual(expected, thisInstance.Count);
            }
        }

        #endregion Save

        #region Create Helper methods

        static readonly Random Random = new Random();

        static ISimulationKey CreateKey()
        {
            return new SimulationKey
            {
                WorkflowID = string.Format("WID-{0}", Random.Next()),
                ActivityID = string.Format("AID-{0}", Random.Next()),
                ScenarioID = string.Format("SID-{0}", Random.Next())
            };
        }

        static ISimulationResult CreateResult(ISimulationKey key)
        {
            return new SimulationResult
            {
                Key = key,
                Value = Dev2BinaryDataListFactory.CreateDataList()
            };
        }

        #endregion

    }
}
