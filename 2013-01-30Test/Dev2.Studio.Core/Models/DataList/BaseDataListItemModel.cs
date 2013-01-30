using Dev2.Studio.Core.Interfaces.DataList;
using System.ComponentModel;

namespace Dev2.Studio.Core.Models.DataList
{
    public abstract class BaseDataListItemModel : INotifyPropertyChanged
    {
        #region Fields

        private string _name;
        private string _displayName;
        private bool _isExpanded = true;
        private OptomizedObservableCollection<IDataListItemModel> _children;

        #endregion Fields

        #region Properties

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                Name = value;
                OnPropertyChanged("DisplayName");
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = ValidateName(value);
                OnPropertyChanged("Name");
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        public OptomizedObservableCollection<IDataListItemModel> Children
        {
            get { return _children ?? (_children = new OptomizedObservableCollection<IDataListItemModel>()); }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        #endregion Properties

        #region Abstract Methods

        public abstract string ValidateName(string name);

        #endregion Abstract Methods

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            //this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}
