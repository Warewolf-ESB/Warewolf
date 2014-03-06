using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows.Media;
using Infragistics.Controls.Schedules;

namespace Infragistics.Services.Schedules
{
	/// <summary>
	/// The contract for a service which communicates list data about schedule objects between a client and server.
	/// </summary>
	/// <seealso cref="WcfListConnectorService"/>
	/// <seealso cref="WcfListConnectorServiceSingle"/>
	/// <seealso cref="WcfListConnectorServiceMulti"/>
	[ServiceContract]
	public interface IWcfListConnectorService
	{
		/// <summary>
		/// Gets the informatoin necessary to set up the data connector on the client.
		/// </summary>
		/// <param name="context">
		/// The client context information and all paramters needed by the remote service call.
		/// </param>
		[OperationContract]
		GetInitialInfoResult GetInitialInfo(GetInitialInfoContext context);

		/// <summary>
		/// Performs an add, update, or remove operation on an activity.
		/// </summary>
		/// <param name="context">
		/// The client context information and all paramters needed by the remote service call.
		/// </param>
		[OperationContract]
		PerformActivityOperationResult PerformActivityOperation(PerformActivityOperationContext context);

		/// <summary>
		/// A dummy method used to check for service availability.
		/// </summary>
		[OperationContract]
		void Ping();

		/// <summary>
		/// Gets a collection of version information for each item source.
		/// </summary>
		/// <param name="context">
		/// The client context information and all paramters needed by the remote service call.
		/// </param>
		[OperationContract]
		PollForItemSourceChangesResult PollForItemSourceChanges(PollForItemSourceChangesContext context);

		/// <summary>
		/// Gets a collection of detailed changes to an item source since a specifiec version of it.
		/// </summary>
		/// <param name="context">
		/// The client context information and all paramters needed by the remote service call.
		/// </param>
		[OperationContract]
		PollForItemSourceChangesDetailedResult PollForItemSourceChangesDetailed(PollForItemSourceChangesDetailedContext context);

		/// <summary>
		/// Gets a list of activities which meet the criteria of the given linq query.
		/// </summary>
		/// <param name="context">
		/// The client context information and all paramters needed by the remote service call.
		/// </param>
		[OperationContract]
		QueryActivitiesResult QueryActivities(QueryActivitiesContext context);
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