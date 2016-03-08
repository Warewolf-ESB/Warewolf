﻿using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;
// ReSharper disable NotAccessedField.Local

namespace Dev2.Activities.Designers2.Core.Web.Put
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class WebPutInputRegion : IWebPutInputArea
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IWebServiceSource> _source;
        private ObservableCollection<INameValue> _headers;
        bool _isVisible;
        private string _queryString;
        private string _requestUrl;
        private string _putData;

        public WebPutInputRegion()
        {
            ToolRegionName = "PutInputRegion";
        }

        public WebPutInputRegion(ModelItem modelItem, ISourceToolRegion<IWebServiceSource> source)
        {
            ToolRegionName = "PutInputRegion";
            _modelItem = modelItem;
            _source = source;
            _source.SomethingChanged += SourceOnSomethingChanged;
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
        }

        private void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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
        public bool IsVisible
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

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            //var ser = new Dev2JsonSerializer();
            //return ser.Deserialize<IToolRegion>(ser.SerializeToBuilder(this));
            var headers2 = new ObservableCollection<INameValue>();
            foreach (var nameValue in Headers)
            {
                headers2.Add(new NameValue(nameValue.Name, nameValue.Value));
            }
            return new WebPutRegionClone()
            {
                Headers = headers2,
                QueryString = QueryString,
                RequestUrl = RequestUrl,
                IsVisible = IsVisible
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as WebPutRegionClone;
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
            }
        }

        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        #endregion

        #region Implementation of IWebPutInputArea

        public string QueryString
        {
            get
            {
                return _modelItem.GetProperty<string>("QueryString") ?? string.Empty;
            }
            set
            {
                _queryString = value ?? string.Empty;
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
        
        public string PutData
        {
            get
            {
                return _modelItem.GetProperty<string>("PutData") ?? string.Empty;
            }
            set
            {
                _putData = value ?? string.Empty;
                _modelItem.SetProperty("PutData", value ?? string.Empty);
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

        #endregion
    }
}