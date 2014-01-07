using Caliburn.Micro;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public class DataListVariable : PropertyChangedBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if(_name == value)
                {
                    return;
                }

                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }
    }
}
