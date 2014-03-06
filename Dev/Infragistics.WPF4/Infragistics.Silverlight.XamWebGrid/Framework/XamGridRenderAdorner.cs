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
using System.Collections.Generic;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// An object that can be used to add additional elements to the RowsPanel of the XamGrid.
    /// </summary>
    public abstract class XamGridRenderAdorner
    {
        #region Members

        XamGrid _grid;

        Dictionary<string, Dictionary<object, UIElement>> _elementsLookupTable = new Dictionary<string, Dictionary<object, UIElement>>();

        List<object> _usedObjects = new List<object>();
        Dictionary<object, List<string>> _usedKeys = new Dictionary<object, List<string>>();

        List<object> _previouslyUsedRows;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializs a new instance of the <see cref="XamGridRenderAdorner"/>
        /// </summary>
        /// <param name="grid"></param>
        protected XamGridRenderAdorner(XamGrid grid)
        {
            this._grid = grid;
        }

        #endregion // Constructor

        #region Methods

        #region Public

        #region RegisterRowsPanel

        /// <summary>
        /// Registers the <see cref="RowsPanel"/> that this <see cref="XamGridRenderAdorner"/> will insert elements into.
        /// </summary>
        /// <param name="panel"></param>
        public void RegisterRowsPanel(RowsPanel panel)
        {
            this.RowsPanel = panel;
        }

        #endregion // RegisterRowsPanel

        #region UnregisterRowsPanel

        /// <summary>
        /// Unregisters the <see cref="RowsPanel"/> that this <see cref="XamGridRenderAdorner"/> inserted elements into.
        /// </summary>
        public void UnregisterRowsPanel()
        {
            this.Reset();

            this.RowsPanel = null;
        }

        #endregion // UnregisterRowsPanel

        #region Reset

        /// <summary>
        /// Releases all elements that this specific <see cref="XamGridRenderAdorner"/> has added to the <see cref="RowsPanel"/>
        /// </summary>
        public virtual void Reset()
        {
            foreach (KeyValuePair<string, Dictionary<object, UIElement>> strLookup in this._elementsLookupTable)
            {
                foreach (KeyValuePair<object, UIElement> objLookup in strLookup.Value)
                {
                    this.UnregisterElement(objLookup.Value);
                }

                strLookup.Value.Clear();
            }

            this._elementsLookupTable.Clear();

            this._usedObjects.Clear();
            this._usedKeys.Clear();
        }
        #endregion // Reset

        #region Measure

        /// <summary>
        /// Performs a measure for all elements that should be added to the <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="availableSize"></param>
        public void Measure(Size availableSize)
        {
            // Actually measure the elements
            this.MeasureAdorners(availableSize);

            // Determine what was used before, but isn't used anymore, and release them.
            foreach (object row in this._previouslyUsedRows)
            {
                if (!this._usedObjects.Contains(row))
                    this.UnregisterObject(row);
                else
                {
                    List<string> usedKeys = this._usedKeys[row];
                    foreach (KeyValuePair<string, Dictionary<object, UIElement>> kvp in this._elementsLookupTable)
                    {
                        if (!usedKeys.Contains(kvp.Key))
                            this.UnregisterElement(row, kvp.Key);
                    }
                }
            }

            this._previouslyUsedRows = null;
        }

        #endregion // Measure        

        #region Initialize

        /// <summary>
        /// Allows the <see cref="XamGridRenderAdorner"/> to prepare itself for a new Layout phase
        /// </summary>
        public virtual void Initialize()
        {
            // Reset what was used. 
            this._previouslyUsedRows = new List<object>(this._usedObjects);
            this._usedObjects.Clear();
            this._usedKeys.Clear();
        }
        #endregion // Initialize

        #endregion // Public

        #region Abstract

        #region MeasureAfterRow

        /// <summary>
        /// If additional content is going to added after the row, this is where its measured
        /// </summary>
        /// <param name="row"></param>
        /// <returns>The additional height that will be appended to the row. </returns>
        public abstract double MeasureAfterRow(RowBase row);

        #endregion // MeasureAfterRow

        #region MeasureAdorners

        /// <summary>
        /// When overriden on a dervied class it measures all elements that will be displayed in the <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns>The total height of all additional elements that will be rendered inline.</returns>
        protected abstract void MeasureAdorners(Size availableSize);

        #endregion // MeasureAdorners

        #region ArrangeAfterRow
        /// <summary>
        /// For each row, additional elements can be arranged after.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="top"></param>
        /// <param name="finalSize"></param>
        /// <returns>The total height of the elements after the row.</returns>
        public abstract double ArrangeAfterRow(RowBase row, double top, Size finalSize);

        #endregion // ArrangeAfterRow

        #region ArrangeAdornments

        /// <summary>
        /// Occurs after all rows have been arranged, it allows the object to render additional elements on top of the <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="finalSize"></param>
        /// <param name="dataRowTop"></param>
        public abstract void ArrangeAdornments(Size finalSize, double dataRowTop);

        #endregion // ArrangeAdornments

        #endregion Abstract

        #region Protected

        #region RegisterObjectUsed

        /// <summary>
        /// Registers a particualr object as being used. 
        /// </summary>
        /// <param name="obj"></param>
        protected void RegisterObjectUsed(object obj)
        {
            if (!this._usedObjects.Contains(obj))
            {
                this._usedObjects.Add(obj);
                this._usedKeys.Add(obj, new List<string>());
            }
        }

        #endregion // RegisterObjectUsed

        #region UnregisterObject

        /// <summary>
        /// Unregisters the specific object, and removes its element from the <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="obj"></param>
        protected void UnregisterObject(object obj)
        {
            foreach (KeyValuePair<string, Dictionary<object, UIElement>> kvp in this._elementsLookupTable)
            {
                this.UnregisterElement(obj, kvp.Key);
            }
        }

        #endregion // UnregisterObject

        #region UnregisterElement

        /// <summary>
        /// Given the the obj and key to look up this object by, the UIElement that it represents its removed from <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        protected void UnregisterElement(object obj, string key)
        {
            Dictionary<object, UIElement> lookup = this._elementsLookupTable[key];
            if (lookup != null)
            {
                if (lookup.ContainsKey(obj))
                {
                    this.UnregisterElement(lookup[obj]);
                    
                    lookup.Remove(obj);
                }
            }
        }

        /// <summary>
        /// Removes the specified <see cref="UIElement"/> fromt he <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="element"></param>
        protected void UnregisterElement(UIElement element)
        {
            this.RowsPanel.Children.Remove(element);
        }

        #endregion // UnregisterElement

        #region GetElement

        /// <summary>
        /// If an element actually exists for the specified keys it is returned, otherwise null is returned. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected UIElement GetElement(object obj, string key)
        {
            this.RegisterObjectUsed(obj);
            
            if (!this._usedKeys[obj].Contains(key))
                this._usedKeys[obj].Add(key);

            UIElement element = null;

            if (this._elementsLookupTable.ContainsKey(key))
            {
                Dictionary<object, UIElement> lookup = this._elementsLookupTable[key];

                if (lookup.ContainsKey(obj))
                    element = lookup[obj];
            }

            return element;
        }

        #endregion // GetElement

        #region AddElement

        /// <summary>
        /// If GetElement returned null, then AddElement should be used to register that element with <see cref="RowsPanel"/>
        /// </summary>
        /// <param name="row"></param>
        /// <param name="key"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        protected UIElement AddElement(object row, string key, UIElement element)
        {
            this.RegisterObjectUsed(row);

            if (!this._usedKeys[row].Contains(key))
                this._usedKeys[row].Add(key);

            if (!this._elementsLookupTable.ContainsKey(key))
                this._elementsLookupTable.Add(key, new Dictionary<object, UIElement>());

            Dictionary<object, UIElement> lookup = this._elementsLookupTable[key];

            if (lookup.ContainsKey(row))
            {
                UIElement elem = lookup[row];
                if (elem != null)
                    this.RowsPanel.Children.Remove(elem);
                lookup.Remove(row);
            }

            lookup.Add(row, element);

            if(!this.RowsPanel.Children.Contains(element))
                this.RowsPanel.Children.Add(element);

            return element;
        }

        #endregion //AddElement

        #endregion // Protected

        #endregion // Methods

        #region Properties

        #region Protected

        #region RowsPanel

        /// <summary>
        /// The <see cref="RowsPanel"/> that this object will append to.
        /// </summary>
        protected RowsPanel RowsPanel
        {
            get;
            private set;
        }

        #endregion // RowsPanel
      
        #endregion // Protected

        #region Public

        #region XamGrid

        /// <summary>
        /// The <see cref="XamGrid"/> that this <see cref="XamGridRenderAdorner"/> belongs to. 
        /// </summary>
        public XamGrid XamGrid
        {
            get { return this._grid; }
        }

        #endregion // XamGrid

        #endregion // Public

        #endregion // Properties
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