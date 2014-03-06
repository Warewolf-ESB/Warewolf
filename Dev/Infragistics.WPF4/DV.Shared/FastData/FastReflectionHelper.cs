using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// A reflection strategy that uses either traditional reflection or compiled lambda expressions
    /// to get property values from an object.
    /// </summary>
    public class FastReflectionHelper
    {
        /// <summary>
        /// Constructs the fast reflection helper.
        /// </summary>
        public FastReflectionHelper()
        {
            this.GetValueMethod = FastReflectionHelper.GetPropertyMode.CompiledLinqExpression;
            Invalid = true;
        }

        private FastReflectionHelper innerHelper = null;

        /// <summary>
        /// Constructs the fast reflection helper.
        /// </summary>
        /// <param name="useTraditionalReflection">Should the helper use traditional (slower) reflection.</param>
        /// <param name="propertyName">The propertyname this will be reflecting on.</param>
        public FastReflectionHelper(bool useTraditionalReflection, string propertyName)
        {
            if (useTraditionalReflection)
            {
                this.GetValueMethod = FastReflectionHelper.GetPropertyMode.TraditionalReflection;
            }
            else
            {
                this.GetValueMethod = FastReflectionHelper.GetPropertyMode.CompiledLinqExpression;
            }
            UpdatePropertyName(propertyName);
        }

        private void UpdatePropertyName(string propertyName)
        {
            if (propertyName == null)
            {
                return;
            }

            // strip special characters
            propertyName = propertyName.TrimStart('.', '[');
            
            int indexOfSpecialChar = propertyName.IndexOfAny(new char[] { '.', '[' });
            if (indexOfSpecialChar > 0)
            {
                string rest = propertyName.Substring(indexOfSpecialChar, propertyName.Length - indexOfSpecialChar);
                innerHelper = new FastReflectionHelper(UseTraditionalReflection, rest);
                this.UpdatePropertyName(propertyName.Substring(0, indexOfSpecialChar));
                return;
            }


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

            if (propertyName.EndsWith("]"))
            {
                propertyName = propertyName.TrimEnd(']');
                int unused;
                if (int.TryParse(propertyName, out unused))
                {
                    this.GetValueMode = FastReflectionHelper.GetValueOperation.IntIndexer;
                }
                else
                {
                    this.GetValueMode = FastReflectionHelper.GetValueOperation.StringIndexer;
                }
            }
            else
            {
                this.GetValueMode = FastReflectionHelper.GetValueOperation.GetProperty;
            }
            this.propertyName = propertyName;
            
        }

        private enum GetValueOperation
        {
            GetProperty,
            StringIndexer,
            IntIndexer
        }
        private GetValueOperation GetValueMode  { get; set; }
        /// <summary>
        /// Gets or sets the property name of the current FastReflectionHelper object.
        /// </summary>
        public string PropertyName
        {
            get { return propertyName; }
            set
            {
                if (propertyName != value)
                {
                    UpdatePropertyName(value);
                    Invalid = string.IsNullOrEmpty(PropertyName);

                    type = null;
                    getPropertyValueDictionary = new Dictionary<Type, Func<object, object>>();
                }
            }
        }
        private string propertyName = null;

        internal enum GetPropertyMode
        {
            CompiledLinqExpression,
            TraditionalReflection,

            TypeDescriptor

        }
        private GetPropertyMode GetValueMethod
        { 




              get; 
              set; 

        }
        /// <summary>
        /// Indicates that current FastReflectionHelper object is not using compiled expressions.
        /// </summary>
        public bool UseTraditionalReflection
        {
            get
            {
                return this.GetValueMethod == FastReflectionHelper.GetPropertyMode.TraditionalReflection;
            }
            set
            {
                if (value)
                {
                    this.GetValueMethod = FastReflectionHelper.GetPropertyMode.TraditionalReflection;
                }
            }
        }

        /// <summary>
        /// Returns true if the fast reflection helper is invalid, probably due to a lack of a property name.
        /// </summary>
        public bool Invalid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the property value from the specified item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Property value or null if the property value cannot be determined.</returns>
        public object GetPropertyValue(object item)
        {
            if (Invalid || item == null)
            {
                return null;
            }

            return GetPropertyValue(item.GetType(), item);
        }

        /// <summary>
        /// Gets the property value for the specified item.
        /// </summary>
        /// <param name="itemType">The item type.</param>
        /// <param name="item">The item containing the property.</param>
        /// <returns>Property value or null if the property value cannot be determined.</returns>
        public object GetPropertyValue(Type itemType, object item)
        {
            if (Invalid || itemType == null || item == null)
            {
                return null;
            }

            object retVal = null;
            switch (this.GetValueMethod)
            {
                case GetPropertyMode.TraditionalReflection:
                    retVal = this.GetPropertyValueTraditional(itemType, item);
                    break;

                case GetPropertyMode.TypeDescriptor:
                    retVal = this.GetPropertyValueTypeDescriptor(itemType, item);
                    break;

                default:
                case GetPropertyMode.CompiledLinqExpression:
                    try
                    {
                        retVal = GetValueFast(itemType, item);
                    }
                    catch (ArgumentException)
                    {
                        // [DN 3/11/2010] failsafe ... adding this to support an alternate runtime environment which seems to have a bug in its System.Reflection.Emit.
                        this.type = null; // this needs to be set back to null to reset the process

                        if (typeof(ICustomTypeDescriptor).IsAssignableFrom(itemType))
                        {
                            this.GetValueMethod = FastReflectionHelper.GetPropertyMode.TypeDescriptor;
                        }
                        else

                        {
                            this.GetValueMethod = FastReflectionHelper.GetPropertyMode.TraditionalReflection;
                        }
                        return GetPropertyValue(itemType, item);
                    }
                    break;
            }            

            if (innerHelper != null && retVal != null)
            {
                retVal = innerHelper.GetPropertyValue(retVal);
            }

            return retVal;
        }

        // cache used by fast and traditional reflection
        private Type type = null;

        #region fast reflection and associated caches


        private LambdaExpression GetIndexingFunction(Type itemType, object item, Type indexerType, Type delegateType)
        {
            PropertyInfo propertyInfo = type.GetProperty("Item", null, new Type[] { indexerType });
            // create obj and string parameters for the Func<obj, string, obj>;  ...in case you were wondering, that third obj is the TResult.
            ParameterExpression objParameter = Expression.Parameter(typeof(object), "obj");
            ParameterExpression indexParameter = Expression.Parameter(indexerType, indexerType.Name);

            // the Expression is going to be something like (obj as Indexable).Item[indexerType];
            // so let's get the conversion expression: (obj as Indexable)
            UnaryExpression conversion = this.GetConversion(type, objParameter);
            // add the indexing expression: Item[indexerType]
            Expression indexer = Expression.MakeIndex(conversion, propertyInfo, new Expression[] { indexParameter });

            // if propertyInfo.propertyType is a value type (like Point), a cast is necessary so that the lambda expression can fit the delegate type
            indexer = this.GetConversion(typeof(object), indexer);

            // lambda it up.  the result can be compiled to a Func
            return Expression.Lambda(delegateType, indexer, objParameter, indexParameter);
        }

        private object GetIntIndexerValue(Type itemType, object item)
        {
            if (itemType != type)
            {
                this.type = itemType;
                this.getIntIndexValue = null;
                if (!this.getIntIndexValueDictionary.TryGetValue(type, out this.getIntIndexValue))
                {
                    if (this.getIntIndexValueDictionary.Count > 32)
                    {
                        this.getIntIndexValueDictionary.Clear();  // don't want any runaways
                    }

                    try
                    {
                        this.getIntIndexValue = (Func<object, int, object>)this.GetIndexingFunction(itemType, item, typeof(int), typeof(Func<object, int, object>)).Compile();
                    }
                    catch (MissingMethodException)
                    {
                        // this happens at design time.
                        return null;
                    }

                    this.getIntIndexValueDictionary.Add(type, getIntIndexValue);

                }
            }
            if (this.getIntIndexValue != null)
            {
                return this.getIntIndexValue(item, int.Parse(this.PropertyName));
            }
            return null;
        }
        private object GetStringIndexerValue(Type itemType, object item)
        {
            if (itemType != type)
            {
                this.type = itemType;
                this.getStringIndexValue = null;
                if (!this.getStringIndexValueDictionary.TryGetValue(type, out this.getStringIndexValue))
                {
                    if (this.getStringIndexValueDictionary.Count > 32)
                    {
                        this.getStringIndexValueDictionary.Clear();  // don't want any runaways
                    }

                    try
                    {
                        this.getStringIndexValue = (Func<object, string, object>)this.GetIndexingFunction(itemType, item, typeof(string), typeof(Func<object, string, object>)).Compile();
                    }
                    catch (MissingMethodException)
                    {
                        // this happens at design time
                        return null;
                    }

                    this.getStringIndexValueDictionary.Add(type, getStringIndexValue);

                }
            }
            if (this.getStringIndexValue != null)
            {
                return this.getStringIndexValue(item, this.PropertyName);
            }
            return null;
        }
        private object GetPropertyValueFast(Type itemType, object item)
        {
            if (itemType != type)
            {
                type = itemType;
                getPropertyValue = null;

                if (!getPropertyValueDictionary.TryGetValue(type, out getPropertyValue))
                {
                    if (getPropertyValueDictionary.Count > 32)
                    {
                        getPropertyValueDictionary.Clear();  // don't want any runaways
                    }

                    PropertyInfo propertyInfo = type.GetProperty(propertyName);

                    if (propertyInfo != null)
                    {
                        ParameterExpression param = Expression.Parameter(typeof(object), "obj");
                        UnaryExpression conversion = this.GetConversion(type, param);

                        UnaryExpression prop = Expression.TypeAs(Expression.Property(conversion, propertyInfo), typeof(object));

                        try
                        {
                            getPropertyValue = (Func<object, object>)Expression.Lambda(prop, param).Compile();
                        }
                        catch (MissingMethodException)
                        {
                            // [DN Dec 13 2011 : 96856] exception found at design time only in Silverlight 5.  throw an argumentexception so it gets handled up the stack and reverts to the failsafe traditional reflection.
                            throw new ArgumentException();
                        }

                    }
                    else if (this.PropertyName != null && this.PropertyName.Length > 0)
                    {
                        // no PropertyInfo was found, but the PropertyName has been set.  Either the PropertyName is invalid, or we need to use TypeDescriptor to find the property.
                        throw new ArgumentException();
                    }
                    getPropertyValueDictionary.Add(type, getPropertyValue);
                }
            }
            if (getPropertyValue != null)
            {
                return getPropertyValue(item);
            }

            if ((this.PropertyName == null || this.PropertyName.Length == 0) && itemType.IsAssignableFrom(item.GetType())) // [DN 3/2/2010:28715] like if you're binding to a doublecollection and your valuemapping is ""
            {
                return item;
            }
            return null;
        }
        private object GetValueFast(Type itemType, object item)
        {
            switch (this.GetValueMode)
            {
                case GetValueOperation.IntIndexer:
                    return this.GetIntIndexerValue(itemType, item);
                case GetValueOperation.StringIndexer:
                    return this.GetStringIndexerValue(itemType, item);
                default:
                case GetValueOperation.GetProperty:
                    return this.GetPropertyValueFast(itemType, item);
            }
            
        }
        private UnaryExpression GetConversion(Type type, Expression expression)
        {
            if (type.IsValueType && !(type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>))))
            {
                return Expression.Convert(expression, type);
            }
            else
            {
                return Expression.TypeAs(expression, type);
            }
        }
        
        
        private Func<object, object> getPropertyValue = null;
        private Dictionary<Type, Func<object, object>> getPropertyValueDictionary = new Dictionary<Type, Func<object, object>>();

        private Func<object, string, object> getStringIndexValue = null;
        private Dictionary<Type, Func<object, string, object>> getStringIndexValueDictionary = new Dictionary<Type, Func<object, string, object>>();

        private Func<object, int, object> getIntIndexValue = null;
        private Dictionary<Type, Func<object, int, object>> getIntIndexValueDictionary = new Dictionary<Type, Func<object, int, object>>();

        #endregion

        #region traditional reflection and associated caches
        private object GetPropertyValueTraditional(Type itemType, object item)
        {
            if (this.PropertyName == null || this.PropertyName.Length == 0)
            {
                return item;
            }
            if (type != itemType)
            {
                type = itemType;
                propertyInfo = type.GetProperty(this.PropertyName);
            }

            return propertyInfo != null ? propertyInfo.GetValue(item, null) : null;
        }
        private PropertyInfo propertyInfo = null;
        #endregion

        #region type descriptor approach
        private object GetPropertyValueTypeDescriptor(Type itemType, object item)
        {
            if (this.type != itemType)
            {
                this.type = itemType;
                this.PropertyDescriptor = TypeDescriptor.GetProperties(item)[this.PropertyName];
            }
            return this.PropertyDescriptor != null ? this.PropertyDescriptor.GetValue(item) : null;
        }
        private PropertyDescriptor PropertyDescriptor { get; set; }
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