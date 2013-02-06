using System;
using System.Collections.Generic;
using Dev2.Enums;

namespace Dev2.Diagnostics
{
    /// <summary>
    /// Defines the requirements for a class whose state can be written to a <see cref="IDebugWriter"/>
    /// </summary>
    public interface IDebugState
    {
        /// <summary>
        /// Gets or sets the workspace ID.
        /// </summary>
        Guid WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// Gets or sets the parent ID.
        /// </summary>
        string ParentID { get; set; }

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
        /// Gets the inputs.
        /// </summary>
        IList<IDebugItem> Inputs { get; }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        IList<IDebugItem> Outputs { get; }

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
        void Write(IDebugWriter writer);

        /// <summary>
        /// Writes this instance to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to which this instance is written.</param>
        void Write(IByteWriterBase writer);
    }
}
