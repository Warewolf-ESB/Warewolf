using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Infragistics.Collections
{
	#region SingleItemEnumerator

	/// <summary>
	/// Implements an enumerator over a single item.
	/// </summary>
	public class SingleItemEnumerator : IEnumerator
	{
		private object _currentItem;
		private object _item;
		private bool _endReached;

		static object UnsetObjectMarker = new object();

		/// <summary>
		/// Initializes a new instance of <see cref="SingleItemEnumerator"/>
		/// </summary>
		/// <param name="item">The single object to be enumerated.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> If item is null the enumerator will behave is if it has no items, i.e. an empty enumerator.</para>
		/// </remarks>
		public SingleItemEnumerator(object item)
		{
			this._item = item;
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
					if (this._endReached)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_4"));
					else
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_3"));
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
			if (this._endReached)
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_4"));

			if (this._item == null ||
				this._currentItem == this._item)
			{
				this._endReached = true;
				return false;
			}

			this._currentItem = this._item;

			return true;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public void Reset()
		{
			this._currentItem = UnsetObjectMarker;
			this._endReached = false;
		}

		#endregion
	}

	#endregion //SingleItemEnumerator

	#region EmptyEnumerator

	/// <summary>
	/// Implements an enumerator with no items.
	/// </summary>
	public sealed class EmptyEnumerator : IEnumerator
	{
		/// <summary>
		/// Returns a singleton instance of an <see cref="EmptyEnumerator"/>
		/// </summary>
		public static readonly EmptyEnumerator Instance = new EmptyEnumerator();

		private EmptyEnumerator()
		{
		}

		#region IEnumerator Members

		object IEnumerator.Current
		{
			get { throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_4")); }
		}

		bool IEnumerator.MoveNext()
		{
			return false;
		}

		void IEnumerator.Reset()
		{
		}

		#endregion
	}

	#endregion //EmptyEnumerator	
    
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