
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime.Configuration.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dev2.Runtime.Configuration.Tests.ComponentModel
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SettingsObjectTests
    {
        [TestMethod]
        public void BuildGraph()
        {
            // Create object graph
            MockSettingsObjectA settingsObjectA = new MockSettingsObjectA
            {
                SettingsB = new MockSettingsObjectB(),
                SettingsC = new MockSettingsObjectC
                {
                    SettingsA = new MockSettingsObjectA()
                },
            };

            List<SettingsObject> settingsGraph = SettingsObject.BuildGraph(settingsObjectA);

            Assert.AreEqual(settingsObjectA.SettingsB, settingsGraph[0].Object, "Properties at root level aren't being found.");
            Assert.AreEqual(settingsObjectA.SettingsC, settingsGraph[1].Object, "Properties at root level aren't being found.");
            Assert.AreEqual(settingsObjectA.SettingsC.SettingsA, settingsGraph[1].Children[0].Object, "Properties aren't being found in recursive call.");

            Assert.AreEqual(0, settingsGraph[0].Children.Count, "settingsObjectA.SettingsB shouldn't have any children since it's properties are null.");
            Assert.AreEqual(1, settingsGraph[1].Children.Count, "settingsObjectA.SettingsC should only have one child.");
            Assert.AreEqual(0, settingsGraph[1].Children[0].Children.Count, "settingsObjectA.SettingsC.SettingsA shouldn't have any children since it's properties are null.");

            Assert.AreEqual(2, settingsGraph.Count, "Two root nodes were expected, one for settingsObjectA.SettingsB and one for settingsObjectA.SettingsC.");
        }

        [TestMethod]
        public void BuildGraphWhereObjectsContainCircularReferencesExpectedCircularReferencesAreIgnored()
        {
            // Create object graph
            MockSettingsObjectA settingsObjectA = new MockSettingsObjectA
            {
                SettingsC = new MockSettingsObjectC(),
            };

            settingsObjectA.SettingsC.SettingsA = settingsObjectA;

            List<SettingsObject> settingsGraph = SettingsObject.BuildGraph(settingsObjectA);

            Assert.AreEqual(settingsObjectA.SettingsC, settingsGraph[0].Object, "Properties at root level aren't being found.");
            Assert.AreEqual(0, settingsGraph[0].Children.Count, "settingsObjectA.SettingsC should have no children because this would be a circular reference.");
            Assert.AreEqual(1, settingsGraph.Count, "One root node was expected for settingsObjectA.SettingsC.");
        }
    }
}
