using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Threading;

namespace Infragistics.Controls.Charts
{
    internal class KDTree2D
    {
        public KDTree2D(PointData[] points, int maxLeafSize)
        {
            lock (_lock)
            {
                Root = KDTreeHelper(points, 0, points.Length - 1, 0, maxLeafSize);
            }
        }

        public KDTree2D()
        {

        }

        protected PointData[] _progressivePoints;
        protected Stack<KDTreeThunk> _progressiveThunks;
        protected List<KDTreeThunk> _toProcess;
        private int _currentProgressiveLevel = 0;
        private object _lock = new object();

        public object SyncLock { get { return _lock; } }

        public event EventHandler ProgressiveThunkCompleted;

        public static KDTree2D GetProgressive(PointData[] points, int maxLeafSize)
        {
            var ret = new KDTree2D();
            ret.Root = new KDTreeNode2D();
            ret.Root.Unfinished = true;
            ret._progressivePoints = points;
            ret._progressiveThunks = new Stack<KDTreeThunk>();
            ret._toProcess = new List<KDTreeThunk>();

            ret._progressiveThunks.Push(
                new KDTreeThunk()
                {
                    StartIndex = 0,
                    EndIndex = points.Length - 1,
                    Level = 0,
                    MaxLeafSize = maxLeafSize,
                    Node = ret.Root
                });

            return ret;
        }

        public bool ProgressiveStep()
        {
            lock (_lock)
            {
                if (_progressiveThunks.Count == 0)
                {
                    _progressivePoints = null;
                    return false;
                }


                _currentProgressiveLevel = _progressiveThunks.Peek().Level;


                while (_progressiveThunks.Count > 0 &&
                    _progressiveThunks.Peek().Level == _currentProgressiveLevel)
                {
                    _toProcess.Add(_progressiveThunks.Pop());
                }

                Thread t = new Thread(ProcessThunks);
                t.Start();

                return true;
            }
        }

        private void ProcessThunks()
        {
            lock (_lock)
            {
                KDTreeThunk t;
                for (var i = 0; i < _toProcess.Count; i++)
                {
                    t = _toProcess[i];
                    KDTreeHelperProgressive(
                        t.Node,
                        _progressivePoints,
                        t.StartIndex,
                        t.EndIndex,
                        t.Level,
                        t.MaxLeafSize);

                }
                _toProcess.Clear();
            }

            if (ProgressiveThunkCompleted != null)
            {
                ProgressiveThunkCompleted(this, new EventArgs());
            }
        }

        private void KDTreeHelperProgressive(
            KDTreeNode2D node,
            PointData[] points,
            int startIndex,
            int endIndex,
            int level,
            int maxLeafSize)
        {
            node.Unfinished = false;
            node.IsX = (level % 2) == 0;
            node.DescendantCount = endIndex - startIndex;

            if (startIndex == endIndex)
            {
                node.Median = points[startIndex];
                return;
            }
            if (startIndex > endIndex)
            {
                return;
            }

            if (endIndex - startIndex + 1 <= maxLeafSize)
            {
                node.Median = points[startIndex];
                node.OtherPoints = new PointData[endIndex - startIndex + 1];
                int j = 0;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    node.OtherPoints[j++] = points[i];
                }
                return;
            }

            var k = Math.Max((endIndex - startIndex) / 2, 1);
            int medianIndex = Select(points, startIndex, endIndex, node.IsX, k);

            node.Median = points[medianIndex];

            if (startIndex <= medianIndex - 1)
            {
                node.LeftChild = new KDTreeNode2D() { Unfinished = true };
                node.DescendantCount = (medianIndex - 1) - startIndex + 1;
                _progressiveThunks.Push(
                    new KDTreeThunk()
                    {
                        StartIndex = startIndex,
                        EndIndex = medianIndex - 1,
                        Level = level + 1,
                        MaxLeafSize = maxLeafSize,
                        Node = node.LeftChild
                    });
            }
            else
            {
                node.LeftChild = null;
            }

            if (medianIndex + 1 <= endIndex)
            {
                node.RightChild = new KDTreeNode2D() { Unfinished = true };
                node.DescendantCount = endIndex - (medianIndex + 1) + 1;
                _progressiveThunks.Push(
                    new KDTreeThunk()
                    {
                        StartIndex = medianIndex + 1,
                        EndIndex = endIndex,
                        Level = level + 1,
                        MaxLeafSize = maxLeafSize,
                        Node = node.RightChild
                    });
            }
            else
            {
                node.RightChild = null;
            }
        }

        private KDTreeNode2D KDTreeHelper(PointData[] points, int startIndex, int endIndex, int level, int maxLeafSize)
        {
            var node = new KDTreeNode2D();
            node.IsX = (level % 2) == 0;
            node.DescendantCount = endIndex - startIndex;

            if (startIndex == endIndex)
            {
                node.Median = points[startIndex];
                return node;
            }
            if (startIndex > endIndex)
            {
                return null;
            }

            if (endIndex - startIndex + 1 <= maxLeafSize)
            {
                node.Median = points[startIndex];
                node.OtherPoints = new PointData[endIndex - startIndex + 1];
                int j = 0;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    node.OtherPoints[j++] = points[i];
                }
                return node;
            }

            var k = Math.Max((endIndex - startIndex) / 2, 1);
            int medianIndex = Select(points, startIndex, endIndex, node.IsX, k);

            node.Median = points[medianIndex];
            node.LeftChild = KDTreeHelper(points, startIndex, medianIndex - 1, level + 1, maxLeafSize);
            node.RightChild = KDTreeHelper(points, medianIndex + 1, endIndex, level + 1, maxLeafSize);

            return node;
        }

        private int Partition(PointData[] points, bool isX, int startIndex, int endIndex, int pivotIndex)
        {
            var pivotValue = isX ? points[pivotIndex].X : points[pivotIndex].Y;

            var temp = points[pivotIndex];
            points[pivotIndex] = points[endIndex];
            points[endIndex] = temp;
            int storeIndex = startIndex;

            for (var i = startIndex; i < endIndex; i++)
            {
                double val;
                if (isX)
                {
                    val = points[i].X;
                }
                else
                {
                    val = points[i].Y;
                }
                if (val <= pivotValue)
                {
                    temp = points[storeIndex];
                    points[storeIndex] = points[i];
                    points[i] = temp;
                    storeIndex++;
                }
            }

            temp = points[endIndex];
            points[endIndex] = points[storeIndex];
            points[storeIndex] = temp;

            return storeIndex;
        }

        private Random _rand = new Random();

        private int Select(PointData[] points, int startIndex, int endIndex, bool isX, int k)
        {
            if (startIndex == endIndex)
            {
                return startIndex;
            }

            var pivotIndex = _rand.Next(startIndex, endIndex);
            var newIndex = Partition(points, isX, startIndex, endIndex, pivotIndex);
            var pivotDistance = newIndex - startIndex + 1;

            if (pivotDistance == k)
            {
                return newIndex;
            }
            else if (k < pivotDistance)
            {
                return Select(points, startIndex, newIndex - 1, isX, k);
            }
            else
            {
                return Select(points, newIndex + 1, endIndex, isX, k - pivotDistance);
            }
        }

        public void KNearest(
            KNearestResults results,
            double xValue,
            double yValue,
            int k)
        {
            lock (_lock)
            {
                KNearestHelper(results, xValue, yValue, k, Root);
            }
        }

        private void KNearestHelper(
            KNearestResults results,
            double xValue,
            double yValue,
            int k,
            KDTreeNode2D current)
        {
            if (current == null || current.Unfinished)
            {
                return;
            }

            if (current.LeftChild == null &&
                current.RightChild == null)
            {
                AddNearest(
                    results,
                    xValue,
                    yValue,
                    current,
                    current.Median,
                    true,
                    0,
                    k);
                if (results.BreakOut)
                {
                    return;
                }

                if (current.OtherPoints != null
                    && current.OtherPoints.Length > 0)
                {
                    for (var i = 0; i < current.OtherPoints.Length; i++)
                    {
                        AddNearest(
                            results,
                            xValue,
                            yValue,
                            current,
                            current.OtherPoints[i],
                            false,
                            i,
                            k);
                        if (results.BreakOut)
                        {
                            return;
                        }
                    }
                }
                return;
            }

            AddNearest(
                    results,
                    xValue,
                    yValue,
                    current,
                    current.Median,
                    true,
                    0,
                    k);
            if (results.BreakOut)
            {
                return;
            }

            if (current.IsX)
            {
                if (xValue <= current.Median.X)
                {
                    KNearestHelper(results,
                        xValue, yValue,
                        k, current.LeftChild);
                    if (results.BreakOut)
                    {
                        return;
                    }

                    if (Dist(xValue, yValue, current.Median.X, yValue) <
                    results.CurrentFurthestDist)
                    {
                        KNearestHelper(results,
                            xValue, yValue,
                            k, current.RightChild);
                    }
                    if (results.BreakOut)
                    {
                        return;
                    }
                }
                else
                {
                    KNearestHelper(results,
                        xValue, yValue,
                        k, current.RightChild);
                    if (results.BreakOut)
                    {
                        return;
                    }

                    if (Dist(xValue, yValue, current.Median.X, yValue) <
                    results.CurrentFurthestDist)
                    {
                        KNearestHelper(results,
                            xValue, yValue,
                            k, current.LeftChild);
                    }
                    if (results.BreakOut)
                    {
                        return;
                    }
                }
            }
            else
            {
                if (yValue <= current.Median.Y)
                {
                    KNearestHelper(results,
                        xValue, yValue,
                        k, current.LeftChild);
                    if (results.BreakOut)
                    {
                        return;
                    }

                    if (Dist(xValue, yValue, xValue, current.Median.Y) <
                    results.CurrentFurthestDist)
                    {
                        KNearestHelper(results,
                            xValue, yValue,
                            k, current.RightChild);
                    }
                    if (results.BreakOut)
                    {
                        return;
                    }
                }
                else
                {
                    KNearestHelper(results,
                        xValue, yValue,
                        k, current.RightChild);
                    if (results.BreakOut)
                    {
                        return;
                    }

                    if (Dist(xValue, yValue, xValue, current.Median.Y) <
                    results.CurrentFurthestDist)
                    {
                        KNearestHelper(results,
                            xValue, yValue,
                            k, current.LeftChild);
                    }
                    if (results.BreakOut)
                    {
                        return;
                    }
                }
            }
        }

        private void AddNearest(
            KNearestResults results,
            double xValue,
            double yValue,
            KDTreeNode2D current,
            PointData pointData,
            bool isMedian,
            int index,
            int k)
        {
            if (results.BreakOut)
            {
                return;
            }
            if (results.ConsideredCount > results.ConsideredCutoff)
            {
                results.BreakOut = true;
                return;
            }

            if (results.Results.Count < k)
            {
                if (double.IsNaN(results.CurrentNearestDist))
                {
                    results.CurrentNearestDist =
                        Dist(xValue, yValue, pointData.X, pointData.Y);
                    results.CurrentFurthestDist = results.CurrentNearestDist;
                    results.CurrentFurthestIndex = 0;
                }

                results.Results.Add(
                    new KNearestResult()
                    {
                        IsMedian = isMedian,
                        Index = index,
                        Node = current,
                        X = pointData.X,
                        Y = pointData.Y
                    });
                results.ConsideredCount++;

                var dist = Dist(xValue, yValue, pointData.X, pointData.Y);
                if (dist <
                    results.CurrentNearestDist)
                {
                    results.CurrentNearestDist = dist;
                }
                if (dist > results.CurrentFurthestDist)
                {
                    results.CurrentFurthestDist = dist;
                    results.CurrentFurthestIndex = results.Results.Count - 1;
                }

                return;
            }

            var newDist = 0;
            if (newDist <
                results.CurrentFurthestDist)
            {
                if (newDist < results.CurrentNearestDist)
                {
                    results.CurrentNearestDist = newDist;
                }
                results.Results[results.CurrentFurthestIndex] = new KNearestResult()
                {
                    IsMedian = isMedian,
                    Index = index,
                    Node = current,
                    X = pointData.X,
                    Y = pointData.Y
                };

                double maxDist = 0;
                double maxIndex = 0;
                for (var i = 0; i < results.Results.Count; i++)
                {
                    var currDist = Dist(xValue, yValue, results.Results[i].X, results.Results[i].Y);
                    if (currDist > maxDist)
                    {
                        maxDist = currDist;
                        maxIndex = i;
                    }
                }

                results.ConsideredCount++;
            }
        }

        private double Dist(double xValue, double yValue, double xValue2, double yValue2)
        {
            return (xValue - xValue2) * (xValue - xValue2) + (yValue - yValue2) * (yValue - yValue2);
        }

        public void GetVisible(
            IList<KDTreeNode2D> nodes,
            SearchArgs args,
            double xMinimum,
            double xMaximum,
            double yMinimum,
            double yMaximum)
        {
            lock (_lock)
            {
                GetVisibleHelper(
                    nodes,
                    Root,
                    args,
                    xMinimum,
                    xMaximum,
                    yMinimum,
                    yMaximum,
                    false,
                    0);
            }
        }

        private void GetVisibleHelper(
            IList<KDTreeNode2D> nodes,
            KDTreeNode2D currentNode,
            SearchArgs args,
            double currentXMinimum,
            double currentXMaximum,
            double currentYMinimum,
            double currentYMaximum,
            bool report,
            int depth)
        {
            if (currentNode == null)
            {
                return;
            }

            if (depth > args.MaxRenderDepth ||
                ((currentYMaximum - currentYMinimum) < args.PixelSizeY &&
                (currentXMaximum - currentXMinimum) < args.PixelSizeX))
            {
                currentNode.SearchData =
                    new SearchData()
                    {
                        IsCutoff = true,
                        MinX = currentXMinimum,
                        MaxX = currentXMaximum,
                        MinY = currentYMinimum,
                        MaxY = currentYMaximum
                    };
                nodes.Add(currentNode);
                return;
            }

            if (currentNode.LeftChild == null &&
                currentNode.RightChild == null)
            {
                nodes.Add(currentNode);
                return;
            }

            double leftXMinimum;
            double leftXMaximum;
            double leftYMinimum;
            double leftYMaximum;
            double rightXMinimum;
            double rightXMaximum;
            double rightYMinimum;
            double rightYMaximum;

            if (currentNode.IsX)
            {
                leftXMinimum = currentXMinimum;
                leftXMaximum = currentNode.Median.X;
                leftYMinimum = currentYMinimum;
                leftYMaximum = currentYMaximum;

                rightXMinimum = currentNode.Median.X;
                rightXMaximum = currentXMaximum;
                rightYMinimum = currentYMinimum;
                rightYMaximum = currentYMaximum;
            }
            else
            {
                leftXMinimum = currentXMinimum;
                leftXMaximum = currentXMaximum;
                leftYMinimum = currentYMinimum;
                leftYMaximum = currentNode.Median.Y;

                rightXMinimum = currentXMinimum;
                rightXMaximum = currentXMaximum;
                rightYMinimum = currentNode.Median.Y;
                rightYMaximum = currentYMaximum;
            }

            if (report)
            {
                nodes.Add(currentNode);
                GetVisibleHelper(nodes,
                    currentNode.LeftChild,
                    args,
                    leftXMinimum,
                    leftXMaximum,
                    leftYMinimum,
                    leftYMaximum,
                    true,
                    depth + 1);
                GetVisibleHelper(nodes,
                    currentNode.RightChild,
                    args,
                    rightXMinimum,
                    rightXMaximum,
                    rightYMinimum,
                    rightYMaximum,
                    true,
                    depth + 1);
            }
            else
            {
                if (leftXMinimum >= args.MinX &&
                    leftXMaximum <= args.MaxX &&
                    leftYMinimum >= args.MinY &&
                    leftYMaximum <= args.MaxY)
                {
                    GetVisibleHelper(
                        nodes,
                        currentNode.LeftChild,
                        args,
                        leftXMinimum,
                        leftXMaximum,
                        leftYMinimum,
                        leftYMaximum,
                        true,
                        depth + 1);
                }
                else if (!(args.MinX > leftXMaximum ||
                    args.MaxX < leftXMinimum ||
                    args.MinY > leftYMaximum ||
                    args.MaxY < leftYMinimum))
                {
                    nodes.Add(currentNode);
                    GetVisibleHelper(
                        nodes,
                        currentNode.LeftChild,
                        args,
                        leftXMinimum,
                        leftXMaximum,
                        leftYMinimum,
                        leftYMaximum,
                        false,
                        depth + 1);
                }

                if (rightXMinimum >= args.MinX &&
                    rightXMaximum <= args.MaxX &&
                    rightYMinimum >= args.MinY &&
                    rightYMaximum <= args.MaxY)
                {
                    GetVisibleHelper(
                        nodes,
                        currentNode.RightChild,
                        args,
                        rightXMinimum,
                        rightXMaximum,
                        rightYMinimum,
                        rightYMaximum,
                        true,
                        depth + 1);
                }
                else if (!(args.MinX > rightXMaximum ||
                    args.MaxX < rightXMinimum ||
                    args.MinY > rightYMaximum ||
                    args.MaxY < rightYMinimum))
                {
                    nodes.Add(currentNode);
                    GetVisibleHelper(
                        nodes,
                        currentNode.RightChild,
                        args,
                        rightXMinimum,
                        rightXMaximum,
                        rightYMinimum,
                        rightYMaximum,
                        false,
                        depth + 1);
                }
            }
        }


        protected KDTreeNode2D Root { get; set; }
    }

    internal class KNearestResults
    {
        public int ConsideredCount;
        public int ConsideredCutoff;
        public List<KNearestResult> Results;
        public bool BreakOut;
        public double CurrentNearestDist;
        public double CurrentFurthestDist;
        public int CurrentFurthestIndex;
    }

    internal class KNearestResult
    {
        public int Index;
        public bool IsMedian;
        public double X;
        public double Y;
        public KDTreeNode2D Node;
    }

    internal class KDTreeThunk
    {
        public int StartIndex;
        public int EndIndex;
        public int Level;
        public int MaxLeafSize;
        public KDTreeNode2D Node;
    }

    internal class SearchArgs
    {
        public double MinX;
        public double MinY;
        public double MaxX;
        public double MaxY;
        public double PixelSizeX;
        public double PixelSizeY;
        public int MaxRenderDepth;
    }

    internal class KDTreeNode2D
    {
        public bool Unfinished;
        public bool IsX;
        public int DescendantCount;
        public PointData Median;
        public KDTreeNode2D LeftChild;
        public KDTreeNode2D RightChild;
        public PointData[] OtherPoints;

        public SearchData SearchData;
    }

    internal class SearchData
    {
        public bool IsCutoff;
        public double MinX;
        public double MaxX;
        public double MinY;
        public double MaxY;
    }

    internal class PointData
    {
        public double X;
        public double Y;
        public int Index;
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved