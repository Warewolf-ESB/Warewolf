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
using System.Windows.Data;
using Infragistics.Controls.Charts.Util;

namespace Infragistics.Controls.Charts
{
    internal class RadialPieSeriesView
        : AnchoredRadialSeriesView
    {
        protected RadialPieSeries RadialPieModel { get; set; }
        public RadialPieSeriesView(RadialPieSeries model)
            : base(model)
        {
            RadialPieModel = model;

            Slices = new Pool<Path>()
            {
                Create = this.PieCreate,
                Activate = this.PieActivate,
                Disactivate = this.PieDisactivate,
                Destroy = this.PieDestroy
            };
        }

        internal Path PieCreate()
        {
            Path slice = new Path() { DataContext = new DataContext() { Series = Model } };

            slice.SetBinding(Shape.FillProperty, new Binding(Series.ActualBrushPropertyName) { Source = Model });
            slice.SetBinding(Shape.StrokeProperty, new Binding(Series.ActualOutlinePropertyName) { Source = Model });
            slice.SetBinding(Shape.StrokeThicknessProperty, new Binding(Series.ThicknessPropertyName) { Source = Model });
            slice.SetBinding(Shape.StrokeDashArrayProperty, new Binding(Series.DashArrayPropertyName)
            {
                Source = Model,
                Converter = new DoubleCollectionDuplicator()
            });

            slice.SetBinding(Shape.StrokeDashCapProperty, new Binding(Series.DashCapPropertyName) { Source = Model });

            return slice;
        }
        internal void PieActivate(Path slice)
        {
            slice.Detach();
            RootCanvas.Children.Add(slice);
        }
        internal void PieDisactivate(Path slice)
        {
            (slice.DataContext as DataContext).Item = null;

            if (slice.Parent as Panel != null)
            {
                RootCanvas.Children.Remove(slice);
            }
        }
        internal void PieDestroy(Path slice)
        {
            slice.ClearValue(Shape.FillProperty);
            slice.ClearValue(Shape.StrokeProperty);
            slice.ClearValue(Shape.StrokeDashArrayProperty);
            slice.ClearValue(Shape.StrokeThicknessProperty);
            slice.ClearValue(Shape.StrokeDashCapProperty);
        }

        internal Pool<Path> Slices;

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);

            if (!IsThumbnailView)
            {
                Model.RenderSeries(false);
            }
        }

        internal void SlicesUpdated()
        {

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