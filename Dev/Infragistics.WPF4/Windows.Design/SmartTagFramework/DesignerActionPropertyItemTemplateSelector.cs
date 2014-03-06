using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Reflection;
using System.Windows.Data;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// Select a DesignerActionPropertyItem template
    /// </summary>
    class DesignerActionPropertyItemTemplateSelector : DataTemplateSelector
    {
        #region Base class overrides

        #region SelectTemplate

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DesignerActionItem designerActionItem = item as DesignerActionItem;
            if (designerActionItem == null)
            {
                throw new ArgumentException("item must be of type DesignerActionItem");
            }

            FrameworkElement element = container as FrameworkElement;
            if (element == null)
            {
                throw new ArgumentException("Null element");
            }

            if (typeof(DesignerActionPropertyItem) != designerActionItem.GetType())
            {
                throw new ArgumentException("This is not DesignerActionPropertyItem");
            }

            DataTemplate template = FindDataTemplate(element, designerActionItem);

            return template;
        }

        #endregion //SelectTemplate

        #endregion //Base class overrides

        #region Methods

        #region Private Methods

        #region FindDataTemplate

		private DataTemplate FindDataTemplate(FrameworkElement element, DesignerActionItem designerActionItem)
        {
            DesignerActionPropertyItem property = designerActionItem as DesignerActionPropertyItem;
            Type propertyType = property.PropertyType;

            DataTemplate template = TryFindDataTemplate(element, property.Name);

            if (template == null)
            {
                template = TryFindDataTemplate(element, propertyType);
            }

            while (template == null && propertyType.BaseType != null)
            {
                propertyType = propertyType.BaseType;
                template = TryFindDataTemplate(element, propertyType);
            }

			// If the property is of type string and the property name is Theme, use the template for the ThemeEditor.
			if (property.Name == "Theme" && propertyType == typeof(string))
				template = TryFindDataTemplate(element, new ComponentResourceKey(typeof(GenericAdorner), "ThemeEditorTemplate")); 


			TypeConverter		converter			= TypeDescriptor.GetConverter(property.PropertyType);
			NullableConverter	nullableConverter	= converter as NullableConverter;

			// Treat object types as strings as long as it is not Nullable.
			if (template			== null				&& 
				propertyType		== typeof(object)	&&
				nullableConverter	== null)
			{
				template = TryFindDataTemplate(element, typeof(string));
			}

            if (template == null)
            {
                if (template == null && converter.CanConvertFrom(typeof(string)))
                {
					if (nullableConverter != null)
					{
						if (nullableConverter.UnderlyingType == typeof(Boolean))
							template = TryFindDataTemplate(element, nullableConverter.UnderlyingType);
						else
						if (nullableConverter.UnderlyingType.BaseType == typeof(Enum))
						{
							template = new DataTemplate();

							FrameworkElementFactory fefComboBox	= new FrameworkElementFactory(typeof(ComboBox));
							Binding					binding		= new Binding();
							binding.Path						= new PropertyPath("Value");
							binding.Mode						= BindingMode.TwoWay;
							binding.Converter					= new NullableEnumItemTypeConverter();
							fefComboBox.SetValue(ComboBox.SelectedValueProperty, binding);
							fefComboBox.SetValue(ComboBox.HorizontalAlignmentProperty, HorizontalAlignment.Right);
							fefComboBox.SetValue(ComboBox.MinWidthProperty, (double)100.0);

							binding								= new Binding();
							binding.Path						= new PropertyPath("Value");
							binding.Mode						= BindingMode.OneTime;
							binding.UpdateSourceTrigger			= UpdateSourceTrigger.PropertyChanged;
							binding.Converter					= new NullableEnumTypeConverter();
							binding.ConverterParameter			= nullableConverter.UnderlyingType;
							fefComboBox.SetValue(ComboBox.ItemsSourceProperty, binding);

							template.VisualTree = fefComboBox;
						}
						else
						if (nullableConverter.UnderlyingType == typeof(Int32)	||
							nullableConverter.UnderlyingType == typeof(int)		||
							nullableConverter.UnderlyingType == typeof(double)	||
							nullableConverter.UnderlyingType == typeof(float))
						{
							template = new DataTemplate();

							FrameworkElementFactory fefTextBox	= new FrameworkElementFactory(typeof(TextBox));
							Binding					binding		= new Binding();
							binding.Path						= new PropertyPath("IsReadOnly");
							fefTextBox.SetValue(TextBox.IsReadOnlyProperty, binding);
							fefTextBox.SetValue(TextBox.MinWidthProperty, (double)75.0);
							fefTextBox.SetValue(TextBox.TextAlignmentProperty, TextAlignment.Right);

							binding								= new Binding();
							binding.Path						= new PropertyPath("Value");
							binding.Mode						= BindingMode.TwoWay;
							binding.UpdateSourceTrigger			= UpdateSourceTrigger.LostFocus;
							binding.Converter					= new GenericValueConverter();
							binding.ConverterParameter			= property.PropertyType;
							binding.ValidationRules.Add(new ExceptionValidationRule());
							fefTextBox.SetValue(TextBox.TextProperty, binding);

							template.VisualTree = fefTextBox;
						}
					}

					if (template == null)
					{
						template = new DataTemplate();

						FrameworkElementFactory fefTextBox	= new FrameworkElementFactory(typeof(TextBox));
						Binding					binding		= new Binding();
						binding.Path						= new PropertyPath("IsReadOnly");
						fefTextBox.SetValue(TextBox.IsReadOnlyProperty, binding);
						fefTextBox.SetValue(TextBox.StyleProperty, element.TryFindResource(new ComponentResourceKey(typeof(GenericAdorner), "ErrorToolTipStyle")));
						fefTextBox.SetValue(TextBox.MinWidthProperty, (double)150.0);

						binding								= new Binding();
						binding.Path						= new PropertyPath("Value");
						binding.Mode						= BindingMode.TwoWay;
						binding.UpdateSourceTrigger			= UpdateSourceTrigger.LostFocus;
						binding.Converter					= new GenericValueConverter();
						binding.ConverterParameter			= property.PropertyType;
						binding.ValidationRules.Add(new ExceptionValidationRule());
						fefTextBox.SetValue(TextBox.TextProperty, binding);

						template.VisualTree = fefTextBox;
					}
				}
            }

            if (template == null)
                template = TryFindDataTemplate(element, "NoDefault");

            return template;
        }

        #endregion //FindDataTemplate

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