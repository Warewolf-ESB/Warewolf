using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A custom <see cref="Panel"/> used to position the <see cref="ApplicationMenu.RecentItems"/> of an <see cref="ApplicationMenu"/>
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ApplicationMenuRecentItemsPanel : StackPanel, IRibbonToolLocation
	{
		#region Constructor
		static ApplicationMenuRecentItemsPanel()
		{
			StackPanel.OrientationProperty.OverrideMetadata(typeof(ApplicationMenuRecentItemsPanel), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceOrientation)));
		}

		/// <summary>
		/// Initializes a new <see cref="ApplicationMenuRecentItemsPanel"/>
		/// </summary>
		public ApplicationMenuRecentItemsPanel()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region CreateUIElementCollection

		/// <summary>
		/// Creates a new UIElementCollection.
		/// </summary>
		/// <param name="logicalParent">The logical parent of the new collection.</param>
		/// <returns>A new collection.</returns>
		protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
		{
			// AS 12/3/09 TFS24394
			// Since the panel is used to display either objects that are logical children of 
			// the application menu (e.g. a toolmenuitem added to its recent items) or the 
			// wrapper (e.g. toolmenuitem) generated to represent its items, we should not make 
			// these logical children just as they would not have been if the element were an 
			// itemshost for an items control (which in essence it is) - at least when this is 
			// used within an applicationmenupresenter
			// 
			//// we need to use the templated parent - the application menu presenter - as the logical parent
			//return new UIElementCollection(this, (base.TemplatedParent == null) ? logicalParent : base.TemplatedParent as FrameworkElement);
			if (this.TemplatedParent is ApplicationMenuPresenter)
				return new UIElementCollection(this, null);

			return new UIElementCollection(this, logicalParent);
		}

		#endregion //CreateUIElementCollection

		#endregion //Base class overrides

		#region Methods

		#region CoerceOrientation
		private static object CoerceOrientation(DependencyObject d, object newValue)
		{
			return KnownBoxes.OrientationVerticalBox;
		}
		#endregion //CoerceOrientation

		#endregion //Methods

		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.ApplicationMenuRecentItems; }
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