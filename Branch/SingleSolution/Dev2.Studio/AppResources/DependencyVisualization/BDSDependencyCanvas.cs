using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPF.JoshSmith.Panels;
using System.Windows;
using System.Windows.Media;

namespace CircularDependencyTool {
    public class BDSDependencyCanvas : DragCanvas {

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved) {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            base.UpdateLayout();
        }
    }
}
