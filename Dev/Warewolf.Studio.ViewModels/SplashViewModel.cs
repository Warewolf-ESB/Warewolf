#pragma warning disable
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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.AntiCorruptionLayer;

namespace Warewolf.Studio.ViewModels
{
    public class SplashViewModel : BindableBase, ISplashViewModel
    {
        private string _serverVersion;
        private string _studioVersion;

        public SplashViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            ExternalProcessExecutor = externalProcessExecutor ?? throw new ArgumentNullException(nameof(externalProcessExecutor));

            var conUri = new Uri(Resources.Languages.Core.ContributorsUrl);
            ContributorsUrl = conUri;
            var comUri = new Uri(Resources.Languages.Core.CommunityUrl);
            CommunityUrl = comUri;
            var expUri = new Uri(Resources.Languages.HelpText.ExpertHelpUrl);
            ExpertHelpUrl = expUri;
            var warewolfUri = new Uri(Resources.Languages.Core.WarewolfUrl);
            WarewolfUrl = warewolfUri;
            WarewolfCopyright = string.Format(Resources.Languages.Core.WarewolfCopyright, DateTime.Now.Year.ToString());

            WarewolfLicense = GlobalConstants.IsLicensed ? GlobalConstants.LicensePlanId : GlobalConstants.LicenseCustomerId;

            ContributorsCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(ContributorsUrl));
            CommunityCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(CommunityUrl));
            ExpertHelpCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(ExpertHelpUrl));
            WarewolfUrlCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(WarewolfUrl));
        }

        public IServer Server { get; set; }
        public IExternalProcessExecutor ExternalProcessExecutor { get; set; }
        public ICommand ContributorsCommand { get; set; }
        public ICommand CommunityCommand { get; set; }
        public ICommand ExpertHelpCommand { get; set; }
        public ICommand WarewolfUrlCommand { get; set; }
        public string ServerVersion
        {
            get => _serverVersion;
            set
            {
                _serverVersion = value;
                OnPropertyChanged("ServerVersion");
            }
        }
        [ExcludeFromCodeCoverage]
        public string StudioVersion
        {
            get => _studioVersion;
            set
            {
                _studioVersion = value;
                OnPropertyChanged("StudioVersion");
            }
        }

       public Uri WarewolfUrl { get; set; }
       public string WarewolfLicense { get; set; }
        public Uri ContributorsUrl { get; set; }
        public Uri CommunityUrl { get; set; }
        public Uri ExpertHelpUrl { get; set; }
        public string WarewolfCopyright { get; set; }
        public void ShowServerStudioVersion()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                var serverVersion = Server.GetServerVersion();
                var splitServerVersion = serverVersion.Split('.');
                if (splitServerVersion.Length > 2 && int.Parse(splitServerVersion[2]) > 6000)
                {
                    var totalDays = Convert.ToDouble(splitServerVersion[2]);
                    var totalSeconds = Convert.ToDouble(splitServerVersion[3])*2;
                    var cSharpEpoc = new DateTime(2000, 1, 1);
                    var compileTime = cSharpEpoc.AddDays(totalDays).AddSeconds(totalSeconds);
                    ServerVersion = "Compiled " + GetInformalDate(compileTime);
                }
                else
                {
                    ServerVersion = "Version " + serverVersion;
                }
                var studioVersion = Utils.FetchVersionInfo();
                var splitStudioVersion = studioVersion.Split('.');
                if (int.Parse(splitStudioVersion[2]) > 6000)
                {
                    var totalDays = Convert.ToDouble(splitStudioVersion[2]);
                    var totalSeconds = Convert.ToDouble(splitStudioVersion[3])*2;
                    var cSharpEpoc = new DateTime(2000, 1, 1);
                    var compileTIme = cSharpEpoc.AddDays(totalDays).AddSeconds(totalSeconds);
                    StudioVersion = "Compiled " + GetInformalDate(compileTIme);
                }
                else
                {
                    StudioVersion = "Version " + studioVersion;
                }
            });
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        static string GetInformalDate(DateTime d)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var sinceThen = DateTime.Now.Subtract(d);
            var totalDays = (int)sinceThen.TotalDays;
            var totalHours = (int)sinceThen.TotalHours;
            var totalMinutes = (int)sinceThen.TotalMinutes;
            if (totalDays < 0 || totalHours < 0 || totalMinutes < 0)
            {
                return null;
            }
            if (totalMinutes == 0)
            {
                return "just Now";
            }
            if (totalMinutes == 1)
            {
                return "a minute ago";
            }
            if (totalHours == 0)
            {
                return $"{totalMinutes} minutes ago";
            }
            if (totalHours == 1)
            {
                return "an hour ago";
            }
            if (totalDays == 0)
            {
                return $"{totalHours} hours ago";
            }
            if (totalDays == 1)
            {
                return "yesterday";
            }
            if (totalDays < 7)
            {
                return $"{totalDays} days ago";
            }
            if (totalDays < 14)
            {
                return "a week ago";
            }
            if (totalDays < 31)
            {
                return $"{Math.Ceiling((double) totalDays/7)} weeks ago";
            }
            if (totalDays < 62)
            {
                return "a month ago";
            }
            if (totalDays < 365)
            {
                return $"{Math.Ceiling((double) totalDays/31)} months ago";
            }
            if (totalDays < 730)
            {
                return "a year ago";
            }
            return $"{Math.Ceiling((double) totalDays/365)} years ago";
        }
    }
}