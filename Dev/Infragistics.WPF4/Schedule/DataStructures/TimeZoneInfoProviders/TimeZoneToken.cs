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
using System.Diagnostics;




namespace Infragistics.Controls.Schedules

{
	#region TimeZoneToken public class

	/// <summary>
	/// A class that represents a specific time zone
	/// </summary>
	/// <seealso cref="TimeZoneInfoProvider.GetTimeZoneToken(string)"/>
	public class TimeZoneToken
	{
		#region Private Members

		private string _id;
		private TimeZoneInfoProvider _provider;

		#endregion //Private Members

		#region Constructor

		internal TimeZoneToken(string id, TimeZoneInfoProvider provider)
		{
			_id = id;
			_provider = provider;
		}

		#endregion //Constructor

		#region Properties

		#region Id

		/// <summary>
		/// A string that identifiies the time zone (read-only) 
		/// </summary>
		public string Id { get { return _id; } }

		#endregion //Id

		#region DisplayName

		/// <summary>
		/// A string used to represent this time zone is the ui (read-only) 
		/// </summary>
		public string DisplayName 
		{ 
			get 
			{ 
				string displayName = _provider.GetDisplayName(this);

				if (string.IsNullOrWhiteSpace(displayName))
				{
					displayName = _provider.GetStandardName(this);
					
					if (string.IsNullOrWhiteSpace(displayName))
					{
						displayName = this._id;
					}
				}

				return displayName;
			} 
		}

		#endregion //DisplayName

		#region Provider

		/// <summary>
		/// Returns the <see cref="TimeZoneInfoProvider"/> that created the token (read-only) 
		/// </summary>
		public TimeZoneInfoProvider Provider { get { return _provider; } }

		#endregion //Provider

		#region Internal properties

		internal TimeZoneInfoProvider.TimeZoneDefinition TimeZoneDefinition { get; set; }


			internal TimeZoneInfo TimeZoneInfo { get; set; }


		#endregion //Internal properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region ConvertFromLocal
		/// <summary>
		/// Converts the specified time from the Local token for the associated provider to the timezone identified by this token instance.
		/// </summary>
		/// <param name="dateTime">DateTime to convert. This is assumed to be an unspecified time relative to the Local timezone of the associated provider</param>
		/// <returns></returns>
		internal DateTime ConvertFromLocal(DateTime dateTime)
		{
			EnsureUnspecified(ref dateTime);
			return _provider.ConvertTime(_provider.LocalToken, this, dateTime);
		}
		#endregion // ConvertFromLocal

		#region ConvertFromUtc
		/// <summary>
		/// Converts the specified time from the Utc token for the associated provider to the timezone identified by this token instance.
		/// </summary>
		/// <param name="dateTime">DateTime to convert. This is assumed to be an unspecified time relative to the Utc timezone of the associated provider</param>
		/// <returns></returns>
		internal DateTime ConvertFromUtc(DateTime dateTime)
		{
			EnsureUnspecified(ref dateTime);
			return _provider.ConvertTime(_provider.UtcToken, this, dateTime);
		}
		#endregion // ConvertFromUtc

		#region ConvertToLocal
		/// <summary>
		/// Converts the specified time from the timezone identified by this token instance to the Local token for the associated provider.
		/// </summary>
		/// <param name="dateTime">DateTime to convert. This is assumed to be an unspecified time relative to this timezone</param>
		/// <returns></returns>
		internal DateTime ConvertToLocal(DateTime dateTime)
		{
			EnsureUnspecified(ref dateTime);
			return _provider.ConvertTime(this, _provider.LocalToken, dateTime);
		}
		#endregion // ConvertToLocal

		#region ConvertToUtc
		/// <summary>
		/// Converts the specified time from the timezone identified by this token instance to the Utc token for the associated provider.
		/// </summary>
		/// <param name="dateTime">DateTime to convert. This is assumed to be an unspecified time relative to this timezone</param>
		/// <returns></returns>
		internal DateTime ConvertToUtc(DateTime dateTime)
		{
			EnsureUnspecified(ref dateTime);

			// JJD /31/11 - TFS70668
			// There are daylight savings cusp times that the OS conversion routines throw an exception trying to 
			// convert to UTC so we need to wrap the conversion call in a try catch
			//return _provider.ConvertTime(this, _provider.UtcToken, dateTime);
			try
			{
				return ConvertToUtcHelper(dateTime);
			}
			catch (ArgumentException)
			{
				try
				{
					// JJD /31/11 - TFS70668
					// Modify the time by by adding stripping off the minutes and adding an hour and try the conversion again
					DateTime try2;
					if (dateTime.Hour < 23)
						try2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + 1, 0, 0, 0, dateTime.Kind);
					else
						try2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0, dateTime.Kind);
					
					return ConvertToUtcHelper(try2);
				}
				catch (ArgumentException)
				{
					// JJD /31/11 - TFS70668
					// If it still throws an exception then strip off everything after the hour and try the conversion again
					DateTime try3;
					if (dateTime.Hour < 23)
						try3 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0, dateTime.Kind);
					else
						try3 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour - 1, 0, 0, 0, dateTime.Kind);

					return ConvertToUtcHelper(try3);
				}
			}
		}

		// JJD /31/11 - TFS70668 - addeed
		private DateTime ConvertToUtcHelper(DateTime dateTime)
		{
			return _provider.ConvertTime(this, _provider.UtcToken, dateTime);
		}

		#endregion // ConvertToUtc

		#endregion // Internal Methods

		#region Private Methods

		#region EnsureUnspecified
		private void EnsureUnspecified(ref DateTime date)
		{
			if (date.Kind != DateTimeKind.Unspecified)
				date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
		}
		#endregion // EnsureUnspecified

		#endregion // Private Methods		

		#endregion // Methods
	}

	#endregion //TimeZoneToken public class

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