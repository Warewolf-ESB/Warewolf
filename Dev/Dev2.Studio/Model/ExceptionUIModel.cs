using Caliburn.Micro;

namespace Dev2.Studio.Model
{
    public class ExceptionUIModel : PropertyChangedBase
    {
        private BindableCollection<ExceptionUIModel> _exception;
        public BindableCollection<ExceptionUIModel> Exception
        {
            get
            {
                return _exception ?? (_exception = new BindableCollection<ExceptionUIModel>());
            }
            set
            {
                if (_exception == value) return;

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
                if (_message == value) return;

                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }
    }
}
