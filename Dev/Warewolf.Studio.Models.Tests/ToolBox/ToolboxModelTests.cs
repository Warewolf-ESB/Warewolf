using System;
using System.Collections.Generic;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Studio.Models.Toolbox;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.Models.Tests.ToolBox
{
    [TestClass]
    public class ToolboxModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ToolboxModel_Ctor_NullParams_ExpectErrors()

        {
            //------------Setup for test--------------------------
           
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IServer>().Object, new Mock<IServer>().Object, new Mock<IPluginProxy>().Object }, typeof(ToolboxModel));
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Ctor")]
        public void ToolboxModel_Ctor_ValidParams_ExpectPropertiesSet()
        {
            //------------Setup for test--------------------------

            var server =  new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);

            //------------Assert Results-------------------------
            PrivateObject p = new PrivateObject(model);
            Assert.AreEqual(server.Object, model.Server);
            Assert.AreEqual(localserver.Object, p.GetField("_localServer"));
            Assert.AreEqual(pluginProxy.Object, p.GetField("_pluginProxy"));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Load")]
        public void ToolboxModel_Load_ValidParams_GetTools()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            var tools = model.GetTools();
            //------------Assert Results-------------------------
            Assert.AreEqual(servertools,tools);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Load")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolboxModel_Load_NullParam_ExpectException()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            var tools = model.IsToolSupported(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(servertools, tools);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Supported")]
        public void ToolboxModel_Supported_ValueExists_ExpectTrue()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            var supported = model.IsToolSupported(servertools[1]);
            //------------Assert Results-------------------------
            Assert.IsTrue(supported);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Supported")]
        public void ToolboxModel_Supported_ValueDoesExists_ExpectFalse()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            var supported = model.IsToolSupported(new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "name", "", new Version(1, 2, 3), false, "cate", ToolType.User, ""));
            //------------Assert Results-------------------------
            Assert.IsFalse(supported);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Supported")]
        public void ToolboxModel_Supported_ValueDoesExistsDifferentVersion_ExpectFalse()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            var supported = model.IsToolSupported(new ToolDescriptor(servertools[0].Id, new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "name", "", new Version(1, 2, 4), false, "cate", ToolType.User, ""));
            //------------Assert Results-------------------------
            Assert.IsFalse(supported);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_LoadLoadTool")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolboxModel_LoadTool_Null_ExpectException()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            model.LoadTool(null,new byte[0]);

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_LoadLoadTool")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolboxModel_LoadTool_NullDll_ExpectException()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            model.LoadTool(servertools[0], null);
            server.Verify(a=>a.ReloadTools());


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_LoadLoadTool")]
        public void ToolboxModel_LoadTool_ValidArgs_ExpectpassThrough()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            model.LoadTool(servertools[0], new byte[0]);
            server.Verify(a => a.ReloadTools());
            pluginProxy.Verify(a => a.LoadTool(servertools[0],It.IsAny<byte[]>()));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_DeleteTool")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolboxModel_DeletTool_NullDescriptor_ExpectException()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            model.DeleteTool( null);
      

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_DeleteTool")]
        public void ToolboxModel_DeletTool_ValidDescriptor_ExpectPassThrough()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            model.DeleteTool(servertools[0]);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Filter")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolboxModel_Filter_NullString_ExpectException()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            model.Filter(null);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Filter")]
        public void ToolboxModel_Filter_EmptyString_ExpectAllvalues()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"dar","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            Assert.AreEqual(2,model.Filter("").Count);


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Filter")]
        public void ToolboxModel_Filter_String_ExpectFilteredValues()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"dar","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            Assert.AreEqual(1, model.Filter("nam").Count);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Connected")]
        public void ToolboxModel_Connected_LocalNotConnected_ExpectFalse()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            server.Setup(a => a.IsConnected()).Returns(true);
            var localserver = new Mock<IServer>();
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"dar","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            Assert.IsFalse(model.IsEnabled());


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Connected")]
        public void ToolboxModel_Connected_RemoteNotConnected_ExpectFalse()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            server.Setup(a => a.IsConnected()).Returns(false);
            var localserver = new Mock<IServer>();
            localserver.Setup(a => a.IsConnected()).Returns(true);
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"dar","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            Assert.IsFalse(model.IsEnabled());


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolboxModel_Connected")]
        public void ToolboxModel_Connected_RemoteAndLocalConnected_ExpectTrue()
        {
            //------------Setup for test--------------------------

            var server = new Mock<IServer>();
            server.Setup(a => a.IsConnected()).Returns(true);
            var localserver = new Mock<IServer>();
            localserver.Setup(a => a.IsConnected()).Returns(true);
            var pluginProxy = new Mock<IPluginProxy>();
            IList<IToolDescriptor> servertools = new IToolDescriptor[]
            {
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"name","",new Version(1,2,3),false,"cate",ToolType.User, ""), 
                new ToolDescriptor(Guid.NewGuid(),new WarewolfType("bob",new Version(),"dave" ),new WarewolfType("bob",new Version(),"dave" ),"dar","",new Version(1,2,3),false,"cate",ToolType.User, "")
            };
            server.Setup(a => a.LoadTools()).Returns(servertools);
            localserver.Setup(a => a.LoadTools()).Returns(servertools);
            //------------Execute Test---------------------------
            var model = new ToolboxModel(server.Object, localserver.Object, pluginProxy.Object);
            Assert.IsTrue(model.IsEnabled());


        }
        // ReSharper restore InconsistentNaming
    }
}
