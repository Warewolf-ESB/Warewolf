using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Warewolf.Core;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class ActionInputDatalistMapperTests
    {
        ActionInputDatalistMapper _actionInputDatalistMapper;

        [TestInitialize]
        public void Initialize()
        {
            IDataListViewModel setupDatalist = new DataListViewModel();
            var mockActiveDataList = new Mock<IActiveDataList>();
            mockActiveDataList.Setup(a => a.ActiveDataList).Returns(setupDatalist);
            _actionInputDatalistMapper = new ActionInputDatalistMapper(mockActiveDataList.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ActionInputDatalistMapper))]
        public void ActionInputDatalistMapper_MapInputsToDatalist_Should_MapToScalar()
        {
            var serviceInputs = new List<IServiceInput>();
            var serviceInput = new ServiceInput("[[a]]", "");
            serviceInputs.Add(serviceInput);

            Assert.AreEqual("", serviceInputs[0].Value);

            _actionInputDatalistMapper.MapInputsToDatalist(serviceInputs);
            Assert.AreEqual("[[a]]", serviceInputs[0].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ActionInputDatalistMapper))]
        public void ActionInputDatalistMapper_MapInputsToDatalist_Should_MapToObject()
        {
            var serviceInputs = new List<IServiceInput>();
            var serviceInput = new ServiceInput("[[a]]", "")
            {
                IsObject = true
            };

            serviceInputs.Add(serviceInput);

            Assert.AreEqual("", serviceInputs[0].Value);

            _actionInputDatalistMapper.MapInputsToDatalist(serviceInputs);

            Assert.AreEqual("@[[a]]", serviceInputs[0].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ActionInputDatalistMapper))]
        public void ActionInputDatalistMapper_MapInputsToDatalist_IsNullOrEmpty_Should_Continue()
        {
            var serviceInputs = new List<IServiceInput>();
            var serviceInput = new ServiceInput("[[a]]", "");
            var serviceInputWithValue = new ServiceInput("[[b]]", "asdf");
            serviceInputs.Add(serviceInput);
            serviceInputs.Add(serviceInputWithValue);

            Assert.AreEqual("", serviceInputs[0].Value);
            Assert.AreEqual("asdf", serviceInputs[1].Value);

            _actionInputDatalistMapper.MapInputsToDatalist(serviceInputs);
            Assert.AreEqual("[[a]]", serviceInputs[0].Value);
            Assert.AreEqual("asdf", serviceInputs[1].Value);
        }
    }
}
