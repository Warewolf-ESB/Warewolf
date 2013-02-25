using System;
using System.Collections.Generic;
using Dev2.DataList.Contract.Interfaces;
using Dev2.DataList.Contract.TO;
using Dev2.Common;

namespace Dev2.DataList.Contract.Builders
{

    /// <summary>
    /// Frames an activities iteration scope allowing for complex DL ops
    /// </summary>
    public  class PayloadIterationFrame<T> : IDataListPayloadIterationFrame<T>
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
        private LiveFlushIterator _flushIterator;
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance has live flushing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has live flushing; otherwise, <c>false</c>.
        /// </value>
        public bool HasLiveFlushing { get; set; }

        /// <summary>
        /// Gets or sets the live flushing location.
        /// </summary>
        /// <value>
        /// The live flushing location.
        /// </value>
        public Guid LiveFlushingLocation { get; set; }

        internal Dev2DataListUpsertPayloadBuilder(bool iterativePayload)
        {
            _iterativePayload = iterativePayload;
        }

        /// <summary>
        /// Flushes the iteration payload.
        /// </summary>
        public void FlushIterationFrame(bool terminalFlush = false)
        {
            if(!HasLiveFlushing)
            {
                _data.Add(_scopedFrame);
                _scopedFrame = new PayloadIterationFrame<T>();
            }
            else
            {
                if (_flushIterator == null && LiveFlushingLocation != GlobalConstants.NullDataListID)
                {
                    _flushIterator = new LiveFlushIterator(LiveFlushingLocation);
                }

                _flushIterator.FlushIterations((_scopedFrame as PayloadIterationFrame<string>), IsIterativePayload(), terminalFlush);

                _scopedFrame = new PayloadIterationFrame<T>();
            }
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
            // Make sure to flush if we are getting the frames
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

        public void PublishLiveIterationData()
        {
            if (HasLiveFlushing && _flushIterator != null)
            {
                _flushIterator.PublishLiveIterationData();
            }
        }

    }
}
