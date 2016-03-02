namespace Dev2.Activities.Designers2.Core.Web.Base
{
    /*   public abstract class WebRegionInputBase<TRegionClone>
        where TRegionClone : IToolRegion, new()
    {
        private const double BaseHeight = 165;
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IWebServiceSource> _source;
        private double _currentHeight;
        private ObservableCollection<INameValue> _headers;
        private double _headersHeight;
        bool _isVisible;
        double _maxHeadersHeight;
        private double _maxHeight;
        private double _minHeight;
        private string _queryString;
        private string _requestUrl;

        protected WebRegionInputBase(WebTooRegionDisplayInfo regionDisplayInfo)
        {
            ToolRegionName = regionDisplayInfo.ToolRegionName;
            SetInitialHeight();
        }

        protected WebRegionInputBase(ModelItem modelItem, ISourceToolRegion<IWebServiceSource> source)
        {
            ToolRegionName = "DeleteInputRegion";
            _modelItem = modelItem;
            _source = source;
            _source.SomethingChanged += SourceOnSomethingChanged;
            SetInitialHeight();
            IsVisible = false;
            SetupHeaders(modelItem);
            if (source != null && source.SelectedSource != null)
            {
                RequestUrl = source.SelectedSource.HostName;
                IsVisible = true;
            }
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            if (_source != null && _source.SelectedSource != null)
            {
                RequestUrl = _source.SelectedSource.HostName;
                QueryString = _source.SelectedSource.DefaultQuery;
                Headers.Clear();
                Headers.Add(new ObservableAwareNameValue(Headers, s =>
                {
                    _modelItem.SetProperty("Headers",
                        _headers.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
                }));
                IsVisible = true;
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"IsVisible");
            OnHeightChanged((IRegionClone)this);
        }

        private void SetupHeaders(ModelItem modelItem)
        {
            var existing = modelItem.GetProperty<IList<INameValue>>("Headers");
            var headerCollection = new ObservableCollection<INameValue>(existing ?? new List<INameValue>());
            headerCollection.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = headerCollection;

            if (Headers.Count == 0)
            {
                Headers.Add(new ObservableAwareNameValue(Headers, s =>
                {
                    _modelItem.SetProperty("Headers",
                        _headers.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
                }));
            }
            else
            {
                var nameValue = Headers.Last();
                if (!string.IsNullOrWhiteSpace(nameValue.Name) || !string.IsNullOrWhiteSpace(nameValue.Value))
                {
                    Headers.Add(new ObservableAwareNameValue(Headers, s =>
                    {
                        _modelItem.SetProperty("Headers",
                            _headers.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
                    }));
                }
            }
            ResetInputsHeight();
        }

        private void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ResetInputsHeight();
            _modelItem.SetProperty("Headers", _headers.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public virtual double MinHeight
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
        public virtual double CurrentHeight
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
        public virtual bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        public virtual double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;

                OnPropertyChanged();

                OnHeightChanged((IRegionClone)this);
            }
        }

        public virtual IList<IToolRegion> Dependants { get; set; }

        public virtual IToolRegion CloneRegion()
        {
            var headers2 = new ObservableCollection<INameValue>();
            foreach (var nameValue in Headers)
            {
                headers2.Add(new NameValue(nameValue.Name, nameValue.Value));
            }
            var regionClone = (IRegionClone)new TRegionClone();

            regionClone.Headers = headers2;
            regionClone.QueryString = QueryString;
            regionClone.RequestUrl = RequestUrl;
            regionClone.IsVisible = IsVisible;

            return regionClone;

        }



        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        public virtual void RestoreRegion(IRegionClone toRestore)
        {
            var region = toRestore;
            if (region != null)
            {
                IsVisible = region.IsVisible;
                QueryString = region.QueryString;
                RequestUrl = region.RequestUrl;
                Headers.Clear();
                Headers.Add(new ObservableAwareNameValue(Headers, s =>
                {
                    _modelItem.SetProperty("Headers",
                        _headers.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
                }));
                if (region.Headers != null)
                {
                    foreach (var nameValue in region.Headers)
                    {
                        Headers.Add(new ObservableAwareNameValue(Headers, s =>
                        {
                            _modelItem.SetProperty("Headers",
                                _headers.Select(a => new NameValue(a.Name, a.Value) as INameValue).ToList());
                        }) { Name = nameValue.Name, Value = nameValue.Value });
                    }
                    Headers.Remove(Headers.First());
                }

                ResetInputsHeight();
            }
        }

        public virtual IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        private void SetInitialHeight()
        {
            MinHeight = BaseHeight;
            MaxHeight = BaseHeight;
            CurrentHeight = BaseHeight;
            MaxHeadersHeight = BaseHeight;
        }

        protected virtual void ResetInputsHeight()
        {
            SetInitialHeight();
            HeadersHeight = GlobalConstants.RowHeaderHeight + Headers.Count * GlobalConstants.RowHeight;
            MaxHeadersHeight = HeadersHeight;
            if (Headers.Count >= 3)
            {
                MinHeight = BaseHeight + GlobalConstants.RowHeaderHeight + GlobalConstants.RowHeight;
                MaxHeight = BaseHeight + GlobalConstants.RowHeaderHeight + GlobalConstants.RowHeight;
                MaxHeadersHeight = 120;
                CurrentHeight = MinHeight;
            }
            else
            {
                var count = 0;
                if (Headers.Count > 0)
                {
                    // Remove the header from the count
                    count = Headers.Count - 1;
                }
                CurrentHeight = GlobalConstants.RowHeaderHeight + count * GlobalConstants.RowHeight;
                if (CurrentHeight < BaseHeight)
                {
                    CurrentHeight = BaseHeight;
                    // Check if the count is greater than 0 before adding the RowHeight
                    if (count > 0)
                    {
                        CurrentHeight += GlobalConstants.RowHeight;
                    }
                }
                MinHeight = CurrentHeight;
                MaxHeight = CurrentHeight;
                OnHeightChanged((IRegionClone)this);
            }
        }

        public event HeightChanged HeightChanged;

        private void OnHeightChanged(IToolRegion args)
        {
            //var handler = Common.Interfaces.ToolBase.HeightChanged;
            //if (handler != null)
            //{
            //    handler(this, args);
            //}
        }

        #endregion

        #region Implementation of IWebInputArea

        public string QueryString
        {
            get
            {
                return _modelItem.GetProperty<string>("QueryString") ?? string.Empty;
            }
            set
            {
                _queryString = value ?? String.Empty;
                _modelItem.SetProperty("QueryString", value ?? string.Empty);
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
        public double MaxHeadersHeight
        {
            get
            {
                return _maxHeadersHeight;
            }
            set
            {
                _maxHeadersHeight = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }*/
}