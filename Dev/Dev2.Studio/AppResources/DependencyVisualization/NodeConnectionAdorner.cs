
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Dev2.AppResources.DependencyVisualization;

// ReSharper disable once CheckNamespace
namespace CircularDependencyTool
{
    /// <summary>
    /// A UI adorner that renders connectors between nodes in a graph.
    /// </summary>
    public class NodeConnectionAdorner : Adorner
    {
        #region Constructor

        public NodeConnectionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            _nodeConnectors = new List<NodeConnector>();
        }

        #endregion // Constructor

        #region Properties

        public Graph Graph
        {
            get { return _graph; }
            set
            {
                if(value == _graph)
                    return;

                _graph = value;

                if(_graph != null)
                    ProcessGraph();
            }
        }

        #endregion // Properties

        #region Private Helpers

        void ProcessGraph()
        {
            RemoveConnectors();

            foreach(Node node in _graph.Nodes)
                foreach(Node dependency in node.NodeDependencies)
                    AddConnector(node, dependency);

            InvalidateMeasure();
        }

        void RemoveConnectors()
        {
            foreach(NodeConnector connector in _nodeConnectors)
            {
                RemoveVisualChild(connector);
                RemoveLogicalChild(connector);
            }

            _nodeConnectors.Clear();
        }

        void AddConnector(Node startNode, Node endNode)
        {
            var connector = new NodeConnector(startNode, endNode);

            _nodeConnectors.Add(connector);

            // Add the connector to the visual and logical tree so that
            // rendering and resource inheritance will work properly.
            AddVisualChild(connector);
            AddLogicalChild(connector);

        }

        #endregion // Private Helpers

        #region Base Class Overrides

        protected override IEnumerator LogicalChildren
        {
            get { return _nodeConnectors.GetEnumerator(); }
        }

        protected override int VisualChildrenCount
        {
            get { return _nodeConnectors.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _nodeConnectors[index];
        }

        protected override Size MeasureOverride(Size constraint)
        {
            foreach(var nodeConnector in _nodeConnectors)
                nodeConnector.Measure(constraint);

            if(Double.IsInfinity(constraint.Width) || Double.IsInfinity(constraint.Height))
                return new Size(10000, 10000);





            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(AdornedElement);

            if(!bounds.IsEmpty)
                foreach(var nodeConnector in _nodeConnectors)
                    nodeConnector.Arrange(bounds);


            return finalSize;
        }

        #endregion // Base Class Overrides

        #region Fields

        readonly List<NodeConnector> _nodeConnectors;
        Graph _graph;

        #endregion // Fields
    }
}
