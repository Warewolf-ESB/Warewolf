using Dev2.Common;
using Dev2.Common.Interfaces.Logging;
using Dev2.Runtime.ESB.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dev2
{
    public class LogFlusherWorker : IServerLifecycleWorker, IDisposable
    {
        Timer _loggerFlushTimer;
        readonly ILogManager _logManager;

        public int TimerDueTime { get; set; } = 10000;

        public LogFlusherWorker(ILogManager logManager)
        {
            _logManager = logManager;
        }

        private void ConfigureLogFlushing()
        {
            if (Config.Server.EnableDetailedLogging)
            {
                Config.Server.OnLogFlushPauseRequested += PerformLogFlushTimerPause;
                Config.Server.OnLogFlushResumeRequested += PerformLogFlushTimerResume;
                _loggerFlushTimer = new Timer(PerformLoggerFlushActions, null, TimerDueTime, Config.Server.LogFlushInterval);
            }
        }

        long _flushing;
        public void PerformLoggerFlushActions(object state)
        {
            if (Interlocked.Exchange(ref _flushing, 1) != 0)
            {
                return;
            }

            try
            {
                _logManager.FlushLogs();
            }
            catch (Exception e)
            {
                //
            }
            finally
            {
                Interlocked.Decrement(ref _flushing);
            }
        }
        void PerformLogFlushTimerPause()
        {
            _loggerFlushTimer.Change(0, Timeout.Infinite);
            while (Interlocked.Read(ref _flushing) > 0)
            {
                Thread.Sleep(100);
            }
        }
        void PerformLogFlushTimerResume()
        {
            _loggerFlushTimer.Change(TimerDueTime, Config.Server.LogFlushInterval);
        }

        public void Execute()
        {
            ConfigureLogFlushing();
        }

        public void Dispose()
        {
            _loggerFlushTimer.Dispose();
        }
    }
}
