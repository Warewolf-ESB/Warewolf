using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Unlimited.Applications.BusinessDesignStudio {
    /// <summary>
    /// Interaction logic for ConfigureSwitchWindow.xaml
    /// </summary>
    public partial class GetStringDataWindow : Window {
        public delegate void GotValueHandler(string stringData);
        public event GotValueHandler GotValueEvent;

        public GetStringDataWindow() {
            InitializeComponent();
            txtDataElementName.Focus();
        }

        public GetStringDataWindow(string windowTitle, string labelContent, string helpString, string val) : this() {
            this.Title = windowTitle;
            lblValue.Content = labelContent;
            txtHelp.Text = helpString;
            txtDataElementName.Text = val;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!Validate()) {
                e.Cancel = true;
            }
        }

        private bool Validate() {
            if (string.IsNullOrEmpty(txtDataElementName.Text)) {
                MessageBox.Show("A value is required", "Error", MessageBoxButton.OK);
                return false;
            }
            if (GotValueEvent != null) {
                GotValueEvent(txtDataElementName.Text);
            }
            return true;
        }

    }
}
