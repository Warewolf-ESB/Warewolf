using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dev2
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool OnPropertyChanged<T>(ref T propertyValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            var propertyChanged = !EqualityComparer<T>.Default.Equals(propertyValue, newValue);
            if(propertyChanged)
            {
                propertyValue = newValue;
                OnPropertyChanged(propertyName);
            }
            return propertyChanged;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }



    }
}