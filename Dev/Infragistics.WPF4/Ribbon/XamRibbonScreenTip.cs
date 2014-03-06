using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Licensing;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A class derived from XamScreenTip that is designed to be used with the XamRibbon control.  
	/// </summary>
	/// <remarks>
	/// <p class="body">This derived version of the <see cref="Infragistics.Windows.Controls.XamScreenTip"/> enforces the Office 2007 UI Design Guidelines related to screentip width sizing.  
	/// Specifically, if the <see cref="ContentImage"/> property (added in this derived class) is set, the XamRibbonScreenTip will set the overall width of the 
	/// control to 318 pixels.  If no image is specified, the overall width of the control will be set to 210 pixels.</p>
	/// <p class="body">In addition, the XamRibbonScreenTip control will set its theming based on the theme being used by the XamRibbon control, and like the XamRibbon, will not respond to 
	/// system theme changes and instead use the theme explicitly set via its Theme property.</p>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.Controls.XamScreenTip"/>
	/// <seealso cref="ContentImage"/>
		// AS 11/7/07 BR21903
		// AS 11/7/07 BR21903
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class XamRibbonScreenTip : XamScreenTip
	{
		#region Member Variables

		// AS 11/7/07 BR21903
		private Infragistics.Windows.Licensing.UltraLicense _license;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="XamRibbonScreenTip"/> class.
		/// </summary>
		public XamRibbonScreenTip()
		{
			// AS 11/7/07 BR21903
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamRibbonScreenTip), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }
		}

		static XamRibbonScreenTip()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamRibbonScreenTip), new FrameworkPropertyMetadata(typeof(XamRibbonScreenTip)));

			// AS 5/9/08
			// Instead of overriding a member from the base class, we need to register the groupings that 
			// should be applied when the theme property is changed.
			//
			ThemeManager.RegisterGroupings(typeof(XamRibbonScreenTip), new string[] { PrimitivesGeneric.Location.Grouping, EditorsGeneric.Location.Grouping, RibbonGeneric.Location.Grouping });
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region GetThemeGroupings
		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			#endregion //GetThemeGroupings	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region ContentImage

		/// <summary>
		/// Identifies the <see cref="ContentImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentImageProperty = DependencyProperty.Register("ContentImage",
			typeof(ImageSource), typeof(XamRibbonScreenTip), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContentImageChanged)));

		private static void OnContentImageChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			XamRibbonScreenTip xamRibbonScreenTip = target as XamRibbonScreenTip;
			if (xamRibbonScreenTip != null)
				xamRibbonScreenTip.SetValue(XamRibbonScreenTip.HasContentImagePropertyKey, e.NewValue != null);
		}

		/// <summary>
		/// Returns/sets the image associated with the content area of the XamScreenTip. The default value is null.
		/// </summary>
		/// <remarks>
		/// <p class="body">If an image is specified, the total width of the XamRibbonScreenTip will be set to 318 pixels as specified in the Office 2007 UI 
		/// Design Guidelines published by Microsoft.  If no image is specified, the width of the XamRibbonScreenTip will be set to 210 pixels.</p>
		/// </remarks>
		/// <seealso cref="ContentImageProperty"/>
		/// <seealso cref="HasContentImage"/>
		//[Description("Returns/sets the image associated with the content area of the XamScreenTip.  The default value is null.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource ContentImage
		{
			get
			{
				return (ImageSource)this.GetValue(XamRibbonScreenTip.ContentImageProperty);
			}
			set
			{
				this.SetValue(XamRibbonScreenTip.ContentImageProperty, value);
			}
		}

				#endregion //ContentImage

				#region HasContentImage

		private static readonly DependencyPropertyKey HasContentImagePropertyKey =
			DependencyProperty.RegisterReadOnly("HasContentImage",
			typeof(bool), typeof(XamRibbonScreenTip), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasContentImage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasContentImageProperty =
			HasContentImagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="ContentImage"/> property is set otherwise returns false.
		/// </summary>
		/// <remarks>
		/// <p class="body">If an image is specified, the total width of the XamRibbonScreenTip will be set to 318 pixels as specified in the Office 2007 UI 
		/// Design Guidelines published by Microsoft.  If no image is specified, the width of the XamRibbonScreenTip will be set to 210 pixels.</p>
		/// </remarks>
		/// <seealso cref="ContentImage"/>
		[Bindable(true)]
		[ReadOnly(true)]
		[Browsable(false)]
		public bool HasContentImage
		{
			get
			{
				return (bool)this.GetValue(XamRibbonScreenTip.HasContentImageProperty);
			}
		}

				#endregion //HasContentImage

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