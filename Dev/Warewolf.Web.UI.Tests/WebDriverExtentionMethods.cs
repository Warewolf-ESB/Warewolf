using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace Warewolf.Web.Tests
{
    public static class WebDriverExtentionMethods
    {
        public static void JavascriptScrollIntoView(this IWebElement Element, IWebDriver driver, bool scrollDown = true)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(" + (scrollDown ? "true" : "false") + ");", Element);
            Assert.IsTrue(Element.Displayed, "Failed to scroll " + Element.TagName + " into view.");
        }
    }
}
