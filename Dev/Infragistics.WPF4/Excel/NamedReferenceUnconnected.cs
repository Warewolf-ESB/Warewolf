using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;

namespace Infragistics.Documents.Excel
{
	// MD 6/16/12 - CalcEngineRefactor
	internal class NamedReferenceUnconnected : NamedReferenceBase
	{
		#region Member Variables

		private WorkbookFormat currentFormat;

		#endregion // Member Variables

		#region Constructor

		public NamedReferenceUnconnected(string name, WorkbookFormat currentFormat)
			: this(name, null, false, currentFormat) { }

		public NamedReferenceUnconnected(string name, object scope, bool hidden, WorkbookFormat currentFormat)
			: base(scope, hidden)
		{
			this.Name = name;
			this.currentFormat = currentFormat;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region CalcReference

		internal override IExcelCalcReference CalcReference
		{
			get { return new UCReference(this.Name); }
		}

		#endregion // CalcReference

		#region Culture

		internal override CultureInfo Culture
		{
			get { return CultureInfo.CurrentCulture; }
		}

		#endregion // Culture

		#region CurrentFormat

		internal override WorkbookFormat CurrentFormat
		{
			get { return this.currentFormat; }
		}

		#endregion // CurrentFormat

		#region Equals

		public override bool Equals(object obj)
		{
			NamedReferenceUnconnected other = obj as NamedReferenceUnconnected;
			return other != null && this.Name == other.Name;
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}

		#endregion // GetHashCode

		#region ToString

		internal override string ToString(Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			WorkbookReferenceBase workbookReference = this.Scope as WorkbookReferenceBase;
			if (workbookReference != null)
				return workbookReference.GetWorkbookReferenceString(externalReferences) + this.Name;

			WorksheetReference worksheetReference = this.Scope as WorksheetReference;
			if (worksheetReference != null)
				return worksheetReference.GetReferenceName(externalReferences) + this.Name;

			return this.Name;
		}

		#endregion // ToString

		#region Workbook

		internal override Workbook Workbook
		{
			get { return null; }
		}

		#endregion // Workbook

		#region WorkbookReference

		internal override WorkbookReferenceBase WorkbookReference
		{
			get { return this.Scope as WorkbookReferenceBase; }
		}

		#endregion // WorkbookReference

		#endregion // Base Class Overrides
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