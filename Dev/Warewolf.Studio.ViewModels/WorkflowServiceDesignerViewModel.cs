using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
using Dev2.Activities;
using Dev2.Activities.Designers2.BaseConvert;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.CaseConvert;
using Dev2.Activities.Designers2.CommandLine;
using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Copy;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.CountRecords;
using Dev2.Activities.Designers2.Create;
using Dev2.Activities.Designers2.DataMerge;
using Dev2.Activities.Designers2.DataSplit;
using Dev2.Activities.Designers2.DateTime;
using Dev2.Activities.Designers2.DateTimeDifference;
using Dev2.Activities.Designers2.Delete;
using Dev2.Activities.Designers2.DeleteRecords;
using Dev2.Activities.Designers2.DropBox.Upload;
using Dev2.Activities.Designers2.Email;
using Dev2.Activities.Designers2.FindIndex;
using Dev2.Activities.Designers2.FindRecordsMultipleCriteria;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Activities.Designers2.FormatNumber;
using Dev2.Activities.Designers2.GatherSystemInformation;
using Dev2.Activities.Designers2.GetWebRequest;
using Dev2.Activities.Designers2.Move;
using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Activities.Designers2.Random;
using Dev2.Activities.Designers2.ReadFile;
using Dev2.Activities.Designers2.ReadFolder;
using Dev2.Activities.Designers2.RecordsLength;
using Dev2.Activities.Designers2.Rename;
using Dev2.Activities.Designers2.Replace;
using Dev2.Activities.Designers2.Script;
using Dev2.Activities.Designers2.Sequence;
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.SortRecords;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Activities.Designers2.UniqueRecords;
using Dev2.Activities.Designers2.Unzip;
using Dev2.Activities.Designers2.WriteFile;
using Dev2.Activities.Designers2.XPath;
using Dev2.Activities.Designers2.Zip;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.Core;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Utilities;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Studio.ViewModels
{
    public class WorkflowServiceDesignerViewModel : BindableBase, IWorkflowServiceDesignerViewModel, IDockViewModel
    {
        bool _isActive;
        string _header;
        readonly WorkflowDesigner _wd;
        private DataListViewModel _dataListViewModel;
        ModelService _modelService;
        readonly WorkflowHelper _helper;

        public WorkflowServiceDesignerViewModel(IXamlResource resource)
        {
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            Resource = resource;
            _wd = new WorkflowDesigner();
            OldIntellisenseStuff();
            DesignerSetup();
            _helper = new WorkflowHelper();
            if (resource.Xaml == null)
            {
                IsNewWorkflow = true;
                Resource.ResourceName = "Untitled 1";
                _wd.Load(_helper.CreateWorkflow("Untitled 1"));
                Resource.Xaml = ServiceDefinition;
            }
            else
            {
                IsNewWorkflow = false;
                var xaml = _helper.SanitizeXaml(Resource.Xaml);
                _wd.Text = xaml.ToString();
                _wd.Load();
             
            }
            Header = Resource.ResourceName;
            _wd.Context.Services.Subscribe<DesignerView>(DesignerViewSubscribe);


            CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
            _wd.ModelChanged += WdOnModelChanged;
            _wd.View.PreviewDrop += ViewPreviewDrop;

            //_wd.View.PreviewMouseDown += ViewPreviewMouseDown;
            _wd.View.Focus();
            _helper.EnsureImplementation(_modelService);
            WorkflowDesignerIcons.Activities.Flowchart = new DrawingBrush(new ImageDrawing(new BitmapImage(new Uri(@"pack://application:,,,/Warewolf.Studio.Themes.Luna;component/Images/Workflow-32.png")), new Rect(0, 0, 16, 16)));
            WorkflowDesignerIcons.Activities.StartNode = new DrawingBrush(new ImageDrawing(new BitmapImage(new Uri(@"pack://application:,,,/Warewolf.Studio.Themes.Luna;component/Images/StartNode.png")), new Rect(0, 0, 32, 32)));
        }

        /// <summary>
        /// Views the preview drop.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        void ViewPreviewDrop(object sender, DragEventArgs e)
        {

            bool dropOccured = true;
            DataObject = e.Data.GetData(typeof(ExplorerItemViewModel));
            if (DataObject != null)
            {
                IsItemDragged.Instance.IsDragged = true;
            }

            var isWorkflow = e.Data.GetData("WorkflowItemTypeNameFormat") as string;
            if (isWorkflow != null)
            {
                // PBI 10652 - 2013.11.04 - TWR - Refactored to enable re-use!

//                var resourcePicked = ResourcePickerDialog.ShowDropDialog(ref _resourcePickerDialog, isWorkflow, out _vm);
//
//                if (_vm != null && resourcePicked)
//                {
//                    e.Data.SetData(_vm.SelectedExplorerItemModel);
//                }
//                if (_vm != null && !resourcePicked)
//                {
//                    e.Handled = true;
//                    dropOccured = false;
//                }
            }
            if (dropOccured)
            {
                //_workspaceSave = false;
                Resource.IsWorkflowSaved = false;
                //NotifyOfPropertyChange(() => DisplayName);
            }
            //_resourcePickerDialog = null;

        }


      
        protected void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete ||      //triggered from deleting an activity
                e.Command == EditingCommands.Delete ||          //triggered from editing displayname, expressions, etc
                e.Command == System.Activities.Presentation.View.DesignerView.CopyCommand ||
                e.Command == System.Activities.Presentation.View.DesignerView.CutCommand)
            {
                PreventCommandFromBeingExecuted(e);
            }
            if (e.Command == ApplicationCommands.Paste || e.Command == System.Activities.Presentation.View.DesignerView.PasteCommand)
            {
                PreventPasteFromBeingExecuted(e);
            }
        }

        /// <summary>
        ///     Prevents the delete from being executed if it is a FlowChart.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        void PreventCommandFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
            if (Designer != null && Designer.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if (selection == null || selection.PrimarySelection == null)
                    return;

                if (selection.PrimarySelection.ItemType != typeof(Flowchart) &&
                   selection.SelectedObjects.All(modelItem => modelItem.ItemType != typeof(Flowchart)))
                    return;
            }

            e.CanExecute = false;
            e.Handled = true;
        }

        void PreventPasteFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
        }

        public WorkflowDesigner Designer { get { return _wd; } }
        protected void WdOnModelChanged(object sender, EventArgs eventArgs)
        {
            var checkServiceDefinition = CheckServiceDefinition();
            var checkDataList = CheckDataList();
            
            var isWorkflowSaved = checkServiceDefinition && checkDataList;
            Resource.IsWorkflowSaved = isWorkflowSaved;
            //_workspaceSave = false;
            if(!isWorkflowSaved)
            {
                Header = Resource.ResourceName + "*";
            }
            else
            {
                Header = Resource.ResourceName;
            }
        }
        string _originalDataList;
        bool CheckDataList()
        {
            if (_originalDataList == null)
                return true;
            if (Resource.DataList != null)
            {
                string currentDataList = Resource.DataList.Replace("<DataList>", "").Replace("</DataList>", "");
                return currentDataList.SpaceCaseInsenstiveComparision(_originalDataList);
            }
            return true;
        }


        void ModelServiceSubscribe(ModelService instance)
        {
            _modelService = instance;
            _modelService.ModelChanged += ModelServiceModelChanged;
        }
        public static readonly string[] SelfConnectProperties =
        {
            "Next", 
            "True", 
            "False", 
            "Default", 
            "Key"
        };

        protected dynamic DataObject { get; set; }

         protected void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
         {
         // BUG 9143 - 2013.07.03 - TWR - added
            if (e.ModelChangeInfo != null &&
                e.ModelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged)
            {
                if (SelfConnectProperties.Contains(e.ModelChangeInfo.PropertyName))
                {
                    if (e.ModelChangeInfo.Subject == e.ModelChangeInfo.Value)
                    {
                        var modelProperty = e.ModelChangeInfo.Value.Properties[e.ModelChangeInfo.PropertyName];
                        if (modelProperty != null)
                        {
                            modelProperty.ClearValue();
                        }
                    }
                    return;
                }

                if (e.ModelChangeInfo.PropertyName == "StartNode")
                {
                    return;
                }
            }

            //ItemsAdded is obsolete - see e.ModelChangeInfo for correct usage
            //Code below is obsolete
#pragma warning disable 618
            if (e.ItemsAdded != null)
            {
                PerformAddItems(e.ItemsAdded.ToList());
            }
            else if (e.PropertiesChanged != null)
            {
                if (e.PropertiesChanged.Any(mp => mp.Name == "Handler"))
                {
                    if (DataObject != null)
                    {
                        ModelItemPropertyChanged(e);
                    }
                    else
                    {
                        ModelItemAdded(e);
                    }
                }
            }
        }

        void ModelItemAdded(ModelChangedEventArgs e)
        {
            ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");
#pragma warning restore 618

            if (modelProperty != null)
            {
//                if (_vm != null)
//                {
//                    IContextualResourceModel resource = _vm.SelectedResourceModel;
//                    if (resource != null)
//                    {
//                        DsfActivity droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());
//
//                        droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.Category;
//                        droppedActivity.IconPath = resource.IconPath;
//
//                        modelProperty.SetValue(droppedActivity);
//                    }
//                    _vm.Dispose();
//                    _vm = null;
//                }
            }
        }

        void ModelItemPropertyChanged(ModelChangedEventArgs e)
        {
#pragma warning disable 618
            var navigationItemViewModel = DataObject as ExplorerItemViewModel;

            // ReSharper disable CSharpWarnings::CS0618
            ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");
            // ReSharper restore CSharpWarnings::CS0618

            if (navigationItemViewModel != null && modelProperty != null)
            {
                //IEnvironmentViewModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == navigationItemViewModel.EnvironmentId);
                //if (environmentModel != null)
                {
//                    var resource = environmentModel.ResourceRepository.FindSingle(c => c.ID == navigationItemViewModel.ResourceId) as IContextualResourceModel;
//                    if (resource != null)
//                    {
//                        //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
//                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());
//                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
//                        d.IconPath = resource.IconPath;
//                        UpdateForRemote(d, resource);
//                        modelProperty.SetValue(d);
//                    }
                }
            }
            DataObject = null;
#pragma warning restore 618
        }

        protected List<ModelItem> PerformAddItems(List<ModelItem> addedItems)
        // ReSharper restore ExcessiveIndentation
        // ReSharper restore MethodTooLong
        {
            for (int i = 0; i < addedItems.Count(); i++)
            {
                var mi = addedItems.ToList()[i];

                if (mi.Content != null)
                {
                    var computedValue = mi.Content.ComputedValue;
                    if (computedValue is IDev2Activity)
                    {
                        //2013.08.19: Ashley Lewis for bug 10116 - New unique id on paste
                        (computedValue as IDev2Activity).UniqueID = Guid.NewGuid().ToString();
                    }
                }


                if (mi.ItemType == typeof(FlowSwitch<string>))
                {
                }
                else if (mi.ItemType == typeof(FlowDecision))
                {
                }
                else if (mi.ItemType == typeof(FlowStep))
                {
                }
            }
            return addedItems;
        }


        void ProcessDataListOnLoad()
        {
            //AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(true, false);
        }

        bool CheckServiceDefinition()
        {
            return ServiceDefinition.IsEqual(Resource.Xaml);
        }

        public StringBuilder ServiceDefinition { get { return _helper.SerializeWorkflow(_modelService); } }

        private void OldIntellisenseStuff()
        {
            _dataListViewModel = new DataListViewModel();
            if (!String.IsNullOrEmpty(Resource.DataList))
            {
                Resource.DataList = Resource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"")
                    .Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
            }
            _dataListViewModel.InitializeDataListViewModel(Resource);
            DataListSingleton.SetDataList(_dataListViewModel);
        }

        private void DesignerSetup()
        {
            var hashTable = new Hashtable
            {
                {WorkflowDesignerColors.FontFamilyKey, Application.Current.Resources["DefaultFontFamily"]},
                {WorkflowDesignerColors.FontSizeKey, Application.Current.Resources["FontSize-Normal"]},
                {WorkflowDesignerColors.FontWeightKey, Application.Current.Resources["DefaultFontWeight"]},
                {WorkflowDesignerColors.RubberBandRectangleColorKey, Application.Current.Resources["DesignerBackground"]},
                {
                    WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey,
                    Application.Current.Resources["WorkflowBackgroundBrush"]
                },
                {
                    WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey,
                    Application.Current.Resources["WorkflowBackgroundBrush"]
                },
                {
                    WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey,
                    Application.Current.Resources["WorkflowSelectedBorderBrush"]
                },
                {
                    WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey,
                    Application.Current.Resources["ShellBarViewBackground"]
                },
                {
                    WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey,
                    Application.Current.Resources["ShellBarViewBackground"]
                },
                {
                    WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey,
                    Application.Current.Resources["ShellBarViewBackground"]
                },
                {WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey, Application.Current.Resources["SolidWhite"]},
                {
                    WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey,
                    Application.Current.Resources["DesignerBackground"]
                },
            };
            _wd.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);
            var designerConfigService = _wd.Context.Services.GetService<DesignerConfigurationService>();
            if (designerConfigService != null)
            {
                // set the runtime Framework version to 4.5 as new features are in .NET 4.5 and do not exist in .NET 4
                designerConfigService.TargetFrameworkName = new FrameworkName(".NETFramework", new Version(4, 5));
                designerConfigService.AutoConnectEnabled = true;
                designerConfigService.AutoSplitEnabled = true;
                designerConfigService.PanModeEnabled = true;
                designerConfigService.RubberBandSelectionEnabled = true;
                designerConfigService.BackgroundValidationEnabled = true;
                    // prevent design-time background validation from blocking UI thread
                // Disabled for now
                designerConfigService.AnnotationEnabled = false;
                designerConfigService.AutoSurroundWithSequenceEnabled = false;
            }
            var wdMeta = new DesignerMetadata();
            wdMeta.Register();
            var designerAttributes = GetTools();
            var builder = new AttributeTableBuilder();
            foreach (var designerAttribute in designerAttributes)
            {
                builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());
            _wd.Context.Services.Subscribe<ModelService>(ModelServiceSubscribe);
        }

        public Dictionary<Type, Type> GetTools()
        {
            var designerAttributes = new Dictionary<Type, Type>
            {
                { typeof(DsfMultiAssignActivity), typeof(MultiAssignDesigner) },
                { typeof(DsfDateTimeActivity), typeof(DateTimeDesigner) },
                { typeof(DsfWebGetRequestActivity), typeof(GetWebRequestDesigner) },
                { typeof(DsfFindRecordsMultipleCriteriaActivity), typeof(FindRecordsMultipleCriteriaDesigner) },
                { typeof(DsfSqlBulkInsertActivity), typeof(SqlBulkInsertDesigner) },
                { typeof(DsfSortRecordsActivity), typeof(SortRecordsDesigner) },
                { typeof(DsfCountRecordsetActivity), typeof(CountRecordsDesigner) },
                { typeof(DsfRecordsetLengthActivity), typeof(RecordsLengthDesigner) },
                { typeof(DsfDeleteRecordActivity), typeof(DeleteRecordsDesigner) },
                { typeof(DsfUniqueActivity), typeof(UniqueRecordsDesigner) },
                { typeof(DsfCalculateActivity), typeof(CalculateDesigner) },
                { typeof(DsfBaseConvertActivity), typeof(BaseConvertDesigner) },
                { typeof(DsfNumberFormatActivity), typeof(FormatNumberDesigner) },
                { typeof(DsfPathCopy), typeof(CopyDesigner) },
                { typeof(DsfPathCreate), typeof(CreateDesigner) },
                { typeof(DsfPathMove), typeof(MoveDesigner) },
                { typeof(DsfPathDelete), typeof(DeleteDesigner) },
                { typeof(DsfFileRead), typeof(ReadFileDesigner) },
                { typeof(DsfFileWrite), typeof(WriteFileDesigner) },
                { typeof(DsfFolderRead), typeof(ReadFolderDesigner) },
                { typeof(DsfPathRename), typeof(RenameDesigner) },
                { typeof(DsfUnZip), typeof(UnzipDesigner) },
                { typeof(DsfZip), typeof(ZipDesigner) },
                { typeof(DsfExecuteCommandLineActivity), typeof(CommandLineDesigner) },
                { typeof(DsfCommentActivity), typeof(CommentDesigner) },
                { typeof(DsfSequenceActivity), typeof(SequenceDesigner) },
                { typeof(DsfDateTimeDifferenceActivity), typeof(DateTimeDifferenceDesigner) },
                { typeof(DsfSendEmailActivity), typeof(EmailDesigner) },
                { typeof(DsfIndexActivity), typeof(FindIndexDesigner) },
                { typeof(DsfRandomActivity), typeof(RandomDesigner) },
                { typeof(DsfReplaceActivity), typeof(ReplaceDesigner) },
                { typeof(DsfScriptingActivity), typeof(ScriptDesigner) },
                { typeof(DsfForEachActivity), typeof(ForeachDesigner) },
                { typeof(DsfCaseConvertActivity), typeof(CaseConvertDesigner) },
                { typeof(DsfDataMergeActivity), typeof(DataMergeDesigner) },
                { typeof(DsfDataSplitActivity), typeof(DataSplitDesigner) },
                { typeof(DsfGatherSystemInformationActivity), typeof(GatherSystemInformationDesigner) },
                { typeof(DsfXPathActivity), typeof(XPathDesigner) },
                { typeof(DsfActivity), typeof(ServiceDesigner) },
                { typeof(DsfDatabaseActivity), typeof(ServiceDesigner) },
                { typeof(DsfWebserviceActivity), typeof(ServiceDesigner) },
                { typeof(DsfPluginActivity), typeof(ServiceDesigner) },                
                { typeof(DsfDropBoxFileActivity), typeof(DropboxUploadFileDesigner) },
            };
            return designerAttributes;
        }

        void DesignerViewSubscribe(DesignerView instance)
        {
            // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
        }

        /// <summary>
        /// The resource that is being represented
        /// </summary>
        public IXamlResource Resource
        {
            get;
            set;
        }
        /// <summary>
        /// Should the hyperlink to execute the service in browser
        /// </summary>
        public bool IsServiceLinkVisible
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Command to execute when the hyperlink is clicked
        /// </summary>
        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The hyperlink text shown
        /// </summary>
        public string DisplayWorkflowLink
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// The designer for the resource
        /// </summary>
        public UIElement DesignerView
        {
            get { return _wd.View; }
        }

        #region Implementation of IActiveAware

        /// <summary>
        /// Gets or sets a value indicating whether the object is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the object is active; otherwise <see langword="false"/>.
        /// </value>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged(() => IsActive);
                if (_isActive)
                {
                    DataListSingleton.SetDataList(_dataListViewModel);
                }
            }
        }
        public event EventHandler IsActiveChanged;

        #endregion

        #region Implementation of IDockAware

        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
                OnPropertyChanged(() => Header);
            }
        }

        public ResourceType? Image
        {
            get { return ResourceType.WorkflowService; }
        }

        public bool IsNewWorkflow { get; set; }
        public string DesignerText
        {
            get
            {
                return _wd.Text;
            }
        }

        #endregion

        public void UpdateHelpDescriptor(string helpText)
        {
        }
    }
}
