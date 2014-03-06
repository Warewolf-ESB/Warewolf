using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;


#pragma warning disable 1574
using Infragistics.Services;

namespace Infragistics.Controls.Schedules.Services



{
    #region ActivityEventArgs Class

    /// <summary>
    /// Base class for activity event args.
    /// </summary>
    public class ActivityEventArgs : EventArgs
    {
        #region Member Vars

        private ActivityBase _activity;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public ActivityEventArgs( ActivityBase activity )
        {
            CoreUtilities.ValidateNotNull( activity );

            _activity = activity;
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region Activity

        /// <summary>
        /// The activity for which the operation is being performed.
        /// </summary>
        public ActivityBase Activity
        {
            get
            {
                return _activity;
            }
        }

        #endregion // Activity

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // ActivityEventArgs Class



#region Infragistics Source Cleanup (Region)



























































































































































































































#endregion // Infragistics Source Cleanup (Region)

	#region ActivityAddedEventArgs Class

	/// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityAdded"/> event.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity that was added.
    /// </para>
    /// </remarks>
    /// <seealso cref="XamScheduleDataManager.ActivityAdded"/>
    public class ActivityAddedEventArgs : ActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public ActivityAddedEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityAddedEventArgs Class


#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

    #region ActivityChangedEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityChanged"/> event.
    /// </summary>
    public class ActivityChangedEventArgs : ActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity">Activity object.</param>
        public ActivityChangedEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityChangedEventArgs Class


#region Infragistics Source Cleanup (Region)































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

    #region ActivityRemovedEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityRemoved"/> event.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity that was removed.
    /// </para>
    /// </remarks>
    /// <seealso cref="XamScheduleDataManager.ActivityAdded"/>
    public class ActivityRemovedEventArgs : ActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public ActivityRemovedEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityRemovedEventArgs Class


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