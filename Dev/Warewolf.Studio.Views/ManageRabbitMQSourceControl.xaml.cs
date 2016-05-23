using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageRabbitMQSourceControl.xaml
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public partial class ManageRabbitMQSourceControl : IView, ICheckControlEnabledView
    {
        public ManageRabbitMQSourceControl()
        {
            InitializeComponent();
        }

        public bool GetControlEnabled(string controlName)
        {
            //switch (controlName)
            //{
            //    case "Save":
            //        var viewModel = DataContext as ManageRabbitMQSourceViewModel;
            //        return viewModel != null && viewModel.OkCommand.CanExecute(null);
            //}
            return false;
        }

        /// <summary>
        /// Attaches events and names to compiled content.
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }
    }
}