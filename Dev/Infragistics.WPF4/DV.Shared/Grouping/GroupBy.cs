using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics
{
    /// <summary>
    /// Groups an itemsource by a grouping column and then flattens the results into rows using the key column
    /// </summary>
    public class GroupBy
         : DependencyObject, IEnumerable, INotifyCollectionChanged
    {
        /// <summary>
        /// Identifies the ItemsSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(GroupBy),
            new PropertyMetadata(null,
                (o, e) => (o as GroupBy)
                    .OnPropertyChanged("ItemsSource", e.OldValue, e.NewValue)));

        /// <summary>
        /// The input data to the grouping operation.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the KeyMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty KeyMemberPathProperty =
            DependencyProperty.Register(
            "KeyMemberPath",
            typeof(string),
            typeof(GroupBy),
            new PropertyMetadata(null,
                (o, e) => (o as GroupBy)
                    .OnPropertyChanged("KeyMemberPath", e.OldValue, e.NewValue)));

        /// <summary>
        /// The property to use as a key to flatten a group into properties on the row.
        /// </summary>
        public string KeyMemberPath
        {
            get { return (string)GetValue(KeyMemberPathProperty); }
            set { SetValue(KeyMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the ValueMemberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register(
            "ValueMemberPath",
            typeof(string),
            typeof(GroupBy),
            new PropertyMetadata(null,
                (o, e) => (o as GroupBy)
                    .OnPropertyChanged("ValueMemberPath", e.OldValue, e.NewValue)));

        /// <summary>
        /// Gets or sets the property name that supplies numeric values.
        /// </summary>
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the GroupMemeberPath dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupMemberPathProperty =
            DependencyProperty.Register(
            "GroupMemberPath",
            typeof(string),
            typeof(GroupBy),
            new PropertyMetadata(null,
                (o, e) => (o as GroupBy)
                    .OnPropertyChanged("GroupMemberPath", e.OldValue, e.NewValue)));

        /// <summary>
        /// The property to use as a the group by clause.
        /// </summary>
        public string GroupMemberPath
        {
            get { return (string)GetValue(GroupMemberPathProperty); }
            set { SetValue(GroupMemberPathProperty, value); }
        }

        private void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            
            RefreshOutput();
        }

        private FastReflectionHelper _frh = new FastReflectionHelper();
        private INotifyCollectionChanged _source = null;

        internal int NumberOfSeries { get; set; }

        private List<string> _seriesKeys = new List<string>();
        internal List<string> SeriesKeys
        {
            get { return _seriesKeys; }
            set { _seriesKeys = value; }
        }

        private void RefreshOutput()
        {
            if (!Valid())
            {
                return;
            }

            Listen();
            RefreshItems();
            RaiseCollectionChanged();
        }

        private class Group
        {
            public object Key { get; set; }
            public IList<object> Items { get; set; }
        }

        private void RefreshItems()
        {
            _frh.PropertyName = GroupMemberPath;
            _output.Clear();

            var groups = (from item in ItemsSource.OfType<object>()
                          group item by _frh.GetPropertyValue(item) into gr
                          select new Group { Key = gr.Key, Items = gr.ToList() })
                         .ToList();

            _frh.PropertyName = KeyMemberPath;
            GroupingProxyHelper.CreateObject creator = GetObjectCreator(groups);

            foreach (var group in groups)
            {
                GroupingBase obj = creator();
                obj.Key = group.Key;
                NumberOfSeries = group.Items.Count;
                SeriesKeys.Clear();

                foreach (var item in group.Items)
                {
                    object key = _frh.GetPropertyValue(item);
                    string keyString = "Unkeyed";
                    if (key != null)
                    {
                        keyString = Sanitize(key.ToString());
                    }

                    obj.SetObjectForKey(keyString, item);
                    SeriesKeys.Add(keyString);
                }

                _output.Add(obj);
            }
        }

        private GroupingProxyHelper.CreateObject GetObjectCreator(List<Group> groups)
        {
            var properties = GetProperties(groups);
            if (GroupingProxyHelper.DistinctKeysTypes(properties) == null)
            {
                return null;
            }
            try
            {
                return GroupingProxyHelper.CreateObjectCreator(properties);
            }
            catch (MissingMethodException)
            {
                // [DN Dec 13 2011 : 96856] exception found at design time only in Silverlight 5.  return a dummy object as a failsafe
                return delegate() { return new GroupingBase(); };
            }
        }

        private List<ConcretePropertyInfo> GetProperties(List<Group> groups)
        {
            var distinctKeyTypes = from g in groups
                                   from item in g.Items
                                   group item by _frh.GetPropertyValue(item) into gr
                                   select new
                                   {
                                       Key = gr.Key,
                                       Types = (from item in gr
                                                group item by item.GetType() into subGr
                                                select subGr.Key)
                                   };

            var distinctCombinations = from kt in distinctKeyTypes
                                       from t in kt.Types
                                       select new
                                       {
                                           Key = kt.Key,
                                           Type = t
                                       };

            var props = new List<ConcretePropertyInfo>();
            foreach (var comb in distinctCombinations)
            {
                object key = comb.Key;
                string keyString = "Unkeyed";
                if (key != null)
                {
                    keyString = Sanitize(key.ToString());
                }

                var clrProps = from p in
                                   comb.Type.GetProperties(
                                       BindingFlags.Public |
                                       BindingFlags.Instance |
                                       BindingFlags.FlattenHierarchy)
                               select p;

                foreach (var clrProp in clrProps)
                {

                    props.Add(
                       new ConcretePropertyInfo()
                       {
                           SourceType = comb.Type,
                           Key = keyString,
                           Name = clrProp.Name,
                           PropertyType = clrProp.PropertyType,
                           PropertyName = keyString + "_" + clrProp.Name
                       }
                   );
                }
            }
            return props;
        }

        private string Sanitize(string key)
        {
            
            return key.Replace(".","__dot__");
        }

        private void Listen()
        {
            if (_source != null)
            {
                _source.CollectionChanged -= Source_CollectionChanged;
            }
            _source = ItemsSource as INotifyCollectionChanged;
            if (_source != null)
            {
                _source.CollectionChanged += Source_CollectionChanged;
            }
        }

        void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            RefreshOutput();
            RaiseCollectionChanged();
        }

        private bool Valid()
        {
            return ItemsSource != null &&
                GroupMemberPath != null &&
                KeyMemberPath != null;
        }

        private void RaiseCollectionChanged()
        {
            
            if (CollectionChanged != null)
            {
                CollectionChanged(
                    this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset));
            }
        }
        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        private List<object> _output = new List<object>();
        /// <summary>
        /// Gets the IEnumerator used for looping through the GroupBy.
        /// </summary>
        /// <returns>The IEnumerator used for looping through the GroupBy.</returns>
        public IEnumerator GetEnumerator()
        {
            if (_output == null)
            {
                return new List<object>().GetEnumerator();
            }

            return _output.GetEnumerator();
        }
    }

    internal class ConcretePropertyInfo
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string PropertyName { get; set; }
        public Type SourceType { get; set; }
        public Type PropertyType { get; set; }
    }
    /// <summary>
    /// Base class for grouping collections by key.
    /// </summary>
    public class GroupingBase
    {
        /// <summary>
        /// They GroupBy key.
        /// </summary>
        public object Key { get; set; }
        /// <summary>
        /// Sets an item to be associated with the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="obj">The data item.</param>
        public void SetObjectForKey(string key, object obj)
        {
            SetObjectForKeyOverride(key, obj);
        }
        /// <summary>
        /// Method to override the behavior of the SetObjectForKey method.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="obj">The data item.</param>
        protected virtual void SetObjectForKeyOverride(string key, object obj) { }
    }

    internal static class GroupingProxyHelper
    {
        private static AssemblyBuilder _ab;
        private static ModuleBuilder _mb;

        public delegate GroupingBase CreateObject();

        public static CreateObject CreateObjectCreator(List<ConcretePropertyInfo> props)
        {
            List<object> parms = new List<object>();

            Type type = CreateProxyType(props);
            ConstructorInfo ci = type.GetConstructor(new Type[] { });

            DynamicMethod meth = new DynamicMethod("Creator", typeof(GroupingBase), new Type[] { });
            var ilGen = meth.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, ci);
            ilGen.Emit(OpCodes.Ret);
            Type delType = typeof(CreateObject);
            return (CreateObject)meth.CreateDelegate(delType);
        }

        public static List<Tuple<string, Type>> DistinctKeysTypes(List<ConcretePropertyInfo> props)
        {
            var keyTypes = from prop in props
                           group prop by new
                           {
                               Key = prop.Key,
                               Type = prop.SourceType
                           } into gr
                           select new Tuple<string, Type>(
                               gr.Key.Key,
                               gr.Key.Type);


            var distinctKeys = from prop in props
                               group prop by prop.Key
                                   into gr
                                   select gr;

            //don't support case where a key is used with two different types.
            if (keyTypes.Count() != distinctKeys.Count())
            {
                return null;
            }

            return keyTypes.ToList();
        }

        private static Type CreateProxyType(List<ConcretePropertyInfo> props)
        {
            if (_ab == null)
            {
                AssemblyName assmName = new AssemblyName("DynamicAssembly");
                _ab = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assmName, AssemblyBuilderAccess.Run);
                _mb = _ab.DefineDynamicModule(assmName.Name);
            }

            TypeBuilder typeBuilder = _mb.DefineType(
            Guid.NewGuid().ToString() + "__proxy", TypeAttributes.Public, typeof(GroupingBase));

            Dictionary<string, FieldBuilder> fields = new Dictionary<string, FieldBuilder>();
            CreateFields(props, typeBuilder, fields);
            CreateProperties(props, typeBuilder, fields);
            CreateSetter(props, typeBuilder, fields);

            Type ret = typeBuilder.CreateType();
            return ret;
        }

        private static void CreateFields(
            List<ConcretePropertyInfo> props,
            TypeBuilder tb,
            Dictionary<string, FieldBuilder> fields)
        {
            foreach (var kt in DistinctKeysTypes(props))
            {
                FieldBuilder fb = tb.DefineField(
                    "_" + kt.Item1,
                    kt.Item2,
                     FieldAttributes.Private);
                fields.Add(kt.Item1, fb);
            }
        }

        private static void CreateSetter(
            List<ConcretePropertyInfo> props,
            TypeBuilder tb,
            Dictionary<string, FieldBuilder> fields)
        {
            MethodBuilder mb = tb.DefineMethod("SetObjectForKeyOverride",
                MethodAttributes.Public | MethodAttributes.Virtual, typeof(void),
                new Type[] { typeof(string), typeof(object) });

            var ilGen = mb.GetILGenerator();

            MethodInfo stringEq = typeof(String).GetMethod(
            "op_Equality",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new Type[]{
                typeof(String),
                typeof(String)
                },
            null
            );
            LocalBuilder comp = ilGen.DeclareLocal(typeof(String));

            Dictionary<string, Label> labels = new Dictionary<string, Label>();
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stloc_0);
            ilGen.Emit(OpCodes.Ldloc_0);
            Label end = ilGen.DefineLabel();
            ilGen.Emit(OpCodes.Brfalse, end);

            foreach (var key in DistinctKeysTypes(props))
            {
                Label label = ilGen.DefineLabel();
                labels[key.Item1] = label;

                ilGen.Emit(OpCodes.Ldloc_0);
                ilGen.Emit(OpCodes.Ldstr, key.Item1);
                ilGen.Emit(OpCodes.Call, stringEq);
                ilGen.Emit(OpCodes.Brtrue, label);
            }
            ilGen.Emit(OpCodes.Br, end);

            foreach (var key in DistinctKeysTypes(props))
            {
                Label label = labels[key.Item1];
                ilGen.MarkLabel(label);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_2);
                ilGen.Emit(OpCodes.Isinst, key.Item2);
                ilGen.Emit(OpCodes.Stfld, fields[key.Item1]);
                ilGen.Emit(OpCodes.Br, end);
            }

            ilGen.MarkLabel(end);
            ilGen.Emit(OpCodes.Ret);
        }

        private static void CreateProperties(List<ConcretePropertyInfo> props,
            TypeBuilder typeBuilder,
            Dictionary<string, FieldBuilder> fields)
        {
            foreach (ConcretePropertyInfo info in props)
            {
                PropertyBuilder pb = typeBuilder.DefineProperty(
                    info.PropertyName, PropertyAttributes.None,
                    info.PropertyType,
                    null);

                var getMethod = CreateGetMethod(info, typeBuilder, fields);
                var setMethod = CreateSetMethod(info, typeBuilder, fields);

                pb.SetGetMethod(getMethod);
                pb.SetSetMethod(setMethod);
            }
        }

        private static MethodBuilder CreateSetMethod(
            ConcretePropertyInfo info,
            TypeBuilder typeBuilder,
            Dictionary<string, FieldBuilder> fields)
        {
            PropertyInfo innerProp = info.SourceType.GetProperty(info.Name);
            MethodInfo orig = innerProp.GetSetMethod();
            var paramTypes = (from p in orig.GetParameters() select p.ParameterType).ToArray();
            MethodBuilder mb = typeBuilder.DefineMethod("set_" + info.PropertyName, orig.Attributes,
                orig.CallingConvention, orig.ReturnType, paramTypes);

            ILGenerator ilGen = mb.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, fields[info.Key]);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Callvirt, orig);
            ilGen.Emit(OpCodes.Ret);

            return mb;
        }

        private static MethodBuilder CreateGetMethod(
            ConcretePropertyInfo info,
            TypeBuilder typeBuilder,
            Dictionary<string, FieldBuilder> fields)
        {
            PropertyInfo innerProp = info.SourceType.GetProperty(info.Name);
            MethodInfo orig = innerProp.GetGetMethod();
            var paramTypes = (from p in orig.GetParameters() select p.ParameterType).ToArray();
            MethodBuilder mb = typeBuilder.DefineMethod("get_" + info.PropertyName, orig.Attributes,
                orig.CallingConvention, orig.ReturnType, paramTypes);

            ILGenerator ilGen = mb.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldfld, fields[info.Key]);
            ilGen.Emit(OpCodes.Callvirt, orig);
            ilGen.Emit(OpCodes.Ret);

            return mb;
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