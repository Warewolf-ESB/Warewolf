using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Abstract base class for controls which will be used inside of the FilterRow
	/// </summary>
	public abstract class FilterControlBase : ContentControl, INotifyPropertyChanged
    {
        #region Properties
        #region Cell
        /// <summary>
		/// The <see cref="FilterRowCell"/> which is associated with the <see cref="FilterControlBase"/>.
		/// </summary>
		public abstract FilterRowCell Cell { get; set; }
        #endregion // Cell
        #endregion // Properties

        #region INotifyPropertyChanged Members

        /// <summary>
		/// Fired when a property changes on the <see cref="FilterControlBase"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invoked when a property changes on the <see cref="FilterControlBase"/> object.
		/// </summary>
		/// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
		protected internal void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>	
		protected internal virtual void EnsureContent()
		{

		}

		#endregion // EnsureContent
	}

	/// <summary>
	/// A control for the FilterRowCell which contains the column editor and a drop down for picking the filter mode.
	/// </summary>
	[TemplatePart(Type = typeof(ComboBox), Name = "FilterOperands")]
	public class FilterControl : FilterControlBase
	{
		#region Members
		ComboBox _comboBox;
		FilterRowCell _cellBase;
		#endregion // Members

		#region Constructor


        /// <summary>
        /// Static constructor for the <see cref="CustomFilterDialogContentControl"/> class.
        /// </summary>
        static FilterControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterControl), new FrameworkPropertyMetadata(typeof(FilterControl)));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="FilterControl"/> class.
		/// </summary>
		public FilterControl()
		{



		}

		#endregion // Constructor

		#region Properties

		#region Cell

		/// <summary>
		/// The <see cref="FilterRowCell"/> which is associated with the <see cref="FilterControlBase"/>.
		/// </summary>
		public override FilterRowCell Cell
		{
			get
			{
				return this._cellBase;
			}
			set
			{
				if (this._cellBase != value)
				{
					this._cellBase = value;
					if (value != null)
					{
						this.SetFilterIcon();
					}
				}
			}
		}
		#endregion // Cell

		#region SelectedItemIcon

		/// <summary>
		/// Identifies the <see cref="SelectedItemIcon"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SelectedItemIconProperty = DependencyProperty.Register("SelectedItemIcon", typeof(DataTemplate), typeof(FilterControl), new PropertyMetadata(new PropertyChangedCallback(SelectedItemIconChanged)));

		/// <summary>
		/// Gets / set the DataTemplate that will be use as the Icon for options in the filtering menu.
		/// </summary>
		public DataTemplate SelectedItemIcon
		{
			get
			{
				return this.GetValue(SelectedItemIconProperty) as DataTemplate;
			}
			set
			{
				this.SetValue(SelectedItemIconProperty, value);
			}
		}

		private static void SelectedItemIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FilterControl fc = (FilterControl)obj;
			fc.OnPropertyChanged("SelectedItemIcon");
		}

		#endregion // SelectedItemIcon

		#endregion // Properties

		#region PopulateMenuItems
		/// <summary>
		/// Populates the combo with the values of the available row filter operands.
		/// </summary>
		protected virtual void PopulateMenuItems()
		{
            if(this.Cell != null)
			    this._comboBox.ItemsSource = this.Cell.Column.FilterColumnSettings.RowFilterOperands;
		}
		#endregion // PopulateMenuItems

		#region Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Builds the visual tree for the <see cref="FilterControl"/>
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_comboBox = base.GetTemplateChild("FilterOperands") as ComboBox;

			if (this._comboBox != null)
			{
				this._comboBox.DropDownOpened += this.ComboBox_DropDownOpened;				
			}

			this.SetFilterIcon();

		}
		#endregion // OnApplyTemplate

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>			
		protected internal override void EnsureContent()
		{
			this.SetFilterIcon();
			base.EnsureContent();
		}

		#endregion // EnsureContent

		#endregion // Overrides

		#region EventHandlers

		void ComboBox_DropDownOpened(object sender, EventArgs e)
		{
            FilterRowCell frc = this.Cell;
			if (frc != null && frc.Column != null)
			{
                Column column = frc.Column;

				column.ColumnLayout.Grid.ExitEditMode(false);

				column.OnShowAvailableColumnFilterOperands();				

				this._comboBox.SelectionChanged -= ComboBox_SelectionChanged;

				PopulateMenuItems();

				FilterOperand selectedItem = frc.FilteringOperandResolved;
				if (selectedItem != null)
				{
					foreach (FilterOperand fo in this._comboBox.Items)
					{
						if (fo.GetType() == selectedItem.GetType())
						{
							this._comboBox.SelectedItem = fo;
							break;
						}
					}
				}
				this._comboBox.SelectionChanged += ComboBox_SelectionChanged;
			}
		}

		void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			FilterRowCell frc = this.Cell;
			if (frc != null)
			{
				FilterOperand foc = this._comboBox.SelectedItem as FilterOperand;
				if (foc != null)
				{
					Column col = frc.Column;

					col.ColumnLayout.Grid.ExitEditMode(false);

					if (col.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
					{
						frc.FilteringOperand = foc;
					}
					else
					{
						col.FilterColumnSettings.FilteringOperand = foc;
					}

					this.SelectedItemIcon = foc.IconResolved;
				}
			}
		}

		#endregion // EventHandlers

		#region Methods

		#region SetFilterIcon

		private void SetFilterIcon()
		{
			if (this.Cell != null && this.Cell.Row != null && this.Cell.Row.ColumnLayout != null && this.Cell.FilteringOperandResolved != null)
			{
				FilterOperand fo = this.Cell.FilteringOperandResolved;
				if (fo != null)
				{
					fo.XamWebGrid = this.Cell.Row.ColumnLayout.Grid;
					this.SelectedItemIcon = fo.IconResolved;
				}
			}
		}

		#endregion // SetFilterIcon

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