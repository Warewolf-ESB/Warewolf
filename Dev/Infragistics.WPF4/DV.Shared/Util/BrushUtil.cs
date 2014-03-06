using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// Utility class for brush-based operations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class BrushUtil
    {
        /// <summary>
        /// Creates a duplicate of the current brush
        /// </summary>
        /// <param name="brush"></param>
        /// <returns>New brush</returns>
        /// <remarks>
        /// This function duplicates the functionality of the missing Clone method.
        /// </remarks>
        public static Brush Duplicate(this Brush brush)
        {
            SolidColorBrush scb = brush as SolidColorBrush; if (scb != null) { return scb.Duplicate(); }
            LinearGradientBrush lgb = brush as LinearGradientBrush; if (lgb != null) { return lgb.Duplicate(); }
            RadialGradientBrush rgb = brush as RadialGradientBrush; if (rgb != null) { return rgb.Duplicate(); }
            ImageBrush ib = brush as ImageBrush; if (ib != null) { return ib.Duplicate(); }





            return null;
        }

        private static GradientStopCollection GradientStops(Color min, double p, GradientStopCollection max, InterpolationMode interpolationMode)
        {
            GradientStopCollection gradientStopCollection = new GradientStopCollection();

            for (int i = 0; i < max.Count; ++i)
            {
                gradientStopCollection.Add(new GradientStop()
                {
                    Offset = max[i].Offset,
                    Color = min.GetInterpolation(p, max[i].Color, interpolationMode),
                });
            }

            return gradientStopCollection;
        }

        private static GradientStopCollection GradientStops(GradientStopCollection min, double p, GradientStopCollection max, InterpolationMode interpolationMode)
        {
            GradientStopCollection gradientStopCollection = new GradientStopCollection();

            int i = 0;

            for (i = 0; i < System.Math.Min(min.Count, max.Count); ++i)
            {
                gradientStopCollection.Add(new GradientStop()
                {
                    Offset = (1.0 - p) * min[i].Offset + p * max[i].Offset,
                    Color = min[i].Color.GetInterpolation(p, max[i].Color, interpolationMode),
                });
            }

            for (; i < min.Count; ++i)
            {
                gradientStopCollection.Add(new GradientStop()
                {
                    Offset = (1.0 - p) * min[i].Offset + p * max[max.Count - 1].Offset,
                    Color = min[i].Color.GetInterpolation(p, max[max.Count - 1].Color, interpolationMode),
                });
            }

            for (; i < max.Count; ++i)
            {
                gradientStopCollection.Add(new GradientStop()
                {
                    Offset = (1.0 - p) * min[min.Count - 1].Offset + p * max[i].Offset,
                    Color = min[min.Count - 1].Color.GetInterpolation(p, max[i].Color, interpolationMode),
                });
            }

            return gradientStopCollection;
        }

        private static Brush SolidSolid(SolidColorBrush min, double p, SolidColorBrush max, InterpolationMode interpolationMode)
        {
            return new SolidColorBrush()
            {
                Color = min.Color.GetInterpolation(p, max.Color, interpolationMode),
                Opacity = (1.0 - p) * min.Opacity + p * max.Opacity,
                // RelativeTransform,
                // Transform
            };
        }

        private static Brush LinearLinear(LinearGradientBrush min, double p, LinearGradientBrush max, InterpolationMode interpolationMode)
        {
            return new LinearGradientBrush()
            {
                ColorInterpolationMode = max.ColorInterpolationMode,
                EndPoint = new Point((1.0 - p) * min.EndPoint.X + p * max.EndPoint.X, (1.0 - p) * min.EndPoint.Y + p * max.EndPoint.Y),
                GradientStops = GradientStops(min.GradientStops, p, max.GradientStops, interpolationMode),
                MappingMode = max.MappingMode,
                Opacity = (1.0 - p) * min.Opacity + p * max.Opacity,
                // RelativeTransform,
                SpreadMethod = max.SpreadMethod,
                StartPoint = new Point(min.StartPoint.X + p * (max.StartPoint.X - min.StartPoint.X), min.StartPoint.Y + p * (max.StartPoint.Y - min.StartPoint.Y)),
                // Transform
            };
        }

        private static Brush RadialRadial(RadialGradientBrush min, double p, RadialGradientBrush max, InterpolationMode interpolationMode)
        {
            return new RadialGradientBrush()
            {
                Center = new Point((1.0 - p) * min.Center.X + p * max.Center.X, (1.0 - p) * min.Center.Y + p * max.Center.Y),
                ColorInterpolationMode = max.ColorInterpolationMode,
                GradientOrigin = max.GradientOrigin,
                GradientStops = GradientStops(min.GradientStops, p, max.GradientStops, interpolationMode),
                MappingMode = max.MappingMode,
                Opacity = (1.0 - p) * min.Opacity + p * max.Opacity,
                RadiusX = (1.0 - p) * min.RadiusX + p * max.RadiusX,
                RadiusY = (1.0 - p) * min.RadiusY + p * max.RadiusY,
                RelativeTransform = max.RelativeTransform.Duplicate(),
                SpreadMethod = max.SpreadMethod,
                Transform = max.RelativeTransform.Duplicate()
            };
        }

        private static Brush SolidLinear(SolidColorBrush min, double p, LinearGradientBrush max, InterpolationMode interpolationMode)
        {
            return new LinearGradientBrush()
            {
                ColorInterpolationMode = max.ColorInterpolationMode,
                EndPoint = max.EndPoint,
                GradientStops = GradientStops(min.Color, p, max.GradientStops, interpolationMode),
                MappingMode = max.MappingMode,
                Opacity = (1.0 - p) * min.Opacity + p * max.Opacity,
                RelativeTransform = max.RelativeTransform.Duplicate(),
                SpreadMethod = max.SpreadMethod,
                StartPoint = max.StartPoint,
                Transform = max.Transform.Duplicate()
            };
        }

        private static Brush SolidRadial(SolidColorBrush min, double p, RadialGradientBrush max, InterpolationMode interpolationMode)
        {
            return new RadialGradientBrush()
            {
                Center = max.Center,
                ColorInterpolationMode = max.ColorInterpolationMode,
                GradientOrigin = max.GradientOrigin,
                GradientStops = GradientStops(min.Color, p, max.GradientStops, interpolationMode),
                MappingMode = max.MappingMode,
                Opacity = (1.0 - p) * min.Opacity + p * max.Opacity,
                RadiusX = max.RadiusX,
                RadiusY = max.RadiusY,
                RelativeTransform = max.RelativeTransform.Duplicate(),
                SpreadMethod = max.SpreadMethod,
                Transform = max.Transform.Duplicate()
            };
        }

        /// <summary>
        /// Creates a new brush as a linear interpolation according to the
        /// interpolation parameter between the two extremes.
        /// </summary>
        /// <param name="minimum">Lower extreme</param>
        /// <param name="interpolation">Interpolation parameter (internally clamped to the range 0.0 to 1.0)</param>
        /// <param name="maximum">Upper extreme</param>
        /// <param name="interpolationMode">Interpolation mode to use.</param>
        /// <returns>New brush.</returns>
        /// <remarks>
        /// When used as an extension method, the current brush object
        /// defines the lower extreme.
        /// </remarks>
        public static Brush GetInterpolation(this Brush minimum, double interpolation, Brush maximum, InterpolationMode interpolationMode)
        {
            SolidColorBrush minSolid = minimum as SolidColorBrush;

            SolidColorBrush maxSolid = maximum as SolidColorBrush;
            LinearGradientBrush maxLinear = maximum as LinearGradientBrush;
            RadialGradientBrush maxRadial = maximum as RadialGradientBrush;

            if (minSolid != null && maxSolid != null) { return SolidSolid(minSolid, interpolation, maxSolid, interpolationMode); }
            if (minSolid != null && maxLinear != null) { return SolidLinear(minSolid, interpolation, maxLinear, interpolationMode); }
            if (minSolid != null && maxRadial != null) { return SolidRadial(minSolid, interpolation, maxRadial, interpolationMode); }

            LinearGradientBrush minLinear = minimum as LinearGradientBrush;

            if (minLinear != null && maxSolid != null) { return SolidLinear(maxSolid, 1.0 - interpolation, minLinear, interpolationMode); }
            if (minLinear != null && maxLinear != null) { return LinearLinear(minLinear, interpolation, maxLinear, interpolationMode); }

            RadialGradientBrush minRadial = minimum as RadialGradientBrush;

            if (minRadial != null && maxSolid != null) { return SolidRadial(maxSolid, 1.0 - interpolation, minRadial, interpolationMode); }
            if (minRadial != null && maxRadial != null) { return RadialRadial(minRadial, interpolation, maxRadial, interpolationMode); }

            if (minimum != null)
            {
                Brush ret = minimum.Duplicate();

                ret.Opacity = (1.0 - interpolation) * minimum.Opacity + interpolation * (maximum != null ? maximum.Opacity : 0.0);

                return ret;
            }

            if (maximum != null)
            {
                Brush ret = maximum.Duplicate();

                ret.Opacity = (1.0 - interpolation) * (minimum != null ? minimum.Opacity : 0.0) + interpolation * minimum.Opacity;

                return ret;
            }

            return minimum; // give up
        }

        /// <summary>
        /// Creates a new brush, based on the reference brush and linearly lightened according
        /// to the lightening parameter.
        /// </summary>
        /// <param name="brush">Reference brush</param>
        /// <param name="interpolation">Lightening parameter (internally clamped to the range -1.0 to 1.0)</param>
        /// <returns>New brush.</returns>
        /// <remarks>
        /// When used as an extension method, the current brush object
        /// is defines the lower extreme.
        /// </remarks>
        public static Brush GetLightened(this Brush brush, double interpolation)
        {
            interpolation = MathUtil.Clamp(interpolation, -1.0, 1.0);

            SolidColorBrush solidColorBrush = brush as SolidColorBrush;

            if (solidColorBrush != null)
            {
                return solidColorBrush.GetLightened(interpolation);
            }

            LinearGradientBrush linearGradientBrush = brush as LinearGradientBrush;

            if (linearGradientBrush != null)
            {
                return linearGradientBrush.GetLightened(interpolation);
            }

            RadialGradientBrush radialGradientBrush = brush as RadialGradientBrush;

            if (radialGradientBrush != null)
            {
                return radialGradientBrush.GetLightened(interpolation);
            }

            ImageBrush imageBrush = brush as ImageBrush;

            if (imageBrush != null)
            {
                return imageBrush.GetLightened(interpolation);
            }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


            return null;
        }

        /// <summary>
        /// Creates a duplicate of the current SolidColorBrush.
        /// </summary>
        /// <param name="brush"></param>
        /// <returns>New SolidColorBrush</returns>
        /// <remarks>
        /// This function duplicates the functionality of the missing Clone method.
        /// </remarks>
        public static SolidColorBrush Duplicate(this SolidColorBrush brush)
        {
            SolidColorBrush dup = new SolidColorBrush()
            {
                Color = brush.Color,
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate()
            };

            return dup;
        }

        /// <summary>
        /// Creates a new brush as an interpolation between the current brush and the
        /// specified brush using the interpolation parameter.
        /// </summary>
        /// <param name="minimum">The minimum brush.</param>
        /// <param name="interpolation">The interpolation parameter.</param>
        /// <param name="maximum">The maximum brush</param>
        /// <param name="interpolationMode">The interpolation mode to use.</param>
        /// <returns>New interpolated Brush</returns>
        public static SolidColorBrush GetInterpolation(this SolidColorBrush minimum, double interpolation, SolidColorBrush maximum, InterpolationMode interpolationMode)
        {
            interpolation = MathUtil.Clamp(interpolation, 0.0, 1.0);

            SolidColorBrush dup = minimum.Duplicate();

            if (maximum != null)
            {
                dup.Color = minimum.Color.GetInterpolation(interpolation, maximum.Color, interpolationMode);
                dup.Opacity = minimum.Opacity + interpolation * (maximum.Opacity - minimum.Opacity);
            }

            return dup;
        }

        /// <summary>
        /// Creates a new brush, based on the reference brush and linearly lightened according
        /// to the lightening parameter.
        /// </summary>
        /// <param name="brush">Reference brush</param>
        /// <param name="interpolation">Lightening parameter.</param>
        /// <returns>New brush.</returns>
        /// <remarks>
        /// When used as an extension method, the current brush object
        /// is defines the lower extreme.
        /// </remarks>
        public static SolidColorBrush GetLightened(this SolidColorBrush brush, double interpolation)
        {
            SolidColorBrush dup = new SolidColorBrush()
            {
                Color = brush.Color.GetLightened(interpolation),
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate()
            };

            return dup;
        }

        /// <summary>
        /// Creates a duplicate of the current LinearGradientBrush.
        /// </summary>
        /// <param name="brush"></param>
        /// <returns>New LinearGradientBrush</returns>
        /// <remarks>
        /// This function duplicates the functionality of the missing Clone method.
        /// </remarks>
        public static LinearGradientBrush Duplicate(this LinearGradientBrush brush)
        {
            LinearGradientBrush dup = new LinearGradientBrush()
            {
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate(),
                ColorInterpolationMode = brush.ColorInterpolationMode,
                EndPoint = brush.EndPoint,
                MappingMode = brush.MappingMode,
                SpreadMethod = brush.SpreadMethod,
                StartPoint = brush.StartPoint
            };

            foreach (GradientStop gradientStop in brush.GradientStops)
            {
                dup.GradientStops.Add(gradientStop.Duplicate());
            }

            return dup;
        }

        /// <summary>
        /// Creates a new brush as an interpolation between the current brush and the
        /// specified brush using the interpolation parameter.
        /// </summary>
        /// <param name="minimum">The minimum brush.</param>
        /// <param name="interpolation">The interpolation parameter.</param>
        /// <param name="maximum">The maximum brush</param>
        /// <param name="interpolationMode">Interpolation mode to use.</param>
        /// <returns>New interpolated Brush</returns>
        public static LinearGradientBrush GetInterpolation(this LinearGradientBrush minimum, double interpolation, LinearGradientBrush maximum, InterpolationMode interpolationMode)
        {
            interpolation = MathUtil.Clamp(interpolation, 0.0, 1.0);

            LinearGradientBrush dup = new LinearGradientBrush()
            {
                // Opacity = min.Opacity + p * (max.Opacity - min.Opacity),
                RelativeTransform = minimum.RelativeTransform.Duplicate(),
                Transform = minimum.Transform.Duplicate(),
                ColorInterpolationMode = minimum.ColorInterpolationMode,
                // EndPoint = new Point(min.EndPoint.X + p * (max.EndPoint.X - min.EndPoint.X), min.EndPoint.Y + p * (max.EndPoint.Y - min.EndPoint.Y)),
                MappingMode = minimum.MappingMode,
                SpreadMethod = minimum.SpreadMethod,
                //StartPoint = new Point(min.StartPoint.X + p * (max.StartPoint.X - min.StartPoint.X), min.StartPoint.Y + p * (max.StartPoint.Y - min.StartPoint.Y)),
            };

            if (maximum != null && minimum.GradientStops.Count == maximum.GradientStops.Count)
            {
                dup.Opacity = minimum.Opacity + interpolation * (maximum.Opacity - minimum.Opacity);
                dup.EndPoint = new Point(minimum.EndPoint.X + interpolation * (maximum.EndPoint.X - minimum.EndPoint.X), minimum.EndPoint.Y + interpolation * (maximum.EndPoint.Y - minimum.EndPoint.Y));
                dup.StartPoint = new Point(minimum.StartPoint.X + interpolation * (maximum.StartPoint.X - minimum.StartPoint.X), minimum.StartPoint.Y + interpolation * (maximum.StartPoint.Y - minimum.StartPoint.Y));

                for (int i = 0; i < System.Math.Min(minimum.GradientStops.Count, maximum.GradientStops.Count); ++i)
                {
                    dup.GradientStops.Add(minimum.GradientStops[i].GetInterpolation(interpolation, maximum.GradientStops[i], interpolationMode));
                }
            }
            else
            {
                Debug.WriteLine("Incompatible brushes in LinearGradientBrush.Interpolate()");
            }

            return dup;
        }

        /// <summary>
        /// Creates a new brush, based on the reference brush and linearly lightened according
        /// to the lightening parameter.
        /// </summary>
        /// <param name="brush">Reference brush.</param>
        /// <param name="interpolation">Lightening parameter.</param>
        /// <returns>New brush.</returns>
        public static LinearGradientBrush GetLightened(this LinearGradientBrush brush, double interpolation)
        {
            LinearGradientBrush dup = new LinearGradientBrush()
            {
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate(),
                ColorInterpolationMode = brush.ColorInterpolationMode,
                EndPoint = brush.EndPoint,
                MappingMode = brush.MappingMode,
                SpreadMethod = brush.SpreadMethod,
                StartPoint = brush.StartPoint
            };

            foreach (GradientStop gradientStop in brush.GradientStops)
            {
                dup.GradientStops.Add(gradientStop.GetLightened(interpolation));
            }

            return dup;
        }

        /// <summary>
        /// Creates a duplicate of the current RadialGradientBrush.
        /// </summary>
        /// <param name="brush"></param>
        /// <returns>New RadialGradientBrush</returns>
        /// <remarks>
        /// This function duplicates the functionality of the missing Clone method.
        /// </remarks>
        public static RadialGradientBrush Duplicate(this RadialGradientBrush brush)
        {
            RadialGradientBrush dup = new RadialGradientBrush()
            {
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate(),
                Center = brush.Center,
                ColorInterpolationMode = brush.ColorInterpolationMode,
                GradientOrigin = brush.GradientOrigin,
                MappingMode = brush.MappingMode,
                RadiusX = brush.RadiusX,
                RadiusY = brush.RadiusY,
                SpreadMethod = brush.SpreadMethod
            };

            foreach (GradientStop gradientStop in brush.GradientStops)
            {
                dup.GradientStops.Add(gradientStop.Duplicate());
            }

            return dup;
        }

        /// <summary>
        /// Creates a new brush as an interpolation between the current brush and the
        /// specified brush using the interpolation parameter.
        /// </summary>
        /// <param name="minimum">The minimum brush.</param>
        /// <param name="interpolation">The interpolation parameter.</param>
        /// <param name="maximum">The maximum brush</param>
        /// <param name="interpolationMode">Interpolation mode to use.</param>
        /// <returns>New interpolated Brush</returns>
        public static RadialGradientBrush GetInterpolation(this RadialGradientBrush minimum, double interpolation, RadialGradientBrush maximum, InterpolationMode interpolationMode)
        {
            interpolation = MathUtil.Clamp(interpolation, 0.0, 1.0);

            RadialGradientBrush dup = minimum.Duplicate();

            if (maximum != null && minimum.GradientStops.Count == maximum.GradientStops.Count)
            {
                dup.Opacity = minimum.Opacity + interpolation * (maximum.Opacity - minimum.Opacity);
                dup.Center = new Point(minimum.Center.X + interpolation * (maximum.Center.X - minimum.Center.X), minimum.Center.Y + interpolation * (maximum.Center.Y - minimum.Center.Y));
                dup.RadiusX = minimum.RadiusX + interpolation * (maximum.RadiusX - minimum.RadiusX);
                dup.RadiusY = minimum.RadiusY + interpolation * (maximum.RadiusY - minimum.RadiusY);

                for (int i = 0; i < dup.GradientStops.Count; ++i)
                {
                    dup.GradientStops[i].Color = minimum.GradientStops[i].Color.GetInterpolation(interpolation, maximum.GradientStops[i].Color, interpolationMode);
                    dup.GradientStops[i].Offset = minimum.GradientStops[i].Offset + interpolation * (maximum.GradientStops[i].Offset - minimum.GradientStops[i].Offset);
                }
            }
            else
            {
                Debug.WriteLine("Incompatible brushes in RadialGradientBrush.Interpolate()");
            }

            return dup;
        }

        /// <summary>
        /// Creates a new brush, based on the reference brush and linearly lightened according
        /// to the lightening parameter.
        /// </summary>
        /// <param name="brush">Reference brush.</param>
        /// <param name="interpolation">Lightening parameter.</param>
        /// <returns>New brush.</returns>
        public static RadialGradientBrush GetLightened(this RadialGradientBrush brush, double interpolation)
        {
            RadialGradientBrush dup = new RadialGradientBrush()
            {
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate(),
                Center = brush.Center,
                ColorInterpolationMode = brush.ColorInterpolationMode,
                GradientOrigin = brush.GradientOrigin,
                MappingMode = brush.MappingMode,
                RadiusX = brush.RadiusX,
                RadiusY = brush.RadiusY,
                SpreadMethod = brush.SpreadMethod
            };

            foreach (GradientStop gradientStop in brush.GradientStops)
            {
                dup.GradientStops.Add(gradientStop.GetLightened(interpolation));
            }

            return dup;
        }

        /// <summary>
        /// Creates a duplicate of the current ImageBrush.
        /// </summary>
        /// <param name="brush"></param>
        /// <returns>New ImageBrush</returns>
        /// <remarks>
        /// This function duplicates the functionality of the missing Clone method.
        /// </remarks>
        public static ImageBrush Duplicate(this ImageBrush brush)
        {
            ImageBrush dup = new ImageBrush()
            {
                Opacity = brush.Opacity,
                RelativeTransform = brush.RelativeTransform.Duplicate(),
                Transform = brush.Transform.Duplicate(),
                AlignmentX = brush.AlignmentX,
                AlignmentY = brush.AlignmentY,
                ImageSource = brush.ImageSource,
                Stretch = brush.Stretch
            };

            return dup;
        }

        /// <summary>
        /// Creates a new brush as an interpolation between the current brush and the
        /// specified brush using the interpolation parameter.
        /// </summary>
        /// <param name="minimum">The minimum brush.</param>
        /// <param name="interpolation">The interpolation parameter.</param>
        /// <param name="maximum">The maximum brush</param>
        /// <param name="interpolationMode">Interpolation mode.</param>
        /// <returns>New interpolated Brush</returns>
        [SuppressMessage("Microsoft.Design", "CA1011", Justification = "Symmetry, even if not required.")]
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Symmetry, even if not required.")]
        public static ImageBrush GetInterpolation(this ImageBrush minimum, double interpolation, ImageBrush maximum, InterpolationMode interpolationMode)
        {
            interpolation = MathUtil.Clamp(interpolation, 0.0, 1.0);

            ImageBrush dup = minimum.Duplicate();

            if (maximum != null)
            {
                // nothing much to interpolate with image brushes

                dup.Opacity = minimum.Opacity + interpolation * (maximum.Opacity - minimum.Opacity);
            }
            else
            {
                Debug.WriteLine("Incompatible brushes in ImageBrush.Interpolate()");
            }

            return dup;
        }

        /// <summary>
        /// Creates a new brush, based on the reference brush and linearly lightened according
        /// to the lightening parameter.
        /// </summary>
        /// <param name="brush">Reference Brush.</param>
        /// <param name="interpolation">Lightening parameter.</param>
        /// <returns>New brush.</returns>
        /// <remarks>
        /// This method is not implemented for ImageBrush, and thus an unlightened duplicate of the given brush will be returned.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "Symmetry, even if not required.")]
        public static ImageBrush GetLightened(this ImageBrush brush, double interpolation)
        {
            ImageBrush dup = brush.Duplicate();

            if (false)
            {
                // lighten that
            }

            return dup;
        }



#region Infragistics Source Cleanup (Region)








































































#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Creates a duplicate of the current GradientStop.
        /// </summary>
        /// <returns>New GradientStop</returns>
        /// <remarks>
        /// This function duplicates the functionality of the missing Clone method.
        /// </remarks>
        public static GradientStop Duplicate(this GradientStop gradientStop)
        {
            GradientStop dup = new GradientStop()
            {
                Color = gradientStop.Color,
                Offset = gradientStop.Offset
            };

            return dup;
        }

        /// <summary>
        /// Creates a new GradientStop as a linear interpolation according to the
        /// interpolation parameter between the two extremes.
        /// </summary>
        /// <param name="minimum">Lower extreme</param>
        /// <param name="interpolation">Interpolation parameter (internally clamped to the range 0.0 to 1.0)</param>
        /// <param name="maximum">Upper extreme</param>
        /// <param name="interpolationMode">Interpolation mode to use.</param>
        /// <returns>New brush.</returns>
        /// <remarks>
        /// When used as an extension method, the current GradientStop object
        /// defines the lower extreme.
        /// </remarks>
        public static GradientStop GetInterpolation(this GradientStop minimum, double interpolation, GradientStop maximum, InterpolationMode interpolationMode)
        {
            interpolation = MathUtil.Clamp(interpolation, 0.0, 1.0);

            GradientStop dup = new GradientStop()
            {
                Color = minimum.Color.GetInterpolation(interpolation, maximum.Color, interpolationMode),
                Offset = minimum.Offset + interpolation * (maximum.Offset - minimum.Offset)
            };

            return dup;
        }

        /// <summary>
        /// Creates a new GradientStop, based on the reference GradientStop and linearly lightened according
        /// to the lightening parameter.
        /// </summary>
        /// <param name="gradientStop">Reference GradientStop.</param>
        /// <param name="interpolation">Lightening parameter.</param>
        /// <returns>New GradientStop.</returns>
        public static GradientStop GetLightened(this GradientStop gradientStop, double interpolation)
        {
            GradientStop dup = new GradientStop()
            {
                Color = gradientStop.Color.GetLightened(interpolation),
                Offset = gradientStop.Offset
            };

            return dup;
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