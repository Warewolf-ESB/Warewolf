/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Runtime;

namespace Dev2.Common.Interfaces.Infrastructure
{
    public interface IExplorerServerResourceRepository : IExplorerResourceRepository
    {
        IExplorerItem Load(string type, string filter);
        void MessageSubscription(IExplorerRepositorySync sync);

        IExplorerItem UpdateItem(IResource resource);

        IExplorerItem Find(Guid id);
        IExplorerItem Find(Func<IExplorerItem,bool> predicate);

        List<string> LoadDuplicate();

        IExplorerItemFactory ExplorerItemFactory { get; }
    }
}