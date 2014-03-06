using System;



using System.Linq;

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
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    internal class AngleRadiusPair
    {
        public int Index { get; set; }
        public double Angle { get; set; }
        public double Radius { get; set; }
    }

    internal class PolarLinePlanner
    {
        public bool UseCartesianInterpolation { get; set; }
        public UnknownValuePlotting UnknownValuePlotting { get; set; }
        public Func<int, double> AngleProvider { get; set; }
        public Func<int, double> RadiusProvider { get; set; }
        public Func<int, double> TransformedXProvider { get; set; }
        public Func<int, double> TransformedYProvider { get; set; }
        public double Resolution { get; set; }
        public int Count { get; set; }
        private Rect _viewport;
        public Rect Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        private Rect _window;
        public Rect Window
        {
            get { return _window; }
            set { _window = value; }
        }

        public Clipper Clipper { get; set; }
        public bool ClippingDisabled { get; set; }
        public bool IsClosed { get; set; }
        private IList<Point> Target { get; set; }

        protected bool Valid()
        {
            if (AngleProvider == null ||
                RadiusProvider == null ||
                TransformedXProvider == null ||
                TransformedYProvider == null ||
                Viewport == Rect.Empty ||
                Window == Rect.Empty)
            {
                return false;
            }

            return true;
        }

        protected double Measure(Func<int, double> X, Func<int, double> Y, int a, int b)
        {
            double x = X(b) - X(a);
            double y = Y(b) - Y(a);

            return x * x + y * y;
        }

        internal IList<AngleRadiusPair> Reduce(IEnumerable<int> viableIndices)
        {
            List<AngleRadiusPair> list = new List<AngleRadiusPair>();

            double measure = Resolution * Resolution;

            IEnumerable<int> indices;
            if (viableIndices != null)
            {
                indices = viableIndices;
            }
            else
            {
                indices = Enumerable.Range(0, Count);
            }

            IEnumerator<int> indicesEnumerator = indices.GetEnumerator();
            bool notDone = true;
            notDone = indicesEnumerator.MoveNext();
            int i = indicesEnumerator.Current;

            while (notDone)
            {
                int j = i;
                notDone = indicesEnumerator.MoveNext();
                i = indicesEnumerator.Current;

                while (notDone && Measure(TransformedXProvider, TransformedYProvider, j, i) < measure)
                {
                    notDone = indicesEnumerator.MoveNext();
                    i = indicesEnumerator.Current;
                }

                AngleRadiusPair pair = new AngleRadiusPair();
                pair.Index = j;
                if (!UseCartesianInterpolation)
                {
                    pair.Angle = AngleProvider(j);
                    pair.Radius = RadiusProvider(j);
                }

                list.Add(pair);
            }

            return list;
        }

        private void PrepareCartesian(IEnumerable<int> viableIndices)
        {
            foreach (AngleRadiusPair pair in Reduce(viableIndices))
            {
                if (double.IsNaN(pair.Angle) ||
                    double.IsInfinity(pair.Angle) ||
                    double.IsNaN(pair.Radius) ||
                    double.IsInfinity(pair.Radius))
                {
                    AddPoint(new Point(double.NaN, double.NaN));
                    continue;
                }

                AddPoint(new Point(TransformedXProvider(pair.Index),
                    TransformedYProvider(pair.Index)));
            }
        }

        private double GetErrorTolerance()
        {
            return Math.Pow(Resolution /
                            (Math.Max(Viewport.Width / Window.Width,
                            Viewport.Height / Window.Height)), 2);
        }

        private void AddFromPolar(double angle, double radius)
        {
            double x = 0.5 + radius * Math.Cos(angle);
            double y = 0.5 + radius * Math.Sin(angle);

            x = _viewport.Left +
                _viewport.Width * (x - _window.Left) / _window.Width;
            y = _viewport.Top +
                _viewport.Height * (y - _window.Top) / _window.Height;

            AddPoint(new Point(x, y));
        }

        private void PreparePolar(IEnumerable<int> viableIndices)
        {
            double error = GetErrorTolerance();

            IList<AngleRadiusPair> pairs = Reduce(viableIndices);

            double a0 = pairs[0].Angle;
            double r0 = pairs[0].Radius;
            double i0 = pairs[0].Index;

            for (int i = 1; i < pairs.Count; i++)
            {
                double ai = pairs[i].Angle;
                double ri = pairs[i].Radius;
                double ii = pairs[i].Index;

                if (double.IsNaN(ai) ||
                    double.IsInfinity(ai) ||
                    double.IsNaN(ri) ||
                    double.IsInfinity(ri))
                {
                    AddPoint(new Point(double.NaN, double.NaN));
                    if (UnknownValuePlotting
                        != UnknownValuePlotting.LinearInterpolate)
                    {
                        i++;
                        if (i < pairs.Count)
                        {
                            a0 = pairs[i].Angle;
                            r0 = pairs[i].Radius;
                        }
                    }
                    continue;
                }

                CreateSpiralPoints(ai, ri, a0, r0, i, error, i0 > ii);

                a0 = ai;
                r0 = ri;
                i0 = ii;
            }
        }

        private void CreateSpiralPoints(double ai, double ri, double a0, double r0, int index, double error, bool wrapAround)
        {
            bool swapped = false;
            if ((ai < a0 && !wrapAround) || (ai > a0 && wrapAround))
            {
                swapped = true;
                double swap = ai;
                ai = a0;
                a0 = swap;

                swap = ri;
                ri = r0;
                r0 = swap;
            }

            IEnumerable<double> ps = Flattener.Spiral(a0, r0, ai, ri, error);
            if (swapped)
            {
                ps = ps.Reverse();
            }

            foreach (double p in ps)
            {
                double a = a0 + p * (ai - a0);
                double r = r0 + p * (ri - r0);

                AddFromPolar(a, r);
            }
        }


        private void EnsureClipper(IList<Point> target)
        {
            double top = Viewport.Top - 10;
            double bottom = Viewport.Bottom + 10;
            double left = Viewport.Left - 10;
            double right = Viewport.Right + 10;

            if (Clipper == null)
            {
                Clipper = new Clipper(left, bottom, right, top, IsClosed) { Target = target };
            }
        }

        private void AddPoint(Point point)
        {
            if (UnknownValuePlotting == Charts.UnknownValuePlotting.LinearInterpolate &&
                !point.IsPlottable())
            {
                return;
            }

            if (ClippingDisabled)
            {
                Target.Add(point);
            }
            else
            {
                Clipper.Add(point);
            }
        }

        public void PrepareLine(IEnumerable<int> viableIndices = null)
        {
            PrepareLine(null, null);
        }

        public void PrepareLine(IList<Point> target, IEnumerable<int> viableIndices = null)
        {
            Target = target;
            if (!Valid())
            {
                return;
            }

            if (Count > 1)
            {
                EnsureClipper(target);

                if (UseCartesianInterpolation)
                {
                    PrepareCartesian(viableIndices);
                }
                else
                {
                    PreparePolar(viableIndices);
                }

                Clipper.Target = null;
            }
        }
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