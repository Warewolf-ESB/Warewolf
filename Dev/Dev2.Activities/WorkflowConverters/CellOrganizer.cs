using Dev2.Common.X6;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.WorkflowConverters
{
    public static class CellOrganizer
    {
        /// <summary>
        /// Organizes cells into a dictionary of parent -> children.
        /// </summary>
        /// <param name="cells">The input list of cells.</param>
        /// <returns>A dictionary where the key is the parent cell and the value is its list of children.</returns>
        public static Dictionary<Cell, List<Cell>> BuildHierarchy(IReadOnlyCollection<Cell> cells)
        {
            if (cells == null || cells.Count == 0)
                return new Dictionary<Cell, List<Cell>>();

            // Index cells by id for parent lookup
            var cellById = cells
                .Where(c => !string.IsNullOrWhiteSpace(c.id))
                .ToDictionary(c => c.id, c => c, StringComparer.OrdinalIgnoreCase);

            var hierarchy = new Dictionary<Cell, List<Cell>>();

            foreach (var cell in cells)
            {
                if (!cell.data.TryGetBool(Constants.ISNESTED, out var isNested) || !isNested)
                    continue;

                if (!cell.data.TryGetString(Constants.PARENTID, out var parentId) || string.IsNullOrWhiteSpace(parentId))
                    continue;

                if (!cellById.TryGetValue(parentId, out var parentCell))
                    continue; // skip if parent not found

                if (!hierarchy.TryGetValue(parentCell, out var children))
                {
                    children = new List<Cell>();
                    hierarchy[parentCell] = children;
                }

                children.Add(cell);
            }

            return hierarchy;
        }
    }
}
