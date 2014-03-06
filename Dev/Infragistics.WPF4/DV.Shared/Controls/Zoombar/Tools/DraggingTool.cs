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
using System.Diagnostics;

namespace Infragistics.Controls
{
    /// <summary>
    /// Represents a tool that is used for dragging the thumb.
    /// </summary>
    public class DraggingTool : Tool, ICancelableTool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DraggingTool"/> class.
        /// </summary>
        public DraggingTool(InteractiveControl view)
            : base(view)
        {

        }

        #region ICancelableTool Members

        bool ICancelableTool.IsCanceled
        {
            get
            {
                return this.IsCanceled;
            }
            set
            {
                this.IsCanceled = value;
            }
        }

        #endregion

        private Point _prevPoint;
        private Point PrevPoint
        {
            get { return _prevPoint; }
            set { _prevPoint = value; }
        }

        private ThumbNode _thumbNode;
        private ThumbNode ThumbNode
        {
            get { return _thumbNode; }
            set { _thumbNode = value; }
        }

        private bool _isCanceled;       
        internal bool IsCanceled
        {
            get { return _isCanceled; }
            set { _isCanceled = value; }
        }


        /// <summary>
        /// Determines whether this tool can start.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// 	<c>true</c> if this tool can start; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanStart(MouseButtonEventArgs e)
        {
             InteractiveElement element = this.View.HitTest(this.LastInput.DocMousePosition);
             if (element != null)
             {
                 ThumbNode thumbNode = element as ThumbNode;
                 if (thumbNode != null)
                 {
                     this.ThumbNode = thumbNode;
                     this.PrevPoint = this.LastInput.DocMousePosition;

                     return true;
                 }
             }

             return false;
        }

        /// <summary>
        /// Called when this tool starts.
        /// </summary>
        public override void Start()
        {
            base.Start();

            this.ThumbNode.Start();
            this.IsCanceled = false;
        }

        /// <summary>
        /// Called when this tool is started and the mouse is moved.
        /// </summary>
        public override void MouseMove()
        {
            Point pt = this.LastInput.DocMousePosition;

            double dx = pt.X - this.PrevPoint.X;
            double dy = pt.Y - this.PrevPoint.Y;

            Point offset = new Point(dx, dy);

            this.ThumbNode.DoMove(offset);

            this.PrevPoint = pt;
        }

        /// <summary>
        /// Called when a key is down.
        /// </summary>
        public override void KeyDown()
        {
            base.KeyDown();

            if (this.LastInput.Key == Key.Escape)
            {
                this.IsCanceled = true;
                this.StopTool();
            }
        }

        /// <summary>
        /// Called when this tool is started and the left mouse button is up.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        public override void MouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.StopTool();
        }

        /// <summary>
        /// Called when this tool stops.
        /// </summary>
        public override void Stop()
        {
            this.ThumbNode.Finish(this.IsCanceled);
            base.Stop();
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