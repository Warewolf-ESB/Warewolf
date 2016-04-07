using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces;

namespace WpfControls.Editors
{
    [TemplatePart(Name = PartEditor, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartSelector, Type = typeof(Selector))]
    public class AutoCompleteTextBox : Control
    {
    

        public static readonly DependencyProperty FilterTypeProperty = DependencyProperty.Register("FilterType", typeof(enIntellisensePartType), typeof(AutoCompleteTextBox), new UIPropertyMetadata(enIntellisensePartType.All));

        public enIntellisensePartType FilterType
        {
            get
            {
                return (enIntellisensePartType)GetValue(FilterTypeProperty);
            }
            set
            {
                SetValue(FilterTypeProperty, value);
            }
        }

        void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            var text = (sender as TextBox).Text;
            if (text == string.Empty)
            {
                ToolTip = _defaultToolTip;
                return;
            }
          var error =   WarewolfDataEvaluationCommon.ParseLanguageExpressionAndValidate(text);
            if (FilterType == enIntellisensePartType.RecordsetsOnly && !error.Item1.IsRecordSetNameExpression)
            {
                ToolTip = error.Item2 != String.Empty ? error.Item2 : "Invalid recordset";
            }
            else if (FilterType == enIntellisensePartType.ScalarsOnly && !error.Item1.IsScalarExpression)
            {
                ToolTip = error.Item2 != String.Empty ? error.Item2 : "Invalid scalar";
            }
            else if (FilterType == enIntellisensePartType.RecordsetFields && !error.Item1.IsRecordSetExpression)
            {
                ToolTip = error.Item2 != String.Empty ? error.Item2 : "Invalid recordset name";
            }
            else ToolTip = error.Item2 != String.Empty ? error.Item2 :  _defaultToolTip??String.Empty  ;
        }

        #region "Fields"

        public const string PartEditor = "PART_Editor";
        public const string PartPopup = "PART_Popup";

        public const string PartSelector = "PART_Selector";
        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(int), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(200));
        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register("DisplayMember", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IconPlacementProperty = DependencyProperty.Register("IconPlacement", typeof(IconPlacement), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(IconPlacement.Left));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(Visibility), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(AutoCompleteTextBox));
        public static readonly DependencyProperty LoadingContentProperty = DependencyProperty.Register("LoadingContent", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register("Provider", typeof(ISuggestionProvider), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(string.Empty));

        private bool _isUpdatingText;

        private bool _selectionCancelled;

        private SuggestionsAdapter _suggestionsAdapter;
        private string _text;
        private int _caretIndex;
        private string _errorToolTip;
        private string _toolTip;
        private string _defaultToolTip;

        #endregion

        #region "Constructors"

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        #endregion

        #region "Properties"

        public BindingEvaluator BindingEvaluator { get; set; }

        public int Delay
        {
            get { return (int)GetValue(DelayProperty); }

            set { SetValue(DelayProperty, value); }
        }

        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }

            set { SetValue(DisplayMemberProperty, value); }
        }

        public TextBox Editor { get; set; }

        public DispatcherTimer FetchTimer { get; set; }

        public string Filter { get; set; }

        public object Icon
        {
            get { return GetValue(IconProperty); }

            set { SetValue(IconProperty, value); }
        }

        public IconPlacement IconPlacement
        {
            get { return (IconPlacement)GetValue(IconPlacementProperty); }

            set { SetValue(IconPlacementProperty, value); }
        }

        public Visibility IconVisibility
        {
            get { return (Visibility)GetValue(IconVisibilityProperty); }

            set { SetValue(IconVisibilityProperty, value); }
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }

            set
            {
                if (value)
                {
                    _text = Editor.Text;
                    _caretIndex = Editor.CaretIndex;
                }
                SetValue(IsDropDownOpenProperty, value);
            }
        }

        public string ErrorToolTip
        {
            get { return _errorToolTip; }
            set
            {
                _errorToolTip = value;
                if (value.Length > 0)
                {
                    ToolTip = value;
                }
            }
        }
        public string DefaultToolTip
        {
            get { return _toolTip; }
            set
            {
                _toolTip = value;
                if (_errorToolTip.Length > 0)
                {
                    ToolTip = value;
                }
            }
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }

            set { SetValue(IsLoadingProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }

            set { SetValue(IsReadOnlyProperty, value); }
        }

        public Selector ItemsSelector { get; set; }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }

            set { SetValue(ItemTemplateProperty, value); }
        }

        public DataTemplateSelector ItemTemplateSelector
        {
            get { return ((DataTemplateSelector)(GetValue(ItemTemplateSelectorProperty))); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        public object LoadingContent
        {
            get { return GetValue(LoadingContentProperty); }

            set { SetValue(LoadingContentProperty, value); }
        }

        public Popup Popup { get; set; }

        public ISuggestionProvider Provider
        {
            get { return (ISuggestionProvider)GetValue(ProviderProperty); }

            set { SetValue(ProviderProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }

            set { SetValue(SelectedItemProperty, value); }
        }

        public SelectionAdapter SelectionAdapter { get; set; }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }

            set
            {
                SetValue(TextProperty, value); 
      
            }
        }

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }

            set { SetValue(WatermarkProperty, value); }
        }

        #endregion

        #region "Methods"

        public static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var act = d as AutoCompleteTextBox;
            if (act != null)
            {
                if (act.Editor != null & !act._isUpdatingText)
                {
                    act._isUpdatingText = true;
                    act.Editor.Text = act.BindingEvaluator.Evaluate(e.NewValue);
                    act._isUpdatingText = false;
                }
            }
        }

        private void ScrollToSelectedItem()
        {
            ListBox listBox = ItemsSelector as ListBox;
            if (listBox != null && listBox.SelectedItem != null)
                listBox.ScrollIntoView(listBox.SelectedItem);
        }

        public override void OnApplyTemplate()
        {
            _defaultToolTip = (string)ToolTip;
            base.OnApplyTemplate();
            Editor = Template.FindName(PartEditor, this) as TextBox;
            if (Editor != null)
            {
                Editor.TextChanged += Editor_TextChanged;
                Editor.LostKeyboardFocus += Editor_LostKeyboardFocus;
            }
            Popup = Template.FindName(PartPopup, this) as Popup;
            ItemsSelector = Template.FindName(PartSelector, this) as Selector;
            BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));

            if (Editor != null)
            {
                Editor.TextChanged += OnEditorTextChanged;
                Editor.PreviewKeyDown += OnEditorKeyDown;
                Editor.LostFocus += OnEditorLostFocus;

                if (SelectedItem != null)
                {
                    Editor.Text = BindingEvaluator.Evaluate(SelectedItem);
                }

            }

            if (Popup != null)
            {
                Popup.StaysOpen = false;
                Popup.Opened += OnPopupOpened;
                Popup.Closed += OnPopupClosed;
            }
            if (ItemsSelector != null)
            {
                SelectionAdapter = new SelectionAdapter(ItemsSelector);
                SelectionAdapter.Commit += OnSelectionAdapterCommit;
                SelectionAdapter.Cancel += OnSelectionAdapterCancel;
                SelectionAdapter.SelectionChanged += OnSelectionAdapterSelectionChanged;
            }
        }

        void Editor_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (WrapInBrackets)
            {
                // ReSharper disable once PossibleNullReferenceException
                try
                {

         
                var txt = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate((sender as TextBox).Text);
                if (txt.IsWarewolfAtomAtomExpression && !string.IsNullOrEmpty(WarewolfDataEvaluationCommon.languageExpressionToString(txt)))
                {
                    Text= AddBracketsToExpression(((TextBox)sender).Text);
                    Editor.Text = Text;
                }
                }
                catch(Exception)
                {
                    // ignored
                }
            }
        }

        public string AddBracketsToExpression(string expression)
        {
            string result = expression.Trim();

            if (!result.StartsWith("[["))
            {
                result = string.Concat(!result.StartsWith("[") ? "[[" : "[", result);
            }

            if (!result.EndsWith("]]"))
            {
                result = string.Concat(result, !expression.EndsWith("]") ? "]]" : "]");
            }

            return result;
        }

        public static readonly DependencyProperty WrapInBracketsProperty = DependencyProperty.Register("WrapInBrackets", typeof(bool), typeof(AutoCompleteTextBox), new UIPropertyMetadata(false));

          public bool WrapInBrackets
        {
            get
            {
                return (bool)GetValue(WrapInBracketsProperty);
            }
            set
            {
                SetValue(WrapInBracketsProperty, value);
            }
        }
        private string GetDisplayText(object dataItem)
        {
            if (BindingEvaluator == null)
            {
                BindingEvaluator = new BindingEvaluator(new Binding(DisplayMember));
            }
            if (dataItem == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(DisplayMember))
            {
                var resbuilder = new IntellisenseStringResultBuilder();
                var editorText = Editor.Text;
                var originalText = _text;
                var originalCaret = _caretIndex;
                if (originalText == String.Empty || dataItem.ToString().StartsWith(originalText))
                {
                    resbuilder.Build(dataItem.ToString(), originalCaret, originalText, editorText);
                    Editor.Text = dataItem.ToString();
                    Editor.SelectionStart = Editor.Text.Length;
                    Editor.SelectionLength = 0;
                    return dataItem.ToString();
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                  

                    var text = resbuilder.Build(dataItem.ToString(), originalCaret, originalText, editorText);
                    Editor.Text = text.Result;
                    Editor.CaretIndex = text.CaretPosition;
                    dataItem = Editor.Text;
                }
            }
            return BindingEvaluator.Evaluate(dataItem);
        }



        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
           if (SelectionAdapter != null)
            {
                if (IsDropDownOpen)
                    SelectionAdapter.HandleKeyDown(e);
                else
                    IsDropDownOpen = e.Key == Key.Down || e.Key == Key.Up;
            }
        }

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                IsDropDownOpen = false;
            }
        }

        private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText)
                return;
            if (FetchTimer == null)
            {
                FetchTimer = new DispatcherTimer();
                FetchTimer.Interval = TimeSpan.FromMilliseconds(Delay);
                FetchTimer.Tick += OnFetchTimerTick;
            }
            FetchTimer.IsEnabled = false;
            FetchTimer.Stop();
            SetSelectedItem(null);
            if (Editor.Text.Length > 0)
            {
                IsLoading = true;
                IsDropDownOpen = true;
                ItemsSelector.ItemsSource = null;
                FetchTimer.IsEnabled = true;
                FetchTimer.Start();
            }
            else
            {
                IsDropDownOpen = false;
            }
        }

        private void OnFetchTimerTick(object sender, EventArgs e)
        {
            FetchTimer.IsEnabled = false;
            FetchTimer.Stop();
            if (Provider != null && ItemsSelector != null)
            {
                Filter = Editor.Text;
                if (_suggestionsAdapter == null)
                {
                    _suggestionsAdapter = new SuggestionsAdapter(this);
                }
                _suggestionsAdapter.GetSuggestions(Filter,FilterType, Editor.Text, Editor.CaretIndex);
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            if (!_selectionCancelled)
            {
                OnSelectionAdapterCommit();
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            _selectionCancelled = false;
            ItemsSelector.SelectedItem = SelectedItem;
        }

        private void OnSelectionAdapterCancel()
        {
            _isUpdatingText = true;
            Editor.Text = SelectedItem == null ? Filter : GetDisplayText(SelectedItem);
            Editor.SelectionStart = Editor.Text.Length;
            Editor.SelectionLength = 0;
            _isUpdatingText = false;
            IsDropDownOpen = false;
            _selectionCancelled = true;
        }

        private void OnSelectionAdapterCommit()
        {
            if (ItemsSelector.SelectedItem != null)
            {
                if (Editor.Text == String.Empty || Editor.Text.StartsWith(ItemsSelector.SelectedItem.ToString()))
                {
                    SelectedItem = ItemsSelector.SelectedItem;
                    _isUpdatingText = true;
                    Editor.Text = GetDisplayText(ItemsSelector.SelectedItem);
                    SetSelectedItem(ItemsSelector.SelectedItem);
                    _isUpdatingText = false;
                    IsDropDownOpen = false;
                }
                else
                {
                    IsDropDownOpen = false;

                }
            }
        }

        private void OnSelectionAdapterSelectionChanged()
        {
            _isUpdatingText = true;
            if (ItemsSelector.SelectedItem == null)
            {
                Editor.Text = Filter;
                Editor.SelectionStart = Editor.Text.Length;
                Editor.SelectionLength = 0;
                ScrollToSelectedItem();
            }
            else
            {
                GetDisplayText(ItemsSelector.SelectedItem);
                //int index = Editor.CaretIndex;
                //Editor.Text = text;
                //Editor.CaretIndex = index;
                //Editor.SelectionStart = index;
                
                ScrollToSelectedItem();
            }
   
            _isUpdatingText = false;
        }

        private void SetSelectedItem(object item)
        {
            _isUpdatingText = true;
            SelectedItem = item;
            _isUpdatingText = false;
        }
        #endregion

        #region "Nested Types"

        private class SuggestionsAdapter
        {

            #region "Fields"

            private AutoCompleteTextBox _actb;

            private string _filter;

            #endregion

            #region "Constructors"

            public SuggestionsAdapter(AutoCompleteTextBox actb)
            {
                _actb = actb;
            }

            #endregion

            #region "Methods"

            public async void GetSuggestions(string searchText,  enIntellisensePartType filterType,string editorText = "", int caretPosition = Int32.MaxValue)
            {
                _filter = searchText;
                _actb.IsLoading = true;

                var list = await GetSuggestionsAsync(
                 searchText,
                 _actb.Provider,

                 caretPosition,
                 editorText, filterType);



                DisplaySuggestions(
                list,
                searchText
            );
            }


            private void DisplaySuggestions(IEnumerable suggestions, string filter)
            {
                if (_filter != filter)
                {
                    return;
                }
                if (_actb.IsDropDownOpen)
                {
                    _actb.IsLoading = false;
                    _actb.ItemsSelector.ItemsSource = suggestions;
                    _actb.IsDropDownOpen = _actb.ItemsSelector.HasItems;
                }

            }

            private Task<IEnumerable> GetSuggestionsAsync(string searchText, ISuggestionProvider provider, int index, string text,enIntellisensePartType filterType)
            {
                
                var task =  new Task<IEnumerable>(() =>
                {
                    IEnumerable list;
                    if (index == text.Length - 1)
                    {
                        list = provider.GetSuggestions(searchText, index, false, filterType);
                    }
                    else
                    {
                        list = provider.GetSuggestions(searchText, index, true, filterType);

                    }
                    return list;

                });
                task.Start();
                return task;
            #endregion

            }


            #endregion

        }
    }
}
