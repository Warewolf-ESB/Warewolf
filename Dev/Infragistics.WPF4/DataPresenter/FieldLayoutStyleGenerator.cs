using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Data;
//using Infragistics.Windows.Input;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using System.Collections.Generic;
using Infragistics.Windows.DataPresenter.Internal;

namespace Infragistics.Windows.DataPresenter
{
	#region FieldLayoutTemplateGenerator abstract base class

	/// <summary>
	/// Base class for initializing a field layout's settings 
	/// </summary>
	public abstract class FieldLayoutTemplateGenerator
	{
		#region Private Members

		private bool _stylesAreDirty = true;
		private bool _separateHeader;
        // AS 2/3/09 Remove unused members
        //private DataRecordSizingMode _dataRecordSizeMode;
		private Grid _templateGrid;
		private ColumnDefinition _defaultColumnDefinition;
		private RowDefinition _defaultRowDefinition;
		private ColumnDefinition[] _cellAreaColumnDefinitions;
		private RowDefinition[] _cellAreaRowDefinitions;

        
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        //private DataTemplate _generatedRecordContentAreaTemplate;
		private DataTemplate _generatedHeaderTemplate;
		private FieldLayout _fieldLayout;
		private LabelLocation _labelLocation;
		private AutoArrangeCells _autoArrangeCells;
		private AutoArrangePrimaryFieldReservation _reservation;
        



		private int _maxGridRows;
        private int _maxGridColumns;
        // AS 2/3/09 Remove unused members
        //private CellContentAlignment _primaryFieldCellContentAlignment;

        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


        // AS 2/3/09 Remove unused members
        //// JJD 4/27/07
		//// Optimization - keep track of wher the prefix area was on the last template gen
		//private Dock? _previousPrefixAreaDock;

		private GridFieldMap			_gridFieldMap = null;

		private DataTemplate _generatedVirtualizedRecordContentAreaTemplate;

		// SSP 4/7/08 - Summaries Functionality
		// 
        private DataTemplate _generatedSummaryRecordContentAreaTemplate;
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        //private DataTemplate _generatedSummaryCellAreaTemplate;
		private DataTemplate _generatedSummaryVirtualCellAreaTemplate;

		internal const string CellAreaItemGridName = "ContentItemGrid";
		// SSP 9/24/08 TFS7268
		// Set a name on the grid that contains headers so the drag manager can find it.
		// 
		internal const string HeaderAreaItemGridName = "HeaderItemGrid";

		private bool _isGeneratingTemplates;

        // AS 2/3/09 Remove unused members
		//// AS 5/8/07 BR22676
		//private bool _canUseVirtualization = true;

        





        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        // Create custom datatemplates that are only used by the templatedatarecord.
        // These are the only templates that use a standard panel. The others use a 
        // VirtualizingDataRecordCellPanel.
        //
        private DataTemplate _generatedTemplateDataRecordContentAreaTemplate;
        private DataTemplate _generatedTemplateDataRecordHeaderAreaTemplate;

        // AS 3/3/09 Optimization
        // Maintain a separate flag so we know whether the templates should be regenerated
        // when the generation is processed.
        //
        private bool _templatesAreDirty = true;

		// AS 7/7/09 TFS19145
		// Maintain a version for when the templates are changed. When they are we want to 
		// release all the cell elements. When they are not then we can try to reuse the
		// elements in the VDRCP.
		//
		private int _version;
		private int _templateVersion;

		#endregion //Private Members	
 
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldLayoutTemplateGenerator"/> class
		/// </summary>
		/// <param name="fieldLayout">The associated <see cref="FieldLayout"/> for which the templates will be generated</param>
		protected FieldLayoutTemplateGenerator(FieldLayout fieldLayout)
		{
			this._fieldLayout = fieldLayout;
		}

		#endregion //Constructors	
    
		#region Constants

		private const int AutoArrangeMaxColumnsSupported = 4096;

		#endregion //Constants	
    
		#region Properties

			#region Public Properties

				#region DefaultCellContentAlignment

		/// <summary>
		/// Returns the default label alignment
		/// </summary>
		public virtual CellContentAlignment DefaultCellContentAlignment
		{
			get { return CellContentAlignment.LabelAboveValueStretch; } 
		}

				#endregion //DefaultCellContentAlignment
		
				#region DefaultAutoArrangeCells (abstract)

		/// <summary>
		/// Return the default flow for field's
		/// </summary>
		public abstract AutoArrangeCells DefaultAutoArrangeCells { get; }

				#endregion //DefaultAutoArrangeCells (abstract)	

                #region FieldLayout

        /// <summary>
		/// Returns the field layout
		/// </summary>
		public FieldLayout FieldLayout
		{
			get { return this._fieldLayout; }
        }

                #endregion //FieldLayout

                #region GeneratedHeaderTemplate

        /// <summary>
        /// Returns the generate template for a separate header area
        /// </summary>
        public DataTemplate GeneratedHeaderTemplate 
		{ 
			get 
			{
				// first call GenerateTemplates which will only create the styles 
				// if they are dirty
				this.GenerateTemplates();

				return this._generatedHeaderTemplate; 
			} 
		}

				#endregion //GeneratedHeaderTemplate	
		
				#region GeneratedRecordContentAreaTemplate

        /// <summary>
        /// Returns the generate style for the item
        /// </summary>
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
		[Obsolete("The 'GeneratedRecordContentAreaTemplate' is no longer created. The 'GeneratedVirtualRecordContentAreaTemplate' will be used instead.")]
        public DataTemplate GeneratedRecordContentAreaTemplate
		{ 
			get 
			{
                
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                return this.GeneratedVirtualRecordContentAreaTemplate;
			} 
		}

				#endregion //GeneratedRecordContentAreaTemplate	

				#region GeneratedVirtualRecordContentAreaTemplate

		/// <summary>
		/// Returns the generate style for the item
		/// </summary>
		public DataTemplate GeneratedVirtualRecordContentAreaTemplate
		{
			get
			{
				// first call GenerateTemplates which will only create the styles 
				// if they are dirty
				this.GenerateTemplates();

				return this._generatedVirtualizedRecordContentAreaTemplate;
			}
		}

				#endregion //GeneratedVirtualRecordContentAreaTemplate	

			#region GeneratedSummaryRecordContentAreaTemplate

		// SSP 4/7/08 - Summaries Functionality
		// 
		/// <summary>
		/// Returns the generated style for the item
		/// </summary>
		public DataTemplate GeneratedSummaryRecordContentAreaTemplate
		{
			get
			{
				// first call GenerateTemplates which will only create the styles 
				// if they are dirty
				this.GenerateTemplates( );

				return this._generatedSummaryRecordContentAreaTemplate;
			}
		}

				#endregion // GeneratedSummaryRecordContentAreaTemplate	

				#region GeneratedSummaryCellAreaTemplate

		// SSP 5/22/08 - BR33108 - Summaries Functionality
		// 
		/// <summary>
		/// Returns the generated style for the item
		/// </summary>
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
		[Obsolete("The 'GeneratedSummaryCellAreaTemplate' is no longer created. The 'GeneratedSummaryVirtualCellAreaTemplate' will be used instead.")]
		public DataTemplate GeneratedSummaryCellAreaTemplate
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                return this.GeneratedSummaryVirtualCellAreaTemplate;
			}
		}

				#endregion // GeneratedSummaryCellAreaTemplate

				#region GeneratedSummaryVirtualCellAreaTemplate

		// SSP 5/22/08 - BR33108 - Summaries Functionality
		// 
		/// <summary>
		/// Returns the generated style for the item
		/// </summary>
		public DataTemplate GeneratedSummaryVirtualCellAreaTemplate
		{
			get
			{
				// first call GenerateTemplates which will only create the styles 
				// if they are dirty
				this.GenerateTemplates( );

				return _generatedSummaryVirtualCellAreaTemplate;
			}
		}

				#endregion // GeneratedSummaryVirtualCellAreaTemplate

				// JM NA 10.1 CardView
				#region GetRecordNavigationDirectionFromCellNavigationDirection

		/// <summary>
		/// Returns the <see cref="PanelNavigationDirection"/> value that should be used to navigate to an adjacent record when the specified cell navigation direction is used and a suitable target cell cannot be found in the current record.
		/// </summary>
		/// <param name="cellNavigationDirection"></param>
		/// <returns></returns>
		public virtual PanelNavigationDirection GetRecordNavigationDirectionFromCellNavigationDirection(PanelNavigationDirection cellNavigationDirection)
		{
			return cellNavigationDirection;
		}

				#endregion //GetRecordNavigationDirectionFromCellNavigationDirection

				#region GridFieldMap

		
		
		
		
		






		internal GridFieldMap GridFieldMap
		{
			get
			{
				if (this._gridFieldMap == null)
					this._gridFieldMap = new GridFieldMap(this);

				return this._gridFieldMap;
			}
		}

				#endregion //GridFieldMap

				#region LogicalOrientation

		/// <summary>
		/// Returns the logical orientation for layouts.
		/// </summary>
		public virtual Orientation LogicalOrientation { get { return Orientation.Vertical; } }

				#endregion //LogicalOrientation

				#region HasLogicalOrientation

		/// <summary>
		/// Returns whether the generator has a logical orientation.
		/// </summary>
		public virtual bool HasLogicalOrientation { get { return false; } }

				#endregion //HasLogicalOrientation

				#region HasSeparateHeader

		/// <summary>
        /// Returns true if a separate header was generated for labels (read-only)
        /// </summary>
        public bool HasSeparateHeader
        {
            get { return this._separateHeader; }
        }

                #endregion //HasSeparateHeader	

                #region CellPresentation

        /// <summary>
        /// Returns the layout mode (read-only)
        /// </summary>
		public abstract CellPresentation CellPresentation { get; }

                #endregion //CellPresentation	

				#region PrimaryFieldDefaultCellContentAlignment

		/// <summary>
		/// Returns the default label alignment for the primary field
		/// </summary>
		public virtual CellContentAlignment PrimaryFieldDefaultCellContentAlignment
		{
            get { return this.DefaultCellContentAlignment; } 
		}

				#endregion //PrimaryFieldDefaultCellContentAlignment
    
				#region SupportsLabelHeaders (virtual)

		/// <summary>
		/// Returns true if the panel supports label headers
		/// </summary>
		public virtual bool SupportsLabelHeaders { get { return false; } }

				#endregion //Public Properties

			#endregion //Public properties

			#region Internal Properties

				// JM NA 10.1 CardView
				#region AreRecordsArrangedInRowsAndCols







		internal virtual bool AreRecordsArrangedInRowsAndCols
		{
			get { return false; }
		}

				#endregion //AreRecordsArrangedInRowsAndCols

				#region DefaultColumnDefinition

		internal ColumnDefinition DefaultColumnDefinition 
		{ 
			get 
			{
				Debug.Assert(!_isGeneratingTemplates);
				return this._defaultColumnDefinition; 
			} 
		}

				#endregion //DefaultColumnDefinition

				#region DefaultRowDefinition

		internal RowDefinition DefaultRowDefinition 
		{ 
			get 
			{
				Debug.Assert(!_isGeneratingTemplates);
				return this._defaultRowDefinition; 
			} 
		}

				#endregion //DefaultRowDefinition

				// JJD 5/1/07 - Optimization
				// Cache the sorted list of fields so we don't re-sort for every size manager
				#region FieldsSortedByColumn

        
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

				#endregion //FieldsSortedByColumn	
    		
				// JJD 5/1/07 - Optimization
				// Cache the sorted list of fields so we don't re-sort for every size manager
				#region FieldsSortedByRow

        
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

				#endregion //FieldsSortedByRow	
    
                // AS 2/27/09 TFS14730
                #region IsDirty
        internal bool IsDirty
        {
            get { return _stylesAreDirty; }
        } 
                #endregion //IsDirty

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
				#region TemplateDataRecordContentAreaTemplate

        /// <summary>
        /// Returns the generate style for the item
        /// </summary>
        internal DataTemplate TemplateDataRecordContentAreaTemplate
		{ 
			get 
			{
				// first call GenerateTemplates which will only create the styles 
				// if they are dirty
				this.GenerateTemplates();

				return this._generatedTemplateDataRecordContentAreaTemplate; 
			} 
		}

				#endregion //TemplateDataRecordContentAreaTemplate	

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
				#region TemplateDataRecordHeaderAreaTemplate

        /// <summary>
        /// Returns the generate style for the item
        /// </summary>
        internal DataTemplate TemplateDataRecordHeaderAreaTemplate
		{ 
			get 
			{
				// first call GenerateTemplates which will only create the styles 
				// if they are dirty
				this.GenerateTemplates();

				return this._generatedTemplateDataRecordHeaderAreaTemplate; 
			} 
		}

				#endregion //TemplateDataRecordHeaderAreaTemplate	

				#region IsGeneratingTemplates
		internal bool IsGeneratingTemplates
		{
			get { return this._isGeneratingTemplates; }
		} 
				#endregion //IsGeneratingTemplates

				#region TemplateGrid

		internal Grid TemplateGrid 
		{ 
			get 
			{
				Debug.Assert(!_isGeneratingTemplates);
				return this._templateGrid; 
			} 
		}

				#endregion //TemplateGrid

				// AS 7/7/09 TFS19145
				#region TemplateVersion
		/// <summary>
		/// The number of times the templates have been invalidated.
		/// </summary>
		internal int TemplateVersion
		{
			get
			{
				return _templateVersion;
			}
		} 
				#endregion //TemplateVersion

				#region UseCellPresenters 







		internal bool UseCellPresenters 
		{ 
			get 
			{
				return this._labelLocation == LabelLocation.InCells;
			} 
		}

				#endregion //Public Properties

				// AS 7/7/09 TFS19145
				#region Version
		/// <summary>
		/// The number of times the styles have been invalidated.
		/// </summary>
		internal int Version
		{
			get
			{
				return _version;
			}
		} 
				#endregion //Version

			#endregion //Internal Properties	
        
			#region Protected Properties

				#region DefaultAutoArrangePrimaryFieldReservation

		/// <summary>
		/// Returns the default AutoArrangePrimaryFieldReservation
		/// </summary>
		protected virtual AutoArrangePrimaryFieldReservation DefaultAutoArrangePrimaryFieldReservation { get { return AutoArrangePrimaryFieldReservation.ReserveFirstRow; } }

				#endregion //DefaultAutoArrangePrimaryFieldReservation

			#endregion //Protected properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region InvalidateGeneratedTemplates

		/// <summary>
		/// Sets a dirty flag so that the next time either of the generated styles are
		/// requested they will be regenerated.
		/// </summary>
		public void InvalidateGeneratedTemplates()
		{
            // AS 3/3/09 Optimization
            // Moved to a new overload so we can conditionally invalidate the templates.
            //
            this.InvalidateGeneratedTemplates(true);
		}

        // AS 3/3/09 Optimization
        // Added new overload so we can conditionally decide if the templates need
        // to be regenerated.
        //
        internal void InvalidateGeneratedTemplates(bool invalidateTemplates)
        {
            this._stylesAreDirty = true;

			// AS 7/7/09 TFS19145
			// We want to be more selective about bumping the template version.
			//
            //if (this._fieldLayout != null && this._fieldLayout.TemplateDataRecordCache != null)
            //    this._fieldLayout.TemplateVersion++;
			if (null != _fieldLayout)
				_fieldLayout.BumpTemplateVersion(invalidateTemplates);


			// AS 7/7/09 TFS19145
			// Bump the version numbers. One is used to indicate when the layout will be 
			// regenerated regardless of whether the templates will change.
			//
			_version++;

            // AS 3/3/09 Optimization
			if (invalidateTemplates)
			{
				_templatesAreDirty = true;

				// AS 7/7/09 TFS19145
				// The other version is used to know when the templates have actually changed.
				//
				_templateVersion++;
			}
        }
				#endregion //InvalidateGeneratedTemplates	
    
				#region GenerateTemplates

		/// <summary>
		/// Generates an item style (and optionally a header style) based on the specified layout
		/// </summary>
		public void GenerateTemplates()
		{

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
            //if (this._stylesAreDirty == false && this._generatedRecordContentAreaTemplate != null)
            if (this._stylesAreDirty == false && this._generatedVirtualizedRecordContentAreaTemplate != null)
                return;

			bool wasGeneratingTemplates = this._isGeneratingTemplates;

			try
			{
				this._isGeneratingTemplates = true;
				this.GenerateTemplatesImpl();

                // JJD 7/21/08 - BR34098 - Optimization
                // Let the dp know that we have generated templates so it knows whether or not
                // to queue invalidations on subsequent changes
                if (this._fieldLayout != null)
                {
                    DataPresenterBase dp = this._fieldLayout.DataPresenter;

                    if ( dp != null )
                        dp.OnFieldLayoutTemplateGeneration();
                }
			}
			finally
			{
				this._isGeneratingTemplates = wasGeneratingTemplates;

				// AS 6/22/09 NA 2009.2 Field Sizing
				// We need to wait until after the generation is done to update the templates.
				//
				if (null != _fieldLayout)
				{
					this._templateGrid = _fieldLayout.DataRecordCellAreaTemplateGridResolved;
					this._defaultColumnDefinition = _fieldLayout.DefaultColumnDefinitionResolved;
					this._defaultRowDefinition = _fieldLayout.DefaultRowDefinitionResolved;
				}

				if (null != this._fieldLayout)
					this._fieldLayout.OnTemplatesGenerated();
			}
		}

		// SSP 4/7/08 - Summaries Functionality
		// 
		private void GenerateSummaryRecordContentAreaTemplate( )
		{
			FrameworkElementFactory fefSummaryRecordContentArea = new FrameworkElementFactory( typeof( SummaryRecordContentArea ) );

			this._generatedSummaryRecordContentAreaTemplate = new DataTemplate( );
			this._generatedSummaryRecordContentAreaTemplate.VisualTree = fefSummaryRecordContentArea;

			// SSP 5/22/08 - BR33108
			// 
			_generatedSummaryVirtualCellAreaTemplate = new DataTemplate( );
			_generatedSummaryVirtualCellAreaTemplate.VisualTree = new FrameworkElementFactory( typeof( VirtualizingSummaryCellPanel ) );
			_generatedSummaryVirtualCellAreaTemplate.Seal( );
		}

		private void GenerateTemplatesImpl()
		{
            #region Setup 

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
			//// unhook any row/column events
			//this.HookGridRowColumnChanged(false);

			// JJD 4/27/07
			// Optimization - hold tha previous virtualized template in a stack variable
			// AS 5/4/07
			// This is actually adding more overhead then it would seem since the virtualizing
			// cell area now has to release its cells which is taking time.
			//
			//DataTemplate previousVirtualizedRecordContentAreaTemplate = this._generatedVirtualizedRecordContentAreaTemplate;

			
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

			bool regenerateTemplates = this._templatesAreDirty;

            if (regenerateTemplates)
            {
                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                //this._generatedRecordContentAreaTemplate = null;
                this._generatedHeaderTemplate = null;
                this._cellAreaColumnDefinitions = null;
                this._cellAreaRowDefinitions = null;
                this._generatedVirtualizedRecordContentAreaTemplate = null;
                // AS 2/3/09 Remove unused members
                //this._canUseVirtualization = true; // AS 5/8/07 BR22676

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                this._generatedTemplateDataRecordContentAreaTemplate = null;
                this._generatedTemplateDataRecordHeaderAreaTemplate = null;
            }

			if (this._fieldLayout == null || this._fieldLayout.DataPresenter == null)
				return;

            // reset the dirty flag
            this._stylesAreDirty = false;
            this._templatesAreDirty = false; // AS 3/3/09 Optimization

			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

            Field primaryField = this._fieldLayout.PrimaryField;

            #endregion //Setup 	

			#region Create the DataRecordCellArea for virtualized cells

            // AS 3/3/09 Optimization
            if (regenerateTemplates)
            {
                // datarecordcellarea to contain a VirtualizingDataRecordCellPanel
                FrameworkElementFactory fefVirtualRecordCellArea = new FrameworkElementFactory(typeof(DataRecordCellArea));

                fefVirtualRecordCellArea.SetValue(DataRecordCellArea.FieldLayoutProperty, this._fieldLayout);
                fefVirtualRecordCellArea.SetBinding(DataRecordCellArea.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._fieldLayout));

                // JM 09-26-07 BR26775
                fefVirtualRecordCellArea.SetBinding(DataRecordCellArea.ScrollableRecordCountVersionProperty, Utilities.CreateBindingObject(DataPresenterBase.ScrollableRecordCountVersionProperty, BindingMode.OneWay, this.FieldLayout.DataPresenter));

                // JM 11-13-07 BR27986
                fefVirtualRecordCellArea.SetBinding(DataRecordCellArea.SortOperationVersionProperty, Utilities.CreateBindingObject(FieldLayout.SortOperationVersionProperty, BindingMode.OneWay, this.FieldLayout));

                FrameworkElementFactory fefInnerRecordCellArea = new FrameworkElementFactory(typeof(VirtualizingDataRecordCellPanel));

                // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                // When autofitting the panel used the name CellAreaItemGridName.
                //
                fefInnerRecordCellArea.SetValue(FrameworkElement.NameProperty, FieldLayoutTemplateGenerator.CellAreaItemGridName);

                fefVirtualRecordCellArea.AppendChild(fefInnerRecordCellArea);

                this._generatedVirtualizedRecordContentAreaTemplate = new DataTemplate();
                this._generatedVirtualizedRecordContentAreaTemplate.VisualTree = fefVirtualRecordCellArea;
            }

			#endregion //Create the DataRecordCellArea for virtualized cells    

			// SSP 4/7/08 - Summaries Functionality
			// 
			#region Create the SummaryRecordContentArea template

            // AS 3/3/09 Optimization
            if (regenerateTemplates)
            {
                this.GenerateSummaryRecordContentAreaTemplate();
            }

			#endregion // Create the SummaryRecordContentArea template

			#region Create the styles and their grids

			FrameworkElementFactory fefHeaderGrid = null;
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
			//FrameworkElementFactory fefItemGrid = new FrameworkElementFactory(typeof(Grid));
            // AS 3/3/09 Optimization
            FrameworkElementFactory fefItemGrid = null;

            // AS 3/3/09 Optimization
            Debug.Assert(regenerateTemplates || this._labelLocation == this._fieldLayout.LabelLocationResolved);

            if (regenerateTemplates)
            {
				fefItemGrid = new FrameworkElementFactory(typeof(StackPanel));

				// AS 10/9/09 TFS22990
				// We need a way of knowing when a new cell/label panel is created.
				//
				fefItemGrid.SetValue(TemplateDataRecordCache.IsTemplateRecordPanelProperty, KnownBoxes.TrueBox);

                FrameworkElementFactory fefRecordCellArea = new FrameworkElementFactory(typeof(DataRecordCellArea));

                // SSP 5/22/08 - BR33108 - Summaries Functionality
                // 
                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                //FrameworkElementFactory fefItemGridForSummary = new FrameworkElementFactory( typeof( Grid ) );
                //fefItemGridForSummary.SetValue( FrameworkElement.NameProperty, FieldLayoutTemplateGenerator.CellAreaItemGridName );
                //_generatedSummaryCellAreaTemplate = new DataTemplate( );
                //_generatedSummaryCellAreaTemplate.VisualTree = fefItemGridForSummary;

                fefItemGrid.SetValue(FrameworkElement.NameProperty, FieldLayoutTemplateGenerator.CellAreaItemGridName);

                fefRecordCellArea.SetValue(DataRecordCellArea.FieldLayoutProperty, this._fieldLayout);
                fefRecordCellArea.SetBinding(DataRecordCellArea.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._fieldLayout));

                // JM 09-26-07 BR26775
                fefRecordCellArea.SetBinding(DataRecordCellArea.ScrollableRecordCountVersionProperty, Utilities.CreateBindingObject(DataPresenterBase.ScrollableRecordCountVersionProperty, BindingMode.OneWay, this.FieldLayout.DataPresenter));

                // JM 11-13-07 BR27986
                fefRecordCellArea.SetBinding(DataRecordCellArea.SortOperationVersionProperty, Utilities.CreateBindingObject(FieldLayout.SortOperationVersionProperty, BindingMode.OneWay, this.FieldLayout));

                fefRecordCellArea.AppendChild(fefItemGrid);

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                //this._generatedRecordContentAreaTemplate = new DataTemplate();
                //this._generatedRecordContentAreaTemplate.VisualTree = fefRecordCellArea;
                this._generatedTemplateDataRecordContentAreaTemplate = new DataTemplate();
                this._generatedTemplateDataRecordContentAreaTemplate.VisualTree = fefRecordCellArea;

                this._separateHeader = false;
                this._labelLocation = this._fieldLayout.LabelLocationResolved;

                if (this.SupportsLabelHeaders &&
                    this._fieldLayout.GetDefaultCellContentAlignment(null) != CellContentAlignment.ValueOnly)
                {
                    switch (this._labelLocation)
                    {
                        case LabelLocation.SeparateHeader:
                        case LabelLocation.Default:
                            {
                                FrameworkElementFactory fefHeaderLabelArea = new FrameworkElementFactory(typeof(HeaderLabelArea));

                                fefHeaderLabelArea.SetValue(HeaderLabelArea.FieldLayoutProperty, this._fieldLayout);
                                fefHeaderLabelArea.SetBinding(HeaderLabelArea.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._fieldLayout));

                                
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

                                FrameworkElementFactory fefTemplateHeaderLabelArea = new FrameworkElementFactory(typeof(HeaderLabelArea));
                                fefTemplateHeaderLabelArea.SetValue(HeaderLabelArea.FieldLayoutProperty, this._fieldLayout);
                                fefTemplateHeaderLabelArea.SetBinding(HeaderLabelArea.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._fieldLayout));

								fefHeaderGrid = new FrameworkElementFactory(typeof(StackPanel));

								// AS 10/9/09 TFS22990
								// We need a way of knowing when a new cell/label panel is created.
								//
								fefHeaderGrid.SetValue(TemplateDataRecordCache.IsTemplateRecordPanelProperty, KnownBoxes.TrueBox);

                                fefHeaderGrid.SetValue(FrameworkElement.NameProperty, HeaderAreaItemGridName);
                                fefTemplateHeaderLabelArea.AppendChild(fefHeaderGrid);
                                this._generatedTemplateDataRecordHeaderAreaTemplate = new DataTemplate();
                                this._generatedTemplateDataRecordHeaderAreaTemplate.VisualTree = fefTemplateHeaderLabelArea;

                                FrameworkElementFactory fefVirtualizingPanel = new FrameworkElementFactory(typeof(VirtualizingDataRecordCellPanel));
                                fefHeaderLabelArea.AppendChild(fefVirtualizingPanel);

                                this._generatedHeaderTemplate = new DataTemplate();
                                this._generatedHeaderTemplate.VisualTree = fefHeaderLabelArea;

                                this._separateHeader = true;
                                break;
                            }
                    }
                }
            }
			#endregion //Create the styles and their grids	

			#region Initialize record selectors

            // AS 3/3/09 Optimization
            if (regenerateTemplates)
            {
                RecordSelectorLocation selectorLocation = this._fieldLayout.RecordSelectorLocationResolved;

                Dock? prefixAreaDock = null;

                if (selectorLocation != RecordSelectorLocation.None)
                {
                    // SSP 4/9/08 - Summaries Functionality
                    // Added isSummaryTemplate parameter. Pass that along.
                    // 
                    //this.InitializePrefixArea(this._generatedRecordContentAreaTemplate, false, selectorLocation );
                    // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                    //this.InitializePrefixArea( this._generatedRecordContentAreaTemplate, false, selectorLocation, false );

                    // JJD 4/27/07
                    // Optimization - hold the dock area for the new template
                    // SSP 4/9/08 - Summaries Functionality
                    // Added isSummaryTemplate parameter. Pass that along.
                    // 
                    //prefixAreaDock = this.InitializePrefixArea(this._generatedVirtualizedRecordContentAreaTemplate, false, selectorLocation );
                    prefixAreaDock = this.InitializePrefixArea(this._generatedVirtualizedRecordContentAreaTemplate, false, selectorLocation, false);

                    // SSP 4/9/08 - Summaries Functionality
                    // 
                    if (null != _generatedSummaryRecordContentAreaTemplate)
                        this.InitializePrefixArea(_generatedSummaryRecordContentAreaTemplate, false, selectorLocation, true);

                    if (this._generatedHeaderTemplate != null)
                        // SSP 4/9/08 - Summaries Functionality
                        // Added isSummaryTemplate parameter. Pass that along.
                        // 
                        //this.InitializePrefixArea(this._generatedHeaderTemplate, true, selectorLocation );
                        this.InitializePrefixArea(this._generatedHeaderTemplate, true, selectorLocation, false);
                }
                else
                {
                    // JJD 1/19/09 - NA 2009 vol 1
                    // bind the margin to the calculated indent
                    this.BindContentSiteMargin(this._generatedVirtualizedRecordContentAreaTemplate.VisualTree, false);

                    if (this._generatedHeaderTemplate != null)
                        this.BindContentSiteMargin(this._generatedHeaderTemplate.VisualTree, true);
                }

                // JJD 4/27/07
                // Optimization - if the dock area is the same as the prvious template then there is
                // no need to use the new one so reset the member variable to the previous guy so
                // we don't blow away perfectly good existing record content areas
                // AS 5/4/07
                //if (previousVirtualizedRecordContentAreaTemplate != null &&
                //	prefixAreaDock == this._previousPrefixAreaDock)
                //	this._generatedVirtualizedRecordContentAreaTemplate = previousVirtualizedRecordContentAreaTemplate;

                // AS 2/3/09 Remove unused members
                //// JJD 4/27/07
                //// Optimization - cache the dock area so we can compare next time around
                //this._previousPrefixAreaDock = prefixAreaDock;
            }
			#endregion //Initialize record selectors

            // JJD 9/8/08
            // This logic was moved up from below so we did it before we did the check for a 0 field count below.
            #region Initialize/Clear the internal field sorted collections

            
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            #endregion //Initialize/Clear the internal field sorted collections	

            // AS 3/3/09 Optimization
            
			if (this._fieldLayout.Fields.Count < 1)
				return;
    
			#region raise the template initializing events

			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

			#endregion //raise the template initializing events

			#region Resolve the AutoArrangeCells setting

			// SSP 8/14/08 - Moveable Fields Feature
			// Once the user rearranges a single field, auto-arrange settings should not be 
			// taken into account.
			// 
			//this._autoArrangeCells = this._fieldLayout.AutoArrangeCellsResolved;

			this._autoArrangeCells = 
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
				// directly accessing the member var.
				// 
				//null != _fieldLayout._dragFieldLayoutInfo
				null != _fieldLayout.GetFieldLayoutInfo( false, false )
				? AutoArrangeCells.Never
				: this._fieldLayout.AutoArrangeCellsResolved;




			#endregion //Resolve the AutoArrangeCells setting

			#region Resolve the PrimaryField reservation

            // AS 2/3/09 Remove unused members
            //if (primaryField != null)
			//	this._primaryFieldCellContentAlignment = primaryField.CellContentAlignmentResolved;
			//else
			//	this._primaryFieldCellContentAlignment = CellContentAlignment.ValueOnly;

            if (this._autoArrangeCells == AutoArrangeCells.Never)
            {
                // ignore PrimaryField reservation in manual mode
                this._reservation = AutoArrangePrimaryFieldReservation.None;
            }
            else
            {
                this._reservation = AutoArrangePrimaryFieldReservation.Default;

                // JJD 12/21/07
                // We need to check HasSettings instead of HasFieldSettings
                //if (this._fieldLayout.HasFieldSettings)
                if (this._fieldLayout.HasSettings)
                    this._reservation = this._fieldLayout.Settings.AutoArrangePrimaryFieldReservation;

                // JJD 12/21/07
                // We need to check HasFieldLayoutSettings on DP
                //if (this._reservation == AutoArrangePrimaryFieldReservation.Default)
                if (this._reservation == AutoArrangePrimaryFieldReservation.Default &&
                    this._fieldLayout.DataPresenter.HasFieldLayoutSettings)
                    this._reservation = this._fieldLayout.DataPresenter.FieldLayoutSettings.AutoArrangePrimaryFieldReservation;

                if (this._reservation == AutoArrangePrimaryFieldReservation.Default)
                    this._reservation = this.DefaultAutoArrangePrimaryFieldReservation;

                if (this._reservation == AutoArrangePrimaryFieldReservation.ReserveFirstRowOrColumnBasedOnFlow)
                {
                    if (this._autoArrangeCells == AutoArrangeCells.LeftToRight)
                        this._reservation = AutoArrangePrimaryFieldReservation.ReserveFirstRow;
                    else
                        this._reservation = AutoArrangePrimaryFieldReservation.ReserveFirstColumn;
                }

                // if the priumary field's row was explicitly set then set the _reservation to none
                // see if we should reserve a row for it
                if (primaryField == null ||
                     !primaryField.IsVisibleInCellArea )
                {
                    this._reservation = AutoArrangePrimaryFieldReservation.None;
                }
            }

            
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			#endregion //Resolve the PrimaryField reservation	

            #region Calculate the max # of grid rows and columns

            this.CalculateMaxGridRowsAndColumns();

            #endregion //Calculate the max # of grid rows and columns	
    
            #region ProcessFields (pass 1 - calulate only)

            
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

            #endregion //ProcessFields (pass 1 - calulate only)	

			// JJD 5/1/07 - Optimization
			// Cache the sorted list of fields so we don't re-sort for every size manager
			#region Cache lists of fields sorted by row and column

            
#region Infragistics Source Cleanup (Region)




































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Cache lists of fields sorted by row and column	
    
            #region ProcessFields (pass 2 - update)

            // Call ProcessFields again with the calculateOnly parameter set to false
            // to actually update the styles
			// SSP 5/22/08 - BR33108 - Summaries Functionality
			// Added fefItemGridForSummary parameter.
			// 
            //this.ProcessFields(primaryField, fefItemGrid, fefHeaderGrid, false);
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
			//this.ProcessFields( primaryField, fefItemGrid, fefItemGridForSummary, fefHeaderGrid, false );
            // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
			//this.ProcessFields( primaryField, fefItemGrid, fefHeaderGrid, false );
            this.ProcessFields(primaryField, fefItemGrid, fefHeaderGrid);

            #endregion //ProcessFields	

			#region Seal the templates

            // AS 3/3/09 Optimization
            if (regenerateTemplates)
            {
                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                //this._generatedRecordContentAreaTemplate.Seal();
                this._generatedTemplateDataRecordContentAreaTemplate.Seal();

                this._generatedVirtualizedRecordContentAreaTemplate.Seal();

                // SSP 5/22/08 - BR33108 - Summaries Functionality
                // 
                _generatedSummaryRecordContentAreaTemplate.Seal();
                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                //_generatedSummaryCellAreaTemplate.Seal( );

                if (null != this._generatedHeaderTemplate)
                    this._generatedHeaderTemplate.Seal();

                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
                if (null != this._generatedTemplateDataRecordHeaderAreaTemplate)
                    this._generatedTemplateDataRecordHeaderAreaTemplate.Seal();
            }

			#endregion //Seal the templates

            #region Raise template initialized events

			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            #endregion //Raise template initialized events	

			#region Initialize the grid field map

			this.InitializeGridFieldMap();

			#endregion //Initialize the grid field map
    
		}

                #endregion //GenerateTemplate	

			#endregion //Public Methods

			#region Internal Methods

				#region GetCellAreaColumnDefinitions

		// Returns the column definition used for this column when
		// the template was last generated 
		internal ColumnDefinition[] GetCellAreaColumnDefinitions()
		{
			if ( this._cellAreaColumnDefinitions == null )
				return new ColumnDefinition[0];

			return (ColumnDefinition[])this._cellAreaColumnDefinitions.Clone();
		}

				#endregion //GetCellAreaColumnDefinitions

				#region GetCellAreaRowDefinitions

		// Returns the column definitions used for this column when
		// the template was last generated 
		internal RowDefinition[] GetCellAreaRowDefinitions()
		{
			if ( this._cellAreaRowDefinitions == null )
				return new RowDefinition[0];

			return (RowDefinition[])this._cellAreaRowDefinitions.Clone();
		}
				#endregion //GetCellAreaRowDefinitions

				#region GetHeaderAreaColumnDefinitions

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetHeaderAreaColumnDefinitions

				#region GetHeaderAreaRowDefinitions

        
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetHeaderAreaRowDefinitions

				#region GetSummaryCellAreaColumnDefinitions

        
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion // GetSummaryCellAreaColumnDefinitions

				#region GetSummaryCellAreaRowDefinitions

        
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

				#endregion // GetSummaryCellAreaRowDefinitions

			#endregion //Internal Methods	
       
			#region Private Methods
    
				#region AddCell

		
#region Infragistics Source Cleanup (Region)





































































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				#endregion //AddCell	
			
				#region AddLabel

		
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				#endregion //AddLabel	
    
				#region AddRowColumnDefinitions

        
#region Infragistics Source Cleanup (Region)










































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

   				#endregion //AddRowColumnDefinitions	

				#region ApplyPrimaryFieldRestrictions

        
#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

				#endregion //ApplyPrimaryFieldRestrictions	
    
				#region AssignPrimaryFieldRowColumn

        
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

				#endregion //AssignRowColumn	
    
				#region AssignRowColumn

        
#region Infragistics Source Cleanup (Region)

























































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //AssignRowColumn	

                // JJD 1/19/09 - NA 2009 vol 1
                #region BindContentSiteMargin

        private void BindContentSiteMargin(FrameworkElementFactory fefContentSite, bool isHeader)
        {
            fefContentSite.SetBinding(FrameworkElement.MarginProperty, GridUtilities.CreateRecordContentMarginBinding(this._fieldLayout, isHeader, fefContentSite.Type));
        }

                #endregion //BindContentSiteMargin	
    
                #region CalculateMaxGridRowsAndColumns

        private void CalculateMaxGridRowsAndColumns()
        {
            this._maxGridRows = -1;
            this._maxGridColumns = -1;

                // ignore PrimaryField maxRows and maxColumns
            if (this._autoArrangeCells == AutoArrangeCells.Never)
                return;

            // JJD 12/21/07
            // We need to check HasSettings instead of HasFieldSettings
            //if (this._fieldLayout.HasFieldSettings)
            if (this._fieldLayout.HasSettings)
            {
                this._maxGridColumns = this._fieldLayout.Settings.AutoArrangeMaxColumns;
                this._maxGridRows = this._fieldLayout.Settings.AutoArrangeMaxRows;
            }

			if (this._maxGridColumns < 0)
			{
                // JJD 12/21/07
                // We need to check HasFieldLayoutSettings 
                if ( this._fieldLayout.DataPresenter.HasFieldLayoutSettings )
				    this._maxGridColumns = this._fieldLayout.DataPresenter.FieldLayoutSettings.AutoArrangeMaxColumns;

				if (this._maxGridColumns < 0)
					this._maxGridColumns = this._fieldLayout.DataPresenter.CurrentViewInternal.DefaultAutoArrangeMaxColumns;
			}

			if (this._maxGridRows < 0)
			{
                // JJD 12/21/07
                // We need to check HasFieldLayoutSettings 
                if ( this._fieldLayout.DataPresenter.HasFieldLayoutSettings )
				    this._maxGridRows = this._fieldLayout.DataPresenter.FieldLayoutSettings.AutoArrangeMaxRows;

				if (this._maxGridRows < 0)
					this._maxGridRows = this._fieldLayout.DataPresenter.CurrentViewInternal.DefaultAutoArrangeMaxRows;
			}

            if (this._autoArrangeCells == AutoArrangeCells.LeftToRight)
            {
                if (this._maxGridColumns < 1)
                    this._maxGridColumns = 0;
            }
            else
            {
                if (this._maxGridRows < 1)
                   this._maxGridRows = 0;
            }
        }

                #endregion //CalculateMaxGridRowsAndColumns	

                #region GetTemplateType

		private CellTemplateType GetTemplateType(CellContentAlignment cellContentAlignment)
        {
            CellTemplateType templateType;
            switch (cellContentAlignment)
            {
                case CellContentAlignment.LabelOnly:
                    templateType = CellTemplateType.LabelOnly;
                    break;
                case CellContentAlignment.ValueOnly:
                    templateType = CellTemplateType.ValueOnly;
                    break;
                default:
					templateType = CellTemplateType.ValueAndLabel;
                    break;
            }
            return templateType;
        }

                #endregion //GetTemplateType	
    
				#region GetFieldSpanInfo

        
#region Infragistics Source Cleanup (Region)











































#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetFieldSpanInfo

                #region GetHorizontalAlignmentCell

        
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

                #endregion //GetHorizontalAlignmentCell

                #region GetHorizontalAlignmentLabel

		internal static HorizontalAlignment GetHorizontalAlignmentLabel(CellContentAlignment cellContentAlignment)
        {
            switch (cellContentAlignment)
            {
                case CellContentAlignment.LabelAboveValueAlignCenter:
                case CellContentAlignment.LabelBelowValueAlignCenter:
                    return HorizontalAlignment.Center;

                case CellContentAlignment.LabelAboveValueAlignLeft:
                case CellContentAlignment.LabelBelowValueAlignLeft:
                case CellContentAlignment.LabelRightOfValueAlignBottom:
                case CellContentAlignment.LabelRightOfValueAlignMiddle:
                case CellContentAlignment.LabelRightOfValueAlignTop:
                    return HorizontalAlignment.Left;

                case CellContentAlignment.LabelBelowValueAlignRight:
                case CellContentAlignment.LabelAboveValueAlignRight:
                case CellContentAlignment.LabelLeftOfValueAlignBottom:
                case CellContentAlignment.LabelLeftOfValueAlignMiddle:
                case CellContentAlignment.LabelLeftOfValueAlignTop:
                    return HorizontalAlignment.Right;

                case CellContentAlignment.LabelRightOfValueStretch:
                case CellContentAlignment.LabelLeftOfValueStretch:
                case CellContentAlignment.LabelAboveValueStretch:
                case CellContentAlignment.LabelBelowValueStretch:
                case CellContentAlignment.ValueOnly:
                case CellContentAlignment.LabelOnly:
                    return HorizontalAlignment.Stretch;
            }

            return HorizontalAlignment.Stretch;
        }

                #endregion //GetHorizontalAlignmentLabel
    
				#region GetPrimaryFieldSpanInfo

        
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetPrimaryFieldSpanInfo

                #region GetVerticalAlignmentCell
        
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

                #endregion //GetVerticalAlignmentCell

                #region GetVerticalAlignmentLabel

        internal static VerticalAlignment GetVerticalAlignmentLabel(CellContentAlignment cellContentAlignment)
        {
            switch (cellContentAlignment)
            {
                case CellContentAlignment.LabelLeftOfValueAlignMiddle:
                case CellContentAlignment.LabelRightOfValueAlignMiddle:
                    return VerticalAlignment.Center;

                case CellContentAlignment.LabelLeftOfValueAlignTop:
                case CellContentAlignment.LabelRightOfValueAlignTop:
                case CellContentAlignment.LabelBelowValueAlignCenter:
                case CellContentAlignment.LabelBelowValueAlignLeft:
                case CellContentAlignment.LabelBelowValueAlignRight:
                case CellContentAlignment.LabelBelowValueStretch:
                    return VerticalAlignment.Top;

                case CellContentAlignment.LabelRightOfValueAlignBottom:
                case CellContentAlignment.LabelLeftOfValueAlignBottom:
                case CellContentAlignment.LabelAboveValueAlignCenter:
                case CellContentAlignment.LabelAboveValueAlignLeft:
                case CellContentAlignment.LabelAboveValueAlignRight:
                case CellContentAlignment.LabelAboveValueStretch:
                    return VerticalAlignment.Bottom;

                case CellContentAlignment.LabelLeftOfValueStretch:
                case CellContentAlignment.LabelRightOfValueStretch:
                case CellContentAlignment.ValueOnly:
                case CellContentAlignment.LabelOnly:
                    return VerticalAlignment.Stretch;
            }

            return VerticalAlignment.Stretch;
        }

                #endregion //GetVerticalAlignmentLabel

				#region HookGridRowColumnChanged
        
#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)

				#endregion //HookGridRowColumnChanged

				#region InitializePrefixArea

		// JJD 4/27/07
		// Optimization - return the prefix dock area that we are using in the template
		private Dock? InitializePrefixArea( DataTemplate template, bool isHeaderTemplate, RecordSelectorLocation selectorLocation
			// SSP 4/9/08 - Summaries Functionality
			// Added isSummaryTemplate parameter.
			// 
			, bool isSummaryTemplate
			)
		{
			// Only add the record selector prefix area if the View wants to display record selectors.
			if (!this._fieldLayout.DataPresenter.CurrentViewInternal.ShouldDisplayRecordSelectors)
				return null;

            
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

            bool followsOrientation = true;

            switch (selectorLocation)
            {
                case RecordSelectorLocation.LeftOfCellArea:
                case RecordSelectorLocation.RightOfCellArea:
                    if (this.LogicalOrientation == Orientation.Horizontal)
                        followsOrientation = false;
                    break;
                case RecordSelectorLocation.AboveCellArea:
                case RecordSelectorLocation.BelowCellArea:
                    if (this.LogicalOrientation == Orientation.Vertical)
                        followsOrientation = false;
                    break;
            }

            if (!followsOrientation && (isHeaderTemplate || isSummaryTemplate))
                return null;

			FrameworkElementFactory fefCellLabelArea = template.VisualTree;

			#region Old Code - Using Grid
			
#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)


			#endregion //Old Code - Using Grid

			FrameworkElementFactory fefOuterPanel = new FrameworkElementFactory(typeof(DockPanel));

            // JJD 1/19/09 - NA 2009 vol 1
            // bind the margin to the calculated indent
            this.BindContentSiteMargin(fefOuterPanel, isHeaderTemplate);

			#region Prefix element

			FrameworkElementFactory fefPrefix;
			DependencyProperty fefFieldLayoutProperty, fefInternalVersionProperty;

			if (isHeaderTemplate)
			{
				fefPrefix = new FrameworkElementFactory(typeof(HeaderPrefixArea));
				fefFieldLayoutProperty = HeaderPrefixArea.FieldLayoutProperty;
				fefInternalVersionProperty = HeaderPrefixArea.InternalVersionProperty;
			}
			// SSP 4/9/08 - Summaries Functionality
			// Added isSummaryTemplate parameter.
			// 
			else if ( isSummaryTemplate )
			{
				fefPrefix = new FrameworkElementFactory( typeof( SummaryRecordPrefixArea ) );
				fefFieldLayoutProperty = SummaryRecordPrefixArea.FieldLayoutProperty;
				fefInternalVersionProperty = SummaryRecordPrefixArea.InternalVersionProperty;
			}
			else
			{
				fefPrefix = new FrameworkElementFactory( typeof( RecordSelector ) );
				fefFieldLayoutProperty = RecordSelector.FieldLayoutProperty;
				fefInternalVersionProperty = RecordSelector.InternalVersionProperty;
			}

			// AS 5/3/07
			// Currently these dependency properties are not "owned" versions of
			// another so they are actually different dependency properties and since
			// we were binding the dp's of the RecordSelector, we didn't get the 
			// fieldlayoutchanged for the HeaderPrefixArea.
			//
			//fefPrefix.SetValue(RecordSelector.FieldLayoutProperty, this._fieldLayout);
			//fefPrefix.SetBinding(RecordSelector.InternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._fieldLayout));
			fefPrefix.SetValue(fefFieldLayoutProperty, this._fieldLayout);
			fefPrefix.SetBinding(fefInternalVersionProperty, Utilities.CreateBindingObject(FieldLayout.InternalVersionProperty, BindingMode.OneWay, this._fieldLayout));

            // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
            // note we only want to do this when the prefix area is in line with the flow of the 
            // fixed area. e.g. if we're scrolling fields from left to right, we only want to fix
            // the record selector if its on the left or right.
            //
            if (followsOrientation)
            {
                RelativeSource rs = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(RecordPresenter), 1);
                DependencyProperty ttProp;

                // depending on whether the element is positioned near or far, we need to 
                // bind to different elements.
                if (selectorLocation == RecordSelectorLocation.AboveCellArea ||
                    selectorLocation == RecordSelectorLocation.LeftOfCellArea)
                {
                    ttProp = RecordPresenter.FixedNearElementTransformProperty;
                }
                else
                    ttProp = RecordPresenter.FixedFarElementTransformProperty;

                fefPrefix.SetBinding(UIElement.RenderTransformProperty, Utilities.CreateBindingObject(ttProp, BindingMode.OneWay, rs));
            }

			#endregion // Prefix element

            Binding extentBinding = Utilities.CreateBindingObject("RecordSelectorExtentResolved", BindingMode.OneWay, this._fieldLayout);

			Dock? dock = null;

			switch (selectorLocation)
			{
				case RecordSelectorLocation.LeftOfCellArea:
					fefPrefix.SetBinding(FrameworkElement.WidthProperty, extentBinding);
					dock = Dock.Left;
					break;
				case RecordSelectorLocation.AboveCellArea:
					fefPrefix.SetBinding(FrameworkElement.HeightProperty, extentBinding);
					dock = Dock.Top;
					break;
				case RecordSelectorLocation.BelowCellArea:
					fefPrefix.SetBinding(FrameworkElement.HeightProperty, extentBinding);
					dock = Dock.Bottom;
					break;
				case RecordSelectorLocation.RightOfCellArea:
					fefPrefix.SetBinding(FrameworkElement.WidthProperty, extentBinding);
					dock = Dock.Right;
					break;
			}

			if ( dock != null )
				 fefPrefix.SetValue(DockPanel.DockProperty, dock.Value);

			fefOuterPanel.AppendChild(fefPrefix);
			fefOuterPanel.AppendChild(fefCellLabelArea);
			template.VisualTree = fefOuterPanel;

			return dock;
		}

				#endregion //InitializePrefixArea	
    
				#region InitializeGridFieldMap

		private void InitializeGridFieldMap()
		{
			this.GridFieldMap.ResetMap();
		}

				#endregion //InitializeGridFieldMap

				#region OnGridColumnWidthChanged
        
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnGridColumnWidthChanged

				#region OnGridRowHeightChanged
        
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnGridRowHeightChanged

				#region ProcessFields

		// SSP 5/22/08 - BR33108 - Summaries Functionality
		// Added fefItemGridForSummary parameter.
		// 
		//private void ProcessFields(Field primaryField, FrameworkElementFactory fefItemGrid, FrameworkElementFactory fefHeaderGrid, bool calculateOnly)
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        // We don't need a summary grid since the summary always sizes to content and doesn't virtualize its cells.
        //
		//private void ProcessFields( Field primaryField, FrameworkElementFactory fefItemGrid, FrameworkElementFactory fefItemGridForSummary, FrameworkElementFactory fefHeaderGrid, bool calculateOnly )
        // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
        // Removed calculateOnly param since the ArrangeHelper class is doing the calculations.
        //
		//private void ProcessFields( Field primaryField, FrameworkElementFactory fefItemGrid, FrameworkElementFactory fefHeaderGrid, bool calculateOnly )
		private void ProcessFields( Field primaryField, FrameworkElementFactory fefItemGrid, FrameworkElementFactory fefHeaderGrid )
        {
            
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			// AS 7/7/09 TFS19145
			// This was only used when calling ProcessFieldHelper which is now removed.
			//
            //CellContentAlignment cellContentAlignment = CellContentAlignment.ValueOnly;

			// JJD 5/1/07
			// Use For loop instead of foreach
            //foreach (Field field in this._fieldLayout.Fields)
            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
            // We are caching the generated position of the preceeding field for collapsed 
            // fields. Since we don't want to follow the position of a field that is in a
            // different fixed area, we need to sort the fields by the fixed location first.
            //
			//FieldCollection fields = this._fieldLayout.Fields;
			//int count = fields.Count;
            int count = _fieldLayout.Fields.Count;
            Field[] fields = new Field[count];
            _fieldLayout.Fields.CopyTo(fields, 0);
            DataPresenterBase dp = _fieldLayout.DataPresenter;

            if (null != dp && dp.IsFixedFieldsSupportedResolved)
            {
                Comparison<Field> comparison = delegate(Field x, Field y)
                {
                    return GridUtilities.Compare(x.FixedLocation, y.FixedLocation);
                };
                IComparer<Field> fieldComparer = Utilities.CreateComparer(comparison);
                Utilities.SortMergeGeneric(fields, fieldComparer);
            }

			// SSP 1/6/09 TFS11860
			// When a new field is added or an existing collapsed field is made visible, we need
			// to make sure it doesn't overlap with other fields. Also if the field was collapsed,
			// it should appear close to where it was when it was collapsed.
			// 
			

			// SSP 6/26/09 - NAS9.2 Field Chooser
			// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
			// directly accessing the member var.
			// 
			//if ( null != _fieldLayout._dragFieldLayoutInfo )
			LayoutInfo flFieldLayoutInfo = _fieldLayout.GetFieldLayoutInfo( false, false );
			if ( null != flFieldLayoutInfo )
			{
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// 
				//_fieldLayout._dragFieldLayoutInfo.EnsureNewItemsDontOverlapHelper( );
				flFieldLayoutInfo.EnsureNewItemsDontOverlapHelper( );

				_fieldLayout._autoGeneratedPositions = null;
			}
			else if ( null == _fieldLayout._autoGeneratedPositions )
				_fieldLayout._autoGeneratedPositions = new LayoutInfo( _fieldLayout );

			LayoutInfo autoGeneratedPositions = _fieldLayout._autoGeneratedPositions;
			ItemLayoutInfo lastGeneratedFieldPos = null;

			

            // AS 1/8/09 NA 2009 Vol 1 - Fixed Fields
            ArrangeHelper arrangeHelper = new ArrangeHelper(_fieldLayout, _autoArrangeCells, primaryField, _reservation, _maxGridRows, _maxGridColumns);

            for (int i = 0; i < count; i++)
            {
				Field field = fields[i];

                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
                field.IsInLayout = arrangeHelper.IsInLayout(field);

                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
				//if ( !field.IsVisibleInCellArea )
                if ( !field.IsInLayout )
				{
					// SSP 1/6/09 TFS11860
					// 

					if ( null != autoGeneratedPositions )
					{
						ItemLayoutInfo cachedPosition;
						// SSP 9/1/09 TFS21347
						// Enclosed the existing code into the if block. If the field was ever 
						// auto-generated then retain its position information rather than 
						// overwriting it with the position of the last auto-generated field.
						// 
						// SSP 2/23/10 - TFS25122 TFS28016
						// Commented out the if condition. It's not necessary due to the TFS25122 fix.
						// 
						//if ( ! autoGeneratedPositions.TryGetValue( field, out cachedPosition ) )
						//{
							// AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
							//autoGeneratedPositions[field] = null != lastGeneratedFieldPos ? lastGeneratedFieldPos.Clone() : new ItemLayoutInfo(0, 0, 1, 1);
							cachedPosition = null != lastGeneratedFieldPos ? lastGeneratedFieldPos.Clone( ) : new ItemLayoutInfo( 0, 0, 1, 1 );
							// SSP 12/23/09 TFS25122
							// When the field is made visible, it should be shown after the last generated field.
							// If the positions are left the same, the LayoutInfo's logic for ensuring items made
							// visible don't overlap will offset other intersecting items which means that the
							// last generated field will be the one to actually get offset to the right, causing
							// the order of the field to be wrong.
							// 
							if ( null != lastGeneratedFieldPos )
							{
								// SSP 2/24/10 TFS28070
								// Take into account whether fields are auto-arranged top-to-bottom versus
								// prev-to-right. Added the if block and enclosed the existing code into the
								// else block.
								// 
								if ( AutoArrangeCells.TopToBottom == _autoArrangeCells )
									cachedPosition.Row += lastGeneratedFieldPos.RowSpan;
								else
									cachedPosition.Column += lastGeneratedFieldPos.ColumnSpan;
							}

							cachedPosition._fixedLocation = field.FixedLocation;
						//}

						// SSP 8/21/09 - TFS19187, TFS19273
						// Since the field is not going to be visible in the layout mark it collapsed.
						// 
						cachedPosition.IsCollapsed = true;

						autoGeneratedPositions[field] = cachedPosition;
					}

					continue;
				}

                
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
                SpanInfo spanInfo = arrangeHelper.GetSpanInfo(field);
                RowColInfo rowColInfo = arrangeHelper.GetRowColumn(field);

				// SSP 1/6/09 TFS11860
				// 

				if ( null != autoGeneratedPositions )
				{
					lastGeneratedFieldPos = new ItemLayoutInfo( rowColInfo.Column, rowColInfo.Row, spanInfo.ColumnSpan, spanInfo.RowSpan );

                    // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                    lastGeneratedFieldPos._fixedLocation = field.FixedLocation;

					// SSP 8/21/09 - TFS19187, TFS19273
					// Since the field is not going to be visible in the layout mark it collapsed.
					// 
					lastGeneratedFieldPos.Visibility = field.VisibilityInCellArea;

					autoGeneratedPositions[field] = lastGeneratedFieldPos;
				}


                
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


                // SSP 5/22/08 - BR33108 - Summaries Functionality
				// Added fefItemGridForSummary parameter. Pass that along.
				// 
                //this.ProcessFieldHelper(field, fefItemGrid, fefHeaderGrid, calculateOnly, cellContentAlignment, ref rowColInfo, ref spanInfo, ref highestRow, ref highestColumn);
                // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
				//this.ProcessFieldHelper( field, fefItemGrid, fefItemGridForSummary, fefHeaderGrid, calculateOnly, cellContentAlignment, ref rowColInfo, ref spanInfo, ref highestRow, ref highestColumn );
                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
				//this.ProcessFieldHelper( field, fefItemGrid, fefHeaderGrid, calculateOnly, cellContentAlignment, ref rowColInfo, ref spanInfo, ref highestRow, ref highestColumn );
				// AS 7/7/09 TFS19145
				// Removed the helper method since we will now let the templatedatarecordcache manage
				// the CellPlaceholder instances within the templates.
				//
				//this.ProcessFieldHelper( field, fefItemGrid, fefHeaderGrid, cellContentAlignment, ref rowColInfo, ref spanInfo);
				field.SetGridPositions(ref rowColInfo, ref spanInfo);
			}

            // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
            //if (!calculateOnly)
            {
                
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)


				this.FieldLayout.SetHasSeparateHeader(this._separateHeader);

                // AS 1/9/09 NA 2009 Vol 1 - Fixed Fields
				//this._fieldLayout.SetGridCounts(highestColumn + 1, highestRow + 1);
				this._fieldLayout.SetGridCounts(arrangeHelper.ColumnCount, arrangeHelper.RowCount);

                
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

            }
        }

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                #endregion //ProcessFields

			#endregion //Private Methods

		#endregion //Methods

		#region SpanInfo struct

		internal struct SpanInfo
		{
			internal int RowSpan;
			internal int ColumnSpan;
		}

		#endregion //SpanInfo struct

		#region RowColInfo struct

		internal struct RowColInfo
		{
			internal int Row;
			internal int Column;
		}

		#endregion //RowColInfo struct

		#region GridSlotComparer private class

		internal class GridSlotComparer : IComparer<Field>
		{
			private bool _isWidth;

			#region Constructor

			internal GridSlotComparer(bool isWidth)
			{
				this._isWidth = isWidth;
			}

			#endregion //Constructor

			#region IComparer<Field> Members

			int IComparer<Field>.Compare(Field x, Field y)
			{
				if (x == y)
					return 0;

				if (x == null)
					return -1;

				if (y == null)
					return 1;

				Field.FieldGridPosition xPos = x.GridPosition;
				Field.FieldGridPosition yPos = y.GridPosition;

				int xSpan = this._isWidth ? xPos.ColumnSpan : xPos.RowSpan;
				int ySpan = this._isWidth ? yPos.ColumnSpan : yPos.RowSpan;

				if (xSpan < ySpan)
					return -1;

				if (xSpan > ySpan)
					return 1;

				int xLastSlot = xSpan + (this._isWidth ? xPos.Column : xPos.Row);
				int yLastSlot = ySpan + (this._isWidth ? yPos.Column : yPos.Row);

				if (xLastSlot < yLastSlot)
					return -1;

				if (xLastSlot > yLastSlot)
					return 1;

				int xIndex = x.Index;
				int yIndex = y.Index;

				if (xIndex < yIndex)
					return -1;

				return 1;
			}

			#endregion
		}

		#endregion //GridSlotComparer private class

		// JM BR31474 4/4/08
		#region GridRowColExtentChangeListener Private Class

        
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

		#endregion //GridRowColExtentChangeListener Private Class

        // AS 1/8/09 NA 2009 Vol 1 - Fixed Fields
        #region ArrangeHelper
        private class ArrangeHelper
        {
            #region Member Variables

            // note: these are only used when calculating an arrange
            // if we have an arrange (because there was a dragdrop) then 
            // we don't have to use this
            private List<Field> _fixedNearFields = new List<Field>();
            private List<Field> _fixedFarFields = new List<Field>();
            private List<Field> _scrollableFields = new List<Field>();

            private readonly Field _primaryField;
            private readonly AutoArrangePrimaryFieldReservation _primaryFieldReservation;
            private readonly AutoArrangeCells _autoArrange;
            private readonly FieldLayout _fieldLayout;
            private readonly int _maxRows;
            private readonly int _maxColumns;

            // used externally
            private LayoutInfo _layoutInfo;
            private int _totalRows;
            private int _totalColumns;

            #endregion //Member Variables

            #region Constructor
            internal ArrangeHelper(FieldLayout fieldLayout, AutoArrangeCells autoArrange,
                Field primaryField, AutoArrangePrimaryFieldReservation primaryFieldReservation,
                int maxRows, int maxColumns)
            {
                _fieldLayout = fieldLayout;
                _autoArrange = autoArrange;
                _primaryField = primaryField;

                Debug.Assert(null != primaryField || primaryFieldReservation == AutoArrangePrimaryFieldReservation.None);

                if (primaryField == null || primaryField.IsVisibleInCellArea == false)
                    primaryFieldReservation = AutoArrangePrimaryFieldReservation.None;

                _primaryFieldReservation = primaryFieldReservation;

                if (autoArrange == AutoArrangeCells.Never)
                {
                    _maxRows = _maxColumns = 0;
                }
                else
                {
                    _maxRows = maxRows;
                    _maxColumns = maxColumns;
                }

                this.Initialize();
            }
            #endregion //Constructor

            #region Properties
            internal int ColumnCount
            {
                get { return this._totalColumns; }
            }

            internal int RowCount
            {
                get { return this._totalRows; }
            } 
            #endregion //Properties

            #region Methods

            #region AddAutoArrange
            private void AddAutoArrange(LayoutInfo layoutInfo, IEnumerator enumerator, int row, int column, int maxRow, int maxColumn)
            {
                if (_autoArrange == AutoArrangeCells.LeftToRight)
                {
                    if (row >= maxRow)
                        return;
                }
                else
                {
                    if (column >= maxColumn)
                        return;
                }

                int origRow = row;
                int origColumn = column;

                while (enumerator.MoveNext())
                {
                    Field field = (Field)enumerator.Current;

                    // we could have a placeholder
                    if (null != field)
                    {
                        ItemLayoutInfo newItem = new ItemLayoutInfo(column, row);
                        newItem._fixedLocation = field.FixedLocation;
                        layoutInfo.Add(field, newItem);
                    }

                    if (_autoArrange == AutoArrangeCells.LeftToRight)
                    {
                        column++;

                        if (column >= maxColumn)
                        {
                            column = origColumn;
                            row++;

                            if (row >= maxRow)
                                break;
                        }
                    }
                    else
                    {
                        row++;

                        if (row >= maxRow)
                        {
                            row = origRow;
                            column++;

                            if (column >= maxColumn)
                                break;
                        }
                    }
                }
            } 
            #endregion //AddAutoArrange

            #region AddAreaExtents
            private static void AddAreaExtents(int nearItemCount, int farItemCount, int scrollableItemCount,
                int remainingExtent, ref int nearCount, ref int farCount, ref int scrollableCount)
            {
                bool hasNear = nearItemCount > 0;
                bool hasFar = farItemCount > 0;
                bool hasScrollable = scrollableItemCount > 0;

                for (int i = 0; i < remainingExtent; i++)
                {
                    int nearRequiredRows = hasNear ? ((nearItemCount - 1) / nearCount) + 1 : 0;
                    int farRequiredRows = hasFar ? ((farItemCount - 1) / farCount) + 1 : 0;
                    int scrollRequiredRows = hasScrollable ? ((scrollableItemCount - 1) / scrollableCount) + 1 : 0;

                    if (scrollRequiredRows > Math.Max(farRequiredRows, nearRequiredRows))
                        scrollableCount++;
                    else if (farRequiredRows > nearRequiredRows)
                        farCount++;
                    else if (hasNear)
                        nearCount++;
                    else if (hasFar)
                        farCount++;
                    else
                    {
                        Debug.Assert(hasScrollable);

                        scrollableCount++;
                    }
                }
            }
            #endregion //AddAreaExtents

            #region AddFields
            private static void AddFields(LayoutInfo layoutInfo, IList<Field> fields, int offsetX, int offsetY)
            {
                foreach (Field field in fields)
                {
                    ItemLayoutInfo itemInfo = new ItemLayoutInfo(
                        field.Column + offsetX,
                        field.Row + offsetY,
                        Math.Max(1, field.ColumnSpan),
                        Math.Max(1, field.RowSpan));

                    itemInfo._fixedLocation = field.FixedLocation;

                    layoutInfo[field] = itemInfo;
                }
            }
            #endregion //AddFields

            #region ArrangeArea
            private void ArrangeArea(LayoutInfo layoutInfo, bool isLeftToRight, bool isHorizontal, ref int row, ref int column,
                int colCount, int rowCount,
                List<Field> fields, List<Field> primaryFieldList,
                int primaryColumnCount, int primaryRowCount, int totalLogicalRowSpan, int totalLogicalColSpan)
            {
                if (fields == primaryFieldList)
                {
                    // if we're reserving a column for the primary field
                    if (primaryColumnCount > 0)
                    {
                        int primaryColumn = isHorizontal ? column - primaryColumnCount : column;

                        ItemLayoutInfo primaryItem = new ItemLayoutInfo(primaryColumn, row,
                            primaryColumnCount, isHorizontal ? rowCount : totalLogicalRowSpan);

                        primaryItem._fixedLocation = _primaryField.FixedLocation;

                        layoutInfo.Add(_primaryField, primaryItem);

                        if (!isHorizontal)
                            column += primaryColumnCount;
                    }
                    else if (primaryRowCount > 0)
                    {
                        int primaryRow = !isHorizontal ? row - primaryRowCount : row;

                        ItemLayoutInfo primaryItem = new ItemLayoutInfo(column, primaryRow,
                            isHorizontal ? colCount : totalLogicalColSpan, primaryRowCount);

                        primaryItem._fixedLocation = _primaryField.FixedLocation;

                        layoutInfo.Add(_primaryField, primaryItem);

                        if (isHorizontal)
                            row += primaryRowCount;
                    }
                }

                AddAutoArrange(layoutInfo, fields.GetEnumerator(), row, column,
                    row + rowCount,
                    column + colCount);

                if (isLeftToRight != isHorizontal)
                {
                    if (isLeftToRight)
                        column += colCount;
                    else
                        row += rowCount;
                }
                else if (isLeftToRight)
                {
                    row += rowCount;
                }
                else
                    column += colCount;
            }

            #endregion //ArrangeArea

            #region IsInLayout
            internal bool IsInLayout(Field field)
            {
                ItemLayoutInfo info;
                if (!_layoutInfo.TryGetValue(field, out info))
                    return false;

                if (info.IsCollapsed)
                    return false;

                return field.IsVisibleInCellArea;
            } 
            #endregion //IsInLayout

            #region GetList
            private List<Field> GetList(FixedFieldLocation fixedLocation)
            {
                switch (fixedLocation)
                {
                    default:
                    case FixedFieldLocation.Scrollable:
                        return _scrollableFields;
                    case FixedFieldLocation.FixedToNearEdge:
                        return _fixedNearFields;
                    case FixedFieldLocation.FixedToFarEdge:
                        return _fixedFarFields;
                }
            } 
            #endregion //GetList

            #region GetLogicalRowColCount
            private static void GetLogicalRowColCount(IEnumerable<Field> fields, out int maxCol, out int maxRow)
            {
                maxCol = 0;
                maxRow = 0;

                foreach (Field field in fields)
                {
                    if (field.IsVisibleInCellArea)
                    {
                        maxCol = Math.Max(maxCol, field.Column + Math.Max(1, field.ColumnSpan));
                        maxRow = Math.Max(maxRow, field.Row + Math.Max(1, field.RowSpan));
                    }
                }
            }
            #endregion //GetLogicalRowColCount

            #region GetRowColumn
            internal RowColInfo GetRowColumn(Field field)
            {
                ItemLayoutInfo itemInfo;
                _layoutInfo.TryGetValue(field, out itemInfo);

                Debug.Assert(null != itemInfo);

                RowColInfo rc = new RowColInfo();

                if (null != itemInfo)
                {
                    rc.Row = itemInfo.Row;
                    rc.Column = itemInfo.Column;
                }

                return rc;
            }
            #endregion //GetRowColumn

            #region GetSpanInfo
            internal SpanInfo GetSpanInfo(Field field)
            {
                ItemLayoutInfo itemInfo;
                _layoutInfo.TryGetValue(field, out itemInfo);

                Debug.Assert(null != itemInfo);

                SpanInfo si = new SpanInfo();

                if (null != itemInfo)
                {
                    si.RowSpan = itemInfo.RowSpan;
                    si.ColumnSpan = itemInfo.ColumnSpan;
                }

                return si;
            }
            #endregion //GetSpanInfo

            #region Initialize
            private void Initialize()
            {
                LayoutInfo layoutInfo = null;


				// SSP 6/26/09 - NAS9.2 Field Chooser
				// Made _dragFieldLayoutInfo private and instead added GetFieldLayoutInfo and 
				// SetDragFieldLayoutInfo methods.
				// 
                //layoutInfo = _fieldLayout._dragFieldLayoutInfo;
				layoutInfo = _fieldLayout.GetFieldLayoutInfo( false, false );


                if (layoutInfo != null)
                {
                    _fieldLayout.ShouldCreateDragFieldLayout = false;

					// AS 3/15/11 TFS65358
					// Moved this logic into a helper routine on the LayoutInfo.
					// If the developer changes one or more field positions and 
					// there is no overlap of the sections then just leave the 
					// structure intact. Otherwise we will continue to do the 
					// processing that we had done previously which would adjust 
					// the positions, move fixed/unfixed items into gaps, etc.
					//
					//// the view orientation may have changed so we have 
					//// to do a verification of the layout
					//layoutInfo.EnsureFixedLocationsDontOverlap();
					//
					//// the FixedLocation of one or more fields may have been set so 
					//// we need to do a pass and process them as needed.
					//layoutInfo.EnsureFixedLocationsInSync();
					layoutInfo.VerifyTemplateGeneratorLayout();
                }
                else
                {
                    bool isFixedSupported = _fieldLayout.DataPresenter != null ? _fieldLayout.DataPresenter.IsFixedFieldsSupportedResolved : false;
                    layoutInfo = new LayoutInfo(_fieldLayout);
                    bool isHorizontal = _fieldLayout.IsHorizontal;

                    #region Build Field Lists

                    // first build a list of the fixed/unfixed fields
                    foreach (Field field in _fieldLayout.Fields)
                    {
                        if (!field.IsVisibleInCellArea)
                            continue;

                        FixedFieldLocation fixedLocation;

                        if (isFixedSupported == false)
                            fixedLocation = FixedFieldLocation.Scrollable;
                        else
                            fixedLocation = field.FixedLocation;

                        switch (fixedLocation)
                        {
                            case FixedFieldLocation.FixedToNearEdge:
                                _fixedNearFields.Add(field);
                                break;
                            case FixedFieldLocation.FixedToFarEdge:
                                _fixedFarFields.Add(field);
                                break;
                            case FixedFieldLocation.Scrollable:
                                _scrollableFields.Add(field);
                                break;
                        }
                    }
                    #endregion //Build Field Lists

                    
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


                    if (_autoArrange == AutoArrangeCells.Never)
                    {
                        ProcessAutoArrangeNever(layoutInfo);
                    }
                    else
                    {
                        ProcessAutoArrange(layoutInfo, isFixedSupported, isHorizontal);
                    }

                    // if the FixedLocation of one or more fields was changed
                    // and we don't have a snapshot then we need to create 
                    // a snapshot now. we will use the autogenerated layout
                    // as that snapshot
                    if (_fieldLayout.ShouldCreateDragFieldLayout)
                    {
                        _fieldLayout.ShouldCreateDragFieldLayout = false;

						// SSP 6/26/09 - NAS9.2 Field Chooser
						// Made _dragFieldLayoutInfo private and instead added GetFieldLayoutInfo and 
						// SetDragFieldLayoutInfo methods.
						// 
                        //_fieldLayout._dragFieldLayoutInfo = layoutInfo;
						_fieldLayout.SetFieldLayoutInfo( layoutInfo, true, false );

                    }
                }

                #region Total Rows/Columns

                // track the number of rows/columns created
                int totalColumns = 0;
                int totalRows = 0;

                foreach (ItemLayoutInfo item in layoutInfo.Values)
                {
					// SSP 1/11/10 TFS26233
					// Skip collapsed items. Enclosed the existing code into the if block.
					// 
					if ( !item.IsCollapsed )
					{
						totalColumns = Math.Max( totalColumns, item.Column + item.ColumnSpan );
						totalRows = Math.Max( totalRows, item.Row + item.RowSpan );
					}
                }

                _totalColumns = totalColumns;
                _totalRows = totalRows;

                #endregion //Total Rows/Columns

                _layoutInfo = layoutInfo;
            }

            #endregion //Initialize

            #region ProcessAutoArrange
            private void ProcessAutoArrange(LayoutInfo layoutInfo, bool isFixedSupported, bool isHorizontal)
            {
                bool isLeftToRight = _autoArrange == AutoArrangeCells.LeftToRight;
                int maxColumns = _maxColumns;
                int maxRows = _maxRows;

                List<Field> primaryFieldList = null;
                FixedFieldLocation? primaryFixedLocation = null;
                int primaryColumnCount = 0;
                int primaryRowCount = 0;

                #region _primaryFieldReservation
                if (_primaryFieldReservation != AutoArrangePrimaryFieldReservation.None)
                {
                    Debug.Assert(null != _primaryField);

                    // primary field reservation
                    primaryFixedLocation = isFixedSupported
                        ? _primaryField.FixedLocation
                        : FixedFieldLocation.Scrollable;

                    // get the area's field list that contains the primary field and 
                    // remove it from that list
                    primaryFieldList = GetList(primaryFixedLocation.Value);
                    primaryFieldList.Remove(_primaryField);

                    if (_primaryFieldReservation == AutoArrangePrimaryFieldReservation.ReserveFirstCell)
                    {
                        // just put it at the front of its list
                        primaryFieldList.Insert(0, _primaryField);
                        primaryFieldList = null;
                    }
                    else
                    {
                        if (_primaryFieldReservation == AutoArrangePrimaryFieldReservation.ReserveFirstColumn)
                        {
                            primaryColumnCount++;
                        }
                        else
                        {
                            primaryRowCount++;
                        }
                    }
                }
                #endregion //_primaryFieldReservation

                int fixedFarColCount = 0;
                int fixedFarRowCount = 0;
                int fixedNearColCount = 0;
                int fixedNearRowCount = 0;
                int scrollableColCount = 0;
                int scrollableRowCount = 0;

                if (_maxColumns > 0 && _maxColumns <= primaryColumnCount)
                {
                    // only the primary field is shown
                }
                else if (_maxRows > 0 && _maxRows <= primaryRowCount)
                {
                    // only the primary field is shown
                }
                else
                {
                    int nearItemCount = _fixedNearFields.Count;
                    int farItemCount = _fixedFarFields.Count;
                    int scrollableItemCount = _scrollableFields.Count;

                    if (maxColumns == 0)
                        maxColumns = int.MaxValue;
                    else
                        maxColumns -= primaryColumnCount;

                    if (maxRows == 0)
                        maxRows = int.MaxValue;
                    else
                        maxRows -= primaryRowCount;

                    if (!isLeftToRight)
                    {
                        GridUtilities.SwapValues(ref maxRows, ref maxColumns);
                    }

                    int extent = isHorizontal != isLeftToRight
                        // Vertical && LeftToRight OR Horizontal && TopToBottom
                        ? nearItemCount + farItemCount + scrollableItemCount
                        // Vertical && TopToBottom OR Horizontal && LeftToRight
                        : Math.Max(nearItemCount, Math.Max(farItemCount, scrollableItemCount));

                    if (extent <= maxColumns)
                    {
                        // only need 1 row/column
                        fixedFarColCount = farItemCount;
                        fixedNearColCount = nearItemCount;
                        scrollableColCount = scrollableItemCount;
                        fixedFarRowCount = Math.Min(farItemCount, 1);
                        fixedNearRowCount = Math.Min(nearItemCount, 1);
                        scrollableRowCount = Math.Min(scrollableItemCount, 1);
                    }
                    else
                    {
                        if (isHorizontal != isLeftToRight)
                        {
                            // allocate at least 1 logical column to each area
                            fixedFarColCount = farItemCount > 0 ? 1 : 0;
                            fixedNearColCount = nearItemCount > 0 ? 1 : 0;
                            scrollableColCount = scrollableItemCount > 0 ? 1 : 0;

                            int remainingExtent = maxColumns - (fixedNearColCount + fixedFarColCount + scrollableColCount);

                            AddAreaExtents(nearItemCount, farItemCount, scrollableItemCount, remainingExtent,
                                ref fixedNearColCount, ref fixedFarColCount, ref scrollableColCount);

                            // calculate the number of rows taking the max rows into account
                            fixedFarRowCount = farItemCount == 0 ? 0 : Math.Min(maxRows, (farItemCount - 1) / fixedFarColCount + 1);
                            fixedNearRowCount = nearItemCount == 0 ? 0 : Math.Min(maxRows, (nearItemCount - 1) / fixedNearColCount + 1);
                            scrollableRowCount = scrollableItemCount == 0 ? 0 : Math.Min(maxRows, (scrollableItemCount - 1) / scrollableColCount + 1); 
                        }
                        else
                        {
                            // Vertical && TopToBottom OR Horizontal && LeftToRight

                            // allocate 1 column per area
                            fixedFarRowCount = farItemCount > 0 ? 1 : 0;
                            fixedNearRowCount = nearItemCount > 0 ? 1 : 0;
                            scrollableRowCount = scrollableItemCount > 0 ? 1 : 0;

                            // we may have more rows than we need
                            maxRows = Math.Min(maxRows,
                                (fixedFarRowCount == 0 ? 0 : (farItemCount - 1) / maxColumns + 1) +
                                (fixedNearRowCount == 0 ? 0 : (nearItemCount - 1) / maxColumns + 1) +
                                (scrollableRowCount == 0 ? 0 : (scrollableItemCount - 1) / maxColumns + 1));

                            int remainingExtent = maxRows - (fixedFarRowCount + fixedNearRowCount + scrollableRowCount);

                            AddAreaExtents(nearItemCount, farItemCount, scrollableItemCount, remainingExtent,
                                ref fixedNearRowCount, ref fixedFarRowCount, ref scrollableRowCount);

                            // calculate the number of rows taking the max rows into account
                            fixedFarColCount = Math.Min(maxColumns, farItemCount);
                            fixedNearColCount = Math.Min(maxColumns, nearItemCount);
                            scrollableColCount = Math.Min(maxColumns, scrollableItemCount); 
                        }
                    }

                    if (!isLeftToRight)
                    {
                        GridUtilities.SwapValues(ref fixedNearRowCount, ref fixedNearColCount);
                        GridUtilities.SwapValues(ref fixedFarColCount, ref fixedFarRowCount);
                        GridUtilities.SwapValues(ref scrollableColCount, ref scrollableRowCount);
                    }
                }

                Trim(_fixedNearFields, fixedNearColCount * fixedNearRowCount);
                Trim(_fixedFarFields, fixedFarColCount * fixedFarRowCount);
                Trim(_scrollableFields, scrollableColCount * scrollableRowCount);

                int row = !isHorizontal ? primaryRowCount : 0;
                int column = isHorizontal ? primaryColumnCount : 0;

				// AS 8/21/09 TFS21049
				// It is possible that the only field in a given section will be the 
				// primary field in which case we need to ensure that that area has 
				// at least 1 row and column. What was happening is that we were giving 
				// the primary field a columnspan of 0. When we went to drag it, we assumed 
				// its column span was doubled but it wasn't because it was only 0.
				//
				if (primaryFixedLocation != null)
				{
					switch (primaryFixedLocation.Value)
					{
						case FixedFieldLocation.Scrollable:
							scrollableColCount = Math.Max(1, scrollableColCount);
							scrollableRowCount = Math.Max(1, scrollableRowCount);
							break;
						case FixedFieldLocation.FixedToNearEdge:
							fixedNearColCount = Math.Max(1, fixedNearColCount);
							fixedNearRowCount = Math.Max(1, fixedNearRowCount);
							break;
						case FixedFieldLocation.FixedToFarEdge:
							fixedFarColCount = Math.Max(1, fixedFarColCount);
							fixedFarRowCount = Math.Max(1, fixedFarRowCount);
							break;
					}
				}

                // we need to know how many rows we will actually use so we know how
                // much the span of a primary field reservation would be
                int totalLogicalRowSpan = Math.Max(fixedFarRowCount, Math.Max(fixedNearRowCount, scrollableRowCount));
                int totalLogicalColSpan = Math.Max(fixedFarColCount, Math.Max(fixedNearColCount, scrollableColCount));

                // if the primary field that needs special handling is in this list...
                ArrangeArea(layoutInfo, isLeftToRight, isHorizontal, 
                    ref row, ref column, 
                    fixedNearColCount, fixedNearRowCount, 
                    _fixedNearFields, primaryFieldList, primaryColumnCount, primaryRowCount,
                    totalLogicalRowSpan, totalLogicalColSpan);
                ArrangeArea(layoutInfo, isLeftToRight, isHorizontal, 
                    ref row, ref column,
                    scrollableColCount, scrollableRowCount, 
                    _scrollableFields, primaryFieldList, primaryColumnCount, primaryRowCount,
                    totalLogicalRowSpan, totalLogicalColSpan);
                ArrangeArea(layoutInfo, isLeftToRight, isHorizontal, 
                    ref row, ref column,
                    fixedFarColCount,
                    fixedFarRowCount, 
                    _fixedFarFields, primaryFieldList, primaryColumnCount, primaryRowCount,
                    totalLogicalRowSpan, totalLogicalColSpan);
            }
            #endregion //ProcessAutoArrange

            #region ProcessAutoArrangeNever
            private void ProcessAutoArrangeNever(LayoutInfo layoutInfo)
            {
                int offsetX, offsetY;
                GetLogicalRowColCount(_fieldLayout.Fields, out offsetX, out offsetY);

                if (_fieldLayout.IsHorizontal)
                    offsetX = 0;
                else
                    offsetY = 0;

                // the near fields can use the actual origins
                AddFields(layoutInfo, _fixedNearFields, 0, 0);

                if (_fixedNearFields.Count > 0)
                {
                    if (_scrollableFields.Count > 0)
                    {
                        // if there are near fixed fields then we need to shift over
                        AddFields(layoutInfo, _scrollableFields, offsetX, offsetY);

                        // increase the offset for the far fixed fields
                        offsetX *= 2;
                        offsetY *= 2;
                    }
                }
                else
                {
                    AddFields(layoutInfo, _scrollableFields, 0, 0);
                }

                if (layoutInfo.Count > 0)
                    AddFields(layoutInfo, _fixedFarFields, offsetX, offsetY);
                else
                    AddFields(layoutInfo, _fixedFarFields, 0, 0);

                layoutInfo.PackLayout();
            } 
            #endregion //ProcessAutoArrangeNever

            #region Trim<T>
            private static void Trim<T>(List<T> items, int maxCount)
            {
                if (items.Count > maxCount)
                    items.RemoveRange(maxCount, items.Count - maxCount);
            } 
	        #endregion //Trim<T>

            #endregion //Methods
        } 
        #endregion //ArrangeHelper
    }

	#endregion //FieldLayoutTemplateGenerator abstract base class

	#region GridViewFieldLayoutTemplateGenerator class

	/// <summary>
	/// Initializes a field layout's settings for a gridview presentation 
	/// </summary>
	public class GridViewFieldLayoutTemplateGenerator : FieldLayoutTemplateGenerator
	{
		#region Private Members

		private Orientation _orientation;

		#endregion //Private Members	
     
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GridViewFieldLayoutTemplateGenerator"/> class
		/// </summary>
		/// <param name="fieldLayout">The <see cref="FieldLayout"/> for which the templates will be generated</param>
		/// <param name="orientation">The orientation of the records in the grid view</param>
		public GridViewFieldLayoutTemplateGenerator(FieldLayout fieldLayout, Orientation orientation)
			: base(fieldLayout)
		{
			this._orientation = orientation;
		}

		#endregion //Constructors	

		#region Properties

			#region Orientation

		/// <summary>
		/// Gets/sets the orientation of the layout (e.g. 'Vertical' means that the fields are lined up verticially).
		/// </summary>
		public Orientation Orientation
		{
			get { return this._orientation; }
			set 
            {
                if (value != this._orientation)
                {
                    this._orientation = value;
                    this.InvalidateGeneratedTemplates();
                }
            }
		}

			#endregion //Orientation

		#endregion //Properties	
        
		#region Base class overrides

			#region DefaultAutoArrangeCells

		/// <summary>
		/// Return the default layout for field's
		/// </summary>
		public override AutoArrangeCells DefaultAutoArrangeCells
		{
			get
			{
				if (this._orientation == Orientation.Horizontal)
					return AutoArrangeCells.TopToBottom;
				else
					return AutoArrangeCells.LeftToRight;
			}
		}

			#endregion //DefaultAutoArrangeCells

			#region DefaultAutoArrangePrimaryFieldReservation

		/// <summary>
		/// Returns the default AutoArrangePrimaryFieldReservation
		/// </summary>
		protected override AutoArrangePrimaryFieldReservation DefaultAutoArrangePrimaryFieldReservation { get { return AutoArrangePrimaryFieldReservation.ReserveFirstCell; } }

			#endregion //DefaultAutoArrangePrimaryFieldReservation

			#region DefaultCellContentAlignment

		/// <summary>
		/// Returns the default label alignment
		/// </summary>
		public override CellContentAlignment DefaultCellContentAlignment
		{
			get
			{
				if (this._orientation == Orientation.Horizontal)
					return CellContentAlignment.LabelLeftOfValueStretch;
				else
					return CellContentAlignment.LabelAboveValueStretch;
			}
		}

			#endregion //DefaultCellContentAlignment

            #region CellPresentation

        /// <summary>
        /// Returns the layout mode (read-only)
        /// </summary>
		public override CellPresentation CellPresentation { get { return CellPresentation.GridView; } }

            #endregion //CellPresentation	

			// JM NA 10.1 CardView - Added.
			#region GetRecordNavigationDirectionFromCellNavigationDirection

		/// <summary>
		/// Returns the <see cref="PanelNavigationDirection"/> value that should be used to navigate to an adjacent record when the specified cell navigation direction is used and a suitable target cell cannot be found in the current record.
		/// </summary>
		/// <param name="cellNavigationDirection"></param>
		/// <returns></returns>
		public override PanelNavigationDirection GetRecordNavigationDirectionFromCellNavigationDirection(PanelNavigationDirection cellNavigationDirection)
		{
			switch (cellNavigationDirection)
			{
				case PanelNavigationDirection.Next:
					if (this.LogicalOrientation == Orientation.Vertical)
						return PanelNavigationDirection.Below;
					else
						return PanelNavigationDirection.Right;

				case PanelNavigationDirection.Previous:
					if (this.LogicalOrientation == Orientation.Vertical)
						return PanelNavigationDirection.Above;
					else
						return PanelNavigationDirection.Left;

				case PanelNavigationDirection.Left:
					if (this.LogicalOrientation == Orientation.Vertical)
						return PanelNavigationDirection.Above;

					break;

				case PanelNavigationDirection.Right:
					if (this.LogicalOrientation == Orientation.Vertical)
						return PanelNavigationDirection.Below;

					break;

				case PanelNavigationDirection.Above:
					if (this.LogicalOrientation == Orientation.Horizontal)
						return PanelNavigationDirection.Left;

					break;

				case PanelNavigationDirection.Below:
					if (this.LogicalOrientation == Orientation.Horizontal)
						return PanelNavigationDirection.Right;

					break;
			}

			return cellNavigationDirection;
		}

			#endregion //GetRecordNavigationDirectionFromCellNavigationDirection

			#region LogicalOrientation

		/// <summary>
		/// Returns the orientation for gridview layouts.
		/// </summary>
		public override Orientation LogicalOrientation { get { return this._orientation; } }

			#endregion //LogicalOrientation

				#region HasLogicalOrientation

		/// <summary>
		/// Returns whether the generator has a logical orientation.
		/// </summary>
		public override bool HasLogicalOrientation { get { return true; } }

				#endregion //HasLogicalOrientation

		#endregion //Base class overrides
	}

	#endregion //GridViewFieldLayoutTemplateGenerator class

	#region HeaderedGridViewFieldLayoutTemplateGenerator class

	/// <summary>
	/// Initializes  a field layout's settings for a gridview presentation that has a separate header 
	/// </summary>
	public class HeaderedGridViewFieldLayoutTemplateGenerator : GridViewFieldLayoutTemplateGenerator
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderedGridViewFieldLayoutTemplateGenerator"/> class
		/// </summary>
		/// <param name="fieldLayout">The <see cref="FieldLayout"/> for which the templates will be generated</param>
		/// <param name="orientation">The orientation of the records in the grid view</param>
		public HeaderedGridViewFieldLayoutTemplateGenerator(FieldLayout fieldLayout, Orientation orientation)
			: base(fieldLayout, orientation)
		{
		}

		#endregion //Constructors	

        
		#region Base class overrides

			#region SupportsLabelHeaders

		/// <summary>
		/// Returns true if the panel supports label headers
		/// </summary>
		public override bool SupportsLabelHeaders { get { return true; } }

			#endregion //SupportsLabelHeaders

		#endregion //Base class overrides
	}

	#endregion //HeaderedGridViewFieldLayoutTemplateGenerator class

	#region CardViewFieldLayoutTemplateGenerator class

	/// <summary>
	/// Initializes a field layout's settings for a gridview presentation 
	/// </summary>
	public class CardViewFieldLayoutTemplateGenerator : FieldLayoutTemplateGenerator
	{
		#region Member Variables

		// JM NA 10.1 CardView 
		private Orientation?					_orientation;
		private bool							_recordsArrangedInRowsAndCols;

		#endregion //Member Variables

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CardViewFieldLayoutTemplateGenerator"/> class
		/// </summary>
		/// <param name="fieldLayout">The <see cref="FieldLayout"/> for which the templates will be generated</param>
		public CardViewFieldLayoutTemplateGenerator(FieldLayout fieldLayout)
			: this(fieldLayout, null, false)
		{
		}

		// JM NA 10.1 CardView - Added.
		/// <summary>
		/// Initializes a new instance of the <see cref="CardViewFieldLayoutTemplateGenerator"/> class
		/// </summary>
		/// <param name="fieldLayout">The <see cref="FieldLayout"/> for which the templates will be generated</param>
		/// <param name="orientation">The LogicalOrientation of the view for which templates will be generated or null if the view has no LogicalOrientation.</param>
		/// <param name="recordsArrangedInRowsAndCols">True if <see cref="Record"/>s are arranged in rows and columns (like <see cref="CardView"/>) or false if they are not (like <see cref="GridView"/> or <see cref="CarouselView"/>).</param>
		public CardViewFieldLayoutTemplateGenerator(FieldLayout fieldLayout, Orientation? orientation, bool recordsArrangedInRowsAndCols)
			: base(fieldLayout)
		{
			this._orientation					= orientation;
			this._recordsArrangedInRowsAndCols	= recordsArrangedInRowsAndCols;
		}

		#endregion //Constructors	

		#region Base class overrides

			// JM NA 10.1 CardView
			#region AreRecordsArrangedInRowsAndCols







		internal override bool AreRecordsArrangedInRowsAndCols
		{
			get { return this._recordsArrangedInRowsAndCols; }
		}

			#endregion //AreRecordsArrangedInRowsAndCols

			#region DefaultAutoArrangeCells

		/// <summary>
		/// Return the default layout for field's
		/// </summary>
		public override AutoArrangeCells DefaultAutoArrangeCells { get { return AutoArrangeCells.TopToBottom; } }

			#endregion //DefaultAutoArrangeCells	

            #region CellPresentation

        /// <summary>
        /// Returns the layout mode (read-only)
        /// </summary>
		public override CellPresentation CellPresentation { get { return CellPresentation.CardView; } }

            #endregion //CellPresentation	

			// JM NA 10.1 CardView - Added.
			#region GetRecordNavigationDirectionFromCellNavigationDirection

		/// <summary>
		/// Returns the <see cref="PanelNavigationDirection"/> value that should be used to navigate to an adjacent record when the specified cell navigation direction is used and a suitable target cell cannot be found in the current record.
		/// </summary>
		/// <param name="cellNavigationDirection"></param>
		/// <returns></returns>
		public override PanelNavigationDirection GetRecordNavigationDirectionFromCellNavigationDirection(PanelNavigationDirection cellNavigationDirection)
		{
			// This method shouldn't normally be called if we do not have a LogicalOrientation
			Debug.Assert(this.HasLogicalOrientation == true, "Unexpected call to GetRecordNavigationDirectionFromCellNavigationDirection");
			if (this.HasLogicalOrientation == false)
				return cellNavigationDirection;

			switch (cellNavigationDirection)
			{
				case PanelNavigationDirection.Next:
					if (this.LogicalOrientation == Orientation.Vertical)
						return PanelNavigationDirection.Below;
					else
						return PanelNavigationDirection.Right;

				case PanelNavigationDirection.Previous:
					if (this.LogicalOrientation == Orientation.Vertical)
						return PanelNavigationDirection.Above;
					else
						return PanelNavigationDirection.Left;

				case PanelNavigationDirection.Left:
					return PanelNavigationDirection.Left;

				case PanelNavigationDirection.Right:
					return PanelNavigationDirection.Right;

				case PanelNavigationDirection.Above:
					return PanelNavigationDirection.Above;

				case PanelNavigationDirection.Below:
					return PanelNavigationDirection.Below;
			}

			return cellNavigationDirection;
		}

			#endregion //GetRecordNavigationDirectionFromCellNavigationDirection

			// JM 01-26-10 NA 10.1 CardView - Added.
			#region HasLogicalOrientation

		/// <summary>
		/// Returns whether the generator has a logical orientation.
		/// </summary>
		public override bool HasLogicalOrientation { get { return this._orientation != null; } }

			#endregion //HasLogicalOrientation

			// JM 01-26-10 NA 10.1 CardView - Added.
			#region LogicalOrientation

		/// <summary>
		/// Returns the orientation for gridview layouts.
		/// </summary>
		public override Orientation LogicalOrientation 
		{ 
			get 
			{ 
				// AS 2/25/10
				// While we shouldn't ask for this if HasLogicalOrientation returns false the thing 
				// is that we are and previously for carousel view this would have returned Vertical
				// so I think we should continue to return that as the default.
				//
				//Debug.Assert(this._orientation.HasValue, "LogicalOrientation is being asked for but it is null!");
				//return this._orientation.HasValue ? this._orientation.Value : Orientation.Horizontal; 
				return this._orientation.HasValue ? this._orientation.Value : Orientation.Vertical; 
			} 
		}

			#endregion //LogicalOrientation

            #region PrimaryFieldDefaultCellContentAlignment

        /// <summary>
        /// Returns the default label alignment for the primary field
        /// </summary>
        public override CellContentAlignment PrimaryFieldDefaultCellContentAlignment
        {
            get { return CellContentAlignment.LabelAboveValueStretch; }
        }

            #endregion //PrimaryFieldDefaultCellContentAlignment
    
		#endregion //Base class overrides
	}

	#endregion //CardViewFieldLayoutTemplateGenerator class

	#region GridFieldMap Class

	
	
	
	
	


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class GridFieldMap
	{
		#region Member Variables

		private Field[,]								_map;
		private FieldLayoutTemplateGenerator			_fieldLayoutTemplateGenerator = null;
		private FieldLayout								_fieldLayout = null;

		// AS 5/30/07 BR23163
		private int										_maxSeqOffset = 0;

        // AS 2/27/09 TFS14730/Optimization
        private Field                                   _firstField;
        private Field                                   _lastField;

		// AS 4/27/09 TFS17122
		private Field[]									_navigationFields;

		#endregion //Member Variables

		#region Constructor

		internal GridFieldMap(FieldLayoutTemplateGenerator fieldLayoutTemplateGenerator)
		{
			this._fieldLayoutTemplateGenerator	= fieldLayoutTemplateGenerator;
			this._fieldLayout					= fieldLayoutTemplateGenerator.FieldLayout;
		}

		#endregion //Constructor

		#region Properties

			#region Map

		/// <summary>
		/// Returns a 2 dimensional array the corresponds in size (i,e, rows and columns) to the grid element
		/// used in the FieldLayout.  Each element in the array contains a reference to the Field that has been
		/// assigned to the correspnding grid cell.  If no Field has been assigned to a grid cell the corresponding
		/// array element will contain null.  Note that if a Field spans grid cells and/or columns, references to that
		/// field will appear in multiple array elements.
		/// </summary>
		public Field[,] Map
		{
			get
			{
                // AS 2/27/09 TFS14730
                // Moved to a helper method since other members need to do this as well.
                //
                //if (this._map == null)
				//	this.Initialize();
                this.VerifyInitialized();

				return this._map;
			}
		}

			#endregion //Map

		#endregion //Properties

		#region Methods

			#region Internal Methods

                // AS 2/27/09 TFS14730
                #region GetFirstField
        internal Field GetFirstField()
        {
            this.VerifyInitialized();

            return _firstField;
        }
                #endregion //GetFirstField

                // AS 2/27/09 TFS14730
                #region GetLastField
        internal Field GetLastField()
        {
            this.VerifyInitialized();

            return _lastField;
        }
                #endregion //GetLastField

				// AS 4/27/09 TFS17122
				#region GetNavigationIndex
		internal int GetNavigationIndex(Field field, int previousIndex)
		{
			this.VerifyInitialized();

			int index = previousIndex;

			if (previousIndex < 0 ||
				previousIndex >= _navigationFields.Length ||
				_navigationFields[previousIndex] != field)
				index = Array.IndexOf(_navigationFields, field);

			return index;
		}
				#endregion //GetNavigationIndex

				// AS 4/27/09 NA 2009.2 ClipboardSupport
				#region GetNavigationOrderFields
		internal Field[] GetNavigationOrderFields()
		{
			this.VerifyInitialized();

			return this._navigationFields;
		}
				#endregion //GetNavigationOrderFields

				#region GetNavigationTargetCell



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		internal Cell GetNavigationTargetCell(Cell currentCell, PanelNavigationDirection navigationDirection, IViewPanel panelNavigator, ISelectionHost selectionHost, bool shiftKeyDown, bool ctlKeyDown, PanelSiblingNavigationStyle siblingNavigationStyle)
		{
			// Try to get the target cell with the same record.
			
			// JJD 5/01/07 - Optimization
			// get the grid postion once
			Field.FieldGridPosition gridPosition = currentCell.Field.GridPosition;

			int		fromRow = gridPosition.Row;
			int		fromCol = gridPosition.Column;
			Cell	cell	= GetCell(currentCell.Record, fromRow, fromCol, navigationDirection, siblingNavigationStyle, false);

			if (cell != null)
				return cell;


			// Didn't find a cell in the same record - if our caller only wanted us to look within the
			// same record, we're done.
			if (siblingNavigationStyle == PanelSiblingNavigationStyle.StayWithinParentAndWrap ||
				siblingNavigationStyle == PanelSiblingNavigationStyle.StayWithinParentNoWrap)
				return null;


			// Since we couldn't navigate to the target cell in the same record, try an adjacent record.
			// Determine the direction in which to look for the adjacent record
			PanelNavigationDirection	recordNavigationDirection	= navigationDirection;
			Orientation					logicalOrientation			= this._fieldLayoutTemplateGenerator.LogicalOrientation;

			if (this._fieldLayoutTemplateGenerator.HasLogicalOrientation)
			{
				// Translate the cell navigation direction to the appropriate record navigation direction.
				// JM NA 10.1 CardView - Moved the following code into a virtual method called GetRecordNavigationDirectionFromCellNavigationDirection
				// in the FieldLayoutTemplateGenerator class, and added code to call the method here.
				#region Old moved code
				
#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

				#endregion //Old moved code
				recordNavigationDirection = this._fieldLayoutTemplateGenerator.GetRecordNavigationDirectionFromCellNavigationDirection(navigationDirection);
			}
			else
			{
				switch (navigationDirection)
				{
					case PanelNavigationDirection.Next:
					case PanelNavigationDirection.Right:
					case PanelNavigationDirection.Below:
						recordNavigationDirection = PanelNavigationDirection.Next;
						break;
					case PanelNavigationDirection.Previous:
					case PanelNavigationDirection.Left:
					case PanelNavigationDirection.Above:
						recordNavigationDirection = PanelNavigationDirection.Previous;
						break;
				}
			}


			// Get the adjacent record.
			// JM 11-26-08 BR19510 [TFS5725]
			//DataRecord adjacentRecord = panelNavigator.GetNavigationTargetRecord(currentCell.Record, recordNavigationDirection, selectionHost, shiftKeyDown, ctlKeyDown, siblingNavigationStyle, typeof(DataRecord)) as DataRecord;
			DataRecord adjacentRecord;
			if (shiftKeyDown)
				adjacentRecord = panelNavigator.GetNavigationTargetRecord(currentCell.Record, recordNavigationDirection, selectionHost, shiftKeyDown, ctlKeyDown, PanelSiblingNavigationStyle.StayWithinParentNoWrap, typeof(DataRecord)) as DataRecord;
			else
				adjacentRecord = panelNavigator.GetNavigationTargetRecord(currentCell.Record, recordNavigationDirection, selectionHost, shiftKeyDown, ctlKeyDown, siblingNavigationStyle, typeof(DataRecord)) as DataRecord;

			// JJD 10/26/11 - TFS91364 
			// Ignore HeaderRecords
			//if (adjacentRecord == null)
			if (adjacentRecord == null || adjacentRecord is HeaderRecord)
				return null;


			// Use the FieldLayout of the adjacent record which could be different that our FieldLayout.
			FieldLayout adjacentRecordFieldLayout = adjacentRecord.FieldLayout;

            // AS 2/27/09 TFS14730
            FieldLayoutTemplateGenerator generator = adjacentRecordFieldLayout.StyleGenerator;

            if (null != generator)
                generator.GridFieldMap.VerifyInitialized();

			// Determine the 'from row' and 'from col' where we should start the search for a target cell in the 
			// adjacent record based on the navigation direction and orientation of the records.
			//
			// [JM BR21398 4/23/07] Remove the check for logical orientation.  The navigationDirection variable has already
			// been 'adjusted' above for the case where the field layout template generator has no logical orientation.
//			if (this._fieldLayoutTemplateGenerator.HasLogicalOrientation)
//			{
				switch (navigationDirection)
				{
					case PanelNavigationDirection.Above:
						if (logicalOrientation == Orientation.Vertical)
							fromRow = adjacentRecordFieldLayout.TotalRowsGenerated - 1;
						else
						{
							fromRow = adjacentRecordFieldLayout.TotalRowsGenerated - 1;
							fromCol = adjacentRecordFieldLayout.TotalColumnsGenerated - 1;
						}

						break;

					case PanelNavigationDirection.Below:
						if (logicalOrientation == Orientation.Vertical)
							fromRow = 0;
						else
						{
							fromRow = 0;
							fromCol = 0;
						}

						break;

					case PanelNavigationDirection.Left:
						// JM NA 10.1 CardView.  Check if records are arranged in rows and cols.
						if (generator.AreRecordsArrangedInRowsAndCols == false)
						{
							if (logicalOrientation == Orientation.Vertical)
							{
								fromRow = adjacentRecordFieldLayout.TotalRowsGenerated - 1;
								fromCol = adjacentRecordFieldLayout.TotalColumnsGenerated - 1;
							}
							else
								fromCol = adjacentRecordFieldLayout.TotalColumnsGenerated - 1;
						}

						break;

					case PanelNavigationDirection.Right:
						// JM NA 10.1 CardView.  Check if records are arranged in rows and cols.
						if (generator.AreRecordsArrangedInRowsAndCols == false)
						{
							if (logicalOrientation == Orientation.Vertical)
							{
								fromRow = 0;
								fromCol = 0;
							}
							else
								fromCol = 0;
						}

						break;

					case PanelNavigationDirection.Previous:
						if (logicalOrientation == Orientation.Vertical)
						{
							fromRow = adjacentRecordFieldLayout.TotalRowsGenerated - 1;
							fromCol = adjacentRecordFieldLayout.TotalColumnsGenerated - 1;
						}
						else
						{
							fromRow = adjacentRecordFieldLayout.TotalRowsGenerated - 1;
							fromCol = adjacentRecordFieldLayout.TotalColumnsGenerated - 1;
						}

						break;

					case PanelNavigationDirection.Next:
						if (logicalOrientation == Orientation.Vertical)
						{
							fromRow = 0;
							fromCol = 0;
						}
						else
						{
							fromRow = 0;
							fromCol = 0;
						}

						break;
				}
//			}
//			else
//			{
//			}


			// Try to navigate to a cell in the adjacent record.
                return GetCell(adjacentRecord, fromRow, fromCol, navigationDirection, siblingNavigationStyle, true);
		}

				#endregion //GetNavigationTargetCell

				#region Initialize







        // AS 2/27/09 TFS14730
        // There are no external callers and it really shouldn't be up to 
        // something external to call this since we don't want to rebuild the 
        // map unless its dirty.
        //
        //internal void Initialize()
		private void Initialize()
		{
            // AS 2/27/09 TFS14730
            // If the templates haven't been generated or its dirty then the Total(Rows|Columns)
            // Generated will be wrong since they are updated by the generator when the templates 
            // are generated.
            //
            this._fieldLayout.VerifyStyleGeneratorTemplates();

			int totalRows		= this._fieldLayout.TotalRowsGenerated;
			int totalColumns	= this._fieldLayout.TotalColumnsGenerated;
			// Allocate a new map.
			this._map = new Field[totalRows, totalColumns];


			// Populate the map with field references.
			foreach (Field field in this._fieldLayout.Fields)
			{
				if (field.CellContentAlignmentResolved	== CellContentAlignment.LabelOnly	||
					field.VisibilityResolved		!= Visibility.Visible				||
					field.IsExpandableResolved		== true) 

					continue;


				// JJD 5/01/07 - Optimization
				// get the grid postion once
				Field.FieldGridPosition gridPosition = field.GridPosition;

				int row;
				//for (row = field.GridPosition.Row;
				//     row < field.GridPosition.Row + field.GridPosition.RowSpan;
				//     row++)
				for (row = gridPosition.Row;
					 row < gridPosition.Row + gridPosition.RowSpan;
					 row++)
				{
					int col;

					if (row < totalRows)
					{
						//for (col = field.GridPosition.Column;
						//     col < field.GridPosition.Column + field.GridPosition.ColumnSpan;
						//     col++)
						for (col = gridPosition.Column;
							 col < gridPosition.Column + gridPosition.ColumnSpan;
							 col++)
						{
							if (col < totalColumns)
							{
								if (this._map[row, col] == null)
									this._map[row, col] = field;
							}
							else
								break;
						}
					}
					else
						break;
				}
			}

			// AS 5/30/07 BR23163
			// We were assuming that the maximum index that we could use was based
			// on the total rows * total columns - 1 but that would not be the case
			// when there is a gap on the end. Since we always navigate by column
			// and then by row, I was able to always look for the last column
			// in the last row that was occupied.
			//
			this._maxSeqOffset = totalColumns * totalRows - 1;

			for (; this._maxSeqOffset > 0; this._maxSeqOffset--)
			{
				int row = this._maxSeqOffset / totalColumns;
				int col = this._maxSeqOffset % totalColumns;

				if (this._map[row, col] != null)
					break;
			}

            // AS 2/27/09 TFS14730/Optimization
            // Previously the record was getting the Total(Rows|Columns)Generated
            // and the map and looking for the field. We should calculate it in the 
            // map and cache it.
            //
            #region Cache First/Last Field

            _firstField = null;

            for (int i = 0; i <= _maxSeqOffset; i++)
            {
                int row = i / totalColumns;
                int col = i % totalColumns;

                _firstField = this._map[row, col];

                if (null != _firstField)
                    break;
            }

            _lastField = null;

            for (int i = _maxSeqOffset; i >= 0; i--)
            {
                int row = i / totalColumns;
                int col = i % totalColumns;

                _lastField = this._map[row, col];

                if (null != _lastField)
                    break;
            } 
            #endregion //Cache First/Last Field

			// AS 4/27/09 TFS17122
			// We can pre-calculate what the next/previous fields will be based on their 
			// position within the map.
			//
			#region Navigation Indexes

			bool leftToRight = true;
			List<Field> navigationFields = new List<Field>();
			for (int i = 0, iCount = leftToRight ? totalRows : totalColumns; i < iCount; i++)
			{
				for (int j = 0, jCount = leftToRight ? totalColumns : totalRows; j < jCount; j++)
				{
					int row = leftToRight ? i : j;
					int col = leftToRight ? j : i;
					Field fld = _map[row, col];

					// skip empty slots
					if (fld == null)
						continue;

					// make sure we skip any we've process - a field that exists to the left/top
					// of the slot we're processing
					if (row > 0 && _map[row - 1, col] == fld)
						continue;

					if (col > 0 && _map[row, col - 1] == fld)
						continue;

					Debug.Assert(navigationFields.Contains(fld) == false);
					navigationFields.Add(fld);
				}
			}

			_navigationFields = navigationFields.ToArray(); 

			#endregion //Navigation Indexes
		}

				#endregion Initialize

				#region ResetMap







		internal void ResetMap()
		{
			this._map = null;

            // AS 2/27/09 TFS14730
            _lastField = _firstField = null;
		}

				#endregion //ResetMap

			#endregion //Internal Methods

			#region Private methods

				// JM NA 10.1 CardView.
				#region CompressFieldMap

		// Compress the specified field map by removing rows and/or columns that contain all collapsed cells.
		private static Field[,] CompressFieldMap(Field[,] fieldMap, Record record)
		{
			// Copy all rows that contain at least 1 uncollapsed cell from fieldMap to map1.
			int			totalRows			= fieldMap.GetLength(0);
			int			totalCols			= fieldMap.GetLength(1);
			Field[,]	map1				= new Field[totalRows, totalCols];
			int			totalCompactedRows	= 0;
			for (int r = 0; r < totalRows; r++)
			{
				bool copyRow = false;
				for (int c = 0; c < totalCols; c++)
				{
					map1[totalCompactedRows, c] = fieldMap[r, c];
					if (false == record.ShouldCollapseCell(fieldMap[r, c]))
						copyRow = true;
				}

				if (copyRow)
					totalCompactedRows++;
			}


			// Copy all columns that contain at least 1 uncollapsed cell from map1 to map2.
			totalRows						= map1.GetLength(0);
			totalCols						= map1.GetLength(1);
			Field[,]	map2				= new Field[totalCompactedRows, totalCols];
			int			totalCompactedCols	= 0;
			for (int c = 0; c < totalCols; c++)
			{
				bool copyCol = false;
				for (int r = 0; r < totalCompactedRows; r++)
				{
					map2[r, totalCompactedCols] = map1[r, c];
					if (false == record.ShouldCollapseCell(map1[r, c]))
						copyCol = true;
				}

				if (copyCol)
					totalCompactedCols++;
			}

			// If map2 has the right number of columns return it, otherwise trim the excess columns.
			if (map2.GetLength(1) == totalCompactedRows)
				return map2;

			Field[,] map3 = new Field[totalCompactedRows, totalCompactedCols];
			for (int r = 0; r < totalCompactedRows; r++)
			{
				for (int c = 0; c < totalCompactedCols; c++)
					map3[r, c] = map2[r, c];
			}

			return map3;
		}

				#endregion //CompressFieldMap

				// AS 4/27/09 TFS17122
				#region GetCellInDirection
		private static Cell GetCellInDirection(Field[,] fields, int row, int column, DataRecord record, 
			Field startingField, bool includeCurrent, PanelNavigationDirection direction, bool wrap)
		{
			// JM NA 10.1 CardView.  Create a new Field map that removes rows/cols that contain all collapsed cells.
			Field[,] fieldsCompacted;
			int rowCount, colCount;
			if (record.ShouldCollapseEmptyCellsResolved)
			{
				fieldsCompacted = CompressFieldMap(fields, record);
				rowCount		= fieldsCompacted.GetLength(0);
				colCount		= fieldsCompacted.GetLength(1);
				
				int adjustedRow, adjustedCol;
				GetFieldRowColInMap(fieldsCompacted, startingField, out adjustedRow, out adjustedCol);
				if (adjustedRow > -1 && adjustedCol > -1)
				{
					row		= adjustedRow;
					column	= adjustedCol;
				}
				else
				{
					row		= Math.Min(rowCount - 1, row);
					column	= Math.Min(colCount - 1, column);
					startingField = fieldsCompacted[row,column];
				}
			}
			else
			{
				fieldsCompacted = fields;
				rowCount = fields.GetLength(0);
				colCount = fields.GetLength(1);
			}

			// JM NA 10.1 CardView.  Use the compacted field map created above to determine the row and col counts.  Also, ensure that
			// the row and column parameters fall within the range of the compactedfield map.

			int count;
			int rowAdjustment = 0;
			int colAdjustment = 0;
			bool isLeftRight = false;

			switch (direction)
			{
				case PanelNavigationDirection.Above:
					count = row + 1;
					rowAdjustment = -1;
					break;
				case PanelNavigationDirection.Below:
					count = rowCount - row;
					rowAdjustment = 1;
					break;
				case PanelNavigationDirection.Left:
					isLeftRight = true;
					count = column + 1;
					colAdjustment = -1;
					break;
				case PanelNavigationDirection.Right:
					isLeftRight = true;
					count = colCount - column;
					colAdjustment = 1;
					break;
				default:
					Debug.Fail("Wrong direction");
					return null;
			}

			Cell cell = null;

			for (int i = 0; i < count; i++)
			{
				int r = row + (rowAdjustment * i);
				int c = column + (colAdjustment * i);

				Debug.Assert(r >= 0 && r < rowCount);
				Debug.Assert(c >= 0 && c < colCount);

				// JM NA 10.1 CardView.  Use the compacted field map created above.
				//Field field = fields[r, c];
				Field field = fieldsCompacted[r, c];

				if (null == field)
					continue;

				// AS 10/13/09 NA 2010.1 - CardView
				// If the cell is collapsed then we need to skip it in the navigation.
				//
				if (record.ShouldCollapseCell(field))
					continue;

				// left/right/above/below were never true spatial navigation since when they 
				// wrapped they would wrap to the next line. since sometimes that would cause
				// you to remain in the same "row" (e.g. 
				// Zero   One
				// Zero   Two
				// navigating from One right would go to Zero and then right again is back to One
				// but if you had 
				// Zero   One
				// Two    Three and navigated from One right it would go to Two) we decided to
				// take the origin of the navigating direction into account. this would allow 
				// you to navigate through all the cells using the arrow keys and still be able
				// to navigate across rows
				//
				// JM NA 10.1 CardView - only make the following checks if we are not using a compacted field map;
				if (false == record.ShouldCollapseEmptyCellsResolved)
				{
					if (isLeftRight && field.GridPosition.Row != r)
						continue;
					else if (!isLeftRight && field.GridPosition.Column != c)
						continue;
				}

				cell = GetNavigationCell(startingField, record, includeCurrent, field);

				if (null != cell)
					break;
			}

			if (null == cell && wrap)
			{
				bool canWrap = false;

				switch (direction)
				{
					case PanelNavigationDirection.Above:
						if (column > 0)
						{
							canWrap = true;
							row = rowCount - 1;
							column--;
						}
						break;
					case PanelNavigationDirection.Below:
						if (column < colCount - 1)
						{
							canWrap = true;
							row = 0;
							column++;
						}
						break;
					case PanelNavigationDirection.Left:
						if (row > 0)
						{
							canWrap = true;
							column = colCount - 1;
							row--;
						}
						break;
					case PanelNavigationDirection.Right:
						if (row < rowCount - 1)
						{
							canWrap = true;
							column = 0;
							row++;
						}
						break;
				}

				if (canWrap)
				{
					// JM NA 10.1 CardView.  Use the compacted field map created above.
					//cell = GetCellInDirection(fields,
					cell = GetCellInDirection(fieldsCompacted,
						row,
						column,
						record, startingField, includeCurrent, direction, wrap);
				}
			}

			return cell;
		} 
				#endregion //GetCellInDirection

				#region GetCell (Old)






		
#region Infragistics Source Cleanup (Region)






































































































































































































#endregion // Infragistics Source Cleanup (Region)

		
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

		
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)














































#endregion // Infragistics Source Cleanup (Region)


				#endregion //GetCell (Old)

				// AS 4/27/09 TFS17122
				#region GetCell
		private static Cell GetCell(DataRecord record, int row, int column, PanelNavigationDirection navigationDirection, PanelSiblingNavigationStyle siblingNavigationStyle, bool includeCurrent)
		{
			// The previous logic didn't take spans into account so when navigating using the 
			// arrow or tab keys you could get "stuck" within the fields and in other cases
			// we just completely skipped fields. For the next/previous we now prebuild a list
			// of the fields in the map and can index into those. For the arrow key navigation 
			// (which wasn't and continues to not be true spatial navigation), we walk over the 
			// rows/columns in the map based on the direction ignoring fields that don't originate
			// in the navigating direction. I.e. if navigating left<=>right in row 1, we ignore a 
			// field that spans 2 but whose origin is 0.
			//
			FieldLayout fl = record.FieldLayout;
			FieldLayoutTemplateGenerator generator = fl.StyleGenerator;
			Debug.Assert(null != generator);
			GridFieldMap map = generator.GridFieldMap;
			map.VerifyInitialized();

			Field[,] fields = map.Map;
			int rowCount = fields.GetLength(0);
			int columnCount = fields.GetLength(1);

			if (columnCount == 0 || rowCount == 0)
				return null;

			// make sure the row/column are in range
			column = Math.Max(0, Math.Min(columnCount - 1, column));
			row = Math.Max(0, Math.Min(rowCount - 1, row));

			Field startingField = fields[row, column];

			switch (navigationDirection)
			{
				case PanelNavigationDirection.Next:
				case PanelNavigationDirection.Previous:
					{
						bool wrap = siblingNavigationStyle == PanelSiblingNavigationStyle.StayWithinParentAndWrap;

						return GetNextPreviousCell(map, row, column, record, startingField, includeCurrent,
							navigationDirection == PanelNavigationDirection.Next,
							wrap);
					}
				case PanelNavigationDirection.Above:
				case PanelNavigationDirection.Below:
				case PanelNavigationDirection.Left:
				case PanelNavigationDirection.Right:
					{
						bool wrap = siblingNavigationStyle == PanelSiblingNavigationStyle.StayWithinParentAndWrap;

						// we still may want to wrap if we are in a logically oriented layout
						// and are navigating against the orientation in which the records are 
						// arranged
						if (!wrap)
						{
							bool hasLogicalOrientation = generator.HasLogicalOrientation;
							Orientation logicalOrientation = generator.LogicalOrientation;
							bool isLeftOrRight = navigationDirection == PanelNavigationDirection.Left ||
								navigationDirection == PanelNavigationDirection.Right;

							// JM NA 10.1 - CardView.
							//wrap = hasLogicalOrientation && (isLeftOrRight == (logicalOrientation == Orientation.Vertical));
							wrap = hasLogicalOrientation && (isLeftOrRight == (logicalOrientation == Orientation.Vertical)) && false == generator.AreRecordsArrangedInRowsAndCols;
						}

						return GetCellInDirection(map.Map, row, column, record, startingField, includeCurrent, 
							navigationDirection,
							wrap);
					}
				default:
					Debug.Fail("Unexpected direction:" + navigationDirection.ToString());
					return null;
			}
		} 
				#endregion //GetCell

				// JM NA 10.1 CardView
				#region GetFieldRowColInMap
		private static void GetFieldRowColInMap(Field[,] fieldMap, Field field, out int row, out int col)
		{
			int totalRows = fieldMap.GetLength(0);
			int totalCols = fieldMap.GetLength(1);
			for (int r = 0; r < totalRows; r++)
			{
				for (int c = 0; c < totalCols; c++)
				{
					if (fieldMap[r, c] == field)
					{
						row = r;
						col = c;
						return;
					}
				}
			}

			row = -1;
			col = -1;
		}
				#endregion //GetFieldRowColInMap

				// AS 4/27/09 TFS17122
				#region GetNavigationCell
		private static Cell GetNavigationCell(Field startingField, DataRecord record, bool includeCurrent, Field field)
		{
			if (field == null)
				return null;

			if (field == startingField && includeCurrent == false)
				return null;

			Debug.Assert(field.Owner == record.FieldLayout);

			Cell cell = record.Cells[field];

			if (null != cell && !cell.IsTabStop)
				return null;

			return cell;
		}
				#endregion //GetNavigationCell

				// AS 4/27/09 TFS17122
				#region GetNextPreviousCell
		private static Cell GetNextPreviousCell(GridFieldMap map, int row, int column, DataRecord record, Field startingField, bool includeCurrent, bool next, bool wrap)
		{
			Field[,] fields = map._map;
			Field[] navFields = map._navigationFields;
			int navFieldCount = navFields.Length;

			if (null == startingField)
			{
				Debug.Assert((row == 0 && column == 0) || (row == fields.GetLength(0) - 1 && column == fields.GetLength(1) - 1));
				Debug.Assert(includeCurrent, "We're not supposed to include the current but we don't have a starting field");

				// SSP 1/8/10 TFS26233
				// If navFields array is empty then return null since there's no next or prev
				// cell to navigate to.
				// 
				if ( 0 == navFieldCount )
					return null;

				if (next)
					startingField = navFields[0];
				else
					startingField = navFields[navFieldCount - 1];
			}

			int startingIndex = startingField.NavigationIndex;

			Cell cell = GetNextPreviousCellHelper(startingField, record, includeCurrent, navFields, next,
				startingIndex,
				next ? navFieldCount - startingIndex : startingIndex + 1);

			if (null == cell && wrap)
			{
				cell = GetNextPreviousCellHelper(startingField, record, includeCurrent, navFields, next,
					next ? 0 : navFieldCount - 1,
					next ? startingIndex - 1 : navFieldCount - startingIndex);
			}

			return cell;
		}
				#endregion //GetNextPreviousCell

				// AS 4/27/09 TFS17122
				#region GetNextPreviousCellHelper
		private static Cell GetNextPreviousCellHelper(Field startingField, DataRecord record, bool includeCurrent, Field[] navigationFields, bool next, int start, int count)
		{
			Debug.Assert(start >= 0 && start < navigationFields.Length);

			if (start < 0 || start >= navigationFields.Length)
				return null;

			for (int i = 0; i < count; i++)
			{
				int index = start + (next ? i : -i);

				Debug.Assert(index >= 0 && index < navigationFields.Length);

				Field field = navigationFields[index];

				Cell cell = GetNavigationCell(startingField, record, includeCurrent, field);

				// AS 10/13/09 NA 2010.1 - CardView
				// If the cell is collapsed then we need to skip it in the navigation.
				//
				//if (null != cell)
				if (null != cell && !record.ShouldCollapseCell(field))
					return cell;
			}

			return null;
		}
				#endregion //GetNextPreviousCellHelper

                // AS 2/27/09 TFS14730
                #region VerifyInitialized
        private void VerifyInitialized()
        {
            if (null == _map)
                this.Initialize();
        } 
                #endregion //VerifyInitialized

			#endregion Private Methods

		#endregion //Methods
	}

	#endregion //GridFieldMap Class

	#region NanToInfinityConverter
    
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)

	#endregion //NanToInfinityConverter

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