﻿using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio
{
    public interface IMenuViewModel
    {
        ICommand NewCommand { get; set; }
        ICommand DeployCommand { get; set; }
        ICommand SaveCommand { get; set; }
        ICommand OpenSettingsCommand { get; set; }
        ICommand OpenSchedulerCommand { get; set; }
        ICommand ExecuteServiceCommand { get; set; }
        ICommand CheckForNewVersionCommand { get; set; }
        ICommand LockCommand { get; set; }
        bool HasNewVersion { get; set; }
        string NewLabel { get; }
        string SaveLabel { get; }
        string DeployLabel { get; }
        string DatabaseLabel { get; }
        string DLLLabel { get; }
        string WebLabel { get; }
        string TaskLabel { get; }
        string DebugLabel { get; }
        string SettingsLabel { get; }
        string SupportLabel { get; }
        string ForumsLabel { get; }
        string ToursLabel { get; }
        string NewVersionLabel { get; }
        string LockLabel { get; }
        string UnLockLabel { get; }
    }
}
