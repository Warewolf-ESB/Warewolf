using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Collections;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// A control used to display all the <see cref="Column"/> objects of a particular <see cref="ColumnLayout"/> in the <see cref="XamGrid"/> that allows
    /// and end user to toggle the Visibility of each Column.
    /// </summary>
    public class ColumnChooserDialog : Control, ICommandTarget, INotifyPropertyChanged
    {
        #region Members

        Popup _popup;
        FrameworkElement _header;
        bool _isMouseDown, _isDragging;
        Point _offsetPoint;
        FrameworkElement _rootElement;

        #endregion // Members

        #region Constructor

        

        /// <summary>
        /// Static constructor for the <see cref="ColumnChooserDialog"/> class.
        /// </summary>
        static ColumnChooserDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColumnChooserDialog), new FrameworkPropertyMetadata(typeof(ColumnChooserDialog)));
        }

        

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnChooserDialog"/> class.
        /// </summary>
        public ColumnChooserDialog()
        {



            this.ViewLabelText = SRGrid.GetString("ColumnChooserDialog_ViewLabel");
            this.ColumnLabelText = SRGrid.GetString("ColumnChooserDialog_ColumnLabel");
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region IsOpen

        /// <summary>
        /// Identifies the <see cref="IsOpen"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ColumnChooserDialog), new PropertyMetadata(new PropertyChangedCallback(IsOpenChanged)));

        /// <summary>
        /// Gets/Sets whether the <see cref="ColumnChooserDialog"/> is open. 
        /// </summary>
        /// <remarks>
        /// When the Dialog is opened, it will center itself to the <see cref="XamGrid"/>
        /// </remarks>
        public bool IsOpen
        {
            get { return (bool)this.GetValue(IsOpenProperty); }
            set { this.SetValue(IsOpenProperty, value); }
        }

        private static void IsOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserDialog dialog = (ColumnChooserDialog)obj;

            if (dialog._popup != null )
            {
                dialog._popup.IsOpen = dialog.IsOpen;

                if(dialog.IsOpen)
                    dialog.IsSortable = dialog.ColumnLayout.Grid.ColumnChooserSettings.AllowColumnMoving;

                dialog.EnsureLocation();              
            }
        }


        #endregion // IsOpen 

        #region ColumnLayout

        /// <summary>
        /// Identifies the <see cref="ColumnLayout"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColumnLayoutProperty = DependencyProperty.Register("ColumnLayout", typeof(ColumnLayout), typeof(ColumnChooserDialog), new PropertyMetadata(new PropertyChangedCallback(ColumnLayoutChanged)));

        /// <summary>
        /// Gets/Sets the current <see cref="ColumnLayout"/> that the <see cref="ColumnChooserDialog"/> is representing.
        /// </summary>
        public ColumnLayout ColumnLayout
        {
            get { return (ColumnLayout)this.GetValue(ColumnLayoutProperty); }
            set { this.SetValue(ColumnLayoutProperty, value); }
        }

        private static void ColumnLayoutChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserDialog dialog = (ColumnChooserDialog)obj;

            if (e.NewValue != null)
            {
                if (dialog.ColumnLayout.Grid != null)
                {
                    Style s = dialog.ColumnLayout.Grid.ColumnChooserSettings.Style;
                    ColumnContentProviderBase.SetControlStyle(dialog, s);
                }
            }
            dialog.OnPropertyChanged("Columns");
        }

        #endregion // ColumnLayout 

        #region Column

        /// <summary>
        /// Identifies the <see cref="Column"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register("Column", typeof(Column), typeof(ColumnChooserDialog), new PropertyMetadata(new PropertyChangedCallback(ColumnChanged)));

        /// <summary>
        /// The Column, whose subcolumns should be displayed in the <see cref="ColumnChooserDialog"/>
        /// </summary>
        public Column Column
        {
            get { return (Column)this.GetValue(ColumnProperty); }
            set { this.SetValue(ColumnProperty, value); }
        }

        private static void ColumnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserDialog dialog = (ColumnChooserDialog)obj;
            if (e.NewValue != null)
            {
                if (dialog.Column.ColumnLayout.Grid != null)
                {
                    Style s = dialog.Column.ColumnLayout.Grid.ColumnChooserSettings.Style;
                    ColumnContentProviderBase.SetControlStyle(dialog, s);
                }
            }

            dialog.OnPropertyChanged("Columns");
        }

        #endregion // Column 
				

        #region Columns

        /// <summary>
        /// Gets a list of <see cref="ColumnBase"/> objects that should be displayed in the <see cref="ColumnChooserDialog"/>
        /// </summary>
        public IList Columns
        {
            get
            {
                IList columns = null;

                if (this.Column != null)
                {
                    columns = this.Column.ResolveChildColumns();
                }
                else if (this.ColumnLayout != null)
                {
                    columns = this.ColumnLayout.Columns;
                }

                return columns;
            }
        }

        #endregion // Columns

        #region ViewLabelText

        /// <summary>
        /// Identifies the <see cref="ViewLabelText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ViewLabelTextProperty = DependencyProperty.Register("ViewLabelText", typeof(string), typeof(ColumnChooserDialog), new PropertyMetadata( new PropertyChangedCallback(ViewLabelTextChanged)));

        /// <summary>
        /// Gets/Sets the Text that is displayed for the view header of the ColumnChooserDialog.
        /// </summary>
        public string ViewLabelText
        {
            get { return (string)this.GetValue(ViewLabelTextProperty); }
            set { this.SetValue(ViewLabelTextProperty, value); }
        }

        private static void ViewLabelTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserDialog ccd = (ColumnChooserDialog)obj;
            ccd.OnPropertyChanged("ViewLabelText");
        }

        #endregion // ViewLabelText 
		
        #region ColumnLabelText

        /// <summary>
        /// Identifies the <see cref="ColumnLabelText"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColumnLabelTextProperty = DependencyProperty.Register("ColumnLabelText", typeof(string), typeof(ColumnChooserDialog), new PropertyMetadata(new PropertyChangedCallback(ColumnLabelTextChanged)));

        /// <summary>
        /// Gets/Sets the Text that is displayed for the column name header of the ColumnChooserDialog.
        /// </summary>
        public string ColumnLabelText
        {
            get { return (string)this.GetValue(ColumnLabelTextProperty); }
            set { this.SetValue(ColumnLabelTextProperty, value); }
        }

        private static void ColumnLabelTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColumnChooserDialog ccd = (ColumnChooserDialog)obj;
            ccd.OnPropertyChanged("ColumnLabelText");
        }

        #endregion // ColumnLabelText 

        #region IsSortable

        /// <summary>
        /// Identifies the <see cref="IsSortable"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsSortableProperty = DependencyProperty.Register("IsSortable", typeof(bool), typeof(ColumnChooserDialog), new PropertyMetadata(new PropertyChangedCallback(IsSortableChanged)));

        /// <summary>
        /// Gets/sets whether columns within the <see cref="ColumnChooserDialog"/> should be movable.
        /// </summary>
        public bool IsSortable
        {
            get { return (bool)this.GetValue(IsSortableProperty); }
            set { this.SetValue(IsSortableProperty, value); }
        }

        private static void IsSortableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // IsSortable 				
				
        #endregion // Public

        #region Internal

        internal Point? InitialLocation
        {
            get;
            set;
        }

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Protected

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param propertyName="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected virtual bool SupportsCommand(ICommand command)
        {
            return (command is ColumnChooserCommandBase);
        }
        #endregion // SupportsCommand

        #region  GetParameter
        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected virtual object GetParameter(CommandSource source)
        {
            return this.ColumnLayout;
        }
        #endregion // GetParameter

        #region EndDrag
        /// <summary>
        /// Ends the current drag operation of the ColumnChooser
        /// </summary>
        protected virtual void EndDrag()
        {
            this._isMouseDown = false;

            if (this._isDragging)
            {
                this._isDragging = false;
                this._header.ReleaseMouseCapture();



#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

            }

            this._header.ReleaseMouseCapture();
        }
        #endregion // EndDrag

        #endregion // Protected

        #region Internal

        #region Invalidate

        internal void Invalidate()
        {
            if (this._popup != null && this._popup.IsOpen)
            {
                this._popup.IsOpen = false;
                this._popup.IsOpen = true;
            }
        }

        #endregion // Invalidate

        #endregion // Internal

        #region Private

        private void ToggleIsOpen()
        {
            this.IsOpen = false;
            this.IsOpen = true;
        }

        private void EnsureLocation()
        {
            if (this.InitialLocation == null)
            {
                if (this.IsOpen && this.ColumnLayout != null && this.ColumnLayout.Grid != null && this.ColumnLayout.Grid.Panel != null)
                {
                    double top = this.ColumnLayout.Grid.Panel.ActualHeight / 2;
                    double left = this.ColumnLayout.Grid.Panel.ActualWidth / 2;

                    if (this._rootElement != null)
                    {
                        this._rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        top -= (this._rootElement.DesiredSize.Height / 2);
                        left -= (this._rootElement.DesiredSize.Width / 2);
                    }

                    this._popup.HorizontalOffset = left;
                    this._popup.VerticalOffset = top;
                }
            }
            else
            {
                this._popup.HorizontalOffset = ((Point)this.InitialLocation).X;
                this._popup.VerticalOffset = ((Point)this.InitialLocation).Y;
            }
        }

        #endregion // Private

        #endregion // Methods

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Initializes Template parts for the <see cref="ColumnChooserDialog"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this._popup != null)
            {
                this._popup.IsOpen = false;


                this._popup.Opened -= Popup_Opened;

            }

            base.OnApplyTemplate();

            this._popup = base.GetTemplateChild("Popup") as Popup;
            if (this._popup != null)
            {

                this._popup.Opened += new EventHandler(Popup_Opened);
                this._popup.Placement = PlacementMode.Relative;
                this._popup.AllowsTransparency = true;

                if (this.IsOpen)
                {
                    this.Dispatcher.BeginInvoke(new Action(this.ToggleIsOpen));
                }
            }

            if (this._header != null)
            {
                this._header.MouseLeftButtonDown -= Header_MouseLeftButtonDown;
                this._header.MouseLeftButtonUp -= Header_MouseLeftButtonUp;
                this._header.MouseMove -= Header_MouseMove;
                this._header.LostMouseCapture -= Header_LostMouseCapture;

            }

            this._header = base.GetTemplateChild("HeaderElem") as FrameworkElement;
            
            if (this._header != null)
            {
                this._header.MouseLeftButtonDown += new MouseButtonEventHandler(Header_MouseLeftButtonDown);
                this._header.MouseLeftButtonUp += new MouseButtonEventHandler(Header_MouseLeftButtonUp);
                this._header.MouseMove += new MouseEventHandler(Header_MouseMove);
                this._header.LostMouseCapture += new MouseEventHandler(Header_LostMouseCapture);
            }

            if(this._rootElement != null)
                this._rootElement.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(RootElement_MouseLeftButtonDown));

            this._rootElement = base.GetTemplateChild("RootElement") as FrameworkElement;

            if(this._rootElement != null)
                this._rootElement.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(RootElement_MouseLeftButtonDown), true);

        }      

        #endregion // OnApplyTemplate

        #endregion // Overrides

        #region EventHandlers

        void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._isMouseDown)
            {
                XamGrid grid = this.ColumnLayout.Grid;
                Panel gridPanel = grid.Panel;

                if (!this._isDragging)
                {
                    this._header.CaptureMouse();

                    this._offsetPoint = e.GetPosition(this._header);

                    this._isDragging = true;
                }
                else
                {
                    Point elemPoint = e.GetPosition(gridPanel);


                    if (SystemParameters.IsMenuDropRightAligned)
                    {
                        double childWidth = 0;

                        if (this._popup.Child != null)
                        {
                            childWidth = this._popup.Child.RenderSize.Width;
                        }

                        this._popup.HorizontalOffset = (elemPoint.X - this._offsetPoint.X + childWidth);
                    }
                    else
                    {
                        this._popup.HorizontalOffset = (elemPoint.X - this._offsetPoint.X);
                    }



                    this._popup.VerticalOffset = (elemPoint.Y - this._offsetPoint.Y);
                }
            }
        }

        void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.EndDrag();
        }

        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._isMouseDown = this._header.CaptureMouse();

            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                this.ColumnLayout.Grid.CloseOpenHeaderDropDownControl();
        }

        void RootElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                this.ColumnLayout.Grid.CloseOpenHeaderDropDownControl();
        }

        void Header_LostMouseCapture(object sender, MouseEventArgs e)
        {
            // So, if a user was to right click, and the SL menu is invoked
            // it could steal focus from the mouse capture, and thus, we might not get notified of 
            // the mouse up, so we get stuck in limbo where we think the drag is still occuring, but its not. 
            this.EndDrag();

            base.OnLostMouseCapture(e);
        }

        void Popup_Opened(object sender, EventArgs e)
        {
            this.EnsureLocation();
        }

        #endregion // EventHandlers

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion		

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="ColumnChooserDialog"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="ColumnChooserDialog"/> object.
        /// </summary>
        /// <param name="name">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }
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