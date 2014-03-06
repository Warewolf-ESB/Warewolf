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
using Infragistics.Controls.Charts.Messaging;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents a connector class that links Model, View and Controller together.
    /// </summary>
    public class SparklineConnector
    {
        internal XamSparkline Model { get; set; }
        internal SparklineController Controller { get; set; }
        internal XamSparklineView View { get; set; }
        internal HorizontalAxisView HorizontalAxis { get; set; }
        internal VerticalAxisView VerticalAxis { get; set; }

        /// <summary>
        /// Creates a new sparkline connector.
        /// </summary>
        /// <param name="parent">Parent object.</param>
        /// <param name="view">Instance of the sparkline view.</param>
        public SparklineConnector(DependencyObject parent, XamSparklineView view)
        {
            View = view;

            while (!(parent is XamSparkline) && parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent == null)
            {
                return;
            }

            Model = parent as XamSparkline;
            HorizontalAxis = Model.HorizontalAxis;
            VerticalAxis = Model.VerticalAxis;

            ServiceProvider sp = new ServiceProvider();
            sp.AddService("ConfigurationMessages", new MessageChannel());
            sp.AddService("RenderingMessages", new MessageChannel());
            sp.AddService("InteractionMessages", new MessageChannel());
            sp.AddService("Model", Model);
            sp.AddService("View", View);
            sp.AddService("HorizontalAxis", HorizontalAxis);
            sp.AddService("VerticalAxis", VerticalAxis);

            Model.ServiceProvider = sp;
            View.ServiceProvider = sp;
            HorizontalAxis.ServiceProvider = sp;
            VerticalAxis.ServiceProvider = sp;
            
            Controller = new SparklineController(sp);
            sp.AddService("Controller", Controller);
        }
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