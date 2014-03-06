using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Security.Permissions;

namespace Infragistics.Windows.Scrolling
{
	/// <summary>
	/// Provides a mechanism to receive notifications when the thumb of a ScrollViewer scrollbar is being dragged.
	/// </summary>
	public static class DeferredScrollService
	{
		#region Member Variables

		private static readonly DependencyProperty DeferredScrollPanel = DependencyProperty.Register("DeferredScrollPanel", typeof(IDeferredScrollPanel), typeof(IDeferredScrollHost));

		#endregion //Member Variables

		#region Methods

		#region Public

		#region RegisterDeferredScrollHost
		/// <summary>
		/// Method used to register a class type that supports deferred scrolling.
		/// </summary>
		/// <param name="classType">The UIElement or ContentElement type that implements the <see cref="IDeferredScrollHost"/> interface.</param>
		/// <exception cref="ArgumentException"><paramref name="classType"/> does not derive from <see cref="UIElement"/> or <see cref="ContentElement"/> or does not implement <see cref="IDeferredScrollHost"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="classType"/> is a null reference (<b>Nothing</b> in Visual Basic)</exception>
		public static void RegisterDeferredScrollHost(Type classType)
		{
			if (null == classType)
				throw new ArgumentNullException("classType");

			if (false == typeof(IDeferredScrollHost).IsAssignableFrom(classType))
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_5" ) );

			EventManager.RegisterClassHandler(classType, Thumb.DragStartedEvent, new DragStartedEventHandler(DeferredScrollService.OnThumbDragStarted), true);
			EventManager.RegisterClassHandler(classType, Thumb.DragCompletedEvent, new DragCompletedEventHandler(DeferredScrollService.OnThumbDragCompleted), true);
		}
		#endregion //RegisterDeferredScrollHost

		#endregion //Public

		#region Private

		#region OnThumbDragCompleted
		private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
		{
			DependencyObject obj = sender as DependencyObject;

			IDeferredScrollPanel panel = obj.GetValue(DeferredScrollPanel) as IDeferredScrollPanel;

			if (null == panel)
				return;

			panel.OnThumbDragComplete(e.Canceled);
		}
		#endregion //OnThumbDragCompleted

		#region OnThumbDragStarted
		private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
		{
			IDeferredScrollHost scrollHost = sender as IDeferredScrollHost;

			IDeferredScrollPanel panel = scrollHost.ScrollPanel;

			if (null == panel)
				return;

			Thumb thumb = e.OriginalSource as Thumb;

			if (null == thumb)
				return;

			ScrollBar scrollBar = thumb.TemplatedParent as ScrollBar;

			// make sure the panel could support deferred scrolling in the specified orientation
			if (scrollBar == null)
				return;

			// make sure its in a scroll viewer
			ScrollViewer scrollViewer = scrollBar.TemplatedParent as ScrollViewer;

			if (null == scrollViewer)
				return;

			if (panel.SupportsDeferredScrolling(scrollBar.Orientation, scrollViewer))
			{
				// store the panel for the host
				DependencyObject obj = scrollHost as DependencyObject;
				obj.SetValue(DeferredScrollPanel, panel);

				// AS 3/19/07 BR21256
				// We were previously passing along the thumb rect in screen coordinates
				// but that requires ui permissions so instead we'll pass along the thumb
				// itself so the callee can use translatepoint, etc. to translate the rect
				// relative to itself.
				//
				// tell the panel to start dealing with deferred scrolling
				panel.OnThumbDragStart(thumb, scrollBar.Orientation);
			}
		}
		#endregion //OnThumbDragStarted

		#endregion //Private

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