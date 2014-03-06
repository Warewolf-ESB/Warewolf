using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class DefinedNameElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "definedName";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            DefinedNameElement.LocalName;

        private const string NameAttributeName = "name";

        // Workbook child attributes        
        private const string CommentAttributeName = "comment";
        private const string CustomMenuAttributeName = "customMenu";
        private const string DescriptionAttributeName = "description";
        private const string HelpAttributeName = "help";
        private const string StatusBarAttributeName = "statusBar";
        private const string LocalSheetIdAttributeName = "localSheetId";
        private const string HiddenAttributeName = "hidden";
        private const string FunctionAttributeName = "function";
        private const string VbProcedureAttributeName = "vbProcedure";
        private const string XlmAttributeName = "xlm";
        private const string FunctionGroupIdAttributeName = "functionGroupId";
        private const string ShortcutKeyAttributeName = "shortcutKey";
        private const string PublishToServerAttributeName = "publishToServer";
        private const string WorkbookParameterAttributeName = "workbookParameter";

        // External book attributes
        private const string RefersToAttributeName = "refersTo";
        private const string SheedIdAttributeName = "sheetId";

        #endregion //Constants

        #region Base Class Overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.definedName; }
        }
        #endregion //Type

        #region Load

        /// <summary>Loads the data for this element from the specified manager.</summary>
        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            Workbook workBook = manager.Workbook;
            ExternalWorkbookReference externalWorkbook = manager.ContextStack[typeof(ExternalWorkbookReference)] as ExternalWorkbookReference;
            object attributeValue = null;
            string name = null;
            string comment = null;
            bool hidden = false;
            bool isFunction = false;
            int sheetId = -1;

			// MD 3/30/11 - TFS69969
			string refersTo = null;

            #region Attribute Loading

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                switch (attributeName)
                {
                    case DefinedNameElement.NameAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, String.Empty);
                            name = (string)attributeValue;
                        }
                        break;

                    case DefinedNameElement.CommentAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, String.Empty);
                            comment = (string)attributeValue;
                        }
                        break;

                    case DefinedNameElement.CustomMenuAttributeName:
                        {
                            // Roundtrip - Page 1887
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, "null");
                        }
                        break;

                    case DefinedNameElement.DescriptionAttributeName:
                        {
                            // Roundtrip - Page 1887
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, "null");
                        }
                        break;

                    case DefinedNameElement.HelpAttributeName:
                        {
                            // Roundtrip - Page 1888
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, "null");                            
                        }
                        break;

                    case DefinedNameElement.StatusBarAttributeName:
                        {
                            // Roundtrip - Page 1889
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, "null");
                        }
                        break;

                    case DefinedNameElement.LocalSheetIdAttributeName:
                        {                            
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
                            sheetId = (int)attributeValue;
                        }
                        break;

                    case DefinedNameElement.HiddenAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                            hidden = (bool)attributeValue;                            
                        }
                        break;

                    case DefinedNameElement.FunctionAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                            isFunction = (bool)attributeValue;
                        }
                        break;

                    case DefinedNameElement.VbProcedureAttributeName:
                        {
                            // Roundtrip - Page 1889
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);                                                        
                        }
                        break;

                    case DefinedNameElement.XlmAttributeName:
                        {
                            // Roundtrip - Page 1890
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        }
                        break;

                    case DefinedNameElement.FunctionGroupIdAttributeName:
                        {
                            // Roundtrip - Page 1887
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.UInt32, null);
                        }
                        break;

                    case DefinedNameElement.ShortcutKeyAttributeName:
                        {
                            // Roundtrip - Page 1889
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, "null");
                        }
                        break;

                    case DefinedNameElement.PublishToServerAttributeName:
                        {
                            // Roundtrip - Page 1889
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        }
                        break;

                    case DefinedNameElement.WorkbookParameterAttributeName:
                        {
                            // Roundtrip - Page 1890
                            //attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        }
                        break;

                    case DefinedNameElement.RefersToAttributeName:
                        {
							// MD 3/30/11 - TFS69969
							attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.ST_Xstring, String.Empty);
							refersTo = (string)attributeValue;
                        }
                        break;

                    case DefinedNameElement.SheedIdAttributeName:
                        {
                            attributeValue = XmlElementBase.GetAttributeValue(attribute, DataType.Integer, -1);
                            sheetId = (int)attributeValue;
                        }
                        break;
                }
            }
            #endregion //Attribute Loading

			// MD 5/25/11 - Data Validations / Page Breaks
			// We should ignore the prefix on built in names.
			bool shouldBeBuiltIn = false;
			if (name.StartsWith(NamedReferenceBase.BuildInNamePrefixFor2007))
			{
				name = name.Substring(NamedReferenceBase.BuildInNamePrefixFor2007.Length);
				shouldBeBuiltIn = true;
			}

            // Both the 'externalBook' and 'workbook' elements have elements with a name of 
            // 'definedName', so we need to check the context stack to see which action we
            // should be taking.
            if (externalWorkbook != null)
            {
                WorkbookReferenceBase workbookReference = manager.WorkbookReferences[manager.WorkbookReferences.Count - 1];
                Debug.Assert((workbookReference is CurrentWorkbookReference) == false);

                object scope = workbookReference;
                if (sheetId > -1)
                    scope = workbookReference.GetWorksheetReference(sheetId);

                NamedReferenceBase reference = workbookReference.GetNamedReference(name, scope, true);

				// MD 4/6/12 - TFS102169
				// Wrapped in an if statement. External named references may not have a refersTo value.
				if(refersTo != null)
				{
				// MD 3/30/11 - TFS69969
				Formula formula = Formula.Parse(refersTo, CellReferenceMode.A1, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture);

				// When we solve the formula on the refersTo attribute, it will be from the context of the loaded workbook, so add the external 
				// workbook's file name to all 3D references in the parsed formula.
				for (int i = 0; i < formula.PostfixTokenList.Count; i++)
				{
					ReferenceToken token = formula.PostfixTokenList[i] as ReferenceToken;
					if (token == null || token.Is3DReference == false)
						continue;

					token.SetWorkbookReference(externalWorkbook);
				}

				reference.FormulaInternal = formula;
				}
            }
            else
            {
                // If we were provided the ID of a sheet with this named reference, we
                // should load the sheet as the scope
                object scope;
				if ( sheetId > -1 && manager.Workbook.Worksheets.Count > sheetId)
                    scope = manager.Workbook.Worksheets[sheetId];
                else
                    scope = manager.Workbook;

                NamedReference reference = new NamedReference(workBook.NamedReferences, scope, hidden);
                reference.Name = name;
                reference.IsFunction = isFunction;
                reference.Comment = comment;

				// MD 10/27/10 - TFS56976
				// This method was incorrect. We don't tell what kind of formula it is by its contents. We tell by what kind of object owns it.
				//bool isNamedReferenceFormula, isExternalFormula;
				//string formulaValue = Utilities.BuildLoadingFormulaReferenceString(value, manager, out isNamedReferenceFormula, out isExternalFormula);
				string formulaValue = Utilities.BuildLoadingFormulaReferenceString(value);

				// MD 10/27/10 - TFS56976
				// This code is incorrect. We don't tell what kind of formula it is by its contents. We tell by what kind of object owns it.
				// Since a named references owns this formula, it is a regular NamedReferenceFormula type.
				//FormulaType type = FormulaType.Formula;
				//if (isExternalFormula)
				//    type = FormulaType.ExternalNamedReferenceFormula;
				//else if (isNamedReferenceFormula)
				//    type = FormulaType.NamedReferenceFormula;
				FormulaType type = FormulaType.NamedReferenceFormula;

				// MD 3/30/11
				// Found whilw fixing TFS69969
				// Get the format from the workbook instead of always using Excel2007.
				//Formula formula = Formula.Parse( formulaValue, manager.Workbook.CellReferenceMode, type, WorkbookFormat.Excel2007, CultureInfo.InvariantCulture );
				// MD 2/23/12 - TFS101504
				// Pass along the OrderedExternalReferences collection as the indexedReferencesDuringLoad parameter.
				//Formula formula = Formula.Parse(formulaValue, manager.Workbook.CellReferenceMode, type, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture);
				Formula formula = Formula.Parse(formulaValue, manager.Workbook.CellReferenceMode, type, manager.Workbook.CurrentFormat, CultureInfo.InvariantCulture, manager.OrderedExternalReferences);

                reference.FormulaInternal = formula;

				// MD 4/6/12 - TFS102169
				// In the workbook part, the defined names are before the custom views, so we need to delay loading the named references
				// until after the custom views are loaded (they might have custom view information).
				//manager.AddNonExternalNamedReferenceDuringLoad( reference, hidden );
				Excel2007WorkbookSerializationManager.NamedReferenceInfo info = new Excel2007WorkbookSerializationManager.NamedReferenceInfo();
				info.Hidden = hidden;
				info.Reference = reference;
				manager.NamedReferenceInfos.Add(info);

				// MD 3/29/11 - TFS63971
				manager.OnFormulaAdded(formula);

				// MD 5/25/11 - Data Validations / Page Breaks
				Debug.Assert(shouldBeBuiltIn == reference.IsBuiltIn, "Something is wrong here.");
            }
        }

        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			ListContext<NamedReferenceBase> namedReferences = manager.ContextStack[ typeof( ListContext<NamedReferenceBase> ) ] as ListContext<NamedReferenceBase>;            
            string attributeValue = null;

			NamedReferenceBase namedRefBase = (NamedReferenceBase)namedReferences.ConsumeCurrentItem();
			NamedReference namedRef = namedRefBase as NamedReference;

            // Serialize the attributes for a 'definedName' element that is a descendent of ExternalBook
			if ( namedRef == null )
            {
                #region 'name' Attribute

                attributeValue = XmlElementBase.GetXmlString(namedRefBase.Name, DataType.ST_Xstring);
                XmlElementBase.AddAttribute(element, DefinedNameElement.NameAttributeName, attributeValue);

                #endregion //'name' Attribute

                // Roundtrip
                #region 'refersTo' Attribute

                //if (namedRefBase.FormulaInternal != null)
                //{
                //    attributeValue = XmlElementBase.GetXmlString(namedRefBase.FormulaInternal.ToString(), DataType.ST_Xstring);
                //    XmlElementBase.AddAttribute(element, DefinedNameElement.RefersToAttributeName, attributeValue);
                //}
                #endregion //'refersTo' Attribute
                //
                #region 'sheetId' Attribute

                //Worksheet worksheet = namedRefBase.Scope as Worksheet;
                //if (worksheet != null)
                //{

                //}
                #endregion //'sheetId' Attribute
            }
            // Serialize the attributes for a 'definedName' element that is a descendent of WorkbookElement
            else
            {
                #region 'name' Attribute

				// MD 5/25/11 - Data Validations / Page Breaks
				// Add in the built in name prefix if necessary.
                //attributeValue = XmlElementBase.GetXmlString(namedRef.Name, DataType.ST_Xstring);
				string name = namedRef.Name;
				if (namedRef.IsBuiltIn)
					name = NamedReferenceBase.BuildInNamePrefixFor2007 + name;

				attributeValue = XmlElementBase.GetXmlString(name, DataType.ST_Xstring);

                XmlElementBase.AddAttribute(element, DefinedNameElement.NameAttributeName, attributeValue);

                #endregion //'name' Attribute

                #region 'comment' Attribute

                attributeValue = XmlElementBase.GetXmlString(namedRef.Comment, DataType.ST_Xstring, null, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, DefinedNameElement.CommentAttributeName, attributeValue);

                #endregion //'comment' Attribute

                // Roundtrip
                //
                #region CustomMenu
                // Name = 'customMenu', Type = ST_Xstring, Default = 

                #endregion CustomMenu
                //
                #region Description
                // Name = 'description', Type = ST_Xstring, Default = 

                #endregion Description
                //
                #region Help
                // Name = 'help', Type = ST_Xstring, Default = 

                #endregion Help
                //
                #region StatusBar
                // Name = 'statusBar', Type = ST_Xstring, Default = 

                #endregion StatusBar

                #region 'localSheetId' Attribute

                Worksheet worksheet = namedRef.Scope as Worksheet;
                if (worksheet != null)
                {
                    attributeValue = XmlElementBase.GetXmlString(worksheet.Index, DataType.UInt);
                    XmlElementBase.AddAttribute(element, DefinedNameElement.LocalSheetIdAttributeName, attributeValue);
                }
                #endregion 'localSheetId' Attribute

                #region 'hidden' Attribute

                attributeValue = XmlElementBase.GetXmlString(namedRef.Hidden, DataType.Boolean, false, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, DefinedNameElement.HiddenAttributeName, attributeValue);

                #endregion //'hidden' Attribute

                #region 'function' Attribute

                attributeValue = XmlElementBase.GetXmlString(namedRef.IsFunction, DataType.Boolean, false, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, DefinedNameElement.FunctionAttributeName, attributeValue);

                #endregion 'function' Attribute

                // Roundtrip
                //
                #region VbProcedure
                // Name = 'vbProcedure', Type = Boolean, Default = False

                #endregion VbProcedure
                //
                #region Xlm
                // Name = 'xlm', Type = Boolean, Default = False

                #endregion Xlm
                //
                #region FunctionGroupId
                // Name = 'functionGroupId', Type = UInt32, Default = 

                #endregion FunctionGroupId
                //
                #region ShortcutKey
                // Name = 'shortcutKey', Type = ST_Xstring, Default = 

                #endregion ShortcutKey
                //
                #region PublishToServer
                // Name = 'publishToServer', Type = Boolean, Default = False

                #endregion PublishToServer
                //
                #region WorkbookParameter
                // Name = 'workbookParameter', Type = Boolean, Default = False

                #endregion WorkbookParameter

                // Save the value            
                if (namedRef.FormulaInternal != null)
                {
                    // We need to make sure that we make any necessary replacements to the formula
                    // before saving it out
                    value = Utilities.BuildSavingFormulaReferenceString(namedRef.FormulaInternal, manager);
                }
                else
                    Utilities.DebugFail("Unexpected lack of formula");
            }
        }
        #endregion //Save

        #endregion //Base Class Overrides
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