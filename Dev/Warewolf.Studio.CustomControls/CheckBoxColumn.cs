using System.Windows;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;

namespace Warewolf.Studio.CustomControls
{
    public class CheckBoxColumn : EditableColumn
    {
        public static readonly DependencyProperty CheckBoxProperty =
            DependencyProperty.Register("CheckBox",
                typeof(string),
                typeof(CheckBoxColumn), new PropertyMetadata(null));

        public string CheckBox
        {
            get { return (string)GetValue(CheckBoxProperty); }
            set
            {
                SetValue(CheckBoxProperty, value);
            }
        }

        protected override ColumnContentProviderBase GenerateContentProvider()
        {
            return new CheckBoxColumnContentProvider();
        }
    }
}