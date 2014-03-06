using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Editors;
using System.Windows.Automation.Peers;
using System.Collections;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.Editors
{
	/// <summary>
	/// Exposes <see cref="XamComboEditor"/> types to UI Automation
	/// </summary>
    public class XamComboEditorAutomationPeer : ValueEditorAutomationPeer,
        IExpandCollapseProvider
	{
       
        #region Constructor
            /// <summary>
		/// Creates a new instance of the <see cref="XamComboEditorAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="XamComboEditor"/> for which the peer is being created</param>
		public XamComboEditorAutomationPeer(XamComboEditor owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		    #region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
        /// <returns>The <b>ComboBox</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.ComboBox;
		}

		#endregion //GetAutomationControlTypeCore

		    #region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="XamComboEditor"/>
		/// </summary>
		/// <returns>A string that contains 'XamComboEditor'</returns>
		protected override string GetClassNameCore()
		{
			return "XamComboEditor";
		}

		#endregion //GetClassNameCore

            #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="XamComboEditorAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
                return this;

            return base.GetPattern(patternInterface);
        }
            #endregion //GetPattern

		#endregion //Base class overrides

		#region Properties

		#region Editor

		private XamComboEditor Editor
		{
			get { return (XamComboEditor)this.Owner; }
		}

		#endregion //Editor

		#endregion //Properties

        #region Methods

            #region RaiseExpandCollapseStateChanged

        internal void RaiseExpandCollapseStateChanged(bool oldValue, bool newValue)
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                RaisePropertyChangedEvent(
                    ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                    oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                    newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
            }
        }
            #endregion //RaiseExpandCollapseStateChanged

        #endregion //Methods

        #region IExpandCollapseProvider Members

        void IExpandCollapseProvider.Collapse()
        {
            if (!base.IsEnabled())
                throw new ElementNotEnabledException();

            Editor.IsDropDownOpen = true;
        }

        void IExpandCollapseProvider.Expand()
        {
            if (!base.IsEnabled())
                throw new ElementNotEnabledException();

            Editor.IsDropDownOpen = false;
        }

        System.Windows.Automation.ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                if (Editor.IsDropDownOpen)
                    return ExpandCollapseState.Expanded;
                else
                    return ExpandCollapseState.Collapsed;
            }
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