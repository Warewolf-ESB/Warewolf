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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A class that controls the add new row settings for an object on the <see cref="ColumnLayout"/>.
	/// </summary>
	public class AddNewRowSettingsOverride : EditingSettingsBaseOverride
	{
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
					settings = this.ColumnLayout.Grid.AddNewRowSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#region Properties

		#region Public

		#region AllowAddNewRow

		/// <summary>
		/// Identifies the <see cref="AllowAddNewRow"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowAddNewRowProperty = DependencyProperty.Register("AllowAddNewRow", typeof(AddNewRowLocation?), typeof(AddNewRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowAddNewRowChanged)));

		/// <summary>
		/// Gets / sets where the <see cref="AddNewRow"/> should be visible on this <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<AddNewRowLocation>))]
		public AddNewRowLocation? AllowAddNewRow
		{
			get { return (AddNewRowLocation?)this.GetValue(AllowAddNewRowProperty); }
			set { this.SetValue(AllowAddNewRowProperty, value); }
		}

		private static void AllowAddNewRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			AddNewRowSettingsOverride settings = (AddNewRowSettingsOverride)obj;
			settings.OnPropertyChanged("AllowAddNewRow");
		}

		#endregion // AllowAddNewRow

		#region AllowAddNewRowResolved
		/// <summary>
		/// Gets the <see cref="AddNewRowLocation"/> which will be applied the current <see cref="ColumnLayout"/>.
		/// </summary>
		public AddNewRowLocation AllowAddNewRowResolved
		{
            get
            {
                if (this.AllowAddNewRow == null)
                {
                    if (this.SettingsObject != null)
                    {
                        return ((AddNewRowSettings)this.SettingsObject).AllowAddNewRow;
                    }
                }
                else
                    return (AddNewRowLocation)this.AllowAddNewRow;

                return (AddNewRowLocation)AddNewRowSettings.AllowAddNewRowProperty.GetMetadata(typeof(AddNewRowSettings)).DefaultValue;
            }
		}
		#endregion // AllowAddNewRowResolved

		#region ExpansionIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ExpansionIndicatorStyleProperty = DependencyProperty.Register("ExpansionIndicatorStyle", typeof(Style), typeof(AddNewRowSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(ExpansionIndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style ExpansionIndicatorStyle
		{
			get { return (Style)this.GetValue(ExpansionIndicatorStyleProperty); }
			set { this.SetValue(ExpansionIndicatorStyleProperty, value); }
		}

		private static void ExpansionIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			AddNewRowSettingsOverride settings = (AddNewRowSettingsOverride)obj;
			if (settings.ColumnLayout != null && settings.ColumnLayout.Grid != null)
				settings.ColumnLayout.Grid.ResetPanelRows();
			settings.OnPropertyChanged("ExpansionIndicatorStyle");
		}

		#endregion // ExpansionIndicatorStyle

		#region ExpansionIndicatorStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style ExpansionIndicatorStyleResolved
		{
			get
			{
				if (this.ExpansionIndicatorStyle == null && this.SettingsObject != null)
					return ((AddNewRowSettings)this.SettingsObject).ExpansionIndicatorStyle;
				else
					return this.ExpansionIndicatorStyle;
			}
		}

		#endregion // ExpansionIndicatorStyleResolved

		#region Style

		/// <summary>
		/// Identifies the <see cref="Style"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty StyleProperty = DependencyProperty.Register("Style", typeof(Style), typeof(AddNewRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(StyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to every <see cref="Cell"/> in the <see cref="AddNewRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style Style
		{
			get { return (Style)this.GetValue(StyleProperty); }
			set { this.SetValue(StyleProperty, value); }
		}

		private static void StyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			AddNewRowSettingsOverride settings = (AddNewRowSettingsOverride)obj;
			settings.OnPropertyChanged("Style");
		}

		#endregion // Style

		#region StyleResolved

		/// <summary>
		/// Resolves the <see cref="AddNewRowSettingsOverride.Style"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style StyleResolved
		{
			get
			{
				if (this.Style == null && this.SettingsObject != null)
					return ((AddNewRowSettings)this.SettingsObject).Style;
				else
					return (Style)this.Style;
			}
		}

		#endregion //StyleResolved

		#region RowSelectorStyle

		/// <summary>
		/// Identifies the <see cref="RowSelectorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowSelectorStyleProperty = DependencyProperty.Register("RowSelectorStyle", typeof(Style), typeof(AddNewRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(RowSelectorStyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to the RowSelector of an <see cref="AddNewRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style RowSelectorStyle
		{
			get { return (Style)this.GetValue(RowSelectorStyleProperty); }
			set { this.SetValue(RowSelectorStyleProperty, value); }
		}

		private static void RowSelectorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			AddNewRowSettingsOverride settings = (AddNewRowSettingsOverride)obj;
			settings.OnPropertyChanged("RowSelectorStyle");
		}

		#endregion // RowSelectorStyle

		#region RowSelectorStyleResolved

		/// <summary>
		/// Resolves the <see cref="AddNewRowSettingsOverride.RowSelectorStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style RowSelectorStyleResolved
		{
			get
			{
				if (this.RowSelectorStyle == null && this.SettingsObject != null)
					return ((AddNewRowSettings)this.SettingsObject).RowSelectorStyle;

				return (Style)this.RowSelectorStyle;
			}
		}

		#endregion //RowSelectorStyleResolved

		#region AddNewRowHeight

		/// <summary>
		/// Identifies the <see cref="AddNewRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AddNewRowHeightProperty = DependencyProperty.Register("AddNewRowHeight", typeof(RowHeight?), typeof(AddNewRowSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AddNewRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="AddNewRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? AddNewRowHeight
		{
			get { return (RowHeight?)this.GetValue(AddNewRowHeightProperty); }
			set { this.SetValue(AddNewRowHeightProperty, value); }
		}

		private static void AddNewRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			AddNewRowSettingsOverride settings = (AddNewRowSettingsOverride)obj;
			settings.OnPropertyChanged("AddNewRowHeight");
		}

		#endregion // AddNewRowHeight

		#region AddNewRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="AddNewRowSettingsOverride.AddNewRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight AddNewRowHeightResolved
		{
            get
            {
                if (this.AddNewRowHeight == null)
                {
                    if (this.SettingsObject != null)
                        return ((AddNewRowSettings)this.SettingsObject).AddNewRowHeight;
                }
                else
                    return (RowHeight)this.AddNewRowHeight;

                return (RowHeight)AddNewRowSettings.AddNewRowHeightProperty.GetMetadata(typeof(AddNewRowSettings)).DefaultValue;
            }
		}

		#endregion //AddNewRowHeightResolved

		#endregion // Public

		#region Protected

		#region ResolveEditingType
		/// <summary>
		/// Determines what <see cref="EditingType"/> will be supported by the <see cref="EditingSettingsBaseOverride"/> object.
		/// </summary>
		/// <returns></returns>
		protected internal override EditingType ResolveEditingType()
		{
			return EditingType.Row;
		}
		#endregion // ResolveEditingType

		#endregion // Protected

		#endregion // Properties
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