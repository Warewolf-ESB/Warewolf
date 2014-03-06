using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Infragistics.Windows.Themes;
using System.ComponentModel;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Provides the ability to perform a scroll operation for a <see cref="ScrollViewer"/> using the middle mouse button.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class MousePanningDecorator : AdornerDecorator
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="MousePanningDecorator"/>
		/// </summary>
		public MousePanningDecorator()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnMouseDown
		/// <summary>
		/// Overriden. Invoked when the mouse button is pressed down on the element. This method is used to initiate the mouse panning operation when the middle mouse button has been pressed down over a <see cref="ScrollViewer"/>.
		/// </summary>
		/// <param name="e">Mouse event arguments</param>
		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.Handled == false &&
				e.ChangedButton == MouseButton.Middle)
			{
				ScrollViewer scrollViewer = null;

				do
				{
					DependencyObject source = scrollViewer ?? e.OriginalSource as DependencyObject;
					scrollViewer = (ScrollViewer)GetAncestor(source, typeof(ScrollViewer), this);

				} while (scrollViewer != null && MousePanningAdorner.CanScroll(scrollViewer) == false);

				if (null != scrollViewer)
				{
					// start a panning operation
					Point point = e.GetPosition(scrollViewer);
					AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(scrollViewer);

					// it is possible that there is no adorner layer above it
					if (null != adornerLayer)
					{
						MousePanningAdorner adorner = new MousePanningAdorner(scrollViewer, point.X, point.Y);
						adornerLayer.Add(adorner);
						if (adorner.CaptureMouse())
						{
							e.Handled = true;
							return;
						}
						else
						{
							adornerLayer.Remove(adorner);
						}
					}
				}
			}

			base.OnMouseDown(e);
		}
		#endregion //OnMouseDown

		#endregion //Base class overrides

		#region Resource Keys

		#region ScrollNSEWImageKey

		/// <summary>
		/// The key used to identify the image used by the decorator when the <see cref="ScrollViewer"/> being panned can scroll in all directions (i.e. both vertically and horizontally).
		/// </summary>
		public static readonly ResourceKey ScrollAllImageKey = new StaticPropertyResourceKey(typeof(MousePanningDecorator), "ScrollAllImageKey");

		#endregion //ScrollAllImageKey

		#region ScrollNSImageKey

		/// <summary>
		/// The key used to identify the image used by the decorator when the <see cref="ScrollViewer"/> being panned can scroll vertically.
		/// </summary>
		public static readonly ResourceKey ScrollNSImageKey = new StaticPropertyResourceKey(typeof(MousePanningDecorator), "ScrollNSImageKey");

		#endregion //ScrollNSImageKey

		#region ScrollEWImageKey

		/// <summary>
		/// The key used to identify the image used by the decorator when the <see cref="ScrollViewer"/> being panned can scroll horizontally.
		/// </summary>
		public static readonly ResourceKey ScrollEWImageKey = new StaticPropertyResourceKey(typeof(MousePanningDecorator), "ScrollEWImageKey");

		#endregion //ScrollEWImageKey 

		#endregion //Resource Keys

		#region Methods

		#region GetAncestor
		private static DependencyObject GetAncestor(DependencyObject source, Type ancestorType, DependencyObject stopAtElement)
		{
			// JJD 8/23/07
			// Call the safer GetParent method that will tolerate FrameworkContentElements
			//DependencyObject ancestor = source != null ? VisualTreeHelper.GetParent(source) : null;
			DependencyObject ancestor = source != null ? Utilities.GetParent(source) : null;

			while (null != ancestor && ancestor != stopAtElement)
			{
				if (ancestorType.IsAssignableFrom(ancestor.GetType()))
					return ancestor;

				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
				//ancestor = VisualTreeHelper.GetParent(ancestor);
				ancestor = Utilities.GetParent(ancestor);
			}

			return null;
		}
		#endregion //GetAncestor

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