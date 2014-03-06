using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup.Primitives;
using System.Windows.Markup;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Collections;
using System.Windows.Controls;
using System.ComponentModel;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows
{
    /// <summary>
    /// Provides information about the clone operation as well as storing cloned instances
    /// </summary>
    internal class CloneInfo
    {
        #region Member Variables

        private object _rootSource;
        private Dictionary<Object, Object> _clonedObjects;
        private IServiceProvider _serviceProvider;

        #endregion //Member Variables

        #region Constructor
        internal CloneInfo(object rootSource)
        {
            this._rootSource = rootSource;
            this._clonedObjects = new Dictionary<Object, Object>();
            XamlDesignerSerializationManager manager = new XamlDesignerSerializationManager(null);
            manager.XamlWriterMode = XamlWriterMode.Expression;
            this._serviceProvider = manager;
        } 
        #endregion //Constructor

        #region Properties
        public object RootSource
        {
            get { return this._rootSource; }
        }

        public IServiceProvider ServiceProvider
        {
            get { return this._serviceProvider; }
        } 
        #endregion //Properties

        #region Methods
        /// <summary>
        /// Tries to return a clone for the specified object if it has been added to the clone info.
        /// </summary>
        /// <param name="source">The object to locate</param>
        /// <param name="clone">An out parameter that is set to the cloned instance of the source</param>
        /// <returns>Returns true if the object has been cloned otherwise false</returns>
        public bool TryGetClonedObject(Object source, out Object clone)
        {
            return this._clonedObjects.TryGetValue(source, out clone);
        }

        /// <summary>
        /// Stores the specified source object and the clone that was created.
        /// </summary>
        /// <param name="source">The object that was cloned</param>
        /// <param name="clone">The clone that represents a copy of the source</param>
        public void AddClone(Object source, Object clone)
        {
            this._clonedObjects.Add(source, clone);
        } 
        #endregion //Methods
    }

    /// <summary>
    /// Helper class for cloning a dependency object using the MarkupObject infrastructure.
    /// </summary>
    internal class CloneManager
    {
        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="CloneManager"/>
        /// </summary>
        public CloneManager()
        {
        } 
        #endregion //Constructor

        #region Methods

        #region Public

        #region Clone
        /// <summary>
        /// Creates a copy of the specified object.
        /// </summary>
        /// <param name="source">The object to be copied.</param>
        /// <returns>A copy of the object.</returns>
        public object Clone(object source)
        {
			// AS 10/1/09 TFS22197
			Debug.Assert(this.ShouldClone(source));

            MarkupObject obj = MarkupWriter.GetMarkupObjectFor(source);
            CloneInfo cloneInfo = new CloneInfo(source);

            object clone = this.Clone(obj, cloneInfo);
            return clone;
        }
        #endregion //Clone

		// AS 10/1/09 TFS22197
		#region ShouldClone
		/// <summary>
		/// Used to determine if a given object should be cloned.
		/// </summary>
		/// <param name="source">The object to evaluate</param>
		/// <returns>A boolean indicating if the object should be cloned.</returns>
		public virtual bool ShouldClone(object source)
		{
			return true;
		} 
		#endregion //ShouldClone

        #endregion //Public

        #region Protected

        #region Clone
        /// <summary>
        /// Helper method used to provide the clone of the specified object.
        /// </summary>
        /// <param name="mo">The markup object representing the object to be cloned.</param>
        /// <param name="cloneInfo">Provides information about clone operation.</param>
        /// <returns>A clone of the specified object</returns>
        protected virtual object Clone(MarkupObject mo, CloneInfo cloneInfo)
        {
			// AS 3/18/11 TFS35776
			// We may want to clone objects that don't derive from DependencyObject we'll store 
			// this as object and check how to treat the object using the new GetCloneBehavior 
			// method.
			//
			//object dependency = mo.Instance as DependencyObject;
            object dependency = mo.Instance;

            // for dependency objects, we'll just use the value as is
			// AS 3/18/11 TFS35776
			// Let the derived class have the option of whether to clone or share the object. We'll
			// default to the behavior we had where we only cloned dependencyobjects.
			//
			//if (dependency == null)
			//    return mo.Instance;
			if (dependency == null || this.GetCloneBehavior(dependency) == CloneBehavior.ShareInstance)
				return dependency;

            // AS 10/1/07
            // If we have already cloned this source object then return the cloned instance.
            //
            if (cloneInfo.TryGetClonedObject(dependency, out dependency))
                return dependency;

            // otherwise clone the object
			// AS 3/18/11 TFS35776
			// We can't cast the object to DependencyObject since we can get here for non-DO types. 
			//
			//dependency = (DependencyObject)Activator.CreateInstance(mo.ObjectType);
			dependency = Activator.CreateInstance(mo.ObjectType);

            // AS 10/1/07
            // Store the cloned dependency objects in case there is a property that references an 
            // object we already cloned since we don't want to clone it again.
            //
			// AS 3/18/11 TFS35776
			//DependencyObject sourceDependency = (DependencyObject)mo.Instance;
            object sourceDependency = mo.Instance;
            cloneInfo.AddClone(sourceDependency, dependency);

            // now copy over the properties
            foreach (MarkupProperty mp in mo.Properties)
            {
                // AS 6/10/08
                // Moved up so we don't bother cloning an element if we should skip the property.
                //
                if (null != mp.DependencyProperty && this.ShouldSkipProperty(mp))
                    continue;

                // AS 6/26/08 BR34367
                //object propValue = mp.Value;
                object propValue = GetValue(sourceDependency, mp);
               
                // JJD 2/11/09 - TFS10860/TFS13609
                // create a stack variale to determine whether to clone the sub object
                bool cloneSubObject = (propValue is Visual || propValue is ContentElement);

                // JJD 2/11/09 - TFS10860/TFS13609
                #region Look for CloneBehavior attribute

                // if the value is not a known type then we need to look for the
                // CloneBehaviorAttribute
                if ( propValue != null
                    && !(propValue is string))
                {
                    Type type = propValue.GetType();

                    // JJD 2/11/09 - TFS10860/TFS13609
                    // ignore value types as well
                    if (!type.IsValueType)
                    {
                        CloneBehavior? cloneBehavior = null;

                        PropertyDescriptor pd = mp.PropertyDescriptor;

                        // JJD 2/11/09 - TFS10860/TFS13609
                        // look for the attribute on the property descriptor 
                        // Note: this will return the attribute even if it is
                        // defined on the class not the property
                        if (pd != null)
                        {
                            foreach (Attribute attr in pd.Attributes)
                            {
                                CloneBehaviorAttribute cb = attr as CloneBehaviorAttribute;

                                if (cb != null)
                                {
                                    cloneBehavior = cb.Behavior;
                                    break;
                                }
                            }
                        }

                        // JJD 2/11/09 - TFS10860/TFS13609
                        // if the attribute was found  then
                        // set our stack variable appropriately.
                        if (cloneBehavior.HasValue)
                        {
                            cloneSubObject = (cloneBehavior.Value == CloneBehavior.CloneObject);
                        }
                    }
                }

                #endregion //Look for CloneBehavior attribute	
    
                // if the value is a visual - e.g. the content of an object
                // is another element

                // JJD 2/11/09 - TFS10860/TFS13609
                // Use stack varibale from above instead
                //if (propValue is Visual || propValue is ContentElement)
                if (cloneSubObject)
                {
					// AS 10/1/09 TFS22197
					if (!this.ShouldClone(propValue))
						continue;

                    // we need to get the markup for that object and use that as the value
                    MarkupObject moChildPropValue = MarkupWriter.GetMarkupObjectFor(propValue);
                    propValue = this.Clone(moChildPropValue, cloneInfo);
                }

                // AS 9/22/08
                // Moved up since this can affect CLR props as well and in the v8.2
                // reporting it does.
                //
                MarkupExtension markupExtension = propValue as MarkupExtension;

                if (null != markupExtension)
                    propValue = markupExtension.ProvideValue(cloneInfo.ServiceProvider);

                // if this is a dependency property then set the value of that
                #region Dependency Property
                if (mp.DependencyProperty != null)
                {
                    // AS 6/10/08
                    // Moved up so we don't bother cloning an element if we should skip the property.
                    //
                    //// AS 11/13/07 BR28346
                    //if (cloneInfo.SkipProperty(mp.DependencyProperty))
                    //	continue;
                    Debug.Assert(dependency is DependencyObject);

                    
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


                    ((DependencyObject)dependency).SetValue(mp.DependencyProperty, propValue);
                    continue;
                }
                else if (mp.PropertyDescriptor != null)
                {
                    // if its a clr property and its not readonly then carry over the value
                    if (mp.PropertyDescriptor.IsReadOnly == false)
                    {
                        mp.PropertyDescriptor.SetValue(dependency, propValue);
                        continue;
                    }
                }
                #endregion //Dependency Property

                #region Iterate the markup items
                if (mp.Items != null && mp.PropertyType != typeof(string))
                {
                    CloneItems(dependency, mp, cloneInfo);
                }
                #endregion //Iterate the markup items
            }

            return dependency;
        }

        #endregion //Clone

        #region CloneItems
        /// <summary>
        /// Helper method for copying the Items of the specified markup property.
        /// </summary>
        /// <param name="clone">The cloned object whose items collection should be updated</param>
        /// <param name="mp">The property with the items to be cloned</param>
        /// <param name="cloneInfo">Provides information about the clone operation</param>
        protected virtual void CloneItems(object clone, MarkupProperty mp, CloneInfo cloneInfo)
        {
            Debug.Assert(null != mp.PropertyDescriptor);
            Debug.Assert(null != mp.Items);

            if (null == mp.Items || null == mp.PropertyDescriptor)
                return;

            // get the collection from the new object
            object collection = mp.PropertyDescriptor.GetValue(clone);
            IList list = collection as IList;

            if (null != list)
            {
                // create all the children
                foreach (MarkupObject moChild in mp.Items)
                {
					// AS 10/1/09 TFS22197
					if (!this.ShouldClone(moChild.Instance))
						continue;

                    object moChildValue = this.Clone(moChild, cloneInfo);

                    list.Add(moChildValue);
                }
            }
            else
            {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            }
        }
        #endregion //CloneItems

		// AS 3/18/11 TFS35776
		// Added a way for the derived class to tell us how the object should be treated in the clone method.
		//
		#region GetCloneBehavior
		/// <summary>
		/// Used to determine whether the specified object should be shared or cloned.
		/// </summary>
		/// <param name="source">The object to evaluate</param>
		/// <returns>A CloneBehavior enum value indicating how to handle the copying.</returns>
		protected virtual CloneBehavior GetCloneBehavior(object source)
		{
			if (source is DependencyObject)
				return CloneBehavior.CloneObject;

			return CloneBehavior.ShareInstance;
		}
		#endregion //GetCloneBehavior

        #region ShouldSkipProperty
        /// <summary>
        /// Used to determine whether the specified property should be skipped
        /// </summary>
        /// <param name="property">The property to evaluate</param>
        /// <returns>Returns false to indicate that the property should be copied.</returns>
        protected virtual bool ShouldSkipProperty(MarkupProperty property)
        {
            return false;
        }
        #endregion //ShouldSkipProperty

        #endregion //Protected

        #region Private

        #region GetValue
		// AS 3/18/11 TFS35776
		//private static object GetValue(DependencyObject source, MarkupProperty mp)
        private static object GetValue(object source, MarkupProperty mp)
        {
			// AS 3/18/11 TFS35776
            //if (mp.DependencyProperty != null)
			DependencyObject d = source as DependencyObject;

            if (mp.DependencyProperty != null && d != null)
            {
                // AS 6/26/08 BR34367
                // There's a bug in the MarkupProperty when the value is an enum.
                // They try to create a StaticExtension using a ValueSerializer
                // but they don't get one back.
                //
				// AS 3/18/11 TFS35776
				//object value = source.ReadLocalValue(mp.DependencyProperty);
                object value = d.ReadLocalValue(mp.DependencyProperty);

                if (value is Enum)
                    return value;
            }
            else if (mp.PropertyDescriptor != null)
            {
                // AS 10/14/08 TFS8676
                // The property may not be a dependency property but the MS MarkupProperty
                // class still causes a NullReferenceException so we'll try to workaround
                // that bug as well by getting the value from the property descriptor.
                //
                object value = mp.PropertyDescriptor.GetValue(source);

                if (value is Enum)
                    return value;
            }

            return mp.Value;
        }
        #endregion //GetValue

        #endregion //Private

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