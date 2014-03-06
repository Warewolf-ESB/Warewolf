using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A control used for visual presenting of an customer object.
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]

    [TemplateVisualState(Name = "Unselected", GroupName = "SelectionStates")]
    [TemplateVisualState(Name = "Selected", GroupName = "SelectionStates")]

    [TemplateVisualState(Name = "Focused", GroupName = "FocusStates")]
    [TemplateVisualState(Name = "Unfocused", GroupName = "FocusStates")]

    [TemplateVisualState(Name = "Standard", GroupName = "MultipleSelectionStates")]
    [TemplateVisualState(Name = "CheckBox", GroupName = "MultipleSelectionStates")]

    [TemplatePart(Name = "SelectedCheckbox", Type = typeof(CheckBox))]
    public class ComboEditorItemControl : ContentControl, IRecyclableElement, INotifyPropertyChanged
    {
        #region Member Variables

        private CheckBox _elementCheckBox;

        #endregion //Member Variables

        #region Constructor

		// JM 05-24-11 Port to WPF.

		static ComboEditorItemControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboEditorItemControl), new FrameworkPropertyMetadata(typeof(ComboEditorItemControl)));
		}


		/// <summary>
        /// Initializes a new instance of the <see cref="ComboEditorItemControl"/> class.
        /// </summary>
        public ComboEditorItemControl()
        {
			// JM 05-24-11 Port to WPF.




            this.Loaded += this.ComboEditorItemControl_Loaded;

			// JM 05-24-11 Port to WPF.  Moved this here from the default style in generic.xaml.



			KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Local);

        }

        #endregion //Constructor

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._elementCheckBox = this.GetTemplateChild("SelectedCheckbox") as CheckBox;

            this.AttachIsSelectedToCheckBox();

            this.EnsureVisualStates();
        }

        #endregion //OnApplyTemplate

		// JM 05-24-11 Port to WPF.  This is only needed in Silverlight.


#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

		#region OnPropertyChanged

		/// <summary>
		/// Called when a property value changes.
		/// </summary>
		/// <param name="e">Information about the property value that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == UIElement.IsMouseOverProperty)
				this.EnsureVisualStates();
		}

		#endregion //OnPropertyChanged


        #region OnMouseLeftButtonDown
        /// <summary>
        /// Called before the MouseLeftButtonDown event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            e.Handled = true;
            this.Item.ComboEditor.OnComboEditorItemClicked(this.Item);
        }
        #endregion // OnMouseLeftButtonDown

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new ComboEditorItemControlAutomationPeer<ComboEditorItem, ComboEditorItemControl>(this);
        }
        #endregion //OnCreateAutomationPeer

        #region MeasureOverride
        /// <summary>
        /// Provides the behavior for the "measure" pass of the <see cref="ComboEditorItemControl"/>.
		/// </summary>
		/// <param propertyName="availableSize">The available size that this object can give to child objects. Infinity can be specified
		/// as a value to indicate the object will size to whatever content is available.</param>
		/// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            this.MeasureRaised = true;
            return base.MeasureOverride(availableSize);
        }

        #endregion // MeasureOverride

        #endregion // Overrides

        #region Properties

        #region Public

        #region Item

        /// <summary>
        /// Gets or sets the parent ComobBoxItem object.
        /// </summary>
        public ComboEditorItem Item
        {
            get;
            private set;
        }

        #endregion //Item

        #endregion //Public

        #region Protected

		// JM 05-24-11 Port to WPF.  This is only needed in Silverlight.


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


        #region DelayRecycling

        /// <summary>
        /// Gets or sets a value indicating whether the object should be recycled.
        /// </summary>
        protected virtual bool DelayRecycling
        {
            get;
            set;
        }

        #endregion // DelayRecycling

        #region OwnerPanel

        /// <summary>
        /// Gets or sets the panel that owns this control.
        /// </summary>
        protected virtual Panel OwnerPanel
        {
            get;
            set;
        }

        #endregion // OwnerPanel

        #endregion // Protected

        #region Internal

        #region MeasureRaised

        internal bool MeasureRaised
        {
            get;
            set;
        }
        #endregion // MeasureRaised

        #endregion // Internal

        #endregion //Properties

        #region Methods

        #region Private

        #region EnsureVisualStates

        /// <summary>
        /// Change the VisualState based on the IsSelected and IsFocused properties.
        /// </summary>
        internal void EnsureVisualStates()
        {
            if (this.Item != null && this.Item.ComboEditor != null)
            {
                if (!this.Item.IsEnabled)
                {
                    this.GoToState(false, (this.Content is Control) ? "Normal" : "Disabled");
                }
                else if (this.IsMouseOver)
                {
                    this.GoToState(true, "MouseOver");
                }
                else
                {
                    this.GoToState(true, "Normal");
                }

                if (this.Item.IsSelected)
                {
                    this.GoToState(true, "Selected");
                }
                else
                {
                    this.GoToState(true, "Unselected");
                }

                if (this.Item.IsFocused)
                {
                    this.GoToState(true, "Focused");
                }
                else
                {
                    this.GoToState(true, "Unfocused");
                }

                if (this.Item.ComboEditor.CheckBoxVisibility.Equals(Visibility.Visible))
                {
                    this.GoToState(false, "CheckBox");
                }
                else
                {
                    this.GoToState(false, "Standard");
                }
            }
        }

        #endregion //EnsureVisualStates

        #region GoToState

        /// <summary>
        /// Sets a new VisualState. 
        /// </summary>
        /// <param name="useTransitions">Specifies if transition is used.</param>
        /// <param name="stateName">The name of the new VisualState.</param>
        private void GoToState(bool useTransitions, string stateName)
        {
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        #endregion //GoToState

        #region AttachIsSelectedToCheckBox
        
        private void AttachIsSelectedToCheckBox()
        {
            if (this._elementCheckBox != null)
            {
               if (this.Item != null)
                {
                    Binding b = new Binding("IsSelected");
                    b.Source = this.Item;
                    b.Mode = BindingMode.TwoWay;
                    this._elementCheckBox.SetBinding(CheckBox.IsCheckedProperty, b);
                }
            }
        }

        #endregion // AttachIsSelectedToCheckBox

        #endregion //Private

        #region Protected

        #region OnAttached

        /// <summary>
        /// Called when the <see cref="ComboEditorItem"/> is attached to the <see cref="ComboEditorItemControl"/>.
        /// </summary>
        /// <param name="comboEditorItem">The <see cref="ComboEditorItem"/> that is being attached to the <see cref="ComboEditorItemControl"/></param>
        protected internal virtual void OnAttached(ComboEditorItem comboEditorItem)
        {
            this.IsEnabled = comboEditorItem.IsEnabled;
            this.Item = comboEditorItem;

            if (this.Item.ComboEditor.ItemTemplate != null)
            {
                this.Content = comboEditorItem.Data;
                this.ContentTemplate = comboEditorItem.ComboEditor.ItemTemplate;
            }
            else
            {
				string path = comboEditorItem.ComboEditor.DisplayMemberPathResolved;	// JM 01-11-12 TFS98440 - Use DisplayMemberPathRESOLVED
                if (path == null)
                    path = "";
                Binding b = new Binding(path);
                b.Mode = BindingMode.OneWay;
                b.Source = comboEditorItem.Data;
                this.SetBinding(ComboEditorItemControl.ContentProperty, b);
            }

            Style s = (this.Item.Style == null) ? this.Item.ComboEditor.ItemContainerStyle : this.Item.Style;

            if (this.Style != s)
            {
                if (s != null)
                    this.Style = s;
                else
                    this.ClearValue(ComboEditorItemControl.StyleProperty);
            }

            this.Item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);

            this.EnsureVisualStates();

            this.AttachIsSelectedToCheckBox();
        }

        #endregion // OnAttached

        #region OnReleased

        /// <summary>
        /// Called when the <see cref="ComboEditorItem"/> releases the <see cref="ComboEditorItemControl"/>.
        /// </summary>
        /// <param name="comboEditorItem">The <see cref="ComboEditorItem"/> that is being released from the <see cref="ComboEditorItemControl"/></param>
        protected internal virtual void OnReleased(ComboEditorItem comboEditorItem)
        {
            this.Item.PropertyChanged -= Item_PropertyChanged;

            this.Item = null;
            this.Content = null;
        }

        #endregion // OnReleased

        #endregion //Protected

        #endregion //Methods

        #region Event Handlers

        #region ComboEditorItemControl_Loaded

        private void ComboEditorItemControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.EnsureVisualStates();
        }

        #endregion //ComboEditorItemControl_Loaded
              
        #region Item_PropertyChanged
        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.EnsureVisualStates();

            if (e.PropertyName == "Style")
            {
                this.Style = (this.Item.Style == null) ? this.Item.ComboEditor.ItemContainerStyle : this.Item.Style;
            }
            else if (e.PropertyName == "Template")
            {
                if (this.Item != null && this.Item.ComboEditor.ItemTemplate != null)
                {
                    this.Content = this.Item.Data;
                    this.ContentTemplate = this.Item.ComboEditor.ItemTemplate;
                }
            }
            if (e.PropertyName == "IsEnabled")
            {
                this.IsEnabled = this.Item.IsEnabled;
            }
        }
        #endregion // Item_PropertyChanged

        #endregion //Event Handlers

        #region IRecyclableElement Members

        bool IRecyclableElement.DelayRecycling
        {
            get
            {
                return this.DelayRecycling;
            }

            set
            {
                this.DelayRecycling = value;
            }
        }

        Panel IRecyclableElement.OwnerPanel
        {
            get
            {
                return this.OwnerPanel;
            }

            set
            {
                this.OwnerPanel = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event raised when a property on this object changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propName">A property name.</param>
        protected virtual void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }

            this.EnsureVisualStates();
        }

        #endregion INotifyPropertyChanged Members
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