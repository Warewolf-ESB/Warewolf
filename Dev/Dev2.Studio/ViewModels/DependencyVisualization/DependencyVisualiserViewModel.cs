using Caliburn.Micro;
using CircularDependencyTool;
using Dev2.Common;
using Dev2.Services.Events;
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
        private ObservableCollection<Graph> _graphs;

        public DependencyVisualiserViewModel(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        } 
        
        public DependencyVisualiserViewModel()
            : base(EventPublishers.Aggregator)
        {
        }

        public ObservableCollection<Graph> Graphs
        {
            get { return _graphs ?? (_graphs = new ObservableCollection<Graph>()); }
        }

        private double _availableWidth;
        public double AvailableWidth
        {
            get
            {
                return _availableWidth;
            }
            set
            {
                if (_availableWidth.CompareTo(value) == 0)
                {
                    return;
                }

                _availableWidth = value;
                BuildGraphs();
                NotifyOfPropertyChange(() => AvailableWidth);
            }
        }

        private double _availableHeight;
        public double AvailableHeight
        {
            get
            {
                return _availableHeight;
            }
            set
            {
                if (_availableHeight.CompareTo(value) == 0)
                {
                    return;
                }

                _availableHeight = value;
                BuildGraphs();
                NotifyOfPropertyChange(() => AvailableHeight);
            }
        }

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
                BuildGraphs();
                NotifyOfPropertyChange(() => ResourceModel);
                if (value != null)
                    NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public bool GetDependsOnMe { get; set; }

        public override string DisplayName
        {
            get
            {
                return string.Format(GetDependsOnMe ? "{0}*Dependants" 
                    : "{0}*Dependencies", ResourceModel.ResourceName);
            }
        }

        // NOTE: This method is invoked from DependencyVisualiser.xaml
        public void BuildGraphs()
        {
            Graphs.Clear();

            dynamic test = new UnlimitedObject();
            test.Service = "FindDependencyService";
            test.ResourceName = ResourceModel.ResourceName;
            test.GetDependsOnMe = GetDependsOnMe;
            var workspaceID = ResourceModel.Environment.Connection.WorkspaceID;
            dynamic data = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(ResourceModel.Environment.Connection.ExecuteCommand(test.XmlString, workspaceID, GlobalConstants.NullDataListID));

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
                        double screenWidth = AvailableWidth;
                        double screenHeight = AvailableHeight - 150;
                        int centerX = Convert.ToInt32(screenWidth / 2);
                        int centerY = Convert.ToInt32(screenHeight / 2);
                        int maxX = Convert.ToInt32(screenWidth);
                        int maxY = Convert.ToInt32(screenHeight);
                        int nodeCount = nodes.Count;
                        double degrees = 360 / nodeCount;
                        int distance = 300;
                        Point centerPoint = new Point(centerX, centerY);
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

                                int xCoOrd = ((int)Math.Round(centerPoint.X - distance * Math.Sin(count)));
                                int yCoOrd = ((int)Math.Round(centerPoint.Y - distance * Math.Cos(count)));

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

            var graphs = BuildGraph(testd);
            Graphs.Add(graphs);
        }

        public Graph BuildGraph(string xmlData)
        {

            if (string.IsNullOrEmpty(xmlData))
            {
                return new Graph("Dependency information could not be retrieved");
            }
            XDocument xdoc = XDocument.Parse(xmlData);

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

                    var dependency = graph.Nodes.FirstOrDefault(n => n.ID == depID);
                    if (dependency != null)
                        node.NodeDependencies.Add(dependency);
                }

                //Now adjust position according to nodesize
                node.LocationX = node.LocationX - node.NodeWidth;
                node.LocationY = node.LocationY - node.NodeHeight/2;
            }

            // Tell the graph to inspect itself for circular dependencies.
            graph.CheckForCircularDependencies();

            return graph;
        }
    }
}