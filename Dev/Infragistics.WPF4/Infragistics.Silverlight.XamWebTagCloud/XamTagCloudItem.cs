using System;
using System.Windows;
using System.Windows.Controls;
using Infragistics.AutomationPeers;


    using Infragistics.Controls.Menus.Primitives;


namespace Infragistics.Controls.Menus
{ 
    /// <summary>
    /// <see cref="XamTagCloudItem"/> exposes all properties and methods necessary to implement 
    /// an Item for a <see cref="XamTagCloud"/>.
    /// </summary>
    public class XamTagCloudItem : HyperlinkButton
    {
        #region Contructors
        /// <summary>
        /// Initializes a new instance of the <see cref="XamTagCloudItem"/> class.
        /// </summary>
        public XamTagCloudItem()
        {

			Infragistics.Windows.Utilities.ValidateLicense(typeof(XamTagCloudItem), this);

            this.DefaultStyleKey = typeof(XamTagCloudItem);
        }
        #endregion //Constructors
       
        #region Properties

        #region Public

        #region Owner

        /// <summary>
        /// Gets or sets a reference to the <see cref="XamTagCloud"/> that this <see cref="XamTagCloudItem"/> belongs to.
        /// </summary>
        public XamTagCloud Owner { get; protected internal set; }

        #endregion //Owner

        #region Weight

        /// <summary>
        /// Identifies the <see cref="Weight"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty WeightProperty = DependencyProperty.Register("Weight", typeof(Double), typeof(XamTagCloudItem), new PropertyMetadata(new PropertyChangedCallback(WeightChanged)));

        /// <summary>
        /// Gets or sets the <see cref="Weight"/> property.
        /// </summary>
        /// <value>Weight</value>
        public Double Weight
        {
            get { return XamTagCloudPanel.GetWeight(this); }
            set { this.SetValue(WeightProperty, value); }
        }

        private static void WeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamTagCloudPanel.SetWeight(obj, (double)e.NewValue);
            XamTagCloudItem item = (XamTagCloudItem)obj;
            if (item.Owner != null)
            {
                item.Owner.InvalidateItemsPanel();
            }
        }

        #endregion // Weight 

        #endregion //Public
        #endregion //Properties

        #region Overrides

        /// <summary>
        /// Returns a <see cref="T:System.Windows.Automation.Peers.HyperlinkButtonAutomationPeer"/> for use by the automation infrastructure.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Automation.Peers.HyperlinkButtonAutomationPeer"/> for the hyperlink button object.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamTagCloudItemAutomationPeer(this);
        }

        #endregion Overrides

        #region Events

        #region OnClick
        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        protected override void OnClick()
        {
            if (this.Owner != null)
            {
                this.Owner.OnXamTagCloudItemClick(this);
            }

            base.OnClick();
        }

        #endregion //OnClick

        #endregion //Events
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