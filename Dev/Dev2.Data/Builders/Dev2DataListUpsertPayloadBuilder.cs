
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
using System.Linq;
using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.TO;

namespace Dev2.Data.Builders
{

    /// <summary>
    /// Frames an activities iteration scope allowing for complex DL ops
    /// </summary>
    public class PayloadIterationFrame<T> : IDataListPayloadIterationFrame<T>
    {
        private readonly IList<DataListPayloadFrameTO<T>> _cache = new List<DataListPayloadFrameTO<T>>();
        private int _idx;

        internal PayloadIterationFrame()
        {
        }

        public bool Add(string exp, T val)
        {
            bool result = false;

            // ReSharper disable CompareNonConstrainedGenericWithNull
            //val != null &&
            if(!string.IsNullOrEmpty(exp))
            // ReSharper restore CompareNonConstrainedGenericWithNull
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
    /// Used to trace StarToFixedIndex operations ;)
    /// </summary>
    internal class UpsertPayloadBuilderIndexIterator
    {
        readonly IDictionary<string, int> _keys = new Dictionary<string, int>(5);

        public void MoveIndexesForward()
        {

            var theKeys = _keys.Keys.ToArray();

            foreach(var key in theKeys)
            {
                _keys[key]++;
            }
        }

        public int FetchCurrentIndex(string key)
        {
            int result;

            // we need to init it on a cache miss ;)
            if(!_keys.TryGetValue(key, out result))
            {
                AddIfNotPresent(key);
                result = 1;
            }

            return result;
        }

        private bool ContainsKey(string key)
        {
            return _keys.ContainsKey(key);
        }

        private void AddIfNotPresent(string key)
        {
            if(!ContainsKey(key))
            {
                _keys[key] = 1;
            }
        }
    }

    /// <summary>
    /// This class is responsible for building up the Upsert payloads [ expressions and values ]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Dev2DataListUpsertPayloadBuilder<T> : IDev2DataListUpsertPayloadBuilder<T>
    {
        private IList<DebugTO> _debugOutputs = new List<DebugTO>();
        private readonly IList<IDataListPayloadIterationFrame<T>> _data = new List<IDataListPayloadIterationFrame<T>>();
        private IDataListPayloadIterationFrame<T> _scopedFrame = new PayloadIterationFrame<T>();
        private readonly bool _iterativePayload;
        private LiveFlushIterator _flushIterator;

        /// <summary>
        /// Gets or sets the is Debug.
        /// </summary>
        /// <value>
        /// If the execution is in debug.
        /// </value>
        public bool IsDebug { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attach debug from expression].
        /// </summary>
        /// <value>
        /// <c>true</c> if [attach debug from expression]; otherwise, <c>false</c>.
        /// </value>
        public bool AttachDebugFromExpression { get; set; }

        public Guid ResourceID { get; set; }

        /// <summary>
        /// Gets or sets the debug outputs.
        /// </summary>
        /// <value>
        /// The list of DebugOutputTO's.
        /// </value>
        public IList<DebugTO> DebugOutputs
        {
            get
            {
                return _debugOutputs;
            }
            set
            {
                _debugOutputs = value;
            }
        }

        public bool RecordSetDataAsCSVToScalar { get; set; }

        // Fields used to achieve the ReplaceStarWithFixedIndex functionality
        private UpsertPayloadBuilderIndexIterator _idxScope;
        private bool _replaceStar;
        public bool ReplaceStarWithFixedIndex
        {
            get { return _replaceStar; }

            set
            {

                _replaceStar = value;
                if(_idxScope == null)
                {
                    _idxScope = new UpsertPayloadBuilderIndexIterator();
                }
            }
        }

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
                if(_flushIterator == null && LiveFlushingLocation != GlobalConstants.NullDataListID)
                {
                    _flushIterator = new LiveFlushIterator(LiveFlushingLocation);
                }

                if(_flushIterator != null)
                {
                    _flushIterator.FlushIterations((_scopedFrame as PayloadIterationFrame<string>), IsIterativePayload(),
                                                   terminalFlush);
                }
                else
                {
                    throw new Exception("Error : Null Frame Iterator");
                }

                _scopedFrame = new PayloadIterationFrame<T>();
            }

            // move the internal index iterators ;)
            if(ReplaceStarWithFixedIndex)
            {
                _idxScope.MoveIndexesForward();
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
            // In select cases is is important to replace (*) with (x) where X start @ 1 and moves up each flush ;)
            if(ReplaceStarWithFixedIndex)
            {
                var idx = _idxScope.FetchCurrentIndex(exp);
                exp = DataListUtil.ReplaceStarWithFixedIndex(exp, idx);
            }

            bool result = _scopedFrame.Add(exp, val);

            return result;
        }

        public void Add(T recordSetGroup)
        {
            _scopedFrame.Add("XXX", recordSetGroup);
        }

        /// <summary>
        /// Fetches the frames.
        /// </summary>
        /// <returns></returns>
        public IList<IDataListPayloadIterationFrame<T>> FetchFrames(bool forceFlush = true)
        {
            // Make sure to flush if we are getting the frames
            if(_scopedFrame.HasData() && forceFlush)
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
            return (_data.Count > 0 || _scopedFrame.HasData());
        }

        public bool IsIterativePayload()
        {
            return _iterativePayload;
        }

        public void PublishLiveIterationData()
        {
            if(HasLiveFlushing && _flushIterator != null)
            {
                _flushIterator.PublishLiveIterationData();
            }
        }
    }
}
