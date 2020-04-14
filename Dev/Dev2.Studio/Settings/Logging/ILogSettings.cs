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

namespace Dev2.Settings.Logging
{
    public interface ILogSettings
    {
        ICommand GetServerLogFileCommand { get; }
        ICommand GetStudioLogFileCommand { get; }
        LogLevel ServerEventLogLevel { get; set; }
        LogLevel StudioEventLogLevel { get; set; }
        string StudioLogMaxSize { get; }
        string ServerLogMaxSize { get; }
        bool CanEditStudioLogSettings { get; }
        bool CanEditLogSettings { get; }
        LogLevel StudioFileLogLevel { get; }
        IEnumerable<string> LoggingTypes { get; }
        string AuditFilePath { get; set; }
        IResource SelectedAuditingSource { get; }
        List<IResource> AuditingSources { get; }
        bool IsLegacy { get; set; }
    }
}