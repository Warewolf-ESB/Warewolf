using System;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoDeleteOperation : PerformBoolIOOperation
    {
        WindowsImpersonationContext ImpersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly SafeTokenHandle _safeToken;
        public DoDeleteOperation(IActivityIOPath path)
        {
            _logOnProvider = new LogonProvider();
            _path = path;
            _safeToken = RequiresAuth(_path, _logOnProvider);
        }
        public override bool ExecuteOperation()
        {
            try
            {
                if (_safeToken != null)
                {
                    return ExecuteOperationWithAuth();
                }
                return DeleteHelper.Delete(_path.Path);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error("Error getting file: " + _path.Path, exception, GlobalConstants.WarewolfError);
                return false;
            }
        }
        public override bool ExecuteOperationWithAuth()
        {
            try
            {                
                using (_safeToken)
                {
                    var newID = new WindowsIdentity(_safeToken.DangerousGetHandle());
                    using (ImpersonatedUser = newID.Impersonate())
                    {
                        return DeleteHelper.Delete(_path.Path);
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, GlobalConstants.Warewolf);
                return false;
            }
            finally
            {
                ImpersonatedUser?.Undo();
            }
        }
    }
}
