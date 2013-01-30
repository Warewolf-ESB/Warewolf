using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Interfaces;
using Dev2.DataList.Contract.TO;

namespace Dev2.DataList.Contract.Builders
{

    /// <summary>
    /// Frames an activities iteration scope allowing for complex DL ops
    /// </summary>
    internal class PayloadIterationFrame<T> : IDataListPayloadIterationFrame<T>
    {
        private readonly IList<DataListPayloadFrameTO<T>> _cache = new List<DataListPayloadFrameTO<T>>();
        private int _idx = 0;

        internal PayloadIterationFrame(){
        }

        public bool Add(string exp, T val)
        {
            bool result = false;
            
            if (val != null && exp != null && exp != string.Empty)
            {
                _cache.Add(new DataListPayloadFrameTO<T>(exp, val));
                result = true;
            }

            return result;
        }

        public DataListPayloadFrameTO<T> FetchNextFrameItem()
        {
            return _cache[_idx++];
        }

        public bool HasData()
        {
            return (_idx < _cache.Count);
        }
    }
    
    /// <summary>
    /// This class is responsible for building up the Upsert payloads [ expressions and values ]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Dev2DataListUpsertPayloadBuilder<T> : IDev2DataListUpsertPayloadBuilder<T>
    {

        private readonly IList<IDataListPayloadIterationFrame<T>> _data = new List<IDataListPayloadIterationFrame<T>>();
        private IDataListPayloadIterationFrame<T> _scopedFrame = new PayloadIterationFrame<T>();
        private readonly bool _iterativePayload;

        internal Dev2DataListUpsertPayloadBuilder(bool iterativePayload)
        {
            _iterativePayload = iterativePayload;
        }

        /// <summary>
        /// Flushes the iteration payload.
        /// </summary>
        public void FlushIterationFrame()
        {
            _data.Add(_scopedFrame);
            _scopedFrame = new PayloadIterationFrame<T>();
        }

        /// <summary>
        /// Adds the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public bool Add(string exp, T val)
        {
            bool result = false;

            result = _scopedFrame.Add(exp, val);
            
            return result;
        }

        /// <summary>
        /// Fetches the frames.
        /// </summary>
        /// <returns></returns>
        public IList<IDataListPayloadIterationFrame<T>> FetchFrames(bool forceFlush = true)
        {
            // Make sure to flush if we are gretting the frames
            if (_scopedFrame.HasData() && forceFlush)
            {
                FlushIterationFrame();
            }
            return _data;
        }

        /// <summary>
        /// Determines whether this instance has data.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has data; otherwise, <c>false</c>.
        /// </returns>
        public bool HasData()
        {
            return (_data.Count > 0);
        }

        public bool IsIterativePayload()
        {
            return _iterativePayload;
        }
    }
}
