using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dev2.CustomControls
{
    [TemplatePart(Name = PART_ButtonBase, Type = typeof(ButtonBase))]
    [TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
    public class FilterTextBox : TextBox
    {
        private const string PART_ButtonBase = "FilterButton";
        private const string PART_TextBox = "TheTextBox";

        private ButtonBase _button;
        private TextBox _textBox;

        public ButtonBase FilterButton
        {
            get
            {
                return _button;
            }
        }
        public TextBox TheTextBox
        {
            get
            {
                return _textBox;
            }
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText",
            typeof(string), typeof(FilterTextBox), new PropertyMetadata(string.Empty));

        public FilterTextBox()
        {
            DefaultStyleKey = typeof(FilterTextBox);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild(PART_TextBox) as TextBox;
            _button = GetTemplateChild(PART_ButtonBase) as ButtonBase;

            if (_button != null)
            {
                _button.Click += ButtonClick;
            }
        }

        void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (_textBox != null)
            {
                _textBox.Text = "";
                _textBox.Focus();
            }
        }
    }
}
