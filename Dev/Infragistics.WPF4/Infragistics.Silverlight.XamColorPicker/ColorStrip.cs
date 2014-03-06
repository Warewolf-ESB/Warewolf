using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A class which will display a group of <see cref="ColorItemBox"/> objects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [TemplatePart(Name = "ColorPickerPanel", Type = typeof(Panel))]
    public class ColorStrip : Control
    {
        #region Members

        Panel _colorPickerPanel;
        List<ColorItemBox> _colorBoxItems;
        List<ColorItem> _colorItems;
        ColorItem _selectedColorItem;
        ColorItem _hoverColorItem;
        bool _suppressSelectedColorItem;

        #endregion // Members

        #region Properties

        #region ItemCount

        /// <summary>
        /// Identifies the <see cref="VisibleItemCount"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty VisibleItemCountProperty = DependencyProperty.Register("VisibleItemCount", typeof(int), typeof(ColorStrip), new PropertyMetadata(10, new PropertyChangedCallback(VisibleItemCountChanged)));

        /// <summary>
        /// Gets / sets how many <see cref="ColorItemBox"/> objects will be visible on the <see cref="ColorStrip"/>.
        /// </summary>
        public int VisibleItemCount
        {
            get { return (int)this.GetValue(VisibleItemCountProperty); }
            set { this.SetValue(VisibleItemCountProperty, value); }
        }

        private static void VisibleItemCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // ItemCount

        #region ColorPalette

        /// <summary>
        /// Identifies the <see cref="ColorPalette"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColorPaletteProperty = DependencyProperty.Register("ColorPalette", typeof(ColorPalette), typeof(ColorStrip), new PropertyMetadata(new PropertyChangedCallback(ColorPaletteChanged)));

        /// <summary>
        /// Get / set the <see cref="ColorPalette"/> which will be displayed by this <see cref="ColorStrip"/>.
        /// </summary>
        public ColorPalette ColorPalette
        {
            get { return (ColorPalette)this.GetValue(ColorPaletteProperty); }
            set { this.SetValue(ColorPaletteProperty, value); }
        }

        private static void ColorPaletteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorStrip cs = (ColorStrip)obj;

            ColorPalette oldPalette = e.OldValue as ColorPalette;
            if (oldPalette != null)
            {
                oldPalette.PropertyChanged -= cs.ColorPalette_PropertyChanged;
            }
            ColorPalette newPalette = e.NewValue as ColorPalette;

            if (newPalette != null)
            {
                newPalette.PropertyChanged += cs.ColorPalette_PropertyChanged;
            }
            cs.EnsurePanelChildrenSetup();
            cs.PopulateChildren();
            cs.EnsureVisualState();
        }

        #endregion // ColorPalette

        #region SelectedColorItem
        /// <summary>
        /// Gets / sets the <see cref="ColorItem"/> that will be seen as selected.
        /// </summary>
        public ColorItem SelectedColorItem
        {
            get
            {
                return this._selectedColorItem;
            }
            set
            {
                ColorItem oldColorItem = this._selectedColorItem;

                if (this._colorItems.Contains(value) || value == null)
                {
                    if (this._selectedColorItem != null)
                        this._selectedColorItem.SetSelectedInternal(false);

                    this._selectedColorItem = value;

                    if (this._selectedColorItem != null)
                        this._selectedColorItem.SetSelectedInternal(true);

                    if (!this._suppressSelectedColorItem)
                        this.OnSelectedColorItemChanged(oldColorItem, value);
                }

                this.EnsureVisualState();
            }
        }
        #endregion // SelectedColorItem

        #region HoverColorItem
        /// <summary>
        /// Gets / sets the <see cref="ColorItem"/> which is currently hovered.
        /// </summary>
        protected internal ColorItem HoverColorItem
        {
            get
            {
                return this._hoverColorItem;
            }
            set
            {
                if (this._hoverColorItem != value)
                {
                    ColorItem oldColorItem = this._hoverColorItem;

                    if (this._colorItems.Contains(value) || value == null)
                    {
                        if (this._hoverColorItem != null)
                            this._hoverColorItem.SetHoverInternal(false);

                        this._hoverColorItem = value;

                        if (this._hoverColorItem != null)
                            this._hoverColorItem.SetHoverInternal(true);

                        this.OnHoverColorItemChanged(oldColorItem, value);
                    }

                    this.EnsureVisualState();
                }
            }
        }

        #endregion // HoverColorItem

        #region DarknessShift

        /// <summary>
        /// Identifies the <see cref="DarknessShift"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DarknessShiftProperty = DependencyProperty.Register("DarknessShift", typeof(double), typeof(ColorStrip), new PropertyMetadata(new PropertyChangedCallback(DarknessShiftChanged)));

        /// <summary>
        /// Gets / sets how the <see cref="ColorPalette"/> will be calculated.
        /// </summary>
        public double DarknessShift
        {
            get { return (double)this.GetValue(DarknessShiftProperty); }
            set { this.SetValue(DarknessShiftProperty, value); }
        }

        private static void DarknessShiftChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // DarknessShift

        #region ColorItemBoxStyle

        /// <summary>
        /// Identifies the <see cref="ColorItemBoxStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ColorItemBoxStyleProperty = DependencyProperty.Register("ColorItemBoxStyle", typeof(Style), typeof(ColorStrip), new PropertyMetadata(new PropertyChangedCallback(ColorItemBoxStyleChanged)));

        /// <summary>
        /// Gets / sets a style that will be applied to the <see cref="ColorItemBox"/> objects in this control.
        /// </summary>
        public Style ColorItemBoxStyle
        {
            get { return (Style)this.GetValue(ColorItemBoxStyleProperty); }
            set { this.SetValue(ColorItemBoxStyleProperty, value); }
        }

        private static void ColorItemBoxStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ColorStrip cs = (ColorStrip)obj;
            if (cs._colorBoxItems != null)
            {
                foreach (ColorItemBox cib in cs._colorBoxItems)
                {
                    if (cs.ColorItemBoxStyle != cib.Style)
                    {
                        if (cs.ColorItemBoxStyle == null)
                        {
                            cib.ClearValue(ColorItemBox.StyleProperty);
                        }
                        else
                        {
                            cib.Style = cs.ColorItemBoxStyle;
                        }
                    }
                }
            }
        }

        #endregion // ColorItemBoxStyle


        #endregion // Properties

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="ColorStrip"/> class.
        /// </summary>
        static ColorStrip()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorStrip), new FrameworkPropertyMetadata(typeof(ColorStrip)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorStrip"/> class.
        /// </summary>
        public ColorStrip()
        {



            this._colorItems = new List<ColorItem>();
            for (int i = 0; i < this.VisibleItemCount; i++)
            {
                this._colorItems.Add(new ColorItem() { Color = Colors.Transparent, Parent = this });
            }
        }

        #endregion // Constructor

        #region EventHandlers

        void ColorPalette_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.EnsurePanelChildrenSetup();
            this.PopulateChildren();
        }

        #endregion // EventHandlers

        #region Overrides

        #region OnApplyTemplate
        /// <summary>
        /// Builds the visual tree for the <see cref="ColorStrip"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _colorPickerPanel = base.GetTemplateChild("ColorPickerPanel") as Panel;

            this.EnsurePanelChildrenSetup();

            this.PopulateChildren();

            this.EnsureVisualState();

        }
        #endregion // OnApplyTemplate

        #endregion // Overrides

        #region Methods

        #region Private

        #region EnsurePanelChildrenSetup

        private void EnsurePanelChildrenSetup()
        {
            if (this._colorPickerPanel != null)
            {
                _colorBoxItems = new List<ColorItemBox>();

                int childrenCount = this._colorPickerPanel.Children.Count;

                int colorPickerItemCount = 0;

                




                for (int i = 0; i < childrenCount; i++)
                {
                    ColorItemBox cbi = this._colorPickerPanel.Children[i] as ColorItemBox;

                    if (this.ColorItemBoxStyle != null)
                    {
                        cbi.Style = this.ColorItemBoxStyle;
                    }
                    else
                    {

                        if (cbi.Style != null)
                        {

                        cbi.ClearValue(ColorItemBox.StyleProperty);

                        }

                    }

                    if (cbi != null)
                    {
                        this._colorBoxItems.Add(cbi);
                        colorPickerItemCount++;
                    }

                    if (colorPickerItemCount >= this.VisibleItemCount)
                    {
                        break;
                    }
                }

                



                while (colorPickerItemCount < this.VisibleItemCount)
                {
                    ColorItemBox cbi = new ColorItemBox();
                    if (this.ColorItemBoxStyle != null)
                        cbi.Style = this.ColorItemBoxStyle;
                    this._colorPickerPanel.Children.Add(cbi);
                    this._colorBoxItems.Add(cbi);
                    colorPickerItemCount++;
                }
            }
        }

        #endregion // EnsurePanelChildrenSetup

        #region PopulateChildren

        private void PopulateChildren()
        {
            int colorsSet = 0;
            if (_colorBoxItems != null)
            {
                if (this.ColorPalette != null)
                {
                    if (this.ColorPalette.Colors != null)
                    {

                        for (; colorsSet < this.ColorPalette.Colors.Count && colorsSet < this._colorItems.Count; colorsSet++)
                        {
                            this._colorItems[colorsSet].Color = this.ColorPalette.Colors[colorsSet].Color;
                            this._colorBoxItems[colorsSet].ColorItem = this._colorItems[colorsSet];
                        }
                    }

                    Brush defaultColorBrush = new SolidColorBrush(this.ColorPalette.DefaultColor);
                    for (; colorsSet < this.VisibleItemCount; colorsSet++)
                    {
                        this._colorItems[colorsSet].Color = this.ColorPalette.DefaultColor;
                        this._colorBoxItems[colorsSet].ColorItem = this._colorItems[colorsSet];
                    }
                }
                else
                {
                    Brush defaultColorBrush = new SolidColorBrush(Colors.Transparent);
                    for (; colorsSet < this.VisibleItemCount; colorsSet++)
                    {
                        this._colorItems[colorsSet].Color = Colors.Transparent;
                        this._colorBoxItems[colorsSet].ColorItem = this._colorItems[colorsSet];
                    }
                }
            }
        }

        #endregion // PopulateChildren

        #endregion // Private

        #region Protected

        #region EnsureVisualState

        /// <summary>
        /// Ensures that <see cref="ColorStrip"/> is in the correct state.
        /// </summary>
        protected void EnsureVisualState()
        {
            if (this._colorBoxItems != null)
            {
                foreach (ColorItemBox cbi in this._colorBoxItems)
                {
                    cbi.EnsureState();
                }
            }
        }

        #endregion // EnsureVisualState

        #region SetSelectedColorItem

        /// <summary>
        /// Sets <see cref="ColorItem"/> to the selected state.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="fromClick"></param>
        protected internal void SetSelectedColorItem(ColorItem ci, bool fromClick)
        {
            if (this._selectedColorItem != ci)
            {
                ColorItem oldColorItem = this._selectedColorItem;

                if (this._colorItems.Contains(ci) || ci == null)
                {
                    if (this._selectedColorItem != null)
                        this._selectedColorItem.SetSelectedInternal(false);

                    this._selectedColorItem = ci;

                    if (this._selectedColorItem != null)
                        this._selectedColorItem.SetSelectedInternal(true);

                    this.OnSelectedColorItemChanged(oldColorItem, ci);
                }

                this.EnsureVisualState();
            }
        }

        #endregion // SetSelectedColorItem

        #region ClearIsHoverSilent

        /// <summary>
        /// Used when the colorpicker is closing so that all boxes will lose their hover state.
        /// </summary>
        protected internal void ClearIsHoverSilent()
        {
            foreach (ColorItem item in this._colorItems)
            {
                item.SetHoverInternal(false);
            }
            this._hoverColorItem = null;
        }

        #endregion // ClearIsHoverSilent

        #region ClearIsSelectedSilent

        /// <summary>
        /// Clears the selected flags on the <see cref="ColorItem"/> object so that it will reset correctly.
        /// </summary>
        protected internal void ClearIsSelectedSilent()
        {
            foreach (ColorItem item in this._colorItems)
            {
                item.SetSelectedInternal(false);
                item.SetWasSelected(false);
            }
            this._selectedColorItem = null;
        }

        #endregion // ClearIsSelectedSilent

        #endregion // Protected

        #region Internal

        internal void ClearSelectedColorInternal()
        {
            this._suppressSelectedColorItem = true;
            this.SelectedColorItem = null;
            this._suppressSelectedColorItem = false;
        }

        internal ColorItem SetPreviouslySelectedColorInteral(Color color)
        {
            ColorItem retValue = null;

            foreach (ColorItem ci in this._colorItems)
            {
                ci.SetHoverInternal(false);
                if (ci.Color == color && retValue == null)
                {
                    ci.SetWasSelected(true);
                    retValue = ci;
                }
                else
                {
                    ci.SetWasSelected(false);
                }
            }
            return retValue;
        }

        #endregion // Internal

        #endregion // Methods

        #region Events

        #region SelectedColorItemChanged

        /// <summary>
        /// Event raised when a <see cref="ColorItem"/> is selected.
        /// </summary>
        public event EventHandler<SelectedColorItemChangedEventArgs> SelectedColorItemChanged;

        /// <summary>
        /// Raises the <see cref="SelectedColorItemChanged"/> event.
        /// </summary>
        /// <param name="oldColorItem"></param>
        /// <param name="newColorItem"></param>
        protected virtual void OnSelectedColorItemChanged(ColorItem oldColorItem, ColorItem newColorItem)
        {
            if (this.SelectedColorItemChanged != null)
            {
                this.SelectedColorItemChanged(this, new SelectedColorItemChangedEventArgs() { OriginalColorItem = oldColorItem, NewColorItem = newColorItem });
            };
        }


        #endregion // SelectedColorItemChanged

        #region HoverColorItemChanged

        /// <summary>
        /// Event raised when a <see cref="ColorItem"/> is hovered.
        /// </summary>
        public event EventHandler<SelectedColorItemChangedEventArgs> HoverColorItemChanged;

        /// <summary>
        /// Raises the <see cref="HoverColorItemChanged"/> event.
        /// </summary>
        /// <param name="oldColorItem"></param>
        /// <param name="newColorItem"></param>
        protected virtual void OnHoverColorItemChanged(ColorItem oldColorItem, ColorItem newColorItem)
        {
            if (this.HoverColorItemChanged != null)
            {
                this.HoverColorItemChanged(this, new SelectedColorItemChangedEventArgs() { OriginalColorItem = oldColorItem, NewColorItem = newColorItem });
            };
        }
        #endregion // HoverColorItemChanged

        

        #endregion // Events
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