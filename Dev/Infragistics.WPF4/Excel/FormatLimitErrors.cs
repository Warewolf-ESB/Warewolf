using System;
using System.Collections.Generic;
using System.Text;




namespace Infragistics.Documents.Excel

{
	internal class FormatLimitErrors
    {
        #region Private Members

        private bool hasErrors;
		private List<NamedReference> namedReferencesWithInvalidReferences;
        private List<string> errors;

        #endregion //Private Members

        #region Constructor

        public FormatLimitErrors() { }

        #endregion //Constructor

        #region Methods

        #region AddError

        public void AddError(string message)
		{
			this.hasErrors = true;
            this.Errors.Add(message);            
        }
        #endregion //AddError

        #endregion //Methods

        #region Properties

        #region Errors

        public List<string> Errors
        {
            get
            {
                if (this.errors == null)
                    this.errors = new List<string>();

                return this.errors;
            }
        }
        #endregion //Errors

        #region HasErrors

        public bool HasErrors
		{
			get { return this.hasErrors; }
		} 

		#endregion HasErrors

		#region NamedReferencesWithInvalidReferences

		public List<NamedReference> NamedReferencesWithInvalidReferences
		{
			get
			{
				if ( this.namedReferencesWithInvalidReferences == null )
					this.namedReferencesWithInvalidReferences = new List<NamedReference>();

				return this.namedReferencesWithInvalidReferences;
			}
		} 

		#endregion NamedReferencesWithInvalidReferences

        #endregion //Properties
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