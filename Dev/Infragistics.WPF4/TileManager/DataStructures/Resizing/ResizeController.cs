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
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Markup;
using Infragistics.Controls.Layouts;

namespace Infragistics.Controls
{
    #region IResizeHost interface

    /// <summary>
    /// Interface implemented by an element that contains resizable items.
    /// </summary>
    internal interface IResizeHost
    {
        #region Properties

            #region Controller

        /// <summary>
        /// Returns the <see cref="ResizeController"/> (read-only).
        /// </summary>
        ResizeController Controller { get; }

            #endregion //Controller	
    
            #region RootElement

        /// <summary>
        /// Returns the root element which normally is the control itself (read-only).
        /// </summary>
        FrameworkElement RootElement { get; }

            #endregion //RootElement

        #endregion //Properties

        #region Methods

            #region AddResizerBar

        /// <summary>
        /// Called at the start of a resize operation to add the resizer bar to the parent tree
        /// </summary>
		/// <param name="resizerBar">The resizer bar to add</param>
        void AddResizerBar(FrameworkElement resizerBar);
            
            #endregion //AddResizerBar	

			// JJD 03/08/12 TFS100150 - Added touch support
			#region CanCaptureMouse
    
		/// <summary>
		/// Determines if the control is in a state to allow mouse capture.
		/// </summary>
		/// <returns>True if the mouse can be captured at this time</returns>
    	bool CanCaptureMouse(MouseButtonEventArgs e, FrameworkElement itemToResize, bool? resizeInXAxis);

   			#endregion //CanCaptureMouse	
    
            #region CanResize

        /// <summary>
        /// Determines if the resizing is allowed.
        /// </summary>
        /// <param name="resizableItem">The item that contains the ResizeContext</param>
        /// <param name="resizeInXAxis">True to resize the width and false to resize the height.</param>
        /// <returns>True if resizing in this dimension is allowed.</returns>
        bool CanResize(FrameworkElement resizableItem, bool resizeInXAxis);

            #endregion //CanResize	
    
            #region GetResizeAreaForItem

        /// <summary>
        /// Gets an element that defines the resize area for an item.
        /// </summary>
        /// <param name="resizableItem">The item to be resized.</param>
        /// <returns>The resize are.</returns>
        FrameworkElement GetResizeAreaForItem(FrameworkElement resizableItem);

            #endregion //GetResizeAreaForItem	
    
            #region RemoveResizerBar

        /// <summary>
        /// Called at the end of a resize operation to remove the resizer bar from the parent tree
        /// </summary>
		/// <param name="resizerBar">The resizer bar to remove</param>
        void RemoveResizerBar(FrameworkElement resizerBar);

            #endregion //RemoveResizerBar	
    
            #region Resize

        /// <summary>
        /// Resizes the item.
        /// </summary>
        /// <param name="resizableItem">The item that contains the ResizeContext</param>
        /// <param name="resizeInXAxis">True to resize the width and false to resize the height.</param>
        /// <param name="delta">The resize delta in device-independent units (1/96th inch per unit).</param>
        void Resize(FrameworkElement resizableItem, bool resizeInXAxis, double delta);

            #endregion //Resize	
    
            #region GetResizeCursor

        /// <summary>
        /// Gets the cursor to use while the mouse is over a resizable border.
        /// </summary>
        /// <param name="resizableItem">The item that contains the ResizeContext</param>
        /// <param name="resizeInXAxis">True to resize the width and false to resize the height.</param>
        /// <param name="cursor">The normal default cursor for this type of a resize operation.</param>
        /// <returns>The cursor to display while the mouse is over the resize border.</returns>
        Cursor GetResizeCursor(FrameworkElement resizableItem, bool resizeInXAxis, Cursor cursor);

            #endregion //GetResizeCursor	
    
            #region InitializeResizeConstraints

        /// <summary>
        /// Called before a resize operation begins. 
        /// </summary>
        /// <param name="resizeArea">The element that defines the resize area.</param>
        /// <param name="resizableItem">The item to be resized</param>
        /// <param name="constraints">The constarints to apply to the resize operation.</param>
		void InitializeResizeConstraints(FrameworkElement resizeArea, FrameworkElement resizableItem, ResizeConstraints constraints);

            #endregion //InitializeResizeConstraints	
    
        #endregion //Methods	
    }

    #endregion IResizeHost interface

	#region IResizeHostMulti
	/// <summary>
	/// Interface implemented by an element that contains resizable items that may support sizing of their contents in both dimensions at the same time.
	/// </summary>
    internal interface IResizeHostMulti : IResizeHost
    {
        #region CanResizeInBothDimensions

        /// <summary>
        /// Determines if the resizing is allowed in both dimensions at the same time.
        /// </summary>
        /// <param name="resizableItem">The item that contains the ResizeContext</param>
         /// <returns>True if resizing in both dimensions is allowed.</returns>
        bool CanResizeInBothDimensions(FrameworkElement resizableItem);

        #endregion //CanResizeInBothDimensions
    
        #region GetMultiResizeCursor

        /// <summary>
        /// Gets the cursor to use while the mouse is over the resize corner.
        /// </summary>
        /// <param name="resizableItem">The item that contains the ResizeContext</param>
        /// <param name="cursor">The normal default cursor for this type of a resize operation.</param>
        /// <returns>The cursor to display while the mouse is over the resize corner.</returns>
        Cursor GetMultiResizeCursor(FrameworkElement resizableItem, Cursor cursor);

        #endregion //GetMultiResizeCursor	
    
        #region Resize

        /// <summary>
        /// Resizes the item in both dimensions.
        /// </summary>
        /// <param name="resizableItem">The item that contains the ResizeContext</param>
        /// <param name="deltaX">The resize delta in the x dimension in device-independent units (1/96th inch per unit).</param>
        /// <param name="deltaY">The resize delta in the x dimension in device-independent units (1/96th inch per unit).</param>
        void ResizeBothDimensions(FrameworkElement resizableItem, double deltaX, double deltaY);

        #endregion //Resize	
    } 
	#endregion //IResizeHostMulti

	#region IAutoResizeHost
	/// <summary>
	/// Interface implemented by an element that contains resizable items that may support sizing to their contents.
	/// </summary>
    // JJD 1/19/10 - TilesControl
    // IAutoResizeHost should not derive from IResizeHost since we are adding
    // A new derived interface, IResizeHostMulti, to allow corner resizing for both width and height at the same time
    //public interface IAutoResizeHost : IResizeHost
	internal interface IAutoResizeHost
	{
		/// <summary>
		/// Invoked to resizes the specified item based on the size of its contents.
		/// </summary>
		/// <param name="resizableItem">The item that contains the ResizeContext</param>
		/// <param name="resizeInXAxis">True to resize the width and false to resize the height.</param>
		/// <returns>Returns true if the auto size was performed; otherwise false is returned.</returns>
		bool PerformAutoSize(FrameworkElement resizableItem, bool resizeInXAxis);
	} 
	#endregion //IAutoResizeHost

    #region ResizeConstraints class

    /// <summary>
    /// An object that contains the constraints used during a resize operation.
    /// </summary>
    internal class ResizeConstraints
    {
        #region Private Members

        private FrameworkElement _resizerBar;
        private double _minExtent = 4.0d;
        private double _maxExtent; // = 0.0d;
        private bool _resizeWhileDragging;
        private bool _cancel;
        private FrameworkElement _resizeArea;

        // AS 12/15/08 NA 2009 Vol 1 - Fixed Fields
        private bool _resizeInXAxis;

        #endregion Private Members

        #region Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="ResizeConstraints"/> class
        /// </summary>
		/// <param name="resizerBar">The resizer bar</param>
        /// <param name="resizeArea">The resize area</param>
        /// <param name="resizeInXAxis">True if the element is being resized along the x-axis; otherwise false if the element is being resized along the y-axis.</param>
        // AS 12/15/08 NA 2009 Vol 1 - Fixed Fields
        // In order to provide the min/max extent we need to know which orientation is being resized.
        //
        //public ResizeConstraints(FrameworkElement child, FrameworkElement resizeArea)
        public ResizeConstraints(FrameworkElement resizerBar, FrameworkElement resizeArea, bool resizeInXAxis)
        {
            this._resizerBar = resizerBar;
            this._resizeArea = resizeArea;

            // AS 12/15/08 NA 2009 Vol 1 - Fixed Fields
            this._resizeInXAxis = resizeInXAxis;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// If set to true will _cancel the operation
        /// </summary>
        public bool Cancel
        {
            get { return this._cancel; }
            set { this._cancel = value; }
        }

        /// <summary>
        /// The minimum extent allowed in device-independent units (1/96th inch per unit)
        /// </summary>
        public double MinExtent
        {
            get { return this._minExtent; }
            set { this._minExtent = value; }
        }

        /// <summary>
        /// The maximum extent allowed in device-independent units (1/96th inch per unit)
        /// </summary>
        public double MaxExtent
        {
            get { return this._maxExtent; }
            set { this._maxExtent = value; }
        }

        /// <summary>
        /// If set to true will cause resizing while the mouse is being dragged
        /// </summary>
        public bool ResizeWhileDragging
        {
            get { return this._resizeWhileDragging; }
            set { this._resizeWhileDragging = value; }
        }

        /// <summary>
        /// The resizer bar indicator (read-only)
        /// </summary>
        public FrameworkElement ResizerBar { get { return this._resizerBar; } }

        /// <summary>
        /// The resize area (read-only)
        /// </summary>
        public FrameworkElement ResizeArea { get { return this._resizeArea; } }

        // AS 12/15/08 NA 2009 Vol 1 - Fixed Fields
        /// <summary>
        /// Indicates whether the element is being resized along the x or y axis (read-only)
        /// </summary>
        public bool ResizeInXAxis { get { return this._resizeInXAxis; } }

        #endregion Properties

    }

    #endregion ResizeConstraints class

    #region ResizeController class

    /// <summary>
    /// Controller object used for element resizing operations
    /// </summary>
    internal class ResizeController
    {
        #region Private Members

        private IResizeHost _host;
        // JJD 1/19/10 - TilesControl support
        // Added support for izing in both dimensions
        //private bool _resizeInXAxis;
        private bool? _resizeInXAxis;
        private bool _mouseEventsHooked;
        // JJD 1/19/10 - TilesControl support
        // Added support for izing in both dimensions
        //private ResizeConstraints _constraints;
        private ResizeConstraints _constraintsInXAxis;
        private ResizeConstraints _constraintsInYAxis;
        private FrameworkElement _resizeTarget;
        private Point _originalDragPoint;
        private Point _lastDragPoint;
		
		// JJD 4/28/11 - TFS73523 
		// Changed to Point so we could keep track of bth dimensions when resizing both width and height
		//private double _originalDragPointOffset;
        private Point _originalDragPointOffset;
		// JJD 4/28/11 - TFS73523 - added
        private Point _resizerBarOffset;

        private double _resizerBarWidth = 6.0d;
		private Cursor _cachedCursor;

		// JJD 4/28/11 - TFS73523 - added
		private double _interItemSpacingX;
		private double _interItemSpacingY;

		// JJD 1/6/12 - TFS98924 - added
		private int _suspendCount;






		// AS 7/29/09
		// While implementing the sizing changes in 9.2, I noticed an issue with immediate resizing 
		// while in auto fit mode. This actually comes up also in 9.2 with synchronized resizing.
		//



		private Point _originalTargetOffset;

		private Cursor _lastCursorSet;

		#endregion //Private Members

        #region Constructor

        /// <summary>
		/// Initializes a new instance of the <see cref="ResizeController"/> class
        /// </summary>
        /// <param name="host">The resize host</param>
        /// <seealso cref="IResizeHost"/>
        public ResizeController(IResizeHost host)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            this._host = host;

			FrameworkElement root = this._host.RootElement;


			root.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);



			root.MouseMove += new MouseEventHandler(OnMouseMove);
		}

        #endregion //Constructor

        #region Properties

            #region Public Properties

                #region Host

        /// <summary>
        /// Returns the associated <see cref="IResizeHost"/> (read-only).
        /// </summary>
        public IResizeHost Host { get { return this._host; } }

                #endregion //Host	

				// JJD 4/28/11 - TFS73523 - added
                #region InterItemSpacingX

        /// <summary>
        /// Gets/sets how much empty space is between resizable elements in the X dimension.  
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this space added to the resize area.</para>
		/// </remarks>
        public double InterItemSpacingX
        {
            get
            {
				return this._interItemSpacingX;
            }
            set
            {
				CoreUtilities.ValidateIsNotNan(value);
				CoreUtilities.ValidateIsNotInfinity(value);
				CoreUtilities.ValidateIsNotNegative(value);

				this._interItemSpacingX = value;
            }
        }

                #endregion //InterItemSpacingX	

				// JJD 4/28/11 - TFS73523 - added
                #region InterItemSpacingY

        /// <summary>
        /// Gets/sets how much empty space is between resizable elements in the Y dimension.  
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> this space is added to the resize area.</para>
		/// </remarks>
        public double InterItemSpacingY
        {
            get
            {
				return this._interItemSpacingY;
            }
            set
            {
				CoreUtilities.ValidateIsNotNan(value);
				CoreUtilities.ValidateIsNotInfinity(value);
				CoreUtilities.ValidateIsNotNegative(value);

				this._interItemSpacingY = value;
            }
        }

                #endregion //InterItemSpacingY	
   
                #region IsResizing

        /// <summary>
        /// Returns true during a resize operation (read-only).
        /// </summary>
        public bool IsResizing
        {
            get
            {
                // JJD 1/19/10 - TilesControl support
                // Check for presence of either _constraintsInXAxis or this._constraintsInYAxis
                return (this._resizeTarget != null &&
                         //this._constraints != null);
                         (this._constraintsInXAxis != null || this._constraintsInYAxis != null));
            }
        }

                #endregion //IsResizing	

                #region ResizerBar

        /// <summary>
        /// The resizer bar (read-only)
        /// </summary>
        /// <remarks>May return null is not resizing or if the resize constraints specify ResizeWhileDragging</remarks>
        public FrameworkElement ResizerBar
        {
            get
            {
                // JJD 1/19/10 - TilesControl support
                // Added support for resizing in both dimemsions
                //if (this.IsResizing &&
                //     this._constraints.ResizeWhileDragging == false)
                //    return this._constraints.ResizerBar;
                if (this.IsResizing)
                {
                    if (this._constraintsInXAxis != null && this._constraintsInXAxis.ResizeWhileDragging == false)
                        return this._constraintsInXAxis.ResizerBar;

                    if (this._constraintsInYAxis != null && this._constraintsInYAxis.ResizeWhileDragging == false)
                        return this._constraintsInYAxis.ResizerBar;
                }

                return null;
            }
        }

                #endregion //ResizerBar	
     
                #region ResizerBarWidth

        /// <summary>
        /// Gets/sets the width of the resizer bar
        /// </summary>
        public double ResizerBarWidth
        {
            get
            {
                return this._resizerBarWidth;
            }
            set
            {
                if (value < 1.0d)
					throw new ArgumentOutOfRangeException("value", TileUtilities.GetString("LE_InvalidResizerBarWidth"));

                this._resizerBarWidth = value;
            }
        }

                #endregion //ResizerBarWidth	
    
            #endregion //Public Properties

			#region Private Properties

				// JJD 03/21/12 - TFS100151 - added
				#region ResizerBarWidthResolvedX

		private double ResizerBarWidthResolvedX
		{
			get
			{
				return Math.Max(this._resizerBarWidth, _interItemSpacingX);
			}
		}

				#endregion //ResizerBarWidthResolvedX

				// JJD 03/21/12 - TFS100151 - added
				#region ResizerBarWidthResolvedY

		private double ResizerBarWidthResolvedY
		{
			get
			{
				return Math.Max(this._resizerBarWidth, _interItemSpacingY);
			}
		}

				#endregion //ResizerBarWidthResolvedY

			#endregion //Private Properties	
    
        #endregion //Properties

		#region Methods

			#region Public Methods

                #region CancelResize

        /// <summary>
        /// Cancel any pending resize operations
        /// </summary>
        public void CancelResize()
        {
            // JJD 1/19/10 - TilesControl support
            // Added support for 2-dimensional resizing
            //this.ClearConstraints();
            this.ClearConstraints(true);
            this.ClearConstraints(false);
        }

                #endregion //CancelResize	
    
                #region ArrangeResizerBar

        /// <summary>
        /// Called during arrange pass to position the resizer bar
        /// </summary>
        public void PositionResizerBar()
        {
            // JJD 1/19/10 - TilesControl support
            // Added support for resizing in 2 dimensions
            #region Old code

            //if (this._constraints == null || this._constraints.ResizerBar == null)
            //    return;

            //double left = 0;
            //double top = 0;

            //if (this._resizeInXAxis)
            //{
            //    left = Canvas.GetLeft(this._constraints.ResizerBar);
            //}
            //else
            //{
            //    top = Canvas.GetTop(this._constraints.ResizerBar);
            //}

            //this._constraints.ResizerBar.Arrange(new Rect(left, top, this._constraints.ResizerBar.DesiredSize.Width, this._constraints.ResizerBar.DesiredSize.Height));

            #endregion //Old code
            if (this._constraintsInXAxis != null && this._constraintsInXAxis.ResizerBar != null)
            {
                double left = Canvas.GetLeft(this._constraintsInXAxis.ResizerBar);
                double top = 0;
                this._constraintsInXAxis.ResizerBar.Arrange(new Rect(left, top, this._constraintsInXAxis.ResizerBar.DesiredSize.Width, this._constraintsInXAxis.ResizerBar.DesiredSize.Height));
            }

            if (this._constraintsInYAxis != null && this._constraintsInYAxis.ResizerBar != null)
            {
                double left = 0;
                double top = Canvas.GetTop(this._constraintsInYAxis.ResizerBar);
                this._constraintsInYAxis.ResizerBar.Arrange(new Rect(left, top, this._constraintsInYAxis.ResizerBar.DesiredSize.Width, this._constraintsInYAxis.ResizerBar.DesiredSize.Height));
            }
        }

                #endregion //ArrangeResizerBar	

				// JJD 1/6/12 - TFS98924 - added
				#region ResumeMouseMoveProcessing

		/// <summary>
		/// Resumes processing the logic in response to mouse moves
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> calls to <b>SuspendMouseMoveProcessing</b> must be paired with corresponding calls to <b>ResumeMouseMoveProcessing</b>.</para>
		/// </remarks>
		/// <returns>The number of times the <b>SuspendMouseMoveProcessing</b>method was called without corresponding calls to this method.</returns>
		/// <seealso cref="ResumeMouseMoveProcessing"/>
		public int ResumeMouseMoveProcessing()
		{
			if (_suspendCount > 0)
				_suspendCount--;

			return _suspendCount;
		}

				#endregion //ResumeMouseMoveProcessing	
    
				// JJD 1/6/12 - TFS98924 - added
				#region SuspendMouseMoveProcessing

		/// <summary>
		/// Suspends processing the logic in response to mouse moves
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> calls to <b>SuspendMouseMoveProcessing</b> must be paired with corresponding calls to <b>ResumeMouseMoveProcessing</b>.</para>
		/// </remarks>
		/// <returns>The number of times this method was called without corresponding calls to <see cref="ResumeMouseMoveProcessing"/>.</returns>
		/// <seealso cref="ResumeMouseMoveProcessing"/>
		public int SuspendMouseMoveProcessing()
		{
			_suspendCount++;
			return _suspendCount;
		}

				#endregion //SuspendMouseMoveProcessing	
    
			#endregion Public Methods

			#region Protected Methods

			#endregion Protected Methods

			#region Internal Methods

				#region BeginResize

        // JJD 1/19/10 - TilesControl support
        // Changed resizeInXAxis to nullable bool
        //internal void BeginResize(MouseButtonEventArgs e, FrameworkElement itemToResize, bool resizeInXAxis )
		internal void BeginResize(MouseButtonEventArgs e, FrameworkElement itemToResize, bool? resizeInXAxis )
		{
			// JJD 03/08/12 TFS100150 - Added touch support
			// Ask the host if it is in a state nwhere we can capture the mouse.
			// If not then return
			if (!this.Host.CanCaptureMouse(e, itemToResize, resizeInXAxis))
				return;

			//// AS 6/16/09 NA 2009.2 Field Sizing
			//if (e.ClickCount == 2)
			//{
			//    IAutoResizeHost autoSizeHost = _host as IAutoResizeHost;

			//    if (null != autoSizeHost)
			//    {
			//        // JJD 1/19/10 - TilesControl support
			//        // See if we are over only one of the edges
			//        if (resizeInXAxis.HasValue)
			//        {
			//            // JJD 1/19/10 - TilesControl support
			//            // Pass the resizeInXAxis.Value in
			//            //if (autoSizeHost.PerformAutoSize(itemToResize, resizeInXAxis))
			//            if (autoSizeHost.PerformAutoSize(itemToResize, resizeInXAxis.Value))
			//            {
			//                e.Handled = true;
			//                return;
			//            }
			//        }
			//        else
			//        {
			//            // JJD 1/19/10 - TilesControl support
			//            // Since we are over the corner call PerformAutoSize for both dimensions
			//            bool autoSizePerformed = autoSizeHost.PerformAutoSize(itemToResize, true);

			//            if (autoSizeHost.PerformAutoSize(itemToResize, false))
			//                autoSizePerformed = true;

			//            if ( autoSizePerformed )
			//            {
			//                e.Handled = true;
			//                return;
			//            }
			//        }
			//    }
			//}

            FrameworkElement resizeArea = this.Host.GetResizeAreaForItem(itemToResize);

            Debug.Assert(resizeArea != null);

            if (resizeArea == null)
                return;

            this._resizeTarget = itemToResize;
			this._resizeInXAxis	= resizeInXAxis;

			this._lastDragPoint = this._originalDragPoint = e.GetPosition(resizeArea);

            // JJD 1/19/10 - TilesControl support
            // Re-factored into InitializeConstraints helper method
            // to support 2 dimensional resizing
            if (resizeInXAxis.HasValue)
            {
                if (this.InitializeConstraints(e, resizeArea, resizeInXAxis.Value) == null)
                    return;
            }
            else
            {
                this.InitializeConstraints(e, resizeArea, true);
            
                // we need to reinitialize the _resizeTarget member since
                // it might have been cleared if the previous InitializeConstraints was
                // canceled
                this._resizeTarget = itemToResize;

                this.InitializeConstraints(e, resizeArea, false);

                if (this._constraintsInXAxis == null &&
                    this._constraintsInYAxis == null)
                    return;
            }

			// JJD 3/06/07 - BR19937
			// cache the root element's cursor before setting it to the resize guy
			this._cachedCursor = this.Host.RootElement.Cursor;

			// JJD 3/06/07 - BR19937
			// set the root element's cursor to the resize guy
			this.Host.RootElement.Cursor = this.GetResizeCursor(itemToResize, resizeInXAxis);
            
			this.HookMouseEvents();

			e.Handled = true;
		}

        // JJD 1/19/10 - TilesControl support
        // Re-factored into InitializeConstraints helper method
        // to support 2 dimensional resizing
        private ResizeConstraints InitializeConstraints(MouseButtonEventArgs e, FrameworkElement resizeArea, bool resizeInXAxis)
        {
            ResizeConstraints constraints = null;

            FrameworkElement resizerBar = this.CreateResizerBar(resizeInXAxis, resizeArea);

			GeneralTransform tranform = resizeArea.TransformToVisual(_resizeTarget);

            //Point ptRelativeToTarget = resizeArea.TranslatePoint(this._originalDragPoint, this._resizeTarget);
            Point ptRelativeToTarget = tranform.Transform(this._originalDragPoint);

            // AS 7/29/09
            // We need to store the relative position of the resize target with respect to the resize area.
            // This is needed because the element's left may change during the drag and therefore the 
            // relative point of the mouse within the target may not reflect the actual delta in size so far.
            //
            //_originalTargetOffset = resizeArea.TranslatePoint(new Point(), _resizeTarget);
            //_originalTargetOffset = resizeArea.TranslatePoint(new Point(), _resizeTarget);
            _originalTargetOffset = tranform.Transform(new Point());

			// JJD 03/21/12 - TFS100151
			// Use the appropriate width for the resizer bar based on the dimension being resized
			double resizerBarWidth = resizeInXAxis ? this.ResizerBarWidthResolvedX : this.ResizerBarWidthResolvedY;

			// JJD 03/21/12 - TFS100151
			// Check the passed in resizeInXAxis instead
            //if (this._resizeInXAxis == true)
			if (resizeInXAxis)
            {
                double targetWidth = this._resizeTarget.ActualWidth;

				// JJD 4/28/11 - TFS73523 
				// Changed to Point so we could keep track of bth dimensions when resizing both width and height
				// JJD 03/21/12 - TFS100151
				// Use the appropriate width for the resizer bar based on the dimension being resized
				//this._originalDragPointOffset = targetWidth - (ptRelativeToTarget.X + (this.ResizerBarWidth / 2));
				this._originalDragPointOffset.X = targetWidth - (ptRelativeToTarget.X + (resizerBarWidth / 2));
				
				// JJD 4/28/11 - TFS73523 - added
				// JJD 03/21/12 - TFS100151
				// Keep the resizer bar aligned to the right edge of the element being resized
				// regardless of where the mouse is located. This allows for a negative _resizerBarOffset.
				//this._resizerBarOffset.X = Math.Max(targetWidth - ptRelativeToTarget.X, 0) - (resizerBarWidth / 2);
				this._resizerBarOffset.X = targetWidth - ptRelativeToTarget.X;
			}
            else
            {
                double targetHeight = this._resizeTarget.ActualHeight;

				// JJD 4/28/11 - TFS73523 
				// Changed to Point so we could keep track of bth dimensions when resizing both width and height
				// JJD 03/21/12 - TFS100151
				// Use the appropriate width for the resizer bar based on the dimension being resized
                //this._originalDragPointOffset = targetHeight - (ptRelativeToTarget.Y + (this.ResizerBarWidth / 2));
				this._originalDragPointOffset.Y = targetHeight - (ptRelativeToTarget.Y + (resizerBarWidth / 2));
				
				// JJD 4/28/11 - TFS73523 - added
				// JJD 03/21/12 - TFS100151
				// Keep the resizer bar aligned to the bottom edge of the element being resized
				// regardless of where the mouse is located. This allows for a negative _resizerBarOffset.
				//this._resizerBarOffset.Y = Math.Max(targetHeight - ptRelativeToTarget.Y, 0) - (resizerBarWidth / 2);
				this._resizerBarOffset.Y = targetHeight - ptRelativeToTarget.Y;
            }







			// AS 12/15/08 NA 2009 Vol 1 - Fixed Fields
            // In order to provide the min/max extents the caller needs to know how the 
            // element is about to be resized.
            //
            // JJD 1/19/10 - TilesControl support
            // Added support for 2-dimensional resizing
            //this._constraints = new ResizeConstraints(child, resizeArea);
            constraints = new ResizeConstraints(resizerBar, resizeArea, resizeInXAxis);

            if (resizeInXAxis)
                this._constraintsInXAxis = constraints;
            else
                this._constraintsInYAxis = constraints;

            // JJD 1/19/10 - TilesControl support
            // Added resizeInXAxis param to add support for 2-dimensional resizing
            //this.PositionResizerBar(e.MouseDevice);
            this.PositionResizerBar(e, resizeInXAxis);

            this.Host.InitializeResizeConstraints(resizeArea, this._resizeTarget, constraints);

            if (constraints.Cancel == true)
            {







				this.ClearConstraints(resizeInXAxis);
                
                return null;
            }







			if (constraints.ResizeWhileDragging == true)
            {
				// JJD 4/28/11 - TFS73523 
				// Changed to Point so we could keep track of bth dimensions when resizing both width and height
				//this._originalDragPointOffset = 0.0;
				this._originalDragPointOffset = new Point();
				// JJD 4/28/11 - TFS73523 - added
				this._resizerBarOffset = new Point(); ;

				constraints.ResizerBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                constraints.ResizerBar.Visibility = Visibility.Visible;
                constraints.ResizerBar.Measure(new Size(resizeArea.ActualWidth, resizeArea.ActualHeight));
            }

            return constraints;
        }

				#endregion BeginResize

			#endregion Internal Methods

			#region Private Methods

				#region ClearConstraints






        // JJD 1/19/10 - TilesControl support
        // Added resizeInXAxis param to add support for 2-dimensional resizing
        //private void ClearConstraints()
		private void ClearConstraints(bool resizeInXAxis)
		{
            ResizeConstraints constraints;

            if (resizeInXAxis)
                constraints = this._constraintsInXAxis;
            else
                constraints = this._constraintsInYAxis;

			if (constraints != null)
			{
                this.Host.RemoveResizerBar(constraints.ResizerBar);

                // JJD 1/19/10 - TilesControl support
                // Added support for 2-dimensional resizing
                //  this._constraints = null;
                if (resizeInXAxis)
				    this._constraintsInXAxis = null;
                else
				    this._constraintsInYAxis = null;


				// JJD 3/06/07 - BR19937
				// reset the cursor
				this.Host.RootElement.Cursor = this._cachedCursor;
			}

			this._resizeTarget = null;

            this.UnhookMouseEvents();
		}

				#endregion ClearConstraints

				#region CreateResizerBar



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        private FrameworkElement CreateResizerBar(bool vertical, FrameworkElement resizeArea)
		{
			// JJD 03/21/12 - TFS100151
			// Use the appropriate width for the resizer bar based on the dimension being resized
			//double resizerBarWidth = this.ResizerBarWidth;
			double resizerBarWidth = vertical ? this.ResizerBarWidthResolvedX : this.ResizerBarWidthResolvedY;

			Rectangle resizerBar = new Rectangle();

			// hide the bar to start with. It is the callers responsibility to
			// show the bar after it has been properly positioned
			resizerBar.Visibility = Visibility.Collapsed;

			resizerBar.Opacity = .18d;
			resizerBar.Fill = new SolidColorBrush(Colors.Black);

			if ( vertical == true )
			{
                resizerBar.Height = resizeArea.ActualHeight;
				resizerBar.Width = resizerBarWidth;
				Canvas.SetTop( resizerBar, 0.0);
			}
			else
			{
				resizerBar.Height = resizerBarWidth;
                resizerBar.Width = resizeArea.ActualWidth;
				Canvas.SetLeft(resizerBar, 0.0);
			}

            this.Host.AddResizerBar(resizerBar);

			return resizerBar;
		}

				#endregion CreateResizerBar

				#region GetAncestorResizableItem
			
		private FrameworkElement GetAncestorResizableItem(FrameworkElement descendant)
		{
			FrameworkElement root = this._host.RootElement;

			// walk up the parent chain looking for a ResizableItem
			//FrameworkElement fe = (FrameworkElement)descendant;
			DependencyObject child = (DependencyObject)descendant;

			while (child != null)
			{
				// stop when we hit the root element
				if (child == root)
					return null;

				if (child is IResizableElement)
					return child as FrameworkElement;

				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
				//child = VisualTreeHelper.GetParent(child) as FrameworkElement;
				// SSP/JJD 9/19/07
				// We don't want to traverse logical hierarchy. When XamComboEditor's drop-down
				// is clicked, we were executing resize logic because from Popup we were getting
				// cell presenter as the resizable element which is wrong. We should only be
				// traversing the visual hierarchy.
				// 
				//child = Utilities.GetParent(child);
				child = VisualTreeHelper.GetParent( child );
			}

			return null;
		}

				#endregion GetAncestorResizableItem

				#region GetConstrainedPosition

		// JJD 7/12/07 - BR23153
		// Added adjustWithOffset param
		//internal double GetConstrainedPosition(MouseDevice device)
        // JJD 1/19/10 - TilesControl support
        // Added resizeInXAxis param to add support for 2-dimensional resizing
		//internal double GetConstrainedPosition(MouseDevice device, bool adjustWithOffset)
		internal double GetConstrainedPosition(MouseEventArgs mouseArgs, bool adjustWithOffset, bool resizeInXAxis)
		{
			// AS 7/29/09
			Point pt;
			return GetConstrainedPosition(mouseArgs, adjustWithOffset, resizeInXAxis, out pt);
		}

		// AS 7/29/09
		// Added an overload so we can pass back the "current position". The intent here is that when in 
		// immediate resize mode the caller is storing the current position and using that as a reference 
		// point. However if the mouse was moved quickly (or consecutively back and forth thereby building 
		// a greater variance) the caller may consider a move to the opposite side to be one that requires a
		// delta change. Since this routine knows when the position is below/above the min/max, it can adjust 
		// the currentResizeAreaPoint accordingly to reflect the actual position at which the min/max would 
		// have been acheived.
		//
        // JJD 1/19/10 - TilesControl support
        // Added resizeInXAxis param to add support for 2-dimensional resizing
        //internal double GetConstrainedPosition(MouseDevice device, bool adjustWithOffset, out Point currentResizeAreaPoint)
		internal double GetConstrainedPosition(MouseEventArgs mouseArgs, bool adjustWithOffset, bool resizeInXAxis, out Point currentResizeAreaPoint)
		{
			currentResizeAreaPoint = new Point();

            // JJD 1/19/10 - TilesControl support
            // Added resizeInXAxis param to add support for 2-dimensional resizing
            ResizeConstraints constraints;

            if (resizeInXAxis)
                constraints = this._constraintsInXAxis;
            else
                constraints = this._constraintsInYAxis;

			if (constraints == null || 
				constraints.Cancel == true )
				return 0.0;

			Point pt = mouseArgs.GetPosition(constraints.ResizeArea);

			GeneralTransform transform = constraints.ResizeArea.TransformToVisual(_resizeTarget);

			//Point ptInTargetCoordinates = constraints.ResizeArea.TranslatePoint(pt, this._resizeTarget);
			Point ptInTargetCoordinates = transform.Transform(pt);

			double newPosition, minPosition, maxPosition, currentValue;

			// AS 7/29/09
			// See BeginResize for details. Essentially the left/top of the element may have changed 
			// since the start of the operation so we cannot just use the current relative point. We 
			// need to adjust that based on the current offset.
			//
			//Point targetOffset = constraints.ResizeArea.TranslatePoint(new Point(), _resizeTarget);
			Point targetOffset = transform.Transform(new Point());

			if (resizeInXAxis)
			{
				// AS 7/29/09
				ptInTargetCoordinates.X += _originalTargetOffset.X - targetOffset.X;

				currentValue = pt.X - ptInTargetCoordinates.X;
				// JJD 7/12/07 - BR23153
				//newPosition = pt.X + this._originalDragPointOffset;
				newPosition = pt.X;
			}
			else
			{
				// AS 7/29/09
				ptInTargetCoordinates.Y += _originalTargetOffset.Y - targetOffset.Y;

				currentValue = pt.Y - ptInTargetCoordinates.Y;
				// JJD 7/12/07 - BR23153
				//newPosition = pt.Y + this._originalDragPointOffset;
				newPosition = pt.Y;
			}







			// JJD 7/12/07 - BR23153
			// Added adjustWithOffset param
			if (adjustWithOffset)
			{
				// JJD 4/28/11 - TFS73523 
				// Changed to Point so we could keep track of bth dimensions when resizing both width and height
				//newPosition += this._originalDragPointOffset;
				if (resizeInXAxis)
					newPosition += this._resizerBarOffset.X;
				else
					newPosition += this._resizerBarOffset.Y;
			}

			minPosition = currentValue + constraints.MinExtent;

            if (constraints.MaxExtent > constraints.MinExtent)
                maxPosition = currentValue + constraints.MaxExtent;
            else
            {
                // JJD 1/22/10
                // When the max extent is less than or equal to the min extent
                // we should set the maxposition to the minposition instead
                // of setting the max position to 0 (infinity).
                // This will prevent the item from being resized in this
                // dimension at all.
                //maxPosition = 0.0d;
				if (constraints.MaxExtent > 0)
					maxPosition = minPosition;
				else
					maxPosition = double.PositiveInfinity;
            }

			// AS 7/29/09
			double calculatedNewPosition = newPosition;

			// make sure the new positionis within the min/max range
			if (newPosition < minPosition)
				newPosition = minPosition;
			else if ( newPosition > maxPosition && maxPosition != 0.0d )
				newPosition = maxPosition;

			// AS 7/29/09
			// The logic above would adjust the position when below/above the min/max. Similarly
			// we need to adjust the currentResizeAreaPoint with the same adjustment so that the 
			// point returned/stored is based on the position on which the new delta would have 
			// been acheived.
			//
			double positionAdjustment = calculatedNewPosition - newPosition;
			currentResizeAreaPoint = pt;

			if (resizeInXAxis)
			{
				currentResizeAreaPoint.X -= positionAdjustment;
			}
			else
			{
				currentResizeAreaPoint.Y -= positionAdjustment;
			}








			return newPosition;
		}

				#endregion GetConstrainedPosition

				#region GetElementToResize

        // JJD 1/19/10 - TilesControl support
        // Changed out param to bool? to support sizing in both dimensions
        //private FrameworkElement GetElementToResize(FrameworkElement element, MouseDevice mouseDevice, out bool processWidth)
		private FrameworkElement GetElementToResize(FrameworkElement element, MouseEventArgs mouseArgs, out bool? processWidth)
		{
			// First check if we can resize the width
			processWidth = true;

			FrameworkElement resizableElement = this.GetAncestorResizableItem(element);

			if (resizableElement == null)
			{
				FrameworkElement root = _host.RootElement;

				// JJD 4/28/11 - TFS73523 
				// Since we aren't over a resizble element
				// try to get a neighboring element based on the item item spacing in the X dimension
				resizableElement = this.GetNeighborUsingInterItemSpacing(root, mouseArgs, true);

				// JJD 4/28/11 - TFS73523 
				// If we still don't hasve an element then 
				// try to get a neighboring element based on the item item spacing in the Y dimension
				if (resizableElement == null)
					resizableElement = this.GetNeighborUsingInterItemSpacing(root, mouseArgs, false);

				if (resizableElement == null)
					return null;
			}

			IResizeHost owner = this.Host;
			if (owner == null)
				return null;

            // JJD 1/19/10 - TilesControl support
            // Call CanResizeInBothDimensions if host impelments new IResizeHostMulti
			IResizeHostMulti resizeHostMulti = owner as IResizeHostMulti;

            bool canResizeInBothDimnesions = false;
			if (resizeHostMulti != null)
                canResizeInBothDimnesions = resizeHostMulti.CanResizeInBothDimensions(resizableElement);

			// AS 5/11/09 TFS17542
			// When an element captures the mouse is may allow the child elements to receive
			// mouse messages so we need to allow the child elements to be resized if they
			// exist within the element with capture so we need to find out what element we 
			// would have used so I reworked this logic and performed it after the call to 
			// GetElementToResizeHelper.
			//
			//// SSP 9/20/07
			//// If some other element has capture then we don't want to show the resize cursor.
			//// 
			//if ( !this.IsResizing && null != mouseDevice )
			//{
			//    IInputElement elementWithCapture = mouseDevice.Captured;
			//    if ( null != elementWithCapture && elementWithCapture != owner.RootElement )
			//        return null;
			//}

			FrameworkElement resizeWidthTarget = this.GetElementToResizeHelper(resizableElement, mouseArgs, processWidth.Value);

            // Only return the resizeWidthTarget if sizing is both dimensions is disallowed
			// JJD 07/12/12 - TFS112221
			// Added MouseEventArgs parameter
			//if (resizeWidthTarget != null && !IsValidResizeElement(resizeWidthTarget))
             if (resizeWidthTarget != null && !IsValidResizeElement(resizeWidthTarget, mouseArgs))
                resizeWidthTarget = null;

			if (resizeWidthTarget != null &&
                canResizeInBothDimnesions == false &&
                owner.CanResize(resizeWidthTarget, processWidth.Value))
				return resizeWidthTarget;

			// then check if we can resize the height
			processWidth = false;

			FrameworkElement resizeHeightTarget = this.GetElementToResizeHelper(resizableElement, mouseArgs, processWidth.Value);

            // Only return the resizeHeightTarget if sizing is both dimensions is disallowed
			// JJD 07/12/12 - TFS112221
			// Added MouseEventArgs parameter
			//if (resizeHeightTarget != null && !IsValidResizeElement(resizeHeightTarget))
            if (resizeHeightTarget != null && !IsValidResizeElement(resizeHeightTarget, mouseArgs))
                resizeHeightTarget = null;

			if (resizeHeightTarget != null &&
                canResizeInBothDimnesions == false &&
                owner.CanResize(resizeHeightTarget, processWidth.Value))
				return resizeHeightTarget;

            if (canResizeInBothDimnesions)
            {
                // At this point if we have a resizeHeightTarget and a resizeWidthTarget then we 
                // are sizing in both dimensions so null out the processWidth boolean 'out param' and
                // return the resize element 
                if (resizeHeightTarget != null &&
                     resizeWidthTarget != null &&
                    resizeHeightTarget == resizeWidthTarget)
                {
                    processWidth = null;

                    return resizeHeightTarget;
                }

                // Since we only have one the set the processWidth boolean 'out param' appropriately and return 
                // the resize element
                if (resizeWidthTarget != null)
                {
                    processWidth = true;
                    return resizeWidthTarget;
                }

                if (resizeHeightTarget != null)
                {
                    processWidth = false;
                    return resizeHeightTarget;
                }
            }

			return null;
		}

		// JJD 1/6/12 - TFS98924 
		// Added callDepth parameter to detect and prevent stack overflows
		//private FrameworkElement GetElementToResizeHelper(FrameworkElement resizableElement, MouseEventArgs mouseArgs, bool processWidth)
		private FrameworkElement GetElementToResizeHelper(FrameworkElement resizableElement, MouseEventArgs mouseArgs, bool processWidth, int callDepth = 0)
		{
			// JJD 1/6/12 - TFS98924 
			// Detect a possible stack overflow and bail
			if (callDepth > 30)
				return null;

			FrameworkElement resizeArea = this._host.GetResizeAreaForItem(resizableElement);

            if (resizeArea == null)
                return null;

			FrameworkElement previousSibling;
			Point pt = mouseArgs.GetPosition(resizableElement);

			if (processWidth == true)
			{
				// see if the mouse is over the left or right edges of the element
				if (pt.X < 3)
				{
					// Since the mouse is within 3 pixels of the left edge of the
					// element look for a sibling to the right of it
					Point ptRelativeToThisElement = mouseArgs.GetPosition(resizableElement);

					ptRelativeToThisElement.X -= 2 + pt.X;

					GeneralTransform transform = resizableElement.TransformToVisual(resizeArea);

					//Point ptReltaiveToContainer = resizableElement.TranslatePoint(ptRelativeToThisElement, resizeArea);
					Point ptReltaiveToContainer = transform.Transform(ptRelativeToThisElement);

					//previousSibling = resizeArea.InputHitTest(ptReltaiveToContainer) as FrameworkElement;
					previousSibling = PresentationUtilities.GetVisualDescendantFromPoint<UIElement>(resizeArea, ptReltaiveToContainer, null) as FrameworkElement;

					if (previousSibling != null)
					{
						// AS 1/22/10
						// We need to make sure we are within a few pixels of the right edge 
						// of the adjacent item so call back into the GetElementToResizeHelper
						// with the sibling we find (if we find one).
						//
						//return GetAncestorResizableItem(previousSibling);
						FrameworkElement ancestor = GetAncestorResizableItem(previousSibling);

						if (null != ancestor && ancestor != resizableElement)
						{
							// JJD 1/6/12 - TFS98924 
							// Bump callDepth parameter to detect and prevent stack overflows
							//return GetElementToResizeHelper(ancestor, mouseArgs, processWidth);
							return GetElementToResizeHelper(ancestor, mouseArgs, processWidth, callDepth + 1);
						}
					}
				}
				else
				{
					// If the the mouse is within 3 pixels of the right left edge
					// return the element
					double width = resizableElement.ActualWidth;

					// JM NA 10.1 CardView - fix this test so it correctly checks to see if we are within a 3 pixel buffer area on the inside of the right edge. 
					//if (pt.X + 3 >= width)
					if (pt.X >= width - 3)
						return resizableElement;
				}
			}
			else
			{
				// see if the mouse is over the top or bottom edges of the element
				if (pt.Y < 3)
				{
					// Since the mouse is within 3 pixels of the top edge of the
					// element look for a sibling to above it
					Point ptRelativeToThisElement = mouseArgs.GetPosition(resizableElement);

					ptRelativeToThisElement.Y -= 2 + pt.Y;

					GeneralTransform transform = resizableElement.TransformToVisual(resizeArea);

					//Point ptReltaiveToContainer = resizableElement.TranslatePoint(ptRelativeToThisElement, resizeArea);
					Point ptReltaiveToContainer = transform.Transform(ptRelativeToThisElement);

					//previousSibling = resizeArea.InputHitTest(ptReltaiveToContainer) as FrameworkElement;
					previousSibling = PresentationUtilities.GetVisualDescendantFromPoint<UIElement>(resizeArea, ptReltaiveToContainer, null) as FrameworkElement;
					
					if (previousSibling != null)
					{
						// AS 1/22/10
						// We need to make sure we are within a few pixels of the right edge 
						// of the adjacent item so call back into the GetElementToResizeHelper
						// with the sibling we find (if we find one).
						//
						//return GetAncestorResizableItem(previousSibling);
						FrameworkElement ancestor = GetAncestorResizableItem(previousSibling);

						if (null != ancestor && ancestor != resizableElement)
						{
							// JJD 1/6/12 - TFS98924 
							// Bump callDepth parameter to detect and prevent stack overflows
							//return GetElementToResizeHelper(ancestor, mouseArgs, processWidth);
							return GetElementToResizeHelper(ancestor, mouseArgs, processWidth, callDepth + 1);
						}
					}
				}
				else
				{
					// If the the mouse is within 3 pixels of the right left edge
					// return the element
					double height = resizableElement.ActualHeight;

					// JM NA 10.1 CardView - fix this test so it correctly checks to see if we are within a 3 pixel buffer area on the inside of the bottom edge. 
					//if (pt.Y + 3 >= height)
					if (pt.Y >= height - 3)
						return resizableElement;
				}
			}

			return null;
		}

				#endregion GetElementToResize

				// JJD 4/28/11 - TFS73523 - added
				#region GetNeighborUsingInterItemSpacing

		private FrameworkElement GetNeighborUsingInterItemSpacing(FrameworkElement root, MouseEventArgs mouseArgs, bool processWidth)
		{
			double adjustmentX;
			double adjustmentY;

			adjustmentX = Math.Ceiling(_interItemSpacingX);
			adjustmentY = Math.Ceiling(_interItemSpacingY);

			if (adjustmentX < .1 && adjustmentY < .1)
				return null;


			Point point = mouseArgs.GetPosition(root);




			// if processWidth is true adjust the point to the left and look for a resizable element and return it
			if (processWidth)
				return this.GetNeighborHelper(root, mouseArgs, point, -adjustmentX, 0);

			// otherwise, the point up and look for a resizable element 
			FrameworkElement neighbor = this.GetNeighborHelper(root, mouseArgs, point, 0, -adjustmentY);

			// if found return it
			if (neighbor != null)
				return neighbor;

			IResizeHostMulti multi = this._host as IResizeHostMulti;

			// if the host supports resizing in both dimensions then look for an element by
			// adjusting the point left and up
			if ( multi != null)
				return this.GetNeighborHelper(root, mouseArgs, point, -adjustmentX, -adjustmentY);

			return null;
		}

				#endregion //GetNeighborUsingInterItemSpacing	
    		
				#region GetNeighborHelper

		// JJD 12/9/11 - TFS85554 - added
		private const double MAX_NEIGHBOR_TEST_ADJUSTMENT = 4;

		private FrameworkElement GetNeighborHelper(FrameworkElement root, MouseEventArgs mouseArgs, Point point, double adjustmentX, double adjustmentY)
		{
			// JJD 12/9/11 - TFS85554
			// In the case of large negative adjustment values we want to hittest
			// in smaller increments until we get a hit to avoid jumping over
			// smaller resizable elements in between.
			// Therefore this method was refactored into the while loop below and a new helper
			// method called GetNeighborResizableElementHelper was added and called from
			// within the loop.
			//point.X += adjustmentX;
			//point.Y += adjustmentY;

			//FrameworkElement descendant = PresentationUtilities.GetVisualDescendantFromPoint<UIElement>(root as UIElement, point, null) as FrameworkElement;
			
			//if (descendant != null)
			//    descendant = this.GetAncestorResizableItem(descendant);

			//return descendant;

			// JJD 07/12/12 - TFS112221
			// Cache the absolute value of the adjustemts so we can keep track of the
			// cumulative adjustments i the while loop below (so we don't exceed then
			double absAdjustmentX = Math.Abs(adjustmentX);
			double absAdjustmentY = Math.Abs(adjustmentY);
			double cumulativeAdjustentX = 0;
			double cumulativeAdjustentY = 0;

			while (adjustmentX != 0 || adjustmentY != 0)
			{
				// JJD 12/9/11 - TFS85554 
				// restrict the size of the adjustment we use in each incremental hit test
				double adjustmentToUseX = Math.Max(adjustmentX, -MAX_NEIGHBOR_TEST_ADJUSTMENT);
				double adjustmentToUseY = Math.Max(adjustmentY, -MAX_NEIGHBOR_TEST_ADJUSTMENT);

				// JJD 12/9/11 - TFS85554 
				// adjust the point based on the resticted adjustmrnt calculated above
				point.X += adjustmentToUseX;
				point.Y += adjustmentToUseY;

				// JJD 12/9/11 - TFS85554 
				// Call new GetNeighborResizableElementHelper helper. It is finds an element 
				// then return it
				FrameworkElement descendant = this.GetNeighborResizableElementHelper(root, point);
				if (descendant != null)
					return descendant;

				// JJD 07/12/12 - TFS112221
				// Keep track of the cumulative absolute value of the adjustemts we have made so far
				cumulativeAdjustentX += Math.Abs(adjustmentToUseX);
				cumulativeAdjustentY += Math.Abs(adjustmentToUseY);

				// JJD 07/12/12 - TFS112221
				// If we reached the total adjustment then return null
				if (cumulativeAdjustentX >= absAdjustmentX &&
					 cumulativeAdjustentY >= absAdjustmentY)
					return null;

				// JJD 12/9/11 - TFS85554
				// If we didn't get a hit and the negative adjustmet value is less than the
				// smaller increment we imposed above and the point value can still be adjusted
				// more then modify the adjustment with the max increment and continue for another loop.
				if ((adjustmentX < -MAX_NEIGHBOR_TEST_ADJUSTMENT && point.X >= MAX_NEIGHBOR_TEST_ADJUSTMENT) ||
					 (adjustmentY < -MAX_NEIGHBOR_TEST_ADJUSTMENT && point.Y >= MAX_NEIGHBOR_TEST_ADJUSTMENT))
				{
					adjustmentX -= adjustmentToUseX;
					adjustmentToUseY -= adjustmentToUseY;
					continue;
				}

				// we haven't found anything then break out to return null
				break;
			}

			return null;

		}
				#endregion //GetNeighborHelper	

				// JJD 12/9/11 - TFS85554 - added
				#region GetNeighborResizableElementHelper

		// JJD 12/9/11 - TFS85554
		// Refactored - added hepler method
		private FrameworkElement GetNeighborResizableElementHelper(FrameworkElement root, Point point)
		{
			FrameworkElement descendant = PresentationUtilities.GetVisualDescendantFromPoint<UIElement>(root as UIElement, point, null) as FrameworkElement;

			if (descendant != null)
				descendant = this.GetAncestorResizableItem(descendant);

			return descendant;

		}

				#endregion //GetNeighborResizableElementHelper	    
				
				#region GetResizeCursor

		// Centralized cursor get logic
        // Added support for 2-dimensional resizing
		private Cursor GetResizeCursor(FrameworkElement resizeableItem, bool? resizeInXAxis)
		{
            if (resizeInXAxis == null)
            {
                 // Added support for 2-dimensional resizing
                IResizeHostMulti hostMulti = this.Host as IResizeHostMulti;

                Debug.Assert(hostMulti != null, "We can't be sizing in both dimensions unles the host implements IResizeHostMulti");

                if ( hostMulti != null )
                    return hostMulti.GetMultiResizeCursor(resizeableItem, System.Windows.Input.Cursors.SizeNWSE);
                else
                    return System.Windows.Input.Cursors.SizeNWSE;
            }
            else
            {
                if (resizeInXAxis == true)
                    return this.Host.GetResizeCursor(resizeableItem, resizeInXAxis.Value, System.Windows.Input.Cursors.SizeWE);
                else
                    return this.Host.GetResizeCursor(resizeableItem, resizeInXAxis.Value, System.Windows.Input.Cursors.SizeNS);
            }
		}

				#endregion //GetResizeCursor	

				#region IsValidResizeElement

		// JJD 07/12/12 - TFS112221
		// Added MouseEventArgs parameter
		//private bool IsValidResizeElement(FrameworkElement element)
		private bool IsValidResizeElement(FrameworkElement element, MouseEventArgs mouseArgs)
		{
			FrameworkElement root = this._host.RootElement;

			//return !(element == root || PresentationUtilities.IsAncestorOf(element, root));
			if (element == root || PresentationUtilities.IsAncestorOf(element, root))
				return false;


			// JJD 07/12/12 - TFS112221
			// Finally get the Resize area and make sure the point in within its bounds
			FrameworkElement resizeArea = this._host.GetResizeAreaForItem(element);

			if (resizeArea != null)
			{
				Point pt = mouseArgs.GetPosition(resizeArea);

				return pt.X >= 0 && pt.Y >= 0 &&
						pt.X < resizeArea.ActualWidth &&
						pt.Y < resizeArea.ActualHeight;
			}

			return true;
		}

				#endregion //IsValidResizeElement	
    
                #region HookMouseEvents

        private void HookMouseEvents()
        {
            FrameworkElement root = this.Host.RootElement;

            root.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
            root.LostMouseCapture += new MouseEventHandler(OnLostMouseCapture);
            root.KeyDown += new KeyEventHandler(OnKeyDown);
            
            this._mouseEventsHooked = true;
            
            root.CaptureMouse();
        }

                #endregion //HookMouseEvents	
           
                #region OnLostMouseCapture

        void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            this.CancelResize();
            this.UnhookMouseEvents();
        }

                #endregion //OnLostMouseCapture	
 
                #region OnKeyDown

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (this._resizeTarget != null && e.Key == Key.Escape)
            {
                this.CancelResize();
                e.Handled = true;
            }
        }

                #endregion //OnKeyDown	
		
				#region OnMouseLeftButtonDown

 		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{

            if (!e.Handled)
            {
				FrameworkElement fe = e.OriginalSource as FrameworkElement;

				if (fe != null)
				{
					bool? processWidth;

					FrameworkElement itemToResize = GetElementToResize(fe, e, out processWidth);

					if (itemToResize != null)
					{



						this.BeginResize(e, itemToResize, processWidth);
					}
				}
            }
		}

				#endregion OnMouseLeftButtonDown
    
                #region OnMouseLeftButtonUp

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this._resizeTarget != null)
            {
                this.ResizeToNewMousePosition(e);

                // JJD 1/19/10 - TilesControl support
                //this.ClearConstraints();
                this.CancelResize();

                e.Handled = true;
            }
        }

                #endregion //OnMouseLeftButtonUp	
    
                #region OnMouseMove

        void OnMouseMove(object sender, MouseEventArgs e)
        {
			// JJD 1/6/12 - TFS98924 
			// If mouse move processing is suspended then bail
			if (_suspendCount > 0)
				return;

			if (this._mouseEventsHooked == false)
			{
				this.SetCursor(sender, e);
				return;
			}

            // JJD 1/19/10 - TilesControl support
            // Added support for 2-dimensional resizing
            //if (this._resizeTarget != null && this._constraints != null && !this._constraints.Cancel)
            if (this._resizeTarget != null && 
                ((this._constraintsInXAxis != null && !this._constraintsInXAxis.Cancel) ||
                (this._constraintsInYAxis != null && !this._constraintsInYAxis.Cancel)))
            {
                //if (this._constraints.ResizeWhileDragging == true)
                //    this.ResizeToNewMousePosition(e.MouseDevice);
                //else
                //    this.PositionResizerBar(e.MouseDevice);
                if ( this._constraintsInXAxis != null )
                {
                    if (this._constraintsInXAxis.ResizeWhileDragging == true)
                        this.ResizeToNewMousePosition(e);
                    else
                        this.PositionResizerBar(e, true);
                }

                if ( this._constraintsInYAxis != null )
                {
                    if (this._constraintsInYAxis.ResizeWhileDragging == true)
                        this.ResizeToNewMousePosition(e);
                    else
                        this.PositionResizerBar(e, false);
                }


                e.Handled = true;

            }
        }

                #endregion //OnMouseMove	

				#region SetCursor

 		private void SetCursor(object sender, MouseEventArgs e)
		{
 			FrameworkElement fe = e.OriginalSource as FrameworkElement;

			if (fe != null)
			{
				bool? processWidth;

				FrameworkElement target = GetElementToResize(fe, e, out processWidth);

				if (target != null)
				{
					Cursor cursor = this.GetResizeCursor(target, processWidth);

					if (cursor != null )
					{
						_lastCursorSet = cursor;
						this._host.RootElement.Cursor = cursor;



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


						return;
					}
				}
			}





			if (_lastCursorSet != null)
			{
				_lastCursorSet = null;
				this._host.RootElement.ClearValue(FrameworkElement.CursorProperty);
			}
		}

				#endregion SetCursor
    
				#region PositionResizerBar

        // JJD 1/19/10 - TilesControl support
        // Added resizeInXAxis param to add support for 2-dimensional resizing
        //internal void PositionResizerBar(MouseDevice device)
		internal void PositionResizerBar(MouseEventArgs mouseArgs, bool resizeInXAxis)
		{
            ResizeConstraints constraints;

            // JJD 1/19/10 - TilesControl support
            // Added support for 2-dimensional resizing
            if (resizeInXAxis)
                constraints = this._constraintsInXAxis;
            else
                constraints = this._constraintsInYAxis;

			if (constraints == null || 
				constraints.Cancel == true || 
				constraints.ResizeWhileDragging == true)
				//constraints.ResizerBar.Visibility == Visibility.Collapsed)
				return;

			double newPosition = this.GetConstrainedPosition(mouseArgs, true, resizeInXAxis);

			if (resizeInXAxis)
				Canvas.SetLeft(constraints.ResizerBar, newPosition);
			else
				Canvas.SetTop(constraints.ResizerBar, newPosition);

            FrameworkElement fe = this.Host as FrameworkElement;

            Debug.Assert(fe != null);

            if ( fe != null )
                fe.InvalidateArrange();
		}

				#endregion PositionResizerBar

				#region ResizeToNewMousePosition

        private void ResizeToNewMousePosition(MouseEventArgs mouseArgs)
        {
            // JJD 1/19/10 - TilesControl support
            // Re-factored code into ResizeToNewMousePositionHelper to add support for 2-dimensional resizing
            IResizeHostMulti hostMulti = this.Host as IResizeHostMulti;

			Point currentPoint = new Point();

            bool resizeInXAxis = ( this._constraintsInXAxis != null );
            bool resizeInYAxis = ( this._constraintsInYAxis != null );

            if (hostMulti != null && resizeInXAxis && resizeInYAxis)
            {
                Point pt = new Point();
                double deltaX = this.ResizeToNewMousePositionHelper(mouseArgs, true, false, out currentPoint);

                pt.X = currentPoint.X;

                double deltaY = this.ResizeToNewMousePositionHelper(mouseArgs, false, false, out currentPoint);

                pt.Y = currentPoint.Y;

                if (deltaX != 0.0 && deltaY != 0.0)
                {


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


					this._lastDragPoint = pt;

					// JJD 4/28/11 - TFS73523 
					// Changed to Point so we could keep track of bth dimensions when resizing both width and height
					//this._originalDragPointOffset = 0.0;
                    this._originalDragPointOffset = new Point();
					// JJD 4/28/11 - TFS73523 - added
					this._resizerBarOffset = new Point(); ;

                    hostMulti.ResizeBothDimensions(this._resizeTarget, deltaX, deltaY);
                }
            }
            else
            {
                if (resizeInXAxis)
                {
                    this.ResizeToNewMousePositionHelper(mouseArgs, true, true, out currentPoint);
                }
                if (resizeInYAxis)
                {
                    this.ResizeToNewMousePositionHelper(mouseArgs, false, true, out currentPoint);
                }
            }
        }

         // Re-factored code into ResizeToNewMousePositionHelper to add support for 2-dimensional resizing
        private double ResizeToNewMousePositionHelper(MouseEventArgs mouseArgs, bool resizeInXAxis, bool callResizeMethod, out Point currentPoint)
		{
			double delta = 0.0;

			// We need to let the GetConstrainedPosition method provide the "currentPoint" since the 
			// actual position may be below/above the position required to acheive the min/max extent.
			//
			double constrainedPosition = this.GetConstrainedPosition(mouseArgs, false, resizeInXAxis, out currentPoint);








			// Use the current point so we can compare it to the last drag point to determine if 
			// the delta we calculate should be positive or negative. This is to insure that
			// we don't go outside the constraints (this can happen
			// in immediate resize mode)
			bool shouldDeltaBePositive;

			if (resizeInXAxis)
			{
				delta = constrainedPosition - this._lastDragPoint.X;
				shouldDeltaBePositive = currentPoint.X > this._lastDragPoint.X;
			}
			else
			{
				delta = constrainedPosition - this._lastDragPoint.Y;
				shouldDeltaBePositive = currentPoint.Y > this._lastDragPoint.Y;
			}

			// If the calculated delta doesn't match the direction then exit
			if (delta > 0)
			{
				if (!shouldDeltaBePositive)
					return 0d;
			}
			else
			{
				if (shouldDeltaBePositive)
					return 0d;
			}


			if ( callResizeMethod && delta != 0.0)
			{






				// we don't need to re-get the position since it was cached above
				this._lastDragPoint = currentPoint;

				// Changed to Point so we could keep track of both dimensions when resizing both width and height
				this._originalDragPointOffset = new Point();
				this._resizerBarOffset = new Point(); ;

				this.Host.Resize(this._resizeTarget, resizeInXAxis, delta);
			}

            return delta;
		}

				#endregion ResizeToNewMousePosition
    
                #region UnhookMouseEvents

        private void UnhookMouseEvents()
        {
            if (this._mouseEventsHooked == false)
                return;

            FrameworkElement root = this.Host.RootElement;

			root.MouseLeftButtonUp -= new MouseButtonEventHandler(OnMouseLeftButtonUp);
            root.LostMouseCapture -= new MouseEventHandler(OnLostMouseCapture);
            root.KeyDown -= new KeyEventHandler(OnKeyDown);

            this._mouseEventsHooked = false;

            root.ReleaseMouseCapture();
        }

                #endregion //UnhookMouseEvents	

				#region UnwireMouseDownElement



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


				#endregion //UnwireMouseDownElement	
			
			#endregion Private Methods

		#endregion Methods

    }

    #endregion //ResizeController class
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