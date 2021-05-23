/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;
using Dev2.Data.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class ValidateAuthorizationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ValidateAuthorization))]
        public void ValidateAuthorization_DoLogOn_IsNull_ExpectTrue()
        {
            //--------------------------Arrange--------------------------
            var mockValidateAuthorization = new Mock<IDev2LogonProvider>();
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            //--------------------------Act------------------------------
            var doLogOn = ValidateAuthorization.DoLogOn(mockValidateAuthorization.Object, mockActivityIOPath.Object);
            //--------------------------Assert---------------------------
            Assert.IsNull(doLogOn);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ValidateAuthorization))]
        public void ValidateAuthorization_RequiresAuth_safeToken_IsNull_ExpectTrue()
        {
            //--------------------------Arrange--------------------------
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            //--------------------------Act------------------------------
            var doLogOn = ValidateAuthorization.RequiresAuth(mockActivityIOPath.Object, mockDev2LogonProvider.Object);
            //--------------------------Assert---------------------------
            Assert.IsNull(doLogOn);
        }
    }
}
