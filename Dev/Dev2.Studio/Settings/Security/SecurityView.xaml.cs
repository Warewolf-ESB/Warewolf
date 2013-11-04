
using System.Windows.Controls;
using System.Windows.Input;

namespace Dev2.Settings.Security
{
    /// <summary>
    /// Interaction logic for SecurityView.xaml
    /// </summary>
    public partial class SecurityView
    {
        public SecurityView()
        {
            InitializeComponent();
            DataContext = SecurityViewModel.Create();
        }

        void OnServerTextChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            ServerPermissionsDataGrid.BeginEdit();
        }

        void OnResourcesTextChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            ResourcePermissionsDataGrid.BeginEdit();
        }

        void OnAddingNewServerItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new WindowsGroupPermission { IsServer = true };
        }

        void OnAddingNewResourceItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new WindowsGroupPermission();
        }
    }
}
