using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Warewolf.Studio.CustomControls
{
    public class HighlightTextBlock : TextBlock
    {
        #region Member Variables

        private DispatcherOperation _pendingUpdate;

        #endregion //Member Variables

        #region Constructor
        static HighlightTextBlock()
        {
        }

        #endregion //Constructor

        #region Base class overrides

        #region OnInitialized
        protected override void OnInitialized(EventArgs e)
        {
            if (_pendingUpdate != null)
                UpdateInlines(null);

            base.OnInitialized(e);
        }
        #endregion //OnInitialized

        #endregion //Base class overrides

        #region Properties

        #region FilterText

        /// <summary>
        /// Identifies the <see cref="FilterText"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText",
            typeof(string), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(null, OnCriteriaChanged));

        /// <summary>
        /// Returns or sets the text that should be highlighted
        /// </summary>
        /// <seealso cref="FilterTextProperty"/>
        [Description("Returns or sets the text that should be highlighted")]
        [Category("Behavior")]
        [Bindable(true)]
        public string FilterText
        {
            get
            {
                return (string)GetValue(FilterTextProperty);
            }
            set
            {
                SetValue(FilterTextProperty, value);
            }
        }

        #endregion //FilterText

        #region FilterTextBackground

        /// <summary>
        /// Identifies the <see cref="FilterTextBackground"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterTextBackgroundProperty = DependencyProperty.Register("FilterTextBackground",
            typeof(Brush), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(Brushes.Yellow, OnCriteriaChanged));

        /// <summary>
        /// Returns or sets the background of the matching text.
        /// </summary>
        /// <seealso cref="FilterTextBackgroundProperty"/>
        [Description("Returns or sets the background of the matching text.")]
        [Category("Behavior")]
        [Bindable(true)]
        public Brush FilterTextBackground
        {
            get
            {
                return (Brush)GetValue(FilterTextBackgroundProperty);
            }
            set
            {
                SetValue(FilterTextBackgroundProperty, value);
            }
        }

        #endregion //FilterTextBackground

        #region FilterTextComparisonType

        /// <summary>
        /// Identifies the <see cref="FilterTextComparisonType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterTextComparisonTypeProperty = DependencyProperty.Register("FilterTextComparisonType",
            typeof(StringComparison), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(StringComparison.CurrentCultureIgnoreCase, OnCriteriaChanged));

        /// <summary>
        /// Returns or sets the StringComparison when locating the FilterText within the RawText.
        /// </summary>
        /// <seealso cref="FilterTextComparisonTypeProperty"/>
        [Description("Returns or sets the StringComparison when locating the FilterText within the RawText.")]
        [Category("Behavior")]
        [Bindable(true)]
        public StringComparison FilterTextComparisonType
        {
            get
            {
                return (StringComparison)GetValue(FilterTextComparisonTypeProperty);
            }
            set
            {
                SetValue(FilterTextComparisonTypeProperty, value);
            }
        }

        #endregion //FilterTextComparisonType

        #region FilterTextForeground

        /// <summary>
        /// Identifies the <see cref="FilterTextForeground"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterTextForegroundProperty = DependencyProperty.Register("FilterTextForeground",
            typeof(Brush), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(Brushes.Black, OnCriteriaChanged));

        /// <summary>
        /// Returns or sets the brushed used for the foreground of the matching text.
        /// </summary>
        /// <seealso cref="FilterTextForegroundProperty"/>
        [Description("Returns or sets the brushed used for the foreground of the matching text.")]
        [Category("Behavior")]
        [Bindable(true)]
        public Brush FilterTextForeground
        {
            get
            {
                return (Brush)GetValue(FilterTextForegroundProperty);
            }
            set
            {
                SetValue(FilterTextForegroundProperty, value);
            }
        }

        #endregion //FilterTextForeground

        #region RawText

        /// <summary>
        /// Identifies the <see cref="RawText"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RawTextProperty = DependencyProperty.Register("RawText",
            typeof(string), typeof(HighlightTextBlock), new FrameworkPropertyMetadata(null, OnCriteriaChanged));

        /// <summary>
        /// Returns or sets the base string that will be displayed by the element.
        /// </summary>
        /// <seealso cref="RawTextProperty"/>
        [Description("Returns or sets the base string that will be displayed by the element.")]
        [Category("Behavior")]
        [Bindable(true)]
        public string RawText
        {
            get
            {
                return (string)GetValue(RawTextProperty);
            }
            set
            {
                SetValue(RawTextProperty, value);
            }
        }

        #endregion //RawText

        #endregion //Properties

        #region Methods

        #region OnCriteriaChanged
        private static void OnCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as HighlightTextBlock;

            if (instance != null && instance._pendingUpdate == null)
            {
                instance._pendingUpdate = instance.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SendOrPostCallback(instance.UpdateInlines), new object[] { null });
            }
        }
        #endregion //OnCriteriaChanged

        #region UpdateInlines
        private void UpdateInlines(object param)
        {
            _pendingUpdate = null;

            string filterText = FilterText;
            string text = RawText;
            var inlines = Inlines;

            try
            {
                inlines.Clear();

                if (string.IsNullOrEmpty(filterText))
                {
                    inlines.Add(text);
                    return;
                }

                var foreground = FilterTextForeground;
                var background = FilterTextBackground;
                var comparison = FilterTextComparisonType;
                var newInlines = new List<Inline>();
                int filterTextLen = filterText.Length;

                int start = 0;

                do
                {
                    int end = text.IndexOf(filterText, start, comparison);

                    string substr = text.Substring(start, (end < 0 ? text.Length : end) - start);
                    newInlines.Add(new Run(substr));

                    if (end < 0)
                        break;

                    var run = new Run(text.Substring(end, filterTextLen));

                    // note we could bind and not rebuild when the background/foreground 
                    // changes but that doesn't seem likely to happen and would add more 
                    // overhead than just referencing the value directly
                    if (null != foreground)
                        run.Foreground = foreground;

                    if (null != background)
                        run.Background = background;

                    newInlines.Add(run);

                    start = end + filterTextLen;
                } while (true);

                inlines.AddRange(newInlines);
            }
            finally
            {
                if (_pendingUpdate != null)
                {
                    _pendingUpdate.Abort();
                    _pendingUpdate = null;
                }
            }
        }
        #endregion //UpdateInlines

        #endregion //Methods
    }
}
