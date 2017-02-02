/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface IPluginServiceModel
    {
        ObservableCollection<IPluginSource> RetrieveSources();
        ICollection<IPluginAction> GetActions(IPluginSource source, INamespaceItem value);
        ICollection<IPluginConstructor> GetConstructors(IPluginSource source, INamespaceItem value);
        ICollection<INamespaceItem> GetNameSpaces(IPluginSource source);
        ICollection<INamespaceItem> GetNameSpacesWithJsonRetunrs(IPluginSource source);
        void CreateNewSource();
        void EditSource(IPluginSource selectedSource);
        string TestService(IPluginService inputValues);
        IStudioUpdateManager UpdateRepository { get; }

        ICollection<IPluginAction> GetActionsWithReturns(IPluginSource source, INamespaceItem ns);
    }


}
