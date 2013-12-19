using System.Windows;
using System.Windows.Documents;
using Dev2.AppResources.DependencyVisualization;

namespace CircularDependencyTool
{
    /// <summary>
    /// Hosts a NodeConnectionAdorner in the adorner layer.
    /// </summary>
    public class NodeConnectionAdornerDecorator
        // Derive from AdornerDecorator in case the 
        // Window that hosts the GraphView has a custom
        // template that does not include an AdornerLayer.
        : AdornerDecorator 
    {
        #region Constructor

        public NodeConnectionAdornerDecorator()
        {
            base.Loaded += this.OnLoaded;
        }

        #endregion // Constructor

        #region OnLoaded

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(this);
            if (layer == null)
                return;

            _adorner = new NodeConnectionAdorner(this);
            layer.Add(_adorner);
            this.GiveGraphToAdorner();
        }

        #endregion // OnLoaded

        #region Graph (DP)

        public Graph Graph
        {
            get { return (Graph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public static readonly DependencyProperty GraphProperty =
            DependencyProperty.Register(
            "Graph",
            typeof(Graph),
            typeof(NodeConnectionAdornerDecorator),
            new UIPropertyMetadata(null, OnGraphChanged));

        static void OnGraphChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as NodeConnectionAdornerDecorator).GiveGraphToAdorner();
        }

        #endregion // Graph (DP)

        #region Private Helpers

        void GiveGraphToAdorner()
        {
            if (_adorner != null && this.Graph != null)
            {
                _adorner.Graph = this.Graph;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {

            return base.MeasureOverride(constraint);
        }

        #endregion // Private Helpers

        #region Fields

        NodeConnectionAdorner _adorner;

        #endregion // Fields
    }
}