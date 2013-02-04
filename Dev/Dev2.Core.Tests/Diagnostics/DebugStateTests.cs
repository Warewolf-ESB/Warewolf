using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Tests.Weave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
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
                ID = "ID",
                ParentID = "ParentID",
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
                EndTime = DateTime.Now.AddMinutes(3)
            };
            debugStateIn.Inputs.Add(new DebugItem
            {
                new DebugItemResult { GroupIndex = 0, GroupName = "Group1", Type = DebugItemResultType.Label, Value = "MyLabel" },
                new DebugItemResult { GroupIndex = 0, GroupName = "Group1", Type = DebugItemResultType.Variable, Value = "[[MyVar]]" },
                new DebugItemResult { GroupIndex = 0, GroupName = "Group1", Type = DebugItemResultType.Value, Value = "MyValue" }
            });

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

            Assert.IsTrue(debugStateOut.Inputs[0].SequenceEqual(debugStateIn.Inputs[0], new DebugItemResultEqualityComparer()));
        }

        #endregion

        #region TryCache

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_NullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugStateMock();
            debugState.TryCache(null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueGreaterThanMaxCharDispatchCount_Expected_TruncatesValueToActCharDispatchCount()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(DebugItem.ActCharDispatchCount, item[0].Value.Length);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueGreaterThanMaxCharDispatchCount_Expected_InvokesSaveFileWithFullContent()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();

            var expectedContents = item[0].Value;

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(1, debugState.SaveFileHitCount);
            Assert.AreEqual(expectedContents, debugState.SaveFileContents);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueEqualToMaxCharDispatchCount_Expected_DoesNotTruncateValueToActCharDispatchCount()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, DebugItem.MaxCharDispatchCount);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(DebugItem.MaxCharDispatchCount, item[0].Value.Length);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueEqualToMaxCharDispatchCount_Expected_DoesNotInvokeSaveFile()
        // ReSharper restore InconsistentNaming
        {
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, DebugItem.MaxCharDispatchCount);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(0, debugState.SaveFileHitCount);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueLessThanMaxCharDispatchCount_Expected_DoesNotTruncateValueToActCharDispatchCount()
        // ReSharper restore InconsistentNaming
        {
            const int ExpectedLength = 100;
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, ExpectedLength);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(ExpectedLength, item[0].Value.Length);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void TryCache_With_ValueLessThanMaxCharDispatchCount_Expected_DoesNotInvokeSaveFile()
        // ReSharper restore InconsistentNaming
        {
            const int ExpectedLength = 100;
            var item = CreateDebugItemWithLongValue();
            item[0].Value = item[0].Value.Substring(0, ExpectedLength);

            var debugState = new DebugStateMock();
            debugState.TryCache(new[] { item });

            Assert.AreEqual(0, debugState.SaveFileHitCount);
        }
        #endregion

        #region SaveFile

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming - Unit Test
        public void SaveFile_With_NullParameters_Expected_ThrowsArgumentNullException()
        // ReSharper restore InconsistentNaming
        {
            var debugState = new DebugState();
            debugState.SaveFile(null);
        }

        [TestMethod]
        // ReSharper disable InconsistentNaming - Unit Test
        public void SaveFile_With_Contents_Expected_SavesFileToDisk()
        // ReSharper restore InconsistentNaming
        {

            var debugState = new DebugState();
            var uri = debugState.SaveFile(LongText);

            var path = new Uri(uri).LocalPath;
            var exists = File.Exists(path);
            Assert.IsTrue(exists);

            var contents = File.ReadAllText(path);
            Assert.AreEqual(LongText, contents);
        }

        #endregion

        #region CreateDebugItemWithLongValue

        static DebugItem CreateDebugItemWithLongValue()
        {
            return new DebugItem
            {
                new DebugItemResult
                {
                    Type = DebugItemResultType.Value,
                    Value = LongText
                }
            };

        }

        #endregion

    }
}
