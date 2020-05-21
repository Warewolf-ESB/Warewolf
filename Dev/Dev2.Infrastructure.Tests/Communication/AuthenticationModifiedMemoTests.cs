/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    public class AuthenticationModifiedMemoTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AuthenticationModifiedMemo))]
        public void AuthenticationModifiedMemo_Constructor_Initializes_Properties  ()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var authenticationModifiedMemo = new AuthenticationModifiedMemo();

            //------------Assert Results-------------------------
            Assert.IsNotNull(authenticationModifiedMemo.ModifiedAuthentication);

        }
    }
}