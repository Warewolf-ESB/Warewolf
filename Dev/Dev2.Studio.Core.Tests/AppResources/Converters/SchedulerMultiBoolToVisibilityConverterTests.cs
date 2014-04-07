using System.Windows;
using Dev2.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
    public class SchedulerMultiBoolToVisibilityConverterTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerMultiBoolToVisibilityConverter_Convert")]
        public void SchedulerMultiBoolToVisibilityConverter_Convert_WithTrueFalse_ReturnsCollapsed()
        {
            SchedulerMultiBoolToVisibilityConverter schedulerMultiBoolToVisibilityConverter = new SchedulerMultiBoolToVisibilityConverter();
            object[] values = { true, false };
            var actual = schedulerMultiBoolToVisibilityConverter.Convert(values, null, null, null);

            Assert.AreEqual(Visibility.Collapsed, actual);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("SchedulerMultiBoolToVisibilityConverter_Convert")]
        public void SchedulerMultiBoolToVisibilityConverter_Convert_WithFalseFalse_ReturnsVisible()
        {
            SchedulerMultiBoolToVisibilityConverter schedulerMultiBoolToVisibilityConverter = new SchedulerMultiBoolToVisibilityConverter();
            object[] values = { false, false };
            var actual = schedulerMultiBoolToVisibilityConverter.Convert(values, null, null, null);

            Assert.AreEqual(Visibility.Visible, actual);
        }
    }
}
