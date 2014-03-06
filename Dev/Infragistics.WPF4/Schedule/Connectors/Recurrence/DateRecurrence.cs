using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Markup;
using System.ComponentModel;







using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Contains information that defines a recurring date-time.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>DateRecurrence</b> is used to specify rules that define a recurring date-time. It's used by the
	/// <see cref="ActivityBase.Recurrence"/> property.
	/// </para>
	/// </remarks>
	/// <seealso cref="ActivityBase.Recurrence"/>
	[ContentProperty("Rules")]
	public sealed class DateRecurrence : RecurrenceBase
	{
		#region Member Vars

		private DateRecurrenceFrequency _frequency;
		private int _interval = 1;
		private int _count;
		private DateTime? _until;

		// SSP 4/5/11 TFS66178
		// Default WeekStart to Monday based on the iCalendar specificiation.
		// 
		//private DayOfWeek _weekStart;
		private DayOfWeek _weekStart = DayOfWeek.Monday;

		private ObservableCollectionExtended<DateRecurrenceRuleBase> _rules; 

		#endregion // Member Vars

		#region Base Overrides

		#region Equals

		/// <summary>
		/// Overridden. Returns true if the specified object equals this object.
		/// </summary>
		/// <param name="obj">Object to compare to.</param>
		/// <returns>True if the specified object equals this object. False otherwise.</returns>
		public override bool Equals( object obj )
		{
			DateRecurrence r = obj as DateRecurrence;

			return null != r
				&& r._count == _count
				&& r._frequency == _frequency
				&& r._interval == _interval
				&& r._until == _until
				&& r._weekStart == _weekStart
				&& ScheduleUtilities.HasSameItems( r._rules, _rules, false );
		} 

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Overridden. Returns the hash code of this object.
		/// </summary>
		/// <returns>Hash code of the object.</returns>
		public override int GetHashCode( )
		{
			return _count.GetHashCode( )
				^ _frequency.GetHashCode( )
				^ _interval.GetHashCode( )
				^ _until.GetHashCode( )
				^ _weekStart.GetHashCode( )
				^ ScheduleUtilities.CombineHashCodes( _rules );
		} 

		#endregion // GetHashCode

		#region ToString

		/// <summary>
		/// Overridden. Returns string representation of the recurrence.
		/// </summary>
		/// <returns>Text that describes the recurrence.</returns>
		public override string ToString( )
		{
			return DateRecurrenceParser.ToDisplayStringHelper( this, null );
		}

		#endregion // ToString

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Specifies the number of times the recurrence will re-occur. If 0 then there's no limit.
		/// </summary>
		/// <remarks>
		/// You can limit the number of times a recurrence re-occurs by either specifying the <b>Count</b> 
		/// property or the <see cref="Until"/> property. Note that <see cref="Count"/>
		/// and <see cref="Until"/> are exclusive. You should only specify one but not both.
		/// </remarks>
		/// <seealso cref="Until"/>
		[DefaultValue((int)0)]
		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				if ( _count != value )
				{
					_count = value;
					this.RaisePropertyChangedEvent( "Count" );
				}
			}
		}

		#endregion // Count

		#region Frequency

		/// <summary>
		/// Specifies the frequency of the recurrence.
		/// </summary>
		/// <seealso cref="DateRecurrenceFrequency"/>
		/// <seealso cref="Interval"/>
		[DefaultValue(DateRecurrenceFrequency.Yearly)]
		public DateRecurrenceFrequency Frequency
		{
			get
			{
				return _frequency;
			}
			set
			{
				if ( _frequency != value )
				{
					_frequency = value;
					this.RaisePropertyChangedEvent( "Frequency" );
				}
			}
		}

		#endregion // Frequency

		#region Interval

		/// <summary>
		/// Specifies how often to recur.
		/// </summary>
		[DefaultValue((int)1)]
		public int Interval
		{
			get
			{
				return _interval;
			}
			set
			{
				if ( _interval != value )
				{
					_interval = value;
					this.RaisePropertyChangedEvent( "Interval" );
				}
			}
		}

		#endregion // Interval

		#region Rules

		/// <summary>
		/// Specifies recurrence rules.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Recurrence rules can be used to specify further criteria regarding when the recurrences recur. For example, 
		/// you can specify one or more rules to indicate that the recurrence should recur in a specific month, in a 
		/// specific week of the year, on a specific week day etc... Rules can limit or in some cases expand the number 
		/// of recurrences that would have occurred based on the Frequency and Interval settings alone.
		/// </para>
		/// </remarks>
		/// <seealso cref="DateRecurrenceRuleBase"/>
		public ObservableCollectionExtended<DateRecurrenceRuleBase> Rules
		{
			get
			{
				if ( null == _rules )
					ScheduleUtilities.ManageListenerHelper( ref _rules, new ObservableCollectionExtended<DateRecurrenceRuleBase>(false, true ), this, true );

				return _rules;
			}
		}

		#endregion // Rules

		#region Until

		/// <summary>
		/// Specifies when to stop to recur.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can limit the recurrence to a date-time. Recurrence will stop recurring after that date-time.
		/// However note that the last recurrence can start on this date-time. Also note that <see cref="Count"/>
		/// and <see cref="Until"/> are exclusive. You should only specify one but not both.
		/// </para>
		/// </remarks>
		/// <see cref="Count"/>



		[DefaultValue(null)]
		public DateTime? Until
		{
			get
			{
				return _until;
			}
			set
			{
				if ( _until != value )
				{
					_until = value;
					this.RaisePropertyChangedEvent( "Until" );
				}
			}
		}

		#endregion // Until

		#region WeekStart

		/// <summary>
		/// Specifies the start of the week. Default value is <b>Monday</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>WeekStart</b> has implications when applying certain rules, like <see cref="WeekOfYearRecurrenceRule"/>, 
		/// regarding when the first week of the year starts. First week of the year is considered to be the first
		/// week that has at least 4 days in the year.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the default value of this property is <i>Monday</i>.
		/// </para>
		/// </remarks>
		[DefaultValue(DayOfWeek.Sunday)]
		public DayOfWeek WeekStart
		{
			get
			{
				return _weekStart;
			}
			set
			{
				if ( _weekStart != value )
				{
					_weekStart = value;
					this.RaisePropertyChangedEvent( "WeekStart" );
				}
			}
		}

		#endregion // WeekStart

		#endregion // Public Properties 

		#region Internal Properties

		#region HasRules

		internal bool HasRules
		{
			get
			{
				return null != _rules && _rules.Count > 0;
			}
		}

		#endregion // HasRules 

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Clone

		internal override RecurrenceBase Clone( )
		{
			DateRecurrence clone = new DateRecurrence( );

			clone._frequency = _frequency;
			clone._interval = _interval;
			clone._count = _count;
			clone._until = _until;
			clone._weekStart = _weekStart;

			if ( this.HasRules )
			{
				foreach ( DateRecurrenceRuleBase rule in _rules )
					clone.Rules.Add( rule.Clone( ) );
			}

			return clone;
		}

		#endregion // Clone 

		#endregion // Internal Methods

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