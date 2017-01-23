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
using System.Data;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.DB
{
    public interface IDbServiceModel
    {
        ObservableCollection<IDbSource> RetrieveSources();
        ICollection<IDbAction> GetActions(IDbSource source);
        void CreateNewSource(enSourceType type);
        void EditSource(IDbSource selectedSource, enSourceType type);
        DataTable TestService(IDatabaseService inputValues);

        IStudioUpdateManager UpdateRepository { get; }

    }
}
