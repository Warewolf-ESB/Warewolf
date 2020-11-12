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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class PersistenceSettingsReadTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_Execute_()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var persistenceSettingsRead = new PersistenceSettingsRead();
            var jsonResult = persistenceSettingsRead.Execute(null, null);

            //------------Assert Results-------------------------

            Assert.AreEqual("{\"$id\":\"1\",\"$type\":\"Dev2.Services.Persistence.PersistenceSettingsTo, Dev2.Infrastructure\"}", jsonResult.ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_CreateSecurityReadEndPoint_IsInstanceOfSecurityRead()
        {
            //------------Setup for test--------------------------
            var securityRead = new Func<IEsbManagementEndpoint>(() => null);

            var settingsRead = new TestSettingsRead(securityRead);

            //------------Execute Test---------------------------
            var endpoint = settingsRead.TestCreateSecurityReadEndPoint();

            //------------Assert Results-------------------------
            Assert.IsNotNull(endpoint);
            Assert.IsInstanceOfType(endpoint, typeof(SecurityRead));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_HandlesType_ReturnsSettingsReadService()
        {
            var persistenceSettingsRead = new PersistenceSettingsRead();
            var result = persistenceSettingsRead.HandlesType();
            Assert.AreEqual(nameof(PersistenceSettingsRead), result);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_CreateServiceEntry_ReturnsDynamicService()
        {
            var esb = new PersistenceSettingsRead();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }
    }
}
