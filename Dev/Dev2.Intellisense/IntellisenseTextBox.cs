/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Data.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors;
// ReSharper disable ForCanBeConvertedToForeach

// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable UnusedMember.Global
// ReSharper disable NonLocalizedString
// ReSharper disable CheckNamespace
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Dev2.UI
{
    public class IntellisenseTextBox : AutoCompleteBox, INotifyPropertyChanged
    {
        readonly List<Key> _wrapInBracketKey = new List<Key> { Key.F6, Key.F7 };

        public IntellisenseTextBox()
        {
            FilterMode = AutoCompleteFilterMode.Custom;
            TextFilter = (search, item) => true;
            _toolTip = new ToolTip();
            CustomSelection = true;
            ItemsSource = IntellisenseResults;
            _desiredResultSet = IntellisenseDesiredResultSet.EntireSet;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        void OnPaste(object sender, DataObjectPastingEventArgs dataObjectPastingEventArgs)
        {
            bool isText = dataObjectPastingEventArgs.SourceDataObject.GetDataPresent(DataFormats.Text, true);
            if (!isText)
            {
                return;
            }

            var text = dataObjectPastingEventArgs.SourceDataObject.GetData(DataFormats.Text) as string;

            if (text != null && text.Contains("\t"))
            {
                var args = new RoutedEventArgs(TabInsertedEvent, this);
                RaiseEvent(args);
            }
        }

        public static readonly RoutedEvent TabInsertedEvent = EventManager.RegisterRoutedEvent("TabInserted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntellisenseTextBox));

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            var originalSource = e.OriginalSource as TextBlock;
            if (originalSource != null)
            {
                InsertItem(originalSource.Text, true);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            bool isOpen = IsDropDownOpen;

            if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Tab)
            {
                const bool isInsert = false;
                bool expand = false;

                object appendText = HandleMultiLine(e, isOpen, ref expand);

                if (isOpen && e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    appendText = SetAppendTextBasedOnSelection();
                }

                if (appendText != null && IsDropDownOpen)
                {
                    e.Handled = true;
                }
                InsertItem(appendText, isInsert);

                HandleSpecialKeys(e);
            }
            else if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (!isOpen)
                {
                    _desiredResultSet = IntellisenseDesiredResultSet.EntireSet;
                    IsDropDownOpen = true;
                }

                e.Handled = true;
            }            
            else if (e.Key == Key.Home || e.Key == Key.End)
            {
                CloseDropDown(true,false);
            }
            else if (e.Key == Key.V && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                IsPaste = true;
            }
        }

        private string HandleMultiLine(KeyEventArgs e, bool isOpen, ref bool expand)
        {
            string appendText = null;
            if(AllowUserInsertLine && !isOpen && e.Key != Key.Tab && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                if(LineCount < TextBox.MaxLines)
                {
                    appendText = Environment.NewLine;
                    expand = true;
                }
            }
            return appendText;
        }

        private void HandleSpecialKeys(KeyEventArgs e)
        {
            if(e.Key != Key.Tab)
            {
                if((e.Key == Key.Enter || e.Key == Key.Return) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift && AcceptsReturn)
                {
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private object SetAppendTextBasedOnSelection()
        {
            object selectedItem;
            object appendText = null;
            if(SelectionAdapter != null && (selectedItem = SelectionAdapter.SelectedItem) != null)
            {
                var verifyPart = selectedItem as IDataListVerifyPart;
                if(verifyPart != null)
                {
                    appendText = verifyPart.DisplayValue;
                }
                else if(SelectionAdapter.SelectedItem is IntellisenseProviderResult)
                {
                    appendText = (IntellisenseProviderResult)SelectionAdapter.SelectedItem;
                }
                else
                {
                    appendText = selectedItem.ToString();
                }
            }
            return appendText;
        }

        public void InsertItem(object item, bool force)
        {
            bool isOpen = IsDropDownOpen;
            string appendText = null;
            bool isInsert = false;
            int index = CaretIndex;
            IIntellisenseProvider currentProvider = new DefaultIntellisenseProvider();

            if (isOpen || force)
            {
                currentProvider = PerformInsertFromDropDown(item, currentProvider, ref appendText, ref isInsert);
            }

            if (appendText != null)
            {
                string currentText = Text;

                int foundLength = 0;
                if (isInsert)
                {
                    if (currentProvider.HandlesResultInsertion)
                    {
                        var context = new IntellisenseProviderContext { CaretPosition = index, InputText = currentText };

                        try
                        {
                            Text = currentProvider.PerformResultInsertion(appendText, context);
                        }
                        // ReSharper disable EmptyGeneralCatchClause
                        catch
                        // ReSharper restore EmptyGeneralCatchClause
                        {
                            //This try catch is to prevent the intellisense box from ever being crashed from a provider.
                            //This catch is intentionally blanks since if a provider throws an exception the intellisense
                            //box should simply ignore that provider.
                        }
                        
                        TextBox?.Select(context.CaretPosition, 0);

                        IsDropDownOpen = false;
                        appendText = null;
                    }
                    else
                    {
                        int foundMinimum = -1;

                        for (int i = index - 1; i >= 0; i--)
                        {
                            if (appendText.StartsWith(currentText.Substring(i, index - i), StringComparison.OrdinalIgnoreCase))
                            {
                                foundMinimum = i;
                                foundLength = index - i;
                            }
                            else if (foundMinimum != -1 || appendText.IndexOf(currentText[i].ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) == -1)
                            {
                                i = -1;
                            }
                        }

                        if (foundMinimum != -1)
                        {
                            index = foundMinimum;
                            Text = currentText = currentText.Remove(foundMinimum, foundLength);
                        }
                    }
                }

                if (appendText != null)
                {
                    AppendText(currentText, index, appendText);
                }
            }

            UpdateErrorState();
            EnsureErrorStatus();
        }

        private IIntellisenseProvider PerformInsertFromDropDown(object item, IIntellisenseProvider currentProvider, ref string appendText, ref bool isInsert)
        {
            var intellisenseProviderResult = item as IntellisenseProviderResult;
            if(intellisenseProviderResult != null)
            {
                currentProvider = intellisenseProviderResult.Provider;
            }

            object selectedItem = item;

            if(SelectionAdapter != null)
            {
                var verifyPart = selectedItem as IDataListVerifyPart;
                if(verifyPart != null)
                {
                    appendText = verifyPart.DisplayValue;
                }
                else
                {
                    if(selectedItem != null)
                    {
                        appendText = selectedItem.ToString();
                    }
                }

                isInsert = true;
                CloseDropDown(true, false);
            }
            return currentProvider;
        }

        private void AppendText(string currentText, int index, string appendText)
        {
            if(currentText.Length == index)
            {
                TextBox?.AppendText(appendText);
                TextBox?.Select(Text.Length, 0);
            }
            else
            {
                currentText = currentText.Insert(index, appendText);
                Text = currentText;
                TextBox?.Select(index + appendText.Length, 0);
            }

            IsDropDownOpen = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HandleWrapInBrackets(e.Key);

            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                CloseDropDown(true, false);
            }
        }

        public void HandleWrapInBrackets(Key e)
        {
            if (_wrapInBracketKey.Contains(e))
            {
                ExecWrapBrackets();
            }
        }

        protected void EnsureErrorStatus()
        {
            var currentText = Text;
            if (string.IsNullOrEmpty(currentText)) return;

            if (AllowMultipleVariables)
            {
                var parts = currentText.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    ValidateText(part.Trim());
                }
            }
            else
            {
                ValidateText(currentText);
            }
        }
        internal void UpdateErrorState()
        {
            EnsureIntellisenseResults(Text, true, _desiredResultSet);
        }

        public bool IsPaste { get; set; }

        public int LineCount { get; set; }



        public bool CheckHasUnicodeInText(string inputText)
        {
            bool hasUnicode = inputText.ContainsUnicodeCharacter();
            if (hasUnicode)
            {
                string previousInput = inputText;
                Text = "";
                CustomContainer.Get<IPopupController>()
                    .ShowInvalidCharacterMessage(previousInput);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.TextChanged" />
        /// event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs" />
        /// that contains the event data.</param>
        protected override void OnTextChanged(RoutedEventArgs e)
        {
            var text = Text ?? string.Empty;
            if (CheckHasUnicodeInText(text))
            {
                return;
            }
            
            ItemsSource = IntellisenseResults;
            base.OnTextChanged(e);
            EnsureErrorStatus();
            _desiredResultSet = string.IsNullOrEmpty(text) ? IntellisenseDesiredResultSet.EntireSet : IntellisenseDesiredResultSet.ClosestMatch;
        }

        private void ValidateText(string text)
        {
            if (!HasError)
            {
                _originalToolTip = ToolTip;
            }
            var error = IntellisenseStringProvider.parseLanguageExpressionAndValidate(text);
            if (FilterType != enIntellisensePartType.JsonObject)
            {
                if (FilterType == enIntellisensePartType.RecordsetsOnly && !error.Item1.IsRecordSetNameExpression)
                {
                    ToolTip = error.Item2 != string.Empty ? error.Item2 : "Invalid recordset";
                    HasError = true;
                }
                else if (FilterType == enIntellisensePartType.ScalarsOnly && !error.Item1.IsScalarExpression)
                {
                    ToolTip = error.Item2 != string.Empty ? error.Item2 : "Invalid scalar";
                    HasError = true;
                }
                else if (FilterType == enIntellisensePartType.RecordsetFields && !error.Item1.IsRecordSetExpression)
                {
                    ToolTip = error.Item2 != string.Empty ? error.Item2 : "Invalid recordset name";
                    HasError = true;
                }               
                else
                {
                    if (error.Item2 != string.Empty)
                    {
                        ToolTip = error.Item2;
                        HasError = true;
                    }
                    else
                    {
                        ToolTip = _originalToolTip;
                        HasError = false;
                    }
                }
            }else
            {
                if (error.Item2 != string.Empty)
                {
                    ToolTip = error.Item2;
                    HasError = true;
                }
                else
                {
                    ToolTip = _originalToolTip;
                    HasError = false;
                }
                
            }
            
        }

        public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register("SelectAllOnGotFocus", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(false));

            public bool SelectAllOnGotFocus
            {
                get
                {
                    return (bool)GetValue(SelectAllOnGotFocusProperty);
                }
                set
                {
                    SetValue(SelectAllOnGotFocusProperty, value);
                }
            }

        private IEnumerable<IntellisenseProviderResult> IntellisenseResults
        {
            get
            {
                
                EnsureIntellisenseResults(Text,true, _desiredResultSet);
                return _intellisenseResults;
            }            
        }

        public void EnsureIntellisenseResults(string text, bool forceUpdate, IntellisenseDesiredResultSet desiredResultSet)
        {
            if (text == null)
            {
                text = string.Empty;
            }
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                bool calculateMode = false;

                if (AllowUserCalculateMode)
                {
                    if (text.Length > 0 && text[0] == '=')
                    {
                        calculateMode = true;
                        text = text.Substring(1);
                    }

                    IsInCalculateMode = calculateMode;
                }
                else if (IsInCalculateMode)
                {
                    calculateMode = true;
                }

                if (forceUpdate)
                {
                    IIntellisenseProvider provider = IntellisenseProvider;
                    var context = new IntellisenseProviderContext { FilterType = FilterType, DesiredResultSet = desiredResultSet, InputText = text, CaretPosition = CaretIndex };

                    if ((context.IsInCalculateMode = calculateMode) && AllowUserCalculateMode)
                    {
                        if (CaretIndex > 0)
                        {
                            context.CaretPosition = CaretIndex - 1;
                        }
                    }

                    IList<IntellisenseProviderResult> results = null;

                    try
                    {
                        results = provider.GetIntellisenseResults(context);
                        _intellisenseResults = results.ToList();
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        //This try catch is to prevent the intellisense box from ever being crashed from a provider.
                        //This catch is intentionally blanks since if a provider throws an exception the intellisense
                        //box should simbly ignore that provider.
                    }
                    ProcessResults(text, results, false);

                }
            }
        }

        private void ProcessResults(string text, IList<IntellisenseProviderResult> results, bool cleared)
        {
            if(results != null && results.Count > 0)
            {
                IntellisenseProviderResult popup = null;

                for(int i = 0; i < results.Count; i++)
                {
                    IntellisenseProviderResult currentResult = results[i];

                    if(!currentResult.IsError)
                    {
                        if(!currentResult.IsPopup)
                        {
                            if(!cleared)
                            {
                                cleared = true;
                            }
                        }
                        else
                        {
                            popup = currentResult;
                        }
                    }
                }
                if(popup != null)
                {
                    string description = popup.Description;

                    _toolTip.Content = string.IsNullOrEmpty(description) ? "" : description;
                    _toolTip.IsOpen = true;
                    ToolTip = _toolTip;
                }
            }
            else
            {
                var ttErrorBuilder = new StringBuilder();
                if(text.Contains("[[") && text.Contains("]]"))
                {
                    if(FilterType == enIntellisensePartType.RecordsetFields || FilterType == enIntellisensePartType.RecordsetsOnly)
                    {
                        if(!(text.Contains("(") && text.Contains(")")))
                        {
                            HasError = true;
                            ttErrorBuilder.AppendLine("Scalar is not allowed");
                        }
                    }
                    else if(FilterType == enIntellisensePartType.ScalarsOnly)
                    {
                        if(text.Contains("(") && text.Contains(")"))
                        {
                            HasError = true;
                            ttErrorBuilder.AppendLine("Recordset is not allowed");
                        }
                    }
                }

                string errorText = ttErrorBuilder.ToString();
                _toolTip.Content = string.IsNullOrEmpty(errorText) ? "" : errorText;
            }
        }

        public int CaretIndex
        {
            get
            {
                return TextBox?.CaretIndex ?? 0;
            }
            set
            {
                if (TextBox != null)
                {
                    TextBox.CaretIndex = value;
                }
            }
        }

        public static readonly DependencyProperty AllowMultilinePasteProperty = DependencyProperty.Register("AllowMultilinePaste", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(true, OnAllowMultilinePasteChanged));

        public bool AllowMultilinePaste
        {
            get
            {
                return (bool)GetValue(AllowMultilinePasteProperty);
            }
            set
            {
                SetValue(AllowMultilinePasteProperty, value);
            }
        }

        static void OnAllowMultilinePasteChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var box = sender as IntellisenseTextBox;
            box?.OnAllowMultilinePasteChanged((bool)args.OldValue, (bool)args.NewValue);
        }

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(true, OnAllowMultilinePasteChanged));

        public bool AcceptsReturn
        {
            get
            {
                return (bool)GetValue(AcceptsReturnProperty);
            }
            set
            {
                SetValue(AcceptsReturnProperty, value);
                TextBox.AcceptsReturn = value;
            }
        }

        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register("AcceptsTab", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata());

        public bool AcceptsTab
        {
            get
            {
                return (bool)GetValue(TextBoxBase.AcceptsTabProperty);
            }
            set
            {
                SetValue(TextBoxBase.AcceptsTabProperty, value);
            }
        }

        public static readonly DependencyProperty IsUndoEnabledProperty = DependencyProperty.Register("IsUndoEnabled", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata());

        public bool IsUndoEnabled
        {
            get
            {
                return (bool)GetValue(TextBoxBase.IsUndoEnabledProperty);
            }
            set
            {
                SetValue(TextBoxBase.IsUndoEnabledProperty, value);
            }
        }

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(IntellisenseTextBox), new PropertyMetadata());

        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)GetValue(TextBox.TextWrappingProperty);
            }
            set
            {
                SetValue(TextBox.TextWrappingProperty, value);
            }
        }

        public static readonly DependencyProperty MinLinesProperty = DependencyProperty.Register("MinLines", typeof(int), typeof(IntellisenseTextBox), new PropertyMetadata());

        public int MinLines
        {
            get
            {
                return (int)GetValue(TextBox.MinLinesProperty);
            }
            set
            {
                SetValue(TextBox.MinLinesProperty, value);
            }
        }

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(IntellisenseTextBox), new PropertyMetadata());

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)GetValue(TextBoxBase.HorizontalScrollBarVisibilityProperty);
            }
            set
            {
                SetValue(TextBoxBase.HorizontalScrollBarVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(IntellisenseTextBox), new PropertyMetadata());

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get
            {
                return (ScrollBarVisibility)GetValue(TextBoxBase.VerticalScrollBarVisibilityProperty);
            }
            set
            {
                SetValue(TextBoxBase.VerticalScrollBarVisibilityProperty, value);
            }
        }

        protected virtual void OnAllowMultilinePasteChanged(bool oldValue, bool newValue)
        {
            if(TextBox != null)
                TextBox.AcceptsReturn = newValue;
        }

        public static readonly DependencyProperty AllowUserCalculateModeProperty =
            DependencyProperty.Register("AllowUserCalculateMode", typeof(bool), typeof(IntellisenseTextBox),
                new PropertyMetadata(false));

        public bool AllowUserCalculateMode
        {
            get
            {
                return (bool)GetValue(AllowUserCalculateModeProperty);
            }
            set
            {
                SetValue(AllowUserCalculateModeProperty, value);
            }
        }

       
        public static readonly DependencyProperty IsInCalculateModeProperty = DependencyProperty.Register("IsInCalculateMode", typeof(bool), typeof(IntellisenseTextBox), new PropertyMetadata(false, OnIsInCalculateModeChanged));

        public bool IsInCalculateMode
        {
            get
            {
                return (bool)GetValue(IsInCalculateModeProperty);
            }
            set
            {
                SetValue(IsInCalculateModeProperty, value);
            }
        }

        protected virtual void OnIsInCalculateModeChanged(bool oldValue, bool newValue)
        {
        }

        static void OnIsInCalculateModeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var box = sender as IntellisenseTextBox;

            box?.OnIsInCalculateModeChanged((bool)args.OldValue, (bool)args.NewValue);
        }

        public static readonly DependencyProperty AllowMultipleVariablesProperty = DependencyProperty.Register("AllowMultipleVariables", typeof(bool), typeof(IntellisenseTextBox), new UIPropertyMetadata(false));

        public bool AllowMultipleVariables
        {
            get
            {
                return (bool)GetValue(AllowMultipleVariablesProperty);
            }
            set
            {
                SetValue(AllowMultipleVariablesProperty, value);
            }
        }


       

        public static readonly DependencyProperty FilterTypeProperty = DependencyProperty.Register("FilterType", typeof(enIntellisensePartType), typeof(IntellisenseTextBox), new UIPropertyMetadata(enIntellisensePartType.All));

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

        public static readonly DependencyProperty WrapInBracketsProperty = DependencyProperty.Register("WrapInBrackets", typeof(bool), typeof(IntellisenseTextBox), new UIPropertyMetadata(false));

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

        public static readonly DependencyProperty IntellisenseProviderProperty = DependencyProperty.Register("IntellisenseProvider", typeof(IIntellisenseProvider), typeof(IntellisenseTextBox), new PropertyMetadata(null));

        public IIntellisenseProvider IntellisenseProvider
        {
            get
            {
                return (IIntellisenseProvider)GetValue(IntellisenseProviderProperty) ?? new DefaultIntellisenseProvider();
            }
            set
            {
                SetValue(IntellisenseProviderProperty, value);
            }
        }

        private readonly ToolTip _toolTip;
        private List<IntellisenseProviderResult> _intellisenseResults;
        private IntellisenseDesiredResultSet _desiredResultSet;
        private object _originalToolTip;

        [ExcludeFromCodeCoverage]
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            BindingExpression be = BindingOperations.GetBindingExpression(this, TextProperty);
            be?.UpdateSource();
            if (SelectAllOnGotFocus)
            {
                TextBox?.SelectAll();
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            LostFocusImpl();
        }

        void LostFocusImpl()
        {
            ExecWrapBrackets();
            //CloseDropDown(true, false);
            BindingExpression be = BindingOperations.GetBindingExpression(this, TextProperty);
            be?.UpdateSource();
        }

        void ExecWrapBrackets()
        {
            if (WrapInBrackets && !string.IsNullOrWhiteSpace(Text))
            {
                Text = AddBracketsToExpression(Text);
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
            if (FilterType == enIntellisensePartType.JsonObject && !result.Contains("@"))
            {
                result = result.Insert(2, "@");
            }

            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Paste()
        {
            TextBox?.Paste();
        }
    }
}
