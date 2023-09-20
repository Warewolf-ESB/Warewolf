/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface ISplashViewModel
    {
        IServer Server { get; set; }
        IExternalProcessExecutor ExternalProcessExecutor { get; set; }
        ICommand ContributorsCommand { get; set; }
        ICommand CommunityCommand { get; set; }
        ICommand ExpertHelpCommand { get; set; }
        ICommand WarewolfUrlCommand { get; }
        string ServerVersion { get; set; }
        string StudioVersion { get; set; }
        Uri WarewolfUrl { get; set; }
        Uri ContributorsUrl { get; set; }
        Uri CommunityUrl { get; set; }
        Uri ExpertHelpUrl { get; set; }
        string WarewolfCopyright { get; set; }
        string WarewolfLicense { get; set; }
    }
}