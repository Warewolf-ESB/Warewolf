using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;







using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	/// <summary>
	/// Represents a journal or a note in a schedule.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// A <b>Journal</b> object represents a journal or a note in the schedule object model. It derives from <see cref="ActivityBase"/>
	/// which is also a base class for <see cref="Appointment"/> and <see cref="Task"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="XamScheduleDataManager.GetActivities(ActivityQuery)"/>
	public class Journal : ActivityBase
	{
		#region Data Structures

		#region StorageProps Class

		internal new class StorageProps
		{
			private const int FIRST_PROP = 1 + ActivityBase.StorageProps.LAST_PROP;

			//internal const int Name = 0 + FIRST_PROP;

			internal class Info : ActivityBase.StorageProps.Info
			{
				protected override void Initialize( IMap<int, StoragePropsInfo.PropInfo> map )
				{
					base.Initialize( map );

					StoragePropsInfo.PropInfo[] infos = new StoragePropsInfo.PropInfo[]
					{
						//new StoragePropsInfo.PropInfo( Name, "Name", typeof( string ), JournalProperty.Name ),
					};

					FillMap( infos, map );
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
					return new Journal( );
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
				return ActivityType.Journal;
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

        #region Public Properties
		
        #endregion // Public Properties
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