using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace Infragistics.Controls.Schedules.Primitives
{

	/// <summary>
	/// Custom element used to display the contents of the tooltip for an <see cref="ActivityPresenter"/>
	/// </summary>
	public class ActivityToolTipInfoPresenter : Control
	{
		#region Constructor
		static ActivityToolTipInfoPresenter()
		{

			ActivityToolTipInfoPresenter.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityToolTipInfoPresenter), new FrameworkPropertyMetadata(typeof(ActivityToolTipInfoPresenter)));

		}

		/// <summary>
		/// Initializes a new <see cref="ActivityToolTipInfoPresenter"/>
		/// </summary>
		public ActivityToolTipInfoPresenter()
		{



		}
		#endregion //Constructor

		#region Properties

		#region ActivityToolTipInfo

		/// <summary>
		/// Identifies the <see cref="ActivityToolTipInfo"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityToolTipInfoProperty = DependencyPropertyUtilities.Register("ActivityToolTipInfo",
			typeof(ActivityToolTipInfo), typeof(ActivityToolTipInfoPresenter),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the <see cref="ActivityToolTipInfo"/> that provides information about the <see cref="ActivityPresenter"/> for which the tooltip is being displayed.
		/// </summary>
		/// <seealso cref="ActivityToolTipInfoProperty"/>
		public ActivityToolTipInfo ActivityToolTipInfo
		{
			get
			{
				return (ActivityToolTipInfo)this.GetValue(ActivityToolTipInfoPresenter.ActivityToolTipInfoProperty);
			}
			set
			{
				this.SetValue(ActivityToolTipInfoPresenter.ActivityToolTipInfoProperty, value);
			}
		}

		#endregion //ActivityToolTipInfo

		#endregion //Properties
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