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
using System.Windows.Markup;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A control which contains a popup and shows a content.
	/// </summary>
	[TemplatePart(Name = "Popup", Type = typeof(Popup))]
	public class HeaderDropDownControl : ContentControl, ICommandTarget
	{
		#region Members

		Popup _popup;
        XamGrid _xamGrid;
        CellControlBase _owner;
        CellBase _cell;




        
        FrameworkElement _popupRootElement;
        UIElement _rootVis;
        bool _isLoaded;
        bool _isMouseDownInside;

		#endregion // Members

		#region Constructor


        static HeaderDropDownControl()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(HeaderDropDownControl), new FrameworkPropertyMetadata(style));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="HeaderDropDownControl"/> class.
		/// </summary>
		public HeaderDropDownControl()
		{
			base.DefaultStyleKey = typeof(HeaderDropDownControl);

            this.Loaded += new RoutedEventHandler(HeaderDropDownControl_Loaded);
            this.Unloaded += new RoutedEventHandler(HeaderDropDownControl_Unloaded);
		}        

        void HeaderDropDownControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (this._cell != this._owner.Cell)
                this.IsOpen = false;

            if (this.IsOpen && this._popup != null)
            {
                this.InvalidateElementOutofPopup();
            }
        }


		#endregion // Constructor

		#region Properties

		#region IsOpen

		/// <summary>
		/// Identifies the <see cref="IsOpen"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(HeaderDropDownControl), new PropertyMetadata(new PropertyChangedCallback(IsOpenChanged)));

		/// <summary>
		/// Gets / sets if the <see cref="HeaderDropDownControl"/> should open it's <see cref="Popup"/> control.
		/// </summary>
		public bool IsOpen
		{
			get { return (bool)this.GetValue(IsOpenProperty); }
			set { this.SetValue(IsOpenProperty, value); }
		}

		private static void IsOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			HeaderDropDownControl hdd = (HeaderDropDownControl)obj;

			bool isOpen = (bool)e.NewValue;

            if (hdd._xamGrid == null)
            {
                if (hdd._popup != null && isOpen)
                {
                    hdd._popup.IsOpen = true;
                }

                DependencyObject  parent = VisualTreeHelper.GetParent(hdd);
                while (parent != null && hdd._xamGrid == null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                    hdd._xamGrid = parent as XamGrid;
                    if (hdd._owner == null)
                        hdd._owner = parent as CellControlBase;
                }
            }

            UIElement rootVis = PlatformProxy.GetRootVisual(hdd._xamGrid);

            if (hdd._popup != null)
            {
                if (isOpen)
                {
                    hdd._popup.IsOpen = isOpen;
                }
                else
                {
                    hdd.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        hdd._popup.IsOpen = isOpen;
                    }
                    ));
                }
            }

			if (isOpen)
			{
                hdd.InvalidateElementOutofPopup();

                hdd._rootVis = rootVis;
                rootVis.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(hdd.HeaderDropDownControl_MouseLeftButtonDown), true);
                rootVis.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(hdd.HeaderDropDownControl_MouseLeftButtonUp), true);
				rootVis.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(hdd.KeyDownHandler), true);
				hdd.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(hdd.KeyDownHandler), true);

				FrameworkElement content = hdd.Content as FrameworkElement;
				if (content != null)
				{
					content.SizeChanged += new SizeChangedEventHandler(hdd.Content_SizeChanged);
					content.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(hdd.KeyDownHandler), true);
				}

                if (hdd._xamGrid != null)
                    hdd._xamGrid.RegisterOpenHeaderDropDownControl(hdd);

                if (hdd._owner != null)
                {
                    hdd._cell = hdd._owner.Cell;
                    hdd.LayoutUpdated += new EventHandler(hdd.HeaderDropDownControl_LayoutUpdated);
                }

                Control ctrl = content as Control;
                if (ctrl != null)
                {
                    ctrl.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ctrl.Focus();
                    }));
                }


                hdd.AlignPopupHorizontal();
                hdd.AlignPopupVertical();


			}
			else
			{
                hdd._rootVis.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(hdd.HeaderDropDownControl_MouseLeftButtonDown));
                hdd._rootVis.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(hdd.HeaderDropDownControl_MouseLeftButtonUp));
                hdd._rootVis.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(hdd.KeyDownHandler));
                hdd._rootVis = null;
				hdd.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(hdd.KeyDownHandler));

				FrameworkElement content = hdd.Content as FrameworkElement;
				if (content != null)
				{
					content.SizeChanged -= hdd.Content_SizeChanged;
					content.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(hdd.KeyDownHandler));
				}

                if (hdd._xamGrid != null)
                    hdd._xamGrid.RegisterOpenHeaderDropDownControl(null);

                if (hdd._owner != null)
                {
                    hdd.LayoutUpdated -= hdd.HeaderDropDownControl_LayoutUpdated;
                }

                // Don't hang on to these referneces.
                hdd._xamGrid = null;
                hdd._owner = null;
                hdd._cell = null;
			}
		}
	
		#endregion // IsOpen

        #region Cell
        /// <summary>
        /// Gets the <see cref="CellBase"/> associated with this control.
        /// </summary>
        public CellBase Cell
        {
            get
            {
                return this._cell;
            }
        }
        #endregion // Cell

        #region AllowResizing

        /// <summary>
        /// Identifies the <see cref="AllowResizing"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowResizingProperty = DependencyProperty.Register("AllowResizing", typeof(bool), typeof(HeaderDropDownControl), new PropertyMetadata(new PropertyChangedCallback(AllowResizingChanged)));

        /// <summary>
        /// Gets/sets whether a resizer appears at the bottom of the HeaderDropDownControl
        /// </summary>
        public bool AllowResizing
        {
            get { return (bool)this.GetValue(AllowResizingProperty); }
            set { this.SetValue(AllowResizingProperty, value); }
        }

        private static void AllowResizingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HeaderDropDownControl hdd = (HeaderDropDownControl)obj;

            hdd.EnsureVisualStates();      
        }

        #endregion // AllowResizing 
				

        #endregion // Properties

        #region Overrides

        /// <summary>
		/// Builds the visual tree for the <see cref="HeaderDropDownControl"/>
		/// </summary>
		public override void OnApplyTemplate()
		{






            if (this._popup != null)
            {
                this._popup.Closed -= Popup_Closed;
            }


			this._popup = base.GetTemplateChild("Popup") as Popup;
            if (this._popup != null)
            {

                this._popup.AllowsTransparency = true;
                this._popup.StaysOpen = false;
                this._popup.Closed += new EventHandler(Popup_Closed);

                UIElement popupChild = this._popup.Child;

                if (popupChild != null)
                {
                    


                    HeaderDropDownContentRootPanel rootPanel = new HeaderDropDownContentRootPanel();
                    this._popup.Child = rootPanel;
                    rootPanel.Children.Add(popupChild);
                }

                this._popupRootElement = this._popup.Child as FrameworkElement;



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

            }
						
			base.OnApplyTemplate();

            this.EnsureVisualStates();
		}        

		#endregion // Overrides

		#region Methods

		#region SupportsCommand
		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
            return command is PopupCommandBase || command is ShowCustomFilterDialogFilterMenuConditionalCommand || command is ShowCompoundFilterDialogFilterMenuConditionalCommand;
		}
		#endregion // SupportsCommand

		#region GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource command)
		{
			return this;
		}
		#endregion // GetParameter

        #region Protected

        #region InvalidateElementOutofPopup

        /// <summary>
        /// Invalidates the the underlying width/height of the element underneath the popup used to capture mouse clicks
        /// </summary>
        protected virtual void InvalidateElementOutofPopup()
        {
            if (this._isLoaded)
            {
                if (this.IsOpen && this._popup != null)
                {


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

               
                }
            }
        }

        #endregion // InvalidateElementOutofPopup

        #endregion // Protected

        #region Private

        #region AlignPopupHorizontal
        private void AlignPopupHorizontal()
		{
            double rootWidth = 1;



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			FrameworkElement fe = (FrameworkElement)this.Content;
			double popupContentWidth = fe.DesiredSize.Width;
            
            UIElement rootVisual = PlatformProxy.GetRootVisual(this);

            double rootOffsetX = 0;

            if (rootVisual != null)
            {
                GeneralTransform transform = this.TransformToVisual(rootVisual);
                Point menuItemOffset = transform.Transform(new Point(0, 0));

                rootOffsetX = menuItemOffset.X;
            }

            if (rootWidth == 0 || popupContentWidth == 0)
            {
                return;
            }
            

			double val = DefineHorizontalOffset(rootOffsetX, rootWidth, popupContentWidth);

			this._popup.HorizontalOffset = val;
		}
		#endregion // AlignPopupHorizontal

		#region DefineHorizontalOffset
		private static double DefineHorizontalOffset(double refferPoint, double rootWidth, double popupContentWidth)
		{

            return Math.Min(refferPoint, 0);


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion // DefineHorizontalOffset


		#region AlignPopupVertical
		private void AlignPopupVertical()
		{
            double rootHeight = 1;



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			FrameworkElement fe = (FrameworkElement)this.Content;

			double popupContentHeight = fe.DesiredSize.Height;

            FrameworkElement parentElement = this.Parent as FrameworkElement;
            UIElement rootVisual = PlatformProxy.GetRootVisual(this);

            double rootOffsetY = 0;
            double parentHeight = 0;

		    if (parentElement != null)
		    {
		        parentHeight = parentElement.ActualHeight;
		    }

            Point parentOffset = new Point(0,0);

            if (rootVisual != null)
            {
                if (parentElement != null)
                {
                    GeneralTransform parentTransform = parentElement.TransformToVisual(rootVisual);
                    parentOffset = parentTransform.Transform(new Point(0, 0));  
                }

                GeneralTransform transform = this.TransformToVisual(rootVisual);

                Point menuItemOffset = transform.Transform(new Point(0, 0));

                rootOffsetY = menuItemOffset.Y;
            }

			if (rootHeight == 0 || popupContentHeight == 0)
			{
				return;
			}

			double val = DefineVerticalOffset(rootOffsetY, rootHeight, popupContentHeight, parentHeight,rootOffsetY- parentOffset.Y );

			this._popup.VerticalOffset = val;
		}
		#endregion // AlignPopupVertical

		#region DefineVerticalOffset
		private static double DefineVerticalOffset(double refferPoint, double rootHeight, double popupContentHeight, double parentHeight, double offsetDelta)
		{

            return offsetDelta;


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion // DefineVerticalOffset

        #region EnsureVisualStates

        private void EnsureVisualStates()
        {
            if (this.AllowResizing)
            {
                VisualStateManager.GoToState(this, "Resizable", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NotResizable", false);
            }
        }

        #endregion // EnsureVisualStates

        #endregion // Private

        #endregion // Methods

        #region EventHandlers

        #region HeaderDropDownControl_MouseLeftButtonDown

        void HeaderDropDownControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dp = e.OriginalSource as DependencyObject;

			bool insideMe = false;

			while (dp != null)
			{
				HeaderDropDownControl hdd = dp as HeaderDropDownControl;

				if (hdd != null)
				{
					if (hdd != this)
					{
						if (hdd.IsOpen)
						{
							this.IsOpen = false;
							break;
						}
					}
					else
					{
						insideMe = true;
						break;
					}
				}

				DependencyObject parent = PlatformProxy.GetParent(dp);

                if (parent == null)
                {
                    FrameworkElement elem = dp as FrameworkElement;
                    if (elem != null)
                    {
                        dp = elem.Parent;
                    }
                    else
                        dp = parent;
                }
                else
                    dp = parent;

			}

            this._isMouseDownInside = insideMe;

			if (!insideMe)
			{
				this.IsOpen = false;
			}
		}

		#endregion // HeaderDropDownControl_MouseLeftButtonDown

        #region HeaderDropDownControl_MouseLeftButtonUp

        void HeaderDropDownControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._isMouseDownInside = false;
        }

        #endregion // HeaderDropDownControl_MouseLeftButtonUp

        #region Content_SizeChanged

        private void Content_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			FrameworkElement fe = (FrameworkElement)sender;

			if (fe.DesiredSize.Width != 0)
			{
				AlignPopupHorizontal();
			}
			if (fe.DesiredSize.Height != 0)
			{
				AlignPopupVertical();
			}
		}

		#endregion // Content_SizeChanged

		#region KeyDownHandler

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

		#endregion // KeyDownHandler

        #region ElementOutofPopup_MouseLeftButtonDown

        void ElementOutofPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.IsOpen = false;
        }
        #endregion // ElementOutofPopup_MouseLeftButtonDown

        #region HeaderDropDownControl_Unloaded
        void HeaderDropDownControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = false;
        }
        #endregion // HeaderDropDownControl_Unloaded

        #region HeaderDropDownControl_Loaded
        void HeaderDropDownControl_Loaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = true;
        }
        #endregion // HeaderDropDownControl_Loaded

        #region Popup_Closed

        void Popup_Closed(object sender, EventArgs e)
        {
            if (!this._isMouseDownInside)
                this.IsOpen = false;
        }

        #endregion // Popup_Closed

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

		#region OpenButtonContent

		/// <summary>
		/// Identifies the <see cref="OpenButtonContent"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty OpenButtonContentProperty = DependencyProperty.Register("OpenButtonContent", typeof(object), typeof(HeaderDropDownControl), new PropertyMetadata(new PropertyChangedCallback(OpenButtonContentChanged)));

		/// <summary>
		/// Gets / sets the content that will be applied to the OpenButton on the <see cref="HeaderDropDownControl"/>.
		/// </summary>
		public object OpenButtonContent
		{
			get { return (object)this.GetValue(OpenButtonContentProperty); }
			set { this.SetValue(OpenButtonContentProperty, value); }
		}

		private static void OpenButtonContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{

		}

		#endregion // OpenButtonContent
	}

    #region HeaderDropDownContentPanel

    /// <summary>
    /// Defines an area within which the content of the <see cref="HeaderDropDownControl"/> will be placed.
    /// </summary>
    public class HeaderDropDownContentRootPanel : Grid
    {
    }

    #endregion // HeaderDropDownContentPanel

	#region XamGridPopupCommandSource

	/// <summary>
	/// The command source object for <see cref="HeaderDropDownControl"/>.
	/// </summary>
	public class XamGridPopupCommandSource : CommandSource
	{
		#region Properties

		#region CommandType
		/// <summary>
		/// Gets / sets the <see cref="XamGridPopupCommand"/> which is to be executed by the command.
		/// </summary>
		public XamGridPopupCommand CommandType
		{
			get;
			set;
		}

		#endregion // CommandType

		#endregion // Properties

		#region Overrides

		#region ResolveCommand
		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			ICommand command = null;
			switch (this.CommandType)
			{
				case XamGridPopupCommand.TogglePopup:
					{
						command = new TogglePopupCommand();
						break;
					}
				case XamGridPopupCommand.OpenPopup:
					{
						command = new OpenPopupCommand();
						break;
					}
				case XamGridPopupCommand.ClosePopup:
					{
						command = new ClosePopupCommand();
						break;
					}
			}
			return command;
		}

		#endregion // ResolveCommand

		#endregion // Overrides
	}

	#endregion // XamGridPopupCommandSource

	#region XamGridPopupCommand
	/// <summary>
	/// An enum describing the commands which can be executed on the <see cref="XamGridPopupCommandSource"/>
	/// </summary>
	public enum XamGridPopupCommand
	{
		/// <summary>
		/// Switches the popup from open to closed, or closed to open.
		/// </summary>
		TogglePopup,
		/// <summary>
		/// Opens the Popup.
		/// </summary>
		OpenPopup,
		/// <summary>
		/// Closes the Popup.
		/// </summary>
		ClosePopup,
	}
	#endregion // XamGridPopupCommand

	#region PopupCommandBase
	/// <summary>
	/// A base class for actions which are executed on <see cref="HeaderDropDownControl"/>
	/// </summary>
	public abstract class PopupCommandBase : CommandBase
	{
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
		#endregion // CanExecute
	}
	#endregion // PopupCommandBase

	#region OpenPopupCommand
	/// <summary>
	/// A command which will open the popup.
	/// </summary>
	public class OpenPopupCommand : PopupCommandBase
	{
		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
			HeaderDropDownControl dropDown = parameter as HeaderDropDownControl;

			if (dropDown != null)
			{
				dropDown.IsOpen = true;
				this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute
	}
	#endregion // OpenPopupCommand

	#region TogglePopupCommand
	/// <summary>
	/// A command which will toggle the popup.
	/// </summary>
	public class TogglePopupCommand : PopupCommandBase
	{
		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
			HeaderDropDownControl dropDown = parameter as HeaderDropDownControl;

			if (dropDown != null)
			{
				dropDown.IsOpen = !dropDown.IsOpen;
				this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute
	}
	#endregion // TogglePopupCommand

	#region ClosePopupCommand
	/// <summary>
	/// A command which will close the popup.
	/// </summary>
	public class ClosePopupCommand : PopupCommandBase
	{
		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
			HeaderDropDownControl dropDown = parameter as HeaderDropDownControl;

			if (dropDown != null)
			{
				dropDown.IsOpen = false;
				this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute
	}
	#endregion // ClosePopupCommand

    #region XamGridPopupFilterMenuConditionalCommandSource
    /// <summary>
    /// A <see cref="CommandSource"/> controlling the popup for the filter menu.
    /// </summary>
    public class XamGridPopupFilterMenuConditionalCommandSource : XamGridPopupCommandSource
    {
        #region ConditionalCommandParameter

        /// <summary>
        /// Identifies the <see cref="ConditionalCommandParameter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ConditionalCommandParameterProperty = DependencyProperty.Register("ConditionalCommandParameter", typeof(List<FilterOperand>), typeof(XamGridPopupFilterMenuConditionalCommandSource), null);

        /// <summary>
        /// A list of <see cref="FilterOperand"/> objects which make up the parameters.
        /// </summary>
        public List<FilterOperand> ConditionalCommandParameter
        {
            get { return (List<FilterOperand>)this.GetValue(ConditionalCommandParameterProperty); }
            set { this.SetValue(ConditionalCommandParameterProperty, value); }
        }

        #endregion // ConditionalCommandParameter 
          
        #region Overrides

        #region ResolveCommand
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            switch (this.CommandType)
            {
                case XamGridPopupCommand.ClosePopup:
                    {
                        return new ClosePopupConditionalCommand();                        
                    }
            }
            return base.ResolveCommand();
        }

        #endregion // ResolveCommand

        #endregion // Overrides
    }

    #endregion // XamGridPopupFilterMenuConditionalCommandSource

    #region ClosePopupConditionalCommand
    /// <summary>
    /// A command which will close the popup.
    /// </summary>
    public class ClosePopupConditionalCommand : PopupCommandBase
    {
        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            HeaderDropDownControl dropDown = parameter as HeaderDropDownControl;

            if (dropDown != null)
            {
                XamGridPopupFilterMenuConditionalCommandSource source = (XamGridPopupFilterMenuConditionalCommandSource)this.CommandSource;
                if (source.ConditionalCommandParameter != null && source.ConditionalCommandParameter.Count > 0)
                {
                    dropDown.IsOpen = false;
                    this.CommandSource.Handled = true;
                }
            }

            base.Execute(parameter);
        }
        #endregion // Execute
    }
    #endregion // ClosePopupConditionalCommand

    #region CustomFilteringDialogFilterMenuCommandSource
    /// <summary>
    /// A <see cref="CommandSource"/> object which controls the custom filter dialog.
    /// </summary>
    public class CustomFilteringDialogFilterMenuCommandSource : CustomFilteringDialogCommandSource
    {
        #region ConditionalCommandParameter

        /// <summary>
        /// Identifies the <see cref="ConditionalCommandParameter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ConditionalCommandParameterProperty = DependencyProperty.Register("ConditionalCommandParameter", typeof(List<FilterOperand>), typeof(CustomFilteringDialogFilterMenuCommandSource), null);
        
        /// <summary>
        /// A list of <see cref="FilterOperand"/> objects which make up the parameters.
        /// </summary>
        public List<FilterOperand> ConditionalCommandParameter
        {
            get { return (List<FilterOperand>)this.GetValue(ConditionalCommandParameterProperty); }
            set { this.SetValue(ConditionalCommandParameterProperty, value); }
        }

        #endregion // ConditionalCommandParameter 

        #region ResolveCommand
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            ICommand command = null;
            switch (this.CommandType)
            {
                case CustomFilteringDialogCommand.ShowCustomFilterDialog:
                    {
                        command = new ShowCustomFilterDialogFilterMenuConditionalCommand();
                        break;
                    }
            }            
            return command;
        }
        #endregion // ResolveCommand
    }
    #endregion // CustomFilteringDialogFilterMenuCommandSource

    #region ShowCustomFilterDialogFilterMenuConditionalCommand

    /// <summary>
    /// A command which will show the <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class ShowCustomFilterDialogFilterMenuConditionalCommand : CellBaseCommandBase
    {
        #region Methods

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

        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            HeaderDropDownControl hddc = parameter as HeaderDropDownControl;
            if (hddc != null)
            {
                this.ExecuteCommand(hddc.Cell);
                this.CommandSource.Handled = false;
                return;
            }
            base.Execute(parameter);
        }
        #endregion // Execute

        #region ExecuteCommand
        /// <summary>
        /// Executes the specific command on the specified <see cref="CellBase"/>
        /// </summary>
        /// <param propertyName="col"></param>
        protected override void ExecuteCommand(CellBase cell)
        {
            CustomFilteringDialogFilterMenuCommandSource source = (CustomFilteringDialogFilterMenuCommandSource)this.CommandSource;

            if (source.ConditionalCommandParameter == null)
                return;

            if (source.ConditionalCommandParameter.Count == 0)
                return;

            if (!source.ConditionalCommandParameter[0].RequiresFilteringInput)
                return;

            XamGrid grid = cell.Row.ColumnLayout.Grid;
            grid.Panel.CustomFilterDialogControl.Cell = cell;
            grid.Panel.CustomFilterDialogControl.Show(source.ConditionalCommandParameter);            
            this.CommandSource.Handled = false;
        }
        #endregion // ExecuteCommand

        #endregion // Methods
    }
    #endregion // ShowCustomFilterDialogFilterMenuConditionalCommand


    #region CompoundFilteringDialogFilterMenuCommandSource
    /// <summary>
    /// A <see cref="CommandSource"/> object which controls the Compound filter dialog.
    /// </summary>
    public class CompoundFilteringDialogFilterMenuCommandSource : CompoundFilteringDialogCommandSource
    {
        #region ConditionalCommandParameter

        /// <summary>
        /// Identifies the <see cref="ConditionalCommandParameter"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ConditionalCommandParameterProperty = DependencyProperty.Register("ConditionalCommandParameter", typeof(List<FilterOperand>), typeof(CompoundFilteringDialogFilterMenuCommandSource), null);

        /// <summary>
        /// A list of <see cref="FilterOperand"/> objects which make up the parameters.
        /// </summary>
        public List<FilterOperand> ConditionalCommandParameter
        {
            get { return (List<FilterOperand>)this.GetValue(ConditionalCommandParameterProperty); }
            set { this.SetValue(ConditionalCommandParameterProperty, value); }
        }

        #endregion // ConditionalCommandParameter

        #region ResolveCommand
        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            ICommand command = null;
            switch (this.CommandType)
            {
                case CompoundFilteringDialogCommand.ShowCompoundFilterDialog:
                    {
                        command = new ShowCompoundFilterDialogFilterMenuConditionalCommand();
                        break;
                    }
            }
            return command;
        }
        #endregion // ResolveCommand
    }
    #endregion // CompoundFilteringDialogFilterMenuCommandSource

    #region ShowCompoundFilterDialogFilterMenuConditionalCommand

    /// <summary>
    /// A command which will show the <see cref="Infragistics.Controls.Grids.Primitives.ColumnFilterDialogControl"/>.
    /// </summary>
    public class ShowCompoundFilterDialogFilterMenuConditionalCommand : CellBaseCommandBase
    {
        #region Methods

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

        #endregion // CanExecute

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            HeaderDropDownControl hddc = parameter as HeaderDropDownControl;
            if (hddc != null)
            {
                this.ExecuteCommand(hddc.Cell);
                this.CommandSource.Handled = false;
                return;
            }
            base.Execute(parameter);
        }
        #endregion // Execute

        #region ExecuteCommand
        /// <summary>
        /// Executes the specific command on the specified <see cref="CellBase"/>
        /// </summary>
        /// <param propertyName="col"></param>
        protected override void ExecuteCommand(CellBase cell)
        {
            CompoundFilteringDialogFilterMenuCommandSource source = (CompoundFilteringDialogFilterMenuCommandSource)this.CommandSource;

            if (source.ConditionalCommandParameter == null)
                return;

            if (source.ConditionalCommandParameter.Count == 0)
                return;

            if (!source.ConditionalCommandParameter[0].RequiresFilteringInput)
                return;

            XamGrid grid = cell.Row.ColumnLayout.Grid;
            grid.Panel.CompoundFilterDialogControl.Cell = cell;
            grid.Panel.CompoundFilterDialogControl.Show(source.ConditionalCommandParameter);
            this.CommandSource.Handled = false;
        }
        #endregion // ExecuteCommand

        #endregion // Methods
    }
    #endregion // ShowCompoundFilterDialogFilterMenuConditionalCommand
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