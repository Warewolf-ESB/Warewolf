using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Infragistics.Collections;
using System.Collections;







namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Contains <see cref="ScheduleDayOfWeek"/> objects for the seven days of week. Each <i>ScheduleDayOfWeek</i> 
	/// object is used to specify settings for the corresponding day of week.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ScheduleDaysOfWeek</b> exposes properties for each day of week: <see cref="ScheduleDaysOfWeek.Monday"/>,
	/// <see cref="ScheduleDaysOfWeek.Tuesday"/>, <see cref="ScheduleDaysOfWeek.Wednesday"/>, 
	/// <see cref="ScheduleDaysOfWeek.Thursday"/>, <see cref="ScheduleDaysOfWeek.Friday"/>, 
	/// <see cref="ScheduleDaysOfWeek.Saturday"/> and <see cref="ScheduleDaysOfWeek.Sunday"/>.
	/// It's used by the ScheduleSettings' <see cref="ScheduleSettings.DaysOfWeek"/> property.
	/// </para>
	/// <para class="body">
	/// You can specifiy settings, like whether the day is a work-day and what the working hours are
	/// using the <see cref="ScheduleDayOfWeek"/> object returned by the property of this object corresponding 
	/// to that day.
	/// </para>
	/// </remarks>
	/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
	/// <seealso cref="ScheduleDayOfWeek"/>
	public class ScheduleDaysOfWeek : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private ScheduleDayOfWeek[] _days;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ScheduleDaysOfWeek"/> class.
		/// </summary>
		public ScheduleDaysOfWeek( )
		{
			_days = new ScheduleDayOfWeek[7];
		}

		#endregion // Constructor

		#region Indexer

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> object for the specified day of week. Default value is <b>Null</b>.
		/// </summary>
		/// <param name="dayOfWeek">Day of week.</param>
		/// <returns>Gets the <see cref="ScheduleDayOfWeek"/> object for the specified day of week. If one hasn't been set previously, returns <b>Null</b>.
		/// </returns>
		public ScheduleDayOfWeek this[DayOfWeek dayOfWeek]
		{
			get
			{
				return _days[(int)dayOfWeek];
			}
			set
			{
				int i = (int)dayOfWeek;
				ScheduleDayOfWeek oldVal = _days[i];
				if ( value != oldVal )
				{
					ScheduleUtilities.ManageListenerHelper( ref oldVal, value, this, true );
					_days[i] = value;
					this.RaisePropertyChangedEvent( this.GetPropertyName( dayOfWeek ) );
				}
			}
		} 

		#endregion // Indexer

		#region Properties

		#region Public Properties

		#region Monday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Monday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Monday
		{
			get
			{
				return this[DayOfWeek.Monday];
			}
			set
			{
				this[DayOfWeek.Monday] = value;
			}
		}

		#endregion // Monday

		#region Tuesday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Tuesday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Tuesday
		{
			get
			{
				return this[DayOfWeek.Tuesday];
			}
			set
			{
				this[DayOfWeek.Tuesday] = value;
			}
		}

		#endregion // Tuesday

		#region Wednesday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Wednesday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Wednesday
		{
			get
			{
				return this[DayOfWeek.Wednesday];
			}
			set
			{
				this[DayOfWeek.Wednesday] = value;
			}
		}

		#endregion // Wednesday

		#region Thursday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Thursday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Thursday
		{
			get
			{
				return this[DayOfWeek.Thursday];
			}
			set
			{
				this[DayOfWeek.Thursday] = value;
			}
		}

		#endregion // Thursday

		#region Friday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Friday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Friday
		{
			get
			{
				return this[DayOfWeek.Friday];
			}
			set
			{
				this[DayOfWeek.Friday] = value;
			}
		}

		#endregion // Friday

		#region Saturday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Saturday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Saturday
		{
			get
			{
				return this[DayOfWeek.Saturday];
			}
			set
			{
				this[DayOfWeek.Saturday] = value;
			}
		}

		#endregion // Saturday

		#region Sunday

		/// <summary>
		/// Gets or sets the <see cref="ScheduleDayOfWeek"/> instance for Sunday. Default value is <b>Null</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <b>Null</b>. In order to specifiy the settings
		/// for this day, create a new instance of <see cref="ScheduleDayOfWeek"/> object and set this property to it.
		/// </para>
		/// </remarks>
		public ScheduleDayOfWeek Sunday
		{
			get
			{
				return this[DayOfWeek.Sunday];
			}
			set
			{
				this[DayOfWeek.Sunday] = value;
			}
		}

		#endregion // Sunday

		#endregion // Public Properties 

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region GetDaySettingsIfAllocated

		internal DaySettings GetDaySettingsIfAllocated( DayOfWeek day )
		{
			ScheduleDayOfWeek x = this[day];
			return null != x ? x.DaySettings : null;
		}

		#endregion // GetDaySettingsIfAllocated

		#region GetPropertyName

		private string GetPropertyName( DayOfWeek dayOfWeek )
		{
			return dayOfWeek.ToString( );
		}

		#endregion // GetPropertyName

		#endregion // Private Methods

		#endregion // Methods
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