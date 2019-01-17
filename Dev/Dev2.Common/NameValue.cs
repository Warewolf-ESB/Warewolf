using Dev2.Common.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dev2.Common
{
    public class ObservableNameValue : NameValue, INotifyPropertyChanged
    {
        public ObservableNameValue()
        {
        }
        public ObservableNameValue(string name, string value)
            :base(name, value)
        {
        }

        public override string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public override string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public override string ToString() => Name;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
