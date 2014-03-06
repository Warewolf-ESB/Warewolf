//#define DEBUG_MEASURE_ARRANGE


    


//#define DEBUG_MEASURE_ARRANGE_LAYOUT_COUNTS


    


using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using Infragistics.Windows.DataPresenter.Internal;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Reporting;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	#region VirtualizingDataRecordCellPanel Class

	/// <summary>
	/// A control that sits within a <see cref="DataRecordCellArea"/> and virtualizes the creation of the contained cells.
	/// </summary>
	//[ToolboxItem(false)]
	// AS 5/4/07 Not Panel
	// We derived from panel since it is the basic element type for arrangement and since it 
	// provided a UIElementsCollection but this actually causes perf issues because we are 
	// adding to the children from the arrange and the UIElementCollection add invalidates
	// the measure of the panel when a child is added.
	//
	//public class VirtualizingDataRecordCellPanel : Panel,
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class VirtualizingDataRecordCellPanel : FrameworkElement
        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
		//, IWeakEventListener
        // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
        , ILayoutContainer
		// AS 10/27/09 NA 2010.1 - CardView
		, IWeakEventListener
	{
		#region Member Variables







        private Control[] _cellElements;
		
		private int _allocatedCellCount;
		private bool _positionAllAllocatedCells;
		private Rect _lastClipRect;
        // AS 12/21/08 NA 2009 Vol 1 - Fixed Fields
		//private SizeHolder _extentSizeHolder;

		// AS 3/20/07 BR21313 
		private bool _isAutoFitInitialized;

		// AS 4/27/07
		private bool _cellElementsDirty = true;

		// AS 5/4/07 Not Panel
		private List<FrameworkElement> children = new List<FrameworkElement>();

		// AS 5/4/07 Optimization
		// Cache the record so we're not getting it from the DataContext dependency property.
		//
		// SSP 4/7/08 - Summaries Functionality
		// Changed to Record to support summary record.
		// 
		
		private Record _record;

		// JJD 5/4/07 - Optimization
		// Added support for label virtualization
		private bool _isHeader;
		private FieldLayout _fieldLayout;

		// AS 5/8/07 Optimization
		private bool _isVerifyingCells = false;

		// AS 5/24/07 Recycle elements
		private bool _isRecycling;

		// JJD 6/11/08 - BR31962
        // Needed flag so we only invalidate the measure in
        // OnLayoutUpdated once
		private bool _requiresAsynchMeasureInvalidation;


        // JJD 9/12/08
        // Added support for printing
        private ReportLayoutInfo _reportLayoutInfo;
        private double? _normalizedOffsetFromPanel;


        // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
        // If we don't have a layout manager for the record but we need a unique copy - 
        // e.g. we are sizing to content - then use this to cache that layout manager.
        //
        private FieldGridBagLayoutManager _recordLayoutManager;

        // cache the position at which we will arrange the cells and use this within the VerifyCellsInView
        private Rect[] _cellElementRects;
        private FieldRectsLayoutContainer _cellPresenterRectContainer;

        private FixedFieldSplitter _nearSplitter;
        private FixedFieldSplitter _farSplitter;
        private Vector _nearSplitterOffset;
        private Vector _farSplitterOffset;
        private Vector _fixedNearOffset;
        private Vector _fixedFarOffset;
        private Rect _scrollableClipRect;
        private bool _usingFixedInfo;
        private RecordPresenter _containingRp;
        private Rect _fixedNearRect;
        private Rect _fixedFarRect;
        private RecordCellAreaBase _containingCellArea;
        private Vector _fixedNearAdjustment;
        private Vector _fixedFarAdjustment;
        private double _prePostPanelSpacing;

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        // See the comment in TryGetPreferredExtent.
        //
        private GridBagLayoutItemDimensionsCollection _cachedDimensions;

        // AS 2/2/09 NA 2009 Vol 1 - Fixed Fields
        // Track the previous measure size so we can use this as the base size when 
        // calculating the size that results from a splitter drag operation.
        //
        private Size _lastMeasureSize;

        private static readonly Field[] EmptyFields = new Field[0];

        // AS 2/10/09
        // We need to cache the old virtualized fields/non virt fields in case 
        // we need to recreate the cell elements.
        //
        private int _templateCacheVersion;
        private IList<Field> _virtualizedFields = EmptyFields;
        private IList<Field> _nonVirtualizedFields = EmptyFields;

		// AS 7/7/09 TFS19145
		// We need to track the version of the generator.
		//
		private int _generatorTemplateVersion = -1;

		// AS 9/17/09 TFS22285
		private int _fieldsVersion = -1;

		// AS 7/21/09 NA 2009.2 Field Sizing
		private int _cellElementVersion;
		private int _lastAutoSizeElementVersion;

		// SSP 2/2/10
		// Added CellsInViewChanged event to the DataPresenterBase.
		// 
		private Rect _lastFieldsInViewClipRect;

		// MD 9/10/10 - TFS37596
		// We need to cache the last sizes used to measure the cell elements.
		private Size?[] _cellElementMeasureSizes;

		// JJD 3/11/11 - TFS67970 - Optimization
		private PropertyValueTracker _recordMouseOverTracker;

		// AS 11/29/10 TFS60418
		private Rect? _lastLayoutContainerRect;
		private int _lastLayoutContainerCount;

		// AS 3/14/11 TFS67970 - Optimization
		private WeakReference _lastLayoutManager;

		// AS 6/27/11 TFS79783
		internal bool IsPerformingAutoSizeMeasure;

		// AS 11/14/11 TFS91077
		private Size? _lastDPMeasureSize;

		#endregion //Member Variables

        #region Constructor

        static VirtualizingDataRecordCellPanel()
		{
			FrameworkElement.DataContextProperty.OverrideMetadata(typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDataContextChanged)));

            // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
            // Since the cell area may clip itself, requests by descendant elements to be brought
            // into view may actually try to bring them into view in the clipped area. To get 
            // around this we need to catch the request and alter it based on our clip.
            // 
            EventManager.RegisterClassHandler(typeof(VirtualizingDataRecordCellPanel), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
        }

		/// <summary>
		/// Initializes a new <see cref="VirtualizingDataRecordCellPanel"/>
		/// </summary>
		public VirtualizingDataRecordCellPanel()
		{
            // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
            // Optimization - instead of overriding OnPropertyChanged.
            //
            this.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (true.Equals(e.NewValue))
                    ((VirtualizingDataRecordCellPanel)sender).InvalidateArrange();
            };
		}

		#endregion //Constructor

		#region Properties

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        #region AutoFitHeight

        /// <summary>
        /// Identifies the <see cref="AutoFitHeight"/> dependency property
        /// </summary>
        private static readonly DependencyProperty AutoFitHeightProperty = DependencyProperty.Register("AutoFitHeight",
            typeof(double), typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAutoFitWidthHeightChanged)));

        private double AutoFitHeight
        {
            get
            {
                return (double)this.GetValue(VirtualizingDataRecordCellPanel.AutoFitHeightProperty);
            }
            set
            {
                this.SetValue(VirtualizingDataRecordCellPanel.AutoFitHeightProperty, value);
            }
        }
        #endregion //AutoFitHeight

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        #region AutoFitWidth

        /// <summary>
        /// Identifies the <see cref="AutoFitWidth"/> dependency property
        /// </summary>
        private static readonly DependencyProperty AutoFitWidthProperty = DependencyProperty.Register("AutoFitWidth",
            typeof(double), typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnAutoFitWidthHeightChanged)));

        private static void OnAutoFitWidthHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
			// AS 5/5/10 TFS29508
			// We only did this because we didn't want to remove the Math.Floor call in CalculateCellAreaAutoFitExtent.
			// Now that we removed that we can remove this adjustment. This adjustment was causing us to ignore small 
			// resize changed in the grid which led to the horizontal scrollbar showing up sometimes.
			//
			//// AS 6/11/09 TFS18382
			//if (GridUtilities.AreClose((double)e.NewValue, (double)e.OldValue, 1))
			if (GridUtilities.AreClose((double)e.NewValue, (double)e.OldValue))
			    return;

            VirtualizingDataRecordCellPanel cellPanel = (VirtualizingDataRecordCellPanel)d;
            DebugArrange(string.Format("Header={0}, Record={1}, Old={2}, New={3}", cellPanel._isHeader, cellPanel._record, e.OldValue, e.NewValue), e.Property.Name + " Changed");

			// AS 11/14/11 TFS91077
			if (null != cellPanel._fieldLayout)
				cellPanel._lastDPMeasureSize = cellPanel._fieldLayout.DataPresenter.LastMeasureSize;
			else
				cellPanel._lastDPMeasureSize = null;

			// JJD 4/21/11 - TFS73048 - Optimaztion - added
			// Instead of calling InvalidateMeasure and InvalidateArrange here, do it asynchronously
			// so we don't interrupt a layout updated pass.
			//cellPanel.InvalidateMeasure();
			//cellPanel.InvalidateArrange();
			// JJD 5/31/11 - TFS76852
			// Only call InvalidateMeasureAsynch if the dp is 
			// not a synchronuous control, i.e. it supports asynchrous processing.
			//GridUtilities.InvalidateMeasureAsynch(cellPanel);
			DataPresenterBase dp = cellPanel._fieldLayout != null ? cellPanel._fieldLayout.DataPresenter : null;

			if (dp == null || !dp.IsSynchronousControl)
				GridUtilities.InvalidateMeasureAsynch(cellPanel);
			else
			{
				cellPanel.InvalidateMeasure();
				cellPanel.InvalidateArrange();
			}

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            // Since the preferred height of a non-virtualized cell is based on the actual width 
            // that it occupies we need to dirty the cache held by the layout manager when the autofit
            // width changes.
            //
            cellPanel.InvalidateLayoutManager();
        }

        private double AutoFitWidth
        {
            get
            {
                return (double)this.GetValue(VirtualizingDataRecordCellPanel.AutoFitWidthProperty);
			}
            set
            {
                this.SetValue(VirtualizingDataRecordCellPanel.AutoFitWidthProperty, value);
            }
        }
        #endregion //AutoFitWidth

        // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
        #region LayoutManagerSize
        internal Size LayoutManagerSize
        {
            get
            {
                Size cellSize = this.RenderSize;
                bool isHorz = null != _fieldLayout && _fieldLayout.IsHorizontal;

                if (_nearSplitter != null)
                {
                    if (isHorz)
                        cellSize.Height -= _nearSplitter.DesiredSize.Height;
                    else
                        cellSize.Width -= _nearSplitter.DesiredSize.Width;
                }

                if (_farSplitter != null)
                {
                    if (isHorz)
                        cellSize.Height -= _farSplitter.DesiredSize.Height;
                    else
                        cellSize.Width -= _farSplitter.DesiredSize.Width;
                }

                return cellSize;
            }
        } 
        #endregion //LayoutManagerSize

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
        #region CellPresenterRectContainer
        private FieldRectsLayoutContainer CellPresenterRectContainer
        {
            get
            {
                if (null == this._cellPresenterRectContainer)
                    this._cellPresenterRectContainer = new FieldRectsLayoutContainer();

                return this._cellPresenterRectContainer;
            }
        } 
        #endregion //CellPresenterRectContainer

		// JM 01-20-09 NA 2009 Vol 1 - Fixed Fields
		#region FarSplitter

		internal FixedFieldSplitter FarSplitter
		{
			get { return this._farSplitter; }
		}

		#endregion //FarSplitter

		#region FieldLayout

		
		
		
		/// <summary>
		/// Returns the associated field layout.
		/// </summary>
		internal FieldLayout FieldLayout
		{
			get
			{
				return _fieldLayout;
			}
		}

		#endregion // FieldLayout

        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedFieldExtent

        internal static readonly DependencyProperty FixedFieldExtentProperty = DependencyProperty.Register("FixedFieldExtent",
            typeof(double), typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

        #endregion //FixedFieldExtent

        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedFieldInfo

        internal static readonly DependencyProperty FixedFieldInfoProperty = DependencyProperty.Register("FixedFieldInfo",
            typeof(FixedFieldInfo) , typeof(VirtualizingDataRecordCellPanel),  new FrameworkPropertyMetadata(null, OnFixedFieldInfoChanged));

        private static void OnFixedFieldInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingDataRecordCellPanel panel = (VirtualizingDataRecordCellPanel)d;

            if (panel != null)
            {
                FixedFieldInfo ffi = (FixedFieldInfo)e.NewValue;
                FixedFieldInfo oldffi = (FixedFieldInfo)e.OldValue;

                if (null != ffi)
                {
                    panel.SetBinding(FixedFieldOffsetProperty, Utilities.CreateBindingObject(FixedFieldInfo.OffsetProperty, BindingMode.OneWay, ffi));
                    panel.SetBinding(FixedFieldExtentProperty, Utilities.CreateBindingObject(FixedFieldInfo.ExtentProperty, BindingMode.OneWay, ffi));
                    panel.SetBinding(FixedFieldViewportExtentProperty, Utilities.CreateBindingObject(FixedFieldInfo.ViewportExtentProperty, BindingMode.OneWay, ffi));

                    // AS 1/27/09
                    // Store references to the panel on the fixed field info so it 
                    // can request the fixed area when paging.
                    //
                    ffi.AddPanel(panel);
                }
                else
                {
                    BindingOperations.ClearBinding(panel, FixedFieldOffsetProperty);
                    BindingOperations.ClearBinding(panel, FixedFieldExtentProperty);
                    BindingOperations.ClearBinding(panel, FixedFieldViewportExtentProperty);
                }

                // AS 1/27/09
                // Unregister with the previous panel.
                //
                if (null != oldffi)
                    oldffi.RemovePanel(panel);

                panel.VerifyFixedFieldOffsets();
            }
        }

        #endregion //FixedFieldInfo

        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedFieldOffset

        internal static readonly DependencyProperty FixedFieldOffsetProperty = DependencyProperty.Register("FixedFieldOffset",
            typeof(double), typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

        private static void OnFixedFieldOffsetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingDataRecordCellPanel panel = (VirtualizingDataRecordCellPanel)d;
            panel.VerifyFixedFieldOffsets();
        }

        #endregion //FixedFieldOffset

        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
        #region FixedFieldViewportExtent

        internal static readonly DependencyProperty FixedFieldViewportExtentProperty = DependencyProperty.Register("FixedFieldViewportExtent",
            typeof(double), typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

        #endregion //FixedFieldViewportExtent

        // AS 2/18/09
        // There are a couple of cases where we want to make sure we use the same layout
        // manager for the heights of all records.
        //
        #region ForceUseCellLayoutManager
        private bool ForceUseCellLayoutManager
        {
            get
            {
                if (null != _fieldLayout
                    && _isHeader == false
                    && _record is DataRecord
                    && !(_record is FilterRecord))
                {
                    switch (_fieldLayout.DataRecordSizingModeResolved)
                    {
                        case DataRecordSizingMode.SizableSynchronized:
                        case DataRecordSizingMode.Fixed:
                            // always use a shared size
                            return true;
                        case DataRecordSizingMode.IndividuallySizable:
                            // unless the record has been resized we want to use the shared size
                            return _record.GetLayoutManager(false) == null;
                    }
                }

                return false;
            }
        }
        #endregion //ForceUseCellLayoutManager

        #region GridColumnWidthVersion

		internal static readonly DependencyProperty GridColumnWidthVersionProperty = FieldLayout.GridColumnWidthVersionProperty.AddOwner(typeof(VirtualizingDataRecordCellPanel),
            // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
            // Optimization - instead of overriding OnPropertyChanged.
            //
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnGridColumnWidthVersionChanged)));

        private static void OnGridColumnWidthVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingDataRecordCellPanel panel = (VirtualizingDataRecordCellPanel)d;

			// AS 3/2/11 TFS66974
			Debug.Assert(!panel._isVerifyingCells, "The layout is being dirtied while verifying?");

            // AS 12/3/08 TFS11189
            // We need to always invalidate the measure since the layout could be horizontal
            // but its size be based on the CellAreaManager in which case the width changing
            // could affect the width of the record.
            //
            //if (null != this._fieldLayout && (this._fieldLayout.IsHorizontal == false || this._fieldLayout.DataRecordSizingModeResolved == DataRecordSizingMode.SizableSynchronized))
			if (null != panel._fieldLayout)
			{
				// JJD 4/21/11 - TFS73048 - Optimaztion - added
				// Instead of calling InvalidateMeasure here, do it asynchronously
				// so we don't interrupt a layout updated pass.
				//panel.InvalidateMeasure();
				// JJD 5/31/11 - TFS76852
				// Only call InvalidateMeasureAsynch if the dp is 
				// not a synchronuous control, i.e. it supports asynchrous processing.
				//GridUtilities.InvalidateMeasureAsynch(panel);
				DataPresenterBase dp = panel._fieldLayout.DataPresenter;

				if (dp == null || !dp.IsSynchronousControl)
					GridUtilities.InvalidateMeasureAsynch(panel);
				else
					panel.InvalidateMeasure();
			}

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            // This was previously only dirtying the layout of the panel but I changed this
            // to a helper method that would also dirty the layout of the record if it had one.
            //
            panel.InvalidateLayoutManager();
        }

		internal int GridColumnWidthVersion
		{
			get
			{
				return (int)this.GetValue(VirtualizingDataRecordCellPanel.GridColumnWidthVersionProperty);
			}
			set
			{
				this.SetValue(VirtualizingDataRecordCellPanel.GridColumnWidthVersionProperty, value);
			}
		}

		#endregion //GridColumnWidthVersion

		// AS 11/14/11 TFS91077
		#region GridMeasureVersion
		private static DependencyProperty GridMeasureVersionProperty = DataPresenterBase.GridMeasureVersionProperty.AddOwner(typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnGridMeasureVersionChanged)));

		private static void OnGridMeasureVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var cellPanel = d as VirtualizingDataRecordCellPanel;
			cellPanel.OnGridMeasureVersionChanged();
		}

		private void OnGridMeasureVersionChanged()
		{
			// since the recordpresenter is measured with infinity (e.g. width for a vertically arranged gridviewpanel) the VDRCP 
			// won't get measured until 1 full measure/arrange pass after the datapresenter and gridviewpanel have been measured and 
			// arranged. at that time the updatelayout will be invoked and the recordpresenter will update its autofitcellareawidth 
			// at which point the VDRCP will be notified. however since we asynchronously invalidate our measure you will see the 
			// scrollbar show up temporarily. in order to address that we need to proactively invalidate the measure of this panel 
			// and everything up to the root panel. in this way when the gridviewpanel measures the child records the VDRCP's 
			// measure will be invoked and the gridviewpanel will get to the see the desired size of the recordpresenter which we 
			// are now (as part of this fix) adjusting the autofitwidth|height using the delta from the available size when the dp 
			// was measured when we last got the autofitwidth|height set and what it is currently.
			if (_lastDPMeasureSize != null && _fieldLayout != null && _fieldLayout.CellPresentation == CellPresentation.GridView)
			{
				var dp = _fieldLayout.DataPresenter;

				if (null != dp && dp.CurrentViewInternal.AutoFitToRecord == false)
				{
					bool invalidateMeasure = false;

					if (!double.IsNaN(this.AutoFitWidth) && !CoreUtilities.AreClose(_lastDPMeasureSize.Value.Width, dp.LastMeasureSize.Width))
						invalidateMeasure = true;
					else if (!double.IsNaN(this.AutoFitHeight) && !CoreUtilities.AreClose(_lastDPMeasureSize.Value.Height, dp.LastMeasureSize.Height))
						invalidateMeasure = true;

					if (invalidateMeasure)
					{
						Utilities.InvalidateMeasure(this, dp.CurrentPanel);
						this.InvalidateMeasure();
					}
				}
			}
		} 
		#endregion //GridMeasureVersion

		#region GridRowHeightVersion

        
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		#endregion //GridRowHeightVersion
		
		// JJD 08/15/12 - TFS119037 - added
		#region IsFieldLayoutDisposed

		private bool IsFieldLayoutDisposed
		{
			get { return _fieldLayout != null && _fieldLayout.WasRemovedFromCollection; }
		}

		#endregion //IsFieldLayoutDisposed	
    
		#region IsHeaderArea

		
		
		/// <summary>
		/// Returns true if this panel displays headers (in the mode where headers and cells are separate).
		/// </summary>
		internal bool IsHeaderArea
		{
			get
			{
				return _isHeader;
			}
		}

		#endregion // IsHeaderArea

		// JJD 3/11/11 - TFS67970 - Optimization
		#region IsMouseOverCellArea

		private bool? IsMouseOverCellArea
		{
			get
			{
				// JJD 3/11/11 - TFS67970 - Optimization
				// If this is not a header and it is a data record then get the IsMouseOver state
				if (this._containingCellArea != null &&
					_isHeader == false &&
					_record is DataRecord)
				{
					return _containingCellArea.IsMouseOver;
				}

				return null;
			}
		}

		#endregion //IsMouseOverCellArea	
    
        // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
        #region IsNestedFixedHeader
        private bool IsNestedFixedHeader
        {
            get
            {
                return _isHeader &&
                    null != _containingRp &&
                    _containingRp.Record != null &&
                    _containingRp.Record.ParentRecord != null;
            }
        }
        #endregion //IsNestedFixedHeader

		// AS 3/14/11 TFS67970 - Optimization
		#region LastLayoutManager
		internal FieldGridBagLayoutManager LastLayoutManager
		{
			get { return Utilities.GetWeakReferenceTargetSafe(_lastLayoutManager) as FieldGridBagLayoutManager; }
			private set
			{
				if (this.LastLayoutManager != value)
					_lastLayoutManager = new WeakReference(value);
			}
		}
		#endregion //LastLayoutManager

		// JM 01-20-09 NA 2009 Vol 1 - Fixed Fields
		#region NearSplitter

		internal FixedFieldSplitter NearSplitter
		{
			get { return this._nearSplitter; }
		}

		#endregion //NearSplitter

		// JJD 5/4/07 - Optimization
		// Added support for label virtualization
		#region NonVirtualizedFields
		internal IList<Field> NonVirtualizedFields
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

                this.VerifyFieldLists();
                return _nonVirtualizedFields;
			}
		}
		#endregion //NonVirtualizedFields

		#region Record

		
		
		/// <summary>
		/// Returns the associated record.
		/// </summary>
		internal Record Record
		{
			get
			{
				return _record;
			}
		}

		#endregion // Record

        // AS 2/13/09 TFS13982
        #region ScrollableAreaRect
        internal Rect ScrollableAreaRect
        {
            get 
            {
                Debug.Assert(this.IsArrangeValid, "The scrollable rect may be invalid. It was meant to be used after the layout was complete - e.g. in the splitter.");
                return _scrollableClipRect; 
            }
        } 
        #endregion //ScrollableAreaRect

		#region ScrollVersion

		internal static readonly DependencyProperty ScrollVersionProperty = DependencyProperty.Register("ScrollVersion",
			typeof(int), typeof(VirtualizingDataRecordCellPanel), new FrameworkPropertyMetadata((int)0,
            // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
            // Optimization - instead of overriding OnPropertyChanged.
            //
            new PropertyChangedCallback(OnScrollVersionChanged)
            ));

        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
        private static void OnScrollVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingDataRecordCellPanel panel = (VirtualizingDataRecordCellPanel)d;

			// JJD 08/15/12 - TFS119037
			// Verify that the fieldlayout is still valid. If not return.
			// This prevents causing the TemplateDataRecordCache for an old (disposed)
			// FieldLayout from getting re-initialized and added to the logical tree
			// of the DP which will leak memory since it will never be removed.
			if (panel.IsFieldLayoutDisposed)
				return;

			// AS 4/27/07
			// We need to dirty it even if no cells were allocated because
			// the change in scroll position could result in the record
			// being brought into view and then it needs to have its cells 
			// allocated.
			//
			//if (this._cellElements != null &&
			//	this._cellElements.Length > this._allocatedCellCount)
			if (panel._cellElementsDirty == false)
			{
                // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
                // Previously we were not binding this property if we had no
                // virtualizable fields but we also need it if we have fixed 
                // info.
                //
				// AS 10/26/10 TFS58064
				//if (panel._usingFixedInfo == false && panel.VirtualizedFields.Count == 0)
				if (panel._usingFixedInfo == false)
				{
					IList<Field> fields = panel.VirtualizedFields;

					if (null == fields || fields.Count == 0)
						return;
				}

				// AS 6/4/07
				// Do not invalidate our arrange if the record is in the process of being
				// collapsed.
				//
                if (panel._isHeader == true || (panel._record != null && panel._record.HasCollapsedAncestor == false))
                {
                    // AS 2/23/09 Optimization
                    // If a scroll (or change in the viewable records) has occured then we only need 
                    // to worry about the clip rect changing so we can hook the layout updated and check
                    // it there invalidating the arrange if needed.
                    //
                    //panel.InvalidateArrange();
                    panel.WireLayouUpdatedEvent();
                }
			}
        }






		internal int ScrollVersion
		{
			get
			{
				return (int)this.GetValue(VirtualizingDataRecordCellPanel.ScrollVersionProperty);
			}
			set
			{
				this.SetValue(VirtualizingDataRecordCellPanel.ScrollVersionProperty, value);
			}
		}


		#endregion //ScrollVersion

		#region TemplateVersion

		internal static readonly DependencyProperty TemplateVersionProperty = FieldLayout.TemplateVersionProperty.AddOwner(typeof(VirtualizingDataRecordCellPanel), 
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnTemplateVersionChanged)));

        private static void OnTemplateVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
			// JJD 4/21/11 - TFS73048 - Optimaztion - added
			// Instead of calling InvalidateMeasure here, do it asynchronously
			// so we don't interrupt a layout updated pass.
			//((VirtualizingDataRecordCellPanel)d).InvalidateMeasure();
			// JJD 5/31/11 - TFS76852
			// Only call InvalidateMeasureAsynch if the dp is 
			// not a synchronuous control, i.e. it supports asynchrous processing.
			//GridUtilities.InvalidateMeasureAsynch((UIElement)d);
			VirtualizingDataRecordCellPanel panel = d as VirtualizingDataRecordCellPanel;
			DataPresenterBase dp = panel._fieldLayout != null ? panel._fieldLayout.DataPresenter : null;

			if (dp == null || !dp.IsSynchronousControl)
				GridUtilities.InvalidateMeasureAsynch(panel);
			else
				panel.InvalidateMeasure();
		}

		internal int TemplateVersion
		{
			get
			{
				return (int)this.GetValue(VirtualizingDataRecordCellPanel.TemplateVersionProperty);
			}
			set
			{
				this.SetValue(VirtualizingDataRecordCellPanel.TemplateVersionProperty, value);
			}
		}

		#endregion //TemplateVersion

		// JJD 5/4/07 - Optimization
		// Added support for label virtualization
		#region VirtualizedFields
		internal IList<Field> VirtualizedFields
		{
			get
			{
                
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

                this.VerifyFieldLists();
                return _virtualizedFields;
			}
		}
		#endregion //VirtualizedFields

		// JJD 5/7/07 - Optimization
		// Added support for label virtualization
		#region VirtualizedFieldsToMeasureinHeader
        
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		#endregion //VirtualizedFieldsToMeasureinHeader

		#endregion //Properties

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			
			// Throwing asserts causes issues with blend. So only do so when not in design mode.
			
			





			// JJD 08/15/12 - TFS119037 
			// Verify that the fieldlayout is still valid. If not return the cached final.
			// This prevents causing the TemplateDataRecordCache for an old (disposed)
			// FieldLayout from getting re-initialized and added to the logical tree
			// of the DP which will leak memory since it will never be removed.
			if (this.IsFieldLayoutDisposed)
				return finalSize;





			if (null != this._fieldLayout)
			{
				// AS 3/2/11 TFS66974
				this.VerifyPropertyDescriptorProvider();

				// AS 4/27/07
				if (this._cellElementsDirty)
					this.VerifyCellElements();

                #region Obsolete
                
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)



















































#endregion // Infragistics Source Cleanup (Region)

                #endregion //Obsolete
                // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
                //
                FieldGridBagLayoutManager lm = this.GetLayoutManager();

				// AS 3/14/11 TFS67970 - Optimization
				this.LastLayoutManager = lm;

                bool isHorizontal = _fieldLayout.IsHorizontal;
                FixedFieldLayoutInfo fixedFieldInfo = _fieldLayout.GetFixedFieldInfo(false);
                bool useCellPresenters = this._fieldLayout != null && this._fieldLayout.UseCellPresenters;

                #region Adjust Size For Splitters

                Size cellSize = finalSize;

                Size nearSplitSize = _nearSplitter != null ? _nearSplitter.DesiredSize : new Size();
                Size farSplitSize = _farSplitter != null ? _farSplitter.DesiredSize : new Size();
                double splitterExtent;

                if (isHorizontal)
                {
                    splitterExtent = nearSplitSize.Height + farSplitSize.Height;
                    cellSize.Height -= splitterExtent;
                }
                else
                {
                    splitterExtent = nearSplitSize.Width + farSplitSize.Width;
                    cellSize.Width -= splitterExtent; 
                }

                #endregion //Adjust Size For Splitters

                object rectContext = new Rect(cellSize);

                IList<Field> nonVirtFields = this.NonVirtualizedFields;

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                if (!isHorizontal && null != nonVirtFields && nonVirtFields.Count > 0)
                {
					// AS 11/14/11 TFS91077
					//Size autoFitSize = new Size(this.AutoFitWidth, this.AutoFitHeight);
					Size autoFitSize = this.GetAutoFitSize();
                    Size templateGridSize = _fieldLayout.TemplateDataRecordCache.TemplateGridSize;

                    this.InitializeCachedDimensions(ref finalSize, splitterExtent, ref autoFitSize, ref templateGridSize);
                }

                lm.InitializePanelReference(this);

                DebugArrange(string.Format("Header={1}, Record={2}, FinalSize={0}", finalSize, this._isHeader, this._record), "Arrange [Start]");

                // calculate the position of all the cell/header elements
                if (useCellPresenters)
                {
                    this.CellPresenterRectContainer.Reset(
                        _fixedNearOffset + _fixedNearAdjustment,                        // near
                        _nearSplitterOffset,                                            // scrollable
                        _fixedFarOffset + _farSplitterOffset + _fixedFarAdjustment);    // far

                    // calculate and merge cell presenter rects
                    lm.CalculateCellPresenterRects(this.CellPresenterRectContainer, cellSize, this._cellElementRects);
                }
                else
                {
					// AS 11/29/10 TFS60418
					// There are a couple of optimizations here. First, if the layout is still 
					// valid then we don't need to update the cell rects. Second, we can get the 
					// layout manager to hold and cache the dimensions and then update the rects 
					// ourselves simulating the layoutcontainer which would have recalculated 
					// the layout.
					// 
                    //lm.LayoutContainer(this, rectContext);
					if (_lastLayoutContainerCount != lm.InvalidateLayoutCount ||
						_lastLayoutContainerRect != (Rect)rectContext)
					{
						_lastLayoutContainerCount = lm.InvalidateLayoutCount;
						_lastLayoutContainerRect = (Rect)rectContext;
						//lm.LayoutContainer(this, rectContext);
						var cachedDims = lm.GetLayoutItemDimensionsCached(this, rectContext);

						foreach (ILayoutItem item in lm.LayoutItems)
						{
							GridBagLayoutItemDimensions dim = cachedDims[item];

							if (dim != null)
								((ILayoutContainer)this).PositionItem(item, dim.Bounds, null);
						}
					}
				}

                // get the actual dimensions used for the arrange
                GridBagLayoutItemDimensionsCollection dims = lm.GetLayoutItemDimensionsCached(CalcSizeLayoutContainer.Instance, rectContext);

                lm.InitializePanelReference(null);

                _cachedDimensions = null;

                // AS 2/25/09 TFS14598
                // Cache the previous adjustments before we calculate the new ones. If they 
                // change then we may need to adjust the cell rects.
                //
                Vector lastNearAdjustment = _fixedNearAdjustment;
                Vector lastFarAdjustment = _fixedFarAdjustment;

                double nearFixedExtent = 0;
                double farFixedExtent = 0;
                _fixedNearAdjustment = new Vector();
                _fixedFarAdjustment = new Vector();

                if (_usingFixedInfo)
                {
                    #region Refactored
                    
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

                    #endregion //Refactored
                    double scrollableAreaExtent;
                    int factor = CalculateFixedExtents(isHorizontal, fixedFieldInfo, 
                        ref nearSplitSize, ref farSplitSize, dims, 
                        out nearFixedExtent, out farFixedExtent, out scrollableAreaExtent);

                    
                    //      if there is not enough room

                    // This used to be done before positioning the splitter but since we are 
                    // adjusting/calculating the near adjustment, we need to do this first.
                    //
                    #region Update ClipRect

                    Rect scrollableClipRect = new Rect();
                    double offset = (double)this.GetValue(FixedFieldOffsetProperty);

                    // AS 2/2/09
                    // Moved the calculation for the scrollable extent into a helper
                    // method to use when calculating a splitter layout.
                    //
                    double scrollableExtent = CalculateScrollableExtent(ref finalSize, isHorizontal, nearFixedExtent, farFixedExtent, offset);

                    // AS 2/3/09
                    // If the fixed area plus the area around the cell panel exceed
                    // the available scrolling space then we need to reduce the scrollable 
                    // extent we calculate and adjust the _fixedFarAdjustment to move any 
                    // far fixed elements back to the right. Essentially when there is not
                    // enough room to show all the fixed elements, we're going to favor the 
                    // near side as we did in the WinGrid.
                    //
                    double possibleScrollableExtent = _prePostPanelSpacing + nearFixedExtent + farFixedExtent - (double)this.GetValue(FixedFieldViewportExtentProperty);

                    if (possibleScrollableExtent > 0)
                    {
                        scrollableExtent += possibleScrollableExtent;

                        if (isHorizontal)
                            _fixedFarAdjustment.Y = possibleScrollableExtent;
                        else
                            _fixedFarAdjustment.X = possibleScrollableExtent;
                    }

                    if (isHorizontal)
                    {
                        scrollableClipRect.Width = finalSize.Width;
                        scrollableClipRect.Y = nearFixedExtent + offset;
                        scrollableClipRect.Height = Math.Max(scrollableExtent, 0);

                        // when the far & near areas overlap we need to shift the 
                        // near elements back to the left and start scrolling the 
                        // record cell area.
                        _fixedNearAdjustment.Y = Math.Min(0, scrollableExtent);
                    }
                    else
                    {
                        scrollableClipRect.Height = finalSize.Height;
                        scrollableClipRect.X = nearFixedExtent + offset;
                        scrollableClipRect.Width = Math.Max(scrollableExtent, 0);

                        // when the far & near areas overlap we need to shift the 
                        // near elements back to the left and start scrolling the 
                        // record cell area.
                        _fixedNearAdjustment.X = Math.Min(0, scrollableExtent);
                    }

                    _scrollableClipRect = scrollableClipRect;

                    _fixedNearRect = new Rect(
                        isHorizontal ? 0 : offset, 
                        !isHorizontal ? 0 : offset, 
                        isHorizontal ? finalSize.Width : nearFixedExtent, 
                        isHorizontal ? nearFixedExtent : finalSize.Height);
                    _fixedFarRect = new Rect(
                        isHorizontal ? 0 : _scrollableClipRect.Right, 
                        !isHorizontal ? 0 : _scrollableClipRect.Bottom, 
                        isHorizontal ? finalSize.Width : farFixedExtent, 
                        isHorizontal ? farFixedExtent : finalSize.Height);

                    _fixedNearRect.Offset(_fixedNearAdjustment);
                    _fixedFarRect.Offset(_fixedFarAdjustment);

                    #endregion //Update ClipRect

                    #region Position Splitters
                    if (null != _nearSplitter)
                    {
                        Rect splitRect = new Rect(_nearSplitter.DesiredSize);
                        int splitOrigin = factor * (fixedFieldInfo.NearFixedOrigin + fixedFieldInfo.NearFixedSpan);

                        if (isHorizontal)
                        {
                            splitRect.Width = finalSize.Width;

                            if (splitOrigin < dims.RowDims.Length)
                                splitRect.Y = dims.RowDims[splitOrigin];
                        }
                        else
                        {
                            splitRect.Height = finalSize.Height;

                            if (splitOrigin < dims.ColumnDims.Length)
                                splitRect.X = dims.ColumnDims[splitOrigin];
                        }

                        splitRect.Offset(_fixedNearOffset + _fixedNearAdjustment);
                        _nearSplitter.Arrange(splitRect);
                    }

                    if (null != _farSplitter)
                    {
                        Rect splitRect = new Rect(_farSplitter.DesiredSize);
                        int splitOrigin = factor * fixedFieldInfo.FarFixedOrigin;

                        if (isHorizontal)
                        {
                            splitRect.Width = finalSize.Width;

                            if (splitOrigin < dims.RowDims.Length)
                                splitRect.Y = dims.RowDims[splitOrigin];
                        }
                        else
                        {
                            splitRect.Height = finalSize.Height;

                            if (splitOrigin < dims.ColumnDims.Length)
                                splitRect.X = dims.ColumnDims[splitOrigin];
                        }

                        splitRect.Offset(_fixedFarOffset + _fixedFarAdjustment);
                        splitRect.Offset(_nearSplitterOffset);

                        _farSplitter.Arrange(splitRect);
                    }
                    #endregion //Position Splitters
                }

				// JJD 3/11/11 - TFS67970 - Optimization - added
				// Also check the _usingFixedInfo flag since we are always caching 
				// the cell area 
				//if (null != _containingCellArea)
				if (null != _containingCellArea && _usingFixedInfo)
                {
                    _containingCellArea.RenderOffset = _fixedNearAdjustment;
                    _containingCellArea.MinCellPanelExtent = nearFixedExtent + farFixedExtent;
                }

                // AS 2/25/09 TFS14598
                // We used the old _fixedFarAdjustment & _fixedNearAdjustment when 
                // calculating/storing the element rects so if those adjustments have 
                // changed then we need to fix up the element rects by the delta.
                //
                Vector farAdjustmentDelta = _fixedFarAdjustment - lastFarAdjustment;
                Vector nearAdjustmentDelta = _fixedNearAdjustment - lastNearAdjustment;
                this.AdjustCellRects(fixedFieldInfo.NearFixedFields, nearAdjustmentDelta);
                this.AdjustCellRects(fixedFieldInfo.FarFixedFields, farAdjustmentDelta);

                #region Position Non-Virtualized Cells

                // now position the non virtualized cells
                if (null != nonVirtFields)
                {
                    for (int i = 0, count = nonVirtFields.Count; i < count; i++)
                    {
                        int cellIndex = nonVirtFields[i].TemplateCellIndex;

                        FrameworkElement el = this._cellElements[cellIndex];

                        // it could be null if we're using labelonly/cellonly 
                        // content style
                        if (null != el)
                        {
                            if (el is CellPresenterBase)
                                this.InitializeCellPresenterRects(el);

							// MD 9/10/10 - TFS37596
							// The arrange helper needs to know the last size we measured the cell with. If we didn't measure the cell yet, 
							// pass in the element rect's size and cache that as the last measured size.
                            //this.ArrangeHelper(el, this._cellElementRects[cellIndex]);
							Rect elementRect = _cellElementRects[cellIndex];
							Size? lastMeasureSize = _cellElementMeasureSizes[cellIndex];

							if (lastMeasureSize.HasValue == false)
							{
								lastMeasureSize = elementRect.Size;
								_cellElementMeasureSizes[cellIndex] = lastMeasureSize;
							}

							this.ArrangeHelper(el, elementRect, lastMeasureSize.Value, cellIndex);
                        }
                    }
                } 
                #endregion //Position Non-Virtualized Cells

                // then verify the virtual cells in view are created/arranged
                this.VerifyCellsInView(finalSize);

                Size minSize = lm.CalculateMinimumSize();

                // the splitter(s) would be additional minimum size
                if (isHorizontal)
                    minSize.Height += nearSplitSize.Height + farSplitSize.Height;
                else
                    minSize.Width += nearSplitSize.Width + farSplitSize.Width; 

                Size arrangeSize = finalSize;

                if (arrangeSize.Width < minSize.Width)
                    arrangeSize.Width = minSize.Width;

                if (arrangeSize.Height < minSize.Height)
                    arrangeSize.Height = minSize.Height;

                DebugArrange(string.Format("Header={1}, Record={2}, FinalSize={0}, ArrangeSize={3}", finalSize, this._isHeader, this._record, arrangeSize), "Arrange [End]");

				// AS 7/21/09 NA 2009.2 Field Sizing
				this.VerifyAutoSizeFields();

                return arrangeSize;
			}

			return base.ArrangeOverride(finalSize);
		}

        #endregion //ArrangeOverride

		// AS 5/4/07 Not Panel
		#region GetVisualChild
		/// <summary>
		/// Gets the cell element at the specified index.
		/// </summary>
		/// <param name="index">Index of the element in the </param>
		/// <returns>The child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
            // AS 2/3/09 NA 2009 Vol 1 - Fixed Fields
            // Pulled the splitter out of the children so we can keep 
            // them on top of the zorder.
            //
            //return this.children[index];
            if (index < this.children.Count)
    			return this.children[index];

            index -= this.children.Count;

            if (_nearSplitter != null)
            {
                if (index == 0)
                    return _nearSplitter;

                index--;
            }

            if (_farSplitter != null)
            {
                if (index == 0)
                    return _farSplitter;
            }

            throw new ArgumentOutOfRangeException();
		} 
		#endregion //GetVisualChild

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			// JJD 08/15/12 - TFS119037 
			// Verify that the fieldlayout is still valid. If not return the cached DesiredSize.
			// This prevents causing the TemplateDataRecordCache for an old (disposed)
			// FieldLayout from getting re-initialized and added to the logical tree
			// of the DP which will leak memory since it will never be removed.
			if (this.IsFieldLayoutDisposed)
				return this.DesiredSize;

            // AS 2/2/09 
            // Cache the measure size so we can use it calculate the results of a 
            // splitter layout drag operation.
            //
            _lastMeasureSize = availableSize;

			
			// Throwing asserts causes issues with blend. So only do so when not in design mode.
			
			










            if (null != this._fieldLayout)
            {
				// AS 3/2/11 TFS66974
				this.VerifyPropertyDescriptorProvider();

				// AS 7/1/09 NA 2009.2 Field Sizing
				_fieldLayout.AutoSizeInfo.Verify();

                // AS 2/10/09
                // If the virtualized/nonvirtualized field lists have changed then we want to
                // release the cell elements.
                //
                this.VerifyFieldLists();


                // JJD 9/19/08 - added support for printing
                this.RefreshReportLayoutInfo();


                // AS 4/27/07
				if (this._cellElementsDirty)
					this.VerifyCellElements();

				// AS 3/20/07 BR21313
				if (this._isAutoFitInitialized == false)
					this.InitializeAutoFitBindings();

                #region Obsolete
                
#region Infragistics Source Cleanup (Region)































































































#endregion // Infragistics Source Cleanup (Region)


					
#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)


















































































































































































#endregion // Infragistics Source Cleanup (Region)

                #endregion //Obsolete
                // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
                FieldGridBagLayoutManager lm = this.GetLayoutManager();

                Debug.Assert(null != lm);

                if (null == lm)
                    return new Size(1, 1);

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                // This block was moved up from below because we need the extents
                // before we get the cached dimensions so we can properly calculate
                // the rect available for the layout manager.
                //
                #region FixedFieldSplitter

                // create/remove the fixed splitters
                this.VerifyFixedState();

                double splitterExtent = 0;
                bool isHorizontal = _fieldLayout.IsHorizontal;

                _nearSplitterOffset = new Vector();
                _farSplitterOffset = new Vector();

                if (null != _nearSplitter)
                {
                    _nearSplitter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Size splitterSize = _nearSplitter.DesiredSize;
                    splitterExtent += isHorizontal ? splitterSize.Height : splitterSize.Width;

                    if (isHorizontal)
                        _nearSplitterOffset.Y = splitterExtent;
                    else
                        _nearSplitterOffset.X = splitterExtent;
                }

                if (null != _farSplitter)
                {
                    _farSplitter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Size splitterSize = _farSplitter.DesiredSize;
                    splitterExtent += isHorizontal ? splitterSize.Height : splitterSize.Width;

                    // note these intentionally include the near splitter extent
                    if (isHorizontal)
                        _farSplitterOffset.Y = splitterExtent;
                    else
                        _farSplitterOffset.X = splitterExtent;
                } 
                #endregion //FixedFieldSplitter

				// AS 11/14/11 TFS91077
				//Size autoFitSize = new Size(this.AutoFitWidth, this.AutoFitHeight);
				Size autoFitSize = this.GetAutoFitSize();

                DebugArrange(string.Format("Header={1}, Record={2}, Available={0}, AutoFitSize={3}", availableSize, this._isHeader, this._record, autoFitSize), "Measure [Start]");

                // if we're not autosizing then we want to use the template grid size as if we're autosizing
				TemplateDataRecordCache templateRecordCache = this._fieldLayout.TemplateDataRecordCache;
                Size templateGridSize = templateRecordCache.TemplateGridSize;

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                // See the comment in TryGetPreferredExtent.
                //
                IList<Field> nonVirtFields = this.NonVirtualizedFields;

                if (!isHorizontal && null != nonVirtFields && nonVirtFields.Count > 0)
                    InitializeCachedDimensions(ref availableSize, splitterExtent, ref autoFitSize, ref templateGridSize);

                lm.InitializePanelReference(this);

                Size size = lm.CalculatePreferredSize();
                Size minSize = lm.CalculateMinimumSize();
                Size maxSize = lm.CalculateMaximumSize();

                lm.InitializePanelReference(null);

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                _cachedDimensions = null;

                // AS 2/18/09
                // If the record sizing mode is fixed/individually sizable then we don't want to
                // use the size of the records layoutmanager or else records could be sized
                // differently in these modes.
                //
                if (this.ForceUseCellLayoutManager)
                {
					// AS 1/25/10 SizableSynchronized changes
					// If we're supporting empty cells then we need to use the record's layout manager.
					//
					if (!_fieldLayout.IsEmptyCellCollapsingSupportedByView || !_record.ShouldCollapseEmptyCellsResolved)
					{
						FieldGridBagLayoutManager lmRecord = _fieldLayout.CellLayoutManager;
						size = lmRecord.CalculatePreferredSize();
					}
                }

                // AS 2/2/09
                // Moved code into a helper method.
                //
                AdjustForSplitter(splitterExtent, isHorizontal, ref size, ref minSize, ref maxSize);

                DebugArrange(string.Format("Size={0}, Min={1}, Max={2}", size, minSize, maxSize), "Measure LayoutManager");

                // Adjust desired size based on constraints/templategrid size
                EnforceMinMax(ref availableSize, ref autoFitSize, ref templateGridSize, ref size, minSize, maxSize, _fieldLayout );

                DebugArrange(size, "Measure Size Returned");

                DebugArrange(string.Format("Available={0}, Header={1}, Record={2}", availableSize, this._isHeader, this._record), "Measure [End]");


                // JJD 9/19/08 - added print support
                // If we are in a report with cell page spanning logic then use the
                // ReportLayoutInfo's calulated extent instead
                if (this._reportLayoutInfo != null)
                {
                    if (isHorizontal)
                        size.Height = Math.Max(size.Height, this._reportLayoutInfo.OverallExtent);
                    else
                        size.Width = Math.Max(size.Width, this._reportLayoutInfo.OverallExtent);
                }


                return size;
            }

            // JJD 1/8/08 - BR29386
            // Return a size of 1,1 instead of empty
            //return Size.Empty;
            return new Size(1, 1);
		}

		#endregion //MeasureOverride

		// AS 5/4/07 Not Panel
		#region LogicalChildren
		/// <summary>
		/// Gets an enumerator for logical child elements in this panel.
		/// </summary>
		protected override System.Collections.IEnumerator LogicalChildren
		{
			get
			{
                // AS 2/3/09 NA 2009 Vol 1 - Fixed Fields
                // Pulled the splitter out of the children so we can keep 
                // them on top of the zorder.
                //
				//return this.children.GetEnumerator();
                if (_nearSplitter == null && _farSplitter == null)
    				return this.children.GetEnumerator();

                return new MultiSourceEnumerator(
                    this.children.GetEnumerator(),
                    new SingleItemEnumerator(_nearSplitter),
                    new SingleItemEnumerator(_farSplitter));
			}
		} 
		#endregion //LogicalChildren

		// AS 5/8/07 Optimization
		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._isVerifyingCells)
				return;

            
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)


			if (child is FixedFieldSplitter == false)
			{
				// AS 12/9/10 TFS61458
				// Reorganized the logic added as part of the fix for TFS60418 so 
				// we just set a flag that indicates if we are allowed to ignore 
				// the change. TFS61458 was introduced by this change because we 
				// were ignoring changes to virtualized cells on the basis that 
				// the size of virtualized cells is not used in the layout size 
				// which is correct. However, even if the field is virtualized 
				// we do care about its visibility changing when the record supports 
				// collapsing cells.
				//
				bool ignoreSizeChange = false;

				// AS 11/29/10 TFS60418
				// We can ignore desired size changes of virtualized cells.
				//
				if (_record != null && _record.RecordType == RecordType.DataRecord)
				{
					Field field = GridUtilities.GetFieldFromControl(child);

					// AS 6/27/11 TFS79783
					// If a cell's desired size changes and we are not in the middle of performing 
					// an autosize then we should re-evaluate the in view records if the field is 
					// autosized.
					//
					if (!IsPerformingAutoSizeMeasure && null != field && null != field.Owner)
						field.Owner.AutoSizeInfo.OnFieldElementDesiredSizeChanged(_record, field);

					if (null != field && field.IsCellVirtualized)
					{
						// in general we don't need to bother invalidating the 
						// layout manager when the desired size of a virtualized 
						// field changes since we're not using the virtualized 
						// element to provide the size
						ignoreSizeChange = true;

						// do additional checks if we're using a layout manager for this panel/record
						if (_recordLayoutManager != null || _record.GetLayoutManager(false) != null)
						{
							// if the record supports collapsing cells then we do want to invalidate 
							// the layout
							if (_record.ShouldCollapseEmptyCellsResolved)
								ignoreSizeChange = false;
						}
					}
				}
				else if (_isHeader || (_record != null && _record.RecordType == RecordType.HeaderRecord))
				{
					Field field = GridUtilities.GetFieldFromControl(child);

					if (null != field && field.IsLabelVirtualized)
						ignoreSizeChange = true;
				}

				if (ignoreSizeChange)
					return;
			}

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            this.InvalidateLayoutManager();

			base.OnChildDesiredSizeChanged(child);
		} 
		#endregion //OnChildDesiredSizeChanged

		#region OnPropertyChanged
        
#region Infragistics Source Cleanup (Region)
























































#endregion // Infragistics Source Cleanup (Region)

			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnPropertyChanged

		// AS 5/4/07 Not Panel
		#region VisualChildrenCount
		/// <summary>
		/// Gets the number of child elements for the panel.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
                // AS 2/3/09 NA 2009 Vol 1 - Fixed Fields
                // Pulled the splitter out of the children so we can keep 
                // them on top of the zorder.
                //
                //return this.children.Count;
				int count = this.children.Count;

                if (null != _nearSplitter)
                    count++;

                if (null != _farSplitter)
                    count++;

                return count;
            }
		} 
		#endregion //VisualChildrenCount

		#endregion //Base class overrides

		#region Methods

		// AS 5/4/07 Not Panel
		#region AddChild
		private void AddChild(FrameworkElement child)
		{
			this.AddLogicalChild(child);
			this.children.Add(child);
			this.AddVisualChild(child);
		} 
		#endregion //AddChild

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region AdjustCellRect

        internal void AdjustCellRect(FixedFieldLocation location, ref Rect rect)
        {
            if (_usingFixedInfo)
            {
                Vector offset = GetFixedAreaOffset(location, true);
                rect.Offset(offset);
            }
        }
        #endregion //AdjustCellRect

        // AS 2/25/09 TFS14598
        #region AdjustCellRects
        private void AdjustCellRects(IList<Field> fields, Vector delta)
        {
            Debug.Assert(null != fields);

            if (delta.X != 0 || delta.Y != 0)
            {
                for (int i = 0, count = fields.Count; i < count; i++)
                {
                    Field fld = fields[i];
                    int index = fld.TemplateCellIndex;

                    if (index >= 0 && index < _cellElementRects.Length)
                    {
                        _cellElementRects[index].Offset(delta);
                    }
                }
            }
        }
        #endregion //AdjustCellRects

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region AdjustDragPoint
        /// <summary>
        /// Helper method to normalize a given drag point such that the point would be over the same elements if they were not fixed.
        /// </summary>
        /// <param name="pt">Mouse point relative to the panel.</param>
        internal void AdjustDragPoint(ref Point pt)
        {
            if (_usingFixedInfo)
            {
                bool isHorz = _fieldLayout.IsHorizontal;

                if (GridUtilities.Contains(_fixedNearRect, pt, !isHorz))
                    pt.Offset(-_fixedNearOffset.X + _fixedNearAdjustment.X, -_fixedNearOffset.Y + _fixedNearAdjustment.Y);
                else if (GridUtilities.Contains(_fixedFarRect, pt, !isHorz))
                    pt.Offset(-_fixedFarOffset.X + _fixedFarAdjustment.X, -_fixedFarOffset.Y + _fixedFarAdjustment.Y);
            }
        }
        #endregion //AdjustDragPoint

        #region AdjustForSplitter
        // AS 2/2/09
        // Refactored into method from code in the measure override.
        //
        private static void AdjustForSplitter(double splitterExtent, bool isHorizontal, ref Size size, ref Size minSize, ref Size maxSize)
        {
            // adjust sizes to include the space needed for the splitter
            if (isHorizontal)
            {
                size.Height += splitterExtent;
                minSize.Height += splitterExtent;
                maxSize.Height += splitterExtent;
            }
            else
            {
                size.Width += splitterExtent;
                minSize.Width += splitterExtent;
                maxSize.Width += splitterExtent;
            }
        }
        #endregion //AdjustForSplitter

        // JJD 5/20/08 - BR32952 
        // Created helper function from code that used to be inside MeasureOverride
        #region AdjustSizeHolderManagerWithActualExtents
        
#region Infragistics Source Cleanup (Region)




















































































































#endregion // Infragistics Source Cleanup (Region)

        #endregion //AdjustSizeHolderManagerWithActualExtents	
    
		#region ApplyTemplateRecursively
		internal static void ApplyTemplateRecursively(DependencyObject dependencyObject)
		{
			FrameworkElement fe = dependencyObject as FrameworkElement;

			if (null != fe)
				fe.ApplyTemplate();

			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(dependencyObject); i < count; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

				if (null != child)
					ApplyTemplateRecursively(child);
			}
		}
		#endregion //ApplyTemplateRecursively

        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
        #region ArrangeHelper

		// MD 9/10/10 - TFS37596
		// Added a parameter so the caller could pass in the last measure size of the elemnt.
        //private void ArrangeHelper(FrameworkElement child, Rect rect)
		private void ArrangeHelper(FrameworkElement child, Rect rect, Size lastMeasureSize, int cellIndex)
        {
            bool wasVerifying = this._isVerifyingCells;

            this._isVerifyingCells = true;

            try
            {

                if (null != this._reportLayoutInfo)
                {
                    // if this is part of a report layout then use its 
                    // offset/extent
                    Field field = GridUtilities.GetFieldFromControl(child);
                    Debug.Assert(null != field);
                    bool isHorizontal = this._fieldLayout.IsHorizontal;
                    Field.FieldGridPosition gridPos = field.GridPosition;

                    int origin = isHorizontal ? gridPos.Row : gridPos.Column;
                    int span = isHorizontal ? gridPos.RowSpan : gridPos.ColumnSpan;
                    double offset = this._reportLayoutInfo.GetOffset(field, origin);
                    double extent = this._reportLayoutInfo.GetExtent(field, origin, span);

                    if (isHorizontal)
                    {
                        rect.Y = offset;
                        rect.Height = extent;
                    }
                    else
                    {
                        rect.X = offset;
                        rect.Width = extent;
                    }
                }


                // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
                if (this._usingFixedInfo)
                    child.CoerceValue(UIElement.ClipProperty);

				// MD 8/12/10
				// Found while fixing TFS26592
				// We are remeasuring here so if the child's DesiredSize is larger than the arrange size, we will 
				// allow it to resize its content so it doesn't get clipped. However, if the desired height of the 
				// child is less than the arrange size, we don't have to have it constrain the height becasue it 
				// won't be clipped anyway. In that case, we can just pass in PositiveInfinity as the height.
				//child.Measure(rect.Size);
				Size measureSize = rect.Size;
				Size desiredSize = child.DesiredSize;

				// AS 5/11/12 TFS110196
				// As with the change below for the width (for TFS101253), we should only do this if the 
				// last measure size was greater otherwise we continue to constrain the height of the cell.
				//
				//if (desiredSize.Height <= measureSize.Height)
				if (desiredSize.Height <= measureSize.Height && lastMeasureSize.Height > measureSize.Height)
				{
					// MD 9/10/10 - TFS37596
					// There are som ecases where we measure the cell without an infinite height. We can't just assume that is the 
					// last measured size so instead we will have the last measured size passed in.
					//measureSize.Height = Double.PositiveInfinity;
					measureSize.Height = lastMeasureSize.Height;
				}

				// AS 11/16/11 TFS95167
				// As we did with the height we need to try to give the element a larger measure size if possible so 
				// it's desired size can change if its criteria changes.
				//
				// AS 2/8/12 TFS101253
				// Reducing the scope of this change so we only do this if the last measure size 
				// was larger than what we are going to use to arrange this. After all the original 
				// issue was that we measured with a constrained size here (smaller than the original 
				// measure size) and when the element now needed more it couldn't get it because we 
				// gave it less than what we measured with originally because it didn't need it at 
				// that time.
				//
				//if (desiredSize.Width <= measureSize.Width)
				if (desiredSize.Width <= measureSize.Width && lastMeasureSize.Width > measureSize.Width)
				{
					measureSize.Width = lastMeasureSize.Width;
				}

				// AS 2/13/12 TFS99812
				// Since we are going to measure the cell we should cache that size or else we could (and in 
				// this situation did) end up comparing the desired size to a size that doesn't represent the 
				// size with which we measured the element to get that desired size.
				//
				_cellElementMeasureSizes[cellIndex] = measureSize;

				child.Measure(measureSize);

                child.Arrange(rect);
            }
            finally
            {
                this._isVerifyingCells = wasVerifying;
            }
        }

        #endregion //ArrangeHelper

        #region CalculateFixedExtents
        // AS 2/2/09
        // Refactored into method from code in the measure override.
        //
        private int CalculateFixedExtents(bool isHorizontal, FixedFieldLayoutInfo fixedFieldInfo,
            ref Size nearSplitSize, ref Size farSplitSize,
            GridBagLayoutItemDimensionsCollection dims,
            out double nearFixedExtent, out double farFixedExtent, out double scrollableExtent)
        {
            int factor = fixedFieldInfo.FieldLayout.UseCellPresenters ? FieldLayoutItem.CellPresenterFactor : 1;
            double[] logicalColumnDims = isHorizontal ? dims.RowDims : dims.ColumnDims;
            int scrollableAreaOrigin = factor * (fixedFieldInfo.NearFixedOrigin + fixedFieldInfo.NearFixedSpan);
            int farAreaEnd = factor * (fixedFieldInfo.FarFixedOrigin + fixedFieldInfo.FarFixedSpan);
            int farOrigin = factor * fixedFieldInfo.FarFixedOrigin;

            nearFixedExtent = scrollableAreaOrigin < logicalColumnDims.Length
                ? logicalColumnDims[scrollableAreaOrigin]
                : 0;
            farFixedExtent = farAreaEnd < logicalColumnDims.Length
                ? logicalColumnDims[farAreaEnd] - logicalColumnDims[farOrigin]
                : 0;

            // AS 3/4/09 TFS14532
            // Added an out parameter to return the extent of the scrollable elements.
            //
            int scrollableAreaEnd = farOrigin;
            scrollableExtent = farAreaEnd < logicalColumnDims.Length
                ? logicalColumnDims[scrollableAreaEnd] - logicalColumnDims[scrollableAreaOrigin]
                : 0;

            // adjust the size to include the splitters
            nearFixedExtent += isHorizontal ? nearSplitSize.Height : nearSplitSize.Width;
            farFixedExtent += isHorizontal ? farSplitSize.Height : farSplitSize.Width;
            return factor;
        }
        #endregion //CalculateFixedExtents

        // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
        #region CalculateFixedRecordAreaEdge
        /// <summary>
        /// Calculates the extent to the right edge of the record area. We needed this since we cannot use 
        /// the full scrollable extent.
        /// </summary>
		// AS 8/28/09 TFS21581
		// We needed additional parameters so we can identify when this is a cellpanel within a summary record 
		// of a groupby record or the cellpanel that positions the header labelpresenters to perform additional 
		// calculations.
		//
        //internal static double CalculateFixedRecordAreaEdge(bool isHorizontal, double cellAreaExtent, RecordPresenter rp)
        internal static double CalculateFixedRecordAreaEdge(bool isHorizontal, double cellAreaExtent, RecordPresenter rp, Record record, FrameworkElement referenceElement, bool isHeader)
        {
            FieldLayout fl = rp.FieldLayout;
            double totalExtent = cellAreaExtent;

			// AS 9/3/09 TFS21581
			// While testing I found a case where the element was detached but got in here so bail out if 
			// we don't have a valid field layout.
			//
			if (fl == null)
				return 0;

			// AS 9/3/09 TFS21581
			// For a header record presenter we want to make sure we measure with the 
			// sibling record (its attach to record) and not the datarecord that might 
			// be within the grouping.
			//
			//Record rc = null != rp ? rp.Record : null;
            Record rc = null != rp ? rp.GetRecordForMeasure() : null;

			// AS 9/3/09 TFS21581 2009.2
			// Account for HeaderRecord
			//
			HeaderRecord hr = rc as HeaderRecord;

            if (null != hr)
            {
                // JJD 11/10/09 - TFS24243
                // use the RecordForLayoutCalculations instead
                //rc = hr.AttachedToRecord;
                rc = hr.RecordForLayoutCalculations;
            }

            if (null != rc)
                totalExtent += rc.CalculateNestedRecordContentNearOffset() + rc.FarOffset;

			// AS 8/28/09 TFS21581 [Start]
			// Account for ContentAreaMargins in a summary record.
			//
			Thickness? contentAreaMargin = null;

			SummaryRecordPresenter srp = rp as SummaryRecordPresenter;

			if (null != srp)
				contentAreaMargin = srp.ContentAreaMargins;
			else if (null != referenceElement && record is SummaryRecord && rp is GroupByRecordPresenter)
			{
				GroupBySummariesPresenter gbsp = Utilities.GetAncestorFromType(referenceElement, typeof(GroupBySummariesPresenter), true, rp) as GroupBySummariesPresenter;

				if (null != gbsp)
					contentAreaMargin = gbsp.ContentAreaMargins;
			}

			if (null != contentAreaMargin)
			{
				if (!isHorizontal)
					totalExtent += contentAreaMargin.Value.Left + contentAreaMargin.Value.Right;
				else
					totalExtent += contentAreaMargin.Value.Top + contentAreaMargin.Value.Bottom;
			}

			bool removeExpansionIndicator = false;
			bool canAddRecordSelector = true;

			if (record is SummaryRecord && rp is SummaryRecordPresenter == false)
			{
				// for a summary record nested within a groupby record, we do not 
				// want to add in the record selector extent since the contentareamargins
				// already includes this value
				canAddRecordSelector = false;
			}
			else if (isHeader && rc != null)
			{
				// for the header on top, we need to exclude the expansion indicator size 
				// from the value we obtained from the record
				DataPresenterBase dp = fl.DataPresenter;

				if (fl.HasGroupBySortFields && rc.ParentRecord is GroupByRecord == false)
				{
					RecordManager rm = rc.RecordManager;

					if (null != rm && rm.Sorted.Count > 0)
					{
						removeExpansionIndicator = rm.Sorted[0].ExpansionIndicatorVisibility == Visibility.Collapsed;
					}
				}
			}

			if (removeExpansionIndicator)
			{
			    RecordManager rm = rp.Record.RecordManager;

		        if (!isHorizontal)
		            totalExtent -= fl.ExpansionIndicatorSize.Width;
		        else
		            totalExtent -= fl.ExpansionIndicatorSize.Height;
			}
			// AS 8/28/09 TFS21581 [End]

			// AS 8/28/09 TFS21581
			//if (null != fl)
            if (null != fl && canAddRecordSelector)
            {
                switch (fl.RecordSelectorLocationResolved)
                {
                    case RecordSelectorLocation.LeftOfCellArea:
                    case RecordSelectorLocation.RightOfCellArea:
                        if (!isHorizontal)
                            totalExtent += fl.RecordSelectorExtentResolved;
                        break;
                    case RecordSelectorLocation.AboveCellArea:
                    case RecordSelectorLocation.BelowCellArea:
                        if (isHorizontal)
                            totalExtent += fl.RecordSelectorExtentResolved;
                        break;
                }
            }

            return totalExtent;
        } 
        #endregion //CalculateFixedRecordAreaEdge

        #region CalculateScrollableExtent
        // AS 2/2/09
        // Refactored into method from code in the arrange override.
        //
        private double CalculateScrollableExtent(ref Size finalSize, bool isHorizontal, double nearFixedExtent, double farFixedExtent, double offset)
        {
            double scrollableExtent;

            if (isHorizontal)
            {
                scrollableExtent = (finalSize.Height + _fixedFarOffset.Y) - offset - farFixedExtent - nearFixedExtent;
            }
            else
            {
                scrollableExtent = (finalSize.Width + _fixedFarOffset.X) - offset - farFixedExtent - nearFixedExtent;
            }

            // AS 2/25/09 TFS14532
            if (GridUtilities.AreClose(0, scrollableExtent))
                scrollableExtent = 0;

            return scrollableExtent;
        }
        #endregion //CalculateScrollableExtent

        #region CalculateSplitterLayout
        // AS 2/2/09
        // Added helper method so we can determine if a splitter drop location would result in a 
        // valid layout - i.e. a layout where we still had room left for the scrollable fields.
        //
        /// <summary>
        /// Calculates the resulting scrollable area extent if a splitter drag operation for the specified fields was to be processed.
        /// </summary>
        /// <param name="affectedFields">A list of the fields whose fixed state is being changed</param>
        /// <param name="splitterType">The type of splitter being moved</param>
        /// <param name="willHaveScrollableFields">An out parameter indicating if the resulting layout would have any scrollable fields.</param>
        internal double CalculateSplitterLayout(List<Field> affectedFields, FixedFieldSplitterType splitterType, out bool willHaveScrollableFields)
        {
            Debug.Assert(null != affectedFields);
            Debug.Assert(null != _fieldLayout);

            willHaveScrollableFields = false;

            if (_fieldLayout == null)
                return 0;

            bool isHorizontal = _fieldLayout.IsHorizontal;
            double currentExtent = isHorizontal ? _scrollableClipRect.Height : _scrollableClipRect.Width;

            if (affectedFields == null || affectedFields.Count == 0)
                return currentExtent;

            FixedFieldLocation oldLocation, newLocation;
            GetSplitterLocations(affectedFields, splitterType, out oldLocation, out newLocation);

            // remove any that cannot be fixed
            for (int i = affectedFields.Count - 1; i >= 0; i--)
            {
                Field field = affectedFields[i];

                if (!field.IsFixedLocationAllowed(newLocation))
                    affectedFields.RemoveAt(i);
            }

            if (affectedFields.Count == 0)
                return currentExtent;

            LayoutInfo layout = null;


			// SSP 6/26/09 - NAS9.2 Field Chooser
			// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
			// directly accessing the member var.
			// 
            //layout = _fieldLayout._dragFieldLayoutInfo;
			layout = _fieldLayout.GetFieldLayoutInfo( false, false );

            // we need to clone the layout because this is a preview so we don't
            // want to manipulate the layout that is actually being used
            if (null != layout)
				// SSP 6/26/09 - NAS9.2 Field Chooser
				// We shouldn't take into account hidden items when manipulating the layout info.
				// Pass along as true the new excludeCollapsedEntries parameter.
				// 
                //layout = layout.Clone();
				layout = layout.Clone( true );


            FieldGridBagLayoutManager sourceManager = this.GetLayoutManager();

            // if we don't have one created that we cloned then create a new one
            // based on the current layout. i would have used the Create method on 
            // the LayoutInfo but I want to use the fixedLocation of the layout items
            if (null == layout)
                layout = sourceManager.CreateLayoutInfo(true);

            // adjust the fixed locations of the affected items
            layout.UpdateFixedLocation(oldLocation, newLocation, affectedFields);

            // now we need to create a grid bag manager that uses the ItemLayoutInfo 
            // as its constraints and measure the new scrollable extent
            FieldGridBagLayoutManager cloneManager = sourceManager.Clone(layout);
            cloneManager.VerifyLayout();

            // get the min/max/preferred for the layout items
            Size prefSize = cloneManager.CalculatePreferredSize();
            Size minSize = cloneManager.CalculateMinimumSize();
            Size maxSize = cloneManager.CalculateMaximumSize();

            #region Get SplitterExtent

            Size nearSplitSize = _nearSplitter != null ? _nearSplitter.DesiredSize : new Size();
            Size farSplitSize = _farSplitter != null ? _farSplitter.DesiredSize : new Size();
            double splitterExtent;

            if (isHorizontal)
                splitterExtent = nearSplitSize.Height + farSplitSize.Height;
            else
                splitterExtent = nearSplitSize.Width + farSplitSize.Width;

            #endregion //Get SplitterExtent

            // the sizes returned from the layoutmanager need to be updated based on the size
            // needed for the splitter(s)
            AdjustForSplitter(splitterExtent, isHorizontal, ref prefSize, ref minSize, ref maxSize);

            // get the 3 sizes we have to work from as the measure size. we cannot use the 
            // current actual width/height because those are formed based on the measured 
            // size which could be different now that the layout has changed.
            Size availableSize = _lastMeasureSize;
			// AS 11/14/11 TFS91077
			//Size autoFitSize = new Size(this.AutoFitWidth, this.AutoFitHeight);
			Size autoFitSize = this.GetAutoFitSize();
            Size templateGridSize = _fieldLayout.TemplateDataRecordCache.TemplateGridSize;

            // using those three sizes we need to adjust the preferred size based on the 
            // available/autofit/templategrid sizes as well as taking the min/max into account
			EnforceMinMax(ref availableSize, ref autoFitSize, ref templateGridSize, ref prefSize, minSize, maxSize, _fieldLayout );

            Size cellSize = prefSize;

            // now we need to remove the splitter extent from the preferred size that was calculated
            if (isHorizontal)
            {
                Debug.Assert(prefSize.Height >= splitterExtent);
                cellSize.Height = Math.Max(prefSize.Height - splitterExtent, 0);
            }
            else
            {
                Debug.Assert(prefSize.Width >= splitterExtent);
                cellSize.Width = Math.Max(prefSize.Width - splitterExtent, 0);
            }

            // now get the layout dimensions for that calculated arrange size
            GridBagLayoutItemDimensionsCollection dims = cloneManager.GetLayoutItemDimensionsCached(CalcSizeLayoutContainer.Instance, new Rect(cellSize));

            // we need to calculate the origin/span for the fixed areas
            FixedFieldLayoutInfo fixedLayoutInfo = new FixedFieldLayoutInfo(_fieldLayout, layout);

            double nearFixedExtent, farFixedExtent, scrollableAreaExtent;

            // get the extent for the near and far areas
            CalculateFixedExtents(isHorizontal, fixedLayoutInfo,
                ref nearSplitSize, ref farSplitSize, dims,
                out nearFixedExtent, out farFixedExtent, out scrollableAreaExtent);

            double scrollOffset = (double)this.GetValue(FixedFieldOffsetProperty);
            
            // calculate what is left for the scrollable area

            // we need to use the current size because the far offset is calculated
            // based on the current width. in theory we could recalculate that using 
            // the calculated width but that may or may not be the fixed element width
            // 
            Size finalSize = new Size(this.ActualWidth, this.ActualHeight);

            double scrollableExtent = CalculateScrollableExtent(ref finalSize, isHorizontal, nearFixedExtent, farFixedExtent, scrollOffset);

            
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

            // AS 3/17/09 TFS14532
            //willHaveScrollableFields = scrollableAreaExtent > scrollableExtent;
            willHaveScrollableFields = scrollableAreaExtent > scrollableExtent &&
                !GridUtilities.AreClose(scrollableAreaExtent, scrollableExtent);

            return scrollableExtent;
        }
        #endregion //CalculateSplitterLayout

        // AS 3/20/07 BR21313
		#region ClearAutoFitBindings
        
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		#endregion //ClearAutoFitBindings
                
        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
        #region CoerceCellClip
        internal static object CoerceCellClip(DependencyObject d, object newValue)
        {
            FrameworkElement element = (FrameworkElement)d;

            VirtualizingDataRecordCellPanel panel = VisualTreeHelper.GetParent(element) as VirtualizingDataRecordCellPanel;

			// AS 3/10/11 TFS66927
			// Since we don't force creation of a _recordLayoutManager when fixing 
			// (as part of the optimizations for TFS60418), we cannot rely on that 
			// member being non-null. So if we have fixed info try to use that member 
			// but if we don't have it then get the layout manager we would use.
			//
            //if (null != panel && null != panel._recordLayoutManager && panel._usingFixedInfo)
			//{
			//    FieldGridBagLayoutManager lm = panel._recordLayoutManager;
			bool needsClipping = null != panel && panel._usingFixedInfo;
			// AS 3/14/11 TFS67970 - Optimization
			//FieldGridBagLayoutManager lm = !needsClipping ? null : panel._recordLayoutManager ?? panel.GetLayoutManager();
			FieldGridBagLayoutManager lm = !needsClipping 
				? null 
				: panel._recordLayoutManager 
					?? panel.LastLayoutManager 
					?? panel.GetLayoutManager();
            if (null != lm)
            {
                Field fld = GridUtilities.GetFieldFromControl(element);
                FieldLayoutItemBase li = lm.GetLayoutItem(fld, element is LabelPresenter);

                // we're only going to clip scrollable elements
                if (null != li && li.FixedLocation == FixedFieldLocation.Scrollable)
                {
                    int cellIndex = fld.TemplateCellIndex;
                    Debug.Assert(cellIndex >= 0);

                    Rect elementRect = cellIndex >= 0 && cellIndex < panel._cellElementRects.Length
                        ? panel._cellElementRects[cellIndex]
                        : LayoutInformation.GetLayoutSlot(element);

                    if (elementRect.Width > 0 && elementRect.Height > 0)
                    {
                        Rect clipRect = panel._scrollableClipRect;

                        Rect intersection = Rect.Intersect(clipRect, elementRect);

						// AS 3/18/11 TFS66714
						bool isHeader = panel._isHeader || panel._record is HeaderRecord;

                        // if it doesn't contain the element then its completely clipped
						// AS 3/18/11 TFS66714
						// We've decided to always use a cliprect for scrollable labels
						// since it is more likely that someone will use a render transform
						// to rotate/skew the header but we won't add the overhead of 
						// creating/freezing a geometry for other cell elements.
						//
						//if (intersection.Width <= 0 || intersection.Height <= 0)
                        if (!isHeader && (intersection.Width <= 0 || intersection.Height <= 0))
                            return Geometry.Empty;
						// AS 3/14/11 TFS67970 - Optimization
						// The rect's may be close but not exact.
						//
						//else if (intersection != elementRect)
						else
						{
							bool isHorizontal = panel._fieldLayout.IsHorizontal;

							// AS 3/18/11 TFS66714
							if (isHeader)
							{
							}
							else
							// AS 3/14/11 TFS67970 - Optimization
							if (!isHorizontal)
							{
								bool nearIsClose = GridUtilities.AreClose(intersection.Left, elementRect.Left);

								// if the left intersects with the fixed area on the left and we don't have any fixing 
								// on the right then the same area would have been clipped by the normal clip rect anyway
								if (nearIsClose && panel._fixedFarRect.Width == 0)
									return newValue;

								bool farIsClose = GridUtilities.AreClose(intersection.Right, elementRect.Right);

								if (farIsClose && (nearIsClose || panel._fixedNearRect.Width == 0))
									return newValue;
							}
							else
							{
								bool nearIsClose = GridUtilities.AreClose(intersection.Top, elementRect.Top);

								// if the top intersects with the fixed area on the top and we don't have any fixing 
								// on the bottom then the same area would have been clipped by the normal clip rect anyway
								if (nearIsClose && panel._fixedFarRect.Height == 0)
									return newValue;

								bool farIsClose = GridUtilities.AreClose(intersection.Bottom, elementRect.Bottom);

								if (farIsClose && (nearIsClose || panel._fixedNearRect.Height == 0))
									return newValue;
							}

							// AS 3/18/11 TFS66714
							// If something is at least partially out of view we'll
							// use the entire scrollable clip rect as the clip bounds.
							//
							//// AS 2/26/09
							//// There seem to be some rounding issues in WPF with how 
							//// pixels are determined to be clipped or not so we will bump
							//// the clipping on the scrolling edge if its not clipped.
							////
							//if (isHorizontal)
							//{
							//    if (intersection.Bottom < clipRect.Bottom)
							//        intersection.Height += 1;
							//    else if (intersection.Top > clipRect.Top)
							//    {
							//        intersection.Height += 1;
							//        intersection.Y--;
							//    }
							//}
							//else
							//{
							//    if (intersection.Right < clipRect.Right)
							//        intersection.Width += 1;
							//    else if (intersection.Left > clipRect.Left)
							//    {
							//        intersection.Width += 1;
							//        intersection.X--;
							//    }
							//}
							intersection = clipRect;

                            // make the rect relative to the element
							intersection.Offset(-elementRect.X, -elementRect.Y);

							// AS 1/3/12 TFS96781
							// If the flow direction differ then we need to account for the transform 
							// since the coordinate systems will differ.
							//
							if (element.FlowDirection != panel.FlowDirection)
							{
								var flowTransform = VisualTreeHelper.GetTransform(element);

								if (null != flowTransform)
									intersection = flowTransform.TransformBounds(intersection);
							}

                            // AS 2/26/09
                            // There seem to be some rounding issues in WPF with how 
                            // pixels are determined to be clipped or not so we will bump
                            // the clipping in the non-scrolling edge.
                            //
                            if (isHorizontal)
                            {
                                intersection.X -= 1;
                                intersection.Width += 2;
                            }
                            else
                            {
                                intersection.Y -= 1;
                                intersection.Height += 2;
                            }

                            // JJD 4/29/10 - Optimization
                            // Freeze the geometry so the framework doesn't need to listen for change
                            //return new RectangleGeometry(intersection);
                            Geometry geometry = new RectangleGeometry(intersection);
                            geometry.Freeze();
                            return geometry;
                        }
                    }
                }
            }

            return newValue;
        } 
        #endregion //CoerceCellClip

        // JJD 6/9/08 - BR31962
        // Added to make sure the rect is constrained to the panel's size
        #region ConstrainCellRectBottom

        private static Rect ConstrainCellRectBottom(Rect rect, double height)
        {
            if (rect.Bottom > height)
                rect.Height = Math.Max(height - rect.Top, 1);

            return rect;
        }

        #endregion //ConstrainCellRectBottom	
    
		#region CreateCellElement

		// JJD 3/11/11 - TFS67970 - Optimization
		// Added isMouseOverCellArea
		//private Control CreateCellElement(int index, Field field, bool useCellPresenter)
		private Control CreateCellElement(int index, Field field, bool useCellPresenter, bool? isMouseOverCellArea)
		{
			Debug.Assert(this._cellElements != null, "The cell elements array should have been allocated by now!");
			Debug.Assert(this._cellElements[index] == null, "We should not get here if we already have a child element!");

			// JJD 5/4/07 - Optimization
			// Added support for label virtualization
			//Control element = this._cellElements[index] = useCellPresenter
			//    ? (Control)new CellPresenter()
			//    : field.GetCellValuePresenter();
			Control element;

			if ( this._isHeader )
				element = field.GetLabelPresenter( );
			// SSP 4/8/08 - Summaries Functionality
			// 
			else if ( _record is SummaryRecord )
			{
				SummaryRecord summaryRecord = _record as SummaryRecord;
				if ( !summaryRecord.HasFixedSummaryResults( field ) )
					return null;

				element = useCellPresenter
					? (Control)new SummaryCellPresenter( )
					: new SummaryResultsPresenter( );
			}
			else
				element = useCellPresenter
					? (Control)new CellPresenter( )
                    // JJD 12/23/08  added support for filtering
					//: field.GetCellValuePresenter( );
                    : _record is FilterRecord 
                        ? (Control)new FilterCellValuePresenter()
					    : field.GetCellValuePresenter( );
				//: (Control)new CellValuePresenter(); // field.GetCellValuePresenter();

			this._cellElements[index] = element;

			// AS 5/4/07 Not Panel
			//this.Children.Add(element);
			this.AddChild(element);

            #region Refactored
            
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

            #endregion // Refactored

			// JJD 3/11/11 - TFS67970 - Optimization
			// Added isMouseOverCellArea
			//this.InitializeCellElement(element, field);
			this.InitializeCellElement(element, field, isMouseOverCellArea);

			Debug.Assert(this._allocatedCellCount >= 0);
			_allocatedCellCount--;

			return element;
		}
		#endregion //CreateCellElement 

        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
        #region Output
		[Conditional("DEBUG_MEASURE_ARRANGE")]
        private static void DebugArrange(object value, string category)
        {
            Debug.WriteLine(value, DateTime.Now.ToString("hh:mm:ss:ffffff") + " VDRCP - " + category);
        }
	    #endregion //Output

		// SSP 4/7/08 - Summaries Functionality
		// 
		#region DirtyCellElements

		internal void DirtyCellElements( bool invalidateMeasureAndArrange )
		{
			this.RemoveCellElements( );
			
			if ( invalidateMeasureAndArrange )
			{
				this.InvalidateMeasure( );
				this.InvalidateArrange( );
			}
		}

		#endregion // DirtyCellElements

        #region EnforceMinMax
		// AS 8/11/09 NA 2009.2 Field Sizing
        //private static void EnforceMinMax(ref Size availableSize, ref Size autoFitSize, ref Size templateGridSize, ref Size size, Size minSize, Size maxSize)
        private static void EnforceMinMax(ref Size availableSize, ref Size autoFitSize, ref Size templateGridSize, ref Size size, Size minSize, Size maxSize, FieldLayout fieldLayout)
        {
            // if we autofit the width or we didn't have an explicit template grid width...
            if (double.IsNaN(autoFitSize.Width) == false || double.IsNaN(templateGridSize.Width))
            {
				// AS 8/11/09 NA 2009.2 Field Sizing
				// When extending the last column we do not want to reduce the extent of the 
				// other fields when there isn't enough room. This is essentially the way we 
				// handled this mode in wingrid and this is true to the name of the mode.
				//
				if (double.IsNaN(autoFitSize.Width) == false 
					&& null != fieldLayout 
					&& fieldLayout.AutoFitModeResolved == AutoFitMode.ExtendLastField)
				{
					autoFitSize.Width = Math.Max(autoFitSize.Width, size.Width);
				}

                // if we had an autofit size and its larger than the size we got back
                // then return that as our desired size (up to the maximum)
				if (autoFitSize.Width > size.Width)
				{
					size.Width = Math.Max(Math.Min(autoFitSize.Width, maxSize.Width), minSize.Width);
				}
				else if (autoFitSize.Width < size.Width)
				{
					size.Width = Math.Max(autoFitSize.Width, minSize.Width);
				}
				// AS 10/9/09 NA 2010.1 - CardView
				// Found this while implementing cardview changes. Basically we should not reduce down 
				// to the available size unless we are autosized. We do want to enforce the minSize.
				//
				//else if (GridUtilities.AreClose(size.Width, availableSize.Width) == false &&
				//    size.Width > availableSize.Width)
				else if (GridUtilities.AreClose(size.Width, availableSize.Width) == false &&
					size.Width > availableSize.Width &&
					fieldLayout.IsAutoFitWidth)
				{
					// if we're trying to use more than available then shrink down towards the min
					size.Width = Math.Max(availableSize.Width, minSize.Width);
				}
				else if (size.Width < minSize.Width)
				{
					// if we're trying to use more than available then shrink down towards the min
					size.Width = minSize.Width;
				}
            }
            else if (false == double.IsNaN(templateGridSize.Width))
            {
                // if we had a template grid size then we must use that
                size.Width = templateGridSize.Width;
            }

            // if we autofit the height or we didn't have an explicit template grid height...
            if (double.IsNaN(autoFitSize.Height) == false || double.IsNaN(templateGridSize.Height))
            {
				// AS 8/11/09 NA 2009.2 Field Sizing
				// When extending the last column we do not want to reduce the extent of the 
				// other fields when there isn't enough room. This is essentially the way we 
				// handled this mode in wingrid and this is true to the name of the mode.
				//
				if (double.IsNaN(autoFitSize.Height) == false
					&& null != fieldLayout
					&& fieldLayout.AutoFitModeResolved == AutoFitMode.ExtendLastField)
				{
					autoFitSize.Height = Math.Max(autoFitSize.Height, size.Height);
				}

				// if we had an autofit size and its larger than the size we got back
                // then return that as our desired size (up to the maximum)
                if (autoFitSize.Height > size.Height)
                {
					size.Height = Math.Max(Math.Min(autoFitSize.Height, maxSize.Height), minSize.Height);
                }
                else if (autoFitSize.Height < size.Height)
                {
					size.Height = Math.Max(autoFitSize.Height, minSize.Height);
                }
				// AS 10/9/09 NA 2010.1 - CardView
				// Found this while implementing cardview changes. Basically we should not reduce down 
				// to the available size unless we are autosized. We do want to enforce the minSize.
				//
				//else if (GridUtilities.AreClose(size.Height, availableSize.Height) == false &&
				//    size.Height > availableSize.Height)
				else if (GridUtilities.AreClose(size.Height, availableSize.Height) == false &&
					size.Height > availableSize.Height &&
					fieldLayout.IsAutoFitHeight)
				{
					// if we're trying to use more than available then shrink down towards the min
					size.Height = Math.Max(availableSize.Height, minSize.Height);
				}
				else if (size.Height < minSize.Height)
				{
					size.Height = minSize.Height;
				}
            }
            else if (false == double.IsNaN(templateGridSize.Height))
            {
                // if we had a template grid size then we must use that
                size.Height = templateGridSize.Height;
            }
        }
        #endregion //EnforceMinMax

		#region EnsureCellIsCreated
        internal void EnsureCellIsCreated(Cell cell)
        {
			// JJD 3/11/11 - TFS67970 - Optimization
			// Added isMouseOverCellArea
			//this.EnsureCellIsCreated(cell.Field);
			this.EnsureCellIsCreated(cell.Field, this.IsMouseOverCellArea);
        }

        // AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
        // Refactored to take a field since the cell itself isn't needed.
        //
		// JJD 3/11/11 - TFS67970 - Optimization
		// Added isMouseOverCellArea
		//private Control EnsureCellIsCreated(Field field)
		private Control EnsureCellIsCreated(Field field, bool? isMouseOverCellArea)
		{
            Control cell = null;

			if (null != this._fieldLayout)
			{
				// AS 4/27/07
				if (this._cellElementsDirty)
					this.VerifyCellElements();

				int index = this._fieldLayout.TemplateDataRecordCache.GetFieldIndex(field);

				if (index >= 0 &&
					null != this._cellElements &&
					this._cellElements[index] == null)
				{
					// JJD 3/11/11 - TFS67970 - Optimization
					// Added isMouseOverCellArea
					//this.CreateCellElement(index, field, this._fieldLayout.UseCellPresenters);
					this.CreateCellElement(index, field, this._fieldLayout.UseCellPresenters, isMouseOverCellArea);

					bool oldValue = this._positionAllAllocatedCells;
					try
					{
						this._positionAllAllocatedCells = true;
						this.InvalidateArrange();
						this.UpdateLayout();
					}
					finally
					{
						this._positionAllAllocatedCells = oldValue;
					}
				}

				if (index >= 0)
	                cell = this._cellElements[index];
			}

            return cell;
		}
		#endregion //EnsureCellIsCreated

		// AS 3/13/07 BR21065
		#region ForceTemplateArrangeFromMeasure
        
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

		#endregion //ForceTemplateArrangeFromMeasure

		// AS 11/14/11 TFS91077
		#region GetAutoFitSize
		private Size GetAutoFitSize()
		{
			var size = new Size(this.AutoFitWidth, this.AutoFitHeight);

			// the AutoFitWidth & AutoFitHeight ultimately come from the AutoFitCellArea(Width|Height) 
			// of the containing RecordPresenter. That is only updated once the layout is updated and 
			// therefore after the measure and arrange. so the VDRCP is basically working with an old 
			// value from the last completed measure/arrange pass. to account for the difference we'll 
			// remove the delta from the measure. since records in cardview size to the card we'll limit 
			// this to grid view.
			//
			if (_lastDPMeasureSize != null && _fieldLayout != null && _fieldLayout.CellPresentation == CellPresentation.GridView)
			{
				var dp = _fieldLayout.DataPresenter;

				if (null != dp)
				{
					if (dp.CurrentViewInternal.AutoFitToRecord == false)
					{
						var currentDpMeasureSize = dp.LastMeasureSize;
						var lastMeasureSize = _lastDPMeasureSize.Value;

						if (!double.IsNaN(size.Width) && !double.IsInfinity(currentDpMeasureSize.Width) && !double.IsInfinity(lastMeasureSize.Width))
							// JJD 1/9/12 - TFS77069
							// We only to adjust the autofit width smaller since there are times when the DP inside
							// a XamTilesControl can get resized bigger (larger than its preferred size) and we 
							// want to use the smalller size in that case.
							//size.Width = Math.Max(size.Width - (lastMeasureSize.Width - currentDpMeasureSize.Width), 0);
							size.Width = Math.Min(size.Width, Math.Max(size.Width - (lastMeasureSize.Width - currentDpMeasureSize.Width), 0));

						if (!double.IsNaN(size.Height) && !double.IsInfinity(currentDpMeasureSize.Height) && !double.IsInfinity(lastMeasureSize.Height))
							// JJD 1/9/12 - TFS77069
							// We only to adjust the autofit height smaller since there are times when the DP inside
							// a XamTilesControl can get resized bigger (larger than its preferred size) and we 
							// want to use the smalller size in that case.
							//size.Height = Math.Max(size.Height - (lastMeasureSize.Height - currentDpMeasureSize.Height), 0);
							size.Height = Math.Min(size.Height, Math.Max(size.Height - (lastMeasureSize.Height - currentDpMeasureSize.Height), 0));
					}
				}
			}

			return size;
		}
		#endregion //GetAutoFitSize

		// AS 6/18/09 NA 2009.2 Field Sizing
		#region GetCellElement
		internal Control GetCellElement(Field field, bool verify )
		{
			int index = field.TemplateCellIndex;

			// AS 3/2/11 66934 - AutoSize
			// Added if block to allow the caller to skip the verification if they know 
			// that they explicitly verified already.
			//
			if (verify)
				this.VerifyCellElements();

			if (index < 0 || index >= _cellElements.Length)
				return null;

			return this._cellElements[index];
		}
		#endregion //GetCellElement

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region GetCellInfo
        internal List<VirtualCellInfo> GetCellInfo(bool includeInViewOnly)
        {
			// AS 4/12/11 TFS62951
			return this.GetCellInfo(includeInViewOnly, true, true);
		}

		// AS 4/12/11 TFS62951
		// Added an overload so we could ignore fixed fields and instead include the scrollable fields 
		// that would be in those areas instead.
		//
		private List<VirtualCellInfo> GetCellInfo(bool includeInViewOnly, bool includeFixedNear, bool includeFixedFar)
		{
            List<VirtualCellInfo> cells = new List<VirtualCellInfo>();
			// AS 4/12/11 TFS62951
			//FieldGridBagLayoutManager lm = this.GetLayoutManager();
            FieldLayout fl = this._fieldLayout;
            bool usingFixedInfo = this._usingFixedInfo;

            if (null != fl)
            {
                // this routine was written primarily for use by the splitters
                // in a fixed situation but if its used otherwise and we're not
                // fixed then use the same clip rect we would use if we were 
                // checking to see what's in view
                Rect totalClipRect = usingFixedInfo
                    ? this._scrollableClipRect
                    : this.GetClipRect(this.RenderSize);

				// AS 4/12/11 TFS62951
				// Refactored this block. We were building a set of the aggregated cell rects
				// since we could be dealing with cell presenters but really we don't have to 
				// do that since it was already done when the arrange happened last. The 
				// _cellElementRects already contains the aggregated and offset rects.
				//
				#region Refactored
				//GridBagLayoutItemDimensionsCollection dims = lm.GetLayoutItemDimensionsCached(CalcSizeLayoutContainer.Instance, new Rect(this.LayoutManagerSize));
				//double[] colDims = dims.ColumnDims;
				//double[] rowDims = dims.RowDims;
				//
				//Vector nearOffset = this._fixedNearOffset + _fixedNearAdjustment;
				//Vector scrollableOffset = this._nearSplitterOffset;
				//Vector farOffset = this._fixedFarOffset + this._farSplitterOffset + _fixedFarAdjustment;
				//
				//#region Get CellRects
				//// since we have up to 2 layout items when using cell presenters
				//// we need to aggregate the cell rects first
				//Dictionary<Field, Rect> cellRects = new Dictionary<Field, Rect>();
				//
				//foreach (ILayoutItem item in lm.LayoutItems)
				//{
				//    FieldLayoutItemBase fieldItem = item as FieldLayoutItemBase;
				//
				//    if (null != fieldItem)
				//    {
				//        IGridBagConstraint gc = (IGridBagConstraint)lm.LayoutItems.GetConstraint(item);
				//        Rect cellRect = new Rect(colDims[gc.Column], rowDims[gc.Row], colDims[gc.Column + gc.ColumnSpan] - colDims[gc.Column], rowDims[gc.Row + gc.RowSpan] - rowDims[gc.Row]);
				//        Field field = fieldItem.Field;
				//
				//        Rect currentCellRect;
				//
				//        if (!cellRects.TryGetValue(field, out currentCellRect))
				//            currentCellRect = cellRect;
				//        else
				//            currentCellRect.Union(cellRect);
				//
				//        cellRects[field] = currentCellRect;
				//    }
				//} 
				//#endregion //Get CellRects
				//
				//foreach (KeyValuePair<Field, Rect> item in cellRects)
				//{
				//    Field field = item.Key;
				//    FieldLayoutItemBase fieldItem = lm.GetLayoutItem(field, false)
				//        ?? lm.GetLayoutItem(field, true);
				//
				//    if (null != fieldItem)
				//    {
				//        Rect cellRect = item.Value;
				//        Rect clipRect;
				//        FixedFieldLocation fixedLocation = usingFixedInfo ? fieldItem.FixedLocation : FixedFieldLocation.Scrollable;
				//
				//        switch (fieldItem.FixedLocation)
				//        {
				//            default:
				//            case FixedFieldLocation.Scrollable:
				//                cellRect.Offset(scrollableOffset);
				//
				//                if (includeInViewOnly && !cellRect.IntersectsWith(totalClipRect))
				//                    continue;
				//
				//                clipRect = Rect.Intersect(totalClipRect, cellRect);
				//                break;
				//            case FixedFieldLocation.FixedToNearEdge:
				//                cellRect.Offset(nearOffset);
				//                clipRect = cellRect;
				//                break;
				//            case FixedFieldLocation.FixedToFarEdge:
				//                cellRect.Offset(farOffset);
				//                clipRect = cellRect;
				//                break;
				//        }
				//
				//        int cellIndex = field.TemplateCellIndex;
				//
				//        if (cellIndex < 0 || cellIndex >= this._cellElements.Length)
				//            continue;
				//
				//        FrameworkElement element = _cellElements[cellIndex];
				//        Rect elementRect = _cellElementRects[cellIndex];
				//
				//        VirtualCellInfo cell = new VirtualCellInfo(cellRect, clipRect, field, fixedLocation, element, elementRect);
				//        cells.Add(cell);
				//    }
				//}
				#endregion //Refactored

				FixedFieldLocation fixedLocation = FixedFieldLocation.Scrollable;

				if (usingFixedInfo)
				{
					if (!includeFixedNear)
						totalClipRect.Union(_fixedNearRect);

					if (!includeFixedFar)
						totalClipRect.Union(_fixedFarRect);
				}

				for (int i = 0, count = _cellElementRects.Length; i < count; i++)
				{
					// note this is a little different than before. previous this would have been the 
					// union of the grid bag cell rects and not just where the element resides
					Rect cellRect = _cellElementRects[i];

					if (cellRect.IsEmpty)
						continue;

					Field field = fl.TemplateDataRecordCache.GetField(i);

					if (field == null)
						continue;

					if (usingFixedInfo)
						fixedLocation = field.FixedLocation;
					
					Rect clipRect;

					switch (fixedLocation)
					{
						default:
						case FixedFieldLocation.Scrollable:
							if (includeInViewOnly && !cellRect.IntersectsWith(totalClipRect))
								continue;

							clipRect = Rect.Intersect(totalClipRect, cellRect);
							break;
						case FixedFieldLocation.FixedToNearEdge:
							if (!includeFixedNear)
								continue;

							clipRect = cellRect;
							break;
						case FixedFieldLocation.FixedToFarEdge:
							if (!includeFixedFar)
								continue;

							clipRect = cellRect;
							break;
					}

					FrameworkElement element = _cellElements[i];
					Rect elementRect = _cellElementRects[i];

					VirtualCellInfo cell = new VirtualCellInfo(clipRect, field, fixedLocation, element, elementRect);
					cells.Add(cell);
				}
			}

            return cells;
        }
        #endregion //GetCellInfo

		#region GetFieldsInView

		// SSP 2/2/10
		// Added CellsInViewChanged event to the DataPresenterBase.
		// 
		internal List<Field> GetFieldsInView( )
		{
			List<Field> list = new List<Field>( );
			Rect visibleArea = this.GetClipRect( this.RenderSize );
			_lastFieldsInViewClipRect = visibleArea;

			if ( null != _fieldLayout && null != _cellElementRects )
			{
				foreach ( Field field in _fieldLayout.Fields )
				{
					int templateCellIndex = field.TemplateCellIndex;
					if ( templateCellIndex >= 0 )
					{
						Rect cellRect = _cellElementRects[templateCellIndex];
						Rect intersection = Rect.Intersect( visibleArea, cellRect );
						if ( !intersection.IsEmpty )
							list.Add( field );
					}
				}
			}

			return list;
		}

		#endregion // GetFieldsInView

		#region GetLogicalCellDimensions
		
#region Infragistics Source Cleanup (Region)






































































#endregion // Infragistics Source Cleanup (Region)

        internal void GetLogicalCellDimensions(List<double> rowDims, List<double> colDims)
        {
            FieldGridBagLayoutManager lm = this.GetLayoutManager();

            GridBagLayoutItemDimensionsCollection dims = lm.GetLayoutItemDimensionsCached(CalcSizeLayoutContainer.Instance, new Rect(this.LayoutManagerSize));

            // when using cell presenters 2 logical columns form 1 
            // grid position column. same for 2 logical grid bag rows 
            // equalling 1 grid position row
            int offset = this._fieldLayout.UseCellPresenters ? FieldLayoutItem.CellPresenterFactor : 1;

            AddExtents(dims.RowDims, rowDims, offset);
            AddExtents(dims.ColumnDims, colDims, offset);

            // add one column of spacing after the last column
            rowDims.Add(0d);
            colDims.Add(0d);

            FixedFieldLayoutInfo fixedFieldInfo = _fieldLayout.GetFixedFieldInfo(false);

            if (_fieldLayout.IsHorizontal)
            {
                rowDims[fixedFieldInfo.NearFixedSpan * 2] = _nearSplitterOffset.Y;
                rowDims[fixedFieldInfo.FarFixedOrigin * 2] = _farSplitterOffset.Y - _nearSplitterOffset.Y;
            }
            else
            {
                colDims[fixedFieldInfo.NearFixedSpan * 2] = _nearSplitterOffset.X;
                colDims[fixedFieldInfo.FarFixedOrigin * 2] = _farSplitterOffset.X - _nearSplitterOffset.X;
            }
        }

        private static void AddExtents(double[] source, List<double> dest, int sourceOffset)
        {
            if (source.Length > 0 && sourceOffset > 0)
            {
                double previousDim = source[0];

                for (int i = 1; i < source.Length; i += sourceOffset)
                {
                    int srcIndex = i + sourceOffset - 1;
                    dest.Add(0d); // assume there is no spacing before any item
                    dest.Add(source[srcIndex] - previousDim);
                    previousDim = source[srcIndex];
                }
            }
        }
        #endregion // GetLogicalCellDimensions

        #region GetCellMeasureSize
        
#region Infragistics Source Cleanup (Region)















































































































#endregion // Infragistics Source Cleanup (Region)

        #endregion //GetCellMeasureSize

		// AS 6/17/09 Optimization
		#region GetCellValuePresenter
		internal CellValuePresenter GetCellValuePresenter(Field field)
		{
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			Control child = GetCellElement(field, true );

			CellPresenter cp = child as CellPresenter;

			if (null != cp)
				return cp.CellValuePresenter;

			return child as CellValuePresenter;
		}
		#endregion //GetCellValuePresenter

        #region GetClipRect

		private Rect GetClipRect(Size currentElementSize)
		{
			// AS 4/12/11 TFS62951
			return this.GetClipRect(currentElementSize, false);
		}

		// AS 4/12/11 TFS62951
		// Added new overload so we could constrain the clip rect by the scroll content presenter.
		// We don't normally go down to this level but for selection hit testing we want to 
		// ignore the elements that would be clipped by the scrollbar, etc.
		//
        private Rect GetClipRect(Size currentElementSize, bool useRootScrollContentPresenter)
		{
			if (null != this._fieldLayout)
			{
				DataPresenterBase dp = this._fieldLayout.DataPresenter;

				if (null != dp)
				{
					UIElement ancestorToClipTo;

                    if (dp.IsReportControl)
                    {
                        ancestorToClipTo = dp.CurrentPanel;

                        Debug.Assert(ancestorToClipTo != null, "Should have CurrentPanel in VirtualizingDataRecordCellPanel");

                        if (ancestorToClipTo == null)
                            ancestorToClipTo = dp;
                    }
                    else
					if (this._fieldLayout.CellPresentation == CellPresentation.GridView)
						ancestorToClipTo = dp;
					else
					{
						Type wrapperType = dp.CurrentViewInternal.RecordPresenterContainerType;

						if (null == wrapperType)
							wrapperType = typeof(RecordPresenter);

						ancestorToClipTo = Utilities.GetAncestorFromType(this, wrapperType, true) as UIElement;
					}

                    // AS 2/24/09
                    // Moved up from below _isHeader check.
                    //
                    // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                    // This isn't specific to fixed fields but I found this while
                    // running some unit tests.
                    //
                    if (null == ancestorToClipTo)
                        return Rect.Empty;

					// AS 6/4/07
					// For headers, we'll make sure that this element is
					// still within the visual tree of the clip to element
					// since we cannot avoid dirtying the arrange when the 
					// scroll version changes.
					//
                    // JJD 3/16/09 - Optimization
                    // We should check this in all cases, not just headers, 
                    // to avoid the overhead of the exception being
                    // thrown and caught inside the call to GetClipRect below
					//if (this._isHeader)
					{
						if (this.IsDescendantOf(ancestorToClipTo) == false)
							return Rect.Empty;
					}

					// AS 4/12/11 TFS62951
					// The ScrollContentPresenter will clip the content so we can optionally use the 
					// SCP closest to the root so we can ignore elements that would be outside that 
					// area (e.g. under the hotizontal/vertical scrollbar.
					//
					if (useRootScrollContentPresenter)
					{
						ScrollContentPresenter scp = null;
						DependencyObject descendant = this;

						while (descendant != ancestorToClipTo)
						{
							if (descendant is ScrollContentPresenter)
								scp = descendant as ScrollContentPresenter;

							descendant = Utilities.GetParent(descendant);
						}

						if (null != scp)
							ancestorToClipTo = scp;
					}

					return GetClipRect(this, currentElementSize, ancestorToClipTo);
				}
			}

			return Rect.Empty;
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static Rect GetClipRect(UIElement descendant, Size descendantSize, UIElement ancestorToClipTo)
		{
            
            
            //
			
			

			Rect clipRect = new Rect(descendantSize);
            // AS 2/3/09 Remove unused members
            //DependencyObject ancestor = descendant;

			
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


			// AS 5/8/07
			// I added a try/catch because if the descendant is not in the visual chain 
			// (e.g. because it was unloaded) then this will generate an exception. In that
			// case we'll consider it to be fully clipped. I found this when switching from
			// carouselview to gridview.
			//
			try
			{
                // AS 2/24/09 Optimization
                // We know one is a descendant of the other so we don't need to call
                // TransformToVisual which first finds the common ancestor and then 
                // transforms.
                //
				//GeneralTransform gt = ancestorToClipTo.TransformToVisual(descendant);
				GeneralTransform gt =  ancestorToClipTo.TransformToDescendant(descendant);
				Rect ancestorRect = gt.TransformBounds(new Rect(new Point(), ((UIElement)ancestorToClipTo).RenderSize));
				clipRect.Intersect(ancestorRect);
			}
			catch (InvalidOperationException)
			{
				return Rect.Empty;
			}

			return clipRect;
			
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)


		} 
		#endregion //GetClipRect

		// AS 5/24/07 Recycle elements
		#region GetField
		private static Field GetField(Control control)
		{
			if (control is CellValuePresenter)
				return ((CellValuePresenter)control).Field;
			else if (control is CellPresenter)
				return ((CellPresenter)control).Field;
			else
				return (Field)control.GetValue(CellValuePresenter.FieldProperty);
		}
		#endregion //GetField

        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
        #region GetFieldKey
        private object GetFieldKey(Field field)
        {
            if (_isHeader)
                return field.LabelElementKey;

            // AS 1/26/09
            if (_record is FilterRecord)
                return field.FilterCellElementKey;

            return field.CellElementKey;
        }
        #endregion //GetFieldKey

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region GetFixedArea
        /// <summary>
        /// Gets the origin/span of a given area.
        /// </summary>
        /// <param name="location">The fixed area for which the position is being requested</param>
        /// <returns>A field position that encompasses the entire area</returns>
        internal FieldPosition GetFixedArea(FixedFieldLocation location)
        {
            if (null == _fieldLayout)
                return new FieldPosition();

            bool isHorz = _fieldLayout.IsHorizontal;
            FixedFieldLayoutInfo ffi = _fieldLayout.GetFixedFieldInfo(true);
            FieldPosition pos;
            int factor = _fieldLayout.UseCellPresenters ? FieldLayoutItem.CellPresenterFactor : 1;

            switch (location)
            {
                case FixedFieldLocation.FixedToNearEdge:
					// SSP 5/2/11 TFS32038
					// Use the new CreateHelper method instead of the constructor because a change
					// was made in the constructor as part of TFS24462 fix where it coerces 0 spans
					// to 1, which we don't want here.
					// 
                    //pos = new FieldPosition(
					pos = FieldPosition.CreateHelper(
						0, 0,
                        factor * (isHorz ? ffi.ColumnCount : ffi.NearFixedSpan),
                        factor * (isHorz ? ffi.NearFixedSpan : ffi.RowCount));
                    break;
                case FixedFieldLocation.FixedToFarEdge:
					// SSP 5/2/11 TFS32038
					// Use the new CreateHelper method instead of the constructor because a change
					// was made in the constructor as part of TFS24462 fix where it coerces 0 spans
					// to 1, which we don't want here.
					// 
					//pos = new FieldPosition(
					pos = FieldPosition.CreateHelper(
                        factor * (isHorz ? 0 : ffi.FarFixedOrigin),
                        factor * (isHorz ? ffi.FarFixedOrigin : 0),
                        factor * (isHorz ? ffi.ColumnCount : ffi.FarFixedSpan),
                        factor * (isHorz ? ffi.FarFixedSpan : ffi.RowCount));
                    break;
                default:
                case FixedFieldLocation.Scrollable:
					// SSP 5/2/11 TFS32038
					// Use the new CreateHelper method instead of the constructor because a change
					// was made in the constructor as part of TFS24462 fix where it coerces 0 spans
					// to 1, which we don't want here.
					// 
					//pos = new FieldPosition(
					pos = FieldPosition.CreateHelper(
                        factor * (isHorz ? 0 : ffi.NearFixedSpan),
                        factor * (isHorz ? ffi.FarFixedSpan : 0),
                        factor * (isHorz ? ffi.ColumnCount : ffi.FarFixedOrigin - ffi.NearFixedSpan),
                        factor * (isHorz ? ffi.FarFixedOrigin - ffi.NearFixedSpan : ffi.RowCount));
                    break;
            }

            return pos;
        }
        #endregion //GetFixedArea

        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
        #region GetFixedAreaOffset
        internal Vector GetFixedAreaOffset(FixedFieldLocation location, bool includeSplitterOffset)
        {
            switch (location)
            {
                case FixedFieldLocation.FixedToNearEdge:
                    return _fixedNearOffset + _fixedNearAdjustment;
                case FixedFieldLocation.FixedToFarEdge:
                    {
                        Vector farAdjustment = _fixedFarOffset + _fixedFarAdjustment;

                        if (includeSplitterOffset)
                            farAdjustment += _farSplitterOffset;

                        return farAdjustment;
                    }
                default:
                case FixedFieldLocation.Scrollable:
                    if (includeSplitterOffset)
                        return _nearSplitterOffset;
                    else
                        return new Vector();
            }
        }
        #endregion //GetFixedAreaOffset

		// AS 4/12/11 TFS62951
		// We need to know when the near/far fixed edges are in view so we can know whether to 
		// ignore fixed fields that are fixed to the near/far edge when we are not scrolled to 
		// their sssociated edge.
		//
		#region GetFixedEdgeScrollState
		internal void GetFixedEdgeScrollState(out bool isScrolledNearEdge, out bool isScrolledFarEdge)
		{
			bool isHorizontal = _fieldLayout != null && _fieldLayout.IsHorizontal;

			Vector v = _fixedNearOffset;
			isScrolledNearEdge = (isHorizontal && (GridUtilities.AreClose(v.Y, 0) || v.Y < 0))
				|| (!isHorizontal && (GridUtilities.AreClose(v.X, 0) || v.X < 0));

			Point bottomRight = _fixedFarRect.BottomRight;
			isScrolledFarEdge = (isHorizontal && (GridUtilities.AreClose(bottomRight.Y, this.ActualHeight) || bottomRight.Y > this.ActualHeight))
				|| (!isHorizontal && (GridUtilities.AreClose(bottomRight.X, this.ActualWidth) || bottomRight.X > this.ActualWidth));
		}
		#endregion //GetFixedEdgeScrollState

		// AS 4/12/11 TFS62951
		#region GetInViewRect
		internal Rect GetInViewRect()
		{
			// get the clip rect ignoring the area that would be clipped by the root ScrollContentPresenter.
			Rect bounds = this.GetClipRect(this.RenderSize, true);

			// AS 4/19/11 TFS73120
			if (_usingFixedInfo)
			{
				bool isScrolledToNearEdge, isScrolledToFarEdge;
				this.GetFixedEdgeScrollState(out isScrolledToNearEdge, out isScrolledToFarEdge);

				bool isHorizontal = _fieldLayout != null && _fieldLayout.IsHorizontal;

				// if the near/far edge is not scrolled into view then use the scrollable rect 
				// as the clipping rect so we're only considering the in view scrollable elements 
				// and the fixed elements if the associated fixed edge is in view
				if (!isScrolledToNearEdge)
				{
					if (isHorizontal)
					{
						double extent = Math.Max(_scrollableClipRect.Y - bounds.Y, 0);
						bounds.Y += extent;
						bounds.Height = Math.Max(bounds.Height - extent, 0);
					}
					else
					{
						double extent = Math.Max(_scrollableClipRect.X - bounds.X, 0);
						bounds.X += extent;
						bounds.Width = Math.Max(bounds.Width - extent, 0);
					}
				}

				if (!isScrolledToFarEdge)
				{
					if (isHorizontal)
					{
						// AS 4/19/11 TFS73120
						//bounds.Height -= Math.Max(bounds.Bottom - _scrollableClipRect.Bottom, 0);
						bounds.Height = Math.Max(bounds.Height - (bounds.Bottom - _scrollableClipRect.Bottom), 0);
					}
					else
					{
						// AS 4/19/11 TFS73120
						//bounds.Width -= Math.Max(bounds.Right - _scrollableClipRect.Right, 0);
						bounds.Width = Math.Max(bounds.Width - (bounds.Right - _scrollableClipRect.Right), 0);
					}
				}
			}

			return bounds;
		}
		#endregion //GetInViewRect

        // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
        #region GetLayoutManager
        internal FieldGridBagLayoutManager GetLayoutManager()
        {
            FieldGridBagLayoutManager lm = null;

            if (null != this._fieldLayout)
            {
                // if the record has one use that
                lm = this._record != null ? this._record.GetLayoutManager(false) : null;

                if (null == lm)
                {
                    // otherwise if we have a cached one then we should use that
                    lm = this._recordLayoutManager;

                    if (null == lm)
                    {
                        // if there is no cached one then get the shared one
                        lm = this._isHeader
                            ? this._fieldLayout.LabelLayoutManager
                            : this._fieldLayout.CellLayoutManager;

                        bool createLocalCopy = false;

						// AS 10/27/09 NA 2010.1 - CardView
						if (_fieldLayout.IsEmptyCellCollapsingSupportedByView)
						{
							// AS 3/18/11 TFS66714
							// Found this while debugging. _record could be null.
							//
							//if (_record.ShouldCollapseEmptyCellsResolved)
							if (_record == null || _record.ShouldCollapseEmptyCellsResolved)
								createLocalCopy = true;
						}

                        // JJD 12/23/08  added support for filtering
                        // if (this._record is SummaryRecord)
                        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                        //if (this._record is SummaryRecord ||
                        //    this._record is FilterRecord)
                        if (_record is SummaryRecord)
                            createLocalCopy = true;
						// AS 11/29/10 TFS60418
						// Just having nonvirtualized fields wouldn't need a copy of the 
						// layout manager. Nor would having fixed fields. Really we only 
						// need it when sizing to content or when individually sizable 
						// and the record has a size but in that case we wouldn't have 
						// gotten down here.
						//
						//else if (this.NonVirtualizedFields.Count > 0)
						//	createLocalCopy = true;
						//else if (_fieldLayout.IsFixedFieldsEnabled)
						//    createLocalCopy = true;
						//else
						//{
						//    // verify that against the sizing mode
						//    if (_fieldLayout.IsDataRecordSizedToContent)
						//        createLocalCopy = true;
						//    // AS 10/27/09 NA 2010.1 - CardView
						//    // Only set it to true if needed.
						//    //
						//    //else
						//    //    createLocalCopy = false;
						//}
						else if (!createLocalCopy)
						{
							switch (FieldGridBagLayoutManager.CalculateSizingMode(_record, _fieldLayout, _isHeader))
							{
								case DataRecordSizingMode.SizedToContentAndFixed:
								case DataRecordSizingMode.SizedToContentAndIndividuallySizable:
									createLocalCopy = true;
									break;
							}
						}

                        if (createLocalCopy)
                        {
                            lm = lm.Clone();
                            this._recordLayoutManager = lm;

							// AS 10/13/09 NA 2010.1 - CardView
							lm.Record = _record;
                        }
                    }
                }
            }

            lm.VerifyLayout();

            return lm;
        } 
        #endregion //GetLayoutManager

		// AS 4/12/11 TFS62951
		// This method was based upon DataPresenterBase.GetNearestCompatibleCell but was 
		// rewritten to handle ignoring fixed fields when required. It also has the benefit 
		// of not forcing the pivot cell to be allocated and not doing lots of point translations.
		//
		#region GetNearestCompatibleField
		internal Field GetNearestCompatibleField(Infragistics.Windows.Selection.ISelectableItem item, Point relativePoint, Field pivotField, Record pivotRecord)
		{
			if (_fieldLayout == null)
				return null;

			bool includeFixedNear = false;
			bool includeFixedFar = false;

			// when we are using fixed fields we want to ignore the fixed fields until we 
			// are scroll over such that the adjacent scrollable edge has been brought into view
			if (_usingFixedInfo)
				this.GetFixedEdgeScrollState(out includeFixedNear, out includeFixedFar);

			var cellInfos = this.GetCellInfo(true, includeFixedNear, includeFixedFar);
			Rect cellInfoRect = Rect.Empty;

			// first look for the cell we are exactly over
			foreach (var cellInfo in cellInfos)
			{
				if (GridUtilities.ContainsExclusive(cellInfo.ClipRect, relativePoint))
					return cellInfo.Field;

				cellInfoRect.Union(cellInfo.ClipRect);
			}

			// if we're over the cell panel and didn't find anything then bail. note i originally 
			// was going to use the GetClipRect but that is actually clipped to the datapresenter 
			// and not the scroll content presenter so it includes the area where the scrollbars are
			if (GridUtilities.ContainsExclusive(cellInfoRect, relativePoint))
				return null;

			if (pivotField == null)
				return null;

			int pivotTemplateIndex = pivotField.TemplateCellIndex;

			if (pivotTemplateIndex < 0 || pivotTemplateIndex >= _cellElementRects.Length)
				return null;

			// get the pivot cell information
			Rect pivotCellRect = _cellElementRects[pivotTemplateIndex];

			// if we don't have that and we are spanning rows (i.e. we are not over the original record)
			// then we need to look for the item in the last/first logical row depending on where the 
			// target record is relative to the pivot record
			var dp = this._fieldLayout.DataPresenter;

			if (dp == null)
				return null;

			var currentView = dp.CurrentViewInternal;

			#region IsBeforeOrAfter Setup

			bool isHorizontal = currentView.LogicalOrientation == Orientation.Horizontal;
			bool? isBeforeOrAfter = null;

			if (pivotRecord == null)
			{
				if (isHorizontal == false)
				{
					// if the mouse is above/below the panel
					if (relativePoint.Y < 0)
						isBeforeOrAfter = true;
					else if (relativePoint.Y >= this.ActualHeight)
						isBeforeOrAfter = false;
				}
				else
				{
					// if the mouse is left/right of the panel
					if (relativePoint.X < 0)
						isBeforeOrAfter = true;
					else if (relativePoint.X >= this.ActualWidth)
						isBeforeOrAfter = false;
				}
			}
			else if (_record != pivotRecord)
			{
				// do what we did before which was to compare the scroll position
				// of the pivot record to the record we are over
				isBeforeOrAfter = _record.OverallScrollPosition < pivotRecord.OverallScrollPosition;
			}
			#endregion //IsBeforeOrAfter Setup

			#region Determine where the passed in point is in relation to the pivot cell

			int compareVertical;
			int compareHorizontal;

			if (relativePoint.X < pivotCellRect.X)
				compareHorizontal = -1;
			else
			{
				if (relativePoint.X > pivotCellRect.Right)
					compareHorizontal = 1;
				else
					compareHorizontal = 0;
			}

			if (relativePoint.Y < pivotCellRect.Y)
				compareVertical = -1;
			else
			{
				if (relativePoint.Y > pivotCellRect.Bottom)
					compareVertical = 1;
				else
					compareVertical = 0;
			}

			// if the pivot cell's record is not the same as the
			// nearest record then look at the relative scroll positions
			if (isBeforeOrAfter != null)
			{
				if (isHorizontal == false)
					compareVertical = isBeforeOrAfter == true ? -1 : 1;
				else
					compareHorizontal = isBeforeOrAfter == true ? -1 : 1;
			}

			#endregion //Determine where the passed in point is in relation to the pivot cell

			#region Determine the nearest grid row

			int nearestRow = -1;

			if (compareVertical == 0)
				nearestRow = pivotField.GridPosition.Row;
			else
			{
				if (compareVertical > 0)
					nearestRow = 0;
				else
					nearestRow = pivotField.Owner.TotalRowsGenerated;

				foreach (var fc in cellInfos)
				{
					Field.FieldGridPosition position = fc.Field.GridPosition;

					if (compareVertical > 0)
					{
						if (nearestRow >= 0 &&
							 nearestRow >= position.Row)
							continue;

						if (fc.ElementRect.Y < relativePoint.Y)
							nearestRow = position.Row;
					}
					else
					{
						if (nearestRow >= 0 &&
							 nearestRow <= position.Row + position.RowSpan)
							continue;

						if (fc.ElementRect.Y > relativePoint.Y)
							nearestRow = position.Row + position.RowSpan;
					}
				}
			}

			Debug.Assert(nearestRow >= 0);

			if (nearestRow < 0)
				return null;

			#endregion //Determine the nearest grid row

			#region Determine the nearest grid column

			int nearestColumn = -1;

			if (compareHorizontal == 0)
				nearestColumn = pivotField.GridPosition.Column;
			else
			{
				if (compareHorizontal > 0)
					nearestColumn = 0;
				else
					nearestColumn = pivotField.Owner.TotalColumnsGenerated;

				foreach (var fc in cellInfos)
				{
					Field.FieldGridPosition position = fc.Field.GridPosition;

					if (compareHorizontal > 0)
					{
						if (nearestColumn >= 0 &&
							 nearestColumn >= position.Column)
							continue;

						if (fc.ElementRect.X < relativePoint.X)
							nearestColumn = position.Column;
					}
					else
					{
						if (nearestColumn >= 0 &&
							 nearestColumn <= position.Column + position.ColumnSpan)
							continue;

						if (fc.ElementRect.X > relativePoint.X)
							nearestColumn = position.Column + position.ColumnSpan;
					}
				}
			}

			Debug.Assert(nearestColumn >= 0);

			if (nearestColumn < 0)
				return null;

			#endregion //Determine the nearest grid column

			#region Create list of all fields on the nearest gris row

			//loop over al the cell and get all of the cell in the target row
			for (int i = cellInfos.Count - 1; i >= 0; i--)
			{
				var cellInfo = cellInfos[i];
				Field.FieldGridPosition position = cellInfo.Field.GridPosition;

				bool remove = true;

				if (compareVertical < 0)
				{
					if (position.Row + position.RowSpan == nearestRow)
						remove = false;
				}
				else
				{
					if (position.Row == nearestRow)
						remove = false;
				}

				if (remove)
				{
					int lastIndex = cellInfos.Count - 1;

					if (lastIndex != i)
						cellInfos[i] = cellInfos[lastIndex];

					cellInfos.RemoveAt(lastIndex);
				}
			}

			#endregion //Create list of all fields on the nearest gris row

			#region Get the cell from that list is in the nearest column

			VirtualCellInfo closestFc = null;

			foreach (var cellInfo in cellInfos)
			{
				Field.FieldGridPosition position = cellInfo.Field.GridPosition;

				if (compareHorizontal < 0)
				{
					if (position.Column + position.ColumnSpan == nearestColumn)
						closestFc = cellInfo;
				}
				else
				{
					if (position.Column == nearestColumn)
						closestFc = cellInfo;
				}
			}
			#endregion //Get the cell from that list is in the nearest column

			if (closestFc == null)
				return null;

			return closestFc.Field;
		}
		#endregion //GetNearestCompatibleField

        // AS 2/18/09
        // I found an issue where by when resizing after having created a local layout manager
        // was resizing the record when it should have resized the shared layout manager.
        //
        #region GetResizeLayoutManager
        internal FieldGridBagLayoutManager GetResizeLayoutManager()
        {
            FieldGridBagLayoutManager lm = this.GetLayoutManager();

			if (this.ForceUseCellLayoutManager)
				lm = _fieldLayout.CellLayoutManager;

            return lm;
        } 
        #endregion //GetResizeLayoutManager

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        #region GetScrollableViewport
        internal double GetScrollableViewport(double viewportExtent, bool ignoreIfInView)
        {
            if (_containingRp == null || _fieldLayout == null)
                return viewportExtent;

            bool isHorizontal = _fieldLayout.IsHorizontal;
			double preRecordArea = CalculateFixedRecordAreaEdge(isHorizontal, 0, _containingRp, _record, this, _isHeader);
            TemplateDataRecordCache recordCache = _fieldLayout.TemplateDataRecordCache;
            preRecordArea += recordCache.GetVirtPanelOffset(true, true);
            double panelExtent = isHorizontal ? this.ActualHeight : this.ActualWidth;

            // if the band would be completely in view when scrolled
            // all the way to the left then we don't need to consider it
            // because no matter how much we scroll it over it would 
            if (ignoreIfInView && panelExtent + preRecordArea <= viewportExtent)
                return viewportExtent;

            if (isHorizontal)
            {
                preRecordArea += _fixedNearRect.Height + _fixedFarRect.Height;
            }
            else
            {
                preRecordArea += _fixedNearRect.Width + _fixedFarRect.Width;
            }

            return viewportExtent - preRecordArea;
        }
        #endregion //GetScrollableViewport

        // SSP 4/9/08 - Summaries Functionality
		// Refactored code. Moved duplicate code into this helper method.
		// 
		#region GetSizeHolderManagerHelper
        
#region Infragistics Source Cleanup (Region)



































































#endregion // Infragistics Source Cleanup (Region)

		#endregion // GetSizeHolderManagerHelper

        // AS 1/30/09 NA 2009 Vol 1 - Fixed Fields
        // Refactored from the ProcessFixedFieldSplitterAction
        //
        #region GetSplitterLocations
        private void GetSplitterLocations(List<Field> affectedFields, FixedFieldSplitterType splitterType, out FixedFieldLocation oldLocation, out FixedFieldLocation newLocation)
        {
            Debug.Assert(null != affectedFields && affectedFields.Count > 0);

            Field firstField = affectedFields[0];

            FieldGridBagLayoutManager lm = this.GetLayoutManager();
            FieldLayoutItemBase firstFieldItem = lm.GetLayoutItem(firstField, false) ?? lm.GetLayoutItem(firstField, true);

            Debug.Assert(null != firstFieldItem);

            oldLocation = null != firstFieldItem ? firstFieldItem.FixedLocation : firstField.FixedLocation;

            switch (splitterType)
            {
                default:
                    Debug.Fail("Unexpected splitter type!");
                    newLocation = oldLocation;
                    return;
                case FixedFieldSplitterType.Far:
                    if (oldLocation == FixedFieldLocation.FixedToFarEdge)
                        newLocation = FixedFieldLocation.Scrollable;
                    else
                        newLocation = FixedFieldLocation.FixedToFarEdge;
                    break;
                case FixedFieldSplitterType.Near:
                    if (oldLocation == FixedFieldLocation.FixedToNearEdge)
                        newLocation = FixedFieldLocation.Scrollable;
                    else
                        newLocation = FixedFieldLocation.FixedToNearEdge;
                    break;
            }
        }
        #endregion //GetSplitterLocations

        #region GetSummaryRowNonVirtFields

        
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		#endregion // GetSummaryRowNonVirtFields

		// AS 5/4/07
		// Moved from VerifyCellsInView to a helper method since we need to be able
		// to position nonvirtualized fields in the arrange.
		//
		#region GetTemplateCellRect
        
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

        
#region Infragistics Source Cleanup (Region)
































































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetTemplateCellRect

		// AS 12/14/07 BR25223
		#region GetTemplateItemSize


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static Size GetTemplateItemSize(Field field, bool isHeader)
		{
			Debug.Assert(field != null && field.Owner != null);

			FieldLayout fieldLayout = field.Owner;
            // AS 2/3/09 Remove unused members
            //bool isHorizontal = fieldLayout.IsHorizontal;
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
			//FieldLayout.SizeHolderManager shm;

			if (fieldLayout.DataPresenter != null)
				fieldLayout.DataPresenter.UpdateLayout();

            #region Commented out
            
#region Infragistics Source Cleanup (Region)


































#endregion // Infragistics Source Cleanup (Region)

            #endregion //Commented out
            FieldGridBagLayoutManager lm = isHeader && fieldLayout.HasSeparateHeader
                ? fieldLayout.LabelLayoutManager : fieldLayout.CellLayoutManager;
            lm.VerifyLayout();

            FieldLayoutItemBase fli = lm.GetLayoutItem(field, isHeader);

            //Debug.Assert(null != fli || field.IsInLayout == false || field.IsVisibleInCellArea == false);

            if (null == fli)
                return new Size();

            // if we have a template grid size then use that for the container size
            TemplateDataRecordCache drCache = fieldLayout.TemplateDataRecordCache;
            Size templateGridSize = drCache.TemplateGridSize;

            // use the template grid size as the container/layout size
            Size layoutSize = templateGridSize;

            // if part of that is undefined then use the preferred
            if (double.IsNaN(templateGridSize.Width) || double.IsNaN(templateGridSize.Height))
            {
                Size preferredLayoutSize = lm.CalculatePreferredSize();

                if (double.IsNaN(templateGridSize.Width))
                    layoutSize.Width = preferredLayoutSize.Width;

                if (double.IsNaN(templateGridSize.Height))
                    layoutSize.Height = preferredLayoutSize.Height;
            }

            FieldRectsLayoutContainer lc = new FieldRectsLayoutContainer();
            lm.LayoutContainer(lc, new Rect(layoutSize));
            Size preferredSize = lc[fli].Size;
            return preferredSize;
		}
		#endregion

        // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
        // Moved the logic from the OnLayoutUpdated into a helper method.
        //
        #region HasClipRectChanged
        private bool HasClipRectChanged()
        {
			// AS 5/11/12 TFS110882
			// The cliprect is being compared against the last cliprect which itself 
			// was based on the finalSize handed into the ArrangeOverride. The DesiredSize
			// may well be small but the RenderSize is what we want to compare against.
			//
            //Rect currentClipRect = this.GetClipRect(this.DesiredSize);
			Rect currentClipRect = this.GetClipRect(this.RenderSize);

            // JJD 10/07/08
            // Use the AreClose method in addition to handle minor rounding differences 
            //if (this._lastClipRect != currentClipRect && this._lastClipRect.Contains(currentClipRect) == false)
            return this._lastClipRect != currentClipRect &&
                GridUtilities.AreClose(this._lastClipRect, currentClipRect) == false &&
                this._lastClipRect.Contains(currentClipRect) == false;
        }
        #endregion //HasClipRectChanged

        // AS 3/20/07 BR21313
		// Since we're not using a grid, the virtualizing panel must bind to the autofit width if 
		// the datapresenter is set to autofit the height and/or width.
		//
		#region InitializeAutoFitBindings
		private void InitializeAutoFitBindings()
		{
			if (this._isAutoFitInitialized)
				return;

			this._isAutoFitInitialized = true;

			// AS 5/4/07 Optimization
			//DataRecord dr = this.DataContext as DataRecord;
			// SSP 4/7/08 - Summaries Functionality
			//DataRecord dr = this._record;
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
            // We need the "header" record to autofit as well but it doesn't have a record.
            //
			//Record record = this._record;
			//DataPresenterBase dp = record != null ? record.DataPresenter : null;
			// AS 6/22/09 NA 2009.2 Field Sizing
            //DataPresenterBase dp = this._fieldLayout != null ? this._fieldLayout.DataPresenter : null;
            FieldLayout fl = _fieldLayout;

			// JJD 10/21/11 - TFS86028 - Optimization
			// Check IsAutoFit property
			//if (fl != null)
			if (fl != null && fl.IsAutoFit)
			{
				// JJD 10/21/11 - TFS86028 - Optimization
				// If we haven't already cached the containing RecordPresener then do so now.
				// This allows us to avoid the use of 'FindAncestor' bindings below which
				// is not only more efficient but will cause the AutoFit... properties
				// to be bound synchronously
				if (_containingRp == null )
					_containingRp = Utilities.GetAncestorFromType((DependencyObject)_containingCellArea ?? this, typeof(RecordPresenter), true) as RecordPresenter;

				Debug.Assert(_containingRp != null, "At this point we should have a containing RecordPresenter");

				// if we are autofitting the height then bind the height of the grid
				if (fl.IsAutoFitHeight)
				{
					// AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
					//this.SetBinding(VirtualizingDataRecordCellPanel.HeightProperty, Utilities.CreateBindingObject(DataRecordPresenter.AutoFitCellAreaHeightProperty, BindingMode.OneWay, new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataRecordPresenter), 1)));
					// SSP 8/14/09 TFS20334
					// Auto-fitting applies to summary records as well and SummaryRecordPresenter doesn't derive from DataRecordPresenter.
					// Here we should be safe using RecordPresenter as the ancestor type since only the DataRecordPresenter
					// and SummaryRecordPresenter instances contain VirtualizingDataRecordCellPanel.
					// 
					//this.SetBinding(VirtualizingDataRecordCellPanel.AutoFitHeightProperty, Utilities.CreateBindingObject(DataRecordPresenter.AutoFitCellAreaHeightProperty, BindingMode.OneWay, new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataRecordPresenter), 1)));
					// JJD 10/21/11 - TFS86028 - Optimization
					// If possible bind directly to the containing RP's AutoFitCellAreaHeight property instead of
					// delaying it by using a 'FindAncestor binding
					if ( _containingRp != null )
						this.SetBinding(VirtualizingDataRecordCellPanel.AutoFitHeightProperty, Utilities.CreateBindingObject(DataRecordPresenter.AutoFitCellAreaHeightProperty, BindingMode.OneWay, _containingRp));
					else
						this.SetBinding(VirtualizingDataRecordCellPanel.AutoFitHeightProperty, Utilities.CreateBindingObject(DataRecordPresenter.AutoFitCellAreaHeightProperty, BindingMode.OneWay, new RelativeSource(RelativeSourceMode.FindAncestor, typeof(RecordPresenter), 1)));
				}

				// if we are autofitting the width then bind the width of the grid
				if (fl.IsAutoFitWidth)
				{
                    // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
					//this.SetBinding(VirtualizingDataRecordCellPanel.WidthProperty, Utilities.CreateBindingObject(DataRecordPresenter.AutoFitCellAreaWidthProperty, BindingMode.OneWay, new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataRecordPresenter), 1)));
					// SSP 8/14/09 TFS20334
					// Auto-fitting applies to summary records as well and SummaryRecordPresenter doesn't derive from DataRecordPresenter.
					// Here we should be safe using RecordPresenter as the ancestor type since only the DataRecordPresenter
					// and SummaryRecordPresenter instances contain VirtualizingDataRecordCellPanel.
					// 
					//this.SetBinding(VirtualizingDataRecordCellPanel.AutoFitWidthProperty, Utilities.CreateBindingObject(DataRecordPresenter.AutoFitCellAreaWidthProperty, BindingMode.OneWay, new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataRecordPresenter), 1)));
					// JJD 10/21/11 - TFS86028 - Optimization
					// If possible bind directly to the containing RP's AutoFitCellAreaWidth property instead of
					// delaying it by using a 'FindAncestor binding
					if ( _containingRp != null )
						this.SetBinding( VirtualizingDataRecordCellPanel.AutoFitWidthProperty, Utilities.CreateBindingObject( DataRecordPresenter.AutoFitCellAreaWidthProperty, BindingMode.OneWay, _containingRp ) );
					else
						this.SetBinding( VirtualizingDataRecordCellPanel.AutoFitWidthProperty, Utilities.CreateBindingObject( DataRecordPresenter.AutoFitCellAreaWidthProperty, BindingMode.OneWay, new RelativeSource( RelativeSourceMode.FindAncestor, typeof( RecordPresenter ), 1 ) ) );
				}

				// AS 11/14/11 TFS91077
				if (fl.IsAutoFit)
					this.SetBinding(GridMeasureVersionProperty, Utilities.CreateBindingObject(DataPresenterBase.GridMeasureVersionProperty, BindingMode.OneWay, _fieldLayout.DataPresenter));
			}
		}
		#endregion //InitializeAutoFitBindings

		// AS 2/26/10 TFS28477
		#region InitializeCachedDimensions()
		internal void InitializeCachedDimensions()
		{
			Debug.Assert(this.IsMeasureValid && this.IsArrangeValid, "This is meant to be used when the layout is done");
			Debug.Assert(_cachedDimensions == null);
			Size renderSize = this.RenderSize;
			Size autoFitSize = new Size(double.NaN, double.NaN);
			Size templateGridSize = new Size(double.NaN, double.NaN);
			double splitterExtent = 0;

			bool isHorizontal = _fieldLayout.IsHorizontal;

			// JJD/AS 1/11/12 - TFS28646
			// We only want to call InitializeCachedDimensions in a vertical orientation
			// so if the orienation is horizontal then just bail
			if (isHorizontal)
				return;

			if (null != _nearSplitter)
				splitterExtent += isHorizontal ? _nearSplitter.ActualHeight : _nearSplitter.ActualWidth;

			if (null != _farSplitter)
				splitterExtent += isHorizontal ? _farSplitter.ActualHeight : _farSplitter.ActualWidth;

			this.InitializeCachedDimensions(ref renderSize, splitterExtent, ref autoFitSize, ref templateGridSize);
		}
		#endregion //InitializeCachedDimensions()

        #region InitializeCachedDimensions
        private void InitializeCachedDimensions(ref Size availableSize, double splitterExtent, ref Size autoFitSize, ref Size templateGridSize)
        {
            Debug.Assert(null != _fieldLayout && false == _fieldLayout.IsHorizontal);

            FieldGridBagLayoutManager lmFieldLayout = _isHeader
                ? _fieldLayout.LabelLayoutManager
                : _fieldLayout.CellLayoutManager;
            Size measureSize = new Size();

            if (!double.IsNaN(autoFitSize.Width))
                measureSize.Width = Math.Max( autoFitSize.Width - splitterExtent, 0 );
            else if (!double.IsNaN(templateGridSize.Width))
                measureSize.Width = Math.Max(templateGridSize.Width - splitterExtent, 0);
            else
            {
                measureSize.Width = lmFieldLayout.CalculatePreferredSize().Width;
            }

            EnforceMinMax(ref availableSize, ref autoFitSize, ref templateGridSize, ref measureSize,
				lmFieldLayout.CalculateMinimumSize(), lmFieldLayout.CalculateMaximumSize(), _fieldLayout );

            measureSize.Height = double.PositiveInfinity;
            _cachedDimensions = lmFieldLayout.GetLayoutItemDimensionsCached(CalcSizeLayoutContainer.Instance, new Rect(measureSize));
        }
        #endregion //InitializeCachedDimensions

        // AS 5/24/07 Recycle elements
		#region InitializeCellElement

		// AS 4/12/11 TFS62951
		// See the comments in OnRequestBringIntoView for details.
		//
		private Control _childBeingInitialized;

		// JJD 3/11/11 - TFS67970 - Optimization
		// Added isMouseOverCellArea
		//private void InitializeCellElement(Control element, Field field)
		private void InitializeCellElement(Control element, Field field, bool? isMouseOverCellArea)
		{
			// AS 4/12/11 TFS62951
			Control previousChildBeingInitialized = _childBeingInitialized;
			_childBeingInitialized = element;

			try
			{
				// AS 7/21/09 NA 2009.2 Field Sizing
				_cellElementVersion++;

				// AS 6/25/09 NA 2009.2 Field Sizing
				InitializeCellElement(element, field, _record, _isHeader);

				// JJD 3/11/11 - TFS67970 - Optimization
				// bypass header rcds and anything other than a DataRecord
				if (!isMouseOverCellArea.HasValue)
					return;

				// JJD 3/11/11 - TFS67970 - Optimization
				// See if the elemnet is a cell value presenter
				CellValuePresenter cvp = element as CellValuePresenter;

				if (cvp == null)
				{
					CellPresenter cp = element as CellPresenter;

					if (cp != null)
						cvp = cp.CellValuePresenter;
				}

				// JJD 3/11/11 - TFS67970 - Optimization
				// Initialize the ismouseover state
				if (cvp != null)
					cvp.OnRecordMouseOverChanged(isMouseOverCellArea.Value);
			}
			finally
			{
				// AS 4/12/11 TFS62951
				_childBeingInitialized = previousChildBeingInitialized;
			}
		}

		// AS 6/25/09 NA 2009.2 Field Sizing
		internal static void InitializeCellElement(Control element, Field field, Record record, bool isHeader)
		{
			element.SetValue(DataItemPresenter.FieldProperty, field);

			// JJD 5/4/07 - Optimization
			// Added support for label virtualization
			if (isHeader == false)
			{
				// JJD 3/9/11 - TFS67970 - Optimization
				// CellValuePresenters set this during the Field set above so there is no need to do it here
				if (!(element is CellValuePresenter))
					element.SetBinding(CellValuePresenter.IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, field));
			}

			
#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


			// SSP 4/8/08 - Summaries Functionality
			// 
			if ( record is SummaryRecord )
			{
				SummaryRecord summaryRecord = (SummaryRecord)record;

				if ( element is SummaryResultsPresenter )
				{
					SummaryResultsPresenter resultsElem = element as SummaryResultsPresenter;

					// SSP 4/10/12 - 108549 - Optimizations
					// Now that we do this in the SummaryResultsPresenter whenever its DataContext changes, we don't need
					// to do it here.
					// 
					//resultsElem.SummaryResults = summaryRecord.GetFixedSummaryResults( field, true );

					// AS 8/24/09 TFS19532
					// See GridUtilities.CoerceFieldElementVisibility for details.
					//
					// JJD 3/9/11 - TFS67970 - Optimization - use the cached binding
					//resultsElem.SetBinding(GridUtilities.FieldVisibilityProperty, Utilities.CreateBindingObject("VisibilityResolved", BindingMode.OneWay, field));
					resultsElem.SetBinding(GridUtilities.FieldVisibilityProperty, field.VisibilityBinding);

					// AS 8/26/09 CellContentAlignment
					// JJD 3/9/11 - TFS67970 - Optimization - use the cached binding
					//resultsElem.SetBinding(GridUtilities.CellContentAlignmentProperty, Utilities.CreateBindingObject("CellContentAlignmentResolved", BindingMode.OneWay, field));
					resultsElem.SetBinding(GridUtilities.CellContentAlignmentProperty, field.CellContentAlignmentBinding);

					// AS 6/21/11 TFS77675
					// The summary record doesn't include "cell elements". Instead it includes the elements it contains 
					// so we need to invalidate the record peer's children.
					//
					record.InvalidatePeer();
				}
			}
		}
		#endregion //InitializeCellElement

        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
        #region InitializeCellPresenterRects
        private void InitializeCellPresenterRects(FrameworkElement element)
        {
            CellPresenterBase cp = element as CellPresenterBase;

            if (null != cp)
            {
                Field field = cp.Field;

                if (null != field)
                {
                    int cellIndex = field.TemplateCellIndex;

                    if (cellIndex >= 0 && cellIndex < this._cellElements.Length)
                    {
                        Rect cellPresenterRect = this._cellElementRects[cellIndex];
						// AS 3/14/11 TFS67970 - Optimization
						//FieldGridBagLayoutManager lm = this.GetLayoutManager();
                        FieldGridBagLayoutManager lm = this.LastLayoutManager ?? this.GetLayoutManager();
                        FieldRectsLayoutContainer lc = this.CellPresenterRectContainer;

                        for (int i = 0; i < 2; i++)
                        {
                            bool isLabel = i == 0;
                            FieldLayoutItemBase layoutItem = lm.GetLayoutItem(field, isLabel);
                            Rect rect;

                            if (null != layoutItem)
                            {
                                rect = lc[layoutItem];

                                // then offset it so the rect returned is relative to the 
                                // cell presenter
                                rect.Offset(-cellPresenterRect.X, -cellPresenterRect.Y);
                            }
                            else
                                rect = new Rect();

                            if (isLabel)
                                cp.LabelRect = rect;
                            else
                                cp.CellRect = rect;
                        }
                    }
                }
            }
        }
        #endregion //InitializeCellPresenterRects

        #region InitializeRect
        
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

		#endregion //InitializeRect

        #region InitializeSplitter
        // AS 2/3/09 NA 2009 Vol 1 - Fixed Fields
        // Refactored from the VerifyFixedState to avoid duplicate code.
        //
        private void InitializeSplitter(ref FixedFieldSplitter splitter, FixedFieldSplitterType splitterType, Visibility visibility, bool isEnabled)
        {
            if (null == splitter)
            {
                Orientation orientation = _fieldLayout.IsHorizontal ? Orientation.Horizontal : Orientation.Vertical;
                splitter = new FixedFieldSplitter(_isHeader, orientation, splitterType);
                this.AddLogicalChild(splitter);
                this.AddVisualChild(splitter);
            }

            splitter.Visibility = visibility;
            splitter.IsEnabled = isEnabled;

            // AS 2/3/09
            // We decided that we don't want the splitters to appear within the 
            // summary record so if the splitter would be visible then we want 
            // to hide it and force it to be disabled.
            //
            if (_record is SummaryRecord)
            {
                if (visibility == Visibility.Visible)
                    splitter.Visibility = Visibility.Hidden;

                splitter.IsEnabled = false;
            }
        }
        #endregion //InitializeSplitter

        // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
        // There were a couple of places that we were invalidating the layout of the panel's
        // layout manager but if we're using that of the record then we would have missed 
        // dirtying it so I moved this into a helper routine.
        //
        #region InvalidateLayoutManager
		private void InvalidateLayoutManager()
        {
            if (null != _recordLayoutManager)
                _recordLayoutManager.InvalidateLayout();

            if (null != _record)
            {
                FieldGridBagLayoutManager lmRecord = _record.GetLayoutManager(false);

                if (null != lmRecord)
                    lmRecord.InvalidateLayout();
            }
        }
        #endregion //InvalidateLayoutManager

		#region OnDataContextChanged
		private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			VirtualizingDataRecordCellPanel cellPanel = (VirtualizingDataRecordCellPanel)d;

			// JJD 5/22/07 - Optimization
			// Added support for record presenter recycling
			FieldLayout oldFieldLayout = cellPanel._fieldLayout;

			// AS 10/27/09 NA 2010.1 - CardView
			if (null != cellPanel._record)
				PropertyChangedEventManager.RemoveListener(cellPanel._record, cellPanel, "CellLayoutVersion");

			// AS 5/4/07 Optimization
			// SSP 4/7/08 - Summaries Functionality
			// Changed to Record to support summary record.
			// 
			//cellPanel._record = e.NewValue as DataRecord;
			cellPanel._record = e.NewValue as Record;

			// AS 10/27/09 NA 2010.1 - CardView
			if (null != cellPanel._record)
				PropertyChangedEventManager.AddListener(cellPanel._record, cellPanel, "CellLayoutVersion");

			if (e.OldValue != null)
			{
				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


                
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			}

			// SSP 4/7/08 - Summaries Functionality
			// Changed to Record to support summary record.
			// 
			//DataRecord record = null;
			Record record = null;
			
			// JJD 5/4/07 - Optimization
			// Added support for label virtualization
			FieldLayout fl =  e.NewValue as FieldLayout;

			if (fl != null)
				cellPanel._isHeader = true;
			else
			{
				// SSP 4/7/08 - Summaries Functionality
				// Changed to Record to support summary record.
				// 
				//record = e.NewValue as DataRecord;
				record = e.NewValue as Record;

				if (record != null)
				{
					fl = record.FieldLayout;

					// MD 8/19/10
					// Let the data record presenter know that this is the associated cell panel.
					DataRecordPresenter drp = record.AssociatedRecordPresenter as DataRecordPresenter;
					if (drp != null)
						drp.AssociatedVirtualizingDataRecordCellPanel = cellPanel;
				}
			}

			// JJD 5/4/07 - Optimization
			// Added support for label virtualization
			//if (record != null)
			if (fl != null)
			{
				cellPanel._fieldLayout = fl;

                // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                cellPanel.InvalidateLayoutManager();

				// AS 10/13/09 NA 2010.1 - CardView
				if (cellPanel._recordLayoutManager != null)
					cellPanel._recordLayoutManager.Record = cellPanel._record;

                // AS 2/23/09
                // When reusing an element we should dirty the measure arrange since
                // the size may be variable.
                //
                if (oldFieldLayout != null)
                {
                    cellPanel.InvalidateMeasure();
                    cellPanel.InvalidateArrange();
                }

				// AS 4/27/07
				// Defer allocating the cell array until needed.
				//
				//int cellCount = fl.TemplateDataRecordCache.GetCellCount();
				//cellPanel._cellElements = new Control[cellCount];
				//cellPanel._allocatedCellCount = cellCount;

				// JJD 5/22/07 - Optimization
				// Added support for record presenter recycling
				// Only dirty the elements if the field layout has changed
				if (oldFieldLayout != cellPanel._fieldLayout)
				{
					// AS 9/17/09 TFS22285
					// I don't think we recycle with different field layouts but if we 
					// do then we should not recycle any cell elements and should 
					// synchronously remove the cell elements so they don't get a 
					// data context change from the old record to the new record 
					// which won't be able to use the currently referenced field.
					//
					//cellPanel._cellElementsDirty = true;
					cellPanel.RemoveCellElements();
				}
				else
				{
					// AS 9/17/09 TFS22285
					// We need to proactively clear up any cell elements for fields that 
					// were removed.
					//
					if (fl.Fields.Version != cellPanel._fieldsVersion)
						cellPanel.RemoveInvalidFieldElements();
				}

                // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
                // We need to always bind and conditionally invalidate. I was finding
                // that some records (e.g. filter record weren't updating otherwise
                // and the splitter element would be scrolled out of view.
                //
                //// JJD 5/4/07 - Optimization
				//// Added support for label virtualization
				//bool hasVirtualizableFields = cellPanel.VirtualizedFields.Count > 0;
                //
				//if (hasVirtualizableFields)
				{
					cellPanel.SetBinding(VirtualizingDataRecordCellPanel.ScrollVersionProperty,
						Utilities.CreateBindingObject(DataPresenterBase.ScrollVersionProperty, BindingMode.OneWay, fl.DataPresenter));
				}

				cellPanel.SetBinding(VirtualizingDataRecordCellPanel.GridColumnWidthVersionProperty,
					Utilities.CreateBindingObject(FieldLayout.GridColumnWidthVersionProperty, BindingMode.OneWay, fl));

                




				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

                // AS 2/10/09
                // We are going to use the template version since there are times when the 
                // template record cache has changed and therefore the virt/nonvirt field 
                // lists may change.
                //
                cellPanel.SetBinding(VirtualizingDataRecordCellPanel.TemplateVersionProperty,
                    Utilities.CreateBindingObject(FieldLayout.TemplateVersionProperty, BindingMode.OneWay, fl));

				// AS 5/24/07 Recycle elements
				cellPanel._isRecycling = fl.DataPresenter.CellContainerGenerationMode == CellContainerGenerationMode.Recycle;

				// AS 7/21/09 NA 2009.2 Field Sizing
				cellPanel._lastAutoSizeElementVersion = -1;
			}
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

            else
            {
                // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
                // Clear the fixed field info so we know to unregister with the 
                // owning panel.
                //
                if (cellPanel._usingFixedInfo)
                {
                    cellPanel._usingFixedInfo = false;
                    cellPanel.ClearValue(FixedFieldInfoProperty);
                }
            }
		}

		#endregion //OnDataContextChanged

		#region OnLayoutUpdated
		// JJD 3/15/11 - TFS65143 - Optimization
		// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
		// and just wire LayoutUpdated on the DP
		//private void OnLayoutUpdated(object sender, EventArgs e)
		private void OnLayoutUpdated()
		{
			// JJD 2/16/12 - TFS101387
			// See if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// If not then just bail.
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
			if (false == this.IsVisible && this.DataContext == null)
				return;

			// JJD 3/15/11 - TFS65143 - Optimization
			// No need to unhook since the DP is maintaining the callback list and automatically removes the entry
            // unhook the event
			//this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);










			// SSP 2/2/10
			// Added CellsInViewChanged event to the DataPresenterBase.
			// 
			// --------------------------------------------------------------------------------
			bool? hasClipRectChanged = null;
			DataPresenterBase dp = null != _fieldLayout ? _fieldLayout.DataPresenter : null;

			if ( null != dp && ! dp._pendingRaiseCellsInViewChangedAsyncHelperHandler 
				&& dp.ShouldRaiseCellsInViewChanged )
			{
				hasClipRectChanged = this.HasClipRectChanged( );

				Rect currentClipRect = this.GetClipRect( this.DesiredSize );
				if ( hasClipRectChanged.Value || ! GridUtilities.AreClose( _lastFieldsInViewClipRect, currentClipRect ) )
				{
					dp.RaiseCellsInViewChangedAsyncHelper( );
				}
			}
			// --------------------------------------------------------------------------------


            // JJD 9/20/08 - added support for printing
            if (this._reportLayoutInfo != null && this._fieldLayout != null)
            {
                if (dp == null)
                    return;

                Panel panel = dp.CurrentPanel;

                if (panel == null)
                    return;

                Point pt = this.TranslatePoint(new Point(), panel);

                double offset;
				IScrollInfo scrollInfo = panel as IScrollInfo;

				if (this._fieldLayout.IsHorizontal)
				{
					offset = pt.Y;

					// JJD 3/23/11 - TFS65143
					// Adjust the offset to compensate for scrolling offset
					if (scrollInfo != null)
						offset += scrollInfo.VerticalOffset;
				}
				else
				{
					offset = pt.X;

					// JJD 3/23/11 - TFS65143
					// Adjust the offset to compensate for scrolling offset
					if (scrollInfo != null)
						offset += scrollInfo.HorizontalOffset;
				}

                // if the offset is a very large negative number then
                // ignore it because the element was positioned out of view
                if (offset > -1000)
                {
                    this._normalizedOffsetFromPanel = offset;

                    // call RefreshReportLayoutInfo which will return true if the offset
                    // and therefore the _reportLayoutInfo has changed
                    if (this.RefreshReportLayoutInfo())
                    {
                        this.InvalidateMeasure();
                        panel.InvalidateArrange();
                        panel.InvalidateMeasure();
                    }
                }

                return;
            }


            // JJD 6/11/08 - BR31962
            // If the measure needs to be invalidated then do that and return
            if (this._requiresAsynchMeasureInvalidation)
            {
                this.InvalidateMeasure();

                Debug.WriteLine("InvalidateMeasure called in VirtualizingDataRecordCellPanel.OnLayoutUpdated");
                Debug.Assert(this._isHeader, "We shoould only invalidate the measure of headers in VirtualizingDataRecordCellPanel.OnLayoutUpdated");
                return;
            }
 
            // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
            // Previously we only hooked the layout updated if we had
            // virtualized fields but since we're also using this 
            // when we have fixed information we can get in here
            // even if we have no virtualized fields so skip the 
            // clip rect check then.
            //
            IList<Field> virtFields = this.VirtualizedFields;

            if (null != virtFields && virtFields.Count > 0)
            {
                
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)

				
				
				
				
				if ( !hasClipRectChanged.HasValue )
					hasClipRectChanged = this.HasClipRectChanged( );
                
				if ( hasClipRectChanged.Value )
                {
                    this.InvalidateArrange();
                }
                else if (this._fieldLayout != null && this.IsNestedFixedHeader)
                {
                    // JJD 2/19/09 - support for printing.
                    // We can't do asynchronous operations during a report operation
                    if (dp != null && dp.IsReportControl == false)
                    {
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                            new GridUtilities.MethodDelegate(VerifyClipRect));
                    }
                }

            }

            // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
            this.VerifyFixedFieldOffsets();
		}

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodDelegate();

		#endregion //OnLayoutUpdated

		// JJD 3/11/11 - TFS67970 - Optimization - added
		#region OnRecordMouseOverChanged

		private void OnRecordMouseOverChanged()
		{
			bool? isMouseOverCellArea = this.IsMouseOverCellArea;

			// bypass header rcds and anything other than a DataRecord
			if (!isMouseOverCellArea.HasValue)
				return;

			int count = this.children.Count;

			if (count > 0)
			{
				// walk over all the children pasing the new state to all CellValuePresenters
				for (int i = 0; i < count; i++)
				{
					FrameworkElement child = this.children[i];
					CellValuePresenter cvp = child as CellValuePresenter;

					if (cvp == null)
					{
						// check for cellpresenters
						CellPresenter cp = child as CellPresenter;

						// get the associated CellValuePresenter
						if (cp != null)
							cvp = cp.CellValuePresenter;
					}

					// call the cvp's OnRecordMouseOverChanged metjod with the new state
					if (cvp != null)
						cvp.OnRecordMouseOverChanged(isMouseOverCellArea.Value);
				}
			}
		}

		#endregion //OnRecordMouseOverChanged

        // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
        #region OnRequestBringIntoView
        private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
			#region Refactored
			
#region Infragistics Source Cleanup (Region)

























































































































































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Refactored

			// skip when the cell panel (which includes our code within this block)
			// is requesting to be brought into view. we only want to adjust requests
			// from descendants
			if (e.OriginalSource == sender)
				return;

			VirtualizingDataRecordCellPanel cellPanel = (VirtualizingDataRecordCellPanel)sender;

			Visual v = e.TargetObject as Visual;
			Rect targetRect = e.TargetRect;
			FieldLayout fl = cellPanel._fieldLayout;

			if (fl == null || v == null)
				return;

			bool isHorz = null != fl ? fl.IsHorizontal : false;

			// get the cell element containing the object requesting to be brought into view
			FrameworkElement directDescendant = GridUtilities.GetImmediateDescendant(v, cellPanel) as FrameworkElement;

			if (null == directDescendant || directDescendant is FixedFieldSplitter)
				return;

			// AS 4/12/11 TFS62951
			// If the cell we are initializing is the active cell then during the InitializeField it 
			// tries to focus itself. That causes a request bring into view for the cell but since 
			// we haven't yet arranged this to-be-recycled-element it is still in the old location 
			// which causes us to scroll the old location into view.
			//
			if (directDescendant == cellPanel._childBeingInitialized)
				return;

			Field field = GridUtilities.GetFieldFromControl(directDescendant);

			if (null == field)
				return;

			UIElement targetElement = v as UIElement;

			if (!targetRect.IsEmpty || null != targetElement)
			{
				if (targetRect.IsEmpty)
					targetRect = new Rect(targetElement.RenderSize);

				// AS 7/27/09 NA 2009.2 Field Sizing
				bool isTargetFocused = directDescendant.IsKeyboardFocused || directDescendant.IsKeyboardFocusWithin;

				if (cellPanel._usingFixedInfo)
				{
					FieldGridBagLayoutManager lm = cellPanel.GetLayoutManager();
					FieldLayoutItemBase layoutItem = lm.GetLayoutItem(field, cellPanel._isHeader) ??
						lm.GetLayoutItem(field, !cellPanel._isHeader);

					if (null != layoutItem && layoutItem.FixedLocation != FixedFieldLocation.Scrollable)
					{
						e.Handled = true;
						return;
					}

					// get the coordinates relative to the panel
					targetRect = v.TransformToAncestor(cellPanel).TransformBounds(targetRect);

					Rect origRect = targetRect;
					Rect clipRect = cellPanel._scrollableClipRect;

					#region Adjust TargetRect
					if (isHorz)
					{
						double adjustment = 0;

						// AS 7/23/09 [Start]
						//if (targetRect.Bottom > clipRect.Bottom)
						//    adjustment += -cellPanel._fixedFarRect.Height;
						if (v != directDescendant && v is UIElement)
						{
							Rect offset = v.TransformToAncestor(directDescendant).TransformBounds(new Rect(((UIElement)v).RenderSize));

							// adjust the cliprect based on where the descendant is within our direct child
							clipRect.Height = Math.Max(clipRect.Height - (directDescendant.ActualHeight - offset.Height), 0);
							clipRect.Y += offset.Y;
						}

						if (targetRect.Height > clipRect.Height)
							targetRect.Height = clipRect.Height;
						// AS 7/23/09 [End]

						if (targetRect.Y < clipRect.Y)
						{
							adjustment += cellPanel._fixedNearRect.Height;

							// AS 8/21/09 TFS19388
							//Record rc = null != cellPanel._containingRp ? cellPanel._containingRp.Record : null;
							Record rc = null != cellPanel._containingRp ? cellPanel._containingRp.GetRecordForMeasure() : null;

							if (null != rc)
								adjustment += rc.CalculateNestedRecordContentNearOffset();

							if (null != fl && fl.RecordSelectorLocationResolved == RecordSelectorLocation.AboveCellArea)
								adjustment += fl.RecordSelectorExtentResolved;
						}
						// AS 7/23/09 - Moved from above
						else if (targetRect.Bottom > clipRect.Bottom)
							adjustment += -cellPanel._fixedFarRect.Height;

						targetRect.Y -= adjustment;
					}
					else
					{
						double adjustment = 0;

						// AS 7/23/09 [Start]
						// I found some issues with navigation to large cells when implementing
						// the autosize in 9.2. Essentially if there is a large cell (larger than 
						// the clip/scrollable rect) we may try to bring the right side of it in 
						// view. Another case was that an element within the cell may ask to be 
						// brought into view and it may be wider than the clip/scrollable rect 
						// so that might cause the gridviewpanel to shift so that its left is 
						// just within view so we need to account for the offset between the 
						// direct descendant and the element requesting to be brought into view
						//
						//if (targetRect.Right > clipRect.Right)
						//    adjustment += -cellPanel._fixedFarRect.Width;
						if (v != directDescendant && v is UIElement)
						{
							Rect offset = v.TransformToAncestor(directDescendant).TransformBounds(new Rect(((UIElement)v).RenderSize));

							// adjust the cliprect based on where the descendant is within our direct child
							clipRect.Width = Math.Max(clipRect.Width - (directDescendant.ActualWidth - offset.Width), 0);
							clipRect.X += offset.X;
						}

						if (targetRect.Width > clipRect.Width)
							targetRect.Width = clipRect.Width;
						// AS 7/23/09 [End]

						if (targetRect.X < clipRect.X)
						{
							adjustment += cellPanel._fixedNearRect.Width;

							Record rc = null != cellPanel._containingRp ? cellPanel._containingRp.Record : null;

							if (null != rc)
								adjustment += rc.CalculateNestedRecordContentNearOffset();

							if (null != fl && fl.RecordSelectorLocationResolved == RecordSelectorLocation.LeftOfCellArea)
								adjustment += fl.RecordSelectorExtentResolved;
						}
						// AS 7/23/09 - Moved from above
						else if (targetRect.Right > clipRect.Right)
							adjustment += -cellPanel._fixedFarRect.Width;

						targetRect.X -= adjustment;
					}
					#endregion //Adjust TargetRect

					// AS 7/23/09
					// In addition, we always want to make the request on our behalf since the element
					// making the request may end up getting reused/moved before the ancestor processes 
					// it or its interpretation of an empty rect could be different.
					//
					//if (origRect != targetRect)
					// AS 7/27/09 NA 2009.2 Field Sizing
					// We need handle requests from the edit cell specially whether we are using fixing or not.
					//
					//{
					//    e.Handled = true;
					//    cellPanel.BringIntoView(targetRect);
					//}
				}
				else
				{
					if (!isTargetFocused)
					{
						// let edit mode end if needed
						return;
					}

					// get the coordinates relative to the panel
					targetRect = v.TransformToAncestor(cellPanel).TransformBounds(targetRect);
				}

				DataPresenterBase dp = fl.DataPresenter;

				try
				{
					// if the request is coming from within the focused cell then we 
					// want to prevent exiting edit mode in response to this request
					if (null != dp && isTargetFocused)
						dp.SuspendEndEditOnScroll();

					e.Handled = true;
					cellPanel.BringIntoView(targetRect);
				}
				finally
				{
					if (null != dp && isTargetFocused)
						dp.ResumeEndEditOnScroll();
				}
			}
		}

        #endregion //OnRequestBringIntoView

        // JM 12/20/08 NA 2009 Vol 1 - Fixed Fields
        #region ProcessFixedFieldSplitterAction

        internal void ProcessFixedFieldSplitterAction(List<Field> affectedFields, FixedFieldSplitterType splitterType)
		{
            if (affectedFields == null || affectedFields.Count == 0)
                return;

            LayoutInfo layoutInfo = null;
            // ensure that we have create a LayoutInfo for the field layout. if we haven't
            // done a drag or fixed a field outside of initialization then we won't have one yet

			// SSP 6/26/09 - NAS9.2 Field Chooser
			// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
			// directly accessing the member var. Commented out the original code and
			// replaced it with GetFieldLayoutInfo that does the same thing. Also we
			// want to clone the layout information rather than directly manipulating
			// the layout info stored on the field layout. Later we are copying over
			// the new layout info to the field layout.
			// 
			// ------------------------------------------------------------------------
			layoutInfo = _fieldLayout.GetFieldLayoutInfo( true, true ).Clone( true );
			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------




            FixedFieldLocation oldLocation, newLocation;
            GetSplitterLocations(affectedFields, splitterType, out oldLocation, out newLocation);

            DataPresenterBase dp = _fieldLayout.DataPresenter;
            FieldPositionChangeReason changeReason = newLocation == FixedFieldLocation.Scrollable
                ? FieldPositionChangeReason.Unfixed
                : FieldPositionChangeReason.Fixed;

			// AS 3/10/11 NA 2011.1 - Async Exporting
			if (!dp.VerifyOperationIsAllowed(UIOperation.FieldFixing))
				return;

            #region Raise FieldPositionChanging
            for (int i = affectedFields.Count - 1; i >= 0; i--)
            {
                Field field = affectedFields[i];

                bool removeField = !field.IsFixedLocationAllowed(newLocation) ||
                    field.FixedLocation == newLocation;

                if (!removeField)
                {
                    // raise the field position changing and exclude any cancel
                    if (null != dp)
                    {
                        FieldPositionChangingEventArgs beforeArgs = new FieldPositionChangingEventArgs(affectedFields[i], changeReason);
                        dp.RaiseFieldPositionChanging(beforeArgs);

                        removeField = beforeArgs.Cancel;
                    }
                }

                if (removeField)
                    affectedFields.RemoveAt(i);
            } 
            #endregion //Raise FieldPositionChanging

            if (affectedFields.Count == 0)
                return;

            layoutInfo.UpdateFixedLocation(oldLocation, newLocation, affectedFields);

			// AS 6/4/09 NA 2009.2 Undo/Redo
			if (null != dp && dp.IsUndoEnabled)
			{
				FieldPositionAction action = new FieldPositionAction(
					affectedFields,
					// SSP 6/26/09 - NAS9.2 Field Chooser
					// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
					// directly accessing the member var.
					// 
					//_fieldLayout._dragFieldLayoutInfo, 
					_fieldLayout.GetFieldLayoutInfo( true, true ), 
					FieldPositionAction.GetUndoReason(changeReason), 
					oldLocation);

				dp.History.AddUndoActionInternal(action);
			}

			// AS 6/4/09
			// Moved this from above.
			//

			// SSP 6/26/09 - NAS9.2 Field Chooser
			// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
			// directly accessing the member var.
			// 
			//_fieldLayout._dragFieldLayoutInfo = layoutInfo;
			_fieldLayout.SetFieldLayoutInfo( layoutInfo, true, false );


            #region Raise FieldPositionChanged
            for (int i = affectedFields.Count - 1; i >= 0; i--)
            {
                
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

                affectedFields[i].SetFixedLocation(newLocation);
            } 
            #endregion //Raise FieldPositionChanged

            // AS 3/3/09 Optimization
            // We don't need to invalidate all the styles - just this field layout's.
            //
            //if (null != dp)
            //    dp.InvalidateGeneratedStyles(true, false);
            if (null != _fieldLayout)
				// AS 7/7/09 TFS19145/Optimization
				// We shouldn't need to bump the internal version for a position change just 
				// like we don't need to when the field visibility changes.
				//
				//_fieldLayout.InvalidateGeneratedStyles(true, false);
				_fieldLayout.InvalidateGeneratedStyles(false, false);

            // AS 3/3/09 Optimization
            // Moved this down to be consistent with the dragmanager handling of
            // firing after it has been updated. Also, we don't want to fire
            // for one before the fixed location is changed for another.
            //
            #region Raise FieldPositionChanged
            if (null != dp)
            {
                for (int i = affectedFields.Count - 1; i >= 0; i--)
                {
                    FieldPositionChangedEventArgs afterArgs = new FieldPositionChangedEventArgs(affectedFields[i], changeReason);
                    dp.RaiseFieldPositionChanged(afterArgs);
                }
            }
            #endregion //Raise FieldPositionChanged
		}

		#endregion //ProcessFixedFieldSplitterAction

		#region Test code



#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)


        #endregion //Test code	

        // JJD 9/19/08 - added support for printing
        // returns true if the cached value has changed
        #region RefreshReportLayoutInfo


        private bool RefreshReportLayoutInfo()
        {
            ReportLayoutInfo oldRli = this._reportLayoutInfo;

            if (this._fieldLayout != null)
                this._reportLayoutInfo = this._fieldLayout.TemplateDataRecordCache.GetReportLayoutInfoForRecord(this._record, this._normalizedOffsetFromPanel);
            else
                this._reportLayoutInfo = null;

            return oldRli != this._reportLayoutInfo;
        }

        #endregion //RefreshReportLayoutInfo

		// AS 2/26/10 TFS28477
		#region ReleaseCachedDimensions
		internal void ReleaseCachedDimensions()
		{
			_cachedDimensions = null;
		}
		#endregion //ReleaseCachedDimensions

		#region RemoveCellElements

        // SSP 4/7/08 - Summaries Functionality
		// 
		private void RemoveCellElements( )
		{
			List<FrameworkElement> children = this.children;
			for ( int i = children.Count - 1; i >= 0; i-- )
			{
				FrameworkElement elem = children[i];

                // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
                if (elem is FixedFieldSplitter)
                    continue;

                // AS 1/7/09
                // I moved this up from below. At least for RemoveLogicalChild, 
                // the framework will ask for the LogicalChildren during the 
                // call in which case it will think it has this element 
                // as a logical child when it really doesn't.
                //
                children.RemoveAt(i);

				this.RemoveVisualChild( elem );
				this.RemoveLogicalChild( elem );

                // AS 1/7/09
                // Moved up.
				//children.RemoveAt( i );
			}

			// AS 6/21/11 TFS77675
			// Dirty the peer associated with the record if we release cell elements.
			//
			if (null != _record)
				_record.InvalidatePeer();

			_cellElementsDirty = true;

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            this.InvalidateLayoutManager();
		}

		
#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)

		#endregion //RemoveCellElements

        // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
        #region RemoveChild
        private void RemoveChild(FrameworkElement child)
        {
            int index = this.children.IndexOf(child);

            if (index >= 0)
            {
                this.children.RemoveAt(index);
                this.RemoveVisualChild(child);
                this.RemoveLogicalChild(child);
            }
        }
        #endregion //RemoveChild

        #region RemoveSplitter
        // AS 2/3/09 NA 2009 Vol 1 - Fixed Fields
        // Refactored from the VerifyFixedState to avoid duplicate code.
        //
        private void RemoveSplitter(ref FixedFieldSplitter splitter)
        {
            if (null != splitter)
            {
                FixedFieldSplitter oldSplitter = splitter;
                splitter = null;
                this.RemoveLogicalChild(oldSplitter);
                this.RemoveVisualChild(oldSplitter);
            }
        }
        #endregion //RemoveSplitter

		// AS 7/7/09 TFS19145
		// This routine is meant to remove only the cell elements associated with fields that were removed
		// from the layout and reposition the reusable elements within the new templatecellindex.
		//
		#region ReuseCellElements
		private bool ReuseCellElements(IList<Field> newVirtFields, IList<Field> newNonVirtFields)
		{
			if (null == _cellElements)
				return false;

			if (_cellElementsDirty)
				return false;

			Record record = this._record;

			if (null == record && !this._isHeader)
				return false;

			// AS 9/28/09 TFS22532
			//// AS 9/17/09 TFS22285
			//_fieldsVersion = _fieldLayout.Fields.Version;
			int fieldVersion = _fieldLayout.Fields.Version;

			if (fieldVersion != _fieldsVersion)
			{
				this.RemoveInvalidFieldElements();
				_fieldsVersion = fieldVersion;
			}

			Control[] oldCellElements = _cellElements;

			TemplateDataRecordCache cache = _fieldLayout.TemplateDataRecordCache;
			int newCellCount = cache.GetCellCount();
			_cellElements = new Control[newCellCount];

			// JM 10/7/10 - Noticed this was missing when trying to fix TFS56631 - this code should have 
			// been added as part of the changes for TFS37596
			// Create the collection of cached cell element measure sizes.
			_cellElementMeasureSizes = new Size?[newCellCount];

			_allocatedCellCount = newCellCount;
			_cellElementRects = new Rect[newCellCount];

			// AS 3/15/10
			// Joe found an issue while adding some optimizations where reusing a 
			// filter record when the cache had changed could leave some filter 
			// cells in the record when they were no longer allowing filtering.
			// To avoid this we need to see which fields have been removed to ensure 
			// that elements for such a field are removed from the panel.
			//
			HashSet removedFields = new HashSet();
			removedFields.AddItems(_nonVirtualizedFields);
			removedFields.AddItems(_virtualizedFields);

			foreach (Field field in newVirtFields)
				removedFields.Remove(field);

			foreach (Field field in newNonVirtFields)
				removedFields.Remove(field);

			// store the updated field lists
			_virtualizedFields = newVirtFields;
			_nonVirtualizedFields = newNonVirtFields;

			Dictionary<object, Stack<Control>> elementsToRemove = _isRecycling ? new Dictionary<object, Stack<Control>>() : null;

			// remove any old cell elements that aren't being used
			for (int i = 0; i < oldCellElements.Length; i++)
			{
				Control ctrl = oldCellElements[i];

				if (ctrl == null)
					continue;

				Field field = GridUtilities.GetFieldFromControl(ctrl);
				int newIndex = field.TemplateCellIndex;

				// if the element isn't needed or somehow we already recreated
				// an element for it...
				// AS 3/15/10
				// We also want to mark the element for removal if the currently associated 
				// field is no longer a virtualized or non-virtualized field - i.e. it was removed.
				//
				//bool removeElement = newIndex < 0 || newIndex >= newCellCount || _cellElements[newIndex] != null;
				bool removeElement = newIndex < 0 || newIndex >= newCellCount || _cellElements[newIndex] != null || removedFields.Exists(field);

				if (removeElement)
				{
					// if we're not recycling then just get rid of the element
					if (elementsToRemove == null)
						RemoveElement(ctrl);
					else
					{
						object key = GetFieldKey(field);
						Stack<Control> controls;

						if (!elementsToRemove.TryGetValue(key, out controls))
							elementsToRemove[key] = controls = new Stack<Control>();

						controls.Push(ctrl);
					}
				}
				else
				{
					// otherwise store it in the new index
					_cellElements[newIndex] = ctrl;
					_allocatedCellCount--;
				}
			}

			// JJD 3/11/11 - TFS67970 - Optimization
			// If this is not a header and it is a data record then get the IsMouseOver state
			bool? isMouseOverCellArea = this.IsMouseOverCellArea;

			// AS 9/28/09 TFS22532
			//if (elementsToRemove.Count > 0)
			if (null != elementsToRemove && elementsToRemove.Count > 0)
			{
				// AS 8/24/09 TFS19532
				// While debugging this I found that we were reusing a summary element 
				// for a field that didn't have any summaries.
				//
				SummaryRecord summaryRecord = _record as SummaryRecord;

				for (int i = 0; i < _cellElements.Length; i++)
				{
					// if there is a slot that is not being used then find an element 
					// that uses the same key value
					if (_cellElements[i] == null)
					{
						Field targetField = cache.GetField(i);
						object targetKey = GetFieldKey(targetField);

						// AS 8/24/09 TFS19532
						// While debugging this I found that we were reusing a summary element 
						// for a field that didn't have any summaries.
						//
						if (null != summaryRecord)
						{
							if (!summaryRecord.HasFixedSummaryResults(targetField))
								continue;
						}

						// AS 3/15/10
						if (removedFields.Exists(targetField))
							continue;

						Stack<Control> controls;

						if (elementsToRemove.TryGetValue(targetKey, out controls))
						{
							// if we have a control then reinitialize it with the target field
							// and store it as the control for that field
							Control ctrl = controls.Pop();

							// JJD 3/11/11 - TFS67970 - Optimization
							// Added isMouseOverCellArea
							//InitializeCellElement(ctrl, targetField);
							InitializeCellElement(ctrl, targetField, isMouseOverCellArea);

							_cellElements[i] = ctrl;
							_allocatedCellCount--;

							if (controls.Count == 0)
							{
								elementsToRemove.Remove(targetKey);

								if (elementsToRemove.Count == 0)
									break;
							}
						}
					}
				}

				if (elementsToRemove.Count > 0)
				{
					foreach (Stack<Control> controls in elementsToRemove.Values)
					{
						foreach (Control ctrl in controls)
							RemoveElement(ctrl);
					}
				}
			}

			bool useCellPresenters = _fieldLayout.UseCellPresenters;

			// lastly make sure all nonvirtfields are allocated
			for (int i = 0, count = newNonVirtFields.Count; i < count; i++)
			{
				Field fld = newNonVirtFields[i];
				int newIndex = fld.TemplateCellIndex;

				if (_cellElements[newIndex] == null)
				{
					// JJD 3/11/11 - TFS67970 - Optimization
					// Added isMouseOverCellArea
					//this.CreateCellElement(newIndex, fld, useCellPresenters);
					this.CreateCellElement(newIndex, fld, useCellPresenters, isMouseOverCellArea);
				}
			}

			this.InvalidateLayoutManager();
			return true;
		}
		#endregion //ReuseCellElements

		// AS 7/7/09 TFS19145
		#region RemoveElement
		private void RemoveElement(Control ctrl)
		{
			int oldIndex = children.IndexOf(ctrl);

			Debug.Assert(oldIndex >= 0);

			if (oldIndex >= 0)
			{
				Debug.Assert(!ctrl.IsKeyboardFocused && !ctrl.IsKeyboardFocusWithin);

				children.RemoveAt(oldIndex);

				this.RemoveVisualChild(ctrl);
				this.RemoveLogicalChild(ctrl);
			}
		} 
		#endregion //RemoveElement

		// AS 9/17/09 TFS22285
		#region RemoveInvalidFieldElements
		private void RemoveInvalidFieldElements()
		{
			if (_cellElements == null)
				return;

			for (int i = 0; i < _cellElements.Length; i++)
			{
				Control cellElement = _cellElements[i];

				if (null == cellElement)
					continue;

				Field f = GridUtilities.GetFieldFromControl(cellElement);

				if (null == f || f.Index >= 0)
					continue;

				_cellElements[i] = null;
				this.RemoveChild(cellElement);
			}
		}
		#endregion //RemoveInvalidFieldElements

		// AS 12/9/08 NA 2009 Vol 1 - Fixed Fields
        #region TryGetPreferredExtent
        internal bool TryGetPreferredExtent(FieldLayoutItemBase fieldLayoutItem, bool cell, bool width, out double extent)
        {
            extent = 0;

            Debug.Assert(width == false, "The width should be based on the field's layout item");
            Debug.Assert(null != fieldLayoutItem);

            if (((ILayoutItem)fieldLayoutItem).Visibility == Visibility.Collapsed)
                return false;

			// AS 2/24/10 TFS28341
			DataRecordSizingMode sizingMode = fieldLayoutItem.RecordSizingMode;

            Field field = fieldLayoutItem.Field;

            // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
            // Actually we don't want to check the isvirtualized flag unless this is a header
            // or a datarecord (non-filter).
            //
            if (this._record is SummaryRecord == false && this._record is FilterRecord == false)
            {
                // if the cell is virtualized then do not use the local cell element's measurement
                // to be consistent in the size we return
                if ((cell && field.IsCellVirtualized) || (!cell && field.IsLabelVirtualized))
                    return false;

				// AS 2/22/10 TFS28162
				// If the field has an explicit height then we should use that value.
				//
				if (cell && !double.IsNaN(field.GetCellHeightResolvedHelper(false)))
					return false;

				// AS 2/22/10 TFS28162
				// If the field has an explicit height then we should use that value.
				//
				if (!cell && !double.IsNaN(field.GetLabelHeightResolvedHelper(false, false)))
					return false;

				// AS 2/24/10 TFS28341
				// Do not return a size based on the actual cell unless we are sized to content.
				// Normally we would not have gotten into here if the record was individually 
				// sizable. We got in here in this case because cells could be collapsed and then 
				// we ignore the ForceUseCellLayoutManager. However even then there was a bug because 
				// we would have gotten in here if the cells were resized. We needed to use the record 
				// layout manager in that case but we don't want to use the size of the actual cell 
				// elements.
				//
				switch (sizingMode)
				{
					case DataRecordSizingMode.SizedToContentAndFixed:
					case DataRecordSizingMode.SizedToContentAndIndividuallySizable:
						break;
					case DataRecordSizingMode.IndividuallySizable:
					default:
						// specifically we want to make sure that if the record is 
						// individually sizable (and not sized to content) that we 
						// bail out. if we get through here then really this is no 
						// different than sized to content and individually sizable
						return false;
				}
			}

            FieldLayout fl = field.Owner;

            // we don't want the cells to be different heights in a horizontal layout. we want
            // to use the shared size info
            if (fl == null || fl.IsHorizontal)
                return false;

			// AS 1/25/10 SizableSynchronized changes
			if (!GridUtilities.IsVariableHeightRecordMode(sizingMode))
				return false;

            this.VerifyCellElements();

            int templateIndex = field.TemplateCellIndex;
            Debug.Assert(templateIndex >= 0 && templateIndex < this._cellElements.Length);

            if (templateIndex < 0 || templateIndex >= this._cellElements.Length)
                return false;

            FieldGridBagLayoutManager lm = this.GetLayoutManager();

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            // For filter records, we're doing virtualization a little different since we don't 
            // want to hydrate a filter record in the template cache. Instead, if the field is 
            // virtualized then we will get the mapped field's element instead.
            //
            if (_record is FilterRecord && field.IsFilterCellVirtualized)
            {
                Field measureField = fl.TemplateDataRecordCache.GetFilterFieldToMeasure(field);
                Debug.Assert(null != measureField);

                if (null != measureField)
                {
                    FieldLayoutItemBase measureFieldItem = lm.GetLayoutItem(measureField, !cell);
                    Debug.Assert(null != measureFieldItem);
                    extent = measureFieldItem.PreferredHeight;
                    return true;
                }
            }

            FrameworkElement element = this._cellElements[templateIndex];

            if (null == element)
            {
                
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


                return false;
            }

            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
            // We cannot use the field's preferred width because that may not be what the field will 
            // actually use for its width. The field may span several columns or even when spanning 
            // only 1, another field may be above/below it and affect the resolved width. Instead we 
            // need to get the extent from the fieldlayout's gridbagmanager.
            //
            //Size newSize = new Size(fieldLayoutItem.PreferredWidth, double.PositiveInfinity);
            double preferredWidth = fieldLayoutItem.PreferredWidth;

            Debug.Assert(null != _cachedDimensions);

            if (null != _cachedDimensions)
            {
                IGridBagConstraint gcField = lm.LayoutItems.GetConstraint(fieldLayoutItem) as IGridBagConstraint;
                Debug.Assert(gcField != null);
                Debug.Assert(gcField.Column + gcField.ColumnSpan < _cachedDimensions.ColumnDims.Length);

				if (null != gcField && gcField.Column + gcField.ColumnSpan < _cachedDimensions.ColumnDims.Length)
				{
					// AS 11/9/11 TFS91453
					// The subtraction could result in rounding issues so if the value is 
					// close so only use it if it is different.
					//
					//preferredWidth = _cachedDimensions.ColumnDims[gcField.Column + gcField.ColumnSpan] - _cachedDimensions.ColumnDims[gcField.Column];
					double tempWidth = _cachedDimensions.ColumnDims[gcField.Column + gcField.ColumnSpan] - _cachedDimensions.ColumnDims[gcField.Column];

					if (!CoreUtilities.AreClose(preferredWidth, tempWidth))
						preferredWidth = tempWidth;
				}
            }

            CellPresenterBase cp = element as CellPresenterBase;

            if (null != cp)
                cp.VerifyLayoutElements();

			// AS 11/16/11 TFS95167
			// Moved to a helper routine. If the field represents an auto sized field (i.e. a field that sizes based on 
			// its content) then we should try to use an infinite width if that stays within the constraint we were using. 
			// Otherwise if we use a constrained size and something happens to the element such that its desired size 
			// could change to a larger value, it could not because it would be constrained by the last measure size.
			//
			//Size newSize = new Size(preferredWidth, double.PositiveInfinity);
			//
			//// MD 9/10/10 - TFS37596
			//// Cache the size with which we are about to measure the cell.
			//_cellElementMeasureSizes[templateIndex] = newSize;
			//
			//element.Measure(newSize);
			//
			//Size desired;
            //
			//if (null != cp)
			//{
			//    FrameworkElement childElement = cp.GetChild(fieldLayoutItem.IsLabel);
			//
			//    Debug.Assert(null != childElement || ((ILayoutItem)fieldLayoutItem).Visibility != Visibility.Visible);
			//
			//    if (null != childElement)
			//        desired = childElement.DesiredSize;
			//    else
			//        desired = new Size();
			//}
			//else
			//    desired = element.DesiredSize;
			//
			//extent = width ? desired.Width : desired.Height;
			bool isAutoFit = AutoSizeFieldLayoutInfo.IsInAutoSizeMode(field, !cell, width);

			if (isAutoFit)
			{
				extent = TryGetPreferredExtentImpl(fieldLayoutItem, new Size(double.PositiveInfinity, double.PositiveInfinity), element, templateIndex, width);

				if (Utilities.LessThanOrClose(extent, preferredWidth))
					return true;
			}

			Size newSize = new Size(preferredWidth, double.PositiveInfinity);
			extent = TryGetPreferredExtentImpl(fieldLayoutItem, newSize, element, templateIndex, width);
			return true;
        }

		// AS 11/16/11 TFS95167
		// Moved a portion of the TryGetPreferredExtent to a separate routine.
		//
		private double TryGetPreferredExtentImpl(FieldLayoutItemBase fieldLayoutItem, Size newSize, FrameworkElement element, int templateIndex, bool width)
		{
			// MD 9/10/10 - TFS37596
			// Cache the size with which we are about to measure the cell.
			_cellElementMeasureSizes[templateIndex] = newSize;

			element.Measure(newSize);

			Size desired;
			CellPresenterBase cp = element as CellPresenterBase;
			
			if (null != cp)
			{
				FrameworkElement childElement = cp.GetChild(fieldLayoutItem.IsLabel);

				Debug.Assert(null != childElement || ((ILayoutItem)fieldLayoutItem).Visibility != Visibility.Visible);

				if (null != childElement)
					desired = childElement.DesiredSize;
				else
					desired = new Size();
			}
			else
				desired = element.DesiredSize;

			return width ? desired.Width : desired.Height;
		}
        #endregion //TryGetPreferredExtent

		#region VerifyCellsInView
        private void VerifyCellsInView(Size elementSize)
		{
			#region Setup

            // JJD 6/11/08 - BR31960
            // Cache the _requiresAsynchMeasureInvalidation flag in a stack variable
            bool isInAsyncMeasureArrangePass = this._requiresAsynchMeasureInvalidation;

            // JJD 6/11/08 - BR31960
            // reset the flag
            this._requiresAsynchMeasureInvalidation = false;

			Debug.Assert(this._fieldLayout != null);

			if (this._fieldLayout == null)
				return;

            // AS 2/3/09 Remove unused members
			//FieldLayoutTemplateGenerator generator = this._fieldLayout.StyleGenerator;
            //TemplateDataRecordCache templateRecordCache = this._fieldLayout.TemplateDataRecordCache;
            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
            //Grid itemGrid = templateRecordCache.GetCellGrid();

			// SSP 2/2/10
			// Added CellsInViewChanged event to the DataPresenterBase. Moved the following 
			// code that updates the _lastClipRect here from below.
			//
			
			DataPresenterBase dataPresenter = _fieldLayout.DataPresenter;

			// start with the cell area clip rect
			Rect clipRect = GetClipRect(elementSize);

			// cache it so we can check it again when the layout is updated
			this._lastClipRect = clipRect;
			

			// AS 5/4/07
			// If there are no virtualized cells then there is nothing to verify.
			//
			// JJD 5/4/07 - Optimization
			// Added support for label virtualization
			//if (templateRecordCache.VirtualizedCellFields.Count == 0)
			IList<Field> virtualizedFields = this.VirtualizedFields;
            if (virtualizedFields.Count == 0)
            {

                // JJD 9/20/08
                // Always wire up the LayoutUpdated event so we can verify the offset used
                // AS 1/16/09 NA 2009 Vol 1 - Fixed Fields
                // We also need to get into the layout updated if we're
                // using fixed fields even if we're not using any 
                // virtualized cells.
                //
                //if (this._reportLayoutInfo != null)
                if (this._reportLayoutInfo != null || _usingFixedInfo
					// SSP 2/2/10
					// Added CellsInViewChanged event to the DataPresenterBase. Also wire into LayoutUpdated
					// if we need to raise that event.
					// 
					|| null != dataPresenter && dataPresenter.ShouldRaiseCellsInViewChanged
					)
                    this.WireLayouUpdatedEvent();

                return;
            }

			bool isHorizontal = this._fieldLayout.IsHorizontal;
			// AS 5/4/07 Optimization
			//DataRecord dr = this.DataContext as DataRecord;
			// SSP 4/7/08 - Summaries Functionality
			// Changed to Record to support summary record.
			// 
			//DataRecord dr = this._record;
			Record record = this._record;

			Debug.Assert(record != null || this._isHeader);

			if (record == null && this._isHeader == false)
				return;

			Debug.Assert(this._cellElements != null);

			if (null == this._cellElements)
				return;

            // JJD 9/20/08
            // Moved logic into WireLayouUpdatedEvent
            this.WireLayouUpdatedEvent();

            // JJD 6/11/08 - BR31960
            // Create a flag to we know if we need to re-measure
            bool requiresMeasurePass = false;

            // JJD 6/11/08 - BR31960
            // Kep trak of the bottomost label bottom value
            double highestLabelBottom = 0;

            
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			// SSP 2/2/10
			// Added CellsInViewChanged event to the DataPresenterBase. Moved the following code above.
			// 
			
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			// AS 4/27/07 Performance
			// if there are no cells allocated and we don't have any rect to give the elements
			// then don't bother...
			bool isEmptyClip = clipRect.Width <= 0 && clipRect.Height <= 0;

			if (isEmptyClip && this._allocatedCellCount == this._cellElements.Length)
				return;

			// AS 4/27/07 Performance
			// Use a local reference to the array.
			//
			Control[] cellElements = this._cellElements;

			// JJD 3/11/11 - TFS67970 - Optimization
			// If this is not a header and it is a data record then get the IsMouseOver state
			bool? isMouseOverCellArea = this.IsMouseOverCellArea;

			// AS 5/4/07
			//List<Field> virtualizedFields = templateRecordCache.VirtualizedCellFields;

			#endregion //Setup

			// AS 5/8/07 Optimization
			bool wasVerifying = this._isVerifyingCells;
			this._isVerifyingCells = true;

            // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
            bool usingFixedInfo = _usingFixedInfo;

//            Debug.WriteLine("VirtualizingDataRecordCellPanel begin VerifyCellsInView size: " + elementSize.ToString());

			// AS 3/10/11 Optimization/TFS66927
			// Exclude what we consider the clipped rect for scrollable fields. Note 
			// we can't just manipulate the clipRect itself since this routine does 
			// create/position the fixed virtualized elements as well and we also 
			// need the actual clip rect so we know when to force an element to 
			// be arranged if it was in view, isn't in the scrollable area but would 
			// be in the viewable area (i.e. within the actual clip rect).
			//
			Rect scrollableClipRect = clipRect;

			if (usingFixedInfo && !scrollableClipRect.IsEmpty)
			{
				if (isHorizontal)
				{
					scrollableClipRect.Y += _fixedNearRect.Height;
					scrollableClipRect.Height = Math.Max(clipRect.Height - (_fixedNearRect.Height + _fixedFarRect.Height), 0);
				}
				else
				{
					scrollableClipRect.X += _fixedNearRect.Width;
					scrollableClipRect.Width = Math.Max(clipRect.Width - (_fixedNearRect.Width + _fixedFarRect.Width), 0);
				}
			}

			try
			{
				// AS 5/24/07 Recycle elements
				#region Obsolete code
				
#region Infragistics Source Cleanup (Region)












































































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)


































#endregion // Infragistics Source Cleanup (Region)

				#endregion //Obsolete code

				int count = virtualizedFields.Count;
				LinkedList<Control> reusableElements = null;
				// AS 11/29/10 TFS60418
				// Previously we were building an array equivalent to the # of virtualized fields
				// but really only need the information for the ones we considered in view. So now 
				// we will just build a list of the indexes of the virtual field indexes we need 
				// to position.
				//
				//Rect[] elementRects = new Rect[count];
				List<int> inViewFields = new List<int>();

				// AS 11/29/10 TFS60418
				// We were doing this within the loop but the state doesn't change during iteration
				// so its better to determine it ahead of time.
				//
				bool calculatehighestLabelBottom = _isHeader && !isHorizontal;

				// this is now a two pass operation. the first pass will see
				// which elements can be reused and where they will be positioned.
				//
				#region Pass #1 - Identify reusable elements and element positions
				for (int i = 0; i < count; i++)
				{
					// AS 5/4/07
					Field field = virtualizedFields[i];
					int cellIndex = field.TemplateCellIndex;

					// AS 11/29/10 TFS60418
					//elementRects[i] = Rect.Empty;

					Control cellElement = cellElements[cellIndex];
					// AS 4/27/07 Performance
					// if there is no clip rect and the cell is not allocated then 
					// skip finding where the cell would be since it won't be in view
					//
					if (isEmptyClip && cellElement == null)
						continue;

                    
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

                    // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
                    Rect templateCellRect = this._cellElementRects[cellIndex];

					// AS 11/29/10 TFS60418
					//if (this._isHeader && !isHorizontal )
					if (calculatehighestLabelBottom)
                    {
                        
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


                        // JJD 6/11/08 - BR31960
                        // Keep track of the highest bottom value
                        highestLabelBottom = Math.Max(highestLabelBottom, templateCellRect.Bottom);
                    }

					// AS 3/10/11 Optimization/TFS66927
					// Use the area excluding the fixed areas for the scrollable elements.
					//
					Rect cellClipRect = clipRect;

					if (usingFixedInfo && field.FixedLocation == FixedFieldLocation.Scrollable)
						cellClipRect = scrollableClipRect;

					bool isInView = templateCellRect.IntersectsWith(cellClipRect);

					// if a cell is no longer in view but was in view then force
					// it to be arranged or else it will remain in view
					if (isInView == false &&
						cellElement != null &&
						LayoutInformation.GetLayoutSlot(cellElement).IntersectsWith(clipRect))
						isInView = true;

					// if its not in view we don't even care if its created yet
					if (isInView == false)
					{
						if (cellElement == null)
							continue;

						if (this._positionAllAllocatedCells == false)
						{
                            // AS 1/26/09 NA 2009 Vol 1 - Fixed Fields
                            // Do not recycle an element that contains the input focus.
                            //
							//if (this._isRecycling)
							if (this._isRecycling && !cellElement.IsKeyboardFocusWithin)
							{
								if (reusableElements == null)
									reusableElements = new LinkedList<Control>();

								reusableElements.AddLast(cellElement);
							}
							continue;
						}
					}

					// AS 11/29/10 TFS60418
					// Build of list of what is in view so we don't have to enumerate
					// over all the fields again below.
					//
					//elementRects[i] = templateCellRect;
					inViewFields.Add(i);
				} 
				#endregion //Pass #1 - Identify reusable elements and element positions

				#region Position and reuse/create elements
				// AS 11/29/10 TFS60418
				// Just loop over the ones in view.
				//
				//for (int i = 0; i < count; i++)
				for (int j = 0, len = inViewFields.Count; j < len; j++)
                {
					// AS 11/29/10 TFS60418
					//Rect templateCellRect = elementRects[i];
					//
					//if (templateCellRect.IsEmpty)
					//    continue;

					// AS 11/29/10 TFS60418
					int i = inViewFields[j];

                    // AS 5/4/07
					Field field = virtualizedFields[i];
                    int cellIndex = field.TemplateCellIndex;

					// AS 11/29/10 TFS60418
					Rect templateCellRect = _cellElementRects[cellIndex];

                    // create the cell if it hasn't been created
                    if (cellElements[cellIndex] == null)
                    {
                        // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                        //// AS 5/24/07 Recycle elements
                        //if (this._isHeader == false && this._isRecycling && null != reusableElements)
                        if (this._isRecycling && null != reusableElements)
                        {
                            for (LinkedListNode<Control> node = reusableElements.First; node != null; node = node.Next)
                            {
                                Field oldField = GetField(node.Value);

                                // AS 1/23/09 NA 2009 Vol 1 - Fixed Fields
                                //if (oldField.CellElementKey.Equals(field.CellElementKey))
                                if (object.Equals(GetFieldKey(oldField), GetFieldKey(field)))
                                {
                                    int oldCellIndex = oldField.TemplateCellIndex;
                                    Control element = cellElements[oldCellIndex];
                                    cellElements[cellIndex] = element;
                                    cellElements[oldCellIndex] = null;

									// JJD 3/11/11 - TFS67970 - Optimization
									// Added isMouseOverCellArea
									//this.InitializeCellElement(element, field);
                                    this.InitializeCellElement(element, field, isMouseOverCellArea);

                                    
                                    
                                    
                                    
                                    
                                    element.InvalidateMeasure();
                                    element.InvalidateArrange();

                                    reusableElements.Remove(node);
                                    break;
                                }
                            }
                        }

						if (cellElements[cellIndex] == null)
						{
							// JJD 3/11/11 - TFS67970 - Optimization
							// Added isMouseOverCellArea
							//this.CreateCellElement(cellIndex, field, this._fieldLayout.UseCellPresenters);
							this.CreateCellElement(cellIndex, field, this._fieldLayout.UseCellPresenters, isMouseOverCellArea);
						}
                    }

                    Control cell = cellElements[cellIndex];

                    // JJD 6/9/08 - BR31962
                    // Constrain the rect so it doesn't extend past the bottom of the record
                    templateCellRect = ConstrainCellRectBottom(templateCellRect, elementSize.Height);

                    // measure, then apply the template and finally arrange the cell
                    // SSP 4/9/08 - Summaries Functionality
                    // Check for null since we'll only create elements for which summaries exist.
                    // 
                    if (null != cell)
                    {
                        // AS 12/19/08 NA 2009 Vol 1 - Fixed Fields
                        if (cell is CellPresenterBase)
                            this.InitializeCellPresenterRects(cell);

                        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
                        if (usingFixedInfo)
                            cell.CoerceValue(UIElement.ClipProperty);

						// MD 9/10/10 - TFS37596
						// Cache the size with which we are about to measure the cell.
						_cellElementMeasureSizes[cellIndex] = templateCellRect.Size;

                        cell.Measure(templateCellRect.Size);
                        cell.Arrange(templateCellRect);
//#if DEBUG
//                    this.OutputMeasure(cell, templateCellRect.Size);
//                    this.OutputArrange(cell, templateCellRect);
//#endif
                    }
                }
				#endregion //Position and reuse/create elements
			}
			finally
			{
				// AS 5/8/07 Optimization
				this._isVerifyingCells = wasVerifying;
            
//                Debug.WriteLine("VirtualizingDataRecordCellPanel end VerifyCellsInView size: " + elementSize.ToString());
            }

            // JJD 6/11/08 - BR31960
            // For headers in a vertical layout if we aren't already in
            // the last asynch measure pass and the requiresMeasurePass is true
            // or the highest label bottom is smaller than the size of the panel
            // then set the flag so we invalidate the measure in OnLayoutUpdated.
            if (this._isHeader && !isHorizontal && !isInAsyncMeasureArrangePass)
            {
                this._requiresAsynchMeasureInvalidation = requiresMeasurePass ||
                    ( highestLabelBottom > 0 && highestLabelBottom < elementSize.Height );
            }
		}

		#endregion //VerifyCellsInView

		// AS 4/27/07
		#region VerifyCellElements





		// AS 3/2/11 66934 - AutoSize
		// Changed to internal so the autosizing logic can invoke it when it first gets the panel.
		//
		//private void VerifyCellElements()
		internal void VerifyCellElements()
		{
			// AS 7/7/09 TFS19145
			this.VerifyFieldLists();

			if (this._cellElementsDirty)
			{
                // AS 2/10/09
                // If we had created cell elements then they would remain in the 
                // visual tree. Since this can now happen when the FilterOperandUIType
                // has changed to/from None for a filter cell, we need to make sure we 
                // release those cells.
                //
                this.RemoveCellElements();

				this._cellElementsDirty = false;

				// AS 5/4/07 Optimization
				//DataRecord dr = this.DataContext as DataRecord;
				// SSP 4/7/08 - Summaries Functionality
				// Changed to Record to support summary record.
				// 
				//DataRecord dr = this._record;
				Record record = this._record;

				// AS 5/11/12 TFS111400
				var fieldLayout = _fieldLayout;

				if (fieldLayout == null)
					return;

				// JJD 5/4/07 - Optimization
				// Added support for label virtualization
				//if (null != dr)
				if (null != record || this._isHeader)
				{
					// AS 9/17/09 TFS22285
					//Debug.Assert(null != _fieldLayout);
					_fieldsVersion = fieldLayout.Fields.Version;

					int cellCount = fieldLayout.TemplateDataRecordCache.GetCellCount();

					// AS 5/11/12 TFS111400
					// Moved up from below.
					//
					IList<Field> nonVirtualizedFields = this.NonVirtualizedFields;

					// AS 5/11/12 TFS111400
					if (_fieldLayout == null || _fieldLayout != fieldLayout || _cellElementsDirty || nonVirtualizedFields == null)
						return;

					this._cellElements = new Control[cellCount];
					this._allocatedCellCount = cellCount;

                    // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
                    this._cellElementRects = new Rect[cellCount];

					// MD 9/10/10 - TFS37596
					// Create the collection of cached cell element measure sizes.
					_cellElementMeasureSizes = new Size?[cellCount];

					// AS 11/29/10 TFS60418
					_lastLayoutContainerRect = null;

					// AS 5/4/07
					// Create the nonvirtualized cells now.
					//
					// JJD 5/4/07 - Optimization
					// Added support for label virtualization
					//List<Field> nonVirtualizedFields = this._fieldLayout.TemplateDataRecordCache.NonVirtualizedCellFields;
					// AS 5/11/12 TFS111400 Moved up
					//IList<Field> nonVirtualizedFields = this.NonVirtualizedFields;
					bool useCellPresenters = fieldLayout.UseCellPresenters;

					// JJD 3/11/11 - TFS67970 - Optimization
					// If this is not a header and it is a data record then get the IsMouseOver state
					bool? isMouseOverCellArea = this.IsMouseOverCellArea;

                    
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


					for (int i = 0, count = nonVirtualizedFields.Count; i < count; i++)
					{
						Field fld = nonVirtualizedFields[i];

						// JJD 3/11/11 - TFS67970 - Optimization
						// Added isMouseOverCellArea
						//this.CreateCellElement(fld.TemplateCellIndex, fld, useCellPresenters);
						this.CreateCellElement(fld.TemplateCellIndex, fld, useCellPresenters, isMouseOverCellArea);
					}
                    
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				}
			}
		} 
		#endregion //VerifyCellElements

        // AS 1/27/09 NA 2009 Vol 1 - Fixed Fields
        #region VerifyClipRect
        private void VerifyClipRect()
        {
            if (HasClipRectChanged())
                this.InvalidateArrange();
        }
        #endregion //VerifyClipRect

        // AS 2/10/09
        #region VerifyFieldLists
        private void VerifyFieldLists()
        {
            IList<Field> oldVirtFields = _virtualizedFields;
            IList<Field> oldNonVirtFields = _nonVirtualizedFields;
            IList<Field> newVirtFields, newNonVirtFields;

            #region Get New Field Lists
            if (_fieldLayout == null)
            {
                newVirtFields = newNonVirtFields = EmptyFields;
            }
            else
            {
                TemplateDataRecordCache recordCache = _fieldLayout.TemplateDataRecordCache;

				// if nothing has changed continue to use the existing lists
				if (recordCache.CacheVersion == _templateCacheVersion)
					return;

				_templateCacheVersion = recordCache.CacheVersion;

                if (!_isHeader && _record is SummaryRecord)
                {
                    newVirtFields = new Field[0];
                    newNonVirtFields = recordCache.AllFields;
                }
                else
                {
                    if (this._isHeader)
                    {
                        newNonVirtFields = recordCache.NonVirtualizedLabelFields;
                        newVirtFields = recordCache.VirtualizedLabelFields;
                    }
                    else if (_record is FilterRecord)
                    {
                        newNonVirtFields = recordCache.NonVirtualizedFilterCellFields;
                        newVirtFields = recordCache.VirtualizedFilterCellFields;
                    }
                    else
                    {
                        newNonVirtFields = recordCache.NonVirtualizedCellFields;
                        newVirtFields = recordCache.VirtualizedCellFields;
                    }
                }
            }
            #endregion //Get New Field Lists

			// AS 8/19/09 TFS20318
			// At least in the case of a filter record, the list may not change since the
			// field added may not need a filter cell but we should still consider the list
			// dirty when our cell count doesn't match the field layout's. Since this is a 
			// quicker/more effecient check we'll do this first and then compare the lists
			// if needed.
			//
			//// if any list has changed then we need to release the cell elements
			//bool hasChanged = !GridUtilities.CompareLists(oldVirtFields, newVirtFields)
			//    || !GridUtilities.CompareLists(oldNonVirtFields, newNonVirtFields);
			bool hasChanged = null != _fieldLayout && (_cellElements == null || GridUtilities.GetCount(_cellElements) != _fieldLayout.TemplateDataRecordCache.GetCellCount());

			if (!hasChanged)
			{
				// if any list has changed then we need to release the cell elements
				hasChanged = !GridUtilities.CompareLists(oldVirtFields, newVirtFields)
					|| !GridUtilities.CompareLists(oldNonVirtFields, newNonVirtFields);
			}

            if (hasChanged)
            {
				// AS 7/7/09 TFS19145 [Start]
				// When the templates have not been regenerated then try to reuse
				// the cell elements.
				//
				FieldLayoutTemplateGenerator generator = _fieldLayout.StyleGenerator;
				bool reuseCells = false;

				if (generator != null)
				{
					if (generator.TemplateVersion != _generatorTemplateVersion)
						_generatorTemplateVersion = generator.TemplateVersion;
					else if (_cellElements != null)
						reuseCells = true;
				}

				if (reuseCells && this.ReuseCellElements(newVirtFields, newNonVirtFields))
					return;
				// AS 7/7/09 TFS19145 [End]

				_virtualizedFields = newVirtFields;
                _nonVirtualizedFields = newNonVirtFields;

                _cellElementsDirty = true;
            }
        }
        #endregion //VerifyFieldLists

        // AS 1/14/09 NA 2009 Vol 1 - Fixed Fields
        #region VerifyFixedFieldOffsets
        private void VerifyFixedFieldOffsets()
        {
            Vector fixedNearOffset = new Vector();
            Vector fixedFarOffset = new Vector();

            // AS 2/3/09
            // Updated this routine to calculate the pre/post cell area spacing separately
            // so we can cache it and use it within the arrange to know if there is any 
            // possible scrolling area.
            //
            double prePostSpacing = double.NaN;

            if (this._fieldLayout != null)
            {
                bool isHorizontal = this._fieldLayout.IsHorizontal;
                double offset = (double)this.GetValue(FixedFieldOffsetProperty);
                double totalExtent = (double)this.GetValue(FixedFieldExtentProperty);
                double fixedAreaExtent = (double)this.GetValue(FixedFieldViewportExtentProperty);

                if (fixedAreaExtent < totalExtent && null != _containingRp)
                {
                    double panelExtent = isHorizontal ? this.ActualHeight : this.ActualWidth;

					double preRecordArea = CalculateFixedRecordAreaEdge(isHorizontal, 0, _containingRp, _isHeader ? null : _record, this, _isHeader);
                    TemplateDataRecordCache templateCache = _fieldLayout.TemplateDataRecordCache;

                    // add in the space for any chrome around the panel within the cell area
                    preRecordArea += templateCache.GetVirtPanelOffset(true, true);

                    // AS 1/22/08
                    // This criteria is incorrect because it assumes that its origin is flush
                    // with the fixed area. The element may be smaller but partially out of 
                    // view because of an offset before/above it.
                    //
                    //if (panelExtent > 0 && panelExtent > fixedAreaExtent)
                    {
                        // we need to include the margins of the record cell area as well
                        // since that will offset the virtualizing cell panel
                        RecordCellAreaBase cellArea = Utilities.GetAncestorFromType(this, typeof(RecordCellAreaBase), true) as RecordCellAreaBase;

                        if (null != cellArea)
                        {
							// AS 9/3/09 TFS21581
							// When there is no record selector, the margins are maintained on the 
							// recordcellareabase element.
							//
                            //FrameworkElement fef = VisualTreeHelper.GetParent(cellArea) as FrameworkElement;
							FrameworkElement fef;

							if (_fieldLayout.RecordSelectorLocationResolved == RecordSelectorLocation.None)
								fef = cellArea;
							else
								fef = VisualTreeHelper.GetParent(cellArea) as FrameworkElement;

                            if (null != fef)
                            {
                                Thickness m = fef.Margin;

                                if (isHorizontal)
                                    preRecordArea += m.Top;
                                else
                                    preRecordArea += m.Left;
                            }
                        }

                        prePostSpacing = preRecordArea;

                        double farEdgeExtent = Math.Min(totalExtent, panelExtent + preRecordArea);

                        double farEdge = -Math.Min(panelExtent, Math.Max(farEdgeExtent - fixedAreaExtent - offset, 0));

                        if (isHorizontal)
                            fixedFarOffset.Y = farEdge;
                        else
                            fixedFarOffset.X = farEdge;
                    }
                }

                if (isHorizontal)
                    fixedNearOffset.Y = offset;
                else
                    fixedNearOffset.X = offset;
            }

            bool hasChanged = _fixedNearOffset != fixedNearOffset || _fixedFarOffset != fixedFarOffset;

            _prePostPanelSpacing = prePostSpacing;

            if (hasChanged)
            {
                _fixedNearOffset = fixedNearOffset;
                _fixedFarOffset = fixedFarOffset;

				// AS 12/9/10 TFS61449
				// Since the cell positions are updated based on the fixed offsets, we need to 
				// ensure we let the positioning logic get processed.
				// 
				_lastLayoutContainerRect = null;

                this.InvalidateArrange();
            }
        } 
        #endregion //VerifyFixedFieldOffsets

        // AS 1/7/09 NA 2009 Vol 1 - Fixed Fields
        #region VerifyFixedState
        private void VerifyFixedState()
        {
			// JJD 3/11/11 - TFS67970 - Optimization
			// Moved here since we always need this to wire the mouse over
			// which eliminates the need to have each cell presenter wire its mouse over 
			RecordCellAreaBase cellArea = Utilities.GetAncestorFromType(this, typeof(RecordCellAreaBase), true) as RecordCellAreaBase;

			if (_containingCellArea != cellArea)
			{
				_containingCellArea = cellArea;

				// JJD 3/11/11 - TFS67970 - Optimization
				// Wire the IsMouseOver instead of having each CellValuePresenter wire it up
				if (cellArea is DataRecordCellArea)
					this._recordMouseOverTracker = new PropertyValueTracker(this._containingCellArea, "IsMouseOver", this.OnRecordMouseOverChanged);
				else
					this._recordMouseOverTracker = null;
			}

			FixedFieldLayoutInfo fixedFieldInfo = _fieldLayout != null ? _fieldLayout.GetFixedFieldInfo(true) : null;

            if (null != fixedFieldInfo)
            {
                bool oldValue = _usingFixedInfo;
                _usingFixedInfo = fixedFieldInfo.HasFixedFields || fixedFieldInfo.HasFixableFields;

                if (oldValue != _usingFixedInfo)
                {
					// AS 12/9/10 TFS61449
					_lastLayoutContainerRect = null;

                    // since we only coerce the clip of the cells when we are using fixed info
                    // we need to explicitly coerce any when this state goes to false
                    if (!_usingFixedInfo)
                    {
                        if (null != _cellElements)
                        {
                            foreach (Control element in this._cellElements)
                            {
                                if (null != element)
                                    element.CoerceValue(UIElement.ClipProperty);
                            }
                        }

                        this.ClearValue(FixedFieldInfoProperty);
                    }
                    else if (_usingFixedInfo)
                    {
						// JJD 3/11/11 - TFS67970 - Optimization
						// Moved logic above since we always need this to wire the mouse over
						// which eliminates the need to have each cell presenter wire its mouse over 
                        //_containingCellArea = Utilities.GetAncestorFromType(this, typeof(RecordCellAreaBase), true) as RecordCellAreaBase;

                        // we need a reference to the record presenter
                        _containingRp = Utilities.GetAncestorFromType((DependencyObject)_containingCellArea ?? this, typeof(RecordPresenter), true) as RecordPresenter;

                        if (null == _containingRp)
                        {
                            RelativeSource rs = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(RecordPresenter), 1);
                            this.SetBinding(FixedFieldInfoProperty, Utilities.CreateBindingObject(RecordPresenter.FixedFieldInfoProperty, BindingMode.OneWay, rs));
                        }
                        else
                            this.SetBinding(FixedFieldInfoProperty, Utilities.CreateBindingObject(RecordPresenter.FixedFieldInfoProperty, BindingMode.OneWay, _containingRp));
                    }
                }

				// JJD 5/2/11 - TFS22941
				// For nested headers we should get the _containingRp which will ensure that the clip rect
				// is re-verified after LayouuUpdated because IsNestedFixedHeader will return true.
				// This fixes a timing problem with the arrange pass that caused an issue when
				// the header was in the adorner layer
				if (this._isHeader == true &&
					_containingRp == null &&
					this.FieldLayout != null )
					_containingRp = Utilities.GetAncestorFromType((DependencyObject)_containingCellArea ?? this, typeof(RecordPresenter), true) as RecordPresenter;
            }

            if (null != fixedFieldInfo && fixedFieldInfo.IncludeSplitterElements)
            {
                // create/initialize the splitters
                this.InitializeSplitter(ref _nearSplitter, FixedFieldSplitterType.Near, fixedFieldInfo.NearSplitterVisibility, fixedFieldInfo.IsNearSplitterEnabled);
                this.InitializeSplitter(ref _farSplitter, FixedFieldSplitterType.Far, fixedFieldInfo.FarSplitterVisibility, fixedFieldInfo.IsFarSplitterEnabled);
            }
            else
            {
                // remove the children and discard the splitters
                RemoveSplitter(ref _nearSplitter);
                RemoveSplitter(ref _farSplitter);
            }
        }
        #endregion //VerifyFixedState

		// AS 6/26/09 NA 2009.2 Field Sizing
		#region VerifyAutoSizeFields
		private void VerifyAutoSizeFields()
		{
			// need to be fully hooked up
			if (_fieldLayout != null)
			{
				bool hasChanged = _lastAutoSizeElementVersion != _cellElementVersion;
				_lastAutoSizeElementVersion = _cellElementVersion;

				if (hasChanged && !_isHeader)
				{
					_fieldLayout.AutoSizeInfo.OnCellPanelChange(this);
				}
			}
		}
		#endregion //VerifyAutoSizeFields

		// AS 3/2/11 TFS66974
		// The property descriptors for the record were being verified when the value of the cell was 
		// being requested which was happening while we were creating the elements for the cells in 
		// view. Since the line count of the field's label changed we ended up invalidating the field 
		// layout cache which cleared the TemplateCellIndex while we were in the middle of processing 
		// the virtualized fields. Really what we want is to verify the descriptors up front before 
		// processing the cells.
		//
		#region VerifyPropertyDescriptorProvider
		private void VerifyPropertyDescriptorProvider()
		{
			DataRecord dr = _record as DataRecord;

			if (null != dr)
			{
				FieldLayout.LayoutPropertyDescriptorProvider provider = dr.PropertyDescriptorProvider;

				if (null != provider)
					provider.VerifyFieldDescriptors();
			}
		}
		#endregion //VerifyPropertyDescriptorProvider

		// JJD 9/20/08 - added
        #region WireLayouUpdatedEvent

        private void WireLayouUpdatedEvent()
        {
			// JJD 2/16/12 - TFS101387
			// See if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// If not then just bail.
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
			if (false == this.IsVisible && this.DataContext == null)
				return;

			// JJD 3/15/11 - TFS65143 - Optimization
			// Instead of having every element wire LayoutUpdated we can maintain a list of pending callbacks
			// and just wire LayoutUpdated on the DP
			//EventHandler handler = new EventHandler(OnLayoutUpdated);
			//this.LayoutUpdated -= handler;
			//this.LayoutUpdated += handler;
			DataPresenterBase dp = _fieldLayout != null ? _fieldLayout.DataPresenter : null;

			Debug.Assert(dp != null, "We should havea DP in WireLayouUpdatedEvent");

			if ( dp != null )
				dp.WireLayoutUpdated(new GridUtilities.MethodDelegate(this.OnLayoutUpdated));
        }

        #endregion //WireLayouUpdatedEvent	
    
		#endregion //Methods

		#region NonVirtualizedFieldInfo
		
#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)

		#endregion //NonVirtualizedFieldInfo

		#region IWeakEventListener Members

		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		// AS 10/27/09 NA 2010.1 - CardView
		// We need this again.
		//
		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				this.InvalidateLayoutManager();
				this.InvalidateMeasure();
				return true;
			}

			return false;
		}
		#endregion //IWeakEventListener

        // AS 12/11/08 NA 2009 Vol 1 - Fixed Fields
        #region ILayoutContainer

        Rect ILayoutContainer.GetBounds(object containerContext)
        {
            Debug.Assert(containerContext is Rect);
            return (Rect)containerContext;
        }

        void ILayoutContainer.PositionItem(ILayoutItem item, Rect rect, object containerContext)
        {
            FieldLayoutItemBase fli = item as FieldLayoutItemBase;

            if (null != fli)
            {
                int index = fli.Field.TemplateCellIndex;

                Debug.Assert(index >= 0);

                if (index >= 0)
                {
                    // adjust based on where the field is fixed since the layout
                    // doesn't know about the splitters
                    AdjustCellRect(fli.FixedLocation, ref rect);

                    this._cellElementRects[index] = rect;
                }
            }
        }
        #endregion //ILayoutContainer
	}

	#endregion // VirtualizingDataRecordCellPanel Class

	#region VirtualizingSummaryCellPanel Class

	// SSP 5/15/08 - Summaries Feature
	// 
	/// <summary>
	/// A control that sits within a <see cref="SummaryRecordCellArea"/> and virtualizes the creation of the contained cells.
	/// </summary>
	public class VirtualizingSummaryCellPanel : VirtualizingDataRecordCellPanel
	{
		
		
	}

	#endregion // VirtualizingSummaryCellPanel Class

    // AS 1/18/09 NA 2009 Vol 1 - Fixed Fields
    #region VirtualCellInfo
    internal class VirtualCellInfo
    {
        #region Member Variables

		// AS 4/12/11 TFS62951
		// We're not using this and I don't want to calculate it if we're not.
		//
		///// <summary>
		///// The relative logical cell(s) that the element would occupy
		///// </summary>
		//internal readonly Rect CellRect;

        /// <summary>
        /// The associated Field
        /// </summary>
        internal readonly Field Field;

        /// <summary>
        /// The current fixed state of the element within the panel. This may differ from the Field's FixedLocation
        /// </summary>
        internal readonly FixedFieldLocation FixedLocation;

        /// <summary>
        /// The portion of the element that would be in view.
        /// </summary>
        internal readonly Rect ClipRect;

        /// <summary>
        /// The associated element or null if one has not been created for the field
        /// </summary>
        internal readonly FrameworkElement Element;
        
        /// <summary>
        /// The position of the element. This differs from the CellRect where the CellRect is the 
        /// logical cell where as the ElementRect is the actual rect at which the cell could be 
        /// positioned within that logical cell which may be smaller or larger than the CellRect.
        /// </summary>
        internal readonly Rect ElementRect;

        #endregion //Member Variables

        #region Constructor
		internal VirtualCellInfo( Rect clipRect, Field field, FixedFieldLocation fixedLocation, FrameworkElement element, Rect elementRect)
        {
			// AS 4/12/11 TFS62951
			//CellRect = cellRect;
            ClipRect = clipRect;
            Field = field;
            FixedLocation = fixedLocation;
            Element = element;
            ElementRect = elementRect;
        }
        #endregion //Constructor
    } 
    #endregion //VirtualCellInfo
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