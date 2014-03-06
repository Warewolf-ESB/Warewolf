using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
    /// <summary>
    /// Exposes <see cref="FixedFieldSplitter"/> types to UI Automation
    /// </summary>
    public class FixedFieldSplitterAutomationPeer : ThumbAutomationPeer,
        ITransformProvider
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="FixedFieldSplitterAutomationPeer"/>
        /// </summary>
        /// <param name="element">The associated element</param>
        public FixedFieldSplitterAutomationPeer(FixedFieldSplitter element)
            : base(element)
        {
        } 
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Thumb</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Thumb;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetAutomationIdCore
        /// <summary>
        /// Returns the <see cref="System.Windows.Automation.AutomationIdentifier"/> for the <see cref="System.Windows.UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
        /// </summary>
        /// <returns>The ui automation identifier</returns>
        protected override string GetAutomationIdCore()
        {
            string id = base.GetAutomationIdCore();

            if (string.IsNullOrEmpty(id))
            {
                FixedFieldSplitter splitter = this.Owner as FixedFieldSplitter;

                // the automation id is not a localized value but there needs
                // to be a way to uniquely identify it within the parent
                id = splitter.SplitterType.ToString();
            }

            return id;
        }
        #endregion //GetAutomationIdCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="FixedFieldSplitter"/>
        /// </summary>
        /// <returns>A string that contains 'FixedFieldSplitter'</returns>
        protected override string GetClassNameCore()
        {
            return "FixedFieldSplitter";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="GroupByArea"/> that corresponds with this <see cref="GroupByAreaAutomationPeer"/>.
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

        #region ITransformProvider Members

        bool ITransformProvider.CanMove
        {
            get { return true; }
        }

        bool ITransformProvider.CanResize
        {
            get { return false; }
        }

        bool ITransformProvider.CanRotate
        {
            get { return false; }
        }

        void ITransformProvider.Move(double x, double y)
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (!((ITransformProvider)this).CanMove)
                throw new InvalidOperationException(Infragistics.Windows.Resources.GetString("LE_CannotPerformAutomationOperation"));

            if (double.IsInfinity(x) || double.IsNaN(x))
                throw new ArgumentOutOfRangeException("x");

            if (double.IsInfinity(y) || double.IsNaN(y))
                throw new ArgumentOutOfRangeException("y");

            ((FixedFieldSplitter)this.Owner).PerformMove(x, y);
        }

        void ITransformProvider.Resize(double width, double height)
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            throw new InvalidOperationException();
        }

        void ITransformProvider.Rotate(double degrees)
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            throw new InvalidOperationException();
        }

        #endregion //ITransformProvider Members
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