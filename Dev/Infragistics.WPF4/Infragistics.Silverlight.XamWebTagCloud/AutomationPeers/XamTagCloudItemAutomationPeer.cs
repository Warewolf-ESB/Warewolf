using System;
using System.Reflection;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Menus;
using System.ComponentModel;

namespace Infragistics.AutomationPeers
{
    /// <summary>
    /// Exposes <see cref="XamTagCloudItem" /> types to UI
    /// </summary>

    [DesignTimeVisible(false)]

    public class XamTagCloudItemAutomationPeer : HyperlinkButtonAutomationPeer
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamTagCloudItemAutomationPeer"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public XamTagCloudItemAutomationPeer(XamTagCloudItem owner)
            : base(owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
        }
        #endregion //Constructors

        #region Properties
        #region OwningTagCloudItem
        private XamTagCloudItem OwningTagCloudItem
        {
            get
            {
                return (XamTagCloudItem)this.Owner;
            }
        }
        #endregion //OwningTagCloudItem

        #region TagCloud
        private XamTagCloud TagCloud
        {
            get
            {
                return this.OwningTagCloudItem.Owner;
            }
        }
        #endregion //TagCloud

        #endregion Properties

        #region Overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Gets the control type for the <see cref="T:System.Windows.Controls.HyperlinkButton"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.HyperlinkButtonAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetAutomationControlType"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="F:System.Windows.Automation.Peers.AutomationControlType.Hyperlink"/> enumeration value.
        /// </returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Hyperlink;
        }
        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="T:System.Windows.UIElement"/> that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. This method is called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetClassName"/>.
        /// </summary>
        /// <returns>
        /// The name of the owner type that is associated with this <see cref="T:System.Windows.Automation.Peers.FrameworkElementAutomationPeer"/>. See Remarks.
        /// </returns>
        protected override string GetClassNameCore()
        {
            return this.OwningTagCloudItem.GetType().Name;
        }
        #endregion //GetClassNameCore

        #region GetNameCore
        /// <summary>
        /// Gets the name of the class of the object associated with this <see cref="T:System.Windows.Automation.Peers.ButtonBaseAutomationPeer"/>. Called by <see cref="M:System.Windows.Automation.Peers.AutomationPeer.GetName"/>.
        /// </summary>
        /// <returns>
        /// A string that contains the class name, minus the accelerator key.
        /// </returns>
        protected override string GetNameCore()
        {
            string nameCore = base.GetNameCore();
            if (string.IsNullOrEmpty(nameCore))
            {
                string propName = this.TagCloud.DisplayMemberPath;
                if (String.IsNullOrEmpty(propName))
                {
                    nameCore = this.OwningTagCloudItem.Content.ToString();
                }
                else
                {
                    Type type = this.TagCloud.GetType();
                    PropertyInfo propInfo = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    nameCore = propInfo.GetValue(this.TagCloud, null).ToString();
                }
            }

            return nameCore;
        }
        #endregion //GetNameCore

        #endregion Overrides
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