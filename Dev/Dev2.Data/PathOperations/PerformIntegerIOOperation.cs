﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Dev2.Data.PathOperations
{
    [ExcludeFromCodeCoverage]
    public abstract class PerformIntegerIOOperation : ImpersonationOperation
    {
        protected PerformIntegerIOOperation(ImpersonationDelegate impersonationDelegate) : base(impersonationDelegate)
        {
        }

        public static bool FileExist(IActivityIOPath path, IFile fileWrapper) => fileWrapper.Exists(path.Path);       
        public abstract int ExecuteOperationWithAuth(Stream src, IActivityIOPath dst);
        public abstract int ExecuteOperation();
    }
}
