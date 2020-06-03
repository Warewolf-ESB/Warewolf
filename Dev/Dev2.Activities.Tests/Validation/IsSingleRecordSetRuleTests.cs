/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
    [TestClass ]
    public class IsSingleRecordSetRuleTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Check_Single_ExpectNull()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(()=>"[[rec().a]]");

            
            //------------Execute Test---------------------------
            Assert.IsNull(isSingeRecordSetRule.Check());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Ctor_Single_Expectmessage_Has_Default()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(() => "[[rec().a]]");


            //------------Execute Test---------------------------
            Assert.IsNull(isSingeRecordSetRule.Check());
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Check_SingleNested_ExpectNull()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(() => "[[rec([[rec().b]]).a]]");


            //------------Execute Test---------------------------
            Assert.IsNull(isSingeRecordSetRule.Check());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Check_TwoIndexes_ExpectError()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(() => "[[rec().a]],[[rec().a]]");

            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.IsSingeRecordSortFieldInvalidErrorTest, isSingeRecordSetRule.ErrorText);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Check_NoColumSpecified_ExpectError()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(() => "[[rec()]]");

            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.IsSingeRecordSortFieldInvalidErrorTest, isSingeRecordSetRule.ErrorText);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Check_NoColumSpecifiedStar_ExpectError()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(() => "[[rec(*)]]");

            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.IsSingeRecordSortFieldInvalidErrorTest, isSingeRecordSetRule.ErrorText);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsSingeRecordSetRule_Check")]

        public void IsSingeRecordSetRule_Check_TwoIndexes_ExpectErrorNoComma()

        {
            //------------Setup for test--------------------------
            var isSingeRecordSetRule = new IsSingleRecordSetRule(() => "[[rec().a]][[rec().a]]");

            GetValue(isSingeRecordSetRule);
        }

        static void GetValue(IsSingleRecordSetRule isSingeRecordSetRule)
        {
            //------------Execute Test---------------------------
            var err = isSingeRecordSetRule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(err);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.IsSingeRecordSortFieldInvalidErrorTest, err.Message);
        }
    }
}
