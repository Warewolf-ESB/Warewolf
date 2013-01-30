using Dev2.Enums;
using System;
using System.Collections.Generic;

namespace Dev2.Diagnostics
{
    /// <summary>
    /// A default debug state
    /// </summary>
    public class DebugState : IDebugState
    {
        #region Ctor

        public DebugState()
        {
            Inputs = new List<IDebugItem>();
            Outputs = new List<IDebugItem>();
        }

        #endregion

        #region IDebugState - Properties

        /// <summary>
        /// Gets or sets the workspace ID.
        /// </summary>
        public Guid WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the parent ID.
        /// </summary>
        public string ParentID { get; set; }

        /// <summary>
        /// Gets or sets the type of the state.
        /// </summary>
        public StateType StateType { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the activity version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is simulation.
        /// </summary>
        public bool IsSimulation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has an error.
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the server ID.
        /// </summary>
        public Guid ServerID { get; set; }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        public IList<IDebugItem> Inputs { get; private set; }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        public IList<IDebugItem> Outputs { get; private set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        #endregion

        #region IDebugState - Write

        /// <summary>
        /// Writes this instance to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to which this instance is written.</param>
        public void Write(IDebugWriter writer)
        {
            if(writer == null)
            {
                return;
            }
            writer.Write(this);
        }

        #endregion

        #region IByteReader/Writer Serialization

        public DebugState(IByteReaderBase reader)
            : this()
        {
            WorkspaceID = reader.ReadGuid();
            ID = reader.ReadString();
            ParentID = reader.ReadString();
            StateType = (StateType)reader.ReadInt32();
            DisplayName = reader.ReadString();
            Name = reader.ReadString();
            ActivityType = (ActivityType)reader.ReadInt32();
            Version = reader.ReadString();
            IsSimulation = reader.ReadBoolean();
            HasError = reader.ReadBoolean();
            ErrorMessage = reader.ReadString();
            Server = reader.ReadString();
            ServerID = reader.ReadGuid();
            StartTime = reader.ReadDateTime();
            EndTime = reader.ReadDateTime();

            Deserialize(reader, Inputs);
            Deserialize(reader, Outputs);
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(WorkspaceID);
            writer.Write(ID);
            writer.Write(ParentID);
            writer.Write((int)StateType);
            writer.Write(DisplayName);
            writer.Write(Name);
            writer.Write((int)ActivityType);
            writer.Write(Version);
            writer.Write(IsSimulation);
            writer.Write(HasError);
            writer.Write(ErrorMessage);
            writer.Write(Server);
            writer.Write(ServerID);
            writer.Write(StartTime);
            writer.Write(EndTime);

            Serialize(writer, Inputs);
            Serialize(writer, Outputs);
        }

        #endregion

        #region IDebugItem serialization helper methods

        static void Serialize(IByteWriterBase writer, IList<IDebugItem> items)
        {
            writer.Write(items.Count);
            // ReSharper disable ForCanBeConvertedToForeach
            for(var i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].Group);
                writer.Write(items[i].Label);
                writer.Write(items[i].Results.Count);
                for(var j = 0; j < items[i].Results.Count; j++)
                {
                    writer.Write(items[i].Results[j].Variable);
                    writer.Write(items[i].Results[j].Value);
                }
            }
            // ReSharper restore ForCanBeConvertedToForeach
        }

        static void Deserialize(IByteReaderBase reader, ICollection<IDebugItem> items)
        {
            var count = reader.ReadInt32();
            for(var i = 0; i < count; i++)
            {
                var item = new DebugItem
                {
                    Group = reader.ReadString(),
                    Label = reader.ReadString()
                };
                var resultCount = reader.ReadInt32();
                for(var j = 0; j < resultCount; j++)
                {
                    item.Results.Add(new DebugItemResult
                    {
                        Variable = reader.ReadString(),
                        Value = reader.ReadString()
                    });
                }
                items.Add(item);
            }
        }

        #endregion
    }
}
