/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests.Operations
{
    [TestClass]
    public class IsBetweenTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_TrueIsReturned()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "50", "2", "100" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_LargerValueFirst_Invoke_TrueIsReturned()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "50", "100", "2" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_FalseIsReturned()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "100", "2", "50" });
            //assert
            Assert.IsFalse(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_IsFromValue_FalseIsReturned()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "2", "2", "50" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_IsToValue_FalseIsReturned()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "50", "2", "50" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_TrueIsReturned_DateTime()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "2016/10/14", "2016/10/13", "2016/10/16" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_LargerValueFirst_Invoke_TrueIsReturned_DateTime()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "2016/10/14", "2016/10/16", "2016/10/13" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_FalseIsReturned_DateTime()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "2016/10/17", "2016/10/13", "2016/10/16" });
            //assert
            Assert.IsFalse(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_IsFromValue_FalseIsReturned_DateTime()
        {
            //init
            var isBetween = new IsBetween();
            //exe
            var result = isBetween.Invoke(new[] { "2016/10/13", "2016/10/13", "2016/10/16" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_IsToValue_FalseIsReturned_DateTime()
        {
            //init
            var comparer = new IsBetween();
            //exe
            var result = comparer.Invoke(new[] { "2016/10/16", "2016/10/13", "2016/10/16" });
            //assert
            Assert.IsTrue(result, "IsBetween returned the wrong result when comparing integers");
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_Invoke_Type_Null_DateTime_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isBetween = new IsBetween();
            var cols = new string[3];
            cols[0] = null;
            cols[1] = null;
            cols[2] = null;

            //------------Execute Test---------------------------
            var result = isBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IsBetween))]
        public void IsBetween_HandlesType_ReturnsType()
        {
            //------------Setup for test--------------------------
            var isBetween = new IsBetween();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(EnDecisionType.IsBetween, isBetween.HandlesType());
        }
    }
}
