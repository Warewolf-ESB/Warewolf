/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.PathOperations.Interfaces;

namespace Dev2.Simulation
{
    /// <summary>
    ///     Defines the requirements for a simulation result
    /// </summary>
    public interface ISimulationResult : IRepositoryItem<ISimulationKey>, IEquatable<ISimulationResult>
    {
        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        IBinaryDataList Value { get; set; }
    }
}