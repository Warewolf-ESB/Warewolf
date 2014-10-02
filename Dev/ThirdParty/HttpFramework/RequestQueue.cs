
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
using System.Threading;

namespace HttpFramework
{
	/// <summary>
	/// Used to queue incoming requests.
	/// </summary>
	class RequestQueue
	{
		private int _maxRequestCount = 25;
		private int _currentRequestCount;
		private int _maxQueueSize = 50;
		private readonly Queue<QueueItem> _queue = new Queue<QueueItem>();
		private readonly ManualResetEvent _event = new ManualResetEvent(false);
		private bool _canRun;
		private Thread _workerThread;
		private readonly ProcessRequestHandler _handler;

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestQueue"/> class.
		/// </summary>
		/// <param name="handler">Called when a request should be processed.</param>
		public RequestQueue(ProcessRequestHandler handler)
		{
			_handler = handler;

		}
		/// <summary>
		/// Used two queue incoming requests to avoid
		/// thread starvation.
		/// </summary>
		private class QueueItem
		{
			public IHttpClientContext Context { get; set; }
			public IHttpRequest Request { get; set; }
		}

		/// <summary>
		/// Gets or sets maximum number of allowed simultaneous requests.
		/// </summary>
		public int MaxRequestCount
		{
			get { return _maxRequestCount; }
			set { _maxRequestCount = value; }
		}

		/// <summary>
		/// Gets or sets maximum number of requests queuing to be handled.
		/// </summary>
		public int MaxQueueSize
		{
			get { return _maxQueueSize; }
			set { _maxQueueSize = value; }
		}

        /// <summary>
        /// Increment the <see cref="CurrentRequestCount"/> by 1 in a thread safe manor.
        /// </summary>
        internal void SafeIncrementRequestCount()
        {
            Interlocked.Increment(ref _currentRequestCount);
        }

        /// <summary>
        /// Decrement the <see cref="CurrentRequestCount"/> by 1 in a thread safe manor.
        /// </summary>
        internal void SafeDecrementRequestCount()
        {
            Interlocked.Decrement(ref _currentRequestCount);
        }


		/// <summary>
		/// Specifies how many requests the HTTP server is currently processing.
		/// </summary>
		internal int CurrentRequestCount
		{
			get { return _currentRequestCount; }
            /* Setter removed in favour of a thread safe increment/decrement.
            set { _currentRequestCount = value; }
             */
		}

		internal bool ShouldQueue
		{
			get
			{
				return _currentRequestCount > _maxRequestCount;
			}
		}
		public void Enqueue(IHttpClientContext context, IHttpRequest request)
		{
			QueueItem item = new QueueItem { Context = context, Request = request };
			lock (_queue)
				_queue.Enqueue(item);
		}



		public void Start()
		{
			_canRun = true;
			if (_workerThread == null)
			{
                _workerThread = new Thread(QueueThread);
			    _workerThread.IsBackground = true;
            }


			if (!_workerThread.IsAlive)
				_workerThread.Start();
		}

		public void Stop()
		{
			// shut down worker thread.
			_canRun = false;
			_event.Set();
			// Check added by Albert, Team MediaPortal
			if (_workerThread != null)
			{
				if (!_workerThread.Join(5000))
					_workerThread.Abort();
				_workerThread = null;
			}
			_workerThread = null;
		}


		/// <summary>
		/// Used to process queued requests.
		/// </summary>
		private void QueueThread()
		{
			try
			{
				while (_canRun)
				{
					_event.WaitOne(Timeout.Infinite, true);
					if (!_canRun)
						break;

					if (CurrentRequestCount > MaxRequestCount)
					{
						_event.Reset();
						continue;
					}

					QueueItem item;
					lock (_queue)
					{
						if (_queue.Count == 0)
						{
							_event.Reset();
							continue;
						}

						item = _queue.Dequeue();
					}

					ThreadPool.QueueUserWorkItem(ProcessRequest, item);
				}
			}
			catch (ThreadAbortException)
			{

			}
		}

		private void ProcessRequest(object state)
		{
			QueueItem item = (QueueItem)state;
			_handler(item.Context, item.Request);
		}

	    public void Trigger()
	    {
	        _event.Set();
	    }
	}

	/// <summary>
	/// Method used to process a queued request
	/// </summary>
	/// <param name="context">Context that the request was received from.</param>
	/// <param name="request">Request to process.</param>
	public delegate void ProcessRequestHandler(IHttpClientContext context, IHttpRequest request);
}
