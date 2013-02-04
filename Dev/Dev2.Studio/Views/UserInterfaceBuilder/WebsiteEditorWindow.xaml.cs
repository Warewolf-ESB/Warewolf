using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Views.WebsiteBuilder
{
    /// <summary>
    /// Interaction logic for WebsiteEditorWindow.xaml
    /// </summary>
    public partial class WebsiteEditorWindow : IHandle<TabClosedMessage>
    {
        #region Class Members

        private readonly WebsiteEditorViewModel _viewModel;
        private IMainViewModel _mainViewModel;

        #endregion

        #region Constructor

        public WebsiteEditorWindow(WebsiteEditorViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            webBrowser.Initialize();
            ImportService.SatisfyImports(this);
            EventAggregator.Subscribe(this);
            SetUpTextEditor();
        }

        #endregion

        #region Properties

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        [Import]
        public IMainViewModel MainViewModel
        {
            get { return _mainViewModel; }
            set
            {
                _mainViewModel = value;
                _viewModel.Browser = webBrowser;
                DataContext = _viewModel;
                _viewModel.Navigate();
            }
        }

        #endregion Properties

        #region Private Methods

        private void SetUpTextEditor()
        {
            FoldingManager foldingManager;
            AbstractFoldingStrategy foldingStrategy;

            txtHtml.Text = _viewModel.Html;
            foldingStrategy = new XmlFoldingStrategy();
            foldingManager = FoldingManager.Install(txtHtml.TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, txtHtml.Document);
            txtHtml.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += (sender, e) =>
            {
                if(foldingStrategy != null && foldingManager != null)
                {
                    foldingStrategy.UpdateFoldings(foldingManager, txtHtml.Document);

                }

            };
            foldingUpdateTimer.Start();

            txtHtml.TextArea.Document.TextChanged += (sender, e) =>
            {
                _viewModel.Html = (sender as dynamic).Text;
            };

        }

        #endregion

        #region Event Handlers

        void layoutTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as WebsiteEditorViewModel;
            if(viewModel != null)
            {

                string uri = _viewModel.Url;

                if(layoutTab.SelectedItem != null)
                {
                    if((layoutTab.SelectedItem as dynamic).Header == "Website")
                    {
                        _viewModel.UpdateModelItem();
                        _viewModel.Navigate();

                    }

                    if((layoutTab.SelectedItem as dynamic).Header == "Preview")
                    {
                        //webBrowser.NavigateToString(viewModel.Html);
                        webBrowser.LoadHtml(viewModel.Html);
                    }

                    if((layoutTab.SelectedItem as dynamic).Header == "Settings")
                    {
                        uri = _viewModel.WizardUrl;

                        dynamic postData = new UnlimitedObject();
                        postData.Dev2WebsiteName = viewModel.ResourceModel.ResourceName;
                        string websiteUri = _viewModel.Url;
                        postData.Dev2WebsiteURL = websiteUri;

                        if(!string.IsNullOrEmpty(viewModel.Html))
                        {
                            dynamic data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(viewModel.Html);
                            if(!data.HasError)
                            {
                                dynamic tags = data.Dev2HTML;
                                if(tags is UnlimitedObject)
                                {
                                    postData.Add(tags);
                                }

                                if(tags is List<UnlimitedObject>)
                                {
                                    foreach(dynamic tag in tags)
                                    {
                                        postData.Add(tag);
                                    }
                                }
                            }
                        }
                        string payLoad = postData.XmlString;
                        webBrowser.Post(uri, viewModel.ResourceEnvironment, payLoad);
                    }
                }
            }
        }

        void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                var layoutObj = (e.Source as dynamic).DataContext as LayoutObjectViewModel;
                if(layoutObj != null)
                {
                    (DataContext as dynamic).SetSelected(layoutObj);
                }
            }
        }

        void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if(e.ClickCount >= 2)
            {

                if(DataContext != null)
                {
                    (DataContext as dynamic).EditCommand.Execute(null);
                }
            }
        }

        void webResourceTreeView_Selected(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedWebResource = (e.OriginalSource as dynamic).DataContext;
        }

        #endregion

        #region Implementation of IHandle<TabClosedMessage>

        public void Handle(TabClosedMessage message)
        {
            if (message.Context.Equals(this))
            {
                EventAggregator.Unsubscribe(this);
                webBrowser.Dispose();
            }
        }

        #endregion
    }
}
