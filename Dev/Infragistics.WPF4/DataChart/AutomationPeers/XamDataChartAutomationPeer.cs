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
using System.Windows.Automation.Peers;

namespace Infragistics.Controls.Charts.AutomationPeers
{
    /// <summary>
    /// The automation peer for the the data chart
    /// </summary>
    public class XamDataChartAutomationPeer : FrameworkElementAutomationPeer
    {
        #region Constructor

        /// <summary>
        /// Instantiates the automation peer.
        /// </summary>
        /// <param name="chart">chart control reference</param>
        public XamDataChartAutomationPeer(XamDataChart chart)
        :
            base(chart)
        {
            this.Chart = chart;
        }

        #endregion //Constructor

        #region Properties

        XamDataChart Chart { get; set; }
        
        #endregion //Properties

        #region Overrides

        /// <summary>
        /// Overrides the framework invocation of what generic type of control this is.
        /// </summary>
        /// <returns>The <see cref="AutomationControlType"/> that describes this control.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        /// <summary>
        /// Overrides the framework invocation requesting a string that describes this control.
        /// </summary>
        /// <returns>A string describing the name of this control.</returns>
        protected override string GetClassNameCore()
        {
            return this.Chart.GetType().Name;
        }

        /// <summary>
        /// Returns the chart control's name.
        /// </summary>
        /// <returns>Chart's name</returns>
        protected override string GetNameCore()
        {
            if (string.IsNullOrEmpty(this.Chart.Name))
            {
                return this.Chart.GetType().Name;
            }
            else
            {
                return this.Chart.Name;
            }
        }

        /// <summary>
        /// Overrides the framework invocation of when a request is made for what types of user interaction are avalible. 
        /// </summary>
        /// <param name="patternInterface">This is the type of user interaction requested.</param>
        /// <returns>An object that can handle this pattern or null if none available.</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            return base.GetPattern(patternInterface);
        }

        #endregion //Overrides
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