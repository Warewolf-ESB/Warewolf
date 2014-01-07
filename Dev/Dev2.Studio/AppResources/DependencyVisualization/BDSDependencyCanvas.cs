using System;
using System.Windows;
using Dev2.CustomControls.Panels;

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
