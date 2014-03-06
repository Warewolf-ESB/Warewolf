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
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// Visual object for the <see cref="ConditionalFormattingCell"/> object.
	/// </summary>
	[TemplateVisualState(GroupName = "ConditionalFormatIcon", Name = "ShowIcon")]
	[TemplateVisualState(GroupName = "ConditionalFormatIcon", Name = "HideIcon")]
	public class ConditionalFormattingCellControl : CellControl
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ConditionalFormattingCellControl"/> class.
		/// </summary>
		public ConditionalFormattingCellControl()
		{
			base.DefaultStyleKey = typeof(ConditionalFormattingCellControl);            
		}

		#endregion // Constructor

		#region Properties

        #region Public 
        #region Icon

        /// <summary>
		/// Identifies the <see cref="Icon"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(DataTemplate), typeof(ConditionalFormattingCellControl), new PropertyMetadata(new PropertyChangedCallback(IconChanged)));

		/// <summary>
		/// Get / set a DataTemplate to be used by the <see cref="ConditionalFormattingCellControl"/> for icon based conditional formatting.
		/// </summary>
		public DataTemplate Icon
		{
			get { return (DataTemplate)this.GetValue(IconProperty); }
			set { this.SetValue(IconProperty, value); }
		}

		private static void IconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingCellControl control = (ConditionalFormattingCellControl)obj;

			if (e.NewValue == null)
				VisualStateManager.GoToState(control, "HideConditionalFormattingIcon", false);
			else
				VisualStateManager.GoToState(control, "ShowConditionalFormattingIcon", false);
		}

		#endregion // Icon

		#region BarDirection

		/// <summary>
		/// Identifies the <see cref="BarDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarDirectionProperty = DependencyProperty.Register("BarDirection", typeof(DataBarDirection?), typeof(ConditionalFormattingCellControl), new PropertyMetadata(null, new PropertyChangedCallback(BarDirectionChanged)));

		/// <summary>
		/// Gets / sets the <see cref="DataBarDirection"/> for the databars displayed by this control.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<DataBarDirection>))]
		public DataBarDirection? BarDirection
		{
			get { return (DataBarDirection?)this.GetValue(BarDirectionProperty); }
			set { this.SetValue(BarDirectionProperty, value); }
		}

		private static void BarDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingCellControl control = (ConditionalFormattingCellControl)obj;
			control.EnsureCurrentState();
			control.OnPropertyChanged("BarDirection");
		}

		#endregion // BarDirection

		#region BarPercentage

		/// <summary>
		/// Identifies the <see cref="BarPercentage"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarPercentageProperty = DependencyProperty.Register("BarPercentage", typeof(double?), typeof(ConditionalFormattingCellControl), new PropertyMetadata(new PropertyChangedCallback(BarPercentageChanged)));

		/// <summary>
		/// Gets / sets the value of the data bar.
		/// </summary>
		public double? BarPercentage
		{
			get { return (double?)this.GetValue(BarPercentageProperty); }
			set { this.SetValue(BarPercentageProperty, value); }
		}

		private static void BarPercentageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingCellControl control = (ConditionalFormattingCellControl)obj;
			control.OnPropertyChanged("BarPercentage");
		}

		#endregion // BarPercentage

		#region BarPositiveOrNegative

		/// <summary>
		/// Identifies the <see cref="BarPositiveOrNegative"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarPositiveOrNegativeProperty = DependencyProperty.Register("BarPositiveOrNegative", typeof(DataBarPositiveNegative), typeof(ConditionalFormattingCellControl), new PropertyMetadata(new PropertyChangedCallback(BarPositiveOrNegativeChanged)));

		/// <summary>
		/// Gets / sets which data bar to use to display.
		/// </summary>
		public DataBarPositiveNegative BarPositiveOrNegative
		{
			get { return (DataBarPositiveNegative)this.GetValue(BarPositiveOrNegativeProperty); }
			set { this.SetValue(BarPositiveOrNegativeProperty, value); }
		}

		private static void BarPositiveOrNegativeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingCellControl control = (ConditionalFormattingCellControl)obj;
			control.OnPropertyChanged("BarPositiveOrNegative");
		}

		#endregion // BarPositiveOrNegative

		#region BarBrush

		/// <summary>
		/// Identifies the <see cref="BarBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarBrushProperty = DependencyProperty.Register("BarBrush", typeof(Brush), typeof(ConditionalFormattingCellControl), new PropertyMetadata(new PropertyChangedCallback(BarBrushChanged)));

		/// <summary>
		/// Gets / sets the <see cref="Brush"/> that will be used to color the positive data bar.
		/// </summary>
		public Brush BarBrush
		{
			get { return (Brush)this.GetValue(BarBrushProperty); }
			set { this.SetValue(BarBrushProperty, value); }
		}

		private static void BarBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingCellControl control = (ConditionalFormattingCellControl)obj;
			control.OnPropertyChanged("BarBrush");
		}

		#endregion // BarBrush

		#region NegativeBarBrush

		/// <summary>
		/// Identifies the <see cref="NegativeBarBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty NegativeBarBrushProperty = DependencyProperty.Register("NegativeBarBrush", typeof(Brush), typeof(ConditionalFormattingCellControl), new PropertyMetadata(new PropertyChangedCallback(NegativeBarBrushChanged)));

		/// <summary>
		/// Gets / sets the <see cref="Brush"/> that will be used to color the negative data bar.
		/// </summary>
		public Brush NegativeBarBrush
		{
			get { return (Brush)this.GetValue(NegativeBarBrushProperty); }
			set { this.SetValue(NegativeBarBrushProperty, value); }
		}

		private static void NegativeBarBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ConditionalFormattingCellControl control = (ConditionalFormattingCellControl)obj;
			control.OnPropertyChanged("NegativeBarBrush");
		}

		#endregion // NegativeBarBrush

		#region AltBackground

		/// <summary>
		/// Identifies the <see cref="AltBackground"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AltBackgroundProperty = DependencyProperty.Register("AltBackground", typeof(Brush), typeof(ConditionalFormattingCellControl), new PropertyMetadata(new PropertyChangedCallback(AltBackgroundChanged)));

		/// <summary>
		/// Gets / sets the <see cref="Brush"/> that will be used by the <see cref="CellControl"/> on alternate rows.
		/// </summary>
		public Brush AltBackground
		{
			get { return (Brush)this.GetValue(AltBackgroundProperty); }
			set { this.SetValue(AltBackgroundProperty, value); }
		}

		private static void AltBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			((ConditionalFormattingCellControl)obj).OnPropertyChanged("AltBackground");
		}

		#endregion // AltBackground
        #endregion // Public 
        
        #region CustomTemplateSet
        
        /// <summary>
        /// If the ConditionalFormatting logic assigns a custom control template, we will need to know later.
        /// </summary>
        internal bool CustomTemplateSet { get; set; }

        #endregion // CustomTemplateSet

        #endregion // Properties

        #region Methods

        #region Overrides

        #region EnsureContent
        /// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>	
		protected internal override void EnsureContent()
		{
			base.EnsureContent();

			if (this.Icon != null)
			{
				VisualStateManager.GoToState(this, "ShowConditionalFormattingIcon", false);
			}
			else
			{
				VisualStateManager.GoToState(this, "HideConditionalFormattingIcon", false);
			}

            if (this.BarDirection != null)
            {
                VisualStateManager.GoToState(this, "ShowDataBar", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideDataBar", false);
            }
		}
		#endregion // EnsureContent

		#region EnsureCurrentState

		/// <summary>
        /// Ensures that <see cref="Infragistics.Controls.Grids.Primitives.CellControlBase"/> is in the correct state.
		/// </summary>
        protected internal override void EnsureCurrentState()
        {
            base.EnsureCurrentState();

            if (this.Icon != null)
            {
                VisualStateManager.GoToState(this, "ShowConditionalFormattingIcon", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideConditionalFormattingIcon", false);
            }

            if (this.BarDirection != null)
            {
                VisualStateManager.GoToState(this, "ShowDataBar", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "HideDataBar", false);
            }

            ConditionalFormattingCell cell = this.Cell as ConditionalFormattingCell;
            if (cell != null && !cell.DisplayContent)
            {
                VisualStateManager.GoToState(this, "HideData", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "ShowData", false);
            }
        }

		#endregion // EnsureCurrentState

        #region OnReleased
        /// <summary>
        /// Called when the <see cref="CellBase"/> releases the <see cref="CellControl"/>.
        /// </summary>
        protected internal override void OnReleased(CellBase cell)
        {
            


            ConditionalFormattingCell cfCell = cell as ConditionalFormattingCell;
            if (cfCell != null)
            {
                cfCell.DisplayContent = true;
                cfCell.ConditionalStyle = null;
            }

            VisualStateManager.GoToState(this, "ShowData", false);

            base.OnReleased(cell);

            this.DataContext = null;
            this.ClearValue(ConditionalFormattingCellControl.DataContextProperty);
        }
        #endregion // OnReleased

        #region OnApplyTemplate
        /// <summary>
        /// Builds the visual tree for the <see cref="ConditionalFormattingCellControl"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.EnsureCurrentState();
        }
        #endregion // OnApplyTemplate

        #endregion // Overrides

        #endregion // Methods        
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