using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System;
using System.ComponentModel;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A dialog control for the <see cref="XamColorPicker"/>.
    /// </summary>
    public class ColorPickerDialog : Control, ICommandTarget, INotifyPropertyChanged
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
        /// Static constructor for the <see cref="ColorPickerDialog"/> class.
        /// </summary>
        static ColorPickerDialog()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPickerDialog), new FrameworkPropertyMetadata(typeof(ColorPickerDialog)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPickerDialog"/> class.
        /// </summary>
        public ColorPickerDialog()
        {



            this.Unloaded += new RoutedEventHandler(ColorPickerDialog_Unloaded);

            this.DialogCaption = AdvancedColorShadePicker.GetString("AdvancedDialogCaption");
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region IsOpen

        /// <summary>
        /// Identifies the <see cref="IsOpen"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(ColorPickerDialog), new PropertyMetadata(new PropertyChangedCallback(IsOpenChanged)));

        /// <summary>
        /// Gets/Sets whether the <see cref="ColorPickerDialog"/> is open. 
        /// </summary>
        /// <remarks>
        /// When the Dialog is opened, it will center itself to the <see cref="XamColorPicker"/>
        /// </remarks>
        public bool IsOpen
        {
            get { return (bool)this.GetValue(IsOpenProperty); }
            set { this.SetValue(IsOpenProperty, value); }
        }

        private static void IsOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerDialog dialog = (ColorPickerDialog)obj;

            if (dialog._popup != null)
            {
                dialog._popup.IsOpen = dialog.IsOpen;

                if (dialog._popup.IsOpen && dialog.ColorPicker != null)
                {

                    dialog._popup.Placement = PlacementMode.Center;


                    double top = dialog.ColorPicker.ActualHeight;
                    double left = dialog.ColorPicker.ActualWidth / 2;

                    if (dialog._rootElement != null)
                    {
                        dialog._rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                        top -= (dialog._rootElement.DesiredSize.Height / 2);

                        if (SystemParameters.IsMenuDropRightAligned)
                            left += (dialog._rootElement.DesiredSize.Width / 2);
                        else
                            left -= (dialog._rootElement.DesiredSize.Width / 2);



                    }

                    dialog._popup.HorizontalOffset = left;
                    dialog._popup.VerticalOffset = Math.Max(top, 0);



#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


                }

                dialog.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(dialog.KeyDownHandler));

                if (dialog._popup.Child != null)
                    dialog._popup.Child.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(dialog.KeyDownHandler));

                if (dialog.IsOpen)
                {
                    dialog.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(dialog.KeyDownHandler), true);
                    if (dialog._popup.Child != null)
                        dialog._popup.Child.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(dialog.KeyDownHandler), true);
                }


                dialog.Dispatcher.BeginInvoke(
                    new Action(()=>
                                   {
                                       dialog._popup.Placement = PlacementMode.Relative;
                                   }));

            }
        }

        #endregion // IsOpen

        #region ColorPicker

        /// <summary>
        /// Identifies the <see cref="ColorPicker"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColorPickerProperty = DependencyProperty.Register("ColorPicker", typeof(XamColorPicker), typeof(ColorPickerDialog), new PropertyMetadata(new PropertyChangedCallback(ColorPickerChanged)));

        /// <summary>
        /// Gets / sets the <see cref="XamColorPicker"/> associated with this control.
        /// </summary>
        public XamColorPicker ColorPicker
        {
            get { return (XamColorPicker)this.GetValue(ColorPickerProperty); }
            set { this.SetValue(ColorPickerProperty, value); }
        }

        private static void ColorPickerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion // ColorPicker

        #region DialogCaption

        /// <summary>
        /// Identifies the <see cref="DialogCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DialogCaptionProperty = DependencyProperty.Register("DialogCaption", typeof(string), typeof(ColorPickerDialog), new PropertyMetadata(new PropertyChangedCallback(DialogCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption seen at the top of the dialog.
        /// </summary>
        public string DialogCaption
        {
            get { return (string)this.GetValue(DialogCaptionProperty); }
            set { this.SetValue(DialogCaptionProperty, value); }
        }

        private static void DialogCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorPickerDialog dialog = (ColorPickerDialog)obj;
            dialog.OnPropertyChanged("DialogCaption");
        }

        #endregion // DialogCaption


        #endregion // Public

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
            return (command is ColorPickerDialogCommandBase);
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
            return this.ColorPicker;
        }
        #endregion // GetParameter

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Initializes Template parts for the <see cref="ColorPickerDialog"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this._popup != null)
            {
                this._popup.Opened -= new EventHandler(Popup_Opened);



            }

            this._popup = base.GetTemplateChild("Popup") as Popup;

            if (this._popup != null)
            {
                this._popup.IsOpen = this.IsOpen;
                this._popup.Opened += new EventHandler(Popup_Opened);


                this._popup.Placement = PlacementMode.Center;
                this._popup.AllowsTransparency = true;




            }

            if (this._header != null)
            {
                this._header.MouseLeftButtonDown -= Header_MouseLeftButtonDown;
                this._header.MouseLeftButtonUp -= Header_MouseLeftButtonUp;
                this._header.MouseMove -= Header_MouseMove;
            }

            this._header = base.GetTemplateChild("HeaderElem") as FrameworkElement;

            if (this._header != null)
            {
                this._header.MouseLeftButtonDown += new MouseButtonEventHandler(Header_MouseLeftButtonDown);
                this._header.MouseLeftButtonUp += new MouseButtonEventHandler(Header_MouseLeftButtonUp);
                this._header.MouseMove += new MouseEventHandler(Header_MouseMove);
            }

            this._rootElement = base.GetTemplateChild("RootElement") as FrameworkElement;
        }

        #endregion // OnApplyTemplate

        #endregion // Overrides

        #region EventHandlers

        #region ColorPickerDialog_Unloaded

        void ColorPickerDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        #endregion // ColorPickerDialog_Unloaded

        #region Popup_Closed


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        #endregion // Popup_Closed

        #region Popup_Opened

        private void Popup_Opened(object sender, EventArgs e)
        {
            Popup popup = sender as Popup;

            if (popup != null && popup.Child != null)
            {
                popup.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }


#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion // Popup_Opened

        #region Header_MouseMove

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._isMouseDown)
            {
                if (!this._isDragging)
                {
                    this._header.CaptureMouse();

                    this._offsetPoint = e.GetPosition(this._header);

                    this._isDragging = true;
                }
                else
                {
                    Point elemPoint = e.GetPosition(this.ColorPicker);

                    this._popup.VerticalOffset = (elemPoint.Y - this._offsetPoint.Y);

                    if (SystemParameters.IsMenuDropRightAligned)
                        this._popup.HorizontalOffset = (elemPoint.X + this._offsetPoint.X);
                    else
                        this._popup.HorizontalOffset = (elemPoint.X - this._offsetPoint.X);



                }
            }
        }

        #endregion // Header_MouseMove

        #region Header_MouseLeftButtonUp

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._isMouseDown = false;
            this._isDragging = false;
            this._header.ReleaseMouseCapture();



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion // Header_MouseLeftButtonUp

        #region Header_MouseLeftButtonDown

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._isMouseDown = true;
        }

        #endregion // Header_MouseLeftButtonDown

        #region KeyDown

        /// <summary>
        /// Called before the <see cref="UIElement.KeyDown"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event.</param>
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Escape)
                {
                    this.IsOpen = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        #endregion // KeyDown

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
        /// Fired when a property changes on the <see cref="XamColorPicker"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="XamColorPicker"/> object.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

namespace Infragistics.Controls.Editors
{

    /// <summary>
    /// An enumeration of available commands for the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/>.
    /// </summary>
    public enum ColorPickerDialogCommand
    {
        /// <summary>
        /// Closes the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/>.
        /// </summary>
        Close,

        /// <summary>
        ///  Opens the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/>.
        /// </summary>
        Open
    }

    /// <summary>
    /// A base class for all commands related to the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/>
    /// </summary>
    public abstract class ColorPickerDialogCommandBase : CommandBase
    {
        #region Overrides

        #region Public

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param propertyName="parameter">The <see cref="XamColorPicker"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            XamColorPicker cp = parameter as XamColorPicker;
            if (cp != null)
            {
                this.ExecuteCommand(cp);
            }
            base.Execute(parameter);
        }
        #endregion // Execute
        #endregion // Public

        #region Protected
        /// <summary>
        /// Executes the specific command on the specified <see cref="XamColorPicker"/>
        /// </summary>
        /// <param propertyName="col"></param>
        protected abstract void ExecuteCommand(XamColorPicker col);
        #endregion // Protected

        #endregion // Overrides
    }

    /// <summary>
    /// A Command that closes the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/> of the <see cref="XamColorPicker"/>
    /// </summary>
    public class ColorPickerDialogCloseCommand : ColorPickerDialogCommandBase
    {
        /// <summary>
        /// Hides the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/> if it's open.
        /// </summary>
        /// <param name="cp"></param>
        protected override void ExecuteCommand(XamColorPicker cp)
        {
            if (cp.ColorPickerDialog != null)
            {
                cp.ColorPickerDialog.IsOpen = false;
            }
        }
    }

    /// <summary>
    /// A Command that opens the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/> of the <see cref="XamColorPicker"/>.
    /// </summary>
    public class ColorPickerDialogOpenCommand : ColorPickerDialogCommandBase
    {
        /// <summary>
        /// Opens the <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/>
        /// </summary>
        /// <param name="cp"></param>
        protected override void ExecuteCommand(XamColorPicker cp)
        {
            if (cp.ColorPickerDialog != null)
            {
                cp.ColorPickerDialog.IsOpen = true;
            }
        }
    }

    /// <summary>
    /// An object that describes what kind of Command should be attached to a <see cref="Infragistics.Controls.Editors.Primitives.ColorPickerDialog"/>, and what should trigger the command.
    /// </summary>
    public class ColorPickerDialogCommandSource : CommandSource
    {
        #region Properties

        #region Public

        /// <summary>
        /// The type of command that will be executed.
        /// </summary>
        public ColorPickerDialogCommand CommandType
        {
            get;
            set;
        }

        #endregion // Public

        #endregion // Properties

        #region Overrides

        #region Protected

        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            ICommand command = null;

            switch (this.CommandType)
            {
                case ColorPickerDialogCommand.Close:
                    {
                        command = new ColorPickerDialogCloseCommand();
                        break;
                    }
                case ColorPickerDialogCommand.Open:
                    {
                        command = new ColorPickerDialogOpenCommand();
                        break;
                    }
            }
            return command;
        }

        #endregion // Protected

        #endregion // Overrides
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