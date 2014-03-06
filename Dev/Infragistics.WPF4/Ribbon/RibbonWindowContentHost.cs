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
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
    /// <summary>
	/// A custom <see cref="ContentControl"/> designed to be used as the <see cref="ContentControl.Content"/> of a <see cref="XamRibbonWindow"/> in order to display a <see cref="XamRibbon"/>, a 
	/// <see cref="System.Windows.Controls.Primitives.StatusBar"/> and the actual content for the window.
    /// </summary>
	/// <remarks>
	/// <p class="body">The RibbonWindowContentHost class is designed to be used as the content of the <see cref="XamRibbonWindow"/> in rich client applications to 
	/// enable the placement of a <see cref="XamRibbon"/> in the caption area (i.e., non-client area) of the window. When the <b>XamRibbon</b> is used in a standard 
	/// <see cref="System.Windows.Window"/> or on a <see cref="System.Windows.Controls.Page"/>, it appears inside the bounds of the window�s or page�s client area.</p>
	/// <p class="body">The RibbonWindowContentHost exposes 3 main properties - <see cref="Ribbon"/>, <see cref="StatusBar"/> and <see cref="ContentControl.Content"/>. The 
	/// <b>Ribbon</b> property should be set to an instance of a <see cref="XamRibbon"/> that will be displayed within the caption area of the containing <b>XamRibbonWindow</b>. 
	/// The <b>StatusBar</b> may optionally be set to a <see cref="System.Windows.Controls.Primitives.StatusBar"/> that will be displayed at the bottom of the 
	/// content. The <b>Content</b> property is used to specify the control or element that represents the client area of the window. This property is normally set to a 
	/// derived <see cref="Panel"/> that then contains the controls that make up the main client area of the window.</p>
	/// </remarks>
    /// <seealso cref="XamRibbonWindow"/>
    /// <seealso cref="XamRibbon"/>
	/// <seealso cref="Ribbon"/>
	/// <seealso cref="StatusBar"/>
	/// <seealso cref="ContentControl.Content"/>
	// AS 10/23/07
    [TemplatePart(Name = "PART_XamRibbonCaption", Type = typeof(FrameworkElement))]
    // AS 10/23/07 XamRibbonWindow IconResolved
    [TemplatePart(Name = "PART_WindowIcon", Type = typeof(FrameworkElement))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RibbonWindowContentHost : ContentControl
    {
        #region Private Members

        private ArrayList _logicalChildren = new ArrayList(3);
        private ResourceDictionary _statusBarResources;
		// AS 6/3/08 BR32772
		//private XamRibbonWindow _ribbonWindow;
        private IRibbonWindow _ribbonWindow;

        #endregion //Private Members	

        #region Constructors

        /// <summary>
		/// Initializes a new instance of <see cref="RibbonWindowContentHost"/>
		/// </summary>
		public RibbonWindowContentHost()
		{
			// AS 7/13/09 TFS18489
			// The default template for the RibbonWindowContentHost was using the WPF Grid's SharedSizeGroup support to 
			// ensure that the height of 2 of the row definitions within the template were the same within the Ribbon
			// WindowBorder as in the other grid where the ribbon and status bar are positioned.
			// 
			this.SetValue(ContentHostPropertyKey, this);

            // JJD 4/30/10 - NA 2010 Volumne 2 - Scenic Riboon
            // set the dynamic resource bindings so we know if the scenic ribbon is being used
            this.SetResourceReference(IsClassicOSThemeProperty, new ComponentResourceKey(typeof(XamRibbon), "IsClassicOSTheme_Identifier"));

			// AS 6/28/12 TFS114953
			this.SetResourceReference( UseScenicApplicationMenuInternalProperty, RibbonWindowContentHost.UsesScenicApplicationMenuKey );
		}

		static RibbonWindowContentHost()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(typeof(RibbonWindowContentHost)));

            double captionButtonWidth = 0;
            double captionButtonAreaWidth = 0;

            try
            {
                captionButtonWidth      = SystemParameters.WindowCaptionButtonWidth;
                captionButtonAreaWidth  = 3 * captionButtonWidth;
            }
            catch(SecurityException)
            {
            }

            CaptionButtonAreaWidthPropertyKey = DependencyProperty.RegisterReadOnly("CaptionButtonAreaWidth",
                typeof(double), typeof(RibbonWindowContentHost),
                new FrameworkPropertyMetadata(captionButtonAreaWidth));
            CaptionButtonAreaWidthProperty = CaptionButtonAreaWidthPropertyKey.DependencyProperty;

            CaptionButtonWidthPropertyKey =
                DependencyProperty.RegisterReadOnly("CaptionButtonWidth",
                typeof(double), typeof(RibbonWindowContentHost), 
                new FrameworkPropertyMetadata(captionButtonWidth));
            CaptionButtonWidthProperty = CaptionButtonWidthPropertyKey.DependencyProperty;

        }

		#endregion //Constructors	
    
		#region Base class overrides

			#region LogicalChildren

			/// <summary>
			/// Returns an enumerator of the logical children
			/// </summary>
			protected override IEnumerator LogicalChildren
			{
				get
				{
					return new MultiSourceEnumerator(new IEnumerator[] { base.LogicalChildren, this._logicalChildren.GetEnumerator() });
				}
			}

			#endregion //LogicalChildren	

			#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the element has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// AS 11/2/07 CaptionHeight - WorkItem #562
			// Bind to the caption height so we pick up changes at runtime.
			//
			FrameworkElement captionBorder = this.GetTemplateChild("PART_XamRibbonCaption") as FrameworkElement;

			if (null != captionBorder)
				this.SetBinding(CaptionHeightProperty, Utilities.CreateBindingObject(FrameworkElement.ActualHeightProperty, BindingMode.OneWay, captionBorder));
			else
				this.ClearValue(CaptionHeightProperty);
		}
			#endregion //OnApplyTemplate

            // JJD 11/30/07 - BR27696
            #region OnVisualParentChanged

        /// <summary>
        /// Invoked when the parent element of this element reports a change to its underlying visual parent.
        /// </summary>
        /// <param name="oldParent">The previous parent. This may be null if the element did not have a parent element previously.</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            // AS 11/2/07 CaptionHeight - WorkItem #562
            // Bind to the os key properties so we know when the os system values have changed. Note I'm not bothering
            // with the WindowCaptionButtonWidth since the value is always wrong when the value changes at runtime - it 
            // is always 1 setting behind (i.e. it returns the previous value).
            //
            this.SetResourceReference(SystemCaptionHeightProperty, SystemParameters.CaptionHeightKey);
            this.SetResourceReference(SystemCaptionButtonHeightProperty, SystemParameters.WindowCaptionButtonHeightKey);
			this.SetResourceReference(SystemCaptionButtonWidthProperty, SystemParameters.WindowCaptionButtonWidthKey); // AS 6/25/12 TFS114953

            this.SetResourceReference(StatusBarItemStyleInternalProperty, RibbonWindowContentHost.StatusBarItemStyleKey);
            this.SetResourceReference(StatusBarSeparatorStyleInternalProperty, RibbonWindowContentHost.StatusBarSeparatorStyleKey);

			// AS 6/3/08 BR32772
			//this._ribbonWindow = Utilities.GetAncestorFromType(this, typeof(XamRibbonWindow), true) as XamRibbonWindow;
            this._ribbonWindow = Utilities.GetAncestorFromType(this, typeof(IRibbonWindow), true) as IRibbonWindow;

            // JJD 11/30/07 - BR27696
            // Initialize the ribbonWindow with a refernece to this control
            if (this._ribbonWindow != null)
            {
                this.VerifyCaptionVisibility();

                this._ribbonWindow.InitializeContentHost(this);

            }

            XamRibbon ribbon = this.Ribbon;

            if (ribbon != null)
                ribbon.InitializeRibbonWindow(this._ribbonWindow);

        }

            #endregion //OnVisualParentChanged	
    
        #endregion //Base class overrides

		#region Properties

			#region Public Properties

				// AS 7/5/11 TFS76818
				#region BackgroundAreaMargin

			private static readonly DependencyPropertyKey BackgroundAreaMarginPropertyKey =
				DependencyProperty.RegisterReadOnly("BackgroundAreaMargin",
				typeof(Thickness), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(new Thickness()));

			/// <summary>
			/// Identifies the <see cref="BackgroundAreaMargin"/> dependency property
			/// </summary>
			public static readonly DependencyProperty BackgroundAreaMarginProperty =
				BackgroundAreaMarginPropertyKey.DependencyProperty;

			/// <summary>
			/// Returns the margin to be applied to the area containing the Content
			/// </summary>
			/// <seealso cref="BackgroundAreaMarginProperty"/>
			[Bindable(true)]
			[ReadOnly(true)]
			[Browsable(false)]
			public Thickness BackgroundAreaMargin
			{
				get
				{
					return (Thickness)this.GetValue(RibbonWindowContentHost.BackgroundAreaMarginProperty);
				}
			}

				#endregion //BackgroundAreaMargin

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region CaptionAreaMargin

        private static readonly DependencyPropertyKey CaptionAreaMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("CaptionAreaMargin",
            typeof(Thickness), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the <see cref="CaptionAreaMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CaptionAreaMarginProperty =
            CaptionAreaMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the CaptionArea (read-only)
        /// </summary>
        /// <seealso cref="CaptionAreaMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness CaptionAreaMargin
        {
            get
            {
                return (Thickness)this.GetValue(RibbonWindowContentHost.CaptionAreaMarginProperty);
            }
        }

                #endregion //CaptionAreaMargin

                #region CaptionButtonAreaWidth

            //private static readonly DependencyPropertyKey CaptionButtonAreaWidthPropertyKey =
            //    DependencyProperty.RegisterReadOnly("CaptionButtonAreaWidth",
            //    typeof(double), typeof(RibbonWindowContentHost),
            //    new FrameworkPropertyMetadata(3 * SystemParameters.WindowCaptionButtonWidth));
            internal static readonly DependencyPropertyKey CaptionButtonAreaWidthPropertyKey;

            /// <summary>
            /// Identifies the <see cref="CaptionButtonAreaWidth"/> property
            /// </summary>
            //public static readonly DependencyProperty CaptionButtonAreaWidthProperty =
            //    CaptionButtonAreaWidthPropertyKey.DependencyProperty;
            public static readonly DependencyProperty CaptionButtonAreaWidthProperty;

            /// <summary>
            /// Returns the total width used by the caption buttons, each determined by the <see cref="CaptionButtonWidth"/> property.
            /// </summary>
			/// <remarks>
			/// <p class="note"><b>Note:</b> This property is designed to be used when <see cref="XamRibbonWindow.IsGlassActive"/> is true to indicate the extent 
			/// required by the caption buttons rendered by the os.</p>
			/// </remarks>
            [Bindable(true)]
            public double CaptionButtonAreaWidth
            {
                get
                {
                    return (double)this.GetValue(RibbonWindowContentHost.CaptionButtonAreaWidthProperty);
                }
            }

				#endregion //CaptionButtonAreaWidth

                #region CaptionButtonHeight

            internal static readonly DependencyPropertyKey CaptionButtonHeightPropertyKey =
                    DependencyProperty.RegisterReadOnly("CaptionButtonHeight",
                    typeof(double), typeof(RibbonWindowContentHost), 
                    new FrameworkPropertyMetadata(SystemParameters.WindowCaptionButtonHeight));

            /// <summary>
            /// Identifies the <see cref="CaptionButtonHeight"/> property.
            /// </summary>
            public static readonly DependencyProperty CaptionButtonHeightProperty =
                CaptionButtonHeightPropertyKey.DependencyProperty;

            /// <summary>
            /// Returns the height of a button in the caption area (i.e. Minimize, Maximize/Restore, or Close).
            /// </summary>
            [Bindable(true)]
            [ReadOnly(true)]
            public double CaptionButtonHeight
            {
                get
                {
                    return (double)this.GetValue(RibbonWindowContentHost.CaptionButtonHeightProperty);
                }
            }

            #endregion //CaptionButtonHeight

                #region CaptionButtonWidth

            //private static readonly DependencyPropertyKey CaptionButtonWidthPropertyKey =
            //    DependencyProperty.RegisterReadOnly("CaptionButtonWidth",
            //    typeof(double), typeof(RibbonWindowContentHost), 
            //    new FrameworkPropertyMetadata(SystemParameters.WindowCaptionButtonWidth));

            ///// <summary>
            ///// Identifies the <see cref="CaptionButtonWidth"/> property.
            ///// </summary>
            //public static readonly DependencyProperty CaptionButtonWidthProperty =
            //    CaptionButtonWidthPropertyKey.DependencyProperty;
            internal static readonly DependencyPropertyKey CaptionButtonWidthPropertyKey;

            /// <summary>
            /// Identifies the <see cref="CaptionButtonWidth"/> property.
            /// </summary>
            public static readonly DependencyProperty CaptionButtonWidthProperty;

            /// <summary>
            /// Returns the width of a button in the caption area (i.e. Minimize, Maximize/Restore, or Close).
            /// </summary>
			/// <remarks>
			/// <p class="note"><b>Note:</b> This property value may not be accurate when the <see cref="XamRibbonWindow.IsGlassActive"/> is true since 
			/// the size of the caption buttons is not constant. Instead, the overall width required for all the caption buttons should be used (<see cref="CaptionButtonAreaWidth"/>).</p>
			/// </remarks>
            /// <seealso cref="CaptionButtonAreaWidth"/>
            [Bindable(true)]
            [ReadOnly(true)]
            public double CaptionButtonWidth
            {
                get
                {
                    return (double)this.GetValue(RibbonWindowContentHost.CaptionButtonWidthProperty);
                }
            }

            #endregion //CaptionButtonWidth

				// AS 10/24/07 AutoHide
				#region CaptionVisibility

			private static readonly DependencyPropertyKey CaptionVisibilityPropertyKey =
				DependencyProperty.RegisterReadOnly("CaptionVisibility",
				typeof(Visibility), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, new PropertyChangedCallback(OnCaptionVisibilityChanged)));

		private static void OnCaptionVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RibbonWindowContentHost contentHost = (RibbonWindowContentHost)d;

			// update the glass extension since the caption height may have changed.
            // We need to delay this action because this method may be called while the Ribbon is
            // still in MeasureOverride, so the CaptionHeight value will not have been calculated yet

            // JJD 11/30/07 - BR27696
            // Use the _ribbonWindow member but check for null first
            //window.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
            //       new XamRibbon.MethodInvoker(window.ExtendGlassIntoClientArea));
            if ( contentHost._ribbonWindow != null )
                contentHost._ribbonWindow.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                    new XamRibbon.MethodInvoker(contentHost._ribbonWindow.ExtendGlassIntoClientArea));
		}

			/// <summary>
			/// Identifies the <see cref="CaptionVisibility"/> dependency property
			/// </summary>
			public static readonly DependencyProperty CaptionVisibilityProperty =
				CaptionVisibilityPropertyKey.DependencyProperty;

			/// <summary>
			/// Returns a <see cref="Visibility"/> that indicates the expected visibility state of the caption of the <see cref="RibbonWindowContentHost"/>.
			/// </summary>
			/// <remarks>
			/// <p class="body">The caption of the <see cref="RibbonWindowContentHost"/> should be visible when either the <see cref="Ribbon"/> property has 
			/// not been set or if the <see cref="Infragistics.Windows.Ribbon.XamRibbon.AutoHideState"/> of the Ribbon is Hidden.</p>
			/// </remarks>
			/// <seealso cref="CaptionVisibilityProperty"/>
			/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.AutoHideState"/>
			//[Description("Returns a 'Visibility' that indicates the expected visibility state of the caption of the 'RibbonWindowContentHost'.")]
			//[Category("Ribbon Properties")]
			[Bindable(true)]
			[ReadOnly(true)]
			public Visibility CaptionVisibility
			{
				get
				{
					return (Visibility)this.GetValue(RibbonWindowContentHost.CaptionVisibilityProperty);
				}
			}

				#endregion //CaptionVisibility

				// AS 6/28/11 TFS76818
				#region ContentAreaMargin

			private static readonly DependencyPropertyKey ContentAreaMarginPropertyKey =
				DependencyProperty.RegisterReadOnly("ContentAreaMargin",
				typeof(Thickness), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(new Thickness()));

			/// <summary>
			/// Identifies the <see cref="ContentAreaMargin"/> dependency property
			/// </summary>
			public static readonly DependencyProperty ContentAreaMarginProperty =
				ContentAreaMarginPropertyKey.DependencyProperty;

			/// <summary>
			/// Returns the margin to be applied to the area containing the Content
			/// </summary>
			/// <seealso cref="ContentAreaMarginProperty"/>
			[Bindable(true)]
			[ReadOnly(true)]
			[Browsable(false)]
			public Thickness ContentAreaMargin
			{
				get
				{
					return (Thickness)this.GetValue(RibbonWindowContentHost.ContentAreaMarginProperty);
				}
			}

				#endregion //ContentAreaMargin

				// AS 7/13/09 TFS18489
				#region ContentHost

			private static readonly DependencyPropertyKey ContentHostPropertyKey
				= DependencyProperty.RegisterAttachedReadOnly("ContentHost", typeof(RibbonWindowContentHost), typeof(RibbonWindowContentHost),
					new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.Inherits));

			/// <summary>
			/// Identifies the ContentHost attached readonly inherited dependency property
			/// </summary>
			/// <seealso cref="GetContentHost"/>
			public static readonly DependencyProperty ContentHostProperty
				= ContentHostPropertyKey.DependencyProperty;

			/// <summary>
			/// Returns the <see cref="RibbonWindowContentHost"/> that contains the specified object or null (Nothing in VB) if the object is not contained within a RibbonWindowContentHost.
			/// </summary>
			/// <seealso cref="ContentHostProperty"/>
			public static RibbonWindowContentHost GetContentHost(DependencyObject d)
			{
				return (RibbonWindowContentHost)d.GetValue(ContentHostProperty);
			}
				#endregion //ContentHost

				// AS 10/23/07 XamRibbonWindow IconResolved
				#region IconResolved

		internal static readonly DependencyPropertyKey IconResolvedPropertyKey =
			DependencyProperty.RegisterReadOnly("IconResolved",
			typeof(ImageSource), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="IconResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IconResolvedProperty =
			IconResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the image used as the icon for the window.
		/// </summary>
		/// <remarks>
		/// <p class="body">This is a read-only property that returns the resolved image that represents the icon 
		/// for the Window. This is necessary since the <see cref="Window.Icon"/> property will return null by default 
		/// and does not return the Icon provided by the operating system.</p>
		/// </remarks>
		/// <seealso cref="IconResolvedProperty"/>
		//[Description("Returns the image used as the icon for the window.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public ImageSource IconResolved
		{
			get
			{
				return (ImageSource)this.GetValue(RibbonWindowContentHost.IconResolvedProperty);
			}
		}

				#endregion //IconResolved

                // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region IsScenicTheme

        internal static readonly DependencyPropertyKey IsScenicThemePropertyKey =
            DependencyProperty.RegisterReadOnly("IsScenicTheme",
            typeof(bool), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsScenicTheme"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsScenicThemeProperty =
            IsScenicThemePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if a 'Scenic' theme is being applied (read-only)
        /// </summary>
        /// <seealso cref="IsScenicThemeProperty"/>
        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
        public bool IsScenicTheme
        {
            get
            {
                return (bool)this.GetValue(RibbonWindowContentHost.IsScenicThemeProperty);
            }
            internal set
            {
                this.SetValue(RibbonWindowContentHost.IsScenicThemePropertyKey, value);
            }
        }

                #endregion //IsScenicTheme

                // JJD 4/30/10 - NA 2010 Volumne 2 - Scenic Riboon
                #region IsScenicThemeKey

		/// <summary>
        /// For internal use only.
		/// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
		public static readonly ResourceKey IsScenicThemeKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "IsScenicThemeKey");

				#endregion //IsScenicThemeKey

                #region MinCaptionHeight

        private static readonly DependencyPropertyKey MinCaptionHeightPropertyKey = DependencyProperty.RegisterReadOnly("MinCaptionHeight",
            typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(SystemParameters.CaptionHeight));

        /// <summary>
        /// Identifies the <see cref="MinCaptionHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinCaptionHeightProperty =
            MinCaptionHeightPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the minimum height of the caption area.
        /// </summary>
        /// <remarks>
        /// When Aero glass is enabled (Windows Vista only), this property will be equivalent to 
        /// the <see cref="SystemParameters.CaptionHeight"/> property; otherwise, if the 
		/// <see cref="Infragistics.Windows.Ribbon.XamRibbon.IsWithinRibbonWindow"/> returns true, we account for the 1px border
        /// that is drawn above the caption area.
        /// </remarks>
        public double MinCaptionHeight
        {
            get
            {
                return (double)this.GetValue(RibbonWindowContentHost.CaptionButtonHeightProperty);
            }
        }

				#endregion //MinCaptionHeight

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region RibbonAreaMargin

        private static readonly DependencyPropertyKey RibbonAreaMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("RibbonAreaMargin",
            typeof(Thickness), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the <see cref="RibbonAreaMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RibbonAreaMarginProperty =
            RibbonAreaMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the RibbonArea (read-only)
        /// </summary>
        /// <seealso cref="RibbonAreaMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness RibbonAreaMargin
        {
            get
            {
                return (Thickness)this.GetValue(RibbonWindowContentHost.RibbonAreaMarginProperty);
            }
        }

                #endregion //RibbonAreaMargin

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
                #region ScenicCloseWindowButtonStyleKey

        /// <summary>
        /// The key used to identify the style used for the Close Window button in the caption area when a 'Scenic' theme is applied on an OS theme where glass is not active.
        /// </summary>
		public static readonly ResourceKey ScenicCloseWindowButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "ScenicCloseWindowButtonStyleKey");

				#endregion //ScenicCloseWindowButtonStyleKey

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
                #region ScenicMaximizeRestoreButtonStyleKey

        /// <summary>
        /// The key used to identify the style used for the Maximize/Restore button in the caption area when a 'Scenic' theme is applied on an OS theme where glass is not active.
        /// </summary>
		public static readonly ResourceKey ScenicMaximizeRestoreButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "ScenicMaximizeRestoreButtonStyleKey");

				#endregion //ScenicMaximizeRestoreButtonStyleKey

                // JJD 5/17/10 - NA 2010 Volumne 2 - Scenic Riboon
                #region ScenicMinimizeButtonStyleKey

        /// <summary>
        /// The key used to identify the style used for the Minimize button in the caption area when a 'Scenic' theme is applied on an OS theme where glass is not active.
        /// </summary>
		public static readonly ResourceKey ScenicMinimizeButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "ScenicMinimizeButtonStyleKey");

				#endregion //ScenicMinimizeButtonStyleKey

				// JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region StatusBarAreaMargin

        private static readonly DependencyPropertyKey StatusBarAreaMarginPropertyKey =
            DependencyProperty.RegisterReadOnly("StatusBarAreaMargin",
            typeof(Thickness), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(new Thickness()));

        /// <summary>
        /// Identifies the <see cref="StatusBarAreaMargin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StatusBarAreaMarginProperty =
            StatusBarAreaMarginPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the margin to be applied to the StatusBarArea (read-only)
        /// </summary>
        /// <seealso cref="StatusBarAreaMarginProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [Browsable(false)]
        public Thickness StatusBarAreaMargin
        {
            get
            {
                return (Thickness)this.GetValue(RibbonWindowContentHost.StatusBarAreaMarginProperty);
            }
        }

                #endregion //StatusBarAreaMargin

				// AS 11/2/07 CaptionHeight - WorkItem #562
				// Added so we can listen for changes to the os setting.
				//
				#region SystemCaptionButtonHeight

		/// <summary>
		/// Stores the current system caption button height.
		/// </summary>
        // AS 8/21/08 BR35778
        // We need this value from the XamRibbonWindow...
        // 
        //private static readonly DependencyProperty SystemCaptionButtonHeightProperty = DependencyProperty.Register("SystemCaptionButtonHeight",
        internal static readonly DependencyProperty SystemCaptionButtonHeightProperty = DependencyProperty.Register("SystemCaptionButtonHeight",
			typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(SystemParameters.WindowCaptionButtonHeight, new PropertyChangedCallback(OnSystemCaptionButtonHeightChanged)));

		private static void OnSystemCaptionButtonHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.SetValue(CaptionButtonHeightPropertyKey, e.NewValue);

            // JJD 11/30/07 - BR27696
			//RibbonWindowContentHost window = (RibbonWindowContentHost)d;
			RibbonWindowContentHost contentHost = (RibbonWindowContentHost)d;

			// AS 6/25/12 TFS114953
			// We need to use the width because the buttons are not square in windows 7 basic.
			//
			//// note, I'm explicitly using the Height because there appears
			//// to be a bug in .net whereby the width is always 1 setting
			//// behind when its changed while the app is running
			//contentHost.SetValue(CaptionButtonWidthPropertyKey, e.NewValue);

            // JJD 11/30/07 - BR27696
            // Use the _ribbonWindow member but check for null first
            //IntPtr handle = IntPtr.Zero;
            //if (contentHost.IsGlassActiveInternal)
            //    handle = new WindowInteropHelper(contentHost).Handle;
			//contentHost.RefreshCaptionButtonAreaWidth(handle);
            if (contentHost._ribbonWindow != null)
            {
                IntPtr handle = IntPtr.Zero;
                if (contentHost._ribbonWindow.IsGlassActiveInternal)
					// AS 6/3/08 BR32772
					//handle = new WindowInteropHelper(contentHost._ribbonWindow).Handle;
                    handle = new WindowInteropHelper(contentHost._ribbonWindow.Window).Handle;

                contentHost._ribbonWindow.RefreshCaptionButtonAreaWidth(handle);
            }
		}

				#endregion //SystemCaptionButtonHeight

				// AS 6/25/12 TFS114953
				#region SystemCaptionButtonWidth

		/// <summary>
		/// Stores the current system caption button width.
		/// </summary>
		internal static readonly DependencyProperty SystemCaptionButtonWidthProperty = DependencyProperty.Register("SystemCaptionButtonWidth",
			typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(SystemParameters.WindowCaptionButtonHeight, new PropertyChangedCallback(OnSystemCaptionButtonWidthChanged)));

		private static void OnSystemCaptionButtonWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// JJD 11/30/07 - BR27696
			//RibbonWindowContentHost window = (RibbonWindowContentHost)d;
			RibbonWindowContentHost contentHost = (RibbonWindowContentHost)d;

			// note, I'm explicitly using the Height because there appears
			// to be a bug in .net whereby the width is always 1 setting
			// behind when its changed while the app is running
			contentHost.SetValue(CaptionButtonWidthPropertyKey, e.NewValue);

			// JJD 11/30/07 - BR27696
			// Use the _ribbonWindow member but check for null first
			//IntPtr handle = IntPtr.Zero;
			//if (contentHost.IsGlassActiveInternal)
			//    handle = new WindowInteropHelper(contentHost).Handle;
			//contentHost.RefreshCaptionButtonAreaWidth(handle);
			if (contentHost._ribbonWindow != null)
			{
				IntPtr handle = IntPtr.Zero;
				if (contentHost._ribbonWindow.IsGlassActiveInternal)
					// AS 6/3/08 BR32772
					//handle = new WindowInteropHelper(contentHost._ribbonWindow).Handle;
					handle = new WindowInteropHelper(contentHost._ribbonWindow.Window).Handle;

				contentHost._ribbonWindow.RefreshCaptionButtonAreaWidth(handle);
			}
		}

				#endregion //SystemCaptionButtonWidth

				#region SystemCaptionHeight

		/// <summary>
 		/// Stores the current system caption height.
		/// </summary>
		private static readonly DependencyProperty SystemCaptionHeightProperty = DependencyProperty.Register("SystemCaptionHeight",
			typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(SystemParameters.CaptionHeight, new PropertyChangedCallback(OnSystemCaptionHeightChanged)));

		private static void OnSystemCaptionHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			d.SetValue(MinCaptionHeightPropertyKey, e.NewValue);
		}

				#endregion //SystemCaptionHeight

				// AS 6/3/08 BR32772
				// Moved here so we can reference it from xaml.
				//
				#region ResizeGripStyleKey

		/// <summary>
		/// The key used to identify the style used for the <see cref="ResizeGrip"/> displayed in the window.
		/// </summary>
		/// <remarks>
		/// <para class="body">To style the ResizeGrip used in the bottom corner of the window place a Style in the window's Resources collection whose TargetType is set 
		/// to <see cref="System.Windows.Controls.Primitives.ResizeGrip"/> and whose key is set to this key, e.g. Key="{x:Static igRibbon:XamRibbonWindow.ResizeGripStyleKey}"</para>
		/// <para class="note"><b>Note:</b> the <see cref="System.Windows.Controls.Primitives.ResizeGrip"/> is only visible when the <see cref="ResizeMode"/> property is set to 'CanResizeWithGrip'.</para>
		/// </remarks>
		public static readonly ResourceKey ResizeGripStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "ResizeGripStyleKey");

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
		public static readonly ResourceKey ScenicResizeGripStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "ScenicResizeGripStyleKey");

				#endregion //ScenicResizeGripStyleKey

				#region Ribbon

		/// <summary>
		/// Identifies the <see cref="Ribbon"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RibbonProperty = DependencyProperty.Register("Ribbon",
			typeof(XamRibbon), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRibbonChanged)));

        private static void OnRibbonChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            // JJD 11/30/07 - BR27696
            // Use the _ribbonWindow member but check for null first
            //XamRibbonWindow window = target as XamRibbonWindowt;
			// AS 6/3/08 BR32772
			//XamRibbonWindow window = null;
			IRibbonWindow window = null;
            RibbonWindowContentHost contentHost = target as RibbonWindowContentHost;

            if (contentHost != null)
            {
                XamRibbon ribbon = e.NewValue as XamRibbon;

                window = contentHost._ribbonWindow;

                // JJD 11/30/07 - BR27696
                // check for null first
                if (window != null)
                {
                    // Synchronize the attached property settings
                    window.SynchronizeIsGlassActive();

                    // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                    window.SynchronizeIsScenicTheme();
                }

				if (ribbon != null)
				{
                    contentHost.AddLogicalChild(ribbon);
                    contentHost._logicalChildren.Add(ribbon);

                    // JJD 11/30/07 - BR27696
                    // check for null first
                    if ( window != null )
					    ribbon.InitializeRibbonWindow(window);

					// AS 10/24/07 AutoHide
                    contentHost.SetBinding(RibbonWindowContentHost.RibbonAutoHideStateProperty, Utilities.CreateBindingObject(XamRibbon.AutoHideStateProperty, BindingMode.OneWay, ribbon));
                    contentHost.SetBinding(RibbonWindowContentHost.RibbonVisibilityProperty, Utilities.CreateBindingObject(XamRibbon.VisibilityProperty, BindingMode.OneWay, ribbon));

					// AS 6/29/12 TFS114953
					contentHost.SetBinding( RibbonWindowContentHost.TabHeaderHeightProperty, Utilities.CreateBindingObject( XamRibbon.TabHeaderHeightProperty, BindingMode.OneWay, ribbon ) );
				}
				else
				{
					// AS 10/24/07 AutoHide
                    contentHost.ClearValue(RibbonWindowContentHost.RibbonAutoHideStateProperty);
                    contentHost.ClearValue(RibbonWindowContentHost.RibbonVisibilityProperty);

					// AS 6/29/12 TFS114953
					contentHost.ClearValue( RibbonWindowContentHost.TabHeaderHeightProperty );
				}

                ribbon = e.OldValue as XamRibbon;
				if (ribbon != null)
				{
                    contentHost._logicalChildren.Remove(ribbon);
                    contentHost.RemoveLogicalChild(ribbon);
					ribbon.InitializeRibbonWindow(null);

                    contentHost.ResetCaptionInfo();
				}

				// AS 10/24/07 AutoHide
                contentHost.VerifyCaptionVisibility();
            }
        }

		/// <summary>
		/// Returns/sets the <see cref="XamRibbon"/> control to be displayed within the caption area of the containing <see cref="XamRibbonWindow"/>.
		/// </summary>
		/// <seealso cref="RibbonProperty"/>
		/// <seealso cref="StatusBar"/>
		//[Description("Returns/sets the XamRibbon control to be displayed within the caption area of the containing XamRibbonWindow.")]
		//[Category("Ribbon Properties")]
		public XamRibbon Ribbon
		{
			get
			{
				return (XamRibbon)this.GetValue(RibbonWindowContentHost.RibbonProperty);
			}
			set
			{
				this.SetValue(RibbonWindowContentHost.RibbonProperty, value);
			}
		}

				#endregion //Ribbon

				#region StatusBar

		/// <summary>
		/// Identifies the <see cref="StatusBar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StatusBarProperty = DependencyProperty.Register("StatusBar",
			typeof(StatusBar), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStatusBarChanged)));

		private static void OnStatusBarChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
            // JJD 11/30/07 - BR27696
            // Use the _ribbonWindow member but check for null first
            //XamRibbonWindow window = target as XamRibbonWindowt;
			// AS 6/3/08 BR32772
			//XamRibbonWindow window = null;
            IRibbonWindow window = null;
            RibbonWindowContentHost contentHost = target as RibbonWindowContentHost;

			Debug.Assert(contentHost != null);

            if (contentHost != null)
            {
                window = contentHost._ribbonWindow;

				StatusBar oldStatusBar = e.OldValue as StatusBar;

				if (oldStatusBar != null)
				{
					// remove the old merged dictionary
					if (contentHost._statusBarResources != null)
					{
						if (oldStatusBar.Resources.MergedDictionaries.Contains(contentHost._statusBarResources))
							oldStatusBar.Resources.MergedDictionaries.Remove(contentHost._statusBarResources);

						contentHost._statusBarResources = null;
					}

					contentHost.RemoveLogicalChild(oldStatusBar);
					contentHost._logicalChildren.Remove(oldStatusBar);
					contentHost.ClearValue(StatusBarVisibilityProperty);
				}

				StatusBar newStatusBar = e.NewValue as StatusBar;

				if (newStatusBar == null)
					return;

                // JJD 11/30/07 - BR27696
                // check for null first
                if (window != null)
                {
                    // Synchronize the attached property settings
                    window.SynchronizeIsGlassActive();
                    
                    // JJD 05/13/10 - NA 2010 volume 2 - Scenic Ribbon support
                    window.SynchronizeIsScenicTheme();
                }

				contentHost.AddLogicalChild(newStatusBar);
				contentHost._logicalChildren.Add(newStatusBar);

				contentHost.SetBinding(StatusBarVisibilityProperty, Utilities.CreateBindingObject(VisibilityProperty, BindingMode.OneWay, newStatusBar));

				// Get the source of any style setting on the statusbar
				BaseValueSource source = DependencyPropertyHelper.GetValueSource(newStatusBar, StyleProperty).BaseValueSource;

				// if the source is less than or equal to an ImplicitStyleReference then set the
				// style property using a dynamic reference based on the static StatusBarStyleKey
				if ((int)source <= (int)BaseValueSource.ImplicitStyleReference)
					newStatusBar.SetResourceReference(StyleProperty, RibbonWindowContentHost.StatusBarStyleKey);

				// always set the DefaultStyleKey property to our static StatusBarStyleKey
				newStatusBar.SetValue(DefaultStyleKeyProperty, RibbonWindowContentHost.StatusBarStyleKey);

				contentHost.UpdateStatusBarResources();               
			}
		}

		/// <summary>
		/// Returns/sets the <see cref="System.Windows.Controls.Primitives.StatusBar"/> to be positioned along the bottom edge of the containing <see cref="XamRibbonWindow"/>.
		/// </summary>
		/// <seealso cref="StatusBarProperty"/>
		/// <seealso cref="XamRibbonWindow"/>
		//[Description("Returns/sets the element that should be positioned along the bottom edge of the window to serve as a status bar.")]
		//[Category("Ribbon Properties")]
		public StatusBar StatusBar
		{
			get
			{
				return (StatusBar)this.GetValue(RibbonWindowContentHost.StatusBarProperty);
			}
			set
			{
				this.SetValue(RibbonWindowContentHost.StatusBarProperty, value);
			}
		}

				#endregion //StatusBar

				// AS 12/2/09 TFS24267
				#region StatusBarPadding

		internal static readonly DependencyPropertyKey StatusBarPaddingPropertyKey =
			DependencyProperty.RegisterReadOnly("StatusBarPadding",
			typeof(Thickness), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(new Thickness()));

		/// <summary>
		/// Identifies the <see cref="StatusBarPadding"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StatusBarPaddingProperty =
			StatusBarPaddingPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the preferred padding for the contained status bar based on the border width and presence of the resize grip.
		/// </summary>
		/// <seealso cref="StatusBarPaddingProperty"/>
		//[Description("Returns the preferred padding for the contained status bar based on the border width and presence of the resize grip.")]
		//[Category("Appearance")]
		[Bindable(true)]
		[ReadOnly(true)]
		public Thickness StatusBarPadding
		{
			get
			{
				return (Thickness)this.GetValue(RibbonWindowContentHost.StatusBarPaddingProperty);
			}
		}

				#endregion //StatusBarPadding

				#region StatusBarStyleKey

		/// <summary>
		/// The key used to identify the style used for the <see cref="System.Windows.Controls.Primitives.StatusBar"/>.
		/// </summary>
        /// <remarks>
        /// <para class="body">To style the <see cref="StatusBar"/> used in the bottom of the window place a Style in the Resources collection whose TargetType is set 
        /// to <see cref="System.Windows.Controls.Primitives.StatusBar"/> and whose key is set to this key, e.g. Key="{x:Static igRibbon:RibbonWindowContentHost.StatusBarStyleKey}".</para>
        /// </remarks>
        /// <seealso cref="StatusBar"/>
		/// <seealso cref="StatusBarItemStyleKey"/>
		/// <seealso cref="StatusBarSeparatorStyleKey"/>
		public static readonly ResourceKey StatusBarStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "StatusBarStyleKey");

				#endregion //StatusBarStyleKey	
		
				#region StatusBarItemStyleKey

		/// <summary>
        /// The key used to identify the style used for <see cref="System.Windows.Controls.Primitives.StatusBarItem"/> controls inside the <see cref="StatusBar"/>.
		/// </summary>
        /// <remarks>
        /// <para class="body">To style the <see cref="System.Windows.Controls.Primitives.StatusBarItem"/>s used inside the <see cref="StatusBar"/> place a Style in the Resources collection whose TargetType is set 
        /// to <see cref="System.Windows.Controls.Primitives.StatusBarItem"/> and whose key is set to this key, e.g. Key="{x:Static igRibbon:RibbonWindowContentHost.StatusBarItemStyleKey}".</para>
        /// </remarks>
        /// <seealso cref="StatusBar"/>
		/// <seealso cref="StatusBarStyleKey"/>
		/// <seealso cref="StatusBarSeparatorStyleKey"/>
		public static readonly ResourceKey StatusBarItemStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "StatusBarItemStyleKey");

				#endregion //StatusBarItemStyleKey	
		
				#region StatusBarSeparatorStyleKey

		/// <summary>
		/// The key used to identify the style used for Separators inside the <see cref="StatusBar"/>.
		/// </summary>
        /// <remarks>
        /// <para class="body">To style the <see cref="System.Windows.Controls.Separator"/>s used inside the <see cref="StatusBar"/> place a Style in the Resources collection whose TargetType is set 
        /// to <see cref="System.Windows.Controls.Separator"/> and whose key is set to this key, e.g. Key="{x:Static igRibbon:RibbonWindowContentHost.StatusBarSeparatorStyleKey}".</para>
        /// </remarks>
        /// <seealso cref="StatusBar"/>
		/// <seealso cref="StatusBarStyleKey"/>
		/// <seealso cref="StatusBarItemStyleKey"/>
		public static readonly ResourceKey StatusBarSeparatorStyleKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "StatusBarSeparatorStyleKey");

				#endregion //StatusBarSeparatorStyleKey	

				// AS 6/21/12 TFS114953
                #region UsesScenicApplicationMenuKey

		/// <summary>
        /// For internal use only.
		/// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
		public static readonly ResourceKey UsesScenicApplicationMenuKey = new StaticPropertyResourceKey(typeof(RibbonWindowContentHost), "UsesScenicApplicationMenuKey");

				#endregion //UsesScenicApplicationMenuKey

			#endregion //Public Properties

			#region Private Properties

				// AS 11/2/07 CaptionHeight - WorkItem #562
				// Changed to a dependency property so we can bind to the actual height of the caption element.
				//
				#region CaptionHeight

		/// <summary>
		/// Used to obtain the actual height of the caption element.
		/// </summary>
		internal static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register("CaptionHeight",
			typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(0d));

				#endregion //CaptionHeight

				// AS 11/2/07 CaptionHeight - WorkItem #562
				// This replaces the old CaptionHeight clr property and is used to track the height of whichever
				// caption element we are using - that of the RibbonWindowContentHost or the XamRibbon.
				//
				#region CaptionHeightResolved

		internal static readonly DependencyProperty CaptionHeightResolvedProperty = DependencyProperty.Register("CaptionHeightResolved",
			typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnCaptionHeightResolvedChanged)));

		internal double CaptionHeightResolved
		{
			get { return (double)this.GetValue(CaptionHeightResolvedProperty); }
		}

		private static void OnCaptionHeightResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            // JJD 11/30/07 - BR27696
            // Use the _ribbonWindow member but check for null first
            //XamRibbonWindow window = d as XamRibbonWindowt;
            RibbonWindowContentHost contentHost = d as RibbonWindowContentHost;

            if (contentHost != null)
            {
				// AS 6/3/08 BR32772
				//XamRibbonWindow window = contentHost._ribbonWindow;
				IRibbonWindow window = contentHost._ribbonWindow;

                if (window != null)
                {
                    window.ExtendGlassIntoClientArea();

                    // AS 11/12/07 BR27344
                    // We need to reshape the window when the caption height changes.
                    //
                    window.UpdateWindowClipRegion();
                }
            }
		}

				#endregion //CaptionHeightResolved

				// AS 10/24/07 AutoHide
				#region RibbonAutoHideState

		/// <summary>
		/// Identifies the <see cref="RibbonAutoHideState"/> dependency property
		/// </summary>
		private static readonly DependencyProperty RibbonAutoHideStateProperty = DependencyProperty.Register("RibbonAutoHideState",
			typeof(RibbonAutoHideState), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(RibbonAutoHideState.NotHidden, new PropertyChangedCallback(OnRibbonAutoHideStateChanged)));

		private static void OnRibbonAutoHideStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((RibbonWindowContentHost)d).VerifyCaptionVisibility();
		}

		/// <summary>
		/// Returns a RibbonAutoHideStateean indicating if the <see cref="Ribbon"/> is currently collapsed.
		/// </summary>
		/// <seealso cref="RibbonAutoHideStateProperty"/>
		private RibbonAutoHideState RibbonAutoHideState
		{
			get
			{
				return (RibbonAutoHideState)this.GetValue(RibbonWindowContentHost.RibbonAutoHideStateProperty);
			}
			set
			{
				this.SetValue(RibbonWindowContentHost.RibbonAutoHideStateProperty, value);
			}
		}

				#endregion //RibbonAutoHideState

				// AS 10/24/07 AutoHide
				#region RibbonVisibility

		private static readonly DependencyProperty RibbonVisibilityProperty =
			DependencyProperty.Register("RibbonVisibility",
			typeof(Visibility), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnRibbonVisibilityChanged)));

		private static void OnRibbonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((RibbonWindowContentHost)d).VerifyCaptionVisibility();
		}

		/// <summary>
		/// Returns the current visibility of the <see cref="Ribbon"/>
		/// </summary>
		/// <seealso cref="RibbonVisibilityProperty"/>
		private Visibility RibbonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(RibbonWindowContentHost.RibbonVisibilityProperty);
			}
		}

				#endregion //RibbonVisibility

				#region StatusBarItemStyleInternal

		private static readonly DependencyProperty StatusBarItemStyleInternalProperty = DependencyProperty.Register("StatusBarItemStyleInternal",
			typeof(Style), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStatusBarInternalStylesChanged)));

		private static void OnStatusBarInternalStylesChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			RibbonWindowContentHost window = target as RibbonWindowContentHost;

			Debug.Assert(window != null);

			if (window != null)
			{
				window.UpdateStatusBarResources();
			}
		}

		private Style StatusBarItemStyleInternal
		{
			get
			{
				return (Style)this.GetValue(RibbonWindowContentHost.StatusBarItemStyleInternalProperty);
			}
			set
			{
				this.SetValue(RibbonWindowContentHost.StatusBarItemStyleInternalProperty, value);
			}
		}

				#endregion //StatusBarItemStyleInternal

				#region StatusBarSeparatorStyleInternal

		private static readonly DependencyProperty StatusBarSeparatorStyleInternalProperty = DependencyProperty.Register("StatusBarSeparatorStyleInternal",
			typeof(Style), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStatusBarInternalStylesChanged)));

		private Style StatusBarSeparatorStyleInternal
		{
			get
			{
				return (Style)this.GetValue(RibbonWindowContentHost.StatusBarSeparatorStyleInternalProperty);
			}
			set
			{
				this.SetValue(RibbonWindowContentHost.StatusBarSeparatorStyleInternalProperty, value);
			}
		}

				#endregion //StatusBarSeparatorStyleInternal

				// AS 6/29/12 TFS114953
				#region TabHeaderHeight

		/// <summary>
		/// Used to obtain the actual height of the caption element.
		/// </summary>
		internal static readonly DependencyProperty TabHeaderHeightProperty = DependencyProperty.Register("TabHeaderHeight",
			typeof(double), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnCaptionHeightResolvedChanged)));

				#endregion //TabHeaderHeight

			#endregion //Private Properties	

            #region Internal Properties

                // JJD 05/25/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region IsClassicOSTheme

        internal static readonly DependencyProperty IsClassicOSThemeProperty =
            DependencyProperty.Register("IsClassicOSTheme",
            typeof(bool), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        internal bool IsClassicOSTheme
        {
            get
            {
                return (bool)this.GetValue(RibbonWindowContentHost.IsClassicOSThemeProperty);
            }
        }

                #endregion //IsClassicOSTheme

				#region StatusBarVisibility

			internal static readonly DependencyProperty StatusBarVisibilityProperty = DependencyProperty.Register("StatusBarVisibility",
				typeof(Visibility), typeof(RibbonWindowContentHost), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, new PropertyChangedCallback(OnStatusBarVisibilityChanged)));

		private static void OnStatusBarVisibilityChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
            // JJD 11/30/07 - BR27696
            // Use the _ribbonWindow member but check for null first
            //XamRibbonWindow window = target as XamRibbonWindowt;
            RibbonWindowContentHost contentHost = target as RibbonWindowContentHost;

            if (contentHost != null)
            {
				// AS 6/3/08 BR32772
				//XamRibbonWindow window = contentHost._ribbonWindow;
				IRibbonWindow window = contentHost._ribbonWindow;

				if (window != null)
				{
					// AS 6/28/11 TFS76818
					// The new ContentAreaMargins are based upon whether the status bar is visible.
					//
					contentHost.CalculateMargins();

					window.UpdateWindowClipRegion();
				}
            }
		}

			internal Visibility StatusBarVisibility
			{
				get
				{
					return (Visibility)this.GetValue(RibbonWindowContentHost.StatusBarVisibilityProperty);
				}
				set
				{
					this.SetValue(RibbonWindowContentHost.StatusBarVisibilityProperty, value);
				}
			}

				#endregion //StatusBarVisibility

				// AS 6/28/12 TFS114953
				#region UseScenicApplicationMenuInternal

			/// <summary>
			/// Identifies the <see cref="UseScenicApplicationMenuInternal"/> dependency property
			/// </summary>
			private static readonly DependencyProperty UseScenicApplicationMenuInternalProperty = DependencyProperty.Register( "UseScenicApplicationMenuInternal",
				typeof( bool ), typeof( RibbonWindowContentHost ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox, new PropertyChangedCallback( OnUseScenicApplicationMenuInternalChanged ) ) );

			private static void OnUseScenicApplicationMenuInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			{
				var instance = d as RibbonWindowContentHost;
				instance.CalculateMargins();
			}

			/// <summary>
			/// The actual height of the top level visual containing the ribbon.
			/// </summary>
			internal bool UseScenicApplicationMenuInternal
			{
				get
				{
					return (bool)this.GetValue( RibbonWindowContentHost.UseScenicApplicationMenuInternalProperty );
				}
				private set
				{
					this.SetValue( RibbonWindowContentHost.UseScenicApplicationMenuInternalProperty, value );
				}
			}

				#endregion //UseScenicApplicationMenuInternal

            #endregion //Internal Properties
    
		#endregion //Properties

        #region Private Methods

            #region Methods

                // JJD 5/27/10 - NA 2010 volume 2 - Scenic Ribbon support
                #region CalculateMargins

            internal void CalculateMargins()
            {
				// AS 6/28/11 TFS76818
				if (_ribbonWindow == null)
					return;

                bool isGlassActive = this._ribbonWindow.IsGlassActiveInternal;
                
                Thickness borderThickness = this.BorderThickness;
                
                if (this._ribbonWindow == null || isGlassActive || borderThickness.Equals(new Thickness()))
                {
                    this.ClearValue(CaptionAreaMarginPropertyKey);
                    this.ClearValue(RibbonAreaMarginPropertyKey);
                    this.ClearValue(StatusBarAreaMarginPropertyKey);
					this.ClearValue(ContentAreaMarginPropertyKey); // AS 6/28/11 TFS76818
					this.ClearValue(BackgroundAreaMarginPropertyKey); // AS 7/5/11 TFS76818
					return;
                }

                bool isScenicTheme = this._ribbonWindow.IsScenicThemeInternal;

				// AS 6/28/11 TFS76818
				// If there is no statusbar and the statusbar area will just be a single pixel, we 
				// need the content area margin to provide the space for the bottom border. Otherwise 
				// just carve out space for the left/right border. 
				//
				Thickness contentAreaMargin;
				if (this.StatusBarVisibility == Visibility.Collapsed && !isScenicTheme)
					contentAreaMargin = new Thickness(borderThickness.Left, 2, borderThickness.Right, borderThickness.Bottom - 1);
				else
					contentAreaMargin = new Thickness(borderThickness.Left, 2, borderThickness.Right, 0);

				this.SetValue(ContentAreaMarginPropertyKey, contentAreaMargin);

				// AS 7/5/11 TFS76818
				// Since the rectangle providing the background fill is supposed to be under the status bar 
				// and content area we need to have a separate margin for that element.
				//
				// AS 10/26/11 TFS90219
				// The top border should only be 1 instead of 2. I think I used two because that was working but 
				// that was only needed because the xamTabControl had a negative 1 for its top margin as a result 
				// of some styling that Harry did back in 2007 when we were originally implementing the ribbon.
				//
				Thickness backgroundAreaMargin = new Thickness(borderThickness.Left, 1, borderThickness.Right, borderThickness.Bottom);
				this.SetValue(BackgroundAreaMarginPropertyKey, backgroundAreaMargin);

                if (!isScenicTheme)
                {
                    this.ClearValue(CaptionAreaMarginPropertyKey);
                    this.SetValue(RibbonAreaMarginPropertyKey, new Thickness(1, 1, 1, -1));

					// AS 6/28/12 TFS114953
					// The status bar should not go to the edge for office 2010.
					//
					//this.SetValue( StatusBarAreaMarginPropertyKey, new Thickness( 1, 0, 1, 1 ) );
					if ( this.UseScenicApplicationMenuInternal )
					{
						double bottom = _ribbonWindow.WindowState == WindowState.Maximized ? 0 : borderThickness.Bottom;
						this.SetValue( StatusBarAreaMarginPropertyKey, new Thickness( borderThickness.Left, 0, borderThickness.Right, bottom ) );
					}
					else
	                    this.SetValue(StatusBarAreaMarginPropertyKey, new Thickness(1, 0, 1, 1));

                    return;
                }

                this.SetValue(CaptionAreaMarginPropertyKey, new Thickness(2,2,2,-2));
                this.SetValue(RibbonAreaMarginPropertyKey, new Thickness(2,2,2,-2));

				var statusMargin = new Thickness(borderThickness.Left, 0, borderThickness.Right, borderThickness.Bottom);

				// AS 10/26/11 TFS85015
				if (_ribbonWindow.WindowState == WindowState.Maximized)
					statusMargin.Bottom = 0;

                this.SetValue(StatusBarAreaMarginPropertyKey, statusMargin);
            }

                #endregion //CalculateMargins	
    
                #region ResetCaptionInfo

        internal void ResetCaptionInfo()
        {
            this.ClearValue(RibbonWindowContentHost.CaptionButtonAreaWidthPropertyKey);
            this.ClearValue(RibbonWindowContentHost.CaptionButtonHeightPropertyKey);
            // AS 2/11/09 TFS11155
            //this.ClearValue(RibbonWindowContentHost.CaptionButtonWidthProperty);
            //this.ClearValue(RibbonWindowContentHost.MinCaptionHeightProperty);                    
            this.ClearValue(RibbonWindowContentHost.CaptionButtonWidthPropertyKey);
            this.ClearValue(RibbonWindowContentHost.MinCaptionHeightPropertyKey);                    
        }
                #endregion //ResetCaptionInfo

                #region UpdateStatusBarResources

        private void UpdateStatusBarResources()
		{
			StatusBar statusbar = this.StatusBar;

			// if we don't have a status bar return
			if (statusbar == null)
			{
				// clear the old resource dictionary
				this._statusBarResources = null;
				return;
			}

			// remove the old merged dictionary
			if (this._statusBarResources != null)
			{
				if (statusbar.Resources.MergedDictionaries.Contains(this._statusBarResources))
					statusbar.Resources.MergedDictionaries.Remove(this._statusBarResources);

				this._statusBarResources = null;
			}

            XamRibbon ribbon = this.Ribbon;

            // JJD 4/30/10 - NA 2010 Volumne 2 - Scenic Ribbon
            // Use the default OS theme for the status bar and separator
            if (ribbon != null && ribbon.IsScenicTheme)
                return;

			Style itemStyle = this.StatusBarItemStyleInternal;
			Style separatorStyle = this.StatusBarSeparatorStyleInternal;

			// if we don't have any styles then return
			if (itemStyle == null && separatorStyle == null)
				return;

			this._statusBarResources = new ResourceDictionary();

			if (itemStyle != null)
				this._statusBarResources.Add(typeof(StatusBarItem), itemStyle);

			if (separatorStyle != null)
				this._statusBarResources.Add(System.Windows.Controls.Primitives.StatusBar.SeparatorStyleKey, separatorStyle);
		
			// add the dictionary to the statusbar's merged dictionaries collection
			statusbar.Resources.MergedDictionaries.Add(this._statusBarResources);

		}

			#endregion //UpdateStatusBarResources

                // AS 10/24/07 AutoHide
                #region VerifyCaptionVisibility

            private void VerifyCaptionVisibility()
            {
                DependencyObject captionHeightSource;

                if (this.Ribbon == null || this.RibbonAutoHideState == RibbonAutoHideState.Hidden || this.RibbonVisibility != Visibility.Visible)
                {
                    this.SetValue(CaptionVisibilityPropertyKey, KnownBoxes.VisibilityVisibleBox);
                    captionHeightSource = this;
                }
                else
                {
                    this.ClearValue(CaptionVisibilityPropertyKey);
                    captionHeightSource = this.Ribbon;
                }

                // AS 11/2/07 CaptionHeight - WorkItem #562
                // Use the actualheight of either the xamribbon caption or the xamribbonwindow caption depending
                // on which is visible.
                //
                this.SetBinding(CaptionHeightResolvedProperty, Utilities.CreateBindingObject(CaptionHeightProperty, BindingMode.OneWay, captionHeightSource));
            }
                #endregion //VerifyCaptionVisibility

            #endregion //Methods

        #endregion //Private Methods	
        
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