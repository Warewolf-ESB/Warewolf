using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.ServiceModel;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageServerControl.xaml
    /// </summary>
    public partial class ManageDatabaseSourceControl:IManageDatabaseSourceView
    {
        public ManageDatabaseSourceControl()
        {
            InitializeComponent();
        }

        public void EnterServerName(string serverName)
        {
            ServerTextBox.Text = serverName;
        }

        public Visibility GetDatabaseDropDownVisibility()
        {
            BindingExpression be = DatabaseComboxContainer.GetBindingExpression(VisibilityProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return DatabaseComboxContainer.Visibility;
        }

        public bool GetControlEnable(string controlName)
        {
            if (controlName == "Save")
            {
                return SaveButton.Command.CanExecute(null);
            }
            if (controlName == "Test Connection")
            {
                return TestConnectionButton.Command.CanExecute(null);
            }
            return false;
        }

        public void SetAuthenticationType(AuthenticationType authenticationType)
        {
            if (authenticationType == AuthenticationType.Windows)
            {
                WindowsRadioButton.IsChecked = true;
            }
            else
            {
                UserRadioButton.IsChecked = true;
            }
        }

        public void SelectDatabase(string databaseName)
        {
            DatabaseComboxBox.SelectedItem = databaseName;
        }

        public Visibility GetUsernameVisibility()
        {
            return UserNamePasswordContainer.Visibility;
        }

        public Visibility GetPasswordVisibility()
        {
            return UserNamePasswordContainer.Visibility;
        }

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            SaveButton.Command.Execute(null);
        }

        public void EnterUserName(string userName)
        {
            UserNameTextBox.Text = userName;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }
    }
}