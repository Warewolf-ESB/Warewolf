using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// A content control that is visible to UI automation
	/// </summary>
	[DesignTimeVisible(false)]
	public class ContentControlWithAutomation : ContentControl
	{
		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="ContentControlWithAutomation"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="ActivityPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new ContentControlWithAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#endregion //Base class overrides	
		
		#region Properties

		#region AutomationControlType

		/// <summary>
		/// Identifies the <see cref="AutomationControlType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutomationControlTypeProperty = DependencyPropertyUtilities.Register("AutomationControlType",
			typeof(AutomationControlType), typeof(ContentControlWithAutomation),
			DependencyPropertyUtilities.CreateMetadata(AutomationControlType.Custom)
			);

		/// <summary>
		/// Returns or sets the AutomationControlType exposed thru UI automation
		/// </summary>
		/// <seealso cref="AutomationControlTypeProperty"/>
		public AutomationControlType AutomationControlType
		{
			get
			{
				return (AutomationControlType)this.GetValue(ContentControlWithAutomation.AutomationControlTypeProperty);
			}
			set
			{
				this.SetValue(ContentControlWithAutomation.AutomationControlTypeProperty, value);
			}
		}

		#endregion //AutomationControlType
		
		#endregion //Properties

	}
}

namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// The UI automation peer associated with a <see cref="ContentControlWithAutomation"/>
	/// </summary>
	public class ContentControlWithAutomationPeer : FrameworkElementAutomationPeer
	{
		internal ContentControlWithAutomationPeer(ContentControlWithAutomation owner)
			: base(owner)
		{
		}

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <see cref="ContentControlWithAutomation"/>.<see cref="ContentControlWithAutomation.AutomationControlType"/> property value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return ((ContentControlWithAutomation)(this.Owner)).AutomationControlType;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="ContentControlWithAutomation"/> class
		/// </summary>
		/// <returns>A string that contains 'ContentControlWithAutomation'</returns>
		protected override string GetClassNameCore()
		{
			return this.Owner.GetType().Name;
		}

		#endregion //GetClassNameCore

		#endregion //Base class overrides
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