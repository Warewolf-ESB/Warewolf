using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Tiles;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;

namespace Infragistics.Windows.Automation.Peers.Tiles
{
    /// <summary>
    /// Exposes <see cref="XamTilesControl"/> type to UI Automation
    /// </summary>
    public class XamTilesControlAutomationPeer : RecyclingItemsControlAutomationPeer
    {
		#region Member Variables

		#endregion //Member Variables

		#region Constructor
		/// <summary>
        /// Creates a new instance of the <see cref="XamTilesControlAutomationPeer"/> class
		/// </summary>
        /// <param name="control">The <see cref="XamTilesControl"/> for which the peer is being created</param>
        public XamTilesControlAutomationPeer(XamTilesControl control)
			: base(control)
		{
		}
		#endregion //Constructor

        #region Base class overrides

        #region CreateAutomationPeer
        /// <summary>
        /// Returns a <see cref="RecycleableItemAutomationPeer"/> that represents the specified item.
        /// </summary>
        /// <param name="item">The item from the list.</param>
        /// <returns>A <see cref="RecycleableItemAutomationPeer"/> that represents the item</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="item"/> is null</exception>
        protected override RecycleableItemAutomationPeer CreateAutomationPeer(object item)
        {
            TileUtilities.ThrowIfNull(item, "item");

            XamTilesControl tc = this.Owner as XamTilesControl;

            return new RecycleableItemAutomationPeer(item, this);
        }
        #endregion //CreateAutomationPeer

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>List</b> enumeration value</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.List;
        }
        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="XamTilesControl"/>
        /// </summary>
        /// <returns>A string that contains 'XamTilesControl'</returns>
        protected override string GetClassNameCore()
        {
            return "XamTilesControl";
        }
        #endregion //GetClassNameCore

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the <see cref="Tile"/> that is associated with this <see cref="TileAutomationPeer"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        public override object GetPattern(PatternInterface patternInterface)
        {
            
            
            if (patternInterface == PatternInterface.Scroll)
            {
                XamTilesControl tc = (XamTilesControl)this.Owner;

                TilesPanel panel = tc.Panel;

                if (panel != null)
                {
                    if (((IScrollInfo)panel).ScrollOwner != null)
                    {
                        AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(((IScrollInfo)panel).ScrollOwner);

                        if (null != peer && peer is IScrollProvider)
                        {
                            peer.EventsSource = this;
                            return peer;
                        }
                    }
                }

                // we do not want to bother returning a scroll interface unless we have a panel
                return null;
            }

            return base.GetPattern(patternInterface);
        }
        #endregion //GetPattern

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