using System;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Select a DesignerActionItem template
    /// </summary>
    class DesignerActionItemTemplateSelector : DataTemplateSelector
    {
        #region Base Class Overrides

        #region SelectTemplate

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DesignerActionItem designerActionItem = item as DesignerActionItem;
            if (designerActionItem == null)
            {
                throw new ArgumentException("An item must be of type DesignerActionItem");
            }

            FrameworkElement element = container as FrameworkElement;
            if (element == null)
            {
                throw new ArgumentException("Null element");
            }

            DataTemplate template = null;
            if (typeof(DesignerActionPropertyItem) == designerActionItem.GetType())
            {
                template = TryFindDataTemplate(element, "DesignerActionPropertyItemTemplate");
            }
			else if (typeof(DesignerActionObjectPropertyItem) == designerActionItem.GetType()  ||
					 designerActionItem.GetType().IsSubclassOf(typeof(DesignerActionObjectPropertyItem)))
			{
				template = TryFindDataTemplate(element, "DesignerActionObjectPropertyItemTemplate");
			}
			else if (typeof(DesignerActionTextItem) == designerActionItem.GetType())
            {
                template = TryFindDataTemplate(element, "DesignerActionTextItemTemplate");
            }
            else if (typeof(DesignerActionMethodItem) == designerActionItem.GetType())
            {
                template = TryFindDataTemplate(element, "DesignerActionMethodItemTemplate");
            }
			else
            {
                template = null;
            }

            return template;
        }

        #endregion //SelectTemplate

        #endregion //Base Class Overrides

        #region Methods

        #region Private Methods

        #region TryFindDataTemplate

        private DataTemplate TryFindDataTemplate(FrameworkElement element, object dataTemplateKey)
        {
            object dataTemplate = element.TryFindResource(dataTemplateKey);
            if (dataTemplate == null)
            {
                dataTemplateKey = new ComponentResourceKey(typeof(GenericAdorner), dataTemplateKey);
                dataTemplate = element.TryFindResource(dataTemplateKey);
            }
            return dataTemplate as DataTemplate;
        }

        #endregion //TryFindDataTemplate

        #endregion //Private Methods

        #endregion //Methods
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