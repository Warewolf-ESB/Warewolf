using System.Collections;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Translators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests
{
    [TestClass]
    public class TranslationConstantsTest
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void TranslationConstants_ShouldReturn()
        {
            var list = TranslationConstants.systemTags as IList;
            Assert.IsTrue(list.Contains(enSystemTag.PostData));
            Assert.IsTrue(list.Contains(enSystemTag.InstanceId));
            Assert.IsTrue(list.Contains(enSystemTag.SystemModel));
        }        
    }
}
