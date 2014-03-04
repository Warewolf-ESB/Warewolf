using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebActivityTests
    {
        [TestMethod]
        public void Given_WebActivityBackedByAModelItem_ExpectedTheServiceNamePropertySetsTheServiceNamePropertyOnTheModelItem()
        {
            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "testResource";
            Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
            Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);

            string serviceName = "cake";

            IWebActivity webActivity = WebActivityFactory.CreateWebActivity(testItem, null, serviceName);

            Assert.AreEqual(serviceName, webActivity.ServiceName);
            Assert.AreEqual("DsfActivity", ModelItemUtils.GetProperty("DisplayName", testItem));
        }
    }
}
