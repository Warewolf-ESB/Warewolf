using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Grids.Primitives
{
    #region DateFilterTreeView

    /// <summary>
    /// A derived <see cref="TreeView"/> control to bring the control under the Infragistics namespace
    /// </summary>
    /// <remarks>This is a derived control to make generic.xaml cleaner.</remarks>    
    [DesignTimeVisible(false)]
    public class DateFilterTreeView : TreeView
    {
        ///<summary>
        /// Creates a System.Windows.Controls.TreeViewItem to display content.
        ///</summary>
        ///
        /// <returns>A System.Windows.Controls.TreeViewItem to use as a container for content.
        /// </returns>        
        protected override DependencyObject GetContainerForItemOverride()
        {
            var obj = new DateFilterTreeViewItem();
            obj.SetBinding(TreeViewItem.IsExpandedProperty, new Binding("IsExpanded") { Mode = BindingMode.TwoWay });
            return obj;
        }

        /// <summary>Prepares the container element to display the specified item.
        /// </summary>
        /// <param name="element">The container element used to display the specified item.</param>
        /// <param name="item">The item to display.</param>        
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
        }
    }


    #endregion // DateFilterTreeView

    #region DateFilterTreeViewItem

    /// <summary>
    /// A derived <see cref="System.Windows.Controls.TreeViewItem"/>.
    /// </summary>
    [DesignTimeVisible(false)]
    public class DateFilterTreeViewItem : TreeViewItem
    {


        /// <summary>
        /// Static constructor for the <see cref="DateFilterTreeViewItem"/> class.
        /// </summary>
        static DateFilterTreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateFilterTreeViewItem), new FrameworkPropertyMetadata(typeof(DateFilterTreeViewItem)));
        }


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DateFilterTreeViewItem"/> class.
        /// </summary>
        public DateFilterTreeViewItem()
        {



        }
        #endregion // Constructor

        ///<summary>
        /// Creates a <see cref="System.Windows.Controls.TreeViewItem"/> to display content.
        ///</summary>
        ///
        /// <returns>A <see cref="System.Windows.Controls.TreeViewItem"/> to use as a container for content.
        /// </returns>    
        protected override DependencyObject GetContainerForItemOverride()
        {
            var obj = new DateFilterTreeViewItem();
            obj.SetBinding(TreeViewItem.IsExpandedProperty, new Binding("IsExpanded") { Mode = BindingMode.TwoWay });
            return obj;
        }

        /// <summary>
        /// Builds the visual tree for the <see cref="DateFilterTreeViewItem"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ToggleButton tb = this.GetTemplateChild("ExpanderButton") as ToggleButton;
            if (tb != null)
            {
                Binding b = new Binding("IsExpanded");
                b.Source = this;
                b.Mode = BindingMode.TwoWay;
                tb.SetBinding(ToggleButton.IsCheckedProperty, b);
            }
        }

    }

    #endregion // DateFilterTreeViewItem

    #region DateFilterHierarchicalDataTemplate

    /// <summary>
    /// A derived <see cref="System.Windows.HierarchicalDataTemplate"/> control to bring the control under the Infragistics namespace
    /// </summary>
    /// <remarks>This is a derived control to make generic.xaml cleaner.</remarks>
    public class DateFilterHierarchicalDataTemplate : System.Windows.HierarchicalDataTemplate
    {
    }

    #endregion // DateFilterHierarchicalDataTemplate

    #region FilterTextBox
    /// <summary>
    /// An inherited <see cref="TextBox"/> which acts as a command target.
    /// </summary>
    [DesignTimeVisible(false)]
    public class FilterTextBox : TextBox, ICommandTarget
    {
        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion // ICommandTarget Members

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param propertyName="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return command is FilterMenuFilterTextBoxCommand;
        }

        #endregion // SupportsCommand

        #region GetParameter

        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            return this;
        }

        #endregion // GetParameter
    }
    #endregion // FilterTextBox

    #region FilterTextBoxWatermarked
    /// <summary>
    /// A control that hosts a FilterTextBox and makes it appear watermarked.
    /// </summary>
    [TemplateVisualState(GroupName = "WaterMarks", Name = "ShowWaterMark")]
    [TemplateVisualState(GroupName = "WaterMarks", Name = "HideWaterMark")]
    [TemplateVisualState(GroupName = "ClearButtonStates", Name = "ShowClearButton")]
    [TemplateVisualState(GroupName = "ClearButtonStates", Name = "HideClearButton")]
    [TemplatePart(Name = "FilterTextBox", Type = typeof(FilterTextBox))]
    [DesignTimeVisible(false)]
    public class FilterTextBoxWatermarked : Control, INotifyPropertyChanged, ICommandTarget
    {
        #region Members
        FilterTextBox _filterTextBox;
        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="FilterTextBoxWatermarked"/> class.
        /// </summary>
        static FilterTextBoxWatermarked()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterTextBoxWatermarked), new FrameworkPropertyMetadata(typeof(FilterTextBoxWatermarked)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FilterTextBoxWatermarked"/> class.
        /// </summary>
        public FilterTextBoxWatermarked()
        {



            this.Loaded += new RoutedEventHandler(FilterTextBoxWatermarked_Loaded);

            this.Unloaded += FilterTextBoxWatermarked_Unloaded;
            this.Focusable = false;            

            this.IsTabStop = false;
        }

        #endregion // Constructor

        #region Overrides

        #region OnApplyTemplate
        /// <summary>
        ///     When overridden in a derived class, is invoked whenever application code
        ///     or internal processes (such as a rebuilding layout pass) call System.Windows.Controls.Control.ApplyTemplate().
        ///     In simplest terms, this means the method is called just before a UI element
        ///     displays in an application. For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._filterTextBox != null)
            {
                this._filterTextBox.GotFocus -= FilterTextBox_GotFocus;
                this._filterTextBox.LostFocus -= FilterTextBox_LostFocus;
            }

            this._filterTextBox = this.GetTemplateChild("FilterTextBox") as FilterTextBox;


            if (this._filterTextBox != null)
            {
                this._filterTextBox.TextChanged += new TextChangedEventHandler(FilterTextBox_TextChanged);

                this._filterTextBox.GotFocus += FilterTextBox_GotFocus;
                this._filterTextBox.LostFocus += FilterTextBox_LostFocus;

            }

        }

        #endregion // OnApplyTemplate

        #region OnLostFocus
        /// <summary>
        /// Called before the System.Windows.UIElement.LostFocus event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            this.EnsureVisualState();
        }
        #endregion // OnLostFocus

        #region OnGotFocus
        /// <summary>
        /// Called before the System.Windows.UIElement.GotFocus event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            VisualStateManager.GoToState(this, "HideWaterMark", false);
        }
        #endregion // OnGotFocus

        #endregion // Overrides


        #region Event Handlers

        void FilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.EnsureVisualState();
        }

        void FilterTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "HideWaterMark", false);
        }

        void FilterTextBoxWatermarked_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this._filterTextBox != null)
            {
                this._filterTextBox.GotFocus -= FilterTextBox_GotFocus;
                this._filterTextBox.LostFocus -= FilterTextBox_LostFocus;
            }
        }
        #endregion // Event Handlers


        #region Properties

        #region FilterSelectionControl

        /// <summary>
        /// Identifies the <see cref="FilterSelectionControl"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty FilterSelectionControlProperty = DependencyProperty.Register("FilterSelectionControl", typeof(FilterSelectionControl), typeof(FilterTextBoxWatermarked), new PropertyMetadata(new PropertyChangedCallback(FilterSelectionControlChanged)));

        /// <summary>
        /// Gets / sets the <see cref="FilterSelectionControl"/> associated with this control.
        /// </summary>
        public FilterSelectionControl FilterSelectionControl
        {
            get { return (FilterSelectionControl)this.GetValue(FilterSelectionControlProperty); }
            set { this.SetValue(FilterSelectionControlProperty, value); }
        }

        private static void FilterSelectionControlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterTextBoxWatermarked ctrl = (FilterTextBoxWatermarked)obj;
            ctrl.OnPropertyChanged("FilterSelectionControl");
        }

        #endregion // FilterSelectionControl

        #region Watermark

        /// <summary>
        /// Identifies the <see cref="Watermark"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(string), typeof(FilterTextBoxWatermarked), new PropertyMetadata(SRGrid.GetString("Watermark"), new PropertyChangedCallback(WatermarkChanged)));

        /// <summary>
        /// Gets / sets the string that will be displayed as the watermark.
        /// </summary>
        public string Watermark
        {
            get { return (string)this.GetValue(WatermarkProperty); }
            set { this.SetValue(WatermarkProperty, value); }
        }

        private static void WatermarkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterTextBoxWatermarked ctrl = (FilterTextBoxWatermarked)obj;
            ctrl.OnPropertyChanged("Watermark");
        }

        #endregion // Watermark

        #endregion // Properties

        #region Methods
        /// <summary>
        /// Ensures that <see cref="FilterTextBoxWatermarked"/> is in the correct state.
        /// </summary>
        protected virtual void EnsureVisualState()
        {
            if (this._filterTextBox != null)
            {
                if (this._filterTextBox.Text.Length == 0)
                {
                    VisualStateManager.GoToState(this, "ShowWaterMark", false);
                    VisualStateManager.GoToState(this, "HideClearButton", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "HideWaterMark", false);
                    VisualStateManager.GoToState(this, "ShowClearButton", false);
                }
            }
            this.EnsureClearButtonState();
        }

        private void EnsureClearButtonState()
        {
            if (this._filterTextBox != null)
            {
                if (this._filterTextBox.Text.Length == 0)
                {
                    VisualStateManager.GoToState(this, "HideClearButton", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "ShowClearButton", false);
                }
            }
        }

        #endregion // Methods

        #region EventHandlers

        void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Fixes and issue with the state when the menu is closed with text writen in the searchbox
            
            this.EnsureVisualState();
        }

        void FilterTextBoxWatermarked_Loaded(object sender, RoutedEventArgs e)
        {

            if (this._filterTextBox != null)
            {
                this._filterTextBox.GotFocus -= FilterTextBox_GotFocus;
                this._filterTextBox.LostFocus -= FilterTextBox_LostFocus;

                this._filterTextBox.GotFocus += FilterTextBox_GotFocus;
                this._filterTextBox.LostFocus += FilterTextBox_LostFocus;
            }

            this.EnsureVisualState();
        }

        #endregion // EventHandlers

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="XamGrid"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="XamGrid"/> object.
        /// </summary>
        /// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion // ICommandTarget Members

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param propertyName="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return command is FilterMenuFilterTextBoxCommand;
        }

        #endregion // SupportsCommand

        #region GetParameter

        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            return this;
        }

        #endregion // GetParameter
    }

    #endregion // FilterTextBoxWatermarked

    #region DateFilterTypeConverter
    /// <summary>
    /// A value converter which will take the enum value for the DateFilterType and return the assocatied string.
    /// </summary>
    public class DateFilterTypeConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Takes a <see cref="DateFilterListDisplayObject"/>, and returns the <see cref="DateFilterListDisplayObject.DateFilterObjectType"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                DateFilterListDisplayObject objectType = (DateFilterListDisplayObject)value;

                switch (objectType.DateFilterObjectType)
                {
                    case (DateFilterObjectType.All):
                    case (DateFilterObjectType.None):
                        {
                            return SRGrid.GetString("FilterWatermark_All");
                        }
                    case (DateFilterObjectType.Year):
                        {
                            return SRGrid.GetString("FilterWatermark_Year");
                        }
                    case (DateFilterObjectType.Month):
                        {
                            return SRGrid.GetString("FilterWatermark_Month");
                        }
                    case (DateFilterObjectType.Date):
                        {
                            return SRGrid.GetString("FilterWatermark_Date");
                        }
                    case (DateFilterObjectType.Hour):
                        {
                            return SRGrid.GetString("FilterWatermark_Hour");
                        }
                    case (DateFilterObjectType.Minute):
                        {
                            return SRGrid.GetString("FilterWatermark_Minute");
                        }
                    case (DateFilterObjectType.Second):
                        {
                            return SRGrid.GetString("FilterWatermark_Second");
                        }
                }
            }
            return SRGrid.GetString("FilterWatermark_All");
        }

        /// <summary>
        /// This is not currently used.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
    #endregion // DateFilterTypeConverter

    #region CustomFilterDialogContentControl
    /// <summary>
    /// A derived <see cref="ContentControl"/> used by the <see cref="ColumnFilterDialogControl"/>.
    /// </summary>
    [DesignTimeVisible(false)]
    public class CustomFilterDialogContentControl : ContentControl
    {

        /// <summary>
        /// Static constructor for the <see cref="CustomFilterDialogContentControl"/> class.
        /// </summary>
        static CustomFilterDialogContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomFilterDialogContentControl), new FrameworkPropertyMetadata(typeof(CustomFilterDialogContentControl)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFilterDialogContentControl"/> class.
        /// </summary>
        public CustomFilterDialogContentControl()
        {



        }
    }
    #endregion // CustomFilterDialogContentControl
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved