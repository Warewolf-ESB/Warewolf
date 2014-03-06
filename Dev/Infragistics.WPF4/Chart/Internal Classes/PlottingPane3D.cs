
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Drawing.Drawing2D;
using System.Windows.Data;
#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class PlottingPane3D : ChartCanvas
    {
        internal void ClearTooltip()
        {
            if (this.tooltip != null)
            {
                this.tooltip.Content = null;
            }
        }
        #region Fields

        // Fields
        private Chart _chart;
        private Scene3D _scene3D;
        private AxisValue _axisX;
        private AxisValue _axisY;
        private AxisValue _axisZ;
        internal ArrayList _chartSeriesArray = new ArrayList();
        private Viewport3D _viewport3D;
        private ToolTip tooltip;
        private ScenePane _scenePane;

        private double _viewportProportion;
        private bool _proportionInit = false;

        #endregion

        #region Properties

        internal Viewport3D Viewport3D
        {
            get { return _viewport3D; }
        }

        internal Chart Chart
        {
            get { return _chart; }
        }

        internal AxisValue AxisX
        {
            get
            {
                return _axisX;
            }
            set
            {
                _axisX = value;
            }
        }

        internal AxisValue AxisY
        {
            get
            {
                return _axisY;
            }
            set
            {
                _axisY = value;
            }
        }

        internal AxisValue AxisZ
        {
            get
            {
                return _axisZ;
            }
            set
            {
                _axisZ = value;
            }
        }

        #endregion

        #region Methods

        internal PlottingPane3D(Chart chart, ScenePane scenePane)
        {
            _scene3D = new Scene3D(chart, 0.02, 1, 1, 1);
            _chart = chart;
            _scenePane = scenePane;

            // init the tooltip
            this.tooltip = new ToolTip();
            this.tooltip.PlacementTarget = this;
            this.ToolTip = tooltip;
            this.tooltip.Visibility = Visibility.Hidden;
            this.tooltip.Opened += new RoutedEventHandler(tooltip_Opened);
        }

        void tooltip_Opened(object sender, RoutedEventArgs e)
        {
            if (!PlottingPane3D.IsToolTipsEnabled(this._chart))
            {
                // [DN 6/25/2008:BR34107] stay closed willya?!?!
                this.tooltip.IsOpen = false;
            }
        }

        internal void Update(string propertyName)
        {
            if (propertyName == "Background")
            {
                if (_scenePane != null)
                {
                    _scenePane.Draw();
                }
            }
        }


        private void GetPerspective(double perspective, out double fieldOfView, out double zPosition)
        {
            if (_chart.ChartCreator.IsSceneType(SceneType.Pie))
            {
                fieldOfView = 1 + 100 * perspective / 100.0;

                zPosition = 1.6 / Math.Tan(fieldOfView / 180.0 * Math.PI / 2.0);
            }
            else
            {
                fieldOfView = 1 + 100 * perspective / 100.0;

                zPosition = 0.9 / Math.Tan(fieldOfView / 180.0 * Math.PI / 2.0);
            }
        }

        /// <summary>
        /// Checks if any data point has tooltips enabled
        /// </summary>
        /// <param name="chart">The Chart</param>
        /// <returns>True if tooltips are enabled</returns>
        internal static bool IsToolTipsEnabled(Chart chart)
        {
            SeriesCollection seriesList = chart.GetContainer().Series;

            foreach (Series series in seriesList)
            {
                foreach (DataPoint point in series.DataPoints)
                {
                    if (point.ToolTip != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void Draw()
        {
            // Declare scene objects.
            this.Children.Clear();

            _viewport3D = new Viewport3D();
            
            Model3DGroup model3DGroup = new Model3DGroup();
            ModelVisual3D modelVisual3D = new ModelVisual3D();

            Camera camera;
            bool oblique = false;

            if (!oblique)
            {
                PerspectiveCamera perspectiveCamera = new PerspectiveCamera();

                // Perspective
                double fieldOfView, zPosition;

                double perspective = 70;
                if (_chart.GetContainer().Scene != null)
                {
                    perspective = _chart.GetContainer().Scene.Perspective;
                }
                GetPerspective(perspective, out fieldOfView, out zPosition);

                // Specify where in the 3D scene the camera is.
                if (_chart.ChartCreator.IsSceneType(SceneType.Pie))
                {
                    perspectiveCamera.Position = new Point3D(0, 0, zPosition);
                }
                else
                {
                    perspectiveCamera.Position = new Point3D(0, -0.1, zPosition);
                }

                // Specify the direction that the camera is pointing.
                perspectiveCamera.LookDirection = new Vector3D(0, 0, -1);

                // Define camera's horizontal field of view in degrees.
                perspectiveCamera.FieldOfView = fieldOfView;

                camera = perspectiveCamera;
            }
            else
            {
                //Define matrices for ViewMatrix and ProjectionMatrix properties.
                Matrix3D vmatrix = new Matrix3D(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
                Matrix3D pmatrix = new Matrix3D(1, 0, 0, 0, 0, 1, 0, 0, -0.707106781, -0.707106781, 0, 0, 0, 0, 0, 1);

                MatrixCamera matrixCamera = new MatrixCamera();

                matrixCamera.ProjectionMatrix = pmatrix;

                camera = matrixCamera;
            }

            // Asign the camera to the viewport
            _viewport3D.Camera = camera;

            _chart.HitTestInfoArray.Clear();

            LightCollection lights = _chart.GetContainer().Lights;
            ModelVisual3D modelVisual3DLight = new ModelVisual3D();
            Model3DGroup model3DGroupLight = new Model3DGroup();

            if (lights.Count == 0)
            {
                // Define the lights cast in the scene. Without light, the 3D object cannot 
                // be seen. Also, the direction of the lights affect shadowing. Note: to 
                // illuminate an object from additional directions, create additional lights.
                AmbientLight ambient = new AmbientLight(Color.FromRgb(55, 55, 55));
                SpotLight spotLight = new SpotLight(Colors.White, new Point3D(0, 2, 5), new Vector3D(0, -2, -5), 40, 20);


                model3DGroupLight.Children.Add(ambient);

                model3DGroupLight.Children.Add(spotLight);
            }
            else
            {
                foreach (Light light in lights)
                {
                    model3DGroupLight.Children.Add(light);
                }
            }

            modelVisual3DLight.Content = model3DGroupLight;

            // Apply a transform to the object. In this sample, a rotation transform is applied,  
            // rendering the 3D object rotated.
            RotateTransform3D rotateTransform3D = new RotateTransform3D();

            if (!_chart.ChartCreator.IsSceneType(SceneType.Pie))
            {
                _scene3D.AxisX = _axisX;
                _scene3D.AxisY = _axisY;
                _scene3D.AxisZ = _axisZ;
                _scene3D.Chart = _chart;
                _scene3D.Draw(model3DGroup, rotateTransform3D);
            }

            foreach (ChartSeries chartSeries in _chartSeriesArray)
            {
                chartSeries.Draw(model3DGroup, rotateTransform3D);
            }

            model3DGroup.Transform = _chart.GetContainer().Transform3D;

            // Add the group of models to the ModelVisual3d.
            modelVisual3D.Content = model3DGroup;

            _viewport3D.Children.Add(modelVisual3D);
            _viewport3D.Children.Add(modelVisual3DLight);
            this.Children.Add(_viewport3D);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (!_proportionInit)
            {
                _viewportProportion = sizeInfo.NewSize.Width / sizeInfo.NewSize.Height;
                _proportionInit = true;   
            }

            if (sizeInfo.NewSize.Width / sizeInfo.NewSize.Height > _viewportProportion)
            {
                _viewport3D.Width = sizeInfo.NewSize.Height * _viewportProportion;
                _viewport3D.Height = sizeInfo.NewSize.Height;
                // [DN February 14, 2012 : 98890] adjust viewport position: if it's going to fill the entire canvas, then center it
                Canvas.SetLeft(_viewport3D, (sizeInfo.NewSize.Width - _viewport3D.Width) / 2.0);
                Canvas.SetTop(_viewport3D, (sizeInfo.NewSize.Height - _viewport3D.Height) / 2.0);
            }
            else
            {
                _viewport3D.Width = sizeInfo.NewSize.Width;
                _viewport3D.Height = sizeInfo.NewSize.Height;
                Canvas.SetLeft(_viewport3D, 0);
                Canvas.SetTop(_viewport3D, 0);
            }
            
            

            
            base.OnRenderSizeChanged(sizeInfo);
        }

        internal void AddChartSeries(ChartSeries chartSeries)
        {
            _chartSeriesArray.Add(chartSeries);
        }

        /// <summary>
        /// This method is used to create tooltips for 3D charts. Tooltips for 3D charts do not work 
        /// same as 2D charts. 2D charts are drawn using Framework elements and they have implemented 
        /// tooltips. 3D charts cannot use tooltips from Framework elements (model 3D is not Framework element). 
        /// This is implementation for 3D charts tooltips.
        /// </summary>
        /// <param name="e">Mouse event arguments</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!IsToolTipsEnabled(_chart))
            {
                return;
            }

            // Call Hit test method to check if a mouse is above a data point.
            HitTestArgs args = _chart.HitTest(e);

            // If the mouse is above a data point, show a tooltip.
            if (args.SelectedObject == null)
            {
                this.tooltip.Visibility = Visibility.Hidden;
                this.tooltip.IsOpen = false;
                return;
            }

            //  Get the tooltip from selected data point.
            object pointToolTip = _chart.GetContainer().Series[args.SeriesIndex].DataPoints[args.PointIndex].ToolTip;
            if (pointToolTip == null)
            {
                return;
            }

            // Get mouse position 
            Point position = e.GetPosition(this);

            ToolTip toolTipType = pointToolTip as ToolTip;
            if (toolTipType != null)
            {
                pointToolTip = toolTipType.Content;
            }

            this.tooltip.Content = pointToolTip;

            this.tooltip.Placement = PlacementMode.Bottom;
            this.tooltip.HorizontalOffset = 20;
            this.tooltip.PlacementRectangle = new Rect(position, new Size(0, 20));

            if (this.tooltip.IsOpen == false)
            {
                this.tooltip.IsOpen = true;
            }

            this.tooltip.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (this.tooltip.Visibility != Visibility.Hidden)
            {
                this.tooltip.IsOpen = false;
                this.tooltip.Visibility = Visibility.Hidden;
            }
        }

        #endregion Methods

    }

    /// <summary>
    /// Internal Hit test result class used for 3D Hit test.
    /// </summary>
    internal class HitTestInfo
    {
        #region Internal Fields

        internal GeometryModel3D GeometryModel3D;
        internal UIElement UIElement;
        internal GraphicsPath Path = null;
        internal int DataPointIndex = -1;
        internal int SeriesIndex = -1;

        #endregion Internal Fields
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