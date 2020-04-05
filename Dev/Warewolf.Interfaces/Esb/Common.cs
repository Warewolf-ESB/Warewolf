/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Warewolf.Esb
{
    public interface IEsbRequest
    {
        string ServiceName { get; }
        Dictionary<string, StringBuilder> Args { get; set; }
        void AddArgument(string key, StringBuilder value);
    }
    public interface IContextualInternalService
    {
        StringBuilder Execute(IInternalExecutionContext internalExecutionContext);
    }
    public interface IInternalExecutionContext
    {
        void RegisterAsClusterEventListener();
        IEsbRequest Request { get; }
        object Workspace { get; set; } // this is actually supposed to be Dev2.Workspaces.IWorkspace
    }

    public interface ICatalogRequest
    {
        IEsbRequest Build();
    }

    public interface ICatalogSubscribeRequest
    {
    }
}
