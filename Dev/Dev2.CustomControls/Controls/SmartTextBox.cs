/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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

namespace Dev2.CustomControls.Controls
{
    /// <summary>
    ///     A TextBox with support for displaying a list of suggestions when the user
    ///     misspells a word.  The user presses the F1 key to display the list of suggestions.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/SmartTextBox.aspx
    /// </remarks>
    public class SmartTextBox : TextBox
    {
        #region Data

        private static readonly string[] noSuggestions = {"(no spelling suggestions)"};
        private readonly UIElementAdorner adorner;
        private readonly ListBox suggestionList;
        private bool areSuggestionsVisible;

        #endregion // Data

        #region Static Constructor

        static SmartTextBox()
        {
            // Register the SuggestionListBoxStyle property.
            SuggestionListBoxStyleProperty = DependencyProperty.Register(
                "SuggestionListBoxStyle",
                typeof (Style),
                typeof (SmartTextBox),
                new UIPropertyMetadata(null, OnSuggestionListBoxStyleChanged));
        }

        #endregion // Static Constructor

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of SmartTextBox.
        /// </summary>
        public SmartTextBox()
        {
            // Make sure that spell checking is active for this TextBox.
            SpellCheck.SetIsEnabled(this, true);

            // Initialize the ListBox which displays suggestions.
            suggestionList = new ListBox();
            ScrollViewer.SetVerticalScrollBarVisibility(suggestionList, ScrollBarVisibility.Hidden);
            suggestionList.IsKeyboardFocusWithinChanged += suggestionList_IsKeyboardFocusWithinChanged;
            suggestionList.ItemContainerGenerator.StatusChanged += suggestionList_ItemContainerGenerator_StatusChanged;
            suggestionList.MouseDoubleClick += suggestionList_MouseDoubleClick;
            suggestionList.PreviewKeyDown += suggestionList_PreviewKeyDown;

            // Initialize the adorner which shows the Listbox.
            adorner = new UIElementAdorner(this, suggestionList);
        }

        #endregion // Constructor

        #region Public Interface

        #region AreSuggestionsVisible

        /// <summary>
        ///     Returns true if the list of suggestions is currently displayed.
        /// </summary>
        public bool AreSuggestionsVisible
        {
            get { return areSuggestionsVisible; }
        }

        #endregion // AreSuggestionsVisible

        #region GetSpellingError

        /// <summary>
        ///     Returns the SpellingError for the word at the current caret index, or null
        ///     if the current word is not misspelled.
        /// </summary>
        public SpellingError GetSpellingError()
        {
            int idx = FindClosestCharacterInCurrentWord();
            return idx < 0 ? null : GetSpellingError(idx);
        }

        #endregion // GetSpellingError

        #region HideSuggestions

        /// <summary>
        ///     Hides the list of suggestions and returns input focus to the input area.
        ///     If the list of suggestions is not already displayed, nothing happens.
        /// </summary>
        public void HideSuggestions()
        {
            if (!AreSuggestionsVisible)
                return;

            suggestionList.ItemsSource = null;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer != null)
                layer.Remove(adorner);

            Focus();

            areSuggestionsVisible = false;
        }

        #endregion // HideSuggestions

        #region IsCurrentWordMisspelled

        /// <summary>
        ///     Returns true if the word at the caret index is misspelled.
        /// </summary>
        public bool IsCurrentWordMisspelled
        {
            get { return GetSpellingError() != null; }
        }

        #endregion // IsCurrentWordMisspelled

        #region ShowSuggestions

        /// <summary>
        ///     Shows the list of suggestions.  If the current word is not misspelled
        ///     this method does nothing.
        /// </summary>
        public void ShowSuggestions()
        {
            if (AreSuggestionsVisible || !IsCurrentWordMisspelled)
                return;

            // If this method was called by external code,
            // the list of suggestions will not be populated yet.
            if (suggestionList.ItemsSource == null)
            {
                AttemptToShowSuggestions();
                return;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer == null)
                return;

            // Position the adorner beneath the misspelled word.
            int idx = FindBeginningOfCurrentWord();
            Rect rect = GetRectFromCharacterIndex(idx);
            adorner.SetOffsets(rect.Left, rect.Bottom);

            // Add the adorner into the adorner layer.
            layer.Add(adorner);

            // Since the ListBox might have a new set of items but has not 
            // rendered yet, we force it to calculate its metrics so that
            // the height animation has a sensible target value.
            suggestionList.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            suggestionList.Arrange(new Rect(new Point(), suggestionList.DesiredSize));

            // Animate the ListBox's height to the natural value.
            var anim = new DoubleAnimation
            {
                From = 0.0,
                To = suggestionList.ActualHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                FillBehavior = FillBehavior.Stop
            };
            suggestionList.BeginAnimation(HeightProperty, anim);

            areSuggestionsVisible = true;
        }

        #endregion // ShowSuggestions

        #region SuggestionListBoxStyle

        /// <summary>
        ///     Represents the SuggestionListBoxStyle property.  This field is read-only.
        /// </summary>
        public static readonly DependencyProperty SuggestionListBoxStyleProperty;

        /// <summary>
        ///     Gets/sets the Style applied to the ListBox which displays spelling suggestions.
        ///     This is a dependency property.
        /// </summary>
        public Style SuggestionListBoxStyle
        {
            get { return (Style) GetValue(SuggestionListBoxStyleProperty); }
            set { SetValue(SuggestionListBoxStyleProperty, value); }
        }

        private static void OnSuggestionListBoxStyleChanged(DependencyObject depObj,
            DependencyPropertyChangedEventArgs e)
        {
            var smartTextBox = depObj as SmartTextBox;
            if (smartTextBox != null)
            {
                smartTextBox.suggestionList.Style = e.NewValue as Style;
            }
        }

        #endregion // SuggestionListBoxStyle

        #endregion // Public Interface

        #region Base Class Overrides

        #region OnMouseDown

        /// <summary>
        ///     Hides the list of suggestions.
        /// </summary>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (AreSuggestionsVisible)
                HideSuggestions();
        }

        #endregion // OnMouseDown

        #region OnPreviewKeyDown

        /// <summary>
        ///     Shows/hides the list of suggestions.
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (AreSuggestionsVisible)
            {
                Debug.Assert(
                    !suggestionList.IsEnabled,
                    @"The SmartTextBox should only get key messages when the ListBox is visible 
					if there are no suggestions and the ListBox is disabled.");

                // There is a misspelled word but there are no suggestions.
                // Hide the list of suggestions and mark the event as handled.
                // Return without calling the base implementation so that the
                // keystroke is completely eaten.
                HideSuggestions();
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);

            if (e.Key == Key.F1)
            {
                Debug.Assert(!AreSuggestionsVisible, "Why is the suggestions list already visible?");

                AttemptToShowSuggestions();

                if (AreSuggestionsVisible)
                    suggestionList.SelectedIndex = 0;
            }
            else if (AreSuggestionsVisible)
            {
                HideSuggestions();
            }
        }

        #endregion // OnPreviewKeyDown

        #region OnRenderSizeChanged

        /// <summary>
        ///     Ensures that the list of suggestions is hidden when the TextBox is resized.
        /// </summary>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (AreSuggestionsVisible)
                HideSuggestions();
        }

        #endregion // OnRenderSizeChanged

        #region OnTextChanged

        /// <summary>
        ///     Hides the list of suggestions if a spelling error no longer exists at the
        ///     current caret location in the TextBox.
        /// </summary>
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (AreSuggestionsVisible)
                AttemptToHideSuggestions();
        }

        #endregion // OnTextChanged

        #endregion // Base Class Overrides

        #region Suggestion List Event Handlers

        #region IsKeyboardFocusWithinChanged

        private void suggestionList_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the list of suggestions no longer contains the input focus
            // hide the list.
            var focused = (bool) e.NewValue;
            if (!focused)
                HideSuggestions();
        }

        #endregion // IsKeyboardFocusWithinChanged

        #region ItemContainerGenerator.StatusChanged

        private void suggestionList_ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (AreSuggestionsVisible &&
                suggestionList.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                // The list of suggestions is visible and its ListBoxItems exist,
                // so give input focus to the first item in the list.
                var firstSuggestion = suggestionList.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                if (firstSuggestion != null)
                    firstSuggestion.Focus();
            }
        }

        #endregion // ItemContainerGenerator.StatusChanged

        #region MouseDoubleClick

        private void suggestionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // The user clicked on a suggestion, so apply it.
            ApplySelectedSuggestion();
        }

        #endregion // MouseDoubleClick

        #region PreviewKeyDown

        private void suggestionList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (suggestionList.SelectedIndex < 0)
                return;

            if (e.Key == Key.Escape)
            {
                HideSuggestions();
            }
            else if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab)
            {
                ApplySelectedSuggestion();

                // Mark the event as handled so that the keystroke
                // does not propogate to the TextBox.
                e.Handled = true;
            }
        }

        #endregion // PreviewKeyDown

        #endregion // Suggestion List Event Handlers

        #region Private Helpers

        #region ApplySelectedSuggestion

        private void ApplySelectedSuggestion()
        {
            if (!AreSuggestionsVisible || suggestionList.SelectedIndex < 0)
                return;

            SpellingError error = GetSpellingError();
            if (error != null)
            {
                var correctWord = suggestionList.SelectedItem as string;
                error.Correct(correctWord);
                CaretIndex = FindEndOfCurrentWord();
                Focus();
            }

            HideSuggestions();
        }

        #endregion // ApplySelectedSuggestion

        #region AttemptToShowSuggestions

        private void AttemptToShowSuggestions()
        {
            if (AreSuggestionsVisible)
                return;

            // If there is no spelling error, there is no
            // need to show the list of suggestions.
            SpellingError error = GetSpellingError();
            if (error == null)
                return;

            suggestionList.ItemsSource = error.Suggestions;

            if (suggestionList.Items.Count == 0)
            {
                // The spell check API has no suggested words
                // so display a message which says so.
                suggestionList.ItemsSource = noSuggestions;
                suggestionList.IsEnabled = false;
            }
            else
            {
                // In case the ListBox was disabled previously
                // we enable now.
                if (!suggestionList.IsEnabled)
                    suggestionList.IsEnabled = true;
            }

            ShowSuggestions();
        }

        #endregion // AttemptToShowSuggestions

        #region AttemptToHideSuggestions

        private void AttemptToHideSuggestions()
        {
            // If there is not still a spelling error at the
            // caret location, hide the suggestions.
            if (AreSuggestionsVisible && !IsCurrentWordMisspelled)
            {
                HideSuggestions();
            }
        }

        #endregion // AttemptToHideSuggestions

        #region FindBeginningOfCurrentWord

        private int FindBeginningOfCurrentWord()
        {
            if (Text == null)
                return -1;

            int idx = CaretIndex;
            while (idx > 0)
            {
                char prevChar = Text[idx - 1];
                if (char.IsWhiteSpace(prevChar) || char.IsPunctuation(prevChar))
                    break;

                --idx;
            }
            return idx;
        }

        #endregion // FindBeginningOfCurrentWord

        #region FindClosestCharacterInCurrentWord

        private int FindClosestCharacterInCurrentWord()
        {
            if (Text == null)
                return -1;

            int idx = CaretIndex;
            if (idx > 0)
            {
                char prevChar = Text[idx - 1];
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

        private int FindEndOfCurrentWord()
        {
            if (Text == null)
                return -1;

            int targetIdx = CaretIndex;
            while (targetIdx < Text.Length)
            {
                char nextChar = Text[targetIdx];
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