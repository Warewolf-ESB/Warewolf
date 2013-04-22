
using System.ComponentModel;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public abstract class SettingsViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Impl

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Fields

        private object _object;

        #endregion

        #region Properties

        public object Object
        {
            get
            {
                return _object;
            }
            set
            {
                _object = value;
                OnPropertyChanged("DataContext");
            }
        }

        #endregion
    }
}
