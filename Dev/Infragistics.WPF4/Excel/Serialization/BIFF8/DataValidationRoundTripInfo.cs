using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.FormulaUtilities;

namespace Infragistics.Documents.Excel.Serialization
{
	// MD 2/1/11 - Data Validation support
	// These are no longer needed for round tripping data because data validations are now fully supported.
	//// MD 9/12/08 - TFS6887
	//#region DataValidationRoundTripInfo
	//
	//internal class DataValidationRoundTripInfo
	//{
	//    //private List<DataValidationCriteria> dataValidations;
	//    private ushort wDviFlags;
	//    private int xLeft;
	//    private int yLeft;
	//
	//    public DataValidationRoundTripInfo() { }
	//
	//    //public List<DataValidationCriteria> DataValidations
	//    //{
	//    //    get
	//    //    {
	//    //        if ( this.dataValidations == null )
	//    //            this.dataValidations = new List<DataValidationCriteria>();
	//
	//    //        return this.dataValidations;
	//    //    }
	//    //}
	//
	//    public ushort WDviFlags
	//    {
	//        get { return this.wDviFlags; }
	//        set { this.wDviFlags = value; }
	//    }
	//
	//    public int XLeft
	//    {
	//        get { return this.xLeft; }
	//        set { this.xLeft = value; }
	//    }
	//
	//    public int YLeft
	//    {
	//        get { return this.yLeft; }
	//        set { this.yLeft = value; }
	//    }
	//} 
	//
	//#endregion DataValidationRoundTripInfo

	//#region DataValidationCriteria class
	//
	//internal class DataValidationCriteria
	//{
	//    private uint dwDvFlags;
	//    private string errorMessage;
	//    private string errorTitle;
	//    private Formula formulaForFirstCondition;
	//    private Formula formulaForSecondCondition;
	//    private string promptBoxMessage;
	//    private string promptBoxTitle;
	//    private List<CellAddressRange> ranges;
	//
	//    public uint DwDvFlags
	//    {
	//        get { return this.dwDvFlags; }
	//        set { this.dwDvFlags = value; }
	//    }
	//
	//    public string ErrorMessage
	//    {
	//        get { return this.errorMessage; }
	//        set { this.errorMessage = value; }
	//    }
	//
	//    public string ErrorTitle
	//    {
	//        get { return this.errorTitle; }
	//        set { this.errorTitle = value; }
	//    }
	//
	//    public Formula FormulaForFirstCondition
	//    {
	//        get { return this.formulaForFirstCondition; }
	//        set { this.formulaForFirstCondition = value; }
	//    }
	//
	//    public Formula FormulaForSecondCondition
	//    {
	//        get { return this.formulaForSecondCondition; }
	//        set { this.formulaForSecondCondition = value; }
	//    }
	//
	//    public string PromptBoxMessage
	//    {
	//        get { return this.promptBoxMessage; }
	//        set { this.promptBoxMessage = value; }
	//    }
	//
	//    public string PromptBoxTitle
	//    {
	//        get { return this.promptBoxTitle; }
	//        set { this.promptBoxTitle = value; }
	//    }
	//
	//    public List<CellAddressRange> Ranges
	//    {
	//        get { return this.ranges; }
	//        set { this.ranges = value; }
	//    }
	//} 
	//
	//#endregion DataValidationCriteria class
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