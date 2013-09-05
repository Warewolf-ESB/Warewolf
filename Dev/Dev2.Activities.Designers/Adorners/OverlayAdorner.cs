using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Dev2.Activities.Annotations;
using Dev2.Activities.Designers;
using Dev2.Activities.QuickVariableInput;
using Dev2.CustomControls.Behavior;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.UI;
using Dev2.Util.ExtensionMethods;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// The adorner used to host the overlay content
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public sealed class OverlayAdorner : AbstractOverlayAdorner, INotifyPropertyChanged
    {
        #region fields

        private VisualCollection _visuals;
        private Border _contentBorder;
        private ContentPresenter _contentPresenter;
        private HelpViewModel _helpContent;
        private OverlayTemplate _uc;

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayAdorner"/> class.
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
            CreateContentContainer(colourBorder);
            FocusManager.SetIsFocusScope(this, true);
            element.DataContextChanged += OnElementOnDataContextChanged;
            HelpContent = new HelpViewModel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverlayAdorner"/> class.
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
        /// Gets or sets the content being displayed by this adorner
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override object Content
        {
            get
            {
                return _contentPresenter.Content;
            }
            protected set
            {
                _contentPresenter.Content = value;
            }
        }

        public HelpViewModel HelpContent
        {
            get
            {
                return _helpContent;
            }
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

        public override string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string),
            typeof(OverlayAdorner), new PropertyMetadata(string.Empty, HelpTextChangedCallback));


        private static void HelpTextChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var adorner = (OverlayAdorner)o;
            var newText = (string)args.NewValue;
            adorner.HelpContent.HelpText = newText;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Hides the content of the adorner.
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
        /// Shows the content of the adorner.
        /// </summary>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ShowContent()
        {
            _uc.EnsureHelpVisibility();

            // Set initial content height/width - DO NOT REMOVE!!!!
            //
            // So that text boxes with Vertical/Horizontal Alignment = Stretch 
            // do not auto grow when user enters new lines but show scrollbars
            UpdateContentSize();

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
        /// Changes the content of the adorner, and makes it visible.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="contentAutomationID">The content automation ID.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override void ChangeContent(object content, string contentAutomationID)
        {
            if(!(content is ActivityTemplate))
            {
                throw new Exception("The user control templates for activities needs to inherit from ActivityTemplate! Please inherit from ActivityTemplate");
            }

            var collectionActivityTemplate = content as CollectionActivityTemplate;
            if(collectionActivityTemplate != null)
            {
                collectionActivityTemplate.Loaded += (sender, args) =>
                    {
                        var template = collectionActivityTemplate;
                        var itemsControl = template.ItemsControl;
                        if(itemsControl == null)
                        {
                            throw new Exception(
                                "The user control templates for collection activities needs to contain an itemscontrol for representing the collection");
                        }
                        itemsControl.SetValue(DataGrid.CanUserResizeColumnsProperty, true);
                        var inputElements = itemsControl.FindVisualChildren<IntellisenseTextBox>();
                        inputElements.ToList().ForEach(i => i.SetValue(MaxHeightProperty, 48D));
                        var sizeSyncBehavior = new DataGridColumnSizeSynchronizationBehavior();
                        var focusBehavior = new DataGridFocusTextOnLoadBehavior();
                        Interaction.GetBehaviors(itemsControl).Add(sizeSyncBehavior);
                        Interaction.GetBehaviors(itemsControl).Add(focusBehavior);
                    };
            }

            Content = content;
            var uiElement = content as FrameworkElement;
            uiElement.AllowDrop = true;
            uiElement.SetValue(AutomationProperties.AutomationIdProperty, contentAutomationID);

            if(uiElement.DataContext == null)
            {
                uiElement.DataContext = DataContext;
            }

            Keyboard.Focus(uiElement);

            AddToButtonsContainer((ActivityTemplate)content);
            ShowContent();
        }

        #endregion

        #region protected overrides

        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Size" /> object representing the amount of layout space needed by the adorner.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Size MeasureOverride(Size constraint)
        {
            _contentBorder.Measure(constraint);
            return _contentBorder.DesiredSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>
        /// The actual size used.
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
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
        /// </returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>The number of visual child elements for this element.</returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        protected override int VisualChildrenCount
        {
            get
            {
                return _visuals.Count;
            }
        }

        #endregion

        #region event handlers

        private void OnElementOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            DataContext = args.NewValue;
        }

        #endregion

        #region private helpers

        private void AddToButtonsContainer(ActivityTemplate activityTemplate)
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

        private void CreateContentContainer(Border colourBorder)
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
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public override void UpdateContentSize()
        {
            _uc.UpdateContentSize();
        }
    }
}
