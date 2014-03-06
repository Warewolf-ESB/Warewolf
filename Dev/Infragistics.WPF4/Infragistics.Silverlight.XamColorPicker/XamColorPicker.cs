using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Controls.Editors.Primitives;
using Infragistics.AutomationPeers;
using Infragistics;
using System.Windows.Automation.Peers;
using System.Diagnostics;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// An editor control which will allow for selection of a color.
    /// </summary>
    [TemplatePart(Name = "Root", Type = typeof(Grid))]
    [TemplatePart(Name = "Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "ColorPickerDialog", Type = typeof(ColorPickerDialog))]
    [TemplatePart(Name = "RootPopupElement", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "CurrentColorStrip", Type = typeof(ColorStrip))]
    [TemplatePart(Name = "StripManager", Type = typeof(ColorStripManager))]
    [TemplatePart(Name = "ColorPalettes", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "RecentColorStrip", Type = typeof(ColorStrip))]
    [TemplatePart(Name = "ColorPalettesButton", Type = typeof(Button))]
    [StyleTypedProperty(Property = "DerivedPaletteColorItemBoxStyle", StyleTargetType = typeof(ColorItemBox))]

    
    

    public class XamColorPicker : Control, INotifyPropertyChanged, ICommandTarget
    {
        #region Members

        FrameworkElement _rootPopupElement;

        bool _allowRecentColorModification = true;
        bool _ignoreIsDropDownChanging;
        ColorPalette _recentColorPalette;
        ColorStrip _currentStrip;
        Panel _derivedColorStripsPanel;
        List<ColorStrip> _derivedColorStrips = new List<ColorStrip>();
        ColorStrip _recentColorsStrip;
        ColorPaletteCollection _colorPalettes;
        ComboBox _colorPalettesCombo;
        bool _isMouseDownInside = false;






        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="XamColorPicker"/> class.
        /// </summary>
        static XamColorPicker()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamColorPicker), new FrameworkPropertyMetadata(typeof(XamColorPicker)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="XamColorPicker"/> class.
        /// </summary>
        public XamColorPicker()
        {





            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamColorPicker), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            this.Loaded += new RoutedEventHandler(XamColorPicker_Loaded);

            this.CurrentPaletteCaption = AdvancedColorShadePicker.GetString("CurrentColorPaletteCaption");
            this.RecentColorPaletteCaption = AdvancedColorShadePicker.GetString("RecentColorPaletteCaption");
            this.DerivedColorPalettesCaption = AdvancedColorShadePicker.GetString("DerivedColorPalettesCaption");
            this.AdvancedButtonCaption = AdvancedColorShadePicker.GetString("AdvancedEditorCaption");
            this.CurrentColorCaption = AdvancedColorShadePicker.GetString("CurrentColorCaption");
        }

        #endregion // Constructor

        #region Methods

        #region Internal

        #region HardSelectNewColor

        /// <summary>
        /// Selects a new color, adding it to the recent color palette.
        /// </summary>
        /// <param name="color"></param>
        internal void HardSelectNewColor(Color? color)
        {
            this.SelectedColor = color;

            if (this.DropDownPopup != null && this.DropDownPopup.IsOpen)
            {
                this.IsDropDownOpen = false;
            }

            if (color != null)
            {
                Color selectedColor = (Color)color;
                if (!this.RecentColorPalette.Colors.Contains(selectedColor))
                    if (this._allowRecentColorModification && !this.RecentColorPalette.Colors.Contains(selectedColor))
                        this.RecentColorPalette.Colors.Add(selectedColor);
            }
        }
        #endregion // HardSelectNewColor

        #region SoftSelectNewColor

        /// <summary>
        /// Selects a color without adding it to the recent colors collection. 
        /// </summary>
        /// <param name="color"></param>
        internal void SoftSelectNewColor(Color? color)
        {
            this._allowRecentColorModification = false;
            if (color == null)
            {
                this.SelectedColor = this.PreviouslySelectedColor;
            }
            else
            {
                this.SelectedColor = color;
            }
            this._allowRecentColorModification = true;
        }

        #endregion // SoftSelectNewColor

        /// <summary>
        /// Gets / sets the <see cref="Color"/> which will be ensured on the control so that the advanced editor is correct.
        /// </summary>
        protected internal Color ColorForAdvancedEditor { get; set; }

        #endregion // Internal

        #region Private

        #region EnsureVisualState

        /// <summary>
        /// Ensures that <see cref="XamColorPicker"/> is in the correct state.
        /// </summary>
        private void EnsureVisualState()
        {
            this.LayoutUpdated -= XamColorPicker_LayoutUpdated;

            if (this.IsDropDownOpen)
            {
                VisualStateManager.GoToState(this, "Open", false);
                this.LayoutUpdated += new EventHandler(XamColorPicker_LayoutUpdated);
            }
            else
            {
                VisualStateManager.GoToState(this, "Closed", false);
            }

            if (this.ColorPalettes.Count > 1)
            {
                VisualStateManager.GoToState(this, "Show", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Hide", false);
            }

            if (this.ShowAdvancedEditorButton)
            {
                VisualStateManager.GoToState(this, "ShowEditorButton", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideEditorButton", false);
            }

            if (this.ShowRecentColorsPalette)
            {
                VisualStateManager.GoToState(this, "ShowRCPalette", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideRCPalette", false);
            }

            if (this.ShowDerivedColorPalettes)
            {
                VisualStateManager.GoToState(this, "ShowDerivedPalettes", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideDerivedPalettes", false);
            }
        }

        #endregion //EnsureVisualState

        #region EnsureDerivedColorStrips

        private void EnsureDerivedColorStrips()
        {
            if (this._derivedColorStripsPanel != null)
            {
                this._derivedColorStripsPanel.Children.Clear();

                if (this.ColorStripManager != null)
                {
                    foreach (ColorStrip cs in this._derivedColorStrips)
                    {
                        this.ColorStripManager.ColorStrips.Remove(cs);
                    }
                }

                this._derivedColorStrips.Clear();

                double darkerStep = 100.0 / (this.DerivedPalettesCount + 2);

                for (int i = 0; i < this.DerivedPalettesCount; i++)
                {
                    ColorStrip cs = new ColorStrip();

                    cs.ColorItemBoxStyle = this.DerivedPaletteColorItemBoxStyle;

                    cs.DarknessShift = Math.Min(darkerStep * (i + 1), 100.0);

                    if (this.ColorStripManager != null)
                    {
                        this.ColorStripManager.ColorStrips.Add(cs);
                    }
                    this._derivedColorStrips.Add(cs);
                    this._derivedColorStripsPanel.Children.Add(cs);
                }
            }
        }

        #endregion // EnsureDerivedColorStrips

        #region SetCurrentColorPalette

        private void SetCurrentColorPalette(ColorPalette cp)
        {
            if (this._currentStrip != null)
            {
                this._currentStrip.ColorPalette = cp;
            }

            this.EnsureDerivedColorStrips();
            if (this.DerivedPalettesCount > 0)
            {
                double darkerStep = 100.0 / this.DerivedPalettesCount;
                for (int i = 0; i < this.DerivedPalettesCount; i++)
                {
                    if (this._derivedColorStrips.Count > 0)
                        this.SetDerivedColorPalettes(cp, this._derivedColorStrips[i]);
                }
            }
            if (this.ColorStripManager != null)
                this.ColorStripManager.SetPreviouslySelectedColorInternal(this.SelectedColor);
        }

        #endregion // SetCurrentColorPalette

        #region SetDerivedColorPalettes

        private void SetDerivedColorPalettes(ColorPalette cp, ColorStrip cs)
        {
            if (cp == null)
            {
                cs.ColorPalette = null;
                return;
            }

            ColorPalette newColorPalette = new ColorPalette();

            foreach (ColorPatch c in cp.Colors)
            {
                HSL hsl = HSL.FromColor(c.Color);

                HSL darkerColor = hsl.Darker(cs.DarknessShift);

                if (darkerColor.H == hsl.H && darkerColor.L == hsl.L && darkerColor.S == hsl.S)
                {
                    darkerColor = hsl.Lighter(cs.DarknessShift);
                }

                newColorPalette.Colors.Add(darkerColor.ToColor());
            }


            cs.ColorPalette = newColorPalette;
        }

        #endregion // SetDerivedColorPalettes

        #region UnhoverAll
        /// <summary>
        /// Called when the dropdown is closed to notify the <see cref="Infragistics.Controls.Editors.Primitives.ColorItemBox"/>s objects that they should unhover.
        /// </summary>
        private void UnhoverAll()
        {
            if (this.ColorStripManager != null)
            {
                this.ColorStripManager.ClearIsHoverSilent();
                this.ColorStripManager.ClearIsSelectedSilent();
            }
        }
        #endregion // UnhoverAll

        #region InvalidateElementOutofPopup



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


        #endregion // InvalidateElementOutofPopup

        #endregion // Private

        #region Static

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources

        #endregion // Static

        #endregion // Methods

        #region Properties

        #region Protected Internal

        #region ColorPickerDialog

        /// <summary>
        /// Gets / sets the <see cref="ColorPickerDialog"/> control that displays the area to select a color.
        /// </summary>
        protected internal ColorPickerDialog ColorPickerDialog
        {
            get;
            set;
        }

        #endregion // ColorPickerDialog

        #region PreviouslySelectedColor

        /// <summary>
        /// Identifies the <see cref="PreviouslySelectedColor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty PreviouslySelectedColorProperty = DependencyProperty.Register("PreviouslySelectedColor", typeof(Color?), typeof(XamColorPicker), new PropertyMetadata(new PropertyChangedCallback(PreviouslySelectedColorChanged)));

        /// <summary>
        /// Gets / sets the color that was selected when the drop down was opened which can be reset if the
        /// editing is cancelled.
        /// </summary>
        /// <remarks>
        /// This property is cleared after a new color is selected.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public Color? PreviouslySelectedColor
        {
            get { return (Color?)this.GetValue(PreviouslySelectedColorProperty); }
            protected set { this.SetValue(PreviouslySelectedColorProperty, value); }
        }

        private static void PreviouslySelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("PreviouslySelectedColor");
            //if (e.NewValue == null)
            //    Debug.WriteLine("null prev color");
            //else
            //Debug.WriteLine(e.NewValue.ToString());
        }

        #endregion // PreviouslySelectedColor

        #region RecentColorPalette

        /// <summary>
        /// Gets a <see cref="ColorPalette"/> object to retain what colors have been recently selected.
        /// </summary>
        protected internal ColorPalette RecentColorPalette
        {
            get
            {
                if (this._recentColorPalette == null)
                {
                    this._recentColorPalette = new ColorPalette();
                    this._recentColorPalette.MaximumColorCount = 10;
                }
                return this._recentColorPalette;
            }
        }

        #endregion // RecentColorPalette

        #region AdvancedEditorPopup

        /// <summary>
        /// Gets / set a <see cref="Popup"/> that shows the advanced editor.
        /// </summary>
        protected internal Popup AdvancedEditorPopup
        {
            get;
            set;
        }

        #endregion // AdvancedEditorPopup

        #region ColorStripManager

        /// <summary>
        /// Get / set the <see cref="ColorStripManager"/> object which controls the interaction between the <see cref="ColorStrip"/>s on the control.
        /// </summary>
        protected internal ColorStripManager ColorStripManager { get; set; }

        #endregion // ColorStripManager

        #region DropDownPopup
        /// <summary>
        /// Gets the <see cref="Popup"/> which displays when you open up the control.
        /// </summary>
        protected Popup DropDownPopup { get; private set; }
        #endregion // DropDownPopup



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

        #endregion // Protected Internal

        #region Public

        #region SelectedColor

        /// <summary>
        /// Identifies the <see cref="SelectedColor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color?), typeof(XamColorPicker), new PropertyMetadata(Colors.Black, new PropertyChangedCallback(SelectedColorPropertyChanged)));

        /// <summary>
        /// Gets / sets the <see cref="Color"/> which is currently selected in the control.
        /// </summary>        
        public Color? SelectedColor
        {
            get { return (Color?)this.GetValue(SelectedColorProperty); }
            set { this.SetValue(SelectedColorProperty, value); }
        }

        private static void SelectedColorPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker cp = (XamColorPicker)obj;
            cp.OnPropertyChanged("SelectedColor");
            if (cp.IsLoaded)
            {
                if (cp.SelectedColor != null)
                {
                    Color selectedColor = (Color)cp.SelectedColor;
                    if (cp._allowRecentColorModification && !cp.RecentColorPalette.Colors.Contains(selectedColor))
                        cp.RecentColorPalette.Colors.Add(selectedColor);
                }

                Color? originalColor = cp.PreviouslySelectedColor;

                if (originalColor == null)
                    originalColor = (Color?)e.OldValue;
                SelectedColorChangedEventArgs args = new SelectedColorChangedEventArgs() { OriginalColor = originalColor, NewColor = (Color?)e.NewValue };
                cp.OnSelectedColorChanged(args);

                XamColorPickerAutomationPeer peer = UIElementAutomationPeer.FromElement(cp) as XamColorPickerAutomationPeer;
                if (null != peer)
                    peer.RaiseValuePropertyChangedEvent(args.OriginalColor, args.NewColor);





            }
        }

        #endregion // SelectedColor

        #region DerivedPalettesCount

        /// <summary>
        /// Identifies the <see cref="DerivedPalettesCount"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DerivedPalettesCountProperty = DependencyProperty.Register("DerivedPalettesCount", typeof(int), typeof(XamColorPicker), new PropertyMetadata(5, new PropertyChangedCallback(DerivedPalettesCountChanged)));

        /// <summary>
        /// Gets / sets how many derived palettes will be created.
        /// </summary>
        public int DerivedPalettesCount
        {
            get { return (int)this.GetValue(DerivedPalettesCountProperty); }
            set { this.SetValue(DerivedPalettesCountProperty, value); }
        }

        private static void DerivedPalettesCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("DerivedPalettesCount");
            xcp.EnsureDerivedColorStrips();
            if (xcp.CurrentPalette != null)
                xcp.SetCurrentColorPalette(xcp.CurrentPalette);
        }

        #endregion // DerivedPalettesCount

        #region AdvancedEditor

        /// <summary>
        /// Identifies the <see cref="AdvancedEditor"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AdvancedEditorProperty = DependencyProperty.Register("AdvancedEditor", typeof(ISupportColorPickerAdvancedEditing), typeof(XamColorPicker), new PropertyMetadata(new PropertyChangedCallback(AdvancedEditorChanged)));

        /// <summary>
        /// Gets / sets an <see cref="ISupportColorPickerAdvancedEditing"/> object which will be used for advanced color editing.
        /// </summary>
        protected ISupportColorPickerAdvancedEditing AdvancedEditor
        {
            get { return (ISupportColorPickerAdvancedEditing)this.GetValue(AdvancedEditorProperty); }
            set { this.SetValue(AdvancedEditorProperty, value); }
        }

        private static void AdvancedEditorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // AdvancedEditor

        #region CurrentPalette

        /// <summary>
        /// Identifies the <see cref="CurrentPalette"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CurrentPaletteProperty = DependencyProperty.Register("CurrentPalette", typeof(ColorPalette), typeof(XamColorPicker), new PropertyMetadata(new PropertyChangedCallback(CurrentPaletteChanged)));

        /// <summary>
        /// Gets / sets the <see cref="ColorPalette"/> object which is currently seen as active.
        /// </summary>
        public ColorPalette CurrentPalette
        {
            get { return (ColorPalette)this.GetValue(CurrentPaletteProperty); }
            set { this.SetValue(CurrentPaletteProperty, value); }
        }

        private static void CurrentPaletteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;

            ColorPalette cp = e.NewValue as ColorPalette;

            ColorPalette oldCp = e.OldValue as ColorPalette;

            if (oldCp != null)
            {
                oldCp.Colors.CollectionChanged -= xcp.Colors_CollectionChanged;
                oldCp.PropertyChanged -= xcp.CurrentPallette_PropertyChanged;
            }

            if (cp != null)
            {
                cp.Colors.CollectionChanged += xcp.Colors_CollectionChanged;
                cp.PropertyChanged += xcp.CurrentPallette_PropertyChanged;
            }

            xcp.SetCurrentColorPalette(xcp.CurrentPalette);
            xcp.OnPropertyChanged("CurrentPalette");
        }

        void CurrentPallette_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.SetCurrentColorPalette(this.CurrentPalette);
        }

        void Colors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SetCurrentColorPalette(this.CurrentPalette);
        }

        #endregion // CurrentPalette

        #region ColorPalettes

        /// <summary>
        /// Gets / sets the <see cref="ColorPaletteCollection"/> which are currently available for selection.
        /// </summary>
        public ColorPaletteCollection ColorPalettes
        {
            get
            {
                if (this._colorPalettes == null)
                {
                    this._colorPalettes = new ColorPaletteCollection();
                    this._colorPalettes.Add(this.StandardPalette);
                    this._colorPalettes.CollectionChanged += ColorPalettes_CollectionChanged;
                }

                return this._colorPalettes;
            }
            set
            {
                if (this._colorPalettes != null)
                {
                    this._colorPalettes.CollectionChanged -= ColorPalettes_CollectionChanged;
                }

                this._colorPalettes = value;

                if (this._colorPalettes != null)
                {
                    this._colorPalettes.CollectionChanged += ColorPalettes_CollectionChanged;
                }
            }
        }

        #endregion // ColorPalettes

        #region ShowAdvancedEditorButton

        /// <summary>
        /// Identifies the <see cref="ShowAdvancedEditorButton"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ShowAdvancedEditorButtonProperty = DependencyProperty.Register("ShowAdvancedEditorButton", typeof(bool), typeof(XamColorPicker), new PropertyMetadata(true, new PropertyChangedCallback(ShowAdvancedEditorButtonChanged)));

        /// <summary>
        /// Gets / sets if the AdvancedEditorButton is displayed.
        /// </summary>
        public bool ShowAdvancedEditorButton
        {
            get { return (bool)this.GetValue(ShowAdvancedEditorButtonProperty); }
            set { this.SetValue(ShowAdvancedEditorButtonProperty, value); }
        }

        private static void ShowAdvancedEditorButtonChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("ShowAdvancedEditorButton");
            xcp.EnsureVisualState();
        }

        #endregion // ShowAdvancedEditorButton

        #region ShowRecentColorsPalette

        /// <summary>
        /// Identifies the <see cref="ShowRecentColorsPalette"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ShowRecentColorsPaletteProperty = DependencyProperty.Register("ShowRecentColorsPalette", typeof(bool), typeof(XamColorPicker), new PropertyMetadata(true, new PropertyChangedCallback(ShowRecentColorsPaletteChanged)));

        /// <summary>
        /// Gets / sets if the <see cref="RecentColorPalette"/> is displayed.
        /// </summary>
        public bool ShowRecentColorsPalette
        {
            get { return (bool)this.GetValue(ShowRecentColorsPaletteProperty); }
            set { this.SetValue(ShowRecentColorsPaletteProperty, value); }
        }

        private static void ShowRecentColorsPaletteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("ShowRecentColorsPalette");
            xcp.EnsureVisualState();
        }

        #endregion // ShowRecentColorsPalette

        #region ShowDerivedColorPalettes

        /// <summary>
        /// Identifies the <see cref="ShowDerivedColorPalettes"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ShowDerivedColorPalettesProperty = DependencyProperty.Register("ShowDerivedColorPalettes", typeof(bool), typeof(XamColorPicker), new PropertyMetadata(true, new PropertyChangedCallback(ShowDerivedColorPalettesChanged)));

        /// <summary>
        /// Gets / sets if the Derived Palettes are displayed.
        /// </summary>
        public bool ShowDerivedColorPalettes
        {
            get { return (bool)this.GetValue(ShowDerivedColorPalettesProperty); }
            set { this.SetValue(ShowDerivedColorPalettesProperty, value); }
        }

        private static void ShowDerivedColorPalettesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("ShowDerivedColorPalettes");
            xcp.EnsureVisualState();
        }

        #endregion // ShowDerivedColorPalettes

        #region Captions

        #region CurrentPaletteCaption

        /// <summary>
        /// Identifies the <see cref="CurrentPaletteCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CurrentPaletteCaptionProperty = DependencyProperty.Register("CurrentPaletteCaption", typeof(string), typeof(XamColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(CurrentPaletteCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the current palette.
        /// </summary>
        public string CurrentPaletteCaption
        {
            get { return (string)this.GetValue(CurrentPaletteCaptionProperty); }
            set { this.SetValue(CurrentPaletteCaptionProperty, value); }
        }

        private static void CurrentPaletteCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("CurrentPaletteCaption");
        }

        #endregion // CurrentPaletteCaption

        #region RecentColorPaletteCaption

        /// <summary>
        /// Identifies the <see cref="RecentColorPaletteCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty RecentColorPaletteCaptionProperty = DependencyProperty.Register("RecentColorPaletteCaption", typeof(string), typeof(XamColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(RecentColorPaletteCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the recent color palette.
        /// </summary>
        public string RecentColorPaletteCaption
        {
            get { return (string)this.GetValue(RecentColorPaletteCaptionProperty); }
            set { this.SetValue(RecentColorPaletteCaptionProperty, value); }
        }

        private static void RecentColorPaletteCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("RecentColorPaletteCaption");
        }

        #endregion // RecentColorPaletteCaption

        #region DerivedColorPalettesCaption

        /// <summary>
        /// Identifies the <see cref="DerivedColorPalettesCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DerivedColorPalettesCaptionProperty = DependencyProperty.Register("DerivedColorPalettesCaption", typeof(string), typeof(XamColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(DerivedColorPalettesCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the current palette.
        /// </summary>
        public string DerivedColorPalettesCaption
        {
            get { return (string)this.GetValue(DerivedColorPalettesCaptionProperty); }
            set { this.SetValue(DerivedColorPalettesCaptionProperty, value); }
        }

        private static void DerivedColorPalettesCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("DerivedColorPalettesCaption");
        }

        #endregion // DerivedColorPalettesCaption

        #region AdvancedButtonCaption

        /// <summary>
        /// Identifies the <see cref="AdvancedButtonCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AdvancedButtonCaptionProperty = DependencyProperty.Register("AdvancedButtonCaption", typeof(string), typeof(XamColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(AdvancedButtonCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the advanced button.
        /// </summary>
        public string AdvancedButtonCaption
        {
            get { return (string)this.GetValue(AdvancedButtonCaptionProperty); }
            set { this.SetValue(AdvancedButtonCaptionProperty, value); }
        }

        private static void AdvancedButtonCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("AdvancedButtonCaption");
        }

        #endregion // AdvancedButtonCaption

        #region CurrentColorCaption

        /// <summary>
        /// Identifies the <see cref="CurrentColorCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CurrentColorCaptionProperty = DependencyProperty.Register("CurrentColorCaption", typeof(string), typeof(XamColorPicker), new PropertyMetadata(null, new PropertyChangedCallback(CurrentColorCaptionChanged)));

        /// <summary>
        /// Gets / sets the caption for the current color area.
        /// </summary>
        public string CurrentColorCaption
        {
            get { return (string)this.GetValue(CurrentColorCaptionProperty); }
            set { this.SetValue(CurrentColorCaptionProperty, value); }
        }

        private static void CurrentColorCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;
            xcp.OnPropertyChanged("CurrentColorCaption");
        }

        #endregion // CurrentColorCaption

        #endregion // Captions

        #region DerivedPaletteColorItemBoxStyle

        /// <summary>
        /// Identifies the <see cref="DerivedPaletteColorItemBoxStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty DerivedPaletteColorItemBoxStyleProperty = DependencyProperty.Register("DerivedPaletteColorItemBoxStyle", typeof(Style), typeof(XamColorPicker), new PropertyMetadata(new PropertyChangedCallback(DerivedPaletteColorItemBoxStyleChanged)));

        /// <summary>
        /// Gets / sets the style applied to the <see cref="ColorItemBox"/> objects on the derived palettes.
        /// </summary>
        public Style DerivedPaletteColorItemBoxStyle
        {
            get { return (Style)this.GetValue(DerivedPaletteColorItemBoxStyleProperty); }
            set { this.SetValue(DerivedPaletteColorItemBoxStyleProperty, value); }
        }

        private static void DerivedPaletteColorItemBoxStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker xcp = (XamColorPicker)obj;

            foreach (ColorStrip cs in xcp._derivedColorStrips)
            {
                cs.ColorItemBoxStyle = xcp.DerivedPaletteColorItemBoxStyle;
            }
        }

        #endregion // DerivedPaletteColorItemBoxStyle

        #endregion // Public

        #endregion // Properties

        #region Overrides

        #region OnApplyTemplate
        /// <summary>
        /// Builds the visual tree for the <see cref="XamColorPicker"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UIElement rootVisual = PlatformProxy.GetRootVisual(this);
            // We can't do this in constructor, because at that point RootVisual is null
            if (rootVisual != null)
            {
                rootVisual.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonDown));                
                rootVisual.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(this.KeyDownHandler));
                rootVisual.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.RootVisual_MouseLeftButtonUp));
            }

            Grid rootElement = base.GetTemplateChild("Root") as Grid;
            if (rootElement != null)
            {
                this.ColorStripManager = rootElement.Resources["StripManager"] as ColorStripManager;
                if (this.ColorStripManager != null)
                {
                    this.ColorStripManager.Parent = this;
                    this.ColorStripManager.SelectedColorItemChanged += ColorStripManager_SelectedColorItemChanged;
                    this.ColorStripManager.HoverColorItemChanged += ColorStripManager_HoverColorItemChanged;
                }

                this.DropDownPopup = this.GetTemplateChild("Popup") as Popup;
                if (this.DropDownPopup != null)
                {

                    



                    this.DropDownPopup.StaysOpen = false;
                    this.DropDownPopup.AllowsTransparency = true;
                    this.DropDownPopup.Placement = PlacementMode.Bottom;
                    this.DropDownPopup.PlacementTarget = this;

                    this.DropDownPopup.MouseDown -= DropDownPopup_MouseDown;
                    this.DropDownPopup.MouseDown += DropDownPopup_MouseDown;

                    this.DropDownPopup.Closed -= DropDownPopup_Closed;
                    this.DropDownPopup.Closed += DropDownPopup_Closed;


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

                }
            }

            Button colorPalettesDropDownButton = base.GetTemplateChild("ColorPalettesButton") as Button;
            if (colorPalettesDropDownButton != null)
                colorPalettesDropDownButton.Click += new RoutedEventHandler(ColorPalettesDropDownButton_Click);

            this._colorPalettesCombo = base.GetTemplateChild("ColorPalettes") as ComboBox;
            if (this._colorPalettesCombo != null)
            {
                this._colorPalettesCombo.KeyDown += new KeyEventHandler(ColorPalettesCombo_KeyDown);
            }

            this.ColorPickerDialog = this.GetTemplateChild("ColorPickerDialog") as ColorPickerDialog;

            this._rootPopupElement = this.GetTemplateChild("RootPopupElement") as FrameworkElement;

            this._currentStrip = base.GetTemplateChild("CurrentColorStrip") as ColorStrip;

            this._derivedColorStripsPanel = base.GetTemplateChild("DerivedPalettes") as Panel;

            EnsureDerivedColorStrips();

            if (this.CurrentPalette == null && this.ColorPalettes.Count > 0)
                this.CurrentPalette = this.ColorPalettes[0];
            else
            {
                this.SetCurrentColorPalette(this.CurrentPalette);
            }

            this._recentColorsStrip = base.GetTemplateChild("RecentColorStrip") as ColorStrip;
            if (this._recentColorsStrip != null)
                this._recentColorsStrip.ColorPalette = this.RecentColorPalette;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            this.EnsureVisualState();
        }

        #endregion // OnApplyTemplate

        #region OnCreateAutomationPeer

        /// <summary>
        ///     When implemented in a derived class, returns class-specific System.Windows.Automation.Peers.AutomationPeer
        ///     implementations for the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        ///    The class-specific System.Windows.Automation.Peers.AutomationPeer subclass
        ///     to return.        
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamColorPickerAutomationPeer(this);
        }
        #endregion // OnCreateAutomationPeer

        #region OnKeyDown
        /// <summary>
        ///     Invoked when an unhandled System.Windows.Input.Keyboard.KeyDown attached
        ///     event reaches an element in its route that is derived from this class. Implement
        ///     this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The System.Windows.Input.KeyEventArgs that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            this.KeyDownHandler(this, e);
            base.OnKeyDown(e);
        }
        #endregion // OnKeyDown

        #endregion // Overrides

        #region Events

        #region SelectedColorChanged

        /// <summary>
        /// Event raised when the <see cref="SelectedColor"/> is changed.
        /// </summary>
        public event EventHandler<SelectedColorChangedEventArgs> SelectedColorChanged;

        /// <summary>
        /// Raises the <see cref="SelectedColorChanged"/> event.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSelectedColorChanged(SelectedColorChangedEventArgs args)
        {
            if (this.SelectedColorChanged != null)
            {
                this.SelectedColorChanged(this, args);
            }
        }

        #endregion // SelectedColorChanged

        #region ColorPalettes_CollectionChanged

        void ColorPalettes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.ColorStripManager != null)
            {
                if (this.ColorPalettes == null || this.ColorPalettes.Count == 0)
                {
                    this.ColorStripManager.ClearIsHoverSilent();
                    this.ColorStripManager.ClearIsSelectedSilent();
                    foreach (ColorStrip cs in this.ColorStripManager.ColorStrips)
                    {
                        cs.ColorPalette = null;
                    }
                    this.CurrentPalette = null;
                    this._recentColorPalette = null;
                    this.SelectedColor = null;
                }
                else
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && this.ColorPalettes.Count == 1)
                    {
                        if (this.CurrentPalette == null)
                        {
                            ColorPalette colorPalette= this.ColorPalettes[0];



                            this.CurrentPalette = colorPalette; 



                        }
                    }
                    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    {
                        if (e.OldItems != null)
                        {
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                ColorPalette cp = (ColorPalette)e.OldItems[i];
                                if (cp == this.CurrentPalette)
                                {
                                    if (this.ColorPalettes.Count > 0)
                                    {
                                        this.CurrentPalette = this.ColorPalettes[0];
                                    }
                                    else
                                    {
                                        this.CurrentPalette = null;
                                    }

                                    if (this.SelectedColor != null)
                                    {
                                        if (this.CurrentPalette == null)
                                            this.SelectedColor = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.EnsureVisualState();
        }

        #endregion // ColorPalettes_CollectionChanged

        #endregion // Events

        #region Event Handlers

        #region ColorStripManager_SelectedColorItemChanged

        void ColorStripManager_SelectedColorItemChanged(object sender, SelectedColorItemChangedEventArgs e)
        {
            Color? newcolor = (e.NewColorItem != null) ? (Color?)e.NewColorItem.Color : null;
            this.HardSelectNewColor(newcolor);
        }

        #endregion // ColorStripManager_SelectedColorItemChanged

        #region XamColorPicker_Loaded

        void XamColorPicker_Loaded(object sender, RoutedEventArgs e)
        {



        }

        #endregion // XamColorPicker_Loaded



#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)



        void DropDownPopup_Closed(object sender, EventArgs e)
        {
            if (!this._isMouseDownInside)
            {
                Color? prevColor = this.PreviouslySelectedColor;
                this.IsDropDownOpen = false;
                if (!this.IsDropDownOpen && prevColor != null)
                {
                    



                    this.PreviouslySelectedColor = prevColor;
                    this.SelectedColor = prevColor;
                    this.PreviouslySelectedColor = null;
                }
            }
        }


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
                    if (this.PreviouslySelectedColor != null)
                        this.SelectedColor = this.PreviouslySelectedColor;
                    bool handled = false;
                    if (this.ColorPickerDialog != null && this.ColorPickerDialog.IsOpen)
                    {
                        handled = true;
                        this.ColorPickerDialog.IsOpen = false;
                    }
                    if (this.IsDropDownOpen)
                    {
                        handled = true;
                        this.IsDropDownOpen = false;
                    }
                    e.Handled = handled;
                    return;
                }
                else if (e.Key == Key.Space || (e.Key == Key.Down && ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)))
                {
                    this.IsDropDownOpen = true;
                    e.Handled = true;
                    return;
                }
            }
        }
        #endregion // KeyDown

        #region ColorStripManager_HoverColorItemChanged

        void ColorStripManager_HoverColorItemChanged(object sender, SelectedColorItemChangedEventArgs e)
        {
            Color? newcolor = (e.NewColorItem != null) ? e.NewColorItem.Color : this.PreviouslySelectedColor;

            this.SoftSelectNewColor(newcolor);
        }

        #endregion // ColorStripManager_HoverColorItemChanged

        #region RootVisual_MouseLeftButtonUp
        void RootVisual_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this._isMouseDownInside = false;
        }
        #endregion // RootVisual_MouseLeftButtonUp

        #region Parent_MouseLeftButtonDown

        void RootVisual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Validate that the item clicked, was ourself
            bool found = false;
            DependencyObject focused = e.OriginalSource as DependencyObject;

            if (focused is UIElement)
            {

            while (focused != null)
            {
                if (object.ReferenceEquals(focused, this))
                {
                    found = true;
                    break;
                }

                // This helps deal with popups that may not be in the same visual tree
                DependencyObject parent = VisualTreeHelper.GetParent(focused);
                if (parent == null)
                {
                    // Try the logical parent.
                    FrameworkElement element = focused as FrameworkElement;
                    if (element != null)
                    {
                        parent = element.Parent;
                    }
                }

                focused = parent;
            }

            }

            this._isMouseDownInside = found;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion //Parent_MouseLeftButtonDown

        #region XamColorPicker_LayoutUpdated
        void XamColorPicker_LayoutUpdated(object sender, EventArgs e)
        {
            this.InvalidateDropDownPosition();



        }
        #endregion // XamColorPicker_LayoutUpdated

        #region ColorPalettesDropDownButton_Click

        void ColorPalettesDropDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._colorPalettesCombo != null)
                this._colorPalettesCombo.IsDropDownOpen = !this._colorPalettesCombo.IsDropDownOpen;
        }

        #endregion // ColorPalettesDropDownButton_Click

        #region ColorPalettesCombo_KeyDown

        void ColorPalettesCombo_KeyDown(object sender, KeyEventArgs e)
        {
            if (!this._colorPalettesCombo.IsDropDownOpen)
            {
                this.KeyDownHandler(this._colorPalettesCombo, e);
            }
        }



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


        #endregion // ColorPalettesCombo_KeyDown


        
        void DropDownPopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }



        #endregion // Event Handlers

        #region DropDown Code

        #region IsDropDownOpen

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(XamColorPicker), new PropertyMetadata(new PropertyChangedCallback(XamColorPicker.OnIsDropDownOpenChanged)));

        /// <summary>
        /// Gets / sets if the dropdown is open.
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)this.GetValue(IsDropDownOpenProperty); }
            set { this.SetValue(IsDropDownOpenProperty, value); }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamColorPicker combo = (XamColorPicker)d;

            if ((!combo.IsLoaded && ((bool)e.NewValue)) || (combo.ColorPickerDialog != null && combo.ColorPickerDialog.IsOpen && combo.ColorPickerDialog.ActualHeight > 0.0))
            {
                combo._ignoreIsDropDownChanging = true;
                combo.IsDropDownOpen = false;
                combo._ignoreIsDropDownChanging = false;
            }
            else
            {
                combo.EnsureVisualState();

                if (!combo._ignoreIsDropDownChanging)
                {
                    if ((bool)e.NewValue) // Opening
                    {
                        if (!combo.OnDropDownOpening())
                        {
                            UIElement rootVisual = PlatformProxy.GetRootVisual(combo);
                            if (rootVisual != null)
                            {
                                rootVisual.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(combo.RootVisual_MouseLeftButtonDown), true);
                                rootVisual.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(combo.KeyDownHandler), true);
                                rootVisual.AddHandler(UIElement.MouseLeftButtonUpEvent,new MouseButtonEventHandler(combo.RootVisual_MouseLeftButtonUp),true);
                            }

                            combo.InvalidateDropDownPosition();
                            if (combo.CurrentPalette == null && combo.ColorPalettes.Count > 0)
                            {
                                combo.CurrentPalette = combo.ColorPalettes[0];
                            }
                            combo.OnDropDownOpened();




                        }
                        else
                        {
                            combo._ignoreIsDropDownChanging = true;
                            combo.IsDropDownOpen = false;
                            combo._ignoreIsDropDownChanging = false;
                        }
                    }
                    else // Closing
                    {
                        if (!combo.OnDropDownClosing())
                        {
                            UIElement rootVisual = PlatformProxy.GetRootVisual(combo);
                            if (rootVisual != null)
                            {
                                rootVisual.RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(combo.RootVisual_MouseLeftButtonDown));                                
                                rootVisual.RemoveHandler(UIElement.KeyDownEvent, new KeyEventHandler(combo.KeyDownHandler));
                                rootVisual.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(combo.RootVisual_MouseLeftButtonUp));
                            }
                            combo.OnDropDownClosed();
                        }
                        else
                        {
                            combo._ignoreIsDropDownChanging = true;
                            combo.IsDropDownOpen = true;
                            combo._ignoreIsDropDownChanging = false;
                        }
                    }
                }
            }
            combo.OnPropertyChanged("IsDropDownOpen");
        }

        #endregion // IsDropDownOpen

        #region DropDownOpening

        /// <summary>
        /// Occurs when the value of the IsDropDownOpen property is changing from false to true. 
        /// </summary>
        public event EventHandler<CancelEventArgs> DropDownOpening;

        /// <summary>
        /// Called before the DropDownOpening event occurs.
        /// </summary>
        protected virtual bool OnDropDownOpening()
        {
            bool returnValue = false;
            if (this.IsLoaded)
            {
                if (this.DropDownOpening != null)
                {
                    CancelEventArgs e = new CancelEventArgs();
                    this.DropDownOpening(this, e);
                    returnValue = e.Cancel;
                }
                if (!returnValue)
                {
                    this.PreviouslySelectedColor = this.SelectedColor;
                }
            }
            return returnValue;
        }

        #endregion //DropDownOpening

        #region DropDownOpened

        /// <summary>
        /// Occurs when the value of the IsDropDownOpen property has changed from false to true and the drop-down is open.
        /// </summary>
        public event EventHandler DropDownOpened;

        /// <summary>
        /// Called before the DropDownOpened event occurs.
        /// </summary>
        protected virtual void OnDropDownOpened()
        {
            if (this.DropDownOpened != null)
            {
                this.DropDownOpened(this, EventArgs.Empty);
            }
            this.ColorStripManager.SetPreviouslySelectedColorInternal(this.SelectedColor);
        }

        #endregion //DropDownOpened

        #region DropDownClosing

        /// <summary>
        /// Occurs when the IsDropDownOpen property is changing from true to false. 
        /// </summary>
        public event EventHandler<CancelEventArgs> DropDownClosing;

        /// <summary>
        /// Called before the DropDownClosing event occurs.
        /// </summary>
        protected virtual bool OnDropDownClosing()
        {
            if (this.DropDownClosing != null)
            {
                CancelEventArgs e = new CancelEventArgs();
                this.DropDownClosing(this, e);
                return e.Cancel;
            }

            return false;
        }

        #endregion //DropDownClosing

        #region DropDownClosed

        /// <summary>
        /// Occurs when the IsDropDownOpen property was changed from true to false and the drop-down is closed.
        /// </summary>
        public event EventHandler DropDownClosed;

        /// <summary>
        /// Called before the DropDownClosed event occurs.
        /// </summary>
        protected virtual void OnDropDownClosed()
        {
            if (this.DropDownClosed != null)
            {
                this.DropDownClosed(this, EventArgs.Empty);
            }

            this.UnhoverAll();

            



            this.ColorForAdvancedEditor = this.PreviouslySelectedColor == null ? Colors.Transparent : (Color)this.PreviouslySelectedColor;
            this.PreviouslySelectedColor = null;
        }

        #endregion // DropDownClosed

        #region InvalidateDropDownPosition
        internal void InvalidateDropDownPosition()
        {



#region Infragistics Source Cleanup (Region)



























































#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion // InvalidateDropDownPosition

        #endregion // DropDown Code

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

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return command is PickerCommandBase;
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this;
        }

        #endregion

        #region Default Color Palettes
        ColorPalette _standardPalette;
        /// <summary>
        /// 
        /// </summary>
        private ColorPalette StandardPalette
        {
            get
            {
                if (this._standardPalette == null)
                {
                    this._standardPalette = new ColorPalette();
                    this._standardPalette.Colors.Add(Colors.White);
                    this._standardPalette.Colors.Add(Colors.Black);
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0xee, 0xec, 0xe1));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0x1f, 0x49, 0x7d));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0x4f, 0x8a, 0xbd));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0xc0, 0x50, 0x4d));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0x9b, 0xbb, 0x59));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0x80, 0x64, 0xa2));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0x4b, 0xac, 0xc6));
                    this._standardPalette.Colors.Add(Color.FromArgb(0xff, 0xf7, 0x96, 0x46));

                    this._standardPalette.DefaultColor = Colors.Transparent;
                }
                return this._standardPalette;
            }
        }

        #endregion // Default Color Palettes
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