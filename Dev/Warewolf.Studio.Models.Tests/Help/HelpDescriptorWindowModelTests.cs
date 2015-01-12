using System;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Models.Help;

namespace Warewolf.Studio.Models.Tests.Help
{
    [TestClass]
    public class HelpDescriptorWindowModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptorWindowModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void HelpDescriptorWindowModel_Ctor_NullArgs_ExpectExceptions()

        {
            //------------Setup for test--------------------------
            // ReSharper disable ObjectCreationAsStatement
            new HelpModel(null);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptorWindowModel_Ctor")]
        public void HelpDescriptorWindowModel_Ctor_ValidArgs_ExpectCorrectIntance()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            agg.Setup(a => a.GetEvent<HelpChangedEvent>()).Returns(new HelpChangedEvent());
            // ReSharper disable ObjectCreationAsStatement
            new HelpModel(agg.Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Execute Test---------------------------
            agg.Verify(a=>a.GetEvent<HelpChangedEvent>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptorWindowModel_Ctor")]
        public void HelpDescriptorWindowModel_Fire_Events_ExpectThatEventIsFired()
        {
            //------------Setup for test--------------------------
            var agg = new Mock<IEventAggregator>();
            agg.Setup(a => a.GetEvent<HelpChangedEvent>()).Returns(new HelpChangedEvent());
            var helpModel = new HelpModel(agg.Object);
            IHelpDescriptor desc = null;
            PrivateObject pvt = new PrivateObject(helpModel);
            helpModel.OnHelpTextReceived += ((a, b) => { desc = b; });
            //------------Execute Test---------------------------
            pvt.Invoke("FireOnHelpReceived", new object[] { new HelpDescriptor("bob", "the", new DrawingImage()) });
            //------------Assert Results-------------------------
            Assert.IsNotNull(desc);
            Assert.AreEqual("bob",desc.Name);
            Assert.AreEqual("the",desc.Description);
        }



        // ReSharper restore InconsistentNaming
    }
}
