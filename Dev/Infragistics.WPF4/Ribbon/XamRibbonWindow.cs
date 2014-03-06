//#define DEBUG_HOOKPROC





using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

using Infragistics.Windows;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Licensing;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Ribbon.Events;
using System.Collections;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Security;
using Infragistics.Shared;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A derived <see cref="Window"/> class that is designed to display a <see cref="XamRibbon"/> within its caption area similar to that of Microsoft Office 2007 applications.
	/// </summary>
    /// <remarks>
	/// <p class="body">The XamRibbonWindow class is derived from the WPF <see cref="Window"/> class. It is designed to be used with the <see cref="XamRibbon"/> in rich client applications to 
	/// enable the placement of the <b>XamRibbon</b> in the caption area (i.e., non-client area) of the window. When the <b>XamRibbon</b> is used in a standard 
	/// WPF <see cref="Window"/> or on a WPF <see cref="Page"/>, it appears inside the bounds of the window�s or page�s non-client area.</p>
	/// <p class="note"><b>Note:</b> The only <see cref="ContentControl.Content"/> that should be placed inside this window is a <see cref="RibbonWindowContentHost"/>. The 
	/// <see cref="XamRibbon"/> is associated with this window by setting the <see cref="RibbonWindowContentHost.Ribbon"/> property of the RibbonWindowContentHost to an instance of a XamRibbon.</p>
	/// </remarks>
	/// <seealso cref="XamRibbon"/>
    /// <seealso cref="RibbonWindowContentHost"/>
	/// <seealso cref="RibbonWindowContentHost.Ribbon"/>
		// AS 11/7/07 BR21903
			// AS 11/7/07 BR21903
	[TemplatePart(Name="PART_ResizeGrip", Type=typeof(ResizeGrip))] // AS 12/3/09 TFS24545
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class XamRibbonWindow : Window
		, IRibbonWindow // AS 6/3/08 BR32772
    {        
		#region Private Members

        private bool isInMessageLoop;
        private bool haveExtendedGlass;
        private bool cachedIsDWMCompositionEnabled;

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        private bool? _cachedIsScenicTheme = null;

        private Color originalBackgroundColor;

		// AS 11/7/07 BR21903
		private UltraLicense _license;

        // JJD 11/30/07 - BR27696
        // added content host reference
        private RibbonWindowContentHost _contentHost;

        // JJD 12/10/07 - BR28958
        // Cache the clipping region for the content
        private Geometry _contentClip;

		// AS 7/20/09 TFS19661
		private bool _hasMinMax;
		private int _minTrackSizeX;
		private int _minTrackSizeY;
		private int _maxTrackSizeX;
		private int _maxTrackSizeY;

		// AS 6/22/11 TFS74670
		// Steve added an attached DP - ModalWindowHelper.IsModalWindowOpen - that indicates when there is a 
		// contained modal XamDialogWindow. When that is true we'll avoid delegating the WM_NCHITTEST to the 
		// DWM so the min/max/close buttons are not ui interactive while the modal dialog is displayed.
		//
		private bool _cachedIsModalWindowOpen = false;

		private List<int> CallWndProcMessages = new List<int>(); //AS 8/6/12 TFS118105

		#endregion //Private Members	

        #region Constants

        // JJD 12/10/07 - BR28958
        // Changed radius to match Office 
        //internal const int WINDOW_CORNER_RADIUS = 5;
        internal const int WINDOW_CORNER_RADIUS = 9;

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        internal const int SCENIC_WINDOW_TOP_CORNER_RADIUS = 9;

        #endregion //Constants

        #region Constructors

        /// <summary>
		/// Initializes a new instance of <see cref="XamRibbonWindow"/>
		/// </summary>
		public XamRibbonWindow()
		{
			// AS 11/7/07 BR21903
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamRibbonWindow), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

            // Initialize the cached flag now, since if we wait until OnSourceInitialized, the OnRibbonChanged
            // event will not have the proper cached value.
            this.cachedIsDWMCompositionEnabled = NativeWindowMethods.IsDWMCompositionEnabled;
			this.SetValue(IsGlassActivePropertyKey, this.cachedIsDWMCompositionEnabled);
		}

		static XamRibbonWindow()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(XamRibbonWindow), new FrameworkPropertyMetadata(typeof(XamRibbonWindow)));

			// AS 6/3/08 BR32772
			// We have to define the style and template in code since we cannot define it in the xaml
			// or else the xaml will not be able to be parsed when run under an xbap.
			//
			#region StyleProperty

			ControlTemplate defaultTemplate = new ControlTemplate(typeof(XamRibbonWindow));
			//<Grid>
			FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(Grid));

			//<AdornerDecorator>
            //  <ContentPresenter/>
            //</AdornerDecorator>
			FrameworkElementFactory fefAdorner = new FrameworkElementFactory(typeof(System.Windows.Documents.AdornerDecorator));
			fefAdorner.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));
			fefRoot.AppendChild(fefAdorner);

			//<ResizeGrip x:Name="WindowResizeGrip"
			//            Visibility="Collapsed"
			//            Style="{DynamicResource {x:Static igRibbon:XamRibbonWindow.ResizeGripStyleKey}}"
			//            HorizontalAlignment="Right"
			//            VerticalAlignment="Bottom"
			//            IsTabStop="False"
			//            />
			// AS 12/3/09 TFS24545
			// Change the default name used to a part so its part of the element's template requirements.
			//
			//const string GripName = "WindowResizeGrip";
			const string GripName = "PART_ResizeGrip";
			FrameworkElementFactory fefGrip = new FrameworkElementFactory(typeof(ResizeGrip));
			fefGrip.Name = GripName;
			fefGrip.SetValue(ResizeGrip.VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
			fefGrip.SetValue(ResizeGrip.StyleProperty, new DynamicResourceExtension(RibbonWindowContentHost.ResizeGripStyleKey));
			fefGrip.SetValue(ResizeGrip.HorizontalAlignmentProperty, KnownBoxes.HorizontalAlignmentRightBox);
			fefGrip.SetValue(ResizeGrip.VerticalAlignmentProperty, KnownBoxes.VerticalAlignmentBottomBox);
			fefGrip.SetValue(ResizeGrip.IsTabStopProperty, KnownBoxes.FalseBox);
            
            // JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
            // Bind the resize grip margin to ResizeGripMarginProperty 
            Binding b = new Binding();
            b.Path = new PropertyPath(XamRibbonWindow.ResizeGripMarginProperty);
            b.Mode = BindingMode.OneWay;
            b.RelativeSource = RelativeSource.TemplatedParent;
            fefGrip.SetBinding(ResizeGrip.MarginProperty, b);

            fefRoot.AppendChild(fefGrip);

		    //</Grid>
			defaultTemplate.VisualTree = fefRoot;

			//<MultiTrigger>
			//  <MultiTrigger.Conditions>
			//    <Condition Property="ResizeMode" Value="CanResizeWithGrip" />
			//    <Condition Property="WindowState" Value="Normal" />
			//  </MultiTrigger.Conditions>
			//  <Setter Property="Visibility" TargetName="WindowResizeGrip" Value="Visible" />
			//</MultiTrigger>
			MultiTrigger multiTrigger = new MultiTrigger();
			multiTrigger.Conditions.Add(new Condition(Window.ResizeModeProperty, ResizeMode.CanResizeWithGrip));
			multiTrigger.Conditions.Add(new Condition(Window.WindowStateProperty, WindowState.Normal));
			multiTrigger.Setters.Add(new Setter(ResizeGrip.VisibilityProperty, KnownBoxes.VisibilityVisibleBox, GripName));
			defaultTemplate.Triggers.Add(multiTrigger);

            // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
            // Add trigger to get scenic style for resize grip if IsScenicTheme is true
            Trigger trigger = new Trigger();
            trigger.Property = XamRibbon.IsScenicThemeProperty;
            trigger.Value = KnownBoxes.TrueBox;
            trigger.Setters.Add(new Setter(FrameworkElement.StyleProperty, new DynamicResourceExtension(RibbonWindowContentHost.ScenicResizeGripStyleKey), GripName));
			defaultTemplate.Triggers.Add(trigger);

			defaultTemplate.Seal();

			Style defaultStyle = new Style(typeof(XamRibbonWindow));
			defaultStyle.Setters.Add(new Setter(XamRibbonWindow.ForegroundProperty, new DynamicResourceExtension(SystemColors.WindowTextBrushKey)));
			defaultStyle.Setters.Add(new Setter(XamRibbonWindow.BackgroundProperty, new DynamicResourceExtension(RibbonBrushKeys.WindowBackgroundBrushKey)));
			defaultStyle.Setters.Add(new Setter(XamRibbonWindow.BorderThicknessProperty, new Thickness()));
			defaultStyle.Setters.Add(new Setter(XamRibbonWindow.BorderBrushProperty, Brushes.Transparent));
			defaultStyle.Setters.Add(new Setter(XamRibbonWindow.MinWidthProperty, 255d));
			defaultStyle.Setters.Add(new Setter(XamRibbonWindow.TemplateProperty, defaultTemplate));
			defaultStyle.Seal();
			StyleProperty.OverrideMetadata(typeof(XamRibbonWindow), new FrameworkPropertyMetadata(defaultStyle)); 

			#endregion //StyleProperty

			// Create and register commands.
			// AS 6/3/08 BR32772
			//XamRibbonWindow.MaximizeCommand = new RoutedCommand("Maximize", typeof(XamRibbonWindow));
			//XamRibbonWindow.MinimizeCommand = new RoutedCommand("Minimize", typeof(XamRibbonWindow));
			//XamRibbonWindow.RestoreCommand = new RoutedCommand("Restore", typeof(XamRibbonWindow));
			//XamRibbonWindow.CloseCommand = new RoutedCommand("Close", typeof(XamRibbonWindow));
			XamRibbonWindow.MaximizeCommand = RibbonWindowCommands.MaximizeCommand;
			XamRibbonWindow.MinimizeCommand = RibbonWindowCommands.MinimizeCommand;
			XamRibbonWindow.RestoreCommand = RibbonWindowCommands.RestoreCommand;
			XamRibbonWindow.CloseCommand = RibbonWindowCommands.CloseCommand;

			CommandManager.RegisterClassCommandBinding(typeof(XamRibbonWindow), new CommandBinding(XamRibbonWindow.MaximizeCommand, new ExecutedRoutedEventHandler(XamRibbonWindow.OnExecuteCommand), new CanExecuteRoutedEventHandler(XamRibbonWindow.OnCanExecuteCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(XamRibbonWindow), new CommandBinding(XamRibbonWindow.MinimizeCommand, new ExecutedRoutedEventHandler(XamRibbonWindow.OnExecuteCommand), new CanExecuteRoutedEventHandler(XamRibbonWindow.OnCanExecuteCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(XamRibbonWindow), new CommandBinding(XamRibbonWindow.RestoreCommand, new ExecutedRoutedEventHandler(XamRibbonWindow.OnExecuteCommand), new CanExecuteRoutedEventHandler(XamRibbonWindow.OnCanExecuteCommand)));
			CommandManager.RegisterClassCommandBinding(typeof(XamRibbonWindow), new CommandBinding(XamRibbonWindow.CloseCommand, new ExecutedRoutedEventHandler(XamRibbonWindow.OnExecuteCommand), new CanExecuteRoutedEventHandler(XamRibbonWindow.OnCanExecuteCommand)));

			// AS 10/23/07 XamRibbonWindow IconResolved
			Window.IconProperty.OverrideMetadata(typeof(XamRibbonWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIconChanged)));

			// AS 1/13/10 TFS25545
			// When restoring from the minimized state we need to invalidate
			// the window visual so the border will be re-rendered.
			//
			Window.WindowStateProperty.OverrideMetadata(typeof(XamRibbonWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnWindowStateChanged)));


			// AS 6/22/12 TFS115111
			//// AS 6/22/11 TFS74670
			//Infragistics.Windows.Internal.ModalWindowHelper.IsModalWindowOpenProperty.OverrideMetadata(typeof(XamRibbonWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsModalWindowOpenChanged)));

		}

		#endregion //Constructors	
    
		#region Base class overrides
        
            #region ArrangeOverride

            // We don't want to call the base implementation of this method because
            // it will size the window base on the WindowState, which is not what we want
            // since we're controlling the size of the form through the NCCALCSIZE message.
            //
            /// <summary>
            /// Overridden.  Positions child elements and determines the size of this element based on the subclassed window.
            /// </summary>
            /// <param name="arrangeBounds">The area within the parent that the element should use to arrange itself and its children.</param>
            /// <returns>The actual size used.</returns>
            protected override Size ArrangeOverride(Size arrangeBounds)
            {
                if (this.VisualChildrenCount > 0)
                {
					// AS 7/17/09 TFS19447
					// I also noticed that we were adjusting the arrangebounds (removing non-client area) and then 
					// actually returning that. We should return the original arrange bounds.
					//
					Size actualArrangeSize = arrangeBounds;

                    if (this.IsGlassActiveInternal)
                    {
						
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

						Size nonClientSize = this.NonClientSize;

						actualArrangeSize.Height = Math.Max(arrangeBounds.Height - nonClientSize.Height, 0);
						actualArrangeSize.Width = Math.Max(arrangeBounds.Width - nonClientSize.Width, 0);
					}
					// AS 12/2/09 TFS24267
					// The window will increase the size of the window based on the border size. For the left/right/top this 
					// is ok but for the bottom when there is a status bar this means that the statusbar is partially out of 
					// view.
					//
					else if (this.WindowState == WindowState.Maximized &&
							_contentHost != null &&
							_contentHost.StatusBar != null &&
							_contentHost.StatusBar.Visibility != Visibility.Collapsed)
					{
						actualArrangeSize.Height = Math.Max(0, actualArrangeSize.Height - this.LogicalBorderSize.Height);
					}

                    FrameworkElement fe = this.GetVisualChild(0) as FrameworkElement;

                    // AS 10/18/08
                    // We were already upcasting to FrameworkElement but now we'll explicitly ensure this
                    // since we need to be able to set the layout transform.
                    //
                    if (null == fe)
                        throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidRibbonWindowRootElement"));

					// AS 7/17/09 TFS19447
                    //fe.Arrange(new Rect(arrangeBounds));
					fe.Arrange(new Rect(actualArrangeSize));

                    // AS 10/18/08 TFS6238/BR34682
                    // We need to apply a layout transform on the root element. The window would have done this
                    // but we don't call the base ArrangeOverride since it will use the assumed non-client area
                    // to reduce the arrange bounds and measure the root child.
                    //
                    if (base.FlowDirection == FlowDirection.RightToLeft)
                        fe.LayoutTransform = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, arrangeBounds.Width, 0.0);
                    else
                        fe.LayoutTransform = null;
                }

                return arrangeBounds;
            }
            #endregion //ArrangeOverride

            #region MeasureOverrides
            /// <summary>
            /// Invoked to measure the element and its children.
            /// </summary>
            /// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
            /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
            protected override Size MeasureOverride(Size availableSize)
            {
				
#region Infragistics Source Cleanup (Region)

























































#endregion // Infragistics Source Cleanup (Region)

				if (this.VisualChildrenCount > 0)
				{
					UIElement element = this.GetVisualChild(0) as UIElement;

					if (null != element)
					{
						Size nonClientSize = new Size();
						Size measureSize = availableSize;

						this.EnforceMinMax(ref measureSize);

						if (this.IsGlassActiveInternal)
						{
							nonClientSize = this.NonClientSize;

							// remove the non-client area before measuring the child (unless the constraint is infinity)
							if (!double.IsPositiveInfinity(measureSize.Width))
							{
								measureSize.Width = Math.Max(measureSize.Width - nonClientSize.Width, 0);
							}

							if (!double.IsPositiveInfinity(measureSize.Height))
							{
								measureSize.Height = Math.Max(measureSize.Height - nonClientSize.Height, 0);
							}
						}
						// AS 12/2/09 TFS24267
						// The window will increase the size of the window based on the border size. For the left/right/top this 
						// is ok but for the bottom when there is a status bar this means that the statusbar is partially out of 
						// view.
						//
						else if (this.WindowState == WindowState.Maximized &&
							_contentHost != null &&
							_contentHost.StatusBar != null && 
							_contentHost.StatusBar.Visibility != Visibility.Collapsed)
						{
							measureSize.Height = Math.Max(0, measureSize.Height - this.LogicalBorderSize.Height);
						}

						element.Measure(measureSize);

						Size desired = element.DesiredSize;

						// then add that non-client area back in
						desired.Width += nonClientSize.Width;
						desired.Height += nonClientSize.Height;

						// AS 7/20/09 TFS19661
						// Since we're now measuring the root child directly we need to 
						// enforce the min/max size.
						//
						this.EnforceMinMax(ref desired);

						return desired;
					}
				}

				return availableSize;
            }
            #endregion //MeasureOverride

			#region OnApplyTemplate
			/// <summary>
			/// Invoked when the template for the element has been applied.
			/// </summary>
			public override void OnApplyTemplate()
			{
				base.OnApplyTemplate();

				// AS 12/3/09 TFS24545
				// We need to know when the resize grip is visible and how wide it is 
				// so we can conditionally exclude that area for the status bar.
				//
				ResizeGrip grip = (this.GetTemplateChild("PART_ResizeGrip") ?? this.GetTemplateChild("WindowResizeGrip")) as ResizeGrip;

				if (null != grip)
				{
					this.SetBinding(ResizeGripVisibilityProperty, Utilities.CreateBindingObject(UIElement.VisibilityProperty, BindingMode.OneWay, grip));
					this.SetBinding(ResizeGripWidthProperty, Utilities.CreateBindingObject(FrameworkElement.ActualWidthProperty, BindingMode.OneWay, grip));
				}
				else
				{
					this.ClearValue(ResizeGripWidthProperty);
					this.ClearValue(ResizeGripVisibilityProperty);
				}
			} 
			#endregion //OnApplyTemplate

            // JJD 12/10/07 - BR28958
            // Draw the outer border on the XamRibbinWindow instead on the RibbonWindowBorder
            #region OnRender

            /// <summary>
            /// Called when the element needs to be rendered
            /// </summary>
            /// <param name="dc">The <see cref="System.Windows.Media.DrawingContext"/> that defines the object to be drawn.</param>
            protected override void OnRender(DrawingContext dc)
            {
                // Bail out if the thickness is, for example, (0,0,0,0), or otherwise not the typical border that we're expecting.
                if (this._contentHost == null ||
                     this.IsGlassActiveInternal ||
                     this.WindowState != WindowState.Normal)
                    return;

                Brush outerBrush;
                if (this.IsActive)
                    outerBrush = this.OuterBrushActive;
                else
                    outerBrush = this.OuterBrush;

                // If the status bar is visible, we need to draw our rounded rectangle, but if it's hidden the bottom
                // borders are not rounded.
                Size size = this.RenderSize;

                Debug.WriteLineIf(this._contentClip == null, "We are drawing the outer window borders but we don't have a clip for the content host");

                Rect rect = new Rect(size);

                // clip out the content area
                if (this._contentClip != null)
                {
                    Geometry clip = new RectangleGeometry(rect);
                    clip = new CombinedGeometry(GeometryCombineMode.Exclude, clip, this._contentClip);

                    // JJD 4/29/10 - Optimization
                    // Freeze the clip geometry so the framework doesn't need to listen for changes
                    clip.Freeze();

                    dc.PushClip(clip);
                }

                dc.DrawRectangle(outerBrush, null, rect);

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
                // For scenic themes draw the outer shadow
                if (this.IsScenicTheme)
                {
                    Brush shadowBrush;
                    if (this.IsActive)
                        shadowBrush = this.OuterShadowBrushActive;
                    else
                        shadowBrush = this.OuterShadowBrush;

                    if (shadowBrush != null && shadowBrush != outerBrush)
                    {
                        Geometry clip = new RectangleGeometry(rect);
                        Geometry clipOffset = new RectangleGeometry(Rect.Offset(rect, -1.0, -1.0));

                        clip = new CombinedGeometry(GeometryCombineMode.Exclude, clip, clipOffset);

                        clip.Freeze();

                        dc.PushClip(clip);

                        dc.DrawRectangle(shadowBrush, null, rect);

                        dc.Pop();
                    }
                }

                // pop off the clipping region
                if (this._contentClip != null)
                    dc.Pop();
           }

            #endregion //OnRender

			#region OnRenderSizeChanged

		/// <summary>       
		/// Raises the <see cref="FrameworkElement.SizeChanged"/> event, using the specified
		/// information as part of the eventual event data.
		/// </summary>
		/// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			this.UpdateWindowClipRegion();

			base.OnRenderSizeChanged(sizeInfo);
		}
			#endregion //OnRenderSizeChanged

            #region OnSourceInitialized

        /// <summary>
		/// Called during window initialization.
		/// </summary>
		/// <param name="e">The event args</param>
		protected override void OnSourceInitialized(EventArgs e)
		{
            // JJD 11/30/07 - BR27696
            // If we don't have a content host then don't bother hooking anything up
            if (this._contentHost == null)
            {
                base.OnSourceInitialized(e);
                return;
            }

			WindowInteropHelper windowHelper = new WindowInteropHelper(this);

			IntPtr hwnd = windowHelper.Handle;

			// AS 5/8/09 TFS16346
			Debug.Assert(hwnd != IntPtr.Zero);

			HwndSource hwndSource = HwndSource.FromHwnd(hwnd);

			hwndSource.AddHook(new HwndSourceHook(HookProc));

            // JJD 12/10/07 removed unnecessary method call
			//this.InitializeWindowStyles(hwnd);

            // Check to see if we need extra borders on the sides of the window (i.e. if glass is disabled), then extend
            // the glass into the caption as necessary.
            this.UpdateWindowBorders();
            this.ExtendGlassIntoClientArea();
            this.SynchronizeCaptionInformation();

            // Update the clip region so that the bottom corners are rounded, assuming we're not on glass
            this.UpdateWindowClipRegion();

			NativeWindowMethods.SetWindowPosApi(
				hwnd,
				IntPtr.Zero,
				0, 0, 0, 0,
				NativeWindowMethods.SWP_NOACTIVATE |
				NativeWindowMethods.SWP_NOMOVE |
				NativeWindowMethods.SWP_NOSIZE |
				NativeWindowMethods.SWP_NOZORDER |
				NativeWindowMethods.SWP_NOOWNERZORDER |
				NativeWindowMethods.SWP_FRAMECHANGED );

			base.OnSourceInitialized(e);
            
            // We need to get the caption button information, but it isn't available to us until later.
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Loaded, new XamRibbon.MethodInvoker(this.SynchronizeCaptionInformation));
		}
			#endregion //OnSourceInitialized	

		#endregion //Base class overrides

		#region Commands

		/// <summary>
		/// Maximizes the <see cref="XamRibbonWindow"/>.
		/// </summary>
		public static readonly RoutedCommand MaximizeCommand;

		/// <summary>
		/// Minimizes the <see cref="XamRibbonWindow"/>.
		/// </summary>
		public static readonly RoutedCommand MinimizeCommand;

		/// <summary>
		/// Restores the <see cref="XamRibbonWindow"/> to its normal size.
		/// </summary>
		public static readonly RoutedCommand RestoreCommand;

		/// <summary>
		/// Closes the <see cref="XamRibbonWindow"/>.
		/// </summary>
		public static readonly RoutedCommand CloseCommand;

		#endregion Commands

		#region Properties

			#region Public Properties

				#region IsGlassActive

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

		internal static readonly DependencyPropertyKey IsGlassActivePropertyKey = XamRibbon.IsGlassActivePropertyKey;

		/// <summary>
		/// Identifies the IsGlassActive attached readonly dependency property
		/// </summary>
		/// <seealso cref="IsGlassActive"/>
		public static readonly DependencyProperty IsGlassActiveProperty =
			IsGlassActivePropertyKey.DependencyProperty.AddOwner(typeof(XamRibbonWindow));


		/// <summary>
		/// Returns a boolean indicating whether the window is rendering using Vista Glass.
		/// </summary>
		/// <value>
		/// True if Vista Glass is enabled (read-only)
		/// </value>
		/// <seealso cref="IsGlassActiveProperty"/>
		public bool IsGlassActive
		{
			get	{ return (bool)this.GetValue(IsGlassActiveProperty); }
		}

				#endregion //IsGlassActive

                // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region IsScenicTheme

         /// <summary>
        /// Identifies the <see cref="IsScenicTheme"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsScenicThemeProperty =
            RibbonWindowContentHost.IsScenicThemePropertyKey.DependencyProperty.AddOwner(typeof(XamRibbonWindow));

        /// <summary>
        /// Returns true if a 'Scenic' theme is being applied (read-only)
        /// </summary>
        /// <seealso cref="IsScenicThemeProperty"/>
        //[Description("Returns true if a 'Scenic' is being applied (read-only)")]
        //[Category("Behavior")]
        [ReadOnly(true)]
        [Browsable(false)]
        public bool IsScenicTheme
        {
            get
            {
                return (bool)this.GetValue(XamRibbonWindow.IsScenicThemeProperty);
            }
        }

                #endregion //IsScenicTheme
        
				#region ResizeGripStyleKey

		/// <summary>
		/// The key used to identify the style used for the <see cref="ResizeGrip"/> displayed in the window.
		/// </summary>
		/// <remarks>
        /// <para class="body">To style the ResizeGrip used in the bottom corner of the window place a Style in the window's Resources collection whose TargetType is set 
        /// to <see cref="System.Windows.Controls.Primitives.ResizeGrip"/> and whose key is set to this key, e.g. Key="{x:Static igRibbon:XamRibbonWindow.ResizeGripStyleKey}"</para>
		/// <para class="note"><b>Note:</b> the <see cref="System.Windows.Controls.Primitives.ResizeGrip"/> is only visible when the <see cref="ResizeMode"/> property is set to 'CanResizeWithGrip'.</para>
		/// </remarks>
		// AS 6/3/08 BR32772
		//public static readonly ResourceKey ResizeGripStyleKey = new StaticPropertyResourceKey(typeof(XamRibbonWindow), "ResizeGripStyleKey");
		public static readonly ResourceKey ResizeGripStyleKey = RibbonWindowContentHost.ResizeGripStyleKey;

				#endregion //ResizeGripStyleKey

                // JJD 4/30/10 - NA 2010 Volumne 2 - Scenic Riboon
				#region ScenicResizeGripStyleKey

		/// <summary>
		/// The key used to identify the style used for the <see cref="ResizeGrip"/> displayed in the window when the 'Scenic' theme is being used.
		/// </summary>
		/// <remarks>
        /// <para class="body">To style the ResizeGrip used in the bottom corner of the window place a Style in the window's Resources collection whose TargetType is set 
        /// to <see cref="System.Windows.Controls.Primitives.ResizeGrip"/> and whose key is set to this key, e.g. Key="{x:Static igRibbon:XamRibbonWindow.ScenicResizeGripStyleKey}"</para>
		/// <para class="note"><b>Note:</b> the <see cref="System.Windows.Controls.Primitives.ResizeGrip"/> is only visible when the <see cref="ResizeMode"/> property is set to 'CanResizeWithGrip'.</para>
		/// </remarks>
		public static readonly ResourceKey ScenicResizeGripStyleKey = RibbonWindowContentHost.ScenicResizeGripStyleKey;

				#endregion //ScenicResizeGripStyleKey

			#endregion //Public Properties	
 
			#region Private Properties

				// AS 8/6/12 TFS118105
				#region IsClassicOsTheme
		private bool IsClassicOsTheme
		{
			get
			{
				if ( this.IsGlassActiveInternal )
					return false;

				return NativeWindowMethods.IsClassicTheme;
			}
		} 
				#endregion //IsClassicOsTheme

                // AS 3/10/09 TFS5796
                #region LogicalBorderSize
        internal Size LogicalBorderSize
        {
            get
            {
                switch (this.ResizeMode)
                {
                    default:
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                    case ResizeMode.CanMinimize:
                        return new Size(SystemParameters.ResizeFrameVerticalBorderWidth, SystemParameters.ResizeFrameHorizontalBorderHeight);
                    case ResizeMode.NoResize:
                        return new Size(SystemParameters.FixedFrameVerticalBorderWidth, SystemParameters.FixedFrameHorizontalBorderHeight);
                }
            }
        } 
                #endregion //LogicalBorderSize

				// AS 7/17/09 TFS19447
				// We were using this within arrange only but we need the same information in the 
				// measure as well so I refactored it into a property.
				//
				#region NonClientSize
		private Size NonClientSize
		{
			get
			{
				if (this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip)
					return new Size(2 * SystemParameters.ResizeFrameVerticalBorderWidth, SystemParameters.ResizeFrameHorizontalBorderHeight);
				else
					return new Size(2 * SystemParameters.FixedFrameVerticalBorderWidth, SystemParameters.FixedFrameHorizontalBorderHeight);
			}
		}
				#endregion //NonClientSize

				#region OuterBrush

		internal static readonly DependencyProperty OuterBrushProperty = DependencyProperty.Register("OuterBrush",
			typeof(Brush), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush OuterBrush
		{
			get
			{
                return (Brush)this.GetValue(XamRibbonWindow.OuterBrushProperty);
			}
			set
			{
                this.SetValue(XamRibbonWindow.OuterBrushProperty, value);
			}
		}

				#endregion //OuterBrush

				#region OuterBrushActive

		internal static readonly DependencyProperty OuterBrushActiveProperty = DependencyProperty.Register("OuterBrushActive",
            typeof(Brush), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush OuterBrushActive
		{
			get
			{
                return (Brush)this.GetValue(XamRibbonWindow.OuterBrushActiveProperty);
			}
			set
			{
                this.SetValue(XamRibbonWindow.OuterBrushActiveProperty, value);
			}
		}

				#endregion //OuterBrushActive

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
				#region OuterShadowBrush

		internal static readonly DependencyProperty OuterShadowBrushProperty = DependencyProperty.Register("OuterShadowBrush",
			typeof(Brush), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush OuterShadowBrush
		{
			get
			{
                return (Brush)this.GetValue(XamRibbonWindow.OuterShadowBrushProperty);
			}
			set
			{
                this.SetValue(XamRibbonWindow.OuterShadowBrushProperty, value);
			}
		}

				#endregion //OuterShadowBrush

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
				#region OuterShadowBrushActive

		internal static readonly DependencyProperty OuterShadowBrushActiveProperty = DependencyProperty.Register("OuterShadowBrushActive",
            typeof(Brush), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

		internal Brush OuterShadowBrushActive
		{
			get
			{
                return (Brush)this.GetValue(XamRibbonWindow.OuterShadowBrushActiveProperty);
			}
			set
			{
                this.SetValue(XamRibbonWindow.OuterShadowBrushActiveProperty, value);
			}
		}

				#endregion //OuterShadowBrushActive

				// AS 12/3/09 TFS24545
				#region ResizeGripVisibility

		/// <summary>
		/// Identifies the <see cref="ResizeGripVisibility"/> dependency property
		/// </summary>
		private static readonly DependencyProperty ResizeGripVisibilityProperty = DependencyProperty.Register("ResizeGripVisibility",
			typeof(Visibility), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, new PropertyChangedCallback(OnResizeGripInfoChanged)));

		private static void OnResizeGripInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbonWindow window = ((XamRibbonWindow)d);
			window.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.DataBind,
				new Infragistics.Windows.Ribbon.XamRibbon.MethodInvoker(window.UpdateWindowBorders));
		}

		private Visibility ResizeGripVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamRibbonWindow.ResizeGripVisibilityProperty);
			}
		}

				#endregion //ResizeGripVisibility

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
				#region ResizeGripMargin

		/// <summary>
		/// Identifies the <see cref="ResizeGripMargin"/> dependency property
		/// </summary>
		private static readonly DependencyProperty ResizeGripMarginProperty = DependencyProperty.Register("ResizeGripMargin",
			typeof(Thickness), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(new Thickness(), new PropertyChangedCallback(OnResizeGripInfoChanged)));

		private Thickness ResizeGripMargin
		{
			get
			{
				return (Thickness)this.GetValue(XamRibbonWindow.ResizeGripMarginProperty);
			}
		}

				#endregion //ResizeGripMargin

				// AS 12/3/09 TFS24545
				#region ResizeGripWidth

		/// <summary>
		/// Identifies the <see cref="ResizeGripWidth"/> dependency property
		/// </summary>
		private static readonly DependencyProperty ResizeGripWidthProperty = DependencyProperty.Register("ResizeGripWidth",
			typeof(double), typeof(XamRibbonWindow), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnResizeGripInfoChanged)));

		private double ResizeGripWidth
		{
			get
			{
				return (double)this.GetValue(XamRibbonWindow.ResizeGripWidthProperty);
			}
		}

				#endregion //ResizeGripWidth

            #endregion //Private Properties	

            #region Internal Properties

				#region IsGlassActiveInternal

                internal bool IsGlassActiveInternal
                {
                    get
                    {
                        return this.cachedIsDWMCompositionEnabled;
                    }
                }

				#endregion //IsGlassActiveInternal

            #endregion //Internal Properties
    
		#endregion //Properties

		#region Methods

            #region Internal Methods

                // JJD 11/30/07 - BR27696
                // added content host reference
                #region InitializeContentHost

                internal void InitializeContentHost(RibbonWindowContentHost contentHost)
                {
                    // JJD 12/5/07 - BR28853
                    // clear out the content clip on the old content
                    if (this._contentHost != null &&
                        this._contentClip != null)
                    {
                        this._contentClip = null;
                        this._contentHost.ClearValue(ClipProperty);
                    }

                    this._contentHost = contentHost;

                    
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

                    
                    // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                    this.SynchronizeIsScenicTheme();

                    this.SynchronizeIsGlassActive();
                    this.ExtendGlassIntoClientArea();
                    this.SynchronizeCaptionInformation();
                }

                #endregion //InitializeContentHost

            #endregion //Internal Methods	
        
			#region Private Methods

				// AS 5/8/09 TFS16670
				// Refactored some code in the HookProc into a helper method to send a message to the window.
				// We have to do this because in WPF you can't call the base wndproc.
				//
				#region CallWndProc
		private IntPtr CallWndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam,
			ref bool handled, int excludedBits)
		{
			return CallWndProc(hwnd, message, wParam, lParam, ref handled, excludedBits, true, false);
		}

		// AS 8/6/12 TFS118105
		private IntPtr CallWndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam,
			ref bool handled, int excludedBits, bool ignoreAllMessages, bool onlyRestoreRemovedBits)
		{
			IntPtr oldStyle = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE);
			int removedBits; //AS 8/6/12 TFS118105
			IntPtr tempStyle = NativeWindowMethods.RemoveBits(oldStyle, excludedBits, out removedBits );

			if (tempStyle != oldStyle)
			{
				NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, tempStyle);
			}

			IntPtr result = this.CallWndProc(hwnd, message, wParam, lParam, ref handled,ignoreAllMessages );

			if (tempStyle != oldStyle)
			{
				// AS 8/6/12 TFS118105
				// When dragging the window from a maximized state to a normal state (i.e. at the end of the WM_NCLBUTTONDOWN
				// handling), the original style included the WS_MAXIMIZED bit but when released it did not (and should not 
				// have). However, because we reset the style to the original style that state was restored and the window 
				// was considered maximized again. To avoid making changes to the existing usage, I'll just readd the removed 
				// bits into the current state when told to and in the already existing cases we'll do what we did before.
				//
				//NativeWindowMethods.SetWindowLong( hwnd, NativeWindowMethods.GWL_STYLE, oldStyle );
				if ( !onlyRestoreRemovedBits )
					NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, oldStyle);
				else
				{
					var currentStyle = NativeWindowMethods.GetWindowLong( hwnd, NativeWindowMethods.GWL_STYLE );
					var currentStyleWithBits = NativeWindowMethods.AddBits( currentStyle, removedBits );
					NativeWindowMethods.SetWindowLong( hwnd, NativeWindowMethods.GWL_STYLE, currentStyleWithBits );
				}
			}

			return result;
		}

		private IntPtr CallWndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam,
			ref bool handled)
		{
			return CallWndProc(hwnd, message, wParam, lParam, ref handled, true);
		}

		// AS 8/6/12 TFS118105
		private IntPtr CallWndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam,
			ref bool handled, bool ignoreAllMessages)
		{
			bool wasInMessageLoop = this.isInMessageLoop;
			IntPtr result = IntPtr.Zero;

			try
			{
				// AS 8/6/12 TFS118105
				//this.isInMessageLoop = true;
				if ( !ignoreAllMessages )
					CallWndProcMessages.Add( message );
				else
					this.isInMessageLoop = true;

				result = NativeWindowMethods.SendMessageApi(hwnd, message, wParam, lParam);
			}
			finally
			{
				// AS 8/6/12 TFS118105
				//this.isInMessageLoop = wasInMessageLoop;
				if ( !ignoreAllMessages )
					CallWndProcMessages.Remove( message );
				else
					this.isInMessageLoop = wasInMessageLoop;
			}

			handled = true;
			return result;
		} 
				#endregion //CallWndProc

                #region ExtendGlassIntoClientArea

        // JJD 11/30/07 - BR27696
        // Made internal so we could call it from the content host
        //private void ExtendGlassIntoClientArea()
        internal void ExtendGlassIntoClientArea()
        {
            // JJD 11/30/07 - BR27696
            // check to make sure content host has been initialized
            //if (this.IsGlassActiveInternal)
            if (this.IsGlassActiveInternal && this._contentHost != null)
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;

				// AS 5/8/09 TFS16346
				if (hwnd == IntPtr.Zero)
					return;

                // Cache the original values in case the user disables glass while the
                // application is running.
                HwndTarget target = HwndSource.FromHwnd(hwnd).CompositionTarget;
                this.originalBackgroundColor = target.BackgroundColor;
                target.BackgroundColor = Colors.Transparent;

				// AS 6/29/12 TFS114953
				double extra = 0;

				if ( !_contentHost.IsScenicTheme && _contentHost.UseScenicApplicationMenuInternal )
					extra = Math.Ceiling((double)_contentHost.GetValue( RibbonWindowContentHost.TabHeaderHeightProperty ));

                // JJD 11/30/07 - BR27696
                // The CaptionHeightResolved proiperty was moved to the content host
                //int captionHeight = Utilities.ConvertFromLogicalPixels(this.CaptionHeightResolved);
				int captionHeight = Utilities.ConvertFromLogicalPixels(this._contentHost.CaptionHeightResolved + extra, this );
                NativeWindowMethods.MARGINS margins = new NativeWindowMethods.MARGINS(0, 0, captionHeight, 0);
                this.haveExtendedGlass = (0 == NativeWindowMethods.DwmExtendFrameIntoClientAreaApi(hwnd, ref margins));
                
                // If for whatever reason the glass couldn't be extended, reset the background values.
                if (!this.haveExtendedGlass)
                {
                    target.BackgroundColor = this.originalBackgroundColor;
                }
            }
        }

				#endregion //ExtendGlassIntoClientArea

				// AS 10/23/07 XamRibbonWindow IconResolved
				#region GetDefaultIcon
		private ImageSource GetDefaultIcon()
		{
			WindowInteropHelper helper = new WindowInteropHelper(this);
			return GetDefaultIcon(helper.Handle);
		}

		private static ImageSource GetDefaultIcon(IntPtr hwnd)
		{
			// AS 5/8/09 TFS16346
			if (hwnd == IntPtr.Zero)
				return null;

			IntPtr defaultHIcon = IntPtr.Zero;

			try
			{
				defaultHIcon = NativeWindowMethods.SendMessageApi(hwnd, NativeWindowMethods.WM_GETICON, new IntPtr(NativeWindowMethods.ICON_SMALL2), IntPtr.Zero);

				if (defaultHIcon == IntPtr.Zero)
					defaultHIcon = NativeWindowMethods.GetClassLongPtr(hwnd, NativeWindowMethods.GCL_HICONSM);

				if (defaultHIcon == IntPtr.Zero)
					defaultHIcon = NativeWindowMethods.LoadImageApi(IntPtr.Zero, NativeWindowMethods.IDI_APPLICATION, NativeWindowMethods.IMAGE_ICON, (int)SystemParameters.SmallIconWidth, (int)SystemParameters.SmallIconHeight, NativeWindowMethods.LR_SHARED);

				if (defaultHIcon != IntPtr.Zero)
				{
					BitmapSource imageSource = Imaging.CreateBitmapSourceFromHIcon(defaultHIcon, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)SystemParameters.SmallIconWidth, (int)SystemParameters.SmallIconHeight));

					BitmapFrame frame = BitmapFrame.Create(imageSource);

					return frame;
				}
			}
			catch
			{
			}

			return null;
		} 
				#endregion //GetDefaultIcon

				// AS 7/20/09 TFS19661
				#region EnforceMinMax
		private void EnforceMinMax(ref Size size)
		{
			Size minSize = new Size(this.MinWidth, this.MinHeight);
			Size maxSize = new Size(this.MaxWidth, this.MaxHeight);

			// take the os min/max size into account
			Size actualMinSize, actualMaxSize;

			if (_hasMinMax)
			{
				actualMinSize = new Size(Utilities.ConvertToLogicalPixels(_minTrackSizeX, this ), Utilities.ConvertToLogicalPixels(_minTrackSizeY, this ));
				actualMaxSize = new Size(Utilities.ConvertToLogicalPixels(_maxTrackSizeX, this ), Utilities.ConvertToLogicalPixels(_maxTrackSizeY, this ));
			}
			else
			{
				actualMinSize = new Size(SystemParameters.MinimumWindowTrackWidth, SystemParameters.MinimumWindowTrackHeight);
				actualMaxSize = new Size(SystemParameters.MaximumWindowTrackWidth, SystemParameters.MaximumWindowTrackHeight);
			}

			// use the larger of the os specified min and the user specified min
			actualMinSize.Width = Math.Max(minSize.Width, actualMinSize.Width);
			actualMinSize.Height = Math.Max(minSize.Height, actualMinSize.Height);

			// if a max has been set and its smaller than the os max then use that
			if (!double.IsPositiveInfinity(maxSize.Width) && maxSize.Width < actualMaxSize.Width)
				actualMaxSize.Width = maxSize.Width;
			if (!double.IsPositiveInfinity(maxSize.Height) && maxSize.Height < actualMaxSize.Height)
				actualMaxSize.Height = maxSize.Height;

			// make sure the max is at least as large as the min
			actualMaxSize.Width = Math.Max(actualMaxSize.Width, actualMinSize.Width);
			actualMaxSize.Height = Math.Max(actualMaxSize.Height, actualMinSize.Height);

			size.Width = Math.Max(actualMinSize.Width, Math.Min(actualMaxSize.Width, size.Width));
			size.Height = Math.Max(actualMinSize.Height, Math.Min(actualMaxSize.Height, size.Height));
		} 
				#endregion //EnforceMinMax

                #region HookProc

        private IntPtr HookProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {


#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			// JJD 11/30/07 - BR27696
            // get out if the content host is null
            // if (this.isInMessageLoop)
            if (this._contentHost == null || this.isInMessageLoop)
                return IntPtr.Zero;

			// AS 8/6/12 TFS118105
			if ( CallWndProcMessages.Contains( message ) )
				return IntPtr.Zero;

            switch (message)
            {
                #region WM_DWMCOMPOSITIONCHANGED

                // Handle the case where the user enables or disables glass while
                // the application is running.
                case NativeWindowMethods.WM_DWMCOMPOSITIONCHANGED:
                    // Reset our flag to the new value
                    this.cachedIsDWMCompositionEnabled = NativeWindowMethods.IsDWMCompositionEnabled;
					this.SetValue(IsGlassActivePropertyKey, this.cachedIsDWMCompositionEnabled);

					this.SynchronizeIsGlassActive();
                    
                    // JJD 06/02/10 - TFS33314
                    // Make sure everything is synced properly based on the new glass state
                    this.SynchronizeIsScenicTheme();
                    this.SynchronizeCaptionInformation();

                    this.UpdateWindowBorders();

                    if (this.IsGlassActiveInternal)
                    {
                        // AS 10/1/09 TFS22098
                        // When going from non-glass to glass we need to clear any region that we 
                        // applied to the window when it was in non-glass to provide the rounded edges.
                        //
                        UpdateWindowClipRegion(IntPtr.Zero, hwnd);

                        this.ExtendGlassIntoClientArea();

						// AS 6/22/11 TFS72532/TFS42892/TFS37239
						// When Aero is re-enabled the glass non-client area is rendered as black. The only 
						// way I have found to get around the issue is to hide and reshow the window. Originally
						// I tried using ShowWindow but that manipulates the z-index. Using SetWindowPos seems 
						// to avoid that issue.
						//
						if (Environment.OSVersion.Version >= new Version(6, 0))
						{
							this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new System.Threading.SendOrPostCallback(delegate(object param)
							{
								Window w = (Window)param;
								IntPtr handle = new WindowInteropHelper(w).Handle;

								if (handle != IntPtr.Zero && NativeWindowMethods.IsWindowVisible(handle))
								{
									int baseFlags = NativeWindowMethods.SWP_NOACTIVATE 
										| NativeWindowMethods.SWP_NOMOVE 
										| NativeWindowMethods.SWP_NOSIZE 
										| NativeWindowMethods.SWP_NOZORDER
										| NativeWindowMethods.SWP_NOOWNERZORDER;

									NativeWindowMethods.SetWindowPosApi(handle, IntPtr.Zero, 0, 0, 0, 0, baseFlags | NativeWindowMethods.SWP_HIDEWINDOW);
									NativeWindowMethods.SetWindowPosApi(handle, IntPtr.Zero, 0, 0, 0, 0, baseFlags | NativeWindowMethods.SWP_SHOWWINDOW);

									// when turning off the themes service and back on I sometimes saw a white line 
									// on the left edge and I think that was a remnant of our border rendering. 
									// invalidating the visual seems to address that
									w.InvalidateVisual();
								}
							}), this);
						}
                    }
                    else
                    {
                        // JJD 06/02/10 - TFS33314
                        // Call UpdateWindowClipRegion since when glass is not active
                        // we clip the window
                        this.UpdateWindowClipRegion();

                        this.RemoveGlassFromClientArea();
                    }

                    break;
                #endregion //WM_DWMCOMPOSITIONCHANGED

				// AS 7/20/09 TFS19661
				#region WM_GETMINMAXINFO
				case NativeWindowMethods.WM_GETMINMAXINFO:
					NativeWindowMethods.MINMAXINFO minMaxInfo = (NativeWindowMethods.MINMAXINFO)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.MINMAXINFO));

					_hasMinMax = true;
					_minTrackSizeX = minMaxInfo.ptMinTrackSize.X;
					_minTrackSizeY = minMaxInfo.ptMinTrackSize.Y;
					_maxTrackSizeX = minMaxInfo.ptMaxTrackSize.X;
					_maxTrackSizeY = minMaxInfo.ptMaxTrackSize.Y;
					break; 
				#endregion //WM_GETMINMAXINFO

                #region WM_NCACTIVATE

                case NativeWindowMethods.WM_NCACTIVATE:
                    if (!this.IsGlassActiveInternal)
                    {
						// AS 5/8/09 TFS16670
						// This isn't specific to this issue but I've been meaning to incorporate 
						// this for a while. Technically we should call the base and not eat the 
						// message. There is an undocumented way to prevent the title from getting 
						// repainted that office uses.
						//
                        //// Prevent the OS from rendering the inactive caption above our window.
                        //handled = true;
                        //return new IntPtr(1);
						return CallWndProc(hwnd, message, wParam, new IntPtr(-1), ref handled);
                    }
                    break;
                #endregion //WM_NCACTIVATE

                #region WM_NCCALCSIZE

                case NativeWindowMethods.WM_NCCALCSIZE:
                    if (wParam.ToInt64() == 1)
                    {
						// AS 9/18/09 TFS19281
						// Use the api state just in case the property hasn't been updated yet.
						//
						WindowState windowState = NativeWindowMethods.GetWindowState(this);

						NativeWindowMethods.NCCALCSIZE_PARAMS ncParams = (NativeWindowMethods.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.NCCALCSIZE_PARAMS));
                        if (this.IsGlassActiveInternal)
                        {
							int frameBorderWidth, frameBorderHeight;
							if (this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip)
							{
								frameBorderWidth = Utilities.ConvertFromLogicalPixels(SystemParameters.ResizeFrameVerticalBorderWidth, this );
								frameBorderHeight = Utilities.ConvertFromLogicalPixels(SystemParameters.ResizeFrameHorizontalBorderHeight, this );
							}
							else
							{
								frameBorderWidth = Utilities.ConvertFromLogicalPixels(SystemParameters.FixedFrameVerticalBorderWidth, this );
								frameBorderHeight = Utilities.ConvertFromLogicalPixels(SystemParameters.FixedFrameHorizontalBorderHeight, this );
							}

							// We need to allow space for the glass borders
                            ncParams.rectProposed.left += frameBorderWidth;
                            ncParams.rectProposed.right -= frameBorderWidth;
                            ncParams.rectProposed.bottom -= frameBorderHeight;

							// AS 9/18/09 TFS19281
                            //Marshal.StructureToPtr(ncParams, lParam, false);
                        }
						
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

						if (windowState == WindowState.Maximized)
						{
							NativeWindowMethods.AdjustMaximizedRect(ref ncParams.rectProposed);
						}
						Marshal.StructureToPtr(ncParams, lParam, false);
						handled = true;
                    }
                    break;
                #endregion //WM_NCCALCSIZE

                #region WM_NCHITTEST

                case NativeWindowMethods.WM_NCHITTEST:
                    // If we're on glass, we need to pass off the hit testing of the caption to the DWM so that
                    // it will render the button glow for us as well as properly detecting the oddly-sized buttons
					if (this.IsGlassActiveInternal && !_cachedIsModalWindowOpen )
                    {
                        int hitResult;
                        NativeWindowMethods.DwmDefWindowProc(hwnd, (uint)message, wParam, lParam, out hitResult);
                        if (hitResult == NativeWindowMethods.HTCLOSE ||
                            hitResult == NativeWindowMethods.HTMINBUTTON ||
                            hitResult == NativeWindowMethods.HTMAXBUTTON)
                        {
                            // We need to let the base implementation handle the message first, otherwise
                            // we will be able to hit-test the buttons correctly, but they won't actually
                            // do anything when clicking them.
                            this.isInMessageLoop = true;
                            NativeWindowMethods.SendMessageApi(hwnd, message, wParam, lParam);
                            this.isInMessageLoop = false;

                            handled = true;
                            return new IntPtr(hitResult);
                        }
                    }

                    // Calculate the point provided and convert it into client coordinates
                    // AS 10/3/07
					//int value = lParam.ToInt32();
                    //Point p = new Point((short)(value & 0xffff), (short)((value >> 16) & 0xffff));
					Point p = new Point(NativeWindowMethods.GetXFromLParam(lParam), NativeWindowMethods.GetYFromLParam(lParam));
                    p = this.PointFromScreen(p);

                    // Get the element at the specified point
                    IInputElement element = this.InputHitTest(p);

					FrameworkElement fe = null;

					if (element is DependencyObject)
					{
						fe = element as FrameworkElement;

						if ( fe == null )
							fe = Utilities.GetAncestorFromType(element as DependencyObject, typeof(FrameworkElement), true) as FrameworkElement;
					}

					// Check to see if the point is over a ResizeGrip
					if (fe != null)
					{
                        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                        // See in the element is inside the ScenicRibbonCaptionArea.
                        // If so, then use the ScenicRibbonCaptionArea as the fe to test
                        // because we do a compare on Name below which is looking for a 
                        // name that is assigned to the ScenicRibbonCaptionArea not one of
                        // its templated items
                        ScenicRibbonCaptionArea scenicCaptionArea = fe.TemplatedParent as ScenicRibbonCaptionArea;

                        if (scenicCaptionArea != null)
                            fe = scenicCaptionArea;

						ResizeGrip grip = fe as ResizeGrip;

						if (grip == null)
							grip = fe.TemplatedParent as ResizeGrip;

						// Make sure the ResizeGrip is from the Window's template
						if (grip != null && grip.TemplatedParent == this)
						{
							handled = true;

							// AS 12/7/07 RightToLeft
							if (grip.FlowDirection == FlowDirection.LeftToRight)
								return new IntPtr(NativeWindowMethods.HTBOTTOMRIGHT);
							else
								return new IntPtr(NativeWindowMethods.HTBOTTOMLEFT);
						}
					}

                    // We should only be doing hit testing for the borders when the ResizeMode of the window allows resizing.
                    if (this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip)
                    {
                        // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                        // Use system paramaters to get the logical border size
                        // double borderWidth = 4;
                        Size borderSize;

                        if (this.IsGlassActiveInternal)
                        {
                            // If glass is active, the coordinates that we get do not include the glass borders
                            // so mousing over, for example, the leftmost border would result in (-8,0).  Similarly,
                            // mousing over the right edge would yield, on a window width of 900, (908,0), so we don't
                            // want to include a width in our comparisons.
                            //borderWidth = 0;
                            borderSize = new Size();
                        }
                        else
                        {
                            // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                            // Use system paramaters to get the logical border size
                            borderSize = this.LogicalBorderSize;
                        }


                        // JJD 11/30/07 - BR27696
                        // Property moved to content host
                        //if (this.IsGlassActiveInternal == false || p.Y < this.MinCaptionHeight)
                        if (this.IsGlassActiveInternal == false || p.Y < this._contentHost.MinCaptionHeight)
                        {
                            // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                            //if (p.X < borderWidth)
                            if (p.X < borderSize.Width)
                            {
                                handled = true;
                                // JJD 11/30/07 - BR27696
                                // Property moved to content host
                                //if (p.Y < this.MinCaptionHeight)
                                if (p.Y < this._contentHost.MinCaptionHeight)
                                    return new IntPtr(NativeWindowMethods.HTTOPLEFT);

                                // JJD 11/30/07 - BR27696
                                // Property moved to content host
                                //if ( ((this.StatusBar == null || this.StatusBar.Visibility != Visibility.Visible) && p.Y > this.ActualHeight - 5) ||
                                //    (this.StatusBar != null && p.Y > this.ActualHeight - this.StatusBar.ActualHeight))
                                StatusBar statusBar = this._contentHost.StatusBar;
                                if ( ((statusBar == null || statusBar.Visibility != Visibility.Visible) && p.Y > this.ActualHeight - 5) ||
                                    (statusBar != null && p.Y > this.ActualHeight - statusBar.ActualHeight))
                                    return new IntPtr(NativeWindowMethods.HTBOTTOMLEFT);

                                return new IntPtr(NativeWindowMethods.HTLEFT);
                            }
                            // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                            //else if (p.X > this.ActualWidth - borderWidth)
                            else if (p.X > this.ActualWidth - borderSize.Width)
                            {
                                handled = true;
                                // JJD 11/30/07 - BR27696
                                // Property moved to content host
                                //if (p.Y < this.MinCaptionHeight)
                                if (p.Y < this._contentHost.MinCaptionHeight)
                                    return new IntPtr(NativeWindowMethods.HTTOPRIGHT);

                                // JJD 11/30/07 - BR27696
                                // Property moved to content host
                                //if ( ((this.StatusBar == null || this.StatusBar.Visibility != Visibility.Visible) && p.Y > this.ActualHeight - 5) ||
                                //    (this.StatusBar != null && p.Y > this.ActualHeight - this.StatusBar.ActualHeight))
                                //    return new IntPtr(NativeWindowMethods.HTBOTTOMRIGHT);
                                StatusBar statusBar = this._contentHost.StatusBar;
                                if ( ((statusBar == null || statusBar.Visibility != Visibility.Visible) && p.Y > this.ActualHeight - 5) ||
                                    (statusBar != null && p.Y > this.ActualHeight - statusBar.ActualHeight))
                                    return new IntPtr(NativeWindowMethods.HTBOTTOMRIGHT);

                                return new IntPtr(NativeWindowMethods.HTRIGHT);
                            }
                            // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                            //else if (p.Y > this.ActualHeight - 5)
                            else if (p.Y > this.ActualHeight - (1 + borderSize.Height))
                            {
                                handled = true;
                                return new IntPtr(NativeWindowMethods.HTBOTTOM);
                            }
							// JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
							//else if (p.Y < 4)
							else if (p.Y < borderSize.Height)
							{
                                handled = true;
                                return new IntPtr(NativeWindowMethods.HTTOP);
                            }
							// AS 10/18/10 TFS42779
							// We still need to use a border extent when using glass since there is 
							// no non-client area on top and therefore the call to the defwndproc 
							// below won't allow the top to be resized.
							//
							else if (this.IsGlassActiveInternal && p.Y < this.LogicalBorderSize.Height)
							{
								IntPtr result = CallWndProc(hwnd, message, wParam, lParam, ref handled);

								if (result.ToInt64() == NativeWindowMethods.HTCLIENT)
								{
									handled = true;
									return new IntPtr(NativeWindowMethods.HTTOP);
								}
							}
                        }
                    }

                    // Check the name of the element to see if we have any special-cases for hit testing
                    if (fe != null)
                    {
                        switch (fe.Name)
                        {
                            case "PART_XamRibbonCaption":
                            case "PART_RibbonCaptionPanel":
								// AS 10/23/07
								// Ignore a caption element of a ribbon that is not the ribbon
								// displayed as the caption area of the window.
								//
								// AS 10/24/07 AutoHide
								// The caption could be in either the ribbon or the window depending on whether the window
								// is showing its own caption so ensure that the templated parent of the caption is the
								// appropriate object.
								//
                                // JJD 11/30/07 - BR27696
                                //DependencyObject captionOwner = this.CaptionVisibility == Visibility.Visible ? (DependencyObject)this : (DependencyObject)this.Ribbon;
                                DependencyObject captionOwner = this._contentHost.CaptionVisibility == Visibility.Visible ? (DependencyObject)this._contentHost : (DependencyObject)this._contentHost.Ribbon;

								if (fe.TemplatedParent == captionOwner)
								{
									handled = true;
									return new IntPtr(NativeWindowMethods.HTCAPTION);
								}
								break;
							// AS 10/23/07 XamRibbonWindow IconResolved
							case "PART_WindowIcon":
                                // JJD 5/21/10 - NA 2010 Volume 2 - Scenic Ribbon support
                                // Added PART_WindowIcon to show the system menu with the scenic theme
								// AS 6/21/12 TFS114953
								// I don't think we need to restrict this to when we are using the scenic theme. If the xamRibbon is 
								// showing its caption and it has a control box then we should treat it as our control box so this 
								// does not need to be dependant upon whether this is scenic or not.
								//
                                //if ( fe.TemplatedParent == this._contentHost.Ribbon && this.IsScenicTheme)
								if ( fe.TemplatedParent == this._contentHost.Ribbon )
								{
									handled = true;
									return new IntPtr(NativeWindowMethods.HTSYSMENU);
								}

                                // JJD 11/30/07 - BR27696
                                //if (this.CaptionVisibility == Visibility.Visible && fe.TemplatedParent == this)
                                if (this._contentHost.CaptionVisibility == Visibility.Visible && fe.TemplatedParent == this._contentHost)
								{
									handled = true;
									return new IntPtr(NativeWindowMethods.HTSYSMENU);
								}
								break;
                        }
                    }

					// AS 9/21/09 TFS20247
					// On Windows 7, the default wndproc is returning HTSYSMENU when in the area 
					// where the control box would be. Since the application menu exists there we 
					// need to suppress that.
					//
					if (this.IsGlassActiveInternal)
					{
						IntPtr result = CallWndProc(hwnd, message, wParam, lParam, ref handled);

						if (result != IntPtr.Zero)
						{
							// JJD 6/13/11 - TFS78205
							// if CallWndProc returned HTCLOSE, HTMAXBUTTON or HTMINBUTTON
							// return HTCLIENT. Otherwise, it it possible to have tooltips for the buttons show
							// inerneath the buttons as well as having the OS draw the buttons in the non client area
							long hittest = result.ToInt64();
							//if (result.ToInt64() == NativeWindowMethods.HTSYSMENU)
							if (hittest == NativeWindowMethods.HTSYSMENU ||
								hittest == NativeWindowMethods.HTCLOSE ||
								hittest == NativeWindowMethods.HTMAXBUTTON ||
								hittest == NativeWindowMethods.HTMINBUTTON)
							{
								return new IntPtr(NativeWindowMethods.HTCLIENT);
							}
						}

						return result;
					}

                    break;
                #endregion //WM_NCHITTEST

                #region WM_NCLBUTTONDBLCLK

                case NativeWindowMethods.WM_NCLBUTTONDBLCLK:
                    {
                        if (wParam.ToInt32() == NativeWindowMethods.HTCAPTION &&
                            !(this.ResizeMode == ResizeMode.CanResizeWithGrip || this.ResizeMode == ResizeMode.CanResize))
                        {
                            handled = true;
                        }
                    }
                    break;

                #endregion //WM_NCLBUTTONDBLCLK

                #region WM_NCPAINT

                case NativeWindowMethods.WM_NCPAINT:
                    // We don't want to draw any non-client area when glass is disabled, but if
                    // glass is turned on, eating this message will cause no glass to be rendered.
                    //
                    // NOTE: We cannot use the IsGlassEnabled property that caches the result
                    // of IsDWMCompositionEnabled because when disabling glass (i.e. Vista Basic) and then
                    // re-enabling glass, we'll get a series of NCPAINT messages *before* getting the
                    // WM_DWMCOMPOSITIONCHANGED message, which results in the glass not being rendered
                    // on the form.
					if (!NativeWindowMethods.IsDWMCompositionEnabled)
						handled = true;

                    break;
                #endregion //WM_NCPAINT

				#region WM_NCRBUTTONUP
				case NativeWindowMethods.WM_NCRBUTTONUP:
					// AS 8/8/12 TFS118105
					if ( this.IsClassicOsTheme )
						return this.CallWndProc( hwnd, NativeWindowMethods.WM_GETSYSMENU, IntPtr.Zero, lParam, ref handled, NativeWindowMethods.WS_CAPTION, false, true );

					// AS 10/3/07
					// Show the system menu when right clicking in the non-client area. I'm using an undocumented
					// windows message but in theory we could also use TrackPopupMenuEx passing in the GetSystemMenu
					// results, getting back the command id and sending that to the window using sendmessage with 
					// a WM_SYSCOMMAND.
					//
					NativeWindowMethods.SendMessageApi(hwnd, NativeWindowMethods.WM_GETSYSMENU, IntPtr.Zero, lParam);
					handled = true;
					break;

				#endregion //WM_NCRBUTTONUP

                #region WM_NCUAHDRAWCAPTION and WM_NCUAHDRAWFRAME

                case NativeWindowMethods.WM_NCUAHDRAWCAPTION:
                case NativeWindowMethods.WM_NCUAHDRAWFRAME:
                    handled = true;
                    break;
                #endregion //WM_NCUAHDRAWCAPTION and WM_NCUAHDRAWFRAME

				#region WM_NC(L|R|M)BUTTONDOWN
				case NativeWindowMethods.WM_NCLBUTTONDOWN:
				case NativeWindowMethods.WM_NCRBUTTONDOWN:
				case NativeWindowMethods.WM_NCMBUTTONDOWN:
                    // JJD 11/30/07 - BR27696
                    XamRibbon ribbon = this._contentHost.Ribbon;

					// AS 10/22/07 BR27620
					if (null != ribbon && ribbon.Mode != RibbonMode.KeyTipsPending)
						ribbon.RestoreNormalMode();

					// AS 8/6/12 TFS118105
					if ( message == NativeWindowMethods.WM_NCLBUTTONDOWN && this.IsClassicOsTheme )
					{
						var ht = (int)wParam.ToInt64();

						switch ( ht )
						{
							case NativeWindowMethods.HTCAPTION:
								{
									var oldWindowState = NativeWindowMethods.GetWindowState( this );
									var result = this.CallWndProc( hwnd, message, wParam, lParam, ref handled, NativeWindowMethods.WS_CAPTION, false, true );

									var currentWindowState = NativeWindowMethods.GetWindowState( this );

									// AS 8/8/12 TFS118105
									// If the window is dragged into a maximized state it isn't filling the screen - 
									// possibly because of the window style change so we'll send a framechange 
									// notificaiton.
									//
									if ( currentWindowState != oldWindowState )
									{
										NativeWindowMethods.SetWindowPosApi(
											hwnd,
											IntPtr.Zero,
											0, 0, 0, 0,
											NativeWindowMethods.SWP_NOACTIVATE |
											NativeWindowMethods.SWP_NOMOVE |
											NativeWindowMethods.SWP_NOSIZE |
											NativeWindowMethods.SWP_NOZORDER |
											NativeWindowMethods.SWP_NOOWNERZORDER |
											NativeWindowMethods.SWP_FRAMECHANGED );
									}

									return result;
								}
							case NativeWindowMethods.HTSYSMENU:
								{
									// AS 8/8/12 TFS118105
									// If the window is maximized adn you click on the system menu then the OS draws the non-client area
									// so we'll strip the caption bit for this area too.
									//
									return this.CallWndProc( hwnd, message, wParam, lParam, ref handled, NativeWindowMethods.WS_CAPTION, false, true );
								}
							default:
								break;
						}
					}

					break; 
				#endregion //WM_NC(L|R|M)BUTTONDOWN

                case NativeWindowMethods.WM_WINDOWPOSCHANGING:
                case NativeWindowMethods.WM_WINDOWPOSCHANGED:




                    break;

                #region WM_MOVE
                case NativeWindowMethods.WM_MOVE:
                    // AS 10/10/08 TFS6236
                    // The Window class takes the client position passed into the WM_MOVE
                    // and adjusts it using AdjustWindowRectEx to calculate the new window
                    // top/left. However this includes the caption size but our wm_nccalcsize
                    // does not leave any nonclient space at the top so the window ends up
                    // storing a value that is higher by the amount of the assumed top non-client
                    // space.
                    //
                    int y = NativeWindowMethods.GetYFromLParam(lParam);
                    int x = NativeWindowMethods.GetXFromLParam(lParam);

                    int style = (int)NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE).ToInt64();
                    int styleEx = (int)NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_EX_STYLE).ToInt64();

                    // get the adjusted rect as the Window class will see it
                    NativeWindowMethods.RECT clientRect = new NativeWindowMethods.RECT();
                    NativeWindowMethods.GetClientRect(hwnd, ref clientRect);
                    NativeWindowMethods.RECT windowRect = clientRect;
                    NativeWindowMethods.AdjustWindowRectExApi(ref windowRect, style, false, styleEx);

                    // now get the actual non-client size
                    NativeWindowMethods.WINDOWINFO windowInfo = new NativeWindowMethods.WINDOWINFO();
                    NativeWindowMethods.GetWindowInfo(hwnd, ref windowInfo);

                    x += (clientRect.left - windowRect.left) - (windowInfo.rcClient.left - windowInfo.rcWindow.left);
                    y += (clientRect.top - windowRect.top) - (windowInfo.rcClient.top - windowInfo.rcWindow.top);

                    IntPtr newLParam = NativeWindowMethods.MakeLParam(x, y);

					
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

					return CallWndProc(hwnd, message, wParam, newLParam, ref handled);
                #endregion //WM_MOVE

				#region WM_SETTEXT
				case NativeWindowMethods.WM_SETTEXT:
				case NativeWindowMethods.WM_SETICON: // AS 10/1/09 TFS22098 - Not related but we should do this.
					// AS 5/8/09 TFS16670
					if (!this.IsGlassActiveInternal)
						return CallWndProc(hwnd, message, wParam, lParam, ref handled, NativeWindowMethods.WS_CAPTION);
					break;
				#endregion //WM_SETTEXT

				// AS 8/6/12 TFS118105
				#region WM_SETCURSOR
				case NativeWindowMethods.WM_SETCURSOR:
					if ( this.IsClassicOsTheme )
						return this.CallWndProc( hwnd, message, wParam, lParam, ref handled, NativeWindowMethods.WS_CAPTION, false, true );
					break;
				#endregion //WM_SETCURSOR
			}

            return IntPtr.Zero;
        }

		        #endregion //HookProc	
 
                // JJD 12/10/07
                #region Commented out unnecesary code

        //        #region InitializeWindowStyles

        //private void InitializeWindowStyles(IntPtr hwnd)
        //{
        //    IntPtr style = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE);
        //    IntPtr exStyle = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_EX_STYLE);

        //    NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, style);
        //    NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_EX_STYLE, exStyle);
        //}

        //        #endregion //InitializeWindowStyles	

                #endregion //Commented out unnecesary code	
    
				#region OnCanExecuteCommand

		private static void OnCanExecuteCommand(object target, CanExecuteRoutedEventArgs args)
		{
			XamRibbonWindow xamRibbonWindow = target as XamRibbonWindow;
			if (xamRibbonWindow != null)
			{
				if (args.Command == XamRibbonWindow.MaximizeCommand)
				{
					args.CanExecute = xamRibbonWindow.WindowState != WindowState.Maximized &&
                        // MBS 10/15/07 - BR27143
                        // The maximize button should be disabled when the ResizeMode is set
                        // to CanMinimize.  When it is set to NoResize, we will not even show
                        // the button.
                        xamRibbonWindow.ResizeMode != ResizeMode.CanMinimize;

					// AS 11/4/11 TFS91009
					if (args.CanExecute && NativeWindowMethods.IsWindowStyleRemoved(xamRibbonWindow, NativeWindowMethods.WS_MAXIMIZEBOX))
						args.CanExecute = false;
				}
				else if (args.Command == XamRibbonWindow.MinimizeCommand)
				{
					args.CanExecute = xamRibbonWindow.WindowState != WindowState.Minimized;

					// AS 11/4/11 TFS91009
					if (args.CanExecute && NativeWindowMethods.IsWindowStyleRemoved(xamRibbonWindow, NativeWindowMethods.WS_MINIMIZEBOX))
						args.CanExecute = false;
				}
				else if (args.Command == XamRibbonWindow.RestoreCommand)
				{
					args.CanExecute = xamRibbonWindow.WindowState != WindowState.Normal &&
                        // MBS 10/15/07 - BR27143
                        // The restore button should be disabled when the ResizeMode is set
                        // to CanMinimize.  When it is set to NoResize, we will not even show
                        // the button.
                        xamRibbonWindow.ResizeMode != ResizeMode.CanMinimize;

					// AS 11/4/11 TFS91009
					if (args.CanExecute && NativeWindowMethods.IsWindowStyleRemoved(xamRibbonWindow,
						xamRibbonWindow.WindowState == WindowState.Maximized ? NativeWindowMethods.WS_MAXIMIZEBOX : NativeWindowMethods.WS_MINIMIZEBOX))
					{
						args.CanExecute = false;
					}
				}
				else if (args.Command == XamRibbonWindow.CloseCommand)
				{
					// AS 11/4/11 TFS91009
					if (!NativeWindowMethods.PreventClose(xamRibbonWindow))
						args.CanExecute = true;
					else
						args.CanExecute = false;
				}
				else
					return;

				args.Handled = true;
			}
		}

				#endregion //OnCanExecuteCommand

				#region OnExecuteCommand

		private static void OnExecuteCommand(object target, ExecutedRoutedEventArgs args)
		{
			XamRibbonWindow xamRibbonWindow = target as XamRibbonWindow;
			if (xamRibbonWindow != null)
			{
				if (args.Command == XamRibbonWindow.MaximizeCommand)
				{
					xamRibbonWindow.WindowState = WindowState.Maximized;
					args.Handled				= true;
				}
				else
				if (args.Command == XamRibbonWindow.MinimizeCommand)
				{
					xamRibbonWindow.WindowState = WindowState.Minimized;
					args.Handled				= true;
				}
				else
				if (args.Command == XamRibbonWindow.RestoreCommand)
				{
					xamRibbonWindow.WindowState = WindowState.Normal;
					args.Handled				= true;
				}
				else
				if (args.Command == XamRibbonWindow.CloseCommand)
				{
					xamRibbonWindow.Close();
					args.Handled = true;
				}
				else
					return;
			}
		}

				#endregion //OnExecuteCommand

				// AS 10/23/07 XamRibbonWindow IconResolved
				#region OnIconChanged
		private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            XamRibbonWindow window = (XamRibbonWindow)d;

            // JJD 11/30/07 - BR27696
            // moved properties to content host
            if (window._contentHost != null)
            {
				// JJD 10/19/10 - TFS34809
				// Always get the window's icon by calling GetDefaultIcon which uses native window API's to get the correct 16x16 sized
				// image. This prevents us picking up a larger image if one exists in the icon file
				//window._contentHost.SetValue(RibbonWindowContentHost.IconResolvedPropertyKey, e.NewValue ?? window.GetDefaultIcon());
				window._contentHost.SetValue(RibbonWindowContentHost.IconResolvedPropertyKey, window.GetDefaultIcon());
            }
		} 
				#endregion //OnIconChanged

				// AS 6/22/11 TFS74670
				#region OnIsModalWindowOpenChanged
		// AS 6/22/12 TFS115111
		//private static void OnIsModalWindowOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		//{
		//    var window = d as XamRibbonWindow;
		//    window._cachedIsModalWindowOpen = (bool)e.NewValue;
		//
		//    var ribbon = window._contentHost != null ? window._contentHost.Ribbon : null;
		//
		//    if (null != ribbon)
		//        ribbon.CoerceValue(FrameworkElement.IsEnabledProperty);
		//}
				#endregion //OnIsModalWindowOpenChanged

				// AS 1/13/10 TFS25545
				#region OnWindowStateChanged
		private static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamRibbonWindow w = (XamRibbonWindow)d;

			if (!w.IsGlassActiveInternal && !WindowState.Minimized.Equals(e.NewValue))
				w.InvalidateVisual();

			// AS 8/19/11 TFS83576
			if (w._contentHost != null)
			{
				var ribbon = w._contentHost.Ribbon;

				// AS 10/26/11 TFS85015
				w._contentHost.CalculateMargins();

				if (null != ribbon)
					ribbon.OnWindowStateChanged();
			}
		} 
				#endregion //OnWindowStateChanged

                #region RefreshCaptionButtonAreaWidth

        internal void RefreshCaptionButtonAreaWidth(IntPtr windowHandle)
        {
            // JJD 11/30/07 - BR27696
            // moved properties to content host
            if (this._contentHost != null)
            {
                bool isGlassActive = this.IsGlassActiveInternal;
                if (isGlassActive)
                {
					// AS 5/8/09 TFS16346
					if (windowHandle == IntPtr.Zero)
						return;

                    NativeWindowMethods.TITLEBARINFOEX info = new NativeWindowMethods.TITLEBARINFOEX();
                    info.rgstate = new uint[NativeWindowMethods.TITLEBARINFOEX.CCHILDREN_TITLEBAR + 1];
                    info.rgrect = new NativeWindowMethods.RECT[NativeWindowMethods.TITLEBARINFOEX.CCHILDREN_TITLEBAR + 1];
                    info.cbSize = (uint)Marshal.SizeOf(info);
                    NativeWindowMethods.SendMessageApi(windowHandle, NativeWindowMethods.WM_GETTITLEBARINFOEX, IntPtr.Zero, ref info);

                    System.Drawing.Rectangle rectMin = info.rgrect[2].Rect;
                    System.Drawing.Rectangle rectMax = info.rgrect[3].Rect;
                    System.Drawing.Rectangle rectHelp = info.rgrect[4].Rect;
                    System.Drawing.Rectangle rectClose = info.rgrect[5].Rect;

                    // AS 3/10/09 TFS15153
                    // Some of the rects might be empty so we need to skip those.
                    //
                    //System.Drawing.Rectangle rectUnion = System.Drawing.Rectangle.Union(rectMin, System.Drawing.Rectangle.Union(rectClose, rectMax));
                    System.Drawing.Rectangle rectUnion = UnionRects(rectMin, rectMax, rectClose);
					double logicalWidth = Utilities.ConvertToLogicalPixels(rectUnion.Width, this );

                    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonAreaWidthPropertyKey, logicalWidth);
				}
                else if (System.Environment.OSVersion.Version.Major > 5)
                {
                    // On Vista Office will draw slightly larger caption buttons than are drawn by a normal window
                    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonAreaWidthPropertyKey, 96.0);
                }
                else
                {
                    // AS 11/2/07 CaptionHeight - WorkItem #562
                    // Note, I'm explicitly using the Height because there appears
                    // to be a bug in .net whereby the width is always 1 setting
                    // behind when its changed while the app is running
					// AS 6/25/12 TFS114953
                    //this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonAreaWidthPropertyKey, SystemParameters.WindowCaptionButtonHeight * 3);
                    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonAreaWidthPropertyKey, SystemParameters.WindowCaptionButtonWidth * 3);
                }
            }
        }

                #endregion //RefreshCaptionButtonAreaWidth

                #region RefreshCaptionButtonHeight

        internal void RefreshCaptionButtonHeight()
        {
            // JJD 11/30/07 - BR27696
            // moved properties to content host
            if (this._contentHost != null)
            {
                // AS 8/21/08 BR35778
                // We shouldn't be hard coding a value for the button height. Instead
                // get the system value from the content host.
                //
                //if (this.IsGlassActiveInternal == false &&
                //     System.Environment.OSVersion.Version.Major > 5)
                //{
                //    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonHeightPropertyKey, 22.0);
                //}
                if (this.IsGlassActiveInternal == false)
                {
                    double height = (double)this._contentHost.GetValue(RibbonWindowContentHost.SystemCaptionButtonHeightProperty);
                    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonHeightPropertyKey, height);
                }
                else
                    this._contentHost.ClearValue(RibbonWindowContentHost.CaptionButtonHeightPropertyKey);
            }
        }
                #endregion //RefreshCaptionButtonHeight

                #region RefreshCaptionButtonWidth

        internal void RefreshCaptionButtonWidth()
        {
            // JJD 11/30/07 - BR27696
            // moved properties to content host
            if (this._contentHost != null)
            {
                // AS 8/21/08 BR35778
                // We shouldn't be hard coding a value for the button height. Instead
                // get the system value from the content host.
                //
                //if (this.IsGlassActiveInternal == false &&
                //    System.Environment.OSVersion.Version.Major > 5)
                //{
                //    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonWidthPropertyKey, 32.0);
                //}
                //else
                //    this._contentHost.ClearValue(RibbonWindowContentHost.CaptionButtonWidthPropertyKey);
                if (this.IsGlassActiveInternal == false)
                {
					// AS 6/25/12 TFS114953
                    //double height = (double)this._contentHost.GetValue(RibbonWindowContentHost.SystemCaptionButtonHeightProperty);
                    double height = (double)this._contentHost.GetValue(RibbonWindowContentHost.SystemCaptionButtonWidthProperty);
                    this._contentHost.SetValue(RibbonWindowContentHost.CaptionButtonWidthPropertyKey, height);
                }
                else
                    this._contentHost.ClearValue(RibbonWindowContentHost.CaptionButtonWidthPropertyKey);
            }
        }
				#endregion //RefreshCaptionButtonWidth

                #region RefreshCaptionHeight

        // AS 8/21/08 BR35778
        // This appears to be a typo. We should not be setting the CaptionButton height
        // but the Caption height. And actually we shouldn't even be setting that since
        // the contenthost is binding its CaptionHeight property to the actual height
        // of the caption element within its template. I'm removing this method since
        // it doesn't appear to be needed.
        
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

                #endregion //RefreshCaptionHeight

                #region RemoveGlassFromClientArea

        private void RemoveGlassFromClientArea()
        {
            if (this.haveExtendedGlass)
			{
				IntPtr hwnd = new WindowInteropHelper(this).Handle;

				// AS 5/8/09 TFS16346
				if (hwnd != IntPtr.Zero)
				{
					HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = this.originalBackgroundColor;
					NativeWindowMethods.MARGINS margins = new NativeWindowMethods.MARGINS();
					this.haveExtendedGlass = (0 != NativeWindowMethods.DwmExtendFrameIntoClientAreaApi(hwnd, ref margins));
				}
				else
				{
					// AS 5/8/09 TFS16346
					this.haveExtendedGlass = false;
				}
			}
        }
        #endregion //RemoveGlassFromClientArea

                #region SendMessageWithoutBits

        private void SendMessageWithoutBits(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, int excludedBits)
        {
            IntPtr oldStyle = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE);
            IntPtr tempStyle = NativeWindowMethods.RemoveBits(oldStyle, excludedBits);
            if (tempStyle != oldStyle)
                NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, tempStyle);

            NativeWindowMethods.SendMessageApi(hwnd, message, wParam, lParam);

            if (tempStyle != oldStyle)
                NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, oldStyle);
        }
                #endregion //SendMessageWithoutBits

                #region SynchronizeCaptionInformation

        private void SynchronizeCaptionInformation()
        {
			// AS 10/23/07
			// We can show a caption even when there is no ribbon.
			//
            //XamRibbon ribbon = this.Ribbon;
            //if (ribbon != null)
			if (true)
            {
                IntPtr handle = IntPtr.Zero;
                if (this.IsGlassActiveInternal)                                    
                    handle = new WindowInteropHelper(this).Handle;                

                this.RefreshCaptionButtonAreaWidth(handle);
                // AS 8/21/08 BR35778
                //this.RefreshCaptionHeight();
                this.RefreshCaptionButtonHeight();
                this.RefreshCaptionButtonWidth();

				// AS 10/23/07 XamRibbonWindow IconResolved
				if (this._contentHost != null)
				{
					// JJD 10/19/10 - TFS34809
					// Always get the window's icon by calling GetDefaultIcon which uses native window API's to get the correct 16x16 sized
					// image. This prevents us picking up a larger image if one exists in the icon file
					//this._contentHost.SetValue(RibbonWindowContentHost.IconResolvedPropertyKey, this.Icon ?? this.GetDefaultIcon());
					this._contentHost.SetValue(RibbonWindowContentHost.IconResolvedPropertyKey, this.GetDefaultIcon());
				}
            }
        }
				#endregion //SynchronizeCaptionInformation

				#region SynchronizeIsGlassActive

        // JJD 11/30/07 - BR27696
        // Made internal so we could call it from the content host
        //private void SynchronizeIsGlassActive()
        internal void SynchronizeIsGlassActive()
		{
			bool isGlassActive = this.IsGlassActiveInternal;
			
			this.SetValue(IsGlassActivePropertyKey, isGlassActive);
		}

				#endregion //SynchronizeIsGlassActive	

                // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
				#region SynchronizeIsScenicTheme

        internal void SynchronizeIsScenicTheme()
		{
            bool isScenicRibbon = false;

            if (this._contentHost != null)
            {
                XamRibbon ribbon = this._contentHost.Ribbon;

                if (ribbon != null)
                    isScenicRibbon = ribbon.IsScenicThemeInternal;

                this._contentHost.IsScenicTheme = IsScenicTheme;
            }

            if (this._cachedIsScenicTheme.HasValue &&
                 this._cachedIsScenicTheme.Value == isScenicRibbon &&
                this._contentHost != null) 
                return;

            this._cachedIsScenicTheme = isScenicRibbon;

            if ( isScenicRibbon == true )
			    this.SetValue(RibbonWindowContentHost.IsScenicThemePropertyKey, KnownBoxes.TrueBox);
            else
                this.ClearValue(RibbonWindowContentHost.IsScenicThemePropertyKey);

            // JJD 12/5/07 - BR28853
            // initialize the dynmaic resource references for the outer border brush keys
            if (this._contentHost != null)
            {
                this._contentHost.IsScenicTheme = isScenicRibbon;

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
                // Use different brush keys for a scenic theme
                if (isScenicRibbon)
                {
                    // set the dynamic resource bindings for when the window is inactive
                    this.SetResourceReference(OuterBrushProperty, RibbonBrushKeys.ScenicWindowBorderOuterBrushKey);

                    // set the dynamic resource bindings for when the window is active
                    this.SetResourceReference(OuterBrushActiveProperty, RibbonBrushKeys.ScenicActiveWindowBorderOuterBrushKey);
                }
                else
                {
                    // set the dynamic resource bindings for when the window is inactive
                    this.SetResourceReference(OuterBrushProperty, RibbonBrushKeys.WindowBorderOuterBrushKey);

                    // set the dynamic resource bindings for when the window is active
                    this.SetResourceReference(OuterBrushActiveProperty, RibbonBrushKeys.ActiveWindowBorderOuterBrushKey);
                }

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
                // set the dynamic resource bindings for when the window is inactive
                this.SetResourceReference(OuterShadowBrushProperty, RibbonBrushKeys.ScenicWindowBorderOuterShadowBrushKey);

                // set the dynamic resource bindings for when the window is active
                this.SetResourceReference(OuterShadowBrushActiveProperty, RibbonBrushKeys.ScenicActiveWindowBorderOuterShadowBrushKey);
            }

            // JJD 5/25/10 
            // The size of the top bordr goes from 1 to 2 in the scenic theme
            this.UpdateWindowBorders();
        }

				#endregion //SynchronizeIsScenicTheme	

                // AS 3/10/09 TFS15153
                #region UnionRects
        private static System.Drawing.Rectangle UnionRects(params System.Drawing.Rectangle[] rects)
        {
            System.Drawing.Rectangle btnRect = System.Drawing.Rectangle.Empty;

            foreach (System.Drawing.Rectangle rect in rects)
            {
                if (rect.IsEmpty)
                    continue;

                if (btnRect.IsEmpty)
                    btnRect = rect;
                else
                    btnRect = System.Drawing.Rectangle.Union(rect, btnRect);
            }

            return btnRect;
        } 
                #endregion //UnionRects

                #region UpdateWindowBorders

        private void UpdateWindowBorders()
        {
            // JJD 12/5/07 - BR28853
            // The window borders are now on the content Host
            // Check to make sure we have a content host
            if (this._contentHost != null)
            {
				// AS 12/2/09 TFS24267
				// We need to expose the amount of spacing for the borders so the statusbar
				// can bring its content in by that much so its not clipped when the window 
				// is maximized.
				//
				Thickness statusBarPadding = new Thickness();

				// AS 12/3/09 TFS24545
				if (this.ResizeGripVisibility != Visibility.Collapsed && !double.IsNaN(this.ResizeGripWidth))
					statusBarPadding.Right = this.ResizeGripWidth;

                if (this.IsGlassActiveInternal)
                {
                    // JJD 12/5/07 - BR28853
                    // The window borders are now on the content Host
                    //this.BorderThickness = new Thickness();
                    this._contentHost.BorderThickness = new Thickness();
                }
                else
                {
                    // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                    // Use system info for the size of the border
                    //int borderWidth = 4;
                    //int borderHeight = 4;
                    Size borderSize = this.LogicalBorderSize;

                    // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                    // Scenic themes have a larger border above the ribbon
                    double topBorderHeight = this.IsScenicTheme ? 2 : 1;

                    // JJD 12/5/07 - BR28853
                    // The window borders are now on the content Host
                    //this.BorderThickness = new Thickness(borderWidth, 1, borderWidth, borderHeight);
                    // JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
                    //this._contentHost.BorderThickness = new Thickness(borderWidth, 1, borderWidth, borderHeight);
                    this._contentHost.BorderThickness = new Thickness(borderSize.Width, topBorderHeight, borderSize.Width, borderSize.Height);

					// AS 6/28/12 TFS114953
					// The status bar will not be over the border in Office 2010.
					//
					if (!_contentHost.UseScenicApplicationMenuInternal)
					{
						// AS 12/2/09 TFS24267
						// JJD 05/17/10 - NA 2010 volume 2 - Scenic Ribbon support
						//statusBarPadding.Left += borderWidth;
						//statusBarPadding.Right += borderWidth;
						statusBarPadding.Left += borderSize.Width;
						statusBarPadding.Right += borderSize.Width;
					}
                }

				// AS 12/2/09 TFS24267
				_contentHost.SetValue(RibbonWindowContentHost.StatusBarPaddingPropertyKey, statusBarPadding);

                // JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                // Have the content host re-calculate its margin properties
                _contentHost.CalculateMargins();

                // JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                // Sync up the resize grip margin with the status bar's margin
                Thickness statusBarAreaMargin = _contentHost.StatusBarAreaMargin;
                this.SetValue(ResizeGripMarginProperty, new Thickness(0, 0, statusBarAreaMargin.Right, statusBarAreaMargin.Bottom));

                if (_contentHost.Ribbon != null)
                    _contentHost.Ribbon.CalculateMargins();
            }

        }

                #endregion //UpdateWindowBorders

                #region UpdateWindowClipRegion

        // JJD 11/30/07 - BR27696
        // Made internal so we could call it from the content host
        //private void UpdateWindowClipRegion()
        internal void UpdateWindowClipRegion()
        {
            // JJD 11/30/07 - BR27696
            // moved properties to content host
            if (this._contentHost != null)
            {
                if (this.IsGlassActiveInternal == false)
                {
					int width = Utilities.ConvertFromLogicalPixels(this.ActualWidth, this ) + 1;
					int height = Utilities.ConvertFromLogicalPixels(this.ActualHeight, this ) + 1;

                    StatusBar statusBar = this._contentHost.StatusBar;

                    // AS 3/10/09
                    // Make sure we get the current window state. The property may not have 
                    // been updated yet.
                    //
                    WindowState windowState = NativeWindowMethods.GetWindowState(this);

                    // AS 3/10/09 TFS5796
                    // When maximized we need to clip out the borders.
                    //
                    if (windowState == WindowState.Maximized)
                    {
                        Size borderSize = this.LogicalBorderSize;
						int borderWidth = Utilities.ConvertFromLogicalPixels(borderSize.Width, this );
						int borderHeight = Utilities.ConvertFromLogicalPixels(borderSize.Height, this );
						int actualWidth = Utilities.ConvertFromLogicalPixels(this.ActualWidth, this );
						int actualHeight = Utilities.ConvertFromLogicalPixels(this.ActualHeight, this );

                        IntPtr rectRegion = NativeWindowMethods.CreateRectRegionApi(borderWidth, borderHeight, actualWidth - borderWidth, actualHeight - borderHeight);
                        this.UpdateWindowClipRegion(rectRegion);
                    }
                    else
                    // JJD 05/25/10 - NA 2010 volume 2 - Scenic Ribbon support
                    // If this is scenic o n classic OS then none of the window corners are rounded
                    if (this._cachedIsScenicTheme == true && this._contentHost.IsClassicOSTheme)
                    {
                        this.UpdateWindowClipRegion(NativeWindowMethods.CreateRectRegionApi(0, 0, width, height));
                    }
                    else
                    if (statusBar != null && statusBar.Visibility == Visibility.Visible)
                    {
                        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                        // In a scenic theme the lower corners of the window are square 
                        //  IntPtr roundedRegion = NativeWindowMethods.CreateRoundRectRgnApi(0, 0, width, height, WINDOW_CORNER_RADIUS, WINDOW_CORNER_RADIUS);
                        IntPtr rgn;
                        if (this._cachedIsScenicTheme == false)
                            rgn = NativeWindowMethods.CreateRoundRectRgnApi(0, 0, width, height, WINDOW_CORNER_RADIUS, WINDOW_CORNER_RADIUS);
                        else
                        {
                            //  create a roundedrect rgn for the top
                            IntPtr topRegion = NativeWindowMethods.CreateRoundRectRgnApi(0, 0, width, height, SCENIC_WINDOW_TOP_CORNER_RADIUS, SCENIC_WINDOW_TOP_CORNER_RADIUS);

                            int bottomRgnOffsetY = Math.Max(height - SCENIC_WINDOW_TOP_CORNER_RADIUS, 0);

                            //  create a rgn for the botom
                            //IntPtr bottomRegion = NativeWindowMethods.CreateRectRegionApi(0, topRegionHeight, width, Math.Max(height - topRegionHeight, 1));
                            IntPtr bottomRegion = NativeWindowMethods.CreateRectRegionApi(0, bottomRgnOffsetY, width, height);

                            // Combine the two regions so that we have rounded top corners but square bottom corners
                            rgn = NativeWindowMethods.CreateRectRegionApi(0, 0, 0, 0);
                            int result = NativeWindowMethods.CombineRgnApi(rgn, topRegion, bottomRegion, NativeWindowMethods.RegionCombineMode.RGN_OR);

                            NativeWindowMethods.DeleteObject(topRegion);
                            NativeWindowMethods.DeleteObject(bottomRegion);
                        }
                       
                        this.UpdateWindowClipRegion(rgn);
                    }
                    else
                    {
						int captionHeight = Utilities.ConvertFromLogicalPixels(this._contentHost.CaptionHeightResolved, this );

                        int cornerRadius;

                        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                        // In a scenic theme the corner radius of the top corners are samller
                        if (this._cachedIsScenicTheme == false)
                            cornerRadius = WINDOW_CORNER_RADIUS;
                        else
                            cornerRadius = SCENIC_WINDOW_TOP_CORNER_RADIUS;

                        // We need to create the correct region for the top corners
                        
                        IntPtr upperRegion = NativeWindowMethods.CreateRoundRectRgnApi(0, 0, width, captionHeight, cornerRadius, cornerRadius);

                        // MBS 5/8/08 - BR32497
                        // When a SizeToContent property is set, we need to subtract a pixel from the width or height as appropriate because
                        // the sizing does not take into account the fact that we'll draw the border.  It seems like it would be safe to 
                        // subtract 1px from the width and height regardless of checking the SizeToContent property, but in the interest 
                        // of a restrictive fix we'll check it anyway
                        //                       
                        //IntPtr lowerRegion = NativeWindowMethods.CreateRectRegionApi(0, captionHeight - WINDOW_CORNER_RADIUS, width, height);
                        int widthAdjustment = 0;
                        int heightAdjustment = 0;
                        switch (this.SizeToContent)
                        {
                            case SizeToContent.Height:
                                heightAdjustment = 1;
                                break;

                            case SizeToContent.Width:
                                widthAdjustment = 1;
                                break;

                            case SizeToContent.WidthAndHeight:
                                widthAdjustment = heightAdjustment = 1;
                                break;
                        }
                        //
                        
                        
                        
                        IntPtr lowerRegion = NativeWindowMethods.CreateRectRegionApi(0, captionHeight - cornerRadius, width - widthAdjustment, height - heightAdjustment);

                        // Combine the two regions so that we have rounded top corners but normal bottom corners
                        IntPtr combinedRegion = NativeWindowMethods.CreateRectRegionApi(0, 0, 0, 0);
                        int result = NativeWindowMethods.CombineRgnApi(combinedRegion, upperRegion, lowerRegion, NativeWindowMethods.RegionCombineMode.RGN_OR);

                        // AS 3/10/09
                        // The window will handle deleting the region we give it but we should have 
                        // deleted the source regions.
                        //
                        NativeWindowMethods.DeleteObject(lowerRegion);
                        NativeWindowMethods.DeleteObject(upperRegion);

                        this.UpdateWindowClipRegion(combinedRegion);
                    }

                    // JJD 12/10/07 - BR28958
                    // Create and set a clipping region on our content so it doesn't overlay our borders 
                    Rect rect = new Rect(this.RenderSize);

                    if (rect.Width > 4 && rect.Height > 4 &&
                        // AS 3/10/09
                        //this.WindowState == WindowState.Normal)
                        windowState == WindowState.Normal)
                    {
                        rect.Inflate(-1, -1);

                        // need to bring in the round radius so that it matched up with the window region
                        // as close as possible
                        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                        // In a scenic theme the lower corners of the window are square so we don't need to do anything
                        if (this._cachedIsScenicTheme == false)
                            this._contentClip = new RectangleGeometry(rect, WINDOW_CORNER_RADIUS - 4, WINDOW_CORNER_RADIUS - 4);
                        else
                            this._contentClip = new RectangleGeometry(rect);

                        // If a statusbar is not shown then union in a rect at the bottom
                        // so the window is not rounded on the bottom.
                        if (this._contentHost.StatusBarVisibility != Visibility.Visible)
                        {
                            Rect bottomRect = new Rect();

							// AS 1/13/10 TFS25545
							// It looks like we added 1 to fix the right edge but the right
							// was incorrect because we didn't offset the X. This caused the 
							// black line on the bottom left edge.
							//
							//bottomRect.Width = rect.Width + 1;
							//bottomRect.Height = Math.Max(4.0, rect.Height / 2);
							bottomRect.Width = rect.Width;
							bottomRect.X = rect.X;
                            bottomRect.Height = Math.Ceiling(Math.Max(4.0, rect.Height / 2 ));

                            bottomRect.Y = rect.Bottom - bottomRect.Height;
                            this._contentClip = new CombinedGeometry( GeometryCombineMode.Union, this._contentClip, new RectangleGeometry(bottomRect));
                        }

                        // JJD 4/29/10 - Optimization
                        // Freeze the clip geometry so the framework doesn't need to listen for changes
                        this._contentClip.Freeze();

                        this._contentHost.Clip = this._contentClip;

                    }
                    else
                    {
                        // JJD 12/10/07 - BR28958
                        // clear out the content clip
                        this._contentClip = null;
                        this._contentHost.ClearValue(ClipProperty);
                    }

					// AS 1/13/10 TFS25545
					// We're updating the clip from the rendersize changed - i.e. after the 
					// initial render so we need to rerender when the clip changes.
					//
					this.InvalidateVisual();
				}
                else
                {
                    // JJD 12/10/07 - BR28958
                    // clear out the content clip
                    this._contentClip = null;
                    this._contentHost.ClearValue(ClipProperty);
                }
            }
        }

        private void UpdateWindowClipRegion(IntPtr newRegion)
        {
            if (newRegion != IntPtr.Zero)
            {
                WindowInteropHelper windowHelper = new WindowInteropHelper(this);
                IntPtr hwnd = windowHelper.Handle;

				// AS 10/1/09 TFS22098
				// Moved to a helper method where we know we can accept an empty 
				// region. We need to be able to clear the region when going from 
				// non-glass to glass.
				//
				//// AS 5/8/09 TFS16346
				//if (hwnd == IntPtr.Zero)
				//    return;
				//
				//NativeWindowMethods.SetWindowRgnApi(hwnd, newRegion, true);
				UpdateWindowClipRegion(newRegion, hwnd);
            }
        }

		// AS 10/1/09 TFS22098
		// Moved here from the UpdateWindowClipRegion(IntPtr) overload. For this 
		// overload we're going to accept an empty region. I also changed this 
		// so we pass false should the window not be visible.
		//
		private static void UpdateWindowClipRegion(IntPtr newRegion, IntPtr hwnd)
		{
			// AS 5/8/09 TFS16346
			if (hwnd == IntPtr.Zero)
				return;

			NativeWindowMethods.SetWindowRgnApi(hwnd, newRegion, NativeWindowMethods.IsWindowVisible(hwnd));
		}
        #endregion //UpdateWindowClipRegion

			#endregion //Private Methods	
    
		#endregion //Methods

		// AS 6/3/08 BR32772
		#region IRibbonWindow Members

        // JJD 05/27/10 - NA 2010 volume 2 - Scenic Ribbon support
        RibbonWindowContentHost IRibbonWindow.ContentHost
		{
			get { return this._contentHost; }
		}

		void IRibbonWindow.InitializeContentHost(RibbonWindowContentHost contentHost)
		{
			this.InitializeContentHost(contentHost);
		}

		void IRibbonWindow.ExtendGlassIntoClientArea()
		{
			this.ExtendGlassIntoClientArea();
		}

		bool IRibbonWindow.IsGlassActiveInternal
		{
			get { return this.IsGlassActiveInternal; }
		}

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        bool IRibbonWindow.IsScenicThemeInternal
		{
			get { return this._cachedIsScenicTheme.HasValue ? this._cachedIsScenicTheme.Value : false; }
		}

        // JJD 05/25/10 - NA 2010 volume 2 - Scenic Ribbon support
        bool IRibbonWindow.IsClassicOSThemeInternal
		{
			get { return this._contentHost != null ? this._contentHost.IsClassicOSTheme : false; }
		}

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        Brush IRibbonWindow.OuterBorderBrush
		{
			get { return this.IsActive ? this.OuterBrushActive : this.OuterBrush; }
		}

        // JJD 05/25/10 - NA 2010 volume 2 - Scenic Ribbon support
        Brush IRibbonWindow.OuterShadowBrush
		{
			get { return this.IsActive ? this.OuterShadowBrushActive : this.OuterShadowBrush; }
		}

		Window IRibbonWindow.Window
		{
			get { return this; }
		}

		void IRibbonWindow.RefreshCaptionButtonAreaWidth(IntPtr handle)
		{
			this.RefreshCaptionButtonAreaWidth(handle);
		}

		void IRibbonWindow.SynchronizeIsGlassActive()
		{
			this.SynchronizeIsGlassActive();
		}

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        void IRibbonWindow.SynchronizeIsScenicTheme()
		{
			this.SynchronizeIsScenicTheme();
		}

		void IRibbonWindow.UpdateWindowClipRegion()
		{
			this.UpdateWindowClipRegion();
		}

		// AS 6/22/11 TFS74670
		bool IRibbonWindow.HasModalDialogWindow
		{
			get { return _cachedIsModalWindowOpen; }
		}
		#endregion //IRibbonWindow Members
	}

	// AS 6/3/08 BR32772
	// We cannot reference a derived Window from within any type or within the xaml.
	//
	internal interface IRibbonWindow
	{
		#region Properties
        
        // JJD 05/27/10 - NA 2010 volume 2 - Scenic Ribbon support
        RibbonWindowContentHost ContentHost { get; }

		bool IsActive { get; }
		bool IsGlassActiveInternal { get; }

		// AS 6/22/11 TFS74670
		bool HasModalDialogWindow { get; }

        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
        bool IsScenicThemeInternal { get; }
        bool IsClassicOSThemeInternal { get; }
        Brush OuterBorderBrush { get; }
        Brush OuterShadowBrush { get; }

		WindowState WindowState { get; }
		System.Windows.Threading.Dispatcher Dispatcher { get; }
		Window Window { get; } 

		#endregion //Properties

		#region Events

		event EventHandler Activated;
		event EventHandler Deactivated;

		#endregion //Events

		#region Methods

		void InvalidateVisual();
		void InitializeContentHost(RibbonWindowContentHost contentHost);
		void ExtendGlassIntoClientArea();
		void RefreshCaptionButtonAreaWidth(IntPtr handle);
		void SynchronizeIsGlassActive();
        
        // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
		void SynchronizeIsScenicTheme();
		
        void UpdateWindowClipRegion(); 

		#endregion //Methods
	}

	/// <summary>
	/// Defines the commands used by the <see cref="XamRibbonWindow"/>
	/// </summary>
	public static class RibbonWindowCommands
	{
		#region Constructor
		static RibbonWindowCommands()
		{
			RibbonWindowCommands.MaximizeCommand = new RoutedCommand("Maximize", typeof(RibbonWindowCommands));
			RibbonWindowCommands.MinimizeCommand = new RoutedCommand("Minimize", typeof(RibbonWindowCommands));
			RibbonWindowCommands.RestoreCommand = new RoutedCommand("Restore", typeof(RibbonWindowCommands));
			RibbonWindowCommands.CloseCommand = new RoutedCommand("Close", typeof(RibbonWindowCommands));
		} 
		#endregion //Constructor

		#region Commands

		/// <summary>
		/// Maximizes the <see cref="XamRibbonWindow"/>.
		/// </summary>
		public static readonly RoutedCommand MaximizeCommand;

		/// <summary>
		/// Minimizes the <see cref="XamRibbonWindow"/>.
		/// </summary>
		public static readonly RoutedCommand MinimizeCommand;

		/// <summary>
		/// Restores the <see cref="XamRibbonWindow"/> to its normal size.
		/// </summary>
		public static readonly RoutedCommand RestoreCommand;

		/// <summary>
		/// Closes the <see cref="XamRibbonWindow"/>.
		/// </summary>
		public static readonly RoutedCommand CloseCommand; 

		#endregion //Commands
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