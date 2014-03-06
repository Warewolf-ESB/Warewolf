using System;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Provides data for Axis RangeChanged events. 
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AxisRangeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes an AxisRangeChangedEventArgs object.
        /// </summary>
        /// <param name="oldMinimumValue">The axis minimum value before the range changed.</param>
        /// <param name="minimumValue">The axis minimum value after the range changed.</param>
        /// <param name="oldMaximumValue">The axis maximum value before the range changed.</param>
        /// <param name="maximumValue">The axis minimum value after the range changed.</param>
        public AxisRangeChangedEventArgs(double oldMinimumValue, double minimumValue,
                                            double oldMaximumValue, double maximumValue)
        {
            OldMinimumValue = oldMinimumValue;
            MinimumValue = minimumValue;

            OldMaximumValue = oldMaximumValue;
            MaximumValue = maximumValue;
        }

        /// <summary>
        /// Gets the minimum value before the range changed. The reported minimum is the effective,
        /// not the set value.
        /// </summary>
        public double OldMinimumValue { get; private set; }

        /// <summary>
        /// Gets the minimum value after the range changed. The reported minimum is the effective,
        /// not the set value.
        /// </summary>
        public double MinimumValue { get; private set; }

        /// <summary>
        /// Gets the maximum value before the range changed. The reported maximum is the effective,
        /// not the set value.
        /// </summary>
        public double OldMaximumValue { get; private set; }

        /// <summary>
        /// Gets the maximum value after the range changed. The reported maximum is the effective,
        /// not the set value.
        /// </summary>
        public double MaximumValue { get; private set; }
    }

    /// <summary>
    /// Represents the method that will handle Axis RangeChanged events.
    /// </summary>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void AxisRangeChangedEventHandler(object sender, AxisRangeChangedEventArgs e);
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