using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Warewolf.Studio.CustomControls {

    public enum SearchMode {
        Instant,
        Delayed,
    }

    public class SearchTextBox : TextBox {
        private static DependencyProperty labelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(SearchTextBox));

        private static DependencyProperty labelTextColorProperty =
            DependencyProperty.Register(
                "LabelTextColor",
                typeof(Brush),
                typeof(SearchTextBox));

        private static DependencyProperty clearSearchCommandProperty =
            DependencyProperty.Register(
                "ClearSearchCommand",
                typeof(ICommand),
                typeof(SearchTextBox));

        private static DependencyProperty searchModeProperty =
            DependencyProperty.Register(
                "SearchMode",
                typeof(SearchMode),
                typeof(SearchTextBox),
                new PropertyMetadata(SearchMode.Instant));

        private static DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasText",
                typeof(bool),
                typeof(SearchTextBox),
                new PropertyMetadata());
        private static DependencyProperty hasTextProperty = HasTextPropertyKey.DependencyProperty;


        private static DependencyProperty searchEventTimeDelayProperty =
            DependencyProperty.Register(
                "SearchEventTimeDelay",
                typeof(Duration),
                typeof(SearchTextBox),
                new FrameworkPropertyMetadata(
                    new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                    OnSearchEventTimeDelayChanged));

        public static readonly RoutedEvent SearchEvent = 
            EventManager.RegisterRoutedEvent(
                "Search",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SearchTextBox));

        public static readonly DependencyProperty ClearSearchToolTipProperty =
            DependencyProperty.Register("ClearSearchToolTip",
            typeof(string),
            typeof(SearchTextBox));

        static SearchTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SearchTextBox),
                new FrameworkPropertyMetadata(typeof(SearchTextBox)));
        }

        private readonly DispatcherTimer _searchEventDelayTimer;

        public SearchTextBox()
        {
            _searchEventDelayTimer = new DispatcherTimer {Interval = SearchEventTimeDelay.TimeSpan};
            _searchEventDelayTimer.Tick += OnSeachEventDelayTimerTick;           
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            HasText = Text.Length != 0;
        }

        void OnSeachEventDelayTimerTick(object o, EventArgs e) {
            _searchEventDelayTimer.Stop();
            RaiseSearchEvent();
        }

        static void OnSearchEventTimeDelayChanged(
            DependencyObject o, DependencyPropertyChangedEventArgs e) {
            if (o is SearchTextBox stb)
            {
                stb._searchEventDelayTimer.Interval = ((Duration)e.NewValue).TimeSpan;
                stb._searchEventDelayTimer.Stop();
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e) {
            base.OnTextChanged(e);
            
            HasText = Text.Length != 0;
            if (!HasText)
            {
                Focus();
            }
        }

       
        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.Key == Key.Escape && SearchMode == SearchMode.Instant) {
                Text = "";
            }
            else if ((e.Key == Key.Return || e.Key == Key.Enter) && 
                SearchMode == SearchMode.Delayed) {
                RaiseSearchEvent();
            }
            else {
                base.OnKeyDown(e);
            }
        }

        private void RaiseSearchEvent() {
            RoutedEventArgs args = new RoutedEventArgs(SearchEvent);
            RaiseEvent(args);
        }

        public string LabelText {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public Brush LabelTextColor {
            get { return (Brush)GetValue(LabelTextColorProperty); }
            set { SetValue(LabelTextColorProperty, value); }
        }

        public ICommand ClearSearchCommand
        {
            get { return (ICommand)GetValue(ClearSearchCommandProperty); }
            set { SetValue(ClearSearchCommandProperty, value); }
        }

        public SearchMode SearchMode {
            get { return (SearchMode)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        public bool HasText {
            get { return (bool)GetValue(HasTextProperty); }
            private set { SetValue(HasTextPropertyKey, value); }
        }

        public Duration SearchEventTimeDelay {
            get { return (Duration)GetValue(SearchEventTimeDelayProperty); }
            set { SetValue(SearchEventTimeDelayProperty, value); }
        }
        public string ClearSearchToolTip
        {
            get
            {
                return (string)GetValue(ClearSearchToolTipProperty);
            }
            set
            {
                SetValue(ClearSearchToolTipProperty, value);
            }
        }

        public static DependencyProperty SearchEventTimeDelayProperty { get => searchEventTimeDelayProperty; set => searchEventTimeDelayProperty = value; }
        public static DependencyProperty HasTextProperty { get => hasTextProperty; set => hasTextProperty = value; }
        public static DependencyProperty SearchModeProperty { get => searchModeProperty; set => searchModeProperty = value; }
        public static DependencyProperty ClearSearchCommandProperty { get => clearSearchCommandProperty; set => clearSearchCommandProperty = value; }
        public static DependencyProperty LabelTextColorProperty { get => labelTextColorProperty; set => labelTextColorProperty = value; }
        public static DependencyProperty LabelTextProperty { get => labelTextProperty; set => labelTextProperty = value; }

        public event RoutedEventHandler Search {
            add { AddHandler(SearchEvent, value); }
            remove { RemoveHandler(SearchEvent, value); }
        }
    }
}
