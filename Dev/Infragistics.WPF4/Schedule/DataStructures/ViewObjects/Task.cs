using System;
using System.Collections.Generic;
using System.Text;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	#region Task Class

	/// <summary>
	/// Represents a task (to-do) in a schedule.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// A <b>Task</b> object represents a task or a to-do in the schedule object model. It derives from <see cref="ActivityBase"/>
	/// which is also a base class for <see cref="Appointment"/> and <see cref="Journal"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Journal"/>
    /// <seealso cref="XamScheduleDataManager.GetActivities(ActivityQuery)"/>
	public class Task : ActivityBase
	{
		#region Data Structures

		#region StorageProps Class

		internal new class StorageProps
		{
			private const int FIRST_PROP = 1 + ActivityBase.StorageProps.LAST_PROP;
            internal const int PercentComplete = 0 + FIRST_PROP;
			// SSP 1/6/12 - NAS12.1 XamGantt
			// Added the following properties.
			// 
			//internal const int UniqueIdPredecessors = 1 + FIRST_PROP;
			//internal const int Tasks = 2 + FIRST_PROP;
			//internal const int ResourceNames = 3 + FIRST_PROP;
			//internal const int ConstraintType = 4 + FIRST_PROP;
			//internal const int ConstraintDate = 5 + FIRST_PROP;
			//internal const int Deadline = 6 + FIRST_PROP;
			//internal const int Expanded = 7 + FIRST_PROP;
			//internal const int RowHeight = 8 + FIRST_PROP;
			//internal const int Manual = 9 + FIRST_PROP;
			//internal const int Milestone = 10 + FIRST_PROP;
			//internal const int ProjectId = 11 + FIRST_PROP;
			//internal const int HideBar = 12 + FIRST_PROP;
			//internal const int Duration = 13 + FIRST_PROP;
			//internal const int Rollup = 14 + FIRST_PROP;
			//internal const int Marked = 15 + FIRST_PROP;
			//internal const int Active = 16 + FIRST_PROP;
			//internal const int DurationText = 17 + FIRST_PROP;
			//internal const int StartText = 18 + FIRST_PROP;
			//internal const int EndText = 19 + FIRST_PROP;
			//internal const int Splits = 20 + FIRST_PROP;
			//internal const int BaselineStart = 21 + FIRST_PROP;
			//internal const int BaselineEnd = 22 + FIRST_PROP;
			//internal const int BaselineDuration = 23 + FIRST_PROP;

			internal class Info : ActivityBase.StorageProps.Info
			{
				protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
				{
					base.Initialize( map );

					StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
                        new TypedPropInfo<ActivityBase, int>( PercentComplete, "PercentComplete", _sr, TaskProperty.PercentageComplete ),
						// SSP 1/6/12 - NAS12.1 XamGantt
						// Added the following properties.
						// 
						//new TypedPropInfo<ActivityBase, string>( UniqueIdPredecessors, "UniqueIdPredecessors", _sr, TaskProperty.UniqueIdPredecessors ),
						//new TypedPropInfo<ActivityBase, string>( Tasks, "Tasks", _sr, TaskProperty.Tasks ),
						//new TypedPropInfo<ActivityBase, string>( ResourceNames, "ResourceNames", _sr, TaskProperty.ResourceNames ),
						//new TypedPropInfo<ActivityBase, TaskConstraintType>( ConstraintType, "ConstraintType", _sr, TaskProperty.ConstraintType ),
						//new TypedPropInfo<ActivityBase, DateTime?>( ConstraintDate, "ConstraintDate", _sr, TaskProperty.ConstraintDate ),
						//new TypedPropInfo<ActivityBase, DateTime?>( Deadline, "Deadline", _sr, TaskProperty.Deadline ),
						//new TypedPropInfo<ActivityBase, bool>( Expanded, "Expanded", _sr, TaskProperty.Expanded ),
						//new TypedPropInfo<ActivityBase, double?>( RowHeight, "RowHeight", _sr, TaskProperty.RowHeight ),
						//new TypedPropInfo<ActivityBase, bool>( Manual, "Manual", _sr, TaskProperty.Manual ),
						//new TypedPropInfo<ActivityBase, bool>( Milestone, "Milestone", _sr, TaskProperty.Milestone ),
						//new TypedPropInfo<ActivityBase, string>( ProjectId, "ProjectId", _sr, TaskProperty.ProjectId ),
						//new TypedPropInfo<ActivityBase, bool>( HideBar, "HideBar", _sr, TaskProperty.HideBar ),
						//new TypedPropInfo<ActivityBase, TimeSpan>( Duration, "Duration", _sr, TaskProperty.Duration ),
						//new TypedPropInfo<ActivityBase, bool>( Rollup, "Rollup", _sr, TaskProperty.Rollup ),
						//new TypedPropInfo<ActivityBase, bool>( Marked, "Marked", _sr, TaskProperty.Marked ),
						//new TypedPropInfo<ActivityBase, bool>( Active, "Active", _sr, TaskProperty.Active ),
						//new TypedPropInfo<ActivityBase, string>( DurationText, "DurationText", _sr, TaskProperty.DurationText ),
						//new TypedPropInfo<ActivityBase, string>( StartText, "StartText", _sr, TaskProperty.StartText ),
						//new TypedPropInfo<ActivityBase, string>( EndText, "EndText", _sr, TaskProperty.EndText ),
						//new TypedPropInfo<ActivityBase, DateRange[]>( Splits, "Splits", _sr, TaskProperty.Splits ),
						//new TypedPropInfo<ActivityBase, DateTime?>( BaselineStart, "BaselineStart", _sr, TaskProperty.BaselineStart ),
						//new TypedPropInfo<ActivityBase, DateTime?>( BaselineEnd, "BaselineEnd", _sr, TaskProperty.BaselineEnd ),
						//new TypedPropInfo<ActivityBase, TimeSpan>( BaselineDuration, "BaselineDuration", _sr, TaskProperty.BaselineDuration )
					};

					FillMap( infos, map );

					int[] varianceDataProps = new int[]
					{
						PercentComplete
					};

					int[] rootSynchronizedVarianceDataProps = new int[]
					{
						//NOTE: Any new members must be added to the end of this list. Don't insert items because
						// these are used to determine the variant property flags.
						// 
						PercentComplete
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
						PercentComplete
						// SSP 1/6/12 - NAS12.1 XamGantt
						// Added the following entries.
						// 
						//UniqueIdPredecessors,
						//Tasks,
						//ResourceNames,
						//ConstraintType,
						//ConstraintDate,
						//Deadline,
						//Expanded,
						//RowHeight,
						//Manual,
						//Milestone,
						//ProjectId,
						//HideBar,
						//Duration,
						//Rollup,
						//Marked,
						//Active,
						//DurationText,
						//StartText,
						//EndText,
						//Splits,
						//BaselineStart,
						//BaselineEnd,
						//BaselineDuration
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
					return new Task( );
				}
			}
		}

		#endregion // StorageProps Class

		#endregion // Data Structures

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
				return ActivityType.Task;
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

		#region Active

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean indicating if the task is active.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool Active
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.Active );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.Active, value );
//            }
//        }

		#endregion // Active

		#region BaselineStart

        // SSP 1/6/12 - NAS12.1 XamGantt
        // 
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public DateTime? BaselineStart
//        {
//            get
//            {
//                return this.GetValueHelper<DateTime?>( StorageProps.BaselineStart );
//            }
//            set
//            {
//                this.SetValueHelper<DateTime?>( StorageProps.BaselineStart, value );
//            }
//        }

		#endregion // BaselineStart

		#region BaselineEnd

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public DateTime? BaselineEnd
//        {
//            get
//            {
//                return this.GetValueHelper<DateTime?>( StorageProps.BaselineEnd );
//            }
//            set
//            {
//                this.SetValueHelper<DateTime?>( StorageProps.BaselineEnd, value );
//            }
//        }

		#endregion // BaselineEnd

		#region BaselineDuration

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//#if WPF && !WCFService
//    [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public TimeSpan BaselineDuration
//        {
//            get
//            {
//                return this.GetValueHelper<TimeSpan>( StorageProps.BaselineDuration );
//            }
//            set
//            {
//                this.SetValueHelper<TimeSpan>( StorageProps.BaselineDuration, value );
//            }
//        }

		#endregion // BaselineDuration

		#region ConstraintType

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets the constraint on the start or finish of the task.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public TaskConstraintType ConstraintType
//        {
//            get
//            {
//                return this.GetValueHelper<TaskConstraintType>( StorageProps.ConstraintType );
//            }
//            set
//            {
//                this.SetValueHelper<TaskConstraintType>( StorageProps.ConstraintType, value );
//            }
//        }

		#endregion // ConstraintType

		#region ConstraintDate

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a nullable DateTime that represents the constrained start or finish date as defined by the ConstraintType.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public DateTime? ConstraintDate
//        {
//            get
//            {
//                return this.GetValueHelper<DateTime?>( StorageProps.ConstraintDate );
//            }
//            set
//            {
//                this.SetValueHelper<DateTime?>( StorageProps.ConstraintDate, value );
//            }
//        }

		#endregion // ConstraintDate

		#region Deadline

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a nullable DateTime that represents the deadline for the task.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public DateTime? Deadline
//        {
//            get
//            {
//                return this.GetValueHelper<DateTime?>( StorageProps.Deadline );
//            }
//            set
//            {
//                this.SetValueHelper<DateTime?>( StorageProps.Deadline, value );
//            }
//        }

		#endregion // Deadline

		#region Duration

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets the total span of the active working time for the task.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public TimeSpan Duration
//        {
//            get
//            {
//                return this.GetValueHelper<TimeSpan>( StorageProps.Duration );
//            }
//            set
//            {
//                this.SetValueHelper<TimeSpan>( StorageProps.Duration, value );
//            }
//        }

		#endregion // Duration

		#region DurationText

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a string representation of the task duration. This is used primarily for manually scheduled tasks.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string DurationText
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.DurationText );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.DurationText, value );
//            }
//        }

		#endregion // DurationText

		#region EndText

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a string representation of the task end/finish date.This is used primarily for the manually scheduled tasks.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string EndText
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.EndText );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.EndText, value );
//            }
//        }

		#endregion // EndText

		#region Expanded

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean indicating if the task is expanded in the UI.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool Expanded
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.Expanded );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.Expanded, value );
//            }
//        }

		#endregion // Expanded

		#region HideBar

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean indicating if the bar appears in the gantt chart.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool HideBar
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.HideBar );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.HideBar, value );
//            }
//        }

		#endregion // HideBar

		#region Manual

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean indicating if the item is manually or automatically scheduled. This property defaults to true.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool Manual
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.Manual );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.Manual, value );
//            }
//        }

		#endregion // Manual

		#region Marked

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean that may be used in filtering or bar styles to identify marked tasks.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool Marked
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.Marked );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.Marked, value );
//            }
//        }

		#endregion // Marked

		#region Milestone

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean indicating if the task represents a milestone.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool Milestone
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.Milestone );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.Milestone, value );
//            }
//        }

		#endregion // Milestone

		#region PercentComplete

		/// <summary>
		/// Gets or sets the percentage complete value of the task.
		/// </summary>
		public int PercentComplete
		{
			get
			{
				return this.GetValueHelper<int>( StorageProps.PercentComplete );
			}
			set
			{
				this.SetValueHelper<int>( StorageProps.PercentComplete, value );
			}
		}

		#endregion // PercentComplete

		#region ProjectId

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets the id of the associated Project.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string ProjectId
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.ProjectId );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.ProjectId, value );
//            }
//        }

		#endregion // ProjectId

		#region ResourceNames

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a string containing the ids of the associated resources along with optionally the unit amount (e.g. "JoeD[50%], JoeM").
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string ResourceNames
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.ResourceNames );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.ResourceNames, value );
//            }
//        }

		#endregion // ResourceNames

		#region Rollup

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a boolean indicating if the subtask is displayed in the corresponding summary task bar(s).
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public bool Rollup
//        {
//            get
//            {
//                return this.GetValueHelper<bool>( StorageProps.Rollup );
//            }
//            set
//            {
//                this.SetValueHelper<bool>( StorageProps.Rollup, value );
//            }
//        }

		#endregion // Rollup

		#region RowHeight

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a nullable Double that represents the fixed row height of the item in the Gantt.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public double? RowHeight
//        {
//            get
//            {
//                return this.GetValueHelper<double?>( StorageProps.RowHeight );
//            }
//            set
//            {
//                this.SetValueHelper<double?>( StorageProps.RowHeight, value );
//            }
//        }

		#endregion // RowHeight

		#region Splits

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets an array of DateRange instances that indicate the splits in the task where work on the task is suspended.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public DateRange[] Splits
//        {
//            get
//            {
//                return this.GetValueHelper<DateRange[]>( StorageProps.Splits );
//            }
//            set
//            {
//                this.SetValueHelper<DateRange[]>( StorageProps.Splits, value );
//            }
//        }

		#endregion // Splits

		#region StartText

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a string representation of the task start date. This is used primarily for the manually scheduled tasks.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string StartText
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.StartText );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.StartText, value );
//            }
//        }

		#endregion // StartText

		#region Tasks

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a string containing the ids of the child tasks.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string Tasks
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.Tasks );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.Tasks, value );
//            }
//        }

		#endregion // Tasks

		#region UniqueIdPredecessors

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
//        /// <summary>
//        /// Returns or sets a string that identifies one or more predecessor tasks along with optionally the relationship between them and the lag/lead between the tasks. 
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public string UniqueIdPredecessors
//        {
//            get
//            {
//                return this.GetValueHelper<string>( StorageProps.UniqueIdPredecessors );
//            }
//            set
//            {
//                this.SetValueHelper<string>( StorageProps.UniqueIdPredecessors, value );
//            }
//        }

		#endregion // UniqueIdPredecessors

		#endregion // Public Properties

		#endregion // Properties
	} 

	#endregion // Task Class
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