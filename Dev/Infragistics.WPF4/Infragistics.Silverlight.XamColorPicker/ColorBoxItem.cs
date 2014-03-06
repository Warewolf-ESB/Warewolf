using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// The <see cref="ColorItemBox"/> is the visual representation of a <see cref="ColorItem"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ColorItemBox : Control, INotifyPropertyChanged
    {





        #region Members
        ColorItem _colorItem;
        #endregion // Members

        #region Constructor

        /// <summary>
        /// Static constructor for the <see cref="ColorItemBox"/> class.
        /// </summary>
        static ColorItemBox()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorItemBox), new FrameworkPropertyMetadata(typeof(ColorItemBox)));
        }


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


        #endregion // Constructor

        #region Properties

        #region ColorItemBrush

        /// <summary>
        /// Identifies the <see cref="ColorItemBrush"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColorItemBrushProperty = DependencyProperty.Register("ColorItemBrush", typeof(Brush), typeof(ColorItemBox), new PropertyMetadata(new PropertyChangedCallback(ColorItemBrushChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Brush"/> which is generated for this control from the set <see cref="ColorItem"/>.
        /// </summary>
        public Brush ColorItemBrush
        {
            get { return (Brush)this.GetValue(ColorItemBrushProperty); }
            private set { this.SetValue(ColorItemBrushProperty, value); }
        }

        private static void ColorItemBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((ColorItemBox)obj).NotifyPropertyChanged("ColorItemBrush");
        }

        #endregion // ColorItemBrush

        #region ColorItem

        /// <summary>
        /// Gets / sets the <see cref="ColorItem"/> associated with this <see cref="ColorItemBox"/>.
        /// </summary>
        public ColorItem ColorItem
        {
            get
            {
                return this._colorItem;
            }
            protected internal set
            {
                if (this._colorItem != value)
                {
                    if (this._colorItem != null)
                    {
                        this._colorItem.PropertyChanged -= ColorItem_PropertyChanged;
                        this._colorItem.ColorItemBox = null;
                    }

                    this._colorItem = value;

                    this.SetupBrush();

                    if (this._colorItem != null)
                    {
                        this._colorItem.PropertyChanged += ColorItem_PropertyChanged;
                        this._colorItem.ColorItemBox = this;
                    }
                }
            }
        }

        #endregion // ColorItem

        #endregion // Properties

        #region Methods

        #region SetupBrush

        private void SetupBrush()
        {
            if (this.ColorItem != null)
            {
                this.ColorItemBrush = new SolidColorBrush(this.ColorItem.Color);
            }
        }
        #endregion // SetupBrush

        #region EnsureState

        /// <summary>
        /// Ensures that the <see cref="ColorItemBox"/> is in the correct state.
        /// </summary>
        protected internal void EnsureState()
        {
            VisualStateManager.GoToState(this, "Normal", false);

            if (this.ColorItem != null && this.ColorItem.IsHover)
            {
                VisualStateManager.GoToState(this, "Hover", false);
            }
            else if (this.ColorItem != null && this.ColorItem.WasSelected)
            {
                VisualStateManager.GoToState(this, "WasSelected", false);
            }
        }

        #endregion // EnsureState

        #endregion // Methods

        #region Overrides

        #region OnMouseEnter

        /// <summary>
        /// Called before the System.Windows.UIElement.MouseEnter event occurs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);








            if (this.ColorItem != null && this.ColorItem.Parent.ColorPalette != null && this.ColorItem.Color != this.ColorItem.Parent.ColorPalette.DefaultColor)
                this.ColorItem.IsHover = true;
        }

        #endregion // OnMouseEnter

        #region OnMouseLeftButtonUp

        /// <summary>
        /// Called before the System.Windows.UIElement.MouseLeftButtonUp event occurs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (this.ColorItem != null)
            {
                this.ColorItem.IsSelected = true;
            }
        }

        #endregion // OnMouseLeftButtonUp

        #region OnApplyTemplate

        #region OnApplyTemplate

        /// <summary>
        /// Builds the visual tree for the <see cref="ColorItemBox"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.EnsureState();
        }

        #endregion // OnApplyTemplate

        #endregion // OnApplyTemplate

        #endregion // Overrides

        #region EventHandlers

        void ColorItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Color")
            {
                this.SetupBrush();
            }
            this.EnsureState();
        }

        #endregion // EventHandlers

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="ColorItemBox"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="ColorItemBox"/> object.
        /// </summary>
        /// <param propertyName="propName">The propertyName of the property that has changed.</param>
        protected void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
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