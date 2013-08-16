using System;
using System.Activities.Statements;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Views;

namespace Dev2.Core.Tests.Views.Global
{
    [TestClass]
    public class ToolboxUserControlTests
    {
        [TestMethod]
        [TestCategory("ToolboxUserControl_BuildToolbox")]
        [Description("ToolboxUserControl 'Control Flow' category must contain FlowDecision and FlowSwitch types.")]
        [Owner("Trevor Williams-Ros")]
        public void ToolboxUserControl_UnitTest_FlowCategory_ContainsFlowDecisionAndFlowSwitch()
        {
            AssertCategoryContainsTypes("Control Flow", typeof(FlowDecision), typeof(FlowSwitch<string>));
        }

        static void AssertCategoryContainsTypes(string categoryName, params Type[] toolTypes)
        {
            var toolbox = new ToolboxUserControl();

            var category = toolbox.tools.Categories.FirstOrDefault(c => c.CategoryName.Equals(categoryName, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(category);

            foreach(var toolType in toolTypes)
            {
                var itemWrapper = category.Tools.FirstOrDefault(t => t.Type == toolType);
                Assert.IsNotNull(itemWrapper);
            }
        }
    }
}
