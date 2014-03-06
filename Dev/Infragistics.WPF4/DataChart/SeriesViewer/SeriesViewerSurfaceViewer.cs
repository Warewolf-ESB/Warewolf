
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
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{

    internal class SeriesViewerSurfaceViewer : DependencyObject,

        IOverviewPlusDetailControl, 

        INotifyPropertyChanged
    {
        public SeriesViewerSurfaceViewer(SeriesViewer model, SeriesViewerView view)
            : base()
        {
            if (model == null || view == null)
            {
                throw new ArgumentNullException("model");
            }
            this.Model = model;
            this.Model.WindowRectChanged += new RectChangedEventHandler(Model_WindowRectChanged);

            this.View = view;

            this.PreviewCanvas = new Canvas();

            this.UpdateZoomLevelDisplayText();
        }

        [Weak]
        private SeriesViewerView View { get; set; }

        private bool SuspendWindowRectChanges { get; set; }
        private void Model_WindowRectChanged(object sender, RectChangedEventArgs e)
        {
            bool suspendWindowRectChangesStored = this.SuspendWindowRectChanges;
            this.SuspendWindowRectChanges = true;
            this.ZoomLevel = 1.0 - Math.Min(e.NewRect.Height, e.NewRect.Width);
            this.SuspendWindowRectChanges = suspendWindowRectChangesStored;
        }

        [Weak]
        private SeriesViewer Model { get; set; }

        private Canvas PreviewCanvas { get; set; }

        #region IOverviewPlusDetailControl Members

        public void ZoomTo100()
        {
            this.Model.WindowRect = XamDataChart.StandardRect;
        }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


        public void ScaleToFit()
        {
            this.Model.WindowRect = XamDataChart.StandardRect;
        }
        
        //public void RenderPreview()
        //{
        //    this.Model.ChartContentManager.RefreshPreview(false);
        //}

        public void RenderPreview()
        {


            if (!IsDirty)
            {
                return;
            }

            XamOverviewPlusDetailPane opd = this.View.OverviewPlusDetailPane;
            if (opd == null || 
                opd.Visibility != Visibility.Visible || 
                opd.Viewport.IsEmpty ||
                opd.Window.IsNull())
            {
                return;
            }

            Rect previewViewport = opd.PreviewViewportdRect;

            double width = previewViewport.Width;
            double height = previewViewport.Height;

            RenderSurface surface = new RenderSurface();
            this.PreviewCanvas.Children.Clear();
            surface.Surface = this.PreviewCanvas;

            surface.Surface.Clip = new RectangleGeometry() { Rect = previewViewport };

            if (opd.PreviewCanvas.Children.Contains(surface.Surface) == false)
            {
                opd.PreviewCanvas.Children.Add(surface.Surface);
            }

            this.View.GetThumbnail(width, height, surface);

            // this line is causing a complicated problem, and I don't believe it belongs here.
            //opd.UpdateLayout();


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

            IsDirty = false;

        }

        public Rect WorldRect
        {
            get 
            {
                return XamDataChart.StandardRect;
            }
        }

        public Rect ViewportRect
        {
            get 
            {
                return this.Model.ViewportRect;
            }
        }

        public double MinimumZoomLevel
        {
            get 
            {
                return 0.0;
            }
        }

        public double MaximumZoomLevel
        {
            get
            {
                if (this.Model.WindowRectMinWidth == 0.0001)
                {
                    // This is for backward compatibility
                    return 0.9;
                }
                else
                {
                    return 1 - this.Model.WindowRectMinWidth;
                }
            }
        }

        #endregion

        private bool _isDirty = true;
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        private const string ZoomLevelPropertyName = "ZoomLevel";
        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register("ZoomLevel", typeof(double), typeof(SeriesViewerSurfaceViewer), new PropertyMetadata(0.0, (sender, e) =>
            {
                (sender as SeriesViewerSurfaceViewer).OnPropertyChanged(ZoomLevelPropertyName, e.OldValue, e.NewValue);
            }));

        [DontObfuscate]
        public double ZoomLevel
        {
            get
            {
                return (double)this.GetValue(ZoomLevelProperty);
            }
            set
            {
                this.SetValue(ZoomLevelProperty, value);
            }
        }

        private void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            switch (propertyName)
            {
                case ZoomLevelPropertyName:
                    
                    // Next two ifs are for coercion when the value is out of the range. It's different in Silverlight and WPF, that's why it's handled when the property changed event is fired
                    if ((double)newValue - this.MinimumZoomLevel < -0.00001)
                    {
                        this.ZoomLevel = this.MinimumZoomLevel;
                        return;
                    } 
                    
                    if ((double)newValue - this.MaximumZoomLevel > 0.0001)
                    {
                        this.ZoomLevel = this.MaximumZoomLevel;
                        return;
                    }

                    // [DN March 2 2012 : 102164] updatingSliderRanges should indicate the OPD is just initializing and shouldn't cause any WindowRect changes.
                    bool updatingSliderRanges = this.Model != null && this.Model.OverviewPlusDetailPane != null && this.Model.OverviewPlusDetailPane.UpdatingSliderRanges;
                    if (!this.SuspendWindowRectChanges && !updatingSliderRanges)
                    {
                        double windowSize = 1.0 - this.ZoomLevel;

                        Point center = this.Model.ActualWindowRect.GetCenter();
                        Rect newRect = new Rect(center.X - windowSize / 2.0, center.Y - windowSize / 2.0, windowSize, windowSize);
                        this.Model.WindowRect = SeriesViewerSurfaceViewer.ChangeRect(this.Model.WindowRect, newRect, this.Model.HorizontalZoomable, this.Model.VerticalZoomable, this.WorldRect);
                    }
                    this.UpdateZoomLevelDisplayText();






                    break;
            }

        }
        private void UpdateZoomLevelDisplayText()
        {
            double zoomLevelDisplay = Math.Round(100 * (1 / (1 - this.ZoomLevel))); // reduce to 100 / zoomlevel?
            if (zoomLevelDisplay <= 1000)
            {
                this.ZoomLevelDisplayText = zoomLevelDisplay.ToString();
            }
            else
            {
                this.ZoomLevelDisplayText = "> 1000";
            }
        }
        private const string ZoomLevelDisplayTextPropertyName = "ZoomLevelDisplayText";
        private string _zoomLevelDisplayText;
        // this text is displayed in the callout above the zoom level slider.
        public string ZoomLevelDisplayText
        {
            get 
            {
                return this._zoomLevelDisplayText;
            }
            set
            {
                bool changed = this.ZoomLevelDisplayText != value;
                if (changed)
                {
                    object oldValue = this.ZoomLevelDisplayText;
                    this._zoomLevelDisplayText = value;
                    this.OnPropertyChanged(ZoomLevelDisplayTextPropertyName, oldValue, value);
                }
            }
        }

        private InteractionState _defaultInteraction;

        /// <summary>
        /// Gets or sets the default interaction state.
        /// </summary>
        /// <value>The default interaction.</value>
        public InteractionState DefaultInteraction
        {
            get 
            { 
                return _defaultInteraction; 
            }
            set 
            { 
                _defaultInteraction = value;

                this.Model.DefaultInteraction = value;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        internal static Rect ChangeRect(Rect oldRect, Rect newRect, bool horizontalZoomable, bool verticalZoomable, Rect worldRect)
        {
            double left, top, width, height;
            if (horizontalZoomable)
            {
                left = newRect.Left;
                width = newRect.Width;
            }
            else
            {
                left = oldRect.Left;
                width = oldRect.Width;
            }
            if (verticalZoomable)
            {
                top = newRect.Top;
                height = newRect.Height;
            }
            else
            {
                top = oldRect.Top;
                height = oldRect.Height;
            }
            double right = left + width;
            double bottom = top + height;

            double leftOverflow = Math.Max(0.0, worldRect.Left - left);
            double rightOverflow = Math.Max(0.0, right - worldRect.Right);
            double topOverflow = Math.Max(0.0, worldRect.Top - top);
            double bottomOverflow = Math.Max(0.0, bottom - worldRect.Bottom);

            left += leftOverflow - rightOverflow;
            top += topOverflow - bottomOverflow;

            Rect result = new Rect(left, top, width, height);
            result.Intersect(worldRect);
            return result;
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