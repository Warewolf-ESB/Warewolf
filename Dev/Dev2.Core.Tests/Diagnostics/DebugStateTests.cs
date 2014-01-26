using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Diagnostics;
using Dev2.Tests.Weave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DebugStateTests
    {
        const string LongText = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."
            + "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. "
            + "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. "
            + "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

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

            var debugState = new DebugState(reader.Object);

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
        public void Write_With_NullDebugWriter_Expected_DoesNothing()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugState();
            debugState.Write((IDebugWriter)null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void Write_With_DebugWriter_Expected_InvokesDebugWriterWithThisState()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugState();

            var writer = new Mock<IDebugWriter>();
            writer.Setup(w => w.Write(It.IsAny<IDebugState>())).Verifiable();

            debugState.Write(writer.Object);

            writer.Verify(w => w.Write(It.Is<IDebugState>(state => state == debugState)));
        }

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

        #endregion

        #region CreateDebugItemWithLongValue

        static DebugItem CreateDebugItemWithLongValue()
        {
            DebugItem result = new DebugItem();
            result.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = LongText });
            return result;
        }

        #endregion

    }
}
