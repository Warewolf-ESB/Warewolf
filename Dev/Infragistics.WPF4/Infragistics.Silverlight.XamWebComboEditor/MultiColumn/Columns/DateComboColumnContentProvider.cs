using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// A class used to provide content to a <see cref="ComboCellBase"/> object for a particular <see cref="ImageComboColumn"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class DateComboColumnContentProvider : TextComboColumnContentProvider
	{
		#region Overrides

		#region ApplyFormatting

		/// <summary>
		/// Allows the <see cref="ComboColumnContentProviderBase"/> to update the value being set for the display element.
		/// </summary>
		/// <param propertyName="value">The original data value from the underlying data.</param>
		/// <param propertyName="column">The <see cref="ComboColumn"/> whose properties should be used to determine the formatting.</param>
		/// <param propertyName="culture"></param>
		/// <returns>The value that should be displayed in the <see cref="ComboCellBase"/></returns>
		public override object ApplyFormatting(object value, ComboColumn column, System.Globalization.CultureInfo culture)
		{
			TextComboColumn col = (TextComboColumn)column;

			if (!string.IsNullOrEmpty(col.FormatString))
				return String.Format(culture, col.FormatString, value);

			if (!string.IsNullOrEmpty(col.DataField.FormatString))
				return String.Format(culture, col.DataField.FormatString, value);

			return String.Format(culture, "{0:d}", value);
		}

		#endregion // ApplyFormatting

		#endregion //Overrides
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