using Caliburn.Micro;
using Dev2.Activities.Help;

namespace Dev2.Activities.Adorners
{
    public class HelpViewModel : Screen
    {
        private string _helpText;
        public string HelpText
        {
            get
            {
                return _helpText;
            }
            set
            {
                if (_helpText == value)
                {
                    return;
                }

                _helpText = value;
                NotifyOfPropertyChange(() => HelpText);
            }
        }

        public override object GetView(object context = null)
        {
            return new HelpView();
        }
    }
}
