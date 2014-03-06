using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents an observable collection of Chart objects.
    /// </summary>
    /// <remarks>
    /// It is not possible to clear the contents of a ChartCollection collection; the Chart
    /// objects must be removed one at a time.
    /// <para>
    /// Changing the name of a Chart will generate a collection changed (Replace) event.
    /// </para>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]



    public class ChartCollection : ObservableCollection<SeriesViewer>

    {
        /// <summary>
        /// Initializes a default, empty data ChartCollection.
        /// </summary>
        public ChartCollection()
        {
        }

        /// <summary>
        /// Occurs just before the current chart collection contents are reset
        /// </summary>
        public event EventHandler<EventArgs> CollectionResetting;



#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Clears the contained items of the collection, but provides a preview of the occurrance 
        /// in the form of the CollectionResetting event.
        /// </summary>
        protected override void ClearItems()
        {
            if (CollectionResetting != null)
            {
                CollectionResetting(this, null);
            }

            base.ClearItems();
        }

        /// <summary>
        /// Gets the first matching chart object by name.
        /// </summary>
        /// <param name="chartName"></param>
        /// <returns></returns>
        public SeriesViewer this[string chartName]
        {
            get
            {


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

                foreach (SeriesViewer chart in this)
                {
                    if (chartName == chart.Name)
                    {
                        return chart;
                    }
                }


                return null;
            }
        }
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