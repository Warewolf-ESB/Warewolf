using log4net.Core;
using System;
using System.Threading;

namespace Log4Net.Async
{
    public sealed class AsyncForwardingAppender : AsyncForwardingAppenderBase
    {
        private static readonly TimeSpan ShutdownFlushTimeout = TimeSpan.FromSeconds(5);
        private static readonly Type ThisType = typeof(AsyncForwardingAppender);
        private const int DefaultBufferSize = 1000;

        private Thread forwardingThread;
        private volatile bool shutDownRequested;

        private readonly object bufferLock = new object();
        private RingBuffer<LoggingEventContext> buffer;

        private bool logBufferOverflow;
        private int bufferOverflowCounter;
        private DateTime lastLoggedBufferOverflow;

        private int bufferSize = DefaultBufferSize;

        public override int BufferSize
        {
            get { return bufferSize; }
            set { SetBufferSize(value); }
        }

        protected override string InternalLoggerName
        {
            get
            {
                return "AsyncForwardingAppender";
            }
        }

        #region Startup

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            InitializeBuffer();
            StartForwarding();
        }

        private void StartForwarding()
        {
            if (shutDownRequested)
            {
                return;
            }

            forwardingThread = new Thread(ForwardingThreadExecute)
            {
                Name = String.Format("{0} Forwarding Appender Thread", Name),
                IsBackground = false,
            };
            forwardingThread.Start();
        }

        #endregion Startup

        #region Shutdown

        protected override void OnClose()
        {
            StopForwarding();
            base.OnClose();
        }

        private void StopForwarding()
        {
            shutDownRequested = true;
            var hasFinishedFlushingBuffer = forwardingThread.Join(ShutdownFlushTimeout);

            if (!hasFinishedFlushingBuffer)
            {
                forwardingThread.Abort();
                ForwardInternalError("Unable to flush the AsyncForwardingAppender buffer in the allotted time, forcing a shutdown", null, ThisType);
            }
        }

        #endregion Shutdown

        #region Appending

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (!shutDownRequested && loggingEvent != null)
            {
                loggingEvent.Fix = Fix;
                buffer.Enqueue(new LoggingEventContext(loggingEvent));
            }
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            if (!shutDownRequested && loggingEvents != null)
            {
                foreach (var loggingEvent in loggingEvents)
                {
                    Append(loggingEvent);
                }
            }
        }

        #endregion Appending

        #region Forwarding

        private void ForwardingThreadExecute()
        {
            while (!shutDownRequested)
            {
                try
                {
                    ForwardLoggingEventsFromBuffer();
                }
                catch (Exception exception)
                {
                    ForwardInternalError("Unexpected error in asynchronous forwarding loop", exception, ThisType);
                }
            }
        }

        private void ForwardLoggingEventsFromBuffer()
        {
            LoggingEventContext loggingEventContext;
            while (!shutDownRequested)
            {
                if (logBufferOverflow)
                {
                    ForwardBufferOverflowError();
                    logBufferOverflow = false;
                }

                while (!buffer.TryDequeue(out loggingEventContext))
                {
                    Thread.Sleep(10);
                    if (shutDownRequested)
                    {
                        break;
                    }
                }

                if (loggingEventContext != null)
                {
                    //HttpContext = loggingEventContext.HttpContext;
                    ForwardLoggingEvent(loggingEventContext.LoggingEvent, ThisType);
                }
            }

            while (buffer.TryDequeue(out loggingEventContext))
            {
                //HttpContext = loggingEventContext.HttpContext;
                ForwardLoggingEvent(loggingEventContext.LoggingEvent, ThisType);
            }
        }

        private void ForwardBufferOverflowError()
        {
            ForwardInternalError(String.Format("Buffer overflow. {0} logging events have been lost in the last 30 seconds. [BufferSize: {1}]", bufferOverflowCounter, bufferSize), null, ThisType);
            lastLoggedBufferOverflow = DateTime.UtcNow;
            bufferOverflowCounter = 0;
        }

        #endregion Forwarding

        #region Buffer Management

        private void SetBufferSize(int newBufferSize)
        {
            lock (bufferLock)
            {
                if (newBufferSize > 0 && newBufferSize != bufferSize)
                {
                    bufferSize = newBufferSize;
                    InitializeBuffer();
                }
            }
        }

        private void InitializeBuffer()
        {
            lock (bufferLock)
            {
                if (buffer == null || buffer.Size != bufferSize)
                {
                    buffer = new RingBuffer<LoggingEventContext>(bufferSize);
                    buffer.BufferOverflow += OnBufferOverflow;
                }
            }
        }

        private void OnBufferOverflow(object sender, EventArgs args)
        {
            bufferOverflowCounter++;

            if (logBufferOverflow)
            {
                return;
            }

            if (lastLoggedBufferOverflow < DateTime.UtcNow.AddSeconds(-30))
            {
                logBufferOverflow = true;
            }
        }

        #endregion Buffer Management
    }
}
