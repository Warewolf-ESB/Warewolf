using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Infragistics.Controls.Menus.Primitives
{
    /// <summary>
    /// A control which will display the node lines in a <see cref="XamDataTree"/>.
    /// </summary>
    public class NodeLineControl : Control, INotifyPropertyChanged
    {
        #region Members

        Panel _sp;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeLineControl"/> class.
        /// </summary>        
        public NodeLineControl()
        {



            this.SnapsToDevicePixels = true;

        }


        /// <summary>
        /// Static constructor for the <see cref="NodeLineControl"/> class.
        /// </summary>
        static NodeLineControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeLineControl), new FrameworkPropertyMetadata(typeof(NodeLineControl)));
        }


        #endregion // Constructor

        #region Properties

        #region Node

        /// <summary>
        /// Identifies the <see cref="Node"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register("Node", typeof(XamDataTreeNode), typeof(NodeLineControl), new PropertyMetadata(new PropertyChangedCallback(NodeChanged)));

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
            NodeLineControl ctrl = (NodeLineControl)obj;
            ctrl.OnPropertyChanged("Node");
            ctrl.BuildNodeIndicators();
            if (ctrl.Node == null)
                ctrl._originalLevel = -1;
            else
                ctrl._originalLevel = ctrl.Node.Manager.Level;
        }

        #endregion // Node

        #region VerticalLine

        /// <summary>
        /// Identifies the <see cref="VerticalLine"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VerticalLineProperty = DependencyProperty.Register("VerticalLine", typeof(DataTemplate), typeof(NodeLineControl), new PropertyMetadata(new PropertyChangedCallback(VerticalLineChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> which will be used to display connections between sibling <see cref="XamDataTreeNode"/>s.
        /// </summary>
        public DataTemplate VerticalLine
        {
            get { return (DataTemplate)this.GetValue(VerticalLineProperty); }
            set { this.SetValue(VerticalLineProperty, value); }
        }

        private static void VerticalLineChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLineControl ctrl = (NodeLineControl)obj;
            ctrl.OnPropertyChanged("VerticalLine");
        }

        #endregion // VerticalLine

        #region HorizontalLine

        /// <summary>
        /// Identifies the <see cref="HorizontalLine"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty HorizontalLineProperty = DependencyProperty.Register("HorizontalLine", typeof(DataTemplate), typeof(NodeLineControl), new PropertyMetadata(new PropertyChangedCallback(HorizontalLineChanged)));

        /// <summary>
        /// Gets / sets the <see cref="DataTemplate"/> which will be used to display connections between <see cref="XamDataTreeNode"/>s and their horizontal line.
        /// </summary>
        public DataTemplate HorizontalLine
        {
            get { return (DataTemplate)this.GetValue(HorizontalLineProperty); }
            set { this.SetValue(HorizontalLineProperty, value); }
        }

        private static void HorizontalLineChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            NodeLineControl ctrl = (NodeLineControl)obj;
            ctrl.OnPropertyChanged("HorizontalLine");
        }

        #endregion // HorizontalLine

        #endregion // Properties

        #region Overrides

        /// <summary>
        /// Builds the visual tree for the <see cref="NodeLineControl"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _sp = base.GetTemplateChild("StackPanel") as NodeLinePanel;

            this.BuildNodeIndicators();
        }


        /// <summary>        
        ///     Provides the behavior for the Measure pass of Silverlight layout. Classes
        ///     can override this method to define their own Measure pass behavior.
        ///</summary>
        ///<param name="availableSize">
        ///     The available size that this object can give to child objects. Infinity (System.Double.PositiveInfinity)
        ///     can be specified as a value to indicate that the object will size to whatever
        ///     content is available.
        ///     </param>
        /// <returns>
        ///     The size that this object determines it needs during layout, based on its
        ///     calculations of the allocated sizes for child objects; or based on other
        ///     considerations, such as a fixed container size.
        ///     </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Node != null && this.Node.JustMoved)
            {
                this.BuildNodeIndicators();
            }

            return base.MeasureOverride(availableSize);
        }

        private int _originalLevel = -1;

        #endregion // Overrides

        #region Methods

        /// <summary>
        /// Creates the node lines that will be used to connect sibling nodes togethers.
        /// </summary>
        protected virtual void BuildNodeIndicators()
        {
            if (this._sp != null && this.Node != null && this.Node.NodeLayout != null)
            {
                this.Width = (this.Node.Manager.Level) * this.Node.NodeLayout.IndentationResolved;

                this._sp.Children.Clear();

                if (this.Node.Manager.ParentNode != null)
                {
                    XamDataTreeNode node = this.Node;

                    if (this.Node.NodeLayout.Tree.NodeLineVisibility == System.Windows.Visibility.Visible)
                    {
                        while (node.Manager.ParentNode != null)
                        {
                            node = node.Manager.ParentNode;
                            if (node.Manager.Nodes.Count >= 1)
                            {
                                if (node.Manager.Nodes.IndexOf(node) != (node.Manager.Nodes.Count - 1))
                                {
                                    FrameworkElement fe = this.VerticalLine.LoadContent() as FrameworkElement;

                                    fe.Width = node.NodeLayout.IndentationResolved;

                                    _sp.Children.Insert(0, fe);
                                }
                                else
                                {
                                    Rectangle rect = new Rectangle() { Width = node.NodeLayout.IndentationResolved, Fill = new SolidColorBrush(Colors.Transparent) };
                                    _sp.Children.Insert(0, rect);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion // Methods

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="XamDataTree"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="XamDataTree"/> object.
        /// </summary>
        /// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
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