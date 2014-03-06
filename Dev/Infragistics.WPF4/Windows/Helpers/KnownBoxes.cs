using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;




namespace Infragistics

{
	/// <summary>
	/// Class that maintains references to commonly boxed values such as booleans.
	/// </summary>



	public static class KnownBoxes

	{
		#region Constants

		#region bool
		/// <summary>
		/// Returns an object containing the boolean 'true'
		/// </summary>
		public static readonly object TrueBox = true;

		/// <summary>
		/// Returns an object containing the boolean 'false'
		/// </summary>
		public static readonly object FalseBox = false;

				#endregion //bool

		#region ScrollBarVisibility
		/// <summary>
		/// Returns an object containing the <see cref="ScrollBarVisibility"/> 'Hidden'.
		/// </summary>
		public static readonly object ScrollBarVisibilityHiddenBox = ScrollBarVisibility.Hidden;

		/// <summary>
		/// Returns an object containing the <see cref="ScrollBarVisibility"/> 'Visible'.
		/// </summary>
		public static readonly object ScrollBarVisibilityVisibleBox = ScrollBarVisibility.Visible;

		/// <summary>
		/// Returns an object containing the <see cref="ScrollBarVisibility"/> 'Disabled'.
		/// </summary>
		public static readonly object ScrollBarVisibilityDisabledBox = ScrollBarVisibility.Disabled;

		/// <summary>
		/// Returns an object containing the <see cref="ScrollBarVisibility"/> 'Auto'.
		/// </summary>
		public static readonly object ScrollBarVisibilityAutoBox = ScrollBarVisibility.Auto;

		#endregion //ScrollBarVisibility

		#region Visibility

		/// <summary>
		/// Returns an object containing the <see cref="Visibility"/> 'Hidden'.
		/// </summary>
		public static readonly object VisibilityHiddenBox = Visibility.Hidden;


		/// <summary>
		/// Returns an object containing the <see cref="Visibility"/> 'Visible'.
		/// </summary>
		public static readonly object VisibilityVisibleBox = Visibility.Visible;

		/// <summary>
		/// Returns an object containing the <see cref="Visibility"/> 'Hidden'.
		/// </summary>
		public static readonly object VisibilityCollapsedBox = Visibility.Collapsed;

		#endregion //Visibility

		#region Orientation
		/// <summary>
		/// Returns an object containing the <see cref="Orientation"/> 'Vertical'.
		/// </summary>
		public static readonly object OrientationVerticalBox = Orientation.Vertical;

		/// <summary>
		/// Returns an object containing the <see cref="Orientation"/> 'Horizontal'.
		/// </summary>
		public static readonly object OrientationHorizontalBox = Orientation.Horizontal;

		#endregion //Orientation

		#region HorizontalAlignment
		/// <summary>
		/// Returns an object containing the <see cref="HorizontalAlignment"/> 'Center'.
		/// </summary>
		public static readonly object HorizontalAlignmentCenterBox = HorizontalAlignment.Center;

		/// <summary>
		/// Returns an object containing the <see cref="HorizontalAlignment"/> 'Left'.
		/// </summary>
		public static readonly object HorizontalAlignmentLeftBox = HorizontalAlignment.Left;

		/// <summary>
		/// Returns an object containing the <see cref="HorizontalAlignment"/> 'Right'.
		/// </summary>
		public static readonly object HorizontalAlignmentRightBox = HorizontalAlignment.Right;

		/// <summary>
		/// Returns an object containing the <see cref="HorizontalAlignment"/> 'Stretch'.
		/// </summary>
		public static readonly object HorizontalAlignmentStretchBox = HorizontalAlignment.Stretch;

		#endregion //HorizontalAlignment

		#region VerticalAlignment
		/// <summary>
		/// Returns an object containing the <see cref="VerticalAlignment"/> 'Center'.
		/// </summary>
		public static readonly object VerticalAlignmentCenterBox = VerticalAlignment.Center;

		/// <summary>
		/// Returns an object containing the <see cref="VerticalAlignment"/> 'Top'.
		/// </summary>
		public static readonly object VerticalAlignmentTopBox = VerticalAlignment.Top;

		/// <summary>
		/// Returns an object containing the <see cref="VerticalAlignment"/> 'Bottom'.
		/// </summary>
		public static readonly object VerticalAlignmentBottomBox = VerticalAlignment.Bottom;

		/// <summary>
		/// Returns an object containing the <see cref="VerticalAlignment"/> 'Stretch'.
		/// </summary>
		public static readonly object VerticalAlignmentStretchBox = VerticalAlignment.Stretch;

		#endregion //VerticalAlignment

		#region Dock

		/// <summary>
		/// Returns an object containing the <see cref="Dock"/> 'Top'.
		/// </summary>
		public static readonly object DockTopBox = Dock.Top;

		/// <summary>
		/// Returns an object containing the <see cref="Dock"/> 'Bottom'.
		/// </summary>
		public static readonly object DockBottomBox = Dock.Bottom;

		/// <summary>
		/// Returns an object containing the <see cref="Dock"/> 'Left'.
		/// </summary>
		public static readonly object DockLeftBox = Dock.Left;

		/// <summary>
		/// Returns an object containing the <see cref="Dock"/> 'Right'.
		/// </summary>
		public static readonly object DockRightBox = Dock.Right;

		#endregion //Dock

		/// <summary>
		/// A boxed instance for a double value of zero.
		/// </summary>
		internal static readonly object DoubleZeroBox = 0d;

		#endregion //Constants

		#region FromValue
		/// <summary>
		/// Returns a boxed representation of the specified <see cref="bool"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="bool"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="bool"/> value</returns>
		public static object FromValue(bool value)
		{
			if (value)
				return TrueBox;
			else
				return FalseBox;
		}

		/// <summary>
		/// Returns a boxed representation of the specified bool? value
		/// </summary>
		/// <param name="value">An instance of bool? for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified bool?/> value</returns>
		public static object FromValue(bool? value)
		{
			if (value == null)
				return null;
			else
				return FromValue(value.Value);
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="ScrollBarVisibility"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="ScrollBarVisibility"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="ScrollBarVisibility"/> value</returns>
		public static object FromValue(ScrollBarVisibility value)
		{
			switch (value)
			{
				default:
				case ScrollBarVisibility.Auto:
					return ScrollBarVisibilityAutoBox;
				case ScrollBarVisibility.Disabled:
					return ScrollBarVisibilityDisabledBox;
				case ScrollBarVisibility.Hidden:
					return ScrollBarVisibilityHiddenBox;
				case ScrollBarVisibility.Visible:
					return ScrollBarVisibilityVisibleBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="Visibility"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="Visibility"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="Visibility"/> value</returns>
		public static object FromValue(Visibility value)
		{
			switch (value)
			{
				case Visibility.Collapsed:
					return VisibilityCollapsedBox;

				case Visibility.Hidden:
					return VisibilityHiddenBox;

				default:
				case Visibility.Visible:
					return VisibilityVisibleBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="Orientation"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="Orientation"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="Orientation"/> value</returns>
		public static object FromValue(Orientation value)
		{
			switch (value)
			{
				default:
				case Orientation.Vertical:
					return OrientationVerticalBox;
				case Orientation.Horizontal:
					return OrientationHorizontalBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="HorizontalAlignment"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="HorizontalAlignment"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="HorizontalAlignment"/> value</returns>
		public static object FromValue(HorizontalAlignment value)
		{
			switch (value)
			{
				default:
				case HorizontalAlignment.Stretch:
					return HorizontalAlignmentStretchBox;
				case HorizontalAlignment.Center:
					return HorizontalAlignmentCenterBox;
				case HorizontalAlignment.Left:
					return HorizontalAlignmentLeftBox;
				case HorizontalAlignment.Right:
					return HorizontalAlignmentRightBox;
			}
		}

		/// <summary>
		/// Returns a boxed representation of the specified <see cref="VerticalAlignment"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="VerticalAlignment"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="VerticalAlignment"/> value</returns>
		public static object FromValue(VerticalAlignment value)
		{
			switch (value)
			{
				default:
				case VerticalAlignment.Stretch:
					return VerticalAlignmentStretchBox;
				case VerticalAlignment.Center:
					return VerticalAlignmentCenterBox;
				case VerticalAlignment.Top:
					return VerticalAlignmentTopBox;
				case VerticalAlignment.Bottom:
					return VerticalAlignmentBottomBox;
			}
		}


		/// <summary>
		/// Returns a boxed representation of the specified <see cref="Dock"/> value
		/// </summary>
		/// <param name="value">An instance of <see cref="Dock"/> for which a cached boxed value is to be returned</param>
		/// <returns>An object that wraps the specified <see cref="Dock"/> value</returns>
		public static object FromValue(Dock value)
		{
			switch (value)
			{
				default:
				case Dock.Top:
					return DockTopBox;
				case Dock.Bottom:
					return DockBottomBox;
				case Dock.Left:
					return DockLeftBox;
				case Dock.Right:
					return DockRightBox;
			}
		}		

		#endregion //FromValue
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