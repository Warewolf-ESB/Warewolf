/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Driver.Serilog;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class SeriLogElasticsearchTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SeriLogElasticsearchConfig))]
        public void SeriLogELasticsearchConfig_NoParamConstructor_Returns_Default()
        {
            var source = new ElasticsearchSource();
            var config = new SeriLogElasticsearchConfig(source);
            Assert.IsNotNull(config.Logger);
            Assert.IsNotNull(config.Endpoint);
        }
    }
}