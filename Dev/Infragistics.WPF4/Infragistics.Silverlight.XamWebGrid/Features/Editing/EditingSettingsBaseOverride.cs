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
	/// A class that controls the editing settings for an object on the <see cref="ColumnLayout"/>.
	/// </summary>
	public abstract class EditingSettingsBaseOverride : SettingsOverrideBase
	{
		#region Overrides

		#region SettingsObject

		/// <summary>
		/// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsOverrideBase"/>
		/// </summary>
		protected override SettingsBase SettingsObject
		{
			get
			{
				return null;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region Properties

		#region IsF2EditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsF2EditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsF2EditingEnabledProperty = DependencyProperty.Register("IsF2EditingEnabled", typeof(bool?), typeof(EditingSettingsBaseOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsF2EditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets if pressing the F2 key will cause the ActiveCell to enter edit mode, on a particular <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? IsF2EditingEnabled
		{
			get { return (bool?)this.GetValue(IsF2EditingEnabledProperty); }
			set { this.SetValue(IsF2EditingEnabledProperty, value); }
		}

		private static void IsF2EditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBaseOverride settings = (EditingSettingsBaseOverride)obj;
			settings.OnPropertyChanged("IsF2EditingEnabled");
		}

		#endregion // IsF2EditingEnabled

		#region IsF2EditingEnabledResolved

		/// <summary>
		/// Resolves the <see cref="EditingSettingsBaseOverride.IsF2EditingEnabled"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public bool IsF2EditingEnabledResolved
		{
            get
            {
                if (this.IsF2EditingEnabled == null)
                {
                    if (this.SettingsObject != null)
                        return ((EditingSettingsBase)this.SettingsObject).IsF2EditingEnabled;
                }
                else
                    return (bool)this.IsF2EditingEnabled;

                return (bool)EditingSettingsBase.IsF2EditingEnabledProperty.GetMetadata(typeof(EditingSettingsBase)).DefaultValue;
            }
		}

		#endregion //IsF2EditingEnabledResolved

		#region IsEnterKeyEditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsEnterKeyEditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsEnterKeyEditingEnabledProperty = DependencyProperty.Register("IsEnterKeyEditingEnabled", typeof(bool?), typeof(EditingSettingsBaseOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsEnterKeyEditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets if pressing the Enter ke will cause the ActiveCell to enter edit mode on a particular <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? IsEnterKeyEditingEnabled
		{
			get { return (bool?)this.GetValue(IsEnterKeyEditingEnabledProperty); }
			set { this.SetValue(IsEnterKeyEditingEnabledProperty, value); }
		}

		private static void IsEnterKeyEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBaseOverride settings = (EditingSettingsBaseOverride)obj;
			settings.OnPropertyChanged("IsEnterKeyEditingEnabled");
		}

		#endregion // IsEnterKeyEditingEnabled

		#region IsEnterKeyEditingEnabledResolved

		/// <summary>
		/// Resolves the <see cref="EditingSettingsBaseOverride.IsEnterKeyEditingEnabled"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public bool IsEnterKeyEditingEnabledResolved
		{
			get
			{
                if (this.IsEnterKeyEditingEnabled == null)
                {
                    if (this.SettingsObject != null)
                        return ((EditingSettingsBase)this.SettingsObject).IsEnterKeyEditingEnabled;
                }
                else
                    return (bool)this.IsEnterKeyEditingEnabled;

                return (bool)EditingSettingsBase.IsEnterKeyEditingEnabledProperty.GetMetadata(typeof(EditingSettingsBase)).DefaultValue;
			}
		}

		#endregion //IsEnterKeyEditingEnabledResolved

		#region IsMouseActionEditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsMouseActionEditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsMouseActionEditingEnabledProperty = DependencyProperty.Register("IsMouseActionEditingEnabled", typeof(MouseEditingAction?), typeof(EditingSettingsBaseOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsMouseActionEditingEnabledChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="MouseEditingAction"/> that can cause a <see cref="Cell"/> to enter edit mode, on a particular <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<MouseEditingAction>))]
		public MouseEditingAction? IsMouseActionEditingEnabled
		{
			get { return (MouseEditingAction?)this.GetValue(IsMouseActionEditingEnabledProperty); }
			set { this.SetValue(IsMouseActionEditingEnabledProperty, value); }
		}

		private static void IsMouseActionEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBaseOverride settings = (EditingSettingsBaseOverride)obj;
			settings.OnPropertyChanged("IsMouseActionEditingEnabled");
		}

		#endregion // IsMouseActionEditingEnabled

		#region IsMouseActionEditingEnabledResolved

		/// <summary>
		/// Resolves the <see cref="EditingSettingsBaseOverride.IsMouseActionEditingEnabled"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public MouseEditingAction IsMouseActionEditingEnabledResolved
		{
			get
			{
                if (this.IsMouseActionEditingEnabled == null)
                {
                    if (this.SettingsObject != null)
                        return ((EditingSettingsBase)this.SettingsObject).IsMouseActionEditingEnabled;
                }
                else
                    return (MouseEditingAction)this.IsMouseActionEditingEnabled;

                return (MouseEditingAction)EditingSettingsBase.IsMouseActionEditingEnabledProperty.GetMetadata(typeof(EditingSettingsBase)).DefaultValue;
			}
		}

		#endregion //IsMouseActionEditingEnabledResolved

		#region IsOnCellActiveEditingEnabled

		/// <summary>
		/// Identifies the <see cref="IsOnCellActiveEditingEnabled"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsOnCellActiveEditingEnabledProperty = DependencyProperty.Register("IsOnCellActiveEditingEnabled", typeof(bool?), typeof(EditingSettingsBaseOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsOnCellActiveEditingEnabledChanged)));


		/// <summary>
		/// Gets/Sets if a <see cref="Cell"/> will enter edit mode when it becomes active, on a particular <see cref="ColumnLayout"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? IsOnCellActiveEditingEnabled
		{
			get { return (bool?)this.GetValue(IsOnCellActiveEditingEnabledProperty); }
			set { this.SetValue(IsOnCellActiveEditingEnabledProperty, value); }
		}

		private static void IsOnCellActiveEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsBaseOverride settings = (EditingSettingsBaseOverride)obj;
			settings.OnPropertyChanged("IsOnCellActiveEditingEnabled");
		}

		#endregion // IsOnCellActiveEditingEnabled

		#region IsOnCellActiveEditingEnabledResolved

		/// <summary>
		/// Resolves the <see cref="EditingSettingsBaseOverride.IsOnCellActiveEditingEnabled"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public bool IsOnCellActiveEditingEnabledResolved
		{
			get
			{
                if (this.IsOnCellActiveEditingEnabled == null)
                {
                    if (this.SettingsObject != null)
                        return ((EditingSettingsBase)this.SettingsObject).IsOnCellActiveEditingEnabled;
                }
                else
                    return (bool)this.IsOnCellActiveEditingEnabled;

                return (bool)EditingSettingsBase.IsOnCellActiveEditingEnabledProperty.GetMetadata(typeof(EditingSettingsBase)).DefaultValue;
			}
		}

		#endregion //IsOnCellActiveEditingEnabledResolved

		#region ResolveEditingType

		/// <summary>
		/// Determines what <see cref="EditingType"/> will be supported by the <see cref="EditingSettingsBaseOverride"/> object.
		/// </summary>
		/// <returns></returns>
		protected internal abstract EditingType ResolveEditingType();

		#endregion // ResolveEditingType

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