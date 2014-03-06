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

namespace Infragistics.Controls.Charts
{
    internal class AngleAxisLabelPanelView
        : AxisLabelPanelBaseView
    {
        protected AngleAxisLabelPanel AngleModel { get; set; }
        public AngleAxisLabelPanelView(AngleAxisLabelPanel model)
            : base(model)
        {
            AngleModel = model;
        }

        internal override void OnInit()
        {
            base.OnInit();

            AngleModel.ClipLabelsToBounds = true;
            AngleModel.SizeChanged += AngleLabelAxisPanel_SizeChanged;
            AngleModel.Loaded += AngleLabelAxisPanel_Loaded;
        }

        private void AngleLabelAxisPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ClipToBounds();
        }

        private void ClipToBounds()
        {
            AngleModel.Clip =
                new RectangleGeometry()
                {
                    Rect = new Rect(
                        0, 0, 
                        AngleModel.ActualWidth, 
                        AngleModel.ActualHeight)
                };
        }

        private void AngleLabelAxisPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClipToBounds();
        }

        internal void DetermineLargestLabels(List<Rect> rectangles)
        {
            AngleModel.LargestWidth = double.MinValue;
            AngleModel.LargestHeight = double.MinValue;

            for (int i = 0; i < Model.TextBlocks.Count; i++)
            {
                FrameworkElement currentTextBlock = Model.TextBlocks[i];
                LabelPosition position = Model.LabelPositions[i];

                Point point = AngleModel.GetPoint(position.Value);

                double x = point.X - currentTextBlock.DesiredSize.Width / 2;
                double y = point.Y - currentTextBlock.DesiredSize.Height / 2;
                double width = currentTextBlock.DesiredSize.Width;
                double height = currentTextBlock.DesiredSize.Height;

                AngleModel.LargestWidth = Math.Max(width, AngleModel.LargestWidth);
                AngleModel.LargestHeight = Math.Max(height, AngleModel.LargestHeight);

                Rect rect = new Rect(x, y, width, height);
                rectangles.Add(rect);
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