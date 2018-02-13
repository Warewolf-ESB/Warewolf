using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using System.IO;
using Dev2.Common.Wrappers;
using Dev2.PathOperations;
using System.Threading;
using System.Security.Principal;
using Dev2.Common.Common;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoPutAction : PerformIntegerIOOperation
    {
        static readonly ReaderWriterLockSlim _fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        WindowsImpersonationContext ImpersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath Destination;
        protected readonly IFile _fileWrapper;
        protected readonly IFilePath _pathWrapper;
        protected readonly IDev2CRUDOperationTO _arguments;
        protected readonly SafeTokenHandle _safeToken;
        protected readonly Stream _currentStream;
        protected readonly string _whereToPut;

        public DoPutAction(Stream currentStream, IActivityIOPath destination, IDev2CRUDOperationTO crudArgument, string whereToPut)
        {
            _logOnProvider = new LogonProvider();
            _pathWrapper = new FilePathWrapper();
            _currentStream = currentStream;
            Destination = destination;
            _arguments = crudArgument;
            _safeToken = RequiresAuth(Destination, _logOnProvider);
            _whereToPut = whereToPut;
        }
        public override int ExecuteOperation()
        {
            var destination = Destination;
            if (!_pathWrapper.IsPathRooted(Destination.Path) && _whereToPut != null)
            {
                destination = ActivityIOFactory.CreatePathFromString(_whereToPut + "\\" + Destination.Path, Destination.Username, Destination.Password, Destination.PrivateKeyFile);
            }
            _fileLock.EnterWriteLock();
            try
            {
                using (_currentStream)
                {
                    if (_safeToken != null)
                    {
                        return ExecuteOperationWithAuth(_currentStream, destination);
                    }
                    return WriteData(_currentStream, destination);
                }
            }
            finally
            {
                _fileLock.ExitWriteLock();
                ImpersonatedUser?.Undo();
            }
        }

        public override int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst)
        {
            using (_safeToken)
            {
                var newID = new WindowsIdentity(_safeToken.DangerousGetHandle());
                using (ImpersonatedUser = newID.Impersonate())
                {
                    return WriteData(src, dst);
                }
            }
        }

        int WriteData(Stream src, IActivityIOPath dst)
        {
            if (FileExist(dst, _fileWrapper) && !_arguments.Overwrite)
            {
                using (var stream = new FileStream(dst.Path, FileMode.Append))
                {
                    src.CopyTo(stream);
                }
                return (int)src.Length;
            }
            if (_arguments.Overwrite)
            {
                File.WriteAllBytes(dst.Path, src.ToByteArray());
                return (int)src.Length;
            }
            return -1;
        }
    }
}
