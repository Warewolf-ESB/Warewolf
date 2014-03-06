using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class DefinedNamesElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "definedNames";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            DefinedNamesElement.LocalName;

        #endregion //Constants

        #region Base Class Overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.definedNames; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            // We don't need to do anything here because this element does not have any attributes
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			WorkbookReferenceBase workbookReference = manager.ContextStack[typeof(ExternalWorkbookReference)] as WorkbookReferenceBase;
			if (workbookReference == null)
				workbookReference = manager.Workbook.CurrentWorkbookReference;

			if (workbookReference.NamedReferences.Count == 0)
            {
                Utilities.DebugFail("We should not be saving the DefinedNamesElement with no named references");
                return;
            }

			List<NamedReferenceBase> lstNamedReferences = new List<NamedReferenceBase>();
			foreach (NamedReferenceBase refBase in workbookReference.NamedReferences)
			{
				// MD 10/22/10
				// Found while fixing TFS36696
				// If a formula references a named reference which doesn't exist, we will add it to the NamedReferences collection of the workbook reference, 
				// but it will have no formula, so skip these references.
				if (refBase.FormulaInternal.PostfixTokenList.Count == 0)
					continue;

				// Create a new 'definedName' element
				XmlElementBase.AddElement(element, DefinedNameElement.QualifiedName);

				// Add the reference to our list
				lstNamedReferences.Add(refBase);
			}

			// Push the list context onto the stack so that the child elements can serialize
			manager.ContextStack.Push(new ListContext<NamedReferenceBase>(lstNamedReferences));
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