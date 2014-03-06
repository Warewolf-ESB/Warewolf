using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


#pragma warning disable 1574
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
	/// <summary>
	/// Represents an appointment in a schedule.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// An <b>Appointment</b> object represents an appointment in the schedule object model. It derives from <see cref="ActivityBase"/>
	/// which is also a base class for <see cref="Journal"/> and <see cref="Task"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="ActivityBase"/>
	/// <seealso cref="Journal"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="XamScheduleDataManager.GetActivities(ActivityQuery)"/>
	public class Appointment : ActivityBase
	{
		#region Data Structures

		#region StorageProps Class

		internal static new class StorageProps
		{
			private const int FIRST_PROP = 1 + ActivityBase.StorageProps.LAST_PROP;

			internal const int Location = 0 + FIRST_PROP;
			//internal const int Participants = 2 + FIRST_PROP;


			internal class Info : ActivityBase.StorageProps.Info
			{
				protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
				{
					base.Initialize( map );

					StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
						new TypedPropInfo<ActivityBase, string>( Location, "Location", _sr, AppointmentProperty.Location ),
						//new TypedPropInfo<ActivityBase, ObservableCollection<Resource>>( Participants, "Participants", _sr, AppointmentProperty.Participants )
						//Reminder, typeof( Reminder ), AppointmentProperty.Reminder
					};

					FillMap( infos, map );

					int[] varianceDataProps = new int[]
					{
						Location
					};

					int[] rootSynchronizedVarianceDataProps = new int[]
					{
						//NOTE: Any new members must be added to the end of this list. Don't insert items because
						// these are used to determine the variant property flags.
						// 
						Location
					};

					// Setup variant property flags. Since these are serialized out, the values for a property must not change.
					// In other words, the above table must remain fixed and any new entries must be added to the end.
					// 
					for ( int i = 0; i < rootSynchronizedVarianceDataProps.Length; i++ )
					{
						_variantPropertyFlags[rootSynchronizedVarianceDataProps[i]] = 1L << ( 32 + i );
					}

					int[] unmappedPropertiesStoreCandidates = new int[]
					{
						Location
					};

					MapsFactory.SetValues( _varianceDataProps, varianceDataProps, true );
					MapsFactory.SetValues( _rootSynchronizedVarianceDataProps, rootSynchronizedVarianceDataProps, true );
					MapsFactory.SetValues( _unmappedPropertiesStoreCandidates, unmappedPropertiesStoreCandidates, true );
				}

				private static Info g_instance = new Info( );

				internal static Info Instance
				{
					get
					{
						return g_instance;
					}
				}

				public override ActivityBase CreateNew( )
				{
					return new Appointment( );
				}
			}

		}

		#endregion // StorageProps Class 

		#endregion // Data Structures

		#region Member Vars

		#endregion // Member Vars

		#region Base Overrides

		#region Properties

		#region ActivityType

		/// <summary>
		/// Gets the ActivityType of this activity.
		/// </summary>
		public override ActivityType ActivityType
		{
			get
			{
				return ActivityType.Appointment;
			}
		}

		#endregion // ActivityType

		#region PropsInfo

		internal override ActivityBase.StorageProps.Info PropsInfo
		{
			get
			{
				return StorageProps.Info.Instance;
			}
		}

		#endregion // PropsInfo  

		#endregion // Properties

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region Location

		/// <summary>
		/// Gets or sets the location of the appointment.
		/// </summary>
		public string Location
		{
			get
			{
				return this.GetValueHelper<string>( StorageProps.Location );
			}
			set
			{
				this.SetValueHelper<string>( StorageProps.Location, value );
			}
		}

		#endregion // Location

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