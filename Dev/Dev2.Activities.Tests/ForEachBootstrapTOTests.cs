using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities
{
    [TestClass]
    public class ForEachBootstrapTOTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewCsvNoValue_AddError()
        {
            //------------Setup for test--------------------------
            var envMock = new Mock<IExecutionEnvironment>();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InCSV, "", "", "", "", "", envMock.Object, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewCsvValue_NoError()
        {
            //------------Setup for test--------------------------
            var envMock = new ExecutionEnvironment();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InCSV, "", "", "1,2,3", "", "", envMock, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.FetchErrors().Count);
            Assert.AreEqual(forEachBootstrapTO.ExeType, enForEachExecutionType.Scalar);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewRecordSetNoValue_AddError()
        {
            //------------Setup for test--------------------------
            var envMock = new ExecutionEnvironment();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InRecordset, "", "", "", "", "", envMock, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.FetchErrors().Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewRecordSetNoValueNoInVaribaleList_AddError()
        {
            //------------Setup for test--------------------------
            var envMock = new Mock<IExecutionEnvironment>();
            envMock.Setup(environment => environment.EvalRecordSetIndexes("[[rec2()]]", It.IsAny<int>())).Verifiable();
            envMock.Setup(environment => environment.HasRecordSet("[[rec2()]]")).Returns(false).Verifiable();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InRecordset, "", "", "", "", "[[rec2()]]", envMock.Object, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewRecordSetValue_NoError()
        {
            //------------Setup for test--------------------------
            var envMock = new ExecutionEnvironment();
            envMock.Assign("[[rec().a]]","Hello There",1);
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InRecordset, "", "", "", "", "[[rec()]]", envMock, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewInRangeNoValue_AddError()
        {
            //------------Setup for test--------------------------
            var envMock = new ExecutionEnvironment();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InRange, "", "", "", "", "", envMock, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewInRangeMinValueOnly_AddError()
        {
            //------------Setup for test--------------------------
            var envMock = new ExecutionEnvironment();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InRange, "1", "", "", "", "", envMock, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ForEachBootstrapTO_Constructor")]
        public void ForEachBootstrapTO_Constructor_IsNewInRangeMinAndMax_NoError()
        {
            //------------Setup for test--------------------------
            var envMock = new ExecutionEnvironment();
            var forEachBootstrapTO = new ForEachBootstrapTO(enForEachType.InRange, "1", "3", "", "", "", envMock, out ErrorResultTO errors, 0);

            //------------Execute Test---------------------------
            Assert.IsNotNull(forEachBootstrapTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.FetchErrors().Count);
        }
    }
}
