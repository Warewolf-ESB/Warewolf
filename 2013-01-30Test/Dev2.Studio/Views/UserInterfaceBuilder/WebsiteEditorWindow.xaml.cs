using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using ICSharpCode.AvalonEdit.Folding;
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
    public partial class WebsiteEditorWindow
    {
        readonly WebsiteEditorViewModel _viewModel;

        private IMainViewModel _mainViewModel;

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

        public WebsiteEditorWindow(WebsiteEditorViewModel viewModel)
        {
            InitializeComponent();
            webBrowser.Initialize();

            _viewModel = viewModel;
            SetUpTextEditor();
        }

        private void SetUpTextEditor()
        {
            FoldingManager foldingManager;
            AbstractFoldingStrategy foldingStrategy;

            txtHtml.Text = _viewModel.Html;
            foldingStrategy = new XmlFoldingStrategy();
            foldingManager = FoldingManager.Install(txtHtml.TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, txtHtml.Document);
            txtHtml.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

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

        private void layoutTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
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


        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                var layoutObj = (e.Source as dynamic).DataContext as LayoutObjectViewModel;
                if(layoutObj != null)
                {
                    (DataContext as dynamic).SetSelected(layoutObj);
                }
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if(e.ClickCount >= 2)
            {

                if(DataContext != null)
                {
                    (DataContext as dynamic).EditCommand.Execute(null);
                }
            }
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void webResourceTreeView_Selected(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedWebResource = (e.OriginalSource as dynamic).DataContext;
        }
    }
}
