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
    public class ElasticsearchConfigTests
    {
        public string ElasticsearchURL { get; private set; }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SeriLogElasticsearchConfig))]
        public void SerilogElasticsearchConfig_NoParamConstructor_Returns_Default()
        {
            //---------------------------------Arrange-----------------------------
            var elasticSearchConfig = new SeriLogElasticsearchConfig();
            //---------------------------------Assert------------------------------
            Assert.AreEqual(expected: @"http://localhost:9200", actual: elasticSearchConfig.Url);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SeriLogElasticsearchConfig))]
        public void SerilogElasticsearchConfig_WithParamConstructor_Returns_Correct_Settings()
        {
            //---------------------------------Arrange-----------------------------
            var settings = new SeriLogElasticsearchConfig.Settings
            {
                Url = "http://localhost:9200",    
            };
            var elasticSearchConfig = new SeriLogElasticsearchConfig(settings);
            //---------------------------------Assert------------------------------
            Assert.AreEqual(expected: @"http://localhost:9200", actual: elasticSearchConfig.Url);
            Assert.IsNotNull(elasticSearchConfig.Logger);
        }        
    }
}