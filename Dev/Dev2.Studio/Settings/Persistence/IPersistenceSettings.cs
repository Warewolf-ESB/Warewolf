/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Settings.Persistence
{
    public interface IPersistenceSettings
    {
        IResource SelectedPersistenceDataSource { get; }
        List<IResource> PersistenceDataSources { get; }
        string SelectedPersistenceScheduler { get; }
        List<string> PersistenceSchedulers { get; }
        bool EncryptDataSource{ get; set; }
        bool Enable{ get; set; }
        bool PrepareSchemaIfNecessary{ get; set; }
        string ServerName { get; set; }
        string DashboardHostname { get; set; }
        string DashboardPort { get; set; }
        string DashboardName { get; set; }
        string HangfireDashboardUrl { get; set; }
        ICommand HangfireDashboardBrowserCommand { get; set; }
    }
}