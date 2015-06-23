using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface ISplashViewModel
    {
        IServer Server { get; set; }
        IExternalProcessExecutor ExternalProcessExecutor { get; set; }
        ICommand ContributorsCommand { get; set; }
        ICommand CommunityCommand { get; set; }
        ICommand ExpertHelpCommand { get; set; }
        ICommand DevUrlCommand { get; }
        ICommand WarewolfUrlCommand { get; }
        string ServerVersion { get; set; }
        string StudioVersion { get; set; }
        Uri DevUrl { get; set; }
        Uri WarewolfUrl { get; set; }
        Uri ContributorsUrl { get; set; }
        Uri CommunityUrl { get; set; }
        Uri ExpertHelpUrl { get; set; }
    }
}