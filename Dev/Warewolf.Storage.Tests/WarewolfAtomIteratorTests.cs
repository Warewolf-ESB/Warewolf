using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class WarewolfAtomIteratorTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void NewInstance_ShouldHave_Constructor()
        {
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            Assert.IsNotNull(warewolfAtomIterator);
            var privateObj = new PrivateObject(warewolfAtomIterator);
            var listRes = privateObj.GetField("_listResult");
            var maxVal = privateObj.GetField("_maxValue");
            Assert.IsNotNull(listRes);
            Assert.IsNotNull(maxVal);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfAtomIterator_GetLength_ShouldBeEqualToMaxVal()
        {
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            Assert.IsNotNull(warewolfAtomIterator);
            var privateObj = new PrivateObject(warewolfAtomIterator);
            var maxVal = (int) privateObj.GetField("_maxValue");
            Assert.IsNotNull(maxVal);
            var length = warewolfAtomIterator.GetLength();
            Assert.AreEqual(maxVal, length);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenListResultIsNull_WarewolfAtomIterator_GetNextValue_ShouldReturnNull()
        {
            var warewolfAtomIterator = new WarewolfAtomIterator(new List<DataStorage.WarewolfAtom>());
            Assert.IsNotNull(warewolfAtomIterator);
            var privateObj = new PrivateObject(warewolfAtomIterator);
            privateObj.GetField("_listResult");
            privateObj.SetField("_listResult", null);
            Assert.IsNotNull(warewolfAtomIterator);
            Assert.IsNull(warewolfAtomIterator.GetNextValue());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenListRes_WarewolfAtomIterator_GetNextValue_ShouldReturn()
        {
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>
            {
                DataStorage.WarewolfAtom.NewDataString("a"),
                DataStorage.WarewolfAtom.NewDataString("b"),
                DataStorage.WarewolfAtom.NewDataString("c")
            };
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            Assert.IsNotNull(warewolfAtomIterator);
            var privateObj = new PrivateObject(warewolfAtomIterator);
            privateObj.GetField("_listResult");
            var value = warewolfAtomIterator.GetNextValue();
            Assert.AreEqual(listResult.First(), value);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WarewolfAtomIterator_GetNextValue_ShouldReturn()
        {
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            Assert.IsNotNull(warewolfAtomIterator);
            var privateObj = new PrivateObject(warewolfAtomIterator);
            privateObj.GetField("_listResult");
            warewolfAtomIterator.GetNextValue();
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GivenEmptyListResult_WarewolfAtomIterator_HasMoreData_ShouldReturnFalse()
        {
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            Assert.IsNotNull(warewolfAtomIterator);
            var hasMoreData = warewolfAtomIterator.HasMoreData();
            Assert.IsFalse(hasMoreData);
        }
    }
}