using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Represents an observable collection of Axis objects.
    /// </summary>
    /// <remarks>
    /// It is not possible to clear the contents of a AxisCollection collection; the Axis
    /// objects must be removed one at a time.
    /// </remarks>
    
    public class AxisCollection : ObservableCollection<Axis>
    {
        /// <summary>
        /// Initializes a default, empty AxisCollection.
        /// </summary>
        public AxisCollection()
        {
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == "Name")
                {
                    for (int i = 0; i < Count; ++i)
                    {
                        if (this[i].Name == e.PropertyName)
                        {
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, this[i], this[i], i));
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Occurs just before the current axis collection contents are reset
        /// </summary>
        public event EventHandler<EventArgs> CollectionResetting;

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
        /// Gets the first matching Axis object by name.
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public Axis this[string axisName]
        {
            get
            {
                foreach (Axis chartAxis in this)
                {
                    if (axisName == chartAxis.Name)
                    {
                        return chartAxis;
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