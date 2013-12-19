using Dev2.DataList.Contract.Network;
using Dev2.DataList.Contract.Persistence;

namespace Dev2.Data.DataListCache {
    public class DataListPersistenceProviderFactory
    {
        private static volatile IDataListPersistenceProvider _memoryProvider;
        private static object _memoryProviderGuard = new object();

        /// <summary>
        /// Creates the memory provider.
        /// </summary>
        /// <returns></returns>
        public static IDataListPersistenceProvider CreateMemoryProvider()
        {
            if (_memoryProvider == null)
            {
                lock (_memoryProviderGuard)
                {
                    if (_memoryProvider == null)
                    {
                        _memoryProvider = new DataListTemporalProvider();
                    }
                }
            }

            return _memoryProvider;
        }

        /// <summary>
        /// Creates the server provider.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns></returns>
        public static IDataListPersistenceProvider CreateServerProvider(INetworkDataListChannel channel)
        {
            IDataListPersistenceProvider provider = new DataListChannelProvider(channel);

            return provider;
        }
    }
}
