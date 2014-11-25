
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Tests.Weave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DebugStateTests
    {

        #region Constructor

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void Constructor_Expected_InitializesInputsAndOutputsAsEmptyLists()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugState();

            Assert.IsNotNull(debugState.Inputs);
            Assert.IsNotNull(debugState.Outputs);

            Assert.AreEqual(0, debugState.Inputs.Count);
            Assert.AreEqual(0, debugState.Outputs.Count);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void Constructor_With_ByteReaderBase_Expected_InvokesByteReaderBase()
        // ReSharper restore InconsistentNaming
        {
            var reader = new Mock<IByteReaderBase>();
            reader.Setup(w => w.ReadInt32()).Verifiable();
            reader.Setup(w => w.ReadString()).Verifiable();
            reader.Setup(w => w.ReadBoolean()).Verifiable();
            reader.Setup(w => w.ReadGuid()).Verifiable();
            reader.Setup(w => w.ReadDateTime()).Verifiable();

            // ReSharper disable ObjectCreationAsStatement
            new DebugState(reader.Object);
            // ReSharper restore ObjectCreationAsStatement

            reader.Verify(w => w.ReadInt32());
            reader.Verify(w => w.ReadString());
            reader.Verify(w => w.ReadBoolean());
            reader.Verify(w => w.ReadGuid());
            reader.Verify(w => w.ReadDateTime());
        }
        #endregion

        #region Write

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void Write_With_ByteWriterBase_Expected_InvokesByteWriterBase()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugState();

            var writer = new Mock<IByteWriterBase>();
            writer.Setup(w => w.Write(It.IsAny<int>())).Verifiable();
            writer.Setup(w => w.Write(It.IsAny<string>())).Verifiable();
            writer.Setup(w => w.Write(It.IsAny<bool>())).Verifiable();
            writer.Setup(w => w.Write(It.IsAny<Guid>())).Verifiable();
            writer.Setup(w => w.Write(It.IsAny<DateTime>())).Verifiable();

            debugState.Write(writer.Object);

            writer.Verify(w => w.Write(It.IsAny<int>()));
            writer.Verify(w => w.Write(It.IsAny<string>()));
            writer.Verify(w => w.Write(It.IsAny<bool>()));
            writer.Verify(w => w.Write(It.IsAny<Guid>()));
            writer.Verify(w => w.Write(It.IsAny<DateTime>()));
        }
        #endregion

        #region Serialization

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void Serialized_Expected_CanBeDeserialized()
        // ReSharper restore InconsistentNaming
        {
            var rw = new MockByteReaderWriter();

            var debugStateIn = DebugStateIn();

            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { GroupIndex = 0, GroupName = "Group1", Type = DebugItemResultType.Label, Value = "MyLabel" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 0, GroupName = "Group1", Type = DebugItemResultType.Variable, Value = "[[MyVar]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 0, GroupName = "Group1", Type = DebugItemResultType.Value, Value = "MyValue" });
            debugStateIn.Inputs.Add(itemToAdd);

            debugStateIn.Write(rw);
            var debugStateOut = new DebugState(rw);

            Assert.AreEqual(debugStateIn.WorkspaceID, debugStateOut.WorkspaceID);
            Assert.AreEqual(debugStateIn.ID, debugStateOut.ID);
            Assert.AreEqual(debugStateIn.ParentID, debugStateOut.ParentID);
            Assert.AreEqual(debugStateIn.StateType, debugStateOut.StateType);
            Assert.AreEqual(debugStateIn.DisplayName, debugStateOut.DisplayName);
            Assert.AreEqual(debugStateIn.Name, debugStateOut.Name);
            Assert.AreEqual(debugStateIn.ActivityType, debugStateOut.ActivityType);
            Assert.AreEqual(debugStateIn.Version, debugStateOut.Version);
            Assert.AreEqual(debugStateIn.IsSimulation, debugStateOut.IsSimulation);
            Assert.AreEqual(debugStateIn.HasError, debugStateOut.HasError);
            Assert.AreEqual(debugStateIn.ErrorMessage, debugStateOut.ErrorMessage);
            Assert.AreEqual(debugStateIn.Server, debugStateOut.Server);
            Assert.AreEqual(debugStateIn.Server, debugStateOut.Server);
            Assert.AreEqual(debugStateIn.ServerID, debugStateOut.ServerID);
            Assert.AreEqual(debugStateIn.StartTime, debugStateOut.StartTime);
            Assert.AreEqual(debugStateIn.EndTime, debugStateOut.EndTime);
            Assert.AreEqual(debugStateIn.SessionID, debugStateOut.SessionID);

            Assert.IsTrue(debugStateOut.Inputs[0].FetchResultsList().SequenceEqual(debugStateIn.Inputs[0].FetchResultsList(), new DebugItemResultEqualityComparer()));
        }

        // ReSharper disable InconsistentNaming
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DebugItem_Add")]
        public void DebugItem_Add_GroupIndexIsGreaterThan10_MoreLinkHasData()
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { GroupIndex = 1, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "1", Type = DebugItemResultType.Variable, Variable = "[[record(1).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 2, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "2", Type = DebugItemResultType.Variable, Variable = "[[record(2).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 3, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "3", Type = DebugItemResultType.Variable, Variable = "[[record(3).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 4, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "4", Type = DebugItemResultType.Variable, Variable = "[[record(4).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 5, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "5", Type = DebugItemResultType.Variable, Variable = "[[record(5).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 6, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "6", Type = DebugItemResultType.Variable, Variable = "[[record(6).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 7, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "7", Type = DebugItemResultType.Variable, Variable = "[[record(7).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 8, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "8", Type = DebugItemResultType.Variable, Variable = "[[record(8).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 9, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "9", Type = DebugItemResultType.Variable, Variable = "[[record(9).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 10, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "10", Type = DebugItemResultType.Variable, Variable = "[[record(10).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 11, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "11", Type = DebugItemResultType.Variable, Variable = "[[record(11).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 12, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "12", Type = DebugItemResultType.Variable, Variable = "[[record(12).row]]" });
            itemToAdd.Add(new DebugItemResult { GroupIndex = 12, GroupName = "[[record(*).row]]", Label = "", Operator = "=", Value = "13", Type = DebugItemResultType.Variable, Variable = "[[record(13).row]]" });

            Assert.AreEqual(11, itemToAdd.ResultsList.Count);

            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[0].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[1].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[2].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[3].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[4].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[5].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[6].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[7].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[8].MoreLink));
            Assert.IsTrue(string.IsNullOrEmpty(itemToAdd.ResultsList[9].MoreLink));
            Assert.IsFalse(string.IsNullOrEmpty(itemToAdd.ResultsList[10].MoreLink));
        }

        #endregion

        #region CreateDebugItemWithLongValue

        #endregion

        #region CreateDebugState
        static DebugState DebugStateIn()
        {
            var debugStateIn = new DebugState
            {
                WorkspaceID = Guid.NewGuid(),
                ID = Guid.NewGuid(),
                ParentID = Guid.NewGuid(),
                StateType = StateType.Before,
                DisplayName = "DisplayName",
                Name = "Name",
                ActivityType = ActivityType.Step,
                Version = "Version",
                IsSimulation = false,
                HasError = false,
                ErrorMessage = "ErrorMessage",
                Server = "Server",
                ServerID = Guid.NewGuid(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(3),
                SessionID = Guid.NewGuid()
            };
            return debugStateIn;
        }
        #endregion
    }
}
