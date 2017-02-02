using System.Linq;
using System.Windows.Controls;
using Caliburn.Micro;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for DeployView.xaml
    /// </summary>
    public partial class DeployView : IView, ICheckControlEnabledView
    {


        public DeployView()
        {
            InitializeComponent();
        }

        public IServer SelectedServer => null;

        public IServer SelectedDestinationServer
        {
            get
            {
                return DestinationConnectControl.SelectedServer;
            }
            set
            {
                DestinationConnectControl.SelectedServer = value;
            }
        }
        public string ErrorMessage
        {
            get
            {
                var be = Status.GetBindingExpression(TextBlock.TextProperty);
                be?.UpdateTarget();
                return Status.Text;
            }
            set
            {
                Status.Text = value;
            }
        }
        public string CanDeploy
        {
            get
            {
                return ((IDeployViewModel)DataContext).DeployCommand.CanExecute(null) ? "Enabled" : "Disabled";
            }
            set
            {
                Deploy.IsEnabled = value=="Enabled";
            }
        }
        public string CanSelectDependencies
        {
            get
            {
                return Dependencies.IsEnabled ? "Enabled" : "Disabled"; 
            }
            set
            {
                Dependencies.IsEnabled = value=="Enabled";
            }
        }
        public string StatusPassedMessage
        {
            get
            {
                return ((IDeployViewModel)DataContext).DeploySuccessMessage??"";
            }
            set
            {
                StatusPass.Text = value;
            }
        }

        public string Services => ((IDeployViewModel)DataContext).ServicesCount;

        public string Sources => ((IDeployViewModel)DataContext).SourcesCount;

        public string New => ((IDeployViewModel)DataContext).NewResourcesCount;

        public string Overrides => ((IDeployViewModel)DataContext).OverridesCount;

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
       
                case "Dependencies":
                    return Dependencies.IsEnabled;
                case "Deploy":
                    return Deploy.IsEnabled;
                default: return false;
            }
        }

        #endregion

        public void SelectPath(string path)
        {
            var explorerItemViewModels = ((IDeployViewModel)DataContext).Source.SelectedEnvironment.AsList();
            explorerItemViewModels.Apply(a =>
            {
                if (a.ResourcePath == path) 
                    a.IsResourceChecked = true; 
                
            });
            var explorerTreeItems = ((IDeployViewModel)DataContext).Source.SelectedItems.ToList();
            ((IDeployViewModel)DataContext).StatsViewModel.Calculate(explorerTreeItems);
        }
        public void UnSelectPath(string path)
        {
            ((IDeployViewModel)DataContext).Source.SelectedEnvironment.AsList().Apply(a =>
            {
                if (a.ResourcePath == path)
                    a.IsResourceUnchecked = false;

            });
            ((IDeployViewModel)DataContext).StatsViewModel.Calculate(((IDeployViewModel)DataContext).Source.SelectedItems.ToList());
        }

        public void SelectDestinationServer(string servername)
        {
            var firstOrDefault = ((IDeployViewModel)DataContext).Destination.ConnectControlViewModel.Servers.FirstOrDefault(a => a.ResourceName == servername);
            ((IDeployViewModel)DataContext).Destination.ConnectControlViewModel.SelectedConnection = firstOrDefault;
        }

        public void DeployItems()
        {
            if (((IDeployViewModel)DataContext).DeployCommand.CanExecute(null))
            ((IDeployViewModel)DataContext).DeployCommand.Execute(null);
        }

        public void SelectDependencies()
        {
            Dependencies.Command.Execute(null);
        }

        public string VerifySelectPath(string path)
        {
            var res = ((IDeployViewModel)DataContext).Source.SelectedEnvironment.AsList().FirstOrDefault(a =>  (a.ResourcePath ==path) && (a.IsResourceChecked ?? false));
            if(res!=null)
            {
                return "Selected";
            }
            return "Not Selected";
        }

        public void SetFilter(string filter)
        {
            ((IDeployViewModel)DataContext).Source.SearchText = filter;
        }

        public string VerifySelectPathVisibility(string path)
        {
            var res = ((IDeployViewModel)DataContext).Source.SelectedEnvironment.AsList().FirstOrDefault(a => a.ResourcePath == path );
            if (res == null)
            {
                return "Not Visible";
            }
            if(!res.IsVisible)
                return "Not Visible";
            return "Visible";
        }

        public bool CheckVisibility(string control, string visibility)
        {
            switch (control)
            {
                case "SourceConnectControl":
                    return SourceConnectControl.Visibility.ToString().ToLower() == visibility.ToLower();
                case "DestinationConnectControl":
                    return DestinationConnectControl.Visibility.ToString().ToLower() == visibility.ToLower();
                case "SourceNavigationView":
                    return SourceNavigationView.Visibility.ToString().ToLower() == visibility.ToLower();
                case "Dependencies":
                    return Dependencies.Visibility.ToString().ToLower() == visibility.ToLower();
                case "Deploy":
                    return Deploy.Visibility.ToString().ToLower() == visibility.ToLower();
                default: return false;
            }
        }

        public void SelectServer(string p0)
        {
            ((IDeployViewModel)DataContext).Source.SelectedEnvironment.IsResourceChecked = true;
        }

        public bool VerifyAllSelected(string p0)
        {
           return ((IDeployViewModel)DataContext).Source.SelectedEnvironment.AsList().All(a => a.IsResourceChecked ?? false);
        }

        public void ConnectSourceServer()
        {
            SourceConnectControl.ConnectButton.Command.Execute(null);
        }

        public void ConnectDestinationServer()
        {
            DestinationConnectControl.ConnectButton.Command.Execute(null);
        }
    }
}
