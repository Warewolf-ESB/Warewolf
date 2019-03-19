#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.DependencyVisualization;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Dev2.Common.Interfaces;
using Warewolf.Resource.Errors;

namespace Dev2.Common
{
    public class DependencyGraphGenerator : IDependencyGraphGenerator
    {
        public IGraph BuildGraph(StringBuilder xmlData, string modelName, double width, double height, int nestingLevel)
        {
            if (xmlData == null || xmlData.Length == 0)
            {
                return new Graph(ErrorResource.DependencyMissing);
            }

            var xe = xmlData.ToXElement();

            // Create a graph.
            var graphElem = xe.AncestorsAndSelf("graph").FirstOrDefault();
            if (graphElem == null)
            {
                return new Graph(ErrorResource.DependencyInormationMalformed);
            }

            try
            {
                var title = graphElem.Attribute("title").Value;
                var graph = new Graph(title);
                double count = 0;

                var nodeElems = graphElem.Elements("node").ToList();

                // Create all of the nodes and add them to the graph.
                foreach (var nodeElem in nodeElems)
                {
                    count = BuildGraphPositionData(modelName, width, height, graph, count, nodeElem);
                }

                // Associate each node with its dependencies.
                AssociateEachNodeWithItsDependencies(nestingLevel, graph, nodeElems);

                // Tell the graph to inspect itself for circular dependencies.
                graph.CheckForCircularDependencies();

                return graph;
            }
            catch (Exception ex)
            {
                return new Graph(ErrorResource.DependencyInormationMalformed);
            }
        }

        private static void AssociateEachNodeWithItsDependencies(int nestingLevel, Graph graph, System.Collections.Generic.List<XElement> nodeElems)
        {
            var graphCount = graph.Nodes.Count - 1;
            if (nestingLevel > 0)
            {
                for (var i = 0; i <= nestingLevel; i++)
                {
                    if (nestingLevel < graphCount)
                    {
                        graph.Nodes.RemoveAt(graphCount);
                        graphCount = graph.Nodes.Count - 1;
                    }
                }
            }
            foreach (var node in graph.Nodes)
            {
                var nodeElem = nodeElems.First(elem => elem.Attribute("id").Value == node.ID);
                var dependencyElems = nodeElem.Elements("dependency");
                foreach (var dependencyElem in dependencyElems)
                {
                    var depID = dependencyElem.Attribute("id").Value;

                    var dependency = graph.Nodes.FirstOrDefault(n => n.ID == depID);
                    if (dependency != null)
                    {
                        node.NodeDependencies.Add(dependency);
                    }
                }

                //Now adjust position according to nodesize
                node.LocationX = node.LocationX - node.NodeWidth;
                node.LocationY = node.LocationY - node.NodeHeight / 2;
            }
        }

        private double BuildGraphPositionData(string modelName, double width, double height, Graph graph, double count, XElement nodeElem)
        {
            var id = nodeElem.Attribute("id").Value;
            var node = CreateNode(nodeElem, modelName, width, height, ref count);

            var alreadyAdded = false;
            foreach (var n in graph.Nodes)
            {
                if (n.ID == id)
                {
                    alreadyAdded = true;
                }
            }

            if (!alreadyAdded)
            {
                graph.Nodes.Add(node);
            }

            return count;
        }

        IDependencyVisualizationNode CreateNode(XElement nodeElm, string resourceName, double width, double height, ref double count)
        {
            var screenWidth = width;
            var screenHeight = height - 150;
            var centerX = Convert.ToInt32(screenWidth / 2);
            var centerY = Convert.ToInt32(screenHeight / 2);
            var maxX = Convert.ToInt32(screenWidth);
            var maxY = Convert.ToInt32(screenHeight);
            const int Distance = 300;
            var centerPoint = new Point(centerX, centerY);

            var tmpX = nodeElm.AttributeSafe("x");
            var tmpY = nodeElm.AttributeSafe("y");
            double.TryParse(tmpX, out double x);
            double.TryParse(tmpY, out double y);


            var id = nodeElm.Attribute("id").Value;
            var isTarget = id == resourceName;

            var broken = string.Equals(nodeElm.Attribute("broken").Value, "true", StringComparison.OrdinalIgnoreCase);

            if (isTarget)
            {
                x = centerX;
                y = centerY;
            }
            else
            {
                if (count > Distance)
                {
                    count = 1.5;
                }

                var xCoOrd = (int)Math.Round(centerPoint.X - Distance * Math.Sin(count));
                var yCoOrd = (int)Math.Round(centerPoint.Y - Distance * Math.Cos(count));

                if (xCoOrd >= maxX)
                {
                    xCoOrd = maxX;
                }

                if (yCoOrd >= maxY)
                {
                    yCoOrd = maxY;
                }

                x = xCoOrd;
                y = yCoOrd;
                count++;
            }

            return new DependencyVisualizationNode(id, x, y, isTarget, broken);
        }
    }
}