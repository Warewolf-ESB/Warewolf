using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A <see cref="ContentControl"/> which is used during conditional formatting to display data bars.
	/// </summary>
	//[TemplatePart(Type = typeof(Grid), Name = "UnidirectionalDataBar")]
	//[TemplatePart(Type = typeof(Grid), Name = "BidirectionalDataBar")]

	[TemplatePart(Type = typeof(ColumnDefinition), Name = "UniDataBar")]
	[TemplatePart(Type = typeof(ColumnDefinition), Name = "BiNegativeColumn")]
	[TemplatePart(Type = typeof(ColumnDefinition), Name = "BiPositiveColumn")]
	[TemplatePart(Type = typeof(ColumnDefinition), Name = "UniDataBarSpacer")]
	[TemplatePart(Type = typeof(ColumnDefinition), Name = "BiNegativeColumnSpacer")]
	[TemplatePart(Type = typeof(ColumnDefinition), Name = "BiPositiveColumnSpacer")]
	public class DataBarControl : ContentControl, INotifyPropertyChanged
	{
		#region Members
		//Grid _unidirectionalDataBar;
		//Grid _bidirectionalDataBar;

		ColumnDefinition _uniDataBar;
		ColumnDefinition _uniDataBarSpacer;
		ColumnDefinition _biNegativeColumn;
		ColumnDefinition _biPositiveColumn;
		ColumnDefinition _biNegativeColumnSpacer;
		ColumnDefinition _biPositiveColumnSpacer;
		#endregion // Members

		#region Constructor

        /// <summary>
        /// Static constructor for the <see cref="DataBarControl"/> class.
        /// </summary>
        static DataBarControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataBarControl), new FrameworkPropertyMetadata(typeof(DataBarControl)));
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataBarControl"/> class.
		/// </summary>
		public DataBarControl()
		{



		}

		#endregion // Constructor

		#region Properties

		#region DataBarDirection

		/// <summary>
		/// Identifies the <see cref="DataBarDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DataBarDirectionProperty = DependencyProperty.Register("DataBarDirection", typeof(DataBarDirection?), typeof(DataBarControl), new PropertyMetadata(null, new PropertyChangedCallback(DataBarDirectionChanged)));

		/// <summary>
		/// Gets / sets the <see cref="DataBarDirection"/> for the databars displayed by this control.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<DataBarDirection>))]
		public DataBarDirection? DataBarDirection
		{
			get { return (DataBarDirection?)this.GetValue(DataBarDirectionProperty); }
			set { this.SetValue(DataBarDirectionProperty, value); }
		}

		private static void DataBarDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DataBarControl dbc = (DataBarControl)obj;
			dbc.SetState_DataBarDirection();
			dbc.SetupDataBars();
			dbc.OnPropertyChanged("DataBarDirection");
		}

		#endregion // DataBarDirection

		#region NegativeValueBarBrush

		/// <summary>
		/// Identifies the <see cref="NegativeValueBarBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty NegativeValueBarBrushProperty = DependencyProperty.Register("NegativeValueBarBrush", typeof(Brush), typeof(DataBarControl), new PropertyMetadata(new PropertyChangedCallback(NegativeValueBarBrushChanged)));

		/// <summary>
		/// Gets / sets the <see cref="Brush"/> that will be used to color the negative data bar.
		/// </summary>
		public Brush NegativeValueBarBrush
		{
			get { return (Brush)this.GetValue(NegativeValueBarBrushProperty); }
			set { this.SetValue(NegativeValueBarBrushProperty, value); }
		}

		private static void NegativeValueBarBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DataBarControl dbc = (DataBarControl)obj;
			dbc.SetState_DataBarDirection();
			dbc.SetupDataBars();
			dbc.SetupUnidirectionalBrush();
			dbc.OnPropertyChanged("NegativeValueBar");
		}

		#endregion // NegativeValueBarBrush

		#region PositiveValueBarBrush

		/// <summary>
		/// Identifies the <see cref="PositiveValueBarBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty PositiveValueBarBrushProperty = DependencyProperty.Register("PositiveValueBarBrush", typeof(Brush), typeof(DataBarControl), new PropertyMetadata(new PropertyChangedCallback(PositiveValueBarBrushChanged)));

		/// <summary>
		/// Gets / sets the <see cref="Brush"/> that will be used to color the positive data bar.
		/// </summary>
		public Brush PositiveValueBarBrush
		{
			get { return (Brush)this.GetValue(PositiveValueBarBrushProperty); }
			set { this.SetValue(PositiveValueBarBrushProperty, value); }
		}

		private static void PositiveValueBarBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DataBarControl dbc = (DataBarControl)obj;
			dbc.SetState_DataBarDirection();
			dbc.SetupDataBars();
			dbc.SetupUnidirectionalBrush();
			dbc.OnPropertyChanged("PositiveValueBarBrush");
		}

		#endregion // PositiveValueBarBrush

		#region BarHeight

		/// <summary>
		/// Identifies the <see cref="BarHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarHeightProperty = DependencyProperty.Register("BarHeight", typeof(double), typeof(DataBarControl), new PropertyMetadata(new PropertyChangedCallback(BarHeightChanged)));

		/// <summary>
		/// Gets / sets the height of the databar. 
		/// </summary>
		public double BarHeight
		{
			get { return (double)this.GetValue(BarHeightProperty); }
			set { this.SetValue(BarHeightProperty, value); }
		}

		private static void BarHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DataBarControl dbc = (DataBarControl)obj;
			dbc.SetState_DataBarDirection();
			dbc.SetupDataBars();
			dbc.OnPropertyChanged("BarHeight");
		}

		#endregion // BarHeight

		#region BarPercentage

		/// <summary>
		/// Identifies the <see cref="BarPercentage"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty BarPercentageProperty = DependencyProperty.Register("BarPercentage", typeof(double?), typeof(DataBarControl), new PropertyMetadata(new PropertyChangedCallback(BarPercentageChanged)));

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
			DataBarControl dbc = (DataBarControl)obj;
			dbc.SetupDataBars();
		}

		#endregion // BarPercentage

		#region DataBarPositiveNegative

		/// <summary>
		/// Identifies the <see cref="DataBarPositiveNegative"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DataBarPositiveNegativeProperty = DependencyProperty.Register("DataBarPositiveNegative", typeof(DataBarPositiveNegative), typeof(DataBarControl), new PropertyMetadata(new PropertyChangedCallback(DataBarPositiveNegativeChanged)));

		/// <summary>
		/// Gets / sets which data bar to use to display.
		/// </summary>
		public DataBarPositiveNegative DataBarPositiveNegative
		{
			get { return (DataBarPositiveNegative)this.GetValue(DataBarPositiveNegativeProperty); }
			set { this.SetValue(DataBarPositiveNegativeProperty, value); }
		}

		private static void DataBarPositiveNegativeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DataBarControl dbc = (DataBarControl)obj;
			dbc.SetupDataBars();
			dbc.SetupUnidirectionalBrush();
			dbc.OnPropertyChanged("DataBarPositiveNegative");
		}

		#endregion // DataBarPositiveNegative

		#region UnidirectionalDataBarBrush

		/// <summary>
		/// Identifies the <see cref="UnidirectionalDataBarBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty UnidirectionalDataBarBrushProperty = DependencyProperty.Register("UnidirectionalDataBarBrush", typeof(Brush), typeof(DataBarControl), new PropertyMetadata(new PropertyChangedCallback(UnidirectionalDataBarBrushChanged)));

		/// <summary>
		/// Gets / sets the <see cref="Brush"/> which is used when only a single databar is being used.
		/// </summary>
		public Brush UnidirectionalDataBarBrush
		{
			get { return (Brush)this.GetValue(UnidirectionalDataBarBrushProperty); }
			protected set { this.SetValue(UnidirectionalDataBarBrushProperty, value); }
		}

		private static void UnidirectionalDataBarBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			DataBarControl dbc = (DataBarControl)obj;
			dbc.OnPropertyChanged("UnidirectionalDataBarBrush");
		}

		#endregion // UnidirectionalDataBarBrush

		#endregion // Properties

		#region Overrides

		/// <summary>
		/// Builds the visual tree for the <see cref="DataBarControl"/>.
		/// </summary>
		public override void OnApplyTemplate()
		{
			//this._unidirectionalDataBar = base.GetTemplateChild("UnidirectionalDataBar") as Grid;
			//this._bidirectionalDataBar = base.GetTemplateChild("BidirectionalDataBar") as Grid;
			this._uniDataBar = base.GetTemplateChild("UniDataBar") as ColumnDefinition;
			this._uniDataBarSpacer = base.GetTemplateChild("UniDataBarSpacer") as ColumnDefinition;
			this._biNegativeColumn = base.GetTemplateChild("BiNegativeColumn") as ColumnDefinition;
			this._biPositiveColumn = base.GetTemplateChild("BiPositiveColumn") as ColumnDefinition;
			this._biNegativeColumnSpacer = base.GetTemplateChild("BiNegativeColumnSpacer") as ColumnDefinition;
			this._biPositiveColumnSpacer = base.GetTemplateChild("BiPositiveColumnSpacer") as ColumnDefinition;
			base.OnApplyTemplate();

			SetState_DataBarDirection();
			SetupDataBars();
		}

		#endregion // Overrides

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Fired when a property changes on the <see cref="DataBarControl"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invoked when a property changes on the <see cref="DataBarControl"/> object.
		/// </summary>
		/// <param name="name">The name of the property that has changed.</param>
		protected virtual void OnPropertyChanged(String name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion

		#region Methods

		#region Private

		#region SetState_DataBarDirection

		private void SetState_DataBarDirection()
		{

			if (this.DataBarDirection == null)
			{
				VisualStateManager.GoToState(this, "None", false);
				return;
			}

			DataBarDirection dbd = (DataBarDirection)this.DataBarDirection;

			SetupDataBars();

			switch (dbd)
			{
				case (Infragistics.Controls.Grids.DataBarDirection.UnidirectionalLeftToRight):
					{
						VisualStateManager.GoToState(this, "UnidirectionalBarLeftToRight", false);
						break;
					}
				case (Infragistics.Controls.Grids.DataBarDirection.UnidirectionalRightToLeft):
					{
						VisualStateManager.GoToState(this, "UnidirectionalBarRightToLeft", false);
						break;
					}
				case (Infragistics.Controls.Grids.DataBarDirection.Bidirectional):
					{
						VisualStateManager.GoToState(this, "BidirectionalBar", false);
						break;
					}
			}
		}

		#endregion // SetState_DataBarDirection

		#region SetupDataBars

		private void SetupDataBars()
		{
			if (this.DataBarDirection == null|| this.BarPercentage==null)
				return;

			double setBarPercentage = this.NormalizeBarPercentage();

			if (this.DataBarDirection == Infragistics.Controls.Grids.DataBarDirection.Bidirectional)
			{
				if (this.DataBarPositiveNegative == DataBarPositiveNegative.Positive)
				{
					if (this._biPositiveColumn != null)
						this._biPositiveColumn.Width = new GridLength(setBarPercentage, GridUnitType.Star);
					if (this._biPositiveColumnSpacer != null)
						this._biPositiveColumnSpacer.Width = new GridLength(1 - setBarPercentage, GridUnitType.Star);

					if (this._biNegativeColumn != null)
						this._biNegativeColumn.Width = new GridLength(0, GridUnitType.Star);
					if (this._biNegativeColumnSpacer != null)
						this._biNegativeColumnSpacer.Width = new GridLength(1, GridUnitType.Star);
				}
				else
				{
					if (this._biNegativeColumn != null)
						this._biNegativeColumn.Width = new GridLength(setBarPercentage, GridUnitType.Star);
					if (this._biNegativeColumnSpacer != null)
						this._biNegativeColumnSpacer.Width = new GridLength(1 - setBarPercentage, GridUnitType.Star);

					if (this._biNegativeColumn != null)
						this._biPositiveColumn.Width = new GridLength(0, GridUnitType.Star);
					if (this._biNegativeColumnSpacer != null)
						this._biPositiveColumnSpacer.Width = new GridLength(1, GridUnitType.Star);
				}
			}
			else if (this.DataBarDirection == Infragistics.Controls.Grids.DataBarDirection.UnidirectionalLeftToRight)
			{
				if (this._uniDataBar != null)
				{
					this._uniDataBar.Width = new GridLength(setBarPercentage, GridUnitType.Star);
					this._uniDataBarSpacer.Width = new GridLength(1 - setBarPercentage, GridUnitType.Star);
				}
			}
			else if (this.DataBarDirection == Infragistics.Controls.Grids.DataBarDirection.UnidirectionalRightToLeft)
			{
				if (this._uniDataBar != null)
				{
					this._uniDataBarSpacer.Width = new GridLength(setBarPercentage, GridUnitType.Star);
					this._uniDataBar.Width = new GridLength(1 - setBarPercentage, GridUnitType.Star);
				}
			}
		}

		#endregion // SetupDataBars

		#region NormalizeBarPercentage

		private double NormalizeBarPercentage()
		{
			double setBarPercentage = (double)this.BarPercentage;

			if (setBarPercentage < 0)
			{
				setBarPercentage = 0;
			}
			else if (setBarPercentage > 100)
			{
				setBarPercentage = 100;
			}
			return setBarPercentage / 100;
		}

		#endregion // NormalizeBarPercentage

		#region SetupUnidirectionalBrush

		private void SetupUnidirectionalBrush()
		{
			if (this.DataBarDirection == null || this.DataBarDirection == Infragistics.Controls.Grids.DataBarDirection.Bidirectional)
			{
				this.UnidirectionalDataBarBrush = this.PositiveValueBarBrush;
				return;
			}

			if (this.DataBarPositiveNegative == DataBarPositiveNegative.Negative)
			{
				this.UnidirectionalDataBarBrush = this.NegativeValueBarBrush;
			}
			else
			{
				this.UnidirectionalDataBarBrush = this.PositiveValueBarBrush;
			}
		}

		#endregion // SetupUnidirectionalBrush

		#endregion // Private

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