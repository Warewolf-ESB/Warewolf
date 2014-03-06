using System;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Windows.Ribbon
{
	internal class KeyTipCollection : ICollection<KeyTip>
	{
		#region Member Variables

		private KeyTipManager owningManager;

		private List<KeyTip> keyTipList = new List<KeyTip>();

		private KeyTipComparer keyTipComparer = new KeyTipComparer();

		#endregion Member Variables

		#region Constructor

		public KeyTipCollection(KeyTipManager owningManager)
		{
			this.owningManager = owningManager;
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region Add

		public int Add(KeyTip keyTip)
		{
			int retValue = this.Count;
			this.keyTipList.Add(keyTip);
			return retValue;
		}

		#endregion Add

		#region BinarySearch

		#endregion BinarySearch

		#region Commented out

		#endregion Commented out

		#region Clone

		public KeyTipCollection Clone()
		{
			KeyTipCollection clonedCollection = new KeyTipCollection(this.owningManager);

			clonedCollection.keyTipList = new List<KeyTip>(this.keyTipList);

			return clonedCollection;
		}

		#endregion Clone

		#region RemoveAt

		public void RemoveAt(int i)
		{
			this.keyTipList.RemoveAt(i);
		}

		#endregion RemoveAt

		#region Search

		internal KeyTip Search(IKeyTipProvider provider)
		{
			foreach (KeyTip keyTip in this)
			{
				if (keyTip.Provider.Equals(provider))
					return keyTip;
			}

			return null;
		}

		#endregion Search

		#region Sort

		public void Sort()
		{
			// MD 8/17/07 - 7.3 Performance
			// Use generics
			//Utilities.SortMerge( this.keyTipList, this.keyTipComparer );
			Utilities.SortMergeGeneric(this.keyTipList, this.keyTipComparer);
		}

		#endregion Sort

		#endregion Public Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region this[ int ]

		public KeyTip this[int index]
		{
			get { return (KeyTip)this.keyTipList[index]; }
		}

		#endregion this[ int ]

		#endregion Public Properties

		#endregion Properties

		#region ICollection Members

		public int Count
		{
			get { return this.keyTipList.Count; }
		}

		#endregion

		#region IEnumerable Members

		public System.Collections.IEnumerator GetEnumerator()
		{
			return this.keyTipList.GetEnumerator();
		}

		#endregion

		#region ICollection<KeyTip> Members

		void ICollection<KeyTip>.Add(KeyTip item)
		{
			this.Add(item);
		}

		void ICollection<KeyTip>.Clear()
		{
			this.keyTipList.Clear();
		}

		bool ICollection<KeyTip>.Contains(KeyTip item)
		{
			return this.keyTipList.Contains(item);
		}

		void ICollection<KeyTip>.CopyTo(KeyTip[] array, int arrayIndex)
		{
			this.keyTipList.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyTip>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyTip>.Remove(KeyTip item)
		{
			return this.keyTipList.Remove(item);
		}

		#endregion

		#region IEnumerable<KeyTip> Members

		IEnumerator<KeyTip> IEnumerable<KeyTip>.GetEnumerator()
		{
			return this.keyTipList.GetEnumerator();
		}

		#endregion

		#region Comparer Classes

		#region KeyTipComparer class

		private class KeyTipComparer : IComparer<KeyTip>
		{
			#region IComparer Members

			int IComparer<KeyTip>.Compare(KeyTip keyTip1, KeyTip keyTip2)
			{
				if (keyTip1 == null && keyTip2 == null)
					return 0;

				if (keyTip2 == null)
					return 1;

				if (keyTip1 == null)
					return -1;

				return String.Compare(keyTip1.Value, keyTip2.Value);
			}

			#endregion
		}

		#endregion KeyTipComparer class

		#region KeyTipOrValueComparer class

		#endregion KeyTipOrValueComparer class

		#endregion Comparer Classes
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