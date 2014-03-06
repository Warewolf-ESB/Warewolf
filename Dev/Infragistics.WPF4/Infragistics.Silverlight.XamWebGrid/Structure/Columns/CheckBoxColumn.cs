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
using System.Windows.Data;
using System.Reflection;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A <see cref="Column"/> that generates a <see cref="CheckBox"/> as the content for a <see cref="Cell"/>.
	/// </summary>
    public class CheckBoxColumn : CustomDisplayEditableColumn
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckBoxColumn"/> class.
		/// </summary>
		public CheckBoxColumn()
		{
		    this.VerticalContentAlignment = this.EditorVerticalContentAlignment = VerticalAlignment.Center;
			this.EditorHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.AddNewRowEditorTemplateVerticalContentAlignment = this.AddNewRowItemTemplateVerticalContentAlignment = VerticalAlignment.Center;
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.AddNewRowEditorTemplateHorizontalContentAlignment = HorizontalAlignment.Center;
		}
		#endregion // Constructor

		#region Overrides

		#region GenerateContentProvider
		/// <summary>
		/// Generates a new <see cref="CheckBoxColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new CheckBoxColumnContentProvider();
		}
		#endregion // GenerateContentProvider

        #region FilterMenuCustomFilterString
        /// <summary>
        /// Gets the default string for the FilterMenu for the CustomFilter text
        /// </summary>
        protected override string FilterMenuCustomFilterString
        {
            get
            {
                return SRGrid.GetString("BoolFilters");
            }
        }
        #endregion // FilterMenuCustomFilterString

		#endregion // Overrides
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