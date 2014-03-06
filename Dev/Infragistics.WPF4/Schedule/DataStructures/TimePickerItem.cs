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
using System.ComponentModel;
using System.Text;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Represents an item in a time picker combo box.
	/// </summary>
	public class TimePickerItem : IEquatable<TimePickerItem>
	{
		#region Member Variables
		private DateTime					_time;
		private TimeSpan					_duration;
		private bool						_hasDuration;
		private string						_displayString;
		private string						_simpleString;
		#endregion //Member Variables

		#region Constructor


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


		internal TimePickerItem(DateTime time)
		{
			CoreUtilities.ValidateNotNull(time);

			this._time = time;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal TimePickerItem(DateTime time, TimeSpan duration)
		{
			CoreUtilities.ValidateNotNull(time);
			CoreUtilities.ValidateNotNull(duration);

			this._time			= time;
			this._duration		= duration;
			this._hasDuration	= true;
		}
		#endregion //Constructor

		#region Base Class Overrides

		#region Equals
		/// <summary>
		/// Determines whether the specified System.Object is equal to the current System.Object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is TimePickerItem == false)
				return false;

			return ((IEquatable<TimePickerItem>)this).Equals((TimePickerItem)obj);
		}
		#endregion //Equals

		#region GetHashCode
		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion //GetHashCode	
    
		#region ToString
		/// <summary>
		/// Returns a System.String that represents the current System.Object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.SimpleString;
		}
		#endregion //ToString

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region DisplayString
		/// <summary>
		/// Returns the string to display for this entry (read only).
		/// </summary>
		public string DisplayString
		{
			get 
			{
				if (string.IsNullOrEmpty(this._displayString))
				{
					StringBuilder sb = new StringBuilder(this.SimpleString);

					// Construct the duration string if we have a duration.
					if (this._hasDuration)
					{
						sb.Append("  (");

						if (this._duration < TimeSpan.FromHours(1.0))
						{
							sb.Append(this._duration.Minutes.ToString("#0"));
							sb.Append(" ");

#pragma warning disable 436
							sb.Append(SR.GetString("LD_TimePicker_Minutes"));
#pragma warning restore 436

						}
						else
						{
							double hours = Convert.ToDouble(this._duration.TotalMinutes) / 60;
							sb.Append(hours.ToString("#0.#"));
							sb.Append(" ");

#pragma warning disable 436
							sb.Append(SR.GetString("LD_TimePicker_Hours"));
#pragma warning restore 436

						}


						sb.Append(")");
					}

					this._displayString = sb.ToString();
				}

				return this._displayString;
			}
		}
		#endregion //DisplayString

		#endregion //Public Properties

		#region Internal Properties

		#region SimpleString
		internal string SimpleString
		{
			get 
			{
				if (string.IsNullOrEmpty(this._simpleString))
					this._simpleString = ScheduleUtilities.GetTimeString(this._time);

				return this._simpleString; 
			}
		}
		#endregion //SimpleString

		#region DateTime
		internal DateTime DateTime
		{
			get { return this._time; }
		}
		#endregion //DateTime

		#endregion //Internal Properties

		#endregion //Properties

		#region IEquatable<TimePickerItem> Members

		bool IEquatable<TimePickerItem>.Equals(TimePickerItem other)
		{
			return other._time == this._time;
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