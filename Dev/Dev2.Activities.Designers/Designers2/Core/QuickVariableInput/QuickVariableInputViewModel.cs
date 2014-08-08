using Dev2.Activities.Preview;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.StringTokenizer.Interfaces;
using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace Dev2.Activities.Designers2.Core.QuickVariableInput
{
    public class QuickVariableInputViewModel : DependencyObject, IClosable, IValidator, IErrorsSource
    {
        public const string SplitTypeIndex = "Index";
        public const string SplitTypeChars = "Chars";
        public const string SplitTypeNewLine = "New Line";
        public const string SplitTypeSpace = "Space";
        public const string SplitTypeTab = "Tab";

        readonly Action<IEnumerable<string>, bool> _addToCollection;

        readonly PreviewViewModel _previewViewModel;
        readonly List<IErrorInfo> _tokenizerValidationErrors = new List<IErrorInfo>();

        #region CTOR

        public QuickVariableInputViewModel(Action<IEnumerable<string>, bool> addToCollection)
        {
            VerifyArgument.IsNotNull("addToCollection", addToCollection);
            _addToCollection = addToCollection;

            IsClosed = true;
            IsValid = true;
            IsSplitTokenEnabled = true;
            SplitTypeList = new List<string> { SplitTypeIndex, SplitTypeChars, SplitTypeNewLine, SplitTypeSpace, SplitTypeTab };
            VariableListString = string.Empty;
            SplitType = "Chars";
            SplitToken = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;

            ClearCommand = new DelegateCommand(DoClear);
            AddCommand = new RelayCommand(DoAdd, o => CanAdd);

            _previewViewModel = new PreviewViewModel
            {
                InputsVisibility = Visibility.Collapsed
            };
            PreviewViewModel.PreviewRequested += DoPreview;
        }

        #endregion

        #region Properties

        public List<IActionableErrorInfo> Errors
        {
            get { return (List<IActionableErrorInfo>)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }

        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(List<IActionableErrorInfo>), typeof(QuickVariableInputViewModel), new PropertyMetadata(null, OnErrorsChanged));

        static void OnErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (QuickVariableInputViewModel)d;
            var errors = e.NewValue as IList<IActionableErrorInfo>;
            var isValid = errors == null || errors.Count == 0;
            viewModel.IsValid = isValid;
        }
        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedProperty, value); }
        }

        public static readonly DependencyProperty IsClosedProperty =
            DependencyProperty.Register("IsClosed", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(true, OnIsClosedChanged));

        static void OnIsClosedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (QuickVariableInputViewModel)d;
            var isClosed = (bool)e.NewValue;
            if(isClosed)
            {
                viewModel.DoClear(null);
            }
        }

        public bool IsSplitOnFocused
        {
            get { return (bool)GetValue(IsSplitOnFocusedProperty); }
            set { SetValue(IsSplitOnFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsSplitOnFocusedProperty =
            DependencyProperty.Register("IsSplitOnFocused", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(false));

        public bool IsSuffixFocused
        {
            get { return (bool)GetValue(IsSuffixFocusedProperty); }
            set { SetValue(IsSuffixFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsSuffixFocusedProperty =
            DependencyProperty.Register("IsSuffixFocused", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(false));

        public bool IsPrefixFocused
        {
            get { return (bool)GetValue(IsPrefixFocusedProperty); }
            set { SetValue(IsPrefixFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsPrefixFocusedProperty =
            DependencyProperty.Register("IsPrefixFocused", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(false));

        public bool IsVariableListFocused
        {
            get { return (bool)GetValue(IsVariableListFocusedProperty); }
            set { SetValue(IsVariableListFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsVariableListFocusedProperty =
            DependencyProperty.Register("IsVariableListFocused", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(false));

        public List<string> SplitTypeList
        {
            get { return (List<string>)GetValue(SplitTypeListProperty); }
            set { SetValue(SplitTypeListProperty, value); }
        }

        public static readonly DependencyProperty SplitTypeListProperty =
            DependencyProperty.Register("SplitTypeList", typeof(List<string>), typeof(QuickVariableInputViewModel), new PropertyMetadata(null));

        public string VariableListString
        {
            get { return (string)GetValue(VariableListStringProperty); }
            set { SetValue(VariableListStringProperty, value); }
        }

        public static readonly DependencyProperty VariableListStringProperty =
            DependencyProperty.Register("VariableListString", typeof(string), typeof(QuickVariableInputViewModel), new PropertyMetadata(null, OnUiStateChanged));

        public string SplitType
        {
            get { return (string)GetValue(SplitTypeProperty); }
            set { SetValue(SplitTypeProperty, value); }
        }

        public static readonly DependencyProperty SplitTypeProperty =
            DependencyProperty.Register("SplitType", typeof(string), typeof(QuickVariableInputViewModel), new PropertyMetadata(null, OnSplitTypeChanged));

        static void OnSplitTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (QuickVariableInputViewModel)d;
            viewModel.SplitTypeChanged();
        }

        public string SplitToken
        {
            get { return (string)GetValue(SplitTokenProperty); }
            set { SetValue(SplitTokenProperty, value); }
        }

        public static readonly DependencyProperty SplitTokenProperty =
            DependencyProperty.Register("SplitToken", typeof(string), typeof(QuickVariableInputViewModel), new PropertyMetadata(null, OnUiStateChanged));

        public string Prefix
        {
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register("Prefix", typeof(string), typeof(QuickVariableInputViewModel), new PropertyMetadata(null, OnUiStateChanged));

        public string Suffix
        {
            get { return (string)GetValue(SuffixProperty); }
            set { SetValue(SuffixProperty, value); }
        }

        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register("Suffix", typeof(string), typeof(QuickVariableInputViewModel), new PropertyMetadata(null, OnUiStateChanged));

        public bool Overwrite
        {
            get { return (bool)GetValue(OverwriteProperty); }
            set { SetValue(OverwriteProperty, value); }
        }

        public static readonly DependencyProperty OverwriteProperty =
            DependencyProperty.Register("Overwrite", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(false));

        public bool IsOverwriteEnabled
        {
            get { return (bool)GetValue(IsOverwriteEnabledProperty); }
            set { SetValue(IsOverwriteEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsOverwriteEnabledProperty =
            DependencyProperty.Register("IsOverwriteEnabled", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(true));

        public bool IsSplitTokenEnabled
        {
            get { return (bool)GetValue(IsSplitTokenEnabledProperty); }
            set { SetValue(IsSplitTokenEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsSplitTokenEnabledProperty =
            DependencyProperty.Register("IsSplitTokenEnabled", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(true));

        public bool CanAdd
        {
            get { return (bool)GetValue(CanAddProperty); }
            set { SetValue(CanAddProperty, value); }
        }

        public static readonly DependencyProperty CanAddProperty =
            DependencyProperty.Register("CanAdd", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(false));

        public bool RemoveEmptyEntries
        {
            get { return (bool)GetValue(RemoveEmptyEntriesProperty); }
            set { SetValue(RemoveEmptyEntriesProperty, value); }
        }

        public static readonly DependencyProperty RemoveEmptyEntriesProperty =
            DependencyProperty.Register("RemoveEmptyEntries", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(true));

        public PreviewViewModel PreviewViewModel
        {
            get
            {
                return _previewViewModel;
            }
        }

        static void OnUiStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (QuickVariableInputViewModel)d;
            viewModel.UpdateUiState();
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; private set; }

        public ICommand ClearCommand { get; private set; }

        #endregion

        #region DoPreview

        protected virtual void DoPreview(object sender, PreviewRequestedEventArgs args)
        {
            PreviewViewModel.Output = string.Empty;
            Validate();

            if(IsValid)
            {
                PreviewViewModel.Output = GetPreviewOutput();
            }
        }

        protected virtual string GetPreviewOutput()
        {
            UpdatePreviewViewModelInputs();

            const int MaxCount = 3;
            var count = 1;

            var result = string.Join(Environment.NewLine, PreviewViewModel.Inputs.Take(MaxCount).Select(input => string.Format("{0} {1}", count++, input.Key)));
            if(PreviewViewModel.Inputs.Count > MaxCount)
            {
                result = string.Join(Environment.NewLine, new[] { result, "..." });
            }

            return result;
        }

        #endregion

        #region DoClear

        protected virtual void DoClear(object o)
        {
            SplitType = "Chars";
            SplitToken = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
            VariableListString = string.Empty;
            Overwrite = false;
            PreviewViewModel.Output = string.Empty;
        }

        #endregion

        #region DoAdd

        protected virtual void DoAdd(object o)
        {
            // Consumer needs to Validate() before calling this method!
            UpdatePreviewViewModelInputs();

            var inputs = PreviewViewModel.Inputs.Select(input => input.Key);
            var enumerable  = inputs as IList<string> ?? inputs.ToList();
            EventPublishers.Aggregator.Publish(new AddStringListToDataListMessage(enumerable.ToList()));
            _addToCollection(enumerable, Overwrite);
            DoClear(o);
        }

        #endregion

        #region UpdatePreviewViewModelInputs

        void UpdatePreviewViewModelInputs()
        {
            var tokenizer = CreateTokenizer();
            DataListUtil.UpsertTokens(PreviewViewModel.Inputs, tokenizer, Prefix, Suffix, RemoveEmptyEntries);
        }

        #endregion

        #region CreateTokenizer

        IDev2Tokenizer CreateTokenizer()
        {
            _tokenizerValidationErrors.Clear();

            var stringToSplit = VariableListString;
            var splitType = SplitType;
            var at = SplitToken;

            if(string.IsNullOrWhiteSpace(stringToSplit))
            {
                return null;
            }

            var dtb = new Dev2TokenizerBuilder { ToTokenize = stringToSplit };

            switch(splitType)
            {
                case "Index":
                    if(!string.IsNullOrEmpty(at))
                    {
                        // No need for try..parse as ValidationErrors() function checks this!
                        var indexNum = int.Parse(at);
                        dtb.AddIndexOp(indexNum);
                    }
                    break;

                case "Space":
                    dtb.AddTokenOp(" ", false);
                    break;

                case "Tab":
                    dtb.AddTokenOp("\t", false);
                    break;

                case "New Line":
                    if(stringToSplit.Contains("\r\n"))
                    {
                        dtb.AddTokenOp("\r\n", false);
                    }
                    else if(stringToSplit.Contains("\n"))
                    {
                        dtb.AddTokenOp("\n", false);
                    }
                    else if(stringToSplit.Contains("\r"))
                    {
                        dtb.AddTokenOp("\r", false);
                    }
                    else
                    {
                        // Assume environment
                        dtb.AddTokenOp(Environment.NewLine, false);
                    }
                    break;

                case "Chars":
                    if(!string.IsNullOrEmpty(at))
                    {
                        dtb.AddTokenOp(at, false);
                    }
                    break;
            }


            try
            {
                return dtb.Generate();
            }
            catch(Exception ex)
            {
                _tokenizerValidationErrors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = ex.Message });
            }

            return null;
        }

        #endregion

        #region Implementation of IValidator

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { SetValue(IsValidProperty, value); }
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(QuickVariableInputViewModel), new PropertyMetadata(true));

        public virtual void Validate()
        {
            Errors = ValidationErrorsImpl().ToList();
        }

        IEnumerable<IActionableErrorInfo> ValidationErrorsImpl()
        {
            if(string.IsNullOrWhiteSpace(VariableListString))
            {
                var doFocused = new Action(() => { IsVariableListFocused = true; });
                yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Variable List String can not be blank/empty" };
            }

            switch(SplitType)
            {
                case "Index":
                    foreach(var error in ValidationErrorsForIndexSplit())
                    {
                        yield return error;
                    }
                    break;

                case "Chars":
                    foreach(var error in ValidationErrorsForCharsSplit())
                    {
                        yield return error;
                    }
                    break;
            }

            if(!string.IsNullOrEmpty(Prefix) && !IsValidRecordsetPrefix(Prefix))
            {
                var doFocused = new Action(() => { IsPrefixFocused = true; });
                yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Prefix contains invalid characters" };
            }

            if(!string.IsNullOrEmpty(Suffix) && !IsValidName(Suffix))
            {
                var doFocused = new Action(() => { IsSuffixFocused = true; });

                yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Suffix contains invalid characters" };
            }
        }

        IEnumerable<IActionableErrorInfo> ValidationErrorsForIndexSplit()
        {
            var doFocused = new Action(() => { IsSplitOnFocused = true; });

            if(!SplitToken.IsWholeNumber())
            {
                yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Please supply a whole positive number for an Index split" };
            }
            else
            {
                int indexToSplitOn;
                if(!int.TryParse(SplitToken, out indexToSplitOn))
                {
                    double doubleToSplitOn;
                    if(double.TryParse(SplitToken, out doubleToSplitOn))
                    {
                        yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Please supply a number less then 2,147,483,647 for an Index split" };
                    }
                    else
                    {
                        yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Please supply a whole positive number for an Index split" };
                    }
                }

                if(indexToSplitOn < 1)
                {
                    yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Please supply a whole positive number for an Index split" };
                }
            }
        }

        IEnumerable<IActionableErrorInfo> ValidationErrorsForCharsSplit()
        {
            var doFocused = new Action(() => { IsSplitOnFocused = true; });

            if(string.IsNullOrEmpty(SplitToken))
            {
                yield return new ActionableErrorInfo(doFocused) { ErrorType = ErrorType.Critical, Message = "Please supply a value for a Character split" };
            }
        }

        static bool IsValidRecordsetPrefix(string value)
        {
            if(value.Contains("(") && value.Contains(")."))
            {
                var startIndex = value.IndexOf("(", StringComparison.Ordinal) + 1;
                var endIndex = value.LastIndexOf(").", StringComparison.Ordinal);

                var tmp = value.Substring(startIndex, endIndex - startIndex);
                var idxNum = 1;
                if(tmp != "*" && !string.IsNullOrEmpty(tmp) && !int.TryParse(tmp, out idxNum))
                {
                    return false;
                }
                if(idxNum < 1)
                {
                    return false;
                }
                value = value.Replace("(" + tmp + ").", string.Empty);
            }
            return IsValidName(value);
        }

        static bool IsValidName(string value)
        {
            if(!string.IsNullOrWhiteSpace(value) && !value.Contains("."))
            {
                try
                {
                    XmlConvert.VerifyName(value);
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region UI Events

        void SplitTypeChanged()
        {
            if(!String.IsNullOrEmpty(SplitType))
            {
                var val = SplitType;
                if(val == "Index" || val == "Chars")
                {
                    IsSplitTokenEnabled = true;
                }
                else
                {
                    SplitToken = string.Empty;
                    IsSplitTokenEnabled = false;
                }
            }
            UpdateUiState();
        }

        void UpdateUiState()
        {
            CanAdd = !string.IsNullOrWhiteSpace(VariableListString) && (!IsSplitTokenEnabled || !string.IsNullOrEmpty(SplitToken));
            if(PreviewViewModel != null)
            {
                PreviewViewModel.CanPreview = CanAdd;
            }
        }

        #endregion

    }
}
