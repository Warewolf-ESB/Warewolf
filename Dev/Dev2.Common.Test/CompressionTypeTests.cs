using System;
using System.Collections.Generic;
using Dev2.Common.Lookups;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class CompressionTypeTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CompressionType_GetTypes")]
        public void CompressionType_GetTypes_ConstructsACollectionOfCompressionTypes_CollectionIsValid()
        {
            var expected = new List<CompressionType>
                {
                    new CompressionType("None", "No Compression"),
                    new CompressionType("Partial", "Best Speed"),
                    new CompressionType("Normal", "Default"),
                    new CompressionType("Max", "Best Compression")
                };

            var actual = CompressionType.GetTypes();
            Assert.IsTrue(actual.Count == 4);
            Assert.AreEqual(expected[0].DisplayName, actual[0].DisplayName);
            Assert.AreEqual(expected[1].DisplayName, actual[1].DisplayName);
            Assert.AreEqual(expected[2].DisplayName, actual[2].DisplayName);
            Assert.AreEqual(expected[3].DisplayName, actual[3].DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CompressionType_GetTypes")]
        public void CompressionType_GetName_CompressionType_MatchesName()
        {
            Assert.AreEqual("None", CompressionType.GetName("No Compression"));
            Assert.AreEqual("Partial", CompressionType.GetName("Best Speed"));
            Assert.AreEqual("Normal", CompressionType.GetName("Default"));
            Assert.AreEqual("Max", CompressionType.GetName("Best Compression"));
            Assert.AreEqual("", CompressionType.GetName(null));
            Assert.AreEqual("", CompressionType.GetName(""));
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CompressionType_Contruct")]
        public void CompressionType_Contruct_CompressionType_DisplayNameIsFormed()
        {
            Assert.AreEqual("None (No Compression)", new CompressionType("None", "No Compression").DisplayName);
            Assert.AreEqual("Partial (Best Speed)", new CompressionType("Partial", "Best Speed").DisplayName);
            Assert.AreEqual("Normal (Default)", new CompressionType("Normal", "Default").DisplayName);
            Assert.AreEqual("Max (Best Compression)", new CompressionType("Max", "Best Compression").DisplayName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CompressionType_Contruct")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompressionType_Contruct_NullArguments_ExceptionIsThrown()
        {
            new CompressionType("None", null);
            new CompressionType(null, "No Compression");
        }
    }
}
