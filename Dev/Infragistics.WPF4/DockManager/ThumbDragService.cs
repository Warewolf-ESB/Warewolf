using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.DockManager
{
	// FUTURE change to be part of mouse service and make it related to capture?
	// FUTURE change from capturing all keys to raising a bubble event?
	/// <summary>
	/// Static class used to intercept keystrokes for <see cref="Thumb"/> while in a drag operation and cancel the drag when the escape key is pressed.
	/// </summary>
	internal static class ThumbDragService
	{
		#region Constructor
		static ThumbDragService()
		{
			EventManager.RegisterClassHandler(typeof(Thumb), Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
			EventManager.RegisterClassHandler(typeof(Thumb), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));
		} 
		#endregion //Constructor

		#region Properties

		#region CancelThumbDragOnEscape

		/// <summary>
		/// Identifies the CancelThumbDragOnEscape attached dependency property
		/// </summary>
		/// <seealso cref="GetCancelThumbDragOnEscape"/>
		/// <seealso cref="SetCancelThumbDragOnEscape"/>
		public static readonly DependencyProperty CancelThumbDragOnEscapeProperty = DependencyProperty.RegisterAttached("CancelThumbDragOnEscape",
			typeof(bool), typeof(ThumbDragService), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Gets the value of the 'CancelThumbDragOnEscape' attached property
		/// </summary>
		/// <seealso cref="CancelThumbDragOnEscapeProperty"/>
		/// <seealso cref="SetCancelThumbDragOnEscape"/>
		public static bool GetCancelThumbDragOnEscape(DependencyObject d)
		{
			return (bool)d.GetValue(ThumbDragService.CancelThumbDragOnEscapeProperty);
		}

		/// <summary>
		/// Sets the value of the 'CancelThumbDragOnEscape' attached property
		/// </summary>
		/// <seealso cref="CancelThumbDragOnEscapeProperty"/>
		/// <seealso cref="GetCancelThumbDragOnEscape"/>
		public static void SetCancelThumbDragOnEscape(DependencyObject d, bool value)
		{
			d.SetValue(ThumbDragService.CancelThumbDragOnEscapeProperty, value);
		}

		#endregion //CancelThumbDragOnEscape

		#endregion //Properties

		#region Methods

		#region Focused Element Event Sinks
		private static void OnFocusedElementPreviewKeyUp(object sender, KeyEventArgs e)
		{
			e.Handled = true;
		}

		private static void OnFocusedElementChanged(object sender, KeyboardFocusChangedEventArgs e)
		{
			// unhook the old element
			UnhookElement(e.OldFocus);

			Thumb thumb = Mouse.Captured as Thumb;
			if (null != thumb && GetCancelThumbDragOnEscape(thumb))
				HookElement(e.NewFocus);
		}

		private static void OnFocusedElementPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				// get the thumb with capture
				Thumb thumb = Mouse.Captured as Thumb;

				if (null != thumb)
					thumb.ReleaseMouseCapture();
			}
			else
				e.Handled = true;
		}
		#endregion //Focused Element Event Sinks

		#region Hook/Unhook Element
		private static void HookElement(IInputElement element)
		{
			if (null != element)
			{
				element.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnFocusedElementChanged);
				element.PreviewKeyDown += new KeyEventHandler(OnFocusedElementPreviewKeyDown);
				element.PreviewKeyUp += new KeyEventHandler(OnFocusedElementPreviewKeyUp);
			}
		}

		private static void UnhookElement(IInputElement element)
		{
			if (null != element)
			{
				element.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnFocusedElementChanged);
				element.PreviewKeyDown -= new KeyEventHandler(OnFocusedElementPreviewKeyDown);
				element.PreviewKeyUp -= new KeyEventHandler(OnFocusedElementPreviewKeyUp);
			}
		}
		#endregion //Hook/Unhook Element

		#region OnThumbDragCompleted
		private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
		{
			// unhook from the focused element
			UnhookElement(Keyboard.FocusedElement);
		}
		#endregion //OnThumbDragCompleted

		#region OnThumbDragStarted
		private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
		{
			Thumb thumb = e.OriginalSource as Thumb;

			// if the thumb opts into this service then catch the
			// preview keydown and the lost keyboard focus of the 
			// currently focused element. in this way, we don't have
			// to steal focus from the active control but we can cancel
			// a drag when escape is pressed and also prevent any other
			// keystroke from getting through while we're processing
			// the drag
			if (null != thumb && GetCancelThumbDragOnEscape(thumb))
			{
				HookElement(Keyboard.FocusedElement);
			}
		}
		#endregion //OnThumbDragStarted

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