/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities;
using Dev2.Studio.Core.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.AppResources.Converters
{
    [TestClass]
	[TestCategory("Studio Resources Core")]
    public class ModelItemToActivityConverterTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ModelItemToActivityConverter))]
        public void ModelItemToActivityConverter_Convert()
        {
            var converter = new ModelItemToActivityConverter();
            var databaseActivity = new DsfDatabaseActivity();
            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(o => o.GetCurrentValue()).Returns(databaseActivity);

            var actual = converter.Convert(mockModelItem.Object, null, null, null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ModelItemToActivityConverter))]
        public void ModelItemToActivityConverter_ConvertBack_Null()
        {
            var converter = new ModelItemToActivityConverter();
            var actual = converter.ConvertBack(null, null, null, null);
            Assert.IsNull(actual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ModelItemToActivityConverter))]
        public void ModelItemToActivityConverter_ConvertBack_DatabaseActivity()
        {
            var converter = new ModelItemToActivityConverter();

            var mockModelItem = new Mock<ModelItem>();
            var databaseActivity = new DsfDatabaseActivity();
            mockModelItem.Setup(o => o.GetCurrentValue()).Returns(databaseActivity);
            var actual = converter.ConvertBack(mockModelItem.Object, null, null, null);
            Assert.AreEqual(databaseActivity, actual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ModelItemToActivityConverter))]
        public void ModelItemToActivityConverter_ConvertBack_PluginActivity()
        {
            var converter = new ModelItemToActivityConverter();

            var mockModelItem = new Mock<ModelItem>();
            var pluginActivity = new DsfPluginActivity();
            mockModelItem.Setup(o => o.GetCurrentValue()).Returns(pluginActivity);
            var actual = converter.ConvertBack(mockModelItem.Object, null, null, null);
            Assert.AreEqual(pluginActivity, actual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ModelItemToActivityConverter))]
        public void ModelItemToActivityConverter_ConvertBack_Activity()
        {
            var converter = new ModelItemToActivityConverter();

            var mockModelItem = new Mock<ModelItem>();
            var activity = new DsfActivity();
            mockModelItem.Setup(o => o.GetCurrentValue()).Returns(activity);
            var actual = converter.ConvertBack(mockModelItem.Object, null, null, null);
            Assert.AreEqual(activity, actual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ModelItemToActivityConverter))]
        public void ModelItemToActivityConverter_ConvertBack_SequenceActivity_Parameter_Resume()
        {
            var converter = new ModelItemToActivityConverter();

            var actual = converter.ConvertBack(null, null, "Resume", null);
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(DsfSequenceActivity), actual.GetType());
        }
    }
}
