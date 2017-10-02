/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using System;
using System.Collections.Generic;
using Dev2.Studio.Core;

namespace Dev2.Studio.Interfaces
{
    public interface IServerRepository : IFrameworkRepository<IServer>
    {
        IServer Source { get; }

        IServer ActiveServer { get; set; }

        event EventHandler<IEnvironmentEditedArgs> ItemEdited;

        bool IsLoaded { get; set; }

        ICollection<IServer> ReloadServers();

        IServer Get(Guid id);

        IList<IServer> LookupEnvironments(IServer defaultEnvironment);

        IList<IServer> LookupEnvironments(IServer defaultEnvironment, IList<string> environmentGuids);
            
        void Clear();

        ICollection<IServer> ReloadAllServers();
    }
}
