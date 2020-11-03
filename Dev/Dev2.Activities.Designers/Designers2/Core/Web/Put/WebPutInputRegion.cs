/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Utils;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers2.Core.Web.Put
{
    public class WebPutInputRegion : IWebPutInputArea
    {
        readonly ModelItem _modelItem;
        readonly ISourceToolRegion<IWebServiceSource> _source;
        ObservableCollection<INameValue> _headers;
        bool _isEnabled;
        string _requestUrl;
        private bool _isPutDataBase64;

        public WebPutInputRegion()
        {
            ToolRegionName = "PutInputRegion";
        }

        void SetupHeaders(ModelItem modelItem)
        {
            var existing = modelItem.GetProperty<IList<INameValue>>("Headers");
            var headers = new ObservableCollection<INameValue>();
            if (existing != null)
            {
                foreach (var header in existing)
                {
                    var nameValue = new NameValue(header.Name, header.Value);
                    nameValue.PropertyChanged += ValueOnPropertyChanged;
                    headers.Add(nameValue);
                }
            }
            else
            {
                var nameValue = new NameValue();
                nameValue.PropertyChanged += ValueOnPropertyChanged;
                headers.Add(nameValue);
            }
            headers.CollectionChanged += HeaderCollectionOnCollectionChanged;
            Headers = headers;

            AddHeaders();
        }

        void HeaderCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetHeaders();
        }

        public WebPutInputRegion(ModelItem modelItem, ISourceToolRegion<IWebServiceSource> source)
        {
            ToolRegionName = "PutInputRegion";
            _modelItem = modelItem;
            _source = source;
            _source.SomethingChanged += SourceOnSomethingChanged;
            IsEnabled = false;
            SetupHeaders(modelItem);
            if (source?.SelectedSource != null)
            {
                RequestUrl = source.SelectedSource.HostName;
                IsEnabled = true;
            }
        }

        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            if (_source?.SelectedSource != null)
            {
                RequestUrl = _source.SelectedSource.HostName;
                QueryString = _source.SelectedSource.DefaultQuery;
                Headers.Clear();
                AddHeaders();
                IsEnabled = true;
            }

            OnPropertyChanged(nameof(IsEnabled));
        }

        public string QueryString
        {
            get => _modelItem.GetProperty<string>("QueryString") ?? string.Empty;
            set
            {
                _modelItem.SetProperty("QueryString", value ?? string.Empty);
                OnPropertyChanged();
            }
        }
        public string RequestUrl
        {
            get => _requestUrl;
            set
            {
                _requestUrl = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<INameValue> Headers
        {
            get => _headers;
            set
            {
                _headers = value;
                _modelItem.SetProperty("Headers", value.ToList());
                OnPropertyChanged();
            }
        }
        public string PutData
        {
            get => _modelItem?.GetProperty<string>("PutData") ?? string.Empty;
            set
            {
                _modelItem.SetProperty("PutData", value ?? string.Empty);
                OnPropertyChanged();
            }
        }

        public bool IsPutDataBase64
        {
            get => _modelItem?.GetProperty<bool>("IsPutDataBase64") ?? _isPutDataBase64; 
            set
            {
                if (_modelItem is null && value != _isPutDataBase64)
                {
                    _isPutDataBase64 = value;
                    _modelItem.SetProperty("IsPutDataBase64", value); 
                    OnPropertyChanged();
                    return;
                }

                _modelItem.SetProperty("IsPutDataBase64", value);
                OnPropertyChanged();
            }
        }


        public string ToolRegionName { get; set; }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            var headers2 = new ObservableCollection<INameValue>();
            foreach (var nameValue in Headers)
            {
                var value = new NameValue(nameValue.Name, nameValue.Value);
                value.PropertyChanged += ValueOnPropertyChanged;
                headers2.Add(value);
            }
            return new WebPutInputRegion(_modelItem, _source)
            {
                Headers = headers2,
                QueryString = QueryString,
                RequestUrl = RequestUrl,
                IsEnabled = IsEnabled,
                PutData = PutData,
                IsPutDataBase64 = IsPutDataBase64
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is WebPutInputRegion region)
            {
                IsEnabled = region.IsEnabled;
                IsPutDataBase64 = region.IsPutDataBase64;
                QueryString = region.QueryString;
                RequestUrl = region.RequestUrl;
                Headers.Clear();
                AddHeaders();
                if (region.Headers != null)
                {
                    foreach (var nameValue in region.Headers)
                    {
                        Headers.Add(new ObservableAwareNameValue(Headers, s => { SetHeaders(); })
                        { Name = nameValue.Name, Value = nameValue.Value });
                    }
                    Headers.Remove(Headers.First());
                }
            }
        }

        private void AddHeaders()
        {
            if (Headers.Count == 0)
            {
                Headers.Add(new ObservableAwareNameValue(Headers, s => { SetHeaders(); }));
                return;
            }
            var nameValue = Headers.Last();
            if (!string.IsNullOrWhiteSpace(nameValue.Name) || !string.IsNullOrWhiteSpace(nameValue.Value))
            {
                Headers.Add(new ObservableAwareNameValue(Headers, s => { SetHeaders(); }));
            }
        }

        private void SetHeaders()
        {
            _modelItem.SetProperty("Headers",
                _headers.Select(a =>
                {
                    var nameValue = new NameValue(a.Name, a.Value);
                    nameValue.PropertyChanged += ValueOnPropertyChanged;
                    return (INameValue)nameValue;
                }).ToList());
        }

        private void ValueOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetHeaders();
            AddHeaders();
        }
        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}