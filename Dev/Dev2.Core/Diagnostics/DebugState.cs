using Dev2.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Diagnostics
{
    /// <summary>
    /// A default debug state
    /// </summary>
    public class DebugState : IDebugState
    {
        static readonly string InvalidFileNameChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

        readonly string _tempPath;

        #region Ctor

        public DebugState()
        {
            Inputs = new List<IDebugItem>();
            Outputs = new List<IDebugItem>();

            _tempPath = Path.Combine(Path.GetTempPath(), "Dev2", "Debug");
            if(!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
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

        void Serialize(IByteWriterBase writer, IList<IDebugItem> items)
        {
            //TryCache(items);

            writer.Write(items.Count);
            // ReSharper disable ForCanBeConvertedToForeach
            for(var i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].MoreText);
                writer.Write(items[i].MoreLink);
                writer.Write(items[i].Count);
                for(var j = 0; j < items[i].Count; j++)
                {
                    var itemResult = items[i][j];
                    writer.Write((int)itemResult.Type);
                    writer.Write(itemResult.Value);
                    writer.Write(itemResult.GroupName);
                    writer.Write(itemResult.GroupIndex);
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
                    MoreText = reader.ReadString(),
                    MoreLink = reader.ReadString()
                };
                var resultCount = reader.ReadInt32();
                for(var j = 0; j < resultCount; j++)
                {
                    item.Add(new DebugItemResult
                    {
                        Type = (DebugItemResultType)reader.ReadInt32(),
                        Value = reader.ReadString(),
                        GroupName = reader.ReadString(),
                        GroupIndex = reader.ReadInt32()
                    });
                }
                items.Add(item);
            }
        }

        #endregion

        #region TryCache

        public void TryCache(IList<IDebugItem> items)
        {
            if(items == null)
            {
                throw new ArgumentNullException("items");
            }

            if(items.Count <= DebugItem.MaxItemDispatchCount)
            {
                return;
            }

            var group = string.Empty;
            var count = 0;
            var i = 0;
            var groupCache = new List<IDebugItem>();

            while(i < items.Count)
            {
                var item = items[i];

                if(string.IsNullOrEmpty(item.Group))
                {
                    // Scalar                    
                    if(groupCache.Count > DebugItem.MaxItemDispatchCount)
                    {
                        SaveGroup(groupCache, group);
                    }
                    groupCache.Clear();
                    group = string.Empty;
                    count = 0;
                    i++;
                }
                else
                {
                    // Recordset
                    if(group != item.Group)
                    {
                        if(groupCache.Count > DebugItem.MaxItemDispatchCount)
                        {
                            SaveGroup(groupCache, group);
                        }

                        groupCache.Clear();
                        group = item.Group;
                        count = 0;
                    }
                    count++;
                    groupCache.Add(item);
                    if(count > DebugItem.MaxItemDispatchCount)
                    {
                        items.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            if(groupCache.Count > DebugItem.MaxItemDispatchCount)
            {
                SaveGroup(groupCache, group);
            }
        }

        #endregion

        #region SaveGroup

        public virtual string SaveGroup(IList<IDebugItem> items, string groupName)
        {
            if(items == null)
            {
                throw new ArgumentNullException("items");
            }
            if(string.IsNullOrEmpty(groupName))
            {
                throw new ArgumentNullException("groupName");
            }

            var fileName = string.Format("{0}-{1}-{2}-{3}.xml", Name, groupName, StateType, DateTime.Now.ToString("s"));
            fileName = InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), ""));

            var path = Path.Combine(_tempPath, fileName);
            var uriPath = new Uri(path).AbsoluteUri;

            var root = new XElement("Items");
            foreach(var item in items)
            {
                item.MoreLink = uriPath;
                item.MoreText = "More...";
                var xml = item.ToXml();
                root.Add(xml);
            }
            root.Save(path);

            return path;
        }

        #endregion

    }
}
