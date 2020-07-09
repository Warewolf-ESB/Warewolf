/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestStepTOTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestStepTO))]
        public void ServiceTestStepTO_Empty_CTOR_ShouldExpectDefaults()
        {
            var sut = new ServiceTestStepTO();

            var result = sut.Type;

            Assert.AreEqual(StepType.Mock, result);
            Assert.IsTrue(sut.MockSelected);
            Assert.IsFalse(sut.AssertSelected);

            result = sut.Type = StepType.Assert;

            Assert.AreEqual(StepType.Assert, result);
            Assert.IsFalse(sut.MockSelected);
            Assert.IsTrue(sut.AssertSelected);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestStepTO))]
        public void ServiceTestStepTO_NonEmpty_CTOR_ShouldExpectSetProperties()
        {
            var sut = new ServiceTestStepTO(Guid.Empty, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert);

            var result = sut.Type;

            Assert.AreEqual(StepType.Assert, result);
            Assert.IsFalse(sut.MockSelected);
            Assert.IsTrue(sut.AssertSelected);

            result = sut.Type = StepType.Mock;

            Assert.AreEqual(StepType.Mock, result);
            Assert.IsTrue(sut.MockSelected);
            Assert.IsFalse(sut.AssertSelected);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestStepTO))]
        [Description("The ActivityID should be the Identifier of the activity this TestStep is in. But UniqueID should identify this TestStep")]
        public void ServiceTestStepTO_NonEmpty_CTOR_When_ActivityID_Is_EmptyGuid_ShouldExpectUniqueID()
        {
            var activityId = Guid.NewGuid();
            var sut = new ServiceTestStepTO(activityId, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
            {
                ActivityID = Guid.Empty
            };

            var result = sut.ActivityID;

            Assert.AreNotEqual(Guid.Empty, result);
            Assert.AreEqual(activityId, result);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServiceTestStepTO))]
        [Description("The ActivityID should be used as UniqueID identifier for TestStep, until this change is adopted across the system")]
        public void ServiceTestStepTO_NonEmpty_CTOR_When_ActivityID_Is_NotEmptyGuid_ShouldExpectSetActivityID()
        {
            var activityId = Guid.NewGuid();
            var sut = new ServiceTestStepTO(activityId, "Activity", new ObservableCollection<IServiceTestOutput>(), StepType.Assert);

            var result = sut.ActivityID;

            Assert.AreNotEqual(Guid.Empty, result);
            Assert.AreEqual(activityId, result);
        }
    }
}
