using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Parameterizes a financial calculation event.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FinancialEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a FinancialEventArgs object.
        /// </summary>
        /// <param name="position">The beginning position that should be calculated from.</param>
        /// <param name="count">The number of positions that should be calculated from the start.</param>
        /// <param name="dataSource">The data to use for the calculation.</param>
        /// <param name="supportingCalculations">The supporting calculations to use in the calculation.</param>
        public FinancialEventArgs(int position, int count, 
            FinancialCalculationDataSource dataSource, 
            FinancialCalculationSupportingCalculations supportingCalculations)
        {
            Position = position;
            Count = count;
            DataSource = dataSource;
            SupportingCalculations = supportingCalculations;
        }

        /// <summary>
        /// The beginning position that should be calculated from.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// The number of positions that should be calculated from the start.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The data to use for the calculation.
        /// </summary>
        public FinancialCalculationDataSource DataSource { get; private set; }

        /// <summary>
        /// The supporting calculations to use in the calculation.
        /// </summary>
        public FinancialCalculationSupportingCalculations SupportingCalculations { get; private set; }

        /// <summary>
        /// Used to specify which columns changing will invalidate the series and cause it to 
        /// be recalculated.
        /// </summary>
        public IList<string> BasedOn { get; set; }
    }

    /// <summary>
    /// For handling financial calculation events.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The parameters of the event.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void FinancialEventHandler(object sender, FinancialEventArgs e);
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