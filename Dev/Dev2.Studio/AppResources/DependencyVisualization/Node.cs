
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Dev2.AppResources.DependencyVisualization
{
    /// <summary>
    /// Represents an item in a graph.
    /// </summary>
    public class Node : INotifyPropertyChanged
    {
        #region Class Members

        private const string _errorImagePath = "/Images/Close_Box_Red.png";

        #endregion Class Members

        #region Fields

        double _locationX, _locationY;
        private readonly bool _isBroken;

        #endregion // Fields

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        #region Constructor

        public Node(string id, double locationX, double locationY, bool isTarget, bool isBroken)
        {
            NodeDependencies = new List<Node>();
            CircularDependencies = new List<CircularDependency>();
            LocationX = locationX;
            LocationY = locationY;
            ID = id;
            IsTargetNode = isTarget;
            _isBroken = isBroken;
        }

        #endregion // Constructor

        #region Properties

        public List<CircularDependency> CircularDependencies { get; private set; }

        public bool HasCircularDependency
        {
            get { return CircularDependencies.Any(); }
        }

        public string ID { get; private set; }

        public bool IsTargetNode { get; set; }
        public string ErrorImagePath { get { return _isBroken ? _errorImagePath : ""; } }

        public double LocationX
        {
            get { return _locationX; }
            set
            {
                if(value == _locationX)
                    return;

                _locationX = value;

                OnPropertyChanged("LocationX");
            }
        }

        public double LocationY
        {
            get { return _locationY; }
            set
            {
                if(value == _locationY)
                    return;

                _locationY = value;

                OnPropertyChanged("LocationY");
            }
        }

        public List<Node> NodeDependencies { get; private set; }

        public double NodeWidth
        {
            get { return 100; }
        }

        public double NodeHeight
        {
            get { return 50; }
        }

        public bool IsBroken { get { return _isBroken; } }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public new string ToString()
        {
            StringBuilder result = new StringBuilder(string.Format("<node id=\"{0}\" x=\"{1}\" y=\"{2}\" broken=\"{3}\">", ID, LocationX, LocationY, IsBroken));

            foreach(var nodeDependency in NodeDependencies)
            {
                result.Append(string.Format("<dependency id=\"{0}\" />", nodeDependency.ID));
            }

            result.Append("</node>");

            return result.ToString();
        }

        public List<CircularDependency> FindCircularDependencies()
        {
            if(NodeDependencies.Count == 0)
                return null;

            var circularDependencies = new List<CircularDependency>();

            var stack = new Stack<NodeInfo>();
            stack.Push(new NodeInfo(this));

            while(stack.Any())
            {
                var current = stack.Peek().GetNextDependency();
                if(current != null)
                {
                    if(current.Node == this)
                    {
                        var nodes = stack.Select(info => info.Node);
                        circularDependencies.Add(new CircularDependency(nodes));
                    }
                    else
                    {
                        bool visited = stack.Any(info => info.Node == current.Node);
                        if(!visited)
                            stack.Push(current);
                    }
                }
                else
                {
                    stack.Pop();
                }
            }

            return circularDependencies;
        }

        private class NodeInfo
        {
            public NodeInfo(Node node)
            {
                Node = node;
                _index = 0;
            }

            public Node Node { get; private set; }

            public NodeInfo GetNextDependency()
            {
                if(_index < Node.NodeDependencies.Count)
                {
                    var nextNode = Node.NodeDependencies[_index++];
                    return new NodeInfo(nextNode);
                }

                return null;
            }

            int _index;
        }

        #endregion // Methods

    }
}
