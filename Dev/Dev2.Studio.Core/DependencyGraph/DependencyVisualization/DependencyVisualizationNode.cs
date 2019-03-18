#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;

namespace Dev2.Common.DependencyVisualization
{
    public class DependencyVisualizationNode : INotifyPropertyChanged, IDependencyVisualizationNode
    {
        #region Class Members


        const string _errorImagePath = "/Images/Close_Box_Red.png";

        #endregion Class Members

        #region Fields

        double _locationX, _locationY;
        readonly bool _isBroken;

        #endregion Fields

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        #region Constructor

        public DependencyVisualizationNode(string id, double locationX, double locationY, bool isTarget, bool isBroken)
        {
            NodeDependencies = new List<IDependencyVisualizationNode>();
            CircularDependencies = new List<ICircularDependency>();
            LocationX = locationX;
            LocationY = locationY;
            ID = id;
            IsTargetNode = isTarget;
            _isBroken = isBroken;
        }

        #endregion Constructor

        #region Properties

        public List<ICircularDependency> CircularDependencies { get; }

        public bool HasCircularDependency => CircularDependencies.Any();

        public string ID { get; }

        public bool IsTargetNode { get; set; }
        public string ErrorImagePath => _isBroken ? _errorImagePath : "";

        public double LocationX
        {
            get { return _locationX; }
            set
            {                
                _locationX = value;
                OnPropertyChanged("LocationX");
            }
        }

        public double LocationY
        {
            get { return _locationY; }
            set
            {                
                _locationY = value;
                OnPropertyChanged("LocationY");
            }
        }

        public List<IDependencyVisualizationNode> NodeDependencies { get; }

        public double NodeWidth => 100;

        public double NodeHeight => 50;

        public bool IsBroken => _isBroken;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public new string ToString()
        {
            var result = new StringBuilder(
                $"<node id=\"{ID}\" x=\"{LocationX}\" y=\"{LocationY}\" broken=\"{IsBroken}\">");

            foreach (var nodeDependency in NodeDependencies)
            {
                result.Append($"<dependency id=\"{nodeDependency.ID}\" />");
            }

            result.Append("</node>");

            return result.ToString();
        }

        public List<ICircularDependency> FindCircularDependencies()
        {
            if (NodeDependencies.Count == 0)
            {
                return null;
            }

            var circularDependencies = new List<ICircularDependency>();

            var stack = new Stack<NodeInfo>();
            stack.Push(new NodeInfo(this));

            while (stack.Any())
            {
                var current = stack.Peek().GetNextDependency();
                if (current != null)
                {
                    PushNodeToStack(circularDependencies, stack, current);
                }
                else
                {
                    stack.Pop();
                }
            }

            return circularDependencies;
        }

        private void PushNodeToStack(List<ICircularDependency> circularDependencies, Stack<NodeInfo> stack, NodeInfo current)
        {
            if (current.Node == this)
            {
                var nodes = stack.Select(info => info.Node);
                circularDependencies.Add(new CircularDependency(nodes));
            }
            else
            {
                var visited = stack.Any(info => info.Node == current.Node);
                if (!visited)
                {
                    stack.Push(current);
                }
            }
        }

        class NodeInfo
        {
            public NodeInfo(IDependencyVisualizationNode node)
            {
                Node = node;
                _index = 0;
            }

            public IDependencyVisualizationNode Node { get; }

            public NodeInfo GetNextDependency()
            {
                if (_index < Node.NodeDependencies.Count)
                {
                    var nextNode = Node.NodeDependencies[_index++];
                    return new NodeInfo(nextNode);
                }

                return null;
            }

            int _index;
        }

        #endregion Methods
    }
}
