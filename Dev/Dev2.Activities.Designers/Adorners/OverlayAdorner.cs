using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Activities.Annotations;
using Dev2.Activities.Designers;
using Dev2.Activities.QuickVariableInput;
using Dev2.Providers.Errors;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Util.ExtensionMethods;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    ///     The adorner used to host the overlay content
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public sealed class OverlayAdorner : AbstractOverlayAdorner, INotifyPropertyChanged
    {
        #region fields

        Border _contentBorder;
        ContentPresenter _contentPresenter;
        HelpViewModel _helpContent;
        OverlayTemplate _uc;
        VisualCollection _visuals;

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayAdorner" /> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="colourBorder">The colour border.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public OverlayAdorner(UIElement adornedElement, Border colourBorder)
            : base(adornedElement)
        {
            var element = adornedElement as Grid;
            if(element == null)
            {
                return;
            }
            DataContext = element.DataContext;
            HelpContent = new HelpViewModel();

            var avm = DataContext as IActivityViewModelBase;
            if(avm != null)
            {
                avm.HelpViewModel = HelpContent;
            }

            CreateContentContainer(colourBorder);
            FocusManager.SetIsFocusScope(this, true);
            element.DataContextChanged += OnElementOnDataContextChanged;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayAdorner" /> class.
        /// </summary>
        /// <param name="adornedElement">The adorned element.</param>
        /// <param name="content">The content.</param>
        /// <param name="colourBorder">The colour border.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public OverlayAdorner(UIElement adornedElement, Visual content, Border colourBorder)
            : this(adornedElement, colourBorder)
        {
            Content = content;
        }

        #endregion ctor

        #region public properties

        /// <summary>
        ///     Gets or sets the content being displayed by this adorner
        /// </summary>
        /// <value>
        ///     The content.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override object Content { get { return _contentPresenter.Content; } protected set { _contentPresenter.Content = value; } }

        public HelpViewModel HelpContent
        {
            get { return _helpContent; }
            set
            {
                if(_helpContent == value)
                {
                    return;
                }

                _helpContent = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region dependency properties

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string),
                typeof(OverlayAdorner), new PropertyMetadata(string.Empty, HelpTextChangedCallback));

        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(List<IActionableErrorInfo>),
                typeof(OverlayAdorner), new PropertyMetadata(new List<IActionableErrorInfo>(), ErrorsChangedCallBack));
        public override string HelpText { get { return (string)GetValue(HelpTextProperty); } set { SetValue(HelpTextProperty, value); } }
        public override List<IActionableErrorInfo> Errors { get { return (List<IActionableErrorInfo>)GetValue(ErrorsProperty); } set { SetValue(ErrorsProperty, value); } }

        static void HelpTextChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var adorner = (OverlayAdorner)o;
            var newText = (string)args.NewValue;
            adorner.HelpContent.HelpText = newText;
        }

        static void ErrorsChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var adorner = (OverlayAdorner)d;
            var newValue = e.NewValue as List<IActionableErrorInfo>;
            if(newValue == null)
            {
                return;
            }
            adorner.HelpContent.Errors = newValue;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Hides the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void HideContent()
        {
            _contentBorder.Visibility = Visibility.Collapsed;
            _contentBorder.BringToFront();
        }

        /// <summary>
        ///     Shows the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ShowContent()
        {
            _contentBorder.Visibility = Visibility.Visible;
            _contentBorder.DataContext = DataContext;
        }

        public override void BringToFront()
        {
            var children = _contentBorder.FindVisualChildren<FrameworkElement>();
            children.ToList().ForEach(c => c.BringToMaxFront());
            _contentBorder.BringToFront();
            this.BringToMaxFront();
        }

        public override void SendtoBack()
        {
            var children = _contentBorder.FindVisualChildren<FrameworkElement>();
            children.ToList().ForEach(c => c.SendToBack());
            _contentBorder.SendToBack();
            this.SendToBack();
        }

        /// <summary>
        ///     Changes the content of the adorner, and makes it visible.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentAutomationID">The content automation ID.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ChangeContent(object content, string contentAutomationID)
        {
            var contentActivityTemplate = content as ActivityTemplate;
            if(contentActivityTemplate == null)
            {
                throw new Exception("The user control templates for activities needs to inherit from ActivityTemplate! Please inherit from ActivityTemplate");
            }

            contentActivityTemplate.Loaded += (sender, args) =>
            {
                var element = content as FrameworkElement;
                var txt = element.FindVisualChildren<TextBox>().FirstOrDefault();
                if(txt != null)
                {
                    txt.Focus();
                }
            };

            Content = content;
            var uiElement = content as FrameworkElement;
            uiElement.AllowDrop = true;
            uiElement.SetValue(AutomationProperties.AutomationIdProperty, contentAutomationID);

            if(uiElement.DataContext == null)
            {
                uiElement.DataContext = DataContext;
            }

            AddToButtonsContainer((ActivityTemplate)content);
            ShowContent();
        }

       

        public override void UpdateContentSize()
        {
             
        }

        #endregion

        #region protected overrides

        /// <summary>
        ///     Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override int VisualChildrenCount { get { return _visuals.Count; } }

        /// <summary>
        ///     Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        ///     A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Size MeasureOverride(Size constraint)
        {
            _contentBorder.Measure(constraint);
            return _contentBorder.DesiredSize;
        }

        /// <summary>
        ///     When overridden in a derived class, positions child elements and determines a size for a
        ///     <see
        ///         cref="T:System.Windows.FrameworkElement" />
        ///     derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        ///     The actual size used.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentBorder.Arrange(new Rect(0, 22,
                finalSize.Width, finalSize.Height));
            return _contentBorder.RenderSize;
        }

        /// <summary>
        ///     Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        ///     The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        #endregion

        #region event handlers

        void OnElementOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            DataContext = args.NewValue;
        }

        #endregion

        #region private helpers

        void AddToButtonsContainer(ActivityTemplate activityTemplate)
        {
            _uc.LeftButtons.ItemsSource = activityTemplate.LeftButtons;
            _uc.RightButtons.ItemsSource = activityTemplate.RightButtons;

            if(activityTemplate is QuickVariableInputView)
            {
                var variableViewModel = DataContext as ActivityCollectionViewModelBase<ActivityDTO>;
                if(variableViewModel != null)
                {
                    _uc.ButtonsContainer.DataContext = variableViewModel.QuickVariableInputViewModel;
                }
            }
            else
            {
                _uc.ButtonsContainer.DataContext = DataContext;
            }
        }

        void CreateContentContainer(Border colourBorder)
        {
            _visuals = new VisualCollection(this);

            _uc = new OverlayTemplate(colourBorder, this, (ActivityViewModelBase)DataContext);
            _contentBorder = _uc.OuterBorder;
            _contentPresenter = _uc.ContentPresenter;

            _visuals.Add(_uc.OuterBorder);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}