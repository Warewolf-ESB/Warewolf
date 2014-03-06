using System;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Menus.Primitives
{
    /// <summary>
    /// A <see cref="Panel"/> which is used to display the node lines in the <see cref="XamDataTree"/>.
    /// </summary>
    public class NodeLinePanel : Panel
    {
        #region Node

        /// <summary>
        /// Identifies the <see cref="Node"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register("Node", typeof(XamDataTreeNode), typeof(NodeLinePanel), new PropertyMetadata(new PropertyChangedCallback(NodeChanged)));

        /// <summary>
        /// Gets / sets the <see cref="XamDataTreeNode"/> which is associated with this <see cref="NodeLineControl"/>.
        /// </summary>
        public XamDataTreeNode Node
        {
            get { return (XamDataTreeNode)this.GetValue(NodeProperty); }
            set { this.SetValue(NodeProperty, value); }
        }

        private static void NodeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion // Node

        #region Overrides

        #region ArrangeOverride
        /// <summary>
        /// Arranges each node that should be visible, one on top of the other, similar to a 
        /// Vertical <see cref="StackPanel"/>.
        /// </summary>
        /// <param propertyName="finalSize">
        /// The final area within the parent that this object 
        /// should use to arrange itself and its children.
        /// </param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size sizeSoFar = new Size(0, finalSize.Height);

            if (this.Node != null && this.Node.NodeLayout != null)
            {
                if (this.Children.Count > 0)
                {
                    double indentation = this.Node.NodeLayout.IndentationResolved;
                    foreach (UIElement child in this.Children)
                    {
                        child.Arrange(new Rect(sizeSoFar.Width, 0, indentation, finalSize.Height));

                        sizeSoFar.Width += child.DesiredSize.Width;
                    }
                }
            }
            return sizeSoFar;
        }

        #endregion // ArrangeOverride

        #region MeasureOverride

        /// <summary>
        /// Determines how the node lines will fit.
        /// </summary>
        /// <param propertyName="availableSize">        
        ///	</param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            double width = 0.0;
            double height = 0.0;
            foreach (UIElement ui in this.Children)
            {
                ui.Measure(availableSize);
                width += ui.DesiredSize.Width;
                height = Math.Max(height, ui.DesiredSize.Height);
            }

            return new Size(width, height);
        }

        #endregion // MeasureOverride

        #endregion // Overrides
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