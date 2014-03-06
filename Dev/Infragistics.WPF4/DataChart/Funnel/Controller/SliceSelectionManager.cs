using System.Collections;
using Infragistics.Controls.Charts.Messaging;
using Infragistics.Controls.Charts.Util;


using System.Collections.Generic;


namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Manages the selection of slices.
    /// </summary>
    internal class SliceSelectionManager
    {



        Dictionary<int,object> _selected = new Dictionary<int,object>();


        /// <summary>
        /// Toggles the selection of a slice at a given index.
        /// </summary>
        /// <param name="index">The index of the slice whose selection should be toggled.</param>
        /// <param name="item">The item at the specified index.</param>
        public void ToggleSelection(int index, object item)
        {
            if (_selected.ContainsKey(GetKey(index)))
            {
                _selected.Remove(GetKey(index));
            }
            else
            {
                _selected[GetKey(index)] = item;
            }
        }

        /// <summary>
        /// Determines if the slice at a given index is selected.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>Whether the slice at the specified index is selected.</returns>
        public bool IsSelected(int index)
        {
            return _selected.ContainsKey(GetKey(index));
        }

        /// <summary>
        /// Returns true if any of the slices are selected.
        /// </summary>
        /// <returns>True, if any of the slices are selected.</returns>
        public bool Any()
        {
            return _selected.Count > 0;
        }

        /// <summary>
        /// Determines if the slice at a given index is unselected.
        /// </summary>
        /// <param name="index">The index to use.</param>
        /// <returns>Whether the slice at the specified index is selected.</returns>
        public bool IsUnselected(int index)
        {
            return !IsSelected(index) && Any();
        }

        /// <summary>
        /// Should be called to notify the manager that the data indexing has changed.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="data"></param>
        public void DataUpdated(DataUpdatedMessage m, DoubleColumn data)
        {
            switch (m.Action)
            {
                case ItemsSourceAction.Change:
                    break;
                case ItemsSourceAction.Insert:
                    ShiftUp(m.Position, m.Count);
                    break;
                case ItemsSourceAction.Remove:
                    Remove(m.Position, m.Count);
                    break;
                case ItemsSourceAction.Replace:
                    for (int i = 0; i < m.Count; i++)
                    {
                        if (_selected.ContainsKey(GetKey(i)))
                        {
                            _selected[GetKey(i)] = data.Values[i];
                        }
                    }
                    break;
                case ItemsSourceAction.Reset:
                    ClearSelection();
                    break;
            }
        }

        private void Remove(int startingPosition, int count)
        {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            List<int> toShift = new List<int>();
            List<object> toShiftObjects = new List<object>();
            foreach (KeyValuePair<int,object> entry in _selected)
            {
                int val = entry.Key;

                if (val >= startingPosition)
                {
                    toShift.Add(val);
                    toShiftObjects.Add(_selected[GetKey(val)]);
                }
            }

            int i = 0;
            foreach (int value in toShift)
            {
                _selected.Remove(GetKey(value));
                if (value - count > startingPosition)
                {
                    _selected[GetKey(value - count)] = toShiftObjects[i];
                }
                i++;
            }
        }

        private void ShiftUp(int startingPosition, int count)
        {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            List<int> toShift = new List<int>();
            List<object> toShiftObjects = new List<object>();
            foreach (KeyValuePair<int,object> entry in _selected)
            {
                int val = entry.Key;

                if (val >= startingPosition)
                {
                    toShift.Add(val);
                    toShiftObjects.Add(_selected[GetKey(val)]);
                }
            }

            int i = 0;
            foreach (int value in toShift)
            {
                _selected.Remove(GetKey(value));
                _selected[GetKey(value + count)] = toShiftObjects[i];
                i++;
            }
        }

        public void ClearSelection()
        {
            _selected.Clear();
        }







        private int GetKey(int index)
        {
            return index;
        }


        internal int[] GetSelectedItems()
        {
            int[] items = new int[_selected.Count];
            int i = 0;


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            foreach (KeyValuePair<int, object> entry in _selected)
            {
                items[i] = entry.Key;
                i++;
            }

            return items;
        }

        internal void SetSelectedItems(int[] indexes, DoubleColumn values)
        {
            ClearSelection();
            foreach (int index in indexes)
            {
                ToggleSelection(index, values.Values[index]);
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