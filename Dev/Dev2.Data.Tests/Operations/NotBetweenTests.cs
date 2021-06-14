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

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class NotBetweenTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_Double_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = "15";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_Double_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = "30";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_Double_And_DateTime_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = "2019/02/12";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_DateTime_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = "2019/02/12";
            cols[1] = "2019/02/11";
            cols[2] = "2019/02/13";

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_DateTime_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = "2019/02/12";
            cols[1] = "2019/02/11";
            cols[2] = "2019/02/10";

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_Unknown_DateTime_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = "2019/02/1234";
            cols[1] = "2019/02/11";
            cols[2] = "2019/02/10";

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_Invoke_Type_Null_DateTime_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            var cols = new string[3];
            cols[0] = null;
            cols[1] = null;
            cols[2] = null;

            //------------Execute Test---------------------------
            var result = notBetween.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NotBetween))]
        public void NotBetween_HandlesType_ReturnsType()
        {
            //------------Setup for test--------------------------
            var notBetween = new NotBetween();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(EnDecisionType.NotBetween, notBetween.HandlesType());
        }
    }
}
