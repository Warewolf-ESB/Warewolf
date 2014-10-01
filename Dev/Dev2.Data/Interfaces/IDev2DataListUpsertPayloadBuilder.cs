
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;

namespace Dev2.DataList.Contract.Builders
{
    public interface IDev2DataListUpsertPayloadBuilder<T>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the is Debug.
        /// </summary>
        /// <value>
        /// If the execution is in debug.
        /// </value>
        bool IsDebug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attach debug from expression].
        /// </summary>
        /// <value>
        /// <c>true</c> if [attach debug from expression]; otherwise, <c>false</c>.
        /// </value>
        bool AttachDebugFromExpression { get; set; }

        /// <summary>
        /// Gets or sets the resource ID.
        /// </summary>
        /// <value>
        /// The resource ID.
        /// </value>
        Guid ResourceID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has live flushing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has live flushing; otherwise, <c>false</c>.
        /// </value>
        bool HasLiveFlushing { get; set; }

        /// <summary>
        /// Gets or sets the live flushing location.
        /// </summary>
        /// <value>
        /// The live flushing location.
        /// </value>
        Guid LiveFlushingLocation { get; set; }

        /// <summary>
        /// Gets or sets the debug outputs.
        /// </summary>
        /// <value>
        /// The list of DebugOutputTO's.
        /// </value>
        IList<DebugTO> DebugOutputs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [record set data as CSV to scalar].
        /// </summary>
        /// <value>
        /// <c>true</c> if [record set data as CSV to scalar]; otherwise, <c>false</c>.
        /// </value>
        bool RecordSetDataAsCSVToScalar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [replace star with fixed index].
        /// </summary>
        /// <value>
        /// <c>true</c> if [replace star with fixed index]; otherwise, <c>false</c>.
        /// </value>
        bool ReplaceStarWithFixedIndex { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Flushes the iteration frame.
        /// </summary>
        /// <param name="finalFlush">if set to <c>true</c> [final flush].</param>
        void FlushIterationFrame(bool finalFlush = true);

        /// <summary>
        /// Adds the expression.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="val">The val.</param>
        bool Add(string exp, T val);

        /// <summary>
        /// Determines whether this instance has data.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has data; otherwise, <c>false</c>.
        /// </returns>
        bool HasData();

        /// <summary>
        /// Fetches the frames.
        /// </summary>
        /// <param name="forceFlush">if set to <c>true</c> [force flush].</param>
        /// <returns></returns>
        IList<IDataListPayloadIterationFrame<T>> FetchFrames(bool forceFlush = true);

        /// <summary>
        /// Determines whether [is iterative payload].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is iterative payload]; otherwise, <c>false</c>.
        /// </returns>
        bool IsIterativePayload();

        /// <summary>
        /// Flushes the live iteration data.
        /// </summary>
        void PublishLiveIterationData();

        #endregion
    }
}
