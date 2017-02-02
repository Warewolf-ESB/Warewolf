/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities
{
    [TestClass]
    public class WorkflowHelperTests
    {

        [TestInitialize]
        public void Init()
        {
            GlobalConstants.Resultscache.Clear();
        }

        #region Expected Namespaces/Assemblies

        static readonly List<string> ExpectedNamespaces = new List<string>
        {
            "Dev2.Common",
            "Dev2.Data.Decisions.Operations",
            "Dev2.Data.SystemTemplates.Models",
            "Dev2.DataList.Contract",
            "Dev2.DataList.Contract.Binary_Objects",
            "Unlimited.Applications.BusinessDesignStudio.Activities"
        };

        static readonly List<string> ExpectedAssemblies = new List<string>
        {
            "Dev2.Common",
            "Dev2.Data",
            "Dev2.Activities"
        };

        #endregion

        #region CreateWorkflow

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WorkflowHelperCreateWorkflowWithNullDisplayNameExpectedThrowsArgumentNullException()
        {
            new WorkflowHelper().CreateWorkflow(null);
        }

        [TestMethod]
        public void WorkflowHelperCreateWorkflowWithDisplayNameExpectedReturnsActivityBuilderWithFlowChartImplementation()
        {
            const string DisplayName = "TestResource";

            var result = new WorkflowHelper().CreateWorkflow(DisplayName);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Implementation, typeof(Flowchart));

            var flowChart = (Flowchart)result.Implementation;
            Assert.AreEqual(flowChart.DisplayName, DisplayName);
        }

        #endregion

        #region SerializeWorkflow

        [TestMethod]
        public void WorkflowHelperSerializeWorkflowWithNullModelServiceExpectedReturnsEmptyString()
        {
            var result = new WorkflowHelper().SerializeWorkflow(null);
            Assert.AreEqual(string.Empty, result.ToString());
        }

        [TestMethod]
        public void WorkflowHelperSerializeWorkflowWithModelServiceExpectedReturnsActivityXaml()
        {
            var modelService = CreateModelService();

            var result = new WorkflowHelper().SerializeWorkflow(modelService.Object).ToString();

            Assert.IsFalse(result.Contains("<?xml version=\"1.0\" encoding=\"utf-16\"?>"));

            var root = XElement.Parse(result);

            XNamespace sads = "http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger";
            var debugSymbol = root.Element(sads + "DebugSymbol.Symbol");
            if(debugSymbol != null)
            {
                Assert.IsTrue(string.IsNullOrEmpty(debugSymbol.Value));
            }

//            XNamespace mva = "clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities";
//            var vbSettings = root.Element(mva + "VisualBasic.Settings");
//            Assert.IsNotNull(vbSettings);

            XNamespace a = "http://schemas.microsoft.com/netfx/2009/xaml/activities";
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

            var namespacesForImplementation = root.Element(a + "TextExpression.NamespacesForImplementation");
            Assert.IsNotNull(namespacesForImplementation);

            XNamespace scg = "clr-namespace:System.Collections.Generic;assembly=mscorlib";
            var nsList = namespacesForImplementation.Element(scg + "List");
            Assert.IsNotNull(nsList);

            var actualNamespaces = nsList.Elements(x + "String").Select(e => e.Value).ToList();
            Assert.IsTrue(ExpectedNamespaces.SequenceEqual(actualNamespaces));

            var referencesForImplementation = root.Element(a + "TextExpression.ReferencesForImplementation");
            Assert.IsNotNull(referencesForImplementation);

            XNamespace sco = "clr-namespace:System.Collections.ObjectModel;assembly=mscorlib";
            var asmList = referencesForImplementation.Element(sco + "Collection");
            Assert.IsNotNull(asmList);

            var actualAssemblies = asmList.Elements(a + "AssemblyReference").Select(e => e.Value).ToList();
            Assert.IsTrue(ExpectedAssemblies.SequenceEqual(actualAssemblies));
        }

        #endregion

        #region CreateModelService

        static Mock<ModelService> CreateModelService()
        {
            var root = new Mock<ModelItem>();
            root.Setup(r => r.GetCurrentValue()).Returns(new WorkflowHelper().CreateWorkflow("TestWorkflow"));

            var modelService = new Mock<ModelService>();
            modelService.Setup(s => s.Root).Returns(root.Object);

            return modelService;
        }

        #endregion
    }
}
