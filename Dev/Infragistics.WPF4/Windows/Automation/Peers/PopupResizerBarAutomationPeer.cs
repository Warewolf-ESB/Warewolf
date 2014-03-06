using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Shared;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers
{
    /// <summary>
    /// Exposes the <see cref="PopupResizerBar"/> to UI Automation
    /// </summary>
    public class PopupResizerBarAutomationPeer : FrameworkElementAutomationPeer, 
        ITransformProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="PopupResizerBarAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="PopupResizerBar"/> for which the peer is being created</param>
        public PopupResizerBarAutomationPeer(PopupResizerBar owner)
            : base(owner)
        {
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Custom</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="PopupResizerBar"/>
        /// </summary>
        /// <returns>A string that contains 'ToolWindow'</returns>
        protected override string GetClassNameCore()
        {
            return "PopupResizerBar";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="PopupResizerBar"/> that corresponds with this <see cref="PopupResizerBarAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {

            if (patternInterface == PatternInterface.Transform)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #endregion //Base class overrides

        #region ITransformProvider

        bool ITransformProvider.CanMove { get { return true; } }

        bool ITransformProvider.CanResize { get { return false; } }

        bool ITransformProvider.CanRotate { get { return false; } }

        void ITransformProvider.Move(double x, double y)
        {
            if (false == this.IsEnabled())
                throw new ElementNotEnabledException();

            if (!((ITransformProvider)this).CanMove)
                throw new InvalidOperationException(SR.GetString("LE_CannotPerformAutomationOperation"));

            PopupResizerBar popupResizerBar = (PopupResizerBar)this.Owner;
            popupResizerBar.PerformMove(x, y);
        }

        void ITransformProvider.Resize(double width, double height)
        {
            if (false == this.IsEnabled())
                throw new ElementNotEnabledException();

            throw new InvalidOperationException(SR.GetString("LE_CannotPerformAutomationOperation"));
        }

        void ITransformProvider.Rotate(double degrees)
        {
            if (false == this.IsEnabled())
                throw new ElementNotEnabledException();

            throw new InvalidOperationException(SR.GetString("LE_CannotPerformAutomationOperation"));
        }

        #endregion //ITransformProvider


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