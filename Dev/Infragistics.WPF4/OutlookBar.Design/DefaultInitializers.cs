using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Model;
using Infragistics.Windows.OutlookBar;
using System.Windows;
using Microsoft.Windows.Design;
using System.Windows.Controls;

namespace Infragistics.Windows.Design.OutlookBar
{
    #region XamOutlookBarDefaultInitializer

    /// <summary>
    /// Provides default values for various XamOutlookBar properties.
    /// </summary>
	public class XamOutlookBarDefaultInitializer : DefaultInitializer
    {
        /// <summary>
        /// Initializes various defaults for the element wrapped by the specified Modelitem.
        /// </summary>
        /// <param name="item">The ModeItem that wraps the element whose defaults are to be set.</param>
        public override void InitializeDefaults(ModelItem item)
        {
			if (item.Context != null)
				this.InitializeDefaults(item, item.Context);
        }

		/// <summary>
		/// This method is called from the Activate method of the PrimarySelectionAdornerProviderWithDeferredInitialization class
		/// to perform initialization of defaults that require an EditingContext. 
		/// </summary>
		/// <param name="item">The Modelitem to be initialized</param>
		/// <param name="editingContext">The EditingContext associated with the item</param>
		public override void InitializeDefaults(ModelItem item, EditingContext editingContext)
		{
			DefaultInitializersHelpers.InitializeDefaultsXamOutlookBar(item, editingContext);
		}
    }
    #endregion //XamOutlookBarDefaultInitializer	

    #region OutlookBarGroupDefaultInitializer

    /// <summary>
    /// Provides default values for various OutlookBarGroup properties.
    /// </summary>
    public class OutlookBarGroupDefaultInitializer : DefaultInitializer
    {
        /// <summary>
        /// Initializes various defaults for the element wrapped by the specified Modelitem.
        /// </summary>
        /// <param name="item">The ModeItem that wraps the element whose defaults are to be set.</param>
        public override void InitializeDefaults(ModelItem item)
        {
			if (item.Context != null)
				this.InitializeDefaults(item, item.Context);
		}

		/// <summary>
		/// This method is called from the Activate method of the PrimarySelectionAdornerProviderWithDeferredInitialization class
		/// to perform initialization of defaults that require an EditingContext. 
		/// </summary>
		/// <param name="item">The Modelitem to be initialized</param>
		/// <param name="editingContext">The EditingContext associated with the item</param>
		public override void InitializeDefaults(ModelItem item, EditingContext editingContext)
		{
			DefaultInitializersHelpers.InitializeDefaultsOutlookBarGroup(item, editingContext);
		}
	}

    #endregion //OutlookBarGroupDefaultInitializer	

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