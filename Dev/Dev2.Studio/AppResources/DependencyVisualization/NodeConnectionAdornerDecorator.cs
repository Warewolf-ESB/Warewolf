
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Documents;
using Dev2.AppResources.DependencyVisualization;

// ReSharper disable once CheckNamespace
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
            Loaded += OnLoaded;
        }

        #endregion // Constructor

        #region OnLoaded

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(this);
            if(layer == null)
                return;

            _adorner = new NodeConnectionAdorner(this);
            layer.Add(_adorner);
            GiveGraphToAdorner();
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
            NodeConnectionAdornerDecorator nodeConnectionAdornerDecorator = sender as NodeConnectionAdornerDecorator;
            if(nodeConnectionAdornerDecorator != null)
            {
                nodeConnectionAdornerDecorator.GiveGraphToAdorner();
            }
        }

        #endregion // Graph (DP)

        #region Private Helpers

        void GiveGraphToAdorner()
        {
            if(_adorner != null && Graph != null)
            {
                _adorner.Graph = Graph;
            }
        }

        #endregion // Private Helpers

        #region Fields

        NodeConnectionAdorner _adorner;

        #endregion // Fields
    }
}
