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
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class)]
    public class WidgetAttribute : Attribute
    {
        /// <summary>
        /// WidgetAttribute constructor.
        /// </summary>
        public WidgetAttribute()
        {
            Name = null;
        }
        /// <summary>
        /// WidgetAttribute constructor.
        /// </summary>
        /// <param name="name">The widget name.</param>
        public WidgetAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// The widget name.
        /// </summary>
        private string Name { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class)]
    public class MainWidgetAttribute : Attribute
    {
        /// <summary>
        /// MainWidgetAttribute constructor.
        /// </summary>
        public MainWidgetAttribute()
        {
            Name = null;
        }
        /// <summary>
        /// MainWidgetAttribute constructor.
        /// </summary>
        /// <param name="name">The main widget name.</param>
        public MainWidgetAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// The main widget name.
        /// </summary>
        private string Name { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.All)]
    public class SuppressWidgetMemberAttribute : Attribute
    {
        /// <summary>
        /// SuppressWidgetMemberAttribute constructor.
        /// </summary>
        public SuppressWidgetMemberAttribute()
        {

        }
    }

    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Property)]
    public class WidgetDefaultStringAttribute : Attribute
    {
        /// <summary>
        /// WidgetDefaultStringAttribute constructor.
        /// </summary>
        /// <param name="value">The widget default.</param>
        public WidgetDefaultStringAttribute(string value)
        {
            Value = value;
        }
        /// <summary>
        /// The widget default.
        /// </summary>
        public string Value { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Property)]
    public class WidgetDefaultNumberAttribute : Attribute
    {
        /// <summary>
        /// WidgetDefaultNumberAttribute constructor.
        /// </summary>
        /// <param name="value">The widget default.</param>
        public WidgetDefaultNumberAttribute(double value)
        {
            Value = value;
        }
        /// <summary>
        /// The widget default.
        /// </summary>
        public double Value { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Property)]
    public class WidgetDefaultBooleanAttribute : Attribute
    {
        /// <summary>
        /// WidgetDefaultBooleanAttribute constructor.
        /// </summary>
        /// <param name="value">The widget default.</param>
        public WidgetDefaultBooleanAttribute(bool value)
        {
            Value = value;
        }
        /// <summary>
        /// The widget default.
        /// </summary>
        public bool Value { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.All)]
    public class SuppressWidgetMemberCopyAttribute : Attribute
    {
        /// <summary>
        /// SuppressWidgetMemberCopyAttribute constructor.
        /// </summary>
        public SuppressWidgetMemberCopyAttribute()
        {

        }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class WidgetModuleAttribute : Attribute
    {
        /// <summary>
        /// WidgetModuleAttribute constructor.
        /// </summary>
        /// <param name="name">The widget module name.</param>
        public WidgetModuleAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// The widget module name.
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class WidgetModuleParentAttribute : Attribute
    {
        /// <summary>
        /// WidgetModuleParentAttribute constructor.
        /// </summary>
        /// <param name="name">The name of the widget parent module.</param>
        public WidgetModuleParentAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// The name of the widget parent module.
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class WidgetIgnoreDependsAttribute : Attribute
    {
        /// <summary>
        /// WidgetIgnoreDependsAttribute constructor.
        /// </summary>
        /// <param name="name">The name of the dependency to ignore.</param>
        public WidgetIgnoreDependsAttribute(string name)
        {
            Name = name;
        }
        /// <summary>
        /// The name of the dependency to ignore.
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.All)]
    public class DontObfuscateAttribute : Attribute
    {
        /// <summary>
        /// DontObfuscateAttribute constructor.
        /// </summary>
        public DontObfuscateAttribute()
        {

        }

    }

    /// <summary>
    /// Attribute used for cross-platform translation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.All)]
    public class WeakAttribute : Attribute
    {
        /// <summary>
        /// WeakAttribute constructor.
        /// </summary>
        public WeakAttribute()
        {

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