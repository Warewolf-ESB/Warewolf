using Dev2.DataList.Contract.Network;
using Dev2.DataList.Contract.Persistence;

namespace Dev2.Data.DataListCache {
    public class DataListPersistenceProviderFactory
    {
        private static volatile IDataListPersistenceProvider _memoryProvider;
        private static object _memoryProviderGuard = new object();

        public static IDataListPersistenceProvider CreateMemoryProvider()
        {
            if (_memoryProvider == null)
            {
                lock (_memoryProviderGuard)
                {
                    if (_memoryProvider == null)
                    {
                        _memoryProvider = new DataListTemporalProvider();
                        _memoryProvider.InitPersistence();
                    }
                }
            }

            return _memoryProvider;
        }

        public static IDataListPersistenceProvider CreateServerProvider(INetworkDataListChannel channel)
        {
            IDataListPersistenceProvider provider = new DataListChannelProvider(channel);
            provider.InitPersistence();
            return provider;
        }
    }
}
