using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Core.Tests.Activities
{
    [TestClass]
    public class ActivityViewModelBaseTest
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void ActivityViewModelBase_HideContent_ModelItemViewIsOfTypeActivityDesignerBase_HideContentIsCalled()
        {
            var mockModelItem = new Mock<ModelItem>();
            var viewMock = new Mock<ActivityDesignerBase>();
            mockModelItem.Setup(p => p.View).Returns(viewMock.Object);
            var testActivityViewModel = new TestActivityViewModelBase(mockModelItem.Object);
            testActivityViewModel.HideContent();
            viewMock.Verify(m => m.HideContent(), Times.Once());
        }
    }
}
