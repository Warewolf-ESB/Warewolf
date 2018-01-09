/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Dev2.Workspaces
{
    public interface IWorkspaceRepository
    {
        int Count
        {
            get;
        }

        IWorkspace ServerWorkspace { get; }

        Guid GetWorkspaceID(WindowsIdentity identity);
        
        void GetLatest(IWorkspace workspace, IList<string> servicesToIgnore);

        IWorkspace Get(Guid workspaceID);

        IWorkspace Get(Guid workspaceID, bool force);

        IWorkspace Get(Guid workspaceID, bool force, bool loadResources);
            
        void Save(IWorkspace workspace);
            
        void Delete(IWorkspace workspace);
        
        void RefreshWorkspaces();
    }
}
