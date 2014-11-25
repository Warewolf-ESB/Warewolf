
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Collections;
using Dev2.Services.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Core.Tests.Services
{
    [TestClass]
    public class HelpConfigurationTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpConfiguration_Constructor")]
        public void HelpConfiguration_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var helpConfiguration = new HelpConfiguration();

            //------------Assert Results-------------------------
            Assert.IsNotNull(helpConfiguration.IsCollapsed);
            Assert.IsInstanceOfType(helpConfiguration.IsCollapsed, typeof(ConcurrentDictionarySafe<Type, bool>));            
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("HelpConfiguration_IsCollapsed")]
        public void HelpConfiguration_IsCollapsed_HasCustomJsonConverterAttribute()
        {
            //------------Setup for test--------------------------
            var converterType = typeof(ConcurrentDictionarySafeConverter<Type, bool>);

            var type = typeof(HelpConfiguration);
            var property = type.GetProperty("IsCollapsed");

            //------------Execute Test---------------------------
            var jsonConverterAttribute = (JsonConverterAttribute)property.GetCustomAttributes(typeof(JsonConverterAttribute), true)[0];

            //------------Assert Results-------------------------
            Assert.AreEqual(converterType, jsonConverterAttribute.ConverterType);
        }
    }
}
