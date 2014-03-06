using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Editors.Primitives;
using System.Windows.Automation;
using Infragistics.Controls.Editors;


namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Exposes <see cref="XamMaskedInput"/> types to UI Automation
	/// </summary>
	public class XamMaskedInputAutomationPeer : ValueInputAutomationPeer,
        // AS 9/5/08 NA 2008 Vol 2
        IExpandCollapseProvider
	{
        #region Member Variables

        // AS 9/5/08 NA 2008 Vol 2
        private ExpandCollapseState? _lastReturnedExpandState;

        #endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="XamMaskedInputAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="XamMaskedInput"/> for which the peer is being created</param>
		public XamMaskedInputAutomationPeer(XamMaskedInput owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Edit</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Edit;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="XamMaskedInput"/>
		/// </summary>
		/// <returns>A string that contains 'XamMaskedInput'</returns>
		protected override string GetClassNameCore()
		{
			return "XamMaskedInput";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="XamMaskedInputAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Text)
				return this;

            // AS 9/5/08 NA 2008 Vol 2
            // AS 10/6/08 TFS7567
            // Meant to return expand/collapse and not toggle - which is not implemented.
            //
            //if (patternInterface == PatternInterface.Toggle)
            if (patternInterface == PatternInterface.ExpandCollapse)
                return this;

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#endregion //Base class overrides	

		#region Properties

		#region Editor

		private XamMaskedInput Editor
		{
			get { return (XamMaskedInput)this.Owner; }
		}

				#endregion //Editor

		#region LastCharacterIndex
		private int LastCharacterIndex
		{
            // AS 10/6/08 TFS7567
			//get { return this.Editor.EditInfo.GetTotalNumberOfDisplayChars(); }
			get { return XamMaskedInput.GetTotalNumberOfDisplayChars( this.Editor.Sections ); }
		}
		#endregion //LastCharacterIndex

		#endregion //Properties	
	
		#region Methods

		#region ElementFromProvider
		internal DependencyObject ElementFromProvider(IRawElementProviderSimple provider)
		{
			DependencyObject owner = null;
			AutomationPeer peer = this.PeerFromProvider(provider);

			if (peer is FrameworkElementAutomationPeer)
			{
				return ( (FrameworkElementAutomationPeer)peer ).Owner;
			}

			if (peer is ContentElementAutomationPeer)
			{
				owner = ((ContentElementAutomationPeer)peer).Owner;
			}

			return owner;
		}
        #endregion //ElementFromProvider

        // AS 9/5/08 NA 2008 Vol 2
        #region RaiseExpandCollapseChanged
        internal void RaiseExpandCollapseChanged()
        {
            if (this._lastReturnedExpandState.HasValue &&
                AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                ExpandCollapseState newState = ((IExpandCollapseProvider)this).ExpandCollapseState;

                if (newState != this._lastReturnedExpandState.Value)
                {
                    this.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, this._lastReturnedExpandState.Value, newState);
                }
            }
        }
        #endregion //RaiseExpandCollapseChanged

		#endregion //Methods

        // AS 9/5/08 NA 2008 Vol 2
        #region IExpandCollapseProvider Members

        private void VerifyCanExpandCollapse()
        {
            if (false == this.IsEnabled())
                throw new ElementNotEnabledException();

            if (this.Editor.HasDropDown == false)
                throw new InvalidOperationException();
        }

        void IExpandCollapseProvider.Collapse()
        {
            this.VerifyCanExpandCollapse();

            if (this.Editor.HasOpenDropDown)
                this.Editor.ToggleDropDown();
        }

        void IExpandCollapseProvider.Expand()
        {
            this.VerifyCanExpandCollapse();

            if (this.Editor.HasDropDown == false)
                throw new InvalidOperationException();

            if (false == this.Editor.HasOpenDropDown)
                this.Editor.ToggleDropDown();
        }

        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get 
            {
                if (false == this.Editor.HasDropDown)
                    this._lastReturnedExpandState = ExpandCollapseState.LeafNode;
                else if (this.Editor.HasOpenDropDown)
                    this._lastReturnedExpandState = ExpandCollapseState.Expanded;
                else
                    this._lastReturnedExpandState = ExpandCollapseState.Collapsed;

                return this._lastReturnedExpandState.Value;
            }
        }

        #endregion //IExpandCollapseProvider
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