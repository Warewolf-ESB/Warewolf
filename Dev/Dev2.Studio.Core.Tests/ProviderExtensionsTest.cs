using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dev2.Intellisense.Provider;

namespace Dev2.Core.Tests
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class ProviderExtensionsTest
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ScalarAndPositionIsInsideVariable_Scalar()
        {
            const string data = "[[var1]]";
            var region = data.RegionInPostion(3);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ScalarAndPositionIsZero_EmptyString()
        {
            const string data = "[[var1]]";
            var region = data.RegionInPostion(0);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ScalarAndPositionIsGreaterThanStringLength_EmptyString()
        {
            const string data = "[[var1]]";
            var region = data.RegionInPostion(100);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_SomeTextAndScalarAndPositionIsInsideVariable_Scalar()
        {
            const string data = "Some text [[var1]]";
            var region = data.RegionInPostion(13);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(10, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_SomeTextAndScalarAndPositionIsZero_EmptyString()
        {
            const string data = "Some text [[var1]]";
            var region = data.RegionInPostion(0);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        public void ProviderExtensions_RegionInPosition_SomeTextAndScalarAndPositionIsGreaterThanStringLength_EmptyString()
        {
            const string data = "Some text [[var1]]";
            var region = data.RegionInPostion(100);
            Assert.AreEqual("", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_TwoScalarsAndPositionIsInsideFirstVariable_Scalar()
        {
            const string data = "[[var1]] [[var2]]";
            var region = data.RegionInPostion(3);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_TwoScalarsAndPositionIsInsideSecondVariable_Scalar()
        {
            const string data = "[[var1]] [[var2]]";
            var region = data.RegionInPostion(13);
            Assert.AreEqual("[[var2]]", region.Name);
            Assert.AreEqual(9, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsTheMainVariable_EntireComplexVariable()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(2);
            Assert.AreEqual(data, region.Name);
            Assert.AreEqual(0, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsTheFirstVariable_OnlyFirstVariableReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(4);
            Assert.AreEqual("[[var1]]", region.Name);
            Assert.AreEqual(2, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsTheSecondVariable_OnlySecondVariableReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(13);
            Assert.AreEqual("[[var2]]", region.Name);
            Assert.AreEqual(10, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsRecordset_OnlyRecordsetReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(21);
            Assert.AreEqual("[[rec([[indexVar]]).fielA]]", region.Name);
            Assert.AreEqual(18, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_RegionInPosition")]
        public void ProviderExtensions_RegionInPosition_ComplexVariableAndPositionIsRecordsetIndex_OnlyRecordsetIndexReturned()
        {
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var region = data.RegionInPostion(27);
            Assert.AreEqual("[[indexVar]]", region.Name);
            Assert.AreEqual(24, region.StartIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_AllIndexesOf")]
        public void ProviderExtensions_AllIndexesOf_TextStartingWithVar_Indexes4And12()
        {
            var expected = new List<int> { 4, 12 };
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var actual = data.AllIndexesOf("var");
            CollectionAssert.AreEqual(actual.ToList(), expected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_AllIndexesOf")]
        public void ProviderExtensions_AllIndexesOf_StringIsEmpty_Indexes0()
        {
            var expected = new List<int> { 0 };
            const string data = "";
            var actual = data.AllIndexesOf("var");
            CollectionAssert.AreEqual(actual.ToList(), expected);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ProviderExtensions_AllIndexesOf")]
        public void ProviderExtensions_AllIndexesOf_SearchStringIsEmpty_Indexes0()
        {
            var expected = new List<int> { 0 };
            const string data = "[[[[var1]][[var2]][[rec([[indexVar]]).fielA]]]]";
            var actual = data.AllIndexesOf("");
            CollectionAssert.AreEqual(actual.ToList(), expected);
        }
    }
}
