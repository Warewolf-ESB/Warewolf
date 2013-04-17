using System;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Network.Channels
{
    public class DataListClientChannel : INetworkDataListChannel, IDisposable
    {
        readonly IEnvironmentConnection _connection;
        volatile bool _isDisposed;

        #region CTOR

        public DataListClientChannel(IEnvironmentConnection connection)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            _connection = connection;
        }

        #endregion

        public Guid ServerID { get { return _connection.ServerID; } }

        #region Implementation of INetworkDataListChannel

        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors)
        {
            if(_isDisposed)
            {
                return false;
            }

            if(datalist == null)
            {
                throw new ArgumentNullException("datalist");
            }

            var message = new WriteDataListMessage
            {
                DatalistID = datalistID,
                Datalist = datalist,
                Errors = errors,
            };

            var rawResultMessage = SendReceiveNetworkMessage(message);
            var resultMessage = rawResultMessage as WriteDataListResultMessage;

            var result = false;

            if(resultMessage != null)
            {
                result = resultMessage.Result;
                UpdateErrorResultTO(errors, resultMessage.Errors);
            }
            else
            {
                UpdateErrorResultTO(errors, rawResultMessage);
            }

            return result;
        }

        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors)
        {
            if(_isDisposed)
            {
                return null;
            }

            var message = new ReadDataListMessage
            {
                DatalistID = datalistID,
                Errors = errors,
            };

            var rawResultMessage = SendReceiveNetworkMessage(message);
            var resultMessage = rawResultMessage as ReadDataListResultMessage;

            IBinaryDataList result = null;

            if(resultMessage != null)
            {
                result = resultMessage.Datalist;
                UpdateErrorResultTO(errors, resultMessage.Errors);
            }
            else
            {
                UpdateErrorResultTO(errors, rawResultMessage);
            }

            return result;
        }

        public void DeleteDataList(Guid datalistID, bool onlyIfNotPersisted)
        {
            if(_isDisposed)
            {
                return;
            }

            var message = new DeleteDataListMessage
            {
                ID = datalistID,
                OnlyIfNotPersisted = onlyIfNotPersisted,
            };

            SendReceiveNetworkMessage(message);
        }

        public bool PersistChildChain(Guid datalistID)
        {
            if(_isDisposed)
            {
                return false;
            }

            var message = new PersistChildChainMessage
            {
                ID = datalistID,
            };

            var rawResultMessage = SendReceiveNetworkMessage(message);
            var resultMessage = rawResultMessage as PersistChildChainResultMessage;

            var result = false;

            if(resultMessage != null)
            {
                result = resultMessage.Result;
            }

            return result;
        }

        #endregion

        #region UpdateErrorResultTO

        static void UpdateErrorResultTO(ErrorResultTO errors, INetworkMessage message)
        {
            var errorMessage = message as ErrorMessage;
            if(errorMessage != null)
            {
                var errorTO = new ErrorResultTO();
                errorTO.AddError(errorMessage.Message);
                UpdateErrorResultTO(errors, errorTO);
            }
        }

        static void UpdateErrorResultTO(ErrorResultTO destination, ErrorResultTO source)
        {
            if(destination == null || source == null)
            {
                return;
            }

            destination.ClearErrors();
            if(source.HasErrors())
            {
                foreach(var error in source.FetchErrors())
                {
                    destination.AddError(error);
                }
            }
        }

        #endregion

        #region SendReceiveNetworkMessage

        INetworkMessage SendReceiveNetworkMessage(INetworkMessage message)
        {
            var result = _connection.SendReceiveNetworkMessage(message);
            return result;
        }

        #endregion

        #region Implementation of IDisposable

        ~DataListClientChannel()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion

    }
}
