// Copyright (C) Josh Smith - February 2007
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using WPF.JoshSmith.Adorners;

namespace WPF.JoshSmith.Controls
{
    /// <summary>
    /// A TextBox with support for displaying a list of suggestions when the user
    /// misspells a word.  The user presses the F1 key to display the list of suggestions.
    /// </summary>
    /// <remarks>
    /// Documentation: http://www.codeproject.com/KB/WPF/SmartTextBox.aspx
    /// </remarks>
    public class SmartTextBox : TextBox
    {
        #region Data

        bool areSuggestionsVisible;
        readonly UIElementAdorner adorner;
        readonly ListBox suggestionList;
        readonly static string[] noSuggestions = { "(no spelling suggestions)" };

        #endregion // Data

        #region Static Constructor

        static SmartTextBox()
        {
            // Register the SuggestionListBoxStyle property.
            SuggestionListBoxStyleProperty = DependencyProperty.Register(
                "SuggestionListBoxStyle",
                typeof(Style),
                typeof(SmartTextBox),
                new UIPropertyMetadata(null, OnSuggestionListBoxStyleChanged));
        }

        #endregion // Static Constructor

        #region Constructor

        /// <summary>
        /// Initializes a new instance of SmartTextBox.
        /// </summary>
        public SmartTextBox()
        {
            // Make sure that spellchecking is active for this TextBox.
            SpellCheck.SetIsEnabled(this, true);

            // Initialize the ListBox which displays suggestions.
            this.suggestionList = new ListBox();
            ScrollViewer.SetVerticalScrollBarVisibility(this.suggestionList, ScrollBarVisibility.Hidden);
            this.suggestionList.IsKeyboardFocusWithinChanged += suggestionList_IsKeyboardFocusWithinChanged;
            this.suggestionList.ItemContainerGenerator.StatusChanged += suggestionList_ItemContainerGenerator_StatusChanged;
            this.suggestionList.MouseDoubleClick += suggestionList_MouseDoubleClick;
            this.suggestionList.PreviewKeyDown += suggestionList_PreviewKeyDown;

            // Initialize the adorner which shows the Listbox.
            this.adorner = new UIElementAdorner(this, this.suggestionList);
        }

        #endregion // Constructor

        #region Public Interface

        #region AreSuggestionsVisible

        /// <summary>
        /// Returns true if the list of suggestions is currently displayed.
        /// </summary>
        public bool AreSuggestionsVisible
        {
            get { return this.areSuggestionsVisible; }
        }

        #endregion // AreSuggestionsVisible

        #region GetSpellingError

        /// <summary>
        /// Returns the SpellingError for the word at the current caret index, or null
        /// if the current word is not misspelled.
        /// </summary>
        public SpellingError GetSpellingError()
        {
            int idx = this.FindClosestCharacterInCurrentWord();
            return idx < 0 ? null : base.GetSpellingError(idx);
        }

        #endregion // GetSpellingError

        #region HideSuggestions

        /// <summary>
        /// Hides the list of suggestions and returns input focus to the input area.  
        /// If the list of suggestions is not already displayed, nothing happens.
        /// </summary>
        public void HideSuggestions()
        {
            if (!this.AreSuggestionsVisible)
                return;

            this.suggestionList.ItemsSource = null;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null)
                layer.Remove(this.adorner);

            base.Focus();

            this.areSuggestionsVisible = false;
        }

        #endregion // HideSuggestions

        #region IsCurrentWordMisspelled

        /// <summary>
        /// Returns true if the word at the caret index is misspelled.
        /// </summary>
        public bool IsCurrentWordMisspelled
        {
            get { return this.GetSpellingError() != null; }
        }

        #endregion // IsCurrentWordMisspelled

        #region ShowSuggestions

        /// <summary>
        /// Shows the list of suggestions.  If the current word is not misspelled
        /// this method does nothing.
        /// </summary>
        public void ShowSuggestions()
        {
            if (this.AreSuggestionsVisible || !this.IsCurrentWordMisspelled)
                return;

            // If this method was called by external code,
            // the list of suggestions will not be populated yet.
            if (this.suggestionList.ItemsSource == null)
            {
                this.AttemptToShowSuggestions();
                return;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer == null)
                return;

            // Position the adorner beneath the misspelled word.
            int idx = this.FindBeginningOfCurrentWord();
            Rect rect = base.GetRectFromCharacterIndex(idx);
            this.adorner.SetOffsets(rect.Left, rect.Bottom);

            // Add the adorner into the adorner layer.
            layer.Add(this.adorner);

            // Since the ListBox might have a new set of items but has not 
            // rendered yet, we force it to calculate its metrics so that
            // the height animation has a sensible target value.
            this.suggestionList.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            this.suggestionList.Arrange(new Rect(new Point(), this.suggestionList.DesiredSize));

            // Animate the ListBox's height to the natural value.
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 0.0;
            anim.To = this.suggestionList.ActualHeight;
            anim.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            anim.FillBehavior = FillBehavior.Stop;
            this.suggestionList.BeginAnimation(ListBox.HeightProperty, anim);

            this.areSuggestionsVisible = true;
        }

        #endregion // ShowSuggestions

        #region SuggestionListBoxStyle

        /// <summary>
        /// Represents the SuggestionListBoxStyle property.  This field is read-only. 
        /// </summary>
        public static readonly DependencyProperty SuggestionListBoxStyleProperty;

        /// <summary>
        /// Gets/sets the Style applied to the ListBox which displays spelling suggestions.
        /// This is a dependency property.
        /// </summary>
        public Style SuggestionListBoxStyle
        {
            get { return (Style)GetValue(SuggestionListBoxStyleProperty); }
            set { SetValue(SuggestionListBoxStyleProperty, value); }
        }

        static void OnSuggestionListBoxStyleChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            SmartTextBox smartTextBox = depObj as SmartTextBox;
            smartTextBox.suggestionList.Style = e.NewValue as Style;
        }

        #endregion // SuggestionListBoxStyle

        #endregion // Public Interface

        #region Base Class Overrides

        #region OnMouseDown

        /// <summary>
        /// Hides the list of suggestions.
        /// </summary>		
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.AreSuggestionsVisible)
                this.HideSuggestions();
        }

        #endregion // OnMouseDown

        #region OnPreviewKeyDown

        /// <summary>
        /// Shows/hides the list of suggestions.
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.AreSuggestionsVisible)
            {
                Debug.Assert(
                    !this.suggestionList.IsEnabled,
                    @"The SmartTextBox should only get key messages when the ListBox is visible 
					if there are no suggestions and the ListBox is disabled.");

                // There is a misspelled word but there are no suggestions.
                // Hide the list of suggestions and mark the event as handled.
                // Return without calling the base implementation so that the
                // keystroke is completely eaten.
                this.HideSuggestions();
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);

            if (e.Key == Key.F1)
            {
                Debug.Assert(!this.AreSuggestionsVisible, "Why is the suggestions list already visible?");

                this.AttemptToShowSuggestions();

                if (this.AreSuggestionsVisible)
                    this.suggestionList.SelectedIndex = 0;
            }
            else if (this.AreSuggestionsVisible)
            {
                this.HideSuggestions();
            }
        }

        #endregion // OnPreviewKeyDown

        #region OnRenderSizeChanged

        /// <summary>
        /// Ensures that the list of suggestions is hidden when the TextBox is resized.
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (this.AreSuggestionsVisible)
                this.HideSuggestions();
        }

        #endregion // OnRenderSizeChanged

        #region OnTextChanged

        /// <summary>
        /// Hides the list of suggestions if a spelling error no longer exists at the
        /// current caret location in the TextBox.
        /// </summary>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (this.AreSuggestionsVisible)
                this.AttemptToHideSuggestions();
        }

        #endregion // OnTextChanged

        #endregion // Base Class Overrides

        #region Suggestion List Event Handlers

        #region IsKeyboardFocusWithinChanged

        void suggestionList_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the list of suggestions no longer contains the input focus
            // hide the list.
            bool focused = (bool)e.NewValue;
            if (!focused)
                this.HideSuggestions();
        }

        #endregion // IsKeyboardFocusWithinChanged

        #region ItemContainerGenerator.StatusChanged

        void suggestionList_ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (this.AreSuggestionsVisible &&
                this.suggestionList.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                // The list of suggestions is visible and its ListBoxItems exist,
                // so give input focus to the first item in the list.
                ListBoxItem firstSuggestion = this.suggestionList.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                if (firstSuggestion != null)
                    firstSuggestion.Focus();
            }
        }

        #endregion // ItemContainerGenerator.StatusChanged

        #region MouseDoubleClick

        void suggestionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // The user clicked on a suggestion, so apply it.
            this.ApplySelectedSuggestion();
        }

        #endregion // MouseDoubleClick

        #region PreviewKeyDown

        void suggestionList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.suggestionList.SelectedIndex < 0)
                return;

            if (e.Key == Key.Escape)
            {
                this.HideSuggestions();
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab)
            {
                this.ApplySelectedSuggestion();

                // Mark the event as handled so that the keystroke
                // does not propogate to the TextBox.
                e.Handled = true;
            }
        }

        #endregion // PreviewKeyDown

        #endregion // Suggestion List Event Handlers

        #region Private Helpers

        #region ApplySelectedSuggestion

        void ApplySelectedSuggestion()
        {
            if (!this.AreSuggestionsVisible || this.suggestionList.SelectedIndex < 0)
                return;

            SpellingError error = this.GetSpellingError();
            if (error != null)
            {
                string correctWord = this.suggestionList.SelectedItem as string;
                error.Correct(correctWord);
                base.CaretIndex = this.FindEndOfCurrentWord();
                base.Focus();
            }

            this.HideSuggestions();
        }

        #endregion // ApplySelectedSuggestion

        #region AttemptToShowSuggestions

        void AttemptToShowSuggestions()
        {
            if (this.AreSuggestionsVisible)
                return;

            // If there is no spelling error, there is no
            // need to show the list of suggestions.
            SpellingError error = this.GetSpellingError();
            if (error == null)
                return;

            this.suggestionList.ItemsSource = error.Suggestions;

            if (this.suggestionList.Items.Count == 0)
            {
                // The spellcheck API has no suggested words
                // so display a message which says so.
                this.suggestionList.ItemsSource = SmartTextBox.noSuggestions;
                this.suggestionList.IsEnabled = false;
            }
            else
            {
                // In case the ListBox was disabled previously
                // we enable now.
                if (!this.suggestionList.IsEnabled)
                    this.suggestionList.IsEnabled = true;
            }

            this.ShowSuggestions();
        }

        #endregion // AttemptToShowSuggestions

        #region AttemptToHideSuggestions

        void AttemptToHideSuggestions()
        {
            // If there is not still a spelling error at the
            // caret location, hide the suggestions.
            if (this.AreSuggestionsVisible && !this.IsCurrentWordMisspelled)
            {
                this.HideSuggestions();
            }
        }

        #endregion // AttemptToHideSuggestions

        #region FindBeginningOfCurrentWord

        int FindBeginningOfCurrentWord()
        {
            if (base.Text == null)
                return -1;

            int idx = base.CaretIndex;
            while (idx > 0)
            {
                char prevChar = base.Text[idx - 1];
                if (char.IsWhiteSpace(prevChar) || char.IsPunctuation(prevChar))
                    break;

                --idx;
            }
            return idx;
        }

        #endregion // FindBeginningOfCurrentWord

        #region FindClosestCharacterInCurrentWord

        int FindClosestCharacterInCurrentWord()
        {
            if (base.Text == null)
                return -1;

            int idx = base.CaretIndex;
            if (idx > 0)
            {
                char prevChar = base.Text[idx - 1];
                // If the caret is at the end of a word
                // then we have to use the preceding character
                // so that the typo will be found.
                if (!char.IsWhiteSpace(prevChar))
                    --idx;
            }
            return idx;
        }

        #endregion // FindClosestCharacterInCurrentWord

        #region FindEndOfCurrentWord

        int FindEndOfCurrentWord()
        {
            if (base.Text == null)
                return -1;

            int targetIdx = base.CaretIndex;
            while (targetIdx < base.Text.Length)
            {
                char nextChar = base.Text[targetIdx];
                if (char.IsWhiteSpace(nextChar) || char.IsPunctuation(nextChar))
                    break;

                ++targetIdx;
            }
            return targetIdx;
        }

        #endregion // FindEndOfCurrentWord

        #endregion // Private Helpers
    }
}