using System.Windows;
using System.Windows.Controls;
using Infragistics.Controls.Menus;

namespace Infragistics.Controls.Menus.Primitives
{
    /// <summary>
    /// A control responcible for displaying the line that connects the <see cref="XamDataTreeNode"/> to the <see cref="NodeLineControl"/> to display node lines.
    /// </summary>
    [TemplateVisualState(GroupName = "Lines", Name = "None")]
    [TemplateVisualState(GroupName = "Lines", Name = "T")]
    [TemplateVisualState(GroupName = "Lines", Name = "L")]
    public class NodeLineTerminator : Control
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeLineTerminator"/> class.
        /// </summary>
        public NodeLineTerminator()
        {




            this.SnapsToDevicePixels = true;

        }


        /// <summary>
        /// Static constructor for the <see cref="NodeLineTerminator"/> class.
        /// </summary>
        static NodeLineTerminator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeLineTerminator), new FrameworkPropertyMetadata(typeof(NodeLineTerminator)));
        }


        #endregion // Constructor

        #region Node

        /// <summary>
        /// Identifies the <see cref="Node"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register("Node", typeof(XamDataTreeNode), typeof(NodeLineTerminator), new PropertyMetadata(new PropertyChangedCallback(NodeChanged)));

        /// <summary>
        /// Gets / sets the <see cref="XamDataTreeNode"/> which is associated with this <see cref="NodeLineTerminator"/>.
        /// </summary>
        public XamDataTreeNode Node
        {
            get { return (XamDataTreeNode)this.GetValue(NodeProperty); }
            set { this.SetValue(NodeProperty, value); }
        }

        private static void NodeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLineTerminator ctrl = (NodeLineTerminator)obj;
            ctrl.EnsureState();
        }

        #endregion // Node

        #region NodeLineEnd

        /// <summary>
        /// Identifies the <see cref="NodeLineEnd"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeLineEndProperty = DependencyProperty.Register("NodeLineEnd", typeof(NodeLineTemination), typeof(NodeLineTerminator), new PropertyMetadata(NodeLineTemination.None, new PropertyChangedCallback(NodeLineEndChanged)));

        /// <summary>
        /// Gets / sets the <see cref="NodeLineTemination"/> value that will determine which state will be displayed.
        /// </summary>
        public NodeLineTemination NodeLineEnd
        {
            get { return (NodeLineTemination)this.GetValue(NodeLineEndProperty); }
            set { this.SetValue(NodeLineEndProperty, value); }
        }

        private static void NodeLineEndChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLineTerminator ctrl = (NodeLineTerminator)obj;

            ctrl.EnsureState();
        }

        #endregion // NodeLineEnd

        #region Methods


        /// <summary>
        /// Ensures that <see cref="NodeLineTerminator"/> is in the correct state.
        /// </summary>
        protected internal virtual void EnsureState()
        {
            if (this.Node == null)
                VisualStateManager.GoToState(this, "None", false);
            else
            {
                switch (this.NodeLineEnd)
                {
                    case (NodeLineTemination.None):
                        {
                            VisualStateManager.GoToState(this, "None", false);
                            break;
                        }
                    case (NodeLineTemination.TShape):
                        {
                            VisualStateManager.GoToState(this, "T", false);
                            break;
                        }
                    case (NodeLineTemination.LShape):
                        {
                            VisualStateManager.GoToState(this, "L", false);
                            break;
                        }
                }
            }
        }

        #endregion // Methods
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