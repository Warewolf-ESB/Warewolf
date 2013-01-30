using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.Views.Diagnostics;
using Infragistics.Windows.DockManager.Events;
using Microsoft.Windows.Controls.Ribbon;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Studio.Views
{

    public partial class MainView : RibbonWindow
    {
        public MainView()
        {
            InitializeComponent();

            InitializeDebugOutputWindow();
            //SetUpTextEditor();
            //Mediator.RegisterToReceiveMessage(MediatorMessages.ShowDataInOutputWindow, ShowDataInOutputWindow);
            //Mediator.RegisterToReceiveMessage(MediatorMessages.AppendDataToOutputWindow, AppendDataToOutputWindow );
            //Mediator.RegisterToReceiveMessage(MediatorMessages.ClearOutputWindow, ClearOutputWindow);
        }

        void InitializeDebugOutputWindow()
        {
            DebugOutputView debugOutputWindow = new DebugOutputView();
            DebugOutputViewModel debugTreeViewModel = new DebugOutputViewModel();
            debugOutputWindow.DataContext = debugTreeViewModel;
            OutputPane.Content = debugOutputWindow;
        }

        /*
        private void ClearOutputWindow(object input) {
            _editor.Text = string.Empty;
            OutputPane.Content = _editor;
        }

        private void AppendDataToOutputWindow(object input) {
            _editor.Text += input;
            OutputPane.Content = _editor;
        }

        private void ShowDataInOutputWindow(object input) {
            _editor.Text = input.ToString();
            OutputPane.Content = _editor;
        }

        private TextEditor _editor;
        private AbstractFoldingStrategy foldingStrategy;
        private FoldingManager foldingManager;

        private void SetUpTextEditor() {
            _editor = new TextEditor();
            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
            _editor.ShowLineNumbers = true;

            foldingStrategy = new XmlFoldingStrategy();
            foldingManager = FoldingManager.Install(_editor.TextArea);
            _editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += (sender, e) => {
                if (foldingStrategy != null && foldingManager != null) {
                    foldingStrategy.UpdateFoldings(foldingManager, _editor.Document);

                }
            };
            foldingUpdateTimer.Start();
        }
        */
        //private void treeView_MouseMove(object sender, MouseEventArgs e)
        //{

        //    var dragData = new DataObject();
        //    if(e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        //_isDragging = true;
        //        dynamic sourceTreeViewElement = e.Source;
        //        if(sourceTreeViewElement != null)
        //        {

        //            dynamic sourceTreeViewElementDataContext = sourceTreeViewElement.DataContext;
        //            if(sourceTreeViewElementDataContext != null)
        //            {
        //                var itemViewModel = sourceTreeViewElementDataContext as AbstractTreeViewModel;

        //                if(itemViewModel != null)
        //                {
        //                    if(!string.IsNullOrEmpty(itemViewModel.ActivityFullName))
        //                    {
        //                        //Sample value for activity full name
        //                        //dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, "System.Activities.Statements.FlowDecision, System.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        //                        dragData.SetData(DragDropHelper.WorkflowItemTypeNameFormat, itemViewModel.ActivityFullName);
        //                        dragData.SetData(itemViewModel);
        //                    }
        //                }

        //                dragData.SetData(sourceTreeViewElementDataContext);
        //            }
        //        }
        //        DragDrop.DoDragDrop(this, dragData, DragDropEffects.Link);
        //    }
        //}

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {

            IMainViewModel dataContext = DataContext as IMainViewModel;

            if (dataContext != null)
            {
                //Push up dependencies to the ViewModel
                //dataContext.UserInterfaceLayoutProvider.Manager = dockManager;
                dataContext.UserInterfaceLayoutProvider.Value.Manager = TabManager;
                dataContext.UserInterfaceLayoutProvider.Value.OutputPane = OutputPane;
                dataContext.UserInterfaceLayoutProvider.Value.NavigationPane = Explorer;
                // TODO PBI 8291
                //dataContext.UserInterfaceLayoutProvider.Value.PropertyPane = Properties;
                //dataContext.UserInterfaceLayoutProvider.Value.DataMappingPane = Mapping;
                dataContext.UserInterfaceLayoutProvider.Value.DataListPane = Variables;
                dataContext.LoadExplorerPage();
                dataContext.AddStartTabs();
            }
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel data = this.DataContext as MainViewModel;
            data.UserInterfaceLayoutProvider.Value.PersistTabs(TabManager.Items);
            Application.Current.Shutdown();
        }

        private void ContentPane_Closing(object sender, PaneClosingEventArgs e)
        {
            ContentControl documentToClose = sender as ContentControl;
            if (documentToClose != null)
            {
                MainViewModel data = this.DataContext as MainViewModel;
                e.Cancel = (!data.UserInterfaceLayoutProvider.Value.RemoveDocument(documentToClose.DataContext));
            }
        }
    }
}
