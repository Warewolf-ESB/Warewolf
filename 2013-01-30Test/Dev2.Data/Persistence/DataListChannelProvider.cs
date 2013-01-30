using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;

namespace Dev2.DataList.Contract.Persistence
{
    public class DataListChannelProvider : IDataListPersistenceProvider, IDisposable
    {
        #region Class Members

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        private INetworkDataListChannel _dataListChannel;

        #endregion Class Members

        #region Constructors

        public DataListChannelProvider(INetworkDataListChannel datalistChannel)
        {
            if (datalistChannel == null)
            {
                throw new ArgumentNullException("datalistChannel");
            }

            _dataListChannel = datalistChannel;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Writes the data list.
        /// </summary>
        /// <param name="datalistID">The datalist ID.</param>
        /// <param name="datalist">The datalist.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="System.ArgumentNullException">datalist</exception>
        /// <exception cref="System.InvalidOperationException">Provider is disposing.</exception>
        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors)
        {
            if (datalist == null)
            {
                throw new ArgumentNullException("datalist");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Provider is disposing.");
                }
            }

            if (errors == null)
            {
                errors = new ErrorResultTO();
            }

            return _dataListChannel.WriteDataList(datalistID, datalist, errors);
        }

        /// <summary>
        /// Reads the datalist.
        /// </summary>
        /// <param name="datalistID">The datalist ID.</param>
        /// <param name="errors">The errors.</param>
        /// <exception cref="System.InvalidOperationException">Provider is disposing.</exception>
        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Provider is disposing.");
                }
            }

            if (errors == null)
            {
                errors = new ErrorResultTO();
            }

            return _dataListChannel.ReadDatalist(datalistID, errors);
        }

        /// <summary>
        /// Deletes the data list.
        /// </summary>
        /// <param name="id">The datalist ID.</param>
        /// <param name="onlyIfNotPersisted">if set to <c>true</c> [only if not persisted].</param>
        /// <exception cref="System.InvalidOperationException">Provider is disposing.</exception>
        public void DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Provider is disposing.");
                }
            }

            _dataListChannel.DeleteDataList(id, onlyIfNotPersisted);
        }

        /// <summary>
        /// Persists the child chain.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <exception cref="System.InvalidOperationException">Provider is disposing.</exception>
        public bool PersistChildChain(Guid id)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Provider is disposing.");
                }
            }

            return _dataListChannel.PersistChildChain(id);
        }

        /// <summary>
        /// Initializes the persistence.
        /// </summary>
        public void InitPersistence()
        {
            //Nothing to do here
        }

        #endregion Methods

        #region Tear Down

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }

                _dataListChannel = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
