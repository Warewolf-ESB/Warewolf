/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ComPluginServiceDefinitionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ComPluginServiceDefinition))]
        public void ComPluginServiceDefinition_Validate_Defaults()
        {
            const string expectedName = "name";
            var expectedGuid = Guid.NewGuid();
            var mockComPluginSource = new Mock<IComPluginSource>();

            var mockServiceInput = new Mock<IServiceInput>();
            var expectedInputs = new List<IServiceInput> { mockServiceInput.Object };

            var mockServiceOutputMapping = new Mock<IServiceOutputMapping>();
            var expectedOutputMappings = new List<IServiceOutputMapping> { mockServiceOutputMapping.Object };

            const string expectedPath = "path";

            var mockPluginAction = new Mock<IPluginAction>();

            var comPluginServiceDefinition = new ComPluginServiceDefinition
            {
                Name = expectedName,
                Id = expectedGuid,
                Source = mockComPluginSource.Object,
                Inputs = expectedInputs,
                OutputMappings = expectedOutputMappings,
                Path = expectedPath,
                Action = mockPluginAction.Object
            };

            Assert.AreEqual(expectedName, comPluginServiceDefinition.Name);
            Assert.AreEqual(expectedGuid, comPluginServiceDefinition.Id);
            Assert.AreEqual(mockComPluginSource.Object, comPluginServiceDefinition.Source);
            Assert.AreEqual(expectedInputs, comPluginServiceDefinition.Inputs);
            Assert.AreEqual(1, comPluginServiceDefinition.Inputs.Count);
            Assert.AreEqual(expectedOutputMappings, comPluginServiceDefinition.OutputMappings);
            Assert.AreEqual(1, comPluginServiceDefinition.OutputMappings.Count);
            Assert.AreEqual(expectedPath, comPluginServiceDefinition.Path);
            Assert.AreEqual(mockPluginAction.Object, comPluginServiceDefinition.Action);
        }
    }
}
