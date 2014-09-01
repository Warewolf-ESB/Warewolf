using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    /// Defines the requirements for a class whose state can be written to a <see cref="IDebugWriter"/>
    /// </summary>
    
    // ReSharper disable InconsistentNaming
    public interface IDebugState : IEquatable<IDebugState>, IXmlSerializable
    {
        /// <summary>
        /// Gets or sets the workspace ID.
        /// </summary>

        Guid WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// Gets or sets the parent ID.
        /// </summary>
        Guid ParentID { get; set; }

        /// <summary>
        /// Gets or sets the type of the state.
        /// </summary>
        StateType StateType { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the activity version.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is simulation.
        /// </summary>
        bool IsSimulation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has an error.
        /// </summary>
        bool HasError { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// Gets or sets the server ID.
        /// </summary>
        Guid ServerID { get; set; }

        /// <summary>
        /// Gets or sets the environment ID.
        /// </summary>
        Guid EnvironmentID { get; set; }

        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        Guid ClientID { get; set; }

        /// <summary>
        /// Gets or sets the server ID.
        /// </summary>
        Guid OriginatingResourceID { get; set; }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        List<IDebugItem> Inputs { get; }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        List<IDebugItem> Outputs { get; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        DateTime EndTime { get; set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Writes this instance to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to which this instance is written.</param>
        void Write(IByteWriterBase writer);

        string Message { get; set; }

        Guid OriginalInstanceID { get; set; }

        int NumberOfSteps { get; set; }

        ExecutionOrigin ExecutionOrigin { get; set; }

        string ExecutionOriginDescription { get; set; }

        string ExecutingUser { get; set; }

        string Origin { get; }

        Guid SessionID { get; set; }
        Guid WorkSurfaceMappingId { get; set; }
        bool IsFinalStep();

        bool IsFirstStep();


    }
}
