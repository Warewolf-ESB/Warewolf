//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Infragistics.Controls.Schedules
//{
//    /// <summary>
//    /// Used for providing schedule data from a Microsoft Exchange Server through a WCF proxy service.
//    /// Note: This data connector will be designed after implementing the direct connect Exchange connector.
//    /// </summary>
//    /// <seealso cref="XamScheduleDataManager.DataConnector"/>
//#if !SILVERLIGHT
//    [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]
//#endif
//    public class WcfExchangeScheduleDataConnector : ScheduleDataConnectorBase
//    {
//        #region Base Class Overrides

//        /// <summary>
//        /// 
//        /// </summary>
//        public override ResourceCollection ResourceItems
//        {
//            get { throw new NotImplementedException(); }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="activityType"></param>
//        /// <returns></returns>
//        protected internal override bool AreActivitiesSupported(ActivityType activityType)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="operation"></param>
//        /// <returns></returns>
//        protected internal override CancelOperationResult CancelPendingOperation(OperationResult operation)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="activityType"></param>
//        /// <param name="errorInfo"></param>
//        /// <returns></returns>
//        protected internal override ActivityBase CreateNew(ActivityType activityType, out DataErrorInfo errorInfo)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="query"></param>
//        /// <returns></returns>
//        protected internal override ActivityQueryResult GetActivities(ActivityQuery query)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="activityType"></param>
//        /// <param name="activityFeature"></param>
//        /// <param name="calendar"></param>
//        /// <returns></returns>
//        protected internal override bool IsActivityFeatureSupported(ActivityType activityType, ActivityFeature activityFeature, ResourceCalendar calendar)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="activityType"></param>
//        /// <param name="activityOperation"></param>
//        /// <param name="calendar"></param>
//        /// <returns></returns>
//        protected internal override bool IsActivityOperationSupported(ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="activity"></param>
//        /// <returns></returns>
//        protected internal override ActivityOperationResult Remove(ActivityBase activity)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="calendar"></param>
//        /// <param name="subscriber"></param>
//        /// <param name="error"></param>
//        protected internal override void SubscribeToReminders(ResourceCalendar calendar, ReminderSubscriber subscriber, out DataErrorInfo error)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="calendar"></param>
//        /// <param name="subscriber"></param>
//        protected internal override void UnsubscribeFromReminders(ResourceCalendar calendar, ReminderSubscriber subscriber)
//        {
//            throw new NotImplementedException();
//        }

//        #endregion  // Base Class Overrides
//    }
//}

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