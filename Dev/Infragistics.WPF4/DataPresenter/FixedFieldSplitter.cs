using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using Infragistics.Windows.Internal;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;

namespace Infragistics.Windows.DataPresenter
{
    // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
    /// <summary>
    /// Indicator used within the <see cref="VirtualizingDataRecordCellPanel"/> to change the <see cref="Field.FixedLocation"/> of the fields.
    /// </summary>
    /// <seealso cref="FieldLayoutSettings.FixedFieldUIType"/>
    //[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class FixedFieldSplitter : Thumb
	{
		#region Member Variables

		private FixedFieldSplitterDragInfo				_dragInfo;

		#endregion //Member Variables

		#region Constructor
		static FixedFieldSplitter()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FixedFieldSplitter), new FrameworkPropertyMetadata(typeof(FixedFieldSplitter)));

			// JJD 07/11/12 - TFS113976
			// Added VerticalCursor and HorizontalCursor properties that we could use in Style triggers to set the
			// appropriate cursor based on Orientation. This was so that we could remove the CursorExtension custom
			// MarkupExtension from our xaml. Expression Blend does not support custom markup extensions inside
 			// property setters
			Cursor vCursor = Utilities.LoadCursor(typeof(Utilities), "ResourceSets/Cursors/verticalSplitter.cur") ?? Cursors.SizeWE;
			VerticalCursorProperty = DependencyProperty.Register("VerticalCursor", typeof(Cursor), typeof(FixedFieldSplitter), new FrameworkPropertyMetadata(vCursor));

			Cursor hCursor = Utilities.LoadCursor(typeof(Utilities), "ResourceSets/Cursors/horizontalSplitter.cur") ?? Cursors.SizeNS;
			HorizontalCursorProperty = DependencyProperty.Register("HorizontalCursor", typeof(Cursor), typeof(FixedFieldSplitter), new FrameworkPropertyMetadata(hCursor));
		}

        /// <summary>
        /// Initializes a new <see cref="FixedFieldSplitter"/>
        /// </summary>
        public FixedFieldSplitter()
        {
        }

        internal FixedFieldSplitter(bool isInHeader, Orientation orientation, FixedFieldSplitterType splitterType) : this()
        {
            this.SetValue(IsInHeaderPropertyKey, KnownBoxes.FromValue(isInHeader));
            this.SetValue(OrientationPropertyKey, KnownBoxes.FromValue(orientation));
            this.SetValue(SplitterTypePropertyKey, splitterType);
        }
        #endregion //Constructor

		#region Base Class Overrides

    		#region OnCreateAutomationPeer
		/// <summary>
        /// Returns an automation peer that exposes the <see cref="FixedFieldSplitter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.FixedFieldSplitterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.FixedFieldSplitterAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer

			#region OnInitialized

		/// <summary>
		/// Invoked when the element is initialized.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.DragStarted	+= new DragStartedEventHandler(OnDragStarted);
			this.DragDelta		+= new DragDeltaEventHandler(OnDragDelta);
			this.DragCompleted	+= new DragCompletedEventHandler(OnDragCompleted);

            // AS 2/4/09 IsHighlighted
            // We need to bind the appropriate property on the field layout so we know
            // when to update the IsHighlighted property.
            //
            VirtualizingDataRecordCellPanel panel = Utilities.GetAncestorFromType(this, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;
            FieldLayout fl = null != panel ? panel.FieldLayout : null;
            
			
			
			

            if (null != fl)
            {
                DependencyProperty prop = this.SplitterType == FixedFieldSplitterType.Far
                    ? FieldLayout.IsOverFarSplitterProperty
                    : FieldLayout.IsOverNearSplitterProperty;

                this.SetBinding(IsOverAnySplitterProperty, Utilities.CreateBindingObject(prop, System.Windows.Data.BindingMode.OneWay, fl));
            }
		}

			#endregion //OnInitialized

            #region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse enters the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse event</param>
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            this.OnMouseEnterLeave(true);

            base.OnMouseEnter(e);
        } 
            #endregion //OnMouseEnter

            #region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse leaves the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse event</param>
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            this.OnMouseEnterLeave(false);

            base.OnMouseLeave(e);
        } 
            #endregion //OnMouseLeave

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				// JJD 07/11/12 - TFS113976 - added
				#region HorizontalCursor

		// JJD 07/11/12 - TFS113976
		// Added VerticalCursor and HorizontalCursor properties that we could use in Style triggers to set the
		// appropriate cursor based on Orientation. This was so that we could remove the CursorExtension custom
		// MarkupExtension from our xaml. Expression Blend does not support custom markup extensions inside
		// property setters

		/// <summary>
		/// Identifies the <see cref="HorizontalCursor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalCursorProperty;

		/// <summary>
		/// Gets/sets the cursor to use when the <see cref="Orientation"/>property is set to 'Horizontal'.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is used by triggers in the default style for this element.</para>
		/// </remarks>
		/// <seealso cref="HorizontalCursorProperty"/>
		/// <seealso cref="VerticalCursor"/>
		//[Description("Gets/sets the cursor to use when the Orientation property is set to 'Horizontal'.")]
		//[Category("Behavior")]
		public Cursor HorizontalCursor
		{
			get
			{
				return (Cursor)this.GetValue(FixedFieldSplitter.HorizontalCursorProperty);
			}
			set
			{
				this.SetValue(FixedFieldSplitter.HorizontalCursorProperty, value);
			}
		}

				#endregion //HorizontalCursor

                #region IsHighlighted

        private static readonly DependencyPropertyKey IsHighlightedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsHighlighted",
            typeof(bool), typeof(FixedFieldSplitter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsHighlighted"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsHighlightedProperty =
            IsHighlightedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns a boolean indicating if the mouse is over the splitter or any other splitter for the same <see cref="FieldLayout"/> with the same <see cref="SplitterType"/>.
        /// </summary>
        /// <seealso cref="IsHighlightedProperty"/>
        //[Description("Returns a boolean indicating if the mouse is over the splitter or any other splitter for the same 'FieldLayout' with the same 'SplitterType'.")]
        //[Category("Appearance")]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsHighlighted
        {
            get
            {
                return (bool)this.GetValue(FixedFieldSplitter.IsHighlightedProperty);
            }
        }

                #endregion //IsHighlighted

				#region IsInHeader

		/// <summary>
        /// IsInHeader Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsInHeaderPropertyKey
            = DependencyProperty.RegisterReadOnly("IsInHeader", typeof(bool), typeof(FixedFieldSplitter),
                new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Identifies the 'IsInHeader' dependency property
        /// </summary>
        public static readonly DependencyProperty IsInHeaderProperty
            = IsInHeaderPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates if the element is being used within a <see cref="HeaderPresenter"/>
        /// </summary>
        /// <seealso cref="IsInHeaderProperty"/>
        //[Description("Indicates if the element is being used within a HeaderPresenter.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public bool IsInHeader
        {
            get { return (bool)GetValue(IsInHeaderProperty); }
        }

        internal void SetIsInHeader(bool value)
        {
            this.SetValue(IsInHeaderPropertyKey, value);
        }

			 #endregion //IsInHeader

				#region Orientation

        private static readonly DependencyPropertyKey OrientationPropertyKey =
            DependencyProperty.RegisterReadOnly("Orientation",
            typeof(Orientation), typeof(FixedFieldSplitter), new FrameworkPropertyMetadata(KnownBoxes.OrientationVerticalBox));

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            OrientationPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the orientation of the splitter.
        /// </summary>
        /// <seealso cref="OrientationProperty"/>
        //[Description("Returns the orientation of the splitter.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Orientation Orientation
        {
            get
            {
                return (Orientation)this.GetValue(FixedFieldSplitter.OrientationProperty);
            }
        }

				#endregion //Orientation

				#region SplitterType

        /// <summary>
        /// SplitterType Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey SplitterTypePropertyKey
            = DependencyProperty.RegisterReadOnly("SplitterType", typeof(FixedFieldSplitterType), typeof(FixedFieldSplitter),
                new FrameworkPropertyMetadata(FixedFieldSplitterType.Near));

        /// <summary>
        /// Identifies the 'SplitterType' dependency property
        /// </summary>
        public static readonly DependencyProperty SplitterTypeProperty
            = SplitterTypePropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates the edge to which the fields will be fixed/unfixed using the splitter.
        /// </summary>
        /// <seealso cref="SplitterTypeProperty"/>
        //[Description("Indicates the edge to which the fields will be fixed/unfixed using the splitter.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public FixedFieldSplitterType SplitterType
        {
            get { return (FixedFieldSplitterType)GetValue(SplitterTypeProperty); }
        }

        internal void SetSplitterType(FixedFieldSplitterType value)
        {
            this.SetValue(SplitterTypePropertyKey, value);
        }

				#endregion //SplitterType

				// JJD 07/11/12 - TFS113976 - added
				#region VerticalCursor
		
		// JJD 07/11/12 - TFS113976
		// Added VerticalCursor and HorizontalCursor properties that we could use in Style triggers to set the
		// appropriate cursor based on Orientation. This was so that we could remove the CursorExtension custom
		// MarkupExtension from our xaml. Expression Blend does not support custom markup extensions inside
		// property setters

		/// <summary>
		/// Identifies the <see cref="VerticalCursor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalCursorProperty;

		/// <summary>
		/// Gets/sets the cursor to use when the <see cref="Orientation"/>property is set to 'Vertical'.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this property is used by triggers in the default style for this element.</para>
		/// </remarks>
		/// <seealso cref="VerticalCursorProperty"/>
		/// <seealso cref="HorizontalCursor"/>
		//[Description("Gets/sets the cursor to use when the Orientation property is set to 'Vertical'.")]
		//[Category("Behavior")]
		public Cursor VerticalCursor
		{
			get
			{
				return (Cursor)this.GetValue(FixedFieldSplitter.VerticalCursorProperty);
			}
			set
			{
				this.SetValue(FixedFieldSplitter.VerticalCursorProperty, value);
			}
		}

				#endregion //VerticalCursor

			#endregion //Public Properties

			#region Internal Properties

				#region FieldLayout

		internal FieldLayout FieldLayout
		{
			get
			{
				if (this._dragInfo.fieldLayout == null)
					this._dragInfo.fieldLayout = this.VirtualizingDataRecordCellPanel.FieldLayout;

				Debug.Assert(this._dragInfo.fieldLayout != null, "FieldLayout is null in FixedFieldSplitter!!");
				return this._dragInfo.fieldLayout;
			}
		}

				#endregion //FieldLayout	
    
        		#region IsOverAnySplitter

		private static readonly DependencyProperty IsOverAnySplitterProperty = DependencyProperty.Register("IsOverAnySplitter",
			typeof(bool), typeof(FixedFieldSplitter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsOverAnySplitterChanged)));

        private static void OnIsOverAnySplitterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(IsHighlightedPropertyKey, e.NewValue);
        }

        		#endregion //IsOverAnySplitter

				#region VirtualizingDataRecordCellPanel

		internal VirtualizingDataRecordCellPanel VirtualizingDataRecordCellPanel
		{
			get
			{
				if (this._dragInfo.virtualizingDataRecordCellPanel == null)
					this._dragInfo.virtualizingDataRecordCellPanel = this.Parent as VirtualizingDataRecordCellPanel;

				Debug.Assert(this._dragInfo.virtualizingDataRecordCellPanel != null, "VirtualizingDataRecordCellPanel is null in FixedFieldSplitter!!");
				return this._dragInfo.virtualizingDataRecordCellPanel;
			}
		}

				#endregion //VirtualizingDataRecordCellPanel	
    
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

            #region AddDropLocations
        // AS 2/2/09
        // Added helper routine to precalculate the locations for a splitter drag operation.
        //
        /// <summary>
        /// Helper method for creating SplitterDropInfo instances that indicate the valid drop locations for the splitter bar.
        /// </summary>
        /// <param name="currentLocation">The current FixedLocation for the Field's whose VirtualCellInfo should be processed in this call.</param>
        /// <param name="newLocation">The FixedLocation to which the fields will be positioned if the splitter operation occurs</param>
        private void AddDropLocations(FixedFieldLocation currentLocation, FixedFieldLocation newLocation)
        {
            #region Validation

            Debug.Assert(null != _dragInfo && null != _dragInfo.virtualCellInfos && null != _dragInfo.virtualizingDataRecordCellPanel);

            if (null == _dragInfo || null == _dragInfo.virtualCellInfos || null == _dragInfo.virtualizingDataRecordCellPanel)
                return;

            Debug.Assert(_dragInfo.dropInfos != null);

            if (_dragInfo.dropInfos == null)
                _dragInfo.dropInfos = new List<SplitterDropInfo>();

            Debug.Assert(currentLocation == FixedFieldLocation.Scrollable || newLocation == FixedFieldLocation.Scrollable);

            #endregion //Validation

            #region Setup

            SplitterMode splitterMode;
            SplitterDragDirection splitterDragDirection;
            Orientation splitterOrientation = this.Orientation;

            if (this.SplitterType == FixedFieldSplitterType.Near)
            {
                if (newLocation == FixedFieldLocation.Scrollable)
                {
                    splitterMode = SplitterMode.IsUnfixingNear;
                    splitterDragDirection = SplitterDragDirection.TowardsNearEdge;
                }
                else
                {
                    splitterMode = SplitterMode.IsFixingNear;
                    splitterDragDirection = SplitterDragDirection.TowardsFarEdge;
                }
            }
            else
            {
                if (newLocation == FixedFieldLocation.Scrollable)
                {
                    splitterMode = SplitterMode.IsUnfixingFar;
                    splitterDragDirection = SplitterDragDirection.TowardsFarEdge;
                }
                else
                {
                    splitterMode = SplitterMode.IsFixingFar;
                    splitterDragDirection = SplitterDragDirection.TowardsNearEdge;
                }
            }
            #endregion //Setup

            #region Filter VirtualCellInfos

            List<VirtualCellInfo> cellInfos = new List<VirtualCellInfo>();

            // first build a list of the usable cells
            foreach (VirtualCellInfo cellInfo in _dragInfo.virtualCellInfos)
            {
                // skip any item that isn't in the source location
                if (cellInfo.FixedLocation != currentLocation)
                    continue;

                // skip any item that can't go to the new location
                if (!cellInfo.Field.IsFixedLocationAllowed(newLocation))
                    continue;

                cellInfos.Add(cellInfo);
            }

            #endregion //Filter VirtualCellInfos

            #region Build SplitterDropInfo List

            List<SplitterDropInfo> dropInfos = new List<SplitterDropInfo>();

            for (int i = 0, count = cellInfos.Count; i < count; i++)
            {
                VirtualCellInfo cellInfo = cellInfos[i];
                Rect clipRect = cellInfo.ClipRect;

                double cellLeftTop, cellRightBottom, cellMidpoint;
                if (splitterOrientation == Orientation.Vertical)
                {
                    cellLeftTop = clipRect.Left;
                    cellRightBottom = clipRect.Right;
                    cellMidpoint = cellLeftTop + (clipRect.Right - clipRect.Left) / 2;
                }
                else
                {
                    cellLeftTop = clipRect.Top;
                    cellRightBottom = clipRect.Bottom;
                    cellMidpoint = cellLeftTop + (clipRect.Bottom - clipRect.Top) / 2;
                }

                SplitterDropInfo dropInfo = new SplitterDropInfo();
                dropInfo.midPt = cellMidpoint;
                dropInfo.dragDirection = splitterDragDirection;

                if (splitterDragDirection == SplitterDragDirection.TowardsNearEdge)
                    dropInfo.splitterPosition = cellLeftTop;
                else
                    dropInfo.splitterPosition = cellRightBottom;

                dropInfos.Add(dropInfo);
            }
            #endregion //Build SplitterDropInfo List

            #region Filter SplitterDropInfos
            if (dropInfos.Count > 1)
            {
                // filter the splitter drop infos for unique values
                dropInfos.Sort(Utilities.CreateComparer(new Comparison<SplitterDropInfo>(this.CompareDropInfos)));

                // to make it easier to remove items we'll iterate the list
                // backwards so we need to reverse the list
                dropInfos.Reverse();
                double previousPosition = dropInfos[dropInfos.Count - 1].splitterPosition;

                for (int i = dropInfos.Count - 2; i >= 0; i--)
                {
                    SplitterDropInfo dropInfo = dropInfos[i];

                    if (dropInfo.splitterPosition == previousPosition)
                        dropInfos.RemoveAt(i);
                    else
                        previousPosition = dropInfo.splitterPosition;
                }
            }
            #endregion //Filter SplitterDropInfos

            #region Validate & Add SplitterDropInfos
            foreach (SplitterDropInfo dropInfo in dropInfos)
            {
                double cellEdge = dropInfo.splitterPosition;

                #region Build AffectedFields List

                Rect affectedFieldRect = this.GetAffectedFieldsRect(splitterMode, cellEdge);

                List<Field> affectedFields = new List<Field>();

                foreach (VirtualCellInfo vci in cellInfos)
                {
                    if (affectedFieldRect.IsEmpty == false &&
                        DoesRectContainRect(affectedFieldRect, vci.ClipRect))
                    {
                        affectedFields.Add(vci.Field);
                    }
                }
                #endregion //Build AffectedFields List

                if (affectedFields.Count == 0)
                    continue;

                // when calculating what elements can be fixed, we need to consider the 
                // amount of space that will be left for the scrollable elements 
                if (newLocation != FixedFieldLocation.Scrollable)
                {
                    bool willHaveScrollableFields;
                    double scrollableExtent = _dragInfo.virtualizingDataRecordCellPanel.CalculateSplitterLayout(affectedFields, this.SplitterType, out willHaveScrollableFields);

                    // if the resulting scrollable extent is too small then its not a valid drop
                    if (willHaveScrollableFields && scrollableExtent <= 40)
                        continue;

                    // if fixing everything would push the fixed fields out of view
                    // then do not support it
                    if (!willHaveScrollableFields && scrollableExtent < 0)
                        continue;
                }

                dropInfo.affectedFields = affectedFields;
                _dragInfo.dropInfos.Add(dropInfo);
            }
            #endregion //Validate & Add SplitterDropInfos
        }
            #endregion //AddDropLocations

            #region CancelDragStarted
        /// <summary>
        /// Helper method to call from within the DragStarted if it is determined that we need to prevent/cancel the drag.
        /// </summary>
        private void CancelDragStarted()
        {
            this._dragInfo = null;

            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            else if (this.IsDragging)
                this.CancelDrag();

            return;
        } 
            #endregion //CancelDragStarted

            #region CompareDropInfos
        private int CompareDropInfos(SplitterDropInfo x, SplitterDropInfo y)
        {
            if (x == y)
                return 0;

            int result = x.splitterPosition.CompareTo(y.splitterPosition);

            if (result == 0)
            {
                if (x.midPt != y.midPt)
                {
                    Debug.Assert(x.dragDirection == y.dragDirection);

                    result = x.midPt.CompareTo(y.midPt);

                    // when 2 different cells have the same splitter position but 
                    // different mid points we must have a case where the cells have 
                    // different spans. in this case we want to keep the ones with
                    // the mid pt further from the splitter.
                    if (x.dragDirection == SplitterDragDirection.TowardsFarEdge)
                    {
                        result = result == 1 ? -1 : 1;
                    }
                }
            }

            return result;
        } 
            #endregion //CompareDropInfos

			#region CreateFixedAreaElementAdorner

		private void CreateFixedAreaElementAdorner(FrameworkElement fixedAreaElement)
		{
			this._dragInfo.fixedAreaElementAdorner = new FixedAreaElementAdorner(fixedAreaElement);

			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(fixedAreaElement);
			if (adornerLayer != null)
				adornerLayer.Add(this._dragInfo.fixedAreaElementAdorner);
		}

			#endregion //CreateFixedAreaElementAdorner

			#region CreateSplitterPreviewRectangleInAdornerLayer

		private bool CreateSplitterPreviewRectangleInAdornerLayer()
		{
			// Create a Rectangle element and set some properties.
			Rectangle previewRectangle	= new Rectangle();

			previewRectangle.Opacity	= .18d;
			previewRectangle.Fill		= Brushes.Black;
			
			if (this.Orientation == Orientation.Vertical)
			{
				previewRectangle.Height = this._dragInfo.fixedFieldInfo.FixedAreaElement.ActualHeight;
				previewRectangle.Width	= this.ActualWidth;
				Canvas.SetTop(previewRectangle, 0.0);
				Canvas.SetLeft(previewRectangle, this.TranslateLeftCoordinate(this, this._dragInfo.fixedFieldInfo.FixedAreaElement, 0, true));
			}
			else
			{
				previewRectangle.Height = this.ActualHeight;
				previewRectangle.Width	= this._dragInfo.fixedFieldInfo.FixedAreaElement.ActualWidth;
				Canvas.SetLeft(previewRectangle, 0.0);
				Canvas.SetTop(previewRectangle, this.TranslateTopCoordinate(this, this._dragInfo.fixedFieldInfo.FixedAreaElement, 0, true));
			}

			// Create the FixedAreaElementAdorner and add it to the AdornerLayer of the FixedAreaElement.
			this.CreateFixedAreaElementAdorner(this._dragInfo.fixedFieldInfo.FixedAreaElement);
			if (this._dragInfo.fixedAreaElementAdorner == null)
			{
				this._dragInfo = null;
				return false;
			}


			// Add the preview rectangle to the adorner.
			this._dragInfo.fixedAreaElementAdorner.AddChildElement(previewRectangle);

			// Save a reference to the preview rectangle.
			this._dragInfo.splitterPreview = previewRectangle;

			return true;
		}

			#endregion //CreateSplitterPreviewRectangleInAdornerLayer

			#region DoesRectContainRect



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static bool DoesRectContainRect(Rect containingRect, Rect containedRect)
		{
			return (containingRect.X <= containedRect.X || GridUtilities.AreClose(containingRect.X, containedRect.X))	&& 
				   (containingRect.Y <= containedRect.Y || GridUtilities.AreClose(containingRect.Y, containedRect.Y))	&&
 
				   ((containingRect.X + containingRect.Width)	>= (containedRect.X + containedRect.Width)	|| GridUtilities.AreClose((containingRect.X + containingRect.Width), (containedRect.X + containedRect.Width))) && 
				   ((containingRect.Y + containingRect.Height)	>= (containedRect.Y + containedRect.Height)	|| GridUtilities.AreClose((containingRect.Y + containingRect.Height), (containedRect.Y + containedRect.Height)));
		}

			#endregion //DoesRectContainRect

			#region GetAffectedFieldsRect

		private Rect GetAffectedFieldsRect(SplitterMode splitterMode, double nearestCellEdge)
		{
            // AS 2/13/09 TFS13982
            // Changed to use the actual scrollable area instead of the calculated
            // one based on the splitter position since this will be compared against 
            // the virtualcellinfos which are based on the scrollable area.
            //
            Rect scrollableArea = this.VirtualizingDataRecordCellPanel.ScrollableAreaRect;

			// Construct a rect (in VirtualizingDataRecordCellPanel coordinates) that represents the 'selected' area.  For example, if the near splitter 
			// is dragged to the right and the current orientation is vertical, the 'selected area' is an imaginary rect with a height equal 
			// to the height of the splitter element and a width equal to the current delta. 
			double left = 0, top = 0, width = 0, height = 0;
			if (this.Orientation == Orientation.Vertical)
			{
				switch (splitterMode)
				{
					case SplitterMode.IsFixingNear:
					case SplitterMode.IsUnfixingFar:
                        // AS 2/13/09 TFS13982
                        //left	= this.TranslateLeftCoordinate(this, this.VirtualizingDataRecordCellPanel, this.ActualWidth, true);
						//top		= this.TranslateTopCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true);
                        //width	= nearestCellEdge - left;
                        //height	= this.ActualHeight;
                        left    = scrollableArea.Left;
                        top     = scrollableArea.Top;
						width	= nearestCellEdge - left;
						height	= scrollableArea.Height;
						break;

					case SplitterMode.IsUnfixingNear:
					case SplitterMode.IsFixingFar:
						left	= nearestCellEdge;
                        // AS 2/13/09 TFS13982
                        //top = this.TranslateTopCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true);
						//width	= this.TranslateLeftCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true) - nearestCellEdge;
                        //height	= this.ActualHeight;
                        top = scrollableArea.Top;
                        width = scrollableArea.Right - nearestCellEdge;
						height	= scrollableArea.Height;
						break;
				}
			}
			else //Orientation.Horizontal
			{
				switch (splitterMode)
				{
					case SplitterMode.IsFixingNear:
					case SplitterMode.IsUnfixingFar:
                        // AS 2/13/09 TFS13982
                        //left = this.TranslateLeftCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true);
                        //top		= this.TranslateTopCoordinate(this, this.VirtualizingDataRecordCellPanel, this.ActualHeight, true);
                        //width	= this.ActualWidth;
                        //height	= nearestCellEdge - top;
                        left = scrollableArea.Left;
						top		= scrollableArea.Top;
						width	= scrollableArea.Width;
						height	= nearestCellEdge - top;
						break;

					case SplitterMode.IsUnfixingNear:
					case SplitterMode.IsFixingFar:
                        // AS 2/13/09 TFS13982
                        //left = this.TranslateLeftCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true);
                        //top		= nearestCellEdge;
                        //width	= this.ActualWidth;
                        //height	= this.TranslateTopCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true) - nearestCellEdge;
                        left = scrollableArea.Left;
						top		= nearestCellEdge;
						width	= scrollableArea.Width;
						height	= scrollableArea.Bottom - nearestCellEdge;
						break;
				}
			}


			// Account for possible rounding errors.
			width	= Math.Max(width, 0);
			height	= Math.Max(height, 0);


			if (left != 0 && top != 0 && width != 0 && height != 0)
				return Rect.Empty;
			else
				return new Rect(left, top, width, height);
		}

			#endregion //GetAffectedFieldsRect

			#region GetOtherSplitterPosInVDRCPCoords

		private double GetOtherSplitterPosInVDRCPCoords()
		{
			if (this.SplitterType == FixedFieldSplitterType.Near)
			{
				if (this.VirtualizingDataRecordCellPanel.FarSplitter			== null ||
					this.VirtualizingDataRecordCellPanel.FarSplitter.Visibility != Visibility.Visible)
					return double.NaN;
				else
				{
					if (this.Orientation == Orientation.Vertical)
						return this.TranslateLeftCoordinate(this.VirtualizingDataRecordCellPanel.FarSplitter, this.VirtualizingDataRecordCellPanel, 0, true);
					else
						return this.TranslateTopCoordinate(this.VirtualizingDataRecordCellPanel.FarSplitter, this.VirtualizingDataRecordCellPanel, 0, true);
				}
			}
			else
			{
				if (this.VirtualizingDataRecordCellPanel.NearSplitter				== null ||
					this.VirtualizingDataRecordCellPanel.NearSplitter.Visibility	!= Visibility.Visible)
					return double.NaN;
				else
				{
					if (this.Orientation == Orientation.Vertical)
						return this.TranslateLeftCoordinate(this.VirtualizingDataRecordCellPanel.NearSplitter, this.VirtualizingDataRecordCellPanel, this.VirtualizingDataRecordCellPanel.NearSplitter.ActualWidth, true);
					else
						return this.TranslateTopCoordinate(this.VirtualizingDataRecordCellPanel.NearSplitter, this.VirtualizingDataRecordCellPanel, this.VirtualizingDataRecordCellPanel.NearSplitter.ActualHeight, true);
				}
			}
		}

			#endregion //GetOtherSplitterPosInVDRCPCoords
			
			#region OnDragCompleted

		void OnDragCompleted(object sender, DragCompletedEventArgs e)
		{
            this.ProcessOnDragCompleted();
        }

        private void ProcessOnDragCompleted()
        {
			// Make sure we were dragging.
			if (this._dragInfo == null)
				return;


			// Cleanup.
			this._dragInfo.fixedAreaElementAdorner.RemoveChildElement(this._dragInfo.splitterPreview);
			this.RemoveFixedAreaElementAdorner(this._dragInfo.fixedFieldInfo.FixedAreaElement);








			// Tell the VirtualizingDataRecordCellPanel to process the affected fields.
			if (this._dragInfo.affectedFields.Count > 0)
			{
				// Turn off the IsFixedStateChanging property before calling the VirtualizingDataRecordCellPanel.
				foreach (Field field in this._dragInfo.affectedFields)
				{
					field.SetValue(Field.IsFixedStateChangingPropertyKey, false);
				}

				this.VirtualizingDataRecordCellPanel.ProcessFixedFieldSplitterAction(this._dragInfo.affectedFields, this.SplitterType);
			}

			this._dragInfo = null;
		}

			#endregion //OnDragCompleted	
    
			#region OnDragDelta

		void OnDragDelta(object sender, DragDeltaEventArgs e)
		{
            this.ProcessOnDragDelta(e.HorizontalChange, e.VerticalChange);
        }

        private void ProcessOnDragDelta(double horizontalChange, double verticalChange)
        {
			// Make sure we have enough info to continue.
			if (this._dragInfo == null)
				return;

			if (this._dragInfo.splitterPreview	== null ||
				this._dragInfo.fixedFieldInfo	== null)
				return;

            if (this._dragInfo.dropInfos == null)
                return;

			// Get the current delta from the appropriate dimensional change.
			double currentDelta = 0;
			if (this.Orientation == Orientation.Vertical)
				currentDelta = horizontalChange;
			else
				currentDelta = verticalChange;

			// Determine the splitter mode.
			SplitterMode splitterMode;
			if (this.SplitterType == FixedFieldSplitterType.Near)
			{
				if (currentDelta > 0)
					splitterMode = SplitterMode.IsFixingNear;
				else
					splitterMode = SplitterMode.IsUnfixingNear;
			}
			else
			{
				if (currentDelta > 0)
					splitterMode = SplitterMode.IsUnfixingFar;
				else
					splitterMode = SplitterMode.IsFixingFar;
			}


			// Determine the Splitter drag direction
			SplitterDragDirection splitterDragDirection;
			if (currentDelta < 0)
				splitterDragDirection = SplitterDragDirection.TowardsNearEdge;
			else
				splitterDragDirection = SplitterDragDirection.TowardsFarEdge;

			// Get the position of the splitter in VirtualizingDataRecordCellPanel coordinates.
			double splitterPosInVDRCPCoords;
			if (this.Orientation == Orientation.Vertical)
				splitterPosInVDRCPCoords = this.TranslateLeftCoordinate(this, this.VirtualizingDataRecordCellPanel, horizontalChange, true);
			else
				splitterPosInVDRCPCoords = this.TranslateTopCoordinate(this, this.VirtualizingDataRecordCellPanel, verticalChange, true);


			// Look through all the cells and determine the nearest cell edge ahead of the splitter based on the current splitterDragDirection, 
			// the splitterPosInFixedAreaElementCoords, the orientation and the 'fixability' of each cell.  Also make sure that the splitter is 
			// past the middle of the cell.
			double					nearestCellEdge		= double.NaN;

            FixedFieldLocation? newLocation;

            switch (splitterMode)
            {
                case SplitterMode.IsFixingFar:
                    newLocation = FixedFieldLocation.FixedToFarEdge;
                    break;
                case SplitterMode.IsFixingNear:
                    newLocation = FixedFieldLocation.FixedToNearEdge;
                    break;
                case SplitterMode.IsUnfixingFar:
                case SplitterMode.IsUnfixingNear:
                    newLocation = FixedFieldLocation.Scrollable;
                    break;
                default:
                    newLocation = null;
                    break;
            }

            // we need to know where the splitter was when we start so we can ignore
            // any drop locations on the other side of the splitter
            double splitterEdge = this.Orientation == Orientation.Vertical 
                ? this.TranslateLeftCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true) 
                : this.TranslateTopCoordinate(this, this.VirtualizingDataRecordCellPanel, 0, true);

            SplitterDropInfo newDropInfo = null;

            foreach (SplitterDropInfo dropInfo in _dragInfo.dropInfos)
            {
                if (splitterDragDirection == SplitterDragDirection.TowardsNearEdge)
                {
                    if (dropInfo.midPt < splitterEdge &&                    // on the near side of the splitter
                        dropInfo.midPt > splitterPosInVDRCPCoords &&        // its closer to the splitter than the delta
                        (double.IsNaN(nearestCellEdge) ||                   // no nearest edge yet
                            dropInfo.splitterPosition < nearestCellEdge))   // or this is closer to the delta
                    {
                        nearestCellEdge = dropInfo.splitterPosition;
                        newDropInfo = dropInfo;
                    }
                }
                else
                {
                    if (dropInfo.midPt > splitterEdge &&                    // on the far side of the splitter
                        dropInfo.midPt < splitterPosInVDRCPCoords &&        // its closer to the splitter than the delta
                        (double.IsNaN(nearestCellEdge) ||                   // no nearest edge yet
                            dropInfo.splitterPosition > nearestCellEdge))   // or this is closer to the delta
                    {
                        nearestCellEdge = dropInfo.splitterPosition;
                        newDropInfo = dropInfo;
                    }
                }
            }

			// Set the splitter preview position to the nearest edge calculated above.

            // if we didn't find a drop location then just position the preview over the splitter
            if (null == newDropInfo)
                nearestCellEdge = splitterEdge;

			// Set the appropriate coordinate on the splitter preview  position depending on our Orientation.
			if (this.Orientation == Orientation.Vertical)
				Canvas.SetLeft(this._dragInfo.splitterPreview, this.TranslateLeftCoordinate(this.VirtualizingDataRecordCellPanel, this._dragInfo.fixedAreaElementAdorner, nearestCellEdge, true));
			else
				Canvas.SetTop(this._dragInfo.splitterPreview, this.TranslateTopCoordinate(this.VirtualizingDataRecordCellPanel, this._dragInfo.fixedAreaElementAdorner, nearestCellEdge, true));

			this._dragInfo.fixedAreaElementAdorner.InvalidateArrange();
			this._dragInfo.fixedAreaElementAdorner.UpdateLayout();

			this._dragInfo.affectedFields.Clear();

            // update the IsFixedStateChanging property for the new affected fields
            foreach (VirtualCellInfo vci in this._dragInfo.virtualCellInfos)
            {
                if (null != newDropInfo && 
                    newDropInfo.affectedFields.Contains(vci.Field))
                {
                    Debug.Assert(vci.Field.IsFixedLocationAllowed(newLocation.Value));
					vci.Field.SetValue(Field.IsFixedStateChangingPropertyKey, KnownBoxes.TrueBox);
                }
                else
                    vci.Field.ClearValue(Field.IsFixedStateChangingPropertyKey);
            }

            if (null != newDropInfo)
                _dragInfo.affectedFields.AddRange(newDropInfo.affectedFields);
		}

			#endregion //OnDragDelta	
    
			#region OnDragStarted

		void OnDragStarted(object sender, DragStartedEventArgs e)
		{
            bool started = this.ProcessOnDragStarted();

            if (!started)
                e.Handled = true;
        }

        private bool ProcessOnDragStarted()
        {
			// Make sure there is no previous drag pending.
			if (this._dragInfo != null)
				return false;

			// Allocate a new drag info instance to hold state while dragging.
			this._dragInfo = new FixedFieldSplitterDragInfo();


			// Make sure we have the context we need.
			if (this.VirtualizingDataRecordCellPanel	== null ||
				this.FieldLayout						== null)
			{
                this.CancelDragStarted();
                return false;
			}


			// Get the FixedFieldInfo from the current view so we can get the FixedAreaElement within which we will draw the splitter preview
			// during dragging.
			this._dragInfo.fixedFieldInfo = null;

			if (this.FieldLayout.DataPresenter						!= null &&
				this.FieldLayout.DataPresenter.CurrentViewInternal	!= null)
			{
                RecordPresenter rp = null;

				if (!this.VirtualizingDataRecordCellPanel.IsHeaderArea)
					rp = this.VirtualizingDataRecordCellPanel.Record.AssociatedRecordPresenter;

                if (null == rp)
                {
                    rp = Utilities.GetAncestorFromType(this.VirtualizingDataRecordCellPanel, typeof(RecordPresenter), true) as RecordPresenter;
                    Debug.Assert(rp != null, "Cannot find RecordPresenter!");
                }

                if (null != rp)
    				this._dragInfo.fixedFieldInfo = this.FieldLayout.DataPresenter.CurrentViewInternal.GetFixedFieldInfo(rp);
			}

			if (this._dragInfo.fixedFieldInfo == null || this._dragInfo.fixedFieldInfo.FixedAreaElement == null)
			{
                this.CancelDragStarted();
                return false;
			}

            // AS 1/28/09
            // If there is a cell in edit mode then we should end edit more before allowing the drag to start since 
            // the result of the drag will be to regenerate the templates.
            //
            if ( !this.FieldLayout.DataPresenter.EndEditMode(true, false) )
            {
                this.CancelDragStarted();
                return false;
            }
			
			// Get the layout information for the 'in view' cells from the VirtualizingDataRecordCellPanel.  We will use this snapshot
			// of the cell layout information for the duration of this splitter drag.
			this._dragInfo.virtualCellInfos	= this.VirtualizingDataRecordCellPanel.GetCellInfo(true);


			// Allocate a new 'affected fields' list.
			this._dragInfo.affectedFields	= new List<Field>();


			// Create a Rectangle in the AdornerLayer of the FixedAreaElement that we will drag around in the UI.
			if (false == this.CreateSplitterPreviewRectangleInAdornerLayer())
			{
                this.CancelDragStarted();
				return false;
			}

            // AS 2/2/09
            // We need to pre-calculate the allowed drop locations for the splitter since 
            // a valid drop location is based on the size left for the scrollable area.
            //
            _dragInfo.dropInfos = new List<SplitterDropInfo>();
            FixedFieldLocation fixedLocation = this.SplitterType == FixedFieldSplitterType.Far
                ? FixedFieldLocation.FixedToFarEdge
                : FixedFieldLocation.FixedToNearEdge;

            AddDropLocations(FixedFieldLocation.Scrollable, fixedLocation);
            AddDropLocations(fixedLocation, FixedFieldLocation.Scrollable);

            return _dragInfo.dropInfos.Count > 0;
		}

			#endregion //OnDragStarted	

            #region OnMouseEnterLeave
        private void OnMouseEnterLeave(bool enter)
        {
            VirtualizingDataRecordCellPanel panel = Utilities.GetAncestorFromType(this, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;
            FieldLayout fl = null != panel ? panel.FieldLayout : null;
            Debug.Assert(null != fl);

            if (null != fl)
            {
                // update the dp on the field layout so that it reflects when the mouse
                // is over any element using the same field layout with the same splitter type
                DependencyProperty prop = this.SplitterType == FixedFieldSplitterType.Far
                    ? FieldLayout.IsOverFarSplitterProperty
                    : FieldLayout.IsOverNearSplitterProperty;

                fl.SetValue(prop, enter ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);
            }
        } 
            #endregion //OnMouseEnterLeave

            #region PerformMove
        internal void PerformMove(double x, double y)
        {
            Point clientPt = Utilities.PointFromScreenSafe(this, new Point(x, y));

            if (this.ProcessOnDragStarted())
            {
                this.ProcessOnDragDelta(clientPt.X, clientPt.Y);
                this.ProcessOnDragCompleted();
            }
        } 
            #endregion //PerformMove

			#region RemoveFixedAreaElementAdorner

		private void RemoveFixedAreaElementAdorner(FrameworkElement fixedAreaElement)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(fixedAreaElement);
			if (adornerLayer != null)
				adornerLayer.Remove(this._dragInfo.fixedAreaElementAdorner);

			this._dragInfo.fixedAreaElementAdorner = null;
		}

			#endregion //RemoveFixedAreaElementAdorner

			#region TranslateLeftCoordinate

		private double TranslateLeftCoordinate(FrameworkElement from, FrameworkElement relativeTo, double left, bool constrainToBounds)
		{
			Point p = new Point(left, 0);

			double translatedLeft = from.TranslatePoint(p, relativeTo).X;
			if (constrainToBounds)
				translatedLeft = Math.Min(Math.Max(translatedLeft, 0), relativeTo.ActualWidth - this.ActualWidth);

			return translatedLeft;
		}

			#endregion //TranslateLeftCoordinate

			#region TranslateTopCoordinate

		private double TranslateTopCoordinate(FrameworkElement from, FrameworkElement relativeTo, double top, bool constrainToBounds)
		{
			Point p = new Point(0, top);

			double translatedTop = from.TranslatePoint(p, relativeTo).Y;
			if (constrainToBounds)
				translatedTop = Math.Min(Math.Max(translatedTop, 0), relativeTo.ActualHeight - this.ActualHeight);

			return translatedTop;
		}

			#endregion //TranslateTopCoordinate

		#endregion //Methods

		#region Nested Private Class FixedFieldSplitterDragInfo

		private class FixedFieldSplitterDragInfo
		{
			internal VirtualizingDataRecordCellPanel				virtualizingDataRecordCellPanel;
			internal FieldLayout									fieldLayout;
			internal List<VirtualCellInfo>							virtualCellInfos;
			internal FixedFieldInfo									fixedFieldInfo;
			internal FixedAreaElementAdorner						fixedAreaElementAdorner;
			internal Rectangle										splitterPreview;
			internal List<Field>									affectedFields;

            // AS 2/2/09
            // Precalculate the value drop positions since we need to calculate the
            // resulting layout to know what the resulting scrollable area extent 
            // will be.
            //
            internal List<SplitterDropInfo>                         dropInfos;

			internal FixedFieldSplitterDragInfo()
			{
			}
		}

		#endregion //Nested Private Class FixedFieldSplitterDragInfo

		#region Nested Private Class SplitterDropInfo
        // AS 2/2/09
        // Added helper class to manage the information we compare against during the OnDragDelta.
        //
        private class SplitterDropInfo
        {
            // the offset that the mouse must be past in order to use this location
            internal double midPt;

            // the offset at which the splitter should be positioned when this location is used
            internal double splitterPosition;

            // the resulting fixed location for the fields
            internal SplitterDragDirection dragDirection;

            // the list of affected fields if this location is used
            internal List<Field> affectedFields;
        }
		#endregion //Nested Private Class SplitterDropInfo

		#region Nested Private Class FixedAreaElementAdorner

	private class FixedAreaElementAdorner : AdornerEx
	{
		#region Member Variables

		private List<UIElement>				_children = null;

		#endregion //Member Variables

		#region Constructor

		internal FixedAreaElementAdorner(FrameworkElement fixedAreaElement) : base(fixedAreaElement)
		{
		}

		#endregion //Constructor	

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (this.Children.Count > 0)
			{
				UIElement splitterPreview = this.Children[0];
				if (splitterPreview != null)
				{
					Rect arrangeRect = new Rect(Canvas.GetLeft(splitterPreview),
												Canvas.GetTop(splitterPreview),
												splitterPreview.DesiredSize.Width,
												splitterPreview.DesiredSize.Height);

					splitterPreview.Arrange(arrangeRect);
				}
			}

			return finalSize;
		}

			#endregion //ArrangeOverride

			#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= this.Children.Count)
				return null;

			return this.Children[index];
		}

			#endregion //GetVisualChild

			#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get { return this.Children.Count; }
		}

			#endregion //VisualChildrenCount

		#endregion //Base Class Overrides

		#region Properties

			#region Children

		private List<UIElement> Children
		{
			get
			{
				if (this._children == null)
					this._children = new List<UIElement>(3);

				return this._children;
			}
		}

			#endregion //Children

		#endregion //Properties

		#region Methods

			#region AddChildElement

		internal void AddChildElement(UIElement childElement)
		{
			this.AddVisualChild(childElement);
			this.Children.Add(childElement);
		}

			#endregion //AddChildElement

			#region RemoveChildElement

		internal void RemoveChildElement(UIElement childElement)
		{
			this.RemoveVisualChild(childElement);
			this.Children.Remove(childElement);
		}

			#endregion //RemoveChildElement

		#endregion //Methods
	}

		#endregion //Nested Private Class FixedAreaElementAdorner

		#region Nested Private Enumeration SplitterMode

		private enum SplitterMode
		{
			Inactive,
			IsFixingNear,
			IsFixingFar,
			IsUnfixingNear,
			IsUnfixingFar
		}

		#endregion //Nested Private Enumeration SplitterMode

		#region Nested Private Enumeration SplitterDragDirection

		private enum SplitterDragDirection
		{
			NotMoving,
			TowardsNearEdge,
			TowardsFarEdge
		}

		#endregion //Nested Private Enumeration SplitterDragDirection
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