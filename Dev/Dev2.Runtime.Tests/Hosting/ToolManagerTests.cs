using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dev2.ToolBox;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    public class ToolManagerTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerToolRepository_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void ServerToolRepository_Ctor_Nulls_ExpectExceptions()

        {
            //------------Setup for test--------------------------
            var serverToolRepository = new ServerToolRepository(null, new string[0]);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerToolRepository_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerToolRepository_Ctor_Nulls_SearchPaths_ExpectExceptions()
        {
            //------------Setup for test--------------------------
            var serverToolRepository = new ServerToolRepository(new string[0], null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerToolRepository_Ctor")]
        public void ServerToolRepository_Ctor_Valid_expect_Empty()
        {
            //------------Setup for test--------------------------
            var serverToolRepository = new ServerToolRepository(new string[0], new string[0]);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerToolRepository_Load")]
        public void ServerToolRepository_Load_ExpectValid()
        {
            //------------Setup for test--------------------------
            var serverToolRepository = new ServerToolRepository(new[]{ AppDomain.CurrentDomain.BaseDirectory + "\\Dev2.Activities.dll" }, new string[0]);

            //------------Execute Test---------------------------
            var tools = serverToolRepository.LoadTools();

            //------------Assert Results-------------------------
            Assert.IsTrue(tools.Any(a=>a.Name == "Case Convert"));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerToolRepository_Load")]
        public void ServerToolRepository_Load_Folder_ExpectValid()
        {
            //------------Setup for test--------------------------

            string dll = AppDomain.CurrentDomain.BaseDirectory + "\\Dev2.Activities.dll";
            System.IO.Directory.CreateDirectory("./plugins");
            File.Copy(dll, AppDomain.CurrentDomain.BaseDirectory + "\\plugins" + "\\Dev2.Activities.dll");
            var serverToolRepository = new ServerToolRepository(new string[] {  }, new [] { AppDomain.CurrentDomain.BaseDirectory + "\\plugins\\" });

            //------------Execute Test---------------------------
            var tools = serverToolRepository.LoadTools();

            //------------Assert Results-------------------------
            Assert.IsTrue(tools.Any(a => a.Name == "Case Convert"));
        }
        // ReSharper restore InconsistentNaming
    }
}