
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models.QuickVariableInput;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace Dev2.Studio.ViewModels.QuickVariableInput
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

        private RelayCommand _cancelCommand;
        private RelayCommand _previewCommand;
        private RelayCommand _addCommand;
        private List<string> _splitTypeList;

        private QuickVariableInputModel _model;

        #endregion

        #region Properties

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

            SplitTypeList = new List<string>();
            SplitTypeList.Add("Index");
            SplitTypeList.Add("Chars");
            SplitTypeList.Add("New Line");
            SplitTypeList.Add("Space");
            SplitTypeList.Add("Tab");
            SplitTypeList.Add("End");
        }

        #endregion

        #region Commands

        public ICommand AddCommand
        {
            get
            {
                if (_addCommand == null)
                {
                    _addCommand = new RelayCommand(param =>
                    {
                        AddToActivity();
                    }, param => true);
                }
                return _addCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(param =>
                    {
                        OnClose();
                    }, param => true);
                }
                return _cancelCommand;
            }
        }

        public ICommand PreviewCommand
        {
            get
            {
                if (_previewCommand == null)
                {
                    _previewCommand = new RelayCommand(param =>
                    {
                        Preview();
                    }, param => true);
                }
                return _previewCommand;
            }
        }

        #endregion

        #region Close

        /// <summary>
        /// Occurs when a close request is recieved
        /// </summary>
        public event EventHandler Close;

        protected void OnClose()
        {
            if (Close != null)
            {
                Close(this, new EventArgs());
            }
        }

        #endregion Close

        #region Methods

        public void AddToActivity()
        {
            List<string> listToAdd = MakeDataListReady(Split());
            _model.AddListToCollection(listToAdd, Overwrite);
            IEventAggregator eventAggregator = ImportService.GetExportValue<IEventAggregator>();

            if (listToAdd != null && eventAggregator != null)
            {
                eventAggregator.Publish(new AddStringListToDataListMessage(listToAdd));
            }
            OnClose();
        }

        public void Preview()
        {
            if (!ShowPreview)
            {

                ShowPreview = true;
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
        }

        public List<string> Split()
        {
            List<string> results = new List<string>();

            IDev2Tokenizer tokenizer = CreateSplitPattern(VariableListString, SplitType, SplitToken);

            while (tokenizer.HasMoreOps())
            {

                string tmp = tokenizer.NextToken();
                if (!string.IsNullOrEmpty(tmp))
                {
                    results.Add(tmp);
                }
            }

            return results;
        }

        public List<string> MakeDataListReady(IList<string> listToMakeReady)
        {
            List<string> results = new List<string>();

            foreach (string s in listToMakeReady)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    string tmp = string.Concat(Prefix, s, Suffix);
                    tmp = DataListUtil.AddBracketsToValueIfNotExist(tmp);
                    results.Add(tmp);
                }
            }

            return results;
        }

        #endregion

        #region Private Methods

        private IDev2Tokenizer CreateSplitPattern(string stringToSplit, string splitType, string at)
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();
            dtb.ToTokenize = stringToSplit;

            switch (splitType)
            {
                case "Index":
                    try
                    {
                        if (!string.IsNullOrEmpty(at))
                        {
                            int indexNum = Convert.ToInt32(at);
                            if (indexNum > 0)
                            {
                                dtb.AddIndexOp(indexNum);
                            }
                        }
                    }
                    catch (Exception) { }
                    break;

                case "End":
                    dtb.AddEoFOp();
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
