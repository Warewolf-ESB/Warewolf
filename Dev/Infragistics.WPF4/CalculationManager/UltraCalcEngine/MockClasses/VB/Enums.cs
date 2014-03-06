namespace Infragistics
{
    #region Public Enums
    /// <summary>
    /// Indicates the first day of the week to use when calling date-related functions.
    /// </summary>
    internal enum FirstDayOfWeek
    {
        /// <summary>
        /// The first day of the week as specified in your system settings This member
        /// is equivalent to the Visual Basic constant vbUseSystemDayOfWeek.
        /// </summary>
        System = 0,
  
        /// <summary>
        /// Sunday (default) This member is equivalent to the Visual Basic constant vbSunday.
        /// </summary>
        Sunday = 1,
    
        /// <summary>
        /// Monday This member is equivalent to the Visual Basic constant vbMonday.
        /// </summary>
        Monday = 2,

        /// <summary>
        /// Tuesday This member is equivalent to the Visual Basic constant vbTuesday.
        /// </summary>
        Tuesday = 3,

        /// <summary>
        /// Wednesday This member is equivalent to the Visual Basic constant vbWednesday.
        /// </summary>
        Wednesday = 4,
   
        /// <summary>
        /// Thursday This member is equivalent to the Visual Basic constant vbThursday.
        /// </summary>
        Thursday = 5,
  
        /// <summary>
        /// Friday This member is equivalent to the Visual Basic constant vbFriday.
        /// </summary>
        Friday = 6,
     
        /// <summary>
        /// Saturday This member is equivalent to the Visual Basic constant vbSaturday.
        /// </summary>
        Saturday = 7,
    }

    /// <summary>
    /// Indicates how to determine and format date intervals when calling date-related functions.
    /// </summary>

	internal enum DateInterval



	{
        /// <summary>
        /// Year format
        /// </summary>
        Year = 0,
 
        /// <summary>
        /// Quarter of year (1 through 4)
        /// </summary>
        Quarter = 1,
    
        /// <summary>
        /// Month (1 through 12)
        /// </summary>
        Month = 2,
   
        /// <summary>
        /// Day of year (1 through 366)
        /// </summary>
        DayOfYear = 3,
   
        /// <summary>
        /// Day of month (1 through 31)
        /// </summary>
        Day = 4,
  
        /// <summary>
        ///  Week of year (1 through 53)
        /// </summary>
        WeekOfYear = 5,

        /// <summary>
        /// Day of week (1 through 7)
        /// </summary>
        Weekday = 6,
    
        /// <summary>
        /// Hour (1 through 24)
        /// </summary>
        Hour = 7,
   
        /// <summary>
        /// Minute (1 through 60)
        /// </summary>
        Minute = 8,
   
        /// <summary>
        /// Second (1 through 60)
        /// </summary>
        Second = 9,
    }

    /// <summary>
    /// Indicates the first week of the year to use when calling date-related functions.
    /// </summary>

	internal enum FirstWeekOfYear



	{
        /// <summary>
        /// The day of the week specified in your system settings as the first day of
        ///     the week This member is equivalent to the Visual Basic constant vbUseSystem.
        /// </summary>
        System = 0,
   
        /// <summary>
        /// The week in which January 1 occurs (default) This member is equivalent to
        ///     the Visual Basic constant vbFirstJan1.
        /// </summary>
        Jan1 = 1,
   
        /// <summary>
        /// The first week that has at least four days in the new year This member is
        ///     equivalent to the Visual Basic constant vbFirstFourDays.
        /// </summary>
        FirstFourDays = 2,
  
        /// <summary>
        ///  The first full week of the year This member is equivalent to the Visual Basic
        ///     constant vbFirstFullWeek.
        /// </summary>
        FirstFullWeek = 3,
    }
    #endregion Public Enums

    #region Internal Enums
    /// <summary>
    /// Indicates when is the due date.
    /// </summary>
    internal enum DueDate
    {
        /// <summary>
        /// Falls at the end of the date interval
        /// </summary>
        EndOfPeriod = 0,

        /// <summary>
        /// Falls at the beginning of the date interval
        /// </summary>
        BegOfPeriod = 1,
    }
    #endregion Internal Enums
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