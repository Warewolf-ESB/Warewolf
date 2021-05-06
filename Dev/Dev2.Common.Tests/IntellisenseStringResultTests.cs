/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class IntellisenseStringResultTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(IntellisenseStringResult))]
        public void IntellisenseStringResult_Validate()
        {
            const string expectedResult = "some result";
            const int expectedCaretPosition = 44;
            var intellisenseStringResult = new IntellisenseStringResult(expectedResult, expectedCaretPosition);

            Assert.AreEqual(expectedResult, intellisenseStringResult.Result);
            Assert.AreEqual(expectedCaretPosition, intellisenseStringResult.CaretPosition);
        }
    }
}
