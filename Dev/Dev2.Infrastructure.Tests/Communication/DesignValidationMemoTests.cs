/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Communication
{
    [TestClass]
    public class DesignValidationMemoTests
    {
        [TestMethod]
        [Description("Constructor must initialize Errors list and set IsValid to true.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        
        public void DesignValidationMemoConstructor_UnitTest_Intialization_ErrorsNotNullAndIsValidTrue()

        {
            var memo = new DesignValidationMemo();
            Assert.IsNotNull(memo.Errors);
            Assert.IsTrue(memo.IsValid);
        }
    }
}
