using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Core;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class ServiceInputNameComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceInputNameComparer_GivenServiceInputsDuplicates_ComparesCorrectly()
        {
            var inputs1 = new List<IServiceInput>()
            {
                new ServiceInput("id", "1"),
                new ServiceInput("ID", "1"),
                new ServiceInput("id", "1"),
                new ServiceInput("name", "1"),
                new ServiceInput("NAME", "1"),
            };
            //---------------Assert Precondition----------------
            var count = inputs1.Count;
            Assert.AreEqual(5, count);
            //---------------Execute Test ----------------------
            var distinct = inputs1.Distinct(new ServiceInputNameComparer()).Count();
            //---------------Test Result -----------------------
            Assert.AreEqual(4, distinct);
        }
    }
}
