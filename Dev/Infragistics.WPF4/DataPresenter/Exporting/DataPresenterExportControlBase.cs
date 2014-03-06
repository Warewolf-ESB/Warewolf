using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Reporting;
using System.Windows;
using System.Diagnostics;
using System.Collections;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    // MBS 7/20/09 - NA9.2 Excel Exporting
    // Refactored logic from the DataPresenterReportControl that could be reused for exporting.
    internal abstract class DataPresenterExportControlBase : DataPresenterBase
    {
        #region Private Members

        private Dictionary<FieldLayout, FieldLayout> _associatedFieldLayouts;
        private bool _bypassInitializeRecordEvent;
        private int _reportVersion;
        private DataPresenterBase _sourceDataPresenter;

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        // Rather than rely on the name of the field (which has perf implications and 
        // also is unreliable since a name is optional at least for unbound fields) 
        // we will maintain a mapping similar to what we did for field layouts.
        //
        private Dictionary<Field, Field> _associatedFields;

        #endregion //Private Members

        #region Constructor

        internal DataPresenterExportControlBase(DataPresenterBase sourceDataPresenter)
            : base()
        {
            this._sourceDataPresenter = sourceDataPresenter;
        }

        #endregion //Constructor

        #region Properties

        #region BypassInitializeRecordEvent

        internal bool BypassInitializeRecordEvent
        {
            get { return this._bypassInitializeRecordEvent; }
            set { this._bypassInitializeRecordEvent = value; }
        }
        #endregion //BypassInitializeRecordEvent

        #region ReportVersion

        internal int ReportVersion { get { return this._reportVersion; } }

        #endregion //ReportVersion

        #region SourceDataPresenter

        internal DataPresenterBase SourceDataPresenter { get { return this._sourceDataPresenter; } }

        #endregion //SourceDataPresenter

        #endregion //Properties

        #region Base Class Overrides

        #region IsExportControl

        internal override bool IsExportControl
        {
            get { return true; }
        }
        #endregion //IsExportControl

        #region IsSynchronousControl

        internal override bool IsSynchronousControl
        {
            get { return true; }
        }
        #endregion //IsSynchronousControl

        #region OnInitializeRecord

        protected internal override void OnInitializeRecord(InitializeRecordEventArgs e)
        {
            // JJD 9/30/08
            // When we are in a report we want to bypass raising the InitializeRecord event
            // until after the state has been copied over from the associated record
            if (this._bypassInitializeRecordEvent)
                return;

            base.OnInitializeRecord(e);
        }
        #endregion //OnInitializeRecord

		#region RaiseInitializeRecord
		// AS 2/11/11 NA 2011.1 Word Writer
		// Optimization. Avoid allocating the event args for the InitializeRecord when we are 
		// by passing.
		//
		// JJD 11/17/11 - TFS78651 
		// Added sortValueChanged
		//internal override void RaiseInitializeRecord(Record record, bool reInitialize)
		internal override void RaiseInitializeRecord( Record record, bool reInitialize, bool sortvalueChanged )
		{
			if (_bypassInitializeRecordEvent)
				return;

			// JJD 11/17/11 - TFS78651 
			// Always pass false into the base implementation for sortValueChanged on an export
			base.RaiseInitializeRecord(record, reInitialize, false);
		} 
		#endregion //RaiseInitializeRecord

        #endregion //Base Class Overrides

        #region Internal Methods

        #region BindToDataSource

        internal void BindToDataSource()
        {
            try
            {
                // Since we prevent the user from assigning the DataSource on the cloned DataPresenter,
                // we need to prevent an exception from being thrown when we assign the value.
                this.SuppressExportingDataSourceException = true;

                // Use DataSourceInternal which supports both DataSource and DataItems.
                //
                this.DataSource = this._sourceDataPresenter.DataSourceInternal as IEnumerable;
            }
            finally
            {
                this.SuppressExportingDataSourceException = false;
            }
        }
        #endregion //BindToDataSource

        #region ClearResources

        internal void ClearResources()
        {
            // clear the field layout map
            this._associatedFieldLayouts = null;

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            this._associatedFields = null;
        }
        #endregion //ClearResources

        #region CloneHelper
        
#region Infragistics Source Cleanup (Region)
































































































#endregion // Infragistics Source Cleanup (Region)

        // JJD 2/9/09 - TFS13678
        // Added copyNonSerializableProperties
        //internal static object CloneHelper(object source)
        internal static object CloneHelper(object source, bool copyNonSerializableProperties)
        {
            //return new CloneManager().Clone(source);
			// AS 3/18/11 TFS35776
            //object clone = new CloneManager().Clone(source);
            object clone = new ExportCloneManager().Clone(source);

            // JJD 2/9/09 - TFS13678
            // Copy over those properties that are marked DesignerSerializationVisibility.Hidden
            // since the CloneHelper method will miss those
            if (copyNonSerializableProperties)
                GridUtilities.CopyNonSerializableSettings(clone, source);

            return clone;
        }
        #endregion //CloneHelper

        #region CloneSourceDataPresenter

		internal void CloneSourceDataPresenter(ReportSection section, IExportOptions options)
        {
			// AS 3/3/11 NA 2011.1 - Async Exporting
			// Do this separately so we can break this up for the async case.
			//
			//// JJD 10/02/08
			//// Access all the records so that we force all FieldLayouts to be allocated
			//// ahead of time
			//this._sourceDataPresenter.RecordManager.AccessAllRecords(true);

            this._bypassInitializeRecordEvent = true;

            // MBS 7/20/09 - NA9.2 Excel Exporting
            // Moved to the OnBeginClone override on the DataPresenterReportControl
            //
            #region Moved

            //this._reportVersion++;

            //// JD 9/30/08
            //// We need to create a container for the section so we can use it as a 
            //// convenient plave to prevent routed events from bubbling up to
            //// the ui datapresenter
            //if (this._sectionContainer == null)
            //{
            //    this._sectionContainer = new SectionContainer();
            //    this._sectionContainer.Section = section;
            //    // Make the section a logical child of the source UI datapresenter
            //    this._sourceDataPresenter.InternalAddLogicalChild(this._sectionContainer);
            //}
            //else
            //    this._sectionContainer.Section = section;

            #endregion //Moved
            //
            this.OnBeginCloneSource(section);

            // MBS 8/14/09 - NA9.2 Excel Exporting
            IDataPresenterExporter exporter = null;
            DataPresenterExportControl exportControl = this as DataPresenterExportControl;
            if(exportControl != null)
                exporter = exportControl.Exporter;

            this.DataSource = null;
            this.FieldLayouts.Clear();

            // MBS 8/25/09 - NA9.2 Excel Exporting
            // In the interest of readability and consistency, moved the logic to create the view into the derived
            // ReportControl and then pass it into this method, since it shouldn't make a difference that we clone it earlier
            //
            #region Moved
            ////Initialize the view
            //this.CurrentViewInternal = ReportViewBase.CloneView(this._sourceDataPresenter);
            //
            //// MBS 7/20/09 - NA9.2 Excel Exporting
            //if (options == null)
            //{
            //    Debug.Assert(this is DataPresenterExportControl, "We should have provided an IExportOptions for an export control");
            //    options = this.CurrentViewInternal as IExportOptions;
            //    Debug.Assert(options != null, "The current view does not implement IExportOptions");
            //}
            #endregion //Moved
            Debug.Assert(options != null, "Expected to have an IExportOptions instance");

            // hide the groupbyarea
            this.GroupByAreaLocation = GroupByAreaLocation.None;

            // set the cell generation mode to LazyLoad to allow use of VirtualizingDataRecordCellPanel
            if (this.CellContainerGenerationMode == CellContainerGenerationMode.PreLoad)
                this.CellContainerGenerationMode = CellContainerGenerationMode.LazyLoad;

            #region Copy the settings

            // MBS 7/28/09 - NA9.2 Excel Exporting
            // We are now using the options that are passed in, which in the case of the 
            // DataPresenterReportControl will still be the ReportViewBase, but that class
            // also implements IExportOptions
            //
            //ReportViewBase view = this.CurrentViewInternal as ReportViewBase;

            if (this._sourceDataPresenter.ReportView != null)
                if (string.IsNullOrEmpty(this._sourceDataPresenter.ReportView.Theme) == false)
                    this.Theme = this._sourceDataPresenter.ReportView.Theme;


            #region Exclude DP's FieldLayoutSettings
            this.FieldLayoutSettings = null;

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (!view.ExcludeFieldLayoutSettings)
            if (options == null || !options.ExcludeFieldLayoutSettings)
            {
                // JJD 2/9/09 - TFS13678
                // Pass true as 2nd param to copy over those properties that are marked 
                // DesignerSerializationVisibility.Hidden
                //this.FieldLayoutSettings = CloneHelper(this._sourceDataPresenter.FieldLayoutSettings) as FieldLayoutSettings;
                this.FieldLayoutSettings = CloneHelper(this._sourceDataPresenter.FieldLayoutSettings, true) as FieldLayoutSettings;

                // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                //
                // MBS 7/28/09 - NA9.2 Excel Exporting
                //this.FieldLayoutSettings.ResetExcludedSettings(view);
                this.FieldLayoutSettings.ResetExcludedSettings(options as ReportViewBase);
            }

            this.FieldLayoutSettings.AutoGenerateFields = this._sourceDataPresenter.FieldLayoutSettings.AutoGenerateFields;

            #endregion

            #region Exclude DP's FieldLayout summary

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (view.ExcludeSummaries)
            if (options != null && options.ExcludeSummaries)
            {
                this.FieldLayoutSettings.SummaryDescriptionVisibility = Visibility.Collapsed;
            }

            // MBS 8/14/09 - NA9.2 Excel Exporting
            if (exporter != null)
                exporter.OnObjectCloned(this._sourceDataPresenter.FieldLayoutSettings, this.FieldLayoutSettings);

            #endregion

            #region Exclude DP's field settings
            this.FieldSettings = null;

            // MBS 7/28/09 - NA9.2 Excel Exporting
            //if (!view.ExcludeFieldSettings)
            if (options == null || !options.ExcludeFieldSettings)
            {
                // JJD 2/9/09 - TFS13678
                // Pass true as 2nd param to copy over those properties that are marked 
                // DesignerSerializationVisibility.Hidden
                //this.FieldSettings = CloneHelper(this._sourceDataPresenter.FieldSettings) as FieldSettings;
                this.FieldSettings = CloneHelper(this._sourceDataPresenter.FieldSettings, true) as FieldSettings;

                
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


                // MBS 7/20/09 - NA9.2 Excel Exporting
                //this.FieldSettings.ResetExcludedSettings(view);
                this.FieldSettings.ResetExcludedSettings(options);
            }

            // MBS 8/14/09 - NA9.2 Excel Exporting
            if (exporter != null)
                exporter.OnObjectCloned(this._sourceDataPresenter.FieldSettings, this.FieldSettings);

            #endregion

            this.SortRecordsByDataType = this._sourceDataPresenter.SortRecordsByDataType;

            #region Copy DP's FieldLayout objects

            this._associatedFieldLayouts = new Dictionary<FieldLayout, FieldLayout>();

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            this._associatedFields = new Dictionary<Field, Field>();

            foreach (FieldLayout fl in this._sourceDataPresenter.FieldLayouts)
            {
                // JM 08-14-08 - In case the source data presenter has not been shown yet, make sure the
                // FieldLayout is initialized.
                //
                // MBS 7/30/09 - NA9.2 Excel Exporting
                // Refactored into a new method
                //
                //if (fl.StyleGenerator == null)
                //    fl.Initialize(this.SourceDataPresenter.CurrentViewInternal.GetFieldLayoutTemplateGenerator(fl));
                Debug.Assert(fl.DataPresenter == this.SourceDataPresenter, "Expected the FielLayout's DP to be the same as the SourceDataPresenter");
                fl.EnsureStyleGeneratorInitialized();

                // JJD 2/9/09 - TFS13678
                // Pass false as 2nd param so we don't copy over those properties that are marked 
                // DesignerSerializationVisibility.Hidden
                //FieldLayout clonedFieldLayout = CloneHelper(fl) as FieldLayout;
                FieldLayout clonedFieldLayout = CloneHelper(fl, false) as FieldLayout;

                this.FieldLayouts.Add(clonedFieldLayout);

                // JJD 10/02/08
                // maintain a dictionary of associated field layouts so we can get cloned fieldlayout
                // from its associated ui fieldlayout
                this._associatedFieldLayouts.Add(fl, clonedFieldLayout);

				// AS 2/11/11 NA 2011.1 Word Writer
				// The rest of this loop has some issues. First, the CloneHelper above would have done a recursive 
				// clone so it would already have copied things like Settings and FieldSettings so to null that out 
				// clone it again doesn't make sense. That being said the CloneHelper call we would have made would 
				// call the CopyNonSerializableSettings method that Joe added for TFS13678 so we still want to call 
				// that. Also, the part that relates to the field's is worst because not only are we cloning the 
				// fields again (something done already by the CloneHelper call for cloning the FieldLayout) but 
				// because instead of using the CloneHelper as we do above, we call our own Clone method on the 
				// Field we only copy a hard coded set of properties and don't conside attached properties. We 
				// still want to call the InitializeFrom method that that Clone method would have called though 
				// to copy over private state information. So that summarizes the changes I'm making below.
				//

				#region Exclude FieldLayout's settings
				// AS 2/11/11 NA 2011.1 Word Writer - See above
				//// Exclude FieldLayout's settings
                //clonedFieldLayout.Settings = null;

                // MBS 7/20/09 - NA9.2 Excel Exporting
                //if ((fl.HasSettings) && (!view.ExcludeFieldLayoutSettings))
				if ((fl.HasSettings) && (options == null || !options.ExcludeFieldLayoutSettings))
				{
					// JJD 2/9/09 - TFS13678
					// Pass true as 2nd param to copy over those properties that are marked 
					// DesignerSerializationVisibility.Hidden
					//clonedFieldLayout.Settings = CloneHelper(fl.Settings) as FieldLayoutSettings;
					// AS 2/11/11 NA 2011.1 Word Writer - See above
					// We only need to copy the non-serializable settings since the others 
					// would have been handled by the clone call above.
					//
					//clonedFieldLayout.Settings = CloneHelper(fl.Settings, true) as FieldLayoutSettings;
					GridUtilities.CopyNonSerializableSettings(clonedFieldLayout.Settings, fl.Settings);

					// AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
					//
					// MBS 7/20/09 - NA9.2 Excel Exporting
					//clonedFieldLayout.Settings.ResetExcludedSettings(view);
					clonedFieldLayout.Settings.ResetExcludedSettings(options as ReportViewBase);
				}
				else
				{
					// AS 2/11/11 NA 2011.1 Word Writer - See above
					// Since the clone may have copied it, clear it now.
					//
					clonedFieldLayout.Settings = null;
				}

                // MBS 8/14/09 - NA9.2 Excel Exporting
                if (exporter != null && clonedFieldLayout.HasSettings)
                    exporter.OnObjectCloned(fl.Settings, clonedFieldLayout.Settings);

				// MD 6/7/10 - ChildRecordsDisplayOrder feature
				// For now, exporting doesn't support setting the ChildRecordsDisplayOrder value on individual field layouts because
				// Excel only supports setting the expansion indicator position at a worksheet level.
				if (clonedFieldLayout.HasSettings)
					clonedFieldLayout.Settings.ClearValue(FieldLayoutSettings.ChildRecordsDisplayOrderProperty);

                #endregion

                #region Exclude FieldLayout's FieldSettings
				// AS 2/11/11 NA 2011.1 Word Writer - See above
				//// Exclude FieldLayout's FieldSettings
                //clonedFieldLayout.FieldSettings = null;

                // MBS 7/20/09 - NA9.2 Excel Exporting
                //if (!view.ExcludeFieldSettings)
				if (options == null || !options.ExcludeFieldSettings)
				{
					// JJD 2/9/09 - TFS13678
					// Pass true as 2nd param to copy over those properties that are marked 
					// DesignerSerializationVisibility.Hidden
					//clonedFieldLayout.FieldSettings = CloneHelper(fl.FieldSettings) as FieldSettings;
					// AS 2/11/11 NA 2011.1 Word Writer - See above
					// We only need to copy the non-serializable settings since the others 
					// would have been handled by the clone call above.
					//
					//clonedFieldLayout.FieldSettings = CloneHelper(fl.FieldSettings, true) as FieldSettings;
					GridUtilities.CopyNonSerializableSettings(clonedFieldLayout.FieldSettings, fl.FieldSettings);

					
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


					// MBS 7/20/09 - NA9.2 Excel Exporting
					//clonedFieldLayout.FieldSettings.ResetExcludedSettings(view);
					clonedFieldLayout.FieldSettings.ResetExcludedSettings(options);
				}
				else
				{
					// AS 2/11/11 NA 2011.1 Word Writer - See above
					// Since the clone may have copied it, clear it now.
					//
					clonedFieldLayout.FieldSettings = null;
				}

                #endregion

                #region Exclude FieldLayout's SummaryDefinitions

                // MBS 7/20/09 - NA9.2 Excel Exporting
                //if (!view.ExcludeSummaries)
                if (options == null || !options.ExcludeSummaries)
                {
					// AS 2/11/11 NA 2011.1 Word Writer
					// I added the DesignerSerializationVisibility attribute to the SummaryDefinitions
					// property so the collection is cloned as part of the CloneHelper call so we don't 
					// have to clone them.
					//
					//foreach (SummaryDefinition def in fl.SummaryDefinitions)
					//{
					//    // JJD 2/9/09 - TFS13678
					//    // Pass false as 2nd param so we don't copy over those properties that are marked 
					//    // DesignerSerializationVisibility.Hidden
					//    //SummaryDefinition clonedDef = CloneHelper(def) as SummaryDefinition;
					//    SummaryDefinition clonedDef = CloneHelper(def, false) as SummaryDefinition;
					//    clonedFieldLayout.SummaryDefinitions.Add(clonedDef);
					//}
                }
                else
                {
                    clonedFieldLayout.Settings.SummaryDescriptionVisibility = Visibility.Collapsed;
                    clonedFieldLayout.SummaryDefinitions.Clear();
                }

                // MBS 8/14/09 - NA9.2 Excel Exporting
                // Force the FieldSettings to be created for the clone if we have them on the FL
                // since we need to copy over the attached properties
                if (exporter != null && fl.HasFieldSettings)
                    exporter.OnObjectCloned(fl.FieldSettings, clonedFieldLayout.FieldSettings);

                #endregion

                #region Copy Fields
				// AS 2/11/11 NA 2011.1 Word Writer - See above
				// Basically the Fields were already copied and that would have 
				// included any attached properties, etc.
				//
				//clonedFieldLayout.Fields.Clear();

                // MBS 7/20/09 - NA9.2 Excel Exporting
                //bool cloneFieldSettings = !view.ExcludeFieldSettings;
                bool cloneFieldSettings = options == null || !options.ExcludeFieldSettings;

				// AS 2/11/11 NA 2011.1 Word Writer - See above
				//foreach (Field field in fl.Fields)
				Debug.Assert(fl.Fields.Count == clonedFieldLayout.Fields.Count, "Fields should have been copied by the CloneHelper that cloned the FieldLayout");
				for (int i = 0; i < fl.Fields.Count; i++)
                {
					// AS 2/11/11 NA 2011.1 Word Writer - See above
					// We don't need to create a new one since the clonehelper for the fieldlayout 
					// copied the fields. Instead we can just copy over the settings the the Field's 
					// Clone method would have done.
					//
					//Field clonedField = field.Clone(cloneFieldSettings);
					Field field = fl.Fields[i];
					Field clonedField = clonedFieldLayout.Fields[i];
					clonedField.InitializeFrom(field);

					// AS 2/11/11 NA 2011.1 Word Writer - See above
					// Since we're not using the field's Clone method and therefore the Settings 
					// would have been cloned from the Source field, we need to clear the settings 
					// if we don't want them.
					//
					// JJD 11/3/11 - TFS95149
					// Added else block to clone the non-serializable settings.
					// This handles properties like style selector that are not cloned above.
					//if (!cloneFieldSettings && clonedField.HasSettings)
					//    clonedField.Settings = null;
					if (!cloneFieldSettings)
					{
						if (clonedField.HasSettings)
							clonedField.Settings = null;
					}
					else
					{
						// JJD 11/3/11 - TFS95149
						// If the source has settings  make sure we also pick up the
						// non-serializable properties, e.g. the style selectors
						if (field.HasSettings)
							GridUtilities.CopyNonSerializableSettings(clonedField.Settings, field.Settings);
					}
					

                    #region Exclude Field's settings

                    // JJD 2/9/09 - TFS13678
                    // If we are excluding field settings then we don't need to go
                    // into the if block at all
                    //if (clonedField.HasSettings)
                    if (cloneFieldSettings == true && clonedField.HasSettings)
                    {
                        // JJD 2/9/09 - TFS13678
                        // We do't need to clone the settings separately since that would have
                        // already been done in the Field.Clone call above
                        //clonedField.Settings = CloneHelper(field.Settings) as FieldSettings;

                        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


                        // MBS 7/20/09 - NA9.2 Excel Exporting
                        //clonedField.Settings.ResetExcludedSettings(view);
                        clonedField.Settings.ResetExcludedSettings(options);
                    }
                    #endregion

                    // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
                    this._associatedFields.Add(field, clonedField);

					// AS 2/11/11 NA 2011.1 Word Writer - See above
					//clonedFieldLayout.Fields.Add(clonedField);

                    // MBS 8/14/09 - NA9.2 Excel Exporting
                    // Force the FieldSettings to be created for the clone if we have them on the FL
                    // since we need to copy over the attached properties
                    if (exporter != null && field.HasSettings)
                        exporter.OnObjectCloned(field.Settings, clonedField.Settings);
                }

                // JJD 9/29/08
                // Clone the DragFieldLayoutInfo in case the user moved the fields around
                fl.CloneDragFieldLayoutInfo(clonedFieldLayout);

                #endregion

                #region Exclude FieldLayout's GroupBySettings and SortOrder

                if (fl.SortedFields.Count > 0)
                {
                    // JJD 1/29/09
                    // Since we will be making multiple updates call BeginUpdate
                    clonedFieldLayout.SortedFields.BeginUpdate();

                    clonedFieldLayout.SortedFields.Clear();

                    try
                    {
                        foreach (FieldSortDescription fsd in fl.SortedFields)
                        {
                            // this is group by field
                            if (fsd.IsGroupBy)
                            {
                                // MBS 7/20/09 - NA9.2 Excel Exporting
                                //if (!view.ExcludeGroupBySettings)
                                if (options == null || !options.ExcludeGroupBySettings)
                                {
                                    FieldSortDescription clonedFsd = new FieldSortDescription();
                                    clonedFsd.Direction = fsd.Direction;
                                    clonedFsd.FieldName = fsd.FieldName;
                                    clonedFsd.IsGroupBy = fsd.IsGroupBy;

                                    clonedFieldLayout.SortedFields.Add(clonedFsd);
                                }
                            }
                            // exclude sort order
                            else
                            {
                                // MBS 7/20/09 - NA9.2 Excel Exporting
                                //if (!view.ExcludeSortOrder)
                                if (options == null || !options.ExcludeSortOrder)
                                {
                                    FieldSortDescription clonedFsd = new FieldSortDescription();
                                    clonedFsd.Direction = fsd.Direction;
                                    clonedFsd.FieldName = fsd.FieldName;
                                    clonedFsd.IsGroupBy = fsd.IsGroupBy;

                                    clonedFieldLayout.SortedFields.Add(clonedFsd);
                                }

                            }
                        }
                    }
                    finally
                    {
                        // JJD 1/29/09
                        // Call EndUpdate since we called BeginUpdate above
                        clonedFieldLayout.SortedFields.EndUpdate();
                    }
                }
                #endregion

                // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
                #region Exclude FieldLayout's RecordFilters

                // MBS 7/20/09 - NA9.2 Excel Exporting
                //if (view.ExcludeRecordFilters)
                if (options != null && options.ExcludeRecordFilters)
                {
                    clonedFieldLayout.RecordFilters.Clear();
                }
                #endregion

            }
            #endregion

            // fix BR35366
            this.AutoFit = this._sourceDataPresenter.AutoFit;

            #endregion //Copy the settings

            // MBS 7/20/09 - NA9.2 Excel Exporting
            // Moved to the OnCloneDataSourceRequested override on the DataPresenterReportControl
            //
            #region Moved

            //// apply data grid style
            //this.Style = DataPresenterReportControlStyle;
            //this.InvalidateMeasure();
            //this.UpdateLayout();

            //// Use DataSourceInternal which supports both DataSource and DataItems.
            //this.DataSource = this._sourceDataPresenter.DataSourceInternal as IEnumerable;

            //this.CreateFlattenedListOfData();

            #endregion //Moved
            //
            this.OnCloneDataSourceRequested();

            // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
            // The layoutmanagers may have been synchronized when the records were cloned.
            //
            foreach (FieldLayout fl in this.FieldLayouts)
                fl.BumpLayoutManagerVersion();

            // MBS 7/20/09 - NA9.2 Excel Exporting
            // Moved to the OnCloneDataSourceRequested override on the DataPresenterReportControl
            //
            #region Moved
            //Debug.Assert(this.CurrentPanel != null);

            //((IEmbeddedVisualPaginator)this.CurrentPanel).BeginPagination(section);
            #endregion //Moved
            //
            this.OnEndCloneSource(section);

            // set the bypass fkag to false since we are done with the records
            this._bypassInitializeRecordEvent = false;
        }
        #endregion //CloneSourceDataPresenter

        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
        #region GetClonedField

        internal Field GetClonedField(Field uiLayout)
        {
            if (this._associatedFields == null)
                return null;

            Field clonedFl;

            this._associatedFields.TryGetValue(uiLayout, out clonedFl);

            return clonedFl;
        }

        #endregion //GetClonedField

        #region GetClonedFieldLayout

        internal FieldLayout GetClonedFieldLayout(FieldLayout uiLayout)
        {
            if (this._associatedFieldLayouts == null)
                return null;

            FieldLayout clonedFl;

            this._associatedFieldLayouts.TryGetValue(uiLayout, out clonedFl);

            return clonedFl;
        }

        #endregion //GetClonedFieldLayout

        #endregion //Internal Methods

        #region Protected Methods

        // MBS 7/28/09 - NA9.2 Excel Exporting
        // Refactored from the DataPresenterReportControl's CreateFlattenedListHelper method.  Most of the 
        // logic can be shared between reporting and exporting, except for when a visible record is found.
        #region TraverseVisibleRecords

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Refactor the enumeration logic into the ExportRecordEnumeratorBase class so 
		// we can break up the operation for asynchronous processing.
		//
		//protected bool TraverseVisibleRecords(ViewableRecordCollection records, RecordManager recordManager, IExportOptions options, List<Record> flattenedList)
		//{
		//    // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
		//    // Copy the filters over before we process the records so their
		//    // filter state will get updated properly
		//    if (recordManager != records.RecordManager)
		//    {
		//        recordManager = records.RecordManager;
		//
		//        if (recordManager != null)
		//        {
		//            RecordManager associatedRm = recordManager.AssociatedRecordManager;
		//
		//            if (associatedRm != null)
		//            {
		//                // Bypass the root manager and don't try to clone its filters since we never 
		//                // use the record manager to hold the filters for root records. we 
		//                // always store them on the fieldlayout
		//                //
		//                // MBS 7/28/09 - NA9.2 Excel Exporting
		//                //if (!view.ExcludeRecordFilters &&
		//                if ((options == null || !options.ExcludeRecordFilters) &&
		//                    !recordManager.IsRootManager)
		//                {
		//                    if (recordManager.RecordFilters.Count == 0)
		//                    {
		//                        if (associatedRm.RecordFiltersIfAllocated != null)
		//                            recordManager.RecordFilters.CloneFrom(associatedRm.RecordFilters);
		//                    }
		//                }
		//            }
		//
		//            ViewableRecordCollection associatedVrc = associatedRm.ViewableRecords;
		//
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
		//
		//    // JJD 10/16/08 - TFS8092
		//    // First copy the records from the ViewableRecordCollection into a stack list
		//    // so that as we walk over the list the count won't change based on 
		//    // visibility of record being set to Collapsed
		//    List<Record> recordList = new List<Record>(records);
		//
		//    // JJD 10/16/08 - TFS8092
		//    // Cache the count since it would be too expensive to get it the loop below
		//    //int count = records.Count;
		//    int count = recordList.Count;
		//
		//    // JJD 10/16/08 - TFS8092
		//    // Don't use an enumerator since raising the InitializeRecord event can result in a listener
		//    // collapsing a record which would in effect pull it out of the ViewableRecordsCollection
		//    //foreach (Record rcd in records)
		//    for (int i = 0; i < count; i++)
		//    {
		//        Record rcd = recordList[i] as Record;
		//
		//        Debug.Assert(rcd != null);
		//
		//        if (rcd == null)
		//            continue;
		//
		//        // JJD 1/20/09 - NA 2009 vol 1 - Record filtering
		//        // ignore FilterRecords
		//        if (rcd is FilterRecord)
		//            continue;
		//
		//        DataRecord dr = rcd as DataRecord;
		//
		//        // ignore add records
		//        if (dr != null && dr.IsAddRecord)
		//            continue;
		//
		//
		//        // JJD 10/17/08 - TFS8092
		//        // Don't add the record until we know if its Visibility is set to Collapsed below
		//        //flattenedList.Add(rcd);
		//
		//        Record associatedRcd = rcd.GetAssociatedRecord();
		//
		//        if (associatedRcd != null)
		//            // MBS 7/28/09 - NA9.2 Excel Exporting
		//            //rcd.CloneAssociatedRecordSettings(associatedRcd, view);
		//            rcd.CloneAssociatedRecordSettings(associatedRcd, options);
		//
		//        // set the bypass fkag to false to we can raise the InitializeRecord event below
		//        //
		//        // MBS 7/24/09 - NA9.2 Excel Exporting
		//        //this._bypassInitializeRecordEvent = false;
		//        this.BypassInitializeRecordEvent = false;
		//
		//        // Raise the InitilizeRecord event now that we have cloned all of the settings
		//        //~ SSP 3/3/09 TFS11407
		//        //~ Pass along the new reInitialize parameter.
		//        //~ 
		//        //~this.RaiseInitializeRecord(rcd);
		//        this.RaiseInitializeRecord(rcd, false);
		//
		//        // reset the flag for the next record
		//        //
		//        // MBS 7/24/09 - NA9.2 Excel Exporting
		//        //this._bypassInitializeRecordEvent = true;
		//        this.BypassInitializeRecordEvent = true;
		//
		//        // JJD 10/17/08 - TFS8092
		//        // Now that we have rasied the InitializeRecord event we can check its Visiblity 
		//        // before adding it to the flattened list
		//        Visibility visibility = rcd.VisibilityResolved;
		//
		//        // MBS 7/28/09 - NA9.2 Excel Exporting
		//        // We basically want to do the same thing for reports and exporting until we actually encounter
		//        // a record, at which point we want to take different action.  Note that we do want to pass off
		//        // all records to the exporter since the user has the option to make those records visible
		//        // through the various events.
		//        bool skipSiblings;
		//        if (!this.OnTraverseRecord(rcd, visibility, recordManager, options, flattenedList, out skipSiblings))
		//            return false;
		//        //
		//        if (skipSiblings)
		//            break;
		//    }
		//
		//    // MBS 7/28/09 - NA9.2 Excel Exporting
		//    return true;
		//}
        #endregion //TraverseVisibleRecords

        #region OnBeginCloneSource

        protected virtual void OnBeginCloneSource(ReportSection section)
        {
            this._reportVersion++;
        }
        #endregion //OnBeginCloneSource

        #region OnCloneDataSourceRequested

        protected virtual void OnCloneDataSourceRequested()
        {
        }
        #endregion //OnCloneDataSourceRequested

        #region OnEndCloneSource

        protected virtual void OnEndCloneSource(ReportSection section)
        {
        }
        #endregion //OnEndCloneSource

        #region OnTraverseVisibleRecord

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Refactor the enumeration logic into the ExportRecordEnumeratorBase class so 
		// we can break up the operation for asynchronous processing.
		//
		//protected virtual bool OnTraverseRecord(Record record, Visibility visibility, RecordManager recordManager, IExportOptions options, 
		//    List<Record> flattenedList, out bool skipSiblings)
		//{
		//    // By default don't skip any rows
		//    skipSiblings = false;
		//
		//    // Return true by default so that row traversal isn't stopped
		//    return true;
		//}
        #endregion //OnTraverseVisibleRecord

        #endregion //Protected Methods

		// AS 3/18/11 TFS35776
		#region ExportCloneManager class
		internal class ExportCloneManager : CloneManager
		{
			#region Clone
			protected override object Clone(System.Windows.Markup.Primitives.MarkupObject mo, CloneInfo cloneInfo)
			{
				// AS 6/20/11 TFS79120
				// The normal clone behavior will just use the public ctor and copy 
				// the public get/set properties as well as any serializable collections.
				// However, the recordfilter has a ConditionGroup that is read-only but 
				// that a LogicalOperator property that was not being copied because 
				// the condition group is enumerable and it just copied the conditions.
				// Even that could be an issue because it wouldn't do a deep clone of 
				// the condition.
				//
				RecordFilter sourceFilter = mo.Instance as RecordFilter;

				if (null != sourceFilter && null != cloneInfo)
				{
					object clonedObject;

					if (!cloneInfo.TryGetClonedObject(sourceFilter, out clonedObject))
					{
						clonedObject = sourceFilter.Clone(true, null, false);
						cloneInfo.AddClone(sourceFilter, clonedObject);
					}

					return clonedObject;
				}


				return base.Clone(mo, cloneInfo);
			} 
			#endregion //Clone

			#region GetCloneBehavior
			protected override CloneBehavior GetCloneBehavior(object source)
			{
				if (source is RecordFilter)
					return CloneBehavior.CloneObject;

				return base.GetCloneBehavior(source);
			} 
			#endregion //GetCloneBehavior
		}
		#endregion //ExportCloneManager class
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