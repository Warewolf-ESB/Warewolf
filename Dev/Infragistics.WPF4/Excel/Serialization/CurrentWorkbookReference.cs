using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;

namespace Infragistics.Documents.Excel.Serialization
{





	internal class CurrentWorkbookReference : WorkbookReferenceBase
	{
		#region Member Variables

		// MD 2/23/12 - TFS101504
		// This reference needs to know the path of the workbook being loaded.
		private string path;

		// MD 3/30/11 - TFS69969
		// We don't need to store this anymore because the workbook is stored on the base now.
		//private Workbook workbook;

		#endregion Member Variables

		#region Constructor

		// MD 3/30/11 - TFS69969
		// The workbook reference no longer keeps a reference to the serialization manager, because it is stored after loading,
		// so instead it will keep a reference to the target workbook of the reference.
		//public CurrentWorkbookReference( WorkbookSerializationManager manager )
		//    : base( manager )
		//{
		//    this.workbook = manager.Workbook;
		//}
		// MD 2/23/12 - TFS101504
		// This reference needs to know the path of the workbook being loaded.
		//public CurrentWorkbookReference(Workbook targetWorkbook)
		//    : base(targetWorkbook) { }
		public CurrentWorkbookReference(Workbook targetWorkbook, string path)
			: base(targetWorkbook) 
		{
			this.path = path;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CreateNamedReference

		public override NamedReferenceBase CreateNamedReference( string name, object scope )
		{
			// MD 6/16/12 - CalcEngineRefactor
			//
			//// MD 4/14/09 - TFS16405
			//// We were never passing the collection into the constructor here. THis is used when deserializing a 
			//// named reference from a 2003 format workbook. Without the collection, the named reference's Workbook 
			//// is always null and the calc formula never gets compiled, so it always returns null as its value.
			////NamedReference namedReference = new NamedReference( null, scope, hidden );
			//// MD 3/30/11 - TFS69969
			//// The workbook is now stored on the base.
			////NamedReference namedReference = new NamedReference( this.workbook.NamedReferences, scope, hidden );
			//NamedReference namedReference = new NamedReference(this.TargetWorkbook.NamedReferences, scope, hidden);
			//
			//// MD 3/22/11 - TFS67606
			//// When creating a named reference internally, don't validate the name. We are either loading or resolving named 
			//// references in formula tokens which have already had the names validated.
			////namedReference.Name = name;
			//namedReference.SetNameInternal(name, false);
			//
			//return namedReference;
			return new NamedReferenceUnconnected(name, scope, false, this.TargetWorkbook.CurrentFormat);
		}

		#endregion CreateNamedReference

		#region CreateWorksheetReference

		public override WorksheetReferenceSingle CreateWorksheetReference(int worksheetIndex)
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return new WorksheetReferenceError(this);

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return new WorksheetReferenceToWorkbook(this);

			WorksheetCollection worksheets = this.TargetWorkbook.Worksheets;
			if (worksheets.Count <= worksheetIndex)
				return new WorksheetReferenceError(this);

			return new WorksheetReferenceLocal(this, worksheets[worksheetIndex]);
		}

		#endregion // CreateWorksheetReference

		// MD 2/23/12 - TFS101504
		#region FileName

		public override string FileName
		{
			get { return path; }
		}

		#endregion // FileName

		#region GetWorksheetName

		public override string GetWorksheetName( int worksheetIndex )
		{
			if (worksheetIndex == EXTERNSHEETRecord.SheetCannotBeFoundIndex)
				return FormulaParser.ReferenceErrorValue;

			if (worksheetIndex == EXTERNSHEETRecord.WorkbookLevelReferenceIndex)
				return null;

			WorksheetCollection worksheets = this.TargetWorkbook.Worksheets;
			if (worksheets.Count <= worksheetIndex)
			{
				// The reference may be needed before the worksheet is actually created. If that is the case, create a 
				// temporary name for the worksheet based on the index. We will replace it later, as soon as the worksheet
				// is actually created.
				return this.GetIndexBasedWorksheetName(worksheetIndex);
			}

			return worksheets[worksheetIndex].Name;
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

			WorksheetCollection worksheets = this.TargetWorkbook.Worksheets;
			for (int i = 0; i < worksheets.Count; i++)
			{
				Worksheet worksheet = worksheets[i];

				if (String.Compare(worksheet.Name, worksheetName, culture, CompareOptions.IgnoreCase) == 0)
					return this.GetWorksheetReference(i);
			}

			// Return the #REF! workbook if the real workbook can't be found
			return this.GetWorksheetReference(EXTERNSHEETRecord.SheetCannotBeFoundIndex);
		}

		#endregion GetWorksheetReference

		#region GetWorksheetReferenceString

		public override string GetWorksheetReferenceString(int firstWorksheetIndex, int lastWorksheetIndex, Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			if (firstWorksheetIndex == lastWorksheetIndex)
				return Utilities.CreateReferenceString(null, this.GetWorksheetName(firstWorksheetIndex));

			return Utilities.CreateReferenceString(null, this.GetWorksheetName(firstWorksheetIndex), this.GetWorksheetName(lastWorksheetIndex));
		}

        #endregion GetWorksheetReferenceString

		// MD 6/13/12 - CalcEngineRefactor
		#region IsExternal

		public override bool IsExternal
		{
			get { return false; }
		}

		#endregion // IsExternal

        // MD 8/20/07 - BR25818
		// A new abstract member was added on the base type, override it and return the current type
		#region NamedReferenceFormulaType







		protected override FormulaType NamedReferenceFormulaType
		{
			get { return FormulaType.NamedReferenceFormula; }
		}

		#endregion NamedReferenceFormulaType

        // MBS 9/10/08 - Excel 2007
        #region WorkbookScope

        internal override object WorkbookScope
        {
            get
            {
				// MD 3/30/11 - TFS69969
				// The workbook is now stored on the base.
				//return this.workbook;
				return this.TargetWorkbook;
            }
        }
        #endregion //WorkbookScope

        #endregion Base Class Overrides

        #region Methods

        #region AddNamedReference

        public void AddNamedReference( NamedReference namedReference )
		{
			// MD 4/14/09 - TFS16405
			// We don't have to clone the named reference to add it to the current workbook reference. At least I
			// don't see a good reason why it was cloned at this point. We just needed to make sure it was added to
			// the collections on the base class, but that can be done manually. Plus, after fixing the real issue
			// behind TFS16405, the workbook thinks a second named reference with the name and scope is being added
			// here, and that's a problem.
			#region Old Code

			
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)


			#endregion Old Code
			Dictionary<object, NamedReferenceBase> scopeDictionary = this.GetScopeDictionary( namedReference.Name );

			Debug.Assert( 
				scopeDictionary.ContainsKey( namedReference.Scope ) == false, 
				"The named reference should not exist in the dictionary already." );

			scopeDictionary[ namedReference.Scope ] = namedReference;

			this.NamedReferencesOrdered.Add( namedReference );
		}

		#endregion AddNamedReference

		// MD 10/7/10 - TFS36582
		#region GetIndexKey







		private string GetIndexBasedWorksheetName(int worksheetIndex)
		{
			return "?" + worksheetIndex;
		} 

		#endregion // GetIndexKey

		// MD 10/7/10 - TFS36582
		#region UpdateWorksheetReference



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public void UpdateWorksheetReference(Worksheet worksheet)
		{
			//this.RenameWorksheet(this.GetIndexBasedWorksheetName(worksheet.Index), worksheet.Name);
		}

		#endregion // UpdateWorksheetReference

		#endregion Methods
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