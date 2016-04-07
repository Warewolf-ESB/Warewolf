using System;
using System.Diagnostics.CodeAnalysis;
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

        public SplashViewModel(IServer server, IExternalProcessExecutor externalProcessExecutor)
        {
            if (server == null) throw new ArgumentNullException("server");
            if (externalProcessExecutor == null) throw new ArgumentNullException("externalProcessExecutor");
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

        public Uri DevUrl { get; set; }
        public Uri WarewolfUrl { get; set; }
        public Uri ContributorsUrl { get; set; }
        public Uri CommunityUrl { get; set; }
        public Uri ExpertHelpUrl { get; set; }
        [ExcludeFromCodeCoverage]
        public string WarewolfCopyright { get; set; }

        public void ShowServerVersion()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                ServerVersion = FormatVersionString(Server.GetServerVersion());
                StudioVersion = FormatVersionString(Utils.FetchVersionInfo());
            });
        }

        private static string FormatVersionString(string rawVersionString)
        {
            var versionParts = rawVersionString.Split('.');
            if(versionParts.Length < 4)
            {
                return null;
            }
            if(versionParts[0] != "0" || versionParts[1] != "0")
            {
                return "Version " + versionParts[0] + "." + versionParts[1] + "." + versionParts[2] + " build " + versionParts[3];
            }
            var epocTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var nowTime = epocTime.AddDays(int.Parse(versionParts[2])).AddSeconds(int.Parse(versionParts[3])*2);
            return "Compiled " + TimeSinceCommit(nowTime);
        }

        private static string TimeSinceCommit(DateTime value)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - value.Ticks);
            double seconds = ts.TotalSeconds;

            // Less than one minute
            if (seconds < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (seconds < 60 * MINUTE)
                return ts.Minutes == 1 ? "one minute ago" : ts.Minutes + " minutes ago";

            if (seconds < 120 * MINUTE)
                return "an hour ago";

            if (seconds < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (seconds < 48 * HOUR)
                return "yesterday";

            if (seconds < 30 * DAY)
                return ts.Days + " days ago";

            if (seconds < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }
    }
}