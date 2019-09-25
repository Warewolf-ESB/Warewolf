/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class LogServerTests
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_Constructor_WhenValidParameters_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------

            //--------------------------------Act-----------------------------------
            //--------------------------------Assert--------------------------------
        }

    }
}
