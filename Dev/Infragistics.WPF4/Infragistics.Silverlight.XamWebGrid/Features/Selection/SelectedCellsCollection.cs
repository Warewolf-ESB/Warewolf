
namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of selected <see cref="Cell"/> objects.
	/// </summary>
	public class SelectedCellsCollection : SelectedCollectionBase<Cell>
	{
		#region OnSelectionChanged

		/// <summary>
		/// Called when the Selection collection has changed. 
		/// </summary>
		protected override void OnSelectionChanged(SelectedCollectionBase<Cell> oldCollection, SelectedCollectionBase<Cell> newCollection)
		{
			if (this.Grid != null)
				this.Grid.OnSelectedCellsCollectionChanged((SelectedCellsCollection)oldCollection, (SelectedCellsCollection)newCollection);
		}

		#endregion // OnSelectionChanged

		#region CreateNewInstance

		/// <summary>
		/// Creates a new instance of this collection.
		/// </summary>
		protected override SelectedCollectionBase<Cell> CreateNewInstance()
		{
			return new SelectedCellsCollection();
		}

		#endregion // CreateNewInstance

        #region AddItem
        /// <summary>
        /// Adds the Cell at the specified index. 
        /// </summary>
        /// <param propertyName="index"></param>
        /// <param propertyName="item"></param>
        protected override void AddItem(int index, Cell item)
        {
            if (item.Column.SupportsActivationAndSelection)
            {
                base.AddItem(index, item);
            }
        }
        #endregion // AddItem
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