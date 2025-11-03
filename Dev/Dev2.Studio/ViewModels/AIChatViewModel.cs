using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.ViewModels
{
    public class AIChatViewModel : Screen
    {
        private string _message;
        private ObservableCollection<string> _messages;
        private ICommand _sendCommand;

        public AIChatViewModel()
        {
            DisplayName = "AI Chat";
            Messages = new ObservableCollection<string>();
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                NotifyOfPropertyChange(() => Messages);
            }
        }

        public ICommand SendCommand
        {
            get
            {
                return _sendCommand ?? (_sendCommand = new RelayCommand(param =>
                {
                    Messages.Add("You: " + Message);
                    var aiService = new Services.AIService();
                    var response = aiService.GetResponse(Message);
                    Messages.Add("AI: " + response);
                    Message = string.Empty;
                }));
            }
        }
    }
}
