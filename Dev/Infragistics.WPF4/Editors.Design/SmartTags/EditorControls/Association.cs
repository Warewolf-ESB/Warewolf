using System;
using Infragistics.Windows.Editors;

namespace Infragistics.Windows.Design.Editors
{
    /// <summary>
    /// A class wich is used for association between predefined DesignerActionList and an Infragistics control
    /// </summary>
    public static class Association
    {
        #region Methods

			#region Public Methods

				#region GetCorrespondingType

		/// <summary>
        /// A method wich is used for association between predefined DesignerActionList and an Infragistics control
        /// </summary>
        /// <param name="controlType">A control type</param>
        /// <returns>Predefined DesignerActionList type</returns>
        public static Type GetCorrespondingType(Type controlType)
        {
			if (controlType == typeof(XamCheckEditor))
			    return typeof(DALXamCheckEditor);
			else
			if (controlType == typeof(XamComboEditor))
				return typeof(DALXamComboEditor);
			else
			if (controlType == typeof(XamTextEditor))
				return typeof(DALXamTextEditor);
			else
			if (controlType == typeof(XamMaskedEditor))
				return typeof(DALXamMaskedEditor);
			else
			if (controlType == typeof(XamDateTimeEditor))
				return typeof(DALXamDateTimeEditor);
			else
			if (controlType == typeof(XamNumericEditor))
				return typeof(DALXamNumericEditor);
			else
			if (controlType == typeof(XamCurrencyEditor))
				return typeof(DALXamCurrencyEditor);

            return null;
		}

				#endregion //GetCorrespondingType

			#endregion //Public Methods

		#endregion //Methods
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