using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Runtime.Services;

namespace Dev2.Tests.Runtime.Dev2.Runtime.Services.Tests
{
    [TestClass]
    public class DataSourcesListTests
    {
        [TestMethod]
        public void List_Expected_ReturnedList()
        {
            var getList = new DataSources();
            Assert.AreNotEqual(getList.list("").Count, 0);
        }
    }
}
