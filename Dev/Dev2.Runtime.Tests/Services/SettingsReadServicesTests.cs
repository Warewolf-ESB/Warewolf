/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Chatbot;
using Dev2.Services.Persistence;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SettingsReadServicesTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ChatbotSettingsRead))]
        public void ChatbotSettingsRead_HandlesType_ReturnsClassName()
        {
            Assert.AreEqual("ChatbotSettingsRead", new ChatbotSettingsRead().HandlesType());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ChatbotSettingsRead))]
        public void ChatbotSettingsRead_CreateServiceEntry_NamedAfterHandlesType()
        {
            var service = new ChatbotSettingsRead();

            var entry = service.CreateServiceEntry();

            Assert.AreEqual(service.HandlesType(), entry.Name);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(ChatbotSettingsRead))]
        public void ChatbotSettingsRead_Execute_ReturnsSerializedSettings()
        {
            var result = new ChatbotSettingsRead().Execute(new Dictionary<string, StringBuilder>(), new Mock<IWorkspace>().Object);

            Assert.IsNotNull(result);
            Assert.IsNotNull(new Dev2JsonSerializer().Deserialize<ChatbotSettingsTo>(result));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_HandlesType_ReturnsClassName()
        {
            Assert.AreEqual("PersistenceSettingsRead", new PersistenceSettingsRead().HandlesType());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_CreateServiceEntry_NamedAfterHandlesType()
        {
            var service = new PersistenceSettingsRead();

            var entry = service.CreateServiceEntry();

            Assert.AreEqual(service.HandlesType(), entry.Name);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(PersistenceSettingsRead))]
        public void PersistenceSettingsRead_Execute_ReturnsSerializedSettings()
        {
            var result = new PersistenceSettingsRead().Execute(new Dictionary<string, StringBuilder>(), new Mock<IWorkspace>().Object);

            Assert.IsNotNull(result);
            Assert.IsNotNull(new Dev2JsonSerializer().Deserialize<PersistenceSettingsTo>(result));
        }
    }
}
