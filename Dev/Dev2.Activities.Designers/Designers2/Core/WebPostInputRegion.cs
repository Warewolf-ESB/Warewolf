#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
using Caliburn.Micro;
using Dev2.Activities.Designers2.Web_Post;
using Dev2.Activities.Designers2.Web_Post_New;
using Dev2.Activities.Utils;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Network;
using Dev2.Studio.Core.Activities.Utils;
//using Microsoft.Practices.ObjectBuilder2;
using Warewolf.Data.Options;
using Warewolf.Options;

namespace Dev2.Activities.Designers2.Core
{
    public class WebPostInputRegion : IWebPostInputArea
    {
        private readonly ModelItem _modelItem;
        private readonly ISourceToolRegion<IWebServiceSource> _source;
        private string _queryString;
        private int _timeout;
        private string _requestUrl;
        private ObservableCollection<INameValue> _headers;
        private ObservableCollection<INameValue> _parameters;
        private ObservableCollection<INameValue> _settings;
        private bool _isEnabled;
        private string _postData;

        public WebPostInputRegion()
        {
            ToolRegionName = "PostInputRegion";
        }

        void SetupHeaders(ModelItem modelItem)
        {
            var existing = modelItem.GetProperty<IList<INameValue>>("Headers");
            var headers = new ObservableCollection<INameValue>();
            if(existing != null)
            {
                foreach(var header in existing)
                {
                    var nameValue = new NameValue(header.Name, header.Value);
                    nameValue.PropertyChanged += HeaderValueOnPropertyChanged;
                    headers.Add(nameValue);
                }
            }
            else
            {
                var nameValue = new NameValue();
                nameValue.PropertyChanged += HeaderValueOnPropertyChanged;
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

        public WebPostInputRegion(ModelItem modelItem, ISourceToolRegion<IWebServiceSource> source)
        {
            ToolRegionName = "PostInputRegion";
            _modelItem = modelItem;
            _source = source;
            _source.SomethingChanged += SourceOnSomethingChanged;
            IsEnabled = false;
            ParameterGroup = $"ParameterGroup{Guid.NewGuid()}";
            SetupHeaders(modelItem);
            if(source?.SelectedSource != null)
            {
                RequestUrl = source.SelectedSource.HostName;
                IsEnabled = true;
            }
            Timeout = Timeout == 0 ? 600 : Timeout;
        }

        public string ParameterGroup { get; }

        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            if(_source?.SelectedSource != null)
            {
                RequestUrl = _source.SelectedSource.HostName;
                QueryString = _source.SelectedSource.DefaultQuery;
                PostData = string.Empty;
                Headers.Clear();
                AddHeaders();
                IsEnabled = true;
                IsManualChecked = true;
            }

            OnPropertyChanged(nameof(IsEnabled));
        }

        public IWebServiceBaseViewModel ViewModel { get; set; }

        public string QueryString
        {
            get => _modelItem.GetProperty<string>("QueryString") ?? string.Empty;
            set
            {
                _queryString = value ?? string.Empty;
                _modelItem.SetProperty("QueryString", value ?? string.Empty);
                OnPropertyChanged();
            }
        }

        public int Timeout
        {
            get => _modelItem.GetProperty<int>("Timeout");
            set
            {
                _timeout = value;
                _modelItem.SetProperty("Timeout", value);
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

        public ObservableCollection<INameValue> Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                _modelItem.SetProperty("Settings", value?.ToList() ?? new List<INameValue>());
                OnPropertyChanged();
            }
        }

        public bool IsManualChecked
        {
            get => GetBoolSetting(nameof(IsManualChecked));
            set
            {
                UpdateBoolSetting(nameof(IsManualChecked), value);
                OnPropertyChanged();
            }
        }

        public bool IsFormDataChecked
        {
            get => GetBoolSetting(nameof(IsFormDataChecked));
            set
            {
                UpdateBoolSetting(nameof(IsFormDataChecked), value);
                OnPropertyChanged();
            }
        }

        public bool IsUrlEncodedChecked
        {
            get => GetBoolSetting(nameof(IsUrlEncodedChecked));
            set
            {
                UpdateBoolSetting(nameof(IsUrlEncodedChecked), value);
                OnPropertyChanged();
            }
        }

        private void UpdateSettings(string variableName, bool value)
        {
            var oldSettings = new INameValue[Settings?.Count ?? 0];
            Settings?.CopyTo(oldSettings, 0);

            var newSettings = new ObservableCollection<INameValue>();
            var currentSetting = oldSettings.ToList().FirstOrDefault(o => o.Name == variableName) ?? new NameValue(variableName, value.ToString());
            currentSetting.Value = value.ToString();
            newSettings.Add(currentSetting);

            foreach(var setting in oldSettings)
            {
                if(newSettings.FirstOrDefault(n => n.Name == setting.Name) == null)
                {
                    var newValue = value ? "false" : setting.Value;
                    newSettings.Add(new NameValue(setting.Name, newValue));
                }
            }

            var variables = new[] { nameof(IsManualChecked), nameof(IsFormDataChecked), nameof(IsUrlEncodedChecked) };
            foreach(var variable in variables)
            {
                if(newSettings.FirstOrDefault(n => n.Name == variable) == null)
                {
                    newSettings.Add(new NameValue(variable, "false"));
                }
            }

            Settings = newSettings;

            OnPropertyChanged(nameof(IsManualChecked));
            OnPropertyChanged(nameof(IsFormDataChecked));
            OnPropertyChanged(nameof(IsUrlEncodedChecked));

            if(ViewModel?.GetType() == typeof(WebPostActivityViewModelNew))
            {
                var list = new List<IOption>(((WebPostActivityViewModelNew)ViewModel).ConditionExpressionOptions.Options);
                list.ForEach(c => ((FormDataOptionConditionExpression)c).IsMultiPart = !IsUrlEncodedChecked);
            }
        }
        
        private bool GetBoolSetting(string variableName)
        {
            if(ViewModel?.GetType() == typeof(WebPostActivityViewModelNew))
            {
                var settings = _modelItem.GetProperty<IList<INameValue>>("Settings");
                return Convert.ToBoolean(settings?.FirstOrDefault(s => s.Name == variableName)?.Value);
            }
            if(_modelItem.GetProperty<bool?>(variableName) != null)
            {
                return _modelItem.GetProperty<bool>(variableName);
            }

            return false;
        }
        
        private void UpdateBoolSetting(string variableName, bool value)
        {
            if(ViewModel != null && ViewModel.GetType() == typeof(WebPostActivityViewModelNew))
            {
                UpdateSettings(variableName, value);
            }
            else
            {
                _modelItem.SetProperty(variableName, value);
                OnPropertyChanged();
            }
        }

        public string PostData
        {
            get => _modelItem.GetProperty<string>("PostData") ?? string.Empty;
            set
            {
                _postData = value ?? string.Empty;
                _modelItem.SetProperty("PostData", value ?? string.Empty);
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
            foreach(var nameValue in Headers)
            {
                var value = new NameValue(nameValue.Name, nameValue.Value);
                value.PropertyChanged += HeaderValueOnPropertyChanged;
                headers2.Add(value);
            }

            return new WebPostInputRegion(_modelItem, _source)
            {
                Headers = headers2,
                PostData = PostData,
                QueryString = QueryString,
                RequestUrl = RequestUrl,
                IsEnabled = IsEnabled,
                Settings = Settings
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if(toRestore is WebPostInputRegion region)
            {
                IsEnabled = region.IsEnabled;
                IsManualChecked = region.IsManualChecked;
                IsFormDataChecked = region.IsFormDataChecked;
                IsUrlEncodedChecked = region.IsUrlEncodedChecked;
                PostData = region.PostData;
                QueryString = region.QueryString;
                RequestUrl = region.RequestUrl;
                Headers.Clear();
                AddHeaders();
                if(region.Headers != null)
                {
                    foreach(var nameValue in region.Headers)
                    {
                        Headers.Add(
                            new ObservableAwareNameValue(Headers, s => { SetHeaders(); })
                                { Name = nameValue.Name, Value = nameValue.Value });
                    }

                    Headers.Remove(Headers.First());
                }
            }
        }

        private void AddHeaders()
        {
            if(Headers.Count == 0)
            {
                Headers.Add(new ObservableAwareNameValue(Headers, s => { SetHeaders(); }));
                return;
            }

            var nameValue = Headers.Last();
            if(!string.IsNullOrWhiteSpace(nameValue.Name) || !string.IsNullOrWhiteSpace(nameValue.Value))
            {
                Headers.Add(new ObservableAwareNameValue(Headers, s => { SetHeaders(); }));
            }
        }

        private void SetHeaders()
        {
            _modelItem.SetProperty(
                "Headers",
                _headers.Select(
                    a =>
                    {
                        var nameValue = new NameValue(a.Name, a.Value);
                        nameValue.PropertyChanged += HeaderValueOnPropertyChanged;
                        return (INameValue)nameValue;
                    }).ToList());
        }

        private void HeaderValueOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetHeaders();
            AddHeaders();
        }

        public EventHandler<List<string>> ErrorsHandler { get; set; }

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