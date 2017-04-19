using Dev2.Common.Interfaces;
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
    }
}
