/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Configuration;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetPersistenceSettingsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetPersistenceSettings))]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetPersistenceSettings_Execute_NullValues_Fails()
        {
            //------------Setup for test--------------------------
            var getPersistenceSettings = new GetPersistenceSettings();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var executeResults = getPersistenceSettings.Execute(null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetPersistenceSettings))]
        public void GetPersistenceSettings_Execute()
        {
            //------------Setup for test--------------------------
            var workspaceMock = new Mock<IWorkspace>();
            var requestArgs = new Dictionary<string, StringBuilder>();

            var getPersistenceSettings = new GetPersistenceSettings();
            var serializer = new Dev2JsonSerializer();

            //------------Execute Test---------------------------
            var executeResults = getPersistenceSettings.Execute(requestArgs, workspaceMock.Object);
            var result = serializer.Deserialize<PersistenceSettingsData>(executeResults);

            //------------Assert Results-------------------------
            Assert.IsNotNull(result.PersistenceDataSource);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GetPersistenceSettings))]
        public void GetPersistenceSettings_CreateServiceEntry_ExpectActions()
        {
            //------------Setup for test--------------------------
            var getPersistenceSettings = new GetPersistenceSettings();
            //------------Execute Test---------------------------
            var dynamicService = getPersistenceSettings.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dynamicService);
            Assert.IsNotNull(dynamicService.Actions);
        }
    }
}
