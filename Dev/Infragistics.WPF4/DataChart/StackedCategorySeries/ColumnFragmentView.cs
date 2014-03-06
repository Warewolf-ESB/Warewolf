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
using Infragistics.Controls.Charts.Util;
using System.Collections.Generic;
using System.Windows.Data;

namespace Infragistics.Controls.Charts
{
    internal class ColumnFragmentView : AnchoredCategorySeriesView
    {
        internal ColumnFragment ColumnFragmentModel { get; private set; }

        public ColumnFragmentView(ColumnFragment model)
            : base(model)
        {
            ColumnFragmentModel = model;

            High = new List<double>();
            Low = new List<double>();

            Columns = new Pool<Rectangle> { Create = ColumnCreate, Activate = ColumnActivate, Disactivate = ColumnDisactivate, Destroy = ColumnDestroy };
        }

        #region Column Pool Implementation
        internal Pool<Rectangle> Columns { get; set; }
        internal List<double> High { get; set; }
        internal List<double> Low { get; set; }

        private Rectangle ColumnCreate()
        {
            Rectangle column = new Rectangle { DataContext = new DataContext { Series = this.Model } };

            column.SetBinding(Rectangle.FillProperty, new Binding(ColumnFragment.ActualBrushPropertyName) { Source = this.Model });
            column.SetBinding(Rectangle.StrokeProperty, new Binding(ColumnFragment.ActualOutlinePropertyName) { Source = this.Model });
            column.SetBinding(Rectangle.StrokeThicknessProperty, new Binding(ColumnFragment.ThicknessPropertyName) { Source = this.Model });
            column.SetBinding(Shape.StrokeDashArrayProperty, new Binding(ColumnFragment.DashArrayPropertyName)
            {
                Source = this.Model,
                Converter = new DoubleCollectionDuplicator()
            });

            column.SetBinding(Rectangle.StrokeDashCapProperty, new Binding(ColumnFragment.DashCapPropertyName) { Source = this.Model });
            column.SetBinding(Rectangle.RadiusXProperty, new Binding(ColumnFragment.RadiusXPropertyName) { Source = this.Model });
            column.SetBinding(Rectangle.RadiusYProperty, new Binding(ColumnFragment.RadiusYPropertyName) { Source = this.Model });

            return column;
        }
        private void ColumnActivate(Rectangle column)
        {
            if (RootCanvas == null) return;

            column.Detach();
            RootCanvas.Children.Add(column);
        }
        private void ColumnDisactivate(Rectangle column)
        {
            if (RootCanvas == null) return;

            (column.DataContext as DataContext).Item = null;

            if (column.Parent as Panel != null)
            {
                RootCanvas.Children.Remove(column);
            }
        }
        private void ColumnDestroy(Rectangle column)
        {
            column.ClearValue(Rectangle.FillProperty);
            column.ClearValue(Rectangle.StrokeProperty);
            column.ClearValue(Rectangle.StrokeThicknessProperty);
            column.ClearValue(Rectangle.StrokeDashCapProperty);
            column.ClearValue(Rectangle.RadiusXProperty);
            column.ClearValue(Rectangle.RadiusYProperty);
        }
        #endregion

        public override void AttachUI(Canvas rootCanvas)
        {
            base.AttachUI(rootCanvas);
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