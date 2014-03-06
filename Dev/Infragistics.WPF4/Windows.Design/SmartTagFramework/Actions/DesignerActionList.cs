using System;
using System.ComponentModel;
using Infragistics.Windows.Design.SmartTagFramework;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using System.Collections.Generic;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Provides the base class for types that represent a list of items included in a smart tag.
    /// </summary>
    public abstract class DesignerActionList
    {
		#region Member Variables

        private DesignerActionItemCollection	_items;
        private ModelItem						_modelItem;
        private EditingContext					_context;
        private Type							_currentType;

		#endregion //Member Variables	
    
		#region Constructors

        /// <summary>
        /// Initializes a new instance of the DesignerActionList class. 
        /// </summary>
        /// <param name="context">EditingContext of the control</param>
        /// <param name="modelItem">ModelItem of the control</param>
		/// <param name="itemsToShow">A list of string identifiers that derived classes can look at to determine which items to include in the list.</param>
		/// <param name="alternateAdornerTitle">Specifies an alternate Title for the GenericAdorner that hosts this DesignerActionList, overriding the one specified in the DesignerActionList itself.</param>        
		public DesignerActionList(EditingContext context, ModelItem modelItem, List<string> itemsToShow, string alternateAdornerTitle)
        {
            if (modelItem == null)
                throw new ApplicationException("ModelItem is null");

            _modelItem		= modelItem;
            _currentType	= modelItem.ItemType;
            _context		= context;
        }

		#endregion //Constructors	
    
		#region Properties

			#region Public Properties

				#region ContextMenuMethodItems

        /// <summary>
        /// Returns method items which are included in the context menu
        /// </summary>
        public DesignerActionItemCollection ContextMenuMethodItems
        {
            get
            {
                DesignerActionItemCollection methodItems = new DesignerActionItemCollection();
                foreach (DesignerActionItem item in _items)
                {
                    if (item is DesignerActionMethodItem)
                    {
                        DesignerActionMethodItem designerActionMethodItem = (DesignerActionMethodItem)item;
                        if (designerActionMethodItem.IncludeAsDesignerVerb)
                        {
                            methodItems.Add(designerActionMethodItem);

                            //checks if the DesignerActionMethodItem with IncludeAsDesignerVerb=true has specified ContextMenuGroup
                            if (null == designerActionMethodItem.ContextMenuGroup)
                            {
                                throw new ApplicationException(string.Format("The DesignerActionMethodItem - \"{0}\" doesn't have ContextMenuGroup specified!", designerActionMethodItem.DisplayName));
                            }
                        }
                    }
                }

                //sort the collection
                methodItems.SortByOrderNumber();

                return methodItems;
            }
        }

				#endregion //ContextMenuMethodItems

				#region GenericAdornerTitle

        /// <summary>
        /// The smart tag's title
        /// </summary>
        public string GenericAdornerTitle
        {
            get;
            set;
        }

				#endregion //GenericAdornerTitle

				#region GenericAdornerMaxHeight

        /// <summary>
        /// The smart tag's MaxHeight
        /// </summary>
        public double GenericAdornerMaxHeight
        {
            get;
            set;
        }

				#endregion //GenericAdornerMaxHeight

				#region GenericAdornerMinHeight

        /// <summary>
        /// The smart tag's MinHeight
        /// </summary>
        public double GenericAdornerMinHeight
        {
            get;
            set;
        }

				#endregion //GenericAdornerMinHeight

				#region GenericAdornerMinWidth

        /// <summary>
        /// The smart tag's MinWidth
        /// </summary>
        public double GenericAdornerMinWidth
        {
            get;
            set;
        }

				#endregion //GenericAdornerMinWidth

				#region GenericAdornerMaxWidth

        /// <summary>
        /// The smart tag's MaxWidth
        /// </summary>
        public double GenericAdornerMaxWidth
        {
            get;
            set;
        }

				#endregion //GenericAdornerMaxWidth

				#region Items

        /// <summary>
        /// Returns all items.
        /// </summary>
        public DesignerActionItemCollection Items
        {
            get
            {
				if (this._items == null)
					this._items = new DesignerActionItemCollection();

                //set the initial values
                foreach (DesignerActionItem item in _items)
                {
                    if (item is DesignerActionPropertyItem)
                    {
                        DesignerActionPropertyItem propertyItem 
													= item as DesignerActionPropertyItem;

                        PropertyDescriptor prpDesc	= Utils.PropertyHelper(_currentType, propertyItem.Name);
                        propertyItem.PropertyType	= prpDesc.PropertyType;
                        propertyItem.Context		= _context;
                        propertyItem.Value			= _modelItem.Properties[propertyItem.Name].ComputedValue;
                    }
                }

                return _items;
            }
        }

				#endregion //Items

				#region MethodItems

        /// <summary>
        /// Returns only the method items
        /// </summary>
        public DesignerActionItemCollection MethodItems
        {
            get
            {
                DesignerActionItemCollection methodItems = new DesignerActionItemCollection();
                foreach (DesignerActionItem item in _items)
                {
                    if (item is DesignerActionMethodItem)
                    {
                        DesignerActionMethodItem methodItem = item as DesignerActionMethodItem;
                        methodItem.ModelItem = this._modelItem;
                        methodItem.Context = this._context;

                        methodItems.Add(methodItem);
                    }
                }

                //sort the collection
                methodItems.SortByOrderNumber();

                return methodItems;
            }
        }

				#endregion //MethodItems

				#region RootContextMenuGroup

        /// <summary>
        /// A MenuGroup object which is the root element of the context menu hierarchy
        /// </summary>
        public MenuGroup RootContextMenuGroup
        {
            get;
            set;
        }

				#endregion //RootContextMenuGroup        

			#endregion //Public Properties

		#endregion //Properties	    
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