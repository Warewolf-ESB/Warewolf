using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Helper class for dealing with brushes.
	/// </summary>
	public class BrushHelper
	{
		#region Constructor
		/// <summary>
		/// This member supports the Infragistics infrastructure and is not meant to be invoked externally.
		/// </summary>
		protected BrushHelper()
		{
		} 
		#endregion //Constructor

		#region Background

		/// <summary>
		/// Background Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty BackgroundProperty =
			DependencyProperty.RegisterAttached("Background", typeof(Brush), typeof(BrushHelper),
				new FrameworkPropertyMetadata((Brush)null));

		/// <summary>
		/// Returns an extra brush that may be used for the background.
		/// </summary>
		/// <remarks>
		/// <p class="body">This attached property can be used for any purpose but was initially 
		/// added to be used as the target of a local style trigger that would ultimately be 
		/// referenced by a style setter. In this way, the triggered value would not override 
		/// (based on the WPF dependency property precedence) the value of ControlTemplate 
		/// triggers.</p>
		/// </remarks>
		public static Brush GetBackground(DependencyObject d)
		{
			return (Brush)d.GetValue(BackgroundProperty);
		}

		/// <summary>
		/// Set the attached <see cref="BackgroundProperty"/>
		/// </summary>
		public static void SetBackground(DependencyObject d, Brush value)
		{
			d.SetValue(BackgroundProperty, value);
		}

		#endregion //Background

		#region BorderBrush

		/// <summary>
		/// BorderBrush Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty BorderBrushProperty =
			DependencyProperty.RegisterAttached("BorderBrush", typeof(Brush), typeof(BrushHelper),
				new FrameworkPropertyMetadata((Brush)null));

		/// <summary>
		/// Returns an extra brush that may be used for the BorderBrush.
		/// </summary>
		/// <remarks>
		/// <p class="body">This attached property can be used for any purpose but was initially 
		/// added to be used as the target of a local style trigger that would ultimately be 
		/// referenced by a style setter. In this way, the triggered value would not override 
		/// (based on the WPF dependency property precedence) the value of ControlTemplate 
		/// triggers.</p>
		/// </remarks>
		public static Brush GetBorderBrush(DependencyObject d)
		{
			return (Brush)d.GetValue(BorderBrushProperty);
		}

		/// <summary>
		/// Set the attached <see cref="BorderBrushProperty"/>
		/// </summary>
		public static void SetBorderBrush(DependencyObject d, Brush value)
		{
			d.SetValue(BorderBrushProperty, value);
		}

		#endregion //BorderBrush
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