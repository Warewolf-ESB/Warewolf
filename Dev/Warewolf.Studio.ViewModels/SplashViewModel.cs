using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class SplashViewModel : ISplashViewModel
    {
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

            ContributorsCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(ContributorsUrl));
            CommunityCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(CommunityUrl));
            ExpertHelpCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(ExpertHelpUrl));
            DevUrlCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(DevUrl));
            WarewolfUrlCommand = new DelegateCommand(() => externalProcessExecutor.OpenInBrowser(WarewolfUrl));
            ShowServerVersion();
        }

        public IServer Server { get; set; }
        public IExternalProcessExecutor ExternalProcessExecutor { get; set; }
        public ICommand ContributorsCommand { get; set; }
        public ICommand CommunityCommand { get; set; }
        public ICommand ExpertHelpCommand { get; set; }
        public ICommand DevUrlCommand { get; set; }
        public ICommand WarewolfUrlCommand { get; set; }
        public string ServerVersion { get; set; }
        public string StudioVersion { get; set; }
        public Uri DevUrl { get; set; }
        public Uri WarewolfUrl { get; set; }
        public Uri ContributorsUrl { get; set; }
        public Uri CommunityUrl { get; set; }
        public Uri ExpertHelpUrl { get; set; }

        void ShowServerVersion()
        {
            ServerVersion = "Version " + Server.GetServerVersion();
            StudioVersion = "Version " + Utils.FetchVersionInfo();
        }
    }
}