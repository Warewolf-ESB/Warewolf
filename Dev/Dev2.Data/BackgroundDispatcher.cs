
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Concurrent;
using System.Threading;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data
{
    public class BackgroundDispatcher
    {
        // The Guid is the workspace ID of the writer
        private static readonly ConcurrentQueue<IBinaryDataList> _binaryDataListQueue = new ConcurrentQueue<IBinaryDataList>();
        private static readonly Thread _waitThread = new Thread(WriteLoop);
        private static readonly ManualResetEventSlim _writeWaithandle = new ManualResetEventSlim(false);
        private static readonly object _waitHandleGuard = new object();
        private static bool _shutdownRequested;

        #region Singleton Instance

        static BackgroundDispatcher _instance;
        public static BackgroundDispatcher Instance
        {
            get
            {
                return _instance ?? (_instance = new BackgroundDispatcher());
            }
        }

        #endregion

        #region Initialization

        static BackgroundDispatcher()
        {
            _waitThread.IsBackground = true;
            _waitThread.Start();
        }

        // Prevent instantiation
        BackgroundDispatcher()
        {

        }

        #endregion

        #region Properties


        #endregion

        #region Shutdown

        public void Shutdown()
        {
            _shutdownRequested = true;
            _writeWaithandle.Set(); // Maybe ?? Might cause more shit
        }

        #endregion Shutdown

        #region Write

        /// <summary>
        /// Writes the given state to the <see cref="IBinaryDataList" /> registered for the given workspace.
        /// <remarks>
        /// This must implement the one-way (fire and forget) message exchange pattern.
        /// </remarks>
        /// </summary>
        /// <param name="binaryDataList">The state to be written.</param>
        /// <returns>The task that was created.</returns>
        public void Add(IBinaryDataList binaryDataList)
        {
            if(binaryDataList != null)
            {
                lock(_waitHandleGuard)
                {
                    _binaryDataListQueue.Enqueue(binaryDataList);
                    _writeWaithandle.Set();
                }
            }
        }

        #endregion

        #region WriteLoop

        private static void WriteLoop()
        {
            while(true)
            {
                _writeWaithandle.Wait();

                if(_shutdownRequested)
                {
                    return;
                }

                IBinaryDataList binaryDataList;

                if(_binaryDataListQueue.TryDequeue(out binaryDataList))
                {
                    binaryDataList.Dispose();
                }

                lock(_waitHandleGuard)
                {
                    if(_binaryDataListQueue.Count == 0)
                    {
                        _writeWaithandle.Reset();
                    }
                }
            }
        }

        #endregion WriteLoop
    }
}
