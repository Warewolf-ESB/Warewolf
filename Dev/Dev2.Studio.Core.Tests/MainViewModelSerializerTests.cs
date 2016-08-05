using System;
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

        const string vmString = "{\"__interceptors\":[{}],\"__target\":null,\"__interfaces\":[\"Moq.IMocked, Moq, Version=4.2.1510.2205, Culture=neutral, PublicKeyToken=69f491c39445e920\",\"Moq.IMocked`1[[Dev2.Interfaces.IMainViewModel, Dev2.Studio.Core, Version=0.0.6061.24376, Culture=neutral, PublicKeyToken=null]], Moq, Version=4.2.1510.2205, Culture=neutral, PublicKeyToken=69f491c39445e920\"],\"__baseType\":\"Moq.Proxy.InterfaceProxy, Moq, Version=4.2.1510.2205, Culture=neutral, PublicKeyToken=69f491c39445e920\",\"__proxyGenerationOptions\":{\"hook\":{},\"selector\":null,\"mixins\":null,\"baseTypeForInterfaceProxy.AssemblyQualifiedName\":\"Moq.Proxy.InterfaceProxy, Moq, Version=4.2.1510.2205, Culture=neutral, PublicKeyToken=69f491c39445e920\"},\"__proxyTypeId\":\"interface.without.target\",\"__targetFieldType\":\"System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\",\"__theInterface\":\"Dev2.Interfaces.IMainViewModel, Dev2.Studio.Core, Version=0.0.6061.24376, Culture=neutral, PublicKeyToken=null\"}";

        [TestMethod]
        public void OnSave_GivenMainViewModel_shouldAndSave()
        {


            // public MainViewModelSerializer(ISerializer serializer, IFile fileWraper)
            var mvm = new Mock<IMainViewModel>();
            var file = new Mock<IFile>();
            file.Setup(p => p.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
            var s = new MainViewModelSerializer(file.Object, mvm.Object);

            //Test
            var saved = s.Save();
            Assert.IsTrue(saved);
            file.VerifyAll();
        }

        [TestMethod]
        public void OnSave_GivenThrowsException_shouldNotSave()
        {


            // public MainViewModelSerializer(ISerializer serializer, IFile fileWraper)
            var mvm = new Mock<IMainViewModel>();
            var file = new Mock<IFile>();
            file.Setup(p => p.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Error"));
            var s = new MainViewModelSerializer(file.Object, mvm.Object);

            //Test
            var saved = s.Save();
            Assert.IsFalse(saved);
            file.VerifyAll();
        }

        [TestMethod]
        public void OnFetch_GivenViewModelStringData_shouldBuildViewModel()
        {


            // public MainViewModelSerializer(ISerializer serializer, IFile fileWraper)
            var mvm = new Mock<IMainViewModel>();
            var file = new Mock<IFile>();
            file.Setup(p => p.ReadAllText(It.IsAny<string>())).Returns(vmString);
            var s = new MainViewModelSerializer(file.Object, mvm.Object);

            var saved = s.Fetch();
            Assert.IsNotNull(saved);
            file.VerifyAll();
        }

        [TestMethod]
        public void OnFetch_GivenHasAnError_shouldReturnNull()
        {


            // public MainViewModelSerializer(ISerializer serializer, IFile fileWraper)
            var mvm = new Mock<IMainViewModel>();
            var file = new Mock<IFile>();
            file.Setup(p => p.ReadAllText(It.IsAny<string>())).Throws(new Exception("Error"));
            var s = new MainViewModelSerializer(file.Object, mvm.Object);

            var saved = s.Fetch();
            Assert.IsNull(saved);
            file.VerifyAll();
        }


    }
}
