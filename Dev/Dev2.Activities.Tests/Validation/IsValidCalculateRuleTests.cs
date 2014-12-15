
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Validation
{
    [TestClass]
    public class IsValidCalculateRuleTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void IsValidCalculateRule_Ctor_NullGet_ExpectError()

        {
            //------------Setup for test--------------------------
// ReSharper disable ObjectCreationAsStatement
            new IsValidCalculateRule(null);
// ReSharper restore ObjectCreationAsStatement
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Check")]
        public void IsValidCalculateRule_Check()
        {

            var x = new IsValidCalculateRule(() => "!~calculation~![[a]]+[[b]]!~~calculation~!");
            Assert.IsNull(x.Check());
        }

        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Check")]
        public void IsValidCalculateRule_Check_Literals()
        {

            var x = new IsValidCalculateRule(() => "!~calculation~!1+2!~~calculation~!");
            Assert.IsNull(x.Check());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Check")]
        public void IsValidCalculateRule_Check_NonCalculationString()
        {

            var x = new IsValidCalculateRule(() => "a");
            Assert.IsNull(x.Check());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Check")]
        public void IsValidCalculateRule_Check_Functions_Valid()
        {

            var x = new IsValidCalculateRule(() => "!~calculation~![[a]]+mod([[b]],2)!~~calculation~!");
            Assert.IsNull(x.Check());
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Check")]
        public void IsValidCalculateRule_Check_Functions_InValid()
        {

            var x = new IsValidCalculateRule(() => "!~calculation~![[a]]+meed( [[b]]!~~calculation~!");
            Assert.IsNotNull(x.Check());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IsValidCalculateRule_Check")]
        public void IsValidCalculateRule_Check_Functions_InValidFunctionSyntax()
        {

            var x = new IsValidCalculateRule(() => "!~calculation~![[a]]+mod( [[b]]!~~calculation~!");
            Assert.IsNotNull(x.Check());
        }
        // ReSharper restore InconsistentNaming
    }
}
