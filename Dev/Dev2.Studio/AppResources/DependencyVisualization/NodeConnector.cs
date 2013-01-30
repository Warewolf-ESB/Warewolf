using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
//using MvvmFoundation.Wpf;
using Petzold.Media2D;
using System.ComponentModel;

namespace CircularDependencyTool
{
    /// <summary>
    /// A visual arrow element that connects two nodes in a graph.
    /// </summary>
    public class NodeConnector : ArrowLine
    {
        #region Constructor

        public NodeConnector(Node startNode, Node endNode)
        {
            _startNode = startNode;
            _endNode = endNode;

            this.SetIsPartOfCircularDependency();
            this.SetToolTip();
            this.UpdateLocations(false);

            startNode.PropertyChanged += new PropertyChangedEventHandler(startNode_PropertyChanged);
            endNode.PropertyChanged += new PropertyChangedEventHandler(endNode_PropertyChanged);

            //_startObserver = new PropertyObserver<Node>(_startNode)
            //    .RegisterHandler(n => n.LocationX, n => this.UpdateLocations(true))
            //    .RegisterHandler(n => n.LocationY, n => this.UpdateLocations(true));

            //_endObserver = new PropertyObserver<Node>(_endNode)
            //    .RegisterHandler(n => n.LocationX, n => this.UpdateLocations(true))
            //    .RegisterHandler(n => n.LocationY, n => this.UpdateLocations(true));
        }

        #endregion // Constructor

        #region IsPartOfCircularDependency (DP)

        public bool IsPartOfCircularDependency
        {
            get { return (bool)GetValue(IsPartOfCircularDependencyProperty); }
            private set { SetValue(IsPartOfCircularDependencyPropertyKey, value); }
        }

        static readonly DependencyPropertyKey IsPartOfCircularDependencyPropertyKey =
             DependencyProperty.RegisterReadOnly(
            "IsPartOfCircularDependency",
            typeof(bool),
            typeof(NodeConnector),
            new UIPropertyMetadata(false));

        public static readonly DependencyProperty IsPartOfCircularDependencyProperty = IsPartOfCircularDependencyPropertyKey.DependencyProperty;

        #endregion // IsPartOfCircularDependency (DP)

        #region Private Helpers

        static Point ComputeLocation(Node node1, Node node2)
        {
            // Initially set the location to the center of the first node.
            Point loc = new Point
            {
                X = node1.LocationX + (node1.NodeWidth / 2),
                Y = node1.LocationY + (node1.NodeHeight / 2)
            };

            bool overlapY = Math.Abs(node1.LocationY - node2.LocationY) < node1.NodeHeight;
            if (!overlapY)
            {
                bool above = node1.LocationY < node2.LocationY;
                if (above)
                    loc.Y += node1.NodeHeight / 2;
                else
                    loc.Y -= node1.NodeHeight / 2;
            }

            bool overlapX = Math.Abs(node1.LocationX - node2.LocationX) < node1.NodeWidth;
            if (!overlapX)
            {
                bool left = node1.LocationX < node2.LocationX;
                if (left)
                    loc.X += node1.NodeWidth / 2;
                else
                    loc.X -= node1.NodeWidth / 2;
            }

            return loc;
        }

        void SetIsPartOfCircularDependency()
        {
            this.IsPartOfCircularDependency = _startNode.CircularDependencies.Intersect(_endNode.CircularDependencies).Any();
        }

        void SetToolTip()
        {
            string toolTipText = String.Format("{0} depends on {1}", _startNode.ID, _endNode.ID);

            if (_endNode.NodeDependencies.Contains(_startNode))
                toolTipText += String.Format("\n{0} depends on {1}", _endNode.ID, _startNode.ID);

            base.ToolTip = toolTipText;
        }

        void UpdateLocations(bool animate)
        {
            var start = ComputeLocation(_startNode, _endNode);
            var end = ComputeLocation(_endNode, _startNode);

            if (animate)
            {
                base.BeginAnimation(ArrowLine.X1Property, CreateAnimation(base.X1, start.X));
                base.BeginAnimation(ArrowLine.Y1Property, CreateAnimation(base.Y1, start.Y)); 
                base.BeginAnimation(ArrowLine.X2Property, CreateAnimation(base.X2, end.X)); 
                base.BeginAnimation(ArrowLine.Y2Property, CreateAnimation(base.Y2, end.Y));
            }
            else
            {
                base.X1 = start.X;
                base.Y1 = start.Y;
                base.X2 = end.X;
                base.Y2 = end.Y;
            }
        }

        static AnimationTimeline CreateAnimation(double from, double to)
        {

            return new DoubleAnimation {
                Duration = _Duration,
                From = from,
                To = to
            



            };
            //return new EasingDoubleAnimation
            //{
            //    Duration = _Duration,
            //    Equation = EasingEquation.ElasticEaseOut,
            //    From = from,
            //    To = to
            //};
        }

        #endregion // Private Helpers

        #region Fields

        static readonly Duration _Duration = new Duration(TimeSpan.FromMilliseconds(10));

        readonly Node _startNode, _endNode;
        //readonly PropertyObserver<Node> _startObserver, _endObserver;

        #endregion Fields

        #region Event Handlers

        void endNode_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LocationX" || e.PropertyName == "LocationY")
            {
                UpdateLocations(true);
            }
        }

        void startNode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LocationX" || e.PropertyName == "LocationY")
            {
                UpdateLocations(true);
            }
        }

        #endregion Event Handlers
    }
}