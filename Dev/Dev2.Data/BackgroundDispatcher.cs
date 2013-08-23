using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data
{
    public class BackgroundDispatcher
    {
        // The Guid is the workspace ID of the writer
        private static ConcurrentQueue<IBinaryDataList> _binaryDataListQueue = new ConcurrentQueue<IBinaryDataList>();
        private static Thread _waitThread = new Thread(WriteLoop);
        private static ManualResetEventSlim _writeWaithandle = new ManualResetEventSlim(false);
        private static object _waitHandleGuard = new object();
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
            lock (_waitHandleGuard)
            {
                IBinaryDataList debugState;
                while (_binaryDataListQueue.Count > 0)
                {
                    _binaryDataListQueue.TryDequeue(out debugState);
                }
                _writeWaithandle.Set();
            }
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
        public Task Add(IBinaryDataList binaryDataList)
        {
            if (binaryDataList == null)
            {
                return null;
            }

            //return Task.Factory.StartNew(() => binaryDataList.Write(writer));

            lock (_waitHandleGuard)
            {
                _binaryDataListQueue.Enqueue(binaryDataList);
                _writeWaithandle.Set();
            }

            Task t = new Task(() => { });
            t.Start();
            return t;
        }

        #endregion

        #region WriteLoop

        private static void WriteLoop()
        {
            while (true)
            {
                _writeWaithandle.Wait();

                if (_shutdownRequested)
                {
                    return;
                }

                IBinaryDataList binaryDataList;

                if (_binaryDataListQueue.TryDequeue(out binaryDataList))
                {
                    binaryDataList.Dispose();
                }

                lock (_waitHandleGuard)
                {
                    if (_binaryDataListQueue.Count == 0)
                    {
                        _writeWaithandle.Reset();
                    }
                }
            }
        }

        #endregion WriteLoop
    }
}