
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebActivity_IsNotAvailable")]
        public void WebActivity_MethodName_ResourcesLoaded_ExpectTrue()
        {
            WebActivity_IsConnected_PassThrough(true, false, true);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebActivity_IsNotAvailable")]
        public void WebActivity_MethodName_NotLocalHost_false()
        {
            WebActivity_IsConnected_PassThrough(false, true, false);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebActivity_IsNotAvailable")]
        public void WebActivity_MethodName_Available_ExpectFalse()
        {
            WebActivity_IsConnected_PassThrough(true, true, false);
        }

        public void WebActivity_IsConnected_PassThrough(bool retConnected, bool retLoaded, bool expected)
        {
            DsfActivity Act = new DsfActivity();
            Act.ServiceName = "testResource";
            Act.InputMapping = @"<Inputs><Input Name=""Host"" Source=""Host"" DefaultValue=""mail.bellevuenet.co.za""><Validator Type=""Required"" /></Input><Input Name=""Port"" Source=""Port"" DefaultValue=""25""><Validator Type=""Required"" /></Input><Input Name=""From"" Source=""From"" DefaultValue=""DynamicServiceFramework@theunlimited.co.za""><Validator Type=""Required"" /></Input><Input Name=""To"" Source=""To""><Validator Type=""Required"" /></Input><Input Name=""Subject"" Source=""Subject""><Validator Type=""Required"" /></Input><Input Name=""BodyType"" Source=""Bodytype"" DefaultValue=""html""><Validator Type=""Required"" /></Input><Input Name=""Body"" Source=""Body""><Validator Type=""Required"" /></Input><Input Name=""Attachment"" Source=""Attachment"" DefaultValue=""NONE""><Validator Type=""Required"" /></Input></Inputs>";
            Act.OutputMapping = @"<Outputs><Output Name=""FailureMessage"" MapsTo=""FailureMessage"" Value=""[[FailureMessage]]"" /><Output Name=""Message"" MapsTo=""Message"" Value=""[[Message]]"" /></Outputs>";
            ModelItem testItem = TestModelItemFactory.CreateModelItem(Act);
            var resource = new Mock<IContextualResourceModel>();
            var env = new Mock<IEnvironmentModel>();
            resource.Setup(a => a.Environment).Returns(env.Object);
            env.Setup(a => a.IsConnected).Returns(retConnected);
            env.Setup(a => a.HasLoadedResources).Returns(retLoaded);
            string serviceName = "cake";

            IWebActivity webActivity = WebActivityFactory.CreateWebActivity(testItem, resource.Object, serviceName);
            Assert.AreEqual(expected,webActivity.IsNotAvailable());
        }
    }
}
