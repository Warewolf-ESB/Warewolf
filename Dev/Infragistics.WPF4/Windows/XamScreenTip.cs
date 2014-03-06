using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Themes;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// The XamScreenTip is derived from the WPF ToolTip class and adds the notion of <see cref="Header"/> and <see cref="Footer"/> to the standard ToolTip.
	/// </summary>
	/// <remarks>
	/// <p class="body">The header area appears at the top of the tooltip (its content provided by the <see cref="Header"/> property) and the footer area appears at 
	/// the bottom of the tooltip (its content provided by the <see cref="Footer"/> property).  In between the <see cref="Header"/> and <see cref="Footer"/> areas is 
	/// the main tooltip area, with its content provided by the base ToolTip class�s Content property.</p>
	/// </remarks>
	/// <seealso cref="Header"/>
	/// <seealso cref="Footer"/>
		// AS 11/7/07 BR21903
		// AS 11/7/07 BR21903
	//[ToolboxItem(false)]	// JM BR28082 11-09-07 - added this here for documentation but commented out and added ToolboxBrowsableAttribute directly to DesignMetadata for the Windows assembly.
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class XamScreenTip : System.Windows.Controls.ToolTip
	{
		#region Member Variables

		// AS 11/7/07 BR21903
		private Infragistics.Windows.Licensing.UltraLicense _license;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="XamScreenTip"/> class.
		/// </summary>
		public XamScreenTip()
		{
			// AS 11/7/07 BR21903
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamScreenTip), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }
		}

		static XamScreenTip()
		{
			// AS 5/9/08
			// register the groupings that should be applied when the theme property is changed
			ThemeManager.RegisterGroupings(typeof(XamScreenTip), new string[] { PrimitivesGeneric.Location.Grouping });

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamScreenTip), new FrameworkPropertyMetadata(typeof(XamScreenTip)));
		}

		#endregion //Constructor

		#region Properties

			#region Public Properties

				#region Footer

		/// <summary>
		/// Identifies the <see cref="Footer"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterProperty = DependencyProperty.Register("Footer",
			typeof(object), typeof(XamScreenTip), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the content for the footer area of the XamScreenTip.  The default is null.
		/// </summary>
		/// <remarks>
		/// <p class="body">The footer area appears at the bottom of the tooltip below the main tooltip content.  If set to null, no footer is displayed.</p>
		/// </remarks>
		/// <seealso cref="FooterProperty"/>
		/// <seealso cref="FooterTemplate"/>
		/// <seealso cref="FooterSeparatorVisibility"/>
		/// <seealso cref="FooterTemplateSelector"/>
		/// <seealso cref="Header"/>
		//[Description("Returns/sets the content for the footer area of the XamScreenTip.")]
		//[Category("Data")]
		[Bindable(true)]
		public object Footer
		{
			get
			{
				return (object)this.GetValue(XamScreenTip.FooterProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.FooterProperty, value);
			}
		}

				#endregion //Footer

				#region FooterSeparatorVisibility

		/// <summary>
		/// Identifies the <see cref="FooterSeparatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterSeparatorVisibilityProperty = DependencyProperty.Register("FooterSeparatorVisibility",
			typeof(Visibility), typeof(XamScreenTip), new FrameworkPropertyMetadata(Visibility.Visible));

		/// <summary>
		/// Returns/sets the Visibility of the <see cref="Footer"/> separator which divides the footer area from the main content.  The default is Visibility.Visible.
		/// </summary>
		/// <seealso cref="FooterSeparatorVisibilityProperty"/>
		/// <seealso cref="Footer"/>
		/// <seealso cref="FooterTemplate"/>
		/// <seealso cref="FooterTemplateSelector"/>
		/// <seealso cref="Header"/>
		//[Description("Returns/sets the Visibility of the footer separator which divides the footer area from the main content.  The default is Visibility.Visible.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Visibility FooterSeparatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamScreenTip.FooterSeparatorVisibilityProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.FooterSeparatorVisibilityProperty, value);
			}
		}

				#endregion //FooterSeparatorVisibility

				#region FooterTemplate

		/// <summary>
		/// Identifies the <see cref="FooterTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register("FooterTemplate",
			typeof(DataTemplate), typeof(XamScreenTip), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the data template used to display the <see cref="Footer"/> content.  The default value is null.
		/// </summary>
		/// <seealso cref="FooterTemplateProperty"/>
		/// <seealso cref="FooterSeparatorVisibility"/>
		/// <seealso cref="Footer"/>
		/// <seealso cref="FooterTemplateSelector"/>
		/// <seealso cref="Header"/>
		//[Description("Returns/sets the data template used to display the footer content.  The default value is null.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public DataTemplate FooterTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamScreenTip.FooterTemplateProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.FooterTemplateProperty, value);
			}
		}

				#endregion //FooterTemplate

				#region FooterTemplateSelector

		/// <summary>
		/// Identifies the <see cref="FooterTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FooterTemplateSelectorProperty = DependencyProperty.Register("FooterTemplateSelector",
			typeof(DataTemplateSelector), typeof(XamScreenTip), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the data template selector to provide customer template selection logic for the <see cref="FooterTemplate"/>.
		/// </summary>
		/// <seealso cref="FooterTemplateSelectorProperty"/>
		/// <seealso cref="FooterTemplate"/>
		/// <seealso cref="FooterSeparatorVisibility"/>
		/// <seealso cref="Footer"/>
		/// <seealso cref="Header"/>
		//[Description("Returns/sets the data template selector to provide customer template selection logic for the footer template.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public DataTemplateSelector FooterTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(XamScreenTip.FooterTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.FooterTemplateSelectorProperty, value);
			}
		}

				#endregion //FooterTemplateSelector

				#region Header

		/// <summary>
		/// Identifies the <see cref="Header"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header",
			typeof(object), typeof(XamScreenTip), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the content for the header area of the XamScreenTip.  The default is null.
		/// </summary>
		/// <remarks>
		/// <p class="body">The header area appears at the bottom of the tooltip below the main tooltip content.  If set to null, no header is displayed.</p>
		/// </remarks>
		/// <seealso cref="HeaderProperty"/>
		/// <seealso cref="HeaderTemplate"/>
		/// <seealso cref="HeaderSeparatorVisibility"/>
		/// <seealso cref="HeaderTemplateSelector"/>
		/// <seealso cref="Footer"/>
		//[Description("Returns/sets the content for the header area of the XamScreenTip.")]
		//[Category("Data")]
		[Bindable(true)]
		public object Header
		{
			get
			{
				return (object)this.GetValue(XamScreenTip.HeaderProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.HeaderProperty, value);
			}
		}

				#endregion //Header

				#region HeaderSeparatorVisibility

		/// <summary>
		/// Identifies the <see cref="HeaderSeparatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderSeparatorVisibilityProperty = DependencyProperty.Register("HeaderSeparatorVisibility",
			typeof(Visibility), typeof(XamScreenTip), new FrameworkPropertyMetadata(Visibility.Collapsed));

		/// <summary>
		/// Returns/sets the Visibility of the <see cref="Header"/> separator which divides the header area from the main content.  The default is Visibility.Collapsed.
		/// </summary>
		/// <seealso cref="HeaderSeparatorVisibilityProperty"/>
		/// <seealso cref="Header"/>
		/// <seealso cref="HeaderTemplate"/>
		/// <seealso cref="HeaderTemplateSelector"/>
		/// <seealso cref="Footer"/>
		//[Description("Returns/sets the Visibility of the header separator which divides the header area from the main content.  The default is Visibility.Collapsed.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Visibility HeaderSeparatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamScreenTip.HeaderSeparatorVisibilityProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.HeaderSeparatorVisibilityProperty, value);
			}
		}

				#endregion //HeaderSeparatorVisibility

				#region HeaderTemplate

		/// <summary>
		/// Identifies the <see cref="HeaderTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate",
			typeof(DataTemplate), typeof(XamScreenTip), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the data template used to display the <see cref="Header"/> content.  The default value is null.
		/// </summary>
		/// <seealso cref="HeaderTemplateProperty"/>
		/// <seealso cref="HeaderSeparatorVisibility"/>
		/// <seealso cref="Header"/>
		/// <seealso cref="HeaderTemplateSelector"/>
		/// <seealso cref="Footer"/>
		//[Description("Returns/sets the data template used to display the header content.  The default value is null.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public DataTemplate HeaderTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamScreenTip.HeaderTemplateProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.HeaderTemplateProperty, value);
			}
		}

				#endregion //HeaderTemplate

				#region HeaderTemplateSelector

		/// <summary>
		/// Identifies the <see cref="HeaderTemplateSelector"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register("HeaderTemplateSelector",
			typeof(DataTemplateSelector), typeof(XamScreenTip), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the data template selector to provide customer template selection logic for the <see cref="HeaderTemplate"/>.
		/// </summary>
		/// <seealso cref="HeaderTemplateSelectorProperty"/>
		/// <seealso cref="HeaderTemplate"/>
		/// <seealso cref="HeaderSeparatorVisibility"/>
		/// <seealso cref="Header"/>
		/// <seealso cref="Footer"/>
		//[Description("Returns/sets the data template selector to provide customer template selection logic for the header template.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public DataTemplateSelector HeaderTemplateSelector
		{
			get
			{
				return (DataTemplateSelector)this.GetValue(XamScreenTip.HeaderTemplateSelectorProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.HeaderTemplateSelectorProperty, value);
			}
		}

				#endregion //HeaderTemplateSelector

				#region Theme

		#region Old Version
		
#region Infragistics Source Cleanup (Region)
















































































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //Old Version
		/// <summary>
		/// Identifies the 'Theme' dependency property
		/// </summary>
		public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamScreenTip), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

		/// <summary>
		/// Event ID for the 'ThemeChanged' routed event
		/// </summary>
		public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamScreenTip));

		private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamScreenTip control = target as XamScreenTip;

			// AS 9/4/09 TFS21087
			// Do not call UpdateLayout while the control is being initialized.
			//
			if (control.IsInitialized)
			{
				// JJD 2/26/07
				// we need to call UpdateLayout after we change the merged dictionaries.
				// Otherwise, the styles from the new merged dictionary are not picked
				// up right away. It seems the framework must be caching some information
				// that doesn't get refreshed until the next layout update
				control.InvalidateMeasure();
				control.UpdateLayout();
			}

			control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
		}

		/// <summary>
		/// Gets/sets the default look for the control.
		/// </summary>
		/// <remarks>
		/// <para class="body">If left set to null then the default 'Generic' theme will be used. 
		/// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b> The following themes are pre-registered by this assembly but additional themes can be registered as well.
		/// <ul>
		/// <li>"Aero" - a theme that is compatible with Vista's 'Aero' theme.</li>
		/// <li>"Generic" - the default theme.</li>
		/// <li>"LunaNormal" - a theme that is compatible with XP's 'blue' theme.</li>
		/// <li>"LunaOlive" - a theme that is compatible with XP's 'olive' theme.</li>
		/// <li>"LunaSilver" - a theme that is compatible with XP's 'silver' theme.</li>
		/// <li>"Office2k7Black" - a theme that is compatible with MS Office 2007's 'Black' theme.</li>
		/// <li>"Office2k7Blue" - a theme that is compatible with MS Office 2007's 'Blue' theme.</li>
		/// <li>"Office2k7Silver" - a theme that is compatible with MS Office 2007's 'Silver' theme.</li>
		/// <li>"Onyx" - a theme that features black and orange highlights.</li>
		/// <li>"Royale" - a theme that features subtle blue highlights.</li>
		/// <li>"RoyaleStrong" - a theme that features strong blue highlights.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
		/// <seealso cref="ThemeProperty"/>
		//[Description("Gets/sets the general look of the control")]
		//[Category("Appearance")]
		[Bindable(true)]
		[DefaultValue((string)null)]
		[TypeConverter(typeof(ThemeTypeConverter))]
		public string Theme
		{
			get
			{
				return (string)this.GetValue(XamScreenTip.ThemeProperty);
			}
			set
			{
				this.SetValue(XamScreenTip.ThemeProperty, value);
			}
		}

		/// <summary>
		/// Called when property 'Theme' changes
		/// </summary>
		protected virtual void OnThemeChanged(string previousValue, string currentValue)
		{
			RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
			newEvent.RoutedEvent = XamScreenTip.ThemeChangedEvent;
			newEvent.Source = this;
			RaiseEvent(newEvent);
		}

		/// <summary>
		/// Occurs when the 'Theme' property changes
		/// </summary>
		//[Description("Occurs when the 'Theme' property changes")]
		//[Category("Behavior")]
		public event RoutedPropertyChangedEventHandler<string> ThemeChanged
		{
			add
			{
				base.AddHandler(XamScreenTip.ThemeChangedEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamScreenTip.ThemeChangedEvent, value);
			}
		}

				#endregion //Theme

			#endregion //Public Properties

		#endregion //Properties
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