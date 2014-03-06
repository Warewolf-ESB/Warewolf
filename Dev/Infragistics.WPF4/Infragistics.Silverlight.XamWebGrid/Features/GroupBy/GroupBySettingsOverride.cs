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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using GropuBy on a particular <see cref="ColumnLayout"/>
	/// </summary>
	public class GroupBySettingsOverride : SettingsOverrideBase
	{
		#region Members

		GroupByMovingIndicator _indicator;
		Popup _indicatorContainer;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupBySettingsOverride"/> class.
		/// </summary>		
		public GroupBySettingsOverride()
		{
			this._indicator = new GroupByMovingIndicator();
			this._indicatorContainer = new Popup();
			this._indicatorContainer.Child = this._indicator;

            this._indicatorContainer.Placement = PlacementMode.Relative;
            this._indicatorContainer.AllowsTransparency = true;

		}

		#endregion // Constructor

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
					settings = this.ColumnLayout.Grid.GroupBySettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#region Properties

		#region Public

		#region IsGroupable

		/// <summary>
		/// Identifies the <see cref="IsGroupable"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsGroupableProperty = DependencyProperty.Register("IsGroupable", typeof(bool?), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsGroupableChanged)));

		/// <summary>
		/// Gets/Sets whether <see cref="Column"/> objects can be grouped via the UI for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? IsGroupable
		{
			get { return (bool?)this.GetValue(IsGroupableProperty); }
			set { this.SetValue(IsGroupableProperty, value); }
		}

		private static void IsGroupableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("IsGroupable");
		}

		#endregion // IsGroupable

		#region IsGroupableResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.IsGroupable"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public bool IsGroupableResolved
		{
			get
			{
                if (this.IsGroupable == null)
                {
                    if (this.SettingsObject != null)
                        return ((GroupBySettings)this.SettingsObject).IsGroupable;
                }
                else
                    return (bool)this.IsGroupable;

                return (bool)GroupBySettings.IsGroupableProperty.GetMetadata(typeof(GroupBySettings)).DefaultValue;
			}
		}

		#endregion //IsGroupableResolved

		#region GroupByMovingIndicatorStyle

		/// <summary>
		/// Identifies the <see cref="GroupByMovingIndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByMovingIndicatorStyleProperty = DependencyProperty.Register("GroupByMovingIndicatorStyle", typeof(Style), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(GroupByMovingIndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> for the <see cref="GroupByMovingIndicator"/> when rearranging headers in the GroupByArea for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByMovingIndicatorStyle
		{
			get { return (Style)this.GetValue(GroupByMovingIndicatorStyleProperty); }
			set { this.SetValue(GroupByMovingIndicatorStyleProperty, value); }
		}

		private static void GroupByMovingIndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("GroupByMovingIndicatorStyle");
		}

		#endregion // GroupByMovingIndicatorStyle

		#region GroupByMovingIndicatorStyleResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.GroupByMovingIndicatorStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByMovingIndicatorStyleResolved
		{
			get
			{
				if (this.GroupByMovingIndicatorStyle == null && this.SettingsObject != null)
					return ((GroupBySettings)this.SettingsObject).GroupByMovingIndicatorStyle;
				else
					return (Style)this.GroupByMovingIndicatorStyle;
			}
		}

		#endregion //GroupByMovingIndicatorStyleResolved

		#region GroupByHeaderStyle

		/// <summary>
		/// Identifies the <see cref="GroupByHeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByHeaderStyleProperty = DependencyProperty.Register("GroupByHeaderStyle", typeof(Style), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(GroupByHeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to the draggable <see cref="GroupByHeaderCellControl"/> objects in the GroupByArea for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByHeaderStyle
		{
			get { return (Style)this.GetValue(GroupByHeaderStyleProperty); }
			set { this.SetValue(GroupByHeaderStyleProperty, value); }
		}

		private static void GroupByHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("GroupByHeaderStyle");
		}

		#endregion // GroupByHeaderStyle

		#region GroupByHeaderStyleResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.GroupByHeaderStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByHeaderStyleResolved
		{
			get
			{
				if (this.GroupByHeaderStyle == null && this.SettingsObject != null)
					return ((GroupBySettings)this.SettingsObject).GroupByHeaderStyle;
				else
					return (Style)this.GroupByHeaderStyle;
			}
		}

		#endregion //GroupByHeaderStyleResolved

		#region GroupByColumnLayoutHeaderStyle

		/// <summary>
		/// Identifies the <see cref="GroupByColumnLayoutHeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByColumnLayoutHeaderStyleProperty = DependencyProperty.Register("GroupByColumnLayoutHeaderStyle", typeof(Style), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(GroupByColumnLayoutHeaderStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to the <see cref="GroupByColumnLayoutHeaderCellControl"/> which represents the <see cref="ColumnLayout"/> of a <see cref="Column"/> that is grouped for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByColumnLayoutHeaderStyle
		{
			get { return (Style)this.GetValue(GroupByColumnLayoutHeaderStyleProperty); }
			set { this.SetValue(GroupByColumnLayoutHeaderStyleProperty, value); }
		}

		private static void GroupByColumnLayoutHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("GroupByColumnLayoutHeaderStyle");
		}

		#endregion // GroupByColumnLayoutHeaderStyle

		#region GroupByColumnLayoutHeaderStyleResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.GroupByColumnLayoutHeaderStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByColumnLayoutHeaderStyleResolved
		{
			get
			{
				if (this.GroupByColumnLayoutHeaderStyle == null && this.SettingsObject != null)
					return ((GroupBySettings)this.SettingsObject).GroupByColumnLayoutHeaderStyle;
				else
					return (Style)this.GroupByColumnLayoutHeaderStyle;
			}
		}

		#endregion //GroupByColumnLayoutHeaderStyleResolved

		#region GroupByRowStyle

		/// <summary>
		/// Identifies the <see cref="GroupByRowStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByRowStyleProperty = DependencyProperty.Register("GroupByRowStyle", typeof(Style), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(GroupByRowStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that will be applied to every GroupBy row for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByRowStyle
		{
			get { return (Style)this.GetValue(GroupByRowStyleProperty); }
			set { this.SetValue(GroupByRowStyleProperty, value); }
		}

		private static void GroupByRowStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("GroupByRowStyle");
		}

		#endregion // GroupByRowStyle

		#region GroupByRowStyleResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.GroupByRowStyle"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public Style GroupByRowStyleResolved
		{
			get
			{
				if (this.GroupByRowStyle == null && this.SettingsObject != null)
					return ((GroupBySettings)this.SettingsObject).GroupByRowStyle;
				else
					return (Style)this.GroupByRowStyle;
			}
		}

		#endregion //GroupByRowStyleResolved

		#region GroupByRowHeight

		/// <summary>
		/// Identifies the <see cref="GroupByRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByRowHeightProperty = DependencyProperty.Register("GroupByRowHeight", typeof(RowHeight?), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(GroupByRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="GroupByRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? GroupByRowHeight
		{
			get { return (RowHeight?)this.GetValue(GroupByRowHeightProperty); }
			set { this.SetValue(GroupByRowHeightProperty, value); }
		}

		private static void GroupByRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("GroupByRowHeight");
		}

		#endregion // GroupByRowHeight

		#region GroupByRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.GroupByRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight GroupByRowHeightResolved
		{
			get
			{
                if (this.GroupByRowHeight == null)
                {
                    if (this.SettingsObject != null)
                        return ((GroupBySettings)this.SettingsObject).GroupByRowHeight;
                }
                else
                    return (RowHeight)this.GroupByRowHeight;

                return (RowHeight)GroupBySettings.GroupByRowHeightProperty.GetMetadata(typeof(GroupBySettings)).DefaultValue;
			}
		}

		#endregion //GroupByRowHeightResolved

		#region GroupByAreaRowHeight

		/// <summary>
		/// Identifies the <see cref="GroupByAreaRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty GroupByAreaRowHeightProperty = DependencyProperty.Register("GroupByAreaRowHeight", typeof(RowHeight?), typeof(GroupBySettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(GroupByAreaRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="GroupByAreaRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? GroupByAreaRowHeight
		{
			get { return (RowHeight?)this.GetValue(GroupByAreaRowHeightProperty); }
			set { this.SetValue(GroupByAreaRowHeightProperty, value); }
		}

		private static void GroupByAreaRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			GroupBySettingsOverride settings = (GroupBySettingsOverride)obj;
			settings.OnPropertyChanged("GroupByAreaRowHeight");
		}

		#endregion // GroupByAreaRowHeight

		#region GroupByAreaRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="GroupBySettingsOverride.GroupByAreaRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight GroupByAreaRowHeightResolved
		{
			get
			{
                if (this.GroupByAreaRowHeight == null)
                {
                    if (this.SettingsObject != null)
                        return ((GroupBySettings)this.SettingsObject).GroupByAreaRowHeight;
                }
                else
                    return (RowHeight)this.GroupByAreaRowHeight;

                return (RowHeight)GroupBySettings.GroupByAreaRowHeightProperty.GetMetadata(typeof(GroupBySettings)).DefaultValue;
			}
		}

		#endregion //GroupByAreaRowHeightResolved

		#endregion // Public

		#region Internal

		#region Indicator

		internal GroupByMovingIndicator Indicator
		{
			get
			{
				Style resolved = this.GroupByMovingIndicatorStyleResolved;

                ColumnContentProviderBase.SetControlStyle(this._indicator, resolved);

				return this._indicator;
			}
		}

		internal Popup IndicatorContainer
		{
			get { return this._indicatorContainer; }
		}

		#endregion // Indicator

		#endregion // Internal

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