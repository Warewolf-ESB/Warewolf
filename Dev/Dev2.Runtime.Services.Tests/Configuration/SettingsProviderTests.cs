/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Runtime.Configuration;
using Dev2.Runtime.Configuration.Tests.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Runtime.Services.Tests
{
    [TestClass]
    public class SettingsProviderTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LoadRuntimeConfigurations))]
        public void SettingsProvider_WebServerUri_NotNull_ExpectFail()
        {
            //-------------------Arrange------------------
            EnvironmentVariables.WebServerUri = "http://TESTUSERNAME:8080/";
            var mockWriter = new Mock<IWriter>();
            //-------------------Act----------------------
            new LoadRuntimeConfigurations(mockWriter.Object).Execute();
            //-------------------Assert-------------------
            mockWriter.Verify(o => o.WriteLine("done."), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LoadRuntimeConfigurations))]
        public void SettingsProvider_WebServerUri_Null_ExpectFail1()
        {
            SettingsProvider.WebServerUri = EnvironmentVariables.WebServerUri;
            var settings = SettingsProvider.Instance.Configuration;

        }
    }
}
