using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Schedules;
using System.Windows.Automation.Provider;
using System.Collections.Generic;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Abstract base class for automation peers of classes that derive from <see cref="ScheduleControlBase"/>
	/// </summary>
	public abstract class ScheduleControlBaseAutomationPeer : FrameworkElementAutomationPeer, 
		ISelectionProvider
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScheduleControlBaseAutomationPeer"/>
		/// </summary>
		/// <param name="owner">The <see cref="ScheduleControlBase"/> that the peer represents</param>
		protected ScheduleControlBaseAutomationPeer(ScheduleControlBase owner)
			: base(owner)
		{
		}
		#endregion // Constructor

		#region Base class overrides

		#region GetPattern
		/// <summary>
		/// Returns the control pattern for the element that is associated with this peer.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		/// <returns>The pattern provider or null</returns>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Selection)
				return this;

			return base.GetPattern(patternInterface);
		}
		#endregion // GetPattern

		#endregion //Base class overrides	

		#region Methods

		#region RaiseSelectionEvents
		internal void RaiseSelectionEvents()
		{
			this.RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
		}
		#endregion // RaiseSelectionEvents

		#endregion //Methods	
    
		#region ISelectionProvider Members

		bool ISelectionProvider.CanSelectMultiple
		{
			get { return true; }
		}

		IRawElementProviderSimple[] ISelectionProvider.GetSelection()
		{
			ScheduleControlBase control = this.Owner as ScheduleControlBase;

			SelectedActivityCollection selectedActivities = control.SelectedActivities;

			int count = selectedActivities.Count;

			if (count == 0)
				return new IRawElementProviderSimple[0];

			List<IRawElementProviderSimple> elementProviders = new List<IRawElementProviderSimple>();

			for (int i = 0; i < count; i++)
			{
				ActivityBase activity = selectedActivities[i];

				ActivityPresenter presenter = control.GetActivityPresenter(activity);

				if ( presenter != null )
					elementProviders.Add(this.ProviderFromPeer(FrameworkElementAutomationPeer.CreatePeerForElement(presenter)));
			}

			return elementProviders.ToArray();
		}

		bool ISelectionProvider.IsSelectionRequired
		{
			get { return false; }
		}

		#endregion
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