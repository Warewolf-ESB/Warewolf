using System;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.AntiCorruptionLayer;

namespace Warewolf.Studio.ViewModels
{
    public class SplashViewModel : BindableBase, ISplashViewModel
    {
        string _serverVersion;
        string _studioVersion;
        string _serverInformationalVersion;
        string _studioInformationalVersion;

        public SplashViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (externalProcessExecutor == null) throw new ArgumentNullException(nameof(externalProcessExecutor));
            Server = server;
            ExternalProcessExecutor = externalProcessExecutor;

            Uri conUri = new Uri(Resources.Languages.Core.ContributorsUrl);
            ContributorsUrl = conUri;
            Uri comUri = new Uri(Resources.Languages.Core.CommunityUrl);
            CommunityUrl = comUri;
            Uri expUri = new Uri(Resources.Languages.Core.ExpertHelpUrl);
            ExpertHelpUrl = expUri;
            Uri devUri = new Uri(Resources.Languages.Core.DevUrl);
            DevUrl = devUri;
            Uri warewolfUri = new Uri(Resources.Languages.Core.WarewolfUrl);
            WarewolfUrl = warewolfUri;
            WarewolfCopyright = Resources.Languages.Core.WarewolfCopyright;

            ContributorsCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(ContributorsUrl));
            CommunityCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(CommunityUrl));
            ExpertHelpCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(ExpertHelpUrl));
            DevUrlCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(DevUrl));
            WarewolfUrlCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(WarewolfUrl));            
        }

        public IServer Server { get; set; }
        public IExternalProcessExecutor ExternalProcessExecutor { get; set; }
        public ICommand ContributorsCommand { get; set; }
        public ICommand CommunityCommand { get; set; }
        public ICommand ExpertHelpCommand { get; set; }
        public ICommand DevUrlCommand { get; set; }
        public ICommand WarewolfUrlCommand { get; set; }
        public string ServerVersion
        {
            get
            {
                return _serverVersion;
            }
            set
            {
                _serverVersion = value;
                OnPropertyChanged("ServerVersion");
            }
        }
        public string ServerInformationalVersion
        {
            get
            {
                return _serverInformationalVersion;
            }
            set
            {
                _serverVersion = value;
                OnPropertyChanged("ServerInformationalVersion");
            }
        }

        public string StudioVersion
        {
            get
            {
                return _studioVersion;
            }
            set
            {
                _studioVersion = value;
                OnPropertyChanged("StudioVersion");
            }
        }
        public string StudioInformationalVersion
        {
            get
            {
                return _studioInformationalVersion;
            }
            set
            {
                _studioInformationalVersion = value;
                OnPropertyChanged("StudioInformationalVersion");
            }
        }

        public Uri DevUrl { get; set; }
        public Uri WarewolfUrl { get; set; }
        public Uri ContributorsUrl { get; set; }
        public Uri CommunityUrl { get; set; }
        public Uri ExpertHelpUrl { get; set; }
        public string WarewolfCopyright { get; set; }

        public void ShowServerVersion()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                var serverVersion = Server.GetServerVersion();
                if (serverVersion.StartsWith("0.0."))
                {
                    var splitServerVersion = serverVersion.Split('.');
                    var totalDays = Convert.ToDouble(splitServerVersion[2]);
                    var totalSeconds = Convert.ToDouble(splitServerVersion[3])*2;
                    var cSharpEpoc = new DateTime(2000, 1, 1);
                    var compileTIme = cSharpEpoc.AddDays(totalDays).AddSeconds(totalSeconds);
                    ServerVersion = "Compiled " + GetInformalDate(compileTIme);
                }
                else
                {
                    ServerVersion = "Version " + serverVersion;
                }
                var studioVersion = Utils.FetchVersionInfo();
                if (studioVersion.StartsWith("0.0."))
                {
                    var splitStudioVersion = studioVersion.Split('.');
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

        static string GetInformalDate(DateTime d)
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