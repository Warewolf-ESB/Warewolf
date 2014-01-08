using Caliburn.Micro;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Model
{
    public class ExceptionUiModel : PropertyChangedBase
    {
        private BindableCollection<ExceptionUiModel> _exception;
        public BindableCollection<ExceptionUiModel> Exception
        {
            get
            {
                return _exception ?? (_exception = new BindableCollection<ExceptionUiModel>());
            }
            set
            {
                if(_exception == value) return;

                _exception = value;
                NotifyOfPropertyChange(() => Exception);
            }
        }

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if(_message == value) return;

                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }
    }
}
