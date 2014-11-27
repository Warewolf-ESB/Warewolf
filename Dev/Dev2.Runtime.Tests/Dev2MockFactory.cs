
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Data;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Network.Execution;
using Moq;

namespace Dev2.DynamicServices.Test
{
    public static class Dev2MockFactory
    {
        public static Mock<IDataListServer> SetupDataListServer(bool writeResult = true, bool persistChildChainResult = true, IBinaryDataList readResult = null, bool readCausesException = false, bool writeCausesException = false, bool persistChildChainCausesException = false, bool deleteCausesException = false)
        {
            Mock<IDataListServer> mockDataListServer = new Mock<IDataListServer>();
            ErrorResultTO errors;

            if(readCausesException)
            {
                mockDataListServer.Setup(e => e.ReadDatalist(It.IsAny<Guid>(), out errors)).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.ReadDatalist(It.IsAny<Guid>(), out errors)).Verifiable();
                mockDataListServer.Setup(e => e.ReadDatalist(It.IsAny<Guid>(), out errors)).Returns(readResult);
            }

            if(writeCausesException)
            {
                mockDataListServer.Setup(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out errors)).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out errors)).Verifiable();
                mockDataListServer.Setup(e => e.WriteDataList(It.IsAny<Guid>(), It.IsAny<IBinaryDataList>(), out errors)).Returns(writeResult);
            }


            if(deleteCausesException)
            {
                mockDataListServer.Setup(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>())).Throws(new Exception());
            }
            else
            {
                mockDataListServer.Setup(e => e.DeleteDataList(It.IsAny<Guid>(), It.IsAny<bool>())).Verifiable();
            }

            return mockDataListServer;
        }

        public static Mock<IExecutionStatusCallbackDispatcher> SetupExecutionStatusCallbackDispatcher(bool addResult = true, bool removeResult = true)
        {
            Mock<IExecutionStatusCallbackDispatcher> mockExecutionStatusCallbackDispatcher = new Mock<IExecutionStatusCallbackDispatcher>();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Add(It.IsAny<Guid>(), It.IsAny<Action<ExecutionStatusCallbackMessage>>())).Returns(addResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Remove(It.IsAny<Guid>())).Returns(removeResult);

            mockExecutionStatusCallbackDispatcher.Setup(e => e.RemoveRange(It.IsAny<IList<Guid>>())).Verifiable();

            mockExecutionStatusCallbackDispatcher.Setup(e => e.Post(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();
            mockExecutionStatusCallbackDispatcher.Setup(e => e.Send(It.IsAny<ExecutionStatusCallbackMessage>())).Verifiable();

            return mockExecutionStatusCallbackDispatcher;
        }



        public static Mock<IDataReader> SetupDataReader(List<object[]> results)
        {
            int readCount = 0;
            var reader = new Mock<IDataReader>();
            // ReSharper disable ImplicitlyCapturedClosure
            reader.Setup(r => r.Read()).Returns(() => readCount < results.Count).Callback(() =>
            // ReSharper restore ImplicitlyCapturedClosure
            {
                readCount++;
            });
            reader.Setup(r => r.GetValues(It.IsAny<object[]>())).Returns(results[readCount].Length);

            return reader;
        }

        public static Mock<IDbConnection> SetupDbConnection()
        {
            var connection = new Mock<IDbConnection>();
            connection.Setup(c => c.Close()).Verifiable();
            connection.Setup(c => c.Dispose()).Verifiable();

            return connection;
        }

        public static Mock<IDbCommand> SetupDbCommand(Mock<IDataReader> dataReader, Mock<IDbConnection> connection)
        {
            var command = new Mock<IDbCommand>();
            command.Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>())).Returns(dataReader.Object);
            command.Setup(c => c.Connection).Returns(connection.Object);

            return command;
        }

        public static List<object[]> FakeDataBrokerTestsResults(int rows, int columns)
        {
            List<object[]> results = new List<object[]>();

            for(int j = 0; j < rows; j++)
            {
                object[] row = new object[columns];
                for(int i = 0; i < columns; i++)
                {
                    row[i] = i;
                }
                results.Add(row);
            }

            return results;
        }
    }
}
