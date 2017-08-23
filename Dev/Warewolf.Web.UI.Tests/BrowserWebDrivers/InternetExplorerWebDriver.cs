using OpenQA.Selenium.IE;

namespace Warewolf.Web.UI.Tests.BrowserWebDrivers
{
    public class InternetExplorerWebDriver : BaseWebDriver
    {
        public InternetExplorerWebDriver() : base(new InternetExplorerDriver())
        {
            
        }
    }
}
