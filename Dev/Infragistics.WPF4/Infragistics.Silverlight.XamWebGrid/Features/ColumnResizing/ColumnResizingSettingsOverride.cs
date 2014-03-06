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
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using ColumnResizingSettings on a particular <see cref="ColumnLayout"/>
	/// </summary>
	public class ColumnResizingSettingsOverride : SettingsOverrideBase
	{
		#region Members

		ResizingIndicator _indicator;
		Popup _indicatorContainer;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnResizingSettingsOverride"/> class.
		/// </summary>		
		public ColumnResizingSettingsOverride()
		{
			this._indicatorContainer = new Popup();
			this._indicator = new ResizingIndicator();
			this._indicatorContainer.Child = this._indicator;


            this._indicatorContainer.Placement = PlacementMode.Relative;
            this._indicatorContainer.AllowsTransparency = true;

		}

		#endregion // Constructor

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
					settings = this.ColumnLayout.Grid.ColumnResizingSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region Properties

		#region Internal

		#region Indicator
		/// <summary>
		/// A <see cref="ResizingIndicator"/> which is displayed when the <see cref="AllowColumnResizing"/> resolves to Indicator
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ResizingIndicator Indicator
		{
			get
			{
				Style resolved = this.IndicatorStyleResolved;
				if (this._indicator.Style != resolved)
					this._indicator.Style = resolved;

				return this._indicator;
			}
		}

		/// <summary>
		/// The Popup control used to display the <see cref="Indicator"/>
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public  Popup IndicatorContainer
		{
			get { return this._indicatorContainer; }
		}

		#endregion // Indicator

		#endregion // Internal

		#region Public
		#region IndicatorStyle

		/// <summary>
		/// Identifies the <see cref="IndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IndicatorStyleProperty = DependencyProperty.Register("IndicatorStyle", typeof(Style), typeof(ColumnResizingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(IndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style IndicatorStyle
		{
			get { return (Style)this.GetValue(IndicatorStyleProperty); }
			set { this.SetValue(IndicatorStyleProperty, value); }
		}

		private static void IndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettingsOverride settings = (ColumnResizingSettingsOverride)obj;
			settings.OnPropertyChanged("IndicatorStyle");
		}

		#endregion // IndicatorStyle

		#region IndicatorStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style IndicatorStyleResolved
		{
			get
			{
				if (this.IndicatorStyle == null && this.SettingsObject != null)
					return ((ColumnResizingSettings)this.SettingsObject).IndicatorStyle;
				else
					return this.IndicatorStyle;
			}
		}

		#endregion // IndicatorStyleResolved

		#region AllowColumnResizing

		/// <summary>
		/// Identifies the <see cref="AllowColumnResizing"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowColumnResizingTypeProperty = DependencyProperty.Register("AllowColumnResizing", typeof(ColumnResizingType?), typeof(ColumnResizingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowColumnResizingChanged)));

		/// <summary>
		/// Gets/sets how column resizing will work for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<ColumnResizingType>))]
		public ColumnResizingType? AllowColumnResizing
		{
			get { return (ColumnResizingType?)this.GetValue(AllowColumnResizingTypeProperty); }
			set { this.SetValue(AllowColumnResizingTypeProperty, value); }
		}

		private static void AllowColumnResizingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettingsOverride settings = (ColumnResizingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowColumnResizing");
		}

		#endregion // AllowColumnMoving

		#region AllowColumnResizingResolved
		/// <summary>
		/// Resolves the type of Column Resizing supported for this <see cref="ColumnLayout"/>
		/// </summary>
		public ColumnResizingType AllowColumnResizingResolved
		{
			get
			{
                if (this.AllowColumnResizing == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnResizingSettings)this.SettingsObject).AllowColumnResizing;
                }
                else
                    return (ColumnResizingType)this.AllowColumnResizing;

                return (ColumnResizingType)ColumnResizingSettings.AllowColumnResizingTypeProperty.GetMetadata(typeof(ColumnResizingSettings)).DefaultValue;
			}
		}
		#endregion // AllowColumnResizingResolved

		#region AllowDoubleClickToSize

		/// <summary>
		/// Identifies the <see cref="AllowDoubleClickToSize"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowDoubleClickToSizeProperty = DependencyProperty.Register("AllowDoubleClickToSize", typeof(bool?), typeof(ColumnResizingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowDoubleClickToSizeChanged)));

		/// <summary>
		/// Gets/sets if double clicking on the column edge will resize for this <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? AllowDoubleClickToSize
		{
			get { return (bool?)this.GetValue(AllowDoubleClickToSizeProperty); }
			set { this.SetValue(AllowDoubleClickToSizeProperty, value); }
		}

		private static void AllowDoubleClickToSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettingsOverride settings = (ColumnResizingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowDoubleClickToSize");
		}

		#endregion // AllowDoubleClickToSize

		#region AllowDoubleClickToSizeResolved

		/// <summary>
		/// Resolves if double clicking the edge will resize for this <see cref="ColumnLayout"/>
		/// </summary>
		public bool AllowDoubleClickToSizeResolved
		{
            get
            {
                if (this.AllowDoubleClickToSize == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnResizingSettings)this.SettingsObject).AllowDoubleClickToSize;
                }
                else
                    return (bool)this.AllowDoubleClickToSize;

                return (bool)ColumnResizingSettings.AllowDoubleClickToSizeProperty.GetMetadata(typeof(ColumnResizingSettings)).DefaultValue;
            }
		}

		#endregion // AllowDoubleClickToSizeResolved


		#region AllowMultipleColumnResize

		/// <summary>
		/// Identifies the <see cref="AllowMultipleColumnResize"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowMultipleColumnResizeProperty = DependencyProperty.Register("AllowMultipleColumnResize", typeof(bool?), typeof(ColumnResizingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowMultipleColumnResizeChanged)));

		/// <summary>
		/// Gets/sets if multiple column resizing will work for this <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? AllowMultipleColumnResize
		{
			get { return (bool?)this.GetValue(AllowMultipleColumnResizeProperty); }
			set { this.SetValue(AllowMultipleColumnResizeProperty, value); }
		}

		private static void AllowMultipleColumnResizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettingsOverride settings = (ColumnResizingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowMultipleColumnResize");
		}

		#endregion // AllowMultipleColumnResize

		#region AllowMultipleColumnResizeResolved

		/// <summary>
		/// Resolves if multiple column resizing for this <see cref="ColumnLayout"/>.
		/// </summary>
		public bool AllowMultipleColumnResizeResolved
		{
            get
            {
                if (this.AllowMultipleColumnResize == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnResizingSettings)this.SettingsObject).AllowMultipleColumnResize;
                }
                else
                    return (bool)this.AllowMultipleColumnResize;

                return (bool)ColumnResizingSettings.AllowMultipleColumnResizeProperty.GetMetadata(typeof(ColumnResizingSettings)).DefaultValue;
            }
		}

		#endregion

		#region AllowAllowCellAreaResizing

		/// <summary>
		/// Identifies the <see cref="AllowCellAreaResizing"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowCellAreaResizingProperty = DependencyProperty.Register("AllowCellAreaResizing", typeof(bool?), typeof(ColumnResizingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowCellAreaResizingChanged)));

		/// <summary>
		/// Gets/sets if cell area resizing will work for this <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? AllowCellAreaResizing
		{
			get { return (bool?)this.GetValue(AllowCellAreaResizingProperty); }
			set { this.SetValue(AllowCellAreaResizingProperty, value); }
		}

		private static void AllowCellAreaResizingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettingsOverride settings = (ColumnResizingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowCellAreaResizing");
		}

		#endregion // AllowAllowCellAreaResizing

		#region AllowCellAreaResizingResolved

		/// <summary>
		/// Resolves if cell area resizing for this <see cref="ColumnLayout"/>.
		/// </summary>
        public bool AllowCellAreaResizingResolved
        {
            get
            {
                if (this.AllowCellAreaResizing == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnResizingSettings)this.SettingsObject).AllowCellAreaResizing;
                }
                else
                    return (bool)this.AllowCellAreaResizing;

                return (bool)ColumnResizingSettings.AllowCellAreaResizingProperty.GetMetadata(typeof(ColumnResizingSettings)).DefaultValue;
            }
        }

		#endregion
		#endregion // Public

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