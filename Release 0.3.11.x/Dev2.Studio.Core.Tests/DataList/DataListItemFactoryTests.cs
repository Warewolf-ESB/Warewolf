using System;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.DataList
{
    [TestClass]
    public class DataListItemFactoryTests
    {
        [TestMethod]
        public void CreateDataListModelWithParentAndDisplayNameShouldCorrectlySetName()
        {
            //-------------------------Setup---------------------------------------
            IDataListItemModel parentItem = DataListItemModelFactory.CreateDataListModel("testRec");
            //-------------------------Execute-----------------------------------
            IDataListItemModel childItem = DataListItemModelFactory.CreateDataListModel("field1", "this is a field", parentItem);
            //-------------------------Assert-------------------------------------
            Assert.AreEqual("field1",childItem.Name);
            Assert.AreEqual("testRec", childItem.Parent.Name);
        }
    }
}
