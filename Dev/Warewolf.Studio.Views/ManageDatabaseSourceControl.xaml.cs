using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageDatabaseSourceControl.xaml
    /// </summary>
    public partial class ManageDatabaseSourceControl : IManageDatabaseSourceView, ICheckControlEnabledView
    {
        public ManageDatabaseSourceControl()
        {
            InitializeComponent();
        }

        private void EnterServerName(string serverName)
        {
            ServerTextBox.Text = serverName;
        }

        public Visibility GetDatabaseDropDownVisibility()
        {
            BindingExpression be = DatabaseComboxContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return DatabaseComboxContainer.Visibility;
        }

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Save":
                    var viewModel = DataContext as ManageDatabaseSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
                case "Test Connection":
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
            try
            {
                DatabaseComboxBox.SelectedItem = databaseName;
            }
            catch (Exception)
            {
                //Stupid exception when running from tests
            }
        }

        public void SelectServer(string serverName)
        {
            try
            {
                EnterServerName(serverName);
            }
            catch (Exception)
            {
                //Stupid exception when running from tests
            }
        }

        public void SelectType(string type)
        {
            try
            {
                ServerTypeComboBox.SelectedItem = type;
            }
            catch (Exception)
            {
                //Stupid exception when running from tests
            }
        }

        public Visibility GetUsernameVisibility()
        {
            BindingExpression be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return UserNamePasswordContainer.Visibility;
        }

        public Visibility GetPasswordVisibility()
        {
            BindingExpression be = UserNamePasswordContainer.GetBindingExpression(VisibilityProperty);
            be?.UpdateTarget();
            return UserNamePasswordContainer.Visibility;
        }

        public void PerformTestConnection()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public void PerformSave()
        {
            var viewModel = DataContext as ManageDatabaseSourceViewModel;
            viewModel?.OkCommand.Execute(null);
        }

        public void EnterUserName(string userName)
        {
            UserNameTextBox.Text = userName;
        }

        public void EnterPassword(string password)
        {
            PasswordTextBox.Password = password;
        }

        public string GetErrorMessage()
        {
            return ((ManageDatabaseSourceViewModel)DataContext).TestMessage;
        }


        // ReSharper disable once InconsistentNaming
        private void XamComboEditor_Loaded(object sender, RoutedEventArgs e)
        {
            //SpecializedTextBox txt = Utilities.GetDescendantFromType(sender as DependencyObject, typeof(SpecializedTextBox), false) as SpecializedTextBox;
            //if (txt != null)
            //{
            //    txt.SelectAll();
            //    txt.Focus();
            //    var selectedItem = DatabaseComboxBox.SelectedItem;
            //    if (selectedItem != null) 
            //        txt.Text = selectedItem.ToString();
            //    DatabaseComboxBox.Style = Application.Current.TryFindResource("XamComboEditorStyle") as Style;
            //}
        }

        public void VerifyServerExistsintComboBox(string serverName)
        {

        }

        public IEnumerable<string> GetServerOptions()
        {

            return new List<string>();
        }

        public string GetSelectedDbOption()
        {
            return ServerTypeComboBox.SelectedItem.ToString();
        }

        public void Test()
        {
            TestConnectionButton.Command.Execute(null);
        }

        public string GetUsername()
        {
            return UserNameTextBox.Text;
        }

        public object GetPassword()
        {
            return PasswordTextBox.Password;
        }

        public string GetHeader()
        {
            return ((ManageDatabaseSourceViewModel)DataContext).HeaderText;
        }
        public string GetTabHeader()
        {
            return ((ManageDatabaseSourceViewModel)DataContext).Header;
        }
        public void CancelTest()
        {
            CancelTestButton.Command.Execute(null);
        }

        void ManageServerControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowsRadioButton.Focus();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
