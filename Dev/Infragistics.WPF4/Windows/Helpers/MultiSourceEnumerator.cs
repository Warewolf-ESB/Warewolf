using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Infragistics.Collections
{
	/// <summary>
	/// Concatenates the items from multiple enumerators in a single enumerator. 
	/// </summary>
	public class MultiSourceEnumerator : IEnumerator
	{
		private IEnumerator[] _sourceEnumerators;
		private int _currentEnumeratorIndex;
		private object _currentItem;
		
		static object UnsetObjectMarker = new object();

		/// <summary>
		/// Initializes a new instance of <see cref="MultiSourceEnumerator"/>
		/// </summary>
		/// <param name="sourceEnumerators">The source enumerators whose items will be logically concatenated by this enumerator.</param>
		/// <exception cref="ArgumentNullException">If sourceEnumerators is null.</exception>
		/// <exception cref="ArgumentException">If sourceEnumerators length is null or the IEnumerator array has any null entries.</exception>
		public MultiSourceEnumerator(params IEnumerator[] sourceEnumerators)
		{
			if (sourceEnumerators == null)
				throw new ArgumentNullException("sourceEnumerators");
			
			if (sourceEnumerators.Length == 0)
				throw new ArgumentException("sourceEnumerators", SR.GetString("LE_ArgumentException_20"));

			for (int i = 0; i < sourceEnumerators.Length; i++)
			{
				if (sourceEnumerators[i] == null)
					throw new ArgumentException("sourceEnumerators", SR.GetString("LE_ArgumentException_20"));
			}

			this._sourceEnumerators = sourceEnumerators;
			this._currentEnumeratorIndex = -1;
			this._currentItem = UnsetObjectMarker;
		}

		#region IEnumerator Members

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
		public object Current
		{
			get
			{
				if (this._currentItem == UnsetObjectMarker)
				{
					if (this._currentEnumeratorIndex == -1)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_3"));
					else
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_4"));
				}

				return this._currentItem;
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public bool MoveNext()
		{
			if (this._currentEnumeratorIndex < 0)
				this._currentEnumeratorIndex = 0;

			if (!this._sourceEnumerators[this._currentEnumeratorIndex].MoveNext())
			{
				// since the curent enumator returned false see if we have any more enumerators
				if (this._currentEnumeratorIndex < this._sourceEnumerators.Length - 1)
				{
					// since we have a least one more enumerator bump the index
					this._currentEnumeratorIndex++;

					// call this method recursively and return its result
					return ((IEnumerator)this).MoveNext();
				}

				// we have passed the end of all enumerators so set the current item to unset
				// and return false
				this._currentItem = UnsetObjectMarker;

				return false;
			}

			this._currentItem = this._sourceEnumerators[this._currentEnumeratorIndex].Current;

			return true;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public void Reset()
		{
			this._currentEnumeratorIndex = -1;
			this._currentItem = UnsetObjectMarker;

			// reset all of the source enumerators
			for (int i = 0; i < this._sourceEnumerators.Length; i++)
				this._sourceEnumerators[i].Reset();
		}

		#endregion
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