using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dev2.Activities.Designers;
using Dev2.Activities.Preview;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Activities.QuickVariableInput
{
    public class QuickVariableInputViewModel : ObservableObject, IHasActivityViewModelBase, IValidator
    {
        string _variableListString;
        string _prefix;
        string _suffix;
        string _splitType;
        string _splitToken;
        bool _overwrite;
        bool _canAdd;

        List<string> _splitTypeList;
        bool _isSplitTokenEnabled = true;
        readonly PreviewViewModel _previewViewModel;

        readonly List<IErrorInfo> _tokenizerValidationErrors = new List<IErrorInfo>();
        bool _isSplitOnFocused;
        bool _isSuffixFocused;
        bool _isPrefixFocused;
        bool _isVariableListFocused;

        #region CTOR

        public QuickVariableInputViewModel(IActivityCollectionViewModel activityCollectionViewModel)
        {
            if(activityCollectionViewModel == null)
            {
                throw new ArgumentNullException("activityCollectionViewModel");
            }
            ActivityCollectionViewModel = activityCollectionViewModel;

            SplitTypeList = new List<string> { "Index", "Chars", "New Line", "Space", "Tab" };
            VariableListString = string.Empty;
            SplitType = "Chars";
            SplitToken = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;

            ClearCommand = new RelayCommand(DoClear, o => true);
            AddCommand = new RelayCommand(DoAdd, o => CanAdd);

            _previewViewModel = new PreviewViewModel
            {
                InputsVisibility = Visibility.Collapsed
            };
            PreviewViewModel.PreviewRequested += DoPreview;
        }

        #endregion

        #region Properties

        public IActivityViewModelBase ActivityViewModelBase { get; set; }

        public bool IsSplitOnFocused { get { return _isSplitOnFocused; } set { OnPropertyChanged("IsSplitOnFocused", ref _isSplitOnFocused, value); } }

        public bool IsSuffixFocused { get { return _isSuffixFocused; } set { OnPropertyChanged("IsSuffixFocused", ref _isSuffixFocused, value); } }

        public bool IsPrefixFocused { get { return _isPrefixFocused; } set { OnPropertyChanged("IsPrefixFocused", ref _isPrefixFocused, value); } }

        public bool IsVariableListFocused { get { return _isVariableListFocused; } set { OnPropertyChanged("IsVariableListFocused", ref _isVariableListFocused, value); } }

        public IActivityCollectionViewModel ActivityCollectionViewModel { get; private set; }

        public List<string> SplitTypeList { get { return _splitTypeList; } set { OnPropertyChanged("SplitTypeList", ref _splitTypeList, value); } }

        public string VariableListString
        {
            get { return _variableListString; }
            set
            {
                OnPropertyChanged("VariableListString", ref _variableListString, value);
                UpdateUIState();
            }
        }

        public string SplitType
        {
            get { return _splitType; }
            set
            {
                OnPropertyChanged("SplitType", ref _splitType, value);
                SplitTypeChanged();
            }
        }

        public string SplitToken
        {
            get { return _splitToken; }
            set
            {
                OnPropertyChanged("SplitToken", ref _splitToken, value);
                UpdateUIState();
            }
        }

        public string Prefix
        {
            get { return _prefix; }
            set
            {
                OnPropertyChanged("Prefix", ref _prefix, value);
                UpdateUIState();
            }
        }

        public string Suffix
        {
            get { return _suffix; }
            set
            {
                OnPropertyChanged("Suffix", ref _suffix, value);
                UpdateUIState();
            }
        }

        public bool Overwrite { get { return _overwrite; } set { OnPropertyChanged("Overwrite", ref _overwrite, value); } }

        public bool IsSplitTokenEnabled { get { return _isSplitTokenEnabled; } set { OnPropertyChanged("IsSplitTokenEnabled", ref _isSplitTokenEnabled, value); } }

        public bool CanAdd { get { return _canAdd; } set { OnPropertyChanged("CanAdd", ref _canAdd, value); } }

        public PreviewViewModel PreviewViewModel
        {
            get
            {
                return _previewViewModel;
            }
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
            var errors = ValidationErrors();

            if(!errors.Any())
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
            var errors = ValidationErrors();

            if(!errors.Any())
            {
                UpdatePreviewViewModelInputs();

                var inputs = PreviewViewModel.Inputs.Select(input => input.Key);
                ActivityCollectionViewModel.AddListToCollection(inputs, Overwrite);
                DoClear(o);
            }
        }

        #endregion

        #region UpdatePreviewViewModelInputs

        void UpdatePreviewViewModelInputs()
        {
            var tokenizer = CreateTokenizer();
            DataListUtil.UpsertTokens(PreviewViewModel.Inputs, tokenizer, Prefix, Suffix);
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

        #region ValidationErrors

        public virtual IEnumerable<IErrorInfo> ValidationErrors()
        {
            var errors = ValidationErrorsImpl().ToList();
            SetHelpErrors(errors);
            return errors;
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
            UpdateUIState();
        }

        void UpdateUIState()
        {
            CanAdd = !string.IsNullOrWhiteSpace(VariableListString) && (!IsSplitTokenEnabled || !string.IsNullOrEmpty(SplitToken));
            if(PreviewViewModel != null)
            {
                PreviewViewModel.CanPreview = CanAdd;
            }
            SetHelpErrors(null);
        }

        void SetHelpErrors(List<IActionableErrorInfo> errors)
        {
            if(ActivityCollectionViewModel.HelpViewModel != null)
            {
                ActivityCollectionViewModel.HelpViewModel.Errors = errors;
            }
        }

        #endregion

    }
}
