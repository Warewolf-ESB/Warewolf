using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Collections.Generic;

namespace Infragistics
{
	internal class DesignTimeHelper
	{
		#region Member Variables

		private static string[] _inheritedUIPropsToAllow = 
		{
			"Tag",
			"DataContext",
			"Style"
		};

		private static string[] _uiControlTypes = new string[] {
				"System.Windows.Controls.Control",
				"System.Windows.FrameworkElement",
				"System.Windows.UIElement",
			};

		private static Attribute[] _hidePropertyAttributes = new Attribute[] {
			BrowsableAttribute.No,
			CategoryAttribute.Default
		};

		[ThreadStatic]
		private static Dictionary<Type, Tuple<int, NewItemTypesAttribute>> _newItemTypesAttributeTable;

		#endregion //Member Variables

		#region Public Methods

		#region CreateNewItemTypesAttribute
		/// <summary>
		/// Returns a <see cref="NewItemTypesAttribute"/> containing all the concrete creatable types that derive/implement the specified type.
		/// </summary>
		/// <param name="baseType">The base class or interface that the types must implement.</param>
		/// <returns>A NewItemTypesAttribute with the types for the specified base type/interface or null if there are no creatable types.</returns>
		public static NewItemTypesAttribute CreateNewItemTypesAttribute(Type baseType)
		{
			if (_newItemTypesAttributeTable == null)
				_newItemTypesAttributeTable = new Dictionary<Type, Tuple<int, NewItemTypesAttribute>>();

			int loadedAssemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;
			Tuple<int, NewItemTypesAttribute> tableValue;

			if (!_newItemTypesAttributeTable.TryGetValue(baseType, out tableValue) ||
				tableValue.Item1 != loadedAssemblyCount)
			{
				var types = GetCreatableTypes(baseType);
				types.Sort(TypeComparison);
				NewItemTypesAttribute attrib = types.Count == 0 ? null : new NewItemTypesAttribute(types.ToArray()) { FactoryType = typeof(ActivatorTypeFactory) };
				tableValue = new Tuple<int, NewItemTypesAttribute>(loadedAssemblyCount, attrib);
				_newItemTypesAttributeTable[baseType] = tableValue;
			}

			return tableValue.Item2;
		} 
		#endregion //CreateNewItemTypesAttribute

		#region GetCreatableTypes
		private static List<Type> GetCreatableTypes(Type baseType)
		{
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			var creatableTypes = new List<Type>();

			// Store the currently loaded assemblies so we can process them when requested.
			//
			foreach (var assembly in assemblies)
			{
				// AssemblyBuilder derived assemblies throw an exception
				// when you try to get the exported types so skip them.
				//
				if (assembly.GetType() == typeof(System.Reflection.Emit.AssemblyBuilder))
					continue;

				// Calling GetCustomAttributes on a reflection only loaded assembly
				// will generate an exception so we'll just ignore those assemblies.
				//
				if (assembly.ReflectionOnly)
					continue;

				Action<Type, IList<Type>, IList<Type>> callback = delegate(Type typeToCheck, IList<Type> availableTypes, IList<Type> listToUpdate)
				{
					if (null != availableTypes)
					{
						foreach (Type type in availableTypes)
						{
							if (type != null && type.IsPublic && !type.IsAbstract && !type.IsPrimitive)
							{
								if (baseType.IsAssignableFrom(type))
								{
									ConstructorInfo info = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

									if (info != null)
										listToUpdate.Add(type);
								}
							}
						}
					}
				};

				try
				{
					Type[] availableTypes = assembly.GetTypes();
					callback(baseType, availableTypes, creatableTypes);
				}
				catch (ReflectionTypeLoadException ex)
				{
					callback(baseType, ex.Types, creatableTypes);
				}
				catch (Exception)
				{
				}
			}

			return creatableTypes;
		} 
		#endregion //GetCreatableTypes

		#region HideInheritedUIProperties
		/// <summary>
		/// Hides all Control, FrameworkElement and UIElement properties for the CallbackType of the specified builder
		/// </summary>
		/// <param name="builder">The builder associated with the CallbackType whose properties should be hidden</param>
		public static void HideInheritedUIProperties(AttributeCallbackBuilder builder)
		{
			if (builder == null)
				throw new ArgumentNullException("builder");

			HideInheritedUIProperties(builder.CallbackType, delegate(string propertyName)
			{
				HideProperty(builder, propertyName);
			});
		}

		/// <summary>
		/// Hides all Control, FrameworkElement and UIElement properties for the CallbackType of the specified builder
		/// </summary>
		/// <param name="builder">The builder associated with the CallbackType whose properties should be hidden</param>
		/// <param name="type">The type for which the properties are to be hidden</param>
		public static void HideInheritedUIProperties(AttributeTableBuilder builder, Type type)
		{
			if (builder == null)
				throw new ArgumentNullException("builder");

			HideInheritedUIProperties(type, delegate(string propertyName)
			{
				HideProperty(builder, type, propertyName);
			});
		}

		private static void HideInheritedUIProperties(Type type, Action<string> hidePropertyMethod)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (hidePropertyMethod == null)
				throw new ArgumentNullException("hidePropertyMethod");

			Type baseType = type.BaseType;

			while (baseType != null && !_uiControlTypes.Contains(baseType.FullName))
				baseType = baseType.BaseType;

			if (null != baseType)
			{
				foreach (var prop in baseType.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public))
				{
					if (!_inheritedUIPropsToAllow.Contains(prop.Name))
						hidePropertyMethod(prop.Name);
				}
			}
		} 
		#endregion //HideInheritedUIProperties

		#region HideProperty
		/// <summary>
		/// Hides the specified property from the design time property grid.
		/// </summary>
		/// <param name="builder">The builder associated with the CallbackType whose properties should be hidden</param>
		/// <param name="propertyName">The name of the property to hide</param>
		public static void HideProperty(AttributeCallbackBuilder builder, string propertyName)
		{
			// note i'm including the CategoryAttribute because it seems that properties 
			// registered to other categories (e.g. background, opacity, opacitymask, isenabled, etc.)
			// were still showing up even with the BrowseableAttribute.No.
			//
			builder.AddCustomAttributes(propertyName, _hidePropertyAttributes);
		}

		/// <summary>
		/// Hides the specified property from the design time property grid.
		/// </summary>
		/// <param name="builder">The builder associated with the CallbackType whose properties should be hidden</param>
		/// <param name="type">The type whose property is to be hidden</param>
		/// <param name="propertyName">The name of the property to hide</param>
		public static void HideProperty(AttributeTableBuilder builder, Type type, string propertyName)
		{
			// note i'm including the CategoryAttribute because it seems that properties 
			// registered to other categories (e.g. background, opacity, opacitymask, isenabled, etc.)
			// were still showing up even with the BrowseableAttribute.No.
			//
			builder.AddCustomAttributes(type, propertyName, _hidePropertyAttributes);
		}
		#endregion //HideProperty

		#endregion //Public Methods

		#region Private Methods

		#region TypeComparison
		private static int TypeComparison(Type x, Type y)
		{
			if (x == y)
				return 0;

			string strX = x != null ? x.Name : string.Empty;
			string strY = y != null ? y.Name : string.Empty;

			return string.Compare(strX, strY, false);
		}
		#endregion //TypeComparison

		#endregion //Private Methods

		private class ActivatorTypeFactory : NewItemFactory
		{
			public override object CreateInstance(Type type)
			{
				return Activator.CreateInstance(type);
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