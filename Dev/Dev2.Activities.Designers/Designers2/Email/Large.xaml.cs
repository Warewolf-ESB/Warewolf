
using System.Windows;

namespace Dev2.Activities.Designers2.Email
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        EmailDesignerViewModel ViewModel { get { return DataContext as EmailDesignerViewModel; } }

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }

        public string ThePassword { get { return ThePasswordBox.Password; } set { ThePasswordBox.Password = value; } }

        void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var viewModel = ViewModel;
            if(viewModel != null)
            {
                ThePassword = viewModel.Password;
            }
        }

        void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var viewModel = ViewModel;
            if(viewModel != null)
            {
                viewModel.Password = ThePassword;
            }
        }

    }
}
