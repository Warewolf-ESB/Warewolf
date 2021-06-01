/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations.Operations;
using Dev2.PathOperations;

namespace Dev2.Data.PathOperations
{
    public abstract class PerformBoolIOOperation : ImpersonationOperation
    {
        protected PerformBoolIOOperation(ImpersonationDelegate impersonationDelegate) : base(impersonationDelegate)
        {
        }

        public static enPathType PathIs(IActivityIOPath path, IFile fileWrapper, IDirectory dirWrapper)
        {
            if (Dev2ActivityIOPathUtils.IsDirectory(path.Path))
            {
                return enPathType.Directory;
            }

            var exists = FileExist(path, fileWrapper) || DirectoryExist(path, dirWrapper);
            var isNotStarWildCard = !Dev2ActivityIOPathUtils.IsStarWildCard(path.Path);
            if (exists && isNotStarWildCard)
            {
                var fa = fileWrapper.GetAttributes(path.Path);
                if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return enPathType.Directory;
                }
            }
            return enPathType.File;
        }
        public static DoDeleteOperation RequiresOverwrite(IDev2CRUDOperationTO arg, IActivityIOPath path, IDev2LogonProvider dev2LogonProvider)
         => arg.Overwrite ? new DoDeleteOperation(path, dev2LogonProvider) : null;

        public static bool FileExist(IActivityIOPath path, IFile fileWrapper) => fileWrapper.Exists(path.Path);
        public static bool DirectoryExist(IActivityIOPath path, IDirectory dirWrapper) => dirWrapper.Exists(path.Path);        
        
        public abstract bool ExecuteOperation();
        public abstract bool ExecuteOperationWithAuth();

    }
}
