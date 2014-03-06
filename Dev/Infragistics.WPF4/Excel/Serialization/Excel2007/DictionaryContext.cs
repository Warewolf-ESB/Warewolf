using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	// MD 5/13/11 - Data Validations / Page Breaks


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal class DictionaryContext<TKey, TValue>
	{
		private IDictionary<TKey, TValue> dicitonary;
		private IEnumerator<KeyValuePair<TKey, TValue>> enumerator;






		public DictionaryContext(IDictionary<TKey, TValue> dicitonary)
		{
			this.dicitonary = dicitonary;
		}






		internal void AddItem(TKey key, TValue value)
		{
			this.dicitonary.Add(key, value);
		}






		internal KeyValuePair<TKey, TValue> ConsumeCurrentItem()
		{
			if (this.enumerator == null)
				this.enumerator = this.dicitonary.GetEnumerator();

			if (this.enumerator.MoveNext() == false)
			{
				this.enumerator.Dispose();
				this.enumerator = null;
				return default(KeyValuePair<TKey, TValue>);
			}

			return this.enumerator.Current;
		}

		internal int Count
		{
			get { return this.dicitonary.Count; }
		}

		internal KeyValuePair<TKey, TValue> CurrentItem
		{
			get
			{
				if (this.enumerator == null)
					return default(KeyValuePair<TKey, TValue>);

				return this.enumerator.Current;
			}
		}

		internal object GetItem(TKey key)
		{
			return this.dicitonary[key];
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