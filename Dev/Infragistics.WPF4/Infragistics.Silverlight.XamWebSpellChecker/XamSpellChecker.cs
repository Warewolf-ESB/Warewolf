using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using System.Net;
using System.Threading;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Interactions.Primitives;
using Infragistics.SpellChecker;


using System.Windows.Navigation;

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// A visual representation allowing a user to spellcheck a string.
    /// </summary>
    [StyleTypedProperty(Property = "DictionaryLoadProgressDialogStyle", StyleTargetType = typeof(ContentControl))]
    [TemplatePart(Name = "VisualTreePlaceHolder", Type = typeof(Panel))]

    
    

    public class XamSpellChecker : Control, IDisposable, ICommandTarget, IProvidePropertyPersistenceSettings
    {
        #region Member Variables

        private const string ElementSpellCheckWindowStyle = "SpellCheckWindowStyle";
        private const string ElementSpellCheckDialogStyle = "SpellCheckDialogStyle";
        private const string ElementDictionaryLoadProgressWindowStyle = "DictionaryLoadProgressWindowStyle";
        private const string ElementDictionaryLoadProgressDialogStyle = "DictionaryLoadProgressDialogStyle";
        private const string ElementMessageWindowStyle = "MessageWindowStyle";
        private const string ElementMessageDialogStyle = "MessageDialogStyle";

        Collection<BadWord> _badWordsSummary;
        bool _isDictionaryLoaded;
        bool _isUserDictionaryLoaded;
        Uri _dictionaryURI;
        Uri _userDictionaryURI;
        SpellCheckerWrapper _checker;
        DownloadHelper _downloadHelper;
        XamSpellCheckerDialogSettings _dialogSettings;
        string _currentSpellCheckBindingValue;

        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        private int oldProgressPecentage = 0;
        ObservableCollection<BindingBase> _spellCheckTargetBindings;



        ObservableCollection<TargetElement> _spellCheckTargetElements;
        Panel _spellCheckerVisualTreePlaceHolderPanel;
        XamSpellCheckerDialogWindow _spellCheckerDialogWindow;
        DictionaryLoadProgressDialog progressDlg;
        int _currentSpellCheckTargetIndex;


        XamDialogWindow _progressWindow;

        internal AutoResetEvent Finished = new AutoResetEvent(false);
        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the XamSpellChecker class.
        /// </summary>
        public XamSpellChecker()
        {
            this.DefaultStyleKey = typeof(XamSpellChecker);
            this._checker = new SpellCheckerWrapper();
            this.SpellOptions = new SpellOptions();
            this.PerformanceOptions = new PerformanceOptions();

            this._spellCheckerDialogWindow = new XamSpellCheckerDialogWindow() { SpellChecker = this };
            this._spellCheckTargetElements = new ObservableCollection<TargetElement>();

            

            this.SetupProcessDialog();

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamSpellChecker), this);


            this.Unloaded += new RoutedEventHandler(XamWebSpellChecker_Unloaded);
            this.Loaded += new RoutedEventHandler(XamWebSpellChecker_Loaded);
            
        }

        #endregion //Constructor

        #region Properties
        #region Private



        private Uri BaseUri
        {
            get;
            set;
        }



        private XamDialogWindow ProgressWindow
        {
            get
            {
                if (this._progressWindow == null)
                {
                    this.SetupDialogWindows();
                }

                return this._progressWindow;
            }
        }

        #endregion

        #region Public

        #region CurrentBadWord

        /// <summary>
        /// Identifies the <see cref="CurrentBadWord"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CurrentBadWordProperty =
            DependencyProperty.Register("CurrentBadWord", typeof(BadWord), typeof(XamSpellChecker),
            new PropertyMetadata(null, null));

        /// <summary>
        /// Gets the current bad word, name is empty if there is no more bad words.
        /// </summary>
        public BadWord CurrentBadWord
        {
            get { return (BadWord)this.GetValue(CurrentBadWordProperty); }
            internal set { this.SetValue(CurrentBadWordProperty, value); }
        }

        #endregion // CurrentBadWord

        #region SpellCheckDialog
        /// <summary>
        /// Gets a reference to the spell checker controls <see cref="XamDialogWindow"/>.
        /// </summary>
        public XamSpellCheckerDialogWindow SpellCheckDialog
        {
            get { return this._spellCheckerDialogWindow; }
        }
        #endregion //SpellCheckDialog

        #region DictionaryUri

        /// <summary>
        /// Gets or sets the Uri that the dictionary is at.
        /// </summary>
        /// <remarks>
        /// <p class="body">When you set this property, the download process starts immediately and asynchronously.
        /// You can subscribe for <see cref="DictionaryProgressChanged"/> event to know the percents that were downloaded.</p>
        /// <p class="body">You can use <see cref="DictionaryDownloadCompleted"/> to be notified when the download process has been completed. </p>
        /// <p class="note">The spellchecking process doesn’t start until the dictionary load is completed.</p>
        /// </remarks>
        public Uri DictionaryUri
        {
            get
            {
                return this._dictionaryURI;
            }

            set
            {
                _isDictionaryLoaded = false;
                _dictionaryURI = value;
            }
        }

        #endregion //DictionaryUri

        #region UserDictionaryUri

        /// <summary>
        /// Gets or sets the Uri that the dictionary is at.
        /// </summary>
        /// <remarks>
        /// <p class="body">When you set this property, the download process starts immediately and asynchronously.</p>
        /// <p class="body">You can use <see cref="UserDictionaryDownloadCompleted"/> to be notified when the download process has been completed.</p>
        /// </remarks>
        public Uri UserDictionaryUri
        {
            get
            {
                return this._userDictionaryURI;
            }

            set
            {
                _isUserDictionaryLoaded = false;
                _userDictionaryURI = value;
            }
        }

        #endregion //UserDictionaryUri

        #region DialogSettings
        /// <summary>
        /// Gets a reference to the <see cref="DialogSettings"/> object that controls all the properties of the <see cref="SpellCheckDialog"/>.
        /// </summary>
        public XamSpellCheckerDialogSettings DialogSettings
        {
            get
            {
                if (this._dialogSettings == null)
                    this._dialogSettings = new XamSpellCheckerDialogSettings() { SpellChecker = this };

                return this._dialogSettings;
            }
            set
            {
                if (value != this._dialogSettings)
                {
                    this._dialogSettings = value;
                    if (value != null)
                        this._dialogSettings.SpellChecker = this;
                }
            }

        }
        #endregion // DialogSettings

        #region PerformanceOptions

        /// <summary>
        /// Identifies the <see cref="PerformanceOptions "/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty PerformanceOptionsProperty =
            DependencyProperty.Register("PerformanceOptions ", typeof(PerformanceOptions), typeof(XamSpellChecker),
            new PropertyMetadata(new PropertyChangedCallback(PerformanceOptionsChanged)));

        /// <summary>
        /// Gets the options available for the spell checker engine. 
        /// </summary>
        public PerformanceOptions PerformanceOptions
        {
            get { return (PerformanceOptions)this.GetValue(PerformanceOptionsProperty); }
            set { this.SetValue(PerformanceOptionsProperty, value); }
        }

        private static void PerformanceOptionsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSpellChecker spellChecker = obj as XamSpellChecker;
            PerformanceOptions options = e.NewValue as PerformanceOptions;
            if (spellChecker._checker == null)
                return;

            if (options != null)
            {
                options.Checker = spellChecker._checker;
            }
        }

        #endregion // PerformanceOptions

        #region DictionaryLoadProgressDialogStyle

        /// <summary>
        /// Identifies the <see cref="DictionaryLoadProgressDialogStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DictionaryLoadProgressDialogStyleProperty =
            DependencyProperty.Register("DictionaryLoadProgressDialogStyle", typeof(Style), typeof(XamSpellChecker), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style for the <see cref="DictionaryLoadProgressDialog"/>
        /// </summary>
        public Style DictionaryLoadProgressDialogStyle
        {
            get { return (Style)this.GetValue(DictionaryLoadProgressDialogStyleProperty); }
            set { this.SetValue(DictionaryLoadProgressDialogStyleProperty, value); }
        }
        #endregion // DictionaryLoadProgressDialogStyle


        #region SpellOptions

        /// <summary>
        /// Identifies the <see cref="SpellOptions"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SpellOptionsProperty =
            DependencyProperty.Register("SpellOptions", typeof(SpellOptions), typeof(XamSpellChecker),
            new PropertyMetadata(new PropertyChangedCallback(SpellOptionsChanged)));

        /// <summary>
        /// Gets or sets the options that pertain to how the <see cref="XamSpellChecker"/> returns it's results.
        /// </summary>
        public SpellOptions SpellOptions
        {
            get { return (SpellOptions)this.GetValue(SpellOptionsProperty); }
            set { this.SetValue(SpellOptionsProperty, value); }
        }

        private static void SpellOptionsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamSpellChecker spellChecker = obj as XamSpellChecker;
            SpellOptions options = e.NewValue as SpellOptions;
            if (spellChecker._checker == null)
                return;

            if (options != null)
            {
                options.Checker = spellChecker._checker;
            }
        }

        #endregion // SpellOptions

        #region SpellCheckTargets



        /// <summary>
        /// A collection of <see cref="BindingBase"/>'s which are used to determine which editiable fields will be spell checked.
        /// </summary>         
        public ObservableCollection<BindingBase> SpellCheckTargets
        {
            get
            {
                if (this._spellCheckTargetBindings == null)
                {
                    this._spellCheckTargetBindings = new ObservableCollection<BindingBase>();

                }
                return this._spellCheckTargetBindings;
            }
        }


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


        #endregion // SpellCheckTargets

        #endregion // Public

        #region Internal


        #region CurrentSpellCheckTargetIndex
        internal int CurrentSpellCheckTargetIndex
        {
            get
            {
                return this._currentSpellCheckTargetIndex;
            }
            set
            {
                this._currentSpellCheckTargetIndex = value;
            }
        }
        #endregion //CurrentSpellCheckTargetIndex

        #region CurrentSpellCheckBindingValue
        internal string CurrentSpellCheckBindingValue
        {
            get
            {
                object value = this.SpellCheckTargetElements[this._currentSpellCheckTargetIndex].Value ?? string.Empty;
                return value.ToString();
            }
            set
            {
                this.SpellCheckTargetElements[this._currentSpellCheckTargetIndex].Value = value;
            }
        }
        #endregion //CurrentSpellCheckBindingValue

        internal ICheckerEngine SpellCheckerWrapper
        {
            get
            {
                return this._checker;
            }
        }

        #region SpellCheckTargetElements
        internal ObservableCollection<TargetElement> SpellCheckTargetElements
        {
            get
            {
                return this._spellCheckTargetElements;
            }

        }
        #endregion //SpellCheckTargetElements

        #region TextContext

        internal TextBlock TextContext
        {
            get;
            set;
        }

        #endregion

        #endregion

        #endregion // Properties

        #region Methods

        #region Public

        #region AddWordToUserDictionary
        /// <summary>
        /// Adds a word to the current <see cref="UserDictionary"/>.
        /// </summary>
        /// <param name="word">The word to add to the user dictionary.</param>
        /// <exception cref="InvalidOperationException">
        /// The user dictionary has not been set.
        /// </exception>
        /// <returns>True if the word was added to the user dictionary; False otherwise.</returns>
        public bool AddWordToUserDictionary(string word)
        {
            EnsureTextIsSingleWord(word);

            if (this._isUserDictionaryLoaded == false)
                throw new InvalidOperationException(SRSP.GetString("UserDictionaryNotSet"));

            return this._checker.AddWord(word);
        }
        #endregion //AddWordToUserDictionary

        #region SpellCheckComplete

        /// <summary>
        /// Finishes the current spellcheck session.
        /// </summary>
        internal void SpellCheckComplete(bool isCanceled)
        {
            if (_downloadHelper.IsBusy)
            {
                CancelAsyncDictionaryLoad();
            }
            else
            {
                OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(null, isCanceled, null));
            }
            this.CurrentBadWord = null;
        }

        #endregion

        #region LoadDictionary
        /// <summary>
        /// Loads stored dictionary from isolated storage.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        public void LoadDictionary(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Open, store))
                {
                    using (BinaryReader r = new System.IO.BinaryReader(isfs))
                    {
                        byte[] bytes = r.ReadBytes((int)isfs.Length);
                        isfs.Read(bytes, 0, (int)isfs.Length);

                        System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);

						// MD 10/10/11 - TFS84743/TFS33353
						// Use the new DictFile constructor overload to specify the file name so we know what extension the file has.
						//this._checker.DictionaryUri = new DictFile(stream);//, bytes);
						this._checker.DictionaryUri = new DictFile(stream, fileName);

                        r.Close();
                    }
                }
            }
        }

        #endregion

        #region LoadUserDictionary
        /// <summary>
        /// Loads stored user dictionary from isolated storage.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        public void LoadUserDictionary(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Open, store))
                {
                    using (BinaryReader r = new System.IO.BinaryReader(isfs))
                    {
                        byte[] bytes = r.ReadBytes((int)isfs.Length);
                        isfs.Read(bytes, 0, (int)isfs.Length);

                        System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
                        this._checker.UserDictionary = stream;
                        r.Close();
                    }
                }
            }
        }

        #endregion

        #region RemoveWordFromUserDictionary
        /// <summary>
        /// Removes a word from the current <see cref="UserDictionary"/>.
        /// </summary>
        /// <param name="word">The word to be removed from the user dictionary.</param>
        /// <exception cref="InvalidOperationException">
        /// The user dictionary has not been set.
        /// </exception>
        /// <returns>True if the word was removed from the user dictionary; False otherwise.</returns>
        public bool RemoveWordFromUserDictionary(string word)
        {
            EnsureTextIsSingleWord(word);

            if (this._isUserDictionaryLoaded == false)
                throw new InvalidOperationException(SRSP.GetString("UserDictionaryNotSet"));

            return this._checker.RemoveWord(word);
        }
        #endregion //RemoveWordFromUserDictionary

        #region SaveDictionary
        /// <summary>
        /// Saves current dictionary in isolated storage.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <exception cref="IsolatedStorageException">If the file already exists and can not be deleted.</exception>
        public bool SaveDictionary(string fileName)
        {
            if (this._checker.DictionaryUri == null)
                return false;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(fileName))
                {
                    try
                    {
                        store.DeleteFile(fileName);
                    }
                    catch (IsolatedStorageException)
                    {
                        throw;
                    }
                }

                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Create, store))
                {
                    using (BinaryWriter sw = new BinaryWriter(isfs))
                    {
                        sw.Write(this._checker.DictionaryUri.dictFileBytes);
                        sw.Close();
                    }
                }
            }
            return true;
        }
        #endregion

        #region SaveUserDictionary
        /// <summary>
        /// Saves current user dictionary in isolated storage
        /// </summary>
        /// <param name="fileName">The name of file</param>
        public bool SaveUserDictionary(string fileName)
        {
            this._checker.WriteUserDictionary(fileName);

            return true;
        }

        #endregion

        #region SpellCheck
        /// <summary>
        /// Spellchecks the string that is in the TextToSpellCheck property.
        /// </summary>
        /// <remarks>
        /// <p class="note">The spellchecking process doesn’t start until the dictionary load is completed.</p>
        /// </remarks>
        /// <exception cref="InvalidOperationException">You can not start spell checking while the previous is not finished.</exception>
        public void SpellCheck()
        {
            
            if (this.Visibility == Visibility.Collapsed)
                throw new Exception(SRSP.GetString("XamSpellCheckerNotVisible"));

            TargetElement te;


            this._spellCheckTargetElements.Clear();
            this._currentSpellCheckTargetIndex = 0;
            for (int i = 0; i < this.SpellCheckTargets.Count; i++)
            {
                te = new TargetElement();
                if (this._spellCheckerVisualTreePlaceHolderPanel != null)
                    this._spellCheckerVisualTreePlaceHolderPanel.Children.Add(te);
                te.SetBinding(TargetElement.ValueProperty, this.SpellCheckTargets[i]);
                this._spellCheckTargetElements.Add(te);
            }

            if (this._spellCheckTargetElements.Count == 0)
                return;

            object value = this._spellCheckTargetElements[this._currentSpellCheckTargetIndex].Value ?? string.Empty;

            this._currentSpellCheckBindingValue = value.ToString();


            if (_downloadHelper != null && _downloadHelper.IsBusy)
            {
                throw new InvalidOperationException(SRSP.GetString("SpellCheckNotComplete"));
            }


            // download dictionary
            _downloadHelper = new DownloadHelper();

            this.BaseUri = BaseUriHelper.GetBaseUri(this);

            _downloadHelper.WorkerSupportsCancellation = true;
            _downloadHelper.DoWork += DoWork;
            _downloadHelper.DownloadProgress += DownloadSubtaskProgress;

            _downloadHelper.DownloadProgressWpf += DownloadSubtaskProgressWpf;

            _downloadHelper.DownloadCompleted += DownloadSubtaskCompleted;
            _downloadHelper.RunWorkerCompleted += DownloadCompleted;
            _downloadHelper.RunWorkerAsync();

            if (this._isDictionaryLoaded == false && DictionaryUri != null)
            {
                ShowProgressWindow();
            }

        }

        private void DownloadSubtaskProgress(object sender, TaskProgressEventArgs e)
        {
            int task = (int)e.taskID;

            if (task == 1)
            {
                if (this.ProgressWindow != null && this.ProgressWindow.Content is DictionaryLoadProgressDialog)
                {
                    (this.ProgressWindow.Content as DictionaryLoadProgressDialog).ProgressValue = e.args.ProgressPercentage;

                }
                OnDictionaryProgressChanged(e.args.ProgressPercentage);
            }
            else
            {
                OnUserDictionaryProgressChanged(e.args.ProgressPercentage);
            }
        }


        //Fixed Bug 34097 - Mihail Mateev 06/10/2010
        private void DownloadSubtaskProgressWpf(object sender, TaskProgressWpfEventArg e)
        {
            int task = (int)e.taskID;

            if (e.args == null || oldProgressPecentage == e.args.ProgressPercentage)
            {
                return;
            }
            oldProgressPecentage = e.args.ProgressPercentage;
            if (task == 1)
            {
                if (this._progressWindow != null && this._progressWindow.Content is DictionaryLoadProgressDialog)
                {
                    (this._progressWindow.Content as DictionaryLoadProgressDialog).ProgressValue = e.args.ProgressPercentage;

                }

                OnDictionaryProgressChanged(e.args.ProgressPercentage);
            }
            else
            {
                OnUserDictionaryProgressChanged(e.args.ProgressPercentage);
            }
        }


        private void DownloadSubtaskCompleted(object sender, TaskCompletedEventArgs e)
        {
            if ((int)e.taskID == 1)
            {
                if (e.args.Cancelled == false && e.args.Error == null)
                    this._isDictionaryLoaded = true;

                OnDictionaryDownloadCompleted(e.args);
            }
            else
            {
                if (e.args.Cancelled == false && e.args.Error == null)
                    this._isUserDictionaryLoaded = true;

                OnUserDictionaryDownloadCompleted(e.args);
            }
        }

        private void DownloadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CloseProgressWindow();

            DownloadHelper helper = (DownloadHelper)sender;
            helper.DoWork -= DoWork;
            helper.RunWorkerCompleted -= DownloadCompleted;
            helper.DownloadProgress -= DownloadSubtaskProgress;

            //Fixed Bug 34097 - Mihail Mateev 06/10/2010
            _downloadHelper.DownloadProgressWpf -= DownloadSubtaskProgressWpf;

            helper.DownloadCompleted -= DownloadSubtaskCompleted;

            if (e.Error != null || e.Cancelled)
            {
                

                OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
                return;
            }

            // start spell checking after the dictionaries have downloaded.
            SpellCheckInternal();
        }


        private void DoWork(object sender, DoWorkEventArgs e)
        {
            DownloadHelper worker = (DownloadHelper)sender;

            //worker.Checker = this; //MM delete???

            // Divide the "work" into two parts, and queue two tasks to run on
            // threadpool threads. 

            // Keep a list of subtasks and a list of their ManualResetEvent objects.
            System.Collections.Generic.List<Subtask> subtasks = new System.Collections.Generic.List<Subtask>();
            System.Collections.Generic.List<WaitHandle> finished = new System.Collections.Generic.List<WaitHandle>();

            #region Download Dictionary

            if (this._isDictionaryLoaded == false && this.DictionaryUri != null)
            {
                int dictionaryTaskID = 1;

                Subtask taskDictionary = new Subtask(this.DictionaryUri, this.BaseUri);




                taskDictionary.DownloadCompleted += (taskSender, args) =>
                {
                    Subtask task = taskSender as Subtask;
                    if (args.Error == null && args.Cancelled == false)
                    {
                        try
                        {
							// MD 10/5/11 - TFS84743/TFS33353
                            //this._checker.DictionaryUri = new DictFile(task.stream);
							this._checker.DictionaryUri = new DictFile(task.stream, this.DictionaryUri.OriginalString);

                            worker.ReportDownloadCompleted(dictionaryTaskID, args);
                        }
                        catch (Exception error)
                        {
                            CancelAsyncDictionaryLoad();
                            worker.ReportDownloadCompleted(dictionaryTaskID, new AsyncCompletedEventArgs(error, args.Cancelled, args.UserState));
                            return;
                        }
                    }
                    else
                    {
                        worker.ReportDownloadCompleted(dictionaryTaskID, args);
                        CancelAsyncDictionaryLoad();
                    }
                };

                taskDictionary.DownloadProgress += (taskSender, args) =>
                {
                    worker.ReportDownloadProgress(dictionaryTaskID, args);
                };


                //Fixed Bug 34097 - Mihail Mateev - 06/10/2010
                taskDictionary.ProgressChanged += (taskSender, args) => worker.ReportDownloadProgressWpf(dictionaryTaskID, args);


                subtasks.Add(taskDictionary);
                finished.Add(taskDictionary.Finished);
                ThreadPool.QueueUserWorkItem(taskDictionary.DoSubtask);
            }
            #endregion

            #region Download user dictionary
            if (this._isUserDictionaryLoaded == false && this.UserDictionaryUri != null)
            {
                int userDictionaryTaskID = 2;

                Subtask taskUserDictionary = new Subtask(this.UserDictionaryUri, this.BaseUri);




                taskUserDictionary.DownloadCompleted += (taskSender, args) =>
                {
                    Subtask task = taskSender as Subtask;
                    if (args.Error == null && args.Cancelled == false)
                    {
                        try
                        {
                            this._checker.UserDictionary = task.stream;
                            worker.ReportDownloadCompleted(userDictionaryTaskID, args);
                        }
                        catch (Exception error)
                        {
                            CancelAsyncDictionaryLoad();
                            worker.ReportDownloadCompleted(userDictionaryTaskID, new AsyncCompletedEventArgs(error, args.Cancelled, args.UserState));
                            return;
                        }
                    }
                    else
                    {
                        CancelAsyncDictionaryLoad();
                        worker.ReportDownloadCompleted(userDictionaryTaskID, args);
                    }
                };

                taskUserDictionary.DownloadProgress += (taskSender, args) =>
                {
                    worker.ReportDownloadProgress(userDictionaryTaskID, args);
                };


                //Fixed Bug 34097 - Mihail Mateev - 06/10/2010
                taskUserDictionary.ProgressChanged += (taskSender, args) => worker.ReportDownloadProgressWpf(userDictionaryTaskID, args);

                subtasks.Add(taskUserDictionary);
                finished.Add(taskUserDictionary.Finished);
                ThreadPool.QueueUserWorkItem(taskUserDictionary.DoSubtask);
            }
            #endregion




            // Create an array of ManualResetEvent wait handles. Each subtask will
            // signal its ManualResetEvent when it is finished.
            WaitHandle[] waitHandles = finished.ToArray();
            if (waitHandles.Length == 0)
                return;

            // Wait for ALL subtasks to complete, and show progress every 1/10 second if
            // the WaitAll times out.
            bool isCancel = false;
            while (!WaitHandle.WaitAll(waitHandles, 100))
            {
                if (worker.CancellationPending && !isCancel)
                {
                    e.Cancel = true;
                    foreach (Subtask task1 in subtasks)
                    {
                        task1.CancelAsync();
                    }
                    isCancel = true;
                }
            }
        }

        /// <summary>
        /// Spellchecks the given string, and sets the TextToSpellCheck property.
        /// </summary>
        /// <param name="text">The Text that is used to spellcheck.</param>
        /// <remarks>
        /// <p class="note">The spellchecking process doesn’t start until the dictionary load is completed.</p>
        /// </remarks>
        /// <exception cref="InvalidOperationException">You can not start spell checking while the previuos is not finished.</exception>
        internal void SpellCheck(string text)
        {
            this._currentSpellCheckBindingValue = text;
            this.SpellCheck();
        }

        #endregion SpellCheck

        #endregion //Public

        #region Protected

        /// <summary>
        /// Fires the <see cref="DictionaryDownloadCompleted"/> event. 
        /// </summary>
        /// <param name="args">A AsyncCompletedEventArgs that contains the event data.</param>
        protected virtual void OnDictionaryDownloadCompleted(AsyncCompletedEventArgs args)
        {
            if (this.DictionaryDownloadCompleted != null)
            {
                this.DictionaryDownloadCompleted(this, args);
            }
        }

        /// <summary>
        /// Fires the <see cref="UserDictionaryDownloadCompleted"/> event. 
        /// </summary>
        /// <param name="args">A AsyncCompletedEventArgs that contains the event data.</param>
        protected virtual void OnUserDictionaryDownloadCompleted(AsyncCompletedEventArgs args)
        {
            if (this.UserDictionaryDownloadCompleted != null)
            {
                this.UserDictionaryDownloadCompleted(this, args);
            }
        }

        /// <summary>
        /// Fires the <see cref="DictionaryProgressChanged"/> event. 
        /// </summary>
        /// <param name="percent">the progress value</param>
        protected virtual void OnDictionaryProgressChanged(int percent)
        {
            if (this.DictionaryProgressChanged != null)
            {
                this.DictionaryProgressChanged(this, new ProgressChangedEventArgs(percent, null));
            }
        }

        /// <summary>
        /// Fires the <see cref="UserDictionaryProgressChanged"/> event. 
        /// </summary>
        /// <param name="percent">the progress value</param>
        protected virtual void OnUserDictionaryProgressChanged(int percent)
        {
            if (this.UserDictionaryProgressChanged != null)
            {
                this.UserDictionaryProgressChanged(this, new ProgressChangedEventArgs(percent, null));
            }
        }

        /// <summary>
        /// Fires the <see cref="SpellCheckCompleted"/> event. 
        /// </summary>
        /// <param name="args">The data for the event</param>
        protected virtual internal void OnSpellCheckCompleted(SpellCheckCompletedEventArgs args)
        {
            if (this.SpellCheckCompleted != null)
            {
                this.SpellCheckCompleted(this, args);
            }
        }
        #endregion

        #region Internal

        #region MoveNextWord
        internal bool MoveNextWord()
        {
            this.CurrentBadWord = this._checker.NextBadWord();


            if (this.CurrentBadWord == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(this.CurrentBadWord.Word))
            {
                this.SpellCheckComplete(false);

                TargetElement te;
                //If we're not on the last editor then increment the target index and begin the next spell check.
                if (this._currentSpellCheckTargetIndex < this._spellCheckTargetBindings.Count - 1)
                {
                    te = this._spellCheckTargetElements[this._currentSpellCheckTargetIndex] as TargetElement;
                    te.Value = this._currentSpellCheckBindingValue;
                    this._currentSpellCheckTargetIndex++;
                    this.SpellCheck();
                }
                else
                {
                    te = this._spellCheckTargetElements[this._currentSpellCheckTargetIndex] as TargetElement;
                    te.Value = this._currentSpellCheckBindingValue;
                    this._currentSpellCheckTargetIndex = 0;

                    VisualStateManager.GoToState(this._spellCheckerDialogWindow, "SpellCheckComplete", true);
                    this._spellCheckerDialogWindow.Show();
                }
                return false;
            }
            else
            {
                this.CurrentBadWord.Suggestions = this._checker.FindSuggestions() as List<string>;
                return true;
            }
        }
        #endregion

        #region SpellCheckInternal
        internal void SpellCheckInternal()
        {
            if (this._badWordsSummary != null)
                this._badWordsSummary.Clear();
            else
                this._badWordsSummary = new Collection<BadWord>();

            this._checker.Check(this._currentSpellCheckBindingValue);

            if (this.MoveNextWord() == false)
            {
                this.DisplaySpellCheckDialog();
                return;
            }
            else
                this.DisplaySpellCheckDialog();


        }
        #endregion

        #region EnsureTextIsSingleWord
        internal static void EnsureTextIsSingleWord(string text)
        {
            if (text == null || text.Length == 0)
                throw new ArgumentException(SRSP.GetString("ArgumentExceptionWordCannotBeNull"), "text");

            foreach (char c in text.Trim())
            {
                if (!Char.IsLetterOrDigit(c) && c != '-' && c != '\'')
                    throw new ArgumentException(SRSP.GetString("ArgumentExceptionMustBeSingleWord"), "text");
            }
        }
        #endregion

        #endregion // internal

        #region Private


        private void CloseSpellCheckWindow()
        {
            if (this._spellCheckerDialogWindow != null)
            {
                this._spellCheckerDialogWindow.Close();
            }
        }


        #region GetFrameworkElement
        private static FrameworkElement GetFrameworkElement(FrameworkElement element, Type elementType)
        {
            if (element.GetType().Equals(elementType)) { return element; }
            ContentControl control = element as ContentControl;
            if (control != null)
            {
                object controlContent = control.Content;
                FrameworkElement content = controlContent as FrameworkElement;
                if (content != null)
                {

                    FrameworkElement el = GetFrameworkElement(content, elementType);
                    if (el != null) { return el; }
                }
            }
            int numChildren = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < numChildren; i++)
            {
                FrameworkElement content = (FrameworkElement)VisualTreeHelper.GetChild(element, i);
                FrameworkElement el = GetFrameworkElement(content, elementType);
                if (el != null) { return el; }
            }
            return null;
        }
        #endregion //GetFrameworkElement


        private void ShowProgressWindow()
        {
            if (this.DictionaryLoadProgressDialogStyle != progressDlg.Style)
                progressDlg.Style = this.DictionaryLoadProgressDialogStyle;

            this.ProgressWindow.Content = progressDlg;

            this.ProgressWindow.Show();
        }

        private void CloseProgressWindow()
        {
            if (this.ProgressWindow != null)
            {
                this.ProgressWindow.Close();
            }
        }

        internal void CancelAsyncDictionaryLoad()
        {
            if(this._downloadHelper != null)
                this._downloadHelper.CancelAsync();
        }

        #region CancelAsyncDictionaryDownload
        /// <summary>
        /// Cancels a pending asynchronous operation. 
        /// </summary>
        /// <remarks class="body">
        /// When you call the CancelAsyncDictionaryDownload method, your application still receives the completion event associated
        /// with the operation. For example, if you call CancelAsyncDictionaryDownload to cancel a <see cref="SpellCheck()"/> operation
        /// and you have specified an event handler for the <see cref="SpellCheckCompleted"/> event, your event handler
        /// receives notification that the operation has ended. To learn whether the operation completed 
        /// successfully, check the Cancelled property of the <see cref="SpellCheckCompletedEventArgs"/> for the relevant 
        /// completed event handler. 
        /// </remarks>
        public void CancelAsyncDictionaryDownload()
        {
            if (_downloadHelper != null)
                _downloadHelper.CancelAsync();
        }

        #endregion

        private void DisplaySpellCheckDialog()
        {
            if (this.CurrentBadWord != null)
            {
                this.CurrentBadWord.Suggestions = this._checker.FindSuggestions() as List<string>;

                VisualStateManager.GoToState(this._spellCheckerDialogWindow, "SpellCheckNormal", true);
            }
            else
            {
                VisualStateManager.GoToState(this._spellCheckerDialogWindow, "SpellCheckComplete", true);
            }

            this._spellCheckerDialogWindow.Show();
        }

        #region SetupDialogWindows
        private void SetupDialogWindows()
        {
            if (this._progressWindow == null)
            {
                this._progressWindow = new XamDialogWindow();
                this._progressWindow.DataContext = this.DialogSettings.DialogStringResources;
                this._progressWindow.StartupPosition = StartupPosition.Center;
                this._progressWindow.CloseButtonVisibility = Visibility.Collapsed;
                this._progressWindow.MinimizeButtonVisibility = Visibility.Collapsed;
                this._progressWindow.MaximizeButtonVisibility = Visibility.Collapsed;
            }
        }
        #endregion // SetupDialogWindows

        #region SetupProcessDialog

        private void SetupProcessDialog()
        {
            if (this.progressDlg != null)
            {
                this.progressDlg.SpellChecker = null;
                this.progressDlg = null;
            }

            this.progressDlg = new DictionaryLoadProgressDialog();
            this.progressDlg.SpellChecker = this;
        }

        #endregion // SetupProcessDialog

        #endregion

        #region Static

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources

        #endregion // Static

        #endregion // Methods

        #region Overrides
        /// <summary>
        /// Builds the visual tree for the <see cref="XamSpellChecker"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            this.SetupProcessDialog();

            bool preexistedSpellCheckerDialog = false;
            bool preexistedProgressWindow = false;

            if (this._spellCheckerVisualTreePlaceHolderPanel != null)
            {
                UIElementCollection children = this._spellCheckerVisualTreePlaceHolderPanel.Children;
                if (children.Contains(this.ProgressWindow))
                {
                    children.Remove(this.ProgressWindow);

                    preexistedProgressWindow = true;

                }
                if (children.Contains(this._spellCheckerDialogWindow))
                {
                    children.Remove(this._spellCheckerDialogWindow);

                    preexistedSpellCheckerDialog = true;

                }
            }

            this.SetupDialogWindows();

            this._spellCheckerVisualTreePlaceHolderPanel = this.GetTemplateChild("VisualTreePlaceHolder") as Panel;

            if (this._spellCheckerVisualTreePlaceHolderPanel != null)
            {
                this._spellCheckerVisualTreePlaceHolderPanel.Children.Add(this.ProgressWindow);

                if (this._progressWindow != null && preexistedProgressWindow)
                    this._progressWindow.InvalidateMeasure();


                this._spellCheckerVisualTreePlaceHolderPanel.Children.Add(this._spellCheckerDialogWindow);

                if (preexistedSpellCheckerDialog && this._spellCheckerDialogWindow != null)
                    this._spellCheckerDialogWindow.InvalidateMeasure();

                this.ProgressWindow.Close();
            }

            base.OnApplyTemplate();
        }
        #endregion //Overrides

        #region Events

        /// <summary>
        /// Occurs when the dictionary is loaded.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> DictionaryDownloadCompleted;

        /// <summary>
        /// Occurs when the user dictionary is loaded.
        /// </summary>
        public event EventHandler<AsyncCompletedEventArgs> UserDictionaryDownloadCompleted;

        /// <summary>
        /// Occurs when the dictionary file is being downloaded and indicates download progress.
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> DictionaryProgressChanged;

        /// <summary>
        /// Occurs when the user dictionary file is being downloaded and indicates download progress.
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> UserDictionaryProgressChanged;

        /// <summary>
        /// Occurs after all or a part of text is about to be spell checked. 
        /// </summary>
        public event EventHandler<SpellCheckCompletedEventArgs> SpellCheckCompleted;

        #endregion // Events

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. 
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// A call to Dispose(false) clean up native resources. A call to Dispose(true) 
        /// clean up both managed and native resources.	
        /// </summary>
        /// <param name="cleanAll"></param>
        protected virtual void Dispose(bool cleanAll)
        {
            if (!cleanAll)
            {
                if (Finished != null)
                {
                    Finished.Close();
                }
            }
        }

        #endregion

        #region ICommandTarget Members
        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return (command is SpellCheckCommand || command is CancelAsyncDictionaryCommand);
        }

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            XamSpellCheckerCommandSource xscc = source as XamSpellCheckerCommandSource;
            if (xscc != null && xscc.CommandType == XamSpellCheckerCommand.CancelAsyncDictionaryDownload)
                return this.progressDlg;

            return this;
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> _propertiesThatShouldntBePersisted;

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
                        "SpellCheckTargets"
					};
                }

                return this._propertiesThatShouldntBePersisted;
            }
        }

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get
            {
                return null;
            }
        }
        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }

        #endregion // PriorityProperties

        #region FinishedLoadingPersistence
        /// <summary>
        /// Used to clean up after the Persistence framework is done loading this control.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {

        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }
        #endregion // FinishedLoadingPersistence

        #endregion

        #region EventHandlers

        void XamWebSpellChecker_Loaded(object sender, RoutedEventArgs e)
        {
            CommandSourceManager.RegisterCommandTarget(this);
        }

        void XamWebSpellChecker_Unloaded(object sender, RoutedEventArgs e)
        {
            CommandSourceManager.UnregisterCommandTarget(this);
        }

        #endregion // EventHandlers
    }
}

namespace Infragistics.Controls.Interactions.Primitives
{
    #region TargetElement
    /// <summary>
    /// For internal use only.
    /// </summary>
    [DesignTimeVisible(false)]
    public class TargetElement : FrameworkElement
    {
        #region Value

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(TargetElement), new PropertyMetadata(new PropertyChangedCallback(ValueChanged)));

        /// <summary>
        /// Gets/Sets the Value associated with the element.
        /// </summary>
        public object Value
        {
            get { return (object)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        private static void ValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion // Value
    }
    #endregion //TargetElement
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved