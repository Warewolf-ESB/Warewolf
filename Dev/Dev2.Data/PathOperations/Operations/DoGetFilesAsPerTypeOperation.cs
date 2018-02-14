using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Warewolf.Resource.Errors;

namespace Dev2.Data.PathOperations.Operations
{
    public class DoGetFilesAsPerTypeOperation : PerformListOfIOPathOperation
    {
        readonly WindowsImpersonationContext ImpersonatedUser;
        protected readonly IDev2LogonProvider _logOnProvider;
        protected readonly IActivityIOPath _path;
        protected readonly IFile _fileWrapper;
        protected readonly IDirectory _dirWrapper;
        protected readonly ReadTypes _type;
        protected readonly IDev2CRUDOperationTO _crudArguments;
        protected readonly string _newPath;

        public DoGetFilesAsPerTypeOperation(IActivityIOPath path, ReadTypes type)
        {
            _logOnProvider = new LogonProvider();
            _fileWrapper = new FileWrapper();
            _dirWrapper = new DirectoryWrapper();
            _path = path;
            _type = type;
            ImpersonatedUser = RequiresAuth(_path, _logOnProvider);
            _newPath = AppendBackSlashes(_path, _fileWrapper, _dirWrapper);
        }
        public override IList<IActivityIOPath> ExecuteOperation()
        {
            try
            {
                if (ImpersonatedUser != null)
                {
                    return ExecuteOperationWithAuth();
                }
                if (!Dev2ActivityIOPathUtils.IsStarWildCard(_newPath))
                {
                    if (_dirWrapper.Exists(_newPath))
                    {
                        return AddDirsToResults(GetDirectoriesForType(_newPath, string.Empty, _type, _dirWrapper), _path);
                    }
                    throw new Exception(string.Format(ErrorResource.DirectoryDoesNotExist, _newPath));

                }
                var baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(_newPath);
                var pattern = Dev2ActivityIOPathUtils.ExtractFileName(_newPath);
                return AddDirsToResults(GetDirectoriesForType(baseDir, pattern, _type, _dirWrapper), _path);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception, GlobalConstants.WarewolfError);
                throw new Exception(string.Format(ErrorResource.DirectoryNotFound, _path.Path));
            }
        }


        public override IList<IActivityIOPath> ExecuteOperationWithAuth()
        {
            try
            {
                using (ImpersonatedUser)
                {
                    if (!Dev2ActivityIOPathUtils.IsStarWildCard(_newPath))
                    {
                        return AddDirsToResults(GetDirectoriesForType(_newPath, string.Empty, _type, _dirWrapper), _path);
                    }
                    var baseDir = Dev2ActivityIOPathUtils.ExtractFullDirectoryPath(_newPath);
                    var pattern = Dev2ActivityIOPathUtils.ExtractFileName(_newPath);
                    return AddDirsToResults(GetDirectoriesForType(baseDir, pattern, _type, _dirWrapper), _path);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
                throw new Exception(string.Format(ErrorResource.DirectoryNotFound, _path.Path));
            }
            finally
            {
                ImpersonatedUser.Undo();
            }
        }
    }
}
