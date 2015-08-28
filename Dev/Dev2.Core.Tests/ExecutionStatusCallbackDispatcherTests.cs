
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Network.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExecutionStatusCallbackDispatcherTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_Where_CallbackIsNull_Expect_ArgumentNullException()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();

            Guid guid = Guid.NewGuid();

            _executionStatusCallbackDispatcher.Add(guid, null);
        }

        [TestMethod]
        public void Add_Where_ItemsDoesntExist_Expect_True()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();

            Guid guid = Guid.NewGuid();
            Action<ExecutionStatusCallbackMessage> callback = m => { };
            
            const bool expected = true;
            bool actual = _executionStatusCallbackDispatcher.Add(guid, callback);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Add_Where_ItemsExists_Expect_False()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();

            Guid guid = Guid.NewGuid();
            Action<ExecutionStatusCallbackMessage> callback = m => { };
            _executionStatusCallbackDispatcher.Add(guid, callback);

            const bool expected = false;
            bool actual = _executionStatusCallbackDispatcher.Add(guid, callback);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Remove_Where_ItemsDoesntExist_Expect_False()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();

            Guid guid = Guid.NewGuid();

            const bool expected = false;
            bool actual = _executionStatusCallbackDispatcher.Remove(guid);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Remove_Where_ItemsExists_Expect_True()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();

            Guid guid = Guid.NewGuid();
            Action<ExecutionStatusCallbackMessage> callback = m => { };
            _executionStatusCallbackDispatcher.Add(guid, callback);

            const bool expected = true;
            bool actual = _executionStatusCallbackDispatcher.Remove(guid);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Send_Where_MessageDoesntHaveCallbackRegistered_Expect_CallbackNotInvoked()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();

            const bool expected = false;
            bool actual = false;

            Guid guid = Guid.NewGuid();
            Action<ExecutionStatusCallbackMessage> callback = m => { actual = true; };
            ExecutionStatusCallbackMessage message = new ExecutionStatusCallbackMessage(Guid.NewGuid(), ExecutionStatusCallbackMessageType.Unknown);

            _executionStatusCallbackDispatcher.Add(guid, callback);
            _executionStatusCallbackDispatcher.Send(message);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Send_Where_MessageIsNull_Expect_ArgumentNullException()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();
            _executionStatusCallbackDispatcher.Send(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Post_Where_MessageIsNull_Expect_ArgumentNullException()
        {
            ExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher = new ExecutionStatusCallbackDispatcher();
            _executionStatusCallbackDispatcher.Post(null);
        }
    }
}
