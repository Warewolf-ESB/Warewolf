using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Warewolf.Studio.CustomControls
{
    public class VariableTextBox : TextBox
    {
        public static DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(VariableTextBox));

        public static DependencyProperty LabelTextColorProperty =
            DependencyProperty.Register(
                "LabelTextColor",
                typeof(Brush),
                typeof(VariableTextBox));

        public static DependencyProperty AddNoteCommandProperty =
            DependencyProperty.Register(
                "AddNoteCommand",
                typeof(ICommand),
                typeof(VariableTextBox));

        public static DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(
                "DeleteCommand",
                typeof(ICommand),
                typeof(VariableTextBox));

        public static DependencyProperty ViewComplexObjectsCommandProperty =
            DependencyProperty.Register(
                "ViewComplexObjectsCommand",
                typeof(ICommand),
                typeof(VariableTextBox));

        private static readonly DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof(bool),
                typeof(VariableTextBox),
                new PropertyMetadata());
        public static DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        public static DependencyProperty AllowNotesProperty =
            DependencyProperty.Register(
                "AllowNotes",
                typeof(bool),
                typeof(VariableTextBox));
        
        public static DependencyProperty IsUsedProperty =
            DependencyProperty.Register(
                "IsUsed",
                typeof(bool),
                typeof(VariableTextBox));

        public static DependencyProperty IsComplexObjectProperty =
            DependencyProperty.Register(
                "IsComplexObject",
                typeof(bool),
                typeof(VariableTextBox));

        static VariableTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(VariableTextBox),
                new FrameworkPropertyMetadata(typeof(VariableTextBox)));
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            HasText = Text.Length != 0;
        }

        public string LabelText {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public Brush LabelTextColor {
            get { return (Brush)GetValue(LabelTextColorProperty); }
            set { SetValue(LabelTextColorProperty, value); }
        }

        public ICommand AddNoteCommand
        {
            get
            {
                return (ICommand)GetValue(AddNoteCommandProperty);
            }
            set
            {
                if (AllowNotes)
                {
                    SetValue(AddNoteCommandProperty, value);
                }
            }
        }

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set
            {
                if (IsUsed)
                {
                    SetValue(DeleteCommandProperty, value);
                }
            }
        }

        public ICommand ViewComplexObjectsCommand
        {
            get
            {
                return (ICommand)GetValue(ViewComplexObjectsCommandProperty);
            }
            set
            {
                if (IsComplexObject)
                {
                    SetValue(ViewComplexObjectsCommandProperty, value);
                }
            }
        }

        public bool HasText {
            get { return (bool)GetValue(HasTextProperty); }
            private set { SetValue(HasTextPropertyKey, value); }
        }

        public bool AllowNotes
        {
            get { return (bool)GetValue(AllowNotesProperty); }
            set { SetValue(AllowNotesProperty, value); }
        }
        
        public bool IsUsed
        {
            get { return (bool)GetValue(IsUsedProperty); }
            set { SetValue(IsUsedProperty, value); }
        }

        public bool IsComplexObject
        {
            get { return (bool)GetValue(IsComplexObjectProperty); }
            set { SetValue(IsComplexObjectProperty, value); }
        }
    }
}
