using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Interfaces;

namespace Dev2.DataList.Contract.Builders
{
    public interface IDev2DataListUpsertPayloadBuilder<T>
    {

        /// <summary>
        /// Flushes the iteration frame.
        /// </summary>
        void FlushIterationFrame();

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
    }
}
