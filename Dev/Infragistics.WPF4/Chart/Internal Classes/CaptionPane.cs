
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Shapes;

#endregion Using

namespace Infragistics.Windows.Chart
{
    internal class CaptionPane : ChartCanvas
    {
        #region Fields

        // Private Fields
        private Caption _caption;
        private TextBlock _textBlock;
        private object _chartParent;
     
        #endregion Fields

        #region Properties

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        #endregion Properties

        #region Methods

        internal CaptionPane(Caption caption)
        {
            _caption = caption;
            _textBlock = new TextBlock();
            _textBlock.Text = _caption.Text;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Size = new Size(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
            Children.Clear();
            Draw();
            base.OnRenderSizeChanged(sizeInfo);
        }

        internal void Draw()
        {
            Children.Clear();
            Background = _caption.Background;
            Size size = GetTextSize();

            Rectangle rectangle = new Rectangle();
            rectangle.Fill = Brushes.Transparent;
            rectangle.Stroke = _caption.BorderBrush;
            ChartCanvas.SetLeft(rectangle, 0);
            ChartCanvas.SetTop(rectangle, 0);
            rectangle.Width = Size.Width;
            rectangle.Height = Size.Height;
            rectangle.StrokeThickness = _caption.BorderThickness;

            Children.Add(rectangle);

            ChartCanvas.SetLeft(_textBlock, Size.Width / 2.0 - size.Width / 2.0);
            ChartCanvas.SetTop(_textBlock, Size.Height / 2.0 - size.Height / 2.0);
            

            _textBlock.Text = _caption.Text;

            if (_caption != null)
            {
                // Take appearance from font properties
                if (_caption.ReadLocalValue(Caption.FontFamilyProperty) != DependencyProperty.UnsetValue || Caption.FontFamilyProperty.DefaultMetadata.DefaultValue != _caption.FontFamily)
                {
                    _textBlock.FontFamily = _caption.FontFamily;
                }

                if (_caption.ReadLocalValue(Caption.FontSizeProperty) != DependencyProperty.UnsetValue || (double)Caption.FontSizeProperty.DefaultMetadata.DefaultValue != _caption.FontSize)
                {
                    _textBlock.FontSize = _caption.FontSize;
                }

                if (_caption.ReadLocalValue(Caption.FontStretchProperty) != DependencyProperty.UnsetValue || (FontStretch)Caption.FontStretchProperty.DefaultMetadata.DefaultValue != _caption.FontStretch)
                {
                    _textBlock.FontStretch = _caption.FontStretch;
                }

                if (_caption.ReadLocalValue(Caption.FontStyleProperty) != DependencyProperty.UnsetValue || (FontStyle)Caption.FontStyleProperty.DefaultMetadata.DefaultValue != _caption.FontStyle)
                {
                    _textBlock.FontStyle = _caption.FontStyle;
                }

                if (_caption.ReadLocalValue(Caption.FontWeightProperty) != DependencyProperty.UnsetValue || (FontWeight)Caption.FontWeightProperty.DefaultMetadata.DefaultValue != _caption.FontWeight)
                {
                    _textBlock.FontWeight = _caption.FontWeight;
                }

                if (_caption.ReadLocalValue(Caption.ForegroundProperty) != DependencyProperty.UnsetValue || Caption.ForegroundProperty.DefaultMetadata.DefaultValue != _caption.Foreground)
                {
                    _textBlock.Foreground = _caption.Foreground;
                }
            }

            if (_caption.MarginType == MarginType.Auto)
            {
                if (size.Height > Size.Height)
                {
                    ChartCanvas.SetTop(_textBlock, 0);
                    _textBlock.Height = Size.Height;
                }
            }

            _textBlock.TextAlignment = TextAlignment.Center;

            Children.Add(_textBlock);

        }
                
        internal Size GetTextSize()
        {
            TextBlock label = _textBlock;
            Chart chart = _chartParent as Chart;
            // Take appearance from font properties
            if (_caption.ReadLocalValue(Caption.FontFamilyProperty) != DependencyProperty.UnsetValue || ((FontFamily)Caption.FontFamilyProperty.DefaultMetadata.DefaultValue) != _caption.FontFamily)
            {
                label.FontFamily = _caption.FontFamily;
            }

            if (_caption.ReadLocalValue(Caption.FontSizeProperty) != DependencyProperty.UnsetValue || (double)Caption.FontSizeProperty.DefaultMetadata.DefaultValue != _caption.FontSize)
            {
                label.FontSize = _caption.FontSize;
            }

            if (_caption.ReadLocalValue(Caption.FontStretchProperty) != DependencyProperty.UnsetValue || (FontStretch)Caption.FontStretchProperty.DefaultMetadata.DefaultValue != _caption.FontStretch)
            {
                label.FontStretch = _caption.FontStretch;
            }

            if (_caption.ReadLocalValue(Caption.FontStyleProperty) != DependencyProperty.UnsetValue || (FontStyle)Caption.FontStyleProperty.DefaultMetadata.DefaultValue != _caption.FontStyle)
            {
                label.FontStyle = _caption.FontStyle;
            }

            if (_caption.ReadLocalValue(Caption.FontWeightProperty) != DependencyProperty.UnsetValue || (FontWeight)Caption.FontWeightProperty.DefaultMetadata.DefaultValue != _caption.FontWeight)
            {
                label.FontWeight = _caption.FontWeight;
            }

            if (_caption.ReadLocalValue(Caption.ForegroundProperty) != DependencyProperty.UnsetValue || (Brush)Caption.ForegroundProperty.DefaultMetadata.DefaultValue != _caption.Foreground)
            {
                label.Foreground = _caption.Foreground;
            }


            label.TextWrapping = TextWrapping.Wrap;
            
			if(this.Size.Width > 0)
            {
                label.MaxWidth = this.Size.Width;
            }
            if (this.Size.Height > 0)
            {
                label.MaxHeight = this.Size.Height;
            }


            chart.Children.Add(label);
            Typeface typeface = new Typeface(label.FontFamily, label.FontStyle, label.FontWeight, label.FontStretch);

            CultureInfo cultureToUse = CultureInformation.CultureToUse;

            FormattedText formattedText = new FormattedText(label.Text, cultureToUse, FlowDirection.LeftToRight, typeface, label.FontSize, Brushes.Black);

            if (this.Size.Width > 0)
            {
                formattedText.MaxTextWidth = this.Size.Width;
            }
            if (this.Size.Height > 0)
            {
                formattedText.MaxTextHeight = this.Size.Height;
            }

            Size size = new Size(formattedText.Width, formattedText.Height);
            chart.Children.Remove(label);
            return size;
        }

        #endregion Methods
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