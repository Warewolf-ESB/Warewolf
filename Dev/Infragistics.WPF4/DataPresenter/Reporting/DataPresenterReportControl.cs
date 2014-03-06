using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Reporting;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.Diagnostics;
using System.Windows.Markup.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{





	internal class DataPresenterReportControl  :
        // MBS 7/24/09 - NA9.2 Excel Exporting
        //DataPresenterBase, IEmbeddedVisualPaginator
        DataPresenterExportControlBase, IEmbeddedVisualPaginator
    {
        #region Member variables
        private SectionContainer _sectionContainer;

        // MBS 7/24/09 - NA9.2 Excel Exporting
        // Refactored into the new base class
        #region Refactored

        //private Dictionary<FieldLayout, FieldLayout> _associatedFieldLayouts;
        //private bool _bypassInitializeRecordEvent;
        //private int _reportVersion;
        //private DataPresenterBase _sourceDataPresenter;

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        // Rather than rely on the name of the field (which has perf implications and 
        // also is unreliable since a name is optional at least for unbound fields) 
        // we will maintain a mapping similar to what we did for field layouts.\
        //
        //private Dictionary<Field, Field> _associatedFields;

        #endregion //Refactored

        #endregion //Member variables

        #region Constructors



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


        internal DataPresenterReportControl(DataPresenterBase sourceDataPresenter)
            // MBS 7/24/09 - NA9.2 Excel Exporting
            //: base()
            : base(sourceDataPresenter)
        {
            // MBS 7/24/09 - NA9.2 Excel Exporting
            //this._sourceDataPresenter = sourceDataPresenter;
        }

        #endregion //Constructors	
    
        #region Base class overrides

        // MBS 7/24/09 - NA9.2 Excel Exporting
        // Moved to base class
        #region OnInitializeRecord - Refactored

        //protected internal override void OnInitializeRecord(InitializeRecordEventArgs e)
        //{
        //    // JJD 9/30/08
        //    // When we are in a report we want to bypass raising the InitializeRecord event
        //    // until after the state has been copied over from the associated record
        //    if (this._bypassInitializeRecordEvent)
        //        return;

        //    base.OnInitializeRecord(e);
        //}

        #endregion Refactored

        // MBS 7/24/09 - NA9.2 Excel Exporting
        // Added overrides to the new methods exposed as part of refactoring the
        // BeginPagination method.  These methods contain portions of code refactored
        // from the origina method and are documented in the CloneSourceDataPresenter method
        #region OnBeginCloneSource

        protected override void OnBeginCloneSource(ReportSection section)
        {
            base.OnBeginCloneSource(section);

            // JD 9/30/08
            // We need to create a container for the section so we can use it as a 
            // convenient plave to prevent routed events from bubbling up to
            // the ui datapresenter
            if (this._sectionContainer == null)
            {
                this._sectionContainer = new SectionContainer();
                this._sectionContainer.Section = section;

                // Make the section a logical child of the source UI datapresenter
                //
                // MBS 7/24/09 - NA9.2 Excel Exporting
                //this._sourceDataPresenter.InternalAddLogicalChild(this._sectionContainer);
                this.SourceDataPresenter.InternalAddLogicalChild(this._sectionContainer);
            }
            else
                this._sectionContainer.Section = section;

            // JJD 9/2/09 - TFS19609
            // If we are inheriting a FlowDirection of RightToLeft from the UI DP
            // (which will show up on the section containter since it is inherited)
            // we want to first flip it on the secrion container and then
            // set RightToLeft on this component. Otherwise, the framework
            // won't apply the proper ttransform since it thiknks the transfer was
            // already done at an ancestor level.
            object localFlowDirection = this.ReadLocalValue(FlowDirectionProperty);

            if (localFlowDirection == DependencyProperty.UnsetValue &&
                this._sectionContainer.FlowDirection == FlowDirection.RightToLeft)
            {
                this._sectionContainer.FlowDirection = FlowDirection.LeftToRight;
                this.FlowDirection = FlowDirection.RightToLeft;
            }

        }
        #endregion //OnBeginCloneSource
        //
        #region OnCloneDataSourceRequested

        protected override void OnCloneDataSourceRequested()
        {
            base.OnCloneDataSourceRequested();

            // apply data grid style
            this.Style = DataPresenterReportControlStyle;
            this.InvalidateMeasure();
            this.UpdateLayout();

            // Use DataSourceInternal which supports both DataSource and DataItems.
            //
            // MBS 7/24/09 - NA9.2 Excel Exporting
            // Use the new method to bind the data source, since we want to bind at a different time
            // when using the ExcelExporter
            //this.DataSource = this._sourceDataPresenter.DataSourceInternal as IEnumerable;
            this.BindToDataSource();

            this.CreateFlattenedListOfData();
        }
        #endregion //OnCloneDataSourceRequested
        //
        #region OnEndCloneSource

        protected override void OnEndCloneSource(ReportSection section)
        {
            base.OnEndCloneSource(section);

            Debug.Assert(this.CurrentPanel != null);

            ((IEmbeddedVisualPaginator)this.CurrentPanel).BeginPagination(section);
        }
        #endregion //OnEndCloneSource

        // MBS 7/28/09 - NA9.2 Excel Exporting
        // Refactored this logic from the original CreateFlattenedListHelper method, which has since been mainly moved
        // to the base DataPresenterExportControlBase class since most of that logic can be shared between
        // exporting and reporting
        #region OnTraverseVisibleRecord

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Refactor the enumeration logic into the ExportRecordEnumeratorBase class so 
		// we can break up the operation for asynchronous processing.
		//
		//protected override bool OnTraverseRecord(Record record, Visibility visibility, RecordManager recordManager, IExportOptions options,
		//    List<Record> flattenedList, out bool skipSiblings)
		//{
		//    if (!base.OnTraverseRecord(record, visibility, recordManager, options, flattenedList, out skipSiblings))
		//        return false;
		//
		//    if (visibility != Visibility.Collapsed)
		//    {
		//        // MD 6/3/10 - ChildRecordsDisplayOrder feature
		//        // Cache the value indicating whether the child record should be displayed after the parent.
		//        bool areChildrenAfterParent = record.AreChildrenAfterParent;
		//
		//        // MD 6/3/10 - ChildRecordsDisplayOrder feature
		//        // Only add the record first if the children should go after the parent.
		//        if (areChildrenAfterParent)
		//        {
		//        // MBS 7/28/09 - NA9.2 Excel Exporting
		//        // Sanity check after all the refactoring
		//        if (flattenedList == null)
		//            Debug.Fail("We should always have List<Record> when traversing rows with the ReportControl");
		//        else
		//            flattenedList.Add(record);
		//        }
		//
		//        // JJD 9/30/08
		//        // If the rcd is expanded and visible then call this method recursively
		//        if (record.IsExpanded && visibility == Visibility.Visible)
		//        {
		//            ViewableRecordCollection children = record.ViewableChildRecords;
		//
		//            if ((children != null) && (children.Count != 0) && record.HasChildren)
		//            {
		//                // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
		//                // Added RecordManager param
		//                //
		//                // MBS 7/28/09 - NA9.2 Excel Exporting
		//                //this.CreateFlattenedListHelper(children, recordManager, ref flattenedList);
		//                this.TraverseVisibleRecords(children, recordManager, options, flattenedList);
		//            }
		//        }
		//
		//        // MD 6/3/10 - ChildRecordsDisplayOrder feature
		//        // If the children should be before the parent, add the record now.
		//        if (areChildrenAfterParent == false)
		//        {
		//            if (flattenedList == null)
		//                Debug.Fail("We should always have List<Record> when traversing rows with the ReportControl");
		//            else
		//                flattenedList.Add(record);
		//        }
		//    }
		//
		//    return true;
		//}
        #endregion OnTraverseVisibleRecord

        #endregion //Base class overrides

        #region Properties

            #region Internal Properties

                // MBS 7/29/09 - NA9.2 Excel Exporting
                // Moved to base class
                #region ReportVersion - Refactored

                //internal int ReportVersion { get { return this._reportVersion; } }

                #endregion //ReportVersion	
    
                #region Section

        internal ReportSection Section 
        { 
            get 
            { 
                return this._sectionContainer != null ? this._sectionContainer.Section as ReportSection : null; 
            } 
        }
        
                #endregion

                // MBS 7/24/09 - NA9.2 Excel Exporting
                // Moved to base class
                #region SourceDataPresenter - Refactored

        //internal DataPresenterBase SourceDataPresenter { get { return this._sourceDataPresenter; } }

                #endregion //SourceDataPresenter

            #endregion //Internal Properties

            #region Private Properties

                #region DataPresenterReportControlStyle static

        private static Style s_DataPresenterReportControlStyle;

        private static Style DataPresenterReportControlStyle
        {
            get
            {
                if (s_DataPresenterReportControlStyle == null)
                {
                    s_DataPresenterReportControlStyle = new Style(typeof(DataPresenterReportControl));

                    FrameworkElementFactory fefBorder = new FrameworkElementFactory(typeof(Border));
                    FrameworkElementFactory fefGrid = new FrameworkElementFactory(typeof(Grid));

                    fefGrid.Name = "PART_ContentSiteGrid";

                    fefBorder.SetBinding(Border.BackgroundProperty, Utilities.CreateBindingObjectLikeAlias(Control.BackgroundProperty));
                    fefBorder.SetBinding(Border.BorderBrushProperty, Utilities.CreateBindingObjectLikeAlias(Control.BorderBrushProperty));
                    fefBorder.SetBinding(Border.BorderThicknessProperty, Utilities.CreateBindingObjectLikeAlias(Control.BorderThicknessProperty));
                    fefBorder.AppendChild(fefGrid);

                    ControlTemplate template = new ControlTemplate(typeof(DataPresenterReportControl));
                    template.VisualTree = fefBorder;

                    s_DataPresenterReportControlStyle.Setters.Add(new Setter(Control.TemplateProperty, template));
                    s_DataPresenterReportControlStyle.Seal();
                }
                return s_DataPresenterReportControlStyle;

            }
        }

                #endregion //DataPresenterReportControlStyle static	
    
            #endregion //Private Properties	
    
        #endregion //Properties	
        
        #region Methods

            #region Internal Methods

                // MBS 7/24/09 - NA9.2 Excel Exporting
                // Refactored to base class
                //
                #region Refactored

                //        #region CloneHelper
                ///* AS 9/22/083
                // * We originally copied the ribbon cloning code but that causes problems
                // * because then we don't end up picking up bug fixes in the other assembly.
                // * I refactored the ribbon's code into a helper class that we can share from
                // * this assembly.
                // * 
                //internal static object CloneHelper(object source)
                //{
                //    // the following requires unmanaged code rights so we will do what the 
                //    // xamlwriter would have done
                //    //string xaml = XamlWriter.Save(tool);
                //    //FrameworkElement clone = XamlReader.Load(new XmlTextReader(new StringReader(xaml))) as FrameworkElement;

                //    MarkupObject obj = MarkupWriter.GetMarkupObjectFor(source);
                //    XamlDesignerSerializationManager manager = new XamlDesignerSerializationManager(null);
                //    manager.XamlWriterMode = XamlWriterMode.Expression;
                //    object clone = CloneHelper(obj, manager);
                //    return clone;
                //}

                //private static object CloneHelper(MarkupObject mo, IServiceProvider serviceProvider)
                //{
                //    DependencyObject dependency = mo.Instance as DependencyObject;

                //    // for dependency objects, we'll just use the value as is
                //    if (dependency is DependencyObject == false)
                //        return mo.Instance;

                //    // otherwise clone the object
                //    dependency = (DependencyObject)Activator.CreateInstance(mo.ObjectType);

                //    // now copy over the properties
                //    //int count =mo.Properties.c
                //    //for (int ind = 0; ind < count; ind++)
                //    //(MarkupProperty mp in mo.Properties)
                //    //IEnumerator<MarkupProperty> enumerator = mo.Properties.GetEnumerator();
                //    //while(enumerator.MoveNext())
                    
                //    foreach (MarkupProperty mp in mo.Properties)
                //    {
                //        //MarkupProperty mp = enumerator.Current;
                //        object propValue = mp.Value;

                //        // if the value is a visual - e.g. the content of an object
                //        // is another element
                //        if (propValue is Visual || propValue is ContentElement)
                //        {
                //            // we need to get the markup for that object and use that as the value
                //            MarkupObject moChildPropValue = MarkupWriter.GetMarkupObjectFor(propValue);
                //            propValue = CloneHelper(moChildPropValue, serviceProvider);
                //        }

                //        // if this is a dependency property then set the value of that
                //        #region Dependency Property
                //        if (mp.DependencyProperty != null)
                //        {
                //            MarkupExtension markupExtension = propValue as MarkupExtension;

                //            // TODO handle markup extension value
                //            if (null != markupExtension)
                //                propValue = markupExtension.ProvideValue(serviceProvider);

                //            dependency.SetValue(mp.DependencyProperty, propValue);
                //        }
                //        // clr .net propety
                //        else
                //        {
                //            if (!mp.IsComposite)
                //                dependency.GetType().GetProperty(mp.Name).SetValue(dependency, propValue, null);  //  SetValue(mp.DependencyProperty, propValue);
                //        }
                //        #endregion //Dependency Property

                //        #region Iterate the markup items
                //        if (mp.Items != null)
                //        {
                //            // get the collection from the new object
                //            object collection = mp.PropertyDescriptor.GetValue(dependency);
                //            IList list = collection as IList;

                //            if (null != list)
                //            {
                //                // create all the children
                //                foreach (MarkupObject moChild in mp.Items)
                //                {
                //                    object moChildValue = CloneHelper(moChild, serviceProvider);
                //                    list.Add(moChildValue);
                //                }
                //            }
                //            //else
                //            //{
                //            //    Debug.WriteLine(string.Format("Items collection present but not an IList - Name={0}, Type={1}", mp.Name, mp.PropertyType));
                //            //}

                //        }
                //        #endregion //Iterate the markup items
                //    }

                //    return dependency;
                //}
                //*/
                //// JJD 2/9/09 - TFS13678
                //// Added copyNonSerializableProperties
                ////internal static object CloneHelper(object source)
                //internal static object CloneHelper(object source, bool copyNonSerializableProperties)
                //{
                //    //return new CloneManager().Clone(source);
                //    object clone = new CloneManager().Clone(source);

                //    // JJD 2/9/09 - TFS13678
                //    // Copy over those properties that are marked DesignerSerializationVisibility.Hidden
                //    // since the CloneHelper method will miss those
                //    if ( copyNonSerializableProperties )
                //        GridUtilities.CopyNonSerializableSettings(clone, source);

                //    return clone;
                //}
                //        #endregion //CloneHelper

                //        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                //        #region GetClonedField

                //internal Field GetClonedField(Field uiLayout)
                //{
                //    if (this._associatedFields == null)
                //        return null;

                //    Field clonedFl;

                //    this._associatedFields.TryGetValue(uiLayout, out clonedFl);

                //    return clonedFl;
                //}

                //        #endregion //GetClonedField	
            
                //        #region GetClonedFieldLayout

                //internal FieldLayout GetClonedFieldLayout(FieldLayout uiLayout)
                //{
                //    if (this._associatedFieldLayouts == null)
                //        return null;

                //    FieldLayout clonedFl;

                //    this._associatedFieldLayouts.TryGetValue(uiLayout, out clonedFl);

                //    return clonedFl;
                //}

                //#endregion //GetClonedFieldLayout	
         
                #endregion //Refactored

            #endregion //Internal Methods

            #region Private Methods

                #region CreateFlattenedListOfData

            private void CreateFlattenedListOfData()
            {
				List<Record> flattenedList = new List<Record>(this.ViewableRecords.ScrollCount);

				// AS 3/3/11 NA 2011.1 - Async Exporting
				// Refactor the enumeration logic into the ExportRecordEnumeratorBase class so 
				// we can break up the operation for asynchronous processing.
				//
				//// JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
				//// Added RecordManager param
				////
				//// MBS 7/28/09 - NA9.2 Excel Exporting
				//// Moved the common logic into a base class for recursively traversing the records
				////
				////this.CreateFlattenedListHelper(this.ViewableRecords, null, ref flattenedList);
				//this.TraverseVisibleRecords(this.ViewableRecords, null, this.CurrentViewInternal as IExportOptions, flattenedList);
				ReportRecordEnumerator enumerator = new ReportRecordEnumerator(this, this.ViewableRecords, this.CurrentViewInternal as IExportOptions, flattenedList);
				ExportProcessResult result = enumerator.Process(null);
				Debug.Assert(result == ExportProcessResult.Completed);

				// JJD 8/13/10 - TFS35641
				// Handle the special case where there weren't any rcds.
				// In this case we need to add the template rcd so the header will be displayed.
				if (flattenedList.Count == 0)
				{
					FieldLayout fl = this.ViewableRecords.FieldLayout;

					if (fl != null && fl.TemplateDataRecord != null)
						flattenedList.Add(fl.TemplateDataRecord);
				}

                this.CurrentRecordListControl.ItemsSource = flattenedList;
            }

            // MBS 7/28/09 - NA9.2 Excel Exporting
            // Refactored into the base class so that it can be used for the exporting logic
            //
            #region Refactored

            //// JJD 10/16/08 - TFS8092
            //// Change the signature so we can deal with the ViewableRecordCollection
            ////private void CreateFlattenedListHelper(IEnumerable records, ref List<Record> flattenedList)
            //// JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
            //// Added RecordManager param
            ////private void CreateFlattenedListHelper(ViewableRecordCollection records, ref List<Record> flattenedList)
            //private void CreateFlattenedListHelper(ViewableRecordCollection records, RecordManager recordManager, ref List<Record> flattenedList)
            //{
            //    ReportViewBase view = this.CurrentViewInternal as ReportViewBase;

            //    // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
            //    // Copy the filters over before we process the records so their
            //    // filter state will get updated properly
            //    if (recordManager != records.RecordManager)
            //    {
            //        recordManager = records.RecordManager;

            //        if (recordManager != null)
            //        {
            //            RecordManager associatedRm = recordManager.AssociatedRecordManager;

            //            if (associatedRm != null)
            //            {
            //                // Bypass the root manager and don't try to clone its filters since we never 
            //                // use the record manager to hold the filters for root records. we 
            //                // always store them on the fieldlayout
            //                if (!view.ExcludeRecordFilters &&
            //                    !recordManager.IsRootManager)
            //                {
            //                    if (recordManager.RecordFilters.Count == 0)
            //                    {
            //                        if (associatedRm.RecordFiltersIfAllocated != null)
            //                            recordManager.RecordFilters.CloneFrom(associatedRm.RecordFilters);
            //                    }
            //                }
            //            }

            //            ViewableRecordCollection associatedVrc = associatedRm.ViewableRecords;

            //            // JJD 7/1/09 - NA 2009 Vol 2 - Record fixing
            //            // Maintain a map for fixed rcds keyed by the associated rcd
            //            // from the main grid
            //            if (associatedVrc != null &&
            //                associatedVrc.CountOfFixedRecordsOnBottom + associatedVrc.CountOfFixedRecordsOnTop > 0)
            //            {
            //                recordManager.ViewableRecords.CloneFixedRecords(associatedVrc);
            //            }
            //        }
            //    }

            //    // JJD 10/16/08 - TFS8092
            //    // First copy the records from the ViewableRecordCollection into a stack list
            //    // so that as we walk over the list the count won't change based on 
            //    // visibility of record being set to Collapsed
            //    List<Record> recordList = new List<Record>(records);

            //    // JJD 10/16/08 - TFS8092
            //    // Cache the count since it would be too expensive to get it the loop below
            //    //int count = records.Count;
            //    int count = recordList.Count;

            //    // JJD 10/16/08 - TFS8092
            //    // Don't use an enumerator since raising the InitializeRecord event can result in a listener
            //    // collapsing a record which would in effect pull it out of the ViewableRecordsCollection
            //    //foreach (Record rcd in records)
            //    for (int i = 0; i < count; i++)
            //    {
            //        Record rcd = recordList[i] as Record;

            //        Debug.Assert(rcd != null);

            //        if (rcd == null)
            //            continue;

            //        // JJD 1/20/09 - NA 2009 vol 1 - Record filtering
            //        // ignore FilterRecords
            //        if (rcd is FilterRecord)
            //            continue;

            //        DataRecord dr = rcd as DataRecord;

            //        // ignore add records
            //        if (dr != null && dr.IsAddRecord)
            //            continue;


            //        // JJD 10/17/08 - TFS8092
            //        // Don't add the record until we know if its Visibility is set to Collapsed below
            //        //flattenedList.Add(rcd);

            //        Record associatedRcd = rcd.GetAssociatedRecord();

            //        if (associatedRcd != null)
            //            rcd.CloneAssociatedRecordSettings(associatedRcd, view);

            //        // set the bypass fkag to false to we can raise the InitializeRecord event below
            //        //
            //        // MBS 7/24/09 - NA9.2 Excel Exporting
            //        //this._bypassInitializeRecordEvent = false;
            //        this.BypassInitializeRecordEvent = false;

            //        // Raise the InitilizeRecord event now that we have cloned all of the settings
            //        //~ SSP 3/3/09 TFS11407
            //        //~ Pass along the new reInitialize parameter.
            //        //~ 
            //        //~this.RaiseInitializeRecord(rcd);
            //        this.RaiseInitializeRecord(rcd, false);

            //        // reset the flag for the next record
            //        //
            //        // MBS 7/24/09 - NA9.2 Excel Exporting
            //        //this._bypassInitializeRecordEvent = true;
            //        this.BypassInitializeRecordEvent = true;

            //        // JJD 10/17/08 - TFS8092
            //        // Now that we have rasied the InitializeRecord event we can check its Visiblity 
            //        // before adding it to the flattened list
            //        Visibility visibility = rcd.VisibilityResolved;

            //        if (visibility != Visibility.Collapsed)
            //        {
            //            flattenedList.Add(rcd);

            //            // JJD 9/30/08
            //            // If the rcd is expanded and visible then call this method recursively
            //            if (rcd.IsExpanded && visibility == Visibility.Visible)
            //            {
            //                ViewableRecordCollection children = rcd.ViewableChildRecords;

            //                if ((children != null) && (children.Count != 0) && rcd.HasChildren)
            //                {
            //                    // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
            //                    // Added RecordManager param
            //                    this.CreateFlattenedListHelper(children, recordManager, ref flattenedList);
            //                }
            //            }
            //        }
            //    }
            //}
                #endregion //Refactored               
        
                    #endregion //CreateFlattenedListOfData	
        
            #endregion //Private Methods	
    
        #endregion //Methods	
    
        #region IEmbeddedVisualPaginator Members

        /// <summary>
        /// Create a copy of UI XamGrid with flattend list as a data source.
        /// </summary>
        /// <param name="section"></param>
        void IEmbeddedVisualPaginator.BeginPagination(ReportSection section)
        {
            // MBS 7/24/09 - NA9.2 Excel Exporting
            // Refactored this logic into a method on the new base class, since much of the cloning
            // logic is the same for exporting as it is for the generation of a report.  There are a couple
            // areas that are not included in the base class that are instead implemented on method overrides,
            // since they are not applicable to exporting.  These areas are commented accordingly in
            // the new method, 'CloneSourceDataPresenter'
            //
            #region Refactored

            //// JJD 10/02/08
            //// Access all the records so that we force all FieldLayouts to be allocated
            //// ahead of time
            //this._sourceDataPresenter.RecordManager.AccessAllRecords(true);

            //this._bypassInitializeRecordEvent = true;

            //this._reportVersion++;

            //// JD 9/30/08
            //// We need to create a container for the section so we can use it as a 
            //// convenient plave to prevent routed events from bubbling up to
            //// the ui datapresenter
            //if (this._sectionContainer == null )
            //{
            //    this._sectionContainer = new SectionContainer();
            //    this._sectionContainer.Section = section;
            //    // Make the section a logical child of the source UI datapresenter
            //    this._sourceDataPresenter.InternalAddLogicalChild(this._sectionContainer);
            //}
            //else
            //    this._sectionContainer.Section = section;

            //this.DataSource = null;
            //this.FieldLayouts.Clear();

            ////Initialize the view
            //this.CurrentViewInternal = ReportViewBase.CloneView( this._sourceDataPresenter );

            //// hide the groupbyarea
            //this.GroupByAreaLocation = GroupByAreaLocation.None;

            //// set the cell generation mode to LazyLoad to allow use of VirtualizingDataRecordCellPanel
            //if ( this.CellContainerGenerationMode == CellContainerGenerationMode.PreLoad )
            //    this.CellContainerGenerationMode = CellContainerGenerationMode.LazyLoad;

            //#region Copy the settings
 
            //ReportViewBase view = this.CurrentViewInternal as ReportViewBase;

            //if (this._sourceDataPresenter.ReportView != null)
            //    if (string.IsNullOrEmpty(this._sourceDataPresenter.ReportView.Theme) == false)
            //        this.Theme = this._sourceDataPresenter.ReportView.Theme;

            
            //#region Exclude DP's FieldLayoutSettings
            //this.FieldLayoutSettings = null;
            //if (!view.ExcludeFieldLayoutSettings)
            //{
            //    // JJD 2/9/09 - TFS13678
            //    // Pass true as 2nd param to copy over those properties that are marked 
            //    // DesignerSerializationVisibility.Hidden
            //    //this.FieldLayoutSettings = CloneHelper(this._sourceDataPresenter.FieldLayoutSettings) as FieldLayoutSettings;
            //    this.FieldLayoutSettings = CloneHelper(this._sourceDataPresenter.FieldLayoutSettings, true) as FieldLayoutSettings;

            //    // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //    this.FieldLayoutSettings.ResetExcludedSettings(view);
            //}

            //this.FieldLayoutSettings.AutoGenerateFields = this._sourceDataPresenter.FieldLayoutSettings.AutoGenerateFields;
            //#endregion

            //#region Exclude DP's FieldLayout summary
            //if (view.ExcludeSummaries)
            //{
            //    this.FieldLayoutSettings.SummaryDescriptionVisibility = Visibility.Collapsed;
            //}
            //#endregion

            //#region Exclude DP's field settings
            //this.FieldSettings = null;
            //if (!view.ExcludeFieldSettings)
            //{
            //    // JJD 2/9/09 - TFS13678
            //    // Pass true as 2nd param to copy over those properties that are marked 
            //    // DesignerSerializationVisibility.Hidden
            //    //this.FieldSettings = CloneHelper(this._sourceDataPresenter.FieldSettings) as FieldSettings;
            //    this.FieldSettings = CloneHelper(this._sourceDataPresenter.FieldSettings, true) as FieldSettings;

            //    /* AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //    if (view.ExcludeEditorSettings)
            //    {
            //        this.FieldSettings.EditorType = typeof(XamTextEditor);
            //        this.FieldSettings.EditorStyle = null;
            //        this.FieldSettings.EditorStyleSelector = null; 
            //    }
            //    if (view.ExcludeCellValuePresenterStyles)
            //    {
            //        this.FieldSettings.CellValuePresenterStyle = null;
            //        this.FieldSettings.CellValuePresenterStyleSelector = null;
            //    }
            //    if (view.ExcludeLabelPresenterStyles)
            //    {
            //        this.FieldSettings.LabelPresenterStyle = null;
            //        this.FieldSettings.LabelPresenterStyleSelector = null;
            //    }
            //    if (view.ExcludeSummaries)
            //    {
            //        this.FieldSettings.AllowSummaries = false;
            //        this.FieldSettings.SummaryUIType =  SummaryUIType.Default;
            //        this.FieldSettings.SummaryDisplayArea = SummaryDisplayAreas.None;
            //    }
            //    */
            //    this.FieldSettings.ResetExcludedSettings(view);
            //}
            //#endregion

            //this.SortRecordsByDataType = this._sourceDataPresenter.SortRecordsByDataType;

            //#region Copy DP's FieldLayout objects

            //this._associatedFieldLayouts = new Dictionary<FieldLayout, FieldLayout>();

            //// AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //this._associatedFields = new Dictionary<Field, Field>();

            //foreach (FieldLayout fl in this._sourceDataPresenter.FieldLayouts)
            //{
            //    // JM 08-14-08 - In case the source data presenter has not been shown yet, make sure the
            //    // FieldLayout is initialized.
            //    if (fl.StyleGenerator == null)
            //        fl.Initialize(this.SourceDataPresenter.CurrentViewInternal.GetFieldLayoutTemplateGenerator(fl));

            //    // JJD 2/9/09 - TFS13678
            //    // Pass false as 2nd param so we don't copy over those properties that are marked 
            //    // DesignerSerializationVisibility.Hidden
            //    //FieldLayout clonedFieldLayout = CloneHelper(fl) as FieldLayout;
            //    FieldLayout clonedFieldLayout = CloneHelper(fl, false) as FieldLayout;

            //    this.FieldLayouts.Add(clonedFieldLayout);

            //    // JJD 10/02/08
            //    // maintain a dictionary of associated field layouts so we can get cloned fieldlayout
            //    // from its associated ui fieldlayout
            //    this._associatedFieldLayouts.Add(fl, clonedFieldLayout);

            //    #region Exclude FieldLayout's settings
            //    // Exclude FieldLayout's settings
            //    clonedFieldLayout.Settings = null;
            //    if ((fl.HasSettings)&&(!view.ExcludeFieldLayoutSettings))
            //    {
            //        // JJD 2/9/09 - TFS13678
            //        // Pass true as 2nd param to copy over those properties that are marked 
            //        // DesignerSerializationVisibility.Hidden
            //        //clonedFieldLayout.Settings = CloneHelper(fl.Settings) as FieldLayoutSettings;
            //        clonedFieldLayout.Settings = CloneHelper(fl.Settings, true) as FieldLayoutSettings;
 
            //        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //        clonedFieldLayout.Settings.ResetExcludedSettings(view);
            //    }
            //    #endregion

            //    #region Exclude FieldLayout's FieldSettings
            //    // Exclude FieldLayout's FieldSettings
            //    clonedFieldLayout.FieldSettings = null;
            //    if (!view.ExcludeFieldSettings)
            //    {
            //        // JJD 2/9/09 - TFS13678
            //        // Pass true as 2nd param to copy over those properties that are marked 
            //        // DesignerSerializationVisibility.Hidden
            //        //clonedFieldLayout.FieldSettings = CloneHelper(fl.FieldSettings) as FieldSettings;
            //        clonedFieldLayout.FieldSettings = CloneHelper(fl.FieldSettings, true) as FieldSettings;

            //        /* AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //        if (view.ExcludeCellValuePresenterStyles)
            //        {
            //            clonedFieldLayout.FieldSettings.CellValuePresenterStyle = null;
            //            clonedFieldLayout.FieldSettings.CellValuePresenterStyleSelector = null;
            //        }
            //        if (view.ExcludeLabelPresenterStyles)
            //        {
            //            clonedFieldLayout.FieldSettings.LabelPresenterStyle = null;
            //            clonedFieldLayout.FieldSettings.LabelPresenterStyleSelector = null;
            //        }
            //        if (view.ExcludeEditorSettings)
            //        {
            //            clonedFieldLayout.FieldSettings.EditorType = typeof(XamTextEditor);
            //            clonedFieldLayout.FieldSettings.EditorStyle = null;
            //            clonedFieldLayout.FieldSettings.EditorStyleSelector = null;
            //        }
            //        if (view.ExcludeSummaries)
            //        {
            //            clonedFieldLayout.FieldSettings.AllowSummaries = false;
            //            clonedFieldLayout.FieldSettings.SummaryDisplayArea = SummaryDisplayAreas.None;
            //            clonedFieldLayout.FieldSettings.SummaryUIType = SummaryUIType.Default;
            //        }
            //        */
            //        clonedFieldLayout.FieldSettings.ResetExcludedSettings(view);
            //    }
            //    #endregion

            //    #region Exclude FieldLayout's SummaryDefinitions
            //    if (!view.ExcludeSummaries)
            //    {
            //        foreach (SummaryDefinition def in fl.SummaryDefinitions)
            //        {
            //            // JJD 2/9/09 - TFS13678
            //            // Pass false as 2nd param so we don't copy over those properties that are marked 
            //            // DesignerSerializationVisibility.Hidden
            //            //SummaryDefinition clonedDef = CloneHelper(def) as SummaryDefinition;
            //            SummaryDefinition clonedDef = CloneHelper(def, false) as SummaryDefinition;
            //            clonedFieldLayout.SummaryDefinitions.Add(clonedDef);
            //        }
            //    }
            //    else
            //    {
            //        clonedFieldLayout.Settings.SummaryDescriptionVisibility = Visibility.Collapsed;
            //        clonedFieldLayout.SummaryDefinitions.Clear();
            //    }
            //    #endregion

            //    #region Copy Fields
            //    clonedFieldLayout.Fields.Clear();

            //    bool cloneFieldSettings = !view.ExcludeFieldSettings;

            //    foreach (Field field in fl.Fields)
            //    {
            //        Field clonedField = field.Clone(cloneFieldSettings);

            //        #region Exclude Field's settings
                    
            //        // JJD 2/9/09 - TFS13678
            //        // If we are excluding field settings then we don't need to go
            //        // into the if block at all
            //        //if (clonedField.HasSettings)
            //        if (cloneFieldSettings == true && clonedField.HasSettings)
            //        {
            //            // JJD 2/9/09 - TFS13678
            //            // We do't need to clone the settings separately since that would have
            //            // already been done in the Field.Clone call above
            //            //clonedField.Settings = CloneHelper(field.Settings) as FieldSettings;

            //            /* AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //            if (view.ExcludeCellValuePresenterStyles)
            //            {
            //                clonedField.Settings.CellValuePresenterStyle = null;
            //                clonedField.Settings.CellValuePresenterStyleSelector = null;
            //            }
            //            if (view.ExcludeLabelPresenterStyles)
            //            {
            //                clonedField.Settings.LabelPresenterStyle = null;
            //                clonedField.Settings.LabelPresenterStyleSelector = null;
            //            }
            //            if (view.ExcludeEditorSettings)
            //            {
            //                clonedField.Settings.EditorType = typeof(XamTextEditor);
            //                clonedField.Settings.EditorStyle = null;
            //                clonedField.Settings.EditorStyleSelector = null;
            //            }
            //            if (view.ExcludeSummaries)
            //            {
            //                clonedField.Settings.AllowSummaries = false;
            //                clonedField.Settings.SummaryDisplayArea = SummaryDisplayAreas.None;  
            //                clonedField.Settings.SummaryUIType = SummaryUIType.Default;
            //            }
            //            */
            //            clonedField.Settings.ResetExcludedSettings(view);
            //        }
            //        #endregion

            //        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //        this._associatedFields.Add(field, clonedField);

            //        clonedFieldLayout.Fields.Add(clonedField);
            //    }

            //    // JJD 9/29/08
            //    // Clone the DragFieldLayoutInfo in case the user moved the fields around
            //    fl.CloneDragFieldLayoutInfo(clonedFieldLayout);

            //    #endregion

            //    #region Exclude FieldLayout's GroupBySettings and SortOrder

            //    if (fl.SortedFields.Count > 0)
            //    {
            //        // JJD 1/29/09
            //        // Since we will be making multiple updates call BeginUpdate
            //        clonedFieldLayout.SortedFields.BeginUpdate();

            //        clonedFieldLayout.SortedFields.Clear();

            //        try
            //        {
            //            foreach (FieldSortDescription fsd in fl.SortedFields)
            //            {
            //                // this is group by field
            //                if (fsd.IsGroupBy)
            //                {
            //                    if (!view.ExcludeGroupBySettings)
            //                    {
            //                        FieldSortDescription clonedFsd = new FieldSortDescription();
            //                        clonedFsd.Direction = fsd.Direction;
            //                        clonedFsd.FieldName = fsd.FieldName;
            //                        clonedFsd.IsGroupBy = fsd.IsGroupBy;

            //                        clonedFieldLayout.SortedFields.Add(clonedFsd);
            //                    }
            //                }
            //                // exclude sort order
            //                else
            //                {
            //                    if (!view.ExcludeSortOrder)
            //                    {
            //                        FieldSortDescription clonedFsd = new FieldSortDescription();
            //                        clonedFsd.Direction = fsd.Direction;
            //                        clonedFsd.FieldName = fsd.FieldName;
            //                        clonedFsd.IsGroupBy = fsd.IsGroupBy;

            //                        clonedFieldLayout.SortedFields.Add(clonedFsd);
            //                    }

            //                }
            //            }
            //        }
            //        finally
            //        {
            //            // JJD 1/29/09
            //            // Call EndUpdate since we called BeginUpdate above
            //            clonedFieldLayout.SortedFields.EndUpdate();
            //        }
            //    }
            //    #endregion

            //    // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
            //    #region Exclude FieldLayout's RecordFilters
            //    if (view.ExcludeRecordFilters)
            //    {
            //        clonedFieldLayout.RecordFilters.Clear();
            //    }
            //    #endregion

            //}
            //#endregion
            
            //// fix BR35366
            //this.AutoFit = this._sourceDataPresenter.AutoFit;

            //#endregion //Copy the settings	

            //// apply data grid style
            //this.Style = DataPresenterReportControlStyle;
            //this.InvalidateMeasure();
            //this.UpdateLayout();

            //// Use DataSourceInternal which supports both DataSource and DataItems.
            //this.DataSource = this._sourceDataPresenter.DataSourceInternal as IEnumerable;

            //this.CreateFlattenedListOfData();

            //// AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //// The layoutmanagers may have been synchronized when the records were cloned.
            ////
            //foreach (FieldLayout fl in this.FieldLayouts)
            //    fl.BumpLayoutManagerVersion();

            //Debug.Assert( this.CurrentPanel != null);

            //((IEmbeddedVisualPaginator)this.CurrentPanel).BeginPagination(section);

            //// set the bypass fkag to false since we are done with the records
            //this._bypassInitializeRecordEvent = false;

            #endregion //Refactored
            //
            // MBS 8/25/09 - NA9.2 Excel Exporting
            // Initialize the view here instead of within the CloneSourceDataPresenter method      
            //
            //this.CloneSourceDataPresenter(section, null);
            this.CurrentViewInternal = ReportViewBase.CloneView(this.SourceDataPresenter);

			// AS 3/3/11 NA 2011.1 - Async Exporting
			// We used to call the AccessAllRecords on the RecordManager within the 
			// CloneSourceDataPresenter method. Now we will handle this separately 
			// because when exporting asynchronously we will want to break this up
			// into multiple passes.
			//
			this.SourceDataPresenter.ExportHelper.AccessAllRecords();

            this.CloneSourceDataPresenter(section, this.CurrentViewInternal as IExportOptions);
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        object IEmbeddedVisualPaginator.CurrentPageDataContext
        {
            get { return ((IEmbeddedVisualPaginator)this.CurrentPanel).CurrentPageDataContext; }
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        int IEmbeddedVisualPaginator.EstimatedPageCount
        {
            get { return ((IEmbeddedVisualPaginator)this.CurrentPanel).EstimatedPageCount; }
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        int IEmbeddedVisualPaginator.LogicalPageNumber
        {
            get { return ((IEmbeddedVisualPaginator)this.CurrentPanel).LogicalPageNumber; }
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        int IEmbeddedVisualPaginator.LogicalPagePartNumber
        {
            get { return ((IEmbeddedVisualPaginator)this.CurrentPanel).LogicalPagePartNumber; }
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        PagePosition IEmbeddedVisualPaginator.CurrentPagePosition
        {
            get { return ((IEmbeddedVisualPaginator)this.CurrentPanel).CurrentPagePosition; }
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        void IEmbeddedVisualPaginator.EndPagination()
        {
            ((IEmbeddedVisualPaginator)this.CurrentPanel).EndPagination();

            // JJD 9/30/08
            if (this._sectionContainer != null)
            {
                // null out the Section property of the container which will
                // remove it from the logical tree
                this._sectionContainer.Section = null;

                // MBS 7/24/09 - NA9.2 Excel Exporting
                //this._sourceDataPresenter.InternalRemoveLogicalChild(this._sectionContainer);
                this.SourceDataPresenter.InternalRemoveLogicalChild(this._sectionContainer);

                this._sectionContainer = null;
            }

            // MBS 7/24/09 - NA9.2 Excel Exporting
            // Refactored into a ClearResources method
            //
            #region Refactored
            //// clear the field layout map
            //this._associatedFieldLayouts = null;

            //// AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            //this._associatedFields = null;
            #endregion //Refactored
            //
            this.ClearResources();
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        /// <returns>if true the move is done; false the end of printing is reached</returns>
        bool IEmbeddedVisualPaginator.MoveToNextPage()
        {
            this.ScrollVersion++;
            return ((IEmbeddedVisualPaginator)this.CurrentPanel).MoveToNextPage();
        }

        /// <summary>
        /// Returns TabularReportViewPanel implementation 
        /// </summary>
        /// <param name="pagePosition">position to where we want the panel to be moved</param>
        /// <returns>if true everything is OK; false invalid position</returns>
        bool IEmbeddedVisualPaginator.MoveToPosition(PagePosition pagePosition)
        {
            this.ScrollVersion++;
            return ((IEmbeddedVisualPaginator)this.CurrentPanel).MoveToPosition(pagePosition);
        }

        #endregion

        internal class SectionContainer : FrameworkElement
        {
            #region Private members

			// AS 8/25/11 TFS82921
			// Changed from ReportSection to DP. There was nothing ReportSection specific and we 
			// need the same thing for the export control.
			//
            private DependencyObject _section;

            #endregion //Private members	
    
            #region Constructors

            static SectionContainer()
            {
                // get all the events registered by DataPresenterBase
                RoutedEvent[] events = EventManager.GetRoutedEventsForOwner(typeof(DataPresenterBase));

                // wire them all up so we can prevent them from bubbling up
                foreach (RoutedEvent evt in events)
                    EventManager.RegisterClassHandler(typeof(SectionContainer), evt, new RoutedEventHandler(OnRoutedEvent));

                // access a static property off the ValueEditor to make sure events are registered
                RoutedEvent re = ValueEditor.EditModeEndedEvent;

                // get all the events registered by ValueEditor
                events = EventManager.GetRoutedEventsForOwner(typeof(ValueEditor));

                Debug.Assert(events != null && events.Length > 0, "No ValueEditor events are registered");

                // wire them all up so we can prevent them from bubbling up
                if (events != null)
                {
                    foreach (RoutedEvent evt in events)
                        EventManager.RegisterClassHandler(typeof(SectionContainer), evt, new RoutedEventHandler(OnRoutedEvent));
                }
            }

            internal SectionContainer()
            {
            }

            #endregion //Constructors	
    
            #region Event handlers

            static private void OnRoutedEvent(object sender, RoutedEventArgs e)
            {
                // set handled to true to prevent the event from bubbling up to the ui datapresenter
                e.Handled = true;
            }

            #endregion //Event handlers	
    
            #region Base class overrides

                #region LogicalChildren

            protected override IEnumerator LogicalChildren
            {
                get
                {
                    return new SingleItemEnumerator(this._section);
                }
            }

                #endregion //LogicalChildren	
  
            #endregion //Base class overrides	
    
            #region Properties

            internal DependencyObject Section
            {
                get { return this._section; }
                set
                {
                    if (value != this._section)
                    {
						// AS 8/25/11 TFS82921
						// Added the GetParent check. We may have removed it based on Mike's code below.
						//
                        if (this._section != null && LogicalTreeHelper.GetParent(_section) == this)
                            this.RemoveLogicalChild(this._section);

                        this._section = value;

						if (this._section != null)
						{
							// MD 6/29/11 - TFS73145
							// If there was an error with the first print attempt and a second one is being made, another SectionContainer
							// may own the ReportSection, so remove it from that parent before adding it to this one.
							SectionContainer otherSection = LogicalTreeHelper.GetParent(value) as SectionContainer;
							if (otherSection != null)
								otherSection.RemoveLogicalChild(value);

							this.AddLogicalChild(this._section);
						}
                    }
                }
            }

            #endregion //Properties	
    
        }
    }
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