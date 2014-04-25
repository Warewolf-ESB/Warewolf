using Dev2.DataList.Contract.Persistence;

namespace Dev2.Data.DataListCache
{
    public class DataListPersistenceProviderFactory
    {
        private static volatile IDataListPersistenceProvider _memoryProvider;
        private static readonly object _memoryProviderGuard = new object();

        /// <summary>
        /// Creates the memory provider.
        /// </summary>
        /// <returns></returns>
        public static IDataListPersistenceProvider CreateMemoryProvider()
        {
            if(_memoryProvider == null)
            {
                lock(_memoryProviderGuard)
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
