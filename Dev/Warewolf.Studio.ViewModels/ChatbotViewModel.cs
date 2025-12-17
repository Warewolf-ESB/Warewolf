#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ChatbotViewModel : BindableBase
    {
        private string _message;
        private ObservableCollection<string> _messages;
        private string _displayName;

        public ChatbotViewModel()
        {
            DisplayName = "Chatbot";
            Messages = new ObservableCollection<string>();
            SendCommand = new DelegateCommand(Send, CanSend);
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(() => Message);
                ((DelegateCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(() => Messages);
            }
        }

        public ICommand SendCommand { get; }

        private bool CanSend()
        {
            return !string.IsNullOrWhiteSpace(Message);
        }

        private void Send()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                Messages.Add($"You: {Message}");
                // TODO: Integrate with AI service
                Messages.Add($"AI: This is a placeholder response to '{Message}'");
                Message = string.Empty;
            }
        }
    }
}
