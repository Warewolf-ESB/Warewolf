using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="ToolMenuItem"/> types to UI Automation
    /// </summary>
    public class ToolMenuItemAutomationPeer : MenuItemAutomationPeer 
		, IToggleProvider // AS 1/11/12 TFS30681
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="ToolMenuItemAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="ToolMenuItem"/> for which the peer is being created</param>
        public ToolMenuItemAutomationPeer(ToolMenuItem owner)
            : base(owner)
        {
        }
        #endregion //Constructor 

        #region Base class overrides

            #region GetChildrenCore
        /// <summary>
        /// Returns the collection of child elements of the <see cref="ToolMenuItem"/> that is associated with this <see cref="ToolMenuItemAutomationPeer"/>
        /// </summary>
        /// <returns>The collection of child elements</returns>
        /// <remarks>
        /// <p class="body">The ToolMenuItem peer returns list of base implementation of MenuItem plus 
        /// Automation peer for embedded <see cref="ToolMenuItem.Tool"/></p>
        /// </remarks> 
        protected override List<AutomationPeer> GetChildrenCore()
        {
            ToolMenuItem toolMenu = this.Owner as ToolMenuItem;

            // get base list
            List<AutomationPeer> list = base.GetChildrenCore();
            if (list == null)
                list = new List<AutomationPeer>();

            // we dont need to add items control item because the base class do it.
            if ((toolMenu.Tool != null)&&((toolMenu.Tool is ItemsControl)==false))
            {
                // Create AutomationPeer for ribbon tool within the menuitem
                AutomationPeer peer = UIElementAutomationPeer.FromElement(toolMenu.Tool as UIElement);
                if (peer == null)
                    peer = UIElementAutomationPeer.CreatePeerForElement(toolMenu.Tool as UIElement);

                if (peer != null)
                    list.Add(peer);
            }
            return list;
        }
        #endregion // GetChildrenCore

            #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="ToolMenuItem"/>
        /// </summary>
        /// <returns>A string that contains 'ToolMenuItem'</returns>
        protected override string GetClassNameCore()
        {
            return "ToolMenuItem";
        }

            #endregion //GetClassNameCore

            #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="DropDownToggle"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        /// <returns>Object that implement IToggleProvider pattern</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
			ToolMenuItem tmi = this.Owner as ToolMenuItem;

			// AS 1/11/12 TFS30681
			// Since we don't (and can't) set the IsCheckable of the ToolMenuItem, we need 
			// to provide the implementation for the Toggle pattern.
			//
			if (patternInterface == PatternInterface.Toggle && tmi.IsCheckableInternal)
                return this;

            return base.GetPattern(patternInterface);
        }
            #endregion // GetPattern

        #endregion //Base class overrides	
    
		#region Methods

		// AS 1/11/12 TFS30681
		#region RaiseToggleStatePropertyChangedEvent
		internal void RaiseToggleStatePropertyChangedEvent(bool oldValue, bool newValue)
		{
			if (oldValue != newValue)
				this.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, ToToggleState(oldValue), ToToggleState(newValue));
		} 
		#endregion //RaiseToggleStatePropertyChangedEvent

		// AS 1/11/12 TFS30681
		#region ToToggleState
		private static ToggleState ToToggleState(bool value)
		{
			return value ? ToggleState.On : ToggleState.Off;
		} 
		#endregion //ToToggleState

		#endregion //Methods

		// AS 1/11/12 TFS30681
		#region IToggleProvider Members

		void IToggleProvider.Toggle()
		{
			if (!this.IsEnabled())
				throw new ElementNotEnabledException();

			ToolMenuItem tmi = this.Owner as ToolMenuItem;

			if (!tmi.IsCheckableInternal)
				throw new InvalidOperationException();

			tmi.IsChecked = !tmi.IsChecked;
		}

		ToggleState IToggleProvider.ToggleState
		{
			get 
			{ 
				ToolMenuItem tmi = this.Owner as ToolMenuItem;
				return ToToggleState(tmi.IsChecked);
			}
		}
		#endregion //IToggleProvider Members
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