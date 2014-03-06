using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;




using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services





{

	/// <summary>
	/// Represents a range of time.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>TimeRange</b> class represents a range of time as defined by start and end <i>TimeSpan</i> values.
	/// </para>
	/// </remarks>
	public struct TimeRange
	{
		#region Nested Data Structures

		#region SerializationInfo Class

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a TimeRange.
		/// </summary>
		internal class SerializationInfo : ObjectSerializationInfo
		{
			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties( )
			{
				return new PropertySerializationInfo[]
				{
					new PropertySerializationInfo( typeof( TimeSpan ), "Start" ),
					new PropertySerializationInfo( typeof( TimeSpan ), "End" ),
				};
			}

			public override Dictionary<string, object> Serialize( object obj )
			{
				TimeRange range = (TimeRange)obj;
				Dictionary<string, object> map = null;

				map = ScheduleUtilities.AddEntryHelper( map, "Start", range.Start );
				map = ScheduleUtilities.AddEntryHelper( map, "End", range.End );

				return map;
			}

			public override object Deserialize( Dictionary<string, object> values )
			{
				TimeRange range = new TimeRange( );

				object v;
				if ( values.TryGetValue( "Start", out v ) )
					range.Start = (TimeSpan)v;

				if ( values.TryGetValue( "End", out v ) )
					range.End = (TimeSpan)v;

				return range;
			}
		}

		#endregion // SerializationInfo Class

		#endregion // Nested Data Structures

		#region Member Vars

		private TimeSpan _start;
		private TimeSpan _end;

		#endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="TimeRange"/>.
        /// </summary>
        /// <param name="start">Start time of the time range, inclusive.</param>
        /// <param name="end">Start time of the time range, exclusive.</param>
        public TimeRange( TimeSpan start, TimeSpan end )
        {
            _start = start;
            _end = end;
        } 

        #endregion // Constructor

		#region Properties

		#region Public Properties

		#region End

		/// <summary>
		/// Specifies the end time of the time range.
		/// </summary>
		public TimeSpan End
		{
			get
			{
				return _end;
			}
			set
			{
				if ( _end != value )
				{
					_end = value;
				}
			}
		}

		#endregion // End

		#region Start

		/// <summary>
		/// Specifies the start time of the time range.
		/// </summary>
		public TimeSpan Start
		{
			get
			{
				return _start;
			}
			set
			{
				if ( _start != value )
				{
					_start = value;
				}
			}
		}

		#endregion // Start

		#endregion // Public Properties

		#endregion // Properties
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