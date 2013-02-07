using Dev2.Composition;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.DataList;
using Dev2.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;

namespace Dev2.Core.Tests.Custom_Dev2_Controls.Intellisense
{
    [TestClass]
    public class IntellisenseTextBoxTests
    {
        private IResourceModel _resourceModel;
        private static readonly object TestGuard = new object();

        #region Test Initialization

        [TestInitialize]
        public void Init()
        {
            Monitor.Enter(TestGuard);

            ImportService.CurrentContext = CompositionInitializer.InitializeForMeflessBaseViewModel();

            var testEnvironmentModel = new Mock<IEnvironmentModel>();
            testEnvironmentModel.Setup(model => model.DsfChannel.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");

            _resourceModel = new ResourceModel(testEnvironmentModel.Object)
            {
                ResourceName = "test",
                ResourceType = ResourceType.Service,
                DataList = @"
            <DataList>
                    <Scalar/>
                    <Country/>
                    <State />
                    <City>
                        <Name/>
                        <GeoLocation />
                    </City>
             </DataList>
            "
            };

            IDataListViewModel setupDatalist = new DataListViewModel();
            DataListSingleton.SetDataList(setupDatalist);
            DataListSingleton.ActiveDataList.InitializeDataListViewModel(_resourceModel);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Monitor.Exit(TestGuard);
        }

        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenGettingResultsGivenAProviderThatThrowsAnException()
        {
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>())).Throws(new Exception());

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.IntellisenseProvider = intellisenseProvider.Object;
            textBox.Text = "[[City([[Scalar]]).Na";

            //There is no assert in this test because the desired result is that an exception isn't thrown
        }

        //BUG 8761
        [TestMethod]
        public void IntellisenseBoxDoesntCrashWhenInsertingResultsGivenAProviderThatThrowsAnException()
        {
            Mock<IIntellisenseProvider> intellisenseProvider = new Mock<IIntellisenseProvider>();
            intellisenseProvider.Setup(a => a.PerformResultInsertion(It.IsAny<string>(), It.IsAny<IntellisenseProviderContext>())).Throws(new Exception());
            intellisenseProvider.Setup(a => a.HandlesResultInsertion).Returns(true);

            IntellisenseProviderResult intellisenseProviderResult = new IntellisenseProviderResult(intellisenseProvider.Object, "City", "cake");

            IntellisenseTextBox textBox = new IntellisenseTextBox();
            textBox.CreateVisualTree();
            textBox.InsertItem(intellisenseProviderResult, true);
            
            //There is no assert in this test because the desired result is that an exception isn't thrown
        }
        
        #endregion Test Initialization
    }
}
