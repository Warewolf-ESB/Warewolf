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

namespace Infragistics
{
    /// <summary>
    /// Mouse interaction helper class.
    /// </summary>
    public class Tool : ITool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tool"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        protected Tool(InteractiveControl view)
        {
            this.View = view;
        }

        #region ITool Members

        /// <summary>
        /// Determines whether this tool can start.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this tool can start; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanStart()
        {
            return false;
        }

        /// <summary>
        /// Determines whether this tool can start.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        /// <returns>
        /// 	<c>true</c> if this tool can start; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanStart(MouseButtonEventArgs e)
        {
            return false;
        }
        
        /// <summary>
        /// Called when this tool starts.
        /// </summary>
        public virtual void Start()
        {
        }

        /// <summary>
        /// Called when a key is down.
        /// </summary>
        public virtual void KeyDown()
        {
        }

        /// <summary>
        /// Called when a key is up.
        /// </summary>
        public virtual void KeyUp()
        {
        }

        /// <summary>
        /// Called when this tool is started and the mouse is moved.
        /// </summary>
        public virtual void MouseMove()
        {
        }

        /// <summary>
        /// Called when this tool is started and the left mouse button is down.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        public virtual void MouseLeftButtonDown(MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Called when this tool is started and the left mouse button is up.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        public virtual void MouseLeftButtonUp(MouseButtonEventArgs e)
        {
        }       

        /// <summary>
        /// Called when this tool is started and the left mouse button is double clicked.
        /// </summary>
        public virtual void OnMouseLeftButtonDoubleClick()
        {
        }

        /// <summary>
        /// Called when this tool stops.
        /// </summary>
        public virtual void Stop()
        {
        }

        #endregion

        private InteractiveControl _view;
        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        public InteractiveControl View
        {
            get
            {
                return this._view;
            }
            private set
            {
                this._view = value;
            }
        }

        /// <summary>
        /// Gets the last input.
        /// </summary>
        /// <value>The last input.</value>
        public InputContext LastInput
        {
            get
            {
                return this._view.LastInput;
            }
        }

        /// <summary>
        /// Stops the tool.
        /// </summary>
        public void StopTool()
        {
            if (this.View.CurrentTool == this)
            {
                this.View.ReleaseMouseCaptures();
                this.View.CurrentTool = null;
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