using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Infragistics.Documents.Excel.FormulaUtilities;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;

namespace Infragistics.Documents.Excel.Serialization
{





	internal abstract class WorkbookReferenceBase
	{
		#region Member Variables

		private Dictionary<ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle>, WorksheetReferenceMulti> multiSheetReferences;
		private Dictionary<string, Dictionary<object, NamedReferenceBase>> namedReferences;
		private ReadOnlyCollection<NamedReferenceBase> namedReferencesReadOnly;
		private List<NamedReferenceBase> namedReferencesOrdered;
		private Workbook targetWorkbook;
		private Dictionary<int, WorksheetReferenceSingle> worksheetReferences;

		#endregion Member Variables

		#region Constructor

		// MD 3/30/11 - TFS69969
		// The workbook reference no longer keeps a reference to the serialization manager, because it is stored after loading,
		// so instead it will keep a reference to the target workbook of the reference.
		//protected WorkbookReferenceBase( WorkbookSerializationManager manager ) 
		//{
		//    this.manager = manager;
		//}
		protected WorkbookReferenceBase(Workbook targetWorkbook)
		{
			this.targetWorkbook = targetWorkbook;
		}

		#endregion Constructor

		#region Methods

		#region Abstract Methods

		public abstract NamedReferenceBase CreateNamedReference(string name, object scope);
		public abstract WorksheetReferenceSingle CreateWorksheetReference(int worksheetIndex);
		public abstract string GetWorksheetName(int worksheetIndex);
		public abstract WorksheetReferenceSingle GetWorksheetReference(string worksheetName);
		public abstract string GetWorksheetReferenceString(int firstWorksheetIndex, int lastWorksheetIndex, Dictionary<WorkbookReferenceBase, int> externalReferences);

        #endregion Abstract Methods

        #region Public Methods

		// MD 6/16/12 - CalcEngineRefactor
		#region ClearNamedReferences

		public void ClearNamedReferences()
		{
			this.namedReferences = null;
			this.namedReferencesOrdered = null;
			this.namedReferencesReadOnly = null;
		}

		#endregion // ClearNamedReferences

		#region Connect

		internal virtual WorkbookReferenceBase Connect(FormulaContext context)
		{
			return this;
		}

		#endregion // Connect

		#region Disconnect

		internal virtual WorkbookReferenceBase Disconnect()
		{
			return new WorkbookReferenceUnconnected(this.FileName);
		}

		#endregion // Disconnect

		// MD 6/18/12 - TFS102878
		#region GetMultiSheetReference

		public WorksheetReference GetMultiSheetReference(WorksheetReferenceSingle firstReference, WorksheetReferenceSingle lastReference)
		{
			Debug.Assert(firstReference.WorkbookReference == this && lastReference.WorkbookReference == this, "This is unexpected.");

			ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle> multiSheetTuple = 
				new ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle>(firstReference, lastReference);

			if (this.multiSheetReferences == null)
				this.multiSheetReferences = new Dictionary<ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle>, WorksheetReferenceMulti>();

			WorksheetReferenceMulti multiSheetReference;
			if (this.multiSheetReferences.TryGetValue(multiSheetTuple, out multiSheetReference) == false)
			{
				multiSheetReference = new WorksheetReferenceMulti(firstReference, lastReference);
				this.multiSheetReferences[multiSheetTuple] = multiSheetReference;
			}

			return multiSheetReference;
		} 

		#endregion // GetMultiSheetReference

		#region GetNamedReference( string, object, bool )



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		public NamedReferenceBase GetNamedReference(string name, object scope, bool createIfNotPresent)
		{
			// MD 4/14/09 - TFS16405
			// This code has been moved to GetScopeDictionary.
			#region Moved

			
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


			#endregion Moved
			Dictionary<object, NamedReferenceBase> scopeDictionary = this.GetScopeDictionary( name );

			NamedReferenceBase reference;
			if ( scopeDictionary.TryGetValue( scope, out reference ) == false )
			{
				if ( createIfNotPresent )
				{
					reference = this.CreateNamedReference( name, scope );

					// MD 8/20/07 - BR25818
					// The formula constructor takes another parameter now which indicates the formula type
					//reference.FormulaInternal = new Formula( CellReferenceMode.A1 );
					reference.FormulaInternal = new Formula( CellReferenceMode.A1, this.NamedReferenceFormulaType );

					scopeDictionary.Add( scope, reference );
					this.NamedReferencesOrdered.Add( reference );
				}
			}

			return reference;
		}

		#endregion GetNamedReference( string, object, bool, bool )

		#region GetWorkbookReferenceString

		public string GetWorkbookReferenceString(Dictionary<WorkbookReferenceBase, int> externalReferences)
		{
			return this.GetWorksheetReferenceString(EXTERNSHEETRecord.WorkbookLevelReferenceIndex, EXTERNSHEETRecord.WorkbookLevelReferenceIndex, externalReferences);
		}

		#endregion // GetWorkbookReferenceString

		#region GetWorksheetReference

		// MD 6/18/12 - TFS102878
		public WorksheetReference GetWorksheetReference(string firstWorksheetName, string lastWorksheetName)
		{
			WorksheetReferenceSingle firstReference = this.GetWorksheetReference(firstWorksheetName);

			if (lastWorksheetName == null)
				return firstReference;

			WorksheetReferenceSingle lastReference = this.GetWorksheetReference(lastWorksheetName);
			return this.GetMultiSheetReference(firstReference, lastReference);
		}







		public WorksheetReferenceSingle GetWorksheetReference(int worksheetIndex)
		{
			if (this.worksheetReferences == null)
				this.worksheetReferences = new Dictionary<int, WorksheetReferenceSingle>();

			WorksheetReferenceSingle reference;
			if (this.worksheetReferences.TryGetValue(worksheetIndex, out reference) == false)
			{
				reference = this.CreateWorksheetReference(worksheetIndex);
				this.worksheetReferences.Add(worksheetIndex, reference);
			}

			return reference;
		}

		#endregion GetWorksheetReference

		#region OnWorksheetIndexesChanged

		public void OnWorksheetIndexesChanged()
		{
			if (this.worksheetReferences == null)
				return;

			List<WorksheetReferenceSingle> worksheetReferencesTemp = new List<WorksheetReferenceSingle>(this.worksheetReferences.Values);

			this.worksheetReferences.Clear();

			for (int i = 0; i < worksheetReferencesTemp.Count; i++)
			{
				WorksheetReferenceSingle worksheetReference = worksheetReferencesTemp[i];
				Debug.Assert(this.worksheetReferences.ContainsKey(worksheetReference.FirstWorksheetIndex) == false, "We should not have duplicate indexes here.");
				this.worksheetReferences[worksheetReference.FirstWorksheetIndex] = worksheetReference;
			}
		}

		#endregion // OnWorksheetIndexesChanged

		#region OnWorksheetRemoved

		public void OnWorksheetRemoved(int oldIndex)
		{
			WorksheetReferenceSingle worksheetReference;
			if (this.worksheetReferences == null ||
				this.worksheetReferences.TryGetValue(oldIndex, out worksheetReference) == false)
				return;

			this.worksheetReferences.Remove(oldIndex);

			if (this.multiSheetReferences != null)
			{
				List<ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle>> keysToRemove = 
					new List<ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle>>();

				foreach (ExcelTuple<WorksheetReferenceSingle, WorksheetReferenceSingle> tuple in this.multiSheetReferences.Keys)
				{
					if (tuple.Item1 == worksheetReference || tuple.Item2 == worksheetReference)
						keysToRemove.Add(tuple);
				}

				for (int i = 0; i < keysToRemove.Count; i++)
					this.multiSheetReferences.Remove(keysToRemove[i]);
			}

			this.OnWorksheetIndexesChanged();
		}

		#endregion // OnWorksheetRemoved

		#endregion Public Methods

		#region Protected Methods

		// MD 4/14/09 - TFS16405
		// This code has been moved from GetNamedReference so it could be used in other places.
		#region GetScopeDictionary

		protected Dictionary<object, NamedReferenceBase> GetScopeDictionary( string name )
		{
			if ( this.namedReferences == null )
			{
				CultureInfo culture = CultureInfo.CurrentCulture;
				if (this.TargetWorkbook != null)
					culture = this.TargetWorkbook.CultureResolved;

				this.namedReferences = new Dictionary<string, Dictionary<object, NamedReferenceBase>>(StringComparer.Create(culture, true));
			}

			Dictionary<object, NamedReferenceBase> scopeDictionary;
			if ( this.namedReferences.TryGetValue( name, out scopeDictionary ) == false )
			{
				scopeDictionary = new Dictionary<object, NamedReferenceBase>();
				this.namedReferences.Add( name, scopeDictionary );
			}

			return scopeDictionary;
		}  

		#endregion GetScopeDictionary

		#endregion Protected Methods

		#endregion Methods

		#region Properties

		// MD 10/8/07 - BR27172
		#region FileName






		public virtual string FileName
		{
			get { return null; }
		}

		#endregion FileName

		#region IsConnected

		public virtual bool IsConnected
		{
			get { return true; }
		}

		#endregion // IsConnected

		// MD 6/13/12 - CalcEngineRefactor
		#region IsExternal

		public abstract bool IsExternal { get; }

		#endregion // IsExternal

		// MD 3/30/11 - TFS69969
		// The workbook reference no longer keeps a reference to the serialization manager, because it is stored after loading.
		//// MD 7/9/08 - Excel 2007 Format
		//#region Manager
		//
		//internal WorkbookSerializationManager Manager
		//{
		//    get { return this.manager; }
		//} 
		//
		//#endregion Manager

		#region NamedReferenceFormulaType

		// MD 8/20/07 - BR25818






		protected abstract FormulaType NamedReferenceFormulaType { get;}

		#endregion NamedReferenceFormulaType

		#region NamedReferencesOrdered

		// MD 4/14/09 - TFS16405
		// Changed the visibility to protected so that it could be used in the CurrentWorkbookReference derived class.
		//private List<NamedReferenceBase> NamedReferencesOrdered
		protected List<NamedReferenceBase> NamedReferencesOrdered
		{
			get
			{
				if ( this.namedReferencesOrdered == null )
					this.namedReferencesOrdered = new List<NamedReferenceBase>();

				return this.namedReferencesOrdered;
			}
		}

		#endregion NamedReferencesOrdered

		#region NamedReferences

		public ReadOnlyCollection<NamedReferenceBase> NamedReferences
		{
			get
			{
				if ( this.namedReferencesReadOnly == null )
					this.namedReferencesReadOnly = this.NamedReferencesOrdered.AsReadOnly();

				return this.namedReferencesReadOnly;
			}
		}

		#endregion NamedReferences

		// MD 3/30/11 - TFS69969
		// The workbook reference no longer keeps a reference to the serialization manager, because it is stored after loading,
		// so instead it will keep a reference to the target workbook of the reference.
		#region TargetWorkbook

		public Workbook TargetWorkbook
		{
			get { return this.targetWorkbook; }
		}

		#endregion // TargetWorkbook

        // MBS 9/10/08 - Excel 2007
        #region WorkbookScope

        internal virtual object WorkbookScope
        {
            get { return this; }
        }

        #endregion //WorkbookScope

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