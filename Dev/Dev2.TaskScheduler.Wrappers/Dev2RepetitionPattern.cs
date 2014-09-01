using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2RepetitionPattern : IRepetitionPattern
    {
        private readonly RepetitionPattern _nativeInstance;

        public Dev2RepetitionPattern(RepetitionPattern nativeInstance)
        {
            _nativeInstance = nativeInstance;
        }

        public void Dispose()
        {
            Instance.Dispose();
        }

        public RepetitionPattern Instance
        {
            get { return _nativeInstance; }
        }

        public TimeSpan Duration
        {
            get { return Instance.Duration; }
            set { Instance.Duration = value; }
        }

        public TimeSpan Interval
        {
            get { return Instance.Interval; }
            set { Instance.Interval = value; }
        }

        public bool StopAtDurationEnd
        {
            get { return Instance.StopAtDurationEnd; }
            set { Instance.StopAtDurationEnd = value; }
        }

        public bool IsSet()
        {
            return Instance.IsSet();
        }
    }
}