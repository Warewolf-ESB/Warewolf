/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class SingleApiTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SingleApi))]
        public void SingleApi_SetProperties_AreEqual_ToPropertyValue_ExpertTrue()
        {
            //----------------------Arrange-----------------------
            var contactTest = new MaintainerApi()
            {
                Fn = "TestName TestLastName",
                Email = "ashley.lewis@dev2.co.za",
                Url = "https://warewolf.io",
                Org = "https://dev2.co.za",
                Adr = "Bellevue, Kloof",
                Tel = "7777",
                XTwitter = "@warewolf",
                XGithub = "Warewolf-ESB/Warewolf",
                Photo = "https://warewolf.io/testimages/logo.png",
                VCard = "39A03A58-978F-4CFB-B1D1-3EFA6C55E380"
            };
            var contactList = new List<MaintainerApi>();
            contactList.Add(contactTest);

            var propertyApi = new PropertyApi()
            {
                Type = "TestType",
                Value = EnvironmentVariables.PublicWebServerUri + "secure/Hello World.api",
            };
            var propertyApisList = new List<PropertyApi>();
            propertyApisList.Add(propertyApi);

            var baseUrl = EnvironmentVariables.PublicWebServerUri + "secure/Acceptance Testing Resources/Execution Engine/Execution Engine Test.json";
            var description = "TestDiscription";
            var humanUrl = "https://warewolf.io";
            var image = "https://warewolf.io/testimages/logo.png";
            var name = "TestName";
            var tags = new List<string>();
            var version = "1.0.1.1";
            //----------------------Assert------------------------
            var singleApi = new SingleApi()
            {
                BaseUrl = baseUrl,
                Contact = contactList,
                Description = description,
                HumanUrl = humanUrl,
                Image = image,
                Name = name,
                Properties = propertyApisList,
                Tags = tags,
                Version = version,
            };
            //----------------------Act---------------------------
            Assert.AreEqual(baseUrl, singleApi.BaseUrl);
            Assert.AreEqual(contactList, singleApi.Contact);
            Assert.AreEqual(description, singleApi.Description);
            Assert.AreEqual(humanUrl, singleApi.HumanUrl);
            Assert.AreEqual(image, singleApi.Image);
            Assert.AreEqual(name, singleApi.Name);
            Assert.AreEqual(propertyApisList, singleApi.Properties);
            Assert.AreEqual(tags, singleApi.Tags);
            Assert.AreEqual(version, singleApi.Version);
        }
    }
}
