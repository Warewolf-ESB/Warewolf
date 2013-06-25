using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Unlimited.Framework;
using Dev2.Studio.Core.Interfaces;
using Moq;
using System.Collections.ObjectModel;
using System.Reflection;
using Dev2.Studio.Core.Factories;
using System.ServiceModel;
using Dev2.Studio.Core;
using Dev2.Studio.Core.ViewModels;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Core.Tests
{


    /// <summary>
    /// Summary description for WebsiteEditorViewModelTest
    /// </summary>
    [TestClass]
    public class WebsiteEditorViewModelTest
    {

        #region Ctor and Attributes

        private TestContext testContextInstance;

        private readonly Mock<IContextualResourceModel> _res = new Mock<IContextualResourceModel>();
        private Mock<IWebActivity> _test;
        private Mock<IResourceRepository> _repo = new Mock<IResourceRepository>();
        private static ImportServiceContext _importServiceContext;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion

        #region Test Init

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _importServiceContext = CompositionInitializer.InitializeMockedMainViewModel();
        }

        [TestInitialize()]
        public void EnvironmentTestsInitialize()
        {
            ImportService.CurrentContext = _importServiceContext;

            //res = ResourceModelFactory.CreateResourceModel(enResourceType.HumanInterfaceProcess, "Test", "Test");
            _res.Setup(c => c.ResourceType).Returns(ResourceType.HumanInterfaceProcess);
            _res.Setup(c => c.ResourceName).Returns("result");
            _res.Setup(c => c.Category).Returns("result");
            _res.Setup(c => c.Environment).Returns(Dev2MockFactory.SetupEnvironmentModel().Object);

            ICollection<IResourceModel> all = new Collection<IResourceModel>();
            //all.Add(res);
            all.Add(_res.Object);

            _repo = new Mock<IResourceRepository>();
            _repo.Setup(c => c.Save(_res.Object));
            _repo.Setup(c => c.All()).Returns(all);



            _test = new Mock<IWebActivity>();

            _test.Setup(c => c.XMLConfiguration).Returns(TestResourceStrings.serviceDefinition);
            _test.Setup(c => c.Html).Returns(TestResourceStrings.websiteEditorValid);
            _test.Setup(c => c.WebsiteServiceName).Returns("Test");
            _test.Setup(c => c.MetaTags).Returns("greatness, success");
            _test.Setup(c => c.ResourceModel).Returns(_res.Object);

        }

        #endregion

        #region Helper Method

        private IWebsiteEditorViewModel FetchTestingViewModel()
        {

            _test.Setup(moq => moq.XMLConfiguration).Returns(TestResourceStrings.WebsiteEditor_Test_Config);
            IWebsiteEditorViewModel viewModel = new WebsiteEditorViewModel(_test.Object);            

            return viewModel;
        }

        private IWebsiteEditorViewModel FetchTestingViewModelWithInvalidConfig()
        {

            _test.Setup(moq => moq.XMLConfiguration).Returns(TestResourceStrings.WebsiteEditor_Test_InvalidXmlConfig);
            IWebsiteEditorViewModel viewModel = new WebsiteEditorViewModel(_test.Object);

            return viewModel;
        }

        #endregion

        #region Constructor Test Cases

        // TO DO: Update Test Methods
        //[TestMethod]
        //public void ConstructorWithNullModelItem() {
        //    try {
        //        IWebsiteEditorViewModel viewModel = new WebsiteEditorViewModel(null);
        //    }catch (ArgumentNullException e) {
        //        Assert.AreEqual("webActivity", e.ParamName);
        //    }
        //}

        //[TestMethod]
        //public void ConstructorWithAllValid() {
        //    IWebsiteEditorViewModel viewModel = FetchTestingViewModel();

        //    Assert.IsNotNull(viewModel);
        //}

        #endregion

        #region Property Test Cases

        // TO DO: Update Test Method
        //[TestMethod]
        //public void RowsProperty() {
        //    IWebsiteEditorViewModel viewModel = FetchTestingViewModel();

        //    Assert.IsTrue(viewModel.Rows == 2);
        //}

        //TO DO: Update Test Method
        //[TestMethod]
        //public void LayoutObjectsProperty() {
        //    IWebsiteEditorViewModel viewModel = FetchTestingViewModel();

        //    Assert.IsNotNull(viewModel.LayoutObjects);
        //}

        #endregion

        #region Method Test

        [TestMethod]
        public void Creation_Expected_XmlConfig_Is_Right()
        {
            IWebsiteEditorViewModel viewModel = FetchTestingViewModel();
            viewModel.Html = TestResourceStrings.WebsiteEditor_Test_HTML;
            viewModel.UpdateModelItem();

            string result = viewModel.LayoutObjects.First(c=>c.WebpartServiceDisplayName == "companyname").XmlConfiguration;
            result = result.Trim();
            Assert.AreEqual(TestResourceStrings.WebsiteEditor_Test_Result.Trim(), result);
        }

        [TestMethod]
        public void CreationWithWrongWebpartName_Expected_Blank_XmlConfig()
        {
            IWebsiteEditorViewModel viewModel = FetchTestingViewModelWithInvalidConfig();
            viewModel.Html = TestResourceStrings.WebsiteEditor_Test_HTML;
            viewModel.UpdateModelItem();

            string result = viewModel.LayoutObjects.First(c => c.WebpartServiceDisplayName == "companyname").XmlConfiguration;
            result = result.Trim();
            Assert.AreEqual(string.Empty, result);
        } 

        [TestMethod]
        public void SetSelected()
        {
            IWebsiteEditorViewModel viewModel = FetchTestingViewModel();
            Mock<ILayoutObjectViewModel> selected = new Mock<ILayoutObjectViewModel>();

            viewModel.SetSelected(selected.Object);

            Assert.IsNotNull(viewModel.SelectedLayoutObject);

        }

        /// <summary>
        ///A test for CheckForDuplicateNames
        ///</summary>
        [TestMethod()]
        public void CheckForDuplicateNamesTest()
        {
            WebsiteEditorViewModel target = new WebsiteEditorViewModel(_test.Object);
            string xmlConfig = StringResourcesTest.WebPartWizards_DuplicateNameCheck;
            string testName = "Dev2elementNameButton";
            string expected = "True,This name already exists";
            string actual;
            actual = target.CheckForDuplicateNames(xmlConfig, testName);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DefaultWebpageIsSpecificNoneResourceModel()
        {
            WebsiteEditorViewModel target = new WebsiteEditorViewModel(_test.Object);
            IResourceModel defaultWebpage = target.SelectedDefaultWebpage;
            Assert.IsTrue(defaultWebpage.ResourceName == "None");
        }

        #endregion

    }
}
