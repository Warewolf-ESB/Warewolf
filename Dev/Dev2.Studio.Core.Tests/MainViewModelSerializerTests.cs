using System;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Core.Tests
{
    [TestClass]
    public class MainViewModelSerializerTests
    {
        [TestMethod]
        public void OnConstruction_ShouldNotThrowException()
        {

            try
            {
                var mvm = new Mock<IMainViewModel>();
                var s = new MainViewModelSerializer(mvm.Object);
                Assert.AreEqual(@"C:\ProgramData\Warewolf\Workspaces\Cache\00000000-0000-0000-0000-000000000000.xml", s.VmPath);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        public void OnSave_GivenMainViewModel_shouldEncryptAndSave()
        {


            // public MainViewModelSerializer(ISerializer serializer, IFile fileWraper)
            var mvm = new Mock<IMainViewModel>();
            var serializer = new Mock<ISerializer>();
            var file = new Mock<IFile>();
            var s = new MainViewModelSerializer(serializer.Object, file.Object, mvm.Object);

            //Test
            var saved = s.Save();
            Assert.IsTrue(saved);
        }


    }
}
