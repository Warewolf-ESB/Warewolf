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
using Warewolf.Driver.Serilog;

namespace Warewolf.Tests.Sinks
{ 
    [TestClass]
    public class ElasticConfigTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SeriLogELasticConfig))]
        public void SerilogElasticConfig_NoParamConstructor_Returns_Default()
        {
            //---------------------------------Arrange-----------------------------
            var elasticConfig = new SeriLogELasticConfig();
            //---------------------------------Assert------------------------------
            Assert.IsNotNull(elasticConfig.Logger);
            Assert.IsNull(elasticConfig.ServerLoggingAddress);
        }
    }
}