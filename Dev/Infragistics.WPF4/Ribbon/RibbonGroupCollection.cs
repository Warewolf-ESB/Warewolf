using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a modifiable observable collection of <see cref="RibbonGroup"/> objects.
	/// </summary>
	/// <seealso cref="RibbonGroup"/>
	public class RibbonGroupCollection : ObservableCollectionExtended<RibbonGroup>
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="RibbonGroupCollection"/> class.
		/// </summary>
		public RibbonGroupCollection()
		{
		}

		#endregion //Constructor	

		#region Resource Keys

		#region PagerScrollLeftButtonStyleKey

		/// <summary>
		/// The key used to identify the style for the left scroll button.
		/// </summary>
		public static readonly ResourceKey PagerScrollLeftButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonGroupCollection), "PagerScrollLeftButtonStyleKey");

		#endregion //PagerScrollLeftButtonStyleKey

		#region PagerScrollRightButtonStyleKey

		/// <summary>
		/// The key used to identify the style for the right scroll button.
		/// </summary>
		public static readonly ResourceKey PagerScrollRightButtonStyleKey = new StaticPropertyResourceKey(typeof(RibbonGroupCollection), "PagerScrollRightButtonStyleKey");

		#endregion //PagerScrollRightButtonStyleKey

		#endregion //Resource Keys

		#region Methods

		// AS 1/8/08 BR29509
		#region RaiseResetNotification
		internal void RaiseResetNotification()
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
		#endregion //RaiseResetNotification 

		#endregion //Methods
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