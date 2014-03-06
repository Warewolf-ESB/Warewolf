using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Provider;
using Infragistics.Controls.Layouts;
using Infragistics.Controls.Layouts.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="TileAreaSplitter"/> type to UI Automation
    /// </summary>
    public class TileAreaSplitterAutomationPeer : FrameworkElementAutomationPeer,
        ITransformProvider
    {
        #region Member Variables

        #endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="TileAreaSplitterAutomationPeer"/> class
        /// </summary>
        /// <param name="splitter">The <see cref="TileAreaSplitter"/> for which the peer is being created</param>
        public TileAreaSplitterAutomationPeer(TileAreaSplitter splitter)
            : base(splitter)
        {
        }

        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>DataItem</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Thumb;
        }

        #endregion //GetAutomationControlTypeCore

		#region GetChildrenCore

		/// <summary>
		/// Creates and returns a list of peers that represent the items in the <see cref="XamTileManager"/>.
		/// </summary>
		/// <returns>A list of <see cref="TileItemAutomationPeer"/>s that represent each item in the list.</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			// return no children since this element implements the Thumb's ITransformProvider's role 
			return new List<AutomationPeer>();
		}

		#endregion //GetChildrenCore	

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="XamTile"/>
        /// </summary>
        /// <returns>A string that contains 'TileAreaSplitter'</returns>
        protected override string GetClassNameCore()
        {
            return "TileAreaSplitter";
        }

        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="UIElement"/> that corresponds with the <see cref="XamTile"/> that is associated with this <see cref="XamTileAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Transform)
                return this;

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

        #region IsOffscreenCore
        /// <summary>
        /// Returns a value that indicates whether the System.Windows.UIElement that corresponds with the object that is associated with this System.Windows.Automation.Peers.AutomationPeer is off the screen.
        /// </summary>
        protected override bool IsOffscreenCore()
        {
            return this.GetBoundingRectangleCore().Equals(new Rect());
        }
        #endregion //IsOffscreenCore

        #endregion //Base class overrides

        #region ITransformProvider Members

        bool ITransformProvider.CanMove
        {
            get
            {
                if (this.IsEnabled() == false)
                    return false;

                TileAreaSplitter splitter = this.Owner as TileAreaSplitter;

                if (!splitter.TileManager.IsInMaximizedMode)
                    return false;

                TileAreaPanel panel = splitter.TileManager.Panel;

                return panel != null && panel.MaximizedModeSettingsSafe.ShowTileAreaSplitter;
            }
        }

        bool ITransformProvider.CanResize
        {
            get
            {
                return false;
            }
        }

        bool ITransformProvider.CanRotate
        {
            get
            {
                return false;
            }
        }

        void ITransformProvider.Move(double x, double y)
        {
            if (this.IsEnabled() == false)
                throw new ElementNotEnabledException();

            if (!((ITransformProvider)this).CanMove)
                throw new InvalidOperationException(TileUtilities.GetString("LE_CannotPerformAutomationOperation"));

            if (double.IsInfinity(x) || double.IsNaN(x))
                throw new ArgumentOutOfRangeException("x");

            if (double.IsInfinity(y) || double.IsNaN(y))
                throw new ArgumentOutOfRangeException("y");

            TileAreaSplitter splitter = this.Owner as TileAreaSplitter;

            if (splitter.TileManager.IsInMaximizedMode)
            {
                TileAreaPanel panel = splitter.TileManager.Panel;

                if (panel != null && panel.MaximizedModeSettingsSafe.ShowTileAreaSplitter)
                {
                    splitter.InitialDeltaRange();

                    double delta;

                    switch (panel.MaximizedModeSettingsSafe.MaximizedTileLocationResolved)
                    {
                        case MaximizedTileLocation.Left:
                        case MaximizedTileLocation.Right:
                            delta = x;
                            break;
                        default:
                            delta = y;
                            break;
                    }

                    delta = splitter.ConstrainDelta(delta);

                    splitter.ProcessDelta(delta, true);
                }
            }
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