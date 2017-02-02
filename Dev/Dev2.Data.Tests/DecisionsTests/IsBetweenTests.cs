/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsBetweenTests
    {
        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsBetween_HandlesType")]
        public void IsBetween_HandlesType_ReturnsIsBetweenType()
        {
            var decisionType = enDecisionType.IsBetween;
            //------------Setup for test--------------------------
            var isBetween = new IsBetween();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isBetween.HandlesType());
        }
    }
}
