
using System.Windows;
using System.Windows.Data;
using Dev2.Activities.Designers2.Core.Adorners;

namespace Dev2.Settings.Security
{
    /// <summary>
    /// Interaction logic for SecurityView.xaml
    /// </summary>
    public partial class SecurityView
    {
        readonly Help.HelpAdorner _serverHelpAdorner;
        readonly Help.HelpAdorner _resourceHelpAdorner;

        public SecurityView()
        {
            InitializeComponent();
            _serverHelpAdorner = new Help.HelpAdorner(ServerHelpToggleButton);
            _resourceHelpAdorner = new Help.HelpAdorner(ResourceHelpToggleButton);

            DataContextChanged += OnDataContextChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if(args.OldValue != null)
            {
                BindingOperations.ClearBinding(_serverHelpAdorner, AdornerControl.IsAdornerVisibleProperty);
                BindingOperations.ClearBinding(_resourceHelpAdorner, AdornerControl.IsAdornerVisibleProperty);
            }

            if(args.NewValue != null)
            {
                BindingOperations.SetBinding(_serverHelpAdorner, AdornerControl.IsAdornerVisibleProperty, new Binding(SecurityViewModel.IsServerHelpVisibleProperty.Name)
                {
                    Source = args.NewValue,
                    Mode = BindingMode.OneWay
                });

                BindingOperations.SetBinding(_resourceHelpAdorner, AdornerControl.IsAdornerVisibleProperty, new Binding(SecurityViewModel.IsResourceHelpVisibleProperty.Name)
                {
                    Source = args.NewValue,
                    Mode = BindingMode.OneWay
                });
            }
        }
    }
}
