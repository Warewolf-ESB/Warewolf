using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// A class providing various options for controlling the exporting of a <see cref="Infragistics.Windows.DataPresenter.Record"/>
    /// through the <see cref="IDataPresenterExporter.ProcessRecord"/> method.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class ProcessRecordParams
    {
        #region Private Members

        bool _skipDescendants;
        bool _skipSiblings;
        bool _terminateExport;

        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ProcessRecordParams()
        {            
        }
        #endregion //Constructor

        #region Public Properties

		#region SkipDescendants

		/// <summary>
        /// Specifies whether to skip the child records of the current record.
        /// </summary>
        public bool SkipDescendants
        {
            get
            {
                return this._skipDescendants;
            }
            set
            {
                this._skipDescendants = value;
            }
        }

        #endregion // SkipDescendants

        #region SkipSiblings

        /// <summary>
        /// Specifies whether to skip sibling records of the current record.
        /// </summary>
        public bool SkipSiblings
        {
            get
            {
                return this._skipSiblings;
            }
            set
            {
                this._skipSiblings = value;
            }
        }

        #endregion // SkipSiblings

		#region TerminateExport

		/// <summary>
        /// Specifies whether to terminate the export process. The current record will not be processed.
        /// </summary>
        public bool TerminateExport
        {
            get
            {
                return this._terminateExport;
            }
            set
            {
                this._terminateExport = value;
            }
        }

        #endregion // TerminateExport

        #endregion //Public Properties

		#region Methods

		// AS 3/3/11 NA 2011.1 - Async Exporting
		#region Reset
		internal void Reset()
		{
			_skipDescendants = false;
			_skipSiblings = false;
			_terminateExport = false;
		}
		#endregion //Reset 

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