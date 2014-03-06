using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{

    /// <summary>
    /// Collection of <see cref="Resource"/> objects.
    /// </summary>
    /// <seealso cref="XamScheduleDataManager.ResourceItems"/>
	public class ResourceCollection : ViewList<Resource>
	{
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="source">Data source list.</param>
        /// <param name="itemFactory">Item factory used for creating new Resource instances as well as getting/setting their data items.</param>
		/// <param name="preverifyCallback">Called before the view list is verified.</param>
        internal ResourceCollection( IEnumerable source, IViewItemFactory<Resource> itemFactory, Func<ViewList<Resource>, IEnumerable> preverifyCallback )
			: base( source, itemFactory, preverifyCallback: preverifyCallback )
        {
        }

		/// <summary>
		/// Constructor. Creates a new instance of <see cref="ResourceCollection"/>.
		/// </summary>
		/// <param name="source">Source collection of resources. Any modifications made to the source
		/// will be reflected by this collection.</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the <i>ResourceCollection</i> is a read-only collection. Any modifications made to 
		/// the source collection are reflected by this collection, assuming the source collection implements 
		/// INotifyCollectionChanged interface.
		/// </para>
		/// </remarks>
		public ResourceCollection( IEnumerable<Resource> source )
			: base( source )
		{
		}

        #endregion // Constructor

        #region GetResourceFromId

        /// <summary>
        /// Gets the resource with the specified id. Returns null if none exists.
        /// </summary>
        /// <param name="id">Resource id.</param>
        /// <returns>Matching Resource object or null if no matching resource is found.</returns>
        public Resource GetResourceFromId( string id )
        {
            
            return this.FirstOrDefault( ii => ii.Id == id );
        }

        #endregion // GetResourceFromId
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