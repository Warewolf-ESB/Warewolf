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
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace Infragistics.Controls
{
    internal class ScrollShape : ShapeElement
    {
        private const string NormalState = "Normal";
        private const string MouseOverState = "MouseOver";
        private const string PressedState = "Pressed";

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollShape"/> class.
        /// </summary>
        /// <param name="frameworkElement">The framework element.</param>
        public ScrollShape(FrameworkElement frameworkElement)
        {
            this.FrameworkElement = frameworkElement;
            this.FrameworkElements.Add(frameworkElement);
        }

        private FrameworkElement _frameworkElement;
        public FrameworkElement FrameworkElement
        {
            get { return _frameworkElement; }
            set { _frameworkElement = value; }
        }

        private XamZoombar Zoombar
        {
            get
            {
                return this.View as XamZoombar;
            }
        }

        private bool _moveLeft;
        public bool MoveLeft
        {
            get { return this._moveLeft; }
            set { this._moveLeft = value; }
        }

        protected override void ChangeFrameworkElementSettings(FrameworkElement frameworkElement)
        {
            // base.ChangeFrameworkElementSettings();
        }

        public override void Render()
        {

        }
        public override void Remove()
        {

        }

        public override void OnMouseLeftButtonDown(MouseEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            ChangeVisualState(PressedState);

            double newValue = this.MoveLeft ? this.Zoombar.Minimum : this.Zoombar.Maximum;

            this.Zoombar.MoveTo(newValue, true);
        }
        public override void OnMouseLeftButtonUp(MouseEventArgs e)
        {
            this.Zoombar.StopMove();

            if (this.IsMouseOver)
            {
                ChangeVisualState(MouseOverState);
            }
            else
            {
                ChangeVisualState(NormalState);
            }

            base.OnMouseLeftButtonUp(e);
        }
        public override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            ChangeVisualState(MouseOverState);
        }
        public override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            ChangeVisualState(NormalState);
        }

        private void ChangeVisualState(string newState)
        {
            foreach (FrameworkElement frameworkElement in this.FrameworkElements)
            {
                Control control = frameworkElement as Control;
                if (control != null)
                {
                    VisualStateManager.GoToState(control, newState, true);
                }
            }
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