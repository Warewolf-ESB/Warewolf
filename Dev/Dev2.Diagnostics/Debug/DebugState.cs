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
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#endregion

namespace Dev2.Diagnostics.Debug
{
    /// <summary>
    ///     A default debug state
    /// </summary>
    [Serializable]
    public class DebugState : IDebugState, INotifyPropertyChanged
    {
        DateTime _startTime;
        String _durationString;
        DateTime _endTime;
        string _errorMessage;
        bool _isDurationVisible;
        string _server;
        Guid _environmentID;
        Guid _disconnectedID;
        Guid? _parentID;
        Guid _id;

        #region Ctor

        public DebugState()
        {
            Inputs = new List<IDebugItem>();
            Outputs = new List<IDebugItem>();
            AssertResultList = new List<IDebugItem>();
            Children = new List<IDebugState>();
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
        public Guid ID
        {
            get { return _id; }
            set { _id = value; }
        }


        /// <summary>
        ///     Gets or sets the parent ID.
        /// </summary>
        public Guid? ParentID
        {
            get { return _parentID; }
            set
            {
                _parentID = value;
                OnPropertyChanged();
            }
        }

        public bool IsAdded { get; set; }
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
        [JsonConverter(typeof(StringEnumConverter))]
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
            set => _durationString = value;
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

                switch (ExecutionOrigin)
                {
                    case ExecutionOrigin.Unknown:
                        return string.Empty;
                    case ExecutionOrigin.Debug:
                        return ExecutionOrigin.GetDescription();
                    case ExecutionOrigin.External:
                        return ExecutionOrigin.GetDescription();
                    case ExecutionOrigin.Workflow:
                        return $"{ExecutionOrigin.GetDescription()} - {ExecutionOriginDescription}";
                    default:
                        break;
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
            writer.Write(ParentID.GetValueOrDefault());
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

        void Serialize(IByteWriterBase writer, IList<IDebugItem> items)
        {

            writer.Write(items.Count);

            for (var i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].FetchResultsList().Count);
                for (var j = 0; j < items[i].FetchResultsList().Count; j++)
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

        }

        static void Deserialize(IByteReaderBase reader, ICollection<IDebugItem> items)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var item = new DebugItem();
                var resultCount = reader.ReadInt32();
                for (var j = 0; j < resultCount; j++)
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


        public bool IsFinalStep()
        {
            return StateType == StateType.End && OriginalInstanceID == ID && (!ParentID.HasValue || ParentID == Guid.Empty);
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

        [XmlIgnore]
        public List<IDebugState> Children { get; set; }

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
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
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
            if (propertyName?.Equals("ParentID") ?? false)
            {
                if (ParentID == Guid.Empty)
                {
                    ParentID = null;
                }
            }
        }
    }
}
