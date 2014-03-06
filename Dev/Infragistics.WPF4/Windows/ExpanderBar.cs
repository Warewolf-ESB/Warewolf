using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Button-derived element that displays a bar used for expanding/collapsing an area.
	/// </summary>
	//[Description("Button derived element that displays a bar used for expanding/collapsing an area.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ExpanderBar : Button
	{
		static ExpanderBar()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpanderBar), new FrameworkPropertyMetadata(typeof(ExpanderBar)));
		}

		#region Public Properties
		#region Styling Properties

		#region BackgroundHover

		/// <summary>
		/// Identifies the <see cref="BackgroundHover"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BackgroundHoverProperty = DependencyProperty.Register("BackgroundHover",
			typeof(Brush), typeof(ExpanderBar), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Brush applied when IsMouseOver = true.
		/// </summary>
		/// <seealso cref="BackgroundHoverProperty"/>
		//[Description("Brush applied when IsMouseOver = true.")]
		//[Category("Brushes")]
		public Brush BackgroundHover
		{
			get
			{
				return (Brush)this.GetValue(ExpanderBar.BackgroundHoverProperty);
			}
			set
			{
				this.SetValue(ExpanderBar.BackgroundHoverProperty, value);
			}
		}

		#endregion BackgroundHover		
				
		#region BorderHoverBrush

		/// <summary>
		/// Identifies the <see cref="BorderHoverBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BorderHoverBrushProperty = DependencyProperty.Register("BorderHoverBrush",
			typeof(Brush), typeof(ExpanderBar), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The border brush applied to the background area when IsMouseOver = true.
		/// </summary>
		/// <seealso cref="BorderHoverBrushProperty"/>
		//[Description("The border brush applied to the background area when IsMouseOver = true.")]
		//[Category("Brushes")]
		public Brush BorderHoverBrush
		{
			get
			{
				return (Brush)this.GetValue(ExpanderBar.BorderHoverBrushProperty);
			}
			set
			{
				this.SetValue(ExpanderBar.BorderHoverBrushProperty, value);
			}
		}

		#endregion BorderHoverBrush		

		#endregion Styling Properties
		#endregion // Public Properties
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