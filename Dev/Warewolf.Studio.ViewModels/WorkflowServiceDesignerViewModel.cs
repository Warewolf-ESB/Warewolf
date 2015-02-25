using System;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.View;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using Dev2.Activities;
using Dev2.Activities.Designers2.BaseConvert;
using Dev2.Activities.Designers2.Calculate;
using Dev2.Activities.Designers2.CaseConvert;
using Dev2.Activities.Designers2.CommandLine;
using Dev2.Activities.Designers2.Comment;
using Dev2.Activities.Designers2.Copy;
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
using Dev2.Activities.Designers2.Service;
using Dev2.Activities.Designers2.SortRecords;
using Dev2.Activities.Designers2.SqlBulkInsert;
using Dev2.Activities.Designers2.UniqueRecords;
using Dev2.Activities.Designers2.Unzip;
using Dev2.Activities.Designers2.WriteFile;
using Dev2.Activities.Designers2.XPath;
using Dev2.Activities.Designers2.Zip;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Utilities;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.Models.Help;

namespace Warewolf.Studio.ViewModels
{
    public class WorkflowServiceDesignerViewModel : BindableBase, IWorkflowServiceDesignerViewModel, IDockViewModel
    {
        readonly IEventAggregator _aggregator;
        bool _isActive;
        string _header;
        readonly WorkflowDesigner _wd;

        public  void ToolDropped(IToolDescriptor tool)
        {
         
        }

        public WorkflowServiceDesignerViewModel(IXamlResource resource,IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            if(resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            _aggregator.GetEvent<ToolDropped>().Subscribe((a=>ToolDropped(a)));

            Resource = resource;
            _wd = new WorkflowDesigner();
            var hashTable = new Hashtable
                {
                    {WorkflowDesignerColors.FontFamilyKey, Application.Current.Resources["DefaultFontFamily"]},
                    {WorkflowDesignerColors.FontSizeKey, Application.Current.Resources["FontSize-Normal"]},
                    {WorkflowDesignerColors.FontWeightKey, Application.Current.Resources["DefaultFontWeight"]},
                    {WorkflowDesignerColors.RubberBandRectangleColorKey, Application.Current.Resources["DesignerBackground"]},
                    {WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey, Application.Current.Resources["WorkflowBackgroundBrush"]},
                    {WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey, Application.Current.Resources["WorkflowBackgroundBrush"]},
                    {WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey, Application.Current.Resources["WorkflowSelectedBorderBrush"]},
                    {WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey, Application.Current.Resources["SolidWhite"]},
                    {WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey, Application.Current.Resources["DesignerBackground"]},
                    
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
                designerConfigService.BackgroundValidationEnabled = true; // prevent design-time background validation from blocking UI thread
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
            if (resource.Xaml == null)
            {
                IsNewWorkflow = true;
                var helper = new WorkflowHelper();
                _wd.Load(helper.CreateWorkflow("Untitled 1"));
                Header = "Untitled 1";
            }
            else
            {
                IsNewWorkflow = false;
                _wd.Text = resource.Xaml.ToString();
                _wd.Load();
            }
          
            _wd.Context.Services.Subscribe<DesignerView>(DesigenrViewSubscribe);
            _wd.View.PreviewDrop += ViewPreviewDrop;
            _wd.View.PreviewDragEnter+=ViewOnPreviewDragEnter;
        }

        private void ViewOnPreviewDragEnter(object sender, DragEventArgs dragEventArgs)
        {
//            if (dragEventArgs != null)
//            {
//                if(!DragDropHelper.AllowDrop(dragEventArgs.Data,_wd.Context,typeof(IDev2Activity)))
//                {
//                    dragEventArgs.Effects = (DragDropEffects.Move & dragEventArgs.AllowedEffects);
//                    dragEventArgs.Handled = true;
//                }
//            }            
        }

        private void ViewPreviewDrop(object sender, DragEventArgs e)
        {
            if (e != null)
            {
                
            }
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
                { typeof(DsfSequenceActivity), typeof(Dev2.Activities.Designers2.Sequence.SequenceDesigner) },
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

        void DesigenrViewSubscribe(DesignerView instance)
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
    }
}
