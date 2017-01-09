/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Preview;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.Designers2.GetWebRequest.GetWebRequestWithTimeout
{
    public class GetWebRequestWithTimeoutDesignerViewModel : ActivityDesignerViewModel
    {
        public GetWebRequestWithTimeoutDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();

            PreviewViewModel = new PreviewViewModel
                {
                    InputsVisibility = Visibility.Collapsed,
                };
            PreviewViewModel.PreviewRequested += DoPreview;

            if (Url == null)
            {
                Url = string.Empty;
            }

            if (Headers == null)
            {
                Headers = string.Empty;
            }
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Utility_Web_Request;
        }

        public PreviewViewModel PreviewViewModel { get; private set; }

        public bool IsUrlFocused
        {
            get { return (bool)GetValue(IsUrlFocusedProperty); }
            set { SetValue(IsUrlFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsUrlFocusedProperty =
            DependencyProperty.Register("IsUrlFocused", typeof(bool), typeof(GetWebRequestWithTimeoutDesignerViewModel),
                                        new PropertyMetadata(false));

        // DO NOT bind to these properties - these are here for convenience only!!!
        private string Url
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        private string Headers
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public override void Validate()
        {
            Errors = null;
            if (string.IsNullOrWhiteSpace(Url) && string.IsNullOrWhiteSpace(TimeOutText))
            {
                return;
            }
            var url = GetUrl();
            if (IsValid)
            {
                ValidateUrl(url);
            }
            if (TimeOutText.Length > 0)
            {
                int res;
                if (!int.TryParse(TimeOutText, out res))
                {
                    if (!DataListUtil.IsValueRecordset(TimeOutText) && !DataListUtil.IsValueScalar(TimeOutText))
                    {
                        Errors = new List<IActionableErrorInfo>
                        {
                         new ActionableErrorInfo { ErrorType = ErrorType.Critical, Message = ErrorResource.InvalidTimeOut }};

                    }
                }
                else
                {
                    if (res < 0)
                    {
                        Errors = new List<IActionableErrorInfo>
                        {
                         new ActionableErrorInfo { ErrorType = ErrorType.Critical, Message = ErrorResource.InvalidTimeOut}};
                    }
                }
            }
        }

        private string TimeOutText
        {
            get { return GetProperty<string>(); }
            // ReSharper disable UnusedMember.Local
            set { SetProperty(value); }
            // ReSharper restore UnusedMember.Local
        }

        #region Overrides of ActivityDesignerViewModel

        protected override void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Url":
                case "Headers":
                    ExtractVariables();
                    break;
            }
        }

        #endregion

        private void ExtractVariables()
        {
            PreviewViewModel.Output = string.Empty;
            var urlVariables = DataListCleaningUtils
                .SplitIntoRegions(Url)
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var headersVariables = DataListCleaningUtils
                .SplitIntoRegions(Headers)
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var variableList = urlVariables.Concat(headersVariables).ToList();

            PreviewViewModel.CanPreview = !string.IsNullOrWhiteSpace(Url);

            if (variableList.Count > 0)
            {
                PreviewViewModel.InputsVisibility = Visibility.Visible;

                // ReSharper disable MaximumChainedReferences
                var mustRemainKeys = PreviewViewModel.Inputs
                                                     .Where(i => variableList.Contains(i.Key))
                                                     .ToList();
                // ReSharper restore MaximumChainedReferences

                // ReSharper disable MaximumChainedReferences
                var mustRemove = PreviewViewModel.Inputs
                                                 .Where(i => !variableList.Contains(i.Key))
                                                 .ToList();
                // ReSharper restore MaximumChainedReferences

                mustRemove.ForEach(r => PreviewViewModel.Inputs.Remove(r));

                mustRemainKeys.ForEach(k => variableList.Remove(k.Key));

                variableList.ForEach(v => PreviewViewModel.Inputs.Add(new ObservablePair<string, string> { Key = v }));
            }
            else
            {
                PreviewViewModel.Inputs.Clear();
                PreviewViewModel.InputsVisibility = Visibility.Collapsed;
            }
        }

        private void DoPreview(object sender, PreviewRequestedEventArgs args)
        {
            Errors = null;
            PreviewViewModel.Output = string.Empty;

            var url = GetUrl(PreviewViewModel.Inputs);
            if (IsValid)
            {
                ValidateUrl(url);
                if (IsValid)
                {
                    PreviewViewModel.Output = GetPreviewOutput(url);
                }
            }
        }

        private string GetUrl(ObservableCollection<ObservablePair<string, string>> inputs = null)
        {
            var url = Url;

            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            var isValid = true;
            var variableList = DataListCleaningUtils.SplitIntoRegions(url);
            foreach (var v in variableList)
            {
                if (v != null)
                {
                    string s;
                    if (inputs != null)
                    {
                        var input = inputs.FirstOrDefault(p => p.Key == v);
                        s = input == null ? string.Empty : input.Value;
                    }
                    else
                    {
                        s = "a"; // random text to replace variable
                    }
                    url = url.Replace(v, s);
                }
                else
                {
                    isValid = false;
                }
            }

            if (!isValid)
            {
                Errors = new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(() => IsUrlFocused = true)
                            {
                                ErrorType = ErrorType.Critical,
                                Message = ErrorResource.OpeningClosingBracketMismatch
                            }
                    };
            }
            else
            {
                if (!url.StartsWith("http://"))
                {
                    url = "http://" + url;
                }
            }

            return url;
        }

        private void ValidateUrl(string urlValue)
        {
            if (string.IsNullOrWhiteSpace(urlValue))
            {
                Errors = new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(() => IsUrlFocused = true)
                            {
                                ErrorType = ErrorType.Critical,
                                Message = "Url must have a value"
                            }
                    };
            }
            else
            {
                Uri uriResult;
                var isValid = Uri.TryCreate(urlValue, UriKind.Absolute, out uriResult) &&
                              (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!isValid)
                {
                    Errors = new List<IActionableErrorInfo>
                        {
                            new ActionableErrorInfo(() => IsUrlFocused = true)
                                {
                                    ErrorType = ErrorType.Critical,
                                    Message = "Please supply a valid url"
                                }
                        };
                }
            }
        }

        public Func<string, string, List<Tuple<string, string>>, string> WebInvoke = (method, url, headers) =>
            {
                var webInvoker = new WebRequestInvoker();
                return webInvoker.ExecuteRequest(method, url, headers);
            };

        private string GetPreviewOutput(string url)
        {
            Errors = null;
            var result = string.Empty;
            try
            {
                var headers = string.IsNullOrEmpty(Headers)
                                  ? new string[0]
                                  : Headers.Split(new[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries);

                // ReSharper disable MaximumChainedReferences
                var headersEntries = headers.Select(header => header.Split(':')).Select(headerSegments => new Tuple<string, string>(headerSegments[0], headerSegments[1])).ToList();
                // ReSharper restore MaximumChainedReferences

                url = PreviewViewModel.Inputs.Aggregate(url,
                                                        (current, previewInput) =>
                                                        current.Replace(previewInput.Key, previewInput.Value));
                result = WebInvoke("GET", url, headersEntries);
                return result;
            }
            catch (Exception ex)
            {
                Errors = new List<IActionableErrorInfo>
                    {
                        new ActionableErrorInfo(() => PreviewViewModel.IsPreviewFocused = true)
                            {
                                ErrorType = ErrorType.Critical,
                                Message = ex.Message
                            }
                    };
            }

            return result;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
