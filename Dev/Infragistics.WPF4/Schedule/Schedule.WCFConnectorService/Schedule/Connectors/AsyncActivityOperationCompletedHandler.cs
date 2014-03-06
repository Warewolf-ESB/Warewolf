using System;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;



using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
	#region PropertyChangeTrigger

	internal class PropertyChangeTrigger<TOwner, TActionData> : PropertyChangeListener<TOwner>
		where TOwner : class
	{
		private ISupportPropertyChangeNotifications _item;
		private string _property;
		protected Action<TOwner, TActionData> _action;
		protected TActionData _actionData;

		protected PropertyChangeTrigger( ISupportPropertyChangeNotifications item, string property, Action<TOwner, TActionData> action, TOwner owner, TActionData actionData )
			: base( owner, null )
		{
			_item = item;
			_property = property;
			_action = action;
			_actionData = actionData;

			item.AddListener( this, false );
		}

		internal static void ExecuteWhenPropertyChanges( ISupportPropertyChangeNotifications item, string property, TOwner owner, TActionData actionData, Action<TOwner, TActionData> action )
		{
			var handler = new PropertyChangeTrigger<TOwner, TActionData>( item, property, action, owner, actionData );
		}

		protected void RemoveListener( )
		{
			_item.RemoveListener( this );
		}

		public override void OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			TOwner owner = this.Owner;

			if ( null != owner && _property == property )
				this.OnPropertyChangedOverride( owner );
		}

		protected virtual void OnPropertyChangedOverride( TOwner owner )
		{
			if ( null != _action )
				_action( owner, _actionData );
		}
	} 

	#endregion // PropertyChangeTrigger

	#region AsyncActivityOperationCompletedHandler Class

	/// <summary>
	/// When an async operation is being performed that requires raising of an event when the operation is 
	/// complete, for example the add appointment operation, this class is used to raise the event when the 
	/// operation completes.
	/// </summary>
	internal class AsyncActivityOperationCompletedHandler<TOwner, TActionData> : PropertyChangeTrigger<TOwner, TActionData>
		where TOwner : class
	{
		protected OperationResult _result;

		protected AsyncActivityOperationCompletedHandler( OperationResult result, Action<TOwner, TActionData> action, TOwner owner, TActionData actionData )
			: base( result, "IsComplete", action, owner, actionData )
		{
			_result = result;
		}

		internal static void ExecuteOnComplete( OperationResult result, Action<TOwner, TActionData> action, TOwner owner, TActionData data )
		{
			if ( result.IsComplete )
			{
				action( owner, data );
			}
			else
			{
				var handler = new AsyncActivityOperationCompletedHandler<TOwner, TActionData>( result, action, owner, data );
			}
		}

		protected override void OnPropertyChangedOverride( TOwner owner )
		{
			if ( _result.IsComplete )
				base.OnPropertyChangedOverride( owner );
		}
	}

	#endregion // AsyncActivityOperationCompletedHandler Class



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