using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;

namespace Infragistics.Controls.Schedules
{
	internal class EndEditForResourcesHelper
	{
		#region Member Variables

		private ExchangeScheduleDataConnector _connector;
		private ErrorCallback _onError;
		private Action _onUserConfigurationUpdateCompleted;
		private ResourceOperationResult _result;
		private Resource _resource; 

		#endregion //Member Variables

		#region Constructor

		private EndEditForResourcesHelper(ExchangeScheduleDataConnector connector, Resource resource)
		{
			_connector = connector;
			_resource = resource;

			_onError = this.OnError;
			_onUserConfigurationUpdateCompleted = this.OnUserConfigurationUpdateCompleted;
		} 

		#endregion //Constructor

		#region Methods

		#region EndEdit

		private ResourceOperationResult EndEdit()
		{
			ExchangeService service = _connector.FindServiceAssociatedWithResource(_resource);

			DataErrorInfo error = null;
			if (service == null)
				error = DataErrorInfo.CreateError(_resource, ExchangeConnectorUtilities.GetString("LE_ErrorUpdatingResource"));

			if (error != null ||
				_connector.ValidateResource(_resource, out error) == false)
			{
				DataErrorInfo tmp;
				_connector.CancelEdit(_resource, out tmp);

				return new ResourceOperationResult(_resource, error, true);
			}

			_result = new ResourceOperationResult(_resource);
			service.UpdateCategories(_onUserConfigurationUpdateCompleted, _onError);
			return _result;
		}

		#endregion //EndEdit

		#region Execute

		public static ResourceOperationResult Execute(ExchangeScheduleDataConnector connector, Resource resource)
		{
			EndEditForResourcesHelper helper = new EndEditForResourcesHelper(connector, resource);
			return helper.EndEdit();
		}

		#endregion //Execute

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			DataErrorInfo errorInfo1;
			_connector.CancelEdit(_resource, out errorInfo1);

			DataErrorInfo errorInfo2 = _connector.GetDataErrorInfo(reason, error, _resource);

			if (errorInfo1 != null)
				errorInfo2 = DataErrorInfo.CreateFromList(new List<DataErrorInfo>() { errorInfo1, errorInfo2 });

			_result.InitializeResult(errorInfo2, true);
			return false;
		}

		#endregion //OnError

		#region OnUserConfigurationUpdateCompleted

		private void OnUserConfigurationUpdateCompleted()
		{
			_resource.ClearBeginEditData();
			_result.InitializeResult(null, true);
		}

		#endregion //OnUserConfigurationUpdateCompleted 

		#endregion //Methods
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