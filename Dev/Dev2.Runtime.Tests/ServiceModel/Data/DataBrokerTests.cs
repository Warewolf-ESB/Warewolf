using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class DataBrokerTests
    {
        #region Execute Tests

        [TestMethod]
        public void ExecuteCommandWhereNoErrorsExpectedDataReaderReadInvocationsOneHigherThanRowProcessorInvocations()
        {
            int readCount = 0;
            var reader = new Mock<IDataReader>();
            reader.Setup(r => r.Read()).Returns(() => readCount < 5).Callback(() =>
                {
                    readCount++;
                });
            reader.Setup(r => r.GetValues(It.IsAny<object[]>())).Returns(3);

            var command = new Mock<IDbCommand>();
            command.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(reader.Object);

            int rowProcessorCount = 0;
            Func<IDataReader, bool> rowProcessor = dataReader => 
            {
                rowProcessorCount++;
                return true;
            };
            
            var dataBroker = new DataBrokerMock();
            dataBroker.Execute(command.Object, rowProcessor);

            int expected = readCount - 1;
            int actual = rowProcessorCount;

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
