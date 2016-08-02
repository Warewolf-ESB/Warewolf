/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;

namespace Dev2.Common
{
    /// <summary>
    /// Used to generate dependency graphs.
    /// Extracted From View Model ;)
    /// </summary>
    public class DependencyGraphGenerator
    {
        /// <summary>
        /// Builds the graph.
        /// </summary>
        /// <param name="xmlData">The XML data.</param>
        /// <param name="modelName">Name of the model.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="nestingLevel">How deep should the graph show.</param>
        /// <returns></returns>
        public Graph BuildGraph(StringBuilder xmlData, string modelName, double width, double height, int nestingLevel)
        {
            if (xmlData == null || xmlData.Length == 0)
            {
                return new Graph(ErrorResource.DependencyMissing);
            }

            XElement xe = xmlData.ToXElement();

            // Create a graph.
            var graphElem = xe.AncestorsAndSelf("graph").FirstOrDefault();
            if (graphElem == null)
            {
                return new Graph(ErrorResource.DependencyInormationMalformed);
            }

            try
            {
                string title = graphElem.Attribute("title").Value;
                var graph = new Graph(title);
                double count = 0;

                var nodeElems = graphElem.Elements("node").ToList();

                // Create all of the nodes and add them to the graph.
                foreach (XElement nodeElem in nodeElems)
                {
                    // build the graph position data
                    string id = nodeElem.Attribute("id").Value;
                    var node = CreateNode(nodeElem, modelName, width, height, ref count);

                    bool alreadyAdded = false;
                    foreach (Node n in graph.Nodes)
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
                }

                // Associate each node with its dependencies.
                int graphCount = graph.Nodes.Count - 1;
                if (nestingLevel > 0)
                {
                    for (int i = 0; i <= nestingLevel; i++)
                    {
                        if (nestingLevel < graphCount)
                        {
                            graph.Nodes.RemoveAt(graphCount);
                            graphCount = graph.Nodes.Count - 1;
                        }
                    }
                }
                foreach (Node node in graph.Nodes)
                {
                    var nodeElem = nodeElems.First(elem => elem.Attribute("id").Value == node.ID);
                    var dependencyElems = nodeElem.Elements("dependency");
                    foreach (XElement dependencyElem in dependencyElems)
                    {
                        string depID = dependencyElem.Attribute("id").Value;

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

                // Tell the graph to inspect itself for circular dependencies.
                graph.CheckForCircularDependencies();

                return graph;
            }
            catch
            {
                return new Graph(ErrorResource.DependencyInormationMalformed);
            }
        }

        private Node CreateNode(XElement nodeElm, string resourceName, double width, double height, ref double count)
        {
            double screenWidth = width;
            double screenHeight = height - 150;
            int centerX = Convert.ToInt32(screenWidth / 2);
            int centerY = Convert.ToInt32(screenHeight / 2);
            int maxX = Convert.ToInt32(screenWidth);
            int maxY = Convert.ToInt32(screenHeight);
            const int Distance = 300;
            Point centerPoint = new Point(centerX, centerY);

            double x;
            double y;

            var tmpX = nodeElm.AttributeSafe("x");
            var tmpY = nodeElm.AttributeSafe("y");
            double.TryParse(tmpX, out x);
            double.TryParse(tmpY, out y);

            string id = nodeElm.Attribute("id").Value;
            bool isTarget = id == resourceName;
            bool broken = String.Equals(nodeElm.Attribute("broken").Value, "true", StringComparison.OrdinalIgnoreCase);

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

                int xCoOrd = (int)Math.Round(centerPoint.X - Distance * Math.Sin(count));
                int yCoOrd = (int)Math.Round(centerPoint.Y - Distance * Math.Cos(count));

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

            return new Node(id, x, y, isTarget, broken);
        }
    }
}