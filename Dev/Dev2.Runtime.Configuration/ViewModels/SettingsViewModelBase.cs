using Caliburn.Micro;
using Dev2.Runtime.Configuration.Services;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public abstract class SettingsViewModelBase : Screen
    {
        #region Fields

        private object _object;

        #endregion

        #region events

        public delegate void UnderlyingObjectChangedHandler();
        public event UnderlyingObjectChangedHandler UnderlyingObjectChanged;

        protected void OnUnderlyingObjectChanged()
        {
            if(UnderlyingObjectChanged != null)
            {
                UnderlyingObjectChanged();
            }
        }

        #endregion events

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
                NotifyOfPropertyChange(() => Object);
                OnUnderlyingObjectChanged();
            }
        }

        public ICommunicationService CommunicationService { get; set; }

        #endregion

    }
}
