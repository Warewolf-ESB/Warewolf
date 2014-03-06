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
	/// An object that contains settings for using Editing on a particular <see cref="ColumnLayout"/>
	/// </summary>
	public class EditingSettingsOverride : EditingSettingsBaseOverride
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
				SettingsBase settings = null;
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.EditingSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#region ResolveEditingType
		/// <summary>
		/// Determines what <see cref="EditingType"/> will be supported by the <see cref="EditingSettingsBaseOverride"/> object.
		/// </summary>
		/// <returns></returns>
		protected internal override EditingType ResolveEditingType()
		{
			return this.AllowEditingResolved;
		}
		#endregion // ResolveEditingType

		#endregion // Overrides

		#region AllowEditing

		/// <summary>
		/// Identifies the <see cref="AllowEditing"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowEditingProperty = DependencyProperty.Register("AllowEditing", typeof(EditingType?), typeof(EditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowEditingChanged)));

		/// <summary>
		/// Gets/Sets if Editing is enabled, and if so, what mode it should be in for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<EditingType>))]
		public EditingType? AllowEditing
		{
			get { return (EditingType?)this.GetValue(AllowEditingProperty); }
			set { this.SetValue(AllowEditingProperty, value); }
		}

		private static void AllowEditingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			EditingSettingsOverride settings = (EditingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowEditing");
		}

		#endregion // AllowEditing

		#region AllowEditingResolved

		/// <summary>
		/// Resolves the <see cref="EditingSettingsOverride.AllowEditing"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public EditingType AllowEditingResolved
		{
            get
            {
                if (this.AllowEditing == null)
                {
                    if (this.SettingsObject != null)
                        return ((EditingSettings)this.SettingsObject).AllowEditing;
                }
                else
                    return (EditingType)this.AllowEditing;

                return (EditingType)EditingSettings.AllowEditingProperty.GetMetadata(typeof(EditingSettings)).DefaultValue;
            }
		}

		#endregion //AllowEditingResolved
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