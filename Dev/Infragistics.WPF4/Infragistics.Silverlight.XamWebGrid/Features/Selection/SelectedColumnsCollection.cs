using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of selected <see cref="Column"/> objects.
	/// </summary>
	public class SelectedColumnsCollection : SelectedCollectionBase<Column>, IProvideCustomPersistence
	{
		#region OnSelectionChanged

		/// <summary>
		/// Called when the Selection collection has changed. 
		/// </summary>
		protected override void OnSelectionChanged(SelectedCollectionBase<Column> oldCollection, SelectedCollectionBase<Column> newCollection)
		{
			if (this.Grid != null)
				this.Grid.OnSelectedColumnsCollectionChanged((SelectedColumnsCollection)oldCollection, (SelectedColumnsCollection)newCollection);
		}

		#endregion // OnSelectionChanged

		#region CreateNewInstance

		/// <summary>
		/// Creates a new instance of this collection.
		/// </summary>
		protected override SelectedCollectionBase<Column> CreateNewInstance()
		{
			return new SelectedColumnsCollection();
		}

		#endregion // CreateNewInstance

        #region AddItem
        /// <summary>
        /// Adds the Column at the specified index. 
        /// </summary>
        /// <param propertyName="index"></param>
        /// <param propertyName="item"></param>
        protected override void AddItem(int index, Column item)
        {
            if (item.SupportsActivationAndSelection)
            {
                base.AddItem(index, item);
            }
        }
        #endregion // AddItem

		#region IProvideCustomPersistence Members

		string IProvideCustomPersistence.Save()
		{
		return	this.Save();
		}

		void IProvideCustomPersistence.Load(object owner, string value)
		{
			this.Load(owner, value);
		}

		#endregion

		#region Save

		/// <summary>
		/// Gets the string representation of the object, that can be later be passed into the Load method of this object, in order to rehydrate.
		/// </summary>
		/// <returns></returns>
		protected virtual string Save()
		{
			string val = "";

			foreach (Column col in this)
			{
				val += col.Key + ":" + col.ColumnLayout.Key + ",";
			}
			return val;
		}

		#endregion // Save
		
		#region Load

		/// <summary>
		/// Takes the string that was created in the Save method, and rehydrates the object. 
		/// </summary>
		/// <param name="owner">This is the object who owns this object as a property.</param>
		/// <param name="value"></param>
		protected virtual void Load(object owner, string value)
		{
			if (value != null)
			{
				SelectionSettings settings = owner as SelectionSettings;

				if (settings != null && settings.Grid != null)
				{
                    this.Clear();

					string[] cols = ((string)value).Split(',');

					foreach (string col in cols)
					{
						if (col.Length > 0)
						{
							string[] keyPair = col.Split(':');

							if (keyPair.Length == 2)
							{
								ColumnLayout layout = this.Grid.ColumnLayouts[keyPair[1]];

								if (layout != null)
								{
									Column column = layout.Columns.DataColumns[keyPair[0]];
									if (column != null)
									{
                                        this.Add(column);
									}
								}
							}
						}
					}

				}
			}
		}

		#endregion // Load
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