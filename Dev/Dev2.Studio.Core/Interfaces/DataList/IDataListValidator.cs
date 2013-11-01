using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core.Interfaces.DataList
{
    public interface IDataListValidator
    {        
        /// <summary>
        /// Adds the specified item to add.
        /// </summary>
        /// <param name="itemToAdd">The item to add.</param>
        void Add(Dev2.Studio.Core.Interfaces.DataList.IDataListItemModel itemToAdd);

        /// <summary>
        /// Moves the specified item to move.
        /// </summary>
        /// <param name="itemToMove">The item to move.</param>
        void Move(IDataListItemModel itemToMove);

        /// <summary>
        /// Removes the specified item to remove.
        /// </summary>
        /// <param name="itemToRemove">The item to remove.</param>
        void Remove(IDataListItemModel itemToRemove);
        
        void ValidateRecordSetChildren(IDataListItemModel parent);
        void ValidateChildren(IDataListItemModel itemToAdd);
    }
}
