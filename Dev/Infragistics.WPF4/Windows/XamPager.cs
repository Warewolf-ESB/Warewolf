using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using Infragistics.Windows.Themes;
using System.Windows.Media;

namespace Infragistics.Windows.Controls
{
	// AS 7/23/07
	// For some background, the main reason why we needed a derived scrollviewer was
	// because the scrollviewer expects a scrollcontentpresenter. However, we could not use
	// a scroll content presenter for scrolling the tab items because tabs extend outside 
	// the bounds of their parent (using a negative margin) but the scrollcontentpresenter
	// overrides GetLayoutClip and forces everything to be clipped to its bounds. That class 
	// is sealed so we have to derive our own contentpresenter - the PagerContentPresenter.
	// We then needed the derived scrollviewer to handle the arrow key logic and also to 
	// allow our pagercontentpresenter to be able to set the ScrollInfo that should be used - 
	// the ScrollInfo property is protected. While we were at it, we exposed properties to 
	// make it easier to control the visibility of the scroll buttons.
	//
	/// <summary>
	/// Custom <see cref="ScrollViewer"/> for scrolling with vertical or horizontal pager buttons.
	/// </summary>
	[TemplatePart(Name = "PART_ScrollLeft", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ScrollRight", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ScrollUp", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_ScrollDown", Type = typeof(FrameworkElement))]
	[TemplatePart(Name = "PART_PagerContentPresenter", Type = typeof(PagerContentPresenter))]
	[StyleTypedProperty(Property = "ScrollLeftButtonStyle", StyleTargetType = typeof(ButtonBase))]
	[StyleTypedProperty(Property = "ScrollRightButtonStyle", StyleTargetType = typeof(ButtonBase))]
	[StyleTypedProperty(Property = "ScrollUpButtonStyle", StyleTargetType = typeof(ButtonBase))]
	[StyleTypedProperty(Property = "ScrollDownButtonStyle", StyleTargetType = typeof(ButtonBase))]
		// AS 11/7/07 BR21903
		// AS 11/7/07 BR21903
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class XamPager : ScrollViewer
	{
		#region Member Variables

		private FrameworkElement _scrollLeftElement;
		private FrameworkElement _scrollRightElement;
		private FrameworkElement _scrollUpElement;
		private FrameworkElement _scrollDownElement;
		private PagerContentPresenter _contentPresenter;

		// AS 11/7/07 BR21903
		private Infragistics.Windows.Licensing.UltraLicense _license;

		// AS 11/27/07 BR28732
		private bool _ignoreBringIntoView = false;

		#endregion //Member Variables

		#region Constructor
		static XamPager()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamPager), new FrameworkPropertyMetadata(typeof(XamPager)));

			// AS 2/5/08 - Optimization
			// Changed to 1 handler creation.
			CanExecuteRoutedEventHandler canScrollHandler = new CanExecuteRoutedEventHandler(OnCanExecuteScrollCommand);
			CommandManager.RegisterClassCommandBinding(typeof(XamPager), new CommandBinding(ScrollBar.LineLeftCommand, null, canScrollHandler));
			CommandManager.RegisterClassCommandBinding(typeof(XamPager), new CommandBinding(ScrollBar.LineRightCommand, null, canScrollHandler));
			CommandManager.RegisterClassCommandBinding(typeof(XamPager), new CommandBinding(ScrollBar.LineUpCommand, null, canScrollHandler));
			CommandManager.RegisterClassCommandBinding(typeof(XamPager), new CommandBinding(ScrollBar.LineDownCommand, null, canScrollHandler));

			// AS 11/27/07 BR28732
			EventManager.RegisterClassHandler(typeof(XamPager), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		}

		/// <summary>
		/// Initializes a new <see cref="XamPager"/>
		/// </summary>
		public XamPager()
		{
			// AS 11/7/07 BR21903
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamPager), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template is applied to the element.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._scrollDownElement = this.GetTemplateChild("PART_ScrollDown") as FrameworkElement;
			this._scrollUpElement = this.GetTemplateChild("PART_ScrollUp") as FrameworkElement;
			this._scrollLeftElement = this.GetTemplateChild("PART_ScrollLeft") as FrameworkElement;
			this._scrollRightElement = this.GetTemplateChild("PART_ScrollRight") as FrameworkElement;
			this._contentPresenter = this.GetTemplateChild("PART_PagerContentPresenter") as PagerContentPresenter;
		}
		#endregion //OnApplyTemplate

		#region OnKeyDown
		/// <summary>
		/// Invoked when a key has been pressed while keyboard focus exists within the element.
		/// </summary>
		/// <param name="e">Provides information about the key event.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(e.Key, "XamPager");

			if (e.Handled == false && e.OriginalSource != this)
			{
				switch (e.Key)
				{
					case Key.Left:
					case Key.Right:
					case Key.Up:
					case Key.Down:
						if (this.DoesParentHandleScrolling == false)
						{
							this.ProcessArrowKey(e);

							// JJD 9/13/07
							// Return here so we don't call the base implementation which will set e.Handled to true always
							// which prevents the arrow keys from being used to navigate outside the scope of the pager
							return;
						}
						break;
					case Key.Home:
					case Key.End:
						// the base scroll viewer will eat a ctrl-home or ctrl-end
						// but doesn't do anything with it
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
							return;
						break;
				}
			}

			base.OnKeyDown(e);
		}
		#endregion //OnKeyDown

		#region OnPropertyChanged
		/// <summary>
		/// Invoked when a property of the element has been changed.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty)
				this.VerifyScrollHorizontalVisibility();
			else if (e.Property == ScrollViewer.ComputedVerticalScrollBarVisibilityProperty)
				this.VerifyScrollVerticalVisibility();

			if (null != this._contentPresenter)
			{
				if (e.Property == XamPager.ScrollLeftVisibilityProperty ||
					e.Property == XamPager.ScrollRightVisibilityProperty ||
					e.Property == XamPager.ScrollUpVisibilityProperty ||
					e.Property == XamPager.ScrollDownVisibilityProperty)
				{
					this._contentPresenter.InvalidateVisual();
				}
			}

			base.OnPropertyChanged(e);
		}
		#endregion //OnPropertyChanged

		#region OnScrollChanged
		/// <summary>
		/// Invoked when the scrolling state has changed.
		/// </summary>
		/// <param name="e">Provides information about the scroll state change</param>
		protected override void OnScrollChanged(ScrollChangedEventArgs e)
		{
			this.VerifyScrollHorizontalVisibility();
			this.VerifyScrollVerticalVisibility();

			if (e.HorizontalChange != 0 || e.VerticalChange != 0)
				CommandManager.InvalidateRequerySuggested();
			else
			{
				// if the scrollable area is no longer scrollable or is now scrollable
				bool wasScrolling = (e.ExtentWidth - e.ExtentWidthChange) - (e.ViewportWidth - e.ViewportWidthChange) > 0;
				bool isScrolling = e.ExtentWidth - e.ViewportWidth > 0;

				if (wasScrolling != isScrolling)
					CommandManager.InvalidateRequerySuggested();
			}

			base.OnScrollChanged(e);
		}
		#endregion //OnScrollChanged

		#endregion //Base class overrides

		#region Properties

		#region Internal

		#region ScrollInfoInternal
		internal IScrollInfo ScrollInfoInternal
		{
			get { return this.ScrollInfo; }
			set { this.ScrollInfo = value; }
		}
		#endregion //ScrollInfoInternal

		#endregion //Internal

		#region Public

		#region ScrollLeftVisibility

		private static readonly DependencyPropertyKey ScrollLeftVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ScrollLeftVisibility",
			typeof(Visibility), typeof(XamPager), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnScrollVisibilityChanged)));

		/// <summary>
		/// Identifies the <see cref="ScrollLeftVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollLeftVisibilityProperty =
			ScrollLeftVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the computed scroll left pager button visibility.
		/// </summary>
		/// <seealso cref="ScrollLeftVisibilityProperty"/>
		//[Description("Indicates the computed scroll left pager button visibility.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility ScrollLeftVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamPager.ScrollLeftVisibilityProperty);
			}
		}

		#endregion //ScrollLeftVisibility

		#region ScrollRightVisibility

		private static readonly DependencyPropertyKey ScrollRightVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ScrollRightVisibility",
			typeof(Visibility), typeof(XamPager), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnScrollVisibilityChanged)));

		/// <summary>
		/// Identifies the <see cref="ScrollRightVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollRightVisibilityProperty =
			ScrollRightVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the computed scroll right pager button visibility.
		/// </summary>
		/// <seealso cref="ScrollRightVisibilityProperty"/>
		//[Description("Indicates the computed scroll right pager button visibility.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility ScrollRightVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamPager.ScrollRightVisibilityProperty);
			}
		}

		#endregion //ScrollRightVisibility

		#region ScrollUpVisibility

		private static readonly DependencyPropertyKey ScrollUpVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ScrollUpVisibility",
			typeof(Visibility), typeof(XamPager), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnScrollVisibilityChanged)));

		/// <summary>
		/// Identifies the <see cref="ScrollUpVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollUpVisibilityProperty =
			ScrollUpVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the computed scroll up pager button visibility.
		/// </summary>
		/// <seealso cref="ScrollUpVisibilityProperty"/>
		//[Description("Indicates the computed scroll up pager button visibility.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility ScrollUpVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamPager.ScrollUpVisibilityProperty);
			}
		}

		#endregion //ScrollUpVisibility

		#region ScrollDownVisibility

		private static readonly DependencyPropertyKey ScrollDownVisibilityPropertyKey =
			DependencyProperty.RegisterReadOnly("ScrollDownVisibility",
			typeof(Visibility), typeof(XamPager), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnScrollVisibilityChanged)));

		/// <summary>
		/// Identifies the <see cref="ScrollDownVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollDownVisibilityProperty =
			ScrollDownVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates the computed scroll down pager button visibility.
		/// </summary>
		/// <seealso cref="ScrollDownVisibilityProperty"/>
		//[Description("Indicates the computed scroll down pager button visibility.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility ScrollDownVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamPager.ScrollDownVisibilityProperty);
			}
		}

		#endregion //ScrollDownVisibility

		#region ScrollLeftButtonStyle

		/// <summary>
		/// Identifies the <see cref="ScrollLeftButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollLeftButtonStyleProperty = DependencyProperty.Register("ScrollLeftButtonStyle",
			typeof(Style), typeof(XamPager), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the style used for the left scroll button
		/// </summary>
		/// <seealso cref="ScrollLeftButtonStyleProperty"/>
		/// <seealso cref="ScrollDownButtonStyle"/>
		/// <seealso cref="ScrollUpButtonStyle"/>
		/// <seealso cref="ScrollRightButtonStyle"/>
		//[Description("Returns or sets the style used for the left scroll button")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Style ScrollLeftButtonStyle
		{
			get
			{
				return (Style)this.GetValue(XamPager.ScrollLeftButtonStyleProperty);
			}
			set
			{
				this.SetValue(XamPager.ScrollLeftButtonStyleProperty, value);
			}
		}

		#endregion //ScrollLeftButtonStyle

		#region ScrollRightButtonStyle

		/// <summary>
		/// Identifies the <see cref="ScrollRightButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollRightButtonStyleProperty = DependencyProperty.Register("ScrollRightButtonStyle",
			typeof(Style), typeof(XamPager), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the style used for the right scroll button
		/// </summary>
		/// <seealso cref="ScrollRightButtonStyleProperty"/>
		/// <seealso cref="ScrollDownButtonStyle"/>
		/// <seealso cref="ScrollUpButtonStyle"/>
		/// <seealso cref="ScrollLeftButtonStyle"/>
		//[Description("Returns or sets the style used for the right scroll button")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Style ScrollRightButtonStyle
		{
			get
			{
				return (Style)this.GetValue(XamPager.ScrollRightButtonStyleProperty);
			}
			set
			{
				this.SetValue(XamPager.ScrollRightButtonStyleProperty, value);
			}
		}

		#endregion //ScrollRightButtonStyle

		#region ScrollUpButtonStyle

		/// <summary>
		/// Identifies the <see cref="ScrollUpButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollUpButtonStyleProperty = DependencyProperty.Register("ScrollUpButtonStyle",
			typeof(Style), typeof(XamPager), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the style used for the up scroll button
		/// </summary>
		/// <seealso cref="ScrollUpButtonStyleProperty"/>
		/// <seealso cref="ScrollDownButtonStyle"/>
		/// <seealso cref="ScrollLeftButtonStyle"/>
		/// <seealso cref="ScrollRightButtonStyle"/>
		//[Description("Returns or sets the style used for the up scroll button")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Style ScrollUpButtonStyle
		{
			get
			{
				return (Style)this.GetValue(XamPager.ScrollUpButtonStyleProperty);
			}
			set
			{
				this.SetValue(XamPager.ScrollUpButtonStyleProperty, value);
			}
		}

		#endregion //ScrollUpButtonStyle

		#region ScrollDownButtonStyle

		/// <summary>
		/// Identifies the <see cref="ScrollDownButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollDownButtonStyleProperty = DependencyProperty.Register("ScrollDownButtonStyle",
			typeof(Style), typeof(XamPager), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the style used for the down scroll button
		/// </summary>
		/// <seealso cref="ScrollDownButtonStyleProperty"/>
		/// <seealso cref="ScrollUpButtonStyle"/>
		/// <seealso cref="ScrollLeftButtonStyle"/>
		/// <seealso cref="ScrollRightButtonStyle"/>
		//[Description("Returns or sets the style used for the down scroll button")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Style ScrollDownButtonStyle
		{
			get
			{
				return (Style)this.GetValue(XamPager.ScrollDownButtonStyleProperty);
			}
			set
			{
				this.SetValue(XamPager.ScrollDownButtonStyleProperty, value);
			}
		}

		#endregion //ScrollDownButtonStyle

		#endregion //Public

		#region Private

		#region DoesParentHandleScrolling
		private bool DoesParentHandleScrolling
		{
			get
			{
				Control parent = this.TemplatedParent as Control;

				if (parent != null)
				{
					try
					{
						System.Reflection.PropertyInfo prop = typeof(Control).GetProperty("HandlesScrolling", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

						if (null != prop && KnownBoxes.TrueBox.Equals(prop.GetValue(parent, null)))
							return true;
					}
					catch (MethodAccessException) // AS 12/5/07 BR28892
					{
					}
					catch (System.Security.SecurityException)
					{
					}
				}

				return false;
			}
		}
		#endregion //DoesParentHandleScrolling

		#endregion //Private

		#endregion //Properties

		#region Methods

		#region GetScrollElement
		internal FrameworkElement GetScrollElement(PagerScrollDirection direction)
		{
			switch (direction)
			{
				case PagerScrollDirection.Up:
					return this._scrollUpElement;
				case PagerScrollDirection.Left:
					return this._scrollLeftElement;
				case PagerScrollDirection.Right:
					return this._scrollRightElement;
				case PagerScrollDirection.Down:
					return this._scrollDownElement;
				default:
					Debug.Fail("Unexpected direction:" + direction.ToString());
					return null;
			}
		}
		#endregion //GetScrollElement

		#region OnCanExecuteScrollCommand
		private static void OnCanExecuteScrollCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			XamPager pager = sender as XamPager;

			if (null != pager)
			{
				if (pager.ScrollInfo != null && pager.ScrollInfo == pager._contentPresenter)
				{
					
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				}
			}
		}
		#endregion //OnCanExecuteScrollCommand

		// AS 11/27/07 BR28732
		#region OnRequestBringIntoView
		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			XamPager pager = sender as XamPager;
			pager.OnRequestBringIntoView(e);
		}

		private void OnRequestBringIntoView(RequestBringIntoViewEventArgs e)
		{
			PagerContentPresenter contentPresenter = this._contentPresenter;

			// if the pager scrolling is handled by the content then we need to adjust the rect
			// because the contained iscrollinfo will not know that some of the area is obscured
			// by the pager buttons
			if (this._ignoreBringIntoView == false && contentPresenter != null && this.ScrollInfo != contentPresenter)
			{
				FrameworkElement targetElement = e.TargetObject as FrameworkElement;

				if (targetElement != null && contentPresenter.IsAncestorOf(targetElement))
				{
					// first get the child element rect
					Rect visualRect = e.TargetRect;

					if (visualRect.IsEmpty)
						visualRect = new Rect(targetElement.RenderSize);

					// now find out how much of it is obscured by the pager buttons
					Rect availableRect = new Rect(contentPresenter.RenderSize);
					Rect availableRectOrig = availableRect;

					contentPresenter.AdjustRect(ref availableRect, PagerScrollDirection.Left);
					contentPresenter.AdjustRect(ref availableRect, PagerScrollDirection.Right);
					contentPresenter.AdjustRect(ref availableRect, PagerScrollDirection.Up);
					contentPresenter.AdjustRect(ref availableRect, PagerScrollDirection.Down);

					// convert relative to the content presenter
					// AS 1/6/10 TFS25834
					//availableRect = contentPresenter.TransformToDescendant(targetElement).TransformBounds(availableRect);
					//availableRectOrig = contentPresenter.TransformToDescendant(targetElement).TransformBounds(availableRectOrig);
					GeneralTransform gt = contentPresenter.TransformToDescendant(targetElement);

					if (gt == null)
						return;

					availableRect = gt.TransformBounds(availableRect);
					availableRectOrig = gt.TransformBounds(availableRectOrig);

					Rect visualRectClipped = Rect.Intersect(visualRect, availableRectOrig);
					Rect intersection = Rect.Intersect(visualRectClipped, availableRect);

					if (intersection == visualRectClipped)
						return;

					if (intersection.Width != visualRectClipped.Width)
					{
						double offsetX = 0;

						// try to get the right side in view
						if (visualRectClipped.Right > availableRect.Right)
							visualRect.Width += visualRectClipped.Right - availableRect.Right;

						// make sure that the left side is in view
						if (visualRectClipped.Left - offsetX - availableRect.Left < 0)
							visualRect.X += visualRectClipped.Left - offsetX - availableRect.Left;
					}

					if (intersection.Height != visualRectClipped.Height)
					{
						double offsetY = 0;

						// try to get the right side in view
						if (visualRectClipped.Bottom > availableRect.Bottom)
							visualRect.Height += visualRectClipped.Bottom - availableRect.Bottom;

						// make sure that the left side is in view
						if (visualRectClipped.Top - offsetY - availableRect.Top < 0)
							visualRect.Y += visualRectClipped.Top - offsetY - availableRect.Top;
					}

					// consider the request handled
					e.Handled = true;

					this._ignoreBringIntoView = true;

					try
					{
						((FrameworkElement)e.TargetObject).BringIntoView(visualRect);
					}
					finally
					{
						this._ignoreBringIntoView = false;
					}
				}
			}
		}
		#endregion //OnRequestBringIntoView

		#region OnScrollVisibilityChanged
		private static void OnScrollVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}
		#endregion //OnScrollVisibilityChanged

		#region ProcessArrowKey
		private void ProcessArrowKey(KeyEventArgs e)
		{
			// we need to have a content presenter and the element with focus
			// must be within the content presenter...
			if (null == this._contentPresenter ||
				e.OriginalSource is DependencyObject == false ||
				false == this._contentPresenter.IsAncestorOf(e.OriginalSource as DependencyObject))
				return;

			FocusNavigationDirection direction;

			switch (e.Key)
			{
				case Key.Left:
					direction = FocusNavigationDirection.Left;
					break;
				case Key.Right:
					direction = FocusNavigationDirection.Right;
					break;
				case Key.Up:
					direction = FocusNavigationDirection.Up;
					break;
				case Key.Down:
					direction = FocusNavigationDirection.Down;
					break;
				default:
					return;
			}

			// if we're to follow the base behavior of the scroll viewer, we want to
			// use the content presenter to find the focus element if the element
			// with focus is not in view. if the element with focus is in view, we want
			// to have it determine the element to focus. if that element is in view,
			// then we would just focus it. if it is not, then we would scroll in 
			// the specified direction and then if it is in view, we would focus it.
			// if we focus OR scroll then we would mark the event handled

			DependencyObject focusedObject = Keyboard.FocusedElement as DependencyObject;
			DependencyObject elementToFocus = null;

			// if its in view then let it decide who should get focus...
			if (this._contentPresenter.IsDescendantInView(focusedObject))
			{
				if (focusedObject is UIElement)
					elementToFocus = ((UIElement)focusedObject).PredictFocus(direction);
				else if (focusedObject is ContentElement)
					elementToFocus = ((ContentElement)focusedObject).PredictFocus(direction);
			}
			else if (this._contentPresenter != null)
			{
				// otherwise let the content presenter choose
				elementToFocus = this._contentPresenter.PredictFocus(direction);
			}

			// if we found something to focus...
			if (elementToFocus is IInputElement && this._contentPresenter.IsAncestorOf(elementToFocus))
			{
				bool isElementInView = this._contentPresenter.IsDescendantInView(elementToFocus);

				// if its not in view then scroll in that direction and see
				// if it comes into view
				if (isElementInView == false && e.KeyboardDevice.Modifiers == ModifierKeys.None)
				{
					e.Handled = true;

					// scroll in the requested direction and check again
					switch (e.Key)
					{
						case Key.Left:
							if (this.FlowDirection == FlowDirection.RightToLeft)
								this.LineRight();
							else
								this.LineLeft();
							break;
						case Key.Right:
							if (this.FlowDirection == FlowDirection.RightToLeft)
								this.LineLeft();
							else
								this.LineRight();
							break;
						case Key.Down:
							this.LineDown();
							break;
						case Key.Up:
							this.LineUp();
							break;
					}

					// force an arrange
					this.UpdateLayout();

					// now check again
					isElementInView = this._contentPresenter.IsDescendantInView(elementToFocus);
				}

				// lastly focus the element if it is in view
				if (isElementInView)
				{
					((IInputElement)elementToFocus).Focus();
					e.Handled = true;
				}
			}
		}
		#endregion //ProcessArrowKey

		#region VerifyScrollHorizontalVisibility
		// AS 7/23/07
		// In theory you could use something like the following to show/hide the 
		// pager buttons:
		//
		//<RepeatButton.Visibility>
		//    <MultiBinding FallbackValue="Visibility.Collapsed" 
		//                  Converter="{StaticResource MenuScrollVisConverter}" 
		//                  ConverterParameter="0">
		//      <Binding Path="ComputedHorizontalScrollBarVisibility" RelativeSource="{RelativeSource TemplatedParent}" />
		//      <Binding Path="HorizontalOffset" RelativeSource="{RelativeSource TemplatedParent}" />
		//      <Binding Path="ExtentWidth" RelativeSource="{RelativeSource TemplatedParent}" />
		//      <Binding Path="ViewportWidth" RelativeSource="{RelativeSource TemplatedParent}" />
		//    </MultiBinding>
		// </RepeatButton.Visibility>
		// 
		// However, this only accounts for the computed scrollbar visibility and does not
		// account for when the scrollbar visibility is set to auto or not. So if the Visibility
		// was not set to Auto and the button wasn't needed, the button would be hidden even
		// though the programmer indicated they wanted the buttons always. Since we already 
		// needed the derived scroll viewer, we exposed properties for the buttons as well.
		//
		private void VerifyScrollHorizontalVisibility()
		{
			object leftVis, rightVis;

			if (this.ComputedHorizontalScrollBarVisibility == Visibility.Visible && this.ScrollInfo != null)
			{
				double scrollableExtent = this.ScrollInfo.ExtentWidth - this.ScrollInfo.ViewportWidth;
				bool isAuto = this.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto;

				leftVis = isAuto && this.ScrollInfo.HorizontalOffset == 0
						? KnownBoxes.VisibilityCollapsedBox
						: KnownBoxes.VisibilityVisibleBox;
				rightVis = isAuto && this.ScrollInfo.HorizontalOffset >= scrollableExtent
						? KnownBoxes.VisibilityCollapsedBox
						: KnownBoxes.VisibilityVisibleBox;
			}
			else
			{
				leftVis = rightVis = KnownBoxes.VisibilityCollapsedBox;
			}

			this.SetValue(ScrollLeftVisibilityPropertyKey, leftVis);
			this.SetValue(ScrollRightVisibilityPropertyKey, rightVis);
		}
		#endregion //VerifyScrollHorizontalVisibility

		#region VerifyScrollVerticalVisibility
		private void VerifyScrollVerticalVisibility()
		{
			object upVis, downVis;

			if (this.ComputedVerticalScrollBarVisibility == Visibility.Visible && this.ScrollInfo != null)
			{
				double scrollableExtent = this.ScrollInfo.ExtentHeight - this.ScrollInfo.ViewportHeight;
				bool isAuto = this.VerticalScrollBarVisibility == ScrollBarVisibility.Auto;

				upVis = isAuto && this.ScrollInfo.VerticalOffset == 0
						? KnownBoxes.VisibilityCollapsedBox
						: KnownBoxes.VisibilityVisibleBox;

				downVis = isAuto && this.ScrollInfo.VerticalOffset >= scrollableExtent
						? KnownBoxes.VisibilityCollapsedBox
						: KnownBoxes.VisibilityVisibleBox;
			}
			else
			{
				upVis = downVis = KnownBoxes.VisibilityCollapsedBox;
			}
			this.SetValue(ScrollUpVisibilityPropertyKey, upVis);
			this.SetValue(ScrollDownVisibilityPropertyKey, downVis);
		}
		#endregion //VerifyScrollVerticalVisibility

		#endregion //Methods
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