using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using System.Windows.Threading;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
    // We need a base class for the "cell areas" so that we can clip their contents
    // based on the amount of the scroll. Otherwise their children will show up under 
    // areas like the record selector.
    //
    #region RecordCellAreaBase
    /// <summary>
    /// Base class for the <see cref="DataRecordCellArea"/>, <see cref="HeaderLabelArea"/> and <see cref="SummaryRecordContentArea"/>
    /// </summary>
    public abstract class RecordCellAreaBase : ContentControl
    {
        #region Member Variables

        private RecordPresenter _containingRp;

        // AS 1/21/09 NA 2009 Vol 1 - Fixed Fields
        // There are a couple of issues that come up when you have a hierarchical
        // layout and one field layout is smaller than the total extent. When you 
        // scroll over such that the record cell area would be scroll out of view
        // then we end up starting to clip the fixed cells. To prevent that aspect
        // we need to tell the record cell area how wide the fixed areas are so 
        // it can use that as a minimum clip extent. Now at that point, the fixed 
        // near cells would remain in view as you scroll over and they won't be 
        // clipped by the recordcellareabase (e.g. datarecordcellarea). The fixed 
        // far elements however would continue being moved out of view on the left.
        // I originally was going to adjust the far fixed cells back by the overlap
        // amount thereby keeping them in view and just to the right of the near
        // fixed cells. While that does work, what ends up happening is that the 
        // highlight/chrome (e.g. selected rect) within the DataRecordCellArea 
        // continues to scroll out of view since it nor its ancestors are not 
        // "fixed". To prevent this issue and keep the near & far elements always 
        // in view, I went with a different approach. When we get to the point 
        // that the fixed near and far overlap, we tell the recordcellareabase
        // how much the overlap is and it will transform itself back to the right/
        // bottom by that amount. Now since that element contains this one, the 
        // near fixed elements which are already scrolled right based on the offset
        // will be moved too far to the right so we then need to adjust them 
        // back to the left (i.e. reduce the amount of the scroll offset that it 
        // uses to position the elements).
        //
        private double _minCellPanelExtent;
        private Vector _renderOffset;
        private TranslateTransform _noScrollableTransform;

        #endregion //Member Variables

        #region Constructor
        static RecordCellAreaBase()
        {
            RecordCellAreaBase.DataContextProperty.OverrideMetadata(typeof(RecordCellAreaBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDataContextChanged)));
            ClipProperty.OverrideMetadata(typeof(RecordCellAreaBase), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceClip)));
            RenderTransformProperty.OverrideMetadata(typeof(RecordCellAreaBase), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceRenderTransform)));

            // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
            // When the cell area gets focus (which happens when a record is focused) the 
            // framework is requesting that it be brought into view. However, that means that 
            // it is scrolled such that the record selector would be brought into view.
            // 
			// AS 8/19/09 TFS17864
			// Moved to a helper method.
			//
            //EventManager.RegisterClassHandler(typeof(RecordCellAreaBase), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
			GridUtilities.SuppressBringIntoView(typeof(RecordCellAreaBase));
        }

        /// <summary>
        /// Initializes a new <see cref="RecordCellAreaBase"/>
        /// </summary>
        protected RecordCellAreaBase()
        {
        } 
        #endregion //Constructor

        #region Base class overrides

        #region OnRenderSizeChanged
        /// <summary>
        /// Invoked when the size of the element has been changed.
        /// </summary>
        /// <param name="sizeInfo">Provides information about the size change</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            FieldLayout fl = _containingRp != null ? _containingRp.FieldLayout : null;

            if (null != fl)
            {
                bool isHorz = fl.IsHorizontal;

                // AS 2/3/09
                // I was previously only coercing the clip when the fixed extent changed
                // but that isn't right because the fixed extent may stay the same but
                // the element could get taller in which case the clip still needs to 
                // be updated.
                //
                this.CoerceValue(ClipProperty);

                if ((isHorz && sizeInfo.HeightChanged) || (!isHorz && sizeInfo.WidthChanged))
                {
                    if (_containingRp.CellArea == this)
                        _containingRp.VerifyFixedFarTransform();
                }
            }

            base.OnRenderSizeChanged(sizeInfo);
        }
        #endregion //OnRenderSizeChanged

        #endregion //Base class overrides

        #region Properties

        #region MinCellPanelExtent
        internal double MinCellPanelExtent
        {
            get { return _minCellPanelExtent; }
            set
            {
                if (value != _minCellPanelExtent && !GridUtilities.AreClose(value, _minCellPanelExtent))
                {
                    _minCellPanelExtent = value;

                    if (null != this.GetValue(FixedFieldInfoProperty))
                        this.CoerceValue(ClipProperty);
                }
            }
        } 
        #endregion //MinCellPanelExtent

        #region FixedFieldInfo

        private static readonly DependencyProperty FixedFieldInfoProperty = VirtualizingDataRecordCellPanel.FixedFieldInfoProperty.AddOwner(typeof(RecordCellAreaBase),
            new FrameworkPropertyMetadata(OnFixedFieldInfoChanged));

        private static void OnFixedFieldInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RecordCellAreaBase panel = (RecordCellAreaBase)d;
            FixedFieldInfo ffi = (FixedFieldInfo)e.NewValue;

            if (null != ffi)
            {
                panel.SetBinding(FixedFieldOffsetProperty, Utilities.CreateBindingObject(FixedFieldInfo.OffsetProperty, BindingMode.OneWay, ffi));
                panel.SetBinding(FixedFieldViewportExtentProperty, Utilities.CreateBindingObject(FixedFieldInfo.ViewportExtentProperty, BindingMode.OneWay, ffi));
				panel.SetBinding(FixedFieldExtentProperty, Utilities.CreateBindingObject(FixedFieldInfo.ExtentProperty, BindingMode.OneWay, ffi)); // AS 1/6/12 TFS32844
			}
            else
            {
                BindingOperations.ClearBinding(panel, FixedFieldOffsetProperty);
                BindingOperations.ClearBinding(panel, FixedFieldViewportExtentProperty);
				BindingOperations.ClearBinding(panel, FixedFieldExtentProperty); // AS 1/6/12 TFS32844
			}

            panel.CoerceValue(ClipProperty);
            panel.CoerceValue(RenderTransformProperty);
        }

        #endregion //FixedFieldInfo

        #region FixedFieldOffset

        internal static readonly DependencyProperty FixedFieldOffsetProperty = VirtualizingDataRecordCellPanel.FixedFieldOffsetProperty.AddOwner(
            typeof(RecordCellAreaBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

        private static void OnFixedFieldOffsetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ClipProperty);
        }

        #endregion //FixedFieldOffset

        #region FixedFieldViewportExtent

        internal static readonly DependencyProperty FixedFieldViewportExtentProperty = VirtualizingDataRecordCellPanel.FixedFieldViewportExtentProperty.AddOwner(
            typeof(RecordCellAreaBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

        #endregion //FixedFieldViewportExtent

		// AS 1/6/12 TFS32844
		#region FixedFieldExtent

		internal static readonly DependencyProperty FixedFieldExtentProperty = VirtualizingDataRecordCellPanel.FixedFieldExtentProperty.AddOwner(
			typeof(RecordCellAreaBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFixedFieldOffsetsChanged)));

		#endregion //FixedFieldExtent

        #region RenderOffset
        internal Vector RenderOffset
        {
            get { return _renderOffset; }
            set
            {
                if (value != _renderOffset)
                {
                    _renderOffset = value;

                    TranslateTransform oldTT = _noScrollableTransform;

                    if (value.X == 0 && value.Y == 0)
                        _noScrollableTransform = null;
                    else
                    {
                        if (_noScrollableTransform == null)
                            _noScrollableTransform = new TranslateTransform(-value.X, -value.Y);
                        else
                        {
                            _noScrollableTransform.X = -value.X;
                            _noScrollableTransform.Y = -value.Y;
                        }
                    }
                    
                    if (oldTT != _noScrollableTransform)
                        this.CoerceValue(RenderTransformProperty);

                    this.CoerceValue(ClipProperty);

                    if (null != _containingRp && this == _containingRp.CellArea)
                        _containingRp.VerifyFixedFarTransform();
                }
            }
        } 
        #endregion //RenderOffset

        #endregion //Properties

        #region Methods

        #region CoerceClip
        private static object CoerceClip(DependencyObject d, object newValue)
        {
            if (newValue != null)
                return newValue;

            RecordCellAreaBase cellArea = (RecordCellAreaBase)d;
            Size size = cellArea.RenderSize;
            RecordPresenter rp = cellArea._containingRp;

            if (size.Width > 0 && size.Height > 0 && null != rp)
            {
                FixedFieldInfo ffi = (FixedFieldInfo)d.GetValue(FixedFieldInfoProperty);
                FieldLayout fl = rp.FieldLayout;

                if (null != ffi && null != fl)
                {
                    bool isHorizontal = fl.IsHorizontal;
                    Rect rect = new Rect(size);
                    double cellAreaExtent = isHorizontal ? size.Height : size.Width;
					// AS 8/28/09 TFS21581
					//double finalExtent = Math.Min(ffi.Extent, VirtualizingDataRecordCellPanel.CalculateFixedRecordAreaEdge(isHorizontal, cellAreaExtent, rp));
					object dataContext = cellArea.DataContext;
					double finalExtent = Math.Min(ffi.Extent, VirtualizingDataRecordCellPanel.CalculateFixedRecordAreaEdge(isHorizontal, cellAreaExtent, rp, dataContext as Record, cellArea, dataContext is FieldLayout));
                    double fixedAreaExtent = ffi.ViewportExtent;
                    double offset = ffi.Offset;
                    double farOffset = -Math.Max(finalExtent - fixedAreaExtent - offset, 0);
                    double minExtent = Math.Max(cellArea._minCellPanelExtent, 0);

                    if (isHorizontal)
                    {
                        // since we may be translating our position towards the far to keep
                        // the highlight in view, we need to take away from the offset 
                        // amount that we used in the rect.
                        offset += cellArea._renderOffset.Y;

                        rect.Y += offset;
                        rect.Height = Math.Max(minExtent, rect.Height - offset + farOffset);

                        // AS 2/3/09
                        // There seem to be some rounding issues in WPF with how 
                        // pixels are determined to be clipped or not so we will bump
                        // the clipping in the non field scrolling orientation.
                        //
                        rect.X -= 1;
                        rect.Width += 2;
                    }
                    else
                    {
                        // since we may be translating our position towards the far to keep
                        // the highlight in view, we need to take away from the offset 
                        // amount that we used in the rect.
                        offset += cellArea._renderOffset.X;

                        rect.X += offset;
                        rect.Width = Math.Max(minExtent, rect.Width - offset + farOffset);

                        // AS 2/3/09
                        // There seem to be some rounding issues in WPF with how 
                        // pixels are determined to be clipped or not so we will bump
                        // the clipping in the non field scrolling orientation.
                        //
                        rect.Y -= 1;
                        rect.Height += 2;
                    }

                    if (rect.Width == 0 || rect.Height == 0)
                        return Geometry.Empty;
                    else
                    {
                        // JJD 4/29/10 - Optimization
                        // Freeze the geometry so the framework doesn't need to listen for change
                        //return new RectangleGeometry(rect);
                        Geometry geometry = new RectangleGeometry(rect);
                        geometry.Freeze();
                        return geometry;
                    }
                }
            }

            return newValue;
        }
        #endregion //CoerceClip

        #region CoerceRenderTransform
        private static object CoerceRenderTransform(DependencyObject d, object newValue)
        {
            Transform tt = newValue as Transform;

            if (null != tt && tt != Transform.Identity)
                return newValue;

            RecordCellAreaBase cellArea = (RecordCellAreaBase)d;

            if (null != cellArea._noScrollableTransform && cellArea.GetValue(FixedFieldInfoProperty) != null)
                return cellArea._noScrollableTransform;

            return newValue;
        }
        #endregion //CoerceRenderTransform

        #region OnDataContextChanged
        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RecordCellAreaBase cellArea = d as RecordCellAreaBase;

            if (null != e.NewValue)
            {
                Record rcd = e.NewValue as Record;
                RecordPresenter rp = null;

                if (null != rcd)
                    rp = RecordPresenter.FromRecord(rcd);

                // AS 3/27/09 TFS15809
                // We may have to do this even if its a DataRecord because it may
                // not have cached the RecordPresenter reference yet.
                //
                //else
                //    rp = Utilities.GetAncestorFromType(cellArea, typeof(RecordPresenter), true) as RecordPresenter;
                if (null == rp)
                    rp = Utilities.GetAncestorFromType(cellArea, typeof(RecordPresenter), true) as RecordPresenter;

                // cache a reference to the rp
                cellArea._containingRp = rp;

                Binding binding;

                if (null != rp)
                {
                    // give the record presenter a pointer back to the cell area so it can calculate
                    // the fixed far extent
                    rp.CellArea = cellArea;

                    binding = Utilities.CreateBindingObject(RecordPresenter.FixedFieldInfoProperty, BindingMode.OneWay, rp);
                }
                else
                {
                    RelativeSource rs = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(RecordPresenter), 1);
                    binding = Utilities.CreateBindingObject(RecordPresenter.FixedFieldInfoProperty, BindingMode.OneWay, rs);
                }

                cellArea.SetBinding(FixedFieldInfoProperty, binding);

                // AS 3/27/09 TFS15809
                // We can't calculate the clip until we have a reference to the RecordPresenter
                // so when we get one we need to re-evaluate the clip.
                //
                if (null != rp)
                    cellArea.CoerceValue(ClipProperty);
            }
            else
            {
                cellArea.ClearValue(FixedFieldInfoProperty);
            }
        }
        #endregion //OnDataContextChanged

        // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
        #region OnRequestBringIntoView
		
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        #endregion //OnRequestBringIntoView

        #endregion //Methods
    } 
    #endregion //RecordCellAreaBase

	/// <summary>
	/// A control that sits within a DataRecordPresenter control and contains the record's cells.  It is used primarily for styling the area around the cells.
	/// </summary>
	//[Description("A control that sits within a DataRecordPresenter control and contains the record's cells.  It is used primarily for styling the area around the cells.")]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,        GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,          GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,       GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,          GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,        GroupName = VisualStateUtilities.GroupActive)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateFilteredIn,      GroupName = VisualStateUtilities.GroupFilter)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilteredOut,     GroupName = VisualStateUtilities.GroupFilter)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateFixed,           GroupName = VisualStateUtilities.GroupFixed)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfixed,         GroupName = VisualStateUtilities.GroupFixed)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateAddRecord,       GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecord,      GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDataRecordAlternateRow, GroupName = VisualStateUtilities.GroupRecord)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFilterRecord,    GroupName = VisualStateUtilities.GroupRecord)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,        GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,      GroupName = VisualStateUtilities.GroupSelection)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateValidEx,         GroupName = VisualStateUtilities.GroupValidationEx)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInvalidEx,       GroupName = VisualStateUtilities.GroupValidationEx)]

	[StyleTypedProperty(Property = "ForegroundStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundActiveStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundAlternateStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundSelectedStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundHoverStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
    // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
	//public class DataRecordCellArea : ContentControl, IWeakEventListener
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DataRecordCellArea : RecordCellAreaBase, IWeakEventListener
	{
		#region Member Variables

		private int _cachedVersion;
		private bool _versionInitialized;
		private bool _isAlternateInitialized;
		private DataRecord _record;
		private StyleSelectorHelper _styleSelectorHelper;

		// JJD 5/29/07 - Optimization - prevent duplicate posts
		private DispatcherOperation _syncPosted;
        
        // JJD 03/30/10 - TFS30222 
        private bool _wasSelected;
        
        // JJD 05/26/10 - TFS32923
        private bool _ignoreIsSelectedClear;


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// SSP 7/19/11 TFS81814
		// 
		private bool _hasPendingHoverEnd;
		private bool _hookedIntoLoaded;
		private bool _hookedIntoUnLoaded;

		#endregion Member Variables

		#region Constructors

		static DataRecordCellArea()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataRecordCellArea), new FrameworkPropertyMetadata(typeof(DataRecordCellArea)));

			//FocusWithinManager.RegisterType(typeof(DataRecordCellArea));

//			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(DataRecordCellArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(DataRecordCellArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));
            DataRecordPresenter.HasDataErrorProperty.OverrideMetadata(typeof(DataRecordCellArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), DataRecordPresenter.HasDataErrorPropertyKey);
            RecordPresenter.IsFilteredOutProperty.OverrideMetadata(typeof(DataRecordCellArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), RecordPresenter.IsFilteredOutPropertyKey);

        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataRecordCellArea"/> class
		/// </summary>
		public DataRecordCellArea()
		{
			// initialize the styleSelectorHelper
			this._styleSelectorHelper = new StyleSelectorHelper(this);
		}

		#endregion Constructors

		#region Base class overrides

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = base.MeasureOverride(availableSize);

			// JJD 5/29/07 - Optimization - prevent duplicate posts
			//this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new MethodDelegate(this.SyncActiveSelectedState));
			//this.SyncActiveSelectedState();
			this.PostSyncActiveSelectedState();

			return size;
		}

			#endregion //MeasureOverride	
    
			// JM 10-27-10 TFS49130/32061 - Added.
			#region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
			// SSP 7/19/11 TFS81814
			// Refactored. Moved the checks into the new CanRaiseHoverEvent method.
			// 
			
			if ( this.CanRaiseHoverEvent( true ) )
				this.RaiseHoverBegin( new RoutedEventArgs( ) );
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			

			base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

		}
            #endregion //OnMouseEnter

			// JM 10-27-10 TFS49130/32061 - Added.
			#region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
			// SSP 7/19/11 TFS81814
			// Refactored. Moved the logic into VerifyPendingHoverEndRaised with 
			// modifications to check the new _hasPendingHoverEnd flag.
			// 
			
			this.VerifyPendingHoverEndRaised( );
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			

			base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

		}
            #endregion //OnMouseLeave

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

			// JM 03-30-11 TFS70613
			if (this.IsSelected == true)
				this.RaiseSelected(new RoutedEventArgs());


            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	

			#region OnPropertyChanged


		/// <summary>
		/// Called when a property is changed.
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			DependencyProperty property = e.Property;

			if (property == DataContextProperty)
			{
				DataRecord record = e.NewValue as DataRecord;

				if (record != this._record)
				{
					// JJD 5/29/07 - allow element to be reused for different records
					#region Obsolete

					//if (record == null)
					//{
					//    // unhook the event listener for the old record
					//    PropertyChangedEventManager.RemoveListener(this._record, this, string.Empty);

					//    this._record = null;
					//}
					//else
					//{
					//    this._record = record;

					#endregion //Obsolete

					
					
					
					
					





					bool isReused = this._record != null;

					if (this._record != null)
					{
						// unhook the event listener for the old record
						PropertyChangedEventManager.RemoveListener(this._record, this, string.Empty);
					}

					this._record = record;

					
					
					
					// JM 09-26-07 BR26775 - Force IsAlternate to get recalculated.
					this._isAlternateInitialized = false;
					this.InitializeIsAlternate( );

					if (this._record != null)
					{
						// use the weak event manager to hook the event so we don't get rooted
						PropertyChangedEventManager.AddListener(this._record, this, string.Empty);

						this.SetValue(IsAddRecordPropertyKey, KnownBoxes.FromValue(this._record.IsAddRecord));
                        
                        // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
						this.SetValue(IsFilterRecordPropertyKey, KnownBoxes.FromValue((bool)(this._record is FilterRecord)));
                        RecordPresenter.SetIsFilteredOutPropertyHelper(this, this._record);

						// initialize the IsActive nd IsSelected properties asynchronously
						// because otherwise it screws up the animations in the default styles
						//this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new MethodDelegate(this.SyncActiveSelectedState));
//						this.SetValue(IsActiveProperty, this._record.IsActive);
//						this.SetValue(IsSelectedProperty, this._record.IsSelected);

						if (isReused)
							this.SyncActiveSelectedState();
					}

					this.SetValue(RecordPropertyKey, this._record);

					// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
					// 
					this.UpdateDataError( );

					// JJD 8/1/07 
					// Refresh the style when this element is being reused for a different record
					if ( this._record != null && e.OldValue != null)
						this._styleSelectorHelper.InvalidateStyle();
				}
			}
			else if (property == ContentProperty)
			{
			}
			else if (property == InternalVersionProperty)
			{
				// JM 09-26-07 BR26775 - Force IsAlternate to get recalculated.
				this._isAlternateInitialized = false;


				this.InitializeVersionInfo();
			}
			// JM 09-26-07 BR26775 - Force IsAlternate to get recalculated when the scrollable record count changes.
			else if (property == DataRecordCellArea.ScrollableRecordCountVersionProperty)
			{
				this._isAlternateInitialized = false;
				this.InitializeIsAlternate();
			}
			// JM 11-13-07 BR27986 - Force IsAlternate to get recalculated when the sort operation version changes.
			else if (property == DataRecordCellArea.SortOperationVersionProperty)
			{
				this._isAlternateInitialized = false;
				this.InitializeIsAlternate();
			}
		}

			#endregion //OnPropertyChanged
			
			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("DataRecordCellArea: ");

			if (this.Record != null)
				sb.Append(this.Record.ToString());

			return sb.ToString();
		}

			#endregion //ToString

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region DataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the read-only <see cref="DataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty DataErrorProperty =
			DataRecordPresenter.DataErrorProperty.AddOwner( typeof( DataRecordCellArea ) );

		/// <summary>
		/// Returns the associated data record's data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DataError</b> property returns the value of the associated DataRecord's
		/// <see cref="DataRecord.DataError"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.DataError"/>
		/// <seealso cref="CellValuePresenter.DataError"/>
		/// <seealso cref="CellValuePresenter.HasDataError"/>
		//[Description( "The record data error (IDataErrorInfo.Error)." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public object DataError
		{
			get
			{
				return (object)this.GetValue( DataErrorProperty );
			}
		}

		internal void UpdateDataError( )
		{
			bool hasDataError = false;
			object dataError = null;

			DataRecord dr = this.Record;
			if ( null != dr )
			{
				dataError = dr.DataError;
				hasDataError = dr.HasDataError;
			}

			this.SetValue( DataRecordPresenter.DataErrorPropertyKey, dataError );
			this.SetValue( DataRecordPresenter.HasDataErrorPropertyKey, hasDataError );
		}

				#endregion // DataError

				#region DataPresenterBase

		/// <summary>
		/// Returns the associated <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> (read-only)
		/// </summary>
		public DataPresenterBase DataPresenter
		{
			get
			{
				Record rcd = this.Record;

				if (rcd != null)
					return rcd.DataPresenter;

				return null;
			}
		}

				#endregion //DataPresenterBase	

				#region FieldLayout

		/// <summary>
		/// Identifies the 'FieldLayout' dependency property
		/// </summary>
		public static readonly DependencyProperty FieldLayoutProperty = DependencyProperty.Register("FieldLayout",
				  typeof(FieldLayout), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFieldLayoutChanged)));

		private static void OnFieldLayoutChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DataRecordCellArea rs = target as DataRecordCellArea;

			if (rs != null)
			{
				rs._cachedFieldLayout = e.NewValue as FieldLayout;
				rs.InitializeVersionInfo();
			}
		}

		private FieldLayout _cachedFieldLayout = null;

		/// <summary>
		/// Returns the associated field layout
		/// </summary>
		//[Description("Returns the associated field layout")]
		//[Category("Behavior")]
		public FieldLayout FieldLayout
		{
			get
			{
				return this._cachedFieldLayout;
			}
			set
			{
				this.SetValue(DataRecordCellArea.FieldLayoutProperty, value);
			}
		}

				#endregion //FieldLayout

				#region HasDataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Identifies the read-only <see cref="HasDataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty HasDataErrorProperty 
			= DataRecordPresenter.HasDataErrorProperty.AddOwner( typeof( DataRecordCellArea ) );

		/// <summary>
		/// Indicates if the associated data record has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasDataError</b> property returns the value of the associated DataRecord's
		/// <see cref="DataRecord.HasDataError"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataRecord.HasDataError"/>
		/// <seealso cref="DataRecord.DataError"/>
		/// <seealso cref="DataRecordPresenter.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.DataError"/>
		/// <seealso cref="CellValuePresenter.HasDataError"/>
		/// <seealso cref="CellValuePresenter.DataError"/>
		//[Description( "Indicates if the record has data error (IDataErrorInfo.Error)." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool HasDataError
		{
			get
			{
				return (bool)this.GetValue( HasDataErrorProperty );
			}
		}

				#endregion // HasDataError

				#region IsActive

		/// <summary>
		/// Identifies the 'IsActive' dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
				typeof(bool), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsActiveChanged)));

		private static void OnIsActiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DataRecordCellArea rp = target as DataRecordCellArea;

			if (rp != null)
			{
				bool newValue = (bool)(e.NewValue);

				if (newValue != rp._cachedIsActive)
					rp._cachedIsActive = newValue;

				if (rp._record != null &&
					rp._record.DataPresenter != null)
				{
					Record activeRecord = rp._record.DataPresenter.ActiveRecord;
					if (newValue)
					{
						if (activeRecord != rp._record)
							rp._record.DataPresenter.ActiveRecord = rp._record;
					}
					else
					{
						if (activeRecord == rp._record)
							rp._record.DataPresenter.ActiveRecord = null;
					}
				}

				// JJD 5/30/07 
				// If we are still out of sync then set the dependencyproperty value back
				if (rp.IsSelected != newValue)
					rp.SetValue(IsActiveProperty, KnownBoxes.FromValue(rp.IsActive));

                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                rp.UpdateVisualStates();

            }
		}

		private bool _cachedIsActive = false;

		/// <summary>
		/// Determines if this is the active record
		/// </summary>
		//[Description("Determines if this is the active record")]
		//[Category("Behavior")]
		public bool IsActive
		{
			get
			{
				return this._cachedIsActive;
			}
			set
			{
				this.SetValue(DataRecordCellArea.IsActiveProperty, value);
			}
		}

				#endregion //IsActive

				#region IsAddRecord

		internal static readonly DependencyPropertyKey IsAddRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsAddRecord",
			typeof(bool), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the 'IsAddRecord' dependency property
		/// </summary>
		public static readonly DependencyProperty IsAddRecordProperty =
			IsAddRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if this is an add record (readonly)
		/// </summary>
		public bool IsAddRecord
		{
			get
			{
				return (bool)this.GetValue(DataRecordCellArea.IsAddRecordProperty);
			}
		}

				#endregion //IsAddRecord

                // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
				#region IsFilterRecord

		internal static readonly DependencyPropertyKey IsFilterRecordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsFilterRecord",
			typeof(bool), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the 'IsFilterRecord' dependency property
		/// </summary>
		public static readonly DependencyProperty IsFilterRecordProperty =
			IsFilterRecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if this is a filter record (readonly)
		/// </summary>
		public bool IsFilterRecord
		{
			get
			{
				return (bool)this.GetValue(DataRecordCellArea.IsFilterRecordProperty);
			}
		}

				#endregion //IsFilterRecord

                // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
				#region IsFilteredOut

		/// <summary>
		/// Identifies the 'IsFilteredOut' dependency property
		/// </summary>
		public static readonly DependencyProperty IsFilteredOutProperty = RecordPresenter.IsFilteredOutProperty.AddOwner(typeof(DataRecordCellArea));

		/// <summary>
		/// Returns true if this is record is filtered out (read-only)
		/// </summary>
        /// <value>
        /// <para class="body"><b>True</b> if the record fails to meet the current effective record filters or <b>false</b> if it does not. 
        /// However, if there are no active record filters then this property returns <b>null</b></para>
        /// </value>
        /// <seealso cref="DataRecord.IsFilteredOut"/>
        /// <seealso cref="IsFilteredOutProperty"/>
        public bool? IsFilteredOut
		{
			get
			{
				return (bool?)this.GetValue(DataRecordCellArea.IsFilteredOutProperty);
			}
		}

				#endregion //IsFilteredOut

				#region IsAlternate

		internal static readonly DependencyPropertyKey IsAlternatePropertyKey =
			DependencyProperty.RegisterReadOnly("IsAlternate",
			typeof(bool), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the 'IsAlternate' dependency property
		/// </summary>
		public static readonly DependencyProperty IsAlternateProperty =
			IsAlternatePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true for every other row in the list (readonly)
		/// </summary>
		public bool IsAlternate
		{
			get
			{
				return (bool)this.GetValue(DataRecordCellArea.IsAlternateProperty);
			}
		}

				#endregion //IsAlternate

				#region IsSelected

		/// <summary>
		/// Identifies the 'IsSelected' dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
				typeof(bool), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIsSelectedChanged), new CoerceValueCallback(CoerceIsSelected)));

		private static void OnIsSelectedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			DataRecordCellArea cellArea = target as DataRecordCellArea;

			Debug.Assert(cellArea != null);

			if (cellArea != null)
			{
				// JJD 5/30/07 
				// Attempt a coerce if the property was cleared
				if (cellArea.ReadLocalValue(IsSelectedProperty) == DependencyProperty.UnsetValue)
				{
					bool newValue = (bool)e.NewValue;

					CoerceIsSelected(cellArea, KnownBoxes.FromValue(newValue));

					// JJD 5/30/07 
					// If we are still out of sync then set the dependencyproperty value back
					if (cellArea.IsSelected != newValue)
						cellArea.SetValue(IsSelectedProperty, KnownBoxes.FromValue(cellArea.IsSelected));
                }

                // JJD 03/30/10 - TFS30222 
                // Call VerifySelectedState to make sure we raise the appropriate Selected/Deselected evnt
				// JM 09-23-11 - TFS89071 Only call VerifySelectedState if the DataRecordCellArea is loaded.
				if (cellArea.IsLoaded)
					cellArea.VerifySelectedState();


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                cellArea.UpdateVisualStates();

            }
		}

		private static object CoerceIsSelected(DependencyObject target, object value)
		{
			DataRecordCellArea rp = target as DataRecordCellArea;

			Debug.Assert(rp != null);
			Debug.Assert(value is bool);

			if (rp != null)
			{
                // JJD 05/26/10 - TFS32923
                // If the ignore flag is set then don't change the state of the record.
                // This flag is set when the element is being unloaded.
                if (rp._ignoreIsSelectedClear)
                    return value;

				Record dr = rp.Record;

				if (dr != null && rp.DataPresenter != null)
				{
					// sync the value on the record
					dr.IsSelected = (bool)value;

					return dr.IsSelected;
				}
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Determines if the record is selected
		/// </summary>
		//[Description("Determines if the record is selected")]
		//[Category("Behavior")]
		public bool IsSelected
		{
			get
			{
				Record rcd = this.Record;

				if (rcd == null)
					return false;

				return rcd.IsSelected;
			}
			set
			{
				this.SetValue(DataRecordCellArea.IsSelectedProperty, value);
			}
		}

				#endregion //IsSelected

				#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey =
			DependencyProperty.RegisterReadOnly("Orientation",
			typeof(Orientation), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
			OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the orientation (vertical/horizontal) of the RecordCellAreas in the containing Panel.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns the orientation (vertical/horizontal) of the RecordCellAreas in the containing Panel.")]
		//[Category("Appearance")]
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(DataRecordCellArea.OrientationProperty); }
		}

				#endregion //Orientation

				#region Record

		private static readonly DependencyPropertyKey RecordPropertyKey =
			DependencyProperty.RegisterReadOnly("Record",
			typeof(DataRecord), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the 'Record' dependency property
		/// </summary>
		public static readonly DependencyProperty RecordProperty =
			RecordPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated record inside a DataPresenterBase (read-only)
		/// </summary>
		//[Description("Returns the associated record inside a DataPresenterBase (read-only)")]
		//[Category("Data")]
		public DataRecord Record
		{
			get
			{
				return this._record;
			}
		}

				#endregion //Record

				#region CornerRadius

				/// <summary>
				/// Identifies the <see cref="CornerRadius"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius",
					typeof(CornerRadius), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(new CornerRadius(4)));

				/// <summary>
				/// Sets the CornerRadius of default borders.
				/// </summary>
				/// <seealso cref="CornerRadiusProperty"/>		
				//[Description("CornerRadius used by borders in default templates.")]
				//[Category("Appearance")]
				public CornerRadius CornerRadius
				{
					get
					{
						return (CornerRadius)this.GetValue(DataRecordCellArea.CornerRadiusProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.CornerRadiusProperty, value);
					}
				}

				#endregion CornerRadius																								

				#region BackgroundHover

				/// <summary>
				/// Identifies the <see cref="BackgroundHover"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty BackgroundHoverProperty = DependencyProperty.Register("BackgroundHover",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The brush applied by default templates when IsMouseOver = true.
				/// </summary>
				/// <seealso cref="BackgroundHoverProperty"/>		
				//[Description("The brush applied by default templates when IsMouseOver = true.")]
				//[Category("Brushes")]
				public Brush BackgroundHover
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BackgroundHoverProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BackgroundHoverProperty, value);
					}
				}

				#endregion BackgroundHover		
						
				#region BorderHoverBrush


				/// <summary>
				/// Identifies the <see cref="BorderHoverBrush"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty BorderHoverBrushProperty = DependencyProperty.Register("BorderHoverBrush",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The border brush applied by default templates when IsMouseOver = true.
				/// </summary>
				/// <seealso cref="BorderHoverBrushProperty"/>		
				//[Description("The border brush applied by default templates when IsMouseOver = true.")]
				//[Category("Brushes")]
				public Brush BorderHoverBrush
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BorderHoverBrushProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BorderHoverBrushProperty, value);
					}
				}

				#endregion BorderHoverBrush		
						
				#region BackgroundActive


				/// <summary>
				/// Identifies the <see cref="BackgroundActive"/> dependency property
				/// </summary>					
				public static readonly DependencyProperty BackgroundActiveProperty = DependencyProperty.Register("BackgroundActive",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The brush applied by default templates when IsActive = true.
				/// </summary>
				/// <seealso cref="BackgroundActiveProperty"/>		
				//[Description("The brush applied by default templates when IsActive = true.")]
				//[Category("Brushes")]
				public Brush BackgroundActive
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BackgroundActiveProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BackgroundActiveProperty, value);
					}
				}

				#endregion BackgroundActive		
						
				#region BorderActiveBrush

				/// <summary>
				/// Identifies the <see cref="BorderActiveBrush"/> dependency property
				/// </summary>			
				public static readonly DependencyProperty BorderActiveBrushProperty = DependencyProperty.Register("BorderActiveBrush",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The border brush applied by default templates when IsActive = true.
				/// </summary>
				/// <seealso cref="BorderActiveBrushProperty"/>		
				//[Description("The border brush applied by default templates when IsActive = true.")]
				//[Category("Brushes")]
				public Brush BorderActiveBrush
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BorderActiveBrushProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BorderActiveBrushProperty, value);
					}
				}

				#endregion BorderActiveBrush		
						
				#region BackgroundAlternate


				/// <summary>
				/// Identifies the <see cref="BackgroundAlternate"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty BackgroundAlternateProperty = DependencyProperty.Register("BackgroundAlternate",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The brush applied by default templates when IsAlternate = true.
				/// </summary>
				/// <seealso cref="BackgroundAlternateProperty"/>		
				//[Description("The brush applied by default templates when IsAlternate = true.")]
				//[Category("Brushes")]
				public Brush BackgroundAlternate
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BackgroundAlternateProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BackgroundAlternateProperty, value);
					}
				}

				#endregion BackgroundAlternate		
						
				#region BorderAlternateBrush

				/// <summary>
				/// Identifies the <see cref="BorderAlternateBrush"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty BorderAlternateBrushProperty = DependencyProperty.Register("BorderAlternateBrush",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The border brush applied by default templates when IsAlternate = true.
				/// </summary>
				/// <seealso cref="BorderAlternateBrushProperty"/>	
				//[Description("The border brush applied by default templates when IsAlternate = true.")]
				//[Category("Brushes")]
				public Brush BorderAlternateBrush
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BorderAlternateBrushProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BorderAlternateBrushProperty, value);
					}
				}

				#endregion BorderAlternateBrush		
						
				#region BackgroundSelected

				/// <summary>
				/// Identifies the <see cref="BackgroundSelected"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty BackgroundSelectedProperty = DependencyProperty.Register("BackgroundSelected",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The brush applied by default templates when IsSelected = true.
				/// </summary>
				/// <seealso cref="BackgroundSelectedProperty"/>		
				//[Description("The brush applied by default templates when IsSelected = true.")]
				//[Category("Brushes")]
				public Brush BackgroundSelected
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BackgroundSelectedProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BackgroundSelectedProperty, value);
					}
				}

				#endregion BackgroundSelected		
						
				#region BorderSelectedBrush

				/// <summary>
				/// Identifies the <see cref="BorderSelectedBrush"/> dependency property
				/// </summary>			
				public static readonly DependencyProperty BorderSelectedBrushProperty = DependencyProperty.Register("BorderSelectedBrush",
					typeof(Brush), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// The border brush applied by default templates when IsSelected = true.
				/// </summary>
				/// <seealso cref="BorderSelectedBrushProperty"/>			
				//[Description("The border brush applied by default templates when IsSelected = true.")]
				//[Category("Brushes")]
				public Brush BorderSelectedBrush
				{
					get
					{
						return (Brush)this.GetValue(DataRecordCellArea.BorderSelectedBrushProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.BorderSelectedBrushProperty, value);
					}
				}

				#endregion BorderSelectedBrush		
				
				#region ForegroundStyle

				/// <summary>
				/// Identifies the <see cref="ForegroundStyle"/> dependency property
				/// </summary>		
				public static readonly DependencyProperty ForegroundStyleProperty = DependencyProperty.Register("ForegroundStyle",
					typeof(Style), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// Style property applied to internal ContentPresenter used by default templates.
				/// </summary>
				/// <seealso cref="ForegroundStyleProperty"/>		
				//[Description("Style property applied to internal ContentPresenter used by default templates. Use to style text values.")]
				//[Category("Appearance")]
				public Style ForegroundStyle
				{
					get
					{
						return (Style)this.GetValue(DataRecordCellArea.ForegroundStyleProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.ForegroundStyleProperty, value);
					}
				}

				#endregion ForegroundStyle
				
				#region ForegroundActiveStyle


				/// <summary>
				/// Identifies the <see cref="ForegroundActiveStyle"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty ForegroundActiveStyleProperty = DependencyProperty.Register("ForegroundActiveStyle",
					typeof(Style), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// Style applied to default ContentPresenter when CellArea.IsActive = true.
				/// </summary>
				/// <seealso cref="ForegroundActiveStyleProperty"/>		
				//[Description("Style applied to default ContentPresenter when CellArea.IsActive = true.")]
				//[Category("Appearance")]
				public Style ForegroundActiveStyle
				{
					get
					{
						return (Style)this.GetValue(DataRecordCellArea.ForegroundActiveStyleProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.ForegroundActiveStyleProperty, value);
					}
				}

				#endregion ForegroundActiveStyle		
				
				#region ForegroundAlternateStyle

				/// <summary>
				/// Identifies the <see cref="ForegroundAlternateStyle"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty ForegroundAlternateStyleProperty = DependencyProperty.Register("ForegroundAlternateStyle",
					typeof(Style), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// Style applied to ContentPresenter when CellArea.IsAlternate = true.
				/// </summary>
				/// <seealso cref="ForegroundAlternateStyleProperty"/>	
				//[Description("Style applied to default ContentPresenter when CellArea.IsAlternate = true. Use to style text values.")]
				//[Category("Appearance")]
				public Style ForegroundAlternateStyle
				{
					get
					{
						return (Style)this.GetValue(DataRecordCellArea.ForegroundAlternateStyleProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.ForegroundAlternateStyleProperty, value);
					}
				}

				#endregion ForegroundAlternateStyle		
													
				#region ForegroundSelectedStyle

				/// <summary>
				/// Identifies the <see cref="ForegroundSelectedStyle"/> dependency property
				/// </summary>		
				public static readonly DependencyProperty ForegroundSelectedStyleProperty = DependencyProperty.Register("ForegroundSelectedStyle",
					typeof(Style), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// Style applied to default ContentPresenter when CellArea.IsSelected = true.
				/// </summary>
				/// <seealso cref="ForegroundSelectedStyleProperty"/>		
				//[Description("Style applied to default ContentPresenter when CellArea.IsSelected = true.")]
				//[Category("Appearance")]
				public Style ForegroundSelectedStyle
				{
					get
					{
						return (Style)this.GetValue(DataRecordCellArea.ForegroundSelectedStyleProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.ForegroundSelectedStyleProperty, value);
					}
				}

				#endregion ForegroundSelectedStyle		
				
				#region ForegroundHoverStyle

				/// <summary>
				/// Identifies the <see cref="ForegroundHoverStyle"/> dependency property
				/// </summary>	
				public static readonly DependencyProperty ForegroundHoverStyleProperty = DependencyProperty.Register("ForegroundHoverStyle",
					typeof(Style), typeof(DataRecordCellArea), new FrameworkPropertyMetadata((object)null));

				/// <summary>
				/// Style applied to default ContentPresenter when Cell.IsMouseOver = true.
				/// </summary>
				/// <seealso cref="ForegroundHoverStyleProperty"/>
				//[Description("Style applied to default ContentPresenter when Cell.IsMouseOver = true.")]
				//[Category("Appearance")]
				public Style ForegroundHoverStyle
				{
					get
					{
						return (Style)this.GetValue(DataRecordCellArea.ForegroundHoverStyleProperty);
					}
					set
					{
						this.SetValue(DataRecordCellArea.ForegroundHoverStyleProperty, value);
					}
				}

				#endregion ForegroundHoverStyle		


			#endregion //Public Properties

			#region Internal Properties

				#region InternalVersion

		internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register("InternalVersion",
			typeof(int), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(0));

		internal int InternalVersion
		{
			get
			{
				return (int)this.GetValue(DataRecordCellArea.InternalVersionProperty);
			}
			set
			{
				this.SetValue(DataRecordCellArea.InternalVersionProperty, value);
			}
		}

				#endregion //InternalVersion

				// JM 09-26-07 BR26775
				#region ScrollableRecordCountVersion

		internal static readonly DependencyProperty ScrollableRecordCountVersionProperty = DataPresenterBase.ScrollableRecordCountVersionProperty.AddOwner(typeof(DataRecordCellArea));

		internal int ScrollableRecordCountVersion
		{
			get
			{
				return (int)this.GetValue(DataRecordCellArea.ScrollableRecordCountVersionProperty);
			}
			set
			{
				this.SetValue(DataRecordCellArea.ScrollableRecordCountVersionProperty, value);
			}
		}

				#endregion //ScrollableRecordCountVersion

				// JM 11-13-07 BR27986
				#region SortOperationVersion

		internal static readonly DependencyProperty SortOperationVersionProperty = DependencyProperty.Register("SortOperationVersion",
			typeof(int), typeof(DataRecordCellArea), new FrameworkPropertyMetadata(0));

		internal int SortOperationVersion
		{
			get
			{
				return (int)this.GetValue(DataRecordCellArea.SortOperationVersionProperty);
			}
			set
			{
				this.SetValue(DataRecordCellArea.SortOperationVersionProperty, value);
			}
		}

				#endregion //SortOperationVersion

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            // set fixed state
            Record rcd = this.Record;

            // set common state
            if (this.IsEnabled == false || (rcd != null && rcd.IsEnabledResolved == false))
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            {
                if (this.IsMouseOver)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
            }

            // set active state
            if ( this.IsActive )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

            // set filter state
            bool? isFilteredOut = this.IsFilteredOut;

            if (isFilteredOut.HasValue && isFilteredOut.Value == true)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilteredOut, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilteredIn, useTransitions);

            if ( rcd != null && rcd.FixedLocation != FixedRecordLocation.Scrollable )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFixed, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfixed, useTransitions);

            // set record state
            if (this.IsFilterRecord)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFilterRecord, useTransitions);
            else
            {
                if (this.IsAddRecord)
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateAddRecord, useTransitions);
                else
                {
                    if (this.IsAlternate)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDataRecordAlternateRow, VisualStateUtilities.StateDataRecord);
                    else
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateDataRecord, useTransitions);
                }
            }

            // set active state
            if ( this.IsSelected )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);

            // set validation state
            if (this.HasDataError)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInvalidEx, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateValidEx, useTransitions);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            DataRecordCellArea cellarea = target as DataRecordCellArea;

            if ( cellarea != null )
                cellarea.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods	

			#endregion //Protected methods

			#region Internal Methods

				// JJD 10/13/10 - TFS57200 - added
				#region SetIsAlternate

		internal void SetIsAlternate(bool isAlternate)
		{
			_isAlternateInitialized = true;

			if (isAlternate)
				this.SetValue(DataRecordCellArea.IsAlternatePropertyKey, KnownBoxes.TrueBox);
			else
				this.ClearValue(DataRecordCellArea.IsAlternatePropertyKey);
		}

				#endregion //SetIsAlternate

			#endregion //Internal Methods	
        
			#region Private Methods

				#region CanRaiseHoverEvent

		// SSP 7/19/11 TFS81814
		// Added CanRaiseHoverEvent method. Code in there was moved from OnMouseEnter/Leave.
		// 
		/// <summary>
		/// Checks various things like IsLoaded, IsInitialized etc...
		/// </summary>
		/// <param name="queryForBegin"></param>
		/// <returns></returns>
		private bool CanRaiseHoverEvent( bool queryForBegin )
		{
			if ( false == this.IsLoaded ||
				false == this.IsInitialized ||
				//null	== this.Parent			||	JM 04-21-11 TFS69043 Removed
				null == this.Record ||
				null == this.Record.AssociatedRecordPresenter ||
				null == this.GetTemplateChild( "Hover" ) ||
				null == VisualTreeHelper.GetParent( this ) )
			{
				return false;
			}

			return true;
		}

				#endregion // CanRaiseHoverEvent

				#region HookUnhook_Loaded

		// SSP 7/19/11 TFS81814
		// 
		private void HookUnhook_Loaded( bool hook )
		{
			if ( hook )
			{
				if ( !_hookedIntoLoaded )
				{
					this.Loaded += new RoutedEventHandler( this.OnLoaded );
					_hookedIntoLoaded = true;
				}
			}
			else
			{
				if ( _hookedIntoLoaded )
				{
					this.Loaded -= new RoutedEventHandler( this.OnLoaded );
					_hookedIntoLoaded = false;
				}
			}
		} 

				#endregion // HookUnhook_Loaded

				#region HookUnhook_UnLoaded

		// SSP 7/19/11 TFS81814
		// 
		private void HookUnhook_UnLoaded( bool hook )
		{
			if ( hook )
			{
				if ( !_hookedIntoUnLoaded )
				{
					this.Unloaded += new RoutedEventHandler( this.OnUnloaded );
					_hookedIntoUnLoaded = true;
				}
			}
			else
			{
				if ( _hookedIntoUnLoaded )
				{
					this.Unloaded -= new RoutedEventHandler( this.OnUnloaded );
					_hookedIntoUnLoaded = false;
				}
			}
		} 

				#endregion // HookUnhook_UnLoaded

				// JM 09-26-07 BR26775
				#region InitializeIsAlternate

		private void InitializeIsAlternate()
		{
			if (this._isAlternateInitialized)
				return;

			// SSP 10/3/07 BR26775 & Optimizations
			// We should be checking HighlightAlternateRecordsResolved in the InitializeIsAlternate method.
			// InitializeIsAlternate should never set the IsAlternate if HighlightAlternateRecordsResolved
			// is false. Before this change, we were checking for HighlightAlternateRecordsResolved before
			// calling this method. However we were not consistent about it.
			// 
			// --------------------------------------------------------------------------------------------
			// JJD 10/13/10 - TFS57200
			// Moved to SetIsAlternate call below
			//_isAlternateInitialized = true;

			bool alternate = false;

			if ( null != _cachedFieldLayout && _cachedFieldLayout.HighlightAlternateRecordsResolved )
			{
				if ( null != _record )
				{
					// JM 10-03-08 [BR34631 TFS6222]
					//int index = _record.OverallScrollPosition;
					int index = _record.VisibleIndex;
					alternate = 0 != index % 2;
				}
			}

			// JJD 10/13/10 - TFS57200
			// Moved to SetIsAlternate
			//this.SetValue( DataRecordCellArea.IsAlternatePropertyKey, KnownBoxes.FromValue( alternate ) );
			this.SetIsAlternate(alternate);

			
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------------------
		}

				#endregion //InitializeIsAlternate

				#region InitializeVersionInfo

		private void InitializeVersionInfo()
		{
			if (this._cachedFieldLayout != null &&
				this._cachedFieldLayout.DataPresenter != null)
			{
				if (this._cachedFieldLayout.StyleGenerator != null)
				{
					int version = this.InternalVersion;

					if (this._cachedVersion != version)
					{
						this._cachedVersion = version;

						if (this._versionInitialized == true)
							this._styleSelectorHelper.InvalidateStyle();

						this.SetValue(DataRecordCellArea.OrientationPropertyKey, this.FieldLayout.StyleGenerator.LogicalOrientation);
					}

					// SSP 10/3/07 BR26775
					// We should be checking HighlightAlternateRecordsResolved in the InitializeIsAlternate method.
					// InitializeIsAlternate should never set the IsAlternate if HighlightAlternateRecordsResolved
					// is false. Commented out the original code below.
					// 
					// --------------------------------------------------------------------------------------------
					this.InitializeIsAlternate( );
					
#region Infragistics Source Cleanup (Region)













































#endregion // Infragistics Source Cleanup (Region)

					// --------------------------------------------------------------------------------------------

					this._versionInitialized = true;
				}
			}
		}

				#endregion //InitializeVersionInfo

				#region OnRecordPropertyChanged

		private void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Debug.Assert(sender is DataRecord);

			if (!(sender is DataRecord))
				return;

			switch (e.PropertyName)
			{
				case "IsAddRecord":
					this.SetValue(IsAddRecordPropertyKey, KnownBoxes.FromValue(((DataRecord)sender).IsAddRecord));
					break;

				case "IsSelected":
					// JM 03-11-10 TFS27879
					object o = this.ReadLocalValue(IsSelectedProperty);
					if (o is bool)
					{
						if ((bool)o != ((DataRecord)sender).IsSelected)
						{
							if (((DataRecord)sender).IsSelected == true)
								this.RaiseSelected(new RoutedEventArgs());
							else
								this.RaiseDeselected(new RoutedEventArgs());
						}
					}

					this.IsSelected = ((DataRecord)sender).IsSelected;
					break;
                
                // JJD 1/7/09 - NA 2009 vol 1 - Record filtering
                case "IsFilteredOut":
                    RecordPresenter.SetIsFilteredOutPropertyHelper(this, ((Record)sender));
                    break;

				case "ActiveCell":
				case "IsActive":
					this.IsActive = ((DataRecord)sender).IsActive;
					break;

				// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
				// 
				case "DataError":
					this.UpdateDataError( );
					break;


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                case "IsEnabledResolved":
                case "FixedLocation":
                    this.UpdateVisualStates();
                    break;

            }

		}

				#endregion //OnRecordPropertyChanged

				// JJD 5/29/07 - Optimization - prevent duplicate posts
				#region PostSyncActiveSelectedState

		private void PostSyncActiveSelectedState()
		{
            // JJD 3/29/08 - added support for printing.
            // We can't do asynchronous operations during a print
			// JJD 2/16/12 - TFS101387
			// See if the element is still being used (i.e. if IsVsible is true or the DataContext is not null).
			// This fixes a memory leak by preventing presenters for old TemplateDataRecords from being rooted.
            //if (this.DataPresenter != null)
            if (this.DataPresenter != null && (this.IsVisible || this.DataContext != null))
            {
                // MBS 7/29/09 - NA9.2 Excel Exporting
                //if (this.DataPresenter.IsReportControl)
                if(this.DataPresenter.IsSynchronousControl)
                    this.SyncActiveSelectedState();
                else
                    if (this._syncPosted == null)
                        this._syncPosted = this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new GridUtilities.MethodDelegate(this.SyncActiveSelectedState));
            }
		}

				#endregion //PostSyncActiveSelectedState	
    
				#region SyncActiveSelectedState

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodDelegate();

		private void SyncActiveSelectedState()
		{
			// JJD 5/29/07 - Optimization - prevent duplicate posts
			// clear member so we can post again
			this._syncPosted = null;

			if (this._record != null)
			{
				this.SetValue(IsActiveProperty, KnownBoxes.FromValue(this._record.IsActive));
				this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(this._record.IsSelected));
			}
		}

				#endregion //SyncActiveSelectedState	

				// JJD 03/30/10 TFS30222 - Added.  
				#region OnUnloaded, OnLoaded

		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// SSP 7/19/11 TFS81814
			// 
			//this.Unloaded	-= new RoutedEventHandler(OnUnloaded);
			this.HookUnhook_UnLoaded( false );

            // Set the wasSelected flag to false so we don't raise the Deactivated event when we
            // clear the IsSelectedProperty below
            bool wasSelected = this._wasSelected;
            this._wasSelected = false;
        
            // JJD 05/26/10 - TFS32923
            // Set a flag so we know the not clear the selection state of the record
            // in the coerce logic
            this._ignoreIsSelectedClear = true;

            try
            {
                this.ClearValue(DataRecordCellArea.IsSelectedProperty);
            }
            finally
            {
                // JJD 05/26/10 - TFS32923
                // clear the flag
                this._ignoreIsSelectedClear = true;
            }

            // JJD 05/05/10 - TFS31370
            // reset the flag so when the element gets reloaded it will raise
            // the appropriate selected state event
            this._wasSelected = wasSelected;

            // only wire the Loaded event if we are the active record
			// SSP 7/19/11 TFS81814
			// 
            //if ( wasSelected )
			//    this.Loaded		+= new RoutedEventHandler(OnLoaded);
			if ( wasSelected || _hasPendingHoverEnd )
				this.HookUnhook_Loaded( true );
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
            
            
			// SSP 7/19/11 TFS81814
			// 
			//this.Loaded		-= new RoutedEventHandler(OnLoaded);
			this.HookUnhook_Loaded( false );

            // JJD 05/05/10 - TFS31370
            // if the record isn't selected then call VerifySelectedState so the
            // appropriate event will get raised
			//if (this._record != null && this._record.IsSelected)
            if (this._record != null)
            {
                if (this._record.IsSelected)
                    this.SetValue(DataRecordCellArea.IsSelectedProperty, KnownBoxes.TrueBox);
                else
                    this.VerifySelectedState();
            }

			// SSP 7/19/11 TFS81814
			// Since we skip raising the HoverEnd when the element is unloaded, we need to make
			// make sure we raise it when the element does get loaded otherwise a record could
			// retain hover highlight even though the mouse is no longer over it.
			// 
			this.VerifyPendingHoverEndRaised( );
		}

				#endregion //OnUnloaded, OnLoaded

				#region VerifyPendingHoverEndRaised

		// SSP 7/19/11 TFS81814
		// Added VerifyPendingHoverEndRaised method.
		// 
		/// <summary>
		/// Raises HoverEnd event if _hasPendingHoverEnd is true and we are in a state where we can
		/// safely raise it (checks like IsLoaded etc...).
		/// </summary>
		private void VerifyPendingHoverEndRaised( )
		{
			if ( _hasPendingHoverEnd )
			{
				if ( this.CanRaiseHoverEvent( false ) )
				{
					this.RaiseHoverEnd( new RoutedEventArgs( ) );
				}
				else
				{
					if ( !this.IsLoaded )
						this.HookUnhook_Loaded( true );
				}
			}
		}

				#endregion // VerifyPendingHoverEndRaised

				// JJD 03/30/10 TFS30222 - Added.  
                #region VerifySelectedState

        private void VerifySelectedState()
        {
            bool isSelectedNow = this.IsSelected;

            if (this._wasSelected != isSelectedNow)
            {
                this._wasSelected = isSelectedNow;

                if (isSelectedNow)
                {
                    this.RaiseSelected(new RoutedEventArgs());

                    // for active records we want to wire either the loaded or the
                    // unloaded event
					if ( this.IsLoaded )
					{
						// SSP 7/19/11 TFS81814
						// 
						//this.Unloaded += new RoutedEventHandler( this.OnUnloaded );
						this.HookUnhook_UnLoaded( true );
					}
					else
					{
						// SSP 7/19/11 TFS81814
						// 
						//this.Loaded += new RoutedEventHandler( this.OnLoaded );
						this.HookUnhook_Loaded( true );
					}
                }
                else
                {
                    this.RaiseDeselected(new RoutedEventArgs());

                    // Make sure we unwire the loaded/unloaded events
					// SSP 7/19/11 TFS81814
					// We need to hook into these events for other purposes as well.
					// 
                    //this.Unloaded -= new RoutedEventHandler(this.OnUnloaded);
                    //this.Loaded -= new RoutedEventHandler(this.OnLoaded);					
                }
            }
        }

                #endregion //VerifySelectedState	
    
			#endregion //Private Methods

		#endregion //Methods

		#region Events

			// JM 10-27-10 TFS49130/32061 - Added.
			#region HoverBeginEvent

		/// <summary>
		/// Event ID for the <see cref="HoverBegin"/> routed event
		/// </summary>
		public static readonly RoutedEvent HoverBeginEvent =
			EventManager.RegisterRoutedEvent("HoverBegin", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DataRecordCellArea));

		/// <summary>
		/// Occurs when the mnouse enters the DataRecordCellArea and elements representing a Hover state should be shown.
		/// </summary>
		protected virtual void OnHoverBegin(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseHoverBegin(RoutedEventArgs args)
		{
			// SSP 7/19/11 TFS81814
			// 
			_hasPendingHoverEnd = true;

			args.RoutedEvent	= DataRecordCellArea.HoverBeginEvent;
			args.Source			= this;
			this.OnHoverBegin(args);
		}

		/// <summary>
		/// Occurs when the mnouse enters the DataRecordCellArea and elements representing a Hover state should be shown.
		/// </summary>
		public event EventHandler<RoutedEventArgs> HoverBegin
		{
			add
			{
				base.AddHandler(DataRecordCellArea.HoverBeginEvent, value);
			}
			remove
			{
				base.RemoveHandler(DataRecordCellArea.HoverBeginEvent, value);
			}
		}

			#endregion //HoverBeginEvent

			// JM 10-27-10 TFS49130/32061 - Added.
			#region HoverEndEvent

		/// <summary>
		/// Event ID for the <see cref="HoverEnd"/> routed event
		/// </summary>
		public static readonly RoutedEvent HoverEndEvent =
			EventManager.RegisterRoutedEvent("HoverEnd", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DataRecordCellArea));

		/// <summary>
		/// Occurs when the mnouse exits the DataRecordCellArea and elements representing a Hover state should be hidden.
		/// </summary>
		protected virtual void OnHoverEnd(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseHoverEnd(RoutedEventArgs args)
		{
			// SSP 7/19/11 TFS81814
			// 
			_hasPendingHoverEnd = false;

			args.RoutedEvent	= DataRecordCellArea.HoverEndEvent;
			args.Source			= this;
			this.OnHoverEnd(args);
		}

		/// <summary>
		/// Occurs when the mnouse exits the DataRecordCellArea and elements representing a Hover state should be hidden.
		/// </summary>
		public event EventHandler<RoutedEventArgs> HoverEnd
		{
			add
			{
				base.AddHandler(DataRecordCellArea.HoverEndEvent, value);
			}
			remove
			{
				base.RemoveHandler(DataRecordCellArea.HoverEndEvent, value);
			}
		}

			#endregion //HoverEndEvent

			// JM 03-11-10 TFS27879 - Added.
			#region SelectedEvent

		/// <summary>
		/// Event ID for the <see cref="Selected"/> routed event
		/// </summary>
		public static readonly RoutedEvent SelectedEvent =
			EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DataRecordCellArea));

		/// <summary>
        /// Occurs when the DataRecordCellArea's record is selected.
        /// </summary>
		protected virtual void OnSelected(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseSelected(RoutedEventArgs args)
		{
			// JM 04-20-11 TFS66425 Return if either condition is true - don't require both.
			// JM 04-19-11 TFS73025
			//if (false == this.IsInitialized && false == this.IsLoaded)
			if (false == this.IsInitialized || false == this.IsLoaded)
				return;

			args.RoutedEvent	= DataRecordCellArea.SelectedEvent;
			args.Source			= this;
			this.OnSelected(args);
		}

		/// <summary>
        /// Occurs when the DataRecordCellArea's record is selected.
        /// </summary>
		public event EventHandler<RoutedEventArgs> Selected
		{
			add
			{
				base.AddHandler(DataRecordCellArea.SelectedEvent, value);
			}
			remove
			{
				base.RemoveHandler(DataRecordCellArea.SelectedEvent, value);
			}
		}

			#endregion //SelectedEvent

			// JM 03-11-10 TFS27879 - Added.
			#region DeselectedEvent

		/// <summary>
		/// Event ID for the <see cref="Deselected"/> routed event
		/// </summary>
		public static readonly RoutedEvent DeselectedEvent =
			EventManager.RegisterRoutedEvent("Deselected", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DataRecordCellArea));

		/// <summary>
		/// Occurs when the DataRecordCellArea's record is de-selected.
		/// </summary>
		protected virtual void OnDeselected(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDeselected(RoutedEventArgs args)
		{
			// JM 04-20-11 TFS66425 Return if either condition is true - don't require both.
			// JM 04-19-11 TFS73025
			//if (false == this.IsInitialized && false == this.IsLoaded)
			if (false == this.IsInitialized || false == this.IsLoaded)
				return;

			args.RoutedEvent	= DataRecordCellArea.DeselectedEvent;
			args.Source			= this;
			this.OnDeselected(args);
		}

		/// <summary>
		/// Occurs when the DataRecordCellArea's record is de-selected.
		/// </summary>
		public event EventHandler<RoutedEventArgs> Deselected
		{
			add
			{
				base.AddHandler(DataRecordCellArea.DeselectedEvent, value);
			}
			remove
			{
				base.RemoveHandler(DataRecordCellArea.DeselectedEvent, value);
			}
		}

			#endregion //DeselectedEvent

		#endregion //Events

		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs args = e as PropertyChangedEventArgs;

				if (args != null)
				{
					this.OnRecordPropertyChanged(sender, args);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for DataRecordCellArea, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for DataRecordCellArea, type: " + managerType != null ? managerType.ToString() : "null");

			return false;
		}

		#endregion

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private DataRecordCellArea _rs;

			internal StyleSelectorHelper(DataRecordCellArea rs) : base(rs)
			{
				this._rs = rs;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._rs == null)
						return null;

					FieldLayout fl = this._rs.FieldLayout;

					if (fl != null)
					{
						DataPresenterBase dp = fl.DataPresenter;

						if (dp != null)
							return dp.InternalRecordCellAreaStyleSelector.SelectStyle(this._rs.DataContext, this._rs);
					}

					return null;
				}
			}
		}

		#endregion //StyleSelectorHelper private class
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