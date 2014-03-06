using System.Collections;

namespace Infragistics.Windows.Design.SmartTagFramework
{  
    /// <summary>
    /// Represents a collection of DesignerActionItem objects. 
    /// </summary>
    public class DesignerActionItemCollection : CollectionBase
    {
        #region Methods

        #region Public Methods

        #region Add

        /// <summary>
        /// Adds the supplied DesignerActionItem to the current collection. 
        /// </summary>
        /// <param name="value">The DesignerActionItem to add. </param>
        /// <returns>The DesignerActionItemCollection index at which the value has been added.</returns>
        public int Add(DesignerActionItem value)
        {
            return base.List.Add(value);
        }

        #endregion //Add	
    
        #region Contains

        /// <summary>
        /// Determines whether the DesignerActionItemCollection contains a specific element. 
        /// </summary>
        /// <param name="value">The DesignerActionItem to locate in the DesignerActionItemCollection.</param>
        /// <returns>true if the DesignerActionItemCollection contains the specified value; otherwise, false.</returns>
        public bool Contains(DesignerActionItem value)
        {
            return base.List.Contains(value);
        }

        #endregion //Contains	
    
        #region CopyTo

        /// <summary>
        /// Copies the elements of the current collection into the supplied array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional DesignerActionItem array that is the destination of the elements copied from the current collection. The array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(DesignerActionItem[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        #endregion //CopyTo	
    
        #region IndexOf

        /// <summary>
        /// Determines the index of a specific item in the collection.
        /// </summary>
        /// <param name="value">The DesignerActionItem to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire DesignerActionItemCollection, if found; otherwise, -1.</returns>
        public int IndexOf(DesignerActionItem value)
        {
            return base.List.IndexOf(value);
        }

        #endregion //IndexOf	
    
        #region Insert

        /// <summary>
        /// Inserts an element into the DesignerActionItemCollection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The DesignerActionItem to insert.</param>
        public void Insert(int index, DesignerActionItem value)
        {
            base.List.Insert(index, value);
        }

        #endregion //Insert	
    
        #region Remove

        /// <summary>
        /// Removes the first occurrence of a specific object from the DesignerActionItemCollection. 
        /// </summary>
        /// <param name="value">The DesignerActionItem to remove from the DesignerActionItemCollection. </param>
        public void Remove(DesignerActionItem value)
        {
            base.List.Remove(value);
        }

        #endregion //Remove	

        #region SortByOrderNumber

		/// <summary>
		/// Sorts the collection by the order number of each item.
		/// </summary>
        public void SortByOrderNumber()
        {
            System.Collections.IComparer sorter = new NameSortHelper();
            InnerList.Sort(sorter);
        }

        #endregion //SortByOrderNumber	
    
        #endregion //Public Methods

        #endregion //Methods	
    
        #region Indexers

        /// <param name="index">The zero based index of the element.</param>       
        public DesignerActionItem this[int index]
        {
            get
            {
                return (DesignerActionItem)base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        #endregion //Indexers

        #region System.Collections.IComparer

        private class NameSortHelper : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                DesignerActionItem item1 = (DesignerActionItem)x;
                DesignerActionItem item2 = (DesignerActionItem)y;

                return item1.OrderNumber.CompareTo(item2.OrderNumber);
            }
        }

        #endregion //System.Collections.IComparer
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