using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;

namespace Infragistics.Documents.Excel.Serialization
{






	internal class ExternalWorkbookReference : WorkbookReferenceBase
	{
		#region Member Variables

		private string workbookFileName;
		private List<string> worksheetNames;

		#endregion Member Variables

		#region Constructor

		// MD 3/30/11 - TFS69969
		// The workbook reference no longer keeps a reference to the serialization manager, because it is stored after loading,
		// so instead it will keep a reference to the target workbook of the reference.
		//public ExternalWorkbookReference( string workbookFileName, WorkbookSerializationManager manager )
		//    : base( manager )
		public ExternalWorkbookReference(string workbookFileName, Workbook targetWorkbook)
			: base(targetWorkbook)
		{
			this.workbookFileName = workbookFileName;
			this.worksheetNames = new List<string>();
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CreateNamedReference

		public override NamedReferenceBase CreateNamedReference( string name, object scope )
		{
			ExternalNamedReference namedReference = new ExternalNamedReference( this, scope );
			
			// MD 3/22/11 - TFS67606
			// When creating a named reference internally, don't validate the name. We are either loading or resolving named 
			// references in formula tokens which have already had the names validated.
			//namedReference.Name = name;
			namedReference.SetNameInternal(name, false);

			return namedReference;
		}

		#endregion CreateNamedReference

		#region CreateWorksheetReference

		public override WorksheetReferenceSingle CreateWorksheetReference(int worksheetIndex)
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return new WorksheetReferenceError(this);

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return new WorksheetReferenceToWorkbook(this);

			return new WorksheetReferenceExternal(this, worksheetIndex);
		}

		#endregion // CreateWorksheetReference

		// MD 10/8/07 - BR27172
		#region FileName






		public override string FileName
		{
			get { return this.workbookFileName; }
		}

		#endregion FileName

		#region GetWorksheetName

		public override string GetWorksheetName( int worksheetIndex )
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return FormulaParser.ReferenceErrorValue;

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return null;

			return this.worksheetNames[ worksheetIndex ];
		}

		#endregion GetWorksheetName

		#region GetWorksheetReference

		public override WorksheetReferenceSingle GetWorksheetReference(string worksheetName)
		{
			if (worksheetName == null)
				return this.GetWorksheetReference(EXTERNSHEETRecord.WorkbookLevelReferenceIndex);

			CultureInfo culture = this.TargetWorkbook.CultureResolved;
			if (String.Compare(worksheetName, FormulaParser.ReferenceErrorValue, culture, CompareOptions.IgnoreCase) == 0)
				return this.GetWorksheetReference(EXTERNSHEETRecord.SheetCannotBeFoundIndex);

			for ( int i = 0; i < this.worksheetNames.Count; i++ )
			{
				string name = this.worksheetNames[ i ];
				if (String.Compare(name, worksheetName, culture, CompareOptions.IgnoreCase) == 0)
					return this.GetWorksheetReference(i);
			}

			this.worksheetNames.Add(worksheetName);
			return this.GetWorksheetReference(this.worksheetNames.Count - 1);
		}

		#endregion GetWorksheetReference

		#region GetWorksheetReferenceString

		public override string GetWorksheetReferenceString(int firstWorksheetIndex, int lastWorksheetIndex, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			bool isIndexed;
			string resolvedFileName = this.GetResolvedFileName(externalReferences, out isIndexed);
			if (firstWorksheetIndex == lastWorksheetIndex)
				return Utilities.CreateReferenceString(resolvedFileName, isIndexed, this.GetWorksheetName(firstWorksheetIndex), null);

			return Utilities.CreateReferenceString(resolvedFileName, isIndexed, this.GetWorksheetName(firstWorksheetIndex), this.GetWorksheetName(lastWorksheetIndex));
		}

		#endregion GetWorksheetReferenceString

		// MD 6/13/12 - CalcEngineRefactor
		#region IsExternal

		public override bool IsExternal
		{
			get { return true; }
		}

		#endregion // IsExternal

		// MD 8/20/07 - BR25818
		// A new abstract member was added on the base type, override it and return the current type
		#region NamedReferenceFormulaType







		protected override FormulaType NamedReferenceFormulaType
		{
			get { return FormulaType.ExternalNamedReferenceFormula; }
		}

		#endregion NamedReferenceFormulaType

		#region ToString

		public override string ToString()
		{
			return this.workbookFileName;
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Methods

		#region GetResolvedFileName

		private string GetResolvedFileName(Dictionary<WorkbookReferenceBase, int> externalReferences, out bool isIndexed)
		{
			if (externalReferences != null)
			{
				int index;
				if (externalReferences.TryGetValue(this, out index))
				{
					isIndexed = true;
					return String.Format("[{0}]", index);
				}
			}

			isIndexed = false;
			return this.FileName;
		}

		#endregion // GetResolvedFileName

		#endregion // Methods

		#region Properties

		// MD 10/8/07 - BR27172
		// This was replaced by the virtual FileName property
		//#region WorkbookFileName
		//
		//public string WorkbookFileName
		//{
		//    get { return this.workbookFileName; }
		//}
		//
		//#endregion WorkbookFileName

		#region WorksheetNames

		public List<string> WorksheetNames
		{
			get { return this.worksheetNames; }
		}

		#endregion WorksheetNames

		#endregion Properties
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