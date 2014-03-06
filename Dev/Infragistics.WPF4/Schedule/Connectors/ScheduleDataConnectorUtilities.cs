using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;






namespace Infragistics.Controls.Schedules

{
	
	
	
	internal abstract class ScheduleDataConnectorUtilities<T>
	{
		protected abstract bool StoreBeginEditData( T item );
		protected abstract bool RestoreBeginEditData( T item );
		protected abstract void ClearBeginEditData( T item );
		public abstract ItemOperationResult<T> CreateOperationResult( T item );
		public abstract ItemOperationResult<T> CreateOperationResult( T item, DataErrorInfo errorInfo, bool markComplete );
		public abstract bool IsAddNew( T item );

		private class ActivityUtils : ScheduleDataConnectorUtilities<ActivityBase> 
		{
			protected override bool StoreBeginEditData( ActivityBase item )
			{
				return item.StoreBeginEditData( );
			}

			protected override bool RestoreBeginEditData( ActivityBase item )
			{
				return item.RestoreBeginEditData( );
			}

			protected override void ClearBeginEditData( ActivityBase item )
			{
				item.ClearBeginEditData( );
			}

			public override ItemOperationResult<ActivityBase> CreateOperationResult( ActivityBase item, DataErrorInfo errorInfo, bool markComplete )
			{
				return new ActivityOperationResult( item, errorInfo, markComplete );
			}

			public override ItemOperationResult<ActivityBase> CreateOperationResult( ActivityBase item )
			{
				return new ActivityOperationResult( item );
			}

			public override bool IsAddNew( ActivityBase item )
			{
				return item.IsAddNew;
			}
		}

		private class ResourceUtils : ScheduleDataConnectorUtilities<Resource>
		{
			protected override bool StoreBeginEditData( Resource item )
			{
				return item.StoreBeginEditData( );
			}

			protected override bool RestoreBeginEditData( Resource item )
			{
				return item.RestoreBeginEditData( );
			}

			protected override void ClearBeginEditData( Resource item )
			{
				item.ClearBeginEditData( );
			}

			public override ItemOperationResult<Resource> CreateOperationResult( Resource item, DataErrorInfo errorInfo, bool markComplete )
			{
				return new ResourceOperationResult( item, errorInfo, markComplete ) as ItemOperationResult<Resource>;
			}

			public override ItemOperationResult<Resource> CreateOperationResult( Resource item )
			{
				return new ResourceOperationResult( item );
			}

			public override bool IsAddNew( Resource item )
			{
				return false;
			}
		}

		public static ScheduleDataConnectorUtilities<T> g_instance;

		public static ScheduleDataConnectorUtilities<T> Instance
		{
			get
			{
				if ( null == g_instance )
				{
					if ( typeof( ActivityBase ) == typeof( T ) )
						g_instance = new ActivityUtils( ) as ScheduleDataConnectorUtilities<T>;
					else if ( typeof( Resource ) == typeof( T ) )
						g_instance = new ResourceUtils( ) as ScheduleDataConnectorUtilities<T>;
					else
						Debug.Assert( false, string.Format( "Add an entry for {0} type.", typeof( T ).Name ) );
				}

				return g_instance;
			}
		}

		#region DefaultBeginEditImplementation

		internal static bool DefaultBeginEditImplementation( T item, out DataErrorInfo errorInfo )
		{
			if ( ! Instance.StoreBeginEditData( item ) )
			{
				errorInfo = ScheduleUtilities.CreateDiagnosticFromId( item, "LE_ActivityBeingEdited" );//"Activity is already being edited."
				return false;
			}

			errorInfo = null;
			return true;
		}

		#endregion // DefaultBeginEditImplementation

		#region DefaultCancelEditImplementation

		internal static bool DefaultCancelEditImplementation( T item, out DataErrorInfo errorInfo )
		{
			Instance.RestoreBeginEditData( item );

			errorInfo = null;
			return true;
		}

		#endregion // DefaultCancelEditImplementation

		#region DefaultEndEditImplementation

		internal static ItemOperationResult<T> DefaultEndEditImplementation( T item )
		{
			Instance.ClearBeginEditData( item );

			return Instance.CreateOperationResult( item, null, true );
		}

		#endregion // DefaultEndEditImplementation
	}

	
#region Infragistics Source Cleanup (Region)







































#endregion // Infragistics Source Cleanup (Region)

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