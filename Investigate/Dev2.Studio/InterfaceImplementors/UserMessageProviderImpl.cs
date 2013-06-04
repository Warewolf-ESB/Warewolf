using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core {
    [Export(typeof(IUserMessageProvider))]
    public class UserMessageProviderImpl : IUserMessageProvider {
        public void ShowUserMessage(string message, string title="") {
            MessageBox.Show(message, title);
        }

        public void ShowUserErrorMessage(string message, string title = "") {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowUserWarningMessage(string message, string title = "") {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
