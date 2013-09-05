using System.Windows;
using Dev2.Activities.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Activities
{
    [TestClass]
    public class HelpTextAttachedPropertyTest
    {
        [TestMethod]
        [TestCategory("HelpTextAttachedProperty_UnitTest")]
        [Description("HelpTextAttachedProperty can get set value")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void HelpTextAttachedProperty_TextProperty_NewTextValue_CanGetSetValue()
        // ReSharper restore InconsistentNaming
        {
            const string Expected = "Some Text Value";
            var depObj = new Mock<DependencyObject>().Object;

            HelpText.SetText(depObj, Expected);

            Assert.AreEqual(Expected, HelpText.GetText(depObj), "HelpTextAttachedProperty cannot get set value");

        }
    }
}
