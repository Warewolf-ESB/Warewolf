
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Data.Util;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Models.QuickVariableInput;

namespace Dev2.ViewModels.QuickVariableInput
{
    public class QuickVariableInputViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private string _variableListString;
        private string _prefix;
        private string _suffix;
        private string _splitType;
        private string _splitToken;
        private bool _overwrite;
        private string _previewText;
        private bool _showPreview;
        private bool _canAdd;

        private DelegateCommand _cancelCommand;
        private DelegateCommand _previewCommand;
        private DelegateCommand _addCommand;
        private List<string> _splitTypeList;
        private readonly List<KeyValuePair<ErrorType, string>> _errorColletion;

        private readonly QuickVariableInputModel _model;

        #endregion

        #region Properties

        public bool CanAdd
        {
            get
            {
                return _canAdd;
            }
            set
            {
                _canAdd = value;
                OnPropertyChanged("CanAdd");
            }
        }

        public QuickVariableInputModel Model
        {
            get
            {
                return _model;
            }
        }

        public List<string> SplitTypeList
        {
            get
            {
                return _splitTypeList;
            }
            set
            {
                _splitTypeList = value;
                OnPropertyChanged("SplitTypeList");
            }
        }

        public string VariableListString
        {
            get
            {
                return _variableListString;
            }
            set
            {
                _variableListString = value;
                OnPropertyChanged("VariableListString");
            }
        }

        public string Prefix
        {
            get
            {
                return _prefix;
            }
            set
            {
                _prefix = value;
                OnPropertyChanged("Prefix");
            }
        }

        public string Suffix
        {
            get
            {
                return _suffix;
            }
            set
            {
                _suffix = value;
                OnPropertyChanged("Suffix");
            }
        }

        public string SplitType
        {
            get
            {
                return _splitType;
            }
            set
            {
                _splitType = value;
                OnPropertyChanged("SplitType");
            }
        }

        public string SplitToken
        {
            get
            {
                return _splitToken;
            }
            set
            {
                _splitToken = value;
                OnPropertyChanged("SplitToken");
            }
        }

        public bool Overwrite
        {
            get
            {
                return _overwrite;
            }
            set
            {
                _overwrite = value;
                OnPropertyChanged("Overwrite");
            }
        }

        public string PreviewText
        {
            get
            {
                return _previewText;
            }
            set
            {
                _previewText = value;
                OnPropertyChanged("PreviewText");
            }
        }

        public bool ShowPreview
        {
            get
            {
                return _showPreview;
            }
            set
            {
                _showPreview = value;
                OnPropertyChanged("ShowPreview");
            }
        }

        #endregion

        #region Ctor

        public QuickVariableInputViewModel(QuickVariableInputModel model)
        {
            _model = model;
            SplitType = "Chars";
            SplitToken = string.Empty;
            VariableListString = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
            CanAdd = false;

            SplitTypeList = new List<string> { "Index", "Chars", "New Line", "Space", "Tab" };
            _errorColletion = new List<KeyValuePair<ErrorType, string>>();
        }

        #endregion

        #region Commands

        public ICommand AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new DelegateCommand(param => AddToActivity()));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new DelegateCommand(param => ClearData()));
            }
        }

        public ICommand PreviewCommand
        {
            get
            {
                return _previewCommand ?? (_previewCommand = new DelegateCommand(param => Preview()));
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when a close request is recieved
        /// </summary>
        public event EventHandler CloseAdornersRequested;

        protected void OnClose()
        {
            if (CloseAdornersRequested != null)
            {
                CloseAdornersRequested(this, new EventArgs());
            }
        }

        #endregion

        #region Clear

        protected void ClearData()
        {
            SplitType = "Chars";
            SplitToken = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
            VariableListString = string.Empty;
            ShowPreview = false;
            _errorColletion.Clear();
            Overwrite = false;
            OnClose();
        }

        #endregion

        #region Methods

        public void AddToActivity()
        {
            if (!ValidateFields())
            {
                PreviewText = _errorColletion[0].Value;
                CanAdd = false;
                ShowPreview = true;
                return;
            }
            List<string> listToAdd = MakeDataListReady(Split());
            if (_errorColletion.Count > 0)
            {
                PreviewText = _errorColletion[0].Value;
                ShowPreview = true;
                return;
            }
            if (listToAdd != null && listToAdd.Count > 0)
            {
                _model.AddListToCollection(listToAdd, Overwrite);
                //    _eventPublisher.Publish(new AddStringListToDataListMessage(listToAdd));  
            }
            ClearData();
        }

        public void Preview()
        {
            if (!ValidateFields())
            {
                CanAdd = false;
                PreviewText = _errorColletion[0].Value;
                ShowPreview = true;
                return;
            }

            PreviewText = string.Empty;
            int count = 1;
            if (!Overwrite)
            {
                count = _model.GetCollectionCount();
                count++;

            }
            IList<string> previewList = MakeDataListReady(Split());
            int previewAmount = previewList.Count;
            if (previewAmount > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    PreviewText = string.Concat(PreviewText, count.ToString(CultureInfo.InvariantCulture), " ", previewList[i], Environment.NewLine);
                    count++;
                }
                PreviewText = PreviewText + "...";
            }
            else
            {
                foreach (string s in previewList)
                {
                    PreviewText = string.Concat(PreviewText, count.ToString(CultureInfo.InvariantCulture), " ", s, Environment.NewLine);
                    count++;
                }
            }
            if (_errorColletion.Count > 0)
            {
                CanAdd = false;
                PreviewText = _errorColletion[0].Value;
                ShowPreview = true;
            }
            else
            {
                if (!ShowPreview)
                {
                    ShowPreview = true;
                }
            }
        }

        public List<string> Split()
        {
            List<string> results = new List<string>();
            try
            {
                IDev2Tokenizer tokenizer = CreateSplitPattern(VariableListString, SplitType, SplitToken);

                while (tokenizer.HasMoreOps())
                {

                    string tmp = tokenizer.NextToken();
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        results.Add(tmp);
                    }
                }
            }
            catch (Exception e)
            {
                _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, e.Message));
                CanAdd = false;
            }



            return results;
        }

        public List<string> MakeDataListReady(IList<string> listToMakeReady)
        {
            return listToMakeReady.Where(s => !string.IsNullOrEmpty(s)).Select(s => string.Concat(Prefix, s, Suffix)).Select(DataListUtil.AddBracketsToValueIfNotExist).Select(tmp => tmp.Replace(" ", "")).ToList();
        }

        #endregion

        #region Private Methods

        private IDev2Tokenizer CreateSplitPattern(string stringToSplit, string splitType, string at)
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = stringToSplit };

            switch (splitType)
            {
                case "Index":
                    if (!string.IsNullOrEmpty(at))
                    {
                        int indexNum;
                        if (int.TryParse(at, out indexNum) && indexNum > 0)
                        {
                            dtb.AddIndexOp(indexNum);
                        }
                    }
                    break;

                case "Space":
                    dtb.AddTokenOp(" ", false);
                    break;

                case "Tab":
                    dtb.AddTokenOp("\t", false);
                    break;

                case "New Line":
                    if (stringToSplit.Contains("\r\n"))
                    {
                        dtb.AddTokenOp("\r\n", false);
                    }
                    else if (stringToSplit.Contains("\n"))
                    {
                        dtb.AddTokenOp("\n", false);
                    }
                    else if (stringToSplit.Contains("\r"))
                    {
                        dtb.AddTokenOp("\r", false);
                    }
                    break;

                case "Chars":
                    if (!string.IsNullOrEmpty(at))
                    {
                        dtb.AddTokenOp(at, false);
                    }
                    break;
            }


            return dtb.Generate();
        }

        private bool ValidateFields()
        {
            _errorColletion.Clear();

            if (SplitType == "Index")
            {
                int indexToSplitOn;
                if (!SplitToken.IsWholeNumber())
                {
                    _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Please supply a whole positive number for an Index split"));
                    return false;
                }
                if (!int.TryParse(SplitToken, out indexToSplitOn))
                {
                    double doubleToSplitOn;
                    _errorColletion.Add(double.TryParse(SplitToken, out doubleToSplitOn) ? new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Please supply a number less then 2,147,483,647 for an Index split") : new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Please supply a whole positive number for an Index split"));
                    return false;
                }

                if (indexToSplitOn < 1)
                {
                    _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Please supply a whole positive number for an Index split"));
                    return false;
                }

            }
            else if (SplitType == "Chars")
            {
                if (string.IsNullOrEmpty(SplitToken))
                {
                    _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Please supply a value for a Character split"));
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(VariableListString))
            {
                _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Variable List String can not be blank/empty"));
                return false;
            }

            if (!string.IsNullOrEmpty(Prefix) && !ValidateRecordsetPrefix(Prefix))
            {

                _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Prefix contains invalid characters"));
                return false;

            }
            if (!string.IsNullOrEmpty(Suffix) && !ValidateName(Suffix))
            {
                _errorColletion.Add(new KeyValuePair<ErrorType, string>(ErrorType.Critical, "Suffix contains invalid characters"));
                return false;
            }

            return true;
        }

        private bool ValidateRecordsetPrefix(string value)
        {

            if (value.Contains("(") && value.Contains(")."))
            {
                int startIndex = value.IndexOf("(", StringComparison.Ordinal) + 1;
                int endIndex = value.LastIndexOf(").", StringComparison.Ordinal);

                string tmp = value.Substring(startIndex, endIndex - startIndex);
                int idxNum = 1;
                if (tmp != "*" && !string.IsNullOrEmpty(tmp) && !int.TryParse(tmp, out idxNum))
                {
                    return false;
                }
                if (idxNum < 1)
                {
                    return false;
                }
                value = value.Replace("(" + tmp + ").", string.Empty);
            }
            return ValidateName(value);
        }

        private bool ValidateName(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !value.Contains("."))
            {
                try
                {
                    XmlConvert.VerifyName(value);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            OnDispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        #endregion
    }
}
