using System.Windows;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Warewolf.Studio.CustomControls
{
    public class TextBoxColumn : EditableColumn
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark",
                typeof(string),
                typeof(TextBoxColumn), new PropertyMetadata(null));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set
            {
                SetValue(WatermarkProperty, value);
            }
        }

        protected override ColumnContentProviderBase GenerateContentProvider()
        {
            return new TextColumnContentProvider();
        }
    }
}