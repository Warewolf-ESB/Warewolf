using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;


using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{



	internal class FormulaElement : XLSXElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_CellFormula"> 
		//   <simpleContent> 
		//   <extension base="ST_Formula"> 
		//   <attribute name="t" type="ST_CellFormulaType" use="optional" default="normal"/> 
		//   <attribute name="aca" type="xsd:boolean" use="optional" default="false"/> 
		//   <attribute name="ref" type="ST_Ref" use="optional"/> 
		//   <attribute name="dt2D" type="xsd:boolean" use="optional" default="false"/> 
		//   <attribute name="dtr" type="xsd:boolean" use="optional" default="false"/> 
		//   <attribute name="del1" type="xsd:boolean" use="optional" default="false"/> 
		//   <attribute name="del2" type="xsd:boolean" use="optional" default="false"/> 
		//   <attribute name="r1" type="ST_CellRef" use="optional"/> 
		//   <attribute name="r2" type="ST_CellRef" use="optional"/> 
		//   <attribute name="ca" type="xsd:boolean" use="optional" default="false"/> 
		//   <attribute name="si" type="xsd:unsignedInt" use="optional"/> 
		//   <attribute name="bx" type="xsd:boolean" use="optional" default="false"/> 
		//   </extension> 
		//   </simpleContent> 
		// </complexType>  
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>f</summary>
		public const string LocalName = "f";
		
		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/f</summary>
		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			FormulaElement.LocalName;

		private const string TAttributeName = "t";
		private const string AcaAttributeName = "aca";
		private const string RefAttributeName = "ref";
		private const string Dt2DAttributeName = "dt2D";
		private const string DtrAttributeName = "dtr";
		private const string Del1AttributeName = "del1";
		private const string Del2AttributeName = "del2";
		private const string R1AttributeName = "r1";
		private const string R2AttributeName = "r2";
		private const string CaAttributeName = "ca";
		private const string SiAttributeName = "si";
		private const string BxAttributeName = "bx";

		#endregion Constants

		#region Base class overrides

		#region Type

	    public override XLSXElementType Type
	    {
		    get { return XLSXElementType.f; }
	    }

        #endregion Type

		#region Load

	    /// <summary>Loads the data for this element from the specified manager.</summary>
	    protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
	    {
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell = manager.ContextStack[typeof(WorksheetCell)] as WorksheetCell;
			//if (cell == null)
			//{
			//    Utilities.DebugFail("Could not get the cell off of the context stack");
			//    return;
			//}
			WorksheetRow row = (WorksheetRow)manager.ContextStack[typeof(WorksheetRow)];
			object columnIndexValue = manager.ContextStack[typeof(ColumnIndex)];

			if (row == null || columnIndexValue == null)
			{
				Utilities.DebugFail("Could not get the cell off of the context stack");
				return;
			}

			ColumnIndex columnIndex = (ColumnIndex)columnIndexValue;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
            //Worksheet worksheet = cell.Worksheet;
			Worksheet worksheet = row.Worksheet;

            ST_CellFormulaType formulaType = ST_CellFormulaType.normal;
            bool recalculateAlways = false;
            bool isTwoDimensional = false;
            bool isDataTableRow = false;
            WorksheetCell r1 = null, r2 = null;
            WorksheetRegion formulaRegion = null;
            Formula cellFormula = null;
            int sharedGroupIndex = -1;

            #region Attribute Deserialization

            object attributeValue = null;
		    foreach ( ExcelXmlAttribute attribute in element.Attributes )
		    {
			    string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );
			    switch ( attributeName )
			    {
				    case FormulaElement.TAttributeName:
				    {
					    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_CellFormulaType, "normal" );
                        formulaType = (ST_CellFormulaType)attributeValue;
				    }
				    break;

				    case FormulaElement.AcaAttributeName:
				    {
					    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        recalculateAlways = (bool)attributeValue;
				    }
				    break;

				    case FormulaElement.RefAttributeName:
				    {
					    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_Ref, "null" );

						// MD 7/26/10 - TFS34398
						// Use the cached worksheet.
                        //formulaRegion = Utilities.ParseA1RegionAddress((string)attributeValue, cell.Worksheet);      
						// MD 4/6/12 - TFS101506
						//formulaRegion = Utilities.ParseA1RegionAddress((string)attributeValue, worksheet);
						formulaRegion = Utilities.ParseA1RegionAddress((string)attributeValue, worksheet, CultureInfo.InvariantCulture);
				    }
				    break;

				    case FormulaElement.Dt2DAttributeName:
				    {
					    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        isTwoDimensional = (bool)attributeValue;						
				    }
				    break;

				    case FormulaElement.DtrAttributeName:
				    {
					    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        isDataTableRow = (bool)attributeValue;
				    }
				    break;

				    case FormulaElement.Del1AttributeName:
				    {
                        // Roundtrip - Page 1968
					    //attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

				    }
				    break;

				    case FormulaElement.Del2AttributeName:
				    {
                        // Roundtrip - Page 1968
                        //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
				    }
				    break;

				    case FormulaElement.R1AttributeName:
				    {
						// MD 4/12/11 - TFS67084
						// Renamed variables for clarity
						//short col;
						//int row;
						//attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, "null");
						//if (Utilities.ParseA1CellAddress((string)attributeValue, WorkbookFormat.Excel2007, out col, out row))                        
						//    r1 = worksheet.Rows[row].Cells[col];                        
						//else
						//    Utilities.DebugFail("Unable to parse the R1 cell");
						short r1ColumnIndex;
						int r1RowIndex;
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, "null");

						// MD 4/6/12 - TFS101506
						//if (Utilities.ParseA1CellAddress((string)attributeValue, WorkbookFormat.Excel2007, out r1ColumnIndex, out r1RowIndex))
						if (Utilities.ParseA1CellAddress((string)attributeValue, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out r1ColumnIndex, out r1RowIndex))
							r1 = worksheet.Rows[r1RowIndex].Cells[r1ColumnIndex];
						else
							Utilities.DebugFail("Unable to parse the R1 cell");
				    }
				    break;

				    case FormulaElement.R2AttributeName:
				    {
						// MD 4/12/11 - TFS67084
						// Renamed variables for clarity
						//short col;
						//int row;
						//attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, "null");
						//if (Utilities.ParseA1CellAddress((string)attributeValue, WorkbookFormat.Excel2007, out col, out row))
						//    r2 = worksheet.Rows[row].Cells[col];
						//else
						//    Utilities.DebugFail("Unable to parse the R2 cell");
						short r2ColumnIndex;
						int r2RowIndex;
						attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_CellRef, "null");

						// MD 4/6/12 - TFS101506
						//if (Utilities.ParseA1CellAddress((string)attributeValue, WorkbookFormat.Excel2007, out r2ColumnIndex, out r2RowIndex))
						if (Utilities.ParseA1CellAddress((string)attributeValue, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, out r2ColumnIndex, out r2RowIndex))
							r2 = worksheet.Rows[r2RowIndex].Cells[r2ColumnIndex];
						else
							Utilities.DebugFail("Unable to parse the R2 cell");
				    }
				    break;

				    case FormulaElement.CaAttributeName:
				    {
					    // We don't need to do anything here since we will always tell Excel
                        // to recalculate the formula
				    }
				    break;

				    case FormulaElement.SiAttributeName:
				    {
                        sharedGroupIndex = (int)XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
				    }
				    break;

				    case FormulaElement.BxAttributeName:
				    {
                        // Roundtrip - Page 1968
					    attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
				    }
				    break;
			    }
            }
            #endregion //Attribute Deserialization

            Debug.Assert(formulaType == ST_CellFormulaType.dataTable || value != null || sharedGroupIndex > -1, 
                "Expected a non-null value for a formula that isn't a data table or in a shared group");

			// MD 10/27/10 - TFS56976
			// This method was incorrect. We don't tell what kind of formula it is by its contents. We tell by what kind of object owns it.
			//value = Utilities.BuildLoadingFormulaReferenceString(value, manager, out isNamedReferenceFormula, out isExternalFormula);
			value = Utilities.BuildLoadingFormulaReferenceString(value);

            switch (formulaType)
            {
                #region Shared

                case ST_CellFormulaType.shared:
                    // Since we don't fully support the method of shared formulas that are created by
                    // Excel (i.e. breaking up a large shared group into multiple blocks), we will
                    // use the range specified by the 'ref' attribute.  Therefore, if the value is null
                    // it was already part of a different formula that was applied and we can ignore it.
                    if (value != null)
                    {
                        // Ensure that we create this as a normal formula, not shared
						// MD 2/23/12 - TFS101504
						// Pass along the OrderedExternalReferences collection as the indexedReferencesDuringLoad parameter.
						//cellFormula = Formula.Parse( value, CellReferenceMode.A1, FormulaType.Formula, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture );
						//cellFormula = Formula.Parse(value, CellReferenceMode.A1, FormulaType.Formula, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);
						cellFormula = Formula.Parse(value, CellReferenceMode.A1, FormulaType.Formula, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);
                        if (recalculateAlways)
                            cellFormula.RecalculateAlways = recalculateAlways;

						// MD 12/27/10 - TFS62014
						// Don't store the shared formula here. Wait until after it is applied to a cell and store the applied formula so it has 
						// a reference to its owning cell.
						//// MD 3/30/10 - TFS30253
						//// Add the formula to the collection of shared formulas so target cells can get the formula and apply it to themselves.
						//manager.SharedFormulas.Add(sharedGroupIndex, cellFormula);

						// MD 5/26/11 - TFS76587
						// Apparently, the shared formula root might not be the top-left cell of the shared range, so we will always apply the formula to
						// the current cell, not the top-left cell of the range.
						//// MD 12/27/10 - TFS62014
						//// Instead of applying the formula in the if...else below, we will now just determine which cell should have the formula
						//// and then we will apply it afterwards.
						//// MD 4/12/11 - TFS67084
						//// Moved away from using WorksheetCell objects.
						////WorksheetCell cellWithFormula;
						//WorksheetRow cellWithFormulaRow;
						//short cellWithFormulaColumnIndex;
						//
						//if (formulaRegion != null)                        
						//{
						//    // MD 3/30/10 - TFS30253
						//    // The shared formulas don't necessarily have to be in contiguous blocks, so just apply the formula to the top left cell for now 
						//    // and let the other target cells indicate when they want to use the shared formula. This will happen when they have a FORMULA record
						//    // which has a single EXP token pointing to the top left cell of the shared range.
						//    //cellFormula.ApplyTo(formulaRegion);            
						//    // MD 12/27/10 - TFS62014
						//    // Just determine the cell with the formula instead of applying it here.
						//    //cellFormula.ApplyTo(formulaRegion.TopLeftCell);
						//    // MD 4/12/11 - TFS67084
						//    // Moved away from using WorksheetCell objects.
						//    //cellWithFormula = formulaRegion.TopLeftCell;
						//    cellWithFormulaRow = formulaRegion.TopRow;
						//    cellWithFormulaColumnIndex = formulaRegion.FirstColumnInternal;
						//}
						//else
						//{
						//    Utilities.DebugFail("We didn't have a formula region for a shared formula");
						//
						//    // As a fallback, just apply the formula to the cell
						//    // MD 12/27/10 - TFS62014
						//    // Just determine the cell with the formula instead of applying it here.
						//    //cellFormula.ApplyTo(cell);
						//    // MD 4/12/11 - TFS67084
						//    // Moved away from using WorksheetCell objects.
						//    //cellWithFormula = cell;
						//    cellWithFormulaRow = row;
						//    cellWithFormulaColumnIndex = columnIndex;
						//}

						// MD 12/27/10 - TFS62014
						// Apply the formula and store that applied formula in the SharedFormulas collection so the formula has a reference to its
						// owning cell.
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//cellFormula.ApplyTo(cellWithFormula);
						//manager.SharedFormulas.Add(sharedGroupIndex, cellWithFormula.Formula);
						// MD 5/26/11 - TFS76587
						// Apparently, the shared formula root might not be the top-left cell of the shared range, so we will always apply the formula to
						// the current cell, not the top-left cell of the range.
						//cellFormula.ApplyTo(cellWithFormulaRow, cellWithFormulaColumnIndex);
						//manager.SharedFormulas.Add(sharedGroupIndex, (Formula)cellWithFormulaRow.GetCellValueInternal(cellWithFormulaColumnIndex));
						cellFormula.ApplyTo(row, columnIndex);
						manager.SharedFormulas.Add(sharedGroupIndex, row.GetCellFormulaInternal(columnIndex));
                    }
					// MD 3/30/10 - TFS30253
					// The shared formulas don't necessarily have to be in contiguous blocks, so we need to apply the shared formula to the cell when we get the 
					// special FORMULA record for the target cell.
					else
					{
						if (manager.SharedFormulas.TryGetValue(sharedGroupIndex, out cellFormula))
						{
							// MD 12/27/10 - TFS62014
							// Shared formulas are relative to the top left cell of their shared region, so if there are relative references,
							// they need to be offset. Therefore, we need to pass in the owning cell, or top left cell, of the shared formula.
							//cellFormula.ApplyTo(cell);
							// MD 4/12/11 - TFS67084
							// Moved away from using WorksheetCell objects.
							//cellFormula.ApplyTo(cellFormula.OwningCell, cell);
							cellFormula.ApplyTo(cellFormula.OwningCellRow, cellFormula.OwningCellColumnIndex, row, columnIndex);
						}
						else
						{
							Utilities.DebugFail("Could not find the shared formula.");
						}
					}
                    break;

                #endregion //Shared

                #region Normal

                case ST_CellFormulaType.normal:
                    FormulaType resolvedFormulaType = FormulaType.Formula;

					// MD 10/27/10 - TFS56976
					// This code is incorrect. We don't tell what kind of formula it is by its contents. We tell by what kind of object owns it.
					// Since a cell owns this formula and it is marked Normal, it is a regular Formula type.
					//if (isExternalFormula)
					//    resolvedFormulaType = FormulaType.ExternalNamedReferenceFormula;
					//else if (isNamedReferenceFormula)
					//    resolvedFormulaType = FormulaType.NamedReferenceFormula;

					// MD 2/23/12 - TFS101504
					// Pass along the OrderedExternalReferences collection as the indexedReferencesDuringLoad parameter.
					//cellFormula = Formula.Parse( value, CellReferenceMode.A1, resolvedFormulaType, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture );
					//cellFormula = Formula.Parse(value, CellReferenceMode.A1, resolvedFormulaType, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);
					cellFormula = Formula.Parse(value, CellReferenceMode.A1, resolvedFormulaType, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);
                    if(recalculateAlways)
                        cellFormula.RecalculateAlways = recalculateAlways;

                    Debug.Assert(formulaRegion == null, "Did not expect to have a formula region for a normal formula");

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
                    //cellFormula.ApplyTo(cell);
					cellFormula.ApplyTo(row, columnIndex);

                    break;

                #endregion //Normal

                #region Array

                case ST_CellFormulaType.array:
					// MD 3/29/11 - TFS63971
					// We don't need to declare another formula variable here. Just use the cellFormula variable.
					// Replaced all arrayFormula references to cellFormula references as well.
					//ArrayFormula arrayFormula = ArrayFormula.Parse( value, CellReferenceMode.A1, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture );
					// MD 2/23/12 - TFS101504
					// Pass along the OrderedExternalReferences collection as the indexedReferencesDuringLoad parameter.
					// To do this, we need to call the Parse method on the Formula class, not the ArrayFormula class, but if we pass in 
					// FormulaType.ArrayFormula as the formula type, we will get back an ArrayFormula instance.
					//cellFormula = ArrayFormula.Parse(value, CellReferenceMode.A1, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture);
					//cellFormula = Formula.Parse(value, CellReferenceMode.A1, FormulaType.ArrayFormula, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);
					cellFormula = Formula.Parse(value, CellReferenceMode.A1, FormulaType.ArrayFormula, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);

                    if(recalculateAlways)
						cellFormula.RecalculateAlways = recalculateAlways;

                    if (formulaRegion != null)
						cellFormula.ApplyTo(formulaRegion);
                    else
                        Utilities.DebugFail("Expected to have a worksheet region for an array formula");

                    break;

                #endregion //Array

                #region DataTable

                case ST_CellFormulaType.dataTable:
                    if (formulaRegion != null)
                    {
						// MD 7/26/10 - TFS34398
						// Use the cached worksheet.
                        //formulaRegion = cell.Worksheet.GetCachedRegion(formulaRegion.FirstRow - 1, formulaRegion.FirstColumn - 1, formulaRegion.LastRow, formulaRegion.LastColumn);
						formulaRegion = worksheet.GetCachedRegion(formulaRegion.FirstRow - 1, formulaRegion.FirstColumn - 1, formulaRegion.LastRow, formulaRegion.LastColumn);

                        WorksheetCell rowInputCell = null, columnInputCell = null;
                        if (isTwoDimensional)
                        {
                            rowInputCell = r1;
                            columnInputCell = r2;
                        }
                        else if (isDataTableRow)
                            rowInputCell = r1;
                        else
                            columnInputCell = r1;

                        worksheet.DataTables.Add(formulaRegion, columnInputCell, rowInputCell);                        
                    }
                    else
                        Utilities.DebugFail("Expected to have a worksheet region for a DataTable");

                    break;

                #endregion //DataTable
            }

			// MD 3/29/11 - TFS63971
			if (cellFormula != null)
				manager.OnFormulaAdded(cellFormula);
        }
		#endregion Load

		#region Save

	    /// <summary>Saves the data for this element to the specified manager.</summary>
	    protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
	    {
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell = manager.ContextStack[typeof(WorksheetCell)] as WorksheetCell;
			//if (cell == null)
			//{
			//    Utilities.DebugFail("Could not get the cell from the context stack");
			//    return;
			//}
			WorksheetRow row = (WorksheetRow)manager.ContextStack[typeof(WorksheetRow)];
			object columnIndexValue = manager.ContextStack[typeof(ColumnIndex)];
			if (columnIndexValue == null || row == null)
			{
				Utilities.DebugFail("Could not get the cell from the context stack");
				return;
			}

			short columnIndex = (short)(ColumnIndex)columnIndexValue;

			// MD 11/5/10 - TFS49093
			// Moved most of the code to the GetSaveValues method.
			#region Refactored
      
					//string attributeValue = null;
			//Formula formula = cell.Formula;
			//ArrayFormula arrayFormula = null;
			//ST_CellFormulaType formulaType = ST_CellFormulaType.normal;
			//WorksheetDataTable dataTable = cell.AssociatedDataTable;            

			//#region 't' Attribute

			//if (dataTable != null)
			//{
			//    if (dataTable.InteriorCells.TopLeftCell != cell)
			//    {
			//        Utilities.DebugFail("We shouldn't be trying to serialize the Formula element for a cell that is not the master cell");
			//        return;
			//    }
			//    formulaType = ST_CellFormulaType.dataTable;
			//}
			//else if (formula != null)
			//{
			//    switch (formula.Type)
			//    {
			//        case FormulaType.ArrayFormula:
			//            formulaType = ST_CellFormulaType.array;
			//            arrayFormula = formula as ArrayFormula;
			//            break;

			//        case FormulaType.NamedReferenceFormula:
			//        case FormulaType.Formula:
			//            formulaType = ST_CellFormulaType.normal;
			//            break;

			//        case FormulaType.SharedFormula:
			//            Utilities.DebugFail("Not expecting to serialize the shared formula optimization");
			//            goto case FormulaType.Formula;

			//        case FormulaType.ExternalNamedReferenceFormula:                        
			//            formulaType = ST_CellFormulaType.normal;
			//            break;

			//        default:
			//            Utilities.DebugFail("We haven't accounted for this type of formula");
			//            goto case FormulaType.Formula;
			//    }
			//}
			//else
			//    Utilities.DebugFail("The was no data table or formula associated with the cell.  We should not have gotten to this point");

			//attributeValue = XmlElementBase.GetXmlString(formulaType, DataType.ST_CellFormulaType, ST_CellFormulaType.normal, false);
			//if (attributeValue != null)
			//    XmlElementBase.AddAttribute(element, FormulaElement.TAttributeName, attributeValue);

			//#endregion //'t' Attribute

			//#region 'aca' Attribute

			//if (formula != null)
			//{
			//    attributeValue = XmlElementBase.GetXmlString(formula.RecalculateAlways, DataType.Boolean, false, false);
			//    if (attributeValue != null)
			//        XmlElementBase.AddAttribute(element, FormulaElement.AcaAttributeName, attributeValue);
			//}

			//#endregion //'aca' Attribute

			//#region 'ref' Attribute

			//string cellRange = null;
			//if (arrayFormula != null)
			//    cellRange = arrayFormula.CellRange.ToString(CellReferenceMode.A1, false, true, true);
			//else if (dataTable != null)
			//    cellRange = dataTable.InteriorCells.ToString(CellReferenceMode.A1, false, true, true);

			//if (cellRange != null)
			//{
			//    attributeValue = XmlElementBase.GetXmlString(cellRange, DataType.ST_Ref);
			//    XmlElementBase.AddAttribute(element, FormulaElement.RefAttributeName, attributeValue);
			//}

			//#endregion //'ref' Attribute

			//if (dataTable != null)
			//{
			//    WorksheetCell colCell = dataTable.ColumnInputCell;
			//    WorksheetCell rowCell = dataTable.RowInputCell;
			//    bool isTwoDimentional = colCell != null && rowCell != null;
			//    Debug.Assert(colCell != null || rowCell != null, "We should have a ColumnInputCell or a RowInputCell");

			//    #region 'dt2D' Attribute

			//    attributeValue = XmlElementBase.GetXmlString(isTwoDimentional, DataType.Boolean, false, false);
			//    if (attributeValue != null)
			//        XmlElementBase.AddAttribute(element, FormulaElement.Dt2DAttributeName, attributeValue);

			//    #endregion 'dt2D' Attribute

			//    #region 'dtr' Attribute

			//    // This attribute has no meaning for a two-dimensional table
			//    if (isTwoDimentional == false)
			//    {
			//        // 'True' here means that 'r1' refers to the rowInputCell
			//        attributeValue = XmlElementBase.GetXmlString(rowCell != null, DataType.Boolean, false, false);
			//        if (attributeValue != null)
			//            XmlElementBase.AddAttribute(element, FormulaElement.DtrAttributeName, attributeValue);
			//    }

			//    #endregion //'dtr' Attribute

			//    #region 'r1' Attribute

			//    // MD 10/13/10 - TFS44275
			//    // It seems we had the row and col input cells reversed here.
			//    //string cellRef = (colCell ?? rowCell).ToString(CellReferenceMode.A1, false, true, true);                
			//    string cellRef = (rowCell ?? colCell).ToString(CellReferenceMode.A1, false, true, true);

			//    attributeValue = XmlElementBase.GetXmlString(cellRef, DataType.ST_CellRef);
			//    XmlElementBase.AddAttribute(element, FormulaElement.R1AttributeName, attributeValue);

			//    #endregion 'r1' Attribute

			//    #region 'r2' Attribute

			//    if (isTwoDimentional)
			//    {
			//        // MD 10/13/10 - TFS44275
			//        // It seems we had the row and col input cells reversed here.
			//        //cellRef = rowCell.ToString(CellReferenceMode.A1, false, true, true);
			//        cellRef = colCell.ToString(CellReferenceMode.A1, false, true, true);

			//        attributeValue = XmlElementBase.GetXmlString(cellRef, DataType.ST_CellRef);
			//        XmlElementBase.AddAttribute(element, FormulaElement.R2AttributeName, attributeValue);
			//    }
			//    #endregion 'r2' Attribute
			//}

			//// Roundtrip:
			////
			//#region Del1
			//// Name = 'del1', Type = Boolean, Default = False

			//#endregion Del1
			////
			//#region Del2
			//// Name = 'del2', Type = Boolean, Default = False

			//#endregion Del2

			//#region 'ca' Attribute
            
			//// Always serialize 'true' here so that the formula is recalculated by Excel
			//attributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean);
			//XmlElementBase.AddAttribute(element, FormulaElement.CaAttributeName, attributeValue);

			//#endregion 'ca' Attribute

			//#region 'si' Attribute

			//// This is not supported for the moment as we will just write the formula
			//// directly on each cell in the group

			//#endregion Si

			//// Roundtrip:
			////
			//#region Bx
			//// Name = 'bx', Type = Boolean, Default = False

			//#endregion Bx

			//// Set the value
			//if (formula != null)
			//    value = Utilities.BuildSavingFormulaReferenceString(formula, manager); 
    
			#endregion // Refactored
			string tAttributeValue;
			string acaAttributeValue;
			string refAttributeValue;
			string dt2DAttributeValue;
			string dtrAttributeValue;
			string r1AttributeValue;
			string r2AttributeValue;
			string caAttributeValue;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//FormulaElement.GetSaveValues(manager, cell,
			FormulaElement.GetSaveValues(manager, row, columnIndex,
				out tAttributeValue, out acaAttributeValue, out refAttributeValue, out dt2DAttributeValue, out dtrAttributeValue,
				out r1AttributeValue, out r2AttributeValue, out caAttributeValue, out value);

			if (tAttributeValue != null)
				XmlElementBase.AddAttribute(element, FormulaElement.TAttributeName, tAttributeValue);

			if (acaAttributeValue != null)
				XmlElementBase.AddAttribute(element, FormulaElement.AcaAttributeName, acaAttributeValue);

			if (refAttributeValue != null)
				XmlElementBase.AddAttribute(element, FormulaElement.RefAttributeName, refAttributeValue);

			if (dt2DAttributeValue != null)
				XmlElementBase.AddAttribute(element, FormulaElement.Dt2DAttributeName, dt2DAttributeValue);

			if (dtrAttributeValue != null)
				XmlElementBase.AddAttribute(element, FormulaElement.DtrAttributeName, dtrAttributeValue);

			if (r1AttributeValue != null)
				XmlElementBase.AddAttribute(element,FormulaElement.R1AttributeName, r1AttributeValue);

			if (r2AttributeValue != null)
				XmlElementBase.AddAttribute(element,FormulaElement.R1AttributeName, r2AttributeValue);

			if (caAttributeValue != null)
				XmlElementBase.AddAttribute(element,FormulaElement.CaAttributeName, caAttributeValue);
	    }
		#endregion Save

		#endregion Base class overrides

		#region Methods

		// MD 11/4/10 - TFS49093
		#region GetSaveValues

		private static void GetSaveValues(
			Excel2007WorkbookSerializationManager manager,
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell,
			WorksheetRow row, short columnIndex,
			out string tAttributeValue,
			out string acaAttributeValue,
			out string refAttributeValue,
			out string dt2DAttributeValue,
			out string dtrAttributeValue,
			out string r1AttributeValue,
			out string r2AttributeValue,
			out string caAttributeValue,
			out string valueToSave)
		{
			tAttributeValue = null;
			acaAttributeValue = null;
			refAttributeValue = null;
			dt2DAttributeValue = null;
			dtrAttributeValue = null;
			r1AttributeValue = null;
			r2AttributeValue = null;
			caAttributeValue = null;
			valueToSave = null;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Formula formula = cell.Formula;
			object cellValueRaw = row.GetCellValueRaw(columnIndex);
			Formula formula = cellValueRaw as Formula;

			ArrayFormula arrayFormula = null;
			ST_CellFormulaType formulaType = ST_CellFormulaType.normal;

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetDataTable dataTable = cell.AssociatedDataTable;
			WorksheetDataTable dataTable = cellValueRaw as WorksheetDataTable;

			#region 't' Attribute

			if (dataTable != null)
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects and wrapped in #if DEBUG so this check doesn't affect performance at runtime.
				//if (dataTable.InteriorCells.TopLeftCell != cell)


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


				formulaType = ST_CellFormulaType.dataTable;
			}
			else if (formula != null)
			{
				switch (formula.Type)
				{
					case FormulaType.ArrayFormula:
						formulaType = ST_CellFormulaType.array;
						arrayFormula = formula as ArrayFormula;
						break;

					case FormulaType.NamedReferenceFormula:
					case FormulaType.Formula:
						formulaType = ST_CellFormulaType.normal;
						break;

					case FormulaType.SharedFormula:
						Utilities.DebugFail("Not expecting to serialize the shared formula optimization");
						goto case FormulaType.Formula;

					case FormulaType.ExternalNamedReferenceFormula:
						formulaType = ST_CellFormulaType.normal;
						break;

					default:
						Utilities.DebugFail("We haven't accounted for this type of formula");
						goto case FormulaType.Formula;
				}
			}
			else
				Utilities.DebugFail("The was no data table or formula associated with the cell.  We should not have gotten to this point");

			tAttributeValue = XmlElementBase.GetXmlString(formulaType, DataType.ST_CellFormulaType, ST_CellFormulaType.normal, false);

			#endregion //'t' Attribute

			#region 'aca' Attribute

			if (formula != null)
				acaAttributeValue = XmlElementBase.GetXmlString(formula.RecalculateAlways, DataType.Boolean, false, false);

			#endregion //'aca' Attribute

			#region 'ref' Attribute

			string cellRange = null;
			if (arrayFormula != null)
				cellRange = arrayFormula.CellRange.ToString(CellReferenceMode.A1, false, true, true);
			else if (dataTable != null)
				cellRange = dataTable.InteriorCells.ToString(CellReferenceMode.A1, false, true, true);

			if (cellRange != null)
				refAttributeValue = XmlElementBase.GetXmlString(cellRange, DataType.ST_Ref);

			#endregion //'ref' Attribute

			if (dataTable != null)
			{
				WorksheetCell colCell = dataTable.ColumnInputCell;
				WorksheetCell rowCell = dataTable.RowInputCell;
				bool isTwoDimentional = colCell != null && rowCell != null;
				Debug.Assert(colCell != null || rowCell != null, "We should have a ColumnInputCell or a RowInputCell");

				#region 'dt2D' Attribute

				dt2DAttributeValue = XmlElementBase.GetXmlString(isTwoDimentional, DataType.Boolean, false, false);

				#endregion 'dt2D' Attribute

				#region 'dtr' Attribute

				// This attribute has no meaning for a two-dimensional table
				if (isTwoDimentional == false)
				{
					// 'True' here means that 'r1' refers to the rowInputCell
					dtrAttributeValue = XmlElementBase.GetXmlString(rowCell != null, DataType.Boolean, false, false);
				}

				#endregion //'dtr' Attribute

				#region 'r1' Attribute

				string cellRef = (rowCell ?? colCell).ToString(CellReferenceMode.A1, false, true, true);

				r1AttributeValue = XmlElementBase.GetXmlString(cellRef, DataType.ST_CellRef);

				#endregion 'r1' Attribute

				#region 'r2' Attribute

				if (isTwoDimentional)
				{
					cellRef = colCell.ToString(CellReferenceMode.A1, false, true, true);

					r2AttributeValue = XmlElementBase.GetXmlString(cellRef, DataType.ST_CellRef);
				}
				#endregion 'r2' Attribute
			}

			#region 'ca' Attribute

			// Always serialize 'true' here so that the formula is recalculated by Excel
			caAttributeValue = XmlElementBase.GetXmlString(true, DataType.Boolean);

			#endregion 'ca' Attribute

			// Set the value
			if (formula != null)
				valueToSave = Utilities.BuildSavingFormulaReferenceString(formula, manager);
		}

		#endregion // GetSaveValues

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			Excel2007WorkbookSerializationManager manager,
			XmlWriter writer,
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell cell)
			WorksheetRow row, short columnIndex)
		{
			writer.WriteStartElement(FormulaElement.LocalName);

			string tAttributeValue;
			string acaAttributeValue;
			string refAttributeValue;
			string dt2DAttributeValue;
			string dtrAttributeValue;
			string r1AttributeValue;
			string r2AttributeValue;
			string caAttributeValue;
			string valueToSave;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//FormulaElement.GetSaveValues(manager, cell,
			FormulaElement.GetSaveValues(manager, row, columnIndex,
				out tAttributeValue, out acaAttributeValue, out refAttributeValue, out dt2DAttributeValue, out dtrAttributeValue,
				out r1AttributeValue, out r2AttributeValue, out caAttributeValue, out valueToSave);

			if (tAttributeValue != null)
				writer.WriteAttributeString(FormulaElement.TAttributeName, tAttributeValue);

			if (acaAttributeValue != null)
				writer.WriteAttributeString(FormulaElement.AcaAttributeName, acaAttributeValue);

			if (refAttributeValue != null)
				writer.WriteAttributeString(FormulaElement.RefAttributeName, refAttributeValue);

			if (dt2DAttributeValue != null)
				writer.WriteAttributeString(FormulaElement.Dt2DAttributeName, dt2DAttributeValue);

			if (dtrAttributeValue != null)
				writer.WriteAttributeString(FormulaElement.DtrAttributeName, dtrAttributeValue);

			if (r1AttributeValue != null)
				writer.WriteAttributeString(FormulaElement.R1AttributeName, r1AttributeValue);

			if (r2AttributeValue != null)
				writer.WriteAttributeString(FormulaElement.R1AttributeName, r2AttributeValue);

			if (caAttributeValue != null)
				writer.WriteAttributeString(FormulaElement.CaAttributeName, caAttributeValue);

			if (valueToSave != null)
				XmlElementBase.WriteString(writer, valueToSave);

			writer.WriteEndElement();
		}

		#endregion // SaveDirectHelper 

		#endregion // Methods
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