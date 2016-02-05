using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers2.Core
{
    public class WebGetInputRegion:IWebGetInputArea
    {
        private readonly IWebServiceSource _src;
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IWebServiceSource> _source;
        private string _queryString;
        private string _requestUrl;
        private ObservableCollection<INameValue> _headers;
        private double _minHeight;
        private double _currentHeight;
        private bool _isVisible;
        private double _maxHeight;
        private string _headerText;
        private double _headersHeight;
        private const double baseHeight = 150;
        //private ICommand _addRowCommand;
        //private ICommand _removeRowCommand;

        public WebGetInputRegion(IWebServiceSource src, ModelItem modelItem)
        {
            _src = src;
            _modelItem = modelItem;
            MinHeight = 150;
            MaxHeight = 150;
            CurrentHeight = 150;
            IsVisible = false;

            SetupHeaders(modelItem);
            QueryString = "";
        }

        private void SetupHeaders(ModelItem modelItem)
        {
            var existing = modelItem.GetProperty<IList<INameValue>>("Headers");
            var headerCollection = new ObservableCollection<INameValue>(existing ?? new List<INameValue>());
            headerCollection.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = headerCollection;
            Headers.Add(new NameValue());
        }

        private void UpdateRequestVariables(string obj)
        {
        }

        private void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            MaxHeight  = baseHeight+ Headers.Count * 45; ;
            HeadersHeight = 15 +Headers.Count * 45;
            _modelItem.SetProperty("Headers", _headers.ToList());
           
        }

        public WebGetInputRegion( ModelItem modelItem,ISourceToolRegion<IWebServiceSource> source)
        {
            
            _modelItem = modelItem;
            _source = source;
            _source.SomethingChanged+= SourceOnSomethingChanged  ;
            MinHeight = 300;
            MaxHeight = 300;
            CurrentHeight = 300;
            IsVisible = false;
            SetupHeaders(modelItem);
            RequestUrl = source.SelectedSource.HostName;
           
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if(_source != null && _source.SelectedSource != null)
            {
                RequestUrl = _source.SelectedSource.HostName;
                QueryString = _source.SelectedSource.DefaultQuery;
            }
            OnPropertyChanged(@"IsVisible");
            OnHeightChanged(this);
        }

        #region Implementation of IWebGetInputArea

        public string QueryString
        {
            get
            {
                return _modelItem.GetProperty<string>("QueryString");
            }
            set
            {
                _queryString = value;
                _modelItem.SetProperty("QueryString",value);
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
        public ObservableCollection<INameValue> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
                _modelItem.SetProperty("Headers", value.ToList());
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
        public string HeaderText
        {
            get
            {
                return _headerText;
            }
            set
            {
                _headerText = value;
                OnPropertyChanged();
            }
        }
        public double HeadersHeight
        {
            get
            {
                return _headersHeight;
            }
            set
            {
                _headersHeight = value;
                OnPropertyChanged();
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