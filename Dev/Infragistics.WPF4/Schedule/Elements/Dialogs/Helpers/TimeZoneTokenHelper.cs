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

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class TimeZoneTokenHelper
	{
		#region Member Variables

		private ActivityBase					_activity;

		#endregion //Member Variables

		#region Constructor
		internal TimeZoneTokenHelper(ActivityBase activity, XamScheduleDataManager dataManager)
		{
			// Validate and save paramaters.
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");

			this.DataManager	= dataManager;
			this.Activity		= activity;
		}
		#endregion //Constructor

		#region Methods

		#region VerifyTimeZoneTokens
		internal void VerifyTimeZoneTokens()
		{
			this.LocalTimeZoneToken = this.DataManager.TimeZoneInfoProviderResolved.LocalToken;

			DataErrorInfo errorInfo;

			// StartTime token - Resolve to the local token if not set.
			this.ActivityStartTimeZoneTokenResolved = null;
			if (null != this.Activity)
				this.ActivityStartTimeZoneTokenResolved =
					ScheduleUtilities.GetTimeZoneToken(this.DataManager.DataConnector,
													   this.Activity.StartTimeZoneId,
													   false,
													   out errorInfo,
													   this.Activity);

			if (this.ActivityStartTimeZoneTokenResolved == null)
				this.ActivityStartTimeZoneTokenResolved = this.LocalTimeZoneToken;


			// StartTime token - Resolve to the local token if not set.
			this.ActivityEndTimeZoneTokenResolved = null;
			if (null != this.Activity)
				this.ActivityEndTimeZoneTokenResolved =
					ScheduleUtilities.GetTimeZoneToken(this.DataManager.DataConnector,
													   this.Activity.EndTimeZoneId,
													   false,
													   out errorInfo,
													   this.Activity);

			if (this.ActivityEndTimeZoneTokenResolved == null)
				this.ActivityEndTimeZoneTokenResolved = this.LocalTimeZoneToken;
		}
		#endregion //VerifyTimeZoneTokens

		#endregion //Methods

		#region Properties

		#region Internal Properties

		#region Activity
		internal ActivityBase Activity
		{
			get { return this._activity; }
			set
			{
				this._activity = value;
				this.VerifyTimeZoneTokens();
			}
		}
		#endregion //Activity

		#region ActivityEndTimeZoneTokenResolved
		internal TimeZoneToken ActivityEndTimeZoneTokenResolved
		{
			get;
			set;
		}
		#endregion //ActivityEndTimeZoneTokenResolved

		#region ActivityStartTimeZoneTokenResolved
		internal TimeZoneToken ActivityStartTimeZoneTokenResolved
		{
			get;
			set;
		}
		#endregion //ActivityStartTimeZoneTokenResolved

		#region LocalTimeZoneToken
		internal TimeZoneToken LocalTimeZoneToken
		{
			get;
			set;
		}
		#endregion //LocalTimeZoneToken

		#endregion //Internal Properties

		#region Private Properties

		#region DataManager
		private XamScheduleDataManager DataManager
		{
			get;
			set;
		}
		#endregion //DataManager

		#endregion //Private Properties

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