using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows;
using System.Security;
using System.Windows.Automation;

namespace Infragistics.AutomationPeers
{
	internal abstract class PeerProxy : AutomationPeer
	{
		#region Member Variables

		// AS 12/8/09 TFS24844
		private static readonly System.Reflection.MethodInfo _updateSubTreeMethod;
		private static bool _updateSubTreeFailed;

		#endregion //Member Variables

		#region Constructor

		static PeerProxy()
		{
			// AS 12/8/09 TFS24844
			try
			{
				_updateSubTreeMethod = typeof(AutomationPeer).GetMethod("UpdateSubtree", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			}
			catch
			{
				_updateSubTreeMethod = null;
			}
		}

		/// <summary>
		/// Creates a new instance of the <see cref="PeerProxy"/> class
		/// </summary>
		protected PeerProxy()
			: base()
		{
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetAcceleratorKeyCore
		/// <summary>
		/// Returns the accelerator key for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The accelerator key</returns>
		protected override string GetAcceleratorKeyCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetAcceleratorKey()
				: string.Empty;
		} 
		#endregion //GetAcceleratorKeyCore

		#region GetAccessKeyCore
		/// <summary>
		/// Returns the access key for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The access key</returns>
		protected override string GetAccessKeyCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetAccessKey()
				: string.Empty;
		} 
		#endregion //GetAccessKeyCore

		#region GetAutomationIdCore
		/// <summary>
		/// Returns the AutomationIdentifier for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The ui automation identifier</returns>
		protected override string GetAutomationIdCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetAutomationId()
				: string.Empty;
		} 
		#endregion //GetAutomationIdCore

		#region GetBoundingRectangleCore
		/// <summary>
		/// Returns the <see cref="Rect"/> that represents the bounding rectangle of the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The bounding rectangle</returns>
		protected override System.Windows.Rect GetBoundingRectangleCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetBoundingRectangle()
				// AS 8/28/09 TFS21509
				// An empty rect actually has an infinite width/height.
				//
				//: Rect.Empty;
				: new Rect();
		} 
		#endregion //GetBoundingRectangleCore

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the object that is associated with this <see cref="AutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();

			if (null != peer)
			{
				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

				if (!_updateSubTreeFailed && null != _updateSubTreeMethod)
				{
					try
					{
						_updateSubTreeMethod.Invoke(peer, Type.EmptyTypes);
					}
					catch (MemberAccessException)
					{
					    _updateSubTreeFailed = true;
					}
					catch (SecurityException)
					{
						_updateSubTreeFailed = true;
					}
				}

				return peer.GetChildren();
			}

			return null;
		}

		#endregion //GetChildrenCore

		#region GetClickablePointCore
		/// <summary>
		/// Returns the <see cref="Point"/> that represents the clickable space for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The point that represents the clickable space on the element</returns>
		protected override System.Windows.Point GetClickablePointCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetClickablePoint()
				: new Point(double.NaN, double.NaN);
		} 
		#endregion //GetClickablePointCore

		#region GetHelpTextCore
		/// <summary>
		/// Returns the string that describes the functionality of the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The help text</returns>
		protected override string GetHelpTextCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetHelpText()
				: string.Empty;
		} 
		#endregion //GetHelpTextCore

		#region GetItemStatusCore
		/// <summary>
		/// Returns a string that conveys the parent status for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The status</returns>
		protected override string GetItemStatusCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetItemStatus()
				: string.Empty;
		} 
		#endregion //GetItemStatusCore

		#region GetItemTypeCore
		/// <summary>
		/// Returns a human readable string that contains the type of item for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The item type</returns>
		protected override string GetItemTypeCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetItemType()
				: null;
		} 
		#endregion //GetItemTypeCore

		#region GetLabeledByCore
		/// <summary>
		/// Returns the <see cref="AutomationPeer"/> for the Label that is targeted to the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The AutomationPeer of the Label that targets this element</returns>
		protected override AutomationPeer GetLabeledByCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetLabeledBy()
				: null;
		} 
		#endregion //GetLabeledByCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetName()
				: string.Empty;
		} 
		#endregion //GetNameCore

		#region GetOrientationCore
		/// <summary>
		/// Returns the value that indicates the direction in which the <see cref="UIElement"/> is laid out.
		/// </summary>
		/// <returns>The direction of the <see cref="UIElement"/> or <b>AutomationOrientation.None</b> if no direction is specified</returns>
		protected override AutomationOrientation GetOrientationCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetOrientation()
				: AutomationOrientation.None;
		} 
		#endregion //GetOrientationCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.GetPattern(patternInterface)
				: null;
		} 
		#endregion //GetPattern

		#region HasKeyboardFocusCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> currently has the keyboard input focus.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> has the keyboard input focus; otherwise <b>false</b>.</returns>
		protected override bool HasKeyboardFocusCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.HasKeyboardFocus()
				: false;
		} 
		#endregion //HasKeyboardFocusCore

		#region IsContentElementCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> contains data that is presented to the user.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> is a content element; otherwise, <b>false</b>.</returns>
		protected override bool IsContentElementCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsContentElement()
				: true;
		} 
		#endregion //IsContentElementCore

		#region IsControlElementCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is understood by the end user as interactive.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> is a control; otherwise, <b>false</b>.</returns>
		protected override bool IsControlElementCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsControlElement()
				: true;
		} 
		#endregion //IsControlElementCore

		#region IsEnabledCore
		/// <summary>
		/// Returns a value indicating whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can receive and send events.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="UIElement"/> can send and receive events; otherwise, <b>false</b>.</returns>
		protected override bool IsEnabledCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsEnabled()
				: false;
		} 
		#endregion //IsEnabledCore

		#region IsKeyboardFocusableCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can accept keyboard focus.
		/// </summary>
		/// <returns><b>True</b> if the element can accept keyboard focus; otherwise, <b>false</b>.</returns>
		protected override bool IsKeyboardFocusableCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsKeyboardFocusable()
				: false;
		} 
		#endregion //IsKeyboardFocusableCore

		#region IsOffscreenCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is off the screen.
		/// </summary>
		/// <returns><b>True</b> if the element is off the screen; otherwise, <b>false</b>.</returns>
		protected override bool IsOffscreenCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsOffscreen()
				: true;
		} 
		#endregion //IsOffscreenCore

		#region IsPasswordCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> contains protected content.
		/// </summary>
		/// <returns><b>True</b> if the element contains protected content; otherwise, <b>false</b>.</returns>
		protected override bool IsPasswordCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsPassword()
				: false;
		} 
		#endregion //IsPasswordCore

		#region IsRequiredForFormCore
		/// <summary>
		/// Returns a value that indicates whether the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> is required to be completed on the form.
		/// </summary>
		/// <returns><b>True</b> if the element is required to be completed; otherwise, <b>false</b>.</returns>
		protected override bool IsRequiredForFormCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			return peer != null
				? peer.IsRequiredForForm()
				: false;
		} 
		#endregion //IsRequiredForFormCore

		#region SetFocusCore
		/// <summary>
		/// Sets the keyboard input focus on the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		protected override void SetFocusCore()
		{
			AutomationPeer peer = this.GetUnderlyingPeer();
			if (peer != null)
			{
				peer.SetFocus();
			}
		} 
		#endregion //SetFocusCore

		#endregion //Base class overrides

		#region Methods

		/// <summary>
		/// Returns the automation peer for which this proxy is associated.
		/// </summary>
		/// <returns>An <see cref="AutomationPeer"/></returns>
		protected abstract AutomationPeer GetUnderlyingPeer();

		// AS 6/8/07 UI Automation
		#region ThrowIfNotEnabled
		/// <summary>
		/// Throws a <see cref="ElementNotEnabledException"/> if the element is not enabled.
		/// </summary>
		protected void ThrowIfNotEnabled()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();
		} 
		#endregion //ThrowIfNotEnabled

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