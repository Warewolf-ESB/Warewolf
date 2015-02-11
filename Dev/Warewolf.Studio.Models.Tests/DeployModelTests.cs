using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Models.Deploy;

namespace Warewolf.Studio.Models.Tests
{
    [TestClass]
    public class DeployModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DeployModel_Ctor_NullParamsSVR_Excepxception()


        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var deployModel = new DeployModel(null, new Mock<IUpdateManager>().Object, qm.Object);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DeployModel_Ctor_NullParamsUM_Excepxception()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var deployModel = new DeployModel(new Mock<IServer>().Object,null, qm.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DeployModel_Ctor_NullParamsQM_Excepxception()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var deployModel = new DeployModel(new Mock<IServer>().Object, new Mock<IUpdateManager>().Object, null);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_FindDependancies")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployModel_FindDependancies_NullParams_Excepxception()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var deployModel = new DeployModel(new Mock<IServer>().Object, new Mock<IUpdateManager>().Object, qm.Object);

            //------------Execute Test---------------------------
    
            //------------Assert Results-------------------------
            deployModel.GetDependancies(Guid.Empty);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_FindDependancies")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployModel_FindDependancies_ValidResource_VerifyThatProxyIsCalled()
        {

            var qm = new Mock<IQueryManager>();
            //------------Setup for test--------------------------
            var deployModel = new DeployModel(new Mock<IServer>().Object,new Mock<IUpdateManager>().Object,qm.Object);
            var res = new Mock<IResource>();
            var resGuid = Guid.NewGuid();
            res.Setup(a => a.ResourceID).Returns(resGuid);
            //------------Execute Test---------------------------
            deployModel.GetDependancies(Guid.Empty);
            //------------Assert Results-------------------------
            qm.Verify(a => a.FetchDependencies(Guid.Empty));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_FindDependancies")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployModel_FindDependancies_ValidResource_VerifyThatProxyIsCalledAndItemsAreReturned()
        {

            var qm = new Mock<IQueryManager>();
            //------------Setup for test--------------------------
            var deployModel = new DeployModel(new Mock<IServer>().Object, new Mock<IUpdateManager>().Object, qm.Object);
            var res = new Mock<IResource>();
            var resGuid = Guid.NewGuid();
            res.Setup(a => a.ResourceID).Returns(resGuid);
            qm.Setup(a => a.FetchDependencies(resGuid)).Returns(new List<IResource>());
            //------------Execute Test---------------------------
            deployModel.GetDependancies(Guid.Empty);
            //------------Assert Results-------------------------
           
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployModel_Save_NullParams_Excepxception()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var deployModel = new DeployModel(new Mock<IServer>().Object, new Mock<IUpdateManager>().Object, qm.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            deployModel.Deploy(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployModel_Save_ValidResource_VerifyThatProxyIsCalled()
        {

            var qm = new Mock<IQueryManager>();
            //------------Setup for test--------------------------
            var deployModel = new DeployModel(new Mock<IServer>().Object, new Mock<IUpdateManager>().Object, qm.Object);
            var res = new Mock<IResource>();
            var resGuid = Guid.NewGuid();
            res.Setup(a => a.ResourceID).Returns(resGuid);
            //------------Execute Test---------------------------
            deployModel.Deploy(res.Object);
            //------------Assert Results-------------------------
            qm.Verify(a => a.FetchDependencies(resGuid));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeployModel_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeployModel_Save_ValidResource_VerifyThatProxyIsCalledAndItemsAreReturned()
        {

            var qm = new Mock<IQueryManager>();
            //------------Setup for test--------------------------
            var deployModel = new DeployModel(new Mock<IServer>().Object, new Mock<IUpdateManager>().Object, qm.Object);
            var res = new Mock<IResource>();
            var resGuid = Guid.NewGuid();
            res.Setup(a => a.ResourceID).Returns(resGuid);
            qm.Setup(a => a.FetchDependencies(resGuid)).Returns(new List<IResource>());
            //------------Execute Test---------------------------
            deployModel.Deploy(res.Object);
            //------------Assert Results-------------------------

        }

    }
    // ReSharper restore InconsistentNaming
}
