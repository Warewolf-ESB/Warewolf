using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;


#pragma warning disable 1574
using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
    #region DaySettingsOverrideCollection Class

    /// <summary>
    /// Collection of <see cref="DaySettingsOverride"/> objects.
    /// </summary>
    /// <remarks>
    /// <b>DaySettingsOverrideCollection</b> is used by the <see cref="ScheduleSettings"/>'s <see cref="ScheduleSettings.DaySettingsOverrides"/>
    /// and <see cref="Resource"/>'s <see cref="Resource.DaySettingsOverrides"/> properties.
    /// </remarks>
    /// <seealso cref="ScheduleSettings.DaySettingsOverrides"/>
    /// <seealso cref="Resource.DaySettingsOverrides"/>
    /// <seealso cref="ScheduleSettings.DaysOfWeek"/>
    public class DaySettingsOverrideCollection : ObservableCollectionExtended<DaySettingsOverride>
    {
        #region Member Vars

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="DaySettingsOverrideCollection"/>.
        /// </summary>
        public DaySettingsOverrideCollection( )
            : base( false, true )
        {
        }

        #endregion // Constructor
    }

    #endregion // DaySettingsOverrideCollection Class
	
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