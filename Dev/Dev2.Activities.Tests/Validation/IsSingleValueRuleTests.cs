
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Validation
{
    [TestClass]
    public class IsSingleValueRuleTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_Single_ExpectNull()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec().a]]");


            //------------Execute Test---------------------------
            Assert.IsNull(isSingleValueRule.Check());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_Scalar_ExpectNull()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec]]");


            //------------Execute Test---------------------------
            Assert.IsNull(isSingleValueRule.Check());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Ctor_Single_Expectmessage_Has_Default()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec().a]]");


            //------------Execute Test---------------------------
            Assert.IsNull(isSingleValueRule.Check());
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_SingleNested_ExpectNull()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec([[rec().b]]).a]]");


            //------------Execute Test---------------------------
            Assert.IsNull(isSingleValueRule.Check());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_TwoIndexes_ExpectError()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec().a]],[[rec().a]]");

            Assert.AreEqual("result field only allows a single result", isSingleValueRule.ErrorText);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_Two_Scalars_ExpectError()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec]],[[bob]]");

            Assert.AreEqual("result field only allows a single result", isSingleValueRule.ErrorText);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_NoColumSpecified_ExpectError()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec()]]");

            Assert.AreEqual("result field only allows a single result", isSingleValueRule.ErrorText);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_NoColumSpecifiedStar_ExpectError()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec(*)]]");

            Assert.AreEqual("result field only allows a single result", isSingleValueRule.ErrorText);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingleValueRule_Check")]
        // ReSharper disable InconsistentNaming
        public void IsSingleValueRule_Check_TwoIndexes_ExpectErrorNoComma()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var isSingleValueRule = new IsSingleValueRule(() => "[[rec().a]][[rec().a]]");

            GetValue(isSingleValueRule);
        }

        static void GetValue(IsSingleValueRule isSingleValueRule)
        {
            //------------Execute Test---------------------------
            var err = isSingleValueRule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(err);
            Assert.AreEqual("The result field only allows a single result", err.Message);
        }
    }
}
