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
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using FixedColumns on a particular <see cref="ColumnLayout"/>
	/// </summary>
	public class FixedColumnSettingsOverride : SettingsOverrideBase, IDisposable
	{
		#region Members

		FixedColumnsCollection _fixedColumnsCollectionLeft, _fixedColumnsCollectionRight;

		#endregion // Members

		#region Properties

		#region AllowFixedColumns

		/// <summary>
		/// Identifies the <see cref="AllowFixedColumns"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowFixedColumnsProperty = DependencyProperty.Register("AllowFixedColumns", typeof(FixedColumnType?), typeof(FixedColumnSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowFixedColumnsChanged)));

		/// <summary>
		/// Gets/Sets whether the an Indicator for Fixed Columns will be displayed on all Columns in a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FixedColumnType>))]
		public FixedColumnType? AllowFixedColumns
		{
			get { return (FixedColumnType?)this.GetValue(AllowFixedColumnsProperty); }
			set { this.SetValue(AllowFixedColumnsProperty, value); }
		}

		private static void AllowFixedColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("AllowFixedColumns");
		}

		#endregion // AllowFixedColumns

		#region AllowFixedColumnsResolved

		/// <summary>
		/// Resolves the <see cref="FixedColumnSettingsOverride.AllowFixedColumns"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FixedColumnType AllowFixedColumnsResolved
		{
			get
			{
                if (this.AllowFixedColumns == null)
                {
                    if (this.SettingsObject != null)
                        return ((FixedColumnSettings)this.SettingsObject).AllowFixedColumns;
                }
                else
                    return (FixedColumnType)this.AllowFixedColumns;

                return (FixedColumnType)FixedColumnSettings.AllowFixedColumnsProperty.GetMetadata(typeof(FixedColumnSettings)).DefaultValue;
			}
		}

		#endregion //AllowFixedColumnsResolved

		#region FixedIndicatorDirection

		/// <summary>
		/// Identifies the <see cref="FixedIndicatorDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedIndicatorDirectionProperty = DependencyProperty.Register("FixedIndicatorDirection", typeof(FixedIndicatorDirection?), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedIndicatorDirectionChanged)));

		/// <summary>
		/// Gets/Sets what side of the <see cref="XamGrid"/> a column should be locked to if the <see cref="Column.IsFixed"/> property is set to true on a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FixedIndicatorDirection>))]
		public FixedIndicatorDirection? FixedIndicatorDirection
		{
			get { return (FixedIndicatorDirection?)this.GetValue(FixedIndicatorDirectionProperty); }
			set { this.SetValue(FixedIndicatorDirectionProperty, value); }
		}

		private static void FixedIndicatorDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedIndicatorDirection");
		}

		#endregion // FixedIndicatorDirection

		#region FixedIndicatorDirectionResolved

		/// <summary>
		/// Resolves the <see cref="FixedColumnSettingsOverride.FixedIndicatorDirection"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FixedIndicatorDirection FixedIndicatorDirectionResolved
		{
			get
			{
                if (this.FixedIndicatorDirection == null)
                {
                    if (this.SettingsObject != null)
                        return ((FixedColumnSettings)this.SettingsObject).FixedIndicatorDirection;
                }
                else
                    return (FixedIndicatorDirection)this.FixedIndicatorDirection;

                return (FixedIndicatorDirection)FixedColumnSettings.FixedIndicatorDirectionProperty.GetMetadata(typeof(FixedColumnSettings)).DefaultValue;
			}
		}

		#endregion //FixedIndicatorDirectionResolved

		#region FixedDropAreaLocation

		/// <summary>
		/// Identifies the <see cref="FixedDropAreaLocation"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedDropAreaLocationProperty = DependencyProperty.Register("FixedDropAreaLocation", typeof(FixedDropAreaLocation?), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedDropAreaLocationChanged)));

		/// <summary>
		/// Gets/Sets where a drop area will be displayed on the <see cref="XamGrid"/> when a <see cref="Column"/> is dragged,
		/// so that user can drop a column in order to lock it in place so that it can'type be scrolled.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<FixedDropAreaLocation>))]
		public FixedDropAreaLocation? FixedDropAreaLocation
		{
			get { return (FixedDropAreaLocation?)this.GetValue(FixedDropAreaLocationProperty); }
			set { this.SetValue(FixedDropAreaLocationProperty, value); }
		}

		private static void FixedDropAreaLocationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedDropAreaLocation");
		}

		#endregion // FixedDropAreaLocation

		#region FixedDropAreaLocationResolved

		/// <summary>
		/// Resolves the <see cref="FixedColumnSettingsOverride.FixedDropAreaLocation"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public FixedDropAreaLocation FixedDropAreaLocationResolved
		{
			get
			{
                if (this.FixedDropAreaLocation == null)
                {
                    if (this.SettingsObject != null)
                        return ((FixedColumnSettings)this.SettingsObject).FixedDropAreaLocation;
                }
                else
                    return (FixedDropAreaLocation)this.FixedDropAreaLocation;

                return (FixedDropAreaLocation)FixedColumnSettings.FixedDropAreaLocationProperty.GetMetadata(typeof(FixedColumnSettings)).DefaultValue;
			}
		}

		#endregion //FixedDropAreaLocationResolved

		#region FixedBorderStyle

		/// <summary>
		/// Identifies the <see cref="FixedBorderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedBorderStyleProperty = DependencyProperty.Register("FixedBorderStyle", typeof(Style), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedBorderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style FixedBorderStyle
		{
			get { return (Style)this.GetValue(FixedBorderStyleProperty); }
			set { this.SetValue(FixedBorderStyleProperty, value); }
		}

		private static void FixedBorderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedBorderStyle");
		}

		#endregion // FixedBorderStyle

		#region FixedBorderStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style FixedBorderStyleResolved
		{
			get
			{
				if (this.FixedBorderStyle == null && this.SettingsObject != null)
					return ((FixedColumnSettings)this.SettingsObject).FixedBorderStyle;
				else
					return this.FixedBorderStyle;
			}
		}

		#endregion // FixedBorderStyleResolved

		#region FixedBorderHeaderStyle

		/// <summary>
		/// Identifies the <see cref="FixedBorderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedBorderHeaderStyleProperty = DependencyProperty.Register("FixedBorderHeaderStyle", typeof(Style), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedBorderHeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style FixedBorderHeaderStyle
		{
			get { return (Style)this.GetValue(FixedBorderHeaderStyleProperty); }
			set { this.SetValue(FixedBorderHeaderStyleProperty, value); }
		}

		private static void FixedBorderHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedBorderHeaderStyle");
		}

		#endregion // FixedBorderHeaderStyle

		#region FixedBorderHeaderStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style FixedBorderHeaderStyleResolved
		{
			get
			{
				if (this.FixedBorderHeaderStyle == null && this.SettingsObject != null)
					return ((FixedColumnSettings)this.SettingsObject).FixedBorderHeaderStyle;
				else
					return this.FixedBorderHeaderStyle;
			}
		}

		#endregion // FixedBorderHeaderStyleResolved

		#region FixedBorderFooterStyle

		/// <summary>
		/// Identifies the <see cref="FixedBorderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedBorderFooterStyleProperty = DependencyProperty.Register("FixedBorderFooterStyle", typeof(Style), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedBorderFooterStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style FixedBorderFooterStyle
		{
			get { return (Style)this.GetValue(FixedBorderFooterStyleProperty); }
			set { this.SetValue(FixedBorderFooterStyleProperty, value); }
		}

		private static void FixedBorderFooterStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedBorderFooterStyle");
		}

		#endregion // FixedBorderFooterStyle

		#region FixedBorderFooterStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style FixedBorderFooterStyleResolved
		{
			get
			{
				if (this.FixedBorderFooterStyle == null && this.SettingsObject != null)
					return ((FixedColumnSettings)this.SettingsObject).FixedBorderFooterStyle;
				else
					return this.FixedBorderFooterStyle;
			}
		}

		#endregion // FixedBorderFooterStyleResolved

		#region FixedDropAreaLeftStyle

		/// <summary>
		/// Identifies the <see cref="FixedDropAreaLeftStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedDropAreaLeftStyleProperty = DependencyProperty.Register("FixedDropAreaLeftStyle", typeof(Style), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedDropAreaLeftStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style FixedDropAreaLeftStyle
		{
			get { return (Style)this.GetValue(FixedDropAreaLeftStyleProperty); }
			set { this.SetValue(FixedDropAreaLeftStyleProperty, value); }
		}

		private static void FixedDropAreaLeftStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedDropAreaLeftStyle");
		}

		#endregion // FixedDropAreaLeftStyle

		#region FixedDropAreaLeftStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style FixedDropAreaLeftStyleResolved
		{
			get
			{
				if (this.FixedDropAreaLeftStyle == null && this.SettingsObject != null)
					return ((FixedColumnSettings)this.SettingsObject).FixedDropAreaLeftStyle;
				else
					return this.FixedDropAreaLeftStyle;
			}
		}

		#endregion // FixedDropAreaLeftStyleResolved

		#region FixedDropAreaRightStyle

		/// <summary>
		/// Identifies the <see cref="FixedDropAreaRightStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedDropAreaRightStyleProperty = DependencyProperty.Register("FixedDropAreaRightStyle", typeof(Style), typeof(FixedColumnSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(FixedDropAreaRightStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style FixedDropAreaRightStyle
		{
			get { return (Style)this.GetValue(FixedDropAreaRightStyleProperty); }
			set { this.SetValue(FixedDropAreaRightStyleProperty, value); }
		}

		private static void FixedDropAreaRightStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettingsOverride settings = (FixedColumnSettingsOverride)obj;
			settings.OnPropertyChanged("FixedDropAreaRightStyle");
		}

		#endregion // FixedDropAreaRightStyle

		#region FixedDropAreaRightStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style FixedDropAreaRightStyleResolved
		{
			get
			{
				if (this.FixedDropAreaRightStyle == null && this.SettingsObject != null)
					return ((FixedColumnSettings)this.SettingsObject).FixedDropAreaRightStyle;
				else
					return this.FixedDropAreaRightStyle;
			}
		}

		#endregion // FixedDropAreaRightStyleResolved

		#region FixedColumnsLeft

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are pinned to the left side of a particular <see cref="ColumnLayout"/>
		/// </summary>
        [Browsable(false)]
		public FixedColumnsCollection FixedColumnsLeft
		{
			get
			{
				if (this._fixedColumnsCollectionLeft == null)
					this._fixedColumnsCollectionLeft = new FixedColumnsCollection(FixedState.Left);
				return this._fixedColumnsCollectionLeft;
			}
		}

		#endregion // FixedColumnsLeft

		#region FixedColumnsRight

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are pinned to the right side of a particular <see cref="ColumnLayout"/>
		/// </summary>
        [Browsable(false)]
		public FixedColumnsCollection FixedColumnsRight
		{
			get
			{
				if (this._fixedColumnsCollectionRight == null)
					this._fixedColumnsCollectionRight = new FixedColumnsCollection(FixedState.Right);
				return this._fixedColumnsCollectionRight;
			}
		}

		#endregion // FixedColumnsRight

		#endregion // Properties

		#region Overrides

		#region SettingsObject

		/// <summary>
		/// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsOverrideBase"/>
		/// </summary>
		protected override SettingsBase SettingsObject
		{
			get
			{
				SettingsBase settings = null;
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.FixedColumnSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="FixedColumnSettingsOverride"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._fixedColumnsCollectionLeft != null)
				this._fixedColumnsCollectionLeft.Dispose();

			if (this._fixedColumnsCollectionRight != null)
				this._fixedColumnsCollectionRight.Dispose();
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="FixedColumnSettingsOverride"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
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