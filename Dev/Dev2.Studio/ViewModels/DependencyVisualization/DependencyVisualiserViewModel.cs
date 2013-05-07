using CircularDependencyTool;
using Dev2.Common;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Unlimited.Framework;

namespace Dev2.Studio.ViewModels.DependencyVisualization
{
    public class DependencyVisualiserViewModel : BaseWorkSurfaceViewModel
    {
        private IContextualResourceModel _resourceModel;
        bool _getDependsOnMe;

        public ObservableCollection<Graph> Graphs { get; set; }

        public IContextualResourceModel ResourceModel
        {
            get
            {
                return _resourceModel;
            }
            set
            {
                if (_resourceModel == value) return;

                _resourceModel = value;
                Graphs = BuildGraphs().ToObservableCollection();
                NotifyOfPropertyChange(() => ResourceModel);
                if (value != null)
                    NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public bool GetDependsOnMe { 
            get
        {
            return _getDependsOnMe;
        }
        set
        {
            _getDependsOnMe = value;
        }}

        public override string DisplayName
        {
            get
            {
                if(GetDependsOnMe)
                {
                    return string.Format("{0}*Dependants",
                        ResourceModel.ResourceName);
                }
                return string.Format("{0}*Dependencies",
                        ResourceModel.ResourceName);
            }
        }

        // NOTE: This method is invoked from DependencyVisualiser.xaml
        public IEnumerable<Graph> BuildGraphs()
        {
            dynamic test = new UnlimitedObject();
            test.Service = "FindDependencyService";
            test.ResourceName = ResourceModel.ResourceName;
            test.GetDependsOnMe = GetDependsOnMe;
            var workspaceID = ((IStudioClientContext)ResourceModel.Environment.DsfChannel).WorkspaceID;
            dynamic data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(ResourceModel.Environment.DsfChannel.ExecuteCommand(test.XmlString, workspaceID, GlobalConstants.NullDataListID));

            if (data == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, test.Service));
            }

            dynamic gr = data.graph;

            string testd = string.Empty;

            if (gr is List<UnlimitedObject>)
            {
                foreach (dynamic item in gr)
                {
                    dynamic nodes = item.node;
                    if (nodes is List<UnlimitedObject>)
                    {
                        double screenWidth = SystemParameters.FullPrimaryScreenWidth - 200;
                        double screenHeight = SystemParameters.FullPrimaryScreenHeight - 200;

                        int centerX = Convert.ToInt32(screenWidth / 2);
                        int centerY = Convert.ToInt32(screenHeight / 2);



                        int maxX = Convert.ToInt32(screenWidth);
                        int maxY = Convert.ToInt32(screenHeight);

                        int nodeCount = nodes.Count;


                        Point centerPoint = new Point(centerX, centerY);
                        Point result = new Point(0, 0);
                        int distance = 300;

                        double degrees = 360 / nodeCount;






                        double count = 1;
                        foreach (dynamic dynNode in nodes)
                        {

                            if (dynNode.id == ResourceModel.ResourceName)
                            {
                                dynNode.x = centerX;
                                dynNode.y = centerY;
                            }
                            else
                            {

                                if (count > 360)
                                    count = 1.5;

                                int xCoOrd = ((int)Math.Round(centerPoint.X + distance * Math.Sin(count)));
                                int yCoOrd = ((int)Math.Round(centerPoint.Y + distance * Math.Cos(count)));

                                if (xCoOrd >= maxX)
                                    xCoOrd = maxX;

                                if (yCoOrd >= maxY)
                                    yCoOrd = maxY;


                                dynNode.x = xCoOrd;
                                dynNode.y = yCoOrd;
                                count++;
                                //if (x >= maxX) {
                                //    x = maxX;
                                //}
                                //x += 60;
                                //dynNode.x = x;


                                //y += 50;
                                //if (y >= maxY) {
                                //    y = maxY;
                                //}
                                //dynNode.y = y;
                            }

                        }

                    }

                    testd = item.XmlString;
                }

            }


            return new List<Graph>
            {
                

                BuildGraph(testd)

            };
        }

        public Graph BuildGraph(string xmlData)
        {

            if (string.IsNullOrEmpty(xmlData))
            {
                return new Graph("Dependency information could not be retrieved");
            }
            XDocument xdoc = XDocument.Parse(xmlData);
            //XDocument xdoc = XDocument.Load(xmlData);



            // Create a graph.
            var graphElem = xdoc.Element("graph");
            string title = graphElem.Attribute("title").Value;
            var graph = new Graph(title);

            var nodeElems = graphElem.Elements("node").ToList();

            // Create all of the nodes and add them to the graph.
            foreach (XElement nodeElem in nodeElems)
            {
                string id = nodeElem.Attribute("id").Value;
                bool broken = String.Equals(nodeElem.Attribute("broken").Value, "true", StringComparison.OrdinalIgnoreCase);
                double x = (double)nodeElem.Attribute("x");
                double y = (double)nodeElem.Attribute("y");
                bool isTarget = id == ResourceModel.ResourceName;

                var node = new Node(id, x, y, isTarget, broken);

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
            foreach (Node node in graph.Nodes)
            {
                var nodeElem = nodeElems.First(elem => elem.Attribute("id").Value == node.ID);
                var dependencyElems = nodeElem.Elements("dependency");
                foreach (XElement dependencyElem in dependencyElems)
                {
                    string depID = dependencyElem.Attribute("id").Value;

                    //if (depID == node.ID)
                    //    throw new Exception("A node cannot be its own dependency.  Node ID = " + depID);

                    var dependency = graph.Nodes.FirstOrDefault(n => n.ID == depID);
                    if (dependency != null)
                        node.NodeDependencies.Add(dependency);
                }
            }

            // Tell the graph to inspect itself for circular dependencies.
            graph.CheckForCircularDependencies();

            return graph;
        }
    }
}