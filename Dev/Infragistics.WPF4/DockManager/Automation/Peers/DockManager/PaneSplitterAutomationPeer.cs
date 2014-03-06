using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

using Infragistics.Shared;
using Infragistics.Windows.DockManager;

namespace Infragistics.Windows.Automation.Peers.DockManager
{
    /// <summary>
    /// Exposes the <see cref="PaneSplitter"/> to UI Automation
    /// </summary>
    public abstract class PaneSplitterAutomationPeer : ThumbAutomationPeer,
        ITransformProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="PaneSplitterAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="PaneSplitter"/> for which the peer is being created</param>
        protected PaneSplitterAutomationPeer(PaneSplitter owner)
            : base(owner)
        {
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="PaneSplitter"/>
        /// </summary>
        /// <returns>A string that contains 'PaneSplitter'</returns>
        protected override string GetClassNameCore()
        {
            return "PaneSplitter";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="PaneSplitter"/> that corresponds with this <see cref="PaneSplitterAutomationPeer"/>.
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
            if (!((ITransformProvider)this).CanMove)
				throw new InvalidOperationException(XamDockManager.GetString("LE_CannotPerformAutomationOperation"));

            if (!this.IsEnabled())
                throw new ElementNotEnabledException();

            if (double.IsInfinity(x) || double.IsNaN(x))
                throw new ArgumentOutOfRangeException("x");

            if (double.IsInfinity(y) || double.IsNaN(y))
                throw new ArgumentOutOfRangeException("y");

            PaneSplitter paneSplitter = (PaneSplitter)this.Owner;
            paneSplitter.PerformMove(x, y);
        }

        void ITransformProvider.Resize(double width, double height)
        {
			throw new InvalidOperationException(XamDockManager.GetString("LE_CannotPerformAutomationOperation"));
        }

        void ITransformProvider.Rotate(double degrees)
        {
			throw new InvalidOperationException(XamDockManager.GetString("LE_CannotPerformAutomationOperation"));
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