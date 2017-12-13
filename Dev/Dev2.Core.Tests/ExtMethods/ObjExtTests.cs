using System;
using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.ExtMethods
{
    [TestClass]
    public class ObjExtTests
    {
        [Serializable]
        class Person
        {
            public string name { get; set; }
            public string name1 { get; set; }
            public string name2 { get; set; }
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeepCopy_GivenCallShould_ShouldCopyAll()
        {
            //---------------Set up test pack-------------------
            var p = new Person();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            var deepCopy = ObjectExtensions.DeepCopy(p);
            //---------------Test Result -----------------------
            var memberInfos = p.GetType().GetMembers();
            var copiedMemmbers = deepCopy.GetType().GetMembers();
            Assert.AreEqual(memberInfos.Length, copiedMemmbers.Length);
        }
    }
}
