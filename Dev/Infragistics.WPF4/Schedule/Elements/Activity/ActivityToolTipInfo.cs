using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Infragistics.AutomationPeers;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{

	/// <summary>
	/// An object that exposes tooltip information for an activity
	/// </summary>
	public class ActivityToolTipInfo : DependencyObject
	{
		#region Private members

		#endregion //Private members	
		
		#region Constructor
		static ActivityToolTipInfo()
		{
		}

		internal ActivityToolTipInfo(ActivityPresenter presenter)
		{
			this.ActivityPresenter = presenter;
		}
		#endregion //Constructor
    
		#region Properties

		#region Public Properties

		#region Activity

		internal static readonly DependencyPropertyKey ActivityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Activity",
			typeof(ActivityBase), typeof(ActivityToolTipInfo), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Activity"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityProperty = ActivityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated activity (read-only)
		/// </summary>
		/// <seealso cref="ActivityProperty"/>
		public ActivityBase Activity
		{
			get
			{
				return (ActivityBase)this.GetValue(ActivityToolTipInfo.ActivityProperty);
			}
			internal set
			{
				this.SetValue(ActivityToolTipInfo.ActivityPropertyKey, value);
			}
		}

		#endregion //Activity

		#region ActivityPresenter

		internal static readonly DependencyPropertyKey ActivityPresenterPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ActivityPresenter",
			typeof(ActivityPresenter), typeof(ActivityToolTipInfo), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ActivityPresenter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityPresenterProperty = ActivityPresenterPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the associated activity presenter (read-only)
		/// </summary>
		/// <seealso cref="ActivityPresenterProperty"/>
		public ActivityPresenter ActivityPresenter
		{
			get
			{
				return (ActivityPresenter)this.GetValue(ActivityToolTipInfo.ActivityPresenterProperty);
			}
			private set
			{
				this.SetValue(ActivityToolTipInfo.ActivityPresenterPropertyKey, value);
			}
		}

		#endregion //ActivityPresenter

		#region Error

		internal static readonly DependencyPropertyKey ErrorPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Error",
			typeof(DataErrorInfo), typeof(ActivityToolTipInfo), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Error"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ErrorProperty = ErrorPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a <see cref="DataErrorInfo"/> object or null (read-only)
		/// </summary>
		/// <seealso cref="ErrorProperty"/>
		public DataErrorInfo Error
		{
			get
			{
				return (DataErrorInfo)this.GetValue(ActivityToolTipInfo.ErrorProperty);
			}
			internal set
			{
				this.SetValue(ActivityToolTipInfo.ErrorPropertyKey, value);
			}
		}

		#endregion //Error
		
		#endregion //Public Properties	
    
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