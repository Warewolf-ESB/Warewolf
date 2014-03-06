using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{

	#region FilterCellCollection Class

	/// <summary>
	/// A collection of <see cref="FilterCell"/> objects exposed off a <see cref="FilterRecord"/> via its 'Cells' property.
	/// </summary>
	/// <remarks>
	/// <b>FilterCellCollection</b> class contains <see cref="FilterCell"/> objects that belong to a <see cref="FilterRecord"/>.
	/// You can get this collection using the 'Cells' property of the <i>FilterRecord</i>.
	/// </remarks>
	/// <seealso cref="FilterRecord"/>
	/// <seealso cref="FilterCell"/>
	/// <seealso cref="DataRecord.Cells"/>
	public class FilterCellCollection : CellCollection
	{
		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterCellCollection"/>.
		/// </summary>
		/// <param name="record">Associated FilterRecord object.</param>
		internal FilterCellCollection( FilterRecord record )
			: base( record )
        {
        }

        #endregion // Constructor

		#region Properties

		    #region Public Properties

                #region Indexers

        /// <summary>
        /// The <see cref="FilterCell"/> at the specified zero-based index (read-only)
        /// </summary>
        public new FilterCell this[int index]
        {
            get
            {
                return base[index] as FilterCell;
            }
        }

        /// <summary>
        /// The <see cref="FilterCell"/> associated with the specified <see cref="Field"/> (read-only)
        /// </summary>
        public new FilterCell this[Field field]
        {
            get
            {
                return base[field] as FilterCell;
            }
        }

        /// <summary>
        /// The <see cref="FilterCell"/> associated with the specified <see cref="Field"/> (read-only)
        /// </summary>
        public new FilterCell this[string fieldName]
        {
            get
            {
                return base[fieldName] as FilterCell;
            }
        }

                #endregion //Indexers	

		        #region Record

		/// <summary>
		/// Returns the associated <see cref="FilterRecord"/>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Record</b> property returns the <see cref="FilterRecord"/> associated with this
		/// collection.
		/// </para>
		/// </remarks>
		/// <seealso cref="FilterRecord"/>
		/// <seealso cref="DataRecord.Cells"/>
		public new FilterRecord Record
		{
			get
			{
				return (FilterRecord)base.Record;
			}
		}

		        #endregion // Record

		    #endregion // Public Properties

		#endregion // Properties

		#region Base Overrides

		#region CreateCell

		/// <summary>
		/// Overridden. Creates a new <see cref="FilterCell"/>.
		/// </summary>
		/// <param name="field">Field for which to create new cell.</param>
		/// <returns>New Cell for the specified field.</returns>
		internal override Cell CreateCell( Field field )
		{
			return new FilterCell( this.Record, field );
		}

		#endregion // CreateCell

		#endregion // Base Overrides
	}

	#endregion // FilterCellCollection Class

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