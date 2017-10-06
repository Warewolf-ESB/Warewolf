using Dev2.Common.ExtMethods;
using Dev2.Factory;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Integration.Tests.Merge
{

    [TestClass]
    public class MergeFactoryTests
    {
        readonly IServerRepository _server = ServerRepository.Instance;
        [TestInitialize]
        [Owner("Nkosinathi Sangweni")]
        public void Init()
        {
            _server.Source.ResourceRepository.ForceLoad();
            CustomContainer.Register(_server);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OpenMergeWindow_NullShellViewModel()
        {
            //---------------Set up test pack-------------------
            MergeFactory factory = new MergeFactory();
            //---------------Execute Test ----------------------
            factory.OpenMergeWindow(null, "");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OpenMergeWindow_GivenShellView_OpensMergeView()
        {
            //---------------Set up test pack-------------------
            var a = XML.XmlResource.Fetch("SameResourceSequence");
            MergeFactory factory = new MergeFactory();
            var mockVm = new Mock<IShellViewModel>();
            mockVm.Setup(p => p.OpenMergeConflictsView(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()));
            //var path = "Dev2.Integration.Tests.XML.SameResourceSequence.xml";
            var path = "XML\\SameResourceSequence.xml";
            factory.OpenMergeWindow(mockVm.Object, "-merge " + path);
            mockVm.Verify(p => p.OpenMergeConflictsView(It.IsAny<IContextualResourceModel>(), It.IsAny<IContextualResourceModel>(), It.IsAny<bool>()));
        }
    }
}
