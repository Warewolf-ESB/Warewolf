using System;
using System.IO;

namespace Infragistics.Windows.Reporting
{
    #region OutputFormat
    /// <summary>
    /// Identifies the output format of an export operation.
    /// </summary>
    /// <seealso cref="Report.Export(OutputFormat)"/>
    /// <seealso cref="Report.Export(OutputFormat, Stream)"/>
    /// <seealso cref="Report.Export(OutputFormat, string)"/>
    /// <seealso cref="Report.Export(OutputFormat, string, bool)"/>
    public enum OutputFormat
    {
        /// <summary>
        /// Exports to XPS format
        /// </summary>
        XPS = 0,
    }
    #endregion



    // JJD 11/10/09 - TFS24546 - added
    #region PrintCancelationReason
    /// <summary>
    /// Indicates why the print operation was canceled. 
    /// </summary>
    /// <seealso cref="Infragistics.Windows.Reporting.Events.PrintEndedEventArgs"/>
    public enum PrintCancelationReason
    {
        /// <summary>
        /// The print operation was not canceled
        /// </summary>
        NotCanceled,

        /// <summary>
        /// The print operation was canceled by the user
        /// </summary>
        User,

        /// <summary>
        /// There aren't any printer's defined.
        /// </summary>
        NoPrinterAvailable,
    }
    #endregion //PrintCancelationReason

    #region PrintStatus
    /// <summary>
    /// Indicates the printing status. 
    /// </summary>
    /// <seealso cref="Infragistics.Windows.Reporting.Events.PrintEndedEventArgs"/>
    public enum PrintStatus
    {
        /// <summary>
        /// The print operation ended successfully
        /// </summary>
        Successful,

        /// <summary>
        /// The print operation was cancelled by the user.
        /// </summary>
        Canceled,
    }
    #endregion
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