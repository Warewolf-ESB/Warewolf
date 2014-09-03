namespace Dev2.Common.Interfaces
{
    public static class ServerStats
    {

        private static int _totalRequests=0;
        public static int TotalRequests { get{return _totalRequests;} }
        private static long _totalTime = 0;
        public static long TotalTime { get { return _totalTime; } }
        public static void IncrementTotalRequests()
        {
            System.Threading.Interlocked.Increment(ref _totalRequests);
        }

        public static void IncrementTotalTime( long time)
        {
            System.Threading.Interlocked.Add(ref _totalTime,time);
        }
    }
}
