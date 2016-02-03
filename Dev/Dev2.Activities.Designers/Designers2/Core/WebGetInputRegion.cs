using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    public class WebGetInputRegion:IWebGetInputArea
    {
        private readonly IWebServiceSource _src;
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IWebServiceSource> _source;
        private string _queryString;
        private string _requestUrl;
        private ICollection<NameValue> _headers;
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;
        //private ICommand _addRowCommand;
        //private ICommand _removeRowCommand;

        public WebGetInputRegion(IWebServiceSource src, ModelItem modelItem)
        {
            _src = src;
            _modelItem = modelItem;
            MinHeight = 150;
            MaxHeight = 150;
            CurrentHeight = 150;
            var headerCollection = new ObservableCollection<NameValue>();
            headerCollection.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = headerCollection;
            Headers.Add(new ObservableAwareNameValue(headerCollection, UpdateRequestVariables));
        }

        private void UpdateRequestVariables(string obj)
        {
        }

        private void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        public WebGetInputRegion( ModelItem modelItem,ISourceToolRegion<IWebServiceSource> source)
        {
            
            _modelItem = modelItem;
            _source = source;
            _source.SomethingChanged+= SourceOnSomethingChanged  ;
            MinHeight = 150;
            MaxHeight = 150;
            CurrentHeight = 150;
            IsVisible = false;
            var headerCollection = new ObservableCollection<NameValue>();
            headerCollection.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = headerCollection;
            Headers.Add(new ObservableAwareNameValue(headerCollection, UpdateRequestVariables));
           
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsVisible");
        }

        #region Implementation of IWebGetInputArea

        public string QueryString
        {
            get
            {
                return _queryString;
            }
            set
            {
                _queryString = value;
                OnPropertyChanged();
            }
        }
        public string RequestUrl
        {
            get
            {
                return _requestUrl;
            }
            set
            {
                _requestUrl = value;
                OnPropertyChanged();
            }
        }
        public ICollection<NameValue> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddRowCommand
        {
            get
            {
                return null;
            }
        }
        public ICommand RemoveRowCommand
        {
            get
            {
                return null;
            }
        }

        #endregion


        #region Implementation of IToolRegion

        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
                OnPropertyChanged();
            }
        }
        public double CurrentHeight
        {
            get
            {
                return _currentHeight;
            }
            set
            {
                _currentHeight = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisible
        {
            get
            {
                return  _source.SelectedSource != null;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;
                
                OnPropertyChanged();
                OnHeightChanged(this);
            }
        }
        public event HeightChanged HeightChanged;

        #endregion

        protected virtual void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if(handler != null)
            {
                handler(this, args);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}