/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;

namespace Dev2.Tests.Runtime.Hosting
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    public class VersionStrategyTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetNextVersion_OldResourceNull_ExpectVersion1()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();
            
            //------------Execute Test---------------------------
            var output =versionStrategy.GetNextVersion(new Mock<IResource>().Object, null, "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber,"1");
            Assert.AreEqual(output.Reason,"save");
            Assert.AreEqual(output.User,"bob");
            Assert.AreEqual(output.DateTimeStamp.Date,DateTime.Today);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetNextVersion_OldResourceNull_Expectincremented()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();

            var oldResource =  new Mock<IResource>();
            var id = Guid.NewGuid();
            var ver = Guid.NewGuid();
            oldResource.Setup(a=>a.VersionInfo).Returns(new VersionInfo(DateTime.Now,"mook","usr","1",id,ver));
            //------------Execute Test---------------------------
            var output = versionStrategy.GetNextVersion(new Mock<IResource>().Object, oldResource.Object, "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber, "2");
            Assert.AreEqual(output.Reason, "save");
            Assert.AreEqual(output.User, "bob");
            Assert.AreEqual(output.DateTimeStamp.Date, DateTime.Today);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetNextVersion_OldResource_Old_ExpectNewVersion()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();

            var oldResource = new Mock<IResource>();

          
            //------------Execute Test---------------------------
            var output = versionStrategy.GetNextVersion(new Mock<IResource>().Object, oldResource.Object, "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber, "1");
            Assert.AreEqual(output.Reason, "save");
            Assert.AreEqual(output.User, "bob");
            Assert.AreEqual(output.DateTimeStamp.Date, DateTime.Today);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetCurrentVersion_OldResource_Old_ExpectNewVersion()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();

            var oldResource = new Mock<IResource>();


            //------------Execute Test---------------------------
            var output = versionStrategy.GetCurrentVersion(new Mock<IResource>().Object, oldResource.Object, "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber, "1");
            Assert.AreEqual(output.Reason, "save");
            Assert.AreEqual(output.User, "bob");
            Assert.AreEqual(output.DateTimeStamp.Date, DateTime.Today);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetCurrentVersion_OldResource_AlwaysIncrementsLastVersion()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();


            //------------Execute Test---------------------------
            var output = versionStrategy.GetCurrentVersion(new Mock<IResource>().Object, new VersionInfo(DateTime.Now,"bob","dave","1",Guid.Empty,Guid.Empty), "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber, "2");
            Assert.AreEqual(output.Reason, "save");
            Assert.AreEqual(output.User, "bob");
            Assert.AreEqual(output.DateTimeStamp.Date, DateTime.Today);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetCurrentVersion_OldResourceNull_GetVersion1()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();


            //------------Execute Test---------------------------
            var output = versionStrategy.GetCurrentVersion(null, new VersionInfo(DateTime.Now, "bob", "dave", "1", Guid.Empty, Guid.Empty), "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber, "2");
            Assert.AreEqual(output.Reason, "save");
            Assert.AreEqual(output.User, "bob");
            Assert.AreEqual(output.DateTimeStamp.Date, DateTime.Today);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetCurrentVersion_OldResource_hasVersion_ExpectExistingVersion()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();
            var id = Guid.NewGuid();
            var ver = Guid.NewGuid();
            var oldResource = new Mock<IResource>();
            oldResource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "12", id, ver));

            //------------Execute Test---------------------------
            var output = versionStrategy.GetCurrentVersion(new Mock<IResource>().Object, oldResource.Object, "bob", "save");


            //------------Assert Results-------------------------
            Assert.AreEqual(output.VersionNumber, "12");
            Assert.AreEqual(output.Reason, "mook");
            Assert.AreEqual(output.User, "usr");
            Assert.AreEqual(output.DateTimeStamp.Date, DateTime.Today);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetNextVersion_Rename_KeepsExistingVersionNumber()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();
            var id = Guid.NewGuid();
            var ver = Guid.NewGuid();
            var oldResource = new Mock<IResource>();
            oldResource.Setup(a => a.ResourceID).Returns(id);
            oldResource.Setup(a => a.VersionInfo).Returns(new VersionInfo(DateTime.Now, "mook", "usr", "7", id, ver));

            //------------Execute Test---------------------------
            var output = versionStrategy.GetNextVersion(new Mock<IResource>().Object, oldResource.Object, "bob", "Rename");

            //------------Assert Results-------------------------
            Assert.AreEqual("7", output.VersionNumber);
            Assert.AreEqual("Rename", output.Reason);
            Assert.AreEqual("bob", output.User);
            Assert.AreEqual(ver, output.VersionId);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("VersionStrategy_GetNextVersion")]
        public void VersionStrategy_GetCurrentVersion_NullVersionInfo_ExpectVersion1()
        {
            //------------Setup for test--------------------------
            var versionStrategy = new VersionStrategy();
            var newResource = new Mock<IResource>();
            newResource.Setup(a => a.ResourceID).Returns(Guid.NewGuid());

            //------------Execute Test---------------------------
            var output = versionStrategy.GetCurrentVersion(newResource.Object, (IVersionInfo)null, "bob", "save");

            //------------Assert Results-------------------------
            Assert.AreEqual("1", output.VersionNumber);
            Assert.AreEqual("save", output.Reason);
            Assert.AreEqual("bob", output.User);
            Assert.AreEqual(DateTime.Today, output.DateTimeStamp.Date);
        }
    }
}
