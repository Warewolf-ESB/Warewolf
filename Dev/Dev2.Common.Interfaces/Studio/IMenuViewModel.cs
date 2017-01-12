/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio
{
    public interface IMenuViewModel
    {
        ICommand NewServiceCommand { get; set; }
        ICommand DeployCommand { get; set; }
        ICommand SaveCommand { get; set; }
        ICommand OpenSettingsCommand { get; set; }
        ICommand OpenSchedulerCommand { get; set; }
        ICommand ExecuteServiceCommand { get; set; }
        ICommand CheckForNewVersionCommand { get; set; }
        ICommand LockCommand { get; set; }
        ICommand SlideOpenCommand { get; set; }
        ICommand SlideClosedCommand { get; set; }
        bool HasNewVersion { get; set; }
        bool IsPanelOpen { get; set; }
        bool IsPanelLockedOpen { get; set; }
        string NewLabel { get; }
        string SaveLabel { get; }
        string DeployLabel { get; }
        string TaskLabel { get; }
        string DebugLabel { get; }
        string SettingsLabel { get; }
        string SupportLabel { get; }
        string NewVersionLabel { get; }
        string LockLabel { get; }
        string LockImage { get; }
        int ButtonWidth { get; }
        ICommand IsOverLockCommand { get; }
        ICommand IsNotOverLockCommand { get; }
        ICommand SupportCommand { get; }
        bool IsProcessing { get; set; }
        ICommand StartPageCommand { get; }

        void Lock();
    }
}
