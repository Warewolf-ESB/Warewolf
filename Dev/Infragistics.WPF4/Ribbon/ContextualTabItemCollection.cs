using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// An observable collection of <see cref="RibbonTabItem"/>s.
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note: </b>The <see cref="RibbonTabItem"/>s in this collection are not contained in the collection returned
	/// by the <see cref="Infragistics.Windows.Ribbon.XamRibbon.Tabs"/> property.  They are, however, contained in the collection returned by the <see cref="XamRibbon"/>'s
	/// read-only <see cref="Infragistics.Windows.Ribbon.XamRibbon.TabsInView"/> property.</p>
	/// </remarks>
	/// <exception cref="InvalidOperationException">If an attempt is made to add a <see cref="ContextualTabGroup"/> whose <see cref="ContextualTabGroup.Key"/> property value conflicts
	/// with the value of the <see cref="ContextualTabGroup.Key"/> property of an existing <see cref="ContextualTabGroup"/> in the collection.</exception>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.Tabs"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.XamRibbon.TabsInView"/>
	/// <seealso cref="RibbonTabItem"/>
	/// <seealso cref="ContextualTabGroup"/>
	/// <seealso cref="ContextualTabGroupCollection"/>
	public class ContextualTabItemCollection : ObservableCollectionExtended<RibbonTabItem>
	{
		#region Member Variables

		private ContextualTabGroup _group;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextualTabItemCollection"/> class.
		/// </summary>
		/// <param name="tabGroup">The owning tab group</param>
		public ContextualTabItemCollection(ContextualTabGroup tabGroup)
			: base(new List<RibbonTabItem>())
		{
			if (null == tabGroup)
				throw new ArgumentNullException("tabGroup");

			this._group = tabGroup;
		}

		#endregion //Constructor

		#region Base class overrides

		#region NotifyItemsChanged
		/// <summary>
		/// Indicates to the base class that the <see cref="OnItemAdded(RibbonTabItem)"/> and <see cref="OnItemRemoved(RibbonTabItem)"/> methods should be invoked.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}
				#endregion //NotifyItemsChanged

		#region OnItemAdded
		/// <summary>
		/// Invoked when a tab item is added to the collection.
		/// </summary>
		/// <param name="itemAdded">The tab item that was added to the ContextualTabGroup</param>
		protected override void OnItemAdded(RibbonTabItem itemAdded)
		{
			Debug.Assert(itemAdded.ContextualTabGroup == null);

			itemAdded.SetValue(RibbonTabItem.ContextualTabGroupPropertyKey, this._group);

			base.OnItemAdded(itemAdded);
		}
		#endregion //OnItemAdded

		#region OnItemRemoved
		/// <summary>
		/// Invoked when a tab item is removed from the collection.
		/// </summary>
		/// <param name="itemRemoved">The tab item that was remove from the ContextualTabGroup</param>
		protected override void OnItemRemoved(RibbonTabItem itemRemoved)
		{
			Debug.Assert(itemRemoved.ContextualTabGroup == this._group);

			if (itemRemoved.ContextualTabGroup == this._group)
				itemRemoved.ClearValue(RibbonTabItem.ContextualTabGroupPropertyKey);

			base.OnItemRemoved(itemRemoved);
		}
		#endregion //OnItemRemoved

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