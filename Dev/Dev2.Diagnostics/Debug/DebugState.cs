/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Logging;

#endregion

namespace Dev2.Diagnostics.Debug
{
    /// <summary>
    ///     A default debug state
    /// </summary>
    [Serializable]
    public class DebugState : IDebugState,INotifyPropertyChanged
    {
        private DateTime _startTime;
        private DateTime _endTime;
        string _errorMessage;
        bool _isDurationVisible;
        string _server;
        Guid _environmentID;
        Guid _disconnectedID;

        #region Ctor

        public DebugState()
        {
            Inputs = new List<IDebugItem>();
            Outputs = new List<IDebugItem>();
            AssertResultList = new List<IDebugItem>();
            DisconnectedID = Guid.NewGuid();

            IsDurationVisible = true;
            _disconnectedID = Guid.NewGuid();
        }

        public Guid DisconnectedID
        {
            get
            {
                return _disconnectedID;
            }
            set
            {
                _disconnectedID = value;
            }
        }

        static DebugState()
        {
            var tempPath = Path.Combine(GlobalConstants.TempLocation, "Warewolf", "Debug");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
        }
        #endregion

        #region IDebugState - Properties

        /// <summary>
        ///     Gets or sets the ID.
        /// </summary>
        public Guid ID { get; set; }
     

        /// <summary>
        ///     Gets or sets the parent ID.
        /// </summary>
        public Guid ParentID { get; set; }
        public Guid SourceResourceID { get; set; }

        /// <summary>
        ///     Gets or sets the server ID.
        /// </summary>
        public Guid ServerID { get; set; }

        /// <summary>
        /// Gets or sets the environment ID.
        /// </summary>
        public Guid EnvironmentID
        {
            get
            {
                return _environmentID;
            }
            set
            {
                _environmentID = value;
            }
        }

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        public Guid ClientID { get; set; }

        /// <summary>
        ///     Gets or sets the type of the state.
        /// </summary>
        public StateType StateType { get; set; }

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has an error.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        public bool HasError { get; set; }

        /// <summary>
        ///     Gets or sets the error message
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
            }
        }

        /// <summary>
        ///     Gets or sets the activity version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets the name of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the type of the activity.
        /// </summary>
        public ActivityType ActivityType { get; set; }

        public TimeSpan Duration => EndTime - StartTime;

        // XmlSerializer does not support TimeSpan, so use this property for serialization 
        // instead.
        public string DurationString
        {
            get
            {
                return XmlConvert.ToString(Duration);
            }
            // ReSharper disable ValueParameterNotUsed
            set
            // ReSharper restore ValueParameterNotUsed
            {
            }
        }

        /// <summary>
        ///     Gets or sets the start time.
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
            }
        }

        /// <summary>
        ///     Gets or sets the end time.
        /// </summary>
        /// <value>
        ///     The end time.
        /// </value>
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged();
                OnPropertyChanged("DurationString");
                OnPropertyChanged("Duration");
            }
        }

        /// <summary>
        ///     Gets the inputs.
        /// </summary>
        public List<IDebugItem> Inputs { get; private set; }

        /// <summary>
        ///     Gets the outputs.
        /// </summary>
        public List<IDebugItem> Outputs { get; private set; }

        /// <summary>
        ///     Gets the outputs.
        /// </summary>
        public List<IDebugItem> AssertResultList { get; private set; }

        /// <summary>
        ///     Gets or sets the server name.
        /// </summary>
        [XmlIgnore]
        public string Server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
            }
        }

        /// <summary>
        ///     Gets or sets the workspace ID.
        /// </summary>
        [XmlIgnore]
        public Guid WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets the original instance ID.
        /// </summary>
        /// <value>
        /// The original instance ID.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        [XmlIgnore]
        public Guid OriginalInstanceID { get; set; }

        /// <summary>
        ///     Gets or sets the server ID.
        /// </summary>
        [XmlIgnore]
        public Guid OriginatingResourceID { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is simulation.
        /// </summary>
        [XmlIgnore]
        public bool IsSimulation { get; set; }

        /// <summary>
        /// Gets or sets a message used to display content in the debug viewmodel
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        [XmlIgnore]
        public string Message { get; set; }

        public int NumberOfSteps { get; set; }

        public string Origin
        {
            get
            {

                switch(ExecutionOrigin)
                {
                    case ExecutionOrigin.Unknown:
                        return string.Empty;
                    case ExecutionOrigin.Debug:
                        return ExecutionOrigin.GetDescription();
                    case ExecutionOrigin.External:
                        return ExecutionOrigin.GetDescription();
                    case ExecutionOrigin.Workflow:
                        return string.Format("{0} - {1}",
                                             ExecutionOrigin.GetDescription(), ExecutionOriginDescription);
                }

                return string.Empty;
            }
        }

        public ExecutionOrigin ExecutionOrigin { get; set; }

        public string ExecutionOriginDescription { get; set; }

        public string ExecutingUser { get; set; }

        public Guid SessionID { get; set; }
        public Guid WorkSurfaceMappingId { get; set; }

        #endregion

        #region IByteReader/Writer Serialization

        public DebugState(IByteReaderBase reader)
            : this()
        {
            WorkspaceID = reader.ReadGuid();
            ID = reader.ReadGuid();
            ParentID = reader.ReadGuid();
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
            OriginatingResourceID = reader.ReadGuid();
            OriginalInstanceID = reader.ReadGuid();
            StartTime = reader.ReadDateTime();
            EndTime = reader.ReadDateTime();
            NumberOfSteps = reader.ReadInt32();
            ExecutionOrigin = (ExecutionOrigin)reader.ReadInt32();
            ExecutionOriginDescription = reader.ReadString();
            ExecutingUser = reader.ReadString();
            EnvironmentID = reader.ReadGuid();
            SessionID = reader.ReadGuid();

            Deserialize(reader, Inputs);
            Deserialize(reader, Outputs);
            Deserialize(reader, AssertResultList);
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
            writer.Write(OriginatingResourceID);
            writer.Write(OriginalInstanceID);
            writer.Write(StartTime);
            writer.Write(EndTime);
            writer.Write(NumberOfSteps);
            writer.Write((int)ExecutionOrigin);
            writer.Write(ExecutionOriginDescription);
            writer.Write(ExecutingUser);
            writer.Write(EnvironmentID);
            writer.Write(SessionID);

            Serialize(writer, Inputs);
            Serialize(writer, Outputs);
            Serialize(writer, AssertResultList);
        }

        #endregion

        #region IDebugItem serialization helper methods

        private void Serialize(IByteWriterBase writer, IList<IDebugItem> items)
        {

            writer.Write(items.Count);
            // ReSharper disable ForCanBeConvertedToForeach
            for(var i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].FetchResultsList().Count);
                for(var j = 0; j < items[i].FetchResultsList().Count; j++)
                {
                    var itemResult = items[i].FetchResultsList()[j];
                    writer.Write((int)itemResult.Type);
                    writer.Write(itemResult.Label);
                    writer.Write(itemResult.Variable);
                    writer.Write(itemResult.Value);
                    writer.Write(itemResult.GroupName);
                    writer.Write(itemResult.GroupIndex);
                    writer.Write(itemResult.MoreLink);
                }
            }
            // ReSharper restore ForCanBeConvertedToForeach
        }

        private static void Deserialize(IByteReaderBase reader, ICollection<IDebugItem> items)
        {
            var count = reader.ReadInt32();
            for(var i = 0; i < count; i++)
            {
                var item = new DebugItem();
                var resultCount = reader.ReadInt32();
                for(var j = 0; j < resultCount; j++)
                {
                    item.Add(new DebugItemResult
                        {
                            Type = (DebugItemResultType)reader.ReadInt32(),
                            Label = reader.ReadString(),
                            Variable = reader.ReadString(),
                            Value = reader.ReadString(),
                            GroupName = reader.ReadString(),
                            GroupIndex = reader.ReadInt32(),
                            MoreLink = reader.ReadString()
                        }, true);
                }
                items.Add(item);
            }
        }

        #endregion


        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            DisplayName = reader.GetAttribute("DisplayName");
            Guid guid;
            Guid.TryParse(reader.GetAttribute("ID"), out guid);
            ID = guid;
            Guid.TryParse(reader.GetAttribute("OriginalInstanceID"), out guid);
            OriginalInstanceID = guid;
            Guid.TryParse(reader.GetAttribute("ParentID"), out guid);
            ParentID = guid;
            Guid.TryParse(reader.GetAttribute("ServerID"), out guid);
            ServerID = guid;

            StateType state;
            Enum.TryParse(reader.GetAttribute("StateType"), out state);
            StateType = state;

            Guid.TryParse(reader.GetAttribute("SessionID"), out guid);
            SessionID = guid;

            while(reader.Read())
            {
                if(reader.IsStartElement("HasError"))
                {
                    var result = reader.ReadElementString("HasError");

                    bool boolean;
                    var exists = bool.TryParse(result, out boolean);
                    HasError = exists && boolean;
                }

                if(reader.IsStartElement("ErrorMessage"))
                {
                    ErrorMessage = reader.ReadElementString("ErrorMessage");
                }

                if(reader.IsStartElement("Version"))
                {
                    Version = reader.ReadElementString("Version");
                }

                if(reader.IsStartElement("Name"))
                {
                    Name = reader.ReadElementString("Name");
                }

                if(reader.IsStartElement("ActivityType"))
                {
                    var result = reader.ReadElementString("ActivityType");

                    ActivityType activityType;
                    Enum.TryParse(result, out activityType);
                    ActivityType = activityType;
                }

                if(reader.IsStartElement("Duration"))
                {
                    DurationString = reader.ReadElementString("Duration");
                }

                if(reader.IsStartElement("StartTime"))
                {
                    var result = reader.ReadElementString("StartTime");

                    DateTime date;
                    DateTime.TryParse(result, out date);
                    StartTime = date;
                }

                if(reader.IsStartElement("EndTime"))
                {
                    var result = reader.ReadElementString("EndTime");

                    DateTime date;
                    DateTime.TryParse(result, out date);
                    EndTime = date;
                }


                if(reader.IsStartElement("Inputs"))
                {
                    Inputs = new List<IDebugItem>();
                    reader.ReadStartElement();
                    while(reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugItem")
                    {
                        var item = new DebugItem();
                        item.ReadXml(reader);
                        Inputs.Add(item);
                    }
                    reader.ReadEndElement();
                }

                if(reader.IsStartElement("Outputs"))
                {
                    Outputs = new List<IDebugItem>();
                    reader.ReadStartElement();
                    while(reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugItem")
                    {
                        var item = new DebugItem();
                        item.ReadXml(reader);
                        Outputs.Add(item);
                    }
                    reader.ReadEndElement();
                }

                if(reader.IsStartElement("AssertResultList"))
                {
                    AssertResultList = new List<IDebugItem>();
                    reader.ReadStartElement();
                    while(reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugItem")
                    {
                        var item = new DebugItem();
                        item.ReadXml(reader);
                        AssertResultList.Add(item);
                    }
                    reader.ReadEndElement();
                }

                if(reader.IsStartElement("ExecutionOrigin"))
                {
                    var result = reader.ReadElementString("ExecutionOrigin");

                    ExecutionOrigin origin;
                    var exists = Enum.TryParse(result, out origin);
                    if(exists)
                    {
                        ExecutionOrigin = origin;
                    }
                }

                if(reader.IsStartElement("ExecutingUser"))
                {
                    ExecutingUser = reader.ReadElementString("ExecutingUser");
                }

                if(reader.IsStartElement("NumberOfSteps"))
                {
                    int numberOfSteps;
                    var success = int.TryParse(reader.ReadElementString("NumberOfSteps"), out numberOfSteps);
                    if(success)
                    {
                        NumberOfSteps = numberOfSteps;
                    }
                }

                if(reader.NodeType == XmlNodeType.EndElement && reader.Name == "DebugState")
                {
                    reader.ReadEndElement();
                    break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            //------Always log these for reconstruction------------
            writer.WriteAttributeString("DisplayName", DisplayName);

            writer.WriteAttributeString("ID", ID.ToString());

            writer.WriteAttributeString("OriginalInstanceID", OriginalInstanceID.ToString());

            writer.WriteAttributeString("ParentID", ParentID.ToString());

            writer.WriteAttributeString("ServerID", ServerID.ToString());

            writer.WriteAttributeString("StateType", StateType.ToString());

            writer.WriteElementString("HasError", HasError.ToString());

            writer.WriteAttributeString("SessionID", SessionID.ToString());

            if(HasError)
            {
                writer.WriteElementString("ErrorMessage", ErrorMessage);
            }
            //-----------------------------

            var settings = WorkflowLoggger.LoggingSettings;

            //Version
            if(settings.IsVersionLogged && !string.IsNullOrWhiteSpace(Version))
            {
                writer.WriteElementString("Version", Version);
            }

            //Type
            if(settings.IsTypeLogged)
            {
                writer.WriteElementString("Name", Name);
                writer.WriteElementString("ActivityType", ActivityType.ToString());
            }

            //Duration
            if(settings.IsDurationLogged)
            {
                if(Duration != default(TimeSpan))
                {
                    writer.WriteElementString("Duration", DurationString);
                }
            }

            //DateTime
            if(settings.IsDataAndTimeLogged)
            {
                if(StartTime != DateTime.MinValue)
                {
                    writer.WriteElementString("StartTime", StartTime.ToString("G"));
                }
                if(EndTime != DateTime.MinValue)
                {
                    writer.WriteElementString("EndTime", EndTime.ToString("G"));
                }
            }


            //Input
            if(settings.IsInputLogged && Inputs.Count > 0)
            {
                writer.WriteStartElement("Inputs");
                writer.WriteAttributeString("Count", Inputs.Count.ToString(CultureInfo.InvariantCulture));

                var inputSer = new XmlSerializer(typeof(DebugItem));
                foreach(var other in Inputs)
                {
                    inputSer.Serialize(writer, other);
                }
                writer.WriteEndElement();
            }

            //Output
            if(settings.IsOutputLogged && Outputs.Count > 0)
            {
                writer.WriteStartElement("Outputs");
                writer.WriteAttributeString("Count", Outputs.Count.ToString(CultureInfo.InvariantCulture));

                var outputSer = new XmlSerializer(typeof(DebugItem));
                foreach(var other in Outputs)
                {
                    outputSer.Serialize(writer, other);
                }
                writer.WriteEndElement();
            }

            //AssertResultList
            if (AssertResultList.Count > 0)
            {
                writer.WriteStartElement("AssertResultList");
                writer.WriteAttributeString("Count", AssertResultList.Count.ToString(CultureInfo.InvariantCulture));

                var assertListSer = new XmlSerializer(typeof(DebugItem));
                foreach(var other in AssertResultList)
                {
                    assertListSer.Serialize(writer, other);
                }
                writer.WriteEndElement();
            }

            //StartBlock
            if(IsFirstStep())
            {
                if(ExecutionOrigin != ExecutionOrigin.Unknown)
                {
                    writer.WriteElementString("ExecutionOrigin", ExecutionOrigin.ToString());
                }
                if(!string.IsNullOrWhiteSpace(ExecutingUser))
                {
                    writer.WriteElementString("ExecutingUser", ExecutingUser);
                }
            }

            //EndBlock

            if(IsFinalStep())
            {
                writer.WriteElementString("NumberOfSteps", NumberOfSteps.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool IsFinalStep()
        {
            return StateType == StateType.End && OriginalInstanceID == ID && ParentID==Guid.Empty;
        }

        public bool IsFirstStep()
        {
            return StateType == StateType.Start &&
                   OriginalInstanceID == ID;
        }

        public bool IsDurationVisible
        {
            get
            {
                return _isDurationVisible;
            }
            set
            {
                _isDurationVisible = value;
            }
        }
        public string ActualType { get; set; }

        #region Implementation of IEquatable<IDebugState>


        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IDebugState other)
        {
            if(ReferenceEquals(null, other))
            {
                return false;
            }
            if(ReferenceEquals(this, other))
            {
                return true;
            }
            return ID.Equals(other.ID) && SessionID.Equals(other.SessionID);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }
            if(ReferenceEquals(this, obj))
            {
                return true;
            }
            if(obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DebugState)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (ID.GetHashCode() * 397) ^ SessionID.GetHashCode();
            }
        }

        public static bool operator ==(DebugState left, DebugState right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DebugState left, DebugState right)
        {
            return !Equals(left, right);
        }

        #endregion

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
