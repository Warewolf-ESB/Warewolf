//using Dev2.DynamicServices.Test;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Data;

//namespace Dev2.Tests.Runtime.ServiceModel.Data
//{
//    [TestClass]
//    public class DataBrokerTests
//    {
//        #region Execute Tests

//        [TestMethod]
//        public void ExecuteCommandWhereNoErrorsExpectedRowProcessorInvocationsEqualToNumberOfRows()
//        {
//            int resultRows = 5;
//            int resultColumns = 3;
//            var reader = Dev2MockFactory.SetupDataReader(Dev2MockFactory.FakeDataBrokerTestsResults(resultRows, resultColumns));
//            var command = Dev2MockFactory.SetupDbCommand(reader, Dev2MockFactory.SetupDbConnection());

//            int rowProcessorCount = 0;
//            Func<IDataReader, bool> rowProcessor = dataReader => 
//            {
//                rowProcessorCount++;
//                return true;
//            };
            
//            var dataBroker = new DataBrokerMock();
//            dataBroker.ExecuteSelect(command.Object, rowProcessor);

//            int expected = resultRows;
//            int actual = rowProcessorCount;

//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        [ExpectedException(typeof(Exception))]
//        public void ExecuteCommandWhereProcessorThrowsExceptionAndContinueOnProcessorExceptionIsFalseExpectedException()
//        {
//            int resultRows = 5;
//            int resultColumns = 3;
//            var reader = Dev2MockFactory.SetupDataReader(Dev2MockFactory.FakeDataBrokerTestsResults(resultRows, resultColumns));
//            var command = Dev2MockFactory.SetupDbCommand(reader, Dev2MockFactory.SetupDbConnection());

//            Func<IDataReader, bool> rowProcessor = dataReader =>
//            {
//                throw new Exception();
//            };

//            var dataBroker = new DataBrokerMock();
//            dataBroker.ExecuteSelect(command.Object, rowProcessor);
//        }

//        [TestMethod]
//        public void ExecuteCommandWhereProcessorThowsExceptionOnSomeRowsAndContinueOnProcessorExceptionIsTrueExpectedRowProcessorInvocationsEqualToNumberOfRowsAndNoExcpetions()
//        {
//            int resultRows = 5;
//            int resultColumns = 3;
//            var reader = Dev2MockFactory.SetupDataReader(Dev2MockFactory.FakeDataBrokerTestsResults(resultRows, resultColumns));
//            var command = Dev2MockFactory.SetupDbCommand(reader, Dev2MockFactory.SetupDbConnection());

//            int rowProcessorCount = 0;
//            Func<IDataReader, bool> rowProcessor = dataReader =>
//            {
//                rowProcessorCount++;

//                if (rowProcessorCount == 2 || rowProcessorCount == 4)
//                {
//                    throw new Exception();
//                }
                
//                return true;    
//            };

//            var dataBroker = new DataBrokerMock();
//            dataBroker.ExecuteSelect(command.Object, rowProcessor, true);

//            int expected = resultRows;
//            int actual = rowProcessorCount;

//            Assert.AreEqual(expected, actual);
//        }

//        #endregion
//    }
//}
