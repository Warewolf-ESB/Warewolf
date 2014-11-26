
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


//using MvvmFoundation.Wpf;

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using Dev2.AppResources.DependencyVisualization;
using Petzold.Media2D;

// ReSharper disable once CheckNamespace
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

            SetIsPartOfCircularDependency();
            SetToolTip();
            UpdateLocations(false);

            startNode.PropertyChanged += StartNodePropertyChanged;
            endNode.PropertyChanged += EndNodePropertyChanged;
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
            if(!overlapY)
            {
                bool above = node1.LocationY < node2.LocationY;
                if(above)
                    loc.Y += node1.NodeHeight / 2;
                else
                    loc.Y -= node1.NodeHeight / 2;
            }

            bool overlapX = Math.Abs(node1.LocationX - node2.LocationX) < node1.NodeWidth;
            if(!overlapX)
            {
                bool left = node1.LocationX < node2.LocationX;
                if(left)
                    loc.X += node1.NodeWidth / 2;
                else
                    loc.X -= node1.NodeWidth / 2;
            }

            return loc;
        }

        void SetIsPartOfCircularDependency()
        {
            IsPartOfCircularDependency = _startNode.CircularDependencies.Intersect(_endNode.CircularDependencies).Any();
        }

        void SetToolTip()
        {
            string toolTipText = String.Format("{0} depends on {1}", _startNode.ID, _endNode.ID);

            if(_endNode.NodeDependencies.Contains(_startNode))
                toolTipText += String.Format("\n{0} depends on {1}", _endNode.ID, _startNode.ID);

            ToolTip = toolTipText;
        }

        void UpdateLocations(bool animate)
        {
            var start = ComputeLocation(_startNode, _endNode);
            var end = ComputeLocation(_endNode, _startNode);

            if(animate)
            {
                BeginAnimation(X1Property, CreateAnimation(X1, start.X));
                BeginAnimation(Y1Property, CreateAnimation(Y1, start.Y));
                BeginAnimation(X2Property, CreateAnimation(X2, end.X));
                BeginAnimation(Y2Property, CreateAnimation(Y2, end.Y));
            }
            else
            {
                X1 = start.X;
                Y1 = start.Y;
                X2 = end.X;
                Y2 = end.Y;
            }
        }

        static AnimationTimeline CreateAnimation(double from, double to)
        {

            return new DoubleAnimation
            {
                Duration = Duration,
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

        static readonly Duration Duration = new Duration(TimeSpan.FromMilliseconds(10));

        readonly Node _startNode, _endNode;
        //readonly PropertyObserver<Node> _startObserver, _endObserver;

        #endregion Fields

        #region Event Handlers

        void EndNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "LocationX" || e.PropertyName == "LocationY")
            {
                UpdateLocations(true);
            }
        }

        void StartNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "LocationX" || e.PropertyName == "LocationY")
            {
                UpdateLocations(true);
            }
        }

        #endregion Event Handlers
    }
}
