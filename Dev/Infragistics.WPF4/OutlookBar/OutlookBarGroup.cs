using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infragistics.Windows.Helpers;
using System.ComponentModel;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.OutlookBar;
using Infragistics.Windows.Internal;
using System.Diagnostics;

namespace Infragistics.Windows.OutlookBar
{
    /// <summary>
    /// Represents a headered content area in a <see cref="XamOutlookBar"/> (commonly referred to as a 'group') that displays image and text in the header and developer-supplied content in the content area.
    /// </summary>
	/// <remarks>
	/// OutlookBarGroups can appear in any of 3 locations in the <see cref="XamOutlookBar"/> control: the navigation area, the overflow area and the overflow context menu.  When displayed in one of these areas,
	/// only the OutlookBarGroup's headser () is shown.  When an OutlookBarGroup is selected, its header (with image and/or text) and content also appear in the selected group area.  
	/// </remarks>

    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,               GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StatePressed,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,             GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateNavigationArea,       GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateOverflowArea,         GroupName = VisualStateUtilities.GroupLocation)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateMinimized,            GroupName = VisualStateUtilities.GroupMinimized)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotMinimized,         GroupName = VisualStateUtilities.GroupMinimized)]

    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,             GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,           GroupName = VisualStateUtilities.GroupSelection)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateLeft,                 GroupName = VisualStateUtilities.GroupSplitterLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRight,                GroupName = VisualStateUtilities.GroupSplitterLocation)]

    [TemplatePart(Name = "PART_Header", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]	// JM 04-30-09 TFS 16159
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class OutlookBarGroup : HeaderedContentControl, INotifyPropertyChanged
    {
        #region Member Variables

        Image _image;   // image element (Small/Large depending on location)
        private double _calculatedHeight;   // desired height in the navigation area
        private double _calculatedWidth;    // desired width in the overflow area
        // last group location       
        private OutlookBarGroupLocation _lastMeasuredLocation = OutlookBarGroupLocation.OverflowContextMenu;

        private bool _isMouseOverGroup = false;     // private member for IsMouseOverGroup property

        private string _key;                        // the Key
        private int _initialOrder = int.MaxValue;   // initial order of appearance in the XamOutlookBar.Groups
        private bool _isVisibleOnStart = true;      // initial status of visibility
        private ImageSource _smallImageResolved = null;

        bool _heightIsFromArrange;                  // indicates that _calculated XXX comes from ArrangeOverride
        bool _widthIsFromArrange;                   // .........
        bool _hasLargeImage = false;

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private PropertyValueTracker _isMinimizedTracker;

        // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		// AS 11/16/11 TFS79544
		private bool _suppressLogicalContent;

		#endregion //Member Variables	
    
        #region Constructors

        static OutlookBarGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OutlookBarGroup),
                new FrameworkPropertyMetadata(typeof(OutlookBarGroup)));


            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(OutlookBarGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

        /// <summary>
        /// Initializes a new OutlookBarGroup.
        /// </summary>
        public OutlookBarGroup()
        {
            _smallImageResolved = SmallImageProperty.GetMetadata(this).DefaultValue as ImageSource;
            _calculatedHeight = _calculatedWidth = 0;
            _isMouseOverGroup = false;      // private member for IsMouseOverGroup property
            _initialOrder = int.MaxValue;   // initial order of appearance in the XamOutlookBar.Groups
            _isVisibleOnStart = true;       // initial status of visibility
        }
        #endregion // Constructors

        #region Base Class Overrides

        #region ArrangeOverride

        /// <summary>
        /// Positions child elements and determines a size for this element.
        /// </summary>
        /// <param name="constraint">The size available to this element for arranging its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size constraint)
        {
            Size sz = base.ArrangeOverride(constraint);

            if (this.Location == OutlookBarGroupLocation.NavigationGroupArea)
            {
                this._calculatedHeight = this.DesiredSize.Height;
                _heightIsFromArrange = true;
            }// end if- save last known height 

            if (this.Location == OutlookBarGroupLocation.OverflowArea)
            {
                this._calculatedWidth = this.DesiredSize.Width;
                _widthIsFromArrange = true;
            }// end if- save last known width on the overflow area

            return sz;
        }

        #endregion //ArrangeOverride	

        #region Automation
        /// <summary>
        /// Returns <see cref="OutlookBarGroup"/> Automation Peer Class <see cref="OutlookBarGroupAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new OutlookBarGroupAutomationPeer(this);
        }
        #endregion

		// AS 11/16/11 TFS79544
		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator to iterate the logical children.
		/// </summary>
		protected override System.Collections.IEnumerator LogicalChildren
		{
			get
			{
				var enumerator = base.LogicalChildren;

				// if we are suppressing the Content from the logical tree,
				// then remove that item from the enumerator we are returning
				if (enumerator != null && this.SuppressLogicalContent)
				{
					var list = new List<object>();

					while (enumerator.MoveNext())
					{
						if (enumerator.Current != this.Content)
							list.Add(enumerator.Current);
					}

					enumerator = list.GetEnumerator();
				}

				return enumerator;
			}
		}
		#endregion //LogicalChildren

		#region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

			// JM 04-30-09 TFS 16159 - Change to use 'PART_' naming convention but support old name for backward compatibility with customer styles.
			_image = this.GetTemplateChild("Image") as Image;
			if (_image == null)
				_image = this.GetTemplateChild("PART_Image") as Image;

            this.HasLargeImage = this.LargeImage != null;

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);


        }

        #endregion //OnApplyTemplate	

		// JM 05-05-09 TFS 17313 - Added
		#region OnContentChanged

		/// <summary>
		/// Called when the Content property changes
		/// </summary>
		/// <param name="oldContent">The old value of the Content property</param>
		/// <param name="newContent">The new value of the Content property</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			// AS 11/16/11 TFS79544
			// If by some chance the content is being changed while we have removed it from the logical tree, 
			// we should put it back in before calling the base since the base will call RemoveLogicalChild.
			//
			if (this.SuppressLogicalContent)
				this.SuppressLogicalContent = false;

			base.OnContentChanged(oldContent, newContent);

			if (this.IsSelected && this.OutlookBar != null)
				this.OutlookBar.UpdateSelectedGroupContent();
		}

		#endregion //OnContentChanged

		// JM 05-05-09 TFS 17313 - Added
		#region OnContentTemplateChanged

		/// <summary>
		/// Called when the ContentTemplate property changes
		/// </summary>
		/// <param name="oldContentTemplate">The old value of the ContentTemplate property</param>
		/// <param name="newContentTemplate">The new value of the ContentTemplate property</param>
		protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
		{
			base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);

			if (this.IsSelected && this.OutlookBar != null)
				this.OutlookBar.UpdateSelectedGroupContent();
		}

		#endregion //OnContentTemplateChanged

		// JM 05-05-09 TFS 17313 - Added
		#region OnContentTemplateSelectorChanged

		/// <summary>
		/// Called when the ContentTemplateSelector property changes
		/// </summary>
		/// <param name="oldContentTemplateSelector">The old value of the ContentTemplateSelector property</param>
		/// <param name="newContentTemplateSelector">The new value of the ContentTemplateSelector property</param>
		protected override void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
		{
			base.OnContentTemplateSelectorChanged(oldContentTemplateSelector, newContentTemplateSelector);

			if (this.IsSelected && this.OutlookBar != null)
				this.OutlookBar.UpdateSelectedGroupContent();
		}

		#endregion //OnContentTemplateSelectorChanged

		#region MeasureOverride

		/// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            bool locationChanged = _lastMeasuredLocation != this.Location;

            if (locationChanged)
            {
                if (double.IsInfinity((constraint.Width)) && OutlookBar!=null)
                {
                    double xobWidth = OutlookBar.ActualWidth;
                    Size noAutoWidthConstraint = new Size(xobWidth, constraint.Height);
                    noAutoWidthConstraint = base.MeasureOverride(noAutoWidthConstraint);
                }
            }

            Size sz = base.MeasureOverride(constraint);

            if (this.Location == OutlookBarGroupLocation.NavigationGroupArea)
            {
                if ((this._calculatedHeight < 1 && !_heightIsFromArrange)||this._calculatedHeight < sz.Height)
                    this._calculatedHeight = sz.Height;
            }// end if- save last known height 

            if (this.Location == OutlookBarGroupLocation.OverflowArea)
            {
                if ((this._calculatedWidth < 1 && !_widthIsFromArrange) || (this._calculatedWidth < sz.Width))
                    this._calculatedWidth = sz.Width;
            }// end if- save last known width on the overflow area

            if (locationChanged)
            {
                if (_lastMeasuredLocation == OutlookBarGroupLocation.NavigationGroupArea)
                    this._calculatedHeight = this.DesiredSize.Height;

                if (_lastMeasuredLocation == OutlookBarGroupLocation.OverflowArea)
                    this._calculatedWidth= this.DesiredSize.Width;
            }

            _lastMeasuredLocation = this.Location;

            return sz;
        }


        #endregion  //MeasureOverride

		// JM 11-19-08 TFS10398 - replace this with OnMouseLeftButtonDown

#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Invoked when an unhandled System.Windows.UIElement.MouseLeftButtonDown routed event is raised on this element.
		/// </summary>
		/// <param name="e">A <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			// JM 01-13-12 TFS96983 Don't capture the mouse - instead maintain a IsMouseLeftButtonPressed property
			// that we can trigger off of in XAML
			//this.CaptureMouse();    // see xaml; used to highlight on mouse down
			this.IsMouseLeftButtonPressed = true;

			base.OnMouseDown(e);

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

		#endregion //OnMouseLeftButtonDown

		#region OnMouseEnter

		/// <summary>
        /// Invoked when an unhandled Mouse.MouseEnter attached event is raised on this element
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            IsMouseOverGroup = IsMouseOverThis(e);
            base.OnMouseEnter(e);
        }

        #endregion //OnMouseEnter	

        #region OnMouseLeftButtonUp

        /// <summary>
        /// Invoked when an unhandled MouseLeftButtonUp routed event reaches an element in its route that is derived from this class
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the left mouse button was released</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();

            IsMouseOverGroup = IsMouseOverThis(e);

            this.IsSelected = IsMouseOverGroup || this.IsSelected;

			// JM 01-13-12 TFS96983
			this.IsMouseLeftButtonPressed = false;

            base.OnMouseLeftButtonUp(e);

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnMouseLeftButtonDown
    
        #region OnMouseLeave

        /// <summary>
        /// Invoked when an unhandled Mouse.MouseLeave attached event is raised on this element
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            IsMouseOverGroup = false;
            base.OnMouseLeave(e);
        }

        #endregion //OnMouseLeave	
    
        #region OnMouseMove

        /// <summary>
        /// Invoked when an unhandled Mouse.MouseMove attached event reaches an element in its route that is derived from this class
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            IsMouseOverGroup = IsMouseOverThis(e);
            base.OnMouseMove(e);
        }

        #endregion //OnMouseMove	
    
        #endregion // Base Class Overrides

        #region Properties

        #region Internal Properties

        #region DesiredOverflowWidth

        




        private double _desiredOverflowWidth = 12;
        internal double DesiredOverflowWidth
        {
            get
            {
                if (_lastMeasuredLocation == OutlookBarGroupLocation.OverflowArea)
                    return DesiredSize.Width;
                else if (_calculatedWidth > 0.1)
                    return _calculatedWidth;
                else
                    return _desiredOverflowWidth;
            }
        }

        #endregion //DesiredOverflowWidth

        #region DesiredNavigationHeight

        




        private double _desiredNavigationHeight= 36;
        internal double DesiredNavigationHeight
        {
            get
            {
                if (_calculatedHeight > 1)
                    return _calculatedHeight;
                else if (_lastMeasuredLocation == OutlookBarGroupLocation.NavigationGroupArea)
                    return DesiredSize.Height;
                else
                    return _desiredNavigationHeight;
            }
            set { _calculatedHeight = value; }
        }

        #endregion //DesiredNavigationHeight

        #region Image

        




        internal Image Image
        {
            get { return _image; }
        }

        #endregion //Image	
    
        #region InitialOrder

        




        internal int InitialOrder
        {
            get { return _initialOrder; }
            set { _initialOrder = value; }
        }

        #endregion //InitialOrder	

        #region IsVisibleOnStart

        




        internal bool IsVisibleOnStart
        {
            get { return _isVisibleOnStart; }
            set { _isVisibleOnStart = value; }
        }

        #endregion //IsVisibleOnStart	
    
        #region DefalultSmallImage

        




        internal ImageSource DefalultSmallImage
        {
            get
            {
                if (this.OutlookBar == null)
                    return null;
                return this.OutlookBar.GetValue(XamOutlookBar.DefaultSmallImageProperty) as ImageSource;
            }
        }

        #endregion //DefalultSmallImage	
    
		// AS 11/16/11 TFS79544
		#region SuppressLogicalContent
		internal bool SuppressLogicalContent
		{
			get { return _suppressLogicalContent; }
			set
			{
				if (value != _suppressLogicalContent)
				{
					_suppressLogicalContent = value;

					if (this.IsContentLogical)
					{
						if (_suppressLogicalContent)
						{
							Debug.Assert(LogicalTreeHelper.GetParent(this.Content as DependencyObject) == this);
							this.RemoveLogicalChild(this.Content);
						}
						else
						{
							Debug.Assert(LogicalTreeHelper.GetParent(this.Content as DependencyObject) == null);
							this.AddLogicalChild(this.Content);
						}
					}
				}
			}
		} 
		#endregion //SuppressLogicalContent

        #endregion //Internal Properties	
    
        #region Public Properties

        #region HasLargeImage
        /// <summary>
        /// Returns true if the <see cref="OutlookBarGroup"/> has a large image, false if not (read only).
        /// </summary>
        /// <seealso cref="LargeImage"/>
		/// <seealso cref="SmallImage"/>
		//[Description("Returns whether the OutlookBarGroup has a large image (read only).")]
        //[Category("OutlookBar Properties")] // Appearance
		[ReadOnly(true)]
        public bool HasLargeImage
        {
            get
            {
                return _hasLargeImage;
            }
            internal set
            {
                if (_hasLargeImage != value)
                {
                    _hasLargeImage = value;
                    NotifyPropertyChanged("HasLargeImage");
                }
            }
        }

        #endregion //HasLargeImage

        #region IsSelected 
        /// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected", typeof(bool), typeof(OutlookBarGroup),
            new FrameworkPropertyMetadata(KnownBoxes.FalseBox, 

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            OnVisualStatePropertyChanged, OnCoerceIsSelectedCallback));




        private static object OnCoerceIsSelectedCallback(DependencyObject obj, object value)
        {
            OutlookBarGroup gr = (OutlookBarGroup)obj;
            if (gr.OutlookBar == null)
                return value; // the group is outside of the XamOutlookBar

			// JM 10-6-10 TFS 37329
			if (false == gr.OutlookBar.Groups.Contains(gr))
				return false;

            return gr.OutlookBar.SelectGroup(gr, (bool)value);
        }

        /// <summary>
        /// Returns/sets a value that indicates if the <see cref="OutlookBarGroup"/> is selected. This is a dependency property.
        /// </summary>
        /// <seealso cref="XamOutlookBar.SelectedGroupProperty"/>
		/// <seealso cref="IsMouseOverGroup"/>
		//[Description("Returns/sets a value that indicates if the OutlookBarGroup is selected. This is a dependency property.")]
        //[Category("OutlookBar Properties")] // Behavior
        [Bindable(true)]
        public bool IsSelected
        {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, value); }
        }
        #endregion //IsSelected

		// JM 01-13-12 TFS96983 Added.
		#region IsMouseLeftButtonPressed
		internal static readonly DependencyPropertyKey IsMouseLeftButtonPressedKey = DependencyProperty.RegisterReadOnly(
			 "IsMouseLeftButtonPressed", typeof(bool), typeof(OutlookBarGroup), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the <see cref="IsMouseLeftButtonPressed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMouseLeftButtonPressedProperty = IsMouseLeftButtonPressedKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the left mouse button is pressed over the <see cref="OutlookBarGroup"/> (read only).
		/// </summary>
		[ReadOnly(true)]
		public bool IsMouseLeftButtonPressed
		{
			get { return (bool)this.GetValue(IsMouseLeftButtonPressedProperty); }
			internal set { this.SetValue(IsMouseLeftButtonPressedKey, value); }
		}

		#endregion //IsMouseLeftButtonPressed

		#region IsMouseOverGroup
		/// <summary>
        /// Returns a value indicating whether the mouse pointer is located over the <see cref="OutlookBarGroup"/> (read only).
        /// </summary>
		/// <seealso cref="IsSelected"/>
		//[Description("Returns a value indicating whether the mouse pointer is located over the OutlookBarGroup (read only).")]
        //[Category("OutlookBar Properties")] // Behavior
		[ReadOnly(true)]
        public bool IsMouseOverGroup
        {
            get { return _isMouseOverGroup; }
            internal set
            {
                if (_isMouseOverGroup != value)
                {
                    _isMouseOverGroup = value;
                    NotifyPropertyChanged("IsMouseOverGroup");

                    // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
                    this.UpdateVisualStates();

                }
            }
        }

        #endregion //IsMouseOverGroup	
    
        #region Key
        /// <summary>
		/// Returns/set a key associated with this <see cref="OutlookBarGroup"/>.  Though not required, if specified this key can be used to access the <see cref="OutlookBarGroup"/> in the <see cref="XamOutlookBar.Groups"/> collection.
        /// </summary>
		/// <seealso cref="XamOutlookBar.Groups"/>
        //[Description("Returns/set a key associated with this OutlookBarGroup.  Though not required, if specified this key can be used to access the OutlookBarGroup in the XamOutlookBar.Groups collection.")]
        //[Category("OutlookBar Properties")] // Behavior
        public string Key
        {
            get { return _key; }
            set 
            {
                if (_key != value)
                {
                    _key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }
        #endregion //Key

        #region Location
        internal static readonly DependencyPropertyKey LocationKey = DependencyProperty.RegisterReadOnly(
             "Location", typeof(OutlookBarGroupLocation), typeof(OutlookBarGroup), new FrameworkPropertyMetadata(

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            new PropertyChangedCallback(OnVisualStatePropertyChanged)

        ));

        /// <summary>
        /// Returns the OutlookBarGroupLocation of the specified group.
        /// </summary>
        public static readonly DependencyProperty LocationProperty = LocationKey.DependencyProperty;

        /// <summary>
        /// Returns the current location of the group (read only). This is a dependency property.
        /// </summary>
        /// <seealso cref="OutlookBarGroupLocation"/>
		//[Description("Returns the location of the group (read only). This is a dependency property.")]
        //[Category("OutlookBar Properties")] // Behavior
        public OutlookBarGroupLocation Location
        {
            get { return (OutlookBarGroupLocation)this.GetValue(LocationProperty); }
        }

        internal void SetLocation(OutlookBarGroupLocation location)
        {
            this.SetValue(LocationKey, location);
        }
        #endregion //Location

        #region LargeImage
        /// <summary>
		/// Identifies the <see cref="LargeImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LargeImageProperty = DependencyProperty.Register(
            "LargeImage", typeof(ImageSource), typeof(OutlookBarGroup), 
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLargeImageChanged)) 

            );
        /// <summary>
        /// Returns/sets the ImageSource for <see cref="OutlookBarGroup"/>'s LargeImage. This is a dependency property.
        /// The OutlookBarGroup displays the large image when it is in the navigation group area.
        /// </summary>
        /// <seealso cref="LargeImageProperty"/>
		/// <seealso cref="SmallImage"/>
		//[Description("Returns/sets the ImageSource for OutlookBarGroup's LargeImage.")]
        //[Category("OutlookBar Properties")] // Appearance
        [Bindable(true)]
        public ImageSource LargeImage
        {
            get { return (ImageSource)this.GetValue(LargeImageProperty); }
            set { this.SetValue(LargeImageProperty, value); }
        }

        private static void OnLargeImageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            OutlookBarGroup gr = o as OutlookBarGroup;

            if (gr == null)
                return;
            gr.HasLargeImage = gr.LargeImage != null;
        }

		
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


        #endregion //LargeImage

        #region OutlookBar
        /// <summary>
        /// Returns the <see cref="XamOutlookBar"/> that contains the <see cref="OutlookBarGroup"/> (read only). 
        /// </summary>
        /// <seealso cref="XamOutlookBar.OutlookBarProperty"/>
        //[Description("Returns XamOutlookBar that contains the OutlookBarGroup (read only).")]
        //[Category("OutlookBar Properties")] // Behavior
		[ReadOnly(true)]
        public XamOutlookBar OutlookBar
        {
            get
            {
                XamOutlookBar outlookBar = XamOutlookBar.GetOutlookBar(this);
                return outlookBar;
            }
        }

        #endregion //OutlookBar

        #region SmallImage
        /// <summary>
		/// Identifies the <see cref="SmallImage"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SmallImageProperty = DependencyProperty.Register(
            "SmallImage", typeof(ImageSource), typeof(OutlookBarGroup),
            new FrameworkPropertyMetadata(OnSmallImageChanged) 

																			   );

        static void OnSmallImageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            OutlookBarGroup gr = o as OutlookBarGroup;

            if (gr == null)
                return;

            gr.SmallImageResolved = e.NewValue as ImageSource;
        }

        /// <summary>
        /// Returns/sets the ImageSource for  <see cref="OutlookBarGroup"/>'s SmallImage. This is a dependency property.
        /// The OutlookBarGroup displays the small image when it is in the overflow area or in the overflow context menu.
        /// </summary>
        /// <seealso cref="SmallImageProperty"/>
		/// <seealso cref="LargeImage"/>
		//[Description("Returns/sets the ImageSource for OutlookBarGroup's SmallImage.")]
        //[Category("OutlookBar Properties")] // Appearance
        [Bindable(true)]
        public ImageSource SmallImage
        {
            get { return (ImageSource)this.GetValue(SmallImageProperty); }
            set { this.SetValue(SmallImageProperty, value); }
        }

        #endregion //SmallImage

        #region SmallImageResolved
        /// <summary>
        /// Returns the <see cref="SmallImage"/> if it has been set, otherwise returns a default small image for the OutlookBarGroup (read only).
        /// </summary>
        /// <seealso cref="XamOutlookBar.DefaultSmallImageKey"/>
        /// <seealso cref="SmallImage"/>
        //[Description("Returns the SmallImage if it has been set, otherwise returns a default small image for the OutlookBarGroup (read only).")]
        //[Category("OutlookBar Properties")] // Behavior
		[ReadOnly(true)]
        public ImageSource SmallImageResolved 
        {
            get { 
                return _smallImageResolved; 
            }
            internal set
            {
                if (_smallImageResolved != value)
                {
                    _smallImageResolved = value == null ? this.DefalultSmallImage : value; ;
                    NotifyPropertyChanged("SmallImageResolved");
                }
            }
        }
        #endregion //SmallImageResolved

        #endregion //Public Properties

		#region Private Properties

		// AS 11/16/11 TFS79544
		#region IsContentLogical
		private bool IsContentLogical
		{
			get
			{
				var content = this.Content as DependencyObject;

				if (null == content)
					return false;

				var logicalChildren = base.LogicalChildren;

				if (null != logicalChildren)
				{
					while (logicalChildren.MoveNext())
					{
						if (content == logicalChildren.Current)
							return true;
					}
				}

				return false;
			}
		}
		#endregion //IsContentLogical

		#endregion //Private Properties

		#endregion //Properties

        #region Methods

        #region Protected Methods

        #region VisualState... Methods


        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {

            // Set Common states
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else if (this.IsMouseOverGroup)
            {
				// JM 01-13-12 TFS96983 We are no longer capturing the mouse - use the new IsMouseLeftButtonPressed property instead.
				//if (this.IsMouseCaptured && this.IsSelected == false)
				if (this.IsMouseLeftButtonPressed && this.IsSelected == false)
					VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StatePressed, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                else
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            }
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            // set location state
            switch (this.Location)
            {
                case OutlookBarGroupLocation.NavigationGroupArea:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateNavigationArea, useTransitions);
                    break;
                case OutlookBarGroupLocation.OverflowArea:
                case OutlookBarGroupLocation.OverflowContextMenu:
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateOverflowArea, useTransitions);
                    break;
            }

            XamOutlookBar olbar = this.OutlookBar;

            // set minimized state
            if (olbar != null && olbar.IsMinimized)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateMinimized, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotMinimized, useTransitions);

            // set selected state
            if (this.IsSelected)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);

            // set splitter location state
            if (olbar != null && olbar.VerticalSplitterLocation == VerticalSplitterLocation.Left)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateLeft, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateRight, useTransitions);

        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            OutlookBarGroup group = target as OutlookBarGroup;

            if (group != null)
                group.UpdateVisualStates();
        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        internal protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        internal protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

        #endregion //Protected Methods

        #region Private Methods

        private bool IsMouseOverThis(MouseEventArgs e)
        {
            // The group receive mouseover when it is selected 
            // and mouse is over selected group area

            // Retrieve the coordinate of the mouse position.
            Point pt = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            DependencyObject o = result == null ? null : result.VisualHit;

            for (; (o != null) && (!(o == this)); o = VisualTreeHelper.GetParent(o)) ;
            return o != null;
        }

        // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private void OnIsMinimizedChanged()
        {

            this.UpdateVisualStates();

        }

        internal void ResolveSmallImage()
        {
            XamOutlookBar olbar = this.OutlookBar;

            // JJD 4/22/10 - NA2010 Vol 2 - Added support for VisualStateManager
            if (olbar != null)
                this._isMinimizedTracker = new PropertyValueTracker(olbar, XamOutlookBar.IsMinimizedProperty, OnIsMinimizedChanged, false);
            
            if (this.SmallImage == null)
                this.SmallImageResolved = this.DefalultSmallImage;
        }


        #endregion //Private Methods	
    
        #endregion //Methods

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        
        /// <summary>
        /// Raises property changed event
        /// </summary>
        /// <param name="e">The DependencyPropertyChangedEventArgs that contains the event data</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            NotifyPropertyChanged(e.Property.Name);
        }

        #endregion
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