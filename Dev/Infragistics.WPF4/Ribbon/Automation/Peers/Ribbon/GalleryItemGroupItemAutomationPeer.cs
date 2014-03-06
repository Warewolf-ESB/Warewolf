using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Collections;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="GalleryItemPresenter"/> types to UI Automation
    /// </summary>
    public class GalleryItemGroupItemAutomationPeer : ItemAutomationPeer
    {

        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="GalleryItemGroupItemAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="GalleryItemGroup"/> for which the peer is being created</param>
        /// <param name="container">The ItemsControlAutomationPeer that is associated with the ItemsControl that holds the <see cref="GalleryItemGroup"/>  collection.</param>
        public GalleryItemGroupItemAutomationPeer(GalleryItemGroup owner, ItemsControlAutomationPeer container)
            : base(owner, container)
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
            return AutomationControlType.ListItem;
        }

        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="GalleryItemPresenter"/>
        /// </summary>
        /// <returns>A string that contains 'GalleryItemPresenter'</returns>
        protected override string GetClassNameCore()
        {
            return "GalleryItemGroup";
        }

        #endregion //GetClassNameCore

        #region GetNameCore
        /// <summary>
        /// Gets a human readable name for <see cref="GalleryItem"/>. 
        /// </summary>
        /// <returns>The string in <see cref="GalleryItem.Text"/>.</returns>
        protected override string GetNameCore()
        {
            return ((GalleryItemGroup)this.Item).Title;
        }
        #endregion

        #region GetPattern
        /// <summary>
        /// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="GalleryItemPresenter"/>.
        /// </summary>
        /// <param name="patternInterface">The pattern being requested</param>
        /// <returns>Object that implement ISelectionItemProvider pattern</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            //if (patternInterface == PatternInterface.SelectionItem)
            //    return this;

            return null;// base.GetPattern(patternInterface);
        }
        #endregion // GetPattern

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