#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    ///     Defines the requirements for a class whose state can be written to a <see cref="IDebugWriter" />
    /// </summary>

    
    public interface IDebugState : IEquatable<IDebugState>
    {
        /// <summary>
        ///     Gets or sets the workspace ID.
        /// </summary>
        Guid WorkspaceID { get; set; }

        /// <summary>
        ///     Gets or sets the ID.
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        ///     Gets or sets the ID.
        /// </summary>
        Guid DisconnectedID { get; set; }


        /// <summary>
        ///     Gets or sets the parent ID.
        /// </summary>
        Guid? ParentID { get; set; } 
        
        /// <summary>
        ///     Gets or sets the Source Resource ID.
        /// </summary>
        Guid SourceResourceID { get; set; }

        /// <summary>
        ///     Gets or sets the type of the state.
        /// </summary>
        StateType StateType { get; set; }

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the activity.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Gets or sets the type of the activity.
        /// </summary>
        ActivityType ActivityType { get; set; }

        /// <summary>
        ///     Gets or sets the activity version.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is simulation.
        /// </summary>
        bool IsSimulation { get; set; }

        bool IsAdded { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has an error.
        /// </summary>
        bool HasError { get; set; }

        /// <summary>
        ///     Gets or sets the error message
        /// </summary>
        string ErrorMessage { get; set; }

        /// <summary>
        ///     Gets or sets the server.
        /// </summary>
        string Server { get;  set; }

        /// <summary>
        ///     Gets or sets the server ID.
        /// </summary>
        Guid ServerID { get; set; }

        /// <summary>
        ///     Gets or sets the environment ID.
        /// </summary>
        Guid EnvironmentID { get; set; }

        /// <summary>
        ///     Gets or sets the client ID.
        /// </summary>
        Guid ClientID { get; set; }

        /// <summary>
        ///     Gets or sets the server ID.
        /// </summary>
        Guid OriginatingResourceID { get; set; }

        /// <summary>
        ///     Gets the inputs.
        /// </summary>
        List<IDebugItem> Inputs { get; }

        /// <summary>
        ///     Gets the outputs.
        /// </summary>
        List<IDebugItem> Outputs { get; }
        
        /// <summary>
        ///     Gets the outputs.
        /// </summary>
        List<IDebugItem> AssertResultList { get; }

        /// <summary>
        ///     Gets or sets the start time.
        /// </summary>
        DateTime StartTime { get; set; }

        /// <summary>
        ///     Gets or sets the end time.
        /// </summary>
        /// <value>
        ///     The end time.
        /// </value>
        DateTime EndTime { get; set; }

        /// <summary>
        ///     Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

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

        bool IsDurationVisible { get; set; }
        string ActualType  { get; set; }

        List<IDebugState> Children { get; set; }
    }
}