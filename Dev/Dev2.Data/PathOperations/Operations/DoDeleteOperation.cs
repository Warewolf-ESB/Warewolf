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
        readonly WindowsImpersonationContext ImpersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        public DoDeleteOperation(IActivityIOPath path)
        {
            _logOnProvider = new LogonProvider();
            _path = path;
            ImpersonatedUser = RequiresAuth(_path, _logOnProvider);
        }
        public DoDeleteOperation(IActivityIOPath path, IDev2LogonProvider logOnProvider)
        {
            _logOnProvider = logOnProvider;
            _path = path;
        }
        public override bool ExecuteOperation()
        {
            try
            {
                if (ImpersonatedUser != null)
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
                using (ImpersonatedUser)
                {
                    return DeleteHelper.Delete(_path.Path);
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
