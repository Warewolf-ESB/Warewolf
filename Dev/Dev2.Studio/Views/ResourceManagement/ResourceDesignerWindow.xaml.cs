using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;

namespace Unlimited.Applications.BusinessDesignStudio.Views {
    /// <summary>
    /// Interaction logic for ResourceDesignerWindow.xaml
    /// </summary>
    public partial class ResourceDesignerWindow : UserControl
    {
        private readonly IResourceDesignerViewModel _resourceDesignerViewModel;

        public ResourceDesignerWindow(IResourceDesignerViewModel resourceDesignerViewModel) {
            InitializeComponent();
            _resourceDesignerViewModel = resourceDesignerViewModel;
            SetupTextEditor();

            Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateResourceDesigner, Update);
        }

        CompletionWindow completionWindow;
        FoldingManager foldingManager;
        AbstractFoldingStrategy foldingStrategy;
        internal void SetupTextEditor() {


            foldingStrategy = new XmlFoldingStrategy();
            txtResourceDef.Text = _resourceDesignerViewModel.ServiceDefinition;
            foldingManager = FoldingManager.Install(txtResourceDef.TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, txtResourceDef.Document);
            txtResourceDef.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();

            DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            foldingUpdateTimer.Tick += (sender, e) => {
                if (foldingStrategy != null && foldingManager != null) {
                    foldingStrategy.UpdateFoldings(foldingManager, txtResourceDef.Document);

                }

            };
            foldingUpdateTimer.Start();

            //Auto Completion wiring
            TextCompositionEventHandler textEntered = (sender, e) => {

                if (e.Text == "<") {

                    // open code completion after the user has pressed dot:
                    completionWindow = new CompletionWindow(txtResourceDef.TextArea);
                    // provide AvalonEdit with the data:
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

                    switch (_resourceDesignerViewModel.ResourceModel.ResourceType) {
                        case ResourceType.Service:
                            data.Add(new TextEditorCompletionData("Service", "Defines the start of a service definition"));
                            data.Add(new TextEditorCompletionData("Action", "Defines a service action"));
                            data.Add(new TextEditorCompletionData("Input", "Defines an input"));
                            data.Add(new TextEditorCompletionData("Validator", "Defines a validator"));

                            break;

                        case ResourceType.Source:
                            data.Add(new TextEditorCompletionData("Source", "Defines the start of a source definition"));
                            break;
                    }
                    completionWindow.Show();
                    completionWindow.Closed += delegate {
                        completionWindow = null;
                    };

                }

                if (e.Text == " ") {
                    completionWindow = new CompletionWindow(txtResourceDef.TextArea);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    data.Add(new TextEditorCompletionData("Name"));
                    data.Add(new TextEditorCompletionData("Type"));

                    switch (_resourceDesignerViewModel.ResourceModel.ResourceType) {
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
                    completionWindow.Show();
                    completionWindow.Closed += delegate {
                        completionWindow = null;
                    };
                }

                if (e.Text == "=" || e.Text == "\"") {

                    completionWindow = new CompletionWindow(txtResourceDef.TextArea);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

                    data.Add(new TextEditorCompletionData("Plugin"));

                    switch (_resourceDesignerViewModel.ResourceModel.ResourceType) {
                        case ResourceType.Service:
                            data.Add(new TextEditorCompletionData("InvokeStoredProc"));
                            data.Add(new TextEditorCompletionData("InvokeWebService"));


                            break;

                        case ResourceType.Source:
                            data.Add(new TextEditorCompletionData("SqlDatabase"));
                            data.Add(new TextEditorCompletionData("WebService"));
                            break;

                    }
                    completionWindow.Show();
                    completionWindow.Closed += delegate {
                        completionWindow = null;
                    };


                }

            };
            TextCompositionEventHandler textEntering = (sender, e) => {
                if (e.Text.Length > 0 && completionWindow != null) {
                    if (!char.IsLetterOrDigit(e.Text[0])) {
                        // Whenever a non-letter is typed while the completion window is open,
                        // insert the currently selected element.
                        completionWindow.CompletionList.RequestInsertion(e);
                    }
                }

            };
            txtResourceDef.TextArea.TextEntered += textEntered;
            txtResourceDef.TextArea.TextEntering += textEntering;
            txtResourceDef.TextArea.Document.TextChanged += (sender, e) => {
                _resourceDesignerViewModel.ServiceDefinition = (sender as dynamic).Text;
            };

        }

        internal void Update(object uopdatedResourceModel)
        {
            IEqualityComparer<IResourceModel> equalityComparer = ResourceModelEqualityComparer.Current;

            IResourceModel resourceModel = uopdatedResourceModel as IResourceModel;

            if (resourceModel != null && equalityComparer.Equals(_resourceDesignerViewModel.ResourceModel, resourceModel))
            {
                _resourceDesignerViewModel.ResourceModel = resourceModel;
                txtResourceDef.Text = _resourceDesignerViewModel.ServiceDefinition;
            }
        }
    }
}
