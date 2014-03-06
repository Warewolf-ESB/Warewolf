using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// An internal control mechanism which combines multiple <see cref="ColorStrip"/> objects for selection purposes.
    /// </summary>
    public class ColorStripManager : DependencyObject
    {
        #region Members

        ColorItem _selectedColorItem;
        ColorItem _hoverColorItem;
        ObservableCollection<ColorStrip> _colorStrips;

        #endregion // Members

        #region Properties

        #region SelectedColorItem

        /// <summary>
        /// Gets / sets the <see cref="ColorItem"/> which is currently selected.
        /// </summary>
        protected internal ColorItem SelectedColorItem
        {
            get
            {
                return this._selectedColorItem;
            }
            set
            {
               // if (this._selectedColorItem != value)
                {
                    ColorItem oldColorItem = this._selectedColorItem;

                    if (this._selectedColorItem != null)
                    {
                        this._selectedColorItem.SetSelectedInternal(false);
                    }

                    this._selectedColorItem = value;

                    this.OnSelectedColorItemChanged(oldColorItem, value);
                }
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

                    if (this._hoverColorItem != null)
                    {
                        this._hoverColorItem.IsHover = false;
                    }

                    this._hoverColorItem = value;

                    this.OnHoverColorItemChanged(oldColorItem, value);
                }
            }
        }

        #endregion // HoverColorItem

        #region Parent

        /// <summary>
        /// Gets / sets the <see cref="XamColorPicker"/> which owns this <see cref="ColorStripManager"/>.
        /// </summary>
        protected internal XamColorPicker Parent { get; set; }

        #endregion // Parent

        #region ColorStrips

        /// <summary>
        /// Gets / sets the collection of <see cref="ColorStrip"/> objects which are managed by this manager.
        /// </summary>
        public ObservableCollection<ColorStrip> ColorStrips
        {
            get
            {
                if (this._colorStrips == null)
                {
                    this._colorStrips = new ObservableCollection<ColorStrip>();
                    this._colorStrips.CollectionChanged += ColorStrips_CollectionChanged;
                }
                return this._colorStrips;
            }
            set
            {
                if (this._colorStrips != value)
                {
                    if (this._colorStrips != null)
                    {
                        this._colorStrips.CollectionChanged -= ColorStrips_CollectionChanged;
                    }
                    this._colorStrips = value;
                    if (this._colorStrips != null)
                    {
                        this._colorStrips.CollectionChanged += ColorStrips_CollectionChanged;
                    }
                }
            }
        }

        #endregion // ColorStrips

        #endregion // Properties

        #region Methods

        #region Private

        #region AddSelectedItemChanged
        private void AddSelectedItemChanged(ObservableCollection<ColorStrip> strips)
        {
            if (strips != null)
            {
                foreach (ColorStrip cs in strips)
                {
                    cs.SelectedColorItemChanged += ColorStrip_SelectedColorItemChanged;
 
                }
            }
        }

 
        #endregion // AddSelectedItemChanged

        #region RemoveSelectedItemChanged
        private void RemoveSelectedItemChanged(ObservableCollection<ColorStrip> strips)
        {
            if (strips != null)
            {
                foreach (ColorStrip cs in strips)
                {
                    cs.SelectedColorItemChanged -= ColorStrip_SelectedColorItemChanged;
                }
            }
        }
        #endregion // RemoveSelectedItemChanged

        #endregion // Private

        #region Protected

        #region SetPreviouslySelectedColorInternal

        Color? _previouslySelectedColor;
        /// <summary>
        /// Sets the color back to it's original color.
        /// </summary>
        /// <param name="color"></param>
        protected internal void SetPreviouslySelectedColorInternal(Color? color)
        {
            if (color == null && this._selectedColorItem == null)
                return;

            bool justClear = color == null;

            if (justClear)
                this._previouslySelectedColor = null;
            else
                this._previouslySelectedColor = (Color)color;

            foreach (ColorStrip cs in this._colorStrips)
            {
                if (justClear)
                {
                    cs.ClearSelectedColorInternal();
                }
                else
                {
                    ColorItem ci = cs.SetPreviouslySelectedColorInteral((Color)color);
                    if (ci != null)
                    {
                        justClear = true;
                        this._selectedColorItem = ci;
                    }
                }
            }
        }

        #endregion // SetPreviouslySelectedColorInternal

        #region ClearIsHoverSilent

        /// <summary>
        /// Used when the colorpicker is closing so that all boxes will lose their hover state.
        /// </summary>
        protected internal void ClearIsHoverSilent()
        {
            foreach (ColorStrip item in this._colorStrips)
            {
                item.ClearIsHoverSilent();
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
            foreach (ColorStrip item in this._colorStrips)
            {
                item.ClearIsSelectedSilent();
            }
            this._selectedColorItem = null;
        }

        #endregion // ClearIsSelectedSilent

        #endregion // Protected

        #endregion // Methods

        #region Events

        #region HoverColorItemChanged

        /// <summary>
        /// Event raised when a <see cref="ColorItem"/> is hovered.
        /// </summary>
        protected internal event EventHandler<SelectedColorItemChangedEventArgs> HoverColorItemChanged;

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

        #endregion // Events

        #region Event Handlers

        void ColorStrip_SelectedColorItemChanged(object sender, SelectedColorItemChangedEventArgs e)
        {
            this.SelectedColorItem = e.NewColorItem;
        }

        void ColorStrips_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        ColorStrip cs = e.NewItems[i] as ColorStrip;
                        if (cs != null)
                        {
                            cs.SelectedColorItemChanged += ColorStrip_SelectedColorItemChanged;
                            cs.HoverColorItemChanged += ColorStrip_HoverColorItemChanged;

                            cs.Loaded += ColorStrip_Loaded;


                        }
                    }
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        ColorStrip cs = e.OldItems[i] as ColorStrip;
                        if (cs != null)
                        {
                            cs.SelectedColorItemChanged -= ColorStrip_SelectedColorItemChanged;
                            cs.HoverColorItemChanged -= ColorStrip_HoverColorItemChanged;

                            cs.Loaded -= ColorStrip_Loaded;

                        }
                    }
                }
            }
        }

        void ColorStrip_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetPreviouslySelectedColorInternal(this._previouslySelectedColor);
        }


        void ColorStrip_HoverColorItemChanged(object sender, SelectedColorItemChangedEventArgs e)
        {
            this.HoverColorItem = e.NewColorItem;
        }

        #endregion // Event Handlers

        #region Manager Attached Property

        /// <summary>
        /// An attached property for <see cref="ColorStrip"/> objects to associate it with a <see cref="ColorStripManager"/>.
        /// </summary>
        public static readonly DependencyProperty ManagerProperty = DependencyProperty.RegisterAttached("Manager",
            typeof(ColorStripManager),
            typeof(ColorStripManager),
            new PropertyMetadata(ManagerChanged));

        /// <summary>
        /// Sets the Manager attached property.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyValue"></param>
        public static void SetManager(DependencyObject obj, ColorStripManager propertyValue)
        {
            obj.SetValue(ManagerProperty, propertyValue);
        }

        /// <summary>
        /// Gets the value of the Manager attached property.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ColorStripManager GetManager(DependencyObject obj)
        {
            return (ColorStripManager)obj.GetValue(ManagerProperty);
        }

        private static void ManagerChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ColorStrip colorStrip = sender as ColorStrip;
            if (colorStrip != null)
            {
                if (args.OldValue != null)
                {
                    ColorStripManager csm = args.OldValue as ColorStripManager;
                    if (csm != null)
                    {
                        csm.ColorStrips.Remove(colorStrip);
                    }
                }

                if (args.NewValue != null)
                {
                    ColorStripManager csm = args.NewValue as ColorStripManager;
                    if (csm != null)
                    {
                        csm.ColorStrips.Add(colorStrip);
                    }
                }

            }
        }

        #endregion //  Manager Attached Property
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