using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Indentation;
using Unlimited.Applications.BusinessDesignStudio;

namespace Dev2.Studio.Views.ResourceManagement
{
    /// <summary>
    ///     Interaction logic for ResourceDesignerWindow.xaml
    /// </summary>
    public partial class ResourceDesignerWindow : IHandle<UpdateResourceDesignerMessage>
    {
        private readonly IDesignerViewModel _resourceDesignerViewModel;

        private CompletionWindow _completionWindow;
        private FoldingManager _foldingManager;
        private AbstractFoldingStrategy _foldingStrategy;

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        public ResourceDesignerWindow(IDesignerViewModel resourceDesignerViewModel)
        {
            InitializeComponent();

            ImportService.SatisfyImports(this);

            _resourceDesignerViewModel = resourceDesignerViewModel;
            SetupTextEditor();
        }

        internal void SetupTextEditor()
        {
            _foldingStrategy = new XmlFoldingStrategy();
            txtResourceDef.Text = _resourceDesignerViewModel.ServiceDefinition;
            _foldingManager = FoldingManager.Install(txtResourceDef.TextArea);
            _foldingStrategy.UpdateFoldings(_foldingManager, txtResourceDef.Document);
            txtResourceDef.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

            var foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += (sender, e) =>
                {
                    if (_foldingStrategy != null && _foldingManager != null)
                    {
                        _foldingStrategy.UpdateFoldings(_foldingManager, txtResourceDef.Document);
                    }
                };
            foldingUpdateTimer.Start();

            //Auto Completion wiring
            TextCompositionEventHandler textEntered = (sender, e) =>
                {
                    if (e.Text == "<")
                    {
                        // open code completion after the user has pressed dot:
                        _completionWindow = new CompletionWindow(txtResourceDef.TextArea);
                        // provide AvalonEdit with the data:
                        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

                        switch (_resourceDesignerViewModel.ResourceModel.ResourceType)
                        {
                            case ResourceType.Service:
                                data.Add(new TextEditorCompletionData("Service",
                                                                      "Defines the start of a service definition"));
                                data.Add(new TextEditorCompletionData("Action", "Defines a service action"));
                                data.Add(new TextEditorCompletionData("Input", "Defines an input"));
                                data.Add(new TextEditorCompletionData("Validator", "Defines a validator"));

                                break;

                            case ResourceType.Source:
                                data.Add(new TextEditorCompletionData("Source",
                                                                      "Defines the start of a source definition"));
                                break;
                        }
                        _completionWindow.Show();
                        _completionWindow.Closed += delegate { _completionWindow = null; };
                    }

                    if (e.Text == " ")
                    {
                        _completionWindow = new CompletionWindow(txtResourceDef.TextArea);
                        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
                        data.Add(new TextEditorCompletionData("Name"));
                        data.Add(new TextEditorCompletionData("Type"));

                        switch (_resourceDesignerViewModel.ResourceModel.ResourceType)
                        {
                            case ResourceType.Service:

                                data.Add(new TextEditorCompletionData("SourceName"));
                                data.Add(new TextEditorCompletionData("SourceMethod"));

                                break;

                            case ResourceType.Source:
                                data.Add(new TextEditorCompletionData("ConnectionString"));
                                data.Add(new TextEditorCompletionData("Uri"));
                                data.Add(new TextEditorCompletionData("AssemblyName"));
                                data.Add(new TextEditorCompletionData("AssemblyLocation"));
                                break;
                        }
                        _completionWindow.Show();
                        _completionWindow.Closed += delegate { _completionWindow = null; };
                    }

                    if (e.Text == "=" || e.Text == "\"")
                    {
                        _completionWindow = new CompletionWindow(txtResourceDef.TextArea);
                        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

                        data.Add(new TextEditorCompletionData("Plugin"));

                        switch (_resourceDesignerViewModel.ResourceModel.ResourceType)
                        {
                            case ResourceType.Service:
                                data.Add(new TextEditorCompletionData("InvokeStoredProc"));
                                data.Add(new TextEditorCompletionData("InvokeWebService"));


                                break;

                            case ResourceType.Source:
                                data.Add(new TextEditorCompletionData("SqlDatabase"));
                                data.Add(new TextEditorCompletionData("WebService"));
                                break;
                        }
                        _completionWindow.Show();
                        _completionWindow.Closed += delegate { _completionWindow = null; };
                    }
                };
            TextCompositionEventHandler textEntering = (sender, e) =>
                {
                    if (e.Text.Length > 0 && _completionWindow != null)
                    {
                        if (!char.IsLetterOrDigit(e.Text[0]))
                        {
                            // Whenever a non-letter is typed while the completion window is open,
                            // insert the currently selected element.
                            _completionWindow.CompletionList.RequestInsertion(e);
                        }
                    }
                };
            txtResourceDef.TextArea.TextEntered += textEntered;
            txtResourceDef.TextArea.TextEntering += textEntering;
            txtResourceDef.TextArea.Document.TextChanged +=
                (sender, e) => { _resourceDesignerViewModel.ServiceDefinition = (sender as dynamic).Text; };
        }

        public void Handle(UpdateResourceDesignerMessage message)
        {
            IEqualityComparer<IResourceModel> equalityComparer = ResourceModelEqualityComparer.Current;

            var resourceModel = message.ResourceModel;

            if (resourceModel != null &&
                equalityComparer.Equals(_resourceDesignerViewModel.ResourceModel, resourceModel))
            {
                _resourceDesignerViewModel.ResourceModel = message.ResourceModel;
                txtResourceDef.Text = _resourceDesignerViewModel.ServiceDefinition;
            }
        }
    }
}