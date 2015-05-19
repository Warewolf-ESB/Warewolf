
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
    public class PermissionsModifiedMemoTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("PermissionsModifiedMemo_Constructor")]
        public void PermissionsModifiedMemo_Constructor_Initializes_Properties  ()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var permissionsModifiedMemo = new PermissionsModifiedMemo();

            //------------Assert Results-------------------------
            Assert.IsNotNull(permissionsModifiedMemo.ModifiedPermissions);

        }
    }
}
