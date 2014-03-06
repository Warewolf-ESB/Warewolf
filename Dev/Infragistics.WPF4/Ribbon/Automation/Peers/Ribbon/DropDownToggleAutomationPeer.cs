using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="DropDownToggle"/> types to UI Automation
    /// </summary>
    public class DropDownToggleAutomationPeer : FrameworkElementAutomationPeer, IToggleProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="DropDownToggleAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="DropDownToggle"/> for which the peer is being created</param>
        public DropDownToggleAutomationPeer(DropDownToggle owner)
            : base(owner)
        {
        }
        #endregion //Constructor 

        #region Base class overrides

            #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>CheckBox</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.CheckBox;
        }

            #endregion //GetAutomationControlTypeCore

            #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="DropDownToggle"/>
        /// </summary>
        /// <returns>A string that contains 'DropDownToggle'</returns>
        protected override string GetClassNameCore()
        {
            return "DropDownToggle";
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
            if (patternInterface == PatternInterface.Toggle)
                return this;

            return base.GetPattern(patternInterface);
        }
            #endregion // GetPattern


        #endregion //Base class overrides	

        #region Methods

        internal virtual void RaiseToggleStatePropertyChangedEvent(bool? oldValue, bool? newValue)
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
            {
                if (oldValue != newValue)
                {
                    base.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, ConvertToToggleState(oldValue), ConvertToToggleState(newValue));
                }
            }
        }
        #endregion

        #region IToggleProvider Members

        void IToggleProvider.Toggle()
        {
            if (!base.IsEnabled())
                throw new ElementNotEnabledException();

            DropDownToggle toggle = Owner as DropDownToggle;
            toggle.IsDroppedDown = !toggle.IsDroppedDown;
        }

        System.Windows.Automation.ToggleState IToggleProvider.ToggleState
        {
            get
            {
                DropDownToggle owner = (DropDownToggle)Owner;
                return ConvertToToggleState(owner.IsDroppedDown); 
            }
        }

        internal static ToggleState ConvertToToggleState(bool? value)
        {
            switch (value)
            {
                case (true): return ToggleState.On;
                case (false): return ToggleState.Off;
                default: return ToggleState.Indeterminate;
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