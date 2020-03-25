/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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
            var source = new SerilogElasticsearchSource
            {
                ResourceID = Guid.Parse("24e12ae4-58b6-4fec-b521-48493230fef7"),
                HostName = "localhost",
                Port = "9200",
                ResourceName = "TestSource"
            };
            var config = new SeriLogElasticsearchConfig(source);
            Assert.IsNotNull(config.Logger);
            Assert.IsNotNull(config.Endpoint);
        }
    }
}