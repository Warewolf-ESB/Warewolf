using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Dev2.Activities.Preview;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;

namespace Dev2.Activities.Designers.DsfGetWebRequest
{
    public class DsfGetWebRequestActivityViewModel : ActivityViewModelBase, IHasActivityViewModelBase
    {
        bool _isUrlFocused;

        public DsfGetWebRequestActivityViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            PreviewViewModel = new PreviewViewModel
            {
                InputsVisibility = Visibility.Collapsed,
            };
            PreviewViewModel.PreviewRequested += DoPreview;
        }

        #region Overrides of ActivityViewModelBase

        public override IEnumerable<IErrorInfo> ValidationErrors()
        {
            return ValidationErrorsImpl().ToList();
        }

        #endregion

        #region Properties

        public PreviewViewModel PreviewViewModel { get; set; }

        public string Url
        {
            get
            {
                var value = ModelItemUtils.GetProperty("Url", ModelItem) as string;
                return value;
            }
            set
            {
                ModelItemUtils.SetProperty("Url", value, ModelItem);
                NotifyOfPropertyChange(() => Url);
                ExtractVariables();
            }
        }

        public string Headers
        {
            get
            {
                var value = ModelItemUtils.GetProperty("Headers", ModelItem) as string;
                return value;
            }
            set
            {
                ModelItemUtils.SetProperty("Headers", value, ModelItem);
                NotifyOfPropertyChange(() => Headers);
                ExtractVariables();
            }
        }
        public string Result
        {
            get
            {
                var value = ModelItemUtils.GetProperty("Result", ModelItem) as string;
                return value;
            }
            set
            {
                ModelItemUtils.SetProperty("Result", value, ModelItem);
                NotifyOfPropertyChange(() => Result);
            }
        }

        public bool IsUrlFocused
        {
            get { return _isUrlFocused; }
            set
            {
                _isUrlFocused = value;
                NotifyOfPropertyChange(() => IsUrlFocused);
            }
        }
        public IActivityViewModelBase ActivityViewModelBase { get { return this; } }

        void ExtractVariables()
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

            if(variableList.Count > 0)
            {
                PreviewViewModel.InputsVisibility = Visibility.Visible;

                var mustRemainKeys = PreviewViewModel.Inputs
                                                     .Where(i => variableList.Contains(i.Key))
                                                     .ToList();

                var mustRemove = PreviewViewModel.Inputs
                                                 .Where(i => !variableList.Contains(i.Key))
                                                 .ToList();

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

        #endregion

        #region DoPreview

        private void DoPreview(object sender, PreviewRequestedEventArgs args)
        {
            PreviewViewModel.Output = string.Empty;
            var variableList = DataListCleaningUtils.SplitIntoRegions(Url);
            var previewUrl = Url;
            variableList.ForEach(v => previewUrl = previewUrl.Replace(v, PreviewViewModel.Inputs.Single(p => p.Key == v).Value));
            var errors = ValidateUrl(previewUrl);
            if(!errors.Any())
            {
                PreviewViewModel.Output = GetPreviewOutput(previewUrl);
            }
        }

        IEnumerable<IActionableErrorInfo> ValidationErrorsImpl()
        {
            return ValidateUrl(Url);
        }

        IEnumerable<IActionableErrorInfo> ValidateUrl(string urlValue)
        {
            var errors = new List<IActionableErrorInfo>();

            if(string.IsNullOrWhiteSpace(urlValue))
            {
                errors.Add(new ActionableErrorInfo(() => IsUrlFocused = true) { ErrorType = ErrorType.Critical, Message = "Url must have a value" });
            }
            else
            {
                if(!(urlValue.Contains("[[") && urlValue.Contains("]]")))
                {

                    Uri uriResult;

                    // we need to remove for validation ;)
                    var testUrl = urlValue.Replace("http://", "");

                    if(Uri.TryCreate(testUrl, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                    {
                        errors.Add(new ActionableErrorInfo(() => IsUrlFocused = true) { ErrorType = ErrorType.Critical, Message = "Please supply a valid url" });
                    }
                }
            }
            SetHelpErrors(errors);
            return errors;
        }

        public Func<string, string, string> WebInvoke = (method, url) =>
        {
            var webInvoker = new WebRequestInvoker();
            return webInvoker.ExecuteRequest(method, url);
        };

        private string GetPreviewOutput(string url)
        {
            HelpViewModel.Errors.Clear();
            string result = "";
            try
            {
                url = PreviewViewModel.Inputs.Aggregate(url, (current, previewInput) => current.Replace(previewInput.Key, previewInput.Value));
                result = WebInvoke("GET", url);
                return result;
            }
            catch(Exception ex)
            {
                var errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(() => PreviewViewModel.IsPreviewFocused = true) { ErrorType = ErrorType.Critical, Message = ex.Message } };
                HelpViewModel.Errors = errors;
            }

            return result;
        }

        void SetHelpErrors(List<IActionableErrorInfo> errors)
        {
            if(HelpViewModel != null)
            {
                HelpViewModel.Errors = errors;
            }
        }

        #endregion
    }
}