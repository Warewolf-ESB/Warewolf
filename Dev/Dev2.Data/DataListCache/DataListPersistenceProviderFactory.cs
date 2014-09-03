namespace Dev2.Data.DataListCache
{
    public class DataListPersistenceProviderFactory
    {
        private static volatile IDataListPersistenceProvider _memoryProvider;
        private static readonly object MemoryProviderGuard = new object();

        /// <summary>
        /// Creates the memory provider.
        /// </summary>
        /// <returns></returns>
        public static IDataListPersistenceProvider CreateMemoryProvider()
        {
            if(_memoryProvider == null)
            {
                lock(MemoryProviderGuard)
                {
                    if(_memoryProvider == null)
                    {
                        _memoryProvider = new DataListTemporalProvider();
                    }
                }
            }

            return _memoryProvider;
        }
    }
}
