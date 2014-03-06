using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a set of methods and properties used to create tickmark values.
    /// </summary>
    public abstract class TickmarkValues : DependencyObject
    {
        /// <summary>
        /// Initializes a new set of tickmark values.
        /// </summary>
        /// <param name="initializationParameters">initialization parameters</param>
        public virtual void Initialize(TickmarkValuesInitializationParameters initializationParameters)
        {
            this.VisibleMaximum = initializationParameters.VisibleMaximum;
        }

        /// <summary>
        /// Gets or sets the maximum tickmark value.
        /// </summary>
        protected double VisibleMaximum { get; set; }

        /// <summary>
        /// Gets or sets the tickmark interval.
        /// </summary>
        protected internal double Interval { get; protected set; }

        /// <summary>
        /// Gets or sets the first tickmark index.
        /// </summary>
        protected internal int FirstIndex { get; protected set; }

        /// <summary>
        /// Gets or sets the last tickmark index.
        /// </summary>
        protected internal int LastIndex { get; protected set; }



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Returns a set of major tickmark values.
        /// </summary>
        /// <returns>Major tickmark values</returns>
        public abstract IEnumerable<double> MajorValues();

        /// <summary>
        /// Returns a set of minor tickmark values.
        /// </summary>
        /// <returns>Minor tickmark values</returns>
        public abstract IEnumerable<double> MinorValues();


        /// <summary>
        /// Gets or sets the number of minor tickmarks between two consecutive major tickmarks.
        /// </summary>
        protected internal int MinorCount { get; protected set; }
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